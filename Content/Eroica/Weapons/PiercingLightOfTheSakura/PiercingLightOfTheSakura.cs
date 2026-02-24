using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Terraria.GameContent;
using MagnumOpus.Content.Eroica.Projectiles;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.Eroica.Weapons.PiercingLightOfTheSakura
{
    public class PiercingLightOfTheSakura : ModItem
    {
        private int shotCounter = 0;

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.damage = 155;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 64;
            Item.height = 24;
            Item.useTime = 8;
            Item.useAnimation = 8;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 2.5f;
            Item.value = Item.sellPrice(gold: 40);
            Item.rare = ModContent.RarityType<EroicaRainbowRarity>();
            Item.UseSound = SoundID.Item11;
            Item.autoReuse = true;
            Item.shoot = ProjectileID.Bullet;
            Item.shootSpeed = 18f;
            Item.useAmmo = AmmoID.Bullet;
            Item.noMelee = true;
            Item.maxStack = 1;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            shotCounter++;

            PiercingLightOfTheSakuraVFX.NormalShotFlash(position, velocity);

            // Every 10th shot fires the special sakura lightning
            if (shotCounter >= 10)
            {
                shotCounter = 0;

                Projectile.NewProjectile(source, position, velocity * 1.2f,
                    ModContent.ProjectileType<PiercingLightOfTheSakuraProjectile>(),
                    (int)(damage * 2.5f), knockback * 2f, player.whoAmI);

                // Spawn seeking crystals
                SeekingCrystalHelper.SpawnEroicaCrystals(
                    source,
                    position + velocity.SafeNormalize(Vector2.UnitX) * 25f,
                    velocity * 0.6f,
                    (int)(damage * 0.4f),
                    knockback,
                    player.whoAmI,
                    5
                );

                SoundEngine.PlaySound(SoundID.Item125 with { Pitch = 0.3f, Volume = 0.8f }, position);

                PiercingLightOfTheSakuraVFX.ChargedShotFlash(position, velocity);

                return false;
            }

            // Normal bullet with dark tracer
            int proj = Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            if (proj >= 0 && proj < Main.maxProjectiles)
            {
                Main.projectile[proj].alpha = 200;
            }

            return false;
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-8f, 0f);
        }

        public override void HoldItem(Player player)
        {
            float chargeProgress = shotCounter / 9f;
            Vector2 gunTip = player.Center + new Vector2(45f * player.direction, -3f);
            PiercingLightOfTheSakuraVFX.ChargeOrbitVFX(gunTip, shotCounter, Main.GameUpdateCount * 0.04f);
            PiercingLightOfTheSakuraVFX.HoldItemVFX(player, shotCounter);
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            EroicaPalette.DrawItemBloom(spriteBatch, Item, rotation, scale);
            Lighting.AddLight(Item.Center, EroicaPalette.Scarlet.ToVector3() * 0.4f);
            return true;
        }

        public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "EroicaWeapon", "The light of fallen heroes guides each shot")
            {
                OverrideColor = EroicaPalette.OrangeGold
            });
        }
    }
}
