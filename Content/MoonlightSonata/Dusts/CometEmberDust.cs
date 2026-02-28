using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using ReLogic.Content;

namespace MagnumOpus.Content.MoonlightSonata.Dusts
{
    /// <summary>
    /// Burning comet ember dust for Resurrection of the Moon  E"The Final Movement".
    /// Hot, weighty fragments that trail smoldering wakes as they decelerate.
    /// Uses SoftGlow3.png for a warm, burning ember appearance.
    /// Color shifts from gold-white core to deep violet as the ember cools.
    /// </summary>
    public class CometEmberDust : ModDust
    {
        public override string Texture => "MagnumOpus/Assets/SandboxLastPrism/Orbs/SoftGlow64";

        public override void OnSpawn(Dust dust)
        {
            dust.alpha = 0;
            dust.fadeIn = 1f;
            dust.noGravity = true;
            dust.noLight = false;
            dust.frame = new Rectangle(0, 0, 64, 64);
            dust.scale *= 0.3f;
        }

        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            return dust.color * dust.fadeIn;
        }

        public override bool Update(Dust dust)
        {
            dust.alpha++;

            if (dust.customData is CometEmberBehavior behavior)
            {
                float lifeProgress = (float)dust.alpha / behavior.Lifetime;

                // Color cooling: hot gold-white ↁEcomet violet ↁEdeep space violet
                float coolingT = lifeProgress;
                if (coolingT < 0.3f)
                {
                    // Hot phase: white-gold
                    dust.color = Color.Lerp(
                        new Color(255, 240, 210),
                        new Color(255, 200, 130),
                        coolingT / 0.3f);
                }
                else if (coolingT < 0.7f)
                {
                    // Cooling phase: gold ↁEcomet violet
                    dust.color = Color.Lerp(
                        new Color(255, 200, 130),
                        new Color(180, 120, 255),
                        (coolingT - 0.3f) / 0.4f);
                }
                else
                {
                    // Dying phase: comet violet ↁEdeep space
                    dust.color = Color.Lerp(
                        new Color(180, 120, 255),
                        new Color(80, 40, 160),
                        (coolingT - 0.7f) / 0.3f);
                }

                // Movement with heavy deceleration (weighty feel)
                dust.position += dust.velocity;
                dust.velocity *= behavior.VelocityDecay;

                // Slight gravity pull for weight
                if (behavior.HasGravity)
                    dust.velocity.Y += 0.04f;

                // Rotation  Etumbling ember
                dust.rotation += behavior.RotationSpeed * (1f - lifeProgress * 0.5f);

                // Scale  Esmoldering pulse that shrinks as ember cools
                float smolder = MathF.Sin(dust.alpha * behavior.SmolderFrequency + behavior.PhaseOffset);
                float smolderScale = 1f + smolder * 0.2f * (1f - lifeProgress);
                dust.scale = behavior.BaseScale * smolderScale * (1f - lifeProgress * 0.7f);

                // Fade  Equick ignite, slow burn-out
                if (dust.alpha < 3)
                    dust.fadeIn = dust.alpha / 3f;
                else
                    dust.fadeIn = MathHelper.Clamp(1f - (lifeProgress - 0.4f) / 0.6f, 0f, 1f);

                if (dust.alpha > behavior.Lifetime || dust.fadeIn < 0.02f)
                    dust.active = false;
            }
            else
            {
                // Default behavior  Esimple ember fade
                dust.position += dust.velocity;
                dust.velocity *= 0.95f;
                dust.velocity.Y += 0.03f;
                dust.rotation += 0.06f;
                dust.scale *= 0.97f;

                float coolingT = (float)dust.alpha / 40f;
                dust.color = Color.Lerp(new Color(255, 220, 160), new Color(120, 60, 200), coolingT);
                dust.fadeIn = MathHelper.Clamp(1f - coolingT, 0f, 1f);

                if (dust.alpha > 40 || dust.fadeIn < 0.02f)
                    dust.active = false;
            }

            if (!dust.noLight)
            {
                float lightIntensity = dust.fadeIn * Math.Min(dust.scale, 1f);
                Lighting.AddLight(dust.position,
                    dust.color.ToVector3() * 0.4f * lightIntensity);
            }

            return false;
        }

        public override bool PreDraw(Dust dust)
        {
            var mainTex = Texture2D.Value;
            Vector2 drawPos = dust.position - Main.screenPosition;
            Vector2 origin = new Vector2(32, 32);

            // Layer 1: Outer smolder glow (warm, diffuse)
            Main.EntitySpriteDraw(mainTex, drawPos, dust.frame,
                (dust.color with { A = 0 }) * 0.35f * dust.fadeIn,
                dust.rotation, origin, dust.scale * 1.8f,
                SpriteEffects.None);

            // Layer 2: Inner bright ember core
            Color coreColor = Color.Lerp(dust.color, Color.White, 0.5f);
            Main.EntitySpriteDraw(mainTex, drawPos, dust.frame,
                (coreColor with { A = 0 }) * 0.65f * dust.fadeIn,
                -dust.rotation * 0.3f, origin, dust.scale * 0.7f,
                SpriteEffects.None);

            // Layer 3: Faint heat distortion halo
            Main.EntitySpriteDraw(mainTex, drawPos, dust.frame,
                (dust.color with { A = 0 }) * 0.12f * dust.fadeIn,
                dust.rotation * 1.5f, origin, dust.scale * 2.5f,
                SpriteEffects.None);

            return false;
        }
    }

    public class CometEmberBehavior
    {
        public float VelocityDecay = 0.95f;
        public float RotationSpeed = 0.06f;
        public float SmolderFrequency = 0.35f;
        public float PhaseOffset;
        public float BaseScale = 0.3f;
        public int Lifetime = 35;
        public bool HasGravity = true;

        public CometEmberBehavior()
        {
            PhaseOffset = Main.rand.NextFloat(MathHelper.TwoPi);
        }

        public CometEmberBehavior(float baseScale, int lifetime, bool hasGravity = true)
        {
            BaseScale = baseScale;
            Lifetime = lifetime;
            HasGravity = hasGravity;
            PhaseOffset = Main.rand.NextFloat(MathHelper.TwoPi);
        }
    }
}
