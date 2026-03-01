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
using MagnumOpus.Content.Fate.ResonantWeapons.TheConductorsLastConstellation.Utilities;
using MagnumOpus.Content.Fate.ResonantWeapons.TheConductorsLastConstellation.Particles;
using MagnumOpus.Content.Fate.ResonantWeapons.TheConductorsLastConstellation.Primitives;
using MagnumOpus.Content.Fate.ResonantWeapons.TheConductorsLastConstellation.Shaders;
using MagnumOpus.Content.Fate.Debuffs;

namespace MagnumOpus.Content.Fate.ResonantWeapons.TheConductorsLastConstellation.Projectiles
{
    /// <summary>
    /// The Conductor's Last Constellation — Main held swing projectile.
    ///
    /// Each swing is a different orchestral gesture — the sword IS the baton:
    ///
    /// 3-PHASE COMBO (Orchestral Movements):
    ///   Phase 0 (Downbeat):   Powerful downward sweep — spawns 3 descending beam columns
    ///   Phase 1 (Crescendo):  Rising sweep — beams intensify and widen
    ///   Phase 2 (Forte):      Wide horizontal sweep with lightning cascade
    ///                          On 3rd combo: Convergence — all beams converge on cursor
    ///
    /// 5-LAYER RENDERING:
    ///   Layer 1: Wide electric glow underlayer (ConductorSwingGlow shader)
    ///   Layer 2: Core lightning trail arc (ConductorSwingTrail shader)
    ///   Layer 3: Lightning spark accents along the arc (zigzag sparks!)
    ///   Layer 4: UV-rotated weapon sprite + tip conductor flare
    ///   Layer 5: Combo aura (electric rings when combo intensity high)
    /// </summary>
    public class ConductorSwingProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Content/Fate/ResonantWeapons/TheConductorsLastConstellation";

        // Swing arc parameters per phase
        // Phase 0: Downbeat (moderate arc, moderate speed — authoritative downstroke)
        // Phase 1: Crescendo (upward arc, fast — building energy)
        // Phase 2: Forte (wide horizontal, slow windup → explosive)
        private static readonly float[] ArcAngles = { 150f, 130f, 180f };
        private static readonly float[] SwingDurations = { 22f, 18f, 26f };
        private static readonly float[] DamageMultipliers = { 1f, 0.95f, 1.35f };

        // Trail system
        private Vector2[] _trailPoints = new Vector2[24];
        private int _trailCount;
        private float _currentAngle;
        private float _startAngle;
        private int _direction;
        private int _phase;
        private bool _firedBeams;

        // Textures (lazy loaded)
        private static Asset<Texture2D> _noiseTex;
        private static Asset<Texture2D> _glowTex;
        private static Asset<Texture2D> _flareTex;

        // Properties
        private Player Owner => Main.player[Projectile.owner];
        private float SwingProgress => Projectile.ai[1] / (SwingDurations[_phase] * 2f);
        private float ArcRadians => MathHelper.ToRadians(ArcAngles[_phase]);

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 24;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 80;
            Projectile.height = 80;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.ownerHitCheck = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.extraUpdates = 1;
        }

        public override void OnSpawn(IEntitySource source)
        {
            var cp = Owner.Conductor();
            _phase = cp.OnSwing();

            // Alternate direction, but Forte always sweeps horizontally
            if (_phase == 2)
                _direction = cp.SwingDirection;
            else
                _direction = cp.SwingDirection;

            Vector2 toMouse = (Main.MouseWorld - Owner.Center).SafeNormalize(Vector2.UnitX);
            float baseAngle = toMouse.ToRotation();

            switch (_phase)
            {
                case 0: // Downbeat: starts above, sweeps downward
                    _startAngle = baseAngle - MathHelper.PiOver2 * _direction;
                    break;
                case 1: // Crescendo: starts below, sweeps upward
                    _startAngle = baseAngle + MathHelper.PiOver4 * _direction;
                    break;
                case 2: // Forte: wide horizontal sweep
                    _startAngle = baseAngle - ArcRadians * 0.5f * _direction;
                    break;
            }

            _currentAngle = _startAngle;
            Projectile.timeLeft = (int)(SwingDurations[_phase] * 2) + 4;
            Projectile.damage = (int)(Projectile.damage * DamageMultipliers[_phase]);
            _firedBeams = false;
        }

        public override void AI()
        {
            Owner.heldProj = Projectile.whoAmI;
            Owner.itemTime = 2;
            Owner.itemAnimation = 2;
            Projectile.Center = Owner.MountedCenter;

            float duration = SwingDurations[_phase] * 2f;
            Projectile.ai[1]++;

            if (Projectile.ai[1] >= duration)
            {
                Projectile.Kill();
                return;
            }

            float progress = Projectile.ai[1] / duration;

            // Swing easing per phase
            float easedProgress;
            switch (_phase)
            {
                case 0: // Downbeat: powerful accelerating downstroke
                    easedProgress = ConductorUtils.QuadIn(progress);
                    break;
                case 1: // Crescendo: smooth building sweep
                    easedProgress = ConductorUtils.SineInOut(progress);
                    break;
                case 2: // Forte: slow windup → explosive horizontal sweep
                    easedProgress = ConductorUtils.ExpIn(progress);
                    break;
                default:
                    easedProgress = progress;
                    break;
            }

            _currentAngle = _startAngle + ArcRadians * easedProgress * _direction;

            float reach = _phase == 2 ? 100f : 85f;

            Projectile.rotation = _currentAngle;
            Vector2 tipPos = Owner.MountedCenter + _currentAngle.ToRotationVector2() * reach;

            // Update trail
            if (_trailCount < _trailPoints.Length)
            {
                _trailPoints[_trailCount] = tipPos;
                _trailCount++;
            }
            else
            {
                Array.Copy(_trailPoints, 1, _trailPoints, 0, _trailPoints.Length - 1);
                _trailPoints[_trailPoints.Length - 1] = tipPos;
            }

            Owner.ChangeDir(Math.Sign(_currentAngle.ToRotationVector2().X));

            // Fire 3 homing beams at swing midpoint
            if (!_firedBeams && progress > 0.35f && Projectile.owner == Main.myPlayer)
            {
                _firedBeams = true;
                FireSpectralBeams(tipPos);
            }

            SpawnSwingParticles(tipPos, progress);

            Color lightCol = ConductorUtils.PaletteLerp(progress);
            Lighting.AddLight(tipPos, lightCol.ToVector3() * 0.7f);

            if ((int)Projectile.ai[1] == (int)(duration * 0.3f))
            {
                SoundStyle sound = _phase == 2
                    ? SoundID.Item71 with { Pitch = -0.3f, Volume = 0.9f }
                    : SoundID.Item1 with { Pitch = 0.3f + _phase * 0.2f, Volume = 0.7f };
                SoundEngine.PlaySound(sound, Owner.Center);
            }
        }

        private void FireSpectralBeams(Vector2 tipPos)
        {
            Vector2 toMouse = (Main.MouseWorld - Owner.Center).SafeNormalize(Vector2.UnitX);
            float baseAngle = toMouse.ToRotation();
            float spreadAngle = MathHelper.ToRadians(18f);

            for (int i = 0; i < 3; i++)
            {
                float angleOffset = MathHelper.Lerp(-spreadAngle, spreadAngle, (i + 0.5f) / 3f);
                Vector2 projVelocity = (baseAngle + angleOffset).ToRotationVector2() * 14f;
                Vector2 spawnPos = tipPos + projVelocity.SafeNormalize(Vector2.Zero) * 20f;

                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    spawnPos, projVelocity,
                    ModContent.ProjectileType<ConductorSwordBeam>(),
                    Projectile.damage, Projectile.knockBack,
                    Projectile.owner, _phase, 0f);
            }
        }

        private void SpawnSwingParticles(Vector2 tipPos, float progress)
        {
            if (Main.dedServ) return;

            var cp = Owner.Conductor();
            float intensity = 0.5f + cp.ComboIntensity * 0.5f;

            // Lightning sparks at blade tip — zigzag motion!
            if (Main.rand.NextBool(2))
            {
                Vector2 sparkVel = (_currentAngle + MathHelper.PiOver2 * _direction).ToRotationVector2() * Main.rand.NextFloat(3f, 7f);
                Color sparkCol = ConductorUtils.GetLightningGradient(Main.rand.NextFloat());
                ConductorParticleHandler.SpawnParticle(new LightningSpark(
                    tipPos + Main.rand.NextVector2Circular(5f, 5f), sparkVel,
                    sparkCol, 0.25f * intensity, 16, 4f, 0.5f));
            }

            // Phase-specific particles
            switch (_phase)
            {
                case 0: // Downbeat: descending conductor motes
                    if (Main.rand.NextBool(3))
                    {
                        Vector2 moteVel = new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(1f, 3f));
                        Color moteCol = ConductorUtils.GetConductorShimmer((float)Main.timeForVisualEffects * 0.05f + Main.rand.NextFloat());
                        ConductorParticleHandler.SpawnParticle(new ConductorMote(
                            tipPos + Main.rand.NextVector2Circular(10f, 10f), moteVel,
                            moteCol, 0.2f, 25));
                    }
                    break;

                case 1: // Crescendo: rising energy motes
                    if (Main.rand.NextBool(3))
                    {
                        Vector2 riseVel = new Vector2(Main.rand.NextFloat(-1f, 1f), -Main.rand.NextFloat(2f, 5f));
                        Color riseCol = Color.Lerp(ConductorUtils.ConductorCyan, ConductorUtils.LightningGold, Main.rand.NextFloat());
                        ConductorParticleHandler.SpawnParticle(new ConductorSpark(
                            tipPos + Main.rand.NextVector2Circular(8f, 8f), riseVel,
                            riseCol, 0.22f * intensity, 18));
                    }
                    break;

                case 2: // Forte: heavy nebula wisps + electric bloom flares
                    if (Main.rand.NextBool(3))
                    {
                        Vector2 wispVel = Main.rand.NextVector2Circular(1.5f, 1.5f);
                        Color wispCol = Color.Lerp(ConductorUtils.BatonPurple, ConductorUtils.ConductorCyan, Main.rand.NextFloat());
                        ConductorParticleHandler.SpawnParticle(new ConductorNebulaWisp(
                            tipPos + Main.rand.NextVector2Circular(15f, 15f), wispVel,
                            wispCol, 0.22f, 40));
                    }
                    if (progress > 0.7f && Main.rand.NextBool(2))
                    {
                        Color flareCol = ConductorUtils.WithWhitePush(ConductorUtils.LightningGold, progress - 0.7f);
                        ConductorParticleHandler.SpawnParticle(new ConductorBloomFlare(
                            tipPos, flareCol, 0.4f * intensity, 12));
                    }
                    break;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<DestinyCollapse>(), 300);
            SpawnImpactVFX(target.Center);

            var cp = Owner.Conductor();

            // 3 cosmic lightning strikes
            for (int strike = 0; strike < 3; strike++)
            {
                Vector2 strikeOffset = Main.rand.NextVector2Circular(25f, 25f);
                SpawnLightningStrikeVFX(target.Center + strikeOffset, 1.0f + strike * 0.15f);
            }

            // 5 seeking crystal shards at 25% dmg (particle-only — no shared system)
            if (Projectile.owner == Main.myPlayer)
            {
                for (int i = 0; i < 5; i++)
                {
                    float angle = MathHelper.TwoPi * i / 5f + Main.rand.NextFloat(-0.2f, 0.2f);
                    Vector2 shardVel = angle.ToRotationVector2() * Main.rand.NextFloat(6f, 10f);
                    Projectile.NewProjectile(
                        Projectile.GetSource_OnHit(target),
                        target.Center + Main.rand.NextVector2Circular(10f, 10f),
                        shardVel,
                        ModContent.ProjectileType<ConductorSwordBeam>(),
                        (int)(damageDone * 0.25f),
                        Projectile.knockBack * 0.3f,
                        Projectile.owner,
                        3f, // ai[0] = 3 means "crystal shard mode"
                        0f);
                }
            }

            // Convergence on 3rd combo hit (Forte)
            if (_phase == 2 && cp.JustTriggeredConvergence)
            {
                SpawnConvergenceVFX(target.Center);
                SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.2f, Volume = 1.0f }, target.Center);
            }
        }

        private void SpawnImpactVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            var cp = Owner.Conductor();
            float intensity = 0.6f + cp.ComboIntensity * 0.4f;

            // Central bloom flash
            ConductorParticleHandler.SpawnParticle(new ConductorBloomFlare(
                pos, ConductorUtils.CelestialWhite, 0.6f * intensity, 15));
            ConductorParticleHandler.SpawnParticle(new ConductorBloomFlare(
                pos, ConductorUtils.ConductorCyan, 0.45f * intensity, 12));

            // Radial lightning spark burst (zigzag!)
            int sparkCount = 8 + (int)(cp.ComboIntensity * 4);
            for (int i = 0; i < sparkCount; i++)
            {
                float angle = MathHelper.TwoPi * i / sparkCount + Main.rand.NextFloat(-0.1f, 0.1f);
                Vector2 sparkVel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 8f) * intensity;
                Color sparkCol = ConductorUtils.GetLightningGradient((float)i / sparkCount);
                ConductorParticleHandler.SpawnParticle(new LightningSpark(
                    pos, sparkVel, sparkCol, 0.3f * intensity, 18, 3f, 0.4f));
            }

            // Glyph accents
            int glyphCount = 2 + (int)(cp.ComboIntensity * 3);
            for (int i = 0; i < glyphCount; i++)
            {
                Vector2 glyphPos = pos + Main.rand.NextVector2Circular(20f, 20f);
                Color glyphCol = ConductorUtils.PaletteLerp(Main.rand.NextFloat());
                ConductorParticleHandler.SpawnParticle(new ConductorGlyph(
                    glyphPos, glyphCol, 0.28f * intensity, 25));
            }

            Lighting.AddLight(pos, ConductorUtils.LightningGold.ToVector3() * 1.0f * intensity);
        }

        private void SpawnLightningStrikeVFX(Vector2 pos, float intensity)
        {
            if (Main.dedServ) return;

            // Vertical line of zigzag lightning sparks
            for (int i = 0; i < 12; i++)
            {
                Vector2 strikePos = pos + new Vector2(Main.rand.NextFloat(-8f, 8f), -i * 12f);
                Vector2 sparkVel = new Vector2(Main.rand.NextFloat(-3f, 3f), -Main.rand.NextFloat(1f, 4f));
                Color col = i < 4 ? ConductorUtils.LightningGold : ConductorUtils.ConductorCyan;
                ConductorParticleHandler.SpawnParticle(new LightningSpark(
                    strikePos, sparkVel, col, 0.28f * intensity, 14, 5f, 0.6f));
            }

            // Flash at strike point
            ConductorParticleHandler.SpawnParticle(new ConductorBloomFlare(
                pos, ConductorUtils.CelestialWhite, 0.5f * intensity, 10));
            ConductorParticleHandler.SpawnParticle(new ConductorBloomFlare(
                pos, ConductorUtils.LightningGold, 0.4f * intensity, 8));

            Lighting.AddLight(pos, ConductorUtils.LightningGold.ToVector3() * 1.2f * intensity);
        }

        private void SpawnConvergenceVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            // Massive bloom flash
            ConductorParticleHandler.SpawnParticle(new ConductorBloomFlare(
                pos, ConductorUtils.CelestialWhite, 1.2f, 20));
            ConductorParticleHandler.SpawnParticle(new ConductorBloomFlare(
                pos, ConductorUtils.ConductorCyan, 0.9f, 18));
            ConductorParticleHandler.SpawnParticle(new ConductorBloomFlare(
                pos, ConductorUtils.BatonPurple, 0.7f, 16));

            // Zigzag lightning ring
            for (int i = 0; i < 16; i++)
            {
                float angle = MathHelper.TwoPi * i / 16f;
                Vector2 sparkVel = angle.ToRotationVector2() * Main.rand.NextFloat(5f, 12f);
                Color sparkCol = ConductorUtils.GetLightningGradient((float)i / 16f);
                ConductorParticleHandler.SpawnParticle(new LightningSpark(
                    pos, sparkVel, sparkCol, 0.35f, 22, 6f, 0.5f));
            }

            // Radial conductor sparks
            for (int i = 0; i < 20; i++)
            {
                float angle = MathHelper.TwoPi * i / 20f + Main.rand.NextFloat(-0.1f, 0.1f);
                Vector2 sparkVel = angle.ToRotationVector2() * Main.rand.NextFloat(6f, 14f);
                Color sparkCol = ConductorUtils.GetLightningGradient((float)i / 20f);
                ConductorParticleHandler.SpawnParticle(new ConductorSpark(
                    pos, sparkVel, sparkCol, 0.35f, 20));
            }

            // Glyph ring
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 glyphPos = pos + angle.ToRotationVector2() * 60f;
                ConductorParticleHandler.SpawnParticle(new ConductorGlyph(
                    glyphPos, ConductorUtils.StarSilver, 0.45f, 35));
            }

            // Nebula wisps expanding outward
            for (int i = 0; i < 10; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 wispVel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 5f);
                Color wispCol = Color.Lerp(ConductorUtils.BatonPurple, ConductorUtils.ConductorCyan, Main.rand.NextFloat());
                ConductorParticleHandler.SpawnParticle(new ConductorNebulaWisp(
                    pos + Main.rand.NextVector2Circular(20f, 20f), wispVel,
                    wispCol, 0.3f, 45));
            }
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float reach = _phase == 2 ? 100f : 85f;
            Vector2 start = Owner.MountedCenter;
            Vector2 end = start + _currentAngle.ToRotationVector2() * reach;
            float _ = 0f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, 30f, ref _);
        }

        // ======================== 5-LAYER RENDERING ========================

        public override bool PreDraw(ref Color lightColor)
        {
            if (Main.dedServ || _trailCount < 2) return false;

            _noiseTex ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/NoiseTextures/MusicalWavePattern");
            _glowTex ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/SandboxLastPrism/Orbs/SoftGlow");
            _flareTex ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/SandboxLastPrism/Flare/flare_16");

            SpriteBatch sb = Main.spriteBatch;
            var cp = Owner.Conductor();
            float comboIntensity = cp.ComboIntensity;
            float progress = SwingProgress;

            DrawLayer1_ElectricGlow(sb, comboIntensity);
            DrawLayer2_CoreTrail(sb, comboIntensity);
            DrawLayer3_LightningSparks(sb, progress, comboIntensity);
            DrawLayer4_WeaponSprite(sb, lightColor);
            DrawLayer5_ComboAura(sb, comboIntensity);

            return false;
        }

        /// <summary>Layer 1: Wide soft electric glow underlayer via shader.</summary>
        private void DrawLayer1_ElectricGlow(SpriteBatch sb, float combo)
        {
            var shader = ConductorShaderLoader.GetSwingGlow();
            if (shader == null || _trailCount < 2) return;

            try
            {
                shader.UseColor(ConductorUtils.VoidBlack.ToVector3());
                shader.UseSecondaryColor(ConductorUtils.BatonPurple.ToVector3());
                shader.Shader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 2f);
                shader.Shader.Parameters["uOpacity"]?.SetValue(0.4f + combo * 0.2f);
                shader.Shader.Parameters["uIntensity"]?.SetValue(1.2f);
                shader.Shader.Parameters["uOverbrightMult"]?.SetValue(1.5f);

                ConductorTrailRenderer.RenderTrail(_trailPoints, new ConductorTrailSettings(
                    (p, _) => (42f + combo * 15f) * (1f - p * 0.5f),
                    (p) => Color.Lerp(ConductorUtils.Additive(ConductorUtils.BatonPurple, 0.3f),
                                      ConductorUtils.Additive(ConductorUtils.VoidBlack, 0.1f), p),
                    shader: shader), _trailCount, 3);
            }
            catch { }
        }

        /// <summary>Layer 2: Core trail arc with conductor lightning shader.</summary>
        private void DrawLayer2_CoreTrail(SpriteBatch sb, float combo)
        {
            var shader = ConductorShaderLoader.GetSwingTrail();
            if (shader == null || _trailCount < 2) return;

            try
            {
                if (_noiseTex?.Value != null)
                    shader.UseImage1(_noiseTex);

                shader.UseColor(ConductorUtils.ConductorCyan.ToVector3());
                shader.UseSecondaryColor(ConductorUtils.BatonPurple.ToVector3());
                shader.Shader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 3f);
                shader.Shader.Parameters["uOpacity"]?.SetValue(0.8f + combo * 0.2f);
                shader.Shader.Parameters["uIntensity"]?.SetValue(1.5f + combo * 0.5f);
                shader.Shader.Parameters["uOverbrightMult"]?.SetValue(1.8f);
                shader.Shader.Parameters["uScrollSpeed"]?.SetValue(1.4f);
                shader.Shader.Parameters["uNoiseScale"]?.SetValue(2.5f);
                shader.Shader.Parameters["uPhase"]?.SetValue(combo);
                shader.Shader.Parameters["uHasSecondaryTex"]?.SetValue(_noiseTex?.Value != null ? 1f : 0f);
                shader.Shader.Parameters["uSecondaryTexScale"]?.SetValue(3f);

                ConductorTrailRenderer.RenderTrail(_trailPoints, new ConductorTrailSettings(
                    (p, _) =>
                    {
                        float baseWidth = 24f + combo * 8f;
                        float taper = MathF.Sin(p * MathHelper.Pi);
                        return baseWidth * (0.3f + taper * 0.7f);
                    },
                    (p) =>
                    {
                        Color c = ConductorUtils.GetLightningGradient(p);
                        float alpha = (1f - p * 0.6f);
                        return ConductorUtils.Additive(c, alpha);
                    },
                    shader: shader), _trailCount, 3);
            }
            catch { }
        }

        /// <summary>Layer 3: Lightning bolt forks — zigzag sparks with forking branches and arc nodes.</summary>
        private void DrawLayer3_LightningSparks(SpriteBatch sb, float progress, float combo)
        {
            if (_flareTex?.Value == null || _trailCount < 3) return;

            try
            {
                ConductorUtils.BeginAdditive(sb);

                var tex = _flareTex.Value;
                Vector2 origin = tex.Size() / 2f;
                float time = (float)Main.timeForVisualEffects;

                // === Primary lightning bolt nodes along the arc ===
                for (int i = 1; i < _trailCount - 1; i += 2)
                {
                    float t = (float)i / _trailCount;
                    float sparkAlpha = (1f - t) * (0.3f + combo * 0.4f);

                    // Rapid electric flicker (reforms 6x/sec for jagged feel)
                    float flickerSeed = MathF.Floor(time * 0.1f + i * 0.7f);
                    float flicker = (MathF.Sin(flickerSeed * 127.1f) * 0.5f + 0.5f) * 0.4f + 0.6f;
                    sparkAlpha *= flicker;

                    if (sparkAlpha < 0.05f) continue;

                    // Zigzag offset perpendicular to trail (sharper displacement)
                    float zigzag = MathF.Sin(i * 1.7f + time * 0.15f) * 8f;
                    Vector2 tangent = Vector2.Zero;
                    if (i > 0 && i < _trailCount - 1)
                        tangent = _trailPoints[i + 1] - _trailPoints[i - 1];
                    if (tangent != Vector2.Zero) tangent.Normalize();
                    Vector2 perp = new(-tangent.Y, tangent.X);

                    Color sparkCol = ConductorUtils.GetConductorShimmer(time * 0.03f + t * 5f);
                    Vector2 drawPos = _trailPoints[i] + perp * zigzag - Main.screenPosition;
                    float sparkScale = 0.15f + combo * 0.08f;

                    // Primary arc node (brighter, with cyan core)
                    sb.Draw(tex, drawPos, null, ConductorUtils.Additive(sparkCol, sparkAlpha),
                        time * 0.3f + i, origin, sparkScale * flicker, SpriteEffects.None, 0f);
                    sb.Draw(tex, drawPos, null, ConductorUtils.Additive(ConductorUtils.CelestialWhite, sparkAlpha * 0.4f),
                        0f, origin, sparkScale * 0.3f, SpriteEffects.None, 0f);

                    // === Forking branch bolts — spawn 1-2 short forks from brighter nodes ===
                    if (sparkAlpha > 0.15f && i < _trailCount - 3)
                    {
                        int forkCount = sparkAlpha > 0.25f ? 2 : 1;
                        for (int f = 0; f < forkCount; f++)
                        {
                            float forkAngle = (MathF.Sin(flickerSeed * (37.7f + f * 91.3f)) - 0.5f) * MathHelper.PiOver2;
                            Vector2 forkDir = (tangent.ToRotation() + forkAngle + perp.ToRotation() * 0.3f).ToRotationVector2();
                            float forkLen = 12f + MathF.Sin(flickerSeed * 53.1f) * 8f;
                            Vector2 forkEnd = drawPos + forkDir * forkLen;

                            // Draw fork as stretched flare
                            Vector2 fDelta = forkEnd - drawPos;
                            float fDist = fDelta.Length();
                            if (fDist > 2f)
                            {
                                float fAngle = fDelta.ToRotation();
                                sb.Draw(tex, drawPos, null,
                                    ConductorUtils.Additive(ConductorUtils.ConductorCyan, sparkAlpha * 0.5f),
                                    fAngle, new Vector2(0, tex.Height * 0.5f),
                                    new Vector2(fDist / tex.Width, 0.04f), SpriteEffects.None, 0f);

                                // Tiny terminus spark at fork end
                                sb.Draw(tex, forkEnd, null,
                                    ConductorUtils.Additive(ConductorUtils.CelestialWhite, sparkAlpha * 0.3f),
                                    time + f * 2f, origin, sparkScale * 0.4f, SpriteEffects.None, 0f);
                            }
                        }
                    }
                }

                // === Connecting bolt segments between consecutive arc nodes ===
                for (int i = 1; i < _trailCount - 3; i += 2)
                {
                    int j = i + 2;
                    if (j >= _trailCount) break;

                    float tAvg = (float)(i + j) / (2f * _trailCount);
                    float segAlpha = (1f - tAvg) * (0.1f + combo * 0.15f);
                    if (segAlpha < 0.03f) continue;

                    Vector2 posA = _trailPoints[i] - Main.screenPosition;
                    Vector2 posB = _trailPoints[j] - Main.screenPosition;

                    // Thin electric connecting bolt
                    Vector2 delta = posB - posA;
                    float dist = delta.Length();
                    if (dist < 3f) continue;
                    float angle = delta.ToRotation();
                    sb.Draw(tex, posA, null,
                        ConductorUtils.Additive(ConductorUtils.ConductorCyan, segAlpha),
                        angle, new Vector2(0, tex.Height * 0.5f),
                        new Vector2(dist / tex.Width, 0.025f), SpriteEffects.None, 0f);
                }

                ConductorUtils.EndAdditive(sb);
            }
            catch
            {
                try { ConductorUtils.EndAdditive(sb); } catch { }
            }
        }

        /// <summary>Layer 4: UV-rotated weapon sprite at current angle + tip conductor flare.</summary>
        private void DrawLayer4_WeaponSprite(SpriteBatch sb, Color lightColor)
        {
            try
            {
                Texture2D weaponTex = TextureAssets.Projectile[Projectile.type].Value;
                Vector2 origin = weaponTex.Size() / 2f;
                float reach = _phase == 2 ? 100f : 85f;
                Vector2 drawPos = Owner.MountedCenter + _currentAngle.ToRotationVector2() * reach * 0.5f - Main.screenPosition;

                Color weaponColor = Color.Lerp(lightColor, ConductorUtils.StarSilver, 0.3f);

                float drawRotation = _currentAngle + MathHelper.PiOver4;
                SpriteEffects fx = _direction < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
                sb.Draw(weaponTex, drawPos, null, weaponColor, drawRotation, origin, Projectile.scale, fx, 0f);

                // Tip conductor flare (additive)
                if (_glowTex?.Value != null)
                {
                    Vector2 tipPos = Owner.MountedCenter + _currentAngle.ToRotationVector2() * reach - Main.screenPosition;
                    float flarePulse = 0.8f + MathF.Sin((float)Main.timeForVisualEffects * 0.08f) * 0.2f;

                    ConductorUtils.BeginAdditive(sb);
                    sb.Draw(_glowTex.Value, tipPos, null, ConductorUtils.Additive(ConductorUtils.ConductorCyan, 0.5f * flarePulse),
                        0f, _glowTex.Value.Size() / 2f, 0.35f * flarePulse, SpriteEffects.None, 0f);
                    sb.Draw(_glowTex.Value, tipPos, null, ConductorUtils.Additive(ConductorUtils.CelestialWhite, 0.3f * flarePulse),
                        0f, _glowTex.Value.Size() / 2f, 0.18f * flarePulse, SpriteEffects.None, 0f);
                    ConductorUtils.EndAdditive(sb);
                }
            }
            catch
            {
                try { ConductorUtils.EndAdditive(sb); } catch { }
            }
        }

        /// <summary>Layer 5: Combo aura — expanding electric pulse waves that radiate outward.</summary>
        private void DrawLayer5_ComboAura(SpriteBatch sb, float combo)
        {
            if (combo < 0.4f) return;
            if (_glowTex?.Value == null) return;

            try
            {
                float auraAlpha = (combo - 0.4f) / 0.6f;
                float time = (float)Main.timeForVisualEffects;

                ConductorUtils.BeginAdditive(sb);

                Vector2 center = Owner.MountedCenter - Main.screenPosition;
                var tex = _glowTex.Value;
                Vector2 origin = tex.Size() / 2f;

                // === Expanding pulse wave rings (like sonar pings radiating outward) ===
                int waveCount = 3;
                float wavePeriod = 1.8f; // seconds per wave cycle
                for (int w = 0; w < waveCount; w++)
                {
                    // Each wave is offset in phase so they stagger
                    float wavePhase = (time * 0.017f + w * (1f / waveCount)) % 1f;
                    float waveRadius = wavePhase * 100f;
                    float waveAlpha = auraAlpha * (1f - wavePhase) * 0.2f; // fades as it expands
                    float waveWidth = 0.04f + wavePhase * 0.02f; // thins slightly as it expands

                    if (waveAlpha < 0.02f) continue;

                    // Draw the expanding ring as multiple points on a circle
                    int segments = 12;
                    for (int s = 0; s < segments; s++)
                    {
                        float angle = s * MathHelper.TwoPi / segments;
                        Vector2 pos = center + angle.ToRotationVector2() * waveRadius;

                        // Alternate cyan and purple segments for electric feel
                        Color segCol = s % 2 == 0 ? ConductorUtils.ConductorCyan : ConductorUtils.BatonPurple;
                        sb.Draw(tex, pos, null, ConductorUtils.Additive(segCol, waveAlpha),
                            angle, origin, waveWidth * 3f, SpriteEffects.None, 0f);
                    }
                }

                // Central electric core (flickers)
                float coreFlicker = MathF.Sin(time * 0.35f) * 0.3f + 0.7f;
                sb.Draw(tex, center, null,
                    ConductorUtils.Additive(ConductorUtils.ConductorCyan, auraAlpha * 0.1f * coreFlicker),
                    0f, origin, 0.5f, SpriteEffects.None, 0f);

                ConductorUtils.EndAdditive(sb);
            }
            catch
            {
                try { ConductorUtils.EndAdditive(sb); } catch { }
            }
        }
    }
}
