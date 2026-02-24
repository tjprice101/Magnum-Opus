using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;
using MagnumOpus.Content.Eroica.ResonanceEnergies;
using MagnumOpus.Content.Eroica.Projectiles;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.Eroica.ResonantWeapons
{
    /// <summary>
    /// Triumphant Fractal - Magic staff that fires three fractal projectiles with massive explosions.
    /// Rainbow rarity, higher tier than Moonlight weapons.
    /// Features Calamity-inspired visual effects with chromatic aberration and pulsing glow.
    /// </summary>
    public class TriumphantFractal : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
            Item.staff[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.damage = 518; // Buffed: ~1244 DPS (518 ÁE60/25), 15% increase
            Item.DamageType = DamageClass.Magic;
            Item.width = 56;
            Item.height = 56;
            Item.useTime = 25;
            Item.useAnimation = 25;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 8f;
            Item.value = Item.sellPrice(gold: 45);
            Item.rare = ModContent.RarityType<EroicaRainbowRarity>();
            Item.UseSound = SoundID.Item43;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<TriumphantFractalProjectile>();
            Item.shootSpeed = 14f;
            Item.mana = 19; // Reduced from 20 (5% reduction)
            Item.noMelee = true;            Item.maxStack = 1;        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Fire 3 fractals in a tight spread
            int numberOfProjectiles = 3;
            float spreadAngle = MathHelper.ToRadians(15);

            for (int i = 0; i < numberOfProjectiles; i++)
            {
                float angle = spreadAngle * ((float)i / (numberOfProjectiles - 1) - 0.5f);
                Vector2 perturbedVelocity = velocity.RotatedBy(angle);

                Projectile.NewProjectile(source, position, perturbedVelocity, type, (int)(damage * 1.15f), knockback, player.whoAmI);
            }

            // === ELEGANT FRACTAL GEOMETRY - Suggested, not overwhelming ===
            
            // Primary hexagonal points (6 points, reduced scale)
            float baseRotation = Main.GameUpdateCount * 0.02f;
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f + baseRotation;
                Vector2 point = position + angle.ToRotationVector2() * 40f;
                float progress = (float)i / 6f;
                Color pointColor = Color.Lerp(UnifiedVFX.Eroica.Gold, UnifiedVFX.Eroica.Scarlet, progress);
                CustomParticles.GenericFlare(point, pointColor, 0.45f, 18);
            }
            
            // Inner triangular pattern (sacred geometry core)
            for (int i = 0; i < 3; i++)
            {
                float triAngle = MathHelper.TwoPi * i / 3f + baseRotation + MathHelper.Pi / 6f;
                Vector2 triPoint = position + triAngle.ToRotationVector2() * 22f;
                CustomParticles.GenericFlare(triPoint, Color.White * 0.9f, 0.4f, 15);
            }
            
            // Central burst - the triumphant core
            CustomParticles.GenericFlare(position, Color.White, 0.8f, 22);
            CustomParticles.HaloRing(position, UnifiedVFX.Eroica.Gold, 0.5f, 18);
            
            // Sakura petals
            ThemedParticles.SakuraPetals(position, 4, 45f);
            
            // Central music burst
            ThemedParticles.MusicNoteBurst(position, UnifiedVFX.Eroica.Gold, 4, 3f);

            return false;
        }

        public override void HoldItem(Player player)
        {
            // === SUBTLE FRACTAL MANDALA ===
            float time = Main.GameUpdateCount * 0.03f;
            
            // Outer ring - 4 points rotating (reduced from 8)
            if (Main.rand.NextBool(12))
            {
                for (int i = 0; i < 4; i++)
                {
                    float angle = MathHelper.TwoPi * i / 4f + time;
                    float radius = 38f + (float)Math.Sin(time * 2f + i) * 6f;
                    Vector2 pos = player.Center + angle.ToRotationVector2() * radius;
                    Color color = Color.Lerp(UnifiedVFX.Eroica.Gold, UnifiedVFX.Eroica.Scarlet, (float)i / 4f);
                    CustomParticles.GenericFlare(pos, color, 0.22f, 10);
                }
            }
            
            // Inner triangle (sacred geometry) - kept but less frequent
            if (Main.rand.NextBool(10))
            {
                for (int i = 0; i < 3; i++)
                {
                    float angle = MathHelper.TwoPi * i / 3f + time * 1.5f;
                    Vector2 pos = player.Center + angle.ToRotationVector2() * 15f;
                    CustomParticles.GenericFlare(pos, Color.White * 0.6f, 0.15f, 8);
                }
            }
            
            // Occasional fractal branch
            if (Main.rand.NextBool(15))
            {
                float branchAngle = Main.rand.NextFloat() * MathHelper.TwoPi;
                Vector2 branchEnd = player.Center + branchAngle.ToRotationVector2() * 28f;
                CustomParticles.GenericFlare(branchEnd, UnifiedVFX.Eroica.Gold * 0.7f, 0.18f, 10);
            }
            
            // Sakura petals drifting
            if (Main.rand.NextBool(20))
                ThemedParticles.SakuraPetals(player.Center, 1, 40f);
            
            // Golden sparkle
            if (Main.rand.NextBool(12))
                CustomParticles.PrismaticSparkle(player.Center + Main.rand.NextVector2Circular(20f, 20f), UnifiedVFX.Eroica.Gold, 0.22f);
            
            // Lighting aura with pulse
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.08f) * 0.1f + 0.9f;
            Lighting.AddLight(player.Center, 0.6f * pulse, 0.3f * pulse, 0.2f * pulse);
        }
        
        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            // Draw glowing backlight effect when dropped in world
            Texture2D texture = TextureAssets.Item[Item.type].Value;
            Vector2 position = Item.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;
            
            // Calculate pulse - faster for magic staff
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.08f) * 0.2f + 1f;
            
            // Begin additive blending for glow
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Outer crimson/scarlet glow
            spriteBatch.Draw(texture, position, null, new Color(200, 50, 50) * 0.5f, rotation, origin, scale * pulse * 1.4f, SpriteEffects.None, 0f);
            
            // Middle orange glow
            spriteBatch.Draw(texture, position, null, new Color(255, 120, 60) * 0.4f, rotation, origin, scale * pulse * 1.2f, SpriteEffects.None, 0f);
            
            // Inner golden-white core
            spriteBatch.Draw(texture, position, null, new Color(255, 220, 150) * 0.3f, rotation, origin, scale * pulse * 1.05f, SpriteEffects.None, 0f);
            
            // Return to normal blending
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Add lighting
            Lighting.AddLight(Item.Center, 1f, 0.5f, 0.3f);
            
            return true; // Draw the normal sprite too
        }

        public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Fires three fractal projectiles in a spread"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Projectiles explode with recursive fractal geometry"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Victory branches infinitely'") { OverrideColor = new Color(200, 50, 50) });
        }
        
        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            // Subtle golden pulse effect in inventory
            Texture2D texture = TextureAssets.Item[Item.type].Value;
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.06f) * 0.1f + 1f;
            
            // Draw golden glow behind
            spriteBatch.Draw(texture, position, frame, UnifiedVFX.Eroica.Gold * 0.2f, 0f, origin, scale * pulse * 1.08f, SpriteEffects.None, 0f);
            
            // Draw main item with pulse
            spriteBatch.Draw(texture, position, frame, drawColor, 0f, origin, scale * pulse, SpriteEffects.None, 0f);
            
            return false;
        }

        // Recipe removed - drops from Eroica, God of Valor
        // public override void AddRecipes()
        // {
        //     CreateRecipe()
        //         .AddIngredient(ModContent.ItemType<ResonantCoreOfEroica>(), 28)
        //         .AddIngredient(ModContent.ItemType<EroicasResonantEnergy>(), 22)
        //         .AddIngredient(ItemID.LunarBar, 16)
        //         .AddTile(TileID.LunarCraftingStation)
        //         .Register();
        // }
    }
}
