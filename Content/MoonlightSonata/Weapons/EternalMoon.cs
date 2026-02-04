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

        public override void SetDefaults()
        {
            Item.width = 50;
            Item.height = 50;
            Item.damage = 300; // Balanced: ~1000 DPS (300 ÁE60/18)
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
            // Subtle ambient moonlight aura (Swan Lake benchmark: minimal HoldItem particles)
            if (Main.rand.NextBool(12))
            {
                Vector2 offset = Main.rand.NextVector2Circular(25f, 25f);
                CustomParticles.PrismaticSparkle(player.Center + offset, CustomParticleSystem.MoonlightColors.Lavender * 0.5f, 0.18f);
            }
            
            // Occasional soft dust
            if (Main.rand.NextBool(15))
            {
                Vector2 sparklePos = player.Center + Main.rand.NextVector2Circular(30f, 30f);
                Dust sparkle = Dust.NewDustPerfect(sparklePos, DustID.Enchanted_Pink, 
                    new Vector2(0, -0.3f), 0, default, 0.7f);
                sparkle.noGravity = true;
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
            Vector2 hitCenter = new Vector2(hitbox.X + hitbox.Width / 2, hitbox.Y + hitbox.Height / 2);
            
            // === SPECTACULAR SWING SYSTEM - ENDGAME TIER (7-8 layered arcs + lunar effects) ===
            SpectacularMeleeSwing.OnSwing(player, hitbox, UnifiedVFX.MoonlightSonata.DarkPurple, UnifiedVFX.MoonlightSonata.LightBlue, 
                SpectacularMeleeSwing.SwingTier.Endgame, SpectacularMeleeSwing.WeaponTheme.MoonlightSonata);
            
            // === CALAMITY-INSPIRED CIRCULAR SMEAR TRAIL ===
            // Calculate swing progress for arc effects
            float swingProgress = 1f - (float)player.itemAnimation / player.itemAnimationMax;
            
            // Crescent slash arc - ethereal lunar blade trail
            if (Main.rand.NextBool(2))
            {
                Vector2 slashDir = (player.itemRotation + MathHelper.PiOver4 * player.direction).ToRotationVector2();
                CustomParticles.SwordArcCrescent(hitCenter, slashDir * 5f, UnifiedVFX.MoonlightSonata.LightBlue * 0.8f, 0.4f);
            }
            
            // === MULTI-LAYER GLOW PARTICLES ===
            // Core moonlight trail with gradient
            ThemedParticles.MoonlightTrail(hitCenter, player.velocity * 0.3f);
            
            // Layered glow sparks - purple core fading to light blue edges
            for (int i = 0; i < 2; i++)
            {
                Vector2 sparkPos = hitCenter + Main.rand.NextVector2Circular(15f, 15f);
                float gradientProgress = Main.rand.NextFloat();
                Color sparkColor = Color.Lerp(UnifiedVFX.MoonlightSonata.DarkPurple, UnifiedVFX.MoonlightSonata.LightBlue, gradientProgress);
                
                var spark = new GenericGlowParticle(sparkPos, -player.velocity * 0.1f + Main.rand.NextVector2Circular(1f, 1f),
                    sparkColor, 0.25f + gradientProgress * 0.15f, 18, true);
                MagnumParticleHandler.SpawnParticle(spark);
            }
            
            // === MUSIC NOTE INTEGRATION - THE SINGING BLADE ===
            // Scatter music notes along the swing arc - VISIBLE SCALE 0.8f+
            if (Main.rand.NextBool(2))
            {
                Vector2 noteVel = (player.direction * Vector2.UnitX).RotatedByRandom(0.8f) * Main.rand.NextFloat(1.5f, 3f);
                Color noteColor = Color.Lerp(UnifiedVFX.MoonlightSonata.MediumPurple, UnifiedVFX.MoonlightSonata.Silver, Main.rand.NextFloat());
                float shimmer = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.15f) * 0.1f;
                ThemedParticles.MusicNote(hitCenter, noteVel, noteColor * 0.9f, 0.85f * shimmer, 35);
                
                // Sparkle companion for visibility
                CustomParticles.PrismaticSparkle(hitCenter + Main.rand.NextVector2Circular(5f, 5f), 
                    UnifiedVFX.MoonlightSonata.Silver * 0.5f, 0.2f);
            }
            
            // Crystal shimmers along swing arc - prismatic moonlight gems
            if (Main.rand.NextBool(3))
            {
                CustomParticles.PrismaticSparkle(hitCenter + Main.rand.NextVector2Circular(12f, 12f), 
                    UnifiedVFX.MoonlightSonata.Silver, 0.28f);
            }
            
            // Ethereal moonlight dust with reduced frequency for cleaner look
            if (Main.rand.NextBool(2))
            {
                Dust glow = Dust.NewDustDirect(hitCenter - new Vector2(8, 8), 16, 16, 
                    DustID.PurpleTorch, 0f, 0f, 150, default, 1.5f);
                glow.noGravity = true;
                glow.velocity = player.velocity * 0.1f + Main.rand.NextVector2Circular(1f, 1f);
            }
            
            // Occasional bright flare pulse with gradient halo
            if (Main.rand.NextBool(6))
            {
                CustomParticles.GenericFlare(hitCenter, UnifiedVFX.MoonlightSonata.LightPurple, 0.5f, 16);
                CustomParticles.HaloRing(hitCenter, UnifiedVFX.MoonlightSonata.MediumPurple * 0.4f, 0.25f, 12);
            }
        }

        public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Apply Musical Dissonance debuff
            target.AddBuff(ModContent.BuffType<Debuffs.MusicsDissonance>(), 180); // 3 seconds
            
            // === SEEKING CRYSTALS - on critical hits ===
            if (hit.Crit)
            {
                SeekingCrystalHelper.SpawnMoonlightCrystals(
                    player.GetSource_ItemUse(player.HeldItem),
                    target.Center,
                    new Vector2(player.direction * 8f, 0f),
                    (int)(damageDone * 0.22f),
                    player.HeldItem.knockBack,
                    player.whoAmI,
                    5
                );
            }
            
            // === MOONLIGHT IMPACT (Trust UnifiedVFX for core effect) ===
            UnifiedVFX.MoonlightSonata.Impact(target.Center, 0.9f);
            
            // Single halo accent
            CustomParticles.HaloRing(target.Center, UnifiedVFX.MoonlightSonata.MediumPurple * 0.5f, 0.35f, 15);
            
            // Gentle music notes
            ThemedParticles.MoonlightMusicNotes(target.Center, 3, 28f);
            
            // Dynamic lighting
            Lighting.AddLight(target.Center, UnifiedVFX.MoonlightSonata.LightBlue.ToVector3() * 0.8f);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            swingCounter++;
            Vector2 direction = velocity.SafeNormalize(Vector2.UnitX);
            
            // === SWING AURA VFX ON EVERY ATTACK ===
            UnifiedVFX.MoonlightSonata.SwingAura(position, direction, 0.8f);
            
            // Crescent slash arc effect
            CustomParticles.SwordArcCrescent(position, velocity * 0.5f, UnifiedVFX.MoonlightSonata.LightBlue, 0.5f);
            
            // Fire 3 waves in a spread pattern
            float spreadAngle = MathHelper.ToRadians(15f);
            for (int i = -1; i <= 1; i++)
            {
                Vector2 spreadVel = velocity.RotatedBy(spreadAngle * i);
                Projectile.NewProjectile(source, position, spreadVel, type, damage, knockback, player.whoAmI);
            }
            
            // Every 4th swing, fire 3 spinning star projectiles - THE CRESCENDO
            if (swingCounter >= 4)
            {
                swingCounter = 0;
                
                // Fire 3 stars back-to-back with slight delay using ai[1] as spawn delay
                Vector2 beamVel = direction * 16f;
                for (int i = 0; i < 3; i++)
                {
                    float spreadOffset = MathHelper.ToRadians(5f * (i - 1));
                    Vector2 starVel = beamVel.RotatedBy(spreadOffset);
                    Vector2 startPos = position + direction * (i * 8);
                    
                    Projectile proj = Projectile.NewProjectileDirect(source, startPos, starVel, 
                        ModContent.ProjectileType<EternalMoonBeam>(), (int)(damage * 1.5f), knockback * 2f, player.whoAmI);
                    proj.ai[1] = i * 3;
                }
                
                // === STAR LAUNCH (4th swing special - keep impactful but cleaner) ===
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item122 with { Volume = 0.8f }, position);
                
                // Central explosion
                UnifiedVFX.MoonlightSonata.Explosion(position, 0.8f);
                
                // Single halo ring
                CustomParticles.HaloRing(position, UnifiedVFX.MoonlightSonata.MediumPurple, 0.5f, 20);
                
                // Music notes burst
                ThemedParticles.MoonlightMusicNotes(position, 6, 40f);
                
                // Gentle spark spray
                for (int i = 0; i < 6; i++)
                {
                    float angle = MathHelper.TwoPi * i / 6f;
                    Vector2 sparkVel = angle.ToRotationVector2() * Main.rand.NextFloat(5f, 9f);
                    Color sparkColor = Color.Lerp(UnifiedVFX.MoonlightSonata.MediumPurple, UnifiedVFX.MoonlightSonata.LightBlue, (float)i / 6f);
                    
                    var spark = new GenericGlowParticle(position, sparkVel, sparkColor, 0.3f, 20, true);
                    MagnumParticleHandler.SpawnParticle(spark);
                }
            }
            
            return false;
        }

        public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Swings release waves of lunar energy"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Hold right-click to charge a devastating lunar storm"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Hits apply Musical Dissonance debuff"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The eternal cycle made blade'")
            {
                OverrideColor = new Microsoft.Xna.Framework.Color(140, 100, 200)
            });
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
