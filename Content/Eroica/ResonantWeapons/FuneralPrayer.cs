using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;
using MagnumOpus.Content.Eroica.ResonanceEnergies;
using MagnumOpus.Content.Eroica.Projectiles;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using System.Collections.Generic;
using System.Linq;

namespace MagnumOpus.Content.Eroica.ResonantWeapons
{
    /// <summary>
    /// Funeral Prayer - Magic weapon that fires 5 large flaming projectiles.
    /// Rainbow rarity, higher tier than Moonlight weapons.
    /// </summary>
    public class FuneralPrayer : ModItem
    {
        // Track beam hits for ricochet beam spawning
        public static Dictionary<int, HashSet<int>> BeamHitTracking = new Dictionary<int, HashSet<int>>();
        
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
            Item.staff[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.damage = 340; // Balanced: ~1133 DPS (340 × 60/18)
            Item.DamageType = DamageClass.Magic;
            Item.width = 50;
            Item.height = 50;
            Item.useTime = 18;
            Item.useAnimation = 18;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 7f;
            Item.value = Item.sellPrice(gold: 35);
            Item.rare = ModContent.RarityType<EroicaRainbowRarity>();
            Item.UseSound = SoundID.Item20;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<FuneralPrayerProjectile>();
            Item.shootSpeed = 16f;
            Item.mana = 14; // 5% mana reduction from 15
            Item.noMelee = true;
            Item.maxStack = 1;
        }

        public override void HoldItem(Player player)
        {
            // === UnifiedVFX EROICA AMBIENT AURA ===
            UnifiedVFX.Eroica.Aura(player.Center, 32f, 0.28f);
            
            // === AMBIENT FRACTAL FLARES - Dark funeral flame geometric pattern ===
            if (Main.rand.NextBool(6))
            {
                // Orbiting dark flames in spiral pattern
                float baseAngle = Main.GameUpdateCount * 0.025f;
                for (int i = 0; i < 5; i++)
                {
                    float angle = baseAngle + MathHelper.TwoPi * i / 5f;
                    float radius = 30f + (float)Math.Sin(Main.GameUpdateCount * 0.04f + i * 0.8f) * 12f;
                    Vector2 flarePos = player.Center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius;
                    // Gradient: Dark crimson to gold
                    float progress = (float)i / 5f;
                    Color fractalColor = Color.Lerp(UnifiedVFX.Eroica.Scarlet, UnifiedVFX.Eroica.Gold, progress * 0.6f + 0.2f);
                    CustomParticles.GenericFlare(flarePos, fractalColor, 0.3f, 17);
                }
            }
            
            // Custom particle dark flames with prismatic accents
            if (Main.rand.NextBool(4))
            {
                float progress = Main.rand.NextFloat();
                Color flameColor = Color.Lerp(UnifiedVFX.Eroica.Scarlet, UnifiedVFX.Eroica.Gold, progress);
                CustomParticles.GenericGlow(player.Center + Main.rand.NextVector2Circular(20f, 20f), flameColor, 0.28f, 15);
            }
            
            // Sakura petals
            if (Main.rand.NextBool(8))
            {
                ThemedParticles.SakuraPetals(player.Center + Main.rand.NextVector2Circular(22f, 22f), 2, 20f);
            }
            
            // Occasional dark halo pulse
            if (Main.rand.NextBool(20))
            {
                CustomParticles.HaloRing(player.Center, UnifiedVFX.Eroica.Scarlet * 0.55f, 0.32f, 20);
            }
            
            // Soft heroic gradient lighting with pulse
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.05f) * 0.1f + 0.9f;
            Vector3 lightColor = Color.Lerp(UnifiedVFX.Eroica.Scarlet, UnifiedVFX.Eroica.Gold, 0.4f).ToVector3();
            Lighting.AddLight(player.Center, lightColor * pulse * 0.5f);
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            // Draw glowing backlight effect when dropped in world
            Texture2D texture = TextureAssets.Item[Item.type].Value;
            Vector2 position = Item.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;
            
            // Calculate pulse - mystical and slow
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.05f) * 0.1f + 1f;
            
            // Begin additive blending for glow
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Outer dark crimson aura - funeral darkness
            spriteBatch.Draw(texture, position, null, new Color(80, 0, 20) * 0.5f, rotation, origin, scale * pulse * 1.4f, SpriteEffects.None, 0f);
            
            // Middle golden glow - heroic valor
            spriteBatch.Draw(texture, position, null, new Color(255, 180, 50) * 0.35f, rotation, origin, scale * pulse * 1.2f, SpriteEffects.None, 0f);
            
            // Inner scarlet/orange glow - flames of passion
            spriteBatch.Draw(texture, position, null, new Color(255, 100, 50) * 0.25f, rotation, origin, scale * pulse * 1.08f, SpriteEffects.None, 0f);
            
            // Return to normal blending
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Add lighting
            Lighting.AddLight(Item.Center, 0.7f, 0.4f, 0.3f);
            
            return true;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Create unique ID for this shot
            int shotId = Main.rand.Next(int.MaxValue);
            BeamHitTracking[shotId] = new HashSet<int>();
            
            // Spawn 5 tracking electric beams in 90-degree arc in front of player
            int beamCount = 5;
            int beamDamage = (int)(damage * 0.66f); // 66% of main weapon damage (10% increase)
            float spreadAngle = MathHelper.ToRadians(90); // 90 degree spread
            
            // Get direction towards cursor
            Vector2 towardsCursor = (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX);
            float baseAngle = towardsCursor.ToRotation();
            
            for (int i = 0; i < beamCount; i++)
            {
                // Spread beams evenly across 90 degrees
                float offsetAngle = spreadAngle * ((float)i / (beamCount - 1) - 0.5f);
                float finalAngle = baseAngle + offsetAngle;
                Vector2 beamVelocity = new Vector2(1f, 0f).RotatedBy(finalAngle) * 15f;
                
                // Pass shotId through ai[0]
                int projIndex = Projectile.NewProjectile(source, player.Center, beamVelocity,
                    ModContent.ProjectileType<FuneralPrayerBeam>(), beamDamage, knockback * 0.5f, player.whoAmI, shotId);
            }
            
            // === UnifiedVFX EROICA CAST EXPLOSION ===
            Vector2 castPos = player.Center + towardsCursor * 30f;
            UnifiedVFX.Eroica.Impact(castPos, 1.2f);
            
            // === FRACTAL BEAM BURST - Funeral prayer geometric pattern ===
            for (int i = 0; i < 5; i++)
            {
                float angle = MathHelper.TwoPi * i / 5f;
                Vector2 flareOffset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 25f;
                float progress = (float)i / 5f;
                Color fractalColor = Color.Lerp(UnifiedVFX.Eroica.Scarlet, UnifiedVFX.Eroica.Gold, progress);
                CustomParticles.GenericFlare(castPos + flareOffset, fractalColor, 0.5f, 19);
            }
            
            // Gradient halo rings
            for (int ring = 0; ring < 3; ring++)
            {
                float progress = (float)ring / 3f;
                Color ringColor = Color.Lerp(UnifiedVFX.Eroica.Scarlet, UnifiedVFX.Eroica.Gold, progress);
                CustomParticles.HaloRing(castPos, ringColor, 0.45f + ring * 0.1f, 15 + ring * 2);
            }
            
            // Musical burst on cast!
            ThemedParticles.EroicaMusicNotes(player.Center, 6, 30f);
            ThemedParticles.EroicaAccidentals(player.Center, 3, 20f);
            
            // Muzzle flash effect - subtle flare with glow and prismatic burst
            CustomParticles.EroicaFlare(player.Center, 0.5f);
            CustomParticles.GenericGlow(player.Center, new Color(255, 150, 80), 0.6f, 20);
            CustomParticles.PrismaticSparkleBurst(castPos, CustomParticleSystem.EroicaColors.Gold, 5);

            return false; // Don't spawn default projectile
        }
        
        public static void RegisterBeamHit(int shotId, int beamIndex)
        {
            if (!BeamHitTracking.ContainsKey(shotId))
                BeamHitTracking[shotId] = new HashSet<int>();
                
            BeamHitTracking[shotId].Add(beamIndex);
            
            // Check if all 5 beams have hit
            if (BeamHitTracking[shotId].Count >= 5)
            {
                // Spawn ricochet beam
                Player player = Main.player.FirstOrDefault(p => p.active);
                if (player != null)
                {
                    // Find nearest enemy to spawn ricochet beam
                    NPC nearestEnemy = null;
                    float minDist = 1000f;
                    for (int i = 0; i < Main.maxNPCs; i++)
                    {
                        NPC npc = Main.npc[i];
                        if (npc.active && !npc.friendly && npc.lifeMax > 5)
                        {
                            float dist = Vector2.Distance(player.Center, npc.Center);
                            if (dist < minDist)
                            {
                                minDist = dist;
                                nearestEnemy = npc;
                            }
                        }
                    }
                    
                    if (nearestEnemy != null)
                    {
                        Vector2 direction = (nearestEnemy.Center - player.Center).SafeNormalize(Vector2.UnitX);
                        Projectile.NewProjectile(player.GetSource_FromThis(), player.Center, direction * 20f,
                            ModContent.ProjectileType<FuneralPrayerRicochetBeam>(), 400, 10f, player.whoAmI);
                        
                        // === PRAYER ANSWERED - THE FIVE FLAMES UNITE! ===
                        // This is THE moment - all 5 beams connected, time for spectacular VFX!
                        
                        // Massive prayer circle glyph formation
                        CustomParticles.GlyphCircle(player.Center, UnifiedVFX.Eroica.Gold, 10, 80f, 0.15f);
                        CustomParticles.GlyphCircle(player.Center, UnifiedVFX.Eroica.Scarlet, 5, 50f, -0.1f);
                        
                        // Divine golden flash - the prayer is heard!
                        CustomParticles.GenericFlare(player.Center, Color.White, 1.5f, 25);
                        CustomParticles.GenericFlare(player.Center, UnifiedVFX.Eroica.Gold, 1.2f, 30);
                        
                        // Five-pointed star of funeral flames converging
                        for (int star = 0; star < 5; star++)
                        {
                            float starAngle = MathHelper.TwoPi * star / 5f - MathHelper.PiOver2;
                            Vector2 starPoint = player.Center + starAngle.ToRotationVector2() * 100f;
                            
                            // Draw lines from each point to center (prayer beams converging)
                            for (int point = 0; point < 8; point++)
                            {
                                float lerp = point / 8f;
                                Vector2 linePos = Vector2.Lerp(starPoint, player.Center, lerp);
                                Color lineColor = Color.Lerp(UnifiedVFX.Eroica.Scarlet, UnifiedVFX.Eroica.Gold, lerp);
                                CustomParticles.GenericFlare(linePos, lineColor, 0.4f - lerp * 0.2f, 18);
                            }
                            
                            // Mourning candle flame at each star point
                            CustomParticles.GenericFlare(starPoint, UnifiedVFX.Eroica.Gold, 0.7f, 25);
                            CustomParticles.HaloRing(starPoint, UnifiedVFX.Eroica.Scarlet * 0.7f, 0.3f, 15);
                        }
                        
                        // Cascading halo rings expanding outward - divine ripples
                        for (int wave = 0; wave < 6; wave++)
                        {
                            float delay = wave * 0.12f;
                            Color waveColor = Color.Lerp(UnifiedVFX.Eroica.Gold, Color.White, wave / 6f) * (1f - wave * 0.1f);
                            CustomParticles.HaloRing(player.Center, waveColor, 0.6f + wave * 0.2f, 20 + wave * 5);
                        }
                        
                        // MUSIC NOTES CASCADE - A funeral hymn rises!
                        ThemedParticles.MusicNoteBurst(player.Center, UnifiedVFX.Eroica.Gold, 12, 6f);
                        ThemedParticles.EroicaMusicNotes(player.Center, 16, 70f);
                        
                        // Rising prayer smoke - souls ascending
                        for (int smoke = 0; smoke < 12; smoke++)
                        {
                            float smokeAngle = MathHelper.TwoPi * smoke / 12f;
                            Vector2 smokePos = player.Center + smokeAngle.ToRotationVector2() * 40f;
                            Vector2 smokeVel = new Vector2(0, -3f) + Main.rand.NextVector2Circular(1f, 0.5f);
                            var smokeParticle = new HeavySmokeParticle(smokePos, smokeVel, 
                                Color.Lerp(UnifiedVFX.Eroica.Scarlet, new Color(30, 10, 10), 0.5f),
                                Main.rand.Next(40, 60), 0.4f, 0.6f, 0.015f, false);
                            MagnumParticleHandler.SpawnParticle(smokeParticle);
                        }
                        
                        // Sakura petals spiral upward - heroic tribute
                        ThemedParticles.SakuraPetals(player.Center, 20, 80f);
                    }
                }
                
                // Clean up tracking
                BeamHitTracking.Remove(shotId);
            }
        }

        // Recipe removed - drops from Eroica, God of Valor
        // public override void AddRecipes()
        // {
        //     CreateRecipe()
        //         .AddIngredient(ModContent.ItemType<ResonantCoreOfEroica>(), 25)
        //         .AddIngredient(ModContent.ItemType<EroicasResonantEnergy>(), 20)
        //         .AddIngredient(ItemID.LunarBar, 15)
        //         .AddTile(TileID.LunarCraftingStation)
        //         .Register();
        // }
    }
}
