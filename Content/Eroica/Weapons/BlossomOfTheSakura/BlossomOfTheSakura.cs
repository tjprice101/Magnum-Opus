using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.Eroica.Projectiles;
using MagnumOpus.Common;

namespace MagnumOpus.Content.Eroica.Weapons.BlossomOfTheSakura
{
    /// <summary>
    /// Blossom of the Sakura — Eroica heat-reactive assault rifle.
    /// Very fast fire rate with homing explosive sakura rounds.
    /// Heat system drives VFX intensity from cool sakura pinks
    /// through crimson to white-hot gold at sustained fire.
    /// Enhanced: overheat pulse, heat mirage, heat propagation to bullets.
    /// </summary>
    public class BlossomOfTheSakura : ModItem
    {
        private int heatLevel = 0;
        private int heatDecayCooldown = 0;
        private const int MaxHeat = 40;
        private bool hasOverheated = false;

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.damage = 75;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 64;
            Item.height = 28;
            Item.useTime = 4;
            Item.useAnimation = 4;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 3f;
            Item.value = Item.sellPrice(gold: 38);
            Item.rare = ModContent.RarityType<EroicaRainbowRarity>();
            Item.UseSound = SoundID.Item11;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<BlossomOfTheSakuraBulletProjectile>();
            Item.shootSpeed = 18f;
            Item.useAmmo = AmmoID.Bullet;
            Item.noMelee = true;
            Item.maxStack = 1;
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-2f, 0f);
        }

        public override void HoldItem(Player player)
        {
            // Heat decay when not firing
            if (heatDecayCooldown > 0)
                heatDecayCooldown--;
            else if (heatLevel > 0)
                heatLevel--;

            float heatProgress = (float)heatLevel / MaxHeat;
            Vector2 gunBarrel = player.Center + new Vector2(40f * player.direction, -2f);
            Vector2 gunBody = player.Center + new Vector2(20f * player.direction, -2f);

            // Heat-reactive barrel VFX
            BlossomOfTheSakuraVFX.BarrelHeatVFX(gunBarrel, gunBody, heatProgress, player.direction);

            // Heat mirage shimmer at medium-high heat
            if (heatProgress > 0.3f)
                BlossomOfTheSakuraVFX.HeatMirage(gunBarrel, heatProgress);

            // Ambient hold VFX (sakura petals, embers, music notes)
            BlossomOfTheSakuraVFX.HoldItemVFX(player, heatProgress);

            // Overheat pulse — triggers once when heat reaches maximum
            if (heatLevel >= MaxHeat && !hasOverheated)
            {
                hasOverheated = true;
                BlossomOfTheSakuraVFX.OverheatPulse(gunBarrel);
            }
            else if (heatLevel < MaxHeat)
            {
                hasOverheated = false;
            }
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            EroicaPalette.DrawItemBloom(spriteBatch, Item, rotation, scale);
            Lighting.AddLight(Item.Center, EroicaPalette.Sakura.ToVector3() * 0.4f);
            return true;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Heat buildup
            heatLevel = Math.Min(heatLevel + 2, MaxHeat);
            heatDecayCooldown = 20;
            float heatProgress = (float)heatLevel / MaxHeat;

            type = ModContent.ProjectileType<BlossomOfTheSakuraBulletProjectile>();

            Vector2 perturbedVelocity = velocity.RotatedByRandom(MathHelper.ToRadians(3));

            // Pass heat progress to bullet via ai[0]
            Projectile.NewProjectile(source, position, perturbedVelocity, type, damage, knockback,
                player.whoAmI, ai0: heatProgress);

            // Muzzle flash VFX
            Vector2 muzzlePos = position + velocity.SafeNormalize(Vector2.Zero) * 25f;
            BlossomOfTheSakuraVFX.MuzzleFlashVFX(muzzlePos, velocity, heatProgress);

            return false;
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            position += velocity.SafeNormalize(Vector2.UnitX * player.direction) * 40f;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1",
                "Very high fire rate assault rifle with homing explosive sakura rounds")
            { OverrideColor = EroicaPalette.Sakura });
            tooltips.Add(new TooltipLine(Mod, "Effect2",
                "Sustained fire heats the barrel — hotter shots track harder and glow brighter")
            { OverrideColor = new Color(240, 180, 100) });
            tooltips.Add(new TooltipLine(Mod, "Effect3",
                "Hits have a chance to unleash seeking valor crystals")
            { OverrideColor = EroicaPalette.Gold });
            tooltips.Add(new TooltipLine(Mod, "Lore",
                "'A storm of sakura and steel — each round a petal torn from spring's last breath'")
            { OverrideColor = EroicaPalette.Scarlet });
        }
    }
}
