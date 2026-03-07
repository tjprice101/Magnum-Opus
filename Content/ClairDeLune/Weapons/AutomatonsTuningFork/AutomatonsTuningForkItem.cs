using MagnumOpus.Common;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Content.ClairDeLune.Weapons.AutomatonsTuningFork.Projectiles;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.ClairDeLune.Weapons.AutomatonsTuningFork
{
    /// <summary>
    /// Automaton's Tuning Fork — Summon weapon with 4-frequency resonance system.
    /// A=Attack (ClockworkGold), C=Defense (NightMistBlue), E=Speed (SoftMoonBlue), G=Damage (mixed).
    /// PerlinFlow zones 5s. Perfect Resonance (2+ overlap = VoronoiCell + 2x damage).
    /// Conductor's Final Note every 30s (15-tile all-frequency zone).
    /// "Every machine has a frequency. Find it, and the world hums with you."
    /// </summary>
    public class AutomatonsTuningForkItem : ModItem
    {
        public override string Texture => "MagnumOpus/Content/ClairDeLune/Weapons/AutomatonsTuningFork/AutomatonsTuningFork";

        public override void SetDefaults()
        {
            Item.width = 36;
            Item.height = 36;
            Item.damage = 3400; // Tier 10 (2800-4200 range)
            Item.DamageType = DamageClass.Summon;
            Item.mana = 20;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 3f;
            Item.value = Item.sellPrice(platinum: 5);
            Item.rare = ModContent.RarityType<ClairDeLuneRarity>();
            Item.UseSound = SoundID.Item44;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<AutomatonMinionProjectile>();
            Item.buffType = ModContent.BuffType<AutomatonsTuningForkBuff>();
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            player.AddBuff(Item.buffType, 2);
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Summon", "Summons an Automaton that emits resonance frequencies"));
            tooltips.Add(new TooltipLine(Mod, "FreqA", "Frequency A: Attack aura — enemies take +20% damage"));
            tooltips.Add(new TooltipLine(Mod, "FreqC", "Frequency C: Defense aura — allies gain +10 defense"));
            tooltips.Add(new TooltipLine(Mod, "FreqE", "Frequency E: Speed aura — allies gain +15% movement speed"));
            tooltips.Add(new TooltipLine(Mod, "FreqG", "Frequency G: Damage aura — all nearby projectiles deal +10% damage"));
            tooltips.Add(new TooltipLine(Mod, "Resonance", "Perfect Resonance: overlapping frequencies create 2x damage zones"));
            tooltips.Add(new TooltipLine(Mod, "Conductor", "Conductor's Final Note: every 30s, all 4 frequencies burst in 15-tile radius"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Every machine has a frequency. Find it, and the world hums with you.'")
            {
                OverrideColor = ClairDeLunePalette.LoreText
            });
        }
    }

    /// <summary>
    /// Automaton's Tuning Fork minion buff.
    /// </summary>
    public class AutomatonsTuningForkBuff : ModBuff
    {
        public override string Texture => "Terraria/Images/Buff_24";

        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            if (player.ownedProjectileCounts[ModContent.ProjectileType<AutomatonMinionProjectile>()] > 0)
            {
                player.buffTime[buffIndex] = 18000;
            }
            else
            {
                player.DelBuff(buffIndex);
                buffIndex--;
            }
        }
    }
}
