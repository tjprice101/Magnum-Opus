using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;
using MagnumOpus.Content.MoonlightSonata.Debuffs;
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

            // Sparkle trail particles
            if (Main.rand.NextBool(2))
            {
                int dustType = Main.rand.NextBool() ? DustID.PurpleCrystalShard : DustID.Enchanted_Pink;
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, 
                    dustType, 0f, 0f, 100, default, 0.8f);
                dust.noGravity = true;
                dust.velocity = -Projectile.velocity * 0.2f + Main.rand.NextVector2Circular(1f, 1f);
            }

            // Magical shimmer particles
            if (Main.rand.NextBool(4))
            {
                Dust shimmer = Dust.NewDustDirect(Projectile.Center + Main.rand.NextVector2Circular(12f, 12f),
                    1, 1, DustID.PinkFairy, 0f, 0f, 150, default, 0.6f);
                shimmer.noGravity = true;
                shimmer.velocity = Main.rand.NextVector2Circular(0.5f, 0.5f);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 origin = texture.Size() / 2f;

            // Draw glowing trail
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                
                float trailProgress = (float)i / Projectile.oldPos.Length;
                float trailAlpha = (1f - trailProgress) * 0.5f;
                float trailScale = Projectile.scale * (1f - trailProgress * 0.5f);
                
                // Purple/pink gradient for trail
                Color trailColor = Color.Lerp(new Color(255, 100, 200), new Color(150, 50, 255), trailProgress);
                trailColor *= trailAlpha;
                
                Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                float trailRot = Projectile.oldRot[i];
                
                spriteBatch.Draw(texture, trailPos, null, trailColor, trailRot, origin, trailScale * 0.8f, SpriteEffects.None, 0f);
            }

            // Draw outer glow (pink)
            Color glowPink = new Color(255, 150, 220, 0) * 0.6f;
            spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, glowPink, 
                Projectile.rotation, origin, Projectile.scale * 1.4f, SpriteEffects.None, 0f);

            // Draw middle glow (purple)
            Color glowPurple = new Color(180, 80, 255, 0) * 0.7f;
            spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, glowPurple, 
                Projectile.rotation, origin, Projectile.scale * 1.2f, SpriteEffects.None, 0f);

            // Draw main sprite with purple/pink tint
            Color mainColor = new Color(220, 140, 255, 200);
            spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, mainColor, 
                Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0f);

            // Draw bright core
            Color coreColor = new Color(255, 200, 255, 100);
            spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, coreColor, 
                Projectile.rotation, origin, Projectile.scale * 0.6f, SpriteEffects.None, 0f);

            return false; // We handled drawing
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<MusicsDissonance>(), 300); // 5 seconds
            
            // Hit burst particles - purple/pink explosion
            for (int i = 0; i < 10; i++)
            {
                int dustType = Main.rand.NextBool() ? DustID.PurpleCrystalShard : DustID.Enchanted_Pink;
                Dust dust = Dust.NewDustDirect(target.Center, 1, 1, dustType,
                    Main.rand.NextFloat(-5f, 5f), Main.rand.NextFloat(-5f, 5f), 100, default, 1.3f);
                dust.noGravity = true;
            }
        }

        public override void OnKill(int timeLeft)
        {
            // Death burst - magical star explosion
            for (int i = 0; i < 20; i++)
            {
                float angle = MathHelper.TwoPi * i / 20f;
                Vector2 velocity = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 5f);
                
                int dustType = Main.rand.NextBool() ? DustID.PurpleCrystalShard : DustID.PinkFairy;
                Dust dust = Dust.NewDustDirect(Projectile.Center, 1, 1, dustType,
                    velocity.X, velocity.Y, 100, default, 1.2f);
                dust.noGravity = true;
            }

            // Central sparkle burst
            for (int i = 0; i < 8; i++)
            {
                Dust sparkle = Dust.NewDustDirect(Projectile.Center, 1, 1, DustID.Enchanted_Pink,
                    Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-3f, 3f), 0, default, 1f);
                sparkle.noGravity = true;
            }
        }
    }
}
