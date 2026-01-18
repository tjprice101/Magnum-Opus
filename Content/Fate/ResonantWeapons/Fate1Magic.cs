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
    /// Fate1Magic - Cosmic Lightning Staff
    /// Channels cosmic lightning toward the cursor. When 3+ unique enemies are hit,
    /// triggers a screen-wide zodiac explosion.
    /// </summary>
    public class Fate1Magic : ModItem
    {
        public override string Texture => "Terraria/Images/Item_" + ItemID.LastPrism;
        
        public override void SetDefaults()
        {
            Item.damage = 460;
            Item.DamageType = DamageClass.Magic;
            Item.width = 40;
            Item.height = 40;
            Item.useTime = 10;
            Item.useAnimation = 10;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 2f;
            Item.value = Item.sellPrice(gold: 55);
            Item.rare = ModContent.RarityType<FateRarity>();
            Item.UseSound = SoundID.Item15;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.channel = true;
            Item.mana = 15;
            Item.shoot = ModContent.ProjectileType<CosmicElectricityStaff>();
            Item.shootSpeed = 1f;
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "FateEffect", "Hold to channel cosmic lightning toward the cursor"));
            tooltips.Add(new TooltipLine(Mod, "FateSpecial", "Hitting 3+ unique enemies triggers a devastating zodiac explosion"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The heavens unleash their fury upon command'")
            {
                OverrideColor = FateCosmicVFX.FateBrightRed
            });
        }
        
        public override bool CanUseItem(Player player)
        {
            return player.ownedProjectileCounts[ModContent.ProjectileType<CosmicElectricityStaff>()] < 1;
        }
        
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Track for star circle effect
            player.GetModPlayer<FateWeaponEffectPlayer>()?.OnFateWeaponAttack(player.Center);
            
            // Spawn channeled lightning staff
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            
            // Initial cast VFX
            FateCosmicVFX.SpawnGlyphBurst(player.Center, 4, 5f, 0.4f);
            
            return false;
        }
    }
}
