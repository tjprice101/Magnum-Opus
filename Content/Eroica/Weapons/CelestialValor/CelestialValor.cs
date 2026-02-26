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
using MagnumOpus.Common.Systems.Particles;
using static MagnumOpus.Common.Systems.Particles.Particle;

namespace MagnumOpus.Content.Eroica.Weapons.CelestialValor
{
    /// <summary>
    /// Celestial Valor — "The Hero's Burning Oath"
    /// 
    /// An Eroica-themed endgame melee greatsword forged from the crystallized
    /// valor of countless fallen heroes. Uses a held-projectile combo system
    /// with a 3-hit escalating combo that crescendos from whisper to war cry.
    /// 
    /// Ambient VFX: When held, the blade radiates a heroic aura of scarlet
    /// embers and gold sparks, with drifting sakura petals and occasional
    /// music notes that float upward like prayers.
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
                "Escalating 3-hit combo launches heroic energy slashes")
            { OverrideColor = EroicaPalette.Flame });
            tooltips.Add(new TooltipLine(Mod, "ValorCrystals",
                "Critical strikes unleash seeking valor crystals")
            { OverrideColor = EroicaPalette.Gold });
            tooltips.Add(new TooltipLine(Mod, "AOEBurst",
                "Projectiles detonate in fiery explosions that chain to nearby enemies")
            { OverrideColor = EroicaPalette.Scarlet });
            tooltips.Add(new TooltipLine(Mod, "Lore",
                "'Each swing carries the final words of heroes who fell with their oath unbroken'")
            { OverrideColor = GetLoreColor() });
        }

        #endregion

        #region ── HoldItem — Enhanced Ambient VFX ──

        public override void HoldItem(Player player)
        {
            base.HoldItem(player);

            if (Main.gameMenu) return;

            // ── Heroic aura — pulsing ring of rising embers ──
            EroicaVFXLibrary.SpawnHeroicAura(player.Center, 42f);

            // ── Ambient music notes — heroic scarlet/gold hue band ──
            if (Main.rand.NextBool(10))
            {
                EroicaVFXLibrary.SpawnMusicNotes(player.Center, 1, 22f);
            }

            // ── Sakura petal drift — gentle courage petals ──
            if (Main.rand.NextBool(20))
            {
                EroicaVFXLibrary.SpawnSakuraPetals(player.Center, 1, 32f);
            }

            // ── Valor sparkles — golden motes drifting upward ──
            if (Main.rand.NextBool(14))
            {
                Vector2 sparklePos = player.Center + Main.rand.NextVector2Circular(28f, 28f);
                EroicaVFXLibrary.SpawnValorSparkles(sparklePos, 1, 12f);
            }

            // ── GlowSpark particles — ambient heroic fire motes ──
            if (Main.rand.NextBool(16))
            {
                Vector2 offset = Main.rand.NextVector2Circular(30f, 30f);
                Vector2 vel = Vector2.UnitY * -Main.rand.NextFloat(0.5f, 1.5f);
                Color sparkColor = Color.Lerp(EroicaPalette.Scarlet, EroicaPalette.Gold, Main.rand.NextFloat());
                var spark = new GlowSparkParticle(player.Center + offset, vel,
                    sparkColor, Main.rand.NextFloat(0.2f, 0.4f), Main.rand.Next(20, 35));
                MagnumParticleHandler.SpawnParticle(spark);
            }

            // ── Pulsing heroic light — crimson ↔ gold oscillation ──
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.05f) * 0.15f + 0.55f;
            Color lightColor = Color.Lerp(EroicaPalette.BladeCrimson, EroicaPalette.Gold,
                (float)Math.Sin(Main.GameUpdateCount * 0.03f) * 0.5f + 0.5f);
            Lighting.AddLight(player.Center, lightColor.ToVector3() * pulse * 0.65f);
        }

        #endregion

        #region ── PreDrawInWorld — Enhanced Item Glow ──

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor,
            ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D texture = TextureAssets.Item[Item.type].Value;
            Vector2 position = Item.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;

            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.05f) * 0.1f + 1f;

            // Additive bloom pass
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            EroicaPalette.DrawItemBloom(spriteBatch, texture, position, origin, rotation, scale, pulse);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // Warm heroic light at item position
            Lighting.AddLight(Item.Center, EroicaPalette.Gold.ToVector3() * 0.55f);

            return true;
        }

        #endregion
    }
}
