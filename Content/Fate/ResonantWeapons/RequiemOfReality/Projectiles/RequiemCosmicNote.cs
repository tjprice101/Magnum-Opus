using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
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
    /// Requiem Cosmic Music Note — Two-phase seeking projectile.
    /// Spawned by the main swing. Each note has a random visual type (quarter, eighth, treble, bass).
    ///
    /// PHASE SYSTEM:
    ///   Phase 0 — FLOAT (Fermata):   Drift outward, slowly decelerate, bob gently
    ///   Phase 1 — SEEK (Allegro):    Lock onto nearest enemy, accelerate with homing
    ///
    /// ai[0] = note visual type (0-3)
    /// ai[1] = frames before seeking begins
    /// </summary>
    public class RequiemCosmicNote : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles Asset Library/MusicNote";

        // Note type textures
        private static readonly string[] NoteTexturePaths = {
            "MagnumOpus/Assets/Particles Asset Library/MusicNote",
            "MagnumOpus/Assets/Particles Asset Library/CursiveMusicNote",
            "MagnumOpus/Assets/Particles Asset Library/MusicNoteWithSlashes",
            "MagnumOpus/Assets/Particles Asset Library/QuarterNote"
        };

        private static Asset<Texture2D>[] _noteTextures;
        private static Asset<Texture2D> _glowTex;

        // State
        private int _noteType;
        private int _seekDelay;
        private bool _seeking;
        private float _bobPhase;
        private NPC _lockedTarget;

        // Trail
        private Vector2[] _trailPositions = new Vector2[12];
        private int _trailCount;

        private Player Owner => Main.player[Projectile.owner];

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 300;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.alpha = 50;
        }

        public override void OnSpawn(IEntitySource source)
        {
            _noteType = (int)MathHelper.Clamp(Projectile.ai[0], 0, 3);
            _seekDelay = (int)MathHelper.Clamp(Projectile.ai[1], 20, 90);
            _seeking = false;
            _bobPhase = Main.rand.NextFloat(MathHelper.TwoPi);
        }

        public override void AI()
        {
            Projectile.ai[1]--;

            if (!_seeking && Projectile.ai[1] <= 0)
            {
                _seeking = true;
                _lockedTarget = FindBestTarget(600f);
            }

            if (_seeking)
                SeekingAI();
            else
                FloatingAI();

            // Constant rotation
            Projectile.rotation += 0.04f + (Projectile.velocity.Length() * 0.01f);

            UpdateTrail();
            SpawnTrailParticles();

            // Lighting
            Color lightCol = GetNoteColor();
            Lighting.AddLight(Projectile.Center, lightCol.ToVector3() * 0.4f);
        }

        private void FloatingAI()
        {
            // Decelerate
            Projectile.velocity *= 0.96f;

            // Gentle bob (sinusoidal)
            _bobPhase += 0.06f;
            Projectile.position.Y += MathF.Sin(_bobPhase) * 0.4f;

            // Slight drift
            Projectile.position.X += MathF.Cos(_bobPhase * 0.7f) * 0.15f;
        }

        private void SeekingAI()
        {
            // Re-acquire target if lost
            if (_lockedTarget == null || !_lockedTarget.active || _lockedTarget.dontTakeDamage)
            {
                _lockedTarget = FindBestTarget(600f);
                if (_lockedTarget == null)
                {
                    // No target: drift toward mouse
                    Vector2 toMouse = (Main.MouseWorld - Projectile.Center).SafeNormalize(Vector2.Zero);
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, toMouse * 8f, 0.05f);
                    return;
                }
            }

            // Homing
            Vector2 toTarget = (_lockedTarget.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
            float targetSpeed = 14f;
            float turnRate = 0.12f;

            Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * targetSpeed, turnRate);

            // Clamp speed
            float speed = Projectile.velocity.Length();
            if (speed > 18f)
                Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.Zero) * 18f;
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

        private void SpawnTrailParticles()
        {
            if (Main.dedServ) return;

            // Trailing motes when seeking
            if (_seeking && Main.rand.NextBool(2))
            {
                RequiemParticleHandler.SpawnParticle(new RequiemMoteParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(6f, 6f),
                    -Projectile.velocity * 0.05f + Main.rand.NextVector2Circular(0.3f, 0.3f),
                    GetNoteColor(), 0.12f, 12));
            }

            // Gentle ambient sparkles when floating
            if (!_seeking && Main.rand.NextBool(5))
            {
                RequiemParticleHandler.SpawnParticle(new RequiemSparkParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(10f, 10f),
                    Main.rand.NextVector2Circular(0.5f, 0.5f),
                    RequiemUtils.ConstellationSilver, 0.1f, 15));
            }
        }

        private Color GetNoteColor()
        {
            return _noteType switch
            {
                0 => RequiemUtils.BrightCrimson,
                1 => RequiemUtils.DarkPink,
                2 => RequiemUtils.ConstellationSilver,
                3 => RequiemUtils.CosmicRose,
                _ => RequiemUtils.BrightCrimson
            };
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<DestinyCollapse>(), 180);

            // Impact explosion: themed to note type
            if (Main.dedServ) return;

            // Bloom flash
            RequiemParticleHandler.SpawnParticle(new RequiemBloomFlare(
                target.Center, GetNoteColor(), 0.4f, 10));
            RequiemParticleHandler.SpawnParticle(new RequiemBloomFlare(
                target.Center, RequiemUtils.SupernovaWhite, 0.25f, 8));

            // Scatter sparks
            for (int i = 0; i < 5; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2Unit() * Main.rand.NextFloat(2f, 6f);
                RequiemParticleHandler.SpawnParticle(new RequiemSparkParticle(
                    target.Center, sparkVel, GetNoteColor(), 0.2f, 10));
            }

            // Scatter small notes
            for (int i = 0; i < 2; i++)
            {
                Vector2 noteVel = Main.rand.NextVector2Unit() * Main.rand.NextFloat(1f, 3f);
                noteVel.Y -= 1f;
                RequiemParticleHandler.SpawnParticle(new RequiemNoteParticle(
                    target.Center, noteVel, GetNoteColor(), 0.2f, 18));
            }

            // Sound — musical chime, pitch varies by note type
            SoundEngine.PlaySound(SoundID.Item26 with
            {
                Pitch = -0.3f + _noteType * 0.25f,
                Volume = 0.5f
            }, target.Center);
        }

        public override void OnKill(int timeLeft)
        {
            if (Main.dedServ) return;

            // Gentle fade-out particles
            for (int i = 0; i < 3; i++)
            {
                RequiemParticleHandler.SpawnParticle(new RequiemMoteParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(8f, 8f),
                    Main.rand.NextVector2Circular(1f, 1f),
                    GetNoteColor(), 0.15f, 15));
            }
        }

        // ======================== RENDERING ========================

        public override bool PreDraw(ref Color lightColor)
        {
            if (Main.dedServ) return false;

            // Load textures
            if (_noteTextures == null)
            {
                _noteTextures = new Asset<Texture2D>[4];
                for (int i = 0; i < 4; i++)
                    _noteTextures[i] = ModContent.Request<Texture2D>(NoteTexturePaths[i]);
            }
            _glowTex ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/SandboxLastPrism/Orbs/SoftGlow");

            SpriteBatch sb = Main.spriteBatch;
            float opacity = 1f - Projectile.alpha / 255f;

            DrawNoteTrail(sb, opacity);
            DrawNoteGlow(sb, opacity);
            DrawNote(sb, lightColor, opacity);

            return false;
        }

        private void DrawNoteTrail(SpriteBatch sb, float opacity)
        {
            if (_trailCount < 3 || !_seeking) return;

            var shader = RequiemShaderLoader.GetNoteTrail();
            if (shader == null) return;

            try
            {
                shader.UseColor(GetNoteColor().ToVector3());
                shader.UseSecondaryColor(RequiemUtils.CosmicVoid.ToVector3());
                shader.Shader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 3f);
                shader.Shader.Parameters["uOpacity"]?.SetValue(0.6f * opacity);
                shader.Shader.Parameters["uIntensity"]?.SetValue(1.2f);
                shader.Shader.Parameters["uFundamentalFreq"]?.SetValue(3f + _noteType * 0.5f);
                shader.Shader.Parameters["uHarmonicBlend"]?.SetValue(0.4f);

                RequiemTrailRenderer.RenderTrail(_trailPositions, new RequiemTrailSettings(
                    (p, _) => 6f * (1f - p * 0.8f),
                    (p) => RequiemUtils.Additive(GetNoteColor(), (1f - p) * opacity),
                    shader: shader), _trailCount, 2);
            }
            catch { }
        }

        private void DrawNoteGlow(SpriteBatch sb, float opacity)
        {
            if (_glowTex?.Value == null) return;

            try
            {
                RequiemUtils.BeginAdditive(sb);

                var tex = _glowTex.Value;
                Vector2 drawPos = Projectile.Center - Main.screenPosition;
                float pulse = 0.8f + MathF.Sin((float)Main.timeForVisualEffects * 0.1f + _bobPhase) * 0.2f;

                Color glowCol = RequiemUtils.Additive(GetNoteColor(), 0.35f * opacity * pulse);
                sb.Draw(tex, drawPos, null, glowCol, 0f, tex.Size() / 2f, 0.25f * pulse, SpriteEffects.None, 0f);

                RequiemUtils.EndAdditive(sb);
            }
            catch
            {
                try { RequiemUtils.EndAdditive(sb); } catch { }
            }
        }

        private void DrawNote(SpriteBatch sb, Color lightColor, float opacity)
        {
            int texIndex = (int)MathHelper.Clamp(_noteType, 0, 3);
            if (_noteTextures == null || _noteTextures[texIndex]?.Value == null) return;

            var tex = _noteTextures[texIndex].Value;
            Vector2 origin = tex.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            Color noteCol = Color.Lerp(lightColor, GetNoteColor(), 0.5f) * opacity;
            sb.Draw(tex, drawPos, null, noteCol, Projectile.rotation, origin, Projectile.scale * 0.4f,
                SpriteEffects.None, 0f);
        }
    }
}
