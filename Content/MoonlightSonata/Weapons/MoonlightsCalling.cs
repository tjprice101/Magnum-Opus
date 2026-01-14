using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria.GameContent;
using MagnumOpus.Content.MoonlightSonata.ResonanceEnergies;
using MagnumOpus.Content.MoonlightSonata.Projectiles;
using MagnumOpus.Content.MoonlightSonata.CraftingStations;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems;

namespace MagnumOpus.Content.MoonlightSonata.Weapons
{
    /// <summary>
    /// Moonlight's Calling - A magic tome that casts rapid moonlight beams.
    /// Dark purple center gradient to light purple, sparkly beams.
    /// </summary>
    public class MoonlightsCalling : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 30;
            Item.damage = 200; // Balanced: ~1000 DPS (200 × 60/12)
            Item.DamageType = DamageClass.Magic;
            Item.mana = 8;
            Item.useTime = 12; // Fast fire rate
            Item.useAnimation = 12;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 3f;
            Item.value = Item.buyPrice(gold: 25);
            Item.rare = ModContent.RarityType<MoonlightSonataRarity>();
            Item.UseSound = SoundID.Item72;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<MoonlightBeam>();
            Item.shootSpeed = 16f;
            Item.noMelee = true;
            Item.staff[Item.type] = true;
            Item.maxStack = 1;
        }

        public override void HoldItem(Player player)
        {
            // GRADIENT COLORS: Dark Purple → Violet → Light Blue
            Color darkPurple = new Color(75, 0, 130);
            Color violet = new Color(138, 43, 226);
            Color lightBlue = new Color(135, 206, 250);
            
            // Ambient fractal orbit pattern with GRADIENT
            if (Main.rand.NextBool(6))
            {
                float baseAngle = Main.GameUpdateCount * 0.025f;
                for (int i = 0; i < 5; i++)
                {
                    float angle = baseAngle + MathHelper.TwoPi * i / 5f;
                    float radius = 28f + (float)Math.Sin(Main.GameUpdateCount * 0.05f + i * 0.6f) * 10f;
                    Vector2 flarePos = player.Center + angle.ToRotationVector2() * radius;
                    float progress = (float)i / 5f;
                    Color fractalColor = Color.Lerp(darkPurple, lightBlue, progress);
                    CustomParticles.GenericFlare(flarePos, fractalColor, 0.28f, 16);
                }
            }
            
            // Magical tome particles with GRADIENT
            if (Main.rand.NextBool(5))
            {
                Vector2 offset = Main.rand.NextVector2Circular(20f, 20f);
                float progress = Main.rand.NextFloat();
                Color gradientColor = Color.Lerp(darkPurple, lightBlue, progress);
                CustomParticles.GenericGlow(player.Center + offset, gradientColor, 0.25f, 18);
            }
            
            // Custom particle ethereal glow
            if (Main.rand.NextBool(8))
            {
                CustomParticles.MoonlightFlare(player.Center + Main.rand.NextVector2Circular(18f, 18f), 0.22f);
            }
            
            // Mystical glow
            Lighting.AddLight(player.Center, 0.3f, 0.2f, 0.45f);
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            // Draw glowing backlight effect when dropped in world
            Texture2D texture = TextureAssets.Item[Item.type].Value;
            Vector2 position = Item.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;
            
            // Calculate pulse - mystical like a calling
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.05f) * 0.1f + 1f;
            
            // Begin additive blending for glow
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Outer deep purple aura
            spriteBatch.Draw(texture, position, null, new Color(70, 30, 110) * 0.45f, rotation, origin, scale * pulse * 1.35f, SpriteEffects.None, 0f);
            
            // Middle violet glow
            spriteBatch.Draw(texture, position, null, new Color(140, 90, 200) * 0.32f, rotation, origin, scale * pulse * 1.18f, SpriteEffects.None, 0f);
            
            // Inner silver/light purple glow
            spriteBatch.Draw(texture, position, null, new Color(200, 180, 255) * 0.22f, rotation, origin, scale * pulse * 1.06f, SpriteEffects.None, 0f);
            
            // Return to normal blending
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Add lighting
            Lighting.AddLight(Item.Center, 0.4f, 0.3f, 0.55f);
            
            return true;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // GRADIENT COLORS
            Color darkPurple = new Color(75, 0, 130);
            Color violet = new Color(138, 43, 226);
            Color lightBlue = new Color(135, 206, 250);
            
            // Add slight spread for rapid fire feel
            float spread = MathHelper.ToRadians(5f);
            velocity = velocity.RotatedByRandom(spread);
            
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            
            // Muzzle flash with GRADIENT burst
            for (int i = 0; i < 6; i++)
            {
                float progress = (float)i / 6f;
                Color flashColor = Color.Lerp(darkPurple, lightBlue, progress);
                CustomParticles.GenericFlare(position + Main.rand.NextVector2Circular(8f, 8f), flashColor, 0.35f, 12);
            }
            
            // Gradient halo rings
            for (int ring = 0; ring < 3; ring++)
            {
                float progress = (float)ring / 3f;
                Color ringColor = Color.Lerp(darkPurple, lightBlue, progress);
                CustomParticles.HaloRing(position, ringColor, 0.25f + ring * 0.1f, 10 + ring * 3);
            }
            
            // Occasional music notes on cast
            if (Main.rand.NextBool(4))
            {
                ThemedParticles.MoonlightMusicNotes(position, 2, 20f);
                CustomParticles.MoonlightMusicNotes(position, 2, 18f);
            }
            
            // Custom particle muzzle flash
            if (Main.rand.NextBool(3))
            {
                CustomParticles.MoonlightFlare(position, 0.3f);
            }
            
            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<MoonlightsResonantEnergy>(), 15)
                .AddIngredient(ModContent.ItemType<ResonantCoreOfMoonlightSonata>(), 5)
                .AddIngredient(ModContent.ItemType<Enemies.ShardsOfMoonlitTempo>(), 10)
                .AddTile(ModContent.TileType<MoonlightAnvilTile>())
                .Register();
        }
    }
}
