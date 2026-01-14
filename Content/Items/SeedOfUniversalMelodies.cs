using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Terraria.DataStructures;

namespace MagnumOpus.Content.Items
{
    /// <summary>
    /// Seed of Universal Melodies - A legendary crafting material that can transform
    /// modded weapons into their Celestial forms, granting new attacks and enhanced stats.
    /// Only usable once per weapon.
    /// </summary>
    public class SeedOfUniversalMelodies : ModItem
    {
        // TODO: Create proper texture - using HeartOfMusic as placeholder
        public override string Texture => "MagnumOpus/Content/Items/HeartOfMusic";

        public override void SetDefaults()
        {
            Item.width = 48;
            Item.height = 48;
            Item.maxStack = 99;
            Item.value = Item.sellPrice(gold: 25);
            Item.rare = ItemRarityID.Purple; // Expert/Master tier rarity
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.UseSound = SoundID.Item4;
            Item.consumable = false; // Don't consume on use - consumed during upgrade
        }

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 3;
            
            // Make the item glow in inventory
            ItemID.Sets.ItemIconPulse[Type] = true;
            ItemID.Sets.ItemNoGravity[Type] = true;
        }

        public override void PostUpdate()
        {
            // Celestial rainbow glow effect
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.03f);
            float r = 0.5f + (float)Math.Sin(Main.GameUpdateCount * 0.02f) * 0.3f;
            float g = 0.5f + (float)Math.Sin(Main.GameUpdateCount * 0.025f + 1f) * 0.3f;
            float b = 0.5f + (float)Math.Sin(Main.GameUpdateCount * 0.03f + 2f) * 0.3f;
            
            Lighting.AddLight(Item.Center, r, g, b);

            // Musical note particles floating upward
            if (Main.rand.NextBool(8))
            {
                int dustType = Main.rand.Next(5) switch
                {
                    0 => DustID.GoldFlame,
                    1 => DustID.PurpleTorch,
                    2 => DustID.IceTorch,
                    3 => DustID.PinkTorch,
                    _ => DustID.SilverCoin
                };
                
                Dust note = Dust.NewDustDirect(Item.position, Item.width, Item.height, dustType, 0f, -1f, 100, default, 1f);
                note.noGravity = true;
                note.velocity = new Vector2(Main.rand.NextFloat(-1f, 1f), -2f);
            }

            // Clockwork gear particles
            if (Main.rand.NextBool(15))
            {
                Dust gear = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.Copper, 0f, 0f, 100, default, 0.8f);
                gear.noGravity = true;
                gear.velocity = Main.rand.NextVector2Circular(1.5f, 1.5f);
            }

            // Voltaic spark
            if (Main.rand.NextBool(20))
            {
                Dust spark = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.Electric, 0f, 0f, 0, default, 0.6f);
                spark.noGravity = true;
                spark.velocity = Main.rand.NextVector2Circular(2f, 2f);
            }
        }

        public override Color? GetAlpha(Color lightColor)
        {
            // Rainbow pulsing self-illumination
            float time = Main.GameUpdateCount * 0.02f;
            byte r = (byte)(180 + Math.Sin(time) * 75);
            byte g = (byte)(180 + Math.Sin(time + 2f) * 75);
            byte b = (byte)(180 + Math.Sin(time + 4f) * 75);
            
            return new Color(r, g, b, 255);
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            // Pulsing glow effect
            Texture2D texture = Terraria.GameContent.TextureAssets.Item[Type].Value;
            Vector2 drawPos = Item.position - Main.screenPosition + new Vector2(Item.width / 2f, Item.height / 2f);
            
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.05f) * 0.15f + 0.85f;
            
            // Rainbow glow layers
            for (int i = 0; i < 6; i++)
            {
                float angle = Main.GameUpdateCount * 0.02f + i * MathHelper.TwoPi / 6f;
                Color glowColor = Main.hslToRgb((i / 6f + Main.GameUpdateCount * 0.005f) % 1f, 1f, 0.6f) * 0.3f;
                Vector2 offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 4f;
                
                spriteBatch.Draw(texture, drawPos + offset, null, glowColor, rotation, texture.Size() / 2f, scale * pulse, SpriteEffects.None, 0f);
            }
            
            return true; // Draw the main item normally
        }

        public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
        {
            // Add special tooltip explaining the upgrade system
            tooltips.Add(new TooltipLine(Mod, "SeedInfo1", "Contains the essence of all cosmic melodies"));
            tooltips.Add(new TooltipLine(Mod, "SeedInfo2", "[c/FFD700:Right-click a Magnum Opus weapon to transform it]"));
            tooltips.Add(new TooltipLine(Mod, "SeedInfo3", "[c/FF6600:Celestial weapons gain new attacks and enhanced power]"));
            tooltips.Add(new TooltipLine(Mod, "SeedInfo4", "[c/FF0000:Each weapon can only be transformed once]"));
        }
    }
}
