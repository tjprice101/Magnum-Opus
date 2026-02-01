using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using MagnumOpus.Content.Winter.Materials;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems;
using static MagnumOpus.Common.Systems.ThemedParticles;

namespace MagnumOpus.Content.Winter.Accessories
{
    /// <summary>
    /// Frostbite Amulet - Offense Winter accessory
    /// Post-Golem tier, applies Frostburn to enemies
    /// </summary>
    public class FrostbiteAmulet : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 28;
            Item.accessory = true;
            Item.rare = ItemRarityID.Yellow;
            Item.value = Item.sellPrice(gold: 10);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetDamage(DamageClass.Generic) += 0.1f; // 10% damage boost
            player.frostBurn = true; // Inflicts Frostburn
            
            // Frost aura
            if (!hideVisual && Main.rand.NextBool(8))
            {
                Vector2 pos = player.Center + Main.rand.NextVector2Circular(35f, 35f);
                Vector2 vel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(-1.5f, 0.5f));
                Color frostColor = Color.Lerp(new Color(173, 216, 230), new Color(0, 255, 255), Main.rand.NextFloat());
                CustomParticles.GenericGlow(pos, vel, frostColor, 0.26f, 24, true);
            }

            // Floating winter melody note
            if (!hideVisual && Main.rand.NextBool(10))
            {
                Vector2 notePos = player.Center + Main.rand.NextVector2Circular(30f, 30f);
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), Main.rand.NextFloat(0.1f, 0.4f)); // Gentle snowfall drift
                Color noteColor = Color.Lerp(new Color(150, 200, 255), new Color(240, 250, 255), Main.rand.NextFloat()) * 0.55f;
                ThemedParticles.MusicNote(notePos, noteVel, noteColor, 0.68f, 35);
            }
            
            Lighting.AddLight(player.Center, new Color(150, 200, 255).ToVector3() * 0.35f);
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Color winterBlue = new Color(173, 216, 230);
            Color winterCyan = new Color(0, 255, 255);
            
            tooltips.Add(new TooltipLine(Mod, "Season", "Winter Accessory") { OverrideColor = winterBlue });
            tooltips.Add(new TooltipLine(Mod, "Damage", "+10% damage") { OverrideColor = winterCyan });
            tooltips.Add(new TooltipLine(Mod, "Frostburn", "Attacks inflict Frostburn") { OverrideColor = winterBlue });
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The cold burns deeper than any flame'") { OverrideColor = Color.Lerp(winterBlue, Color.White, 0.3f) });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<WinterResonantEnergy>(), 2)
                .AddIngredient(ModContent.ItemType<FrostEssence>(), 4)
                .AddIngredient(ModContent.ItemType<ShardOfStillness>(), 12)
                .AddIngredient(ItemID.FrostCore, 1)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
    
    /// <summary>
    /// Stillness Shrine - Defense/Regen Winter accessory
    /// Post-Golem tier, provides defense and regen when stationary
    /// </summary>
    public class StillnessShrine : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 26;
            Item.height = 30;
            Item.accessory = true;
            Item.rare = ItemRarityID.Yellow;
            Item.value = Item.sellPrice(gold: 10);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.statDefense += 8;
            player.lifeRegen += 2;
            
            // Bonus when stationary
            if (player.velocity.Length() < 1f)
            {
                player.statDefense += 8; // +8 more when still (16 total)
                player.lifeRegen += 4; // +4 more regen when still
                player.endurance += 0.1f; // 10% damage reduction when still
                
                // Stillness shrine effect
                if (!hideVisual && Main.rand.NextBool(5))
                {
                    float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                    Vector2 pos = player.Center + angle.ToRotationVector2() * Main.rand.NextFloat(20f, 40f);
                    Color shrineColor = Color.Lerp(new Color(200, 230, 255), Color.White, Main.rand.NextFloat());
                    CustomParticles.GenericFlare(pos, shrineColor * 0.6f, 0.2f, 22);
                }

                // Floating winter melody note when still
                if (!hideVisual && Main.rand.NextBool(8))
                {
                    Vector2 notePos = player.Center + Main.rand.NextVector2Circular(30f, 30f);
                    Vector2 noteVel = new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), Main.rand.NextFloat(0.1f, 0.4f)); // Gentle snowfall drift
                    Color noteColor = Color.Lerp(new Color(150, 200, 255), new Color(240, 250, 255), Main.rand.NextFloat()) * 0.55f;
                    ThemedParticles.MusicNote(notePos, noteVel, noteColor, 0.68f, 35);
                }
            }
            
            Lighting.AddLight(player.Center, new Color(200, 220, 255).ToVector3() * 0.3f);
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Color winterBlue = new Color(173, 216, 230);
            Color winterWhite = new Color(200, 230, 255);
            
            tooltips.Add(new TooltipLine(Mod, "Season", "Winter Accessory") { OverrideColor = winterBlue });
            tooltips.Add(new TooltipLine(Mod, "Base", "+8 defense, +2 life regeneration") { OverrideColor = winterWhite });
            tooltips.Add(new TooltipLine(Mod, "Stationary", "While stationary: +8 additional defense, +4 life regen, +10% DR") { OverrideColor = winterBlue });
            tooltips.Add(new TooltipLine(Mod, "Lore", "'In perfect stillness, the cold becomes a shield'") { OverrideColor = Color.Lerp(winterWhite, Color.White, 0.3f) });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<WinterResonantEnergy>(), 2)
                .AddIngredient(ModContent.ItemType<FrostEssence>(), 4)
                .AddIngredient(ModContent.ItemType<ShardOfStillness>(), 12)
                .AddIngredient(ItemID.FrozenTurtleShell, 1)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
    
    /// <summary>
    /// Glacial Heart - Ultimate Winter accessory
    /// Post-Golem tier, provides immunity to frost and significant bonuses
    /// </summary>
    public class GlacialHeart : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 28;
            Item.accessory = true;
            Item.rare = ItemRarityID.Cyan;
            Item.value = Item.sellPrice(gold: 12);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            // Immunity to frost effects
            player.buffImmune[BuffID.Frozen] = true;
            player.buffImmune[BuffID.Chilled] = true;
            player.buffImmune[BuffID.Frostburn] = true;
            
            // Combat bonuses
            player.GetDamage(DamageClass.Generic) += 0.08f;
            player.GetCritChance(DamageClass.Generic) += 6;
            player.statDefense += 6;
            
            // Glacial heart visual
            if (!hideVisual)
            {
                if (Main.rand.NextBool(10))
                {
                    Vector2 pos = player.Center + Main.rand.NextVector2Circular(30f, 30f);
                    Vector2 vel = Main.rand.NextVector2Circular(1f, 1f);
                    Color heartColor = Color.Lerp(new Color(0, 255, 255), Color.White, Main.rand.NextFloat(0.3f));
                    CustomParticles.GenericFlare(pos, heartColor * 0.7f, 0.25f, 20);
                }
                
                // Orbiting ice crystals
                if (Main.rand.NextBool(15))
                {
                    float angle = Main.GameUpdateCount * 0.025f + Main.rand.NextFloat(MathHelper.TwoPi);
                    Vector2 orbitPos = player.Center + angle.ToRotationVector2() * 40f;
                    CustomParticles.GenericFlare(orbitPos, new Color(200, 240, 255) * 0.6f, 0.18f, 16);
                }

                // Floating winter melody note
                if (Main.rand.NextBool(10))
                {
                    Vector2 notePos = player.Center + Main.rand.NextVector2Circular(30f, 30f);
                    Vector2 noteVel = new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), Main.rand.NextFloat(0.1f, 0.4f)); // Gentle snowfall drift
                    Color noteColor = Color.Lerp(new Color(150, 200, 255), new Color(240, 250, 255), Main.rand.NextFloat()) * 0.55f;
                    ThemedParticles.MusicNote(notePos, noteVel, noteColor, 0.68f, 35);
                }
            }
            
            Lighting.AddLight(player.Center, new Color(150, 220, 255).ToVector3() * 0.4f);
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Color winterBlue = new Color(173, 216, 230);
            Color winterCyan = new Color(0, 255, 255);
            
            tooltips.Add(new TooltipLine(Mod, "Season", "Winter Accessory") { OverrideColor = winterBlue });
            tooltips.Add(new TooltipLine(Mod, "Stats", "+8% damage, +6% crit, +6 defense") { OverrideColor = winterCyan });
            tooltips.Add(new TooltipLine(Mod, "Immunity", "Immune to Frozen, Chilled, and Frostburn") { OverrideColor = winterBlue });
            tooltips.Add(new TooltipLine(Mod, "Lore", "'A heart of ice beats eternal'") { OverrideColor = Color.Lerp(winterCyan, Color.White, 0.3f) });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<WinterResonantEnergy>(), 3)
                .AddIngredient(ModContent.ItemType<FrostEssence>(), 6)
                .AddIngredient(ModContent.ItemType<ShardOfStillness>(), 18)
                .AddIngredient(ItemID.FrozenTurtleShell, 1)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}
