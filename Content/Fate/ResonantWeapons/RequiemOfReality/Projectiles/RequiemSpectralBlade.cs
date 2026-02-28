using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.Fate.ResonantWeapons.RequiemOfReality.Utilities;
using MagnumOpus.Content.Fate.ResonantWeapons.RequiemOfReality.Particles;
using MagnumOpus.Content.Fate.ResonantWeapons.RequiemOfReality.Primitives;
using MagnumOpus.Content.Fate.ResonantWeapons.RequiemOfReality.Shaders;
using MagnumOpus.Content.Fate.Debuffs;

namespace MagnumOpus.Content.Fate.ResonantWeapons.RequiemOfReality.Projectiles
{
    /// <summary>
    /// Requiem Spectral Blade — 6-phase autonomous combo projectile.
    /// Spawns on the 4th swing (Finale movement). Operates independently of the player.
    ///
    /// PHASE SYSTEM (like a musical cadence):
    ///   Phase 0 — RISE (Suspension):  Float upward from spawn, expanding glow
    ///   Phase 1 — ORBIT (Trill):      Rapid spin above player head, building energy
    ///   Phase 2 — DETONATE (Sfz):     Explosive cosmic burst, AoE damage ring
    ///   Phase 3 — SEEK (Glissando):   Identify nearest enemy, curve toward it
    ///   Phase 4 — SLASH (Staccato):   Two rapid through-slashes on target
    ///   Phase 5 — RETURN (Ritenuto):  Decelerate and fade back to player
    /// </summary>
    public class RequiemSpectralBlade : ModProjectile
    {
        public override string Texture => "MagnumOpus/Content/Fate/ResonantWeapons/RequiemOfReality";

        // Phase durations (frames)
        private const int RISE_DURATION = 30;
        private const int ORBIT_DURATION = 40;
        private const int DETONATE_DURATION = 15;
        private const int SEEK_DURATION = 30;
        private const int SLASH_DURATION = 24;
        private const int RETURN_DURATION = 35;
        private const int TOTAL_DURATION = RISE_DURATION + ORBIT_DURATION + DETONATE_DURATION + SEEK_DURATION + SLASH_DURATION + RETURN_DURATION;

        // Phase tracking
        private int _phase;
        private int _phaseTimer;
        private int _slashCount;
        private Vector2 _orbitCenter;
        private float _orbitAngle;
        private NPC _target;
        private Vector2 _slashStart;
        private Vector2 _slashEnd;
        private float _spinRotation;

        // Trail
        private Vector2[] _trailPositions = new Vector2[20];
        private int _trailCount;

        // Textures
        private static Asset<Texture2D> _glowTex;
        private static Asset<Texture2D> _flareTex;

        private Player Owner => Main.player[Projectile.owner];

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 60;
            Projectile.height = 60;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 8;
            Projectile.timeLeft = TOTAL_DURATION + 20;
            Projectile.alpha = 0;
        }

        public override void OnSpawn(IEntitySource source)
        {
            _phase = 0;
            _phaseTimer = 0;
            _slashCount = 0;
            _orbitCenter = Owner.MountedCenter - new Vector2(0, 60f);
            _orbitAngle = Main.rand.NextFloat(MathHelper.TwoPi);
            _spinRotation = 0f;

            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.2f, Volume = 0.8f }, Projectile.Center);
        }

        public override void AI()
        {
            _phaseTimer++;

            switch (_phase)
            {
                case 0: Phase0_Rise(); break;
                case 1: Phase1_Orbit(); break;
                case 2: Phase2_Detonate(); break;
                case 3: Phase3_Seek(); break;
                case 4: Phase4_Slash(); break;
                case 5: Phase5_Return(); break;
            }

            UpdateTrail();
            SpawnAmbientParticles();

            // Spin rotation
            float spinSpeed = _phase switch
            {
                0 => 0.08f,
                1 => 0.25f,
                2 => 0.4f,
                3 => 0.15f,
                4 => 0.35f,
                _ => 0.05f
            };
            _spinRotation += spinSpeed;

            // Lighting
            Color lightCol = RequiemUtils.PaletteLerp((float)_phaseTimer / 60f);
            Lighting.AddLight(Projectile.Center, lightCol.ToVector3() * 0.6f);
        }

        // ======================== PHASE IMPLEMENTATIONS ========================

        /// <summary>Phase 0: Rise upward from spawn point with expanding glow.</summary>
        private void Phase0_Rise()
        {
            float progress = (float)_phaseTimer / RISE_DURATION;
            float eased = RequiemUtils.CubicOut(progress);

            // Float upward
            Vector2 target = Owner.MountedCenter - new Vector2(0, 80f);
            Projectile.Center = Vector2.Lerp(Owner.MountedCenter, target, eased);
            Projectile.velocity = Vector2.Zero;

            // Expanding spawn glow
            if (Main.rand.NextBool(2))
            {
                Color glowCol = RequiemUtils.PaletteLerp(Main.rand.NextFloat(0.3f, 0.7f));
                RequiemParticleHandler.SpawnParticle(new RequiemMoteParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(20f, 20f),
                    Main.rand.NextVector2Circular(1f, 1f),
                    glowCol, 0.3f * (1f + eased), 20));
            }

            if (_phaseTimer >= RISE_DURATION)
                TransitionPhase(1);
        }

        /// <summary>Phase 1: Rapid spin orbit above player, building energy.</summary>
        private void Phase1_Orbit()
        {
            float progress = (float)_phaseTimer / ORBIT_DURATION;
            float orbitRadius = 40f - progress * 15f; // Tighten spiral
            float orbitSpeed = 0.15f + progress * 0.12f; // Accelerate

            _orbitCenter = Owner.MountedCenter - new Vector2(0, 70f);
            _orbitAngle += orbitSpeed;

            Projectile.Center = _orbitCenter + _orbitAngle.ToRotationVector2() * orbitRadius;
            Projectile.velocity = Vector2.Zero;

            // Building energy particles
            if (Main.rand.NextBool(2))
            {
                Vector2 sparkDir = (_orbitAngle + MathHelper.PiOver2).ToRotationVector2();
                RequiemParticleHandler.SpawnParticle(new RequiemSparkParticle(
                    Projectile.Center, sparkDir * Main.rand.NextFloat(2f, 4f),
                    RequiemUtils.GetCosmicGradient(progress), 0.2f + progress * 0.15f, 12));
            }

            // Music notes spiral inward
            if (Main.rand.NextBool(4))
            {
                RequiemParticleHandler.SpawnParticle(new RequiemNoteParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(30f, 30f),
                    (Projectile.Center - Projectile.oldPosition).SafeNormalize(Vector2.Zero) * 0.5f,
                    RequiemUtils.ConstellationSilver, 0.25f, 20));
            }

            // Sound buildup
            if (_phaseTimer == ORBIT_DURATION / 2)
                SoundEngine.PlaySound(SoundID.Item29 with { Pitch = 0.5f, Volume = 0.5f }, Projectile.Center);

            if (_phaseTimer >= ORBIT_DURATION)
                TransitionPhase(2);
        }

        /// <summary>Phase 2: Explosive detonation — AoE damage ring.</summary>
        private void Phase2_Detonate()
        {
            float progress = (float)_phaseTimer / DETONATE_DURATION;

            // Stay stationary during explosion
            Projectile.velocity = Vector2.Zero;

            // Frame 1: Big explosion VFX
            if (_phaseTimer == 1)
            {
                SoundEngine.PlaySound(SoundID.Item14 with { Pitch = -0.3f, Volume = 0.9f }, Projectile.Center);

                // Massive bloom flash
                RequiemParticleHandler.SpawnParticle(new RequiemBloomFlare(
                    Projectile.Center, RequiemUtils.SupernovaWhite, 1.0f, 18));
                RequiemParticleHandler.SpawnParticle(new RequiemBloomFlare(
                    Projectile.Center, RequiemUtils.BrightCrimson, 0.7f, 15));

                // Radial spark burst
                for (int i = 0; i < 16; i++)
                {
                    float angle = MathHelper.TwoPi * i / 16f;
                    Vector2 sparkVel = angle.ToRotationVector2() * Main.rand.NextFloat(6f, 12f);
                    Color sparkCol = RequiemUtils.GetCosmicGradient((float)i / 16f);
                    RequiemParticleHandler.SpawnParticle(new RequiemSparkParticle(
                        Projectile.Center, sparkVel, sparkCol, 0.35f, 18));
                }

                // Glyph ring
                for (int i = 0; i < 6; i++)
                {
                    float angle = MathHelper.TwoPi * i / 6f;
                    Vector2 glyphPos = Projectile.Center + angle.ToRotationVector2() * 50f;
                    RequiemParticleHandler.SpawnParticle(new RequiemGlyphParticle(
                        glyphPos, RequiemUtils.DarkPink, 0.4f, 30));
                }

                // Music note explosion
                for (int i = 0; i < 8; i++)
                {
                    Vector2 noteVel = Main.rand.NextVector2Unit() * Main.rand.NextFloat(3f, 7f);
                    RequiemParticleHandler.SpawnParticle(new RequiemNoteParticle(
                        Projectile.Center, noteVel, RequiemUtils.ConstellationSilver, 0.35f, 25));
                }
            }

            // Expanding nebula wisps
            if (Main.rand.NextBool(2))
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float dist = progress * 80f;
                Vector2 wispPos = Projectile.Center + angle.ToRotationVector2() * dist;
                RequiemParticleHandler.SpawnParticle(new RequiemNebulaWisp(
                    wispPos, angle.ToRotationVector2() * 1f,
                    RequiemUtils.NebulaMist, 0.2f, 25));
            }

            if (_phaseTimer >= DETONATE_DURATION)
            {
                // Find target for seek phase
                _target = FindBestTarget(800f);
                TransitionPhase(3);
            }
        }

        /// <summary>Phase 3: Curve toward nearest enemy (glissando arc).</summary>
        private void Phase3_Seek()
        {
            float progress = (float)_phaseTimer / SEEK_DURATION;

            if (_target != null && _target.active && !_target.dontTakeDamage)
            {
                // Smooth homing toward target
                Vector2 toTarget = (_target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                float speed = 8f + progress * 16f; // Accelerate
                float turnSpeed = 0.08f + progress * 0.12f;

                Vector2 desiredVel = toTarget * speed;
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, desiredVel, turnSpeed);
                Projectile.Center += Projectile.velocity;

                // Seeking trail sparks
                if (Main.rand.NextBool(2))
                {
                    RequiemParticleHandler.SpawnParticle(new RequiemSparkParticle(
                        Projectile.Center, -Projectile.velocity.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(1f, 3f),
                        RequiemUtils.BrightCrimson, 0.2f, 10));
                }
            }
            else
            {
                // No target — fly toward mouse position
                Vector2 toMouse = (Main.MouseWorld - Projectile.Center).SafeNormalize(Vector2.Zero);
                Projectile.velocity = toMouse * 12f;
                Projectile.Center += Projectile.velocity;
            }

            if (_phaseTimer >= SEEK_DURATION)
            {
                // Set up first slash
                _slashCount = 0;
                SetupSlash();
                TransitionPhase(4);
            }
        }

        /// <summary>Phase 4: Two rapid through-slashes on target (staccato).</summary>
        private void Phase4_Slash()
        {
            float slashHalf = SLASH_DURATION / 2f;
            float localTimer = _phaseTimer;

            // Which slash are we on?
            if (localTimer > slashHalf && _slashCount == 0)
            {
                _slashCount = 1;
                SetupSlash();
            }

            float slashProgress = (localTimer - _slashCount * slashHalf) / slashHalf;
            slashProgress = MathHelper.Clamp(slashProgress, 0f, 1f);
            float eased = RequiemUtils.QuadIn(slashProgress);

            // Interpolate along slash line
            Projectile.Center = Vector2.Lerp(_slashStart, _slashEnd, eased);

            // Slash VFX
            if (Main.rand.NextBool(2))
            {
                Vector2 moveDir = (_slashEnd - _slashStart).SafeNormalize(Vector2.Zero);
                RequiemParticleHandler.SpawnParticle(new RequiemSparkParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(8f, 8f),
                    moveDir.RotatedByRandom(0.4f) * Main.rand.NextFloat(3f, 7f),
                    RequiemUtils.SupernovaWhite, 0.25f, 8));
            }

            // Impact at end of each slash
            if (slashProgress >= 0.95f && slashProgress < 0.98f)
            {
                SoundEngine.PlaySound(SoundID.Item71 with { Pitch = 0.2f + _slashCount * 0.3f, Volume = 0.7f },
                    Projectile.Center);

                RequiemParticleHandler.SpawnParticle(new RequiemBloomFlare(
                    Projectile.Center, RequiemUtils.BrightCrimson, 0.5f, 10));
            }

            if (_phaseTimer >= SLASH_DURATION)
                TransitionPhase(5);
        }

        /// <summary>Phase 5: Decelerate and return to player (ritenuto).</summary>
        private void Phase5_Return()
        {
            float progress = (float)_phaseTimer / RETURN_DURATION;
            float eased = RequiemUtils.SineInOut(progress);

            Vector2 toPlayer = (Owner.MountedCenter - Projectile.Center).SafeNormalize(Vector2.Zero);
            float speed = 6f + eased * 10f;
            Projectile.velocity = toPlayer * speed;
            Projectile.Center += Projectile.velocity;

            // Fade out
            Projectile.alpha = (int)(progress * 255f);

            // Fading motes
            if (Main.rand.NextBool(3))
            {
                RequiemParticleHandler.SpawnParticle(new RequiemMoteParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(10f, 10f),
                    -Projectile.velocity * 0.1f,
                    RequiemUtils.FatePurple, 0.2f * (1f - progress), 15));
            }

            // Kill when close to player or time runs out
            if (Projectile.Center.Distance(Owner.MountedCenter) < 30f || _phaseTimer >= RETURN_DURATION)
            {
                Projectile.Kill();
            }
        }

        // ======================== HELPER METHODS ========================

        private void TransitionPhase(int newPhase)
        {
            _phase = newPhase;
            _phaseTimer = 0;
        }

        private void SetupSlash()
        {
            Vector2 targetPos = _target != null && _target.active ? _target.Center : Main.MouseWorld;
            float angle = _slashCount == 0
                ? Main.rand.NextFloat(-0.5f, -0.3f) // Downward diagonal
                : Main.rand.NextFloat(0.3f, 0.5f);  // Upward diagonal

            Vector2 offset = angle.ToRotationVector2() * 120f;
            _slashStart = targetPos - offset;
            _slashEnd = targetPos + offset;
        }

        private NPC FindBestTarget(float range)
        {
            NPC best = null;
            float bestDist = range;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage || npc.immortal) continue;

                float dist = Vector2.Distance(Projectile.Center, npc.Center);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    best = npc;
                }
            }

            return best;
        }

        private void UpdateTrail()
        {
            if (_trailCount < _trailPositions.Length)
            {
                _trailPositions[_trailCount] = Projectile.Center;
                _trailCount++;
            }
            else
            {
                Array.Copy(_trailPositions, 1, _trailPositions, 0, _trailPositions.Length - 1);
                _trailPositions[_trailPositions.Length - 1] = Projectile.Center;
            }
        }

        private void SpawnAmbientParticles()
        {
            if (Main.dedServ) return;

            // Constant gentle motes
            if (Main.rand.NextBool(4))
            {
                Color moteCol = RequiemUtils.PaletteLerp(Main.rand.NextFloat());
                RequiemParticleHandler.SpawnParticle(new RequiemMoteParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(15f, 15f),
                    Main.rand.NextVector2Circular(0.5f, 0.5f),
                    moteCol, 0.15f, 20));
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<DestinyCollapse>(), 300);

            // Impact sparks
            for (int i = 0; i < 6; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2Unit() * Main.rand.NextFloat(3f, 8f);
                RequiemParticleHandler.SpawnParticle(new RequiemSparkParticle(
                    target.Center, sparkVel, RequiemUtils.BrightCrimson, 0.25f, 12));
            }

            RequiemParticleHandler.SpawnParticle(new RequiemBloomFlare(
                target.Center, RequiemUtils.SupernovaWhite, 0.4f, 10));
        }

        public override bool? CanDamage()
        {
            // Only damage during slash and detonate phases
            return _phase == 2 || _phase == 4 ? null : false;
        }

        // ======================== RENDERING ========================

        public override bool PreDraw(ref Color lightColor)
        {
            if (Main.dedServ) return false;

            _glowTex ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/SandboxLastPrism/Orbs/SoftGlow");
            _flareTex ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/SandboxLastPrism/Flare/flare_16");

            SpriteBatch sb = Main.spriteBatch;
            float opacity = 1f - Projectile.alpha / 255f;

            DrawSpectralTrail(sb, opacity);
            DrawBladeSprite(sb, lightColor, opacity);
            DrawCosmicGlow(sb, opacity);

            return false;
        }

        private void DrawSpectralTrail(SpriteBatch sb, float opacity)
        {
            if (_trailCount < 3) return;

            var shader = RequiemShaderLoader.GetSwingTrail();
            if (shader == null) return;

            try
            {
                shader.UseColor(RequiemUtils.BrightCrimson.ToVector3());
                shader.UseSecondaryColor(RequiemUtils.DarkPink.ToVector3());
                shader.Shader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 4f);
                shader.Shader.Parameters["uOpacity"]?.SetValue(0.7f * opacity);
                shader.Shader.Parameters["uIntensity"]?.SetValue(1.5f);
                shader.Shader.Parameters["uOverbrightMult"]?.SetValue(2.0f);

                RequiemTrailRenderer.RenderTrail(_trailPositions, new RequiemTrailSettings(
                    (p, _) => 18f * (1f - p * 0.7f),
                    (p) => RequiemUtils.Additive(RequiemUtils.GetCosmicGradient(p), (1f - p) * opacity),
                    shader: shader), _trailCount, 2);
            }
            catch { }
        }

        private void DrawBladeSprite(SpriteBatch sb, Color lightColor, float opacity)
        {
            try
            {
                Texture2D tex = TextureAssets.Projectile[Projectile.type].Value;
                Vector2 origin = tex.Size() / 2f;

                // Ghostly spectral tint
                Color bladeColor = Color.Lerp(RequiemUtils.ConstellationSilver, RequiemUtils.DarkPink, 0.3f) * opacity;

                float drawRot = _spinRotation;
                sb.Draw(tex, Projectile.Center - Main.screenPosition, null, bladeColor, drawRot,
                    origin, Projectile.scale * 0.9f, SpriteEffects.None, 0f);

                // Afterimage layer
                Color afterColor = RequiemUtils.FatePurple * (opacity * 0.3f);
                sb.Draw(tex, Projectile.Center - Main.screenPosition + new Vector2(2f, 2f), null, afterColor,
                    drawRot - 0.1f, origin, Projectile.scale * 0.85f, SpriteEffects.None, 0f);
            }
            catch { }
        }

        private void DrawCosmicGlow(SpriteBatch sb, float opacity)
        {
            if (_glowTex?.Value == null) return;

            try
            {
                RequiemUtils.BeginAdditive(sb);

                var tex = _glowTex.Value;
                Vector2 origin = tex.Size() / 2f;
                Vector2 drawPos = Projectile.Center - Main.screenPosition;

                // Phase-dependent glow
                float glowScale = _phase switch
                {
                    0 => 0.3f + (float)_phaseTimer / RISE_DURATION * 0.3f,
                    1 => 0.6f + MathF.Sin(_phaseTimer * 0.3f) * 0.1f,
                    2 => 1.0f - (float)_phaseTimer / DETONATE_DURATION * 0.5f,
                    3 => 0.4f,
                    4 => 0.5f + MathF.Sin(_phaseTimer * 0.8f) * 0.15f,
                    _ => 0.3f * (1f - (float)_phaseTimer / RETURN_DURATION)
                };

                Color innerCol = RequiemUtils.Additive(RequiemUtils.BrightCrimson, 0.5f * opacity);
                Color outerCol = RequiemUtils.Additive(RequiemUtils.FatePurple, 0.25f * opacity);

                sb.Draw(tex, drawPos, null, outerCol, 0f, origin, glowScale * 1.5f, SpriteEffects.None, 0f);
                sb.Draw(tex, drawPos, null, innerCol, 0f, origin, glowScale, SpriteEffects.None, 0f);

                RequiemUtils.EndAdditive(sb);
            }
            catch
            {
                try { RequiemUtils.EndAdditive(sb); } catch { }
            }
        }
    }
}
