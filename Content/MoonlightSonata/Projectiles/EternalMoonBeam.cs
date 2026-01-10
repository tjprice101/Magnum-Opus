using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;
using MagnumOpus.Content.MoonlightSonata.Debuffs;
using MagnumOpus.Common.Systems;
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
            
            // Custom particle trail effect
            CustomParticles.MoonlightTrail(Projectile.Center, Projectile.velocity, 0.2f);
            
            // Ambient prismatic sparkle dust - floating gem particles
            if (Main.rand.NextBool(5))
            {
                Vector2 offset = Main.rand.NextVector2Circular(12f, 12f);
                CustomParticles.PrismaticSparkleAmbient(Projectile.Center + offset, CustomParticleSystem.MoonlightColors.Lavender, 8f, 2);
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
            
            // Magic sparkle field aura burst on hit - enchantment impact
            CustomParticles.MagicSparkleFieldBurst(target.Center, CustomParticleSystem.MoonlightColors.Violet, 4, 20f);
            
            // Prismatic impact sparkles
            CustomParticles.PrismaticSparkleBurst(target.Center, new Color(220, 140, 255), 5);
            
            // Hit burst particles - purple/pink explosion (reduced)
            for (int i = 0; i < 10; i++)
            {
                int dustType = Main.rand.NextBool() ? DustID.PurpleCrystalShard : DustID.Enchanted_Pink;
                Dust dust = Dust.NewDustDirect(target.Center, 1, 1, dustType,
                    Main.rand.NextFloat(-5f, 5f), Main.rand.NextFloat(-5f, 5f), 100, default, 1.2f);
                dust.noGravity = true;
            }
        }

        public override void OnKill(int timeLeft)
        {
            // Sword arc vortex on death - spinning star explosion
            CustomParticles.SwordArcVortex(Projectile.Center, CustomParticleSystem.MoonlightColors.Violet, 3, 0.35f);
            
            // Rising magic sparkle field - ethereal dissipation
            CustomParticles.MagicSparkleFieldRising(Projectile.Center, CustomParticleSystem.MoonlightColors.Silver, 5);
            
            // Themed bloom burst
            ThemedParticles.MoonlightBloomBurst(Projectile.Center, 0.6f);
            
            // Death burst - magical star explosion (reduced)
            for (int i = 0; i < 16; i++)
            {
                float angle = MathHelper.TwoPi * i / 16f;
                Vector2 velocity = angle.ToRotationVector2() * Main.rand.NextFloat(2.5f, 5f);
                
                int dustType = Main.rand.NextBool() ? DustID.PurpleCrystalShard : DustID.PinkFairy;
                Dust dust = Dust.NewDustDirect(Projectile.Center, 1, 1, dustType,
                    velocity.X, velocity.Y, 100, default, 1.1f);
                dust.noGravity = true;
            }
        }
    }
}
