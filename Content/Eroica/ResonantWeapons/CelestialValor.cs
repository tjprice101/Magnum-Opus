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
            
            // === UnifiedVFX EROICA AMBIENT AURA ===
            UnifiedVFX.Eroica.Aura(player.Center, 40f, 0.35f);
            
            // === SUBTLE AMBIENT FLARES ===
            if (Main.rand.NextBool(15))
            {
                float angle = Main.GameUpdateCount * 0.025f;
                float radius = 32f + (float)Math.Sin(Main.GameUpdateCount * 0.05f) * 8f;
                Vector2 flarePos = player.Center + angle.ToRotationVector2() * radius;
                Color fractalColor = Color.Lerp(UnifiedVFX.Eroica.Scarlet, UnifiedVFX.Eroica.Gold, 0.5f);
                CustomParticles.GenericFlare(flarePos, fractalColor, 0.28f, 15);
            }
            
            // Sakura petal drift
            if (Main.rand.NextBool(14))
                ThemedParticles.SakuraPetals(player.Center, 1, 30f);
            
            // Occasional prismatic sparkle
            if (Main.rand.NextBool(18))
            {
                CustomParticles.PrismaticSparkle(player.Center + Main.rand.NextVector2Circular(25f, 25f), UnifiedVFX.Eroica.Gold, 0.22f);
            }
            
            // Pulsing heroic light with gradient color
            float pulse = (float)Math.Sin(visualTimer * 0.05f) * 0.12f + 0.88f;
            Color lightColor = Color.Lerp(UnifiedVFX.Eroica.Crimson, UnifiedVFX.Eroica.Gold, pulse);
            Lighting.AddLight(player.Center, lightColor.ToVector3() * 0.5f * pulse);
        }

        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            Vector2 hitCenter = hitbox.Center.ToVector2();
            
            // === SPECTACULAR SWING SYSTEM - ENDGAME TIER (7-8 layered arcs + cosmic effects) ===
            SpectacularMeleeSwing.OnSwing(player, hitbox, UnifiedVFX.Eroica.Scarlet, UnifiedVFX.Eroica.Gold, 
                SpectacularMeleeSwing.SwingTier.Endgame, SpectacularMeleeSwing.WeaponTheme.Eroica);
            
            // === IRIDESCENT WINGSPAN STYLE - HEAVY DUST TRAILS (2+ per frame) ===
            for (int i = 0; i < 2; i++)
            {
                // Main heroic trail - Scarlet to Gold gradient
                float progress = Main.rand.NextFloat();
                Color dustColor = Color.Lerp(UnifiedVFX.Eroica.Scarlet, UnifiedVFX.Eroica.Gold, progress);
                int dustType = progress < 0.5f ? DustID.RedTorch : DustID.GoldFlame;
                Dust d = Dust.NewDustPerfect(hitCenter + Main.rand.NextVector2Circular(8f, 8f), dustType,
                    player.velocity * 0.3f + Main.rand.NextVector2Circular(3f, 3f),
                    0, dustColor, 1.7f);
                d.noGravity = true;
                d.fadeIn = 1.3f;
            }
            
            // === CONTRASTING SPARKLES - Sakura pink against gold ===
            if (Main.rand.NextBool(2))
            {
                Dust sakura = Dust.NewDustPerfect(hitCenter + Main.rand.NextVector2Circular(10f, 10f), DustID.PinkFairy,
                    player.velocity * 0.2f + Main.rand.NextVector2Circular(2f, 2f),
                    0, UnifiedVFX.Eroica.Sakura, 1.3f);
                sakura.noGravity = true;
            }
            
            // === VALOR SHIMMER - Like rainbow but with heroic tones ===
            if (Main.rand.NextBool(3))
            {
                // Cycle through valor colors: red -> crimson -> gold -> yellow
                float valorHue = 0.0f + Main.rand.NextFloat() * 0.12f; // Red-orange-yellow range
                Color valorShimmer = Main.hslToRgb(valorHue, 1f, 0.65f);
                Dust v = Dust.NewDustPerfect(hitCenter, DustID.GoldFlame,
                    player.velocity * 0.2f + Main.rand.NextVector2Circular(1.5f, 1.5f), 0, valorShimmer, 1.4f);
                v.noGravity = true;
            }
            
            // === FREQUENT FLARES ===
            if (Main.rand.NextBool(2))
            {
                float flareProgress = Main.rand.NextFloat();
                Color flareColor = Color.Lerp(UnifiedVFX.Eroica.Scarlet, UnifiedVFX.Eroica.Gold, flareProgress);
                CustomParticles.GenericFlare(hitCenter + Main.rand.NextVector2Circular(12f, 12f), flareColor, 0.45f, 16);
            }
            
            // Swing trail with occasional sword arc slash
            if (Main.rand.NextBool(3))
            {
                Vector2 swingDir = (hitCenter - player.Center).SafeNormalize(Vector2.UnitX);
                CustomParticles.SwordArcSlash(hitCenter, swingDir, CustomParticleSystem.EroicaColors.Scarlet * 0.7f, 0.35f, swingDir.ToRotation());
            }
            
            // Sakura petals in swing arc (increased frequency)
            if (Main.rand.NextBool(3))
            {
                ThemedParticles.SakuraPetals(hitCenter, 1, 25f);
            }
            
            // === MUSIC NOTES - The hero's anthem! ===
            if (Main.rand.NextBool(6))
            {
                Color noteColor = Color.Lerp(UnifiedVFX.Eroica.Gold, UnifiedVFX.Eroica.Sakura, Main.rand.NextFloat());
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -1f);
                ThemedParticles.MusicNote(hitCenter, noteVel, noteColor, 0.35f, 30);
            }
        }

        public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            // === IRIDESCENT WINGSPAN STYLE - LAYERED IMPACT ===
            UnifiedVFX.Eroica.Impact(target.Center, 1.3f);
            
            // === MUSIC NOTES BURST! ===
            ThemedParticles.EroicaMusicNotes(target.Center, 8, 40f);
            
            // === GRADIENT HALO RINGS (3 stacked) ===
            for (int ring = 0; ring < 3; ring++)
            {
                float progress = (float)ring / 3f;
                Color ringColor = Color.Lerp(UnifiedVFX.Eroica.Scarlet, UnifiedVFX.Eroica.Gold, progress);
                CustomParticles.HaloRing(target.Center, ringColor, 0.4f + ring * 0.12f, 14 + ring * 3);
            }
            
            // === VALOR SHIMMER FLARE BURST ===
            for (int i = 0; i < 10; i++)
            {
                float valorHue = 0.0f + (i / 10f) * 0.12f;
                Color flareColor = Main.hslToRgb(valorHue, 1f, 0.7f);
                CustomParticles.GenericFlare(target.Center + Main.rand.NextVector2Circular(18f, 18f), flareColor, 0.55f, 22);
            }
            
            // === RADIAL DUST EXPLOSION ===
            for (int i = 0; i < 16; i++)
            {
                float angle = MathHelper.TwoPi * i / 16f;
                float progress = (float)i / 16f;
                Color dustColor = Color.Lerp(UnifiedVFX.Eroica.Scarlet, UnifiedVFX.Eroica.Gold, progress);
                int dustType = i % 2 == 0 ? DustID.GoldFlame : DustID.RedTorch;
                Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * Main.rand.NextFloat(5f, 10f);
                Dust d = Dust.NewDustPerfect(target.Center, dustType, vel, 0, dustColor, 1.7f);
                d.noGravity = true;
                d.fadeIn = 1.1f;
            }
            
            // === SAKURA PETAL BURST (enhanced) ===
            ThemedParticles.SakuraPetals(target.Center, 6, 40f);
            
            // === SAKURA SPARKLES ===
            for (int i = 0; i < 6; i++)
            {
                Dust sakura = Dust.NewDustPerfect(target.Center + Main.rand.NextVector2Circular(15f, 15f), DustID.PinkFairy,
                    Main.rand.NextVector2Circular(5f, 5f), 0, UnifiedVFX.Eroica.Sakura, 1.5f);
                sakura.noGravity = true;
            }
            
            // Spawn seeking crystals on crit - Celestial Valor power
            if (hit.Crit)
            {
                SeekingCrystalHelper.SpawnEroicaCrystals(
                    player.GetSource_ItemUse(Item),
                    target.Center,
                    (Main.MouseWorld - target.Center).SafeNormalize(Vector2.UnitX) * 8f,
                    (int)(damageDone * 0.25f),
                    Item.knockBack * 0.5f,
                    player.whoAmI,
                    5);
            }
            
            Lighting.AddLight(target.Center, 1.2f, 0.6f, 0.3f);
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
