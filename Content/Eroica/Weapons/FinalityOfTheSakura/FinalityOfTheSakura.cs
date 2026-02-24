using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria.Audio;
using Terraria.GameContent;
using MagnumOpus.Content.Eroica.Minions;
using MagnumOpus.Common;

namespace MagnumOpus.Content.Eroica.Weapons.FinalityOfTheSakura
{
    public class FinalityOfTheSakura : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 48;
            Item.height = 48;
            Item.damage = 320;
            Item.DamageType = DamageClass.Summon;
            Item.mana = 20;
            Item.useTime = 36;
            Item.useAnimation = 36;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 5f;
            Item.value = Item.buyPrice(platinum: 1);
            Item.rare = ModContent.RarityType<EroicaRainbowRarity>();
            Item.UseSound = null;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<SakuraOfFate>();
            Item.buffType = ModContent.BuffType<SakuraOfFateBuff>();
            Item.maxStack = 1;
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            EroicaPalette.DrawItemBloom(spriteBatch, Item, rotation, scale);
            Lighting.AddLight(Item.Center, EroicaPalette.DeepScarlet.ToVector3() * 0.4f);
            return true;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            player.AddBuff(Item.buffType, 18000);
            position = Main.MouseWorld;

            FinalityOfTheSakuraVFX.SummonExplosionVFX(position);

            // Summoning sounds
            SoundEngine.PlaySound(SoundID.Item119 with { Volume = 0.8f, Pitch = -0.4f }, position);
            SoundEngine.PlaySound(SoundID.Item73 with { Volume = 0.6f, Pitch = -0.3f }, position);

            // Spawn the Sakura of Fate
            Projectile.NewProjectile(source, position, Vector2.Zero, type, damage, knockback, player.whoAmI);

            return false;
        }

        public override void HoldItem(Player player)
        {
            FinalityOfTheSakuraVFX.HoldItemVFX(player);
        }

        public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "MinionInfo", "Summons a Sakura of Fate to fight for you")
            {
                OverrideColor = EroicaPalette.Sakura
            });

            tooltips.Add(new TooltipLine(Mod, "FlameInfo", "The Sakura fires black and scarlet flames at enemies")
            {
                OverrideColor = EroicaPalette.BladeCrimson
            });

            tooltips.Add(new TooltipLine(Mod, "FateInfo", "'A final blossom before eternal night'")
            {
                OverrideColor = new Color(100, 100, 100)
            });
        }
    }
}
