using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Terraria.GameContent;
using MagnumOpus.Content.Eroica.Projectiles;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.Eroica.ResonantWeapons
{
    /// <summary>
    /// Piercing Light of the Sakura - A fast-firing rifle that channels the essence of valor.
    /// Every 10th shot fires a special sakura projectile that calls down black, gold, and red lightning.
    /// Rainbow rarity, drops from Eroica, God of Valor.
    /// </summary>
    public class PiercingLightOfTheSakura : ModItem
    {
        private int shotCounter = 0;
        
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.damage = 155; // Balanced: ~1163 DPS (155 ÁE60/8)
            Item.DamageType = DamageClass.Ranged;
            Item.width = 64;
            Item.height = 24;
            Item.useTime = 8; // Very fast firing
            Item.useAnimation = 8;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 2.5f;
            Item.value = Item.sellPrice(gold: 40);
            Item.rare = ModContent.RarityType<EroicaRainbowRarity>();
            Item.UseSound = SoundID.Item11;
            Item.autoReuse = true;
            Item.shoot = ProjectileID.Bullet;
            Item.shootSpeed = 18f;
            Item.useAmmo = AmmoID.Bullet;
            Item.noMelee = true;
            Item.maxStack = 1;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            shotCounter++;
            
            // GRADIENT COLORS: Scarlet ↁECrimson ↁEGold
            Color eroicaScarlet = new Color(139, 0, 0);
            Color eroicaCrimson = new Color(220, 50, 50);
            Color eroicaGold = new Color(255, 215, 0);
            
            // Muzzle flash with gradient custom particles
            for (int i = 0; i < 5; i++)
            {
                float progress = (float)i / 5f;
                Color flashColor = Color.Lerp(eroicaScarlet, eroicaGold, progress);
                CustomParticles.GenericFlare(position, flashColor, 0.35f + progress * 0.2f, 12);
            }
            CustomParticles.HaloRing(position, eroicaCrimson, 0.3f, 10);
            
            // Every 10th shot, fire the special sakura lightning projectile
            if (shotCounter >= 10)
            {
                shotCounter = 0;
                
                // Fire the special projectile instead of normal bullet
                Projectile.NewProjectile(source, position, velocity * 1.2f, 
                    ModContent.ProjectileType<PiercingLightOfTheSakuraProjectile>(), 
                    (int)(damage * 2.5f), knockback * 2f, player.whoAmI);
                
                // === SPAWN SEEKING CRYSTALS - HEROIC VALOR BURST ===
                SeekingCrystalHelper.SpawnEroicaCrystals(
                    source,
                    position + velocity.SafeNormalize(Vector2.UnitX) * 25f,
                    velocity * 0.6f,
                    (int)(damage * 0.4f),
                    knockback,
                    player.whoAmI,
                    5  // 5 golden crystals
                );
                
                // Special firing effect with gradient burst
                SoundEngine.PlaySound(SoundID.Item125 with { Pitch = 0.3f, Volume = 0.8f }, position);
                
                // Single halo burst
                CustomParticles.HaloRing(position, eroicaGold, 0.5f, 16);
                
                // Music notes and petals - reduced
                ThemedParticles.EroicaMusicNotes(position, 3, 28f);
                ThemedParticles.SakuraPetals(position, 3, 25f);
                
                // Central white flash and themed effects
                CustomParticles.GenericFlare(position, Color.White, 0.8f, 12);
                ThemedParticles.EroicaHaloBurst(position, 0.8f);
                
                return false; // Don't fire normal bullet
            }
            
            // Normal bullet with tracer effect - spawn black bullet
            int proj = Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            // Make bullet appear darker with black trail particles
            if (proj >= 0 && proj < Main.maxProjectiles)
            {
                Main.projectile[proj].alpha = 200; // Darken the bullet
            }
            
            // Add gradient tracer particles to normal shots
            for (int i = 0; i < 3; i++)
            {
                float progress = (float)i / 3f;
                Color tracerColor = Color.Lerp(eroicaScarlet, eroicaCrimson, progress);
                CustomParticles.GenericGlow(position, tracerColor, 0.25f, 8);
            }
            
            return false;
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-8f, 0f);
        }

        public override void HoldItem(Player player)
        {
            // === UNIQUE: CHARGING SHOT COUNTER VISUALIZATION ===
            // This weapon's identity is the 10th shot special - SHOW IT VISUALLY!
            
            float chargeProgress = shotCounter / 9f; // 0 to 1 as we approach shot 10
            Vector2 gunTip = player.Center + new Vector2(45f * player.direction, -3f);
            
            // === CHARGE ORB FORMATION ===
            // 9 orbiting points appear one by one as shots are fired
            if (shotCounter > 0)
            {
                float orbitRadius = 18f + chargeProgress * 8f;
                float rotationSpeed = Main.GameUpdateCount * 0.04f;
                
                for (int i = 0; i < shotCounter; i++)
                {
                    float angle = rotationSpeed + MathHelper.TwoPi * i / 9f;
                    Vector2 orbPos = gunTip + angle.ToRotationVector2() * orbitRadius;
                    
                    // Color transitions from scarlet (1st shot) to gold (9th shot)
                    float colorProgress = i / 8f;
                    Color orbColor = Color.Lerp(UnifiedVFX.Eroica.Scarlet, UnifiedVFX.Eroica.Gold, colorProgress);
                    
                    // Each orb pulses and grows slightly as charge builds
                    float orbPulse = (float)Math.Sin(Main.GameUpdateCount * 0.1f + i * 0.5f) * 0.1f + 1f;
                    float orbScale = 0.2f + chargeProgress * 0.15f;
                    CustomParticles.GenericFlare(orbPos, orbColor, orbScale * orbPulse, 4);
                }
                
                // === CENTRAL CHARGE CORE ===
                // The core grows brighter as we approach the 10th shot
                if (chargeProgress > 0.3f)
                {
                    float coreIntensity = (chargeProgress - 0.3f) / 0.7f;
                    Color coreColor = Color.Lerp(UnifiedVFX.Eroica.Crimson, Color.White, coreIntensity * 0.5f);
                    CustomParticles.GenericFlare(gunTip, coreColor, 0.25f + coreIntensity * 0.3f, 3);
                    
                    // Mini lightning arcs between orbs when charge is high
                    if (chargeProgress > 0.6f && Main.rand.NextBool(8))
                    {
                        CustomParticles.HaloRing(gunTip, UnifiedVFX.Eroica.Gold * 0.6f, 0.15f + coreIntensity * 0.1f, 8);
                    }
                }
                
                // === CONVERGENCE WARNING ===
                // At 8-9 shots, particles start converging toward gun tip
                if (shotCounter >= 7)
                {
                    float urgency = (shotCounter - 6) / 3f;
                    if (Main.rand.NextBool((int)(6 - urgency * 4)))
                    {
                        Vector2 spawnPos = gunTip + Main.rand.NextVector2CircularEdge(35f, 35f);
                        Vector2 convergeVel = (gunTip - spawnPos).SafeNormalize(Vector2.Zero) * (2f + urgency * 2f);
                        Color convergeColor = Color.Lerp(UnifiedVFX.Eroica.Sakura, UnifiedVFX.Eroica.Gold, urgency);
                        var convergeParticle = new GenericGlowParticle(spawnPos, convergeVel, convergeColor, 0.2f, 12, true);
                        MagnumParticleHandler.SpawnParticle(convergeParticle);
                    }
                }
            }
            
            // === AMBIENT SAKURA PETALS - gentle when uncharged, intense when charged ===
            if (Main.rand.NextBool((int)(12 - chargeProgress * 6)))
            {
                ThemedParticles.SakuraPetals(player.Center + Main.rand.NextVector2Circular(20f, 20f), 1, 15f);
            }
            
            // === MUSIC NOTE - The building crescendo ===
            if (chargeProgress > 0.5f && Main.rand.NextBool(15))
            {
                ThemedParticles.EroicaMusicNotes(gunTip, 1, 20f);
            }
            
            // Dynamic lighting based on charge
            float lightIntensity = 0.3f + chargeProgress * 0.4f;
            Color lightColor = Color.Lerp(UnifiedVFX.Eroica.Scarlet, UnifiedVFX.Eroica.Gold, chargeProgress);
            Lighting.AddLight(gunTip, lightColor.ToVector3() * lightIntensity);
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            // Draw glowing backlight effect when dropped in world
            Texture2D texture = TextureAssets.Item[Item.type].Value;
            Vector2 position = Item.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;
            
            // Calculate pulse - sharp and piercing like lightning
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.07f) * 0.12f + 1f;
            float flicker = Main.rand.NextBool(15) ? 1.2f : 1f;
            
            // Begin additive blending for glow
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Outer black/shadow aura - darkness before lightning (Eroica black/crimson)
            spriteBatch.Draw(texture, position, null, new Color(30, 20, 25) * 0.4f * flicker, rotation, origin, scale * pulse * 1.4f, SpriteEffects.None, 0f);
            
            // Middle crimson/scarlet glow - sakura blood
            spriteBatch.Draw(texture, position, null, new Color(200, 50, 50) * 0.35f * flicker, rotation, origin, scale * pulse * 1.2f, SpriteEffects.None, 0f);
            
            // Inner gold/white glow - piercing light
            spriteBatch.Draw(texture, position, null, new Color(255, 220, 150) * 0.28f * flicker, rotation, origin, scale * pulse * 1.08f, SpriteEffects.None, 0f);
            
            // Return to normal blending
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Add lighting
            Lighting.AddLight(Item.Center, 0.6f, 0.35f, 0.25f);
            
            return true;
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            // Make normal bullets black by using a custom projectile color
            // This is handled in Shoot method by setting bullet alpha/color
        }

        public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "EroicaWeapon", "The light of fallen heroes guides each shot")
            {
                OverrideColor = new Color(255, 200, 100)
            });
        }

        // No recipe - drops from boss
    }
}
