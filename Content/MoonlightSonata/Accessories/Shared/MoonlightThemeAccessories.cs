using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Content.Common.Accessories;
using MagnumOpus.Content.MoonlightSonata.HarmonicCores;
using MagnumOpus.Content.MoonlightSonata.ResonanceEnergies;
using MagnumOpus.Content.MoonlightSonata.CraftingStations;
using MagnumOpus.Content.MoonlightSonata.VFX.Accessories;
using System;
using System.Collections.Generic;
using MagnumOpus.Content.Materials.EnemyDrops;

namespace MagnumOpus.Content.MoonlightSonata.Accessories
{
    #region Theme Colors
    
    public static class MoonlightColors
    {
        public static readonly Color DarkPurple = new Color(75, 0, 130);
        public static readonly Color MediumPurple = new Color(138, 43, 226);
        public static readonly Color Purple = new Color(138, 43, 226); // Alias for MediumPurple
        public static readonly Color LightBlue = new Color(135, 206, 250);
        public static readonly Color Silver = new Color(220, 220, 235);
        public static readonly Color Violet = new Color(180, 100, 220);
    }
    
    #endregion

    #region Adagio Pendant

    /// <summary>
    /// Adagio Pendant - Moonlight Sonata Tier 1 Theme Accessory.
    /// 5% chance to refund 20% of mana used on magic attacks.
    /// 10% chance to apply "Shattered Moon" reducing enemy defense by 20%.
    /// </summary>
    public class AdagioPendant : ModItem
    {
        public override string Texture => "MagnumOpus/Content/MoonlightSonata/Accessories/Shared/AdagioPendant";

        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 28;
            Item.accessory = true;
            Item.value = Item.buyPrice(platinum: 1);
            Item.rare = ModContent.RarityType<MoonlightSonataRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<AdagioPendantPlayer>().hasAdagioPendant = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "ManaRefund", "5% chance to refund 20% of mana used on magic attacks")
            {
                OverrideColor = MoonlightColors.LightBlue
            });

            tooltips.Add(new TooltipLine(Mod, "ShatteredMoon", "Magic attacks have a 10% chance to apply 'Shattered Moon'")
            {
                OverrideColor = MoonlightColors.DarkPurple
            });

            tooltips.Add(new TooltipLine(Mod, "ShatteredMoonDesc", "Shattered Moon: reduces enemy defense by 20%")
            {
                OverrideColor = MoonlightColors.Violet
            });

            tooltips.Add(new TooltipLine(Mod, "Flavor", "'The adagio plays softly in the moonlit night'")
            {
                OverrideColor = new Color(140, 100, 200)
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ResonantCoreOfMoonlightSonata>(), 15)
                .AddIngredient(ModContent.ItemType<MelodicCharm>(), 1)
                .AddIngredient(ModContent.ItemType<MoonlightsResonantEnergy>(), 1)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }

    public class AdagioPendantPlayer : ModPlayer
    {
        public bool hasAdagioPendant;

        public override void ResetEffects()
        {
            hasAdagioPendant = false;
        }

        public override void ModifyManaCost(Item item, ref float reduce, ref float mult)
        {
            // 5% chance to refund 20% of mana cost
            if (hasAdagioPendant && item.DamageType == DamageClass.Magic && Main.rand.NextFloat() < 0.05f)
            {
                mult *= 0.8f; // 20% reduction
            }
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (hasAdagioPendant && proj.owner == Player.whoAmI && proj.DamageType == DamageClass.Magic)
            {
                TryApplyShatteredMoon(target);
            }
        }

        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (hasAdagioPendant && item.DamageType == DamageClass.Magic)
            {
                TryApplyShatteredMoon(target);
            }
        }

        private void TryApplyShatteredMoon(NPC target)
        {
            // 10% chance to apply Shattered Moon (defense reduction)
            if (Main.rand.NextFloat() < 0.10f)
            {
                // Use Ichor as proxy for defense reduction
                target.AddBuff(BuffID.Ichor, 300); // 5 seconds
            }
        }
    }

    #endregion

    #region Sonata's Embrace

    /// <summary>
    /// Sonata's Embrace - Moonlight Sonata Tier 2 Theme Accessory (Ultimate).
    /// The full power of Moonlight Sonata crystallized into wearable form.
    /// All Moonlight bonuses maximized, enemies hit are "Moonstruck" (slowed, reduced damage).
    /// </summary>
    public class SonatasEmbrace : ModItem
    {
        public override string Texture => "MagnumOpus/Content/MoonlightSonata/Accessories/Shared/SonatasEmbrace";

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.value = Item.buyPrice(platinum: 3);
            Item.rare = ModContent.RarityType<MoonlightSonataRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var modPlayer = player.GetModPlayer<SonatasEmbracePlayer>();
            modPlayer.hasSonatasEmbrace = true;

            bool isNight = !Main.dayTime;

            // +18% damage at night, +8% during day
            if (isNight)
            {
                player.GetDamage(DamageClass.Generic) += 0.18f;
            }
            else
            {
                player.GetDamage(DamageClass.Generic) += 0.08f;
            }

            // +20% crit chance at night
            if (isNight)
            {
                player.GetCritChance(DamageClass.Generic) += 20;
            }

            // -15% mana cost always
            player.manaCost -= 0.15f;

            // Moonstruck debuff is applied via SonatasEmbracePlayer.OnHitNPC
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "ManaRefund", "10% chance to refund 20% of mana used on magic attacks")
            {
                OverrideColor = MoonlightColors.LightBlue
            });

            tooltips.Add(new TooltipLine(Mod, "NightDamage", "+18% damage at night, +8% during day")
            {
                OverrideColor = MoonlightColors.DarkPurple
            });

            tooltips.Add(new TooltipLine(Mod, "NightCrit", "+20% critical strike chance at night")
            {
                OverrideColor = MoonlightColors.LightBlue
            });

            tooltips.Add(new TooltipLine(Mod, "ManaCost", "-15% mana cost")
            {
                OverrideColor = MoonlightColors.Violet
            });

            tooltips.Add(new TooltipLine(Mod, "Moonstruck", "Magic attacks inflict 'Moonstruck' - slowed movement, -15 defense")
            {
                OverrideColor = new Color(200, 180, 255)
            });

            tooltips.Add(new TooltipLine(Mod, "Flavor", "'The moon's embrace is both gentle and absolute'")
            {
                OverrideColor = new Color(140, 100, 200)
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<AdagioPendant>(), 1)
                .AddIngredient(ModContent.ItemType<ResonantCoreOfMoonlightSonata>(), 25)
                .AddIngredient(ModContent.ItemType<MoonlightsResonantEnergy>(), 10)
                .AddIngredient(ModContent.ItemType<LunarEssence>(), 10)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }

    public class SonatasEmbracePlayer : ModPlayer
    {
        public bool hasSonatasEmbrace;

        public override void ResetEffects()
        {
            hasSonatasEmbrace = false;
        }

        public override void ModifyManaCost(Item item, ref float reduce, ref float mult)
        {
            // 10% chance to refund 20% of mana cost
            if (hasSonatasEmbrace && item.DamageType == DamageClass.Magic && Main.rand.NextFloat() < 0.10f)
            {
                mult *= 0.8f;
            }
        }

        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (hasSonatasEmbrace && item.DamageType == DamageClass.Magic)
            {
                ApplyMoonstruck(target);
            }
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (hasSonatasEmbrace && proj.owner == Player.whoAmI && proj.DamageType == DamageClass.Magic)
            {
                ApplyMoonstruck(target);
            }
        }

        private void ApplyMoonstruck(NPC target)
        {
            // Apply Moonstruck debuff (Slow + Ichor effect for damage reduction)
            target.AddBuff(BuffID.Slow, 180); // 3 seconds slow
            target.AddBuff(BuffID.Ichor, 120); // 2 seconds defense reduction as proxy for damage dealt reduction
            
            // Visual feedback
            if (Main.rand.NextBool(3))
            {
            }

            // Unified moonstruck application flash
            SonatasEmbraceVFX.MoonstruckApplicationFlash(target.Center);
        }
    }

    #endregion
}
