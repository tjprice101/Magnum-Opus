using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.Shaders;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.VFX;

namespace MagnumOpus.Content.ClairDeLune.Weapons.ClockworkGrimoire.Projectiles
{
    /// <summary>
    /// Pendulum Zone - shader-driven persistent AoE that swings left/right dealing damage at extremes.
    /// 3 render passes: (1) ArcanePageFlow swinging zone body, (2) ClairDeLuneMoonlit ambient glow,
    /// (3) Bloom stacking for pendulum core, swing extreme pulse, arm indicator.
    /// MarbleSwirl pattern with oscillating UV, rhythmic intensity pulsing.
    /// ai[0] = synergy flag (1 = enhanced, 50% larger).
    /// </summary>
    public class PendulumZoneProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

        private int _timer;
        private const int Duration = 300;
        private const float BaseRadius = 96f;
        private const float SwingFrequency = 0.05f;
        private const float SwingAmplitude = 48f;

        // --- Shader + texture caching ---
        private static Effect _arcanePagesShader;
        private static Effect _moonlitShader;
        private static Asset<Texture2D> _softCircle;
        private static Asset<Texture2D> _softRadialBloom;
        private static Asset<Texture2D> _pointBloom;
        private static Asset<Texture2D> _starFlare;
        private static Asset<Texture2D> _noiseTex;

        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.timeLeft = Duration;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 30;
        }

        public override void AI()
        {
            _timer++;

            float swing = MathF.Sin(_timer * SwingFrequency);
            bool synergy = Projectile.ai[0] == 1;
            float radius = BaseRadius * (synergy ? 1.5f : 1f);

            if (MathF.Abs(swing) > 0.9f)
                Projectile.localNPCHitCooldown = 10;
            else
                Projectile.localNPCHitCooldown = 60;

            Vector2 swingOffset = new Vector2(swing * SwingAmplitude, 0);
            Vector2 centerPos = Projectile.Center + swingOffset;

            // Swing particles
            if (_timer % 4 == 0)
            {
                float pAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 pPos = centerPos + pAngle.ToRotationVector2() * Main.rand.NextFloat(radius * 0.3f, radius);
                Color pCol = Color.Lerp(ClairDeLunePalette.SoftBlue, ClairDeLunePalette.MoonbeamGold, MathF.Abs(swing));
                var p = new GenericGlowParticle(pPos, new Vector2(swing * 0.5f, 0),
                    pCol with { A = 0 } * 0.2f, 0.05f, 10, true);
                MagnumParticleHandler.SpawnParticle(p);
            }

            // Extreme pulse particles
            if (MathF.Abs(swing) > 0.95f && _timer % 8 == 0)
            {
                var pulse = new BloomParticle(centerPos, Vector2.Zero,
                    ClairDeLunePalette.ClockworkBrass with { A = 0 } * 0.3f, 0.25f, 6);
                MagnumParticleHandler.SpawnParticle(pulse);
            }

            Lighting.AddLight(centerPos, ClairDeLunePalette.ClockworkBrass.ToVector3() * 0.3f);
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float swing = MathF.Sin(_timer * SwingFrequency);
            bool synergy = Projectile.ai[0] == 1;
            float radius = BaseRadius * (synergy ? 1.5f : 1f);
            Vector2 center = Projectile.Center + new Vector2(swing * SwingAmplitude, 0);
            return Vector2.Distance(center, targetHitbox.Center.ToVector2()) < radius;
        }

        private void LoadTextures()
        {
            _softCircle ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/MasksAndShapes/SoftCircle", AssetRequestMode.ImmediateLoad);
            _softRadialBloom ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom", AssetRequestMode.ImmediateLoad);
            _pointBloom ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom", AssetRequestMode.ImmediateLoad);
            _starFlare ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/StarFlare", AssetRequestMode.ImmediateLoad);
            _noiseTex ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/NoiseTextures/MarbleSwirlNoise", AssetRequestMode.ImmediateLoad);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (Main.dedServ) return false;
            LoadTextures();

            SpriteBatch sb = Main.spriteBatch;
            Matrix matrix = Main.GameViewMatrix.TransformationMatrix;

            float swing = MathF.Sin(_timer * SwingFrequency);
            bool synergy = Projectile.ai[0] == 1;
            float radius = BaseRadius * (synergy ? 1.5f : 1f);
            float fade = Projectile.timeLeft < 30 ? Projectile.timeLeft / 30f : 1f;
            float pulse = 1f + 0.15f * MathF.Abs(swing);

            Vector2 swingOffset = new Vector2(swing * SwingAmplitude, 0);
            Vector2 drawCenter = Projectile.Center + swingOffset - Main.screenPosition;

            DrawArcanePageZone(sb, matrix, drawCenter, radius, fade, swing);   // Pass 1
            DrawMoonlitAmbient(sb, matrix, drawCenter, radius, fade, pulse);   // Pass 2
            DrawBloomComposite(sb, matrix, drawCenter, radius, fade, swing, pulse); // Pass 3
            return false;
        }

        // ---- PASS 1: ArcanePageFlow for swinging zone with oscillating scroll ----
        private void DrawArcanePageZone(SpriteBatch sb, Matrix matrix, Vector2 drawCenter, float radius, float fade, float swing)
        {
            _arcanePagesShader ??= ShaderLoader.ArcanePages;
            if (_arcanePagesShader == null) return;

            sb.End();

            // Oscillate scroll direction with pendulum swing
            float scrollDir = swing * 2.0f;
            _arcanePagesShader.Parameters["uColor"]?.SetValue(ClairDeLunePalette.SoftBlue.ToVector4());
            _arcanePagesShader.Parameters["uSecondaryColor"]?.SetValue(ClairDeLunePalette.MoonbeamGold.ToVector4());
            _arcanePagesShader.Parameters["uOpacity"]?.SetValue(fade * 0.45f);
            _arcanePagesShader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
            _arcanePagesShader.Parameters["uIntensity"]?.SetValue(1.0f);
            _arcanePagesShader.Parameters["uOverbrightMult"]?.SetValue(1.3f);
            _arcanePagesShader.Parameters["uScrollSpeed"]?.SetValue(1.0f + MathF.Abs(scrollDir) * 0.5f);
            _arcanePagesShader.Parameters["uDistortionAmt"]?.SetValue(0.02f);
            _arcanePagesShader.Parameters["uHasSecondaryTex"]?.SetValue(_noiseTex != null);

            if (_noiseTex != null)
            {
                Main.graphics.GraphicsDevice.Textures[1] = _noiseTex.Value;
                _arcanePagesShader.Parameters["uSecondaryTexScale"]?.SetValue(1.5f);
                _arcanePagesShader.Parameters["uSecondaryTexScroll"]?.SetValue(new Vector2(scrollDir * 0.1f, -0.2f));
            }

            _arcanePagesShader.CurrentTechnique = _arcanePagesShader.Techniques["ArcanePageFlow"];

            sb.Begin(SpriteSortMode.Immediate, BlendState.Additive,
                SamplerState.LinearWrap, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, _arcanePagesShader, matrix);

            Texture2D sc = _softCircle.Value;
            float diam = radius * 2f;
            sb.Draw(sc, drawCenter, null, Color.White, 0f, sc.Size() * 0.5f,
                diam / sc.Width, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                SamplerState.LinearClamp, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null, matrix);
        }

        // ---- PASS 2: ClairDeLuneMoonlit ambient glow at swing center ----
        private void DrawMoonlitAmbient(SpriteBatch sb, Matrix matrix, Vector2 drawCenter, float radius, float fade, float pulse)
        {
            _moonlitShader ??= ShaderLoader.ClairDeLuneMoonlit;
            if (_moonlitShader == null) return;

            sb.End();

            _moonlitShader.Parameters["uColor"]?.SetValue(ClairDeLunePalette.NightMist.ToVector4());
            _moonlitShader.Parameters["uSecondaryColor"]?.SetValue(ClairDeLunePalette.DreamHaze.ToVector4());
            _moonlitShader.Parameters["uOpacity"]?.SetValue(fade * 0.2f * pulse);
            _moonlitShader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
            _moonlitShader.Parameters["uIntensity"]?.SetValue(0.7f);
            _moonlitShader.Parameters["uOverbrightMult"]?.SetValue(1.0f);
            _moonlitShader.Parameters["uScrollSpeed"]?.SetValue(0.3f);
            _moonlitShader.Parameters["uHasSecondaryTex"]?.SetValue(false);

            _moonlitShader.CurrentTechnique = _moonlitShader.Techniques["MoonlitGlow"];

            sb.Begin(SpriteSortMode.Immediate, BlendState.Additive,
                SamplerState.LinearWrap, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, _moonlitShader, matrix);

            Texture2D sc = _softCircle.Value;
            float ambientDiam = radius * 2.4f;
            sb.Draw(sc, drawCenter, null, Color.White, 0f, sc.Size() * 0.5f,
                ambientDiam / sc.Width, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                SamplerState.LinearClamp, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null, matrix);
        }

        // ---- PASS 3: Bloom composite (pendulum arm, core, swing extreme marks) ----
        private void DrawBloomComposite(SpriteBatch sb, Matrix matrix, Vector2 drawCenter, float radius, float fade, float swing, float pulse)
        {
            Texture2D srb = _softRadialBloom.Value;
            Texture2D pb = _pointBloom.Value;
            Texture2D sf = _starFlare.Value;
            Vector2 anchorDraw = Projectile.Center - Main.screenPosition;

            // Pendulum arm — line from anchor to swing center
            int armPts = 8;
            for (int i = 0; i < armPts; i++)
            {
                float t = (float)i / armPts;
                Vector2 armPos = Vector2.Lerp(anchorDraw, drawCenter, t);
                float armAlpha = 0.12f * (1f - t * 0.3f) * fade;
                sb.Draw(pb, armPos, null, ClairDeLunePalette.ClockworkBrass with { A = 0 } * armAlpha,
                    0f, pb.Size() * 0.5f, 4f / pb.Width, SpriteEffects.None, 0f);
            }

            // Swing extreme marks at max displacement
            float extremeAlpha = MathF.Max(MathF.Abs(swing) - 0.7f, 0f) / 0.3f;
            if (extremeAlpha > 0.01f)
            {
                sb.Draw(sf, drawCenter, null,
                    ClairDeLunePalette.MoonbeamGold with { A = 0 } * 0.25f * extremeAlpha * fade,
                    Main.GameUpdateCount * 0.08f, sf.Size() * 0.5f,
                    (16f + extremeAlpha * 8f) / sf.Width, SpriteEffects.None, 0f);
            }

            // Zone ambient halo
            sb.Draw(srb, drawCenter, null,
                ClairDeLunePalette.NightMist with { A = 0 } * 0.08f * fade * pulse,
                0f, srb.Size() * 0.5f, radius * 2f / srb.Width, SpriteEffects.None, 0f);

            // Mid ring
            sb.Draw(srb, drawCenter, null,
                ClairDeLunePalette.ClockworkBrass with { A = 0 } * 0.12f * fade,
                0f, srb.Size() * 0.5f, radius * 1.4f / srb.Width, SpriteEffects.None, 0f);

            // Core glow — intensity follows swing
            sb.Draw(pb, drawCenter, null,
                ClairDeLunePalette.MoonbeamGold with { A = 0 } * 0.2f * fade * MathF.Abs(swing),
                0f, pb.Size() * 0.5f, 10f / pb.Width, SpriteEffects.None, 0f);

            // Anchor point marker
            sb.Draw(pb, anchorDraw, null,
                ClairDeLunePalette.ClockworkBrass with { A = 0 } * 0.15f * fade,
                0f, pb.Size() * 0.5f, 6f / pb.Width, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullNone, null, matrix);
        }
    }
}
