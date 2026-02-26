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
using MagnumOpus.Common.Systems.VFX.Core;
using MagnumOpus.Content.MoonlightSonata;
using MagnumOpus.Content.MoonlightSonata.Dusts;
using MagnumOpus.Content.MoonlightSonata.ResonanceEnergies;
using MagnumOpus.Content.MoonlightSonata.CraftingStations;
using MagnumOpus.Content.MoonlightSonata.Enemies;

namespace MagnumOpus.Content.MoonlightSonata.Weapons.EternalMoon
{
    /// <summary>
    /// EternalMoon — "The Eternal Tide".
    /// Moonlight Sonata melee weapon with 5-phase tidal lunar combo.
    /// Held-projectile combo system — each swing echoes the quiet sorrow of moonlight on water.
    /// </summary>
    public class EternalMoon : MeleeSwingItemBase
    {
        #region Abstract Overrides

        protected override int SwingProjectileType => ModContent.ProjectileType<EternalMoonSwing>();
        protected override int ComboStepCount => 5;

        #endregion

        #region Virtual Overrides

        protected override Color GetLoreColor() => new Color(140, 100, 200);

        protected override void SetWeaponDefaults()
        {
            Item.damage = 300;
            Item.DamageType = DamageClass.MeleeNoSpeed;
            Item.useTime = 18;
            Item.useAnimation = 18;
            Item.knockBack = 7f;
            Item.width = 50;
            Item.height = 50;
            Item.autoReuse = true;
            Item.value = Item.buyPrice(gold: 25);
            Item.rare = ModContent.RarityType<MoonlightSonataRarity>();
        }

        protected override void AddWeaponTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "TidalCombo",
                "5-phase Tidal Lunar Cycle — New Moon, Waxing, Half Moon, Waning, and Full Moon crescendo")
            { OverrideColor = MoonlightVFXLibrary.IceBlue });
            tooltips.Add(new TooltipLine(Mod, "TidalWash",
                "Swings unleash crescent waves and curved tidal wash arcs that escalate with each phase")
            { OverrideColor = EternalMoonVFX.TidalFoam });
            tooltips.Add(new TooltipLine(Mod, "GhostReflection",
                "Half Moon phase summons a ghost reflection swing — a spectral echo of the blade")
            { OverrideColor = MoonlightVFXLibrary.Violet });
            tooltips.Add(new TooltipLine(Mod, "Crescendo",
                "Full Moon finale: tidal detonation, homing moonlight beams, and radial wave burst")
            { OverrideColor = EternalMoonVFX.CrescentGlow });
            tooltips.Add(new TooltipLine(Mod, "SeekingCrystals",
                "Critical hits spawn seeking lunar crystals")
            { OverrideColor = MoonlightVFXLibrary.Silver });
            tooltips.Add(new TooltipLine(Mod, "TidalAura",
                "Tidal moonlight aura while held")
            { OverrideColor = EternalMoonVFX.TidalFoam });
            tooltips.Add(new TooltipLine(Mod, "Lore",
                "'The eternal cycle made blade — each swing echoes moonlight on water'")
            { OverrideColor = GetLoreColor() });
        }

        #endregion

        #region HoldItem — Tidal Moonlight Aura

        public override void HoldItem(Player player)
        {
            base.HoldItem(player);

            if (Main.dedServ) return;

            // Orbiting TidalMoonDust — 3 flowing water motes circling the player
            if (Main.rand.NextBool(7))
            {
                float angle = Main.GameUpdateCount * 0.03f;
                for (int i = 0; i < 3; i++)
                {
                    float orbitAngle = angle + MathHelper.TwoPi * i / 3f;
                    float radius = 30f + MathF.Sin(Main.GameUpdateCount * 0.05f + i * 0.8f) * 8f;
                    Vector2 tidalPos = player.Center + orbitAngle.ToRotationVector2() * radius;
                    float progress = (float)i / 3f;
                    Color tidalColor = Color.Lerp(EternalMoonVFX.DeepTide,
                        EternalMoonVFX.TidalFoam, progress);
                    Dust tidal = Dust.NewDustPerfect(tidalPos,
                        ModContent.DustType<TidalMoonDust>(),
                        Vector2.Zero, 0, tidalColor, 0.2f);
                    tidal.customData = new TidalMoonBehavior
                    {
                        DriftAmplitude = 1.5f,
                        DriftFrequency = 0.12f,
                        VelocityDecay = 0.97f,
                        BaseScale = 0.2f,
                        Lifetime = 30
                    };
                }
            }

            // LunarMote crescent sparkles — 2 orbiting crescents
            if (Main.rand.NextBool(10))
            {
                float moteAngle = Main.GameUpdateCount * 0.04f;
                for (int i = 0; i < 2; i++)
                {
                    float orbitAngle = moteAngle + MathHelper.Pi * i;
                    float radius = 22f + MathF.Sin(Main.GameUpdateCount * 0.07f + i * 1.2f) * 5f;
                    Vector2 motePos = player.Center + orbitAngle.ToRotationVector2() * radius;
                    Color moteColor = Color.Lerp(MoonlightVFXLibrary.Violet,
                        MoonlightVFXLibrary.IceBlue, (float)i / 2f);
                    Dust mote = Dust.NewDustPerfect(motePos,
                        ModContent.DustType<LunarMote>(),
                        Vector2.Zero, 0, moteColor, 0.22f);
                    mote.customData = new LunarMoteBehavior(player.Center, orbitAngle)
                    {
                        OrbitRadius = radius,
                        OrbitSpeed = 0.04f,
                        Lifetime = 25,
                        FadePower = 0.92f
                    };
                }
            }

            // StarPointDust twinkles — ambient tidal sparkles
            if (Main.rand.NextBool(14))
            {
                Vector2 sparkleOffset = Main.rand.NextVector2Circular(25f, 25f);
                Color starColor = Color.Lerp(EternalMoonVFX.CrescentGlow,
                    EternalMoonVFX.TidalFoam, Main.rand.NextFloat());
                Dust star = Dust.NewDustPerfect(player.Center + sparkleOffset,
                    ModContent.DustType<StarPointDust>(),
                    Vector2.Zero, 0, starColor, 0.18f);
                star.customData = new StarPointBehavior
                {
                    RotationSpeed = 0.08f,
                    TwinkleFrequency = 0.35f,
                    Lifetime = 28,
                    FadeStartTime = 8
                };
            }

            // Pulsing tidal moonlight glow
            float pulse = 0.6f + MathF.Sin(Main.GameUpdateCount * 0.08f) * 0.15f;
            Lighting.AddLight(player.Center, MoonlightVFXLibrary.Violet.ToVector3() * pulse * 0.4f);
        }

        #endregion

        #region PreDrawInWorld — Tidal Moonlight Bloom

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor,
            ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D texture = TextureAssets.Item[Item.type].Value;
            Vector2 drawPos = Item.Center - Main.screenPosition;
            Vector2 origin = texture.Size() * 0.5f;
            float pulse = 1f + MathF.Sin(Main.GameUpdateCount * 0.06f) * 0.08f;

            // 5-layer bloom using {A=0} premultiplied alpha trick

            // Layer 1: Outer deep tide halo
            spriteBatch.Draw(texture, drawPos, null,
                (EternalMoonVFX.DeepTide with { A = 0 }) * 0.2f, rotation, origin,
                scale * 1.4f * pulse, SpriteEffects.None, 0f);

            // Layer 2: DarkPurple mid halo
            spriteBatch.Draw(texture, drawPos, null,
                (MoonlightVFXLibrary.DarkPurple with { A = 0 }) * 0.3f, rotation, origin,
                scale * 1.25f * pulse, SpriteEffects.None, 0f);

            // Layer 3: Violet glow
            spriteBatch.Draw(texture, drawPos, null,
                (MoonlightVFXLibrary.Violet with { A = 0 }) * 0.35f, rotation, origin,
                scale * 1.12f * pulse, SpriteEffects.None, 0f);

            // Layer 4: Ice blue inner
            spriteBatch.Draw(texture, drawPos, null,
                (MoonlightVFXLibrary.IceBlue with { A = 0 }) * 0.4f, rotation, origin,
                scale * 1.05f * pulse, SpriteEffects.None, 0f);

            // Layer 5: White-hot core
            spriteBatch.Draw(texture, drawPos, null,
                (Color.White with { A = 0 }) * 0.25f, rotation, origin,
                scale * pulse, SpriteEffects.None, 0f);

            Lighting.AddLight(Item.Center, MoonlightVFXLibrary.Violet.ToVector3() * 0.5f);

            return true;
        }

        #endregion

        #region Recipe

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<MoonlightsResonantEnergy>(), 30)
                .AddIngredient(ModContent.ItemType<ResonantCoreOfMoonlightSonata>(), 5)
                .AddIngredient(ModContent.ItemType<ShardsOfMoonlitTempo>(), 10)
                .AddTile(ModContent.TileType<MoonlightAnvilTile>())
                .Register();
        }

        #endregion
    }
}
