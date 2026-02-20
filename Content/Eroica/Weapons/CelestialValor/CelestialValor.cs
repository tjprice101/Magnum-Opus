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
using MagnumOpus.Content.Eroica;
using static MagnumOpus.Common.Systems.ThemedParticles;

namespace MagnumOpus.Content.Eroica.Weapons.CelestialValor
{
    /// <summary>
    /// Celestial Valor — Eroica-themed endgame melee weapon.
    /// A blade forged from the valor of countless heroes, using held-projectile combo architecture.
    /// 3-hit escalating combo that fires increasing numbers of heroic projectiles.
    /// </summary>
    public class CelestialValor : MeleeSwingItemBase
    {
        #region ── Abstract Overrides (MeleeSwingItemBase) ──

        protected override int SwingProjectileType => ModContent.ProjectileType<CelestialValorSwing>();
        protected override int ComboStepCount => 3;

        #endregion

        #region ── Virtual Overrides ──

        protected override Color GetLoreColor() => EroicaPalette.EffectTooltip;

        protected override void SetWeaponDefaults()
        {
            Item.width = 80;
            Item.height = 80;
            Item.damage = 320;
            Item.DamageType = DamageClass.MeleeNoSpeed;
            Item.useTime = 22;
            Item.useAnimation = 22;
            Item.knockBack = 7.5f;
            Item.scale = 1.3f;
            Item.value = Item.sellPrice(gold: 45);
            Item.rare = ModContent.RarityType<EroicaRainbowRarity>();
            Item.UseSound = SoundID.Item1;
        }

        protected override void AddWeaponTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "HeroicCombo",
                "Escalating 3-hit combo fires heroic projectiles")
            { OverrideColor = new Color(255, 150, 100) });
            tooltips.Add(new TooltipLine(Mod, "ValorCrystals",
                "Critical strikes unleash seeking valor crystals")
            { OverrideColor = EroicaPalette.Gold });
            tooltips.Add(new TooltipLine(Mod, "Lore",
                "'A blade forged from the valor of countless heroes'")
            { OverrideColor = GetLoreColor() });
        }

        #endregion

        #region ── HoldItem — Ambient VFX ──

        public override void HoldItem(Player player)
        {
            base.HoldItem(player);

            if (Main.gameMenu) return;

            // Eroica heroic aura
            UnifiedVFX.Eroica.Aura(player.Center, 40f, 0.35f);

            // Ambient scarlet/gold flares
            if (Main.rand.NextBool(12))
            {
                Vector2 offset = Main.rand.NextVector2Circular(20f, 20f);
                Color flareColor = Main.rand.NextBool() ? EroicaPalette.Scarlet : EroicaPalette.Gold;
                CustomParticles.GenericFlare(player.Center + offset, flareColor, 0.25f, 15);
            }

            // Sakura petal drift
            if (Main.rand.NextBool(25))
            {
                ThemedParticles.SakuraPetals(player.Center, 1, 30f);
            }

            // Prismatic gold sparkle
            if (Main.rand.NextBool(18))
            {
                Vector2 sparkleOffset = Main.rand.NextVector2Circular(25f, 25f);
                CustomParticles.PrismaticSparkle(player.Center + sparkleOffset, EroicaGold, 0.22f);
            }

            // Pulsing heroic light — crimson to gold
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.05f) * 0.15f + 0.55f;
            Color lightColor = Color.Lerp(EroicaPalette.BladeCrimson, EroicaPalette.Gold, (float)Math.Sin(Main.GameUpdateCount * 0.03f) * 0.5f + 0.5f);
            Lighting.AddLight(player.Center, lightColor.ToVector3() * pulse * 0.6f);
        }

        #endregion

        #region ── PreDrawInWorld — Item Glow ──

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor,
            ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D texture = TextureAssets.Item[Item.type].Value;
            Vector2 position = Item.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;

            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.05f) * 0.1f + 1f;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            EroicaPalette.DrawItemBloom(spriteBatch, texture, position, origin, rotation, scale, pulse);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Lighting.AddLight(Item.Center, EroicaPalette.Gold.ToVector3() * 0.5f);

            return true;
        }

        #endregion
    }
}
