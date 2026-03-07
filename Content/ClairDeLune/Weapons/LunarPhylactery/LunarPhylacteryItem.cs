using MagnumOpus.Common;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Content.ClairDeLune.Weapons.LunarPhylactery.Projectiles;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.ClairDeLune.Weapons.LunarPhylactery
{
    /// <summary>
    /// Lunar Phylactery — Summon weapon. Moonlight sentinel crystal minion.
    /// VoronoiCell-style body, sustained beam (40px range), Soul-Link HP scaling,
    /// Phylactery Pulse healing every 10s (3% max HP), Beam Crossing AoE.
    /// "A vessel for souls lost to time."
    /// </summary>
    public class LunarPhylacteryItem : ModItem
    {
        public override string Texture => "MagnumOpus/Content/ClairDeLune/Weapons/LunarPhylactery/LunarPhylactery";

        public override void SetDefaults()
        {
            Item.width = 40;
            Item.height = 40;
            Item.damage = 3100; // Tier 10 (2800-4200 range)
            Item.DamageType = DamageClass.Summon;
            Item.mana = 18;
            Item.useTime = 28;
            Item.useAnimation = 28;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 4f;
            Item.value = Item.sellPrice(platinum: 5);
            Item.rare = ModContent.RarityType<ClairDeLuneRarity>();
            Item.UseSound = SoundID.Item44;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<MoonlightSentinelProjectile>();
            Item.buffType = ModContent.BuffType<LunarPhylacteryBuff>();
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            player.AddBuff(Item.buffType, 2);

            // Scale damage based on player HP (Soul-Link)
            float hpRatio = player.statLife / (float)player.statLifeMax2;
            int scaledDamage = (int)(damage * (0.7f + hpRatio * 0.3f)); // 70%-100% based on HP

            Projectile.NewProjectile(source, position, velocity, type, scaledDamage, knockback, player.whoAmI);
            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Summon", "Summons a Moonlight Sentinel crystal to fight for you"));
            tooltips.Add(new TooltipLine(Mod, "Behavior", "Fires sustained moonlight beams at nearby enemies"));
            tooltips.Add(new TooltipLine(Mod, "SoulLink", "Soul-Link: sentinel damage scales with your HP (70%-100%)"));
            tooltips.Add(new TooltipLine(Mod, "Pulse", "Phylactery Pulse: heals 3% max HP every 10 seconds"));
            tooltips.Add(new TooltipLine(Mod, "Crossing", "Beam Crossing: overlapping beams from multiple sentinels create AoE bursts"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'A vessel for souls lost to time.'")
            {
                OverrideColor = ClairDeLunePalette.LoreText
            });
        }
    }

    /// <summary>
    /// Lunar Phylactery minion buff.
    /// </summary>
    public class LunarPhylacteryBuff : ModBuff
    {
        public override string Texture => "Terraria/Images/Buff_24";

        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            if (player.ownedProjectileCounts[ModContent.ProjectileType<MoonlightSentinelProjectile>()] > 0)
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
