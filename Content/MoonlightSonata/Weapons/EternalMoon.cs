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
using MagnumOpus.Content.MoonlightSonata.ResonanceEnergies;
using MagnumOpus.Content.MoonlightSonata.CraftingStations;
using MagnumOpus.Content.MoonlightSonata.Enemies;

namespace MagnumOpus.Content.MoonlightSonata.Weapons
{
    /// <summary>
    /// EternalMoon — Moonlight Sonata melee weapon.
    /// Held-projectile combo system with 3-phase lunar combo.
    /// The eternal cycle made blade — each swing echoes the quiet sorrow of moonlight.
    /// </summary>
    public class EternalMoon : MeleeSwingItemBase
    {
        #region Theme Colors

        private static readonly Color MoonlightPurple = new Color(138, 43, 226);
        private static readonly Color MoonlightBlue = new Color(135, 206, 250);
        private static readonly Color MoonlightSilver = new Color(220, 220, 235);

        #endregion

        #region Abstract Overrides

        protected override int SwingProjectileType => ModContent.ProjectileType<EternalMoonSwing>();
        protected override int ComboStepCount => 3;

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
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Each swing releases lunar wave projectiles"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Finale combo phase unleashes moonlight beams"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Critical hits spawn seeking lunar crystals"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The eternal cycle made blade'")
            {
                OverrideColor = GetLoreColor()
            });
        }

        #endregion

        #region HoldItem — Moonlight Aura

        public override void HoldItem(Player player)
        {
            base.HoldItem(player);

            if (Main.dedServ) return;

            // Ambient moonlight aura particles
            if (Main.rand.NextBool(6))
            {
                float angle = Main.GameUpdateCount * 0.03f;
                for (int i = 0; i < 3; i++)
                {
                    float orbitAngle = angle + MathHelper.TwoPi * i / 3f;
                    float radius = 30f + MathF.Sin(Main.GameUpdateCount * 0.05f + i * 0.8f) * 8f;
                    Vector2 flarePos = player.Center + orbitAngle.ToRotationVector2() * radius;
                    float progress = (float)i / 3f;
                    Color flareColor = Color.Lerp(MoonlightPurple, MoonlightBlue, progress);
                    Dust d = Dust.NewDustPerfect(flarePos, DustID.Enchanted_Pink,
                        Vector2.Zero, 0, flareColor * 0.6f, 0.6f);
                    d.noGravity = true;
                }
            }

            // Soft purple ambient sparkle
            if (Main.rand.NextBool(10))
            {
                Vector2 sparkleOffset = Main.rand.NextVector2Circular(25f, 25f);
                Dust d = Dust.NewDustPerfect(player.Center + sparkleOffset, DustID.PurpleTorch,
                    Vector2.Zero, 0, MoonlightPurple * 0.4f, 0.5f);
                d.noGravity = true;
            }

            // Pulsing moonlight glow
            float pulse = 0.6f + MathF.Sin(Main.GameUpdateCount * 0.08f) * 0.15f;
            Lighting.AddLight(player.Center, MoonlightPurple.ToVector3() * pulse * 0.4f);
        }

        #endregion

        #region PreDrawInWorld — Moonlight Glow

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D texture = TextureAssets.Item[Item.type].Value;
            Vector2 drawPos = Item.Center - Main.screenPosition;
            Vector2 origin = texture.Size() * 0.5f;
            float pulse = 1f + MathF.Sin(Main.GameUpdateCount * 0.06f) * 0.08f;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // Outer purple glow
            Color outerGlow = MoonlightPurple with { A = 0 };
            spriteBatch.Draw(texture, drawPos, null, outerGlow * 0.25f, rotation, origin,
                scale * 1.3f * pulse, SpriteEffects.None, 0f);

            // Mid blue glow
            Color midGlow = MoonlightBlue with { A = 0 };
            spriteBatch.Draw(texture, drawPos, null, midGlow * 0.35f, rotation, origin,
                scale * 1.15f * pulse, SpriteEffects.None, 0f);

            // Inner silver glow
            Color innerGlow = MoonlightSilver with { A = 0 };
            spriteBatch.Draw(texture, drawPos, null, innerGlow * 0.4f, rotation, origin,
                scale * 1.05f * pulse, SpriteEffects.None, 0f);

            // White-hot core
            spriteBatch.Draw(texture, drawPos, null, Color.White with { A = 0 } * 0.3f, rotation, origin,
                scale * pulse, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

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
