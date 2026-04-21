using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.Nachtmusik;
using MagnumOpus.Content.Nachtmusik.Weapons.SerenadeOfDistantStars.Projectiles;
using MagnumOpus.Content.Nachtmusik.Systems;
using MagnumOpus.Common.Systems.VFX;

namespace MagnumOpus.Content.Nachtmusik.Weapons.SerenadeOfDistantStars
{
    public class SerenadeOfDistantStars : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 36;
            Item.height = 70;
            Item.damage = 1200;
            Item.DamageType = DamageClass.Ranged;
            Item.useTime = 18;
            Item.useAnimation = 18;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 4f;
            Item.value = Item.sellPrice(gold: 48);
            Item.rare = ModContent.RarityType<NachtmusikRarity>();
            Item.UseSound = SoundID.Item91 with { Pitch = 0.2f, Volume = 0.8f };
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.shoot = ModContent.ProjectileType<SerenadeStarProjectile>();
            Item.shootSpeed = 16f;
            Item.useAmmo = AmmoID.Bullet;
            Item.crit = 20;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            var combat = player.GetModPlayer<NachtmusikCombatPlayer>();
            int stacks = combat.SerenadeRhythmStacks;

            // Determine orb behavior based on rhythm stacks
            float homing = stacks >= 5 ? 0.12f : 0.066f;
            int flags = 0;
            float scale = 1f;

            if (stacks >= 2)
                flags |= GenericHomingOrbChild.FLAG_ACCELERATE;
            if (stacks >= 3)
                flags |= GenericHomingOrbChild.FLAG_PIERCE;
            if (stacks >= 5)
                scale = 1.3f;

            // Spawn the orb instead of a basic projectile
            GenericHomingOrbChild.SpawnChild(
                source, position, velocity.SafeNormalize(Vector2.UnitX) * Item.shootSpeed,
                damage, knockback, player.whoAmI,
                homing, flags, GenericHomingOrbChild.THEME_NACHTMUSIK,
                scale, 120);

            // Also spawn the tracking projectile (thin, invisible — just for hit detection to increment stacks)
            Projectile.NewProjectile(source, position, velocity,
                ModContent.ProjectileType<SerenadeStarProjectile>(), damage, knockback, player.whoAmI);
            return false;
        }

        public override Vector2? HoldoutOffset() => new Vector2(-6f, 0f);

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Consecutive hits build Rhythm stacks (max 5)"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Higher stacks upgrade orb speed, pierce, and homing"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The light left a star ages ago, just to find you. And it never missed.'")
            {
                OverrideColor = NachtmusikPalette.LoreText
            });
        }
    }
}
