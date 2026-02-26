using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;
using MagnumOpus.Common;
using MagnumOpus.Common.BaseClasses;

namespace MagnumOpus.Content.Eroica.Weapons.SakurasBlossom
{
    /// <summary>
    /// Sakura's Blossom — Eroica endgame melee weapon using held-projectile swing system.
    /// Each swing blooms with petals — a 4-phase sakura combo that escalates spectral copies.
    /// Drops from Eroica, God of Valor (no recipe).
    /// </summary>
    public class SakurasBlossom : MeleeSwingItemBase
    {
        #region Abstract Overrides (MeleeSwingItemBase)

        protected override int SwingProjectileType => ModContent.ProjectileType<SakurasBlossomSwing>();
        protected override int ComboStepCount => 4;

        #endregion

        #region Virtual Overrides

        protected override Color GetLoreColor() => EroicaPalette.Scarlet;

        protected override void SetWeaponDefaults()
        {
            Item.width = 70;
            Item.height = 70;
            Item.damage = 350;
            Item.DamageType = DamageClass.MeleeNoSpeed;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.knockBack = 8f;
            Item.scale = 1.3f;
            Item.value = Item.sellPrice(gold: 40);
            Item.rare = ModContent.RarityType<EroicaRainbowRarity>();
            Item.UseSound = SoundID.Item1;
        }

        protected override void AddWeaponTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "SakuraCombo",
                "4-phase sakura combo spawns escalating spectral blade copies")
            { OverrideColor = EroicaPalette.Sakura });
            tooltips.Add(new TooltipLine(Mod, "Spectral",
                "Spectral copies home to enemies and scatter petal bursts on impact")
            { OverrideColor = new Color(255, 180, 200) });
            tooltips.Add(new TooltipLine(Mod, "SeekingCrystals",
                "Hits have a chance to unleash seeking valor crystals")
            { OverrideColor = EroicaPalette.Gold });
            tooltips.Add(new TooltipLine(Mod, "Lore",
                "'Each petal carries the memory of a hero who chose beauty over survival'")
            { OverrideColor = GetLoreColor() });
        }

        #endregion

        #region HoldItem — Ambient Sakura Aura

        public override void HoldItem(Player player)
        {
            base.HoldItem(player);

            if (Main.gameMenu) return;

            SakurasBlossomVFX.HoldItemVFX(player);
        }

        #endregion

        #region PreDrawInWorld — Item Glow

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor,
            ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D texture = TextureAssets.Item[Item.type].Value;
            Vector2 position = Item.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;

            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.055f) * 0.12f + 1f;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            EroicaPalette.DrawItemBloom(spriteBatch, texture, position, origin, rotation, scale, pulse);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Lighting.AddLight(Item.Center, EroicaPalette.Scarlet.ToVector3() * 0.6f);

            return true;
        }

        #endregion

        // No recipe — drops from Eroica, God of Valor
    }
}
