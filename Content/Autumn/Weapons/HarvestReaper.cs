using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Content.Autumn.Materials;
using static MagnumOpus.Common.Systems.ThemedParticles;

namespace MagnumOpus.Content.Autumn.Weapons
{
    /// <summary>
    /// Harvest Reaper — Autumn-themed melee weapon (Post-Wall of Flesh tier).
    /// A massive scythe channeling autumn's decay, using held-projectile combo architecture.
    /// </summary>
    public class HarvestReaper : ModItem
    {
        #region ── Theme Colors ──

        private static readonly Color AutumnOrange = new Color(255, 140, 50);
        private static readonly Color AutumnBrown = new Color(139, 90, 43);
        private static readonly Color AutumnRed = new Color(178, 34, 34);
        private static readonly Color AutumnGold = new Color(218, 165, 32);
        private static readonly Color DecayPurple = new Color(100, 50, 120);

        #endregion

        public override void SetDefaults()
        {
            Item.width = 70;
            Item.height = 70;
            Item.damage = 145;
            Item.DamageType = DamageClass.MeleeNoSpeed;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 7f;
            Item.value = Item.buyPrice(gold: 35);
            Item.rare = ItemRarityID.Lime;
            Item.UseSound = null;
            Item.autoReuse = true;
            Item.channel = true;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.shoot = ModContent.ProjectileType<HarvestReaperSwing>();
            Item.shootSpeed = 1f;
        }

        public override bool CanShoot(Player player)
        {
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (p.active && p.owner == player.whoAmI && p.type == Item.shoot)
                    return false;
            }
            return true;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,
            Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(source, player.MountedCenter,
                (Main.MouseWorld - player.MountedCenter).SafeNormalize(Vector2.UnitX),
                type, damage, knockback, player.whoAmI, 0f, 0);
            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "ReapingStrike", "Large sweeping arcs with decay particles") { OverrideColor = AutumnOrange });
            tooltips.Add(new TooltipLine(Mod, "SoulHarvest", "Kill enemies to spawn soul wisps that restore 12 HP when picked up") { OverrideColor = DecayPurple });
            tooltips.Add(new TooltipLine(Mod, "AutumnsDecay", "Every 5th hit applies stacking decay debuff") { OverrideColor = AutumnRed });
            tooltips.Add(new TooltipLine(Mod, "TwilightSlash", "Every 8th swing unleashes a massive crescent wave") { OverrideColor = AutumnGold });
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The harvest moon's final reaping'") { OverrideColor = Color.Lerp(AutumnOrange, AutumnBrown, 0.5f) });
        }

        #region ── HoldItem — Ambient VFX ──

        public override void HoldItem(Player player)
        {

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
            try
            {
                Texture2D bloomTex = MagnumTextureRegistry.GetSoftGlow();
                Vector2 position = Item.Center - Main.screenPosition;
                Vector2 bloomOrigin = bloomTex.Size() / 2f;

                float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.04f) * 0.12f + 1f;

                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                spriteBatch.Draw(bloomTex, position, null, AutumnOrange * 0.35f, 0f, bloomOrigin, scale * pulse * 0.15f, SpriteEffects.None, 0f);
                spriteBatch.Draw(bloomTex, position, null, AutumnGold * 0.25f, 0f, bloomOrigin, scale * pulse * 0.11f, SpriteEffects.None, 0f);
                spriteBatch.Draw(bloomTex, position, null, AutumnRed * 0.2f, 0f, bloomOrigin, scale * 0.08f, SpriteEffects.None, 0f);

                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            }
            catch { }
            finally
            {
                try { spriteBatch.End(); } catch { }
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                    DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }

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
                .AddIngredient(ModContent.ItemType<DormantAutumnCore>(), 1)
                .AddIngredient(ItemID.SoulofFright, 8)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }

        #endregion
    }
}
