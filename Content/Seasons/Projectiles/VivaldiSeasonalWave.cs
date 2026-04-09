using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Graphics;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;
using static MagnumOpus.Common.Systems.DynamicParticleEffects;
using ReLogic.Content;

namespace MagnumOpus.Content.Seasons.Projectiles
{
    /// <summary>
    /// Vivaldi Seasonal Wave - Main projectile for Four Seasons Blade
    /// TRUE_VFX_STANDARDS: 6-layer spinning flares, dense dust, orbiting music notes, glimmer cascade
    /// Changes appearance and effects based on the season (ai[0])
    /// </summary>
    public class VivaldiSeasonalWave : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/ImpactEffects/ImpactEllipse";
        
        private VertexStrip _strip;
        private static readonly Color SpringPink = new Color(255, 183, 197);
        private static readonly Color SpringGreen = new Color(144, 238, 144);
        private static readonly Color SummerGold = new Color(255, 215, 0);
        private static readonly Color SummerOrange = new Color(255, 140, 0);
        private static readonly Color AutumnOrange = new Color(255, 140, 50);
        private static readonly Color AutumnBrown = new Color(139, 90, 43);
        private static readonly Color WinterBlue = new Color(150, 220, 255);
        private static readonly Color WinterWhite = new Color(240, 250, 255);

        private int SeasonIndex => (int)Projectile.ai[0];

        private Color PrimaryColor => SeasonIndex switch
        {
            0 => SpringPink,
            1 => SummerGold,
            2 => AutumnOrange,
            _ => WinterBlue
        };

        private Color SecondaryColor => SeasonIndex switch
        {
            0 => SpringGreen,
            1 => SummerOrange,
            2 => AutumnBrown,
            _ => WinterWhite
        };

        // Season-specific hue ranges for color oscillation
        private float HueMin => SeasonIndex switch
        {
            0 => 0.92f,  // Spring - pink/magenta
            1 => 0.08f,  // Summer - orange/gold
            2 => 0.06f,  // Autumn - orange/brown
            _ => 0.52f   // Winter - cyan/blue
        };

        private float HueMax => SeasonIndex switch
        {
            0 => 0.98f,  // Spring
            1 => 0.14f,  // Summer
            2 => 0.12f,  // Autumn
            _ => 0.62f   // Winter
        };

        // Season-specific dust types
        private int SeasonDustType => SeasonIndex switch
        {
            0 => DustID.PinkFairy,       // Spring
            1 => DustID.SolarFlare,      // Summer
            2 => DustID.AmberBolt,       // Autumn
            _ => DustID.IceTorch         // Winter
        };

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 16;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 70;
            Projectile.height = 70;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 5;
            Projectile.timeLeft = 90;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 30;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 8;
        }

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            
            // === ARCING SLASH ROTATION ===
            Vector2 toCursor = Main.MouseWorld - owner.Center;
            float cursorAngle = toCursor.ToRotation();
            float velocityAngle = Projectile.velocity.ToRotation();
            float blendFactor = 0.7f;
            Projectile.rotation = MathHelper.Lerp(velocityAngle, cursorAngle, blendFactor);

            Projectile.velocity *= 0.97f;

            // === DENSE DUST TRAIL - 2+ per frame GUARANTEED ===
            for (int i = 0; i < 2; i++)
            {
                Vector2 dustPos = Projectile.Center + Main.rand.NextVector2Circular(18f, 18f);
                Vector2 dustVel = -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(2f, 2f);
                float dustHue = HueMin + Main.rand.NextFloat() * (HueMax - HueMin);
                Color dustColor = Main.hslToRgb(dustHue, 0.85f, 0.7f);
                Dust d = Dust.NewDustPerfect(dustPos, SeasonDustType, dustVel, 80, dustColor, 1.6f);
                d.noGravity = true;
                d.fadeIn = 1.3f;
            }

            // === CONTRASTING SPARKLES - 1-in-2 ===
            if (Main.rand.NextBool(2))
            {
                Vector2 sparkPos = Projectile.Center + Main.rand.NextVector2Circular(15f, 15f);
                Dust contrast = Dust.NewDustPerfect(sparkPos, DustID.WhiteTorch,
                    -Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(1.5f, 1.5f), 0, Color.White, 1.2f);
                contrast.noGravity = true;
            }

            // === FREQUENT FLARES - 1-in-2 ===
            if (Main.rand.NextBool(2))
            {
                Vector2 flareOffset = Main.rand.NextVector2Circular(15f, 15f);
                float flareHue = HueMin + Main.rand.NextFloat() * (HueMax - HueMin);
                Color flareColor = Main.hslToRgb(flareHue, 0.9f, 0.72f);
                CustomParticles.GenericFlare(Projectile.Center + flareOffset, flareColor, 0.38f, 12);
            }

            // === SEASONAL GLOW TRAIL ===
            if (Main.rand.NextBool(2))
            {
                Vector2 trailPos = Projectile.Center + Main.rand.NextVector2Circular(18f, 18f);
                Vector2 trailVel = -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(2f, 2f);
                float trailHue = HueMin + Main.rand.NextFloat() * (HueMax - HueMin);
                Color trailColor = Main.hslToRgb(trailHue, 0.8f, 0.65f);
                var trail = new GenericGlowParticle(trailPos, trailVel, trailColor * 0.65f, 0.32f, 18, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }

            // === ORBITING MUSIC NOTES - LOCKED TO PROJECTILE ===
            float orbitAngle = Main.GameUpdateCount * 0.08f;
            if (Main.rand.NextBool(5))
            {
                for (int n = 0; n < 3; n++)
                {
                    float noteAngle = orbitAngle + MathHelper.TwoPi * n / 3f;
                    Vector2 noteOffset = noteAngle.ToRotationVector2() * 25f;
                    Vector2 notePos = Projectile.Center + noteOffset;
                    Vector2 noteVel = Projectile.velocity * 0.75f + noteAngle.ToRotationVector2() * 0.5f;
                    float noteHue = HueMin + (n / 3f) * (HueMax - HueMin);
                    Color noteColor = Main.hslToRgb(noteHue, 0.88f, 0.72f);
                    ThemedParticles.MusicNote(notePos, noteVel, noteColor, 0.78f, 35);

                    var sparkle = new SparkleParticle(notePos, noteVel * 0.4f, SecondaryColor * 0.6f, 0.25f, 18);
                    MagnumParticleHandler.SpawnParticle(sparkle);
                }
            }

            // === 4-ELEMENT SEASONAL ORBIT ===
            if (Main.rand.NextBool(4))
            {
                for (int i = 0; i < 4; i++)
                {
                    float elementAngle = orbitAngle * 1.2f + MathHelper.TwoPi * i / 4f;
                    float elementRadius = 20f + (float)Math.Sin(Main.GameUpdateCount * 0.08f + i * 0.7f) * 8f;
                    Vector2 elementPos = Projectile.Center + elementAngle.ToRotationVector2() * elementRadius;
                    float elementHue = HueMin + (i / 4f) * (HueMax - HueMin);
                    Color elementColor = Main.hslToRgb(elementHue, 0.85f, 0.7f);
                    CustomParticles.GenericFlare(elementPos, elementColor * 0.55f, 0.2f, 10);
                }
            }

            // === SPARKLE ACCENT ===
            if (Main.rand.NextBool(3))
            {
                var sparkle = new SparkleParticle(Projectile.Center + Main.rand.NextVector2Circular(12f, 12f), 
                    -Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(1.5f, 1.5f), SecondaryColor * 0.75f, 0.32f, 18);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            // === COLOR OSCILLATION - hslToRgb ===
            if (Main.rand.NextBool(3))
            {
                float hue = (Main.GameUpdateCount * 0.022f + Main.rand.NextFloat(0.1f)) % 1f;
                hue = HueMin + (hue * (HueMax - HueMin));
                Color shiftColor = Main.hslToRgb(hue, 0.88f, 0.72f);
                CustomParticles.GenericFlare(Projectile.Center, shiftColor, 0.3f, 10);
            }

            Lighting.AddLight(Projectile.Center, PrimaryColor.ToVector3() * 0.7f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Season-specific effects
            switch (SeasonIndex)
            {
                case 0: // Spring
                    target.AddBuff(BuffID.Poisoned, 180);
                    break;
                case 1: // Summer
                    target.AddBuff(BuffID.OnFire3, 240);
                    target.AddBuff(BuffID.Daybreak, 120);
                    break;
                case 2: // Autumn
                    target.AddBuff(BuffID.CursedInferno, 200);
                    Player owner = Main.player[Projectile.owner];
                    if (Main.rand.NextFloat() < 0.3f)
                    {
                        owner.Heal(Math.Max(1, damageDone / 25));
                    }
                    break;
                case 3: // Winter
                    target.AddBuff(BuffID.Frostburn2, 240);
                    if (Main.rand.NextFloat() < 0.2f)
                    {
                        target.AddBuff(BuffID.Frozen, 60);
                    }
                    break;
            }

            // === 3-LAYER FLASH CASCADE ===
            CustomParticles.GenericFlare(target.Center, Color.White, 0.6f, 20);
            CustomParticles.GenericFlare(target.Center, PrimaryColor, 0.5f, 18);
            CustomParticles.GenericFlare(target.Center, SecondaryColor, 0.42f, 16);

            // === 6 MUSIC NOTES WITH GRADIENT ===
            for (int n = 0; n < 6; n++)
            {
                float angle = MathHelper.TwoPi * n / 6f + Main.rand.NextFloat(-0.2f, 0.2f);
                Vector2 noteVel = angle.ToRotationVector2() * Main.rand.NextFloat(2.5f, 4.5f);
                float noteHue = HueMin + (n / 6f) * (HueMax - HueMin);
                Color noteColor = Main.hslToRgb(noteHue, 0.88f, 0.72f);
                ThemedParticles.MusicNote(target.Center, noteVel, noteColor, 0.78f, 32);
            }

            // === 6-POINT SEASONAL BURST ===
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 offset = angle.ToRotationVector2() * 25f;
                float burstHue = HueMin + (i / 6f) * (HueMax - HueMin);
                Color burstColor = Main.hslToRgb(burstHue, 0.85f, 0.7f);
                CustomParticles.GenericFlare(target.Center + offset, burstColor * 0.65f, 0.32f, 14);
            }

            // === 2 HALO RINGS ===
            CustomParticles.HaloRing(target.Center, PrimaryColor, 0.4f, 16);
            CustomParticles.HaloRing(target.Center, SecondaryColor * 0.7f, 0.32f, 14);

            // === 8 SPARKLE PARTICLES ===
            for (int s = 0; s < 8; s++)
            {
                float angle = MathHelper.TwoPi * s / 8f;
                Vector2 sparkVel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 5f);
                float sparkHue = HueMin + (s / 8f) * (HueMax - HueMin);
                Color sparkColor = Main.hslToRgb(sparkHue, 0.82f, 0.72f);
                var sparkle = new SparkleParticle(target.Center, sparkVel, sparkColor, 0.3f, 18);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            // === 8 GLOW PARTICLES ===
            for (int i = 0; i < 8; i++)
            {
                Vector2 glowVel = Main.rand.NextVector2Circular(5f, 5f);
                float glowHue = HueMin + Main.rand.NextFloat() * (HueMax - HueMin);
                Color glowColor = Main.hslToRgb(glowHue, 0.8f, 0.65f);
                var burst = new GenericGlowParticle(target.Center, glowVel, glowColor * 0.65f, 0.28f, 18, true);
                MagnumParticleHandler.SpawnParticle(burst);
            }
            
            // === DYNAMIC: Season-based Impact ===
            switch (SeasonIndex)
            {
                case 0: SpringImpact(target.Center, 0.9f); SpringVerdantTrail(target.Center, Projectile.velocity, 0.7f); break;
                case 1: SummerImpact(target.Center, 0.9f); SummerHeatWave(target.Center, 0.75f); break;
                case 2: AutumnImpact(target.Center, 0.9f); AutumnDecaySpiral(target.Center, 0.7f); break;
                case 3: WinterImpact(target.Center, 0.9f); WinterBlizzardVeil(target.Center, 0.75f); break;
            }
            DramaticImpact(target.Center, PrimaryColor, SecondaryColor, 0.45f, 18);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            var config = SeasonIndex switch
            {
                0 => IncisorOrbRenderer.Spring,
                1 => IncisorOrbRenderer.Summer,
                2 => IncisorOrbRenderer.Autumn,
                _ => IncisorOrbRenderer.Winter,
            };
            IncisorOrbRenderer.DrawOrbVisuals(Main.spriteBatch, Projectile, config, ref _strip);
            return false;
        }

        public override void OnKill(int timeLeft)
        {
            // === 4-LAYER CENTRAL GLIMMER CASCADE ===
            for (int layer = 0; layer < 4; layer++)
            {
                float layerScale = 0.4f + layer * 0.15f;
                float layerAlpha = 0.82f - layer * 0.15f;
                float layerHue = HueMin + (layer / 4f) * (HueMax - HueMin);
                Color layerColor = Color.Lerp(Color.White, Main.hslToRgb(layerHue, 0.85f, 0.72f), layer / 4f);
                CustomParticles.GenericFlare(Projectile.Center, layerColor * layerAlpha, layerScale, 18 - layer * 2);
            }
            
            // === 4 EXPANDING GLOW RINGS WITH hslToRgb ===
            for (int ring = 0; ring < 4; ring++)
            {
                float ringHue = HueMin + (ring / 4f) * (HueMax - HueMin);
                Color ringColor = Main.hslToRgb(ringHue, 0.82f, 0.68f);
                float ringScale = 0.35f + ring * 0.12f;
                CustomParticles.HaloRing(Projectile.Center, ringColor, ringScale, 14 + ring * 2);
            }

            // === 8-POINT SEASONAL PATTERN ===
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 offset = angle.ToRotationVector2() * 30f;
                float patternHue = HueMin + (i / 8f) * (HueMax - HueMin);
                Color patternColor = Main.hslToRgb(patternHue, 0.85f, 0.7f);
                CustomParticles.GenericFlare(Projectile.Center + offset, patternColor * 0.6f, 0.32f, 14);
            }

            // === 8 MUSIC NOTES FINALE ===
            for (int n = 0; n < 8; n++)
            {
                float angle = MathHelper.TwoPi * n / 8f;
                Vector2 noteVel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 5.5f);
                float noteHue = HueMin + (n / 8f) * (HueMax - HueMin);
                Color noteColor = Main.hslToRgb(noteHue, 0.88f, 0.72f);
                ThemedParticles.MusicNote(Projectile.Center, noteVel, noteColor, 0.8f, 35);
            }

            // === 10 SPARKLE BURST ===
            for (int s = 0; s < 10; s++)
            {
                float angle = MathHelper.TwoPi * s / 10f;
                Vector2 sparkVel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 7f);
                float sparkHue = HueMin + (s / 10f) * (HueMax - HueMin);
                Color sparkColor = Main.hslToRgb(sparkHue, 0.82f, 0.72f);
                var sparkle = new SparkleParticle(Projectile.Center, sparkVel, sparkColor, 0.35f, 20);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            // === 10 GLOW BURST ===
            for (int i = 0; i < 10; i++)
            {
                Vector2 burstVel = Main.rand.NextVector2Circular(6f, 6f);
                float burstHue = HueMin + Main.rand.NextFloat() * (HueMax - HueMin);
                Color burstColor = Main.hslToRgb(burstHue, 0.8f, 0.65f);
                var burst = new GenericGlowParticle(Projectile.Center, burstVel, burstColor * 0.7f, 0.32f, 20, true);
                MagnumParticleHandler.SpawnParticle(burst);
            }
            
            // === VANILLA DUST ===
            for (int i = 0; i < 8; i++)
            {
                Vector2 dustVel = Main.rand.NextVector2Circular(6f, 6f);
                Dust dust = Dust.NewDustPerfect(Projectile.Center, SeasonDustType, dustVel, 0, PrimaryColor, 1.2f);
                dust.noGravity = true;
                dust.fadeIn = 1f;
            }
            
            Lighting.AddLight(Projectile.Center, PrimaryColor.ToVector3() * 1.2f);
        }
    }
}
