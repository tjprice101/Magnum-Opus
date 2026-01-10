using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;
using MagnumOpus.Content.MoonlightSonata.ResonanceEnergies;
using MagnumOpus.Content.MoonlightSonata.CraftingStations;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems;

namespace MagnumOpus.Content.MoonlightSonata.ResonantWeapons
{
    /// <summary>
    /// Incisor of Moonlight - A powerful sword with ethereal moonlight effects.
    /// Features Calamity-inspired visual effects with purple/silver glowing aura.
    /// </summary>
    public class IncisorOfMoonlight : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            // Stronger than Zenith (190 damage)
            Item.damage = 280; // Balanced: Premium melee ~1400 DPS with projectile
            Item.DamageType = DamageClass.Melee;
            Item.width = 60;
            Item.height = 60;
            Item.useTime = 12; // Fast swing
            Item.useAnimation = 12;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 6.5f;
            Item.value = Item.sellPrice(gold: 25);
            Item.rare = ModContent.RarityType<EroicaRainbowRarity>();
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.useTurn = false;
            Item.shoot = ModContent.ProjectileType<MoonlightWaveProjectile>();
            Item.shootSpeed = 12f;
            Item.maxStack = 1;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<MoonlightsResonantEnergy>(), 20)
                .AddIngredient(ModContent.ItemType<ResonantCoreOfMoonlightSonata>(), 10)
                .AddIngredient(ModContent.ItemType<Enemies.ShardsOfMoonlitTempo>(), 25)
                .AddTile(ModContent.TileType<MoonlightAnvilTile>())
                .Register();
        }

        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            // Prismatic sparkle trail on swing - ethereal moonlight gems
            if (Main.rand.NextBool(3))
            {
                Vector2 sparklePos = new Vector2(hitbox.X + Main.rand.Next(hitbox.Width), hitbox.Y + Main.rand.Next(hitbox.Height));
                CustomParticles.PrismaticSparkle(sparklePos, CustomParticleSystem.MoonlightColors.Random(), 0.25f);
            }
            
            // Main purple dust with reduced frequency for cleaner look
            if (Main.rand.NextBool(3))
            {
                Dust dust = Dust.NewDustDirect(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height, 
                    DustID.PurpleTorch, player.velocity.X * 0.2f, player.velocity.Y * 0.2f, 150, default, 1.1f);
                dust.noGravity = true;
                dust.velocity *= 1.2f;
            }

            // Crystal accents - less frequent, more impactful
            if (Main.rand.NextBool(5))
            {
                Dust dust2 = Dust.NewDustDirect(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height, 
                    DustID.PurpleCrystalShard, 0f, 0f, 100, default, 0.8f);
                dust2.noGravity = true;
                dust2.velocity = Main.rand.NextVector2Circular(2f, 2f);
            }
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Spawn 2 moon-themed projectiles in a 40 degree cone
            float coneAngle = MathHelper.ToRadians(40f);
            float halfCone = coneAngle / 2f;
            
            // Calculate projectile damage (split between two)
            int projDamage = (int)(damage * 0.65f);
            
            // Left projectile (-20 degrees from center)
            Vector2 leftVelocity = velocity.RotatedBy(-halfCone);
            Projectile.NewProjectile(source, position, leftVelocity, type, projDamage, knockback, player.whoAmI);
            
            // Right projectile (+20 degrees from center)
            Vector2 rightVelocity = velocity.RotatedBy(halfCone);
            Projectile.NewProjectile(source, position, rightVelocity, type, projDamage, knockback, player.whoAmI);
            
            // Elegant moonlight crescent slash between the projectiles
            CustomParticles.SwordArcCrescent(position, velocity * 0.6f, CustomParticleSystem.MoonlightColors.Lavender, 0.55f);
            
            // Moonlight flare at origin - ethereal flash
            CustomParticles.MoonlightFlare(position, 0.6f);
            
            // Generic flare burst for dramatic flair
            CustomParticles.GenericFlare(position, new Color(180, 140, 255), 0.45f, 18);
            
            // Prismatic sparkle accents radiating outward
            for (int i = 0; i < 4; i++)
            {
                Vector2 sparkleOffset = Main.rand.NextVector2Circular(20f, 20f);
                CustomParticles.PrismaticSparkle(position + sparkleOffset, CustomParticleSystem.MoonlightColors.Random(), 0.25f);
            }
            
            // Magic sparkle field rising effect - channeled energy
            CustomParticles.MagicSparkleFieldRising(position, CustomParticleSystem.MoonlightColors.Violet, 3);
            
            // Musical notes floating from swing
            CustomParticles.MoonlightMusicNotes(position, 3, 30f);
            
            // Themed moonlight sparks shooting forward
            ThemedParticles.MoonlightSparks(position, velocity, 6, 5f);
            
            // Light purple dust accent burst
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.Lerp(-halfCone, halfCone, i / 7f);
                Vector2 dustVel = velocity.RotatedBy(angle) * Main.rand.NextFloat(0.15f, 0.35f);
                Dust dust = Dust.NewDustDirect(position, 1, 1, DustID.PurpleTorch, dustVel.X, dustVel.Y, 80, default, 1.2f);
                dust.noGravity = true;
                dust.fadeIn = 0.8f;
            }
            
            // Silver shimmer dust in cone
            for (int i = 0; i < 4; i++)
            {
                Vector2 dustVel = velocity.RotatedByRandom(halfCone) * Main.rand.NextFloat(0.2f, 0.4f);
                Dust shimmer = Dust.NewDustDirect(position, 1, 1, DustID.SilverCoin, dustVel.X, dustVel.Y, 100, default, 0.8f);
                shimmer.noGravity = true;
            }

            return false; // We already created the projectiles
        }

        public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Moonlight themed impact - clean prismatic burst
            ThemedParticles.MoonlightImpact(target.Center, 0.7f);
            
            // Prismatic sparkle burst on hit - gem-like impact
            CustomParticles.PrismaticSparkleBurst(target.Center, CustomParticleSystem.MoonlightColors.Violet, 6);
            
            // Single elegant flare
            CustomParticles.MoonlightFlare(target.Center, 0.5f);
            
            // Light dust accent
            for (int i = 0; i < 8; i++)
            {
                Dust dust = Dust.NewDustDirect(target.position, target.width, target.height, 
                    DustID.PurpleTorch, 0f, 0f, 150, default, 1.2f);
                dust.noGravity = true;
                dust.velocity = Main.rand.NextVector2Circular(4f, 4f);
            }
        }

        public override void HoldItem(Player player)
        {
            // Magic sparkle field ambient aura - subtle enchantment glow
            if (Main.rand.NextBool(12))
            {
                Vector2 offset = Main.rand.NextVector2Circular(30f, 30f);
                CustomParticles.MagicSparkleFieldAura(player.Center + offset, CustomParticleSystem.MoonlightColors.Silver * 0.6f, 0.3f, 30);
            }
            
            // Occasional prismatic twinkle
            if (Main.rand.NextBool(15))
            {
                Vector2 offset = Main.rand.NextVector2Circular(25f, 25f);
                CustomParticles.PrismaticSparkle(player.Center + offset, CustomParticleSystem.MoonlightColors.Lavender, 0.2f);
            }
            
            // Soft purple lighting
            Lighting.AddLight(player.Center, 0.25f, 0.18f, 0.45f);
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            // Draw glowing backlight effect when dropped in world
            Texture2D texture = TextureAssets.Item[Item.type].Value;
            Vector2 position = Item.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;
            
            // Calculate pulse - slow and ethereal for moonlight theme
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.04f) * 0.12f + 1f;
            
            // Begin additive blending for glow
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Outer deep purple aura
            spriteBatch.Draw(texture, position, null, new Color(75, 0, 130) * 0.45f, rotation, origin, scale * pulse * 1.35f, SpriteEffects.None, 0f);
            
            // Middle blue-purple glow
            spriteBatch.Draw(texture, position, null, new Color(138, 43, 226) * 0.35f, rotation, origin, scale * pulse * 1.2f, SpriteEffects.None, 0f);
            
            // Inner silver/lavender glow
            spriteBatch.Draw(texture, position, null, new Color(200, 180, 255) * 0.25f, rotation, origin, scale * pulse * 1.08f, SpriteEffects.None, 0f);
            
            // Return to normal blending
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Add lighting
            Lighting.AddLight(Item.Center, 0.4f, 0.3f, 0.7f);
            
            return true; // Draw the normal sprite too
        }
    }
}
