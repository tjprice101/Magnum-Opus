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
using MagnumOpus.Common.Systems.Particles;

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
            Item.damage = 200; // Balanced: ~1000 DPS (200 ÁE60/12)
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
            // Subtle ambient aura (Swan Lake benchmark: minimal HoldItem)
            if (Main.rand.NextBool(15))
            {
                Vector2 offset = Main.rand.NextVector2Circular(22f, 22f);
                Color gradientColor = Color.Lerp(UnifiedVFX.MoonlightSonata.DarkPurple, UnifiedVFX.MoonlightSonata.LightBlue, Main.rand.NextFloat());
                CustomParticles.PrismaticSparkle(player.Center + offset, gradientColor * 0.5f, 0.18f);
            }
            
            // Rare music note
            if (Main.rand.NextBool(25))
            {
                Vector2 notePos = player.Center + Main.rand.NextVector2Circular(25f, 25f);
                ThemedParticles.MusicNote(notePos, new Vector2(0, -0.5f), UnifiedVFX.MoonlightSonata.MediumPurple * 0.6f, 0.2f, 30);
            }
            
            // Pulsing mystical glow
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.06f) * 0.1f + 0.9f;
            Lighting.AddLight(player.Center, 0.35f * pulse, 0.22f * pulse, 0.5f * pulse);
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
            // Add slight spread for rapid fire feel
            float spread = MathHelper.ToRadians(5f);
            velocity = velocity.RotatedByRandom(spread);
            
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            
            Vector2 direction = velocity.SafeNormalize(Vector2.UnitX);
            
            // === GENTLE MUZZLE FLASH (Swan Lake benchmark) ===
            // Central flash
            CustomParticles.GenericFlare(position, UnifiedVFX.MoonlightSonata.LightBlue * 0.7f, 0.4f, 12);
            
            // Single halo
            CustomParticles.HaloRing(position, UnifiedVFX.MoonlightSonata.MediumPurple * 0.5f, 0.25f, 12);
            
            // Gentle music notes
            ThemedParticles.MoonlightMusicNotes(position, 2, 18f);
            
            return false;
        }

        public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Fires rapid moonlight beams that pierce enemies"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Beams leave sparkle trails and create prismatic impacts"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The moon whispers secrets to those who listen'")
            {
                OverrideColor = new Microsoft.Xna.Framework.Color(140, 100, 200)
            });
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
