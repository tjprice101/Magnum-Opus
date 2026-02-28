using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using ReLogic.Content;

namespace MagnumOpus.Content.MoonlightSonata.Dusts
{
    /// <summary>
    /// Crescent-shaped moonlight mote that gently orbits and fades.
    /// Inspired by VFX+ PixelGlowOrb's multi-layer rendering approach.
    /// Used across all Moonlight Sonata weapons for ambient lunar particles.
    /// </summary>
    public class LunarMote : ModDust
    {
        public override string Texture => "MagnumOpus/Assets/Particles Asset Library/Stars/4PointedStarSoft";

        private static Texture2D _glowTex;

        public override void Load()
        {
            _glowTex = ModContent.Request<Texture2D>(
                "MagnumOpus/Assets/SandboxLastPrism/Orbs/SoftGlow", AssetRequestMode.ImmediateLoad).Value;
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
            dust.scale *= 0.4f;
        }

        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            return dust.color * dust.fadeIn;
        }

        public override bool Update(Dust dust)
        {
            dust.alpha++;

            if (dust.customData is LunarMoteBehavior behavior)
            {
                // Orbital motion around spawn center
                float orbitSpeed = behavior.OrbitSpeed;
                float orbitRadius = behavior.OrbitRadius;
                float angle = dust.alpha * orbitSpeed + behavior.InitialAngle;

                if (behavior.OrbitCenter != Vector2.Zero)
                {
                    Vector2 targetPos = behavior.OrbitCenter + angle.ToRotationVector2() * orbitRadius;
                    dust.position = Vector2.Lerp(dust.position, targetPos, 0.08f);
                }
                else
                {
                    dust.position += dust.velocity;
                }

                // Bobbing vertical motion
                dust.position.Y += MathF.Sin(dust.alpha * 0.08f) * 0.3f;

                // Gentle rotation
                dust.rotation += behavior.RotationSpeed;

                // Fade timing
                if (dust.alpha > behavior.FadeStartTime)
                {
                    dust.fadeIn *= behavior.FadePower;
                    dust.scale *= 0.97f;
                }

                // Scale pulse
                float pulse = 1f + MathF.Sin(dust.alpha * 0.12f) * 0.08f;
                dust.scale *= pulse / (1f + MathF.Sin((dust.alpha - 1) * 0.12f) * 0.08f);

                if (dust.alpha > behavior.Lifetime || dust.scale < 0.05f)
                    dust.active = false;
            }
            else
            {
                dust.position += dust.velocity;
                dust.velocity *= 0.96f;
                dust.rotation += 0.03f;

                if (dust.alpha > 15)
                {
                    dust.fadeIn *= 0.94f;
                    dust.scale *= 0.97f;
                }

                if (dust.scale < 0.05f || dust.alpha > 60)
                    dust.active = false;
            }

            if (!dust.noLight)
                Lighting.AddLight(dust.position, dust.color.ToVector3() * 0.35f * dust.scale * dust.fadeIn);

            return false;
        }

        public override bool PreDraw(Dust dust)
        {
            var mainTex = Texture2D.Value;
            Vector2 drawPos = dust.position - Main.screenPosition;
            Vector2 origin = new Vector2(64, 64);

            // Layer 1: Soft glow halo behind
            if (_glowTex != null)
            {
                Main.EntitySpriteDraw(_glowTex, drawPos, null,
                    (dust.color with { A = 0 }) * 0.25f * dust.fadeIn,
                    0f, _glowTex.Size() / 2f, dust.scale * 1.8f,
                    SpriteEffects.None);
            }

            // Layer 2: Main crescent with rotation
            Main.EntitySpriteDraw(mainTex, drawPos, dust.frame,
                (dust.color with { A = 0 }) * 0.8f * dust.fadeIn,
                dust.rotation, origin, dust.scale,
                SpriteEffects.None);

            // Layer 3: Bright white core
            Main.EntitySpriteDraw(mainTex, drawPos, dust.frame,
                (Color.White with { A = 0 }) * 0.3f * dust.fadeIn,
                dust.rotation, origin, dust.scale * 0.6f,
                SpriteEffects.None);

            return false;
        }
    }

    public class LunarMoteBehavior
    {
        public Vector2 OrbitCenter = Vector2.Zero;
        public float OrbitSpeed = 0.04f;
        public float OrbitRadius = 25f;
        public float InitialAngle;
        public float RotationSpeed = 0.03f;
        public int FadeStartTime = 25;
        public float FadePower = 0.94f;
        public int Lifetime = 60;

        public LunarMoteBehavior() { }

        public LunarMoteBehavior(Vector2 orbitCenter, float initialAngle = 0f)
        {
            OrbitCenter = orbitCenter;
            InitialAngle = initialAngle;
        }
    }
}
