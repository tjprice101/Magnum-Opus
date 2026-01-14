using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria.GameContent;
using MagnumOpus.Content.MoonlightSonata.ResonanceEnergies;
using MagnumOpus.Content.MoonlightSonata.Projectiles;
using MagnumOpus.Content.MoonlightSonata.CraftingStations;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.MoonlightSonata.Weapons
{
    /// <summary>
    /// Eternal Moon - A heavy sword that sends out waves of purple energy when swung.
    /// Fires multiple projectiles and occasionally a massive beam.
    /// Hold right-click to charge a devastating lunar storm attack!
    /// Applies Musical Dissonance debuff on hit.
    /// </summary>
    public class EternalMoon : ModItem
    {
        private int swingCounter = 0;
        
        // Charged melee attack config
        private ChargedMeleeConfig chargedConfig;
        
        private ChargedMeleeConfig GetChargedConfig()
        {
            if (chargedConfig == null)
            {
                chargedConfig = new ChargedMeleeConfig
                {
                    PrimaryColor = UnifiedVFX.MoonlightSonata.DarkPurple,
                    SecondaryColor = UnifiedVFX.MoonlightSonata.Silver,
                    ChargeTime = 55f,
                    SpawnThemeMusicNotes = (pos, count, radius) => ThemedParticles.MoonlightMusicNotes(pos, count, radius),
                    SpawnThemeExplosion = (pos, scale) => UnifiedVFX.MoonlightSonata.Explosion(pos, scale),
                    DrawThemeLightning = (start, end) => MagnumVFX.DrawMoonlightLightning(start, end, 14, 25f, 4, 0.5f)
                };
            }
            return chargedConfig;
        }

        public override void SetDefaults()
        {
            Item.width = 50;
            Item.height = 50;
            Item.damage = 300; // Balanced: ~1000 DPS (300 × 60/18)
            Item.DamageType = DamageClass.Melee;
            Item.useTime = 18; // Faster swing
            Item.useAnimation = 18;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 7f;
            Item.value = Item.buyPrice(gold: 25);
            Item.rare = ModContent.RarityType<MoonlightSonataRarity>();
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<EternalMoonWave>();
            Item.shootSpeed = 14f;
            Item.maxStack = 1;
        }

        public override void HoldItem(Player player)
        {
            // === CHARGED MELEE ATTACK SYSTEM ===
            var chargedPlayer = player.GetModPlayer<ChargedMeleePlayer>();
            
            // Start charging on right-click
            if (Main.mouseRight && !chargedPlayer.IsCharging && !chargedPlayer.IsReleasing)
            {
                chargedPlayer.TryStartCharging(Item, GetChargedConfig());
            }
            
            // Update charging state
            if (chargedPlayer.IsCharging || chargedPlayer.IsReleasing)
            {
                chargedPlayer.UpdateCharging(Main.mouseRight);
            }
            
            // === AMBIENT FRACTAL FLARES - Lunar geometric pattern ===
            if (Main.rand.NextBool(6))
            {
                // Moon phase orbital pattern
                float baseAngle = Main.GameUpdateCount * 0.02f;
                for (int i = 0; i < 4; i++)
                {
                    float angle = baseAngle + MathHelper.TwoPi * i / 4f;
                    float radius = 35f + (float)Math.Sin(Main.GameUpdateCount * 0.035f + i * MathHelper.PiOver2) * 12f;
                    Vector2 flarePos = player.Center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius;
                    // Moonlight colors: deep purple to silver
                    float hue = (Main.GameUpdateCount * 0.01f + i * 0.25f) % 1f;
                    Color fractalColor = Color.Lerp(CustomParticleSystem.MoonlightColors.DeepPurple, CustomParticleSystem.MoonlightColors.Silver, hue);
                    CustomParticles.GenericFlare(flarePos, fractalColor, 0.32f, 18);
                }
            }
            
            // Ethereal moonlight particles while holding - increased frequency and variety
            if (Main.rand.NextBool(3))
            {
                Vector2 offset = Main.rand.NextVector2Circular(30f, 30f);
                ThemedParticles.MoonlightAura(player.Center + offset, 22f);
            }
            
            // Custom particle moonlight glow - more frequent with prismatic accents
            if (Main.rand.NextBool(4))
            {
                CustomParticles.MoonlightFlare(player.Center + Main.rand.NextVector2Circular(25f, 25f), 0.35f);
                CustomParticles.PrismaticSparkle(player.Center + Main.rand.NextVector2Circular(30f, 30f), CustomParticleSystem.MoonlightColors.Lavender, 0.2f);
            }
            
            // Floating sparkle motes around the player
            if (Main.rand.NextBool(5))
            {
                Vector2 sparklePos = player.Center + Main.rand.NextVector2Circular(40f, 40f);
                Dust sparkle = Dust.NewDustPerfect(sparklePos, DustID.Enchanted_Pink, 
                    new Vector2(0, -0.5f) + Main.rand.NextVector2Circular(0.3f, 0.3f), 0, default, 0.9f);
                sparkle.noGravity = true;
                sparkle.fadeIn = 1.2f;
            }
            
            // Occasional bright flare pulse with halo
            if (Main.rand.NextBool(12))
            {
                CustomParticles.GenericFlare(player.Center, new Color(200, 150, 255), 0.5f, 20);
            }
            if (Main.rand.NextBool(25))
            {
                CustomParticles.HaloRing(player.Center, CustomParticleSystem.MoonlightColors.Lavender * 0.4f, 0.3f, 20);
            }
            
            // Soft purple lighting aura - stronger with pulse
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.04f) * 0.1f + 0.9f;
            Lighting.AddLight(player.Center, 0.45f * pulse, 0.25f * pulse, 0.65f * pulse);
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            // ENHANCED: Draw dramatic glowing backlight effect when dropped in world
            Texture2D texture = TextureAssets.Item[Item.type].Value;
            Vector2 position = Item.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;
            
            // Calculate dual pulse - layered rhythmic pulsing like moonlit waves
            float pulse1 = (float)Math.Sin(Main.GameUpdateCount * 0.035f) * 0.15f + 1f;
            float pulse2 = (float)Math.Sin(Main.GameUpdateCount * 0.055f + 1f) * 0.1f + 1f;
            float combinedPulse = pulse1 * pulse2;
            
            // Begin additive blending for glow
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // NEW: Outer ethereal corona - soft pink/purple haze
            spriteBatch.Draw(texture, position, null, new Color(100, 50, 140) * 0.3f, rotation, origin, scale * pulse2 * 1.6f, SpriteEffects.None, 0f);
            
            // Outer deep purple aura - eternal darkness (enhanced)
            spriteBatch.Draw(texture, position, null, new Color(80, 30, 120) * 0.5f, rotation, origin, scale * combinedPulse * 1.45f, SpriteEffects.None, 0f);
            
            // Middle violet glow - moonlight essence (enhanced)
            spriteBatch.Draw(texture, position, null, new Color(180, 100, 240) * 0.4f, rotation, origin, scale * combinedPulse * 1.25f, SpriteEffects.None, 0f);
            
            // NEW: Bright magenta accent layer
            spriteBatch.Draw(texture, position, null, new Color(255, 120, 220) * 0.25f, rotation, origin, scale * pulse1 * 1.12f, SpriteEffects.None, 0f);
            
            // Inner silver/lavender glow - lunar radiance (enhanced)
            spriteBatch.Draw(texture, position, null, new Color(240, 220, 255) * 0.35f, rotation, origin, scale * pulse1 * 1.06f, SpriteEffects.None, 0f);
            
            // NEW: Hot white core flare
            spriteBatch.Draw(texture, position, null, new Color(255, 255, 255) * 0.15f, rotation, origin, scale * 0.95f, SpriteEffects.None, 0f);
            
            // Return to normal blending
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Add stronger lighting
            Lighting.AddLight(Item.Center, 0.6f, 0.4f, 0.85f);
            
            return true;
        }

        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            // Purple particle trail when swinging - more frequent
            if (Main.rand.NextBool(2))
            {
                Vector2 hitCenter = new Vector2(hitbox.X + hitbox.Width / 2, hitbox.Y + hitbox.Height / 2);
                ThemedParticles.MoonlightTrail(hitCenter, player.velocity * 0.3f);
            }
            
            // NEW: Glowing swing trail sparkles
            Vector2 swingPos = new Vector2(hitbox.X + hitbox.Width / 2, hitbox.Y + hitbox.Height / 2);
            if (Main.rand.NextBool(2))
            {
                Dust glow = Dust.NewDustDirect(swingPos - new Vector2(8, 8), 16, 16, 
                    DustID.PurpleTorch, 0f, 0f, 150, default, 1.5f);
                glow.noGravity = true;
                glow.velocity = player.velocity * 0.1f + Main.rand.NextVector2Circular(1f, 1f);
            }
            
            // NEW: Crystal shimmers along swing arc
            if (Main.rand.NextBool(4))
            {
                Dust crystal = Dust.NewDustDirect(swingPos - new Vector2(5, 5), 10, 10, 
                    DustID.PurpleCrystalShard, 0f, 0f, 100, default, 1.0f);
                crystal.noGravity = true;
                crystal.velocity *= 0.3f;
            }
            
            // NEW: Occasional bright flare during swing
            if (Main.rand.NextBool(8))
            {
                CustomParticles.GenericFlare(swingPos, new Color(220, 180, 255), 0.4f, 15);
            }
        }

        public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Apply Musical Dissonance debuff
            target.AddBuff(ModContent.BuffType<Debuffs.MusicsDissonance>(), 180); // 3 seconds
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            swingCounter++;
            
            // Fire 3 waves in a spread pattern
            float spreadAngle = MathHelper.ToRadians(15f);
            for (int i = -1; i <= 1; i++)
            {
                Vector2 spreadVel = velocity.RotatedBy(spreadAngle * i);
                Projectile.NewProjectile(source, position, spreadVel, type, damage, knockback, player.whoAmI);
            }
            
            // Every 4th swing, fire 3 spinning star projectiles in quick succession
            if (swingCounter >= 4)
            {
                swingCounter = 0;
                
                // Fire 3 stars back-to-back with slight delay using ai[1] as spawn delay
                Vector2 beamVel = velocity.SafeNormalize(Vector2.UnitX) * 16f;
                for (int i = 0; i < 3; i++)
                {
                    // Slight spread and position offset for visual interest
                    float spreadOffset = MathHelper.ToRadians(5f * (i - 1)); // -5, 0, +5 degrees
                    Vector2 starVel = beamVel.RotatedBy(spreadOffset);
                    Vector2 startPos = position + velocity.SafeNormalize(Vector2.Zero) * (i * 8); // Stagger starting positions
                    
                    Projectile proj = Projectile.NewProjectileDirect(source, startPos, starVel, 
                        ModContent.ProjectileType<EternalMoonBeam>(), (int)(damage * 1.5f), knockback * 2f, player.whoAmI);
                    
                    // Use ai[1] to delay each star's movement slightly (they'll catch up visually)
                    proj.ai[1] = i * 3; // Frame delay for staggered launch effect
                }
                
                // Visual and audio feedback for stars - ENHANCED
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item122, position);
                
                // Burst of particles - more dramatic
                ThemedParticles.MoonlightBloomBurst(position, 2f);
                ThemedParticles.MoonlightSparkles(position, 15, 35f);
                CustomParticles.MoonlightBossAttack(position, 16);
                
                // Musical notes burst! - enhanced
                ThemedParticles.MoonlightMusicNotes(position, 12, 40f);
                ThemedParticles.MoonlightClef(position, Main.rand.NextBool(), 1.5f);
                CustomParticles.MoonlightMusicNotes(position, 8, 45f);
                
                // NEW: Dramatic flare burst on star launch
                CustomParticles.GenericFlare(position, new Color(220, 160, 255), 1.2f, 30);
                CustomParticles.GenericFlare(position, new Color(255, 200, 255), 0.8f, 25);
                
                // NEW: Halo effect
                CustomParticles.MoonlightHalo(position, 0.7f);
            }
            
            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<MoonlightsResonantEnergy>(), 30)
                .AddIngredient(ModContent.ItemType<ResonantCoreOfMoonlightSonata>(), 5)
                .AddIngredient(ModContent.ItemType<Enemies.ShardsOfMoonlitTempo>(), 10)
                .AddTile(ModContent.TileType<MoonlightAnvilTile>())
                .Register();
        }
    }
}
