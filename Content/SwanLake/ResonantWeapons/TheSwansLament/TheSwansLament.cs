using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Content.SwanLake.ResonantWeapons.TheSwansLament.Projectiles;
using MagnumOpus.Content.SwanLake.ResonantWeapons.TheSwansLament.Utilities;

namespace MagnumOpus.Content.SwanLake.ResonantWeapons.TheSwansLament
{
    /// <summary>
    /// The Swan's Lament — a mourning shotgun that finds catharsis in destruction.
    /// Fires a wide spread of dark bullets. Each enemy kill triggers "Lament's Echo":
    /// increased fire rate + wider spread for a chain-reaction window.
    /// On-death Destruction Halos deal AoE damage with prismatic revelation flashes.
    /// </summary>
    public class TheSwansLament : ModItem
    {
        public override string Texture => "MagnumOpus/Content/SwanLake/ResonantWeapons/TheSwansLament/TheSwansLament";

        public override void SetDefaults()
        {
            Item.damage = 180;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 60;
            Item.height = 24;
            Item.useTime = 35;
            Item.useAnimation = 35;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 5.5f;
            Item.rare = ModContent.RarityType<SwanRarity>();
            Item.value = Item.sellPrice(gold: 60);
            Item.shoot = ProjectileID.PurificationPowder;
            Item.shootSpeed = 14f;
            Item.useAmmo = AmmoID.Bullet;
            Item.autoReuse = true;
            Item.crit = 10;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1",
                "Fires a mourning volley of dark bullets in a wide arc"));
            tooltips.Add(new TooltipLine(Mod, "Effect2",
                "Killing enemies triggers Lament's Echo — briefly increasing fire rate and spread"));
            tooltips.Add(new TooltipLine(Mod, "Effect3",
                "Slain enemies release Destruction Halos that damage nearby foes"));
            tooltips.Add(new TooltipLine(Mod, "Lore",
                "'Even in grief, the swan's cry shakes the heavens.'")
            {
                OverrideColor = new Color(240, 240, 255)
            });
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,
            Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            var echoPlayer = player.GetModPlayer<LamentPlayer>();
            float fireRateMult = echoPlayer.FireRateMult;
            float spreadMult = echoPlayer.SpreadMult;

            // Base 10-15 bullets, Echo doesn't change count but tightens/widens spread
            int bulletCount = Main.rand.Next(10, 16);
            float baseSpread = MathHelper.ToRadians(22f); // ±22° base
            float actualSpread = baseSpread * spreadMult;

            for (int i = 0; i < bulletCount; i++)
            {
                float angle = Main.rand.NextFloat(-actualSpread, actualSpread);
                float speedVariance = Main.rand.NextFloat(0.85f, 1.15f);
                Vector2 bulletVel = velocity.RotatedBy(angle) * speedVariance;

                Projectile.NewProjectile(source, position, bulletVel,
                    ModContent.ProjectileType<LamentBulletProj>(),
                    damage, knockback, player.whoAmI);
            }

            return false; // we handle all projectile spawning
        }

        public override void ModifyWeaponDamage(Player player, ref StatModifier damage)
        {
            // No damage modification from Echo — it's fire rate only
        }

        public override float UseSpeedMultiplier(Player player)
        {
            var echoPlayer = player.GetModPlayer<LamentPlayer>();
            return 1f / echoPlayer.FireRateMult; // lower FireRateMult = faster attacks
        }

        public override Vector2? HoldoutOffset() => new Vector2(-10f, 0f);
    }
}
