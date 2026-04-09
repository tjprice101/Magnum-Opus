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
using MagnumOpus.Content.Summer.Materials;
using static MagnumOpus.Common.Systems.ThemedParticles;

namespace MagnumOpus.Content.Summer.Weapons
{
    /// <summary>
    /// Zenith Cleaver - Summer-themed broadsword (Post-Mechs tier)
    /// A blazing blade that channels the power of the midsummer sun.
    /// Now uses the held-projectile swing system via MeleeSwingItemBase.
    /// </summary>
    public class ZenithCleaver : ModItem
    {
        #region Theme Colors

        private static readonly Color SunGold = new Color(255, 215, 0);
        private static readonly Color SunOrange = new Color(255, 140, 0);
        private static readonly Color SunWhite = new Color(255, 250, 240);
        private static readonly Color SunRed = new Color(255, 100, 50);

        #endregion

        public override void SetDefaults()
        {
            Item.width = 60;
            Item.height = 60;
            Item.damage = 115;
            Item.DamageType = DamageClass.MeleeNoSpeed;
            Item.useTime = 22;
            Item.useAnimation = 22;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 6.5f;
            Item.value = Item.buyPrice(gold: 25);
            Item.rare = ItemRarityID.Pink;
            Item.UseSound = null;
            Item.autoReuse = true;
            Item.channel = true;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.shoot = ModContent.ProjectileType<ZenithCleaverSwing>();
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
            tooltips.Add(new TooltipLine(Mod, "SolarRadiance", "Swings emit radiant energy waves") { OverrideColor = SunGold });
            tooltips.Add(new TooltipLine(Mod, "Sunstroke", "Enemies are afflicted with intense burning") { OverrideColor = SunOrange });
            tooltips.Add(new TooltipLine(Mod, "ZenithStrike", "Every 3rd combo finisher unleashes a massive solar flare") { OverrideColor = SunRed });
            tooltips.Add(new TooltipLine(Mod, "HeatMirage", "Daytime grants +15% melee critical strike chance") { OverrideColor = SunWhite });
        }

        #region Hold Effects

        public override void HoldItem(Player player)
        {

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
            spriteBatch.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
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
                .AddIngredient(ModContent.ItemType<DormantSummerCore>(), 1)
                .AddIngredient(ItemID.SoulofMight, 8)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }

        #endregion
    }
}
