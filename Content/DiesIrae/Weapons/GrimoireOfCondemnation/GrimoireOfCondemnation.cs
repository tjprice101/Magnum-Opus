using MagnumOpus.Common;
using MagnumOpus.Content.DiesIrae.Weapons.GrimoireOfCondemnation.Projectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using System;
using Terraria;
using Terraria.GameContent;

namespace MagnumOpus.Content.DiesIrae.Weapons.GrimoireOfCondemnation
{
    public class GrimoireOfCondemnation : ModItem
    {
        private int castCount = 0;

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.damage = 1650; // Tier 8 (1600-2400 range)
            Item.DamageType = DamageClass.Magic;
            Item.mana = 12;
            Item.useTime = 15;
            Item.useAnimation = 15;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 4f;
            Item.value = Item.sellPrice(platinum: 1, gold: 50);
            Item.rare = ModContent.RarityType<global::MagnumOpus.Common.DiesIraeRarity>();
            Item.UseSound = SoundID.Item103;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.channel = true;
            Item.shoot = ModContent.ProjectileType<BlazingShardProjectile>();
            Item.shootSpeed = 12f;
            Item.crit = 15;
        }

        public override bool AltFunctionUse(Player player) => true;

        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                // Dark Sermon — higher mana cost
                Item.mana = 40;
                Item.useTime = 30;
                Item.useAnimation = 30;
                Item.channel = false;
            }
            else
            {
                Item.mana = 12;
                Item.useTime = 15;
                Item.useAnimation = 15;
                Item.channel = true;
            }
            return base.CanUseItem(player);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse == 2)
            {
                // Dark Sermon — ritual circle at cursor position
                Vector2 targetPos = Main.MouseWorld;
                Projectile.NewProjectile(source, targetPos, Vector2.Zero,
                    ModContent.ProjectileType<DarkSermonSigilProjectile>(),
                    (int)(damage * 2.5f), knockback, player.whoAmI);
                return false;
            }

            // Condemnation Wave beam
            castCount++;
            int actualDamage = damage;

            // Page Turn: every 7th cast = +30% damage
            if (castCount % 7 == 0)
            {
                actualDamage = (int)(damage * 1.3f);
                Utilities.GrimoireOfCondemnationUtils.DoPageTurn(player.Center);
            }

            Projectile.NewProjectile(source, position, velocity, type, actualDamage, knockback, player.whoAmI);
            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Channels a sweepable Condemnation Wave beam that widens over time"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Every 7th cast: Page Turn — beam deals +30% damage"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Right-click: Dark Sermon — summons a ritual circle that detonates after 3 seconds"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Kills power next cast (+5% per condemned, max +50%)"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Every name written in this book burns twice — once on the page, once in flesh.'")
            {
                OverrideColor = new Color(200, 50, 30)
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

            // Switch to additive blend for glow layers
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            DiesIraePalette.DrawItemBloom(spriteBatch, tex, pos, origin, rotation, scale, pulse);

            // Restore alpha blend for vanilla drawing
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Lighting.AddLight(Item.Center, DiesIraePalette.InfernalRed.ToVector3() * 0.35f);
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
            Color glowColor = Color.Lerp(DiesIraePalette.InfernalRed, DiesIraePalette.JudgmentGold, cycle) * 0.24f;
            spriteBatch.Draw(tex, position, frame, glowColor with { A = 0 }, 0f, origin, scale * pulse * 1.1f, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);

            spriteBatch.Draw(tex, position, frame, drawColor, 0f, origin, scale, SpriteEffects.None, 0f);
            return false;
        }
    }
}