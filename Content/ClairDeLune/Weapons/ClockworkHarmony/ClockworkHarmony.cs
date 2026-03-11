using MagnumOpus.Common;
using MagnumOpus.Content.ClairDeLune.Weapons.ClockworkHarmony.Projectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.ClairDeLune.Weapons.ClockworkHarmony
{
    /// <summary>
    /// Clockwork Harmony — Ranged launcher that fires 3 sizes of clockwork gears.
    /// Small Gear (direct, fast, 1 bounce). Medium Gear (arc-lob, 3 bounces).
    /// Drive Gear (hold+release, slow heavy 2x damage). Gears mesh on collision.
    /// Gear Recall pulls all deployed gears back creating vortex.
    /// </summary>
    public class ClockworkHarmony : ModItem
    {
        private int _shotCounter;

        public override void SetDefaults()
        {
            Item.width = 88;
            Item.height = 88;
            Item.DamageType = DamageClass.Ranged;
            Item.damage = 3400; // Tier 10 (2800-4200 range)
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 9f;
            Item.value = Item.sellPrice(platinum: 5);
            Item.rare = ModContent.RarityType<ClairDeLuneRarity>();
            Item.UseSound = SoundID.Item61;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.crit = 18;
            Item.shoot = ModContent.ProjectileType<SmallGearProjectile>();
            Item.shootSpeed = 16f;
        }

        public override bool AltFunctionUse(Player player)
        {
            return true;
        }

        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                // Alt fire: Medium arc-lobbed gear
                Item.useTime = 28;
                Item.useAnimation = 28;
                Item.shootSpeed = 10f;
            }
            else
            {
                Item.useTime = 20;
                Item.useAnimation = 20;
                Item.shootSpeed = 16f;
            }
            return base.CanUseItem(player);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Vector2 dir = (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX);

            if (player.altFunctionUse == 2)
            {
                // Medium Gear — arc-lobbed, 3 bounces
                Vector2 vel = dir * 10f + new Vector2(0, -3f); // Slight upward arc
                Projectile.NewProjectile(source, player.Center, vel,
                    ModContent.ProjectileType<MediumGearProjectile>(),
                    (int)(damage * 1.2f), knockback, player.whoAmI);
            }
            else
            {
                _shotCounter++;

                if (_shotCounter % 8 == 0)
                {
                    // Every 8th shot: Drive Gear (heavy, slow, 2x damage)
                    Projectile.NewProjectile(source, player.Center, dir * 6f,
                        ModContent.ProjectileType<DriveGearProjectile>(),
                        damage * 2, knockback * 1.5f, player.whoAmI);
                }
                else
                {
                    // Normal: Small Gear (fast, direct)
                    Projectile.NewProjectile(source, player.Center, dir * 16f,
                        ModContent.ProjectileType<SmallGearProjectile>(),
                        damage, knockback, player.whoAmI);
                }
            }

            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Fires clockwork gears of three sizes that mesh on collision"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Right click to launch medium arc-lobbed gears"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Every 8th shot fires a heavy Drive Gear dealing double damage"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Meshing gears create sustained spinning AoE damage zones"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Harmony isn't found. It's engineered.'")
            {
                OverrideColor = ClairDeLunePalette.LoreText
            });
        }
    
        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D tex = TextureAssets.Item[Item.type].Value;
            Vector2 pos = Item.Center - Main.screenPosition;
            Vector2 origin = tex.Size() * 0.5f;

            float time = Main.GameUpdateCount * 0.05f;
            float pulse = 1f + (float)Math.Sin(time * 2.2f) * 0.05f
                + (float)Math.Sin(time * 3.8f) * 0.03f;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            ClairDeLunePalette.DrawItemBloom(spriteBatch, tex, pos, origin, rotation, scale, pulse);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Lighting.AddLight(Item.Center, ClairDeLunePalette.SoftBlue.ToVector3() * 0.35f);
            return true;
        }

        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            Texture2D tex = TextureAssets.Item[Item.type].Value;
            float time = Main.GameUpdateCount * 0.04f;
            float pulse = 1f + (float)Math.Sin(time * 2f) * 0.06f;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);

            float cycle = (float)Math.Sin(time * 0.7f) * 0.5f + 0.5f;
            Color glowColor = Color.Lerp(ClairDeLunePalette.SoftBlue, ClairDeLunePalette.PearlWhite, cycle) * 0.24f;
            spriteBatch.Draw(tex, position, frame, glowColor with { A = 0 }, 0f, origin, scale * pulse * 1.1f, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);

            spriteBatch.Draw(tex, position, frame, drawColor, 0f, origin, scale, SpriteEffects.None, 0f);
            return false;
        }
    }
}
