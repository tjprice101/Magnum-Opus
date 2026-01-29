using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria.GameContent;
using MagnumOpus.Content.Summer.Materials;
using MagnumOpus.Content.Summer.Projectiles;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.Summer.Weapons
{
    /// <summary>
    /// Zenith Cleaver - Summer-themed broadsword (Post-Mechs tier)
    /// A blazing blade that channels the power of the midsummer sun.
    /// - Solar Radiance: Swings emit radiant energy waves
    /// - Sunstroke: Enemies hit are afflicted with burning DOT
    /// - Zenith Strike: Every 7th swing unleashes a massive solar flare
    /// - Heat Mirage: Daytime grants +15% crit and afterimages
    /// </summary>
    public class ZenithCleaver : ModItem
    {
        private int swingCounter = 0;

        private static readonly Color SunGold = new Color(255, 215, 0);
        private static readonly Color SunOrange = new Color(255, 140, 0);
        private static readonly Color SunWhite = new Color(255, 250, 240);
        private static readonly Color SunRed = new Color(255, 100, 50);

        public override void SetDefaults()
        {
            Item.width = 60;
            Item.height = 60;
            Item.damage = 115;
            Item.DamageType = DamageClass.Melee;
            Item.useTime = 22;
            Item.useAnimation = 22;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 6.5f;
            Item.value = Item.buyPrice(gold: 25);
            Item.rare = ItemRarityID.Pink;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.useTurn = false;
            Item.shoot = ModContent.ProjectileType<SolarWave>();
            Item.shootSpeed = 14f;
        }

        public override void HoldItem(Player player)
        {
            // Heat Mirage: Daytime bonuses
            if (Main.dayTime)
            {
                player.GetCritChance(DamageClass.Melee) += 15;
            }

            // Blazing aura
            if (Main.rand.NextBool(5))
            {
                float angle = Main.GameUpdateCount * 0.04f;
                for (int i = 0; i < 3; i++)
                {
                    float orbAngle = angle + MathHelper.TwoPi * i / 3f;
                    float radius = 40f + (float)Math.Sin(Main.GameUpdateCount * 0.06f + i) * 10f;
                    Vector2 orbPos = player.Center + orbAngle.ToRotationVector2() * radius;
                    Color orbColor = Color.Lerp(SunGold, SunOrange, Main.rand.NextFloat());
                    CustomParticles.GenericFlare(orbPos, orbColor * 0.7f, 0.35f, 16);
                }
            }

            // Rising heat shimmer
            if (Main.rand.NextBool(8))
            {
                Vector2 shimmerPos = player.Center + new Vector2(Main.rand.NextFloat(-30f, 30f), Main.rand.NextFloat(10f, 30f));
                Vector2 shimmerVel = new Vector2(0, -Main.rand.NextFloat(1f, 2.5f));
                var shimmer = new GenericGlowParticle(shimmerPos, shimmerVel, SunOrange * 0.4f, 0.25f, 30, true);
                MagnumParticleHandler.SpawnParticle(shimmer);
            }

            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.06f) * 0.15f + 0.7f;
            Lighting.AddLight(player.Center, SunGold.ToVector3() * pulse);
        }

        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            // Swing trail
            if (Main.rand.NextBool(2))
            {
                Vector2 trailPos = hitbox.Center.ToVector2() + Main.rand.NextVector2Circular(15f, 15f);
                Vector2 trailVel = (player.direction * Vector2.UnitX).RotatedByRandom(0.6f) * Main.rand.NextFloat(2f, 5f);
                Color trailColor = Color.Lerp(SunGold, SunOrange, Main.rand.NextFloat());
                var trail = new GenericGlowParticle(trailPos, trailVel, trailColor * 0.8f, 0.35f, 18, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }

            // Heat embers
            if (Main.rand.NextBool(3))
            {
                Dust dust = Dust.NewDustPerfect(hitbox.Center.ToVector2(), DustID.SolarFlare, Main.rand.NextVector2Circular(3f, 3f), 0, SunOrange, 1.1f);
                dust.noGravity = true;
            }
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            swingCounter++;

            // Solar Radiance: Emit energy wave on every swing
            Vector2 spawnPos = player.Center + velocity.SafeNormalize(Vector2.Zero) * 30f;
            Projectile.NewProjectile(source, spawnPos, velocity, type, damage / 2, knockback * 0.5f, player.whoAmI);

            // Swing VFX
            CustomParticles.GenericFlare(spawnPos, SunGold, 0.55f, 15);

            // Zenith Strike: Every 7th swing
            if (swingCounter >= 7)
            {
                swingCounter = 0;

                // Massive solar flare
                CustomParticles.GenericFlare(spawnPos, Color.White, 1.2f, 25);
                CustomParticles.GenericFlare(spawnPos, SunGold, 1.0f, 22);
                CustomParticles.HaloRing(spawnPos, SunOrange * 0.8f, 0.65f, 20);

                // Big solar projectile
                Projectile.NewProjectile(source, spawnPos, velocity * 1.5f, ModContent.ProjectileType<ZenithFlare>(), damage * 2, knockback * 2f, player.whoAmI);

                // Radial flare burst
                for (int i = 0; i < 8; i++)
                {
                    float angle = MathHelper.TwoPi * i / 8f;
                    Vector2 burstVel = angle.ToRotationVector2() * 10f;
                    Projectile.NewProjectile(source, spawnPos, burstVel, type, damage / 3, knockback * 0.3f, player.whoAmI);
                }

                // VFX burst
                for (int i = 0; i < 12; i++)
                {
                    float angle = MathHelper.TwoPi * i / 12f;
                    Vector2 burstVel = angle.ToRotationVector2() * Main.rand.NextFloat(5f, 10f);
                    Color burstColor = Color.Lerp(SunGold, SunRed, (float)i / 12f);
                    var burst = new GenericGlowParticle(spawnPos, burstVel, burstColor * 0.8f, 0.4f, 25, true);
                    MagnumParticleHandler.SpawnParticle(burst);
                }
            }

            return false;
        }

        public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Sunstroke: Apply burning debuff
            target.AddBuff(BuffID.OnFire3, 180); // Hellfire for 3 seconds
            target.AddBuff(BuffID.Daybreak, 120); // Daybreak stacking

            // Impact VFX
            CustomParticles.GenericFlare(target.Center, SunOrange, 0.65f, 18);
            CustomParticles.HaloRing(target.Center, SunGold * 0.6f, 0.4f, 15);

            // Ember burst
            for (int i = 0; i < 8; i++)
            {
                Vector2 emberVel = Main.rand.NextVector2Circular(6f, 6f);
                Color emberColor = Color.Lerp(SunGold, SunRed, Main.rand.NextFloat());
                var ember = new GenericGlowParticle(target.Center, emberVel, emberColor * 0.75f, 0.3f, 20, true);
                MagnumParticleHandler.SpawnParticle(ember);
            }
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D texture = TextureAssets.Item[Item.type].Value;
            Vector2 position = Item.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;

            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.055f) * 0.15f + 1f;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            spriteBatch.Draw(texture, position, null, SunGold * 0.4f, rotation, origin, scale * pulse * 1.4f, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, position, null, SunOrange * 0.35f, rotation, origin, scale * pulse * 1.2f, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, position, null, SunRed * 0.25f, rotation, origin, scale * 1.08f, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Lighting.AddLight(Item.Center, SunGold.ToVector3() * 0.6f);

            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "SolarRadiance", "Swings emit radiant energy waves") { OverrideColor = SunGold });
            tooltips.Add(new TooltipLine(Mod, "Sunstroke", "Enemies are afflicted with intense burning") { OverrideColor = SunOrange });
            tooltips.Add(new TooltipLine(Mod, "ZenithStrike", "Every 7th swing unleashes a massive solar flare") { OverrideColor = SunRed });
            tooltips.Add(new TooltipLine(Mod, "HeatMirage", "Daytime grants +15% melee critical strike chance") { OverrideColor = SunWhite });
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The sun itself bows to the blade's radiance'") { OverrideColor = Color.Lerp(SunGold, SunOrange, 0.5f) });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<SolsticeBar>(), 16)
                .AddIngredient(ModContent.ItemType<SummerResonantEnergy>(), 1)
                .AddIngredient(ItemID.SoulofMight, 8)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}
