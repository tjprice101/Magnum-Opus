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
using MagnumOpus.Content.Fate.ResonantWeapons.FractalOfTheStars.Utilities;
using MagnumOpus.Content.Fate.ResonantWeapons.FractalOfTheStars.Particles;
using MagnumOpus.Content.Fate.ResonantWeapons.FractalOfTheStars.Primitives;
using MagnumOpus.Content.Fate.ResonantWeapons.FractalOfTheStars.Shaders;
using MagnumOpus.Content.Fate.Debuffs;

namespace MagnumOpus.Content.Fate.ResonantWeapons.FractalOfTheStars.Projectiles
{
    /// <summary>
    /// Fractal of the Stars — Main held swing projectile.
    ///
    /// 3-PHASE COMBO:
    ///   Phase 0 (Horizontal Sweep):  Wide horizontal sweep across — constellation sparks scatter
    ///   Phase 1 (Rising Uppercut):   Fast upward diagonal slash — star particles rise upward
    ///   Phase 2 (Gravity Slam):      Overhead slam downward — Star Fracture explosion on hit
    ///
    /// 5-LAYER RENDERING:
    ///   Layer 1: Wide stellar glow underlayer (FractalSwingGlow shader)
    ///   Layer 2: Core constellation trail arc (FractalSwingTrail shader)
    ///   Layer 3: Star spark accents along the arc
    ///   Layer 4: UV-rotated weapon sprite + tip star flare
    ///   Layer 5: Combo aura (stellar rings when combo >= 2)
    /// </summary>
    public class FractalSwingProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Content/Fate/ResonantWeapons/FractalOfTheStars";

        // Swing arc parameters per phase
        // Phase 0: Horizontal Sweep (wide, moderate speed)
        // Phase 1: Rising Uppercut (narrow, fast)
        // Phase 2: Gravity Slam (narrow vertical, slow windup → fast slam)
        private static readonly float[] ArcAngles = { 170f, 130f, 120f };        // Degrees
        private static readonly float[] SwingDurations = { 22f, 16f, 24f };      // Frames
        private static readonly float[] DamageMultipliers = { 1f, 0.95f, 1.4f }; // Slam hits hardest

        // Trail system
        private Vector2[] _trailPoints = new Vector2[24];
        private int _trailCount;
        private float _currentAngle;
        private float _startAngle;
        private int _direction; // 1 or -1
        private int _phase;    // 0-2

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
            Projectile.localNPCHitCooldown = -1; // One hit per swing
            Projectile.extraUpdates = 1;         // Smoother motion
        }

        public override void OnSpawn(IEntitySource source)
        {
            var fp = Owner.Fractal();
            _phase = fp.OnSwing(); // Gets current phase BEFORE advancing

            // Direction: alternate based on swing count, but slam always comes down
            if (_phase == 2)
                _direction = 1; // Always top-to-bottom for slam
            else
                _direction = fp.SwingDirection;

            // Start angle based on phase
            Vector2 toMouse = (Main.MouseWorld - Owner.Center).SafeNormalize(Vector2.UnitX);
            float baseAngle = toMouse.ToRotation();

            switch (_phase)
            {
                case 0: // Horizontal sweep: arc centered on aim direction
                    _startAngle = baseAngle - ArcRadians * 0.5f * _direction;
                    break;
                case 1: // Rising uppercut: starts below, arcs upward
                    _startAngle = baseAngle + MathHelper.PiOver4 * _direction;
                    break;
                case 2: // Gravity slam: starts above, slams downward
                    _startAngle = baseAngle - MathHelper.PiOver2;
                    break;
            }

            _currentAngle = _startAngle;
            Projectile.timeLeft = (int)(SwingDurations[_phase] * 2) + 4;
            Projectile.damage = (int)(Projectile.damage * DamageMultipliers[_phase]);
        }

        public override void AI()
        {
            // Keep attached to player
            Owner.heldProj = Projectile.whoAmI;
            Owner.itemTime = 2;
            Owner.itemAnimation = 2;
            Projectile.Center = Owner.MountedCenter;

            float duration = SwingDurations[_phase] * 2f; // doubled by extraUpdates
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
                case 0: // Horizontal Sweep: smooth sine
                    easedProgress = FractalUtils.SineInOut(progress);
                    break;
                case 1: // Rising Uppercut: fast explosive start
                    easedProgress = FractalUtils.QuadOut(progress);
                    break;
                case 2: // Gravity Slam: slow windup → explosive slam
                    easedProgress = FractalUtils.ExpIn(progress);
                    break;
                default:
                    easedProgress = progress;
                    break;
            }

            // Calculate current angle
            _currentAngle = _startAngle + ArcRadians * easedProgress * _direction;

            // Weapon reach (longer on slam)
            float reach = _phase == 2 ? 100f : 82f;

            // Set rotation for collision and rendering
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

            // Player direction
            Owner.ChangeDir(Math.Sign(_currentAngle.ToRotationVector2().X));

            // Spawn particles along arc
            SpawnSwingParticles(tipPos, progress);

            // Lighting
            Color lightCol = FractalUtils.PaletteLerp(progress);
            Lighting.AddLight(tipPos, lightCol.ToVector3() * 0.7f);

            // Sound at midpoint
            if ((int)Projectile.ai[1] == (int)(duration * 0.3f))
            {
                SoundStyle sound = _phase == 2
                    ? SoundID.Item71 with { Pitch = -0.4f, Volume = 0.9f }
                    : SoundID.Item1 with { Pitch = 0.4f + _phase * 0.2f, Volume = 0.7f };
                SoundEngine.PlaySound(sound, Owner.Center);
            }
        }

        private void SpawnSwingParticles(Vector2 tipPos, float progress)
        {
            if (Main.dedServ) return;

            var fp = Owner.Fractal();
            float intensity = 0.5f + fp.ComboIntensity * 0.5f;

            // Star sparks at blade tip
            if (Main.rand.NextBool(2))
            {
                Vector2 sparkVel = (_currentAngle + MathHelper.PiOver2 * _direction).ToRotationVector2() * Main.rand.NextFloat(2f, 5f);
                Color sparkCol = FractalUtils.GetStellarGradient(Main.rand.NextFloat());
                FractalParticleHandler.SpawnParticle(new FractalSpark(
                    tipPos + Main.rand.NextVector2Circular(5f, 5f), sparkVel,
                    sparkCol, 0.25f * intensity, 14));
            }

            // Phase-specific particles
            switch (_phase)
            {
                case 0: // Horizontal Sweep: constellation star particles scatter
                    if (Main.rand.NextBool(4))
                    {
                        Vector2 starVel = Main.rand.NextVector2Circular(2f, 2f);
                        Color starCol = FractalUtils.GetStarShimmer((float)Main.timeForVisualEffects * 0.05f + Main.rand.NextFloat());
                        FractalParticleHandler.SpawnParticle(new FractalStarParticle(
                            tipPos + Main.rand.NextVector2Circular(10f, 10f), starVel,
                            starCol, 0.2f, 25));
                    }
                    break;

                case 1: // Rising Uppercut: star particles float upward
                    if (Main.rand.NextBool(3))
                    {
                        Vector2 riseVel = new Vector2(Main.rand.NextFloat(-1f, 1f), -Main.rand.NextFloat(2f, 4f));
                        Color riseCol = Color.Lerp(FractalUtils.StarGold, FractalUtils.ConstellationWhite, Main.rand.NextFloat());
                        FractalParticleHandler.SpawnParticle(new FractalMote(
                            tipPos + Main.rand.NextVector2Circular(8f, 8f), riseVel,
                            riseCol, 0.2f * intensity, 20));
                    }
                    break;

                case 2: // Gravity Slam: heavy nebula wisps + bloom flares near impact
                    if (Main.rand.NextBool(3))
                    {
                        Vector2 wispVel = Main.rand.NextVector2Circular(1f, 1f);
                        Color wispCol = Color.Lerp(FractalUtils.FractalPurple, FractalUtils.NebulaPink, Main.rand.NextFloat());
                        FractalParticleHandler.SpawnParticle(new FractalNebulaWisp(
                            tipPos + Main.rand.NextVector2Circular(15f, 15f), wispVel,
                            wispCol, 0.2f, 40));
                    }
                    if (progress > 0.7f && Main.rand.NextBool(2))
                    {
                        Color flareCol = FractalUtils.WithWhitePush(FractalUtils.StarGold, progress - 0.7f);
                        FractalParticleHandler.SpawnParticle(new FractalBloomFlare(
                            tipPos, flareCol, 0.4f * intensity, 12));
                    }
                    break;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<DestinyCollapse>(), 300);
            SpawnImpactVFX(target.Center);

            var fp = Owner.Fractal();

            // Spawn orbiting spectral star blades on every hit (max 6)
            if (fp.OrbitBladeCount < 6)
            {
                float orbitAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                Projectile.NewProjectile(
                    Projectile.GetSource_OnHit(target),
                    Owner.Center,
                    Vector2.Zero,
                    ModContent.ProjectileType<FractalOrbitBlade>(),
                    (int)(Projectile.damage * 0.4f),
                    Projectile.knockBack * 0.3f,
                    Projectile.owner,
                    orbitAngle, // ai[0] = starting orbit angle
                    0f);       // ai[1] = timer
            }

            // Star Fracture on 3rd combo hit (Gravity Slam)
            if (_phase == 2 && fp.JustTriggeredStarFracture)
            {
                SpawnStarFractureVFX(target.Center);
                SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.2f, Volume = 1.0f }, target.Center);
            }
        }

        private void SpawnImpactVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            var fp = Owner.Fractal();
            float intensity = 0.6f + fp.ComboIntensity * 0.4f;

            // Central bloom flash
            FractalParticleHandler.SpawnParticle(new FractalBloomFlare(
                pos, FractalUtils.SupernovaFlash, 0.6f * intensity, 15));
            FractalParticleHandler.SpawnParticle(new FractalBloomFlare(
                pos, FractalUtils.StarGold, 0.45f * intensity, 12));

            // Radial star spark burst
            int sparkCount = 8 + (int)(fp.ComboIntensity * 4);
            for (int i = 0; i < sparkCount; i++)
            {
                float angle = MathHelper.TwoPi * i / sparkCount + Main.rand.NextFloat(-0.1f, 0.1f);
                Vector2 sparkVel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 8f) * intensity;
                Color sparkCol = FractalUtils.GetStellarGradient((float)i / sparkCount);
                FractalParticleHandler.SpawnParticle(new FractalSpark(
                    pos, sparkVel, sparkCol, 0.3f * intensity, 16));
            }

            // Glyph accents
            int glyphCount = 2 + (int)(fp.ComboIntensity * 3);
            for (int i = 0; i < glyphCount; i++)
            {
                Vector2 glyphPos = pos + Main.rand.NextVector2Circular(20f, 20f);
                Color glyphCol = FractalUtils.PaletteLerp(Main.rand.NextFloat());
                FractalParticleHandler.SpawnParticle(new FractalGlyph(
                    glyphPos, glyphCol, 0.28f * intensity, 25));
            }

            // Star particles on impact
            for (int i = 0; i < 3; i++)
            {
                Vector2 starVel = Main.rand.NextVector2Circular(3f, 3f);
                starVel.Y -= 2f;
                Color starCol = FractalUtils.GetStarShimmer((float)Main.timeForVisualEffects * 0.03f + i);
                FractalParticleHandler.SpawnParticle(new FractalStarParticle(
                    pos + Main.rand.NextVector2Circular(10f, 10f), starVel,
                    starCol, 0.25f, 28));
            }

            Lighting.AddLight(pos, FractalUtils.StarGold.ToVector3() * 1.0f * intensity);
        }

        private void SpawnStarFractureVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            // Massive bloom flash
            FractalParticleHandler.SpawnParticle(new FractalBloomFlare(
                pos, FractalUtils.SupernovaFlash, 1.2f, 20));
            FractalParticleHandler.SpawnParticle(new FractalBloomFlare(
                pos, FractalUtils.StarGold, 0.9f, 18));
            FractalParticleHandler.SpawnParticle(new FractalBloomFlare(
                pos, FractalUtils.FractalPurple, 0.7f, 16));

            // Large star particle ring (fractal splitting stars!)
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 starVel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 8f);
                Color starCol = FractalUtils.GetStellarGradient((float)i / 8f);
                FractalParticleHandler.SpawnParticle(new FractalStarParticle(
                    pos, starVel, starCol, 0.4f, 35, 5));
            }

            // Radial spark explosion (20 sparks)
            for (int i = 0; i < 20; i++)
            {
                float angle = MathHelper.TwoPi * i / 20f + Main.rand.NextFloat(-0.1f, 0.1f);
                Vector2 sparkVel = angle.ToRotationVector2() * Main.rand.NextFloat(6f, 14f);
                Color sparkCol = FractalUtils.GetStellarGradient((float)i / 20f);
                FractalParticleHandler.SpawnParticle(new FractalSpark(
                    pos, sparkVel, sparkCol, 0.35f, 20));
            }

            // Glyph ring
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 glyphPos = pos + angle.ToRotationVector2() * 60f;
                FractalParticleHandler.SpawnParticle(new FractalGlyph(
                    glyphPos, FractalUtils.CelestialLavender, 0.45f, 35));
            }

            // Nebula wisps expanding outward
            for (int i = 0; i < 10; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 wispVel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 5f);
                Color wispCol = Color.Lerp(FractalUtils.FractalPurple, FractalUtils.NebulaPink, Main.rand.NextFloat());
                FractalParticleHandler.SpawnParticle(new FractalNebulaWisp(
                    pos + Main.rand.NextVector2Circular(20f, 20f), wispVel,
                    wispCol, 0.3f, 45));
            }
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float reach = _phase == 2 ? 100f : 82f;
            Vector2 start = Owner.MountedCenter;
            Vector2 end = start + _currentAngle.ToRotationVector2() * reach;
            float _ = 0f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, 30f, ref _);
        }

        // ======================== 5-LAYER RENDERING ========================

        public override bool PreDraw(ref Color lightColor)
        {
            if (Main.dedServ || _trailCount < 2) return false;

            _noiseTex ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/NoiseTextures/StarFieldScatter");
            _glowTex ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/SandboxLastPrism/Orbs/SoftGlow");
            _flareTex ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/SandboxLastPrism/Flare/flare_16");

            SpriteBatch sb = Main.spriteBatch;
            var fp = Owner.Fractal();
            float comboIntensity = fp.ComboIntensity;
            float progress = SwingProgress;

            try
            {
                // End SpriteBatch before GPU primitive trail draws
                sb.End();

                // GPU primitive layers (trail renderers use DrawUserIndexedPrimitives)
                DrawLayer1_StellarGlow(sb, comboIntensity);
                DrawLayer2_CoreTrail(sb, comboIntensity);

                // Restart SpriteBatch for sprite-based layers
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                    DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

                // Sprite-based layers (manage their own additive state changes)
                DrawLayer3_StarSparks(sb, progress, comboIntensity);
                DrawLayer4_WeaponSprite(sb, lightColor);
                DrawLayer5_ComboAura(sb, comboIntensity);
            }
            catch
            {
                try
                {
                    sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                        DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                }
                catch { }
            }

            // Theme accents (additive pass)
            try
            {
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
                FractalUtils.DrawThemeAccents(sb, Projectile.Center, 1f, 0.4f);
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            }
            catch { }

            return false;
        }

        /// <summary>Layer 1: Wide soft stellar glow underlayer via shader.</summary>
        private void DrawLayer1_StellarGlow(SpriteBatch sb, float combo)
        {
            var shader = FractalShaderLoader.GetSwingGlow();
            if (shader == null || _trailCount < 2) return;

            try
            {
                shader.UseColor(FractalUtils.StarVoid.ToVector3());
                shader.UseSecondaryColor(FractalUtils.FractalPurple.ToVector3());
                shader.Shader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 2f);
                shader.Shader.Parameters["uOpacity"]?.SetValue(0.4f + combo * 0.2f);
                shader.Shader.Parameters["uIntensity"]?.SetValue(1.2f);
                shader.Shader.Parameters["uOverbrightMult"]?.SetValue(1.5f);

                FractalTrailRenderer.RenderTrail(_trailPoints, new FractalTrailSettings(
                    (p, _) => (42f + combo * 15f) * (1f - p * 0.5f),
                    (p) => Color.Lerp(FractalUtils.Additive(FractalUtils.FractalPurple, 0.3f),
                                      FractalUtils.Additive(FractalUtils.StarVoid, 0.1f), p),
                    shader: shader), _trailCount, 3);
            }
            catch { }
        }

        /// <summary>Layer 2: Core trail arc with star fire shader.</summary>
        private void DrawLayer2_CoreTrail(SpriteBatch sb, float combo)
        {
            var shader = FractalShaderLoader.GetSwingTrail();
            if (shader == null || _trailCount < 2) return;

            try
            {
                if (_noiseTex?.Value != null)
                    shader.UseImage1(_noiseTex);

                shader.UseColor(FractalUtils.StarGold.ToVector3());
                shader.UseSecondaryColor(FractalUtils.FractalPurple.ToVector3());
                shader.Shader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 3f);
                shader.Shader.Parameters["uOpacity"]?.SetValue(0.8f + combo * 0.2f);
                shader.Shader.Parameters["uIntensity"]?.SetValue(1.5f + combo * 0.5f);
                shader.Shader.Parameters["uOverbrightMult"]?.SetValue(1.8f);
                shader.Shader.Parameters["uScrollSpeed"]?.SetValue(1.2f);
                shader.Shader.Parameters["uNoiseScale"]?.SetValue(2.5f);
                shader.Shader.Parameters["uPhase"]?.SetValue(combo);
                shader.Shader.Parameters["uHasSecondaryTex"]?.SetValue(_noiseTex?.Value != null ? 1f : 0f);
                shader.Shader.Parameters["uSecondaryTexScale"]?.SetValue(3f);

                FractalTrailRenderer.RenderTrail(_trailPoints, new FractalTrailSettings(
                    (p, _) =>
                    {
                        float baseWidth = 24f + combo * 8f;
                        float taper = MathF.Sin(p * MathHelper.Pi);
                        return baseWidth * (0.3f + taper * 0.7f);
                    },
                    (p) =>
                    {
                        Color c = FractalUtils.GetStellarGradient(p);
                        float alpha = (1f - p * 0.6f);
                        return FractalUtils.Additive(c, alpha);
                    },
                    shader: shader), _trailCount, 3);
            }
            catch { }
        }

        /// <summary>Layer 3: Constellation star nodes connected by light lines along the swing arc.</summary>
        private void DrawLayer3_StarSparks(SpriteBatch sb, float progress, float combo)
        {
            if (_flareTex?.Value == null || _trailCount < 3) return;

            try
            {
                FractalUtils.BeginAdditive(sb);

                var tex = _flareTex.Value;
                Vector2 origin = tex.Size() / 2f;
                float time = (float)Main.timeForVisualEffects;

                // === Constellation connecting lines between every other star node ===
                for (int i = 1; i < _trailCount - 3; i += 2)
                {
                    int j = i + 2;
                    if (j >= _trailCount) break;

                    float tA = (float)i / _trailCount;
                    float tB = (float)j / _trailCount;
                    float lineAlpha = (1f - (tA + tB) * 0.5f) * (0.15f + combo * 0.2f);

                    if (lineAlpha < 0.03f) continue;

                    Vector2 posA = _trailPoints[i] - Main.screenPosition;
                    Vector2 posB = _trailPoints[j] - Main.screenPosition;
                    Color lineCol = FractalUtils.Additive(FractalUtils.ConstellationWhite, lineAlpha * 0.6f);

                    // Draw thin connecting line as stretched flare
                    Vector2 delta = posB - posA;
                    float dist = delta.Length();
                    if (dist < 2f) continue;
                    float angle = delta.ToRotation();
                    float lineScaleX = dist / tex.Width;
                    sb.Draw(tex, posA, null, lineCol, angle, new Vector2(0, tex.Height * 0.5f),
                        new Vector2(lineScaleX, 0.03f), SpriteEffects.None, 0f);
                }

                // === Star node sprites at each trail position ===
                for (int i = 1; i < _trailCount - 1; i += 2)
                {
                    float t = (float)i / _trailCount;
                    float sparkAlpha = (1f - t) * (0.3f + combo * 0.4f);

                    // Slower, deliberate twinkle (geometric precision, not random shimmer)
                    float twinkle = MathF.Sin(time * 0.15f + i * MathHelper.TwoPi / 7f) * 0.25f + 0.75f;
                    sparkAlpha *= twinkle;

                    if (sparkAlpha < 0.05f) continue;

                    Color sparkCol = FractalUtils.GetStarShimmer(time * 0.03f + t * 5f);
                    Vector2 drawPos = _trailPoints[i] - Main.screenPosition;
                    float sparkScale = 0.18f + combo * 0.1f;

                    // Diamond-oriented 4-point star (static rotation for geometric feel)
                    sb.Draw(tex, drawPos, null, FractalUtils.Additive(sparkCol, sparkAlpha),
                        MathHelper.PiOver4, origin, sparkScale * twinkle, SpriteEffects.None, 0f);
                    sb.Draw(tex, drawPos, null, FractalUtils.Additive(sparkCol, sparkAlpha * 0.7f),
                        MathHelper.PiOver4 + MathHelper.PiOver2, origin, sparkScale * twinkle * 0.8f, SpriteEffects.None, 0f);

                    // Small bright core dot at each node
                    sb.Draw(tex, drawPos, null, FractalUtils.Additive(FractalUtils.ConstellationWhite, sparkAlpha * 0.5f),
                        0f, origin, sparkScale * 0.25f, SpriteEffects.None, 0f);
                }

                FractalUtils.EndAdditive(sb);
            }
            catch
            {
                try { FractalUtils.EndAdditive(sb); } catch { }
            }
        }

        /// <summary>Layer 4: UV-rotated weapon sprite at current angle + tip star flare.</summary>
        private void DrawLayer4_WeaponSprite(SpriteBatch sb, Color lightColor)
        {
            try
            {
                Texture2D weaponTex = TextureAssets.Projectile[Projectile.type].Value;
                Vector2 origin = weaponTex.Size() / 2f;
                float reach = _phase == 2 ? 100f : 82f;
                Vector2 drawPos = Owner.MountedCenter + _currentAngle.ToRotationVector2() * reach * 0.5f - Main.screenPosition;

                // Stellar tint
                Color weaponColor = Color.Lerp(lightColor, FractalUtils.ConstellationWhite, 0.3f);

                float drawRotation = _currentAngle + MathHelper.PiOver4;
                SpriteEffects fx = _direction < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
                sb.Draw(weaponTex, drawPos, null, weaponColor, drawRotation, origin, Projectile.scale, fx, 0f);

                // Tip star flare (additive)
                if (_glowTex?.Value != null)
                {
                    Vector2 tipPos = Owner.MountedCenter + _currentAngle.ToRotationVector2() * reach - Main.screenPosition;
                    float flarePulse = 0.8f + MathF.Sin((float)Main.timeForVisualEffects * 0.08f) * 0.2f;

                    FractalUtils.BeginAdditive(sb);
                    sb.Draw(_glowTex.Value, tipPos, null, FractalUtils.Additive(FractalUtils.StarGold, 0.5f * flarePulse),
                        0f, _glowTex.Value.Size() / 2f, 0.35f * flarePulse, SpriteEffects.None, 0f);
                    sb.Draw(_glowTex.Value, tipPos, null, FractalUtils.Additive(FractalUtils.SupernovaFlash, 0.3f * flarePulse),
                        0f, _glowTex.Value.Size() / 2f, 0.18f * flarePulse, SpriteEffects.None, 0f);
                    FractalUtils.EndAdditive(sb);
                }
            }
            catch
            {
                try { FractalUtils.EndAdditive(sb); } catch { }
            }
        }

        /// <summary>Layer 5: Combo aura — rotating hexagonal constellation orbit when combo high.</summary>
        private void DrawLayer5_ComboAura(SpriteBatch sb, float combo)
        {
            if (combo < 0.4f) return;
            if (_glowTex?.Value == null) return;

            try
            {
                float auraAlpha = (combo - 0.4f) / 0.6f; // 0→1 over 0.4→1.0
                float time = (float)Main.timeForVisualEffects;

                FractalUtils.BeginAdditive(sb);

                Vector2 center = Owner.MountedCenter - Main.screenPosition;
                var tex = _glowTex.Value;
                Vector2 origin = tex.Size() / 2f;

                // Slowly rotating geometric orbit points (hexagonal arrangement)
                int pointCount = 6;
                float orbitRadius = 55f + auraAlpha * 25f;
                float rotAngle = time * 0.015f;
                for (int p = 0; p < pointCount; p++)
                {
                    float angle = rotAngle + p * MathHelper.TwoPi / pointCount;
                    Vector2 pointPos = center + angle.ToRotationVector2() * orbitRadius;
                    float pointAlpha = auraAlpha * 0.18f;

                    // Alternate gold and purple for geometric constellation feel
                    Color pointCol = p % 2 == 0 ? FractalUtils.StarGold : FractalUtils.FractalPurple;
                    sb.Draw(tex, pointPos, null, FractalUtils.Additive(pointCol, pointAlpha),
                        angle, origin, 0.15f, SpriteEffects.None, 0f);

                    // Faint connecting line from orbit point to center
                    Vector2 delta = center - pointPos;
                    float dist = delta.Length();
                    float lineAngle = delta.ToRotation();
                    sb.Draw(tex, pointPos, null, FractalUtils.Additive(FractalUtils.ConstellationWhite, pointAlpha * 0.3f),
                        lineAngle, new Vector2(0, tex.Height * 0.5f),
                        new Vector2(dist / tex.Width, 0.015f), SpriteEffects.None, 0f);
                }

                // Subtle central star glow
                sb.Draw(tex, center, null, FractalUtils.Additive(FractalUtils.StarGold, auraAlpha * 0.08f),
                    0f, origin, 0.6f, SpriteEffects.None, 0f);

                FractalUtils.EndAdditive(sb);
            }
            catch
            {
                try { FractalUtils.EndAdditive(sb); } catch { }
            }
        }
    }
}
