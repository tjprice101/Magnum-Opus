using MagnumOpus.Common;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Content.OdeToJoy.Weapons.ThornSprayRepeater.Projectiles;
using MagnumOpus.Content.OdeToJoy.Weapons.ThornSprayRepeater.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.Fate.CraftingStations;
using MagnumOpus.Content.OdeToJoy.HarmonicCores;
using MagnumOpus.Content.OdeToJoy.ResonanceEnergies;

namespace MagnumOpus.Content.OdeToJoy.Weapons.ThornSprayRepeater
{
    public class ThornSprayRepeater : ModItem
    {
        private int _shotCounter;

        public override void SetDefaults()
        {
            Item.width = 52;
            Item.height = 24;
            Item.damage = 2400;
            Item.DamageType = DamageClass.Ranged;
            Item.useTime = 5;
            Item.useAnimation = 5;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 2f;
            Item.value = Item.sellPrice(platinum: 3, gold: 50);
            Item.rare = ModContent.RarityType<OdeToJoyRarity>();
            Item.UseSound = SoundID.Item5;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.crit = 12;
            Item.shoot = ModContent.ProjectileType<ThornSprayProjectile>();
            Item.shootSpeed = 16f;
            Item.useAmmo = AmmoID.Arrow;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            _shotCounter++;
            int projType = ModContent.ProjectileType<ThornSprayProjectile>();

            // Bloom Reload: shots 30-35 in a 36-shot cycle deal 1.5x damage and pass ai[1]=1
            bool isBloomReload = (_shotCounter % 36) >= 30;
            int finalDamage = isBloomReload ? (int)(damage * 1.5f) : damage;
            float bloomFlag = isBloomReload ? 1f : 0f;

            Projectile.NewProjectile(source, position, velocity, projType, finalDamage, knockback, player.whoAmI, ai1: bloomFlag);

            var thornPlayer = player.ThornSprayRepeater();
            thornPlayer.isActive = true;
            thornPlayer.shotCounter = _shotCounter;

            return false;
        }

        public override void HoldItem(Player player)
        {
            if (Main.rand.NextBool(4))
            {
                Vector2 offset = Main.rand.NextVector2Circular(20f, 20f);
                Color col = OdeToJoyPalette.GetGardenGradient(Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(player.Center + offset, DustID.GreenTorch,
                    new Vector2(0, -0.8f) + Main.rand.NextVector2Circular(0.4f, 0.4f), 0, col, 0.5f);
                d.noGravity = true;
            }

            float pulse = 0.7f + 0.3f * MathF.Sin(Main.GlobalTimeWrappedHourly * 3f);
            Lighting.AddLight(player.Center, new Vector3(0.4f, 0.35f, 0.15f) * pulse);
        }

        public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.06f) * 0.1f + 0.2f;
            Texture2D tex = Terraria.GameContent.TextureAssets.Item[Type].Value;
            Vector2 drawPos = Item.position - Main.screenPosition + new Vector2(Item.width / 2f, Item.height);
            Vector2 origin = new Vector2(tex.Width / 2f, tex.Height);
            spriteBatch.Draw(tex, drawPos, null, OdeToJoyPalette.LeafGreen with { A = 0 } * pulse, rotation, origin, scale * 1.05f, SpriteEffects.None, 0f);
            spriteBatch.Draw(tex, drawPos, null, OdeToJoyPalette.WarmAmber with { A = 0 } * (pulse * 0.6f), rotation, origin, scale * 1.02f, SpriteEffects.None, 0f);
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Converts arrows into rapid-fire crystalline thorns at 12 shots per second"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Thorns stack Thorn Accumulation on enemies — at 25 stacks, all thorns detonate simultaneously"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "After 36 shots, enters Bloom Reload — heals 15 HP and next 6 shots deal 50% bonus damage"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Standing still tightens spread and increases thorn velocity by 20%"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'A thousand thorns. A thousand tiny joys. A thousand reasons to stay down.'")
            {
                OverrideColor = OdeToJoyPalette.LoreText
            });
        }
    }
}
