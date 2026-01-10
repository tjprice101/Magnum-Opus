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
            // Dark red and gold particles while holding
            if (Main.rand.NextBool(2))
            {
                Vector2 offset = Main.rand.NextVector2Circular(22f, 22f);
                int dustType = Main.rand.NextBool() ? DustID.GoldFlame : DustID.Torch;
                Dust particle = Dust.NewDustDirect(player.Center + offset, 1, 1, dustType, 0f, -1.2f, 150, default, 1.0f);
                particle.noGravity = true;
                particle.velocity *= 0.35f;
                if (dustType == DustID.Torch)
                    particle.color = new Color(139, 0, 0); // Dark red
            }
            
            // Custom particle dark flames
            if (Main.rand.NextBool(5))
            {
                CustomParticles.EroicaFlare(player.Center + Main.rand.NextVector2Circular(20f, 20f), 0.3f);
            }
            
            // Soft heroic lighting
            Lighting.AddLight(player.Center, 0.5f, 0.3f, 0.2f);
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
            
            // Musical burst on cast!
            ThemedParticles.EroicaMusicNotes(player.Center, 6, 30f);
            ThemedParticles.EroicaAccidentals(player.Center, 3, 20f);
            
            // Muzzle flash effect - subtle flare with glow
            CustomParticles.EroicaFlare(player.Center, 0.5f);
            CustomParticles.GenericGlow(player.Center, new Color(255, 150, 80), 0.6f, 20);

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
