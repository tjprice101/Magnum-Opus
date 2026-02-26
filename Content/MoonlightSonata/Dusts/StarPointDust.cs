using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using ReLogic.Content;

namespace MagnumOpus.Content.MoonlightSonata.Dusts
{
    /// <summary>
    /// Sharp 4-point star sparkle dust for constellation and precision effects.
    /// Inspired by VFX+ GlowPixelCross's multi-layer approach.
    /// Rotates rapidly, scales down quickly — surgical, precise feel.
    /// Used for Incisor's constellation trail and resonant impacts.
    /// </summary>
    public class StarPointDust : ModDust
    {
        public override string Texture => "MagnumOpus/Assets/Particles/CrispStar4";

        private static Texture2D _glowTex;

        public override void Load()
        {
            _glowTex = ModContent.Request<Texture2D>(
                "MagnumOpus/Assets/Particles/SoftGlow2", AssetRequestMode.ImmediateLoad).Value;
        }

        public override void Unload()
        {
            _glowTex = null;
        }

        public override void OnSpawn(Dust dust)
        {
            dust.alpha = 0;
            dust.fadeIn = 1f;
            dust.noGravity = true;
            dust.noLight = false;
            dust.frame = new Rectangle(0, 0, 128, 128);
            dust.scale *= 0.25f;
        }

        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            return dust.color * dust.fadeIn;
        }

        public override bool Update(Dust dust)
        {
            dust.alpha++;

            if (dust.customData is StarPointBehavior behavior)
            {
                dust.position += dust.velocity;
                dust.velocity *= behavior.VelocityDecay;

                // Fast rotation for twinkling effect
                dust.rotation += behavior.RotationSpeed;

                // Scale pulse — rapid twinkle
                float twinkle = 1f + MathF.Sin(dust.alpha * behavior.TwinkleFrequency) * 0.2f;
                float baseFade = dust.alpha > behavior.FadeStartTime ?
                    MathF.Pow(behavior.FadePower, dust.alpha - behavior.FadeStartTime) : 1f;
                dust.fadeIn = baseFade * twinkle;

                // Gradual scale reduction
                if (dust.alpha > behavior.FadeStartTime)
                    dust.scale *= behavior.ScaleDecay;

                if (dust.alpha > behavior.Lifetime || dust.scale < 0.03f)
                    dust.active = false;
            }
            else
            {
                dust.position += dust.velocity;
                dust.velocity *= 0.93f;
                dust.rotation += 0.12f;

                float twinkle = 1f + MathF.Sin(dust.alpha * 0.4f) * 0.15f;
                dust.fadeIn = (dust.alpha > 10 ?
                    MathF.Pow(0.92f, dust.alpha - 10) : 1f) * twinkle;

                if (dust.alpha > 8)
                    dust.scale *= 0.96f;

                if (dust.scale < 0.03f || dust.alpha > 40)
                    dust.active = false;
            }

            if (!dust.noLight)
                Lighting.AddLight(dust.position,
                    dust.color.ToVector3() * 0.5f * dust.scale * dust.fadeIn);

            return false;
        }

        public override bool PreDraw(Dust dust)
        {
            var mainTex = Texture2D.Value;
            Vector2 drawPos = dust.position - Main.screenPosition;
            Vector2 origin = new Vector2(64, 64);

            // Layer 1: Soft glow halo (large, dim)
            if (_glowTex != null)
            {
                Main.EntitySpriteDraw(_glowTex, drawPos, null,
                    (dust.color with { A = 0 }) * 0.2f * dust.fadeIn,
                    0f, _glowTex.Size() / 2f, dust.scale * 2.5f,
                    SpriteEffects.None);
            }

            // Layer 2: Colored star (main)
            Main.EntitySpriteDraw(mainTex, drawPos, dust.frame,
                (dust.color with { A = 0 }) * 0.9f * dust.fadeIn,
                dust.rotation, origin, dust.scale,
                SpriteEffects.None);

            // Layer 3: Counter-rotating star overlay (slightly offset)
            Main.EntitySpriteDraw(mainTex, drawPos, dust.frame,
                (dust.color with { A = 0 }) * 0.4f * dust.fadeIn,
                -dust.rotation * 0.6f, origin, dust.scale * 0.7f,
                SpriteEffects.None);

            // Layer 4: White-hot core
            Main.EntitySpriteDraw(mainTex, drawPos, dust.frame,
                (Color.White with { A = 0 }) * 0.5f * dust.fadeIn,
                dust.rotation, origin, dust.scale * 0.35f,
                SpriteEffects.None);

            return false;
        }
    }

    public class StarPointBehavior
    {
        public float RotationSpeed = 0.12f;
        public float VelocityDecay = 0.93f;
        public float TwinkleFrequency = 0.4f;
        public int FadeStartTime = 8;
        public float FadePower = 0.92f;
        public float ScaleDecay = 0.96f;
        public int Lifetime = 40;

        public StarPointBehavior() { }

        public StarPointBehavior(float rotSpeed, int lifetime)
        {
            RotationSpeed = rotSpeed;
            Lifetime = lifetime;
        }
    }
}
