using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Content.Nachtmusik.Weapons.ConductorOfConstellations.Projectiles;
using MagnumOpus.Content.Nachtmusik.Weapons.ConductorOfConstellations.Buffs;
using MagnumOpus.Content.Nachtmusik.Weapons.ConductorOfConstellations.Utilities;

namespace MagnumOpus.Content.Nachtmusik.Weapons.ConductorOfConstellations
{
    /// <summary>
    /// Conductor of Constellations — The ultimate summon weapon from Nachtmusik.
    /// Summons a Stellar Conductor that commands star attacks.
    /// 2 minion slots, fires star barrages and periodic orchestra burst attacks.
    /// Uses a placeholder texture since no item PNG exists.
    /// </summary>
    public class ConductorOfConstellations : ModItem
    {
        public override string Texture => "Terraria/Images/Item_" + ItemID.RainbowCrystalStaff;

        public override void SetDefaults()
        {
            Item.width = 48;
            Item.height = 48;
            Item.damage = 1250;
            Item.DamageType = DamageClass.Summon;
            Item.mana = 35;
            Item.useTime = 35;
            Item.useAnimation = 35;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 5f;
            Item.value = Item.sellPrice(gold: 55);
            Item.rare = ModContent.RarityType<NachtmusikRarity>();
            Item.UseSound = SoundID.Item82;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<StellarConductorMinion>();
            Item.buffType = ModContent.BuffType<ConductorOfConstellationsBuff>();
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            player.AddBuff(Item.buffType, 2);

            Vector2 spawnPos = Main.MouseWorld;

            // Grand entrance VFX + screen shake
            ConductorOfConstellationsVFX.SummonVFX(spawnPos);
            MagnumScreenEffects.AddScreenShake(4f);

            Projectile.NewProjectile(source, spawnPos, Vector2.Zero, type, damage, knockback, player.whoAmI);

            return false;
        }

        public override void HoldItem(Player player)
        {
            int minionCount = player.ownedProjectileCounts[ModContent.ProjectileType<StellarConductorMinion>()];
            ConductorOfConstellationsVFX.HoldItemVFX(player, minionCount);
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Summon", "Summons a Stellar Conductor to command the cosmos"));
            tooltips.Add(new TooltipLine(Mod, "Slots", "Uses 2 minion slots"));
            tooltips.Add(new TooltipLine(Mod, "Behavior", "The conductor fires star barrages and periodic burst attacks"));
            tooltips.Add(new TooltipLine(Mod, "Debuff", "All attacks inflict Celestial Harmony"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Raise your baton and command the stars themselves'")
            {
                OverrideColor = new Color(100, 120, 200)
            });
        }
    }
}
