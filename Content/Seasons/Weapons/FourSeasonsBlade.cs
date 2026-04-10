using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;
using Terraria.DataStructures;
using MagnumOpus.Common;
using MagnumOpus.Content.Spring.Materials;
using MagnumOpus.Content.Summer.Materials;
using MagnumOpus.Content.Autumn.Materials;
using MagnumOpus.Content.Winter.Materials;
using MagnumOpus.Content.Seasons.Projectiles;
using MagnumOpus.Content.Spring.Weapons;
using MagnumOpus.Content.Summer.Weapons;
using MagnumOpus.Content.Autumn.Weapons;
using MagnumOpus.Content.Winter.Weapons;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using static MagnumOpus.Common.Systems.ThemedParticles;

namespace MagnumOpus.Content.Seasons.Weapons
{
    /// <summary>
    /// Four Seasons Blade — cycles through Spring, Summer, Autumn, Winter combo phases.
    /// Each season applies unique debuffs, healing, and visual effects via the swing projectile.
    /// Every 4th complete cycle triggers a devastating Crescendo burst.
    /// </summary>
    public class FourSeasonsBlade : ModItem
    {
        private static readonly Color SpringPink = new Color(255, 183, 197);
        private static readonly Color SpringGreen = new Color(144, 238, 144);
        private static readonly Color SummerGold = new Color(255, 215, 0);
        private static readonly Color SummerOrange = new Color(255, 140, 0);
        private static readonly Color AutumnOrange = new Color(255, 140, 50);
        private static readonly Color AutumnBrown = new Color(139, 90, 43);
        private static readonly Color WinterBlue = new Color(150, 220, 255);
        private static readonly Color WinterWhite = new Color(240, 250, 255);

        private int displaySeason = 0;

        private Color GetCurrentSeasonColor() => displaySeason switch
        {
            0 => SpringPink, 1 => SummerGold, 2 => AutumnOrange, 3 => WinterBlue, _ => SpringPink
        };

        private Color GetCurrentSeasonSecondary() => displaySeason switch
        {
            0 => SpringGreen, 1 => SummerOrange, 2 => AutumnBrown, 3 => WinterWhite, _ => SpringGreen
        };

        public override void SetDefaults()
        {
            Item.width = 78;
            Item.height = 78;
            Item.damage = 285;
            Item.DamageType = DamageClass.Melee;
            Item.useTime = 22;
            Item.useAnimation = 22;
            Item.knockBack = 7f;
            Item.value = Item.buyPrice(platinum: 1, gold: 50);
            Item.rare = ItemRarityID.Red;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.channel = true;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.shoot = ModContent.ProjectileType<FourSeasonsBladeSwing>();
            Item.shootSpeed = 1f;
            Item.UseSound = null;
        }

        public override bool CanShoot(Player player)
        {
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                if (Main.projectile[i].active && Main.projectile[i].owner == player.whoAmI &&
                    Main.projectile[i].type == ModContent.ProjectileType<FourSeasonsBladeSwing>())
                    return false;
            }
            return true;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, ai0: 0);
            return false;
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return GetCurrentSeasonColor() * 1.2f;
        }

        public override void HoldItem(Player player)
        {

            Color seasonColor = GetCurrentSeasonColor();
            Color secondaryColor = GetCurrentSeasonSecondary();

            if (Main.rand.NextBool(15))
            {
                Vector2 offset = Main.rand.NextVector2Circular(20f, 20f);
                Vector2 vel = Main.rand.NextVector2Circular(1f, 1f);
                var particle = new GenericGlowParticle(
                    player.Center + offset, vel,
                    seasonColor * 0.6f, 0.3f, 25, true);
                MagnumParticleHandler.SpawnParticle(particle);
            }

            if (Main.rand.NextBool(20))
            {
                Vector2 noteOffset = Main.rand.NextVector2Circular(25f, 25f);
                Vector2 noteVel = new Vector2(0, -1f) + Main.rand.NextVector2Circular(0.5f, 0.5f);
                MusicNote(player.Center + noteOffset, noteVel, secondaryColor, 0.7f, 30);
            }

            float pulse = 0.6f + MathF.Sin(Main.GameUpdateCount * 0.05f) * 0.15f;
            Lighting.AddLight(player.Center, seasonColor.ToVector3() * pulse);
        }



        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D texture = TextureAssets.Item[Item.type].Value;
            Vector2 drawPos = Item.position - Main.screenPosition + new Vector2(Item.width / 2, Item.height / 2);
            Vector2 origin = texture.Size() / 2f;

            try
            {
                Texture2D bloomTex = MagnumTextureRegistry.GetSoftGlow();
                Vector2 bloomOrigin = bloomTex.Size() / 2f;

                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                float pulse = 0.8f + MathF.Sin(Main.GameUpdateCount * 0.05f) * 0.2f;

                Color primaryGlow = GetCurrentSeasonColor() with { A = 0 } * 0.35f * pulse;
                Color springGlow = SpringPink with { A = 0 } * 0.2f * pulse;
                Color winterGlow = WinterBlue with { A = 0 } * 0.2f * pulse;

                spriteBatch.Draw(bloomTex, drawPos, null, primaryGlow, 0f, bloomOrigin, scale * 0.16f, SpriteEffects.None, 0f);
                spriteBatch.Draw(bloomTex, drawPos, null, springGlow, 0f, bloomOrigin, scale * 0.12f, SpriteEffects.None, 0f);
                spriteBatch.Draw(bloomTex, drawPos, null, winterGlow, 0f, bloomOrigin, scale * 0.09f, SpriteEffects.None, 0f);

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

            spriteBatch.Draw(texture, drawPos, null, lightColor, rotation, origin, scale, SpriteEffects.None, 0f);

            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "SeasonalCycle", "Cycles through all four seasons with each swing")
            { OverrideColor = Color.Lerp(SpringPink, WinterBlue, 0.5f) });
            tooltips.Add(new TooltipLine(Mod, "Spring", "Spring: Heals on hit and poisons enemies")
            { OverrideColor = SpringPink });
            tooltips.Add(new TooltipLine(Mod, "Summer", "Summer: Scorches enemies with intense flames")
            { OverrideColor = SummerGold });
            tooltips.Add(new TooltipLine(Mod, "Autumn", "Autumn: Steals life and curses foes")
            { OverrideColor = AutumnOrange });
            tooltips.Add(new TooltipLine(Mod, "Winter", "Winter: Freezes and frostburns enemies")
            { OverrideColor = WinterBlue });
            tooltips.Add(new TooltipLine(Mod, "Crescendo", "Every 4th full cycle unleashes a devastating crescendo")
            { OverrideColor = Color.White });
            tooltips.Add(new TooltipLine(Mod, "Lore", "'A blade that sings the eternal cycle of life'")
            { OverrideColor = Color.Lerp(SpringPink, SummerGold, 0.5f) });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<BlossomsEdge>(), 1)
                .AddIngredient(ModContent.ItemType<ZenithCleaver>(), 1)
                .AddIngredient(ModContent.ItemType<HarvestReaper>(), 1)
                .AddIngredient(ModContent.ItemType<GlacialExecutioner>(), 1)
                .AddIngredient(ModContent.ItemType<SpringResonantEnergy>(), 10)
                .AddIngredient(ModContent.ItemType<SummerResonantEnergy>(), 10)
                .AddIngredient(ModContent.ItemType<AutumnResonantEnergy>(), 10)
                .AddIngredient(ModContent.ItemType<WinterResonantEnergy>(), 10)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}
