using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria.GameContent;
using MagnumOpus.Content.MoonlightSonata.ResonanceEnergies;
using MagnumOpus.Content.MoonlightSonata.Minions;
using MagnumOpus.Content.MoonlightSonata.CraftingStations;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems;

namespace MagnumOpus.Content.MoonlightSonata.Weapons
{
    /// <summary>
    /// Staff of the Lunar Phases - Summons a Goliath of Moonlight.
    /// A massive lunar guardian that fires healing beams at enemies.
    /// </summary>
    public class StaffOfTheLunarPhases : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 44;
            Item.height = 44;
            Item.damage = 280; // Balanced summon damage for Moonlight tier
            Item.DamageType = DamageClass.Summon;
            Item.mana = 15;
            Item.useTime = 36;
            Item.useAnimation = 36;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 4f;
            Item.value = Item.buyPrice(gold: 30);
            Item.rare = ModContent.RarityType<MoonlightSonataRarity>();
            Item.UseSound = SoundID.Item44;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<GoliathOfMoonlight>();
            Item.buffType = ModContent.BuffType<GoliathOfMoonlightBuff>();
            Item.maxStack = 1;
        }

        public override void HoldItem(Player player)
        {
            // Magical moonlight particles while holding the staff
            if (Main.rand.NextBool(4))
            {
                Vector2 offset = Main.rand.NextVector2Circular(22f, 22f);
                ThemedParticles.MoonlightSparkles(player.Center + offset, 2, 10f);
            }
            
            // Soft mystical glow
            Lighting.AddLight(player.Center, 0.3f, 0.25f, 0.5f);
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            // Draw glowing backlight effect when dropped in world
            Texture2D texture = TextureAssets.Item[Item.type].Value;
            Vector2 position = Item.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;
            
            // Calculate pulse - mystical and otherworldly
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.04f) * 0.12f + 1f;
            
            // Begin additive blending for glow
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Outer deep indigo aura - cosmic power
            spriteBatch.Draw(texture, position, null, new Color(50, 30, 100) * 0.45f, rotation, origin, scale * pulse * 1.35f, SpriteEffects.None, 0f);
            
            // Middle purple/blue glow - lunar phases
            spriteBatch.Draw(texture, position, null, new Color(120, 100, 220) * 0.35f, rotation, origin, scale * pulse * 1.18f, SpriteEffects.None, 0f);
            
            // Inner cyan/white glow - goliath's power
            spriteBatch.Draw(texture, position, null, new Color(180, 220, 255) * 0.25f, rotation, origin, scale * pulse * 1.06f, SpriteEffects.None, 0f);
            
            // Return to normal blending
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Add lighting
            Lighting.AddLight(Item.Center, 0.4f, 0.35f, 0.65f);
            
            return true;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Apply the buff
            player.AddBuff(Item.buffType, 18000);
            
            // Spawn position at mouse
            position = Main.MouseWorld;
            
            // Epic summoning effects - massive moonlight vortex for Goliath
            for (int i = 0; i < 50; i++)
            {
                float angle = MathHelper.TwoPi * i / 50f;
                Vector2 dustPos = position + new Vector2((float)System.Math.Cos(angle), (float)System.Math.Sin(angle)) * 80f;
                Vector2 dustVel = (position - dustPos).SafeNormalize(Vector2.Zero) * 6f;
                int dustType = Main.rand.NextBool() ? DustID.PurpleTorch : DustID.IceTorch;
                Dust dust = Dust.NewDustPerfect(dustPos, dustType, dustVel, 100, default, 2.2f);
                dust.noGravity = true;
            }
            
            // Inner purple burst
            for (int i = 0; i < 35; i++)
            {
                Vector2 dustVel = Main.rand.NextVector2Circular(10f, 10f);
                Dust dust = Dust.NewDustDirect(position, 1, 1, DustID.PurpleTorch, dustVel.X, dustVel.Y, 100, default, 2.2f);
                dust.noGravity = true;
            }
            
            // Light blue accents
            for (int i = 0; i < 25; i++)
            {
                Vector2 dustVel = Main.rand.NextVector2Circular(8f, 8f);
                Dust dust = Dust.NewDustDirect(position, 1, 1, DustID.IceTorch, dustVel.X, dustVel.Y, 100, default, 1.8f);
                dust.noGravity = true;
            }
            
            // White sparkles
            for (int i = 0; i < 20; i++)
            {
                Vector2 dustVel = Main.rand.NextVector2Circular(6f, 6f);
                Dust dust = Dust.NewDustDirect(position, 1, 1, DustID.SparksMech, dustVel.X, dustVel.Y, 100, Color.White, 1.5f);
                dust.noGravity = true;
            }
            
            // Powerful summoning sound
            Terraria.Audio.SoundEngine.PlaySound(SoundID.Item119 with { Volume = 1f, Pitch = -0.2f }, position);
            Terraria.Audio.SoundEngine.PlaySound(SoundID.Item82 with { Volume = 0.6f }, position);
            
            // Spawn the Goliath
            Projectile.NewProjectile(source, position, Vector2.Zero, type, damage, knockback, player.whoAmI);
            
            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<MoonlightsResonantEnergy>(), 20)
                .AddIngredient(ModContent.ItemType<ResonantCoreOfMoonlightSonata>(), 20)
                .AddIngredient(ModContent.ItemType<Enemies.ShardsOfMoonlitTempo>(), 20)
                .AddTile(ModContent.TileType<MoonlightAnvilTile>())
                .Register();
        }
        
        public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "MinionInfo", "Summons a Goliath of Moonlight")
            {
                OverrideColor = new Color(180, 150, 255)
            });
            
            tooltips.Add(new TooltipLine(Mod, "BeamInfo", "Fires explosive moonlight beams that heal you")
            {
                OverrideColor = new Color(150, 200, 255)
            });
            
            tooltips.Add(new TooltipLine(Mod, "HealInfo", "Each beam hit restores 10 health")
            {
                OverrideColor = new Color(100, 255, 150)
            });
        }
    }
}
