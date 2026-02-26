using System;
using Microsoft.Xna.Framework;
using Terraria;

namespace MagnumOpus.Content.Eroica.Weapons.CelestialValor.Dusts
{
    /// <summary>
    /// Factory methods for Celestial Valor dust behavior configs.
    /// Follows the SLPDustBehaviorUtil pattern — creates typed behavior
    /// objects stored in dust.customData for Update/PreDraw dispatch.
    /// </summary>
    public static class CVDustBehaviorUtil
    {
        // ═══════════════════════════════════════════════════════
        //  BEHAVIOR DATA CLASSES
        // ═══════════════════════════════════════════════════════

        public class HeroicEmberBehavior
        {
            public float VelocityDecay;
            public float ScaleShrinkStart;
            public float MinScale;
            public float FadeRate;
            public float RotationSpeed;
            public int MaxLife;
            public int Age;
            public Color StartColor;
            public Color EndColor;

            public void Update(Dust dust)
            {
                Age++;
                float lifeProgress = (float)Age / MaxLife;

                dust.velocity *= VelocityDecay;
                dust.rotation += RotationSpeed;
                dust.color = Color.Lerp(StartColor, EndColor, lifeProgress);

                if (lifeProgress > ScaleShrinkStart)
                {
                    float shrinkProgress = (lifeProgress - ScaleShrinkStart) / (1f - ScaleShrinkStart);
                    dust.scale = MathHelper.Lerp(dust.scale, MinScale, shrinkProgress * 0.08f);
                }

                dust.color *= (1f - lifeProgress * FadeRate);

                if (Age >= MaxLife || dust.scale < 0.05f)
                    dust.active = false;
            }
        }

        public class ValorSparkBehavior
        {
            public float VelocityDecay;
            public float ScaleOscillationSpeed;
            public float ScaleOscillationAmp;
            public float BaseScale;
            public int MaxLife;
            public int Age;

            public void Update(Dust dust)
            {
                Age++;
                float lifeProgress = (float)Age / MaxLife;

                dust.velocity *= VelocityDecay;
                dust.scale = BaseScale + (float)Math.Sin(Age * ScaleOscillationSpeed) * ScaleOscillationAmp;
                dust.scale *= (1f - lifeProgress);

                // Elongate along velocity direction
                if (dust.velocity.LengthSquared() > 0.5f)
                    dust.rotation = dust.velocity.ToRotation();

                if (Age >= MaxLife || dust.scale < 0.03f)
                    dust.active = false;
            }
        }

        public class SakuraPetalBehavior
        {
            public float DriftSpeed;
            public float RotationDrift;
            public float GravityStrength;
            public float SineAmplitude;
            public float SineFrequency;
            public int MaxLife;
            public int Age;

            public void Update(Dust dust)
            {
                Age++;
                float lifeProgress = (float)Age / MaxLife;

                // Gentle sine-wave horizontal drift
                dust.velocity.X += (float)Math.Sin(Age * SineFrequency) * SineAmplitude;
                dust.velocity.Y += GravityStrength;
                dust.velocity *= 0.97f;

                dust.rotation += RotationDrift;
                dust.scale *= (1f - lifeProgress * 0.02f);
                dust.color *= (1f - lifeProgress * 0.5f);

                if (Age >= MaxLife || dust.scale < 0.05f)
                    dust.active = false;
            }
        }

        public class FlameRibbonBehavior
        {
            public float VelocityDecay;
            public float RotationAccel;
            public float ScaleGrowth;
            public float MaxScaleMultiplier;
            public int MaxLife;
            public int Age;
            public Color CoreColor;
            public Color EdgeColor;

            public void Update(Dust dust)
            {
                Age++;
                float lifeProgress = (float)Age / MaxLife;

                dust.velocity *= VelocityDecay;
                dust.rotation += RotationAccel * (1f - lifeProgress);

                // Grow then shrink
                if (lifeProgress < 0.3f)
                    dust.scale = MathHelper.Lerp(dust.scale, dust.scale * MaxScaleMultiplier, ScaleGrowth);
                else
                    dust.scale *= (1f - (lifeProgress - 0.3f) * 0.04f);

                dust.color = Color.Lerp(CoreColor, EdgeColor, lifeProgress);
                dust.color *= (1f - lifeProgress);

                if (Age >= MaxLife || dust.scale < 0.05f)
                    dust.active = false;
            }
        }

        // ═══════════════════════════════════════════════════════
        //  FACTORY METHODS
        // ═══════════════════════════════════════════════════════

        /// <summary>Rising ember that fades from scarlet to gold.</summary>
        public static HeroicEmberBehavior CreateHeroicEmber(int maxLife = 40)
        {
            return new HeroicEmberBehavior
            {
                VelocityDecay = 0.96f,
                ScaleShrinkStart = 0.5f,
                MinScale = 0.1f,
                FadeRate = 0.7f,
                RotationSpeed = Main.rand.NextFloat(-0.05f, 0.05f),
                MaxLife = maxLife,
                Age = 0,
                StartColor = EroicaPalette.Scarlet,
                EndColor = EroicaPalette.Gold,
            };
        }

        /// <summary>Bright fast-moving spark that elongates along velocity.</summary>
        public static ValorSparkBehavior CreateValorSpark(float baseScale = 1.2f, int maxLife = 25)
        {
            return new ValorSparkBehavior
            {
                VelocityDecay = 0.92f,
                ScaleOscillationSpeed = 0.4f,
                ScaleOscillationAmp = 0.15f,
                BaseScale = baseScale,
                MaxLife = maxLife,
                Age = 0,
            };
        }

        /// <summary>Slow-drifting sakura petal with sine-wave wobble.</summary>
        public static SakuraPetalBehavior CreateSakuraPetal(int maxLife = 60)
        {
            return new SakuraPetalBehavior
            {
                DriftSpeed = Main.rand.NextFloat(0.3f, 0.8f),
                RotationDrift = Main.rand.NextFloat(-0.04f, 0.04f),
                GravityStrength = 0.015f,
                SineAmplitude = 0.08f,
                SineFrequency = 0.12f,
                MaxLife = maxLife,
                Age = 0,
            };
        }

        /// <summary>Flame ribbon that grows, then shrinks — used for heroic fire trails.</summary>
        public static FlameRibbonBehavior CreateFlameRibbon(int maxLife = 35)
        {
            return new FlameRibbonBehavior
            {
                VelocityDecay = 0.94f,
                RotationAccel = Main.rand.NextFloat(-0.08f, 0.08f),
                ScaleGrowth = 0.02f,
                MaxScaleMultiplier = 1.4f,
                MaxLife = maxLife,
                Age = 0,
                CoreColor = EroicaPalette.HotCore,
                EdgeColor = EroicaPalette.DeepScarlet,
            };
        }
    }
}
