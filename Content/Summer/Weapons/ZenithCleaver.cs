using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;
using MagnumOpus.Common.BaseClasses;
using MagnumOpus.Content.Summer.Materials;
using MagnumOpus.Common.Systems.Particles;
using static MagnumOpus.Common.Systems.ThemedParticles;

namespace MagnumOpus.Content.Summer.Weapons
{
    /// <summary>
    /// Zenith Cleaver - Summer-themed broadsword (Post-Mechs tier)
    /// A blazing blade that channels the power of the midsummer sun.
    /// Now uses the held-projectile swing system via MeleeSwingItemBase.
    /// </summary>
    public class ZenithCleaver : MeleeSwingItemBase
    {
        #region Theme Colors

        private static readonly Color SunGold = new Color(255, 215, 0);
        private static readonly Color SunOrange = new Color(255, 140, 0);
        private static readonly Color SunWhite = new Color(255, 250, 240);
        private static readonly Color SunRed = new Color(255, 100, 50);

        #endregion

        #region Abstract Overrides

        protected override int SwingProjectileType => ModContent.ProjectileType<ZenithCleaverSwing>();
        protected override int ComboStepCount => 3;

        protected override Color GetLoreColor() => Color.Lerp(SunGold, SunOrange, 0.5f);

        #endregion

        #region Weapon Setup

        protected override void SetWeaponDefaults()
        {
            Item.width = 60;
            Item.height = 60;
            Item.damage = 115;
            Item.DamageType = DamageClass.MeleeNoSpeed;
            Item.useTime = 22;
            Item.useAnimation = 22;
            Item.knockBack = 6.5f;
            Item.value = Item.buyPrice(gold: 25);
            Item.rare = ItemRarityID.Pink;
            Item.UseSound = SoundID.Item1;
        }

        #endregion

        #region Tooltips

        protected override void AddWeaponTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "SolarRadiance", "Swings emit radiant energy waves") { OverrideColor = SunGold });
            tooltips.Add(new TooltipLine(Mod, "Sunstroke", "Enemies are afflicted with intense burning") { OverrideColor = SunOrange });
            tooltips.Add(new TooltipLine(Mod, "ZenithStrike", "Every 3rd combo finisher unleashes a massive solar flare") { OverrideColor = SunRed });
            tooltips.Add(new TooltipLine(Mod, "HeatMirage", "Daytime grants +15% melee critical strike chance") { OverrideColor = SunWhite });
        }

        #endregion

        #region Hold Effects

        public override void HoldItem(Player player)
        {
            base.HoldItem(player);

            // Heat Mirage: Daytime bonuses
            if (Main.dayTime)
            {
                player.GetCritChance(DamageClass.Melee) += 15;
            }

            // Sparse heat shimmer — subtle ambient effect
            if (Main.rand.NextBool(25))
            {
                Vector2 shimmerPos = player.Center + new Vector2(Main.rand.NextFloat(-25f, 25f), Main.rand.NextFloat(8f, 20f));
                Vector2 shimmerVel = new Vector2(0, -Main.rand.NextFloat(0.5f, 1.2f));
                var shimmer = new GenericGlowParticle(shimmerPos, shimmerVel, SunOrange * 0.3f, 0.15f, 22, true);
                MagnumParticleHandler.SpawnParticle(shimmer);
            }

            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.06f) * 0.1f + 0.4f;
            Lighting.AddLight(player.Center, SunGold.ToVector3() * pulse);
        }

        #endregion

        #region World Drawing

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D texture = TextureAssets.Item[Item.type].Value;
            Vector2 position = Item.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;

            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.055f) * 0.15f + 1f;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            spriteBatch.Draw(texture, position, null, SunGold * 0.4f, rotation, origin, scale * pulse * 1.4f, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, position, null, SunOrange * 0.35f, rotation, origin, scale * pulse * 1.2f, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, position, null, SunRed * 0.25f, rotation, origin, scale * 1.08f, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Lighting.AddLight(Item.Center, SunGold.ToVector3() * 0.6f);

            return true;
        }

        #endregion

        #region Recipes

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<SolsticeBar>(), 16)
                .AddIngredient(ModContent.ItemType<SummerResonantEnergy>(), 1)
                .AddIngredient(ItemID.SoulofMight, 8)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }

        #endregion
    }
}
