using Microsoft.Xna.Framework;

namespace MagnumOpus.Content.Eroica.Weapons.SakurasBlossom.Dusts
{
    /// <summary>
    /// Static factory for creating pre-configured dust behavior objects.
    /// Follows SLPDustBehaviorUtil pattern — concise spawning code, detailed tuning here.
    /// </summary>
    public static class SBDustBehaviorUtil
    {
        // ═══════════════════════════════════════════════════════
        //  SAKURA PETAL DUST
        // ═══════════════════════════════════════════════════════

        public static SakuraPetalBehavior Petal(
            float driftFrequency = 2.5f,
            float driftAmplitude = 1.2f,
            float gravity = 0.04f,
            float rotationSpeed = 0.03f,
            float baseScale = 0.35f,
            int lifetime = 45)
        {
            return new SakuraPetalBehavior
            {
                DriftFrequency = driftFrequency,
                DriftAmplitude = driftAmplitude,
                Gravity = gravity,
                RotationSpeed = rotationSpeed,
                BaseScale = baseScale,
                Lifetime = lifetime
            };
        }

        // ═══════════════════════════════════════════════════════
        //  POLLEN MOTE DUST
        // ═══════════════════════════════════════════════════════

        public static PollenMoteBehavior Pollen(
            float riseSpeed = 0.6f,
            float driftAmplitude = 0.8f,
            float twinkleSpeed = 0.25f,
            float baseScale = 0.2f,
            int lifetime = 30)
        {
            return new PollenMoteBehavior
            {
                RiseSpeed = riseSpeed,
                DriftAmplitude = driftAmplitude,
                TwinkleSpeed = twinkleSpeed,
                BaseScale = baseScale,
                Lifetime = lifetime
            };
        }

        // ═══════════════════════════════════════════════════════
        //  BLOSSOM GLOW ORB
        // ═══════════════════════════════════════════════════════

        public static BlossomGlowOrbBehavior GlowOrb(
            float decelerationPower = 0.88f,
            float pulseFrequency = 0.12f,
            float glowIntensity = 0.6f,
            float baseScale = 0.4f,
            int lifetime = 25)
        {
            return new BlossomGlowOrbBehavior
            {
                DecelerationPower = decelerationPower,
                PulseFrequency = pulseFrequency,
                GlowIntensity = glowIntensity,
                BaseScale = baseScale,
                Lifetime = lifetime
            };
        }

        // ═══════════════════════════════════════════════════════
        //  PETAL WIND LINE
        // ═══════════════════════════════════════════════════════

        public static PetalWindLineBehavior WindLine(
            float velFadePower = 0.94f,
            float shrinkYPower = 0.88f,
            int timeToStartShrink = 12,
            int killEarlyTime = 60,
            float xScale = 1f,
            float yScale = 0.5f,
            bool drawWhiteCore = true)
        {
            return new PetalWindLineBehavior
            {
                VelFadePower = velFadePower,
                ShrinkYPower = shrinkYPower,
                TimeToStartShrink = timeToStartShrink,
                KillEarlyTime = killEarlyTime,
                Vec2Scale = new Vector2(xScale, yScale),
                DrawWhiteCore = drawWhiteCore
            };
        }

        // ═══════════════════════════════════════════════════════
        //  SAKURA EMBER DUST
        // ═══════════════════════════════════════════════════════

        public static SakuraEmberBehavior Ember(
            float gravity = 0.06f,
            float rotationSpeed = 0.15f,
            float velDecay = 0.96f,
            float baseScale = 0.3f,
            int lifetime = 25)
        {
            return new SakuraEmberBehavior
            {
                Gravity = gravity,
                RotationSpeed = rotationSpeed,
                VelDecay = velDecay,
                BaseScale = baseScale,
                Lifetime = lifetime
            };
        }

        // ═══════════════════════════════════════════════════════
        //  BLOSSOM RING DUST
        // ═══════════════════════════════════════════════════════

        public static BlossomRingBehavior Ring(
            float expandSpeed = 0.08f,
            float maxScale = 1.5f,
            float fadePower = 0.92f,
            int lifetime = 25)
        {
            return new BlossomRingBehavior
            {
                ExpandSpeed = expandSpeed,
                MaxScale = maxScale,
                FadePower = fadePower,
                Lifetime = lifetime
            };
        }

        // ═══════════════════════════════════════════════════════
        //  SPRING SPARK DUST
        // ═══════════════════════════════════════════════════════

        public static SpringSparkBehavior Spark(
            float velDecay = 0.90f,
            float rotationSpeed = 0.2f,
            float twinkleSpeed = 0.25f,
            float baseScale = 0.3f,
            int lifetime = 20)
        {
            return new SpringSparkBehavior
            {
                VelDecay = velDecay,
                RotationSpeed = rotationSpeed,
                TwinkleSpeed = twinkleSpeed,
                BaseScale = baseScale,
                Lifetime = lifetime
            };
        }
    }

    // ═══════════════════════════════════════════════════════════
    //  BEHAVIOR DATA CLASSES
    // ═══════════════════════════════════════════════════════════

    public class SakuraPetalBehavior
    {
        public float DriftFrequency = 2.5f;
        public float DriftAmplitude = 1.2f;
        public float Gravity = 0.04f;
        public float RotationSpeed = 0.03f;
        public float BaseScale = 0.35f;
        public int Lifetime = 45;
    }

    public class PollenMoteBehavior
    {
        public float RiseSpeed = 0.6f;
        public float DriftAmplitude = 0.8f;
        public float TwinkleSpeed = 0.25f;
        public float BaseScale = 0.2f;
        public int Lifetime = 30;
    }

    public class BlossomGlowOrbBehavior
    {
        public float DecelerationPower = 0.88f;
        public float PulseFrequency = 0.12f;
        public float GlowIntensity = 0.6f;
        public float BaseScale = 0.4f;
        public int Lifetime = 25;
    }

    public class PetalWindLineBehavior
    {
        public float VelFadePower = 0.94f;
        public float ShrinkYPower = 0.88f;
        public int TimeToStartShrink = 12;
        public int KillEarlyTime = 60;
        public Vector2 Vec2Scale = new Vector2(1f, 0.5f);
        public bool DrawWhiteCore = true;
        public float InitialVelLength;
    }

    public class SakuraEmberBehavior
    {
        public float Gravity = 0.06f;
        public float RotationSpeed = 0.15f;
        public float VelDecay = 0.96f;
        public float BaseScale = 0.3f;
        public int Lifetime = 25;
    }

    public class BlossomRingBehavior
    {
        public float ExpandSpeed = 0.08f;
        public float MaxScale = 1.5f;
        public float FadePower = 0.92f;
        public int Lifetime = 25;
    }

    public class SpringSparkBehavior
    {
        public float VelDecay = 0.90f;
        public float RotationSpeed = 0.2f;
        public float TwinkleSpeed = 0.25f;
        public float BaseScale = 0.3f;
        public int Lifetime = 20;
        public float PhaseOffset;
    }
}
