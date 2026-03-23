using System;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems.Shaders;
using MagnumOpus.Content.MoonlightSonata.Weapons.IncisorOfMoonlight.Buffs;
using MagnumOpus.Content.MoonlightSonata.Weapons.IncisorOfMoonlight.Particles;
using MagnumOpus.Content.MoonlightSonata.Weapons.IncisorOfMoonlight.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.MoonlightSonata.Weapons.IncisorOfMoonlight.Projectiles
{
    /// <summary>
    /// Persistent circular lunar damage zone spawned by SuperLunarOrbProj on impact.
    /// Dual-noise UV-scrolling shader with Moonlight Sonata palette.
    /// Deals periodic damage to enemies standing within. 4 second duration.
    /// </summary>
    public class LunarZoneProj : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.RainbowCrystalExplosion;

        private const int MaxLifetime = 240;
        private const int FadeInFrames = 20;
        private const int FadeOutFrames = 40;
        private const float ZoneRadius = 80f;
        private const float DrawScale = 0.5f;

        private int timer;
        private float seed;
        private Effect zoneShader;
        private bool spawnedImpactVFX;

        // Textures
        private static Asset<Texture2D> _noiseFBM;
        private static Asset<Texture2D> _noisePerlin;
        private static Asset<Texture2D> _gradientLUT;
        private static Asset<Texture2D> _softCircle;
        private static Asset<Texture2D> _softGlow;
        private static Asset<Texture2D> _pointBloom;

        // Moonlight Sonata palette
        private static readonly Color Violet = new Color(138, 43, 226);
        private static readonly Color IceBlue = new Color(135, 206, 250);
        private static readonly Color DeepPurple = new Color(90, 50, 160);
        private static readonly Color MoonWhite = new Color(220, 230, 255);

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DrawScreenCheckFluff[Projectile.type] = 500;
        }

        public override void SetDefaults()
        {
            Projectile.width = (int)(ZoneRadius * 2);
            Projectile.height = (int)(ZoneRadius * 2);
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.timeLeft = MaxLifetime;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 30;
            Projectile.hide = false;
            Projectile.alpha = 0;
        }

        private void LoadTextures()
        {
            const string Noise = "MagnumOpus/Assets/VFX Asset Library/NoiseTextures/";
            const string Gradients = "MagnumOpus/Assets/VFX Asset Library/ColorGradients/";
            const string Masks = "MagnumOpus/Assets/VFX Asset Library/MasksAndShapes/";
            const string Bloom = "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/";

            _noiseFBM ??= ModContent.Request<Texture2D>(Noise + "TileableFBMNoise", AssetRequestMode.ImmediateLoad);
            _noisePerlin ??= ModContent.Request<Texture2D>(Noise + "PerlinNoise", AssetRequestMode.ImmediateLoad);
            _gradientLUT ??= ModContent.Request<Texture2D>(Gradients + "MoonlightSonataGradientLUTandRAMP", AssetRequestMode.ImmediateLoad);
            _softCircle ??= ModContent.Request<Texture2D>(Masks + "SoftCircle", AssetRequestMode.ImmediateLoad);
            _softGlow ??= ModContent.Request<Texture2D>(Bloom + "SoftGlow", AssetRequestMode.ImmediateLoad);
            _pointBloom ??= ModContent.Request<Texture2D>(Bloom + "PointBloom", AssetRequestMode.ImmediateLoad);
        }

        public override void AI()
        {
            if (timer == 0)
                seed = Main.rand.NextFloat(100f);

            timer++;
            Projectile.velocity = Vector2.Zero;

            float alpha = GetAlphaMultiplier();
            Lighting.AddLight(Projectile.Center, Violet.ToVector3() * alpha * 0.6f);

            // Impact VFX on first frame
            if (!spawnedImpactVFX && !Main.dedServ)
            {
                spawnedImpactVFX = true;
                SpawnImpactVFX();
            }

            // Constellation spark dust inside zone
            if (Main.rand.NextBool(3) && !Main.dedServ)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float radius = Main.rand.NextFloat(ZoneRadius * 0.9f);
                Vector2 dustPos = Projectile.Center + angle.ToRotationVector2() * radius;
                Vector2 dustVel = new Vector2(0, -Main.rand.NextFloat(0.5f, 1.5f));

                Color sparkColor = IncisorUtils.MulticolorLerp(Main.rand.NextFloat(),
                    Violet, IceBlue, MoonWhite);
                var spark = new ConstellationSparkParticle(
                    dustPos, dustVel, false,
                    Main.rand.Next(12, 20), Main.rand.NextFloat(0.08f, 0.16f),
                    sparkColor, new Vector2(0.5f, 1.2f), quickShrink: true);
                IncisorParticleHandler.SpawnParticle(spark);
            }

            // Occasional larger sparkle burst
            if (Main.rand.NextBool(12) && !Main.dedServ)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float radius = Main.rand.NextFloat(ZoneRadius * 0.5f);
                Vector2 burstPos = Projectile.Center + angle.ToRotationVector2() * radius;

                for (int i = 0; i < 3; i++)
                {
                    Vector2 vel = Main.rand.NextVector2Circular(2f, 2f) + new Vector2(0, -0.5f);
                    Color moteColor = IncisorUtils.MulticolorLerp(Main.rand.NextFloat(),
                        DeepPurple, Violet, IceBlue);
                    var mote = new LunarMoteParticle(
                        burstPos, vel, Main.rand.NextFloat(0.2f, 0.4f), moteColor,
                        Main.rand.Next(18, 28), 2.5f, 3.5f, hueShift: 0.01f);
                    IncisorParticleHandler.SpawnParticle(mote);
                }
            }
        }

        private void SpawnImpactVFX()
        {
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.2f, Volume = 0.7f }, Projectile.Center);

            // Radial burst of constellation sparks
            for (int i = 0; i < 25; i++)
            {
                float angle = MathHelper.TwoPi * i / 25f + Main.rand.NextFloat(-0.1f, 0.1f);
                float speed = Main.rand.NextFloat(3f, 7f);
                Vector2 vel = angle.ToRotationVector2() * speed;
                Color sparkColor = IncisorUtils.MulticolorLerp(Main.rand.NextFloat(),
                    IncisorUtils.IncisorPalette);
                var spark = new ConstellationSparkParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(8f, 8f),
                    vel, true, Main.rand.Next(14, 24),
                    Main.rand.NextFloat(0.12f, 0.3f), sparkColor,
                    new Vector2(0.6f, 1.4f));
                IncisorParticleHandler.SpawnParticle(spark);
            }

            // Lunar motes drifting outward
            for (int i = 0; i < 10; i++)
            {
                float angle = MathHelper.TwoPi * i / 10f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(1.5f, 4f);
                Color moteColor = IncisorUtils.MulticolorLerp(Main.rand.NextFloat(),
                    DeepPurple, Violet, IceBlue, MoonWhite);
                var mote = new LunarMoteParticle(
                    Projectile.Center, vel, Main.rand.NextFloat(0.3f, 0.6f), moteColor,
                    Main.rand.Next(20, 35), 3f, 4f, hueShift: 0.015f);
                IncisorParticleHandler.SpawnParticle(mote);
            }

            // Music notes
            MoonlightVFXLibrary.SpawnMusicNotes(Projectile.Center, count: 6, spread: 35f,
                minScale: 0.7f, maxScale: 1.1f, lifetime: 50);
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

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            Vector2 closestPoint = new Vector2(
                MathHelper.Clamp(Projectile.Center.X, targetHitbox.Left, targetHitbox.Right),
                MathHelper.Clamp(Projectile.Center.Y, targetHitbox.Top, targetHitbox.Bottom));
            float dist = Vector2.Distance(Projectile.Center, closestPoint);
            return dist <= ZoneRadius;
        }

        // =====================================================================
        // RENDERING
        // =====================================================================

        public override bool PreDraw(ref Color lightColor)
        {
            if (Main.dedServ) return false;

            SpriteBatch sb = Main.spriteBatch;
            try
            {
                LoadTextures();
                Vector2 drawPos = Projectile.Center - Main.screenPosition;
                float alpha = GetAlphaMultiplier();

                // Layer 1: Ambient bloom
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                    Main.DefaultSamplerState, DepthStencilState.None,
                    RasterizerState.CullCounterClockwise, null,
                    Main.GameViewMatrix.TransformationMatrix);

                DrawAmbientBloom(sb, drawPos, alpha);

                // Layer 2: Shader zone
                DrawShaderZone(sb, drawPos, alpha);

                // Layer 3: Edge sparkles
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                    Main.DefaultSamplerState, DepthStencilState.None,
                    RasterizerState.CullCounterClockwise, null,
                    Main.GameViewMatrix.TransformationMatrix);

                DrawEdgeSparkles(sb, drawPos, alpha);
            }
            catch { }
            finally
            {
                try { sb.End(); } catch { }
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                    DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }

            return false;
        }

        private void DrawAmbientBloom(SpriteBatch sb, Vector2 drawPos, float alpha)
        {
            Texture2D softGlow = _softGlow?.Value;
            if (softGlow == null) return;

            Vector2 glowOrigin = softGlow.Size() / 2f;
            float pulse = 0.9f + 0.1f * MathF.Sin((float)Main.timeForVisualEffects * 0.04f + seed);

            Color outerColor = DeepPurple with { A = 0 };
            sb.Draw(softGlow, drawPos, null, outerColor * (0.25f * alpha * pulse),
                0f, glowOrigin, 0.3f * pulse, SpriteEffects.None, 0f);

            Color innerColor = Violet with { A = 0 };
            sb.Draw(softGlow, drawPos, null, innerColor * (0.3f * alpha * pulse),
                0f, glowOrigin, 0.18f * pulse, SpriteEffects.None, 0f);
        }

        private void DrawShaderZone(SpriteBatch sb, Vector2 drawPos, float alpha)
        {
            float time = (float)Main.timeForVisualEffects;

            if (zoneShader == null)
                zoneShader = ShaderLoader.LunarZone;

            if (zoneShader == null) return;

            zoneShader.Parameters["uTime"]?.SetValue(time * 0.015f + seed);
            zoneShader.Parameters["scrollSpeed1"]?.SetValue(0.15f);
            zoneShader.Parameters["scrollSpeed2"]?.SetValue(-0.1f);
            zoneShader.Parameters["rotationSpeed"]?.SetValue(0.08f);
            zoneShader.Parameters["circleRadius"]?.SetValue(0.42f);
            zoneShader.Parameters["edgeSoftness"]?.SetValue(0.08f);
            zoneShader.Parameters["intensity"]?.SetValue(2.0f);
            zoneShader.Parameters["primaryColor"]?.SetValue(Violet.ToVector3());
            zoneShader.Parameters["coreColor"]?.SetValue(IceBlue.ToVector3());
            zoneShader.Parameters["fadeAlpha"]?.SetValue(alpha);

            float breathe = 0.88f + 0.12f * MathF.Sin(time * 0.06f + seed);
            zoneShader.Parameters["breathe"]?.SetValue(breathe);

            zoneShader.Parameters["noiseTex"]?.SetValue(_noiseFBM?.Value);
            zoneShader.Parameters["noise2Tex"]?.SetValue(_noisePerlin?.Value);
            zoneShader.Parameters["gradientTex"]?.SetValue(_gradientLUT?.Value);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                SamplerState.LinearWrap, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, zoneShader,
                Main.GameViewMatrix.TransformationMatrix);

            Texture2D circleTex = _softCircle?.Value;
            if (circleTex != null)
            {
                sb.Draw(circleTex, drawPos, null, Color.White * alpha,
                    0f, circleTex.Size() / 2f, DrawScale, SpriteEffects.None, 0f);
            }

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);
        }

        private void DrawEdgeSparkles(SpriteBatch sb, Vector2 drawPos, float alpha)
        {
            float time = (float)Main.timeForVisualEffects;
            Texture2D pointBloom = _pointBloom?.Value;
            if (pointBloom == null) return;

            Vector2 bloomOrigin = pointBloom.Size() / 2f;

            int sparkleCount = 8;
            for (int i = 0; i < sparkleCount; i++)
            {
                float baseAngle = (i / (float)sparkleCount) * MathHelper.TwoPi;
                float animAngle = baseAngle + time * 0.008f + seed;
                float radiusOffset = 0.85f + 0.1f * MathF.Sin(time * 0.03f + i * 1.5f);

                Vector2 sparkleOffset = animAngle.ToRotationVector2() * (ZoneRadius * radiusOffset * 0.75f);
                float sparkleAlpha = 0.3f + 0.2f * MathF.Sin(time * 0.06f + i * 2.1f);

                Color sparkleColor = (i % 2 == 0 ? IceBlue : Violet) with { A = 0 };
                float sparkleScale = 0.1f + 0.05f * MathF.Sin(time * 0.08f + i * 1.3f);

                sb.Draw(pointBloom, drawPos + sparkleOffset, null,
                    sparkleColor * (sparkleAlpha * alpha),
                    0f, bloomOrigin, sparkleScale, SpriteEffects.None, 0f);
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<LunarResonanceDebuff>(), 120);

            // Small sparkle burst on damage tick
            for (int i = 0; i < 4; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(3f, 3f);
                Color sparkColor = IncisorUtils.MulticolorLerp(Main.rand.NextFloat(),
                    Violet, IceBlue, MoonWhite);
                var spark = new ConstellationSparkParticle(
                    target.Center, vel, false,
                    Main.rand.Next(8, 14), Main.rand.NextFloat(0.06f, 0.14f),
                    sparkColor, new Vector2(0.4f, 1f), quickShrink: true);
                IncisorParticleHandler.SpawnParticle(spark);
            }
        }

        public override Color? GetAlpha(Color lightColor) => Color.White;
    }
}
