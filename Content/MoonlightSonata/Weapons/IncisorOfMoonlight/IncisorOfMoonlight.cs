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
using MagnumOpus.Content.MoonlightSonata.ResonanceEnergies;
using MagnumOpus.Content.MoonlightSonata.CraftingStations;

namespace MagnumOpus.Content.MoonlightSonata.Weapons.IncisorOfMoonlight
{
    /// <summary>
    /// Incisor of Moonlight — "The Stellar Scalpel".
    /// Moonlight Sonata endgame melee weapon using held-projectile swing system.
    /// A crescent blade forged from crystallized moonlight — 4-phase lunar combo
    /// with escalating wave projectiles. Precise, surgical, constellation-themed.
    /// </summary>
    public class IncisorOfMoonlight : MeleeSwingItemBase
    {
        #region Abstract Overrides

        protected override int SwingProjectileType => ModContent.ProjectileType<IncisorOfMoonlightSwing>();
        protected override int ComboStepCount => 4;

        #endregion

        #region Virtual Overrides

        protected override Color GetLoreColor() => new Color(140, 100, 200);

        protected override void SetWeaponDefaults()
        {
            Item.width = 60;
            Item.height = 60;
            Item.damage = 280;
            Item.DamageType = DamageClass.MeleeNoSpeed;
            Item.useTime = 12;
            Item.useAnimation = 12;
            Item.knockBack = 6.5f;
            Item.value = Item.sellPrice(gold: 25);
            Item.rare = ModContent.RarityType<EroicaRainbowRarity>();
            Item.UseSound = SoundID.Item1;
        }

        protected override void AddWeaponTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "LunarCombo",
                "4-phase lunar combo fires escalating crescent wave projectiles")
            { OverrideColor = MoonlightVFXLibrary.IceBlue });
            tooltips.Add(new TooltipLine(Mod, "SeekingCrystals",
                "Hits unleash seeking moonlight crystals on critical strikes")
            { OverrideColor = MoonlightVFXLibrary.Silver });
            tooltips.Add(new TooltipLine(Mod, "MoonlightAura",
                "Ethereal moonlight aura while held")
            { OverrideColor = MoonlightVFXLibrary.Violet });
            tooltips.Add(new TooltipLine(Mod, "Lore",
                "'A blade forged from crystallized moonlight'")
            { OverrideColor = GetLoreColor() });
        }

        #endregion

        #region Recipe

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<MoonlightsResonantEnergy>(), 20)
                .AddIngredient(ModContent.ItemType<ResonantCoreOfMoonlightSonata>(), 10)
                .AddIngredient(ModContent.ItemType<Enemies.ShardsOfMoonlitTempo>(), 25)
                .AddTile(ModContent.TileType<MoonlightAnvilTile>())
                .Register();
        }

        #endregion

        #region HoldItem — Resonant Aura

        public override void HoldItem(Player player)
        {
            base.HoldItem(player);

            if (Main.dedServ) return;

            // Orbiting silver motes — 3 tiny points circling the player (precision feel)
            if (Main.rand.NextBool(6))
            {
                float angle = Main.GameUpdateCount * 0.035f;
                for (int i = 0; i < 3; i++)
                {
                    float orbitAngle = angle + MathHelper.TwoPi * i / 3f;
                    float radius = 28f + MathF.Sin(Main.GameUpdateCount * 0.06f + i * 0.9f) * 6f;
                    Vector2 motePos = player.Center + orbitAngle.ToRotationVector2() * radius;
                    float progress = (float)i / 3f;
                    Color moteColor = Color.Lerp(MoonlightVFXLibrary.Violet,
                        IncisorOfMoonlightVFX.ResonantSilver, progress);
                    Dust d = Dust.NewDustPerfect(motePos, DustID.Enchanted_Pink,
                        Vector2.Zero, 0, moteColor * 0.5f, 0.5f);
                    d.noGravity = true;
                }
            }

            // Ambient sparkle (sparse — precision, not excess)
            if (Main.rand.NextBool(12))
            {
                Vector2 offset = Main.rand.NextVector2Circular(22f, 22f);
                Dust d = Dust.NewDustPerfect(player.Center + offset, DustID.PurpleTorch,
                    Vector2.Zero, 0, MoonlightVFXLibrary.Violet * 0.35f, 0.45f);
                d.noGravity = true;
            }

            // Pulsing moonlight glow
            float pulse = 0.55f + MathF.Sin(Main.GameUpdateCount * 0.07f) * 0.12f;
            Lighting.AddLight(player.Center, MoonlightVFXLibrary.Violet.ToVector3() * pulse * 0.35f);
        }

        #endregion

        #region PreDrawInWorld — Resonant Bloom

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor,
            ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D texture = TextureAssets.Item[Item.type].Value;
            Vector2 drawPos = Item.Center - Main.screenPosition;
            Vector2 origin = texture.Size() * 0.5f;
            float pulse = 1f + MathF.Sin(Main.GameUpdateCount * 0.04f) * 0.08f;

            // 4-layer bloom using {A=0} premultiplied alpha trick
            // Renders additively under AlphaBlend without SpriteBatch restart

            // Layer 1: Outer deep resonance halo
            spriteBatch.Draw(texture, drawPos, null,
                (IncisorOfMoonlightVFX.DeepResonance with { A = 0 }) * 0.3f, rotation, origin,
                scale * 1.35f * pulse, SpriteEffects.None, 0f);

            // Layer 2: Mid violet glow
            spriteBatch.Draw(texture, drawPos, null,
                (MoonlightVFXLibrary.Violet with { A = 0 }) * 0.35f, rotation, origin,
                scale * 1.18f * pulse, SpriteEffects.None, 0f);

            // Layer 3: Inner ice blue
            spriteBatch.Draw(texture, drawPos, null,
                (MoonlightVFXLibrary.IceBlue with { A = 0 }) * 0.4f, rotation, origin,
                scale * 1.06f * pulse, SpriteEffects.None, 0f);

            // Layer 4: White-hot core
            spriteBatch.Draw(texture, drawPos, null,
                (Color.White with { A = 0 }) * 0.25f, rotation, origin,
                scale * pulse, SpriteEffects.None, 0f);

            Lighting.AddLight(Item.Center, MoonlightVFXLibrary.Violet.ToVector3() * 0.5f);

            return true;
        }

        #endregion
    }
}
