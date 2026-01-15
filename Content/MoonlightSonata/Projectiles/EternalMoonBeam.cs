using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;
using MagnumOpus.Content.MoonlightSonata.Debuffs;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using System;

namespace MagnumOpus.Content.MoonlightSonata.Projectiles
{
    /// <summary>
    /// A spinning mana crystal star projectile fired by the Eternal Moon sword.
    /// Uses the Mana Crystal texture with purple/pink hue, spinning as it flies.
    /// </summary>
    public class EternalMoonBeam : ModProjectile
    {
        // Use the Mana Crystal item texture
        public override string Texture => "Terraria/Images/Item_" + ItemID.ManaCrystal;

        // Track spin rotation separately from movement rotation
        private float SpinRotation
        {
            get => Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 12;
            ProjectileID.Sets.TrailingMode[Type] = 2; // Store rotation too
        }

        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 3; // Can hit 3 enemies
            Projectile.timeLeft = 120; // 2 seconds of flight
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.alpha = 0;
            Projectile.light = 0.6f;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
            Projectile.scale = 1.2f;
        }

        public override void AI()
        {
            // Spin the star
            SpinRotation += 0.25f;
            Projectile.rotation = SpinRotation;

            // Add purple/pink lighting
            Lighting.AddLight(Projectile.Center, 0.6f, 0.2f, 0.8f);

            // Slight homing toward nearby enemies
            float homingRange = 300f;
            float homingStrength = 0.03f;
            NPC closestNPC = null;
            float closestDist = homingRange;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && npc.CanBeChasedBy())
                {
                    float dist = Vector2.Distance(Projectile.Center, npc.Center);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closestNPC = npc;
                    }
                }
            }

            if (closestNPC != null)
            {
                Vector2 toTarget = closestNPC.Center - Projectile.Center;
                toTarget.Normalize();
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * Projectile.velocity.Length(), homingStrength);
            }

            // Enhanced sparkle trail using ThemedParticles
            ThemedParticles.MoonlightTrail(Projectile.Center, Projectile.velocity);
            
            // === CALAMITY-INSPIRED SPINNING STAR TRAIL ===
            // Multi-layer glow particles with gradient
            for (int i = 0; i < 2; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(10f, 10f);
                float progress = Main.rand.NextFloat();
                Color trailColor = Color.Lerp(UnifiedVFX.MoonlightSonata.DarkPurple, UnifiedVFX.MoonlightSonata.LightBlue, progress);
                
                var glow = new GenericGlowParticle(Projectile.Center + offset, -Projectile.velocity * 0.12f,
                    trailColor, 0.25f + progress * 0.15f, 16, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            // Orbiting star points that spin with the projectile
            if (Main.rand.NextBool(2))
            {
                for (int i = 0; i < 4; i++)
                {
                    float starAngle = SpinRotation + MathHelper.TwoPi * i / 4f;
                    Vector2 starPoint = Projectile.Center + starAngle.ToRotationVector2() * 14f;
                    float starProgress = (float)i / 4f;
                    Color starColor = Color.Lerp(UnifiedVFX.MoonlightSonata.MediumPurple, UnifiedVFX.MoonlightSonata.Silver, starProgress);
                    CustomParticles.GenericFlare(starPoint, starColor, 0.2f, 10);
                }
            }
            
            // Music notes shedding from the spinning star
            if (Main.rand.NextBool(6))
            {
                Color noteColor = Color.Lerp(UnifiedVFX.MoonlightSonata.MediumPurple, UnifiedVFX.MoonlightSonata.Silver, Main.rand.NextFloat());
                ThemedParticles.MusicNote(Projectile.Center, -Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(1f, 1f), noteColor, 0.22f, 28);
            }
            
            // Prismatic sparkle dust - floating gem particles
            if (Main.rand.NextBool(4))
            {
                Vector2 offset = Main.rand.NextVector2Circular(12f, 12f);
                CustomParticles.PrismaticSparkle(Projectile.Center + offset, UnifiedVFX.MoonlightSonata.Silver, 0.18f);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 origin = texture.Size() / 2f;
            
            // Switch to additive blending for vibrant glow
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, 
                SamplerState.LinearClamp, DepthStencilState.None, 
                RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // Draw glowing trail
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                
                float trailProgress = (float)i / Projectile.oldPos.Length;
                float trailAlpha = (1f - trailProgress) * 0.6f;
                float trailScale = Projectile.scale * (1f - trailProgress * 0.5f);
                
                // Purple/pink gradient for trail
                Color trailColor = Color.Lerp(new Color(255, 100, 200), new Color(150, 50, 255), trailProgress);
                trailColor *= trailAlpha;
                
                Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                float trailRot = Projectile.oldRot[i];
                
                spriteBatch.Draw(texture, trailPos, null, trailColor, trailRot, origin, trailScale * 0.8f, SpriteEffects.None, 0f);
                
                // White sparkle highlight on recent trail
                if (i < 4)
                {
                    Color whiteGlow = Color.White * trailAlpha * 0.6f;
                    spriteBatch.Draw(texture, trailPos, null, whiteGlow, trailRot, origin, trailScale * 0.4f, SpriteEffects.None, 0f);
                }
            }

            // Draw outer glow (pink) - ENHANCED
            Color glowPink = new Color(255, 150, 220, 0) * 0.8f;
            spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, glowPink, 
                Projectile.rotation, origin, Projectile.scale * 1.7f, SpriteEffects.None, 0f);
            
            // NEW: Extra outer ethereal halo
            Color haloColor = new Color(200, 100, 255, 0) * 0.4f;
            spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, haloColor, 
                Projectile.rotation, origin, Projectile.scale * 2.0f, SpriteEffects.None, 0f);

            // Draw middle glow (purple) - ENHANCED
            Color glowPurple = new Color(180, 80, 255, 0) * 0.9f;
            spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, glowPurple, 
                Projectile.rotation, origin, Projectile.scale * 1.35f, SpriteEffects.None, 0f);
            
            // NEW: Bright inner magenta accent
            Color innerMagenta = new Color(255, 100, 200, 0) * 0.7f;
            spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, innerMagenta, 
                Projectile.rotation, origin, Projectile.scale * 1.15f, SpriteEffects.None, 0f);
                
            // Reset to normal blending
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                SamplerState.LinearClamp, DepthStencilState.None,
                RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // Draw main sprite with purple/pink tint
            Color mainColor = new Color(220, 140, 255, 200);
            spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, mainColor, 
                Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0f);

            // Draw bright white core
            Color coreColor = Color.White * 0.8f;
            spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, coreColor, 
                Projectile.rotation, origin, Projectile.scale * 0.5f, SpriteEffects.None, 0f);

            return false; // We handled drawing
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<MusicsDissonance>(), 300); // 5 seconds
            
            // === CALAMITY-INSPIRED IMPACT ===
            // Central flash
            CustomParticles.GenericFlare(target.Center, Color.White, 0.7f, 20);
            CustomParticles.GenericFlare(target.Center, UnifiedVFX.MoonlightSonata.LightBlue, 0.55f, 18);
            
            // UnifiedVFX impact
            UnifiedVFX.MoonlightSonata.Impact(target.Center, 0.7f);
            
            // Fractal flare burst - 8-point star matching the spinning star theme
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 flareOffset = angle.ToRotationVector2() * 32f;
                float progress = (float)i / 8f;
                Color fractalColor = Color.Lerp(UnifiedVFX.MoonlightSonata.DarkPurple, UnifiedVFX.MoonlightSonata.LightBlue, progress);
                CustomParticles.GenericFlare(target.Center + flareOffset, fractalColor, 0.5f, 18);
            }
            
            // Gradient halo rings
            for (int ring = 0; ring < 4; ring++)
            {
                float ringProgress = (float)ring / 4f;
                Color ringColor = Color.Lerp(UnifiedVFX.MoonlightSonata.DarkPurple, UnifiedVFX.MoonlightSonata.Silver, ringProgress);
                CustomParticles.HaloRing(target.Center, ringColor, 0.3f + ring * 0.12f, 14 + ring * 4);
            }
            
            // Spark spray
            for (int i = 0; i < 10; i++)
            {
                float angle = MathHelper.TwoPi * i / 10f + Main.rand.NextFloat(-0.25f, 0.25f);
                Vector2 sparkVel = angle.ToRotationVector2() * Main.rand.NextFloat(5f, 10f);
                float progress = (float)i / 10f;
                Color sparkColor = Color.Lerp(UnifiedVFX.MoonlightSonata.MediumPurple, UnifiedVFX.MoonlightSonata.Silver, progress);
                
                var spark = new GenericGlowParticle(target.Center, sparkVel, sparkColor, 0.35f, 20, true);
                MagnumParticleHandler.SpawnParticle(spark);
            }
            
            // Music notes on hit
            ThemedParticles.MoonlightMusicNotes(target.Center, 5, 30f);
        }

        public override void OnKill(int timeLeft)
        {
            // === CALAMITY-INSPIRED DEATH EXPLOSION ===
            // Central flash
            CustomParticles.GenericFlare(Projectile.Center, Color.White * 0.9f, 0.65f, 20);
            CustomParticles.GenericFlare(Projectile.Center, UnifiedVFX.MoonlightSonata.LightBlue, 0.55f, 18);
            
            // Themed bloom burst
            ThemedParticles.MoonlightBloomBurst(Projectile.Center, 0.7f);
            
            // Fractal burst
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 flareOffset = angle.ToRotationVector2() * 25f;
                float progress = (float)i / 6f;
                Color fractalColor = Color.Lerp(UnifiedVFX.MoonlightSonata.DarkPurple, UnifiedVFX.MoonlightSonata.LightBlue, progress);
                CustomParticles.GenericFlare(Projectile.Center + flareOffset, fractalColor, 0.4f, 16);
            }
            
            // Gradient halo rings
            for (int ring = 0; ring < 3; ring++)
            {
                float ringProgress = (float)ring / 3f;
                Color ringColor = Color.Lerp(UnifiedVFX.MoonlightSonata.DarkPurple, UnifiedVFX.MoonlightSonata.LightBlue, ringProgress);
                CustomParticles.HaloRing(Projectile.Center, ringColor, 0.25f + ring * 0.1f, 12 + ring * 3);
            }
            
            // Death spark spray
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                Vector2 velocity = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f);
                float progress = (float)i / 12f;
                Color deathColor = Color.Lerp(UnifiedVFX.MoonlightSonata.MediumPurple, UnifiedVFX.MoonlightSonata.Silver, progress);
                
                var deathSpark = new GenericGlowParticle(Projectile.Center, velocity, deathColor, 0.3f, 18, true);
                MagnumParticleHandler.SpawnParticle(deathSpark);
            }
            
            // Music notes burst
            ThemedParticles.MoonlightMusicNotes(Projectile.Center, 4, 25f);
        }
    }
}
