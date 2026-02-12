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
using static MagnumOpus.Common.Systems.ThemedParticles;

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

            // Sparse heat shimmer - EARLY GAME: subtle
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

        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            Vector2 hitCenter = hitbox.Center.ToVector2();
            
            // === SPECTACULAR SWING SYSTEM - MID TIER (3-4 arcs with heat effects) ===
            SpectacularMeleeSwing.OnSwing(player, hitbox, SunGold, SunOrange, 
                SpectacularMeleeSwing.SwingTier.Mid, SpectacularMeleeSwing.WeaponTheme.Summer);
            
            // === CALAMITY-STANDARD VFX (Summer Theme) ===
            
            // HEAVY DUST TRAILS - golden sun fire gradient (2+ per frame)
            for (int d = 0; d < 2; d++)
            {
                Vector2 dustPos = new Vector2(hitbox.X + Main.rand.Next(hitbox.Width), hitbox.Y + Main.rand.Next(hitbox.Height));
                Dust dust1 = Dust.NewDustPerfect(dustPos, DustID.SolarFlare, player.velocity * 0.3f + Main.rand.NextVector2Circular(2f, 2f), 0, SunGold, 1.2f);
                dust1.noGravity = true;
                dust1.fadeIn = 1.4f;
                
                Dust dust2 = Dust.NewDustPerfect(dustPos + Main.rand.NextVector2Circular(5f, 5f), DustID.Enchanted_Gold, player.velocity * 0.2f, 0, SunOrange, 1.0f);
                dust2.noGravity = true;
                dust2.fadeIn = 1.3f;
            }
            
            // CONTRASTING SPARKLES - white-hot solar sparkles (1-in-2)
            if (Main.rand.NextBool(2))
            {
                Vector2 sparklePos = new Vector2(hitbox.X + Main.rand.Next(hitbox.Width), hitbox.Y + Main.rand.Next(hitbox.Height));
                var sparkle = new SparkleParticle(sparklePos, player.velocity * 0.15f + Main.rand.NextVector2Circular(1f, 1f), SunWhite, 0.5f, 18);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            // SOLAR SHIMMER TRAILS - cycling gold to orange hues via hslToRgb (1-in-3)
            if (Main.rand.NextBool(3))
            {
                Vector2 shimmerPos = new Vector2(hitbox.X + Main.rand.Next(hitbox.Width), hitbox.Y + Main.rand.Next(hitbox.Height));
                // Solar hues: 0.10-0.16 (gold-orange range)
                float hue = Main.rand.NextFloat(0.10f, 0.16f);
                Color shimmerColor = Main.hslToRgb(hue, 1f, 0.65f);
                var shimmer = new GenericGlowParticle(shimmerPos, player.velocity * 0.25f + Main.rand.NextVector2Circular(1.5f, 1.5f), shimmerColor, 0.35f, 22, true);
                MagnumParticleHandler.SpawnParticle(shimmer);
            }
            
            // PEARLESCENT SUNFIRE EFFECTS - color shifting solar radiance (1-in-4)
            if (Main.rand.NextBool(4))
            {
                Vector2 pearlPos = new Vector2(hitbox.X + Main.rand.Next(hitbox.Width), hitbox.Y + Main.rand.Next(hitbox.Height));
                float colorShift = (float)Math.Sin(Main.GameUpdateCount * 0.15f) * 0.5f + 0.5f;
                Color pearlColor = Color.Lerp(SunGold, SunWhite, colorShift) * 0.75f;
                var pearl = new GenericGlowParticle(pearlPos, player.velocity * 0.2f + new Vector2(0, -0.8f), pearlColor, 0.32f, 20, true);
                MagnumParticleHandler.SpawnParticle(pearl);
            }
            
            // FREQUENT FLARES - solar glow flares (1-in-2)
            if (Main.rand.NextBool(2))
            {
                Vector2 flarePos = new Vector2(hitbox.X + Main.rand.Next(hitbox.Width), hitbox.Y + Main.rand.Next(hitbox.Height));
                Color flareColor = Color.Lerp(SunGold, SunOrange, Main.rand.NextFloat());
                CustomParticles.GenericFlare(flarePos, flareColor, Main.rand.NextFloat(0.25f, 0.4f), 14);
            }
            
            // HEAT EMBER PARTICLES - rising embers (1-in-3)
            if (Main.rand.NextBool(3))
            {
                Vector2 emberPos = new Vector2(hitbox.X + Main.rand.Next(hitbox.Width), hitbox.Y + Main.rand.Next(hitbox.Height));
                Vector2 emberVel = player.velocity * 0.1f + new Vector2(Main.rand.NextFloat(-1f, 1f), -Main.rand.NextFloat(1f, 2.5f));
                Color emberColor = Color.Lerp(SunOrange, SunRed, Main.rand.NextFloat());
                var ember = new GenericGlowParticle(emberPos, emberVel, emberColor * 0.7f, 0.28f, 25, true);
                MagnumParticleHandler.SpawnParticle(ember);
            }
            
            // MUSIC NOTES - summer melody (1-in-6)
            if (Main.rand.NextBool(6))
            {
                Vector2 notePos = new Vector2(hitbox.X + Main.rand.Next(hitbox.Width), hitbox.Y + Main.rand.Next(hitbox.Height));
                Vector2 noteVel = player.velocity * 0.1f + new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-2f, -0.5f));
                ThemedParticles.MusicNote(notePos, noteVel, SunGold, 0.85f, 28);
            }
            
            // PULSING LIGHT
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.12f) * 0.15f + 0.6f;
            Lighting.AddLight(hitCenter, SunGold.ToVector3() * pulse);
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

                // Zenith Strike burst VFX - EARLY GAME: moderate for special attack
                CustomParticles.GenericFlare(spawnPos, SunGold, 0.7f, 18);
                CustomParticles.HaloRing(spawnPos, SunOrange * 0.5f, 0.4f, 15);

                // Big solar projectile
                Projectile.NewProjectile(source, spawnPos, velocity * 1.5f, ModContent.ProjectileType<ZenithFlare>(), damage * 2, knockback * 2f, player.whoAmI);

                // Radial flare burst
                for (int i = 0; i < 8; i++)
                {
                    float angle = MathHelper.TwoPi * i / 8f;
                    Vector2 burstVel = angle.ToRotationVector2() * 10f;
                    Projectile.NewProjectile(source, spawnPos, burstVel, type, damage / 3, knockback * 0.3f, player.whoAmI);
                }

                // VFX burst - reduced for EARLY GAME
                for (int i = 0; i < 5; i++)
                {
                    float angle = MathHelper.TwoPi * i / 5f;
                    Vector2 burstVel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f);
                    Color burstColor = Color.Lerp(SunGold, SunOrange, (float)i / 5f);
                    var burst = new GenericGlowParticle(spawnPos, burstVel, burstColor * 0.6f, 0.25f, 18, true);
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
            
            // === SEEKING SUMMER SOLAR CRYSTALS (every hit spawns 2-3) ===
            int crystalCount = Main.rand.Next(2, 4);
            SeekingCrystalHelper.SpawnSummerCrystals(
                player.GetSource_OnHit(target), target.Center, (target.Center - player.Center).SafeNormalize(Vector2.Zero) * 4f, 
                (int)(damageDone * 0.3f), hit.Knockback, player.whoAmI, crystalCount);

            // === CALAMITY-STANDARD IMPACT VFX ===
            
            // GRADIENT HALO RINGS (4 stacked, outer to inner)
            for (int h = 0; h < 4; h++)
            {
                float progress = h / 4f;
                Color haloColor = Color.Lerp(SunGold, SunRed, progress);
                float haloScale = 0.5f - h * 0.08f;
                int haloLife = 18 - h * 2;
                CustomParticles.HaloRing(target.Center, haloColor * (0.75f - progress * 0.2f), haloScale, haloLife);
            }
            
            // SOLAR SHIMMER FLARES - radial burst
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                Vector2 flarePos = target.Center + angle.ToRotationVector2() * Main.rand.NextFloat(8f, 22f);
                float hue = Main.rand.NextFloat(0.08f, 0.18f); // Gold-orange-red range
                Color flareColor = Main.hslToRgb(hue, 0.95f, 0.65f);
                CustomParticles.GenericFlare(flarePos, flareColor, Main.rand.NextFloat(0.28f, 0.42f), 15);
            }
            
            // RADIAL DUST BURST
            for (int d = 0; d < 16; d++)
            {
                Vector2 dustVel = Main.rand.NextVector2Circular(7f, 7f);
                Dust dust = Dust.NewDustPerfect(target.Center, Main.rand.NextBool() ? DustID.SolarFlare : DustID.Enchanted_Gold, dustVel, 0, Color.White, 1.15f);
                dust.noGravity = true;
                dust.fadeIn = 1.3f;
            }
            
            // WHITE-HOT SPARKLES
            for (int s = 0; s < 6; s++)
            {
                Vector2 sparklePos = target.Center + Main.rand.NextVector2Circular(16f, 16f);
                var sparkle = new SparkleParticle(sparklePos, Main.rand.NextVector2Circular(3.5f, 3.5f), SunWhite, 0.48f, 17);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            // EMBER BURST
            for (int i = 0; i < 10; i++)
            {
                Vector2 emberVel = Main.rand.NextVector2Circular(6f, 6f) + new Vector2(0, -1f);
                Color emberColor = Color.Lerp(SunGold, SunRed, Main.rand.NextFloat());
                var ember = new GenericGlowParticle(target.Center, emberVel, emberColor * 0.75f, 0.32f, 24, true);
                MagnumParticleHandler.SpawnParticle(ember);
            }
            
            // MUSIC NOTES BURST
            for (int n = 0; n < 6; n++)
            {
                float angle = MathHelper.TwoPi * n / 6f + Main.rand.NextFloat(-0.3f, 0.3f);
                Vector2 noteVel = angle.ToRotationVector2() * Main.rand.NextFloat(2.5f, 4.5f);
                Color noteColor = Color.Lerp(SunGold, SunOrange, Main.rand.NextFloat());
                ThemedParticles.MusicNote(target.Center, noteVel, noteColor, 0.8f, 26);
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
