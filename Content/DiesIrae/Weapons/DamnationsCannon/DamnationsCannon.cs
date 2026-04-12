using MagnumOpus.Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.DiesIrae.Weapons.DamnationsCannon
{
    public class DamnationsCannon : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 80;
            Item.height = 36;
            Item.damage = 2350;
            Item.DamageType = DamageClass.Ranged;
            Item.useTime = 45;
            Item.useAnimation = 45;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 10f;
            Item.value = Item.sellPrice(platinum: 2, gold: 50);
            Item.rare = ModContent.RarityType<DiesIraeRarity>();
            Item.UseSound = SoundID.Item38 with { Pitch = -0.3f, Volume = 1.2f };
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<Projectiles.IgnitedWrathBallProjectile>();
            Item.shootSpeed = 14f;
            Item.useAmmo = AmmoID.Rocket;
            Item.crit = 20;
        }

        public override Vector2? HoldoutOffset() => new Vector2(-14f, 2f);

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Lobs an arcing ball of concentrated wrath that explodes on impact"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Explosions spawn 5 homing shrapnel fragments and a hellfire zone"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Hellfire zones persist for 5 seconds with escalating damage"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'This is not a weapon. This is a sentence.'")
            {
                OverrideColor = DiesIraePalette.LoreText
            });
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(source, player.MountedCenter, velocity, type, damage, knockback, player.whoAmI);
            return false;
        }

        public override void HoldItem(Player player)
        {
            if (Main.rand.NextBool(3))
            {
                Vector2 offset = Main.rand.NextVector2Circular(20f, 20f);
                Color col = DiesIraePalette.GetFireGradient(Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(player.Center + offset, DustID.Torch,
                    new Vector2(0, -1f) + Main.rand.NextVector2Circular(0.5f, 0.5f), 0, col, 0.6f);
                d.noGravity = true;
            }
            float pulse = 0.7f + 0.3f * MathF.Sin(Main.GlobalTimeWrappedHourly * 3f);
            Lighting.AddLight(player.Center, DiesIraePalette.InfernalRed.ToVector3() * 0.4f * pulse);
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
