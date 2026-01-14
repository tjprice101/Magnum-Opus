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
    /// Celestial Valor - A mighty greatsword infused with heroic spirit.
    /// Swing 1: fires 1 projectile, Swing 2: fires 2, Swing 3: fires 3, then repeats.
    /// Rainbow rarity, drops from Eroica, God of Valor.
    /// Hold right-click to charge a devastating celestial storm attack!
    /// Uses 6x6 sprite sheet for swing animation (horizontal sprite).
    /// Features Calamity-inspired visual effects with glowing outline and pulsing aura.
    /// </summary>
    public class CelestialValor : ModItem
    {
        private int swingCounter = 0;
        private float visualTimer = 0f;
        
        // Charged melee attack config
        private ChargedMeleeConfig chargedConfig;
        
        private ChargedMeleeConfig GetChargedConfig()
        {
            if (chargedConfig == null)
            {
                chargedConfig = new ChargedMeleeConfig
                {
                    PrimaryColor = UnifiedVFX.Eroica.Scarlet,
                    SecondaryColor = UnifiedVFX.Eroica.Gold,
                    ChargeTime = 55f,
                    SpawnThemeMusicNotes = (pos, count, radius) => ThemedParticles.EroicaMusicNotes(pos, count, radius),
                    SpawnThemeExplosion = (pos, scale) => UnifiedVFX.Eroica.Explosion(pos, scale),
                    DrawThemeLightning = (start, end) => MagnumVFX.DrawEroicaLightning(start, end, 10, 35f, 3, 0.35f)
                };
            }
            return chargedConfig;
        }
        
        // 6x6 sprite sheet animation
        private const int FrameColumns = 6;
        private const int FrameRows = 6;
        private const int TotalFrames = 36;
        
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.damage = 320; // Balanced: ~1150 effective DPS with projectiles
            Item.DamageType = DamageClass.Melee;
            Item.width = 80;
            Item.height = 80;
            Item.useTime = 22;
            Item.useAnimation = 22;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 7.5f;
            Item.value = Item.sellPrice(gold: 45);
            Item.rare = ModContent.RarityType<EroicaRainbowRarity>();
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<CelestialValorProjectile>();
            Item.shootSpeed = 14f;
            Item.noMelee = false;
            Item.maxStack = 1;
            Item.scale = 1.3f;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Increment swing counter (1, 2, 3, then reset to 1)
            swingCounter++;
            if (swingCounter > 3)
                swingCounter = 1;
            
            // Get direction toward mouse
            Vector2 towardsMouse = (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX);
            
            // Fire projectiles based on swing count
            int projectileCount = swingCounter;
            float spreadAngle = projectileCount > 1 ? MathHelper.ToRadians(15f) : 0f;
            
            for (int i = 0; i < projectileCount; i++)
            {
                float angleOffset = 0f;
                if (projectileCount > 1)
                {
                    // Spread projectiles evenly
                    angleOffset = spreadAngle * ((float)i / (projectileCount - 1) - 0.5f) * 2f;
                }
                
                Vector2 projectileVelocity = towardsMouse.RotatedBy(angleOffset) * Item.shootSpeed;
                Projectile.NewProjectile(source, player.Center, projectileVelocity, type, (int)(damage * 0.88f), knockback, player.whoAmI);
            }
            
            // Heroic sword arc based on swing count - escalating visuals
            Vector2 arcPos = player.Center + towardsMouse * 40f;
            
            // UnifiedVFX impact based on swing intensity
            UnifiedVFX.Eroica.SwingAura(arcPos, towardsMouse, 0.6f + swingCounter * 0.3f);
            
            if (swingCounter == 1)
            {
                // Single crescent slash
                CustomParticles.SwordArcSlash(arcPos, towardsMouse, UnifiedVFX.Eroica.Gold, 0.5f);
            }
            else if (swingCounter == 2)
            {
                // Double helix intertwined slashes - scarlet and gold
                CustomParticles.SwordArcDoubleHelix(arcPos, towardsMouse * 4f, UnifiedVFX.Eroica.Scarlet, UnifiedVFX.Eroica.Gold, 0.5f);
                // Extra halo ring for combo
                CustomParticles.HaloRing(arcPos, UnifiedVFX.Eroica.Crimson, 0.4f, 15);
            }
            else
            {
                // Triple burst - heroic finale with full UnifiedVFX explosion
                UnifiedVFX.Eroica.Impact(arcPos, 1.2f);
                CustomParticles.SwordArcBurst(arcPos, UnifiedVFX.Eroica.Gold, 5, 0.5f);
                // Sakura petals for triumphant finish
                ThemedParticles.SakuraPetals(arcPos, 6, 40f);
            }
            
            // Fractal flare burst with gradient - signature MagnumOpus look
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 flareOffset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * (20f + swingCounter * 8f);
                float progress = (float)i / 6f;
                Color fractalColor = Color.Lerp(UnifiedVFX.Eroica.Scarlet, UnifiedVFX.Eroica.Gold, progress);
                CustomParticles.GenericFlare(arcPos + flareOffset, fractalColor, 0.35f + swingCounter * 0.1f, 16);
            }
            
            // Swing particles - reduced for cleaner look
            ThemedParticles.EroicaSparks(arcPos, towardsMouse, 4 + swingCounter, 6f);
            
            // Musical notes burst on swing
            ThemedParticles.EroicaMusicNotes(player.Center + towardsMouse * 30f, swingCounter + 2, 25f);
            
            return false;
        }

        public override void HoldItem(Player player)
        {
            // Increment visual timer for pulsing effects
            visualTimer += 1f;
            
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
            
            // === UnifiedVFX EROICA AMBIENT AURA ===
            UnifiedVFX.Eroica.Aura(player.Center, 40f, 0.35f);
            
            // === AMBIENT FRACTAL FLARES - Celestial star pattern ===
            if (Main.rand.NextBool(6))
            {
                // Celestial star burst pattern with gradient
                float baseAngle = Main.GameUpdateCount * 0.025f;
                for (int i = 0; i < 5; i++)
                {
                    float angle = baseAngle + MathHelper.TwoPi * i / 5f;
                    float radius = 35f + (float)Math.Sin(Main.GameUpdateCount * 0.05f + i * 0.7f) * 12f;
                    Vector2 flarePos = player.Center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius;
                    // Gradient colors: scarlet to gold
                    float progress = (float)i / 5f;
                    Color fractalColor = Color.Lerp(UnifiedVFX.Eroica.Scarlet, UnifiedVFX.Eroica.Gold, progress);
                    CustomParticles.GenericFlare(flarePos, fractalColor, 0.35f, 18);
                }
            }
            
            // Sakura petal drift
            if (Main.rand.NextBool(12))
                ThemedParticles.SakuraPetals(player.Center, 1, 35f);
            
            // Occasional prismatic sparkle - heroic gleam with mini fractals
            if (Main.rand.NextBool(10))
            {
                CustomParticles.PrismaticSparkle(player.Center + Main.rand.NextVector2Circular(28f, 28f), UnifiedVFX.Eroica.Gold, 0.25f);
            }
            
            // Celestial halo pulse
            if (Main.rand.NextBool(18))
            {
                CustomParticles.HaloRing(player.Center, UnifiedVFX.Eroica.Gold * 0.5f, 0.35f, 22);
            }
            
            // Pulsing heroic light with gradient color
            float pulse = (float)Math.Sin(visualTimer * 0.05f) * 0.12f + 0.88f;
            Color lightColor = Color.Lerp(UnifiedVFX.Eroica.Crimson, UnifiedVFX.Eroica.Gold, pulse);
            Lighting.AddLight(player.Center, lightColor.ToVector3() * 0.5f * pulse);
        }

        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            // Swing trail with occasional sword arc slash
            if (Main.rand.NextBool(4))
            {
                Vector2 hitCenter = new Vector2(hitbox.X + hitbox.Width / 2, hitbox.Y + hitbox.Height / 2);
                Vector2 swingDir = (hitCenter - player.Center).SafeNormalize(Vector2.UnitX);
                CustomParticles.SwordArcSlash(hitCenter, swingDir, CustomParticleSystem.EroicaColors.Scarlet * 0.7f, 0.3f, swingDir.ToRotation());
            }
            
            // Themed trail particles
            if (Main.rand.NextBool(3))
            {
                Vector2 hitCenter = new Vector2(hitbox.X + hitbox.Width / 2, hitbox.Y + hitbox.Height / 2);
                ThemedParticles.EroicaTrail(hitCenter, player.velocity * 0.25f);
            }
        }

        public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            // === UnifiedVFX EROICA IMPACT - Full themed explosion ===
            UnifiedVFX.Eroica.Impact(target.Center, 1.3f);
            
            // === FRACTAL IMPACT BURST - Celestial star explosion with gradient ===
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 flareOffset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 32f;
                float progress = (float)i / 8f;
                Color fractalColor = Color.Lerp(UnifiedVFX.Eroica.Scarlet, UnifiedVFX.Eroica.Gold, progress);
                CustomParticles.GenericFlare(target.Center + flareOffset, fractalColor, 0.55f, 20);
            }
            
            // Heroic impact - golden prismatic burst
            CustomParticles.PrismaticSparkleBurst(target.Center, UnifiedVFX.Eroica.Gold, 8);
            
            // Impact halos - layered gradient cascade
            for (int ring = 0; ring < 3; ring++)
            {
                float progress = (float)ring / 3f;
                Color ringColor = Color.Lerp(UnifiedVFX.Eroica.Scarlet, UnifiedVFX.Eroica.Gold, progress);
                CustomParticles.HaloRing(target.Center, ringColor, 0.5f + ring * 0.15f, 14 + ring * 3);
            }
            
            // Sakura petal burst on hit
            ThemedParticles.SakuraPetals(target.Center, 4, 35f);
            
            // Musical accidentals on hit
            CustomParticles.EroicaMusicNotes(target.Center, 2, 18f);
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            // Draw glowing backlight effect when dropped in world
            Texture2D texture = TextureAssets.Item[Item.type].Value;
            Vector2 position = Item.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;
            
            // Calculate pulse
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.05f) * 0.15f + 1f;
            
            // Begin additive blending for glow
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Outer scarlet glow
            spriteBatch.Draw(texture, position, null, new Color(255, 80, 60) * 0.4f, rotation, origin, scale * pulse * 1.3f, SpriteEffects.None, 0f);
            
            // Inner golden glow
            spriteBatch.Draw(texture, position, null, new Color(255, 200, 100) * 0.3f, rotation, origin, scale * pulse * 1.15f, SpriteEffects.None, 0f);
            
            // Return to normal blending
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Add lighting
            Lighting.AddLight(Item.Center, 0.8f, 0.4f, 0.2f);
            
            return true; // Draw the normal sprite too
        }

        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            // Use the inventory image for inventory display
            return true;
        }

        public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "EroicaWeapon", "A blade forged from the valor of countless heroes")
            {
                OverrideColor = new Color(255, 200, 100)
            });
            tooltips.Add(new TooltipLine(Mod, "SwingCombo", "Fires 1/2/3 projectiles on consecutive swings")
            {
                OverrideColor = new Color(255, 150, 100)
            });
        }
    }
}
