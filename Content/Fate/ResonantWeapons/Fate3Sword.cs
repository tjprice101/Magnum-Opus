using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Content.Fate.Debuffs;
using MagnumOpus.Content.Fate.Projectiles;

namespace MagnumOpus.Content.Fate.ResonantWeapons
{
    /// <summary>
    /// Fate3Sword - Cosmic Channeler
    /// A held channeling sword that projects a cosmic beam. The beam brightens the longer you hold it.
    /// Periodic lightning strikes at the cursor position while channeling.
    /// </summary>
    public class Fate3Sword : ModItem
    {
        public override string Texture => "Terraria/Images/Item_" + ItemID.Zenith;
        
        public override void SetDefaults()
        {
            Item.damage = 680;
            Item.DamageType = DamageClass.Melee;
            Item.width = 54;
            Item.height = 54;
            Item.useTime = 5;
            Item.useAnimation = 5;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 4f;
            Item.value = Item.sellPrice(gold: 55);
            Item.rare = ModContent.RarityType<FateRarity>();
            Item.UseSound = null;
            Item.autoReuse = true;
            Item.channel = true;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.shoot = ModContent.ProjectileType<CosmicBeamHeldSword>();
            Item.shootSpeed = 1f;
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "FateEffect", "Hold to channel a cosmic beam that intensifies over time"));
            tooltips.Add(new TooltipLine(Mod, "FateSpecial", "Lightning strikes the cursor periodically while channeling"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Channel the cosmic storm through your blade'")
            {
                OverrideColor = FateCosmicVFX.FateBrightRed
            });
        }
        
        public override bool CanUseItem(Player player)
        {
            return player.ownedProjectileCounts[ModContent.ProjectileType<CosmicBeamHeldSword>()] < 1;
        }
        
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Track for star circle effect
            player.GetModPlayer<FateWeaponEffectPlayer>()?.OnFateWeaponAttack(player.Center);
            return true;
        }
    }
}
