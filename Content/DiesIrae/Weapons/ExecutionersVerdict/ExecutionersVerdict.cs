using MagnumOpus.Common;
using MagnumOpus.Common.BaseClasses;
using MagnumOpus.Content.DiesIrae.Weapons.ExecutionersVerdict.Projectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.DiesIrae.Weapons.ExecutionersVerdict
{
    /// <summary>
    /// EXECUTIONER'S VERDICT — The Judicial Greatsword.
    /// Methodical heavy strikes that apply Judgment Marks.
    /// 3-phase combo: Arraignment → Cross-Examination → The Verdict.
    /// At 3 marks: Verdict Execution trigger.
    /// 
    /// Extends MeleeSwingItemBase for proper held-projectile swing behavior
    /// with combo tracking and post-swing stasis gating.
    /// </summary>
    public class ExecutionersVerdict : MeleeSwingItemBase
    {
        protected override int SwingProjectileType => ModContent.ProjectileType<ExecutionersVerdictSwing>();
        protected override int ComboStepCount => 3;
        protected override int ComboResetDelay => 50;

        protected override Color GetLoreColor() => new Color(200, 50, 30);

        protected override void SetWeaponDefaults()
        {
            Item.width = 80;
            Item.height = 80;
            Item.damage = 2400; // Tier 8 (1600-2400 range), slow heavy hitter
            Item.useTime = 32;
            Item.useAnimation = 32;
            Item.knockBack = 12f;
            Item.value = Item.sellPrice(platinum: 2, gold: 50);
            Item.rare = ModContent.RarityType<DiesIraeRarity>();
            Item.crit = 25;
            Item.scale = 1.8f;
        }

        protected override void AddWeaponTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "3-phase judgment combo applies Judgment Marks"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Marked enemies take increased damage per mark"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "At 3 marks: triggers Verdict Execution"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Executes non-boss enemies below 15% health instantly"));
            tooltips.Add(new TooltipLine(Mod, "Effect5", "Deals 50% more damage to enemies below 30% health"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The verdict was written before you were born'")
            {
                OverrideColor = new Color(200, 50, 30)
            });
        }

        protected override void OnShoot(Player player, int projectileIndex)
        {
            // Spawn 3 verdict bolts per swing that track enemies
            int boltCount = 3;
            for (int i = 0; i < boltCount; i++)
            {
                float spread = MathHelper.ToRadians(15f) * (i - 1);
                Vector2 aim = player.MountedCenter.DirectionTo(Main.MouseWorld).RotatedBy(spread);
                Projectile.NewProjectile(player.GetSource_ItemUse(player.HeldItem),
                    player.MountedCenter, aim * 12f,
                    ModContent.ProjectileType<VerdictBolt>(),
                    (int)(Item.damage * 0.35f), Item.knockBack * 0.3f,
                    player.whoAmI);
            }
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