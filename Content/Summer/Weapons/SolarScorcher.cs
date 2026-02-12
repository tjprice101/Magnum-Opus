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
    /// Solar Scorcher - Summer-themed flamethrower (Post-Mechs tier)
    /// Unleashes streams of concentrated solar fire.
    /// - Searing Spray: Continuous solar flame stream (68 damage/tick)
    /// - Heatwave: Every 2 seconds creates expanding heat pulse
    /// - Solar Buildup: Continuous fire increases damage over time (up to +30%)
    /// - Mirage Shield: Standing still creates heat barrier that damages nearby enemies
    /// </summary>
    public class SolarScorcher : ModItem
    {
        private int fireTimer = 0;
        private int heatwaveTimer = 0;
        private float damageBonus = 0f;
        private int idleTimer = 0;

        private static readonly Color SunGold = new Color(255, 215, 0);
        private static readonly Color SunOrange = new Color(255, 140, 0);
        private static readonly Color SunWhite = new Color(255, 250, 240);
        private static readonly Color SunRed = new Color(255, 100, 50);

        public override void SetDefaults()
        {
            Item.width = 54;
            Item.height = 24;
            Item.damage = 68;
            Item.DamageType = DamageClass.Ranged;
            Item.useTime = 6;
            Item.useAnimation = 6;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 1f;
            Item.value = Item.buyPrice(gold: 25);
            Item.rare = ItemRarityID.Pink;
            Item.UseSound = SoundID.Item34;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<SolarFlameStream>();
            Item.shootSpeed = 16f;
            Item.noMelee = true;
            Item.useAmmo = AmmoID.Gel;
            Item.channel = true;
        }

        public override void HoldItem(Player player)
        {
            // Track if player is standing still for Mirage Shield
            if (Math.Abs(player.velocity.X) < 0.5f && Math.Abs(player.velocity.Y) < 0.5f)
            {
                idleTimer++;
                
                // Mirage Shield: After 60 frames of standing still
                if (idleTimer >= 60)
                {
                    // Visual heat barrier
                    if (Main.rand.NextBool(4))
                    {
                        float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                        float radius = 50f + Main.rand.NextFloat(20f);
                        Vector2 heatPos = player.Center + angle.ToRotationVector2() * radius;
                        Vector2 heatVel = (angle + MathHelper.PiOver2).ToRotationVector2() * 2f;
                        Color heatColor = Color.Lerp(SunOrange, SunRed, Main.rand.NextFloat()) * 0.5f;
                        var heat = new GenericGlowParticle(heatPos, heatVel, heatColor, 0.28f, 20, true);
                        MagnumParticleHandler.SpawnParticle(heat);
                    }

                    // Damage nearby enemies
                    if (idleTimer % 30 == 0)
                    {
                        foreach (NPC npc in Main.npc)
                        {
                            if (!npc.active || npc.friendly || npc.dontTakeDamage) continue;
                            float dist = Vector2.Distance(player.Center, npc.Center);
                            if (dist < 80f)
                            {
                                int shieldDamage = (int)(Item.damage * 0.3f);
                                npc.SimpleStrikeNPC(shieldDamage, 0, false, 0f, DamageClass.Ranged);
                                npc.AddBuff(BuffID.OnFire3, 60);
                                
                                CustomParticles.GenericFlare(npc.Center, SunOrange, 0.4f, 12);
                            }
                        }
                    }
                }
            }
            else
            {
                idleTimer = 0;
            }

            // Decay damage bonus when not firing
            if (!player.channel)
            {
                fireTimer = 0;
                damageBonus = Math.Max(0f, damageBonus - 0.005f);
            }

            // ========== CALAMITY-STANDARD VFX PATTERN ==========
            // HEAVY DUST TRAILS - 2+ per frame with fadeIn
            for (int d = 0; d < 2; d++)
            {
                Vector2 dustPos = player.Center + Main.rand.NextVector2Circular(22f, 22f);
                Dust dust = Dust.NewDustPerfect(dustPos, DustID.Torch, new Vector2(0, -Main.rand.NextFloat(0.4f, 1.2f)), 0, SunOrange, Main.rand.NextFloat(1.1f, 1.5f));
                dust.noGravity = true;
                dust.fadeIn = 1.4f;
            }
            
            // CONTRASTING SPARKLES - different color (white-hot contrast)
            if (Main.rand.NextBool(2))
            {
                Vector2 sparklePos = player.Center + Main.rand.NextVector2Circular(28f, 28f);
                CustomParticles.PrismaticSparkle(sparklePos, SunWhite, Main.rand.NextFloat(0.35f, 0.5f));
            }
            
            // SHIMMER TRAILS - rising heat waves with color cycling
            if (Main.rand.NextBool(3))
            {
                float hue = 0.08f + Main.rand.NextFloat(0.05f); // Gold to orange range
                Color shimmerColor = Main.hslToRgb(hue, 1f, 0.7f);
                Vector2 shimmerPos = player.Center + Main.rand.NextVector2Circular(30f, 30f);
                Vector2 shimmerVel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -Main.rand.NextFloat(0.8f, 1.5f));
                var shimmer = new GenericGlowParticle(shimmerPos, shimmerVel, shimmerColor * 0.65f, 0.28f, 22, true);
                MagnumParticleHandler.SpawnParticle(shimmer);
            }
            
            // MUSIC NOTES - visible scale with solar theme
            if (Main.rand.NextBool(6))
            {
                Vector2 notePos = player.Center + Main.rand.NextVector2Circular(35f, 35f);
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-0.8f, 0.8f), -Main.rand.NextFloat(0.6f, 1.4f));
                Color noteColor = Color.Lerp(SunGold, SunOrange, Main.rand.NextFloat());
                ThemedParticles.MusicNote(notePos, noteVel, noteColor, Main.rand.NextFloat(0.85f, 1.1f), 32);
            }
            
            // ORBITING SOLAR FLARES - rotating heat points
            if (Main.rand.NextBool(4))
            {
                float orbitAngle = Main.GameUpdateCount * 0.06f + Main.rand.NextFloat(MathHelper.TwoPi);
                float orbitRadius = 40f + Main.rand.NextFloat(15f);
                Vector2 orbitPos = player.Center + orbitAngle.ToRotationVector2() * orbitRadius;
                CustomParticles.GenericFlare(orbitPos, SunRed * 0.6f, 0.32f, 14);
            }
            
            // Enhanced dynamic lighting
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.05f) * 0.15f + 0.65f;
            Lighting.AddLight(player.Center, SunOrange.ToVector3() * pulse);
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            // Solar Buildup: Increase damage based on continuous fire time
            fireTimer++;
            damageBonus = Math.Min(0.30f, fireTimer / 300f); // Max +30% after 5 seconds of fire
            damage = (int)(damage * (1f + damageBonus));

            // Heatwave timer
            heatwaveTimer++;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Spray spread
            float spread = MathHelper.ToRadians(Main.rand.NextFloat(-8f, 8f));
            velocity = velocity.RotatedBy(spread);

            // ========== SPECTACULAR SOLAR SCORCHER MUZZLE FLASH ==========
            // MULTI-LAYER FLARE - central solar burst
            CustomParticles.GenericFlare(position, Color.White, 0.55f, 10);
            CustomParticles.GenericFlare(position, SunGold, 0.45f, 12);
            CustomParticles.GenericFlare(position, SunOrange * 0.8f, 0.35f, 14);
            
            // DIRECTIONAL HEAT SPARKS - along firing direction
            for (int i = 0; i < 4; i++)
            {
                Vector2 sparkVel = velocity.SafeNormalize(Vector2.UnitX).RotatedByRandom(0.4f) * Main.rand.NextFloat(4f, 8f);
                Color sparkColor = Color.Lerp(SunOrange, SunRed, Main.rand.NextFloat());
                Dust spark = Dust.NewDustPerfect(position, DustID.Torch, sparkVel, 0, sparkColor, 1.3f);
                spark.noGravity = true;
                spark.fadeIn = 1.2f;
            }
            
            // CONTRASTING WHITE-HOT SPARKLE
            CustomParticles.PrismaticSparkle(position, SunWhite, 0.4f);

            // Fire stream projectile
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);

            // Heatwave: Every 2 seconds
            if (heatwaveTimer >= 120)
            {
                heatwaveTimer = 0;
                
                // Spawn heatwave pulse
                Projectile.NewProjectile(source, player.Center, Vector2.Zero, ModContent.ProjectileType<HeatwavePulse>(), damage / 2, 0f, player.whoAmI);
                
                // ========== SPECTACULAR HEATWAVE VFX ==========
                // CENTRAL SOLAR BURST
                CustomParticles.GenericFlare(player.Center, Color.White, 0.9f, 18);
                CustomParticles.GenericFlare(player.Center, SunGold, 0.7f, 20);
                
                // 6-LAYER GRADIENT HALO CASCADE - gold to red
                for (int ring = 0; ring < 6; ring++)
                {
                    float progress = ring / 6f;
                    Color ringColor = Color.Lerp(SunGold, SunRed, progress);
                    float ringScale = 0.35f + ring * 0.12f;
                    int ringLife = 14 + ring * 3;
                    CustomParticles.HaloRing(player.Center, ringColor * (0.7f - progress * 0.3f), ringScale, ringLife);
                }
                
                // RADIAL HEAT DUST BURST
                for (int i = 0; i < 12; i++)
                {
                    float angle = MathHelper.TwoPi * i / 12f;
                    Vector2 dustVel = angle.ToRotationVector2() * Main.rand.NextFloat(5f, 9f);
                    Dust heat = Dust.NewDustPerfect(player.Center, DustID.Torch, dustVel, 0, SunOrange, 1.4f);
                    heat.noGravity = true;
                    heat.fadeIn = 1.3f;
                }
                
                // MUSIC NOTE RING - solar symphony
                for (int i = 0; i < 4; i++)
                {
                    float angle = MathHelper.TwoPi * i / 4f + Main.rand.NextFloat(0.2f);
                    Vector2 noteVel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 4f);
                    Color noteColor = Color.Lerp(SunGold, SunOrange, Main.rand.NextFloat());
                    ThemedParticles.MusicNote(player.Center, noteVel, noteColor, 0.9f, 28);
                }
                
                // SPARKLE ACCENTS
                for (int i = 0; i < 6; i++)
                {
                    Vector2 sparklePos = player.Center + Main.rand.NextVector2Circular(40f, 40f);
                    CustomParticles.PrismaticSparkle(sparklePos, SunWhite, 0.45f);
                }
            }

            return false;
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D texture = TextureAssets.Item[Item.type].Value;
            Vector2 position = Item.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;

            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.05f) * 0.1f + 1f;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            spriteBatch.Draw(texture, position, null, SunOrange * 0.35f, rotation, origin, scale * pulse * 1.3f, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, position, null, SunRed * 0.28f, rotation, origin, scale * pulse * 1.15f, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, position, null, SunGold * 0.22f, rotation, origin, scale * 1.05f, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Lighting.AddLight(Item.Center, SunOrange.ToVector3() * 0.5f);

            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "SearingSpray", "Sprays continuous solar flames") { OverrideColor = SunOrange });
            tooltips.Add(new TooltipLine(Mod, "Heatwave", "Every 2 seconds creates an expanding heat pulse") { OverrideColor = SunGold });
            tooltips.Add(new TooltipLine(Mod, "SolarBuildup", "Continuous fire increases damage up to 30%") { OverrideColor = SunRed });
            tooltips.Add(new TooltipLine(Mod, "MirageShield", "Standing still creates a heat barrier that damages nearby foes") { OverrideColor = SunWhite });
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Harness the scorching breath of summer's peak'") { OverrideColor = Color.Lerp(SunOrange, SunRed, 0.5f) });
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
