using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.MoonlightSonata.Debuffs;

namespace MagnumOpus.Content.MoonlightSonata.ResonantWeapons
{
    public class MoonlightWaveProjectile : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 1;
        }

        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 5; // Can hit 5 enemies
            Projectile.timeLeft = 60; // Lasts 1 second
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 1;
            Projectile.alpha = 50;
            Projectile.scale = 1f;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
            // Grow slightly over time for wave effect
            Projectile.scale += 0.02f;
            Projectile.alpha += 3;

            if (Projectile.alpha > 255)
            {
                Projectile.Kill();
                return;
            }

            // Set rotation to match velocity direction (no spinning)
            Projectile.rotation = Projectile.velocity.ToRotation();

            // Purple sparkly flaming effect
            // Main flame particles
            for (int i = 0; i < 2; i++)
            {
                Dust flame = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, 
                    DustID.PurpleTorch, 0f, 0f, 100, default, 1.4f);
                flame.noGravity = true;
                flame.velocity = Projectile.velocity * 0.05f + Main.rand.NextVector2Circular(2f, 2f);
                flame.fadeIn = 1.2f;
            }

            // Sparkly particles
            if (Main.rand.NextBool(2))
            {
                Dust sparkle = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, 
                    DustID.Enchanted_Pink, 0f, 0f, 0, default, 1.0f);
                sparkle.noGravity = true;
                sparkle.velocity = Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(1.5f, 1.5f);
            }

            // Purple crystal shards for extra sparkle
            if (Main.rand.NextBool(3))
            {
                Dust crystal = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, 
                    DustID.PurpleCrystalShard, 0f, 0f, 100, default, 0.9f);
                crystal.noGravity = true;
                crystal.velocity = Main.rand.NextVector2Circular(2f, 2f);
            }

            // Trailing flame effect
            Dust trail = Dust.NewDustDirect(Projectile.Center - Projectile.velocity * 0.5f, 1, 1, 
                DustID.PurpleTorch, 0f, 0f, 150, default, 1.2f);
            trail.noGravity = true;
            trail.velocity *= 0.1f;

            // Light emission - brighter purple glow
            Lighting.AddLight(Projectile.Center, 0.6f, 0.2f, 0.8f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Apply Music's Dissonance debuff for 5 seconds
            target.AddBuff(ModContent.BuffType<MusicsDissonance>(), 300);

            // Burst of musical energy on hit with flaming effect
            for (int i = 0; i < 12; i++)
            {
                Dust dust = Dust.NewDustDirect(target.Center, 1, 1, DustID.PurpleTorch, 0f, 0f, 100, default, 1.5f);
                dust.noGravity = true;
                dust.velocity = Main.rand.NextVector2Circular(6f, 6f);
                dust.fadeIn = 1.3f;
            }

            // Extra sparkles on hit
            for (int i = 0; i < 6; i++)
            {
                Dust sparkle = Dust.NewDustDirect(target.Center, 1, 1, DustID.Enchanted_Pink, 0f, 0f, 0, default, 0.9f);
                sparkle.noGravity = true;
                sparkle.velocity = Main.rand.NextVector2Circular(4f, 4f);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // Custom drawing for wave effect
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 drawOrigin = new Vector2(texture.Width / 2, texture.Height / 2);
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            // Calculate fade based on alpha
            float fadeAlpha = 1f - (Projectile.alpha / 255f);
            Color drawColor = new Color(180, 100, 255) * fadeAlpha;

            // Draw multiple layers for wave effect
            for (int i = 0; i < 3; i++)
            {
                float layerScale = Projectile.scale * (1f + i * 0.2f);
                float layerAlpha = fadeAlpha * (1f - i * 0.3f);
                Color layerColor = drawColor * layerAlpha;

                Main.EntitySpriteDraw(texture, drawPos, null, layerColor, 
                    Projectile.rotation + i * 0.5f, drawOrigin, layerScale, SpriteEffects.None, 0);
            }

            return false;
        }

        public override void OnKill(int timeLeft)
        {
            // Final burst of particles
            for (int i = 0; i < 10; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, 
                    DustID.PurpleTorch, 0f, 0f, 100, default, 0.8f);
                dust.noGravity = true;
                dust.velocity = Main.rand.NextVector2Circular(3f, 3f);
            }
        }

        public override Color? GetAlpha(Color lightColor)
        {
            // Always visible with purple tint
            float alpha = 1f - (Projectile.alpha / 255f);
            return new Color(180, 100, 255) * alpha;
        }
    }
}
