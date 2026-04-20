using MagnumOpus.Common;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Content.DiesIrae.Weapons.SinCollector.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.DiesIrae.Weapons.SinCollector
{
    public class SinCollector : ModItem
    {
        private int _shotCounter;

        public override void SetDefaults()
        {
            Item.width = 70;
            Item.height = 28;
            Item.damage = 2400;
            Item.DamageType = DamageClass.Ranged;
            Item.useTime = 5;
            Item.useAnimation = 5;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 8f;
            Item.value = Item.sellPrice(platinum: 2, gold: 50);
            Item.rare = ModContent.RarityType<DiesIraeRarity>();
            Item.UseSound = SoundID.Item40 with { Pitch = -0.2f };
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<Projectiles.SinBulletProjectile>();
            Item.shootSpeed = 25f;
            Item.useAmmo = AmmoID.Bullet;
            Item.crit = 35;
        }

        public override bool AltFunctionUse(Player player) => true;

        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                var sinPlayer = player.SinCollector();
                // Need at least 10 sins to expend
                return sinPlayer.sinsCollected >= 10;
            }
            return base.CanUseItem(player);
        }

        public override Vector2? HoldoutOffset() => new Vector2(-10f, 0f);

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Each hit collects sin from the target"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Right-click expends collected sin for devastating enhanced shots"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "10+ Sins: Penance Shot, 20+: Absolution, 30: Damnation"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Your sins are not forgiven. They are collected.'")
            {
                OverrideColor = DiesIraePalette.LoreText
            });
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            var sinPlayer = player.SinCollector();
            sinPlayer.isActive = true;

            // Right-click: Sin Expenditure
            if (player.altFunctionUse == 2)
            {
                int sins = sinPlayer.sinsCollected;
                int tier;
                int expendDamage;

                if (sins >= 30)
                {
                    tier = 3; // Damnation
                    expendDamage = damage * 3;
                }
                else if (sins >= 20)
                {
                    tier = 2; // Absolution
                    expendDamage = damage * 2;
                }
                else
                {
                    tier = 1; // Penance
                    expendDamage = (int)(damage * 1.5f);
                }

                int projType = ModContent.ProjectileType<Projectiles.SinExpendProjectile>();
                Terraria.Projectile.NewProjectile(source, player.MountedCenter, velocity, projType,
                    expendDamage, knockback, player.whoAmI, ai0: tier);

                sinPlayer.ConsumeSins();
                return false;
            }

            // Left-click: Rapid straight shots
            _shotCounter++;

            // Bloom Reload: every 36 shots, next 6 are enhanced
            bool bloomReload = (_shotCounter % 36) >= 30; // shots 30-35 of each cycle are enhanced
            float ai0 = bloomReload ? 1f : 0f;
            int finalDamage = bloomReload ? (int)(damage * 1.5f) : damage;

            Terraria.Projectile.NewProjectile(source, player.MountedCenter, velocity, type,
                finalDamage, knockback, player.whoAmI, ai0: ai0);
            return false;
        }

        public override void HoldItem(Player player)
        {
            var sinPlayer = player.SinCollector();
            sinPlayer.isActive = true;

            if (Main.rand.NextBool(3))
            {
                Vector2 offset = Main.rand.NextVector2Circular(20f, 20f);
                Color col = DiesIraePalette.GetFireGradient(Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(player.Center + offset, DustID.Torch,
                    new Vector2(0, -1f) + Main.rand.NextVector2Circular(0.5f, 0.5f), 0, col, 0.6f);
                d.noGravity = true;
            }

            // Intensity scales with sin count
            float sinIntensity = sinPlayer.GetSinIntensity();
            float pulse = 0.7f + 0.3f * MathF.Sin(Main.GlobalTimeWrappedHourly * 3f);
            Lighting.AddLight(player.Center, DiesIraePalette.InfernalRed.ToVector3() * (0.4f + sinIntensity * 0.3f) * pulse);
        }

        public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.06f) * 0.1f + 0.2f;
            Texture2D tex = Terraria.GameContent.TextureAssets.Item[Type].Value;
            Vector2 drawPos = Item.position - Main.screenPosition + new Vector2(Item.width / 2f, Item.height);
            Vector2 origin = new Vector2(tex.Width / 2f, tex.Height);
            spriteBatch.Draw(tex, drawPos, null, DiesIraePalette.InfernalRed with { A = 0 } * pulse,
                rotation, origin, scale * 1.05f, SpriteEffects.None, 0f);
            spriteBatch.Draw(tex, drawPos, null, DiesIraePalette.JudgmentGold with { A = 0 } * (pulse * 0.7f),
                rotation, origin, scale * 1.02f, SpriteEffects.None, 0f);
        }
    }
}
