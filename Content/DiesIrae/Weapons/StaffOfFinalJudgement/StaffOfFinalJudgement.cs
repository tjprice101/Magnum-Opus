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

namespace MagnumOpus.Content.DiesIrae.Weapons.StaffOfFinalJudgement
{
    public class StaffOfFinalJudgement : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 48;
            Item.height = 48;
            Item.damage = 1950;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 22;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 5f;
            Item.value = Item.sellPrice(platinum: 2);
            Item.rare = ModContent.RarityType<global::MagnumOpus.Common.DiesIraeRarity>();
            Item.UseSound = SoundID.Item117;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<Projectiles.FloatingIgnitionProjectile>();
            Item.shootSpeed = 4f;
            Item.crit = 18;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Places floating fire mines that auto-arm after 1 second"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Armed mines detonate when enemies approach within 3 tiles"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Adjacent mines chain-detonate with +30% bonus damage"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "3+ mines detonating within 1 second triggers Judgment Storm — massive fire rain"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Judgment does not chase. Judgment waits.'")
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