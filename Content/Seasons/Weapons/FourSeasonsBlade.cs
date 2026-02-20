using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;
using MagnumOpus.Common.BaseClasses;
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
    public class FourSeasonsBlade : MeleeSwingItemBase
    {
        private static readonly Color SpringPink = new Color(255, 183, 197);
        private static readonly Color SpringGreen = new Color(144, 238, 144);
        private static readonly Color SummerGold = new Color(255, 215, 0);
        private static readonly Color SummerOrange = new Color(255, 140, 0);
        private static readonly Color AutumnOrange = new Color(255, 140, 50);
        private static readonly Color AutumnBrown = new Color(139, 90, 43);
        private static readonly Color WinterBlue = new Color(150, 220, 255);
        private static readonly Color WinterWhite = new Color(240, 250, 255);

        private int cycleCount = 0;

        protected override int SwingProjectileType => ModContent.ProjectileType<FourSeasonsBladeSwing>();
        protected override int ComboStepCount => 4;

        protected override Color GetLoreColor() => GetCurrentSeasonColor();

        protected override void SetWeaponDefaults()
        {
            Item.width = 78;
            Item.height = 78;
            Item.damage = 285;
            Item.useTime = 22;
            Item.useAnimation = 22;
            Item.knockBack = 7f;
            Item.value = Item.buyPrice(platinum: 1, gold: 50);
            Item.rare = ItemRarityID.Red;
            Item.UseSound = SoundID.Item1;
        }

        private Color GetCurrentSeasonColor()
        {
            int season = CurrentComboStep;
            return season switch
            {
                0 => SpringPink,
                1 => SummerGold,
                2 => AutumnOrange,
                3 => WinterBlue,
                _ => SpringPink
            };
        }

        private Color GetCurrentSeasonSecondary()
        {
            int season = CurrentComboStep;
            return season switch
            {
                0 => SpringGreen,
                1 => SummerOrange,
                2 => AutumnBrown,
                3 => WinterWhite,
                _ => SpringGreen
            };
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return GetCurrentSeasonColor() * 1.2f;
        }

        public override void HoldItem(Player player)
        {
            base.HoldItem(player);

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

        protected override void OnShoot(Player player, int projectileIndex)
        {
            // CurrentComboStep has ALREADY been advanced by base.Shoot().
            // The season that was just swung is (CurrentComboStep + 3) % 4.
            int swungSeason = (CurrentComboStep + 3) % 4;
            Color swungColor = swungSeason switch
            {
                0 => SpringPink,
                1 => SummerGold,
                2 => AutumnOrange,
                3 => WinterBlue,
                _ => SpringPink
            };
            Color swungSecondary = swungSeason switch
            {
                0 => SpringGreen,
                1 => SummerOrange,
                2 => AutumnBrown,
                3 => WinterWhite,
                _ => SpringGreen
            };

            // VFX burst on swing
            CustomParticles.GenericFlare(player.Center, swungColor, 0.6f, 15);
            CustomParticles.HaloRing(player.Center, swungColor * 0.8f, 0.35f, 12);

            for (int i = 0; i < 3; i++)
            {
                Vector2 noteVel = Main.rand.NextVector2Circular(3f, 3f);
                MusicNote(player.Center + noteVel * 5f, noteVel, swungColor, 0.75f, 25);
            }

            for (int i = 0; i < 6; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(4f, 4f);
                Color c = Color.Lerp(swungColor, swungSecondary, Main.rand.NextFloat());
                var glow = new GenericGlowParticle(
                    player.Center + vel * 3f, vel, c * 0.7f, 0.3f, 18, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }

            // Track Crescendo: every complete cycle (after Winter = season 3)
            if (swungSeason == 3)
            {
                cycleCount++;
                if (cycleCount >= 4)
                {
                    cycleCount = 0;
                    SpawnCrescendo(player);
                }
            }
        }

        private void SpawnCrescendo(Player player)
        {
            var source = player.GetSource_ItemUse(Item);
            Vector2 direction = (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX);

            for (int i = 0; i < 4; i++)
            {
                float angleOffset = MathHelper.ToRadians(-30f + i * 20f);
                Vector2 vel = direction.RotatedBy(angleOffset) * 16f;
                int damage = (int)(Item.damage * 1.5f);
                Projectile.NewProjectile(source, player.Center, vel,
                    ModContent.ProjectileType<VivaldiSeasonalWave>(),
                    damage, Item.knockBack, player.whoAmI);
            }

            // Massive Crescendo VFX
            CustomParticles.GenericFlare(player.Center, Color.White, 1.2f, 25);
            CustomParticles.GenericFlare(player.Center, SpringPink, 0.9f, 22);
            CustomParticles.GenericFlare(player.Center, SummerGold, 0.8f, 20);
            CustomParticles.GenericFlare(player.Center, AutumnOrange, 0.7f, 18);
            CustomParticles.GenericFlare(player.Center, WinterBlue, 0.6f, 16);

            for (int i = 0; i < 4; i++)
            {
                Color[] seasonColors = { SpringPink, SummerGold, AutumnOrange, WinterBlue };
                float angle = MathHelper.TwoPi * i / 4f;
                Vector2 noteVel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f);
                MusicNote(player.Center, noteVel, seasonColors[i], 0.9f, 35);
            }

            for (int ring = 0; ring < 6; ring++)
            {
                Color[] ringSeasons = { SpringPink, SummerGold, AutumnOrange, WinterBlue, SpringGreen, SummerOrange };
                CustomParticles.HaloRing(player.Center, ringSeasons[ring], 0.3f + ring * 0.12f, 15 + ring * 3);
            }

            for (int i = 0; i < 16; i++)
            {
                float angle = MathHelper.TwoPi * i / 16f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 8f);
                Color[] allColors = { SpringPink, SpringGreen, SummerGold, SummerOrange, AutumnOrange, AutumnBrown, WinterBlue, WinterWhite };
                Color c = allColors[i % allColors.Length];
                var burst = new GenericGlowParticle(player.Center, vel, c * 0.8f, 0.4f, 22, true);
                MagnumParticleHandler.SpawnParticle(burst);
            }

            MagnumScreenEffects.AddScreenShake(6f);
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D texture = TextureAssets.Item[Item.type].Value;
            Vector2 drawPos = Item.position - Main.screenPosition + new Vector2(Item.width / 2, Item.height / 2);
            Vector2 origin = texture.Size() / 2f;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            float pulse = 0.8f + MathF.Sin(Main.GameUpdateCount * 0.05f) * 0.2f;

            Color springGlow = SpringPink with { A = 0 } * 0.3f * pulse;
            Color summerGlow = SummerGold with { A = 0 } * 0.25f * pulse;
            Color autumnGlow = AutumnOrange with { A = 0 } * 0.25f * pulse;
            Color winterGlow = WinterBlue with { A = 0 } * 0.3f * pulse;
            Color primaryGlow = GetCurrentSeasonColor() with { A = 0 } * 0.4f * pulse;

            spriteBatch.Draw(texture, drawPos, null, springGlow, rotation, origin, scale * 1.15f, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, drawPos, null, summerGlow, rotation, origin, scale * 1.12f, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, drawPos, null, autumnGlow, rotation, origin, scale * 1.10f, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, drawPos, null, winterGlow, rotation, origin, scale * 1.08f, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, drawPos, null, primaryGlow, rotation, origin, scale * 1.05f, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            spriteBatch.Draw(texture, drawPos, null, lightColor, rotation, origin, scale, SpriteEffects.None, 0f);

            return false;
        }

        protected override void AddWeaponTooltips(List<TooltipLine> tooltips)
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
