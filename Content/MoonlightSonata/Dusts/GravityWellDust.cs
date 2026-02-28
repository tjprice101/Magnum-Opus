using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using ReLogic.Content;

namespace MagnumOpus.Content.MoonlightSonata.Dusts
{
    /// <summary>
    /// Gravity well dust for the Goliath of Moonlight  Ecosmic entity particles.
    /// Heavy particles that spiral inward toward a gravitational center, creating
    /// the visual of matter being drawn into a cosmic entity's gravitational pull.
    /// Uses GlowingHalo2.png for a dense nebula appearance.
    /// Color shifts from deep space violet ↁEnebula purple ↁEstar core white.
    /// </summary>
    public class GravityWellDust : ModDust
    {
        public override string Texture => "MagnumOpus/Assets/SandboxLastPrism/Orbs/circle_05";

        public override void OnSpawn(Dust dust)
        {
            dust.alpha = 0;
            dust.fadeIn = 0f;
            dust.noGravity = true;
            dust.noLight = false;
            dust.frame = new Rectangle(0, 0, 64, 64);
            dust.scale *= 0.25f;
        }

        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            return dust.color * dust.fadeIn;
        }

        public override bool Update(Dust dust)
        {
            dust.alpha++;

            if (dust.customData is GravityWellBehavior behavior)
            {
                float lifeProgress = (float)dust.alpha / behavior.Lifetime;

                // Color intensification: deep space ↁEnebula purple ↁEstar core
                float colorT = lifeProgress;
                if (colorT < 0.4f)
                {
                    // Approaching phase: dark cosmic void ↁEgravity well purple
                    dust.color = Color.Lerp(
                        new Color(30, 12, 60),
                        new Color(100, 60, 180),
                        colorT / 0.4f);
                }
                else if (colorT < 0.75f)
                {
                    // Close approach: gravity well ↁEnebula purple (brightening)
                    dust.color = Color.Lerp(
                        new Color(100, 60, 180),
                        new Color(180, 140, 255),
                        (colorT - 0.4f) / 0.35f);
                }
                else
                {
                    // Terminal phase: nebula ↁEstar core white (intense)
                    dust.color = Color.Lerp(
                        new Color(180, 140, 255),
                        new Color(255, 240, 220),
                        (colorT - 0.75f) / 0.25f);
                }

                // Spiral inward toward gravity center
                if (behavior.GravityCenter != Vector2.Zero)
                {
                    Vector2 toCenter = behavior.GravityCenter - dust.position;
                    float dist = toCenter.Length();

                    if (dist > 2f)
                    {
                        toCenter.Normalize();

                        // Tangential component for spiral motion
                        Vector2 tangent = new Vector2(-toCenter.Y, toCenter.X) * behavior.SpiralSpeed;

                        // Radial pull increases as particle gets closer (gravitational acceleration)
                        float pullStrength = behavior.PullStrength * (1f + lifeProgress * 2f);
                        Vector2 radialPull = toCenter * pullStrength;

                        dust.velocity += radialPull + tangent * (1f - lifeProgress * 0.5f);
                    }
                }

                // Apply velocity with deceleration
                dust.position += dust.velocity;
                dust.velocity *= behavior.VelocityDecay;

                // Rotation  Espinning as it spirals
                dust.rotation += behavior.RotationSpeed * (1f + lifeProgress);

                // Scale  Egrows slightly as it intensifies, then shrinks at terminal phase
                float gravitationalScale;
                if (lifeProgress < 0.6f)
                    gravitationalScale = 1f + lifeProgress * 0.5f;
                else
                    gravitationalScale = 1.3f - (lifeProgress - 0.6f) * 2f;

                float pulseRate = behavior.PulseFrequency;
                float pulse = 1f + MathF.Sin(dust.alpha * pulseRate + behavior.PhaseOffset) * 0.15f;
                dust.scale = behavior.BaseScale * gravitationalScale * pulse;

                // Fade  Equick ignite, hold, then absorb at end
                if (dust.alpha < 4)
                    dust.fadeIn = dust.alpha / 4f;
                else if (lifeProgress < 0.7f)
                    dust.fadeIn = 1f;
                else
                    dust.fadeIn = MathHelper.Clamp(1f - (lifeProgress - 0.7f) / 0.3f, 0f, 1f);

                if (dust.alpha > behavior.Lifetime || dust.fadeIn < 0.02f)
                    dust.active = false;
            }
            else
            {
                // Default behavior  Esimple spiral fade
                dust.position += dust.velocity;
                dust.velocity *= 0.96f;
                dust.rotation += 0.05f;
                dust.scale *= 0.98f;

                float colorT = (float)dust.alpha / 35f;
                dust.color = Color.Lerp(new Color(60, 30, 120), new Color(200, 160, 255), colorT);
                dust.fadeIn = MathHelper.Clamp(1f - colorT, 0f, 1f);

                if (dust.alpha > 35 || dust.fadeIn < 0.02f)
                    dust.active = false;
            }

            if (!dust.noLight)
            {
                float lightIntensity = dust.fadeIn * Math.Min(dust.scale, 1f);
                Lighting.AddLight(dust.position,
                    dust.color.ToVector3() * 0.35f * lightIntensity);
            }

            return false;
        }

        public override bool PreDraw(Dust dust)
        {
            var mainTex = Texture2D.Value;
            Vector2 drawPos = dust.position - Main.screenPosition;
            Vector2 origin = new Vector2(32, 32);

            // Layer 1: Outer nebula haze (diffuse, wide)
            Main.EntitySpriteDraw(mainTex, drawPos, dust.frame,
                (dust.color with { A = 0 }) * 0.25f * dust.fadeIn,
                dust.rotation, origin, dust.scale * 2.2f,
                SpriteEffects.None);

            // Layer 2: Mid gravity well glow
            Color midColor = Color.Lerp(dust.color, new Color(150, 80, 220), 0.4f);
            Main.EntitySpriteDraw(mainTex, drawPos, dust.frame,
                (midColor with { A = 0 }) * 0.45f * dust.fadeIn,
                -dust.rotation * 0.5f, origin, dust.scale * 1.2f,
                SpriteEffects.None);

            // Layer 3: Inner star-core bright point
            Color coreColor = Color.Lerp(dust.color, Color.White, 0.6f);
            Main.EntitySpriteDraw(mainTex, drawPos, dust.frame,
                (coreColor with { A = 0 }) * 0.60f * dust.fadeIn,
                dust.rotation * 1.3f, origin, dust.scale * 0.5f,
                SpriteEffects.None);

            return false;
        }
    }

    public class GravityWellBehavior
    {
        public Vector2 GravityCenter = Vector2.Zero;
        public float PullStrength = 0.08f;
        public float SpiralSpeed = 0.4f;
        public float VelocityDecay = 0.97f;
        public float RotationSpeed = 0.04f;
        public float PulseFrequency = 0.3f;
        public float PhaseOffset;
        public float BaseScale = 0.25f;
        public int Lifetime = 30;

        public GravityWellBehavior()
        {
            PhaseOffset = Main.rand.NextFloat(MathHelper.TwoPi);
        }

        public GravityWellBehavior(Vector2 center, float baseScale = 0.25f, int lifetime = 30)
        {
            GravityCenter = center;
            BaseScale = baseScale;
            Lifetime = lifetime;
            PhaseOffset = Main.rand.NextFloat(MathHelper.TwoPi);
        }
    }
}
