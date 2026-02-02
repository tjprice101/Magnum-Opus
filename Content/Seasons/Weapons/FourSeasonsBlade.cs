using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria.GameContent;
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
    /// Four Seasons Blade - The ultimate seasonal melee weapon (Post-Moon Lord tier)
    /// A legendary blade that channels all four seasons in devastating harmony.
    /// - Seasonal Cycle: Each swing cycles through Spring ↁESummer ↁEAutumn ↁEWinter effects
    /// - Vernal Bloom (Spring): Healing petals and life regeneration
    /// - Solar Fury (Summer): Explosive fire damage and radiant burst
    /// - Harvest Reaping (Autumn): Life steal and soul damage
    /// - Glacial Wrath (Winter): Freeze and shattering crits
    /// - Vivaldi's Crescendo: Every 4th complete cycle unleashes all seasons simultaneously
    /// </summary>
    public class FourSeasonsBlade : ModItem
    {
        // Season colors
        private static readonly Color SpringPink = new Color(255, 183, 197);
        private static readonly Color SpringGreen = new Color(144, 238, 144);
        private static readonly Color SummerGold = new Color(255, 215, 0);
        private static readonly Color SummerOrange = new Color(255, 140, 0);
        private static readonly Color AutumnOrange = new Color(255, 140, 50);
        private static readonly Color AutumnBrown = new Color(139, 90, 43);
        private static readonly Color WinterBlue = new Color(150, 220, 255);
        private static readonly Color WinterWhite = new Color(240, 250, 255);

        private int seasonIndex = 0; // 0=Spring, 1=Summer, 2=Autumn, 3=Winter
        private int cycleCount = 0;

        public override void SetDefaults()
        {
            Item.width = 78;
            Item.height = 78;
            Item.damage = 285;
            Item.DamageType = DamageClass.Melee;
            Item.useTime = 22;
            Item.useAnimation = 22;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 7f;
            Item.value = Item.buyPrice(platinum: 1, gold: 50);
            Item.rare = ItemRarityID.Red;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.useTurn = false;
            Item.shoot = ModContent.ProjectileType<VivaldiSeasonalWave>();
            Item.shootSpeed = 16f;
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return GetCurrentSeasonColor() * 1.2f;
        }

        private Color GetCurrentSeasonColor()
        {
            return seasonIndex switch
            {
                0 => SpringPink,
                1 => SummerGold,
                2 => AutumnOrange,
                3 => WinterBlue,
                _ => Color.White
            };
        }

        private Color GetCurrentSeasonSecondary()
        {
            return seasonIndex switch
            {
                0 => SpringGreen,
                1 => SummerOrange,
                2 => AutumnBrown,
                3 => WinterWhite,
                _ => Color.White
            };
        }

        public override void HoldItem(Player player)
        {
            // Ambient seasonal particles
            Color primaryColor = GetCurrentSeasonColor();
            Color secondaryColor = GetCurrentSeasonSecondary();

            if (Main.rand.NextBool(15))
            {
                float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                float radius = Main.rand.NextFloat(35f, 50f);
                Vector2 particlePos = player.Center + angle.ToRotationVector2() * radius;
                Vector2 particleVel = angle.ToRotationVector2() * Main.rand.NextFloat(1f, 1.5f);
                Color particleColor = Color.Lerp(primaryColor, secondaryColor, Main.rand.NextFloat()) * 0.35f;
                var particle = new GenericGlowParticle(particlePos, particleVel, particleColor, 0.22f, 22, true);
                MagnumParticleHandler.SpawnParticle(particle);
            }

            // Musical notation - visible scale 0.7f+
            if (Main.rand.NextBool(20))
            {
                float noteAngle = Main.GameUpdateCount * 0.04f + Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 notePos = player.Center + noteAngle.ToRotationVector2() * 40f;
                Vector2 noteVel = new Vector2(0, Main.rand.NextFloat(-1f, -0.4f));
                ThemedParticles.MusicNote(notePos, noteVel, primaryColor, 0.7f, 30);
            }

            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.05f) * 0.12f + 0.45f;
            Lighting.AddLight(player.Center, primaryColor.ToVector3() * pulse);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Vector2 spawnPos = player.Center + velocity.SafeNormalize(Vector2.Zero) * 50f;

            // Spawn seasonal wave projectile
            int proj = Projectile.NewProjectile(source, spawnPos, velocity, type, damage, knockback, player.whoAmI, seasonIndex);

            // Seasonal swing VFX
            Color primaryColor = GetCurrentSeasonColor();
            Color secondaryColor = GetCurrentSeasonSecondary();

            // ☁ELAYERED BLOOM FLARES
            CustomParticles.GenericFlare(spawnPos, Color.White, 0.75f, 20);
            CustomParticles.GenericFlare(spawnPos, primaryColor, 0.6f, 18);
            CustomParticles.HaloRing(spawnPos, secondaryColor * 0.6f, 0.45f, 15);

            // ☁EMUSICAL NOTATION - Notes scatter on swing! - VISIBLE SCALE 0.72f+
            for (int n = 0; n < 3; n++)
            {
                float noteAngle = velocity.ToRotation() + MathHelper.ToRadians(Main.rand.NextFloat(-60f, 60f));
                Vector2 noteVel = noteAngle.ToRotationVector2() * Main.rand.NextFloat(2f, 4f);
                ThemedParticles.MusicNote(spawnPos, noteVel, primaryColor, 0.72f, 32);
            }

            for (int i = 0; i < 10; i++)
            {
                float angle = velocity.ToRotation() + MathHelper.ToRadians(Main.rand.NextFloat(-45f, 45f));
                Vector2 burstVel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 8f);
                Color burstColor = Color.Lerp(primaryColor, secondaryColor, Main.rand.NextFloat()) * 0.5f;
                var burst = new GenericGlowParticle(spawnPos, burstVel, burstColor, 0.28f, 18, true);
                MagnumParticleHandler.SpawnParticle(burst);
                
                // ☁ESPARKLE accents on burst
                if (i % 3 == 0)
                {
                    var sparkle = new SparkleParticle(spawnPos, burstVel * 0.6f, secondaryColor * 0.6f, 0.22f, 15);
                    MagnumParticleHandler.SpawnParticle(sparkle);
                }
            }

            // Cycle season
            seasonIndex = (seasonIndex + 1) % 4;
            
            // Track complete cycles
            if (seasonIndex == 0)
            {
                cycleCount++;

                // Vivaldi's Crescendo - every 4th complete cycle
                if (cycleCount >= 4)
                {
                    cycleCount = 0;
                    SpawnCrescendo(player, source, spawnPos, velocity, damage, knockback);
                }
            }

            return false;
        }

        private void SpawnCrescendo(Player player, IEntitySource source, Vector2 position, Vector2 velocity, int damage, float knockback)
        {
            // Spawn waves for ALL four seasons simultaneously
            for (int i = 0; i < 4; i++)
            {
                float angleOffset = MathHelper.ToRadians(-30f + i * 20f);
                Vector2 waveVel = velocity.RotatedBy(angleOffset) * 1.2f;
                Projectile.NewProjectile(source, position, waveVel, ModContent.ProjectileType<VivaldiSeasonalWave>(),
                    (int)(damage * 1.5f), knockback, player.whoAmI, i);
            }

            // Massive VFX burst
            CustomParticles.GenericFlare(position, Color.White, 1.5f, 35);
            CustomParticles.GenericFlare(position, Color.White * 0.7f, 1.1f, 30);
            
            // Musical crescendo - grand note explosion! Visible scale 0.8f+
            for (int n = 0; n < 4; n++)
            {
                float noteAngle = MathHelper.TwoPi * n / 4f;
                Vector2 noteVel = noteAngle.ToRotationVector2() * Main.rand.NextFloat(3f, 5f);
                Color noteColor = n switch { 0 => SpringPink, 1 => SummerGold, 2 => AutumnOrange, _ => WinterBlue };
                ThemedParticles.MusicNote(position, noteVel, noteColor, 0.8f, 35);
            }
            
            // Multi-season halo cascade
            CustomParticles.HaloRing(position, SpringPink * 0.6f, 0.7f, 24);
            CustomParticles.HaloRing(position, SummerGold * 0.5f, 0.55f, 21);
            CustomParticles.HaloRing(position, AutumnOrange * 0.45f, 0.45f, 18);
            CustomParticles.HaloRing(position, WinterBlue * 0.4f, 0.35f, 16);

            // Seasonal burst particles
            for (int i = 0; i < 10; i++)
            {
                float angle = MathHelper.TwoPi * i / 10f;
                Vector2 burstVel = angle.ToRotationVector2() * Main.rand.NextFloat(7f, 11f);
                Color burstColor = (i % 4) switch
                {
                    0 => SpringPink,
                    1 => SummerGold,
                    2 => AutumnOrange,
                    _ => WinterBlue
                };
                var burst = new GenericGlowParticle(position, burstVel, burstColor * 0.55f, 0.38f, 24, true);
                MagnumParticleHandler.SpawnParticle(burst);
            }

            // Screen effects
            MagnumScreenEffects.AddScreenShake(6f);
        }

        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            Color primaryColor = GetCurrentSeasonColor();
            Color secondaryColor = GetCurrentSeasonSecondary();
            Vector2 hitCenter = hitbox.Center.ToVector2();

            // === SPECTACULAR SWORD ARC SYSTEM - Endgame tier for combined seasonal blade ===
            SpectacularMeleeSwing.OnSwing(player, hitbox, primaryColor, secondaryColor, 
                SpectacularMeleeSwing.SwingTier.Endgame, SpectacularMeleeSwing.WeaponTheme.Seasons);

            // Clean, flowing seasonal trail - fewer but more vibrant particles
            if (Main.rand.NextBool(3))
            {
                Vector2 trailPos = hitCenter + Main.rand.NextVector2Circular(10f, 10f);
                Vector2 trailVel = player.velocity * 0.15f + Main.rand.NextVector2Circular(1.5f, 1.5f);
                Color trailColor = Color.Lerp(primaryColor, secondaryColor, Main.rand.NextFloat()) * 0.7f;
                var trail = new GenericGlowParticle(trailPos, trailVel, trailColor, 0.28f, 16, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }
            
            // Single clean sparkle accent per swing
            if (Main.rand.NextBool(5))
            {
                Vector2 sparklePos = hitCenter + Main.rand.NextVector2Circular(15f, 15f);
                var sparkle = new SparkleParticle(sparklePos, Vector2.Zero, secondaryColor * 0.6f, 0.22f, 12);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            // Music notes - elegant, single notes floating in the blade's wake
            if (Main.rand.NextBool(6))
            {
                Vector2 notePos = hitCenter + Main.rand.NextVector2Circular(8f, 8f);
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-0.8f, 0.8f), Main.rand.NextFloat(-1.5f, -0.5f));
                ThemedParticles.MusicNote(notePos, noteVel, primaryColor * 0.85f, 0.7f, 30);
            }
        }

        public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Previous season effects (since we cycle before hit)
            int prevSeason = (seasonIndex + 3) % 4;

            switch (prevSeason)
            {
                case 0: // Spring - Healing
                    if (Main.rand.NextFloat() < 0.25f)
                    {
                        int healAmount = Math.Max(1, damageDone / 20);
                        player.Heal(healAmount);
                        
                        // Healing VFX with sparkles
                        for (int i = 0; i < 5; i++)
                        {
                            Vector2 petalPos = target.Center + Main.rand.NextVector2Circular(30f, 30f);
                            Vector2 petalVel = (player.Center - petalPos).SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(3f, 6f);
                            var petal = new GenericGlowParticle(petalPos, petalVel, SpringPink * 0.6f, 0.25f, 20, true);
                            MagnumParticleHandler.SpawnParticle(petal);
                            
                            // ☁ESPRING SPARKLE
                            var sparkle = new SparkleParticle(petalPos, petalVel * 0.5f, SpringGreen * 0.5f, 0.2f, 18);
                            MagnumParticleHandler.SpawnParticle(sparkle);
                        }
                        
                        // ☁ESPRING MUSIC NOTE - VISIBLE SCALE 0.7f+
                        ThemedParticles.MusicNote(target.Center, new Vector2(0, -1.5f), SpringPink, 0.7f, 30);
                    }
                    target.AddBuff(BuffID.Poisoned, 180);
                    break;

                case 1: // Summer - Fire explosion
                    target.AddBuff(BuffID.OnFire3, 300);
                    target.AddBuff(BuffID.Daybreak, 180);
                    
                    // Fire burst VFX with sparkles
                    CustomParticles.GenericFlare(target.Center, SummerGold, 0.7f, 20);
                    CustomParticles.GenericFlare(target.Center, SummerOrange, 0.55f, 16);
                    for (int i = 0; i < 6; i++)
                    {
                        Vector2 fireVel = Main.rand.NextVector2Circular(5f, 5f);
                        var fire = new GenericGlowParticle(target.Center, fireVel, SummerOrange * 0.6f, 0.3f, 16, true);
                        MagnumParticleHandler.SpawnParticle(fire);
                        
                        // ☁ESUMMER SPARKLE
                        var sparkle = new SparkleParticle(target.Center, fireVel * 0.7f, SummerGold * 0.6f, 0.22f, 14);
                        MagnumParticleHandler.SpawnParticle(sparkle);
                    }
                    
                    // ☁ESUMMER MUSIC NOTE - VISIBLE SCALE 0.7f+
                    ThemedParticles.MusicNote(target.Center, new Vector2(0, -1.5f), SummerGold, 0.7f, 30);
                    break;

                case 2: // Autumn - Life steal
                    int stealAmount = Math.Max(1, damageDone / 15);
                    player.Heal(stealAmount);
                    target.AddBuff(BuffID.CursedInferno, 240);
                    
                    // Soul drain VFX with glyphs for decay theme
                    CustomParticles.GenericFlare(target.Center, AutumnOrange, 0.55f, 16);
                    for (int i = 0; i < 4; i++)
                    {
                        Vector2 soulPos = target.Center + Main.rand.NextVector2Circular(20f, 20f);
                        Vector2 soulVel = (player.Center - soulPos).SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(4f, 8f);
                        var soul = new GenericGlowParticle(soulPos, soulVel, AutumnOrange * 0.5f, 0.22f, 25, true);
                        MagnumParticleHandler.SpawnParticle(soul);
                    }
                    
                    // ☁EAUTUMN GLYPH - decay theme
                    CustomParticles.Glyph(target.Center, AutumnBrown * 0.4f, 0.22f, -1);
                    
                    // ☁EAUTUMN MUSIC NOTE - VISIBLE SCALE 0.7f+
                    ThemedParticles.MusicNote(target.Center, new Vector2(0, -1.5f), AutumnOrange, 0.7f, 30);
                    break;

                case 3: // Winter - Freeze
                    target.AddBuff(BuffID.Frostburn2, 300);
                    if (Main.rand.NextFloat() < 0.3f)
                    {
                        target.AddBuff(BuffID.Frozen, 90);
                        
                        // Freeze VFX with sparkles
                        CustomParticles.GenericFlare(target.Center, WinterWhite, 0.75f, 22);
                        CustomParticles.GenericFlare(target.Center, WinterBlue, 0.6f, 18);
                        CustomParticles.HaloRing(target.Center, WinterBlue * 0.6f, 0.5f, 18);
                        
                        // ☁EWINTER SPARKLES - icy shimmer
                        for (int s = 0; s < 4; s++)
                        {
                            Vector2 sparklePos = target.Center + Main.rand.NextVector2Circular(25f, 25f);
                            var sparkle = new SparkleParticle(sparklePos, Main.rand.NextVector2Circular(2f, 2f), WinterWhite * 0.7f, 0.28f, 20);
                            MagnumParticleHandler.SpawnParticle(sparkle);
                        }
                    }
                    
                    // ☁EWINTER MUSIC NOTE - VISIBLE SCALE 0.7f+
                    ThemedParticles.MusicNote(target.Center, new Vector2(0, -1.5f), WinterBlue, 0.7f, 30);
                    break;
            }

            // Standard impact VFX with layered bloom
            Color currentColor = prevSeason switch
            {
                0 => SpringPink,
                1 => SummerGold,
                2 => AutumnOrange,
                _ => WinterBlue
            };
            CustomParticles.GenericFlare(target.Center, Color.White, 0.6f, 18);
            CustomParticles.GenericFlare(target.Center, currentColor, 0.5f, 16);
            CustomParticles.HaloRing(target.Center, currentColor * 0.5f, 0.35f, 14);
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D texture = TextureAssets.Item[Item.type].Value;
            Vector2 position = Item.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;

            Color primaryColor = GetCurrentSeasonColor();
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.05f) * 0.12f + 1f;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // Multi-seasonal glow layers
            spriteBatch.Draw(texture, position, null, SpringPink * 0.2f, rotation, origin, scale * pulse * 1.35f, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, position, null, SummerGold * 0.18f, rotation, origin, scale * 1.25f, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, position, null, AutumnOrange * 0.16f, rotation, origin, scale * 1.15f, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, position, null, WinterBlue * 0.14f, rotation, origin, scale * 1.08f, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, position, null, primaryColor * 0.35f, rotation, origin, scale * 1.02f, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Lighting.AddLight(Item.Center, primaryColor.ToVector3() * 0.6f);

            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "SeasonalCycle", "Each swing cycles through the four seasons") { OverrideColor = Color.White });
            tooltips.Add(new TooltipLine(Mod, "Spring", "Spring: Healing petals restore life") { OverrideColor = SpringPink });
            tooltips.Add(new TooltipLine(Mod, "Summer", "Summer: Solar fury inflicts burning devastation") { OverrideColor = SummerGold });
            tooltips.Add(new TooltipLine(Mod, "Autumn", "Autumn: Harvest reaping drains enemy life force") { OverrideColor = AutumnOrange });
            tooltips.Add(new TooltipLine(Mod, "Winter", "Winter: Glacial wrath freezes enemies solid") { OverrideColor = WinterBlue });
            tooltips.Add(new TooltipLine(Mod, "Crescendo", "Every 4th cycle unleashes Vivaldi's Crescendo - all seasons at once!") { OverrideColor = Color.Lerp(Color.White, Color.Gold, 0.5f) });
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The seasons dance upon its edge'") { OverrideColor = Color.Lerp(SpringPink, WinterBlue, 0.5f) });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                // Combine the 4 lower-tier seasonal melee weapons
                .AddIngredient(ModContent.ItemType<BlossomsEdge>(), 1)
                .AddIngredient(ModContent.ItemType<ZenithCleaver>(), 1)
                .AddIngredient(ModContent.ItemType<HarvestReaper>(), 1)
                .AddIngredient(ModContent.ItemType<GlacialExecutioner>(), 1)
                // Plus 10 of each Seasonal Resonant Energy
                .AddIngredient(ModContent.ItemType<SpringResonantEnergy>(), 10)
                .AddIngredient(ModContent.ItemType<SummerResonantEnergy>(), 10)
                .AddIngredient(ModContent.ItemType<AutumnResonantEnergy>(), 10)
                .AddIngredient(ModContent.ItemType<WinterResonantEnergy>(), 10)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}
