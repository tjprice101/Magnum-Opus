using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.Seasons.Projectiles
{
    /// <summary>
    /// Vivaldi Seasonal Wave - Main projectile for Four Seasons Blade
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

        public override void SetStaticDefaults()
        {
            // ENHANCED: Longer trail for dramatic arc visibility
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            // ENHANCED: Larger hitbox for better collision
            Projectile.width = 70;
            Projectile.height = 70;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 5;
            Projectile.timeLeft = 90; // Longer lifespan for visibility
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 30;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 8; // Slightly faster hit rate
        }

        public override void AI()
        {
            // Get player for cursor-relative rotation
            Player owner = Main.player[Projectile.owner];
            
            // === ARCING SLASH ROTATION - Always angled forward toward cursor ===
            // Calculate angle from player toward mouse cursor
            Vector2 toCursor = Main.MouseWorld - owner.Center;
            float cursorAngle = toCursor.ToRotation();
            
            // Blend between velocity direction and cursor direction for natural arc feel
            float velocityAngle = Projectile.velocity.ToRotation();
            float blendFactor = 0.7f; // 70% toward cursor, 30% velocity
            Projectile.rotation = MathHelper.Lerp(velocityAngle, cursorAngle, blendFactor);

            // Slow down over time
            Projectile.velocity *= 0.97f;

            // Season-specific trail
            if (Main.rand.NextBool(2))
            {
                Vector2 trailPos = Projectile.Center + Main.rand.NextVector2Circular(20f, 20f);
                Vector2 trailVel = -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(2f, 2f);
                Color trailColor = Color.Lerp(PrimaryColor, SecondaryColor, Main.rand.NextFloat()) * 0.55f;
                var trail = new GenericGlowParticle(trailPos, trailVel, trailColor, 0.32f, 20, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }

            // ☁EMUSICAL NOTATION - Vivaldi's seasonal symphony (VISIBLE SCALE 0.78f+)
            if (Main.rand.NextBool(4))
            {
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-1.5f, -0.5f));
                ThemedParticles.MusicNote(Projectile.Center, noteVel, PrimaryColor * 0.9f, 0.78f, 38);
            }
            
            // ☁ESPARKLE ACCENT - Seasonal shimmer cascade
            if (Main.rand.NextBool(3))
            {
                var sparkle = new SparkleParticle(Projectile.Center + Main.rand.NextVector2Circular(15f, 15f), -Projectile.velocity * 0.08f, SecondaryColor, 0.32f, 20);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            // Periodic flares
            if (Projectile.timeLeft % 5 == 0)
            {
                CustomParticles.GenericFlare(Projectile.Center, PrimaryColor * 0.5f, 0.35f, 12);
            }

            Lighting.AddLight(Projectile.Center, PrimaryColor.ToVector3() * 0.6f);
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
                    // Small life steal
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

            // Impact VFX
            CustomParticles.GenericFlare(target.Center, PrimaryColor, 0.55f, 18);
            CustomParticles.HaloRing(target.Center, SecondaryColor * 0.5f, 0.4f, 15);

            for (int i = 0; i < 6; i++)
            {
                Vector2 burstVel = Main.rand.NextVector2Circular(5f, 5f);
                Color burstColor = Color.Lerp(PrimaryColor, SecondaryColor, Main.rand.NextFloat()) * 0.5f;
                var burst = new GenericGlowParticle(target.Center, burstVel, burstColor, 0.25f, 16, true);
                MagnumParticleHandler.SpawnParticle(burst);
            }

            // ☁EMUSICAL IMPACT - Conductor's symphony strike
            ThemedParticles.MusicNoteBurst(target.Center, PrimaryColor * 0.8f, 6, 4f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D texture = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SwordArc5").Value;
            Texture2D glowTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow2").Value;
            Texture2D flareTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/EnergyFlare3").Value;
            Vector2 origin = texture.Size() / 2f;
            Vector2 glowOrigin = glowTex.Size() / 2f;
            Vector2 flareOrigin = flareTex.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            
            // ENHANCED: Stronger pulsing animation for dramatic visibility
            float pulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.15f) * 0.2f;
            float shimmer = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.22f + Projectile.whoAmI) * 0.15f;
            
            // Color with alpha removed for proper additive blending (Fargos pattern)
            Color primaryBloom = PrimaryColor with { A = 0 };
            Color secondaryBloom = SecondaryColor with { A = 0 };
            Color whiteBloom = Color.White with { A = 0 };

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // === BRILLIANT TRAIL - Every position for maximum visibility ===
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                
                float progress = (float)i / Projectile.oldPos.Length;
                float fadeOut = 1f - progress;
                Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                
                // Gradient color through trail
                Color trailGradient = Color.Lerp(primaryBloom, secondaryBloom, progress);
                
                // ENHANCED: 3-layer trail with much larger scales for visibility
                float outerScale = 1.4f * fadeOut * pulse;
                spriteBatch.Draw(texture, trailPos, null, trailGradient * 0.35f * fadeOut, Projectile.oldRot[i], origin, outerScale, SpriteEffects.None, 0f);
                
                float midScale = 0.9f * fadeOut;
                spriteBatch.Draw(texture, trailPos, null, primaryBloom * 0.5f * fadeOut, Projectile.oldRot[i], origin, midScale, SpriteEffects.None, 0f);
                
                float innerScale = 0.55f * fadeOut;
                spriteBatch.Draw(texture, trailPos, null, whiteBloom * 0.6f * fadeOut, Projectile.oldRot[i], origin, innerScale, SpriteEffects.None, 0f);
            }

            // === MAIN PROJECTILE - DRAMATICALLY ENHANCED BLOOM LAYERS ===
            
            // Layer 1: Massive outer ethereal glow
            spriteBatch.Draw(texture, drawPos, null, secondaryBloom * 0.25f, Projectile.rotation, origin, 2.0f * pulse, SpriteEffects.None, 0f);
            
            // Layer 2: Main outer glow
            spriteBatch.Draw(texture, drawPos, null, primaryBloom * 0.4f, Projectile.rotation, origin, 1.5f * pulse, SpriteEffects.None, 0f);
            
            // Layer 3: Vibrant middle layer
            spriteBatch.Draw(texture, drawPos, null, primaryBloom * 0.55f, Projectile.rotation, origin, 1.0f, SpriteEffects.None, 0f);
            
            // Layer 4: Bright inner glow
            Color innerGlow = Color.Lerp(primaryBloom, whiteBloom, 0.5f);
            spriteBatch.Draw(texture, drawPos, null, innerGlow * 0.7f, Projectile.rotation, origin, 0.65f * shimmer, SpriteEffects.None, 0f);
            
            // Layer 5: White-hot center core
            spriteBatch.Draw(texture, drawPos, null, whiteBloom * 0.85f, Projectile.rotation, origin, 0.4f, SpriteEffects.None, 0f);
            
            // === ENERGY FLARE at center for brilliant shine ===
            float flareRotation = Main.GameUpdateCount * 0.08f;
            spriteBatch.Draw(flareTex, drawPos, null, whiteBloom * 0.6f, flareRotation, flareOrigin, 0.35f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(flareTex, drawPos, null, primaryBloom * 0.4f, -flareRotation, flareOrigin, 0.5f * pulse, SpriteEffects.None, 0f);
            
            // === SPARKLE ACCENTS along the arc ===
            float sparklePhase = Main.GameUpdateCount * 0.2f;
            for (int i = 0; i < 3; i++)
            {
                float sparkleAngle = Projectile.rotation + MathHelper.ToRadians(-30 + i * 30);
                float sparkleOffset = 25f + i * 15f;
                Vector2 sparklePos = drawPos + sparkleAngle.ToRotationVector2() * sparkleOffset;
                float sparkleIntensity = 0.4f + (float)Math.Sin(sparklePhase + i * 1.2f) * 0.2f;
                spriteBatch.Draw(glowTex, sparklePos, null, whiteBloom * sparkleIntensity, 0f, glowOrigin, 0.25f * pulse, SpriteEffects.None, 0f);
            }
            
            // === CENTER GLOW for ambient bloom ===
            spriteBatch.Draw(glowTex, drawPos, null, primaryBloom * 0.45f, 0f, glowOrigin, 0.8f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(glowTex, drawPos, null, whiteBloom * 0.3f, 0f, glowOrigin, 0.4f, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }

        public override void OnKill(int timeLeft)
        {
            // === CORE FLASH CASCADE ===
            CustomParticles.GenericFlare(Projectile.Center, Color.White, 0.8f, 20);
            CustomParticles.GenericFlare(Projectile.Center, PrimaryColor, 0.7f, 22);
            CustomParticles.GenericFlare(Projectile.Center, SecondaryColor, 0.55f, 18);
            
            // === GRADIENT HALO RINGS (reduced) ===
            for (int ring = 0; ring < 2; ring++)
            {
                Color ringColor = Color.Lerp(PrimaryColor, SecondaryColor, ring * 0.5f);
                float ringScale = 0.4f + ring * 0.15f;
                CustomParticles.HaloRing(Projectile.Center, ringColor * 0.55f, ringScale, 16);
            }

            // === RADIAL SPARK BURST (reduced) ===
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 burstVel = angle.ToRotationVector2() * Main.rand.NextFloat(5f, 8f);
                Color burstColor = Color.Lerp(PrimaryColor, SecondaryColor, (float)i / 6f) * 0.6f;
                var burst = new GenericGlowParticle(Projectile.Center, burstVel, burstColor, 0.3f, 18, true);
                MagnumParticleHandler.SpawnParticle(burst);
            }
            
            // === VANILLA DUST (reduced) ===
            for (int i = 0; i < 4; i++)
            {
                Vector2 dustVel = Main.rand.NextVector2Circular(5f, 5f);
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.MagicMirror, dustVel, 0, PrimaryColor, 0.9f);
                dust.noGravity = true;
            }

            // === MUSICAL FINALE - Four Seasons grand finale ===
            ThemedParticles.MusicNoteBurst(Projectile.Center, PrimaryColor * 0.8f, 6, 5f);
            ThemedParticles.MusicNoteRing(Projectile.Center, SecondaryColor * 0.7f, 50f, 6);
            
            // Dynamic lighting flash
            Lighting.AddLight(Projectile.Center, PrimaryColor.ToVector3() * 1.2f);
        }
    }
}
