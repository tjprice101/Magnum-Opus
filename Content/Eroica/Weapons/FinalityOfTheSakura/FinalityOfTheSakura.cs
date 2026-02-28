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
using System.Collections.Generic;

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
            return true;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            player.AddBuff(Item.buffType, 18000);
            position = Main.MouseWorld;

            // Summoning sounds
            SoundEngine.PlaySound(SoundID.Item119 with { Volume = 0.8f, Pitch = -0.4f }, position);
            SoundEngine.PlaySound(SoundID.Item73 with { Volume = 0.6f, Pitch = -0.3f }, position);

            // Spawn the Sakura of Fate
            Projectile.NewProjectile(source, position, Vector2.Zero, type, damage, knockback, player.whoAmI);

            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Summons a Sakura of Fate — a spectral guardian of black flame"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "The Sakura fires rapid streams of dark scarlet fire at nearby enemies"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Flames occasionally spawn seeking crystals that chain to nearby foes"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The last petal falls — and with it, the world remembers what it meant to bloom'")
            {
                OverrideColor = new Color(200, 50, 50)
            });
        }
    }
}
