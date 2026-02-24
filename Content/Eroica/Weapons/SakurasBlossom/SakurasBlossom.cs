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
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.Eroica.ResonantWeapons
{
    /// <summary>
    /// Sakura's Blossom — Eroica endgame melee weapon using held-projectile swing system.
    /// Each swing blooms with petals — a 4-phase sakura combo that escalates spectral copies.
    /// Drops from Eroica, God of Valor (no recipe).
    /// </summary>
    public class SakurasBlossom : MeleeSwingItemBase
    {
        #region Theme Colors

        private static readonly Color EroicaScarlet = new Color(200, 50, 50);
        private static readonly Color EroicaGold = new Color(255, 215, 0);
        private static readonly Color SakuraPink = new Color(255, 150, 180);

        #endregion

        #region Abstract Overrides (MeleeSwingItemBase)

        protected override int SwingProjectileType => ModContent.ProjectileType<SakurasBlossomSwing>();
        protected override int ComboStepCount => 4;

        #endregion

        #region Virtual Overrides

        protected override Color GetLoreColor() => new Color(200, 50, 50);

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
                "4-phase sakura combo spawns escalating spectral copies")
            { OverrideColor = SakuraPink });
            tooltips.Add(new TooltipLine(Mod, "SeekingCrystals",
                "Hits have a chance to unleash seeking valor crystals")
            { OverrideColor = EroicaGold });
            tooltips.Add(new TooltipLine(Mod, "Lore",
                "'Petals fall, heroes rise'")
            { OverrideColor = GetLoreColor() });
        }

        #endregion

        #region HoldItem — Ambient Sakura Aura

        public override void HoldItem(Player player)
        {
            base.HoldItem(player);

            if (Main.gameMenu) return;

            // Eroica ambient aura
            UnifiedVFX.Eroica.Aura(player.Center, 32f, 0.3f);

            // Subtle sakura orbiting flare
            if (Main.rand.NextBool(15))
            {
                float angle = Main.GameUpdateCount * 0.03f;
                float radius = 30f + (float)Math.Sin(Main.GameUpdateCount * 0.05f) * 10f;
                Vector2 flarePos = player.Center + angle.ToRotationVector2() * radius;
                Color fractalColor = Color.Lerp(SakuraPink, EroicaGold, 0.4f);
                CustomParticles.GenericFlare(flarePos, fractalColor, 0.25f, 14);
            }

            // Sakura petals floating
            if (Main.rand.NextBool(12))
            {
                ThemedParticles.SakuraPetals(player.Center + Main.rand.NextVector2Circular(25f, 25f), 1, 20f);
            }

            // Prismatic sparkle
            if (Main.rand.NextBool(15))
            {
                CustomParticles.PrismaticSparkle(player.Center + Main.rand.NextVector2Circular(25f, 25f), SakuraPink, 0.2f);
            }

            // Heroic gradient light with pulse
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.06f) * 0.1f + 0.9f;
            Vector3 lightColor = Color.Lerp(EroicaScarlet, EroicaGold, 0.4f).ToVector3();
            Lighting.AddLight(player.Center, lightColor * pulse * 0.55f);
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

            // Outer deep scarlet aura
            spriteBatch.Draw(texture, position, null, new Color(180, 40, 50) * 0.45f,
                rotation, origin, scale * pulse * 1.38f, SpriteEffects.None, 0f);
            // Middle crimson/pink glow
            spriteBatch.Draw(texture, position, null, new Color(255, 100, 120) * 0.35f,
                rotation, origin, scale * pulse * 1.2f, SpriteEffects.None, 0f);
            // Inner golden/white glow
            spriteBatch.Draw(texture, position, null, new Color(255, 230, 180) * 0.25f,
                rotation, origin, scale * pulse * 1.08f, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Lighting.AddLight(Item.Center, 0.65f, 0.35f, 0.3f);

            return true;
        }

        #endregion

        // No recipe — drops from Eroica, God of Valor
    }
}
