using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using static MagnumOpus.Common.Systems.DynamicParticleEffects;

namespace MagnumOpus.Content.Seasons.Projectiles
{
    /// <summary>
    /// Vivaldi Seasonal Wave - Main projectile for Four Seasons Blade
    /// TRUE_VFX_STANDARDS: 6-layer spinning flares, dense dust, orbiting music notes, glimmer cascade
    /// Changes appearance and effects based on the season (ai[0])
    /// </summary>
    public class VivaldiSeasonalWave : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/SwordArc5";
        
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
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20;
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
                case 0: SpringImpact(target.Center, 0.9f); break;
                case 1: SummerImpact(target.Center, 0.9f); break;
                case 2: AutumnImpact(target.Center, 0.9f); break;
                case 3: WinterImpact(target.Center, 0.9f); break;
            }
            DramaticImpact(target.Center, PrimaryColor, SecondaryColor, 0.45f, 18);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D arcTexture = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SwordArc5").Value;
            Texture2D flareTexture = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/EnergyFlare").Value;
            Texture2D flareTexture2 = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/EnergyFlare3").Value;
            Texture2D softGlow = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow2").Value;
            Vector2 arcOrigin = arcTexture.Size() / 2f;
            Vector2 flareOrigin = flareTexture.Size() / 2f;
            Vector2 flareOrigin2 = flareTexture2.Size() / 2f;
            Vector2 glowOrigin = softGlow.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            
            float time = Main.GameUpdateCount * 0.055f;
            float pulse = 1f + (float)Math.Sin(time * 2.5f) * 0.18f;
            float shimmer = 1f + (float)Math.Sin(time * 3f + Projectile.whoAmI) * 0.12f;
            
            // Color with alpha removed for proper additive blending (Fargos pattern)
            Color primaryBloom = PrimaryColor with { A = 0 };
            Color secondaryBloom = SecondaryColor with { A = 0 };
            Color whiteBloom = Color.White with { A = 0 };

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // === BRILLIANT TRAIL WITH GRADIENT (TRUE_VFX_STANDARDS) ===
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                
                float progress = (float)i / Projectile.oldPos.Length;
                float fadeOut = 1f - progress;
                Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                
                // Gradient color through trail using hslToRgb
                float trailHue = HueMin + progress * (HueMax - HueMin);
                Color trailGradient = Main.hslToRgb(trailHue, 0.85f, 0.7f) with { A = 0 };
                
                // 3-layer trail
                float outerScale = 1.4f * fadeOut * pulse;
                spriteBatch.Draw(arcTexture, trailPos, null, trailGradient * 0.35f * fadeOut, Projectile.oldRot[i], arcOrigin, outerScale, SpriteEffects.None, 0f);
                
                float midScale = 0.9f * fadeOut;
                spriteBatch.Draw(arcTexture, trailPos, null, primaryBloom * 0.5f * fadeOut, Projectile.oldRot[i], arcOrigin, midScale, SpriteEffects.None, 0f);
                
                float innerScale = 0.55f * fadeOut;
                spriteBatch.Draw(arcTexture, trailPos, null, whiteBloom * 0.6f * fadeOut, Projectile.oldRot[i], arcOrigin, innerScale, SpriteEffects.None, 0f);
            }

            // === 6-LAYER SPINNING FLARES (TRUE_VFX_STANDARDS) ===
            // Layer 1: Soft glow base (large, dim)
            spriteBatch.Draw(softGlow, drawPos, null, secondaryBloom * 0.3f, 0f, glowOrigin, 0.9f * pulse, SpriteEffects.None, 0f);

            // Layer 2: First flare spinning clockwise
            float hue2 = HueMin + 0.2f * (HueMax - HueMin);
            Color layer2Color = Main.hslToRgb(hue2, 0.88f, 0.72f) with { A = 0 };
            spriteBatch.Draw(flareTexture, drawPos, null, layer2Color * 0.55f, time, flareOrigin, 0.4f * pulse, SpriteEffects.None, 0f);

            // Layer 3: Second flare spinning counter-clockwise
            float hue3 = HueMin + 0.5f * (HueMax - HueMin);
            Color layer3Color = Main.hslToRgb(hue3, 0.85f, 0.68f) with { A = 0 };
            spriteBatch.Draw(flareTexture2, drawPos, null, layer3Color * 0.5f, -time * 0.75f, flareOrigin2, 0.35f * pulse, SpriteEffects.None, 0f);

            // Layer 4: Third flare different speed
            float hue4 = HueMin + 0.8f * (HueMax - HueMin);
            Color layer4Color = Main.hslToRgb(hue4, 0.9f, 0.75f) with { A = 0 };
            spriteBatch.Draw(flareTexture, drawPos, null, layer4Color * 0.58f, time * 1.35f, flareOrigin, 0.3f * pulse, SpriteEffects.None, 0f);

            // Layer 5: Main glow layer
            spriteBatch.Draw(flareTexture2, drawPos, null, primaryBloom * 0.6f, -time * 0.5f, flareOrigin2, 0.25f * shimmer, SpriteEffects.None, 0f);

            // Layer 6: Bright white-hot core
            spriteBatch.Draw(flareTexture, drawPos, null, whiteBloom * 0.72f, 0f, flareOrigin, 0.15f, SpriteEffects.None, 0f);

            // === MAIN ARC LAYERS ===
            // Massive outer ethereal glow
            spriteBatch.Draw(arcTexture, drawPos, null, secondaryBloom * 0.25f, Projectile.rotation, arcOrigin, 2.0f * pulse, SpriteEffects.None, 0f);
            
            // Main outer glow
            spriteBatch.Draw(arcTexture, drawPos, null, primaryBloom * 0.4f, Projectile.rotation, arcOrigin, 1.5f * pulse, SpriteEffects.None, 0f);
            
            // Vibrant middle layer
            spriteBatch.Draw(arcTexture, drawPos, null, primaryBloom * 0.55f, Projectile.rotation, arcOrigin, 1.0f, SpriteEffects.None, 0f);
            
            // Bright inner glow
            Color innerGlow = Color.Lerp(primaryBloom, whiteBloom, 0.5f);
            spriteBatch.Draw(arcTexture, drawPos, null, innerGlow * 0.7f, Projectile.rotation, arcOrigin, 0.65f * shimmer, SpriteEffects.None, 0f);
            
            // White-hot center core
            spriteBatch.Draw(arcTexture, drawPos, null, whiteBloom * 0.85f, Projectile.rotation, arcOrigin, 0.4f, SpriteEffects.None, 0f);
            
            // === 4 ORBITING SPARK POINTS ===
            float sparkOrbitAngle = time * 1.4f;
            for (int i = 0; i < 4; i++)
            {
                float sparkAngle = sparkOrbitAngle + MathHelper.TwoPi * i / 4f;
                Vector2 sparkPos = drawPos + sparkAngle.ToRotationVector2() * 30f;
                float sparkHue = HueMin + (i / 4f) * (HueMax - HueMin);
                Color sparkColor = Main.hslToRgb(sparkHue, 0.88f, 0.78f) with { A = 0 };
                spriteBatch.Draw(flareTexture, sparkPos, null, sparkColor * 0.5f, 0f, flareOrigin, 0.1f * pulse, SpriteEffects.None, 0f);
            }
            
            // === SPARKLE ACCENTS along the arc ===
            float sparklePhase = time * 2.8f;
            for (int i = 0; i < 3; i++)
            {
                float sparkleAngle = Projectile.rotation + MathHelper.ToRadians(-30 + i * 30);
                float sparkleOffset = 25f + i * 15f;
                Vector2 sparklePos = drawPos + sparkleAngle.ToRotationVector2() * sparkleOffset;
                float sparkleIntensity = 0.4f + (float)Math.Sin(sparklePhase + i * 1.2f) * 0.2f;
                spriteBatch.Draw(softGlow, sparklePos, null, whiteBloom * sparkleIntensity, 0f, glowOrigin, 0.22f * pulse, SpriteEffects.None, 0f);
            }
            
            // === CENTER GLOW for ambient bloom ===
            spriteBatch.Draw(softGlow, drawPos, null, primaryBloom * 0.45f, 0f, glowOrigin, 0.7f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(softGlow, drawPos, null, whiteBloom * 0.3f, 0f, glowOrigin, 0.35f, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

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
