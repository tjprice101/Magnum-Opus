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
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Content.Fate.ResonantWeapons.FractalOfTheStars.Utilities;
using MagnumOpus.Content.Fate.ResonantWeapons.FractalOfTheStars.Particles;
using MagnumOpus.Content.Fate.ResonantWeapons.FractalOfTheStars.Primitives;
using MagnumOpus.Content.Fate.ResonantWeapons.FractalOfTheStars.Shaders;
using MagnumOpus.Content.Fate.Debuffs;

namespace MagnumOpus.Content.Fate.ResonantWeapons.FractalOfTheStars.Projectiles
{
    /// <summary>
    /// Fractal of the Stars  EMain held swing projectile.
    ///
    /// 3-PHASE COMBO:
    ///   Phase 0 (Horizontal Sweep):  Wide horizontal sweep across  Econstellation sparks scatter
    ///   Phase 1 (Rising Uppercut):   Fast upward diagonal slash  Estar particles rise upward
    ///   Phase 2 (Gravity Slam):      Overhead slam downward  EStar Fracture explosion on hit
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

        // Incisor-style 3-segment piecewise curves (windup → swing → settle)
        private static readonly FractalUtils.CurveSegment[][] PhaseCurves = new[]
        {
            // Phase 0 (Horizontal Sweep): wide smooth sweep
            new FractalUtils.CurveSegment[]
            {
                new(0f, 0.25f, 0f, 0.14f, FractalUtils.QuadOut),
                new(0.25f, 0.83f, 0.14f, 0.92f, FractalUtils.SineInOut),
                new(0.83f, 1.0f, 0.92f, 1.0f, FractalUtils.QuadOut),
            },
            // Phase 1 (Rising Uppercut): explosive fast, minimal windup
            new FractalUtils.CurveSegment[]
            {
                new(0f, 0.16f, 0f, 0.08f, FractalUtils.SineOut),
                new(0.16f, 0.76f, 0.08f, 0.95f, FractalUtils.CubicIn),
                new(0.76f, 1.0f, 0.95f, 1.0f, FractalUtils.SineOut),
            },
            // Phase 2 (Gravity Slam): long dramatic windup → devastating slam
            new FractalUtils.CurveSegment[]
            {
                new(0f, 0.35f, 0f, 0.10f, FractalUtils.SineOut),
                new(0.35f, 0.88f, 0.10f, 0.96f, FractalUtils.ExpIn),
                new(0.88f, 1.0f, 0.96f, 1.0f, FractalUtils.QuadOut),
            },
        };

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

        // SmearDistort overlay textures (Foundation-tier: 3-sublayer shader-driven arc distortion)
        private static Asset<Texture2D> _smearArcTexture;
        private static Asset<Texture2D> _smearNoiseTex;
        private static Asset<Texture2D> _smearGradientTex;
        private Effect _smearDistortShader;
        private bool _smearShaderLoaded;

        // Crescent bloom textures (6-layer graduated bloom at blade tip)
        private static Asset<Texture2D> _bloomCircle;
        private static Asset<Texture2D> _softRadialBloom;
        private static Asset<Texture2D> _starFlareTex;

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

            // 3-segment piecewise swing: windup → accelerating sweep → settle
            float easedProgress = FractalUtils.PiecewiseAnimation(progress, PhaseCurves[_phase]);

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

            // NOTE: SpriteBatch bloom draws removed — SpawnImpactVFX is called from
            // OnHitNPC (Update phase) where no SpriteBatch is active.
            // Impact visuals handled by particles below.

            // Central particle bloom flash
            FractalParticleHandler.SpawnParticle(new FractalBloomFlare(
                pos, FractalUtils.SupernovaFlash, 0.7f * intensity, 15));
            FractalParticleHandler.SpawnParticle(new FractalBloomFlare(
                pos, FractalUtils.StarGold, 0.5f * intensity, 12));

            // Radial star spark burst (increased count for more impact)
            int sparkCount = 12 + (int)(fp.ComboIntensity * 6);
            for (int i = 0; i < sparkCount; i++)
            {
                float angle = MathHelper.TwoPi * i / sparkCount + Main.rand.NextFloat(-0.1f, 0.1f);
                Vector2 sparkVel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 10f) * intensity;
                Color sparkCol = FractalUtils.GetStellarGradient((float)i / sparkCount);
                FractalParticleHandler.SpawnParticle(new FractalSpark(
                    pos, sparkVel, sparkCol, 0.35f * intensity, 18));
            }

            // Directional slash mark particle burst (perpendicular to swing)
            Vector2 slashDir = (_currentAngle + MathHelper.PiOver2 * _direction).ToRotationVector2();
            for (int i = 0; i < 6; i++)
            {
                float spread = Main.rand.NextFloat(-0.3f, 0.3f);
                Vector2 markVel = slashDir.RotatedBy(spread) * Main.rand.NextFloat(5f, 12f);
                Color markCol = Color.Lerp(FractalUtils.StarGold, FractalUtils.ConstellationWhite, Main.rand.NextFloat());
                FractalParticleHandler.SpawnParticle(new FractalSpark(
                    pos, markVel, markCol, 0.25f * intensity, 14));
            }

            // Glyph accents (more dramatic)
            int glyphCount = 3 + (int)(fp.ComboIntensity * 4);
            for (int i = 0; i < glyphCount; i++)
            {
                Vector2 glyphPos = pos + Main.rand.NextVector2Circular(25f, 25f);
                Color glyphCol = FractalUtils.PaletteLerp(Main.rand.NextFloat());
                FractalParticleHandler.SpawnParticle(new FractalGlyph(
                    glyphPos, glyphCol, 0.32f * intensity, 28));
            }

            // Star particles cascading upward from impact
            for (int i = 0; i < 5; i++)
            {
                Vector2 starVel = new Vector2(Main.rand.NextFloat(-2f, 2f), -Main.rand.NextFloat(3f, 6f));
                Color starCol = FractalUtils.GetStarShimmer((float)Main.timeForVisualEffects * 0.03f + i);
                FractalParticleHandler.SpawnParticle(new FractalStarParticle(
                    pos + Main.rand.NextVector2Circular(10f, 10f), starVel,
                    starCol, 0.3f, 32));
            }

            // Music notes on higher combo
            if (fp.ComboIntensity > 0.3f)
            {
                int noteCount = 2 + (int)(fp.ComboIntensity * 3);
                for (int i = 0; i < noteCount; i++)
                {
                    float noteAngle = MathHelper.TwoPi * i / noteCount + Main.rand.NextFloat(-0.3f, 0.3f);
                    Vector2 noteVel = noteAngle.ToRotationVector2() * Main.rand.NextFloat(2f, 4f);
                    noteVel.Y -= 1.5f; // Float upward
                    Color noteCol = Color.Lerp(FractalUtils.StarGold, FractalUtils.CelestialLavender, Main.rand.NextFloat());
                    FractalParticleHandler.SpawnParticle(new FractalMote(
                        pos + Main.rand.NextVector2Circular(15f, 15f), noteVel,
                        noteCol, 0.2f, 35));
                }
            }

            // Screen-space flash light burst
            Lighting.AddLight(pos, FractalUtils.StarGold.ToVector3() * 1.2f * intensity);
            Lighting.AddLight(pos, FractalUtils.SupernovaFlash.ToVector3() * 0.8f * intensity);
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

        // ======================== SMEAR DISTORT OVERLAY (FOUNDATION-TIER) ========================

        /// <summary>
        /// Layer 0: SmearDistortShader overlay from SwordSmearFoundation.
        /// Renders the SwordArcSmear texture with fluid distortion shader in 3 sub-layers
        /// (outer glow ↁEmain body ↁEbright core), using Fate StarFractal gradient.
        /// Distortion and flow scale with combo intensity.
        /// </summary>
        private void DrawSmearOverlay()
        {
            float progress = SwingProgress;
            if (progress < 0.15f || progress > 0.95f) return;

            // Lazy-load SmearDistortShader
            if (!_smearShaderLoaded)
            {
                _smearShaderLoaded = true;
                try
                {
                    _smearDistortShader = ModContent.Request<Effect>(
                        "MagnumOpus/Content/FoundationWeapons/SwordSmearFoundation/Shaders/SmearDistortShader",
                        AssetRequestMode.ImmediateLoad).Value;
                }
                catch { _smearDistortShader = null; }
            }

            _smearArcTexture ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/SlashArcs/SwordArcSmear");
            _smearNoiseTex ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/NoiseTextures/TileableFBMNoise");
            _smearGradientTex ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/ColorGradients/FateGradientLUTandRAMP");

            if (_smearArcTexture?.Value == null) return;

            Texture2D smearTex = _smearArcTexture.Value;
            Vector2 smearOrigin = smearTex.Size() / 2f;
            SpriteBatch sb = Main.spriteBatch;
            Vector2 drawOrigin = Owner.MountedCenter - Main.screenPosition;

            // Scale smear to match blade reach
            float reach = _phase == 2 ? 100f : 82f;
            float maxDim = MathF.Max(smearTex.Width, smearTex.Height);
            float smearScale = (reach * 2.2f) / maxDim;

            // Rotation follows the current swing angle
            float smearRotation = _currentAngle + (_direction < 0 ? MathHelper.Pi : 0f);

            // Fade envelope: smooth in at 0.15, sustain, fade at 0.85
            float smearAlpha;
            if (progress < 0.25f)
                smearAlpha = (progress - 0.15f) / 0.10f;
            else if (progress > 0.85f)
                smearAlpha = (0.95f - progress) / 0.10f;
            else
                smearAlpha = 1f;
            smearAlpha = MathHelper.Clamp(smearAlpha, 0f, 1f);

            var fp = Owner.Fractal();
            float comboScale = fp.ComboIntensity;
            smearAlpha *= (0.6f + comboScale * 0.4f);

            // Combo-scaling: distort and flow intensify with combo
            float baseDistort = MathHelper.Lerp(0.06f, 0.14f, comboScale);
            float flowSpeed = MathHelper.Lerp(0.5f, 1.0f, comboScale);

            if (_smearDistortShader != null)
            {
                // === SHADER PATH: fluid distortion + Fate gradient coloring ===
                sb.End();
                sb.Begin(SpriteSortMode.Immediate, BlendState.Additive,
                    SamplerState.LinearWrap, DepthStencilState.None,
                    RasterizerState.CullCounterClockwise, null,
                    Main.GameViewMatrix.EffectMatrix);

                float time = (float)Main.gameTimeCache.TotalGameTime.TotalSeconds;

                _smearDistortShader.Parameters["uTime"]?.SetValue(time);
                _smearDistortShader.Parameters["fadeAlpha"]?.SetValue(smearAlpha);
                _smearDistortShader.Parameters["flowSpeed"]?.SetValue(flowSpeed);
                _smearDistortShader.Parameters["noiseScale"]?.SetValue(2.5f);
                if (_smearNoiseTex?.Value != null)
                    _smearDistortShader.Parameters["noiseTex"]?.SetValue(_smearNoiseTex.Value);
                if (_smearGradientTex?.Value != null)
                    _smearDistortShader.Parameters["gradientTex"]?.SetValue(_smearGradientTex.Value);

                // Sub-layer A: Wide outer stellar glow (strong distortion, cosmic turbulence)
                _smearDistortShader.Parameters["distortStrength"]?.SetValue(baseDistort);
                _smearDistortShader.CurrentTechnique.Passes[0].Apply();
                sb.Draw(smearTex, drawOrigin, null,
                    Color.White * smearAlpha * 0.45f,
                    smearRotation, smearOrigin,
                    smearScale * 1.18f, SpriteEffects.None, 0f);

                // Sub-layer B: Main smear body (medium distortion)
                _smearDistortShader.Parameters["distortStrength"]?.SetValue(baseDistort * 0.6f);
                _smearDistortShader.CurrentTechnique.Passes[0].Apply();
                sb.Draw(smearTex, drawOrigin, null,
                    Color.White * smearAlpha * 0.75f,
                    smearRotation, smearOrigin,
                    smearScale, SpriteEffects.None, 0f);

                // Sub-layer C: Bright stellar core (subtle distortion, sharp detail)
                _smearDistortShader.Parameters["distortStrength"]?.SetValue(baseDistort * 0.3f);
                _smearDistortShader.CurrentTechnique.Passes[0].Apply();
                sb.Draw(smearTex, drawOrigin, null,
                    Color.White * smearAlpha * 0.6f,
                    smearRotation, smearOrigin,
                    smearScale * 0.82f, SpriteEffects.None, 0f);

                sb.End();
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                    Main.DefaultSamplerState, DepthStencilState.None,
                    Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }
            else
            {
                // === FALLBACK: static colored layers (no shader available) ===
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                    Main.DefaultSamplerState, DepthStencilState.None,
                    RasterizerState.CullCounterClockwise, null,
                    Main.GameViewMatrix.EffectMatrix);

                sb.Draw(smearTex, drawOrigin, null,
                    FractalUtils.FractalPurple * smearAlpha * 0.35f,
                    smearRotation, smearOrigin,
                    smearScale * 1.18f, SpriteEffects.None, 0f);

                sb.Draw(smearTex, drawOrigin, null,
                    FractalUtils.StarGold * smearAlpha * 0.65f,
                    smearRotation, smearOrigin,
                    smearScale, SpriteEffects.None, 0f);

                sb.Draw(smearTex, drawOrigin, null,
                    FractalUtils.SupernovaFlash * smearAlpha * 0.5f,
                    smearRotation, smearOrigin,
                    smearScale * 0.82f, SpriteEffects.None, 0f);

                sb.End();
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                    Main.DefaultSamplerState, DepthStencilState.None,
                    Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }
        }

        // ======================== CRESCENT BLOOM (6-LAYER MOONLIGHT-TIER) ========================

        /// <summary>
        /// Layer 6: Multi-layer crescent bloom overlay at blade tip.
        /// Uses SoftRadialBloom for gentle outer halos and PointBloom for sharp inner core,
        /// with palette-driven color interpolation for richer stellar gradients.
        /// Scales with combo intensity  Ebarely visible at combo 0, dramatic at max.
        /// </summary>
        private void DrawCrescentBloom()
        {
            float progress = SwingProgress;
            if (progress < 0.2f || progress > 0.9f) return;

            _bloomCircle ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom");
            _softRadialBloom ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom");
            _starFlareTex ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/StarFlare");

            if (_bloomCircle?.Value == null || _softRadialBloom?.Value == null) return;

            Texture2D sharpBloom = _bloomCircle.Value;
            Texture2D softBloom = _softRadialBloom.Value;
            SpriteBatch sb = Main.spriteBatch;

            float reach = _phase == 2 ? 100f : 82f;
            Vector2 tipPos = Owner.MountedCenter + _currentAngle.ToRotationVector2() * reach - Main.screenPosition;

            var fp = Owner.Fractal();
            float comboIntensity = fp.ComboIntensity;
            float crescentScale = 0.35f + 0.3f * comboIntensity;

            // Fade in/out envelope
            float bloomProgress = (progress - 0.2f) / 0.7f; // 0ↁE over 0.2ↁE.9
            float crescentOpacity = MathF.Sin(bloomProgress * MathHelper.Pi) * 0.6f * (0.5f + comboIntensity * 0.5f);

            // Phase-specific color shift: purple at sweep start ↁEgold at peak ↁEwhite at slam
            float paletteT = 0.15f + progress * 0.7f + _phase * 0.1f;
            Color outerColor = FractalUtils.GetStellarGradient(paletteT - 0.15f);
            Color innerColor = FractalUtils.GetStellarGradient(paletteT + 0.15f);

            try
            {
                FractalUtils.BeginAdditive(sb);

                // Layer 1: Wide soft radial halo (SoftRadialBloom)
                outerColor.A = 0;
                sb.Draw(softBloom, tipPos, null, outerColor * crescentOpacity * 0.25f,
                    0f, softBloom.Size() / 2f, crescentScale * 0.2f, SpriteEffects.None, 0f);

                // Layer 2: Mid-range stellar glow (SoftRadialBloom)  Epurple body
                Color midColor = FractalUtils.FractalPurple with { A = 0 };
                sb.Draw(softBloom, tipPos, null, midColor * crescentOpacity * 0.35f,
                    _currentAngle * 0.5f, softBloom.Size() / 2f, crescentScale * 0.13f, SpriteEffects.None, 0f);

                // Layer 3: Inner crescent core (PointBloom)  Ebright gold
                innerColor.A = 0;
                sb.Draw(sharpBloom, tipPos, null, innerColor * crescentOpacity * 0.8f,
                    _currentAngle, sharpBloom.Size() / 2f, crescentScale * 0.065f, SpriteEffects.None, 0f);

                // Layer 4: White-hot supernova center (PointBloom)
                Color coreWhite = FractalUtils.SupernovaFlash with { A = 0 };
                sb.Draw(sharpBloom, tipPos, null, coreWhite * crescentOpacity * 0.5f,
                    _currentAngle, sharpBloom.Size() / 2f, crescentScale * 0.03f, SpriteEffects.None, 0f);

                // Layer 5: Cross star flare at blade tip
                if (_starFlareTex?.Value != null)
                {
                    Texture2D starFlare = _starFlareTex.Value;
                    float flarePulse = 0.85f + 0.15f * MathF.Sin((float)Main.timeForVisualEffects * 0.06f);
                    Color flareColor = innerColor with { A = 0 };
                    // Vertical flare
                    sb.Draw(starFlare, tipPos, null, flareColor * crescentOpacity * 0.5f * flarePulse,
                        MathHelper.PiOver2, starFlare.Size() / 2f,
                        new Vector2(crescentScale * 0.1f, crescentScale * 0.25f) * flarePulse, SpriteEffects.None, 0f);
                    // Horizontal flare (perpendicular cross)
                    sb.Draw(starFlare, tipPos, null, flareColor * crescentOpacity * 0.35f * flarePulse,
                        0f, starFlare.Size() / 2f,
                        new Vector2(crescentScale * 0.08f, crescentScale * 0.2f) * flarePulse, SpriteEffects.None, 0f);
                }

                // Layer 6: Nebula glow orb overlay (pulsing, behind crescent)
                float orbPulse = 0.85f + 0.15f * MathF.Sin(progress * MathHelper.Pi * 3f);
                Color orbColor = FractalUtils.NebulaPink with { A = 0 };
                sb.Draw(softBloom, tipPos, null, orbColor * crescentOpacity * 0.2f * orbPulse,
                    0f, softBloom.Size() / 2f, crescentScale * 0.16f * orbPulse, SpriteEffects.None, 0f);

                FractalUtils.EndAdditive(sb);
            }
            catch
            {
                try { FractalUtils.EndAdditive(sb); } catch { }
            }
        }

        // ======================== 7-LAYER RENDERING ========================

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
                // === Layer 0: SmearDistort overlay (Foundation-tier fluid distortion arc) ===
                DrawSmearOverlay();

                // End SpriteBatch before GPU primitive trail draws
                sb.End();

                // === Layer 1: Wide stellar glow underlayer (GPU primitives) ===
                DrawLayer1_StellarGlow(sb, comboIntensity);
                // === Layer 2: Core constellation trail arc (GPU primitives) ===
                DrawLayer2_CoreTrail(sb, comboIntensity);

                // Restart SpriteBatch for sprite-based layers
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                    DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

                // === Layer 3: Constellation star sparks along arc ===
                DrawLayer3_StarSparks(sb, progress, comboIntensity);
                // === Layer 4: UV-rotated weapon sprite + tip flare ===
                DrawLayer4_WeaponSprite(sb, lightColor);
                // === Layer 5: Combo aura (stellar rings when combo >= 2) ===
                DrawLayer5_ComboAura(sb, comboIntensity);
                // === Layer 6: Multi-layer crescent bloom at blade tip ===
                DrawCrescentBloom();
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
                sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
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

                    // Star4Soft sparkle accent — fractal starpoint
                    Texture2D starTex = MagnumTextureRegistry.GetStar4Soft();
                    if (starTex != null)
                    {
                        Vector2 starOrigin = starTex.Size() / 2f;
                        float starRot = (float)Main.timeForVisualEffects * 0.06f;
                        sb.Draw(starTex, tipPos, null, FractalUtils.Additive(FractalUtils.StarGold, 0.4f * flarePulse),
                            starRot, starOrigin, 0.10f * flarePulse, SpriteEffects.None, 0f);
                        sb.Draw(starTex, tipPos, null, FractalUtils.Additive(FractalUtils.SupernovaFlash, 0.25f * flarePulse),
                            -starRot * 0.7f, starOrigin, 0.06f * flarePulse, SpriteEffects.None, 0f);
                    }
                    FractalUtils.EndAdditive(sb);
                }
            }
            catch
            {
                try { FractalUtils.EndAdditive(sb); } catch { }
            }
        }

        /// <summary>Layer 5: Combo aura  Erotating hexagonal constellation orbit when combo high.</summary>
        private void DrawLayer5_ComboAura(SpriteBatch sb, float combo)
        {
            if (combo < 0.4f) return;
            if (_glowTex?.Value == null) return;

            try
            {
                float auraAlpha = (combo - 0.4f) / 0.6f; // 0ↁE over 0.4ↁE.0
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
