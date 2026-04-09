using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems;
using MagnumOpus.Content.Common.Accessories;
using MagnumOpus.Content.LaCampanella.HarmonicCores;
using MagnumOpus.Content.LaCampanella.ResonanceEnergies;
using MagnumOpus.Content.MoonlightSonata.CraftingStations;
using System;
using System.Collections.Generic;
using MagnumOpus.Content.Materials.EnemyDrops;

namespace MagnumOpus.Content.LaCampanella.Accessories
{
    #region Theme Colors
    
    public static class CampanellaColors
    {
        public static readonly Color Black = new Color(30, 20, 25);
        public static readonly Color Orange = new Color(255, 140, 40);
        public static readonly Color Gold = new Color(218, 165, 32);
        public static readonly Color Yellow = new Color(255, 200, 50);
        public static readonly Color DarkOrange = new Color(200, 80, 20);
    }
    
    #endregion

    #region Chime of Flames

    /// <summary>
    /// Chime of Flames - La Campanella Tier 1 Theme Accessory.
    /// +1 Minion Slot, 5% whip hits inflict "Tolling Death" - second strike at 75% damage.
    /// </summary>
    public class ChimeOfFlames : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 28;
            Item.accessory = true;
            Item.value = Item.buyPrice(platinum: 1);
            Item.rare = ModContent.RarityType<LaCampanellaRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var modPlayer = player.GetModPlayer<ChimeOfFlamesPlayer>();
            modPlayer.hasChimeOfFlames = true;

            // +1 Minion Slot
            player.maxMinions += 1;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "MinionSlot", "+1 minion slot")
            {
                OverrideColor = CampanellaColors.Gold
            });

            tooltips.Add(new TooltipLine(Mod, "TollingDeath", "5% chance for whip hits to inflict 'Tolling Death'")
            {
                OverrideColor = CampanellaColors.Orange
            });

            tooltips.Add(new TooltipLine(Mod, "TollingDesc", "Tolling Death: every hit strikes a second time at 75% damage")
            {
                OverrideColor = CampanellaColors.DarkOrange
            });

            tooltips.Add(new TooltipLine(Mod, "Flavor", "'Each chime carries the heat of a thousand flames'")
            {
                OverrideColor = new Color(255, 140, 40)
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ResonantCoreOfLaCampanella>(), 15)
                .AddIngredient(ModContent.ItemType<MelodicCharm>(), 1)
                .AddIngredient(ModContent.ItemType<LaCampanellaResonantEnergy>(), 1)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }

    public class ChimeOfFlamesPlayer : ModPlayer
    {
        public bool hasChimeOfFlames;

        public override void ResetEffects()
        {
            hasChimeOfFlames = false;
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!hasChimeOfFlames || proj.owner != Player.whoAmI) return;
            
            // Only whip projectiles
            if (ProjectileID.Sets.IsAWhip[proj.type] && Main.rand.NextFloat() < 0.05f)
            {
                ApplyTollingDeath(target, damageDone);
            }
        }

        private void ApplyTollingDeath(NPC target, int damageDone)
        {
            // Second strike at 75% damage
            int secondStrike = (int)(damageDone * 0.75f);
            if (secondStrike > 0 && Main.myPlayer == Player.whoAmI)
            {
                target.SimpleStrikeNPC(secondStrike, 0, false, 0, null, false, 0, true);
            }
            
            // Apply Withered Weapon
            target.AddBuff(BuffID.WitheredWeapon, 180);
            
            Terraria.Audio.SoundEngine.PlaySound(SoundID.Item35 with { Pitch = 0.5f, Volume = 0.6f }, target.Center);
        }
    }

    #endregion

    #region Infernal Virtuoso

    /// <summary>
    /// Infernal Virtuoso - La Campanella Tier 2 Theme Accessory (Ultimate).
    /// Immune to fire, +2 Minion Slots, 10% whip Tolling Death, whip+summon inflict Ichor+Cursed Inferno.
    /// </summary>
    public class InfernalVirtuoso : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.value = Item.buyPrice(platinum: 3);
            Item.rare = ModContent.RarityType<LaCampanellaRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var modPlayer = player.GetModPlayer<InfernalVirtuosoPlayer>();
            modPlayer.hasInfernalVirtuoso = true;

            // +2 Minion Slots
            player.maxMinions += 2;

            // Fire immunity
            player.buffImmune[BuffID.OnFire] = true;
            player.buffImmune[BuffID.OnFire3] = true;
            player.buffImmune[BuffID.Burning] = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "FireImmune", "Immune to fire debuffs")
            {
                OverrideColor = new Color(255, 150, 100)
            });

            tooltips.Add(new TooltipLine(Mod, "MinionSlots", "+2 minion slots")
            {
                OverrideColor = CampanellaColors.Gold
            });

            tooltips.Add(new TooltipLine(Mod, "TollingDeath", "10% chance for whip hits to inflict 'Tolling Death'")
            {
                OverrideColor = CampanellaColors.Orange
            });

            tooltips.Add(new TooltipLine(Mod, "TollingDesc", "Tolling Death: every hit strikes a second time at 75% damage")
            {
                OverrideColor = CampanellaColors.DarkOrange
            });

            tooltips.Add(new TooltipLine(Mod, "Debuffs", "Whip and summon attacks inflict Ichor and Cursed Inferno")
            {
                OverrideColor = CampanellaColors.Yellow
            });

            tooltips.Add(new TooltipLine(Mod, "Flavor", "'The virtuoso's fingers dance across keys of flame and shadow'")
            {
                OverrideColor = new Color(255, 140, 40)
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ChimeOfFlames>(), 1)
                .AddIngredient(ModContent.ItemType<ResonantCoreOfLaCampanella>(), 25)
                .AddIngredient(ModContent.ItemType<LaCampanellaResonantEnergy>(), 10)
                .AddIngredient(ModContent.ItemType<BellEssence>(), 10)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }

    public class InfernalVirtuosoPlayer : ModPlayer
    {
        public bool hasInfernalVirtuoso;

        public override void ResetEffects()
        {
            hasInfernalVirtuoso = false;
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!hasInfernalVirtuoso || proj.owner != Player.whoAmI) return;
            
            bool isWhip = ProjectileID.Sets.IsAWhip[proj.type];
            bool isSummon = proj.DamageType == DamageClass.Summon || proj.minion || proj.sentry;
            
            // Whip and summon attacks inflict Ichor and Cursed Inferno
            if (isWhip || isSummon)
            {
                target.AddBuff(BuffID.Ichor, 180);
                target.AddBuff(BuffID.CursedInferno, 180);
            }
            
            // 10% Tolling Death on whip hits
            if (isWhip && Main.rand.NextFloat() < 0.10f)
            {
                int secondStrike = (int)(damageDone * 0.75f);
                if (secondStrike > 0 && Main.myPlayer == Player.whoAmI)
                {
                    target.SimpleStrikeNPC(secondStrike, 0, false, 0, null, false, 0, true);
                }
                target.AddBuff(BuffID.WitheredWeapon, 180);
                
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item45 with { Pitch = 0.3f, Volume = 0.8f }, target.Center);
            }
        }
    }

    #endregion
}
