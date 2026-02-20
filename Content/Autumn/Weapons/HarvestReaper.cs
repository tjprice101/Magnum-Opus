using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;
using MagnumOpus.Common.BaseClasses;
using MagnumOpus.Content.Autumn.Materials;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using static MagnumOpus.Common.Systems.ThemedParticles;

namespace MagnumOpus.Content.Autumn.Weapons
{
    /// <summary>
    /// Harvest Reaper — Autumn-themed melee weapon (Post-Plantera tier).
    /// A massive scythe channeling autumn's decay, using held-projectile combo architecture.
    /// </summary>
    public class HarvestReaper : MeleeSwingItemBase
    {
        #region ── Theme Colors ──

        private static readonly Color AutumnOrange = new Color(255, 140, 50);
        private static readonly Color AutumnBrown = new Color(139, 90, 43);
        private static readonly Color AutumnRed = new Color(178, 34, 34);
        private static readonly Color AutumnGold = new Color(218, 165, 32);
        private static readonly Color DecayPurple = new Color(100, 50, 120);

        #endregion

        #region ── Abstract Overrides (MeleeSwingItemBase) ──

        protected override int SwingProjectileType => ModContent.ProjectileType<HarvestReaperSwing>();
        protected override int ComboStepCount => 4;

        #endregion

        #region ── Virtual Overrides ──

        protected override Color GetLoreColor() => Color.Lerp(AutumnOrange, AutumnBrown, 0.5f);

        protected override void SetWeaponDefaults()
        {
            Item.width = 70;
            Item.height = 70;
            Item.damage = 145;
            Item.DamageType = DamageClass.MeleeNoSpeed;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.knockBack = 7f;
            Item.value = Item.buyPrice(gold: 35);
            Item.rare = ItemRarityID.Lime;
            Item.UseSound = SoundID.Item71;
        }

        protected override void AddWeaponTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "ReapingStrike", "Large sweeping arcs with decay particles") { OverrideColor = AutumnOrange });
            tooltips.Add(new TooltipLine(Mod, "SoulHarvest", "Kills generate soul wisps that heal player") { OverrideColor = DecayPurple });
            tooltips.Add(new TooltipLine(Mod, "AutumnsDecay", "Every 5th hit applies stacking decay debuff") { OverrideColor = AutumnRed });
            tooltips.Add(new TooltipLine(Mod, "TwilightSlash", "Every 8th swing unleashes a massive crescent wave") { OverrideColor = AutumnGold });
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The harvest moon's final reaping'") { OverrideColor = GetLoreColor() });
        }

        #endregion

        #region ── HoldItem — Ambient VFX ──

        public override void HoldItem(Player player)
        {
            base.HoldItem(player);

            if (Main.gameMenu) return;

            // Decay leaf particles — drifting autumn leaves
            if (Main.rand.NextBool(25))
            {
                Vector2 offset = Main.rand.NextVector2Circular(20f, 20f);
                Vector2 vel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(0.3f, 0.8f));
                Color leafColor = Color.Lerp(AutumnOrange, AutumnBrown, Main.rand.NextFloat());
                var leaf = new GenericGlowParticle(player.Center + offset, vel, leafColor * 0.6f, 0.25f, 40, true);
                MagnumParticleHandler.SpawnParticle(leaf);
            }

            // Ambient pulse lighting
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.04f) * 0.1f + 0.5f;
            Lighting.AddLight(player.Center, AutumnOrange.ToVector3() * pulse * 0.5f);
        }

        #endregion

        #region ── PreDrawInWorld — Item Glow ──

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D texture = TextureAssets.Item[Item.type].Value;
            Vector2 position = Item.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;

            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.04f) * 0.12f + 1f;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            spriteBatch.Draw(texture, position, null, AutumnOrange * 0.4f, rotation, origin, scale * pulse * 1.4f, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, position, null, AutumnGold * 0.3f, rotation, origin, scale * pulse * 1.2f, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, position, null, AutumnRed * 0.25f, rotation, origin, scale * 1.05f, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Lighting.AddLight(Item.Center, AutumnOrange.ToVector3() * 0.5f);

            return true;
        }

        #endregion

        #region ── Recipe ──

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<HarvestBar>(), 18)
                .AddIngredient(ModContent.ItemType<AutumnResonantEnergy>(), 1)
                .AddIngredient(ItemID.SoulofFright, 8)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }

        #endregion
    }
}
