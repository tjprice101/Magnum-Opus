using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static MagnumOpus.Content.MoonlightSonata.Weapons.MoonlightsCalling.Utilities.SerenadeUtils;

namespace MagnumOpus.Content.MoonlightSonata.Weapons.MoonlightsCalling.Projectiles
{
    /// <summary>
    /// MoonlightPuddle — A persistent prismatic damage zone left where the
    /// Serenade Holdout mega-beam hits enemies or terminates.
    ///
    /// VISUAL ARCHITECTURE (Foundation: ImpactFoundation + DamageZoneShader):
    /// 1. AMBIENT BLOOM — Soft prismatic glow beneath the zone
    /// 2. SHADER ZONE — DamageZoneShader rendering scrolling noise masked to a
    ///    breathing circle, colored via Moonlight Sonata gradient LUT
    /// 3. EDGE SPARKLES — Orbiting prismatic sparkle points at zone boundary
    ///
    /// The zone fades in over 12 frames, persists with breathing pulse, and
    /// fades out over the last 40 frames. Musical wave noise creates a
    /// harmonic, resonant visual matching the Serenade identity.
    ///
    /// VFX-only: friendly=false, 0 damage.
    /// (Damage is handled by SerenadeHoldout's DamageAlongBeam directly.)
    ///
    /// ai[0] = resonance level (0-4) for intensity scaling
    /// </summary>
    public class MoonlightPuddle : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.RainbowCrystalExplosion;

        private const int MaxLifetime = 150; // 2.5 seconds
        private const int FadeInFrames = 12;
        private const int FadeOutFrames = 40;
        private const float DrawScale = 0.25f;
        private const float ZoneRadiusVisual = 35f;

        // Texture paths — sourced from VFX Asset Library
        private static readonly string BloomPath = "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/";
        private static readonly string MaskPath = "MagnumOpus/Assets/VFX Asset Library/MasksAndShapes/";
        private static readonly string NoisePath = "MagnumOpus/Assets/VFX Asset Library/NoiseTextures/";
        private static readonly string GradPath = "MagnumOpus/Assets/VFX Asset Library/ColorGradients/";

        private int timer;
        private float seed;
        private Effect zoneShader;

        // Cached textures
        private static Asset<Texture2D> _softGlow;
        private static Asset<Texture2D> _pointBloom;
        private static Asset<Texture2D> _softCircle;
        private static Asset<Texture2D> _noiseMusicalWave;
        private static Asset<Texture2D> _gradMoonlightSonata;

        private int ResonanceLevel => (int)Projectile.ai[0];

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DrawScreenCheckFluff[Projectile.type] = 400;
        }

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = MaxLifetime;
            Projectile.hide = false;
            Projectile.alpha = 255;
        }

        public override void AI()
        {
            if (timer == 0)
            {
                seed = Main.rand.NextFloat(100f);
                EnsureTextures();
            }

            timer++;
            Projectile.velocity = Vector2.Zero;

            float alpha = GetAlphaMultiplier();

            // Prismatic lighting
            Color lightCol = GetBeamGradient(MathHelper.Clamp(timer / (float)MaxLifetime * 0.5f + 0.25f, 0f, 1f));
            float resonanceBoost = 1f + ResonanceLevel * 0.12f;
            Lighting.AddLight(Projectile.Center, lightCol.ToVector3() * alpha * 0.4f * resonanceBoost);

            // Sparkle dust particles inside zone
            if (!Main.dedServ && Main.rand.NextBool(4))
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float radius = Main.rand.NextFloat(ZoneRadiusVisual * 0.85f);
                Vector2 dustPos = Projectile.Center + angle.ToRotationVector2() * radius;
                Vector2 dustVel = new Vector2(0, -Main.rand.NextFloat(0.4f, 1.2f));

                Color col = GetSpectralColor(Main.rand.Next(SpectralColors.Length));
                Dust dust = Dust.NewDustPerfect(dustPos, DustID.RainbowMk2, dustVel,
                    newColor: col, Scale: Main.rand.NextFloat(0.2f, 0.45f));
                dust.noGravity = true;
                dust.fadeIn = 0.4f;
            }
        }

        private void EnsureTextures()
        {
            _softGlow ??= ModContent.Request<Texture2D>(BloomPath + "SoftGlow", AssetRequestMode.ImmediateLoad);
            _pointBloom ??= ModContent.Request<Texture2D>(BloomPath + "PointBloom", AssetRequestMode.ImmediateLoad);
            _softCircle ??= ModContent.Request<Texture2D>(MaskPath + "SoftCircle", AssetRequestMode.ImmediateLoad);
            _noiseMusicalWave ??= ModContent.Request<Texture2D>(NoisePath + "MusicalWavePattern", AssetRequestMode.ImmediateLoad);
            _gradMoonlightSonata ??= ModContent.Request<Texture2D>(GradPath + "MoonlightSonataGradientLUTandRAMP", AssetRequestMode.ImmediateLoad);
        }

        private float GetAlphaMultiplier()
        {
            int framesRemaining = Projectile.timeLeft;
            float fadeIn = MathHelper.Clamp(timer / (float)FadeInFrames, 0f, 1f);
            float fadeOut = framesRemaining < FadeOutFrames
                ? framesRemaining / (float)FadeOutFrames
                : 1f;
            return fadeIn * fadeOut;
        }

        // =================================================================
        // RENDERING
        // =================================================================

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float alpha = GetAlphaMultiplier();
            float resMult = 1f + ResonanceLevel * 0.1f;

            // Prismatic zone colors
            Color edgeColor = GetBeamGradient(0.2f + ResonanceLevel * 0.1f);
            Color coreColor = MoonWhite;

            // Switch to additive
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

            // ---- LAYER 1: AMBIENT BLOOM ----
            DrawAmbientBloom(sb, drawPos, edgeColor, alpha, resMult);

            // ---- LAYER 2: SHADER ZONE ----
            DrawShaderZone(sb, drawPos, edgeColor, coreColor, alpha, resMult);

            // ---- LAYER 3: EDGE SPARKLES ----
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

            DrawEdgeSparkles(sb, drawPos, alpha, resMult);

            // ---- RESTORE ----
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

            return false;
        }

        // =================================================================
        // LAYER 1: AMBIENT BLOOM
        // =================================================================

        private void DrawAmbientBloom(SpriteBatch sb, Vector2 drawPos, Color edge, float alpha, float resMult)
        {
            Texture2D softGlow = _softGlow.Value;
            Vector2 origin = softGlow.Size() / 2f;
            float pulse = 0.92f + 0.08f * MathF.Sin((float)Main.timeForVisualEffects * 0.04f + seed);

            // Wide ambient glow
            sb.Draw(softGlow, drawPos, null, edge * (0.18f * alpha * pulse * resMult), 0f,
                origin, 0.18f * pulse * resMult, SpriteEffects.None, 0f);

            // Inner glow — violet tint
            sb.Draw(softGlow, drawPos, null, PrismViolet * (0.22f * alpha * pulse * resMult), 0f,
                origin, 0.1f * pulse * resMult, SpriteEffects.None, 0f);
        }

        // =================================================================
        // LAYER 2: SHADER-DRIVEN NOISE ZONE
        // =================================================================

        private void DrawShaderZone(SpriteBatch sb, Vector2 drawPos,
            Color primary, Color core, float alpha, float resMult)
        {
            float time = (float)Main.timeForVisualEffects;

            // Load shader
            if (zoneShader == null)
            {
                zoneShader = ModContent.Request<Effect>(
                    "MagnumOpus/Content/FoundationWeapons/ImpactFoundation/Shaders/DamageZoneShader",
                    AssetRequestMode.ImmediateLoad).Value;
            }

            // Configure — slower, more ethereal than the Foundation DamageZone
            zoneShader.Parameters["uTime"]?.SetValue(time * 0.01f + seed);
            zoneShader.Parameters["scrollSpeed"]?.SetValue(0.15f);
            zoneShader.Parameters["rotationSpeed"]?.SetValue(0.08f);
            zoneShader.Parameters["circleRadius"]?.SetValue(0.40f);
            zoneShader.Parameters["edgeSoftness"]?.SetValue(0.07f);
            zoneShader.Parameters["intensity"]?.SetValue(1.6f * resMult);
            zoneShader.Parameters["primaryColor"]?.SetValue(primary.ToVector3());
            zoneShader.Parameters["coreColor"]?.SetValue(core.ToVector3());
            zoneShader.Parameters["fadeAlpha"]?.SetValue(alpha);

            // Breathing pulse
            float breathe = 0.88f + 0.12f * MathF.Sin(time * 0.045f + seed);
            zoneShader.Parameters["breathe"]?.SetValue(breathe);

            // Musical wave noise — harmonic patterns
            zoneShader.Parameters["noiseTex"]?.SetValue(_noiseMusicalWave.Value);
            zoneShader.Parameters["gradientTex"]?.SetValue(_gradMoonlightSonata.Value);

            // Begin shader batch
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                SamplerState.LinearWrap, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, zoneShader,
                Main.GameViewMatrix.TransformationMatrix);

            Texture2D circleTex = _softCircle.Value;
            Vector2 circleOrigin = circleTex.Size() / 2f;

            sb.Draw(circleTex, drawPos, null, Color.White * alpha, 0f,
                circleOrigin, DrawScale * resMult, SpriteEffects.None, 0f);

            // End shader batch
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);
        }

        // =================================================================
        // LAYER 3: EDGE SPARKLES
        // =================================================================

        private void DrawEdgeSparkles(SpriteBatch sb, Vector2 drawPos, float alpha, float resMult)
        {
            float time = (float)Main.timeForVisualEffects;
            Texture2D pointBloom = _pointBloom.Value;
            Vector2 bloomOrigin = pointBloom.Size() / 2f;

            // Prismatic sparkle points orbiting the zone edge
            int sparkleCount = 5 + ResonanceLevel;
            for (int i = 0; i < sparkleCount; i++)
            {
                float baseAngle = i / (float)sparkleCount * MathHelper.TwoPi;
                float animAngle = baseAngle + time * 0.008f + seed;
                float radiusOffset = 0.82f + 0.12f * MathF.Sin(time * 0.025f + i * 1.7f);

                Vector2 sparkleOffset = animAngle.ToRotationVector2() * (ZoneRadiusVisual * radiusOffset * 0.75f);
                float sparkleAlpha = 0.25f + 0.18f * MathF.Sin(time * 0.05f + i * 2.3f);

                // Cycle through spectral colors
                Color sparkleColor = GetSpectralColor(i % SpectralColors.Length);
                float sparkleScale = 0.06f + 0.03f * MathF.Sin(time * 0.07f + i * 1.5f);

                sb.Draw(pointBloom, drawPos + sparkleOffset, null,
                    sparkleColor * (sparkleAlpha * alpha * resMult),
                    0f, bloomOrigin, sparkleScale * resMult, SpriteEffects.None, 0f);
            }
        }

        public override Color? GetAlpha(Color lightColor) => Color.White;
    }
}
