using MagnumOpus.Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using System;
using Terraria;
using Terraria.GameContent;
namespace MagnumOpus.Content.DiesIrae.Weapons.ChainOfJudgment
{
    public class ChainOfJudgment : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 50;
            Item.height = 50;
            Item.damage = 2400;
            Item.DamageType = DamageClass.Melee;
            Item.useTime = 22;
            Item.useAnimation = 22;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 6f;
            Item.value = Item.sellPrice(platinum: 2, gold: 50);
            Item.rare = ModContent.RarityType<global::MagnumOpus.Common.DiesIraeRarity>();
            Item.UseSound = SoundID.Item153;
            Item.autoReuse = true;
            Item.useTurn = false;
            Item.scale = 1.3f;
            Item.crit = 15;
            Item.shoot = ModContent.ProjectileType<Projectiles.JudgmentChainProjectile>();
            Item.shootSpeed = 16f;
            Item.noMelee = true;
            Item.noUseGraphic = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Hurls a blazing chain whip with 12-tile reach"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Hits build Chain Link stacks — at 5 stacks, enemies become Fully Bound"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Every 5th hit triggers chain lightning that arcs between nearby foes"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'No sinner escapes the chain. It finds them in the dark.'")
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