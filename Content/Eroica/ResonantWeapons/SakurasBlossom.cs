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

namespace MagnumOpus.Content.Eroica.ResonantWeapons
{
    /// <summary>
    /// Sakura's Blossom - Melee weapon that creates spectral copies seeking enemies.
    /// Rainbow rarity, higher tier than Moonlight weapons.
    /// Hold right-click to charge a devastating sakura storm attack!
    /// </summary>
    public class SakurasBlossom : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.damage = 350; // Balanced: ~1050 DPS (350 ÁE60/20)
            Item.DamageType = DamageClass.Melee;
            Item.width = 70;
            Item.height = 70;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 8f;
            Item.value = Item.sellPrice(gold: 40);
            Item.rare = ModContent.RarityType<EroicaRainbowRarity>();
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.useTurn = false;
            Item.shoot = ModContent.ProjectileType<SakurasBlossomSpectral>();
            Item.shootSpeed = 10f;
            Item.maxStack = 1;
        }

        public override void HoldItem(Player player)
        {
            // === UnifiedVFX EROICA AMBIENT AURA ===
            UnifiedVFX.Eroica.Aura(player.Center, 32f, 0.3f);
            
            // === SUBTLE SAKURA AMBIENT ===
            if (Main.rand.NextBool(15))
            {
                float angle = Main.GameUpdateCount * 0.03f;
                float radius = 30f + (float)Math.Sin(Main.GameUpdateCount * 0.05f) * 10f;
                Vector2 flarePos = player.Center + angle.ToRotationVector2() * radius;
                Color fractalColor = Color.Lerp(UnifiedVFX.Eroica.Sakura, UnifiedVFX.Eroica.Gold, 0.4f);
                CustomParticles.GenericFlare(flarePos, fractalColor, 0.25f, 14);
            }
            
            // Sakura petals floating
            if (Main.rand.NextBool(12))
            {
                ThemedParticles.SakuraPetals(player.Center + Main.rand.NextVector2Circular(25f, 25f), 1, 20f);
            }
            
            // Occasional prismatic sparkle
            if (Main.rand.NextBool(15))
            {
                CustomParticles.PrismaticSparkle(player.Center + Main.rand.NextVector2Circular(25f, 25f), UnifiedVFX.Eroica.Sakura, 0.2f);
            }
            
            // Heroic gradient light with pulse
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.06f) * 0.1f + 0.9f;
            Vector3 lightColor = Color.Lerp(UnifiedVFX.Eroica.Scarlet, UnifiedVFX.Eroica.Gold, 0.4f).ToVector3();
            Lighting.AddLight(player.Center, lightColor * pulse * 0.55f);
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            // Draw glowing backlight effect when dropped in world
            Texture2D texture = TextureAssets.Item[Item.type].Value;
            Vector2 position = Item.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;
            
            // Calculate pulse - powerful and blossoming
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.055f) * 0.12f + 1f;
            
            // Begin additive blending for glow
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Outer deep scarlet aura - sakura's spirit
            spriteBatch.Draw(texture, position, null, new Color(180, 40, 50) * 0.45f, rotation, origin, scale * pulse * 1.38f, SpriteEffects.None, 0f);
            
            // Middle crimson/pink glow - cherry blossom
            spriteBatch.Draw(texture, position, null, new Color(255, 100, 120) * 0.35f, rotation, origin, scale * pulse * 1.2f, SpriteEffects.None, 0f);
            
            // Inner golden/white glow - valor's light
            spriteBatch.Draw(texture, position, null, new Color(255, 230, 180) * 0.25f, rotation, origin, scale * pulse * 1.08f, SpriteEffects.None, 0f);
            
            // Return to normal blending
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Add lighting
            Lighting.AddLight(Item.Center, 0.65f, 0.35f, 0.3f);
            
            return true;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Get cursor direction
            Vector2 cursorDirection = velocity;
            cursorDirection.Normalize();

            // Create 3 spectral swords in a 100 degree cone towards cursor
            float spreadAngle = MathHelper.ToRadians(100f); // 100 degree total spread
            float startAngle = -spreadAngle / 2f; // Start at -50 degrees from center

            for (int i = 0; i < 3; i++)
            {
                // Evenly distribute the 3 swords across the 100 degree cone
                float angle = startAngle + (spreadAngle / 2f) * i; // -50°, 0°, +50°
                Vector2 spectralVelocity = cursorDirection.RotatedBy(angle) * 15f;

                Projectile.NewProjectile(source, player.Center, spectralVelocity,
                    ModContent.ProjectileType<SakurasBlossomSpectral>(), damage, knockback, player.whoAmI);
            }
            
            // Musical burst on swing!
            ThemedParticles.EroicaMusicNotes(player.Center + cursorDirection * 30f, 4, 25f);

            return false;
        }

        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            // === SPECTACULAR SWING SYSTEM - ENDGAME TIER (7-8 layered arcs + sakura effects) ===
            SpectacularMeleeSwing.OnSwing(player, hitbox, UnifiedVFX.Eroica.Sakura, UnifiedVFX.Eroica.Gold, 
                SpectacularMeleeSwing.SwingTier.Endgame, SpectacularMeleeSwing.WeaponTheme.Eroica);
            
            // === BLOOMING SAKURA EFFECT ===
            // The blade LITERALLY blooms with petals on every swing - a flower unfurling
            
            Vector2 hitboxCenter = hitbox.Center.ToVector2();
            float swingProgress = player.itemAnimation / (float)player.itemAnimationMax;
            float bloomIntensity = (float)Math.Sin(swingProgress * MathHelper.Pi); // Peak at mid-swing
            
            // === SAKURA PETAL BLOOM - Petals spiral outward from the blade ===
            if (Main.rand.NextBool(2))
            {
                // Petals emerge from the blade edge like a flower opening
                int petalCount = 2 + (int)(bloomIntensity * 3);
                for (int i = 0; i < petalCount; i++)
                {
                    Vector2 petalPos = hitboxCenter + Main.rand.NextVector2Circular(hitbox.Width * 0.4f, hitbox.Height * 0.4f);
                    float petalAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                    Vector2 petalVel = petalAngle.ToRotationVector2() * Main.rand.NextFloat(1.5f, 4f);
                    petalVel.Y -= 1.5f; // Petals drift upward like they're caught in wind
                    
                    // Color gradient from deep pink to pale sakura
                    float colorProgress = Main.rand.NextFloat();
                    Color petalColor = Color.Lerp(UnifiedVFX.Eroica.Sakura, new Color(255, 200, 220), colorProgress);
                    
                    var petal = new GenericGlowParticle(petalPos, petalVel, petalColor, 0.25f + bloomIntensity * 0.15f, 25, true);
                    MagnumParticleHandler.SpawnParticle(petal);
                }
            }
            
            // === GOLDEN POLLEN MOTES - Sparkling pollen released from the bloom ===
            if (bloomIntensity > 0.3f && Main.rand.NextBool(3))
            {
                for (int i = 0; i < 3; i++)
                {
                    Vector2 pollenPos = hitboxCenter + Main.rand.NextVector2Circular(hitbox.Width * 0.5f, hitbox.Height * 0.5f);
                    Vector2 pollenVel = Main.rand.NextVector2Circular(2f, 2f);
                    pollenVel.Y -= 0.5f; // Pollen floats up
                    
                    Color pollenColor = Color.Lerp(UnifiedVFX.Eroica.Gold, UnifiedVFX.Eroica.Sakura, Main.rand.NextFloat(0.3f));
                    var pollen = new GenericGlowParticle(pollenPos, pollenVel, pollenColor * 0.9f, 0.15f, 35, true);
                    MagnumParticleHandler.SpawnParticle(pollen);
                }
            }
            
            // === BLOSSOM CORE GLOW - The heart of the flower pulses with each swing ===
            if (Main.rand.NextBool(4))
            {
                float coreScale = 0.35f + bloomIntensity * 0.25f;
                Color coreColor = Color.Lerp(UnifiedVFX.Eroica.Scarlet, UnifiedVFX.Eroica.Gold, bloomIntensity);
                CustomParticles.GenericFlare(hitboxCenter, coreColor, coreScale, 12);
            }
            
            // === MUSIC NOTES - The song of spring ===
            if (Main.rand.NextBool(8))
            {
                Vector2 notePos = hitboxCenter + Main.rand.NextVector2Circular(20f, 20f);
                Vector2 noteVel = new Vector2(player.direction * 2f, -1.5f);
                ThemedParticles.EroicaMusicNotes(notePos, 1, 15f);
            }
            
            // === SCARLET EMBER TRAIL - Fire of passion follows the blade ===
            if (Main.rand.NextBool(3))
            {
                Vector2 emberPos = hitboxCenter + Main.rand.NextVector2Circular(hitbox.Width * 0.3f, hitbox.Height * 0.3f);
                Vector2 emberVel = new Vector2(player.direction * Main.rand.NextFloat(2f, 4f), Main.rand.NextFloat(-1f, 1f));
                Color emberColor = Color.Lerp(UnifiedVFX.Eroica.Scarlet, UnifiedVFX.Eroica.Crimson, Main.rand.NextFloat());
                var ember = new GenericGlowParticle(emberPos, emberVel, emberColor, 0.2f, 18, true);
                MagnumParticleHandler.SpawnParticle(ember);
            }
        }

        public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            // === UnifiedVFX EROICA IMPACT ===
            UnifiedVFX.Eroica.Impact(target.Center, 1.2f);
            
            // Single impact halo
            CustomParticles.HaloRing(target.Center, UnifiedVFX.Eroica.Sakura, 0.45f, 15);
            
            // Sakura petals burst
            ThemedParticles.SakuraPetals(target.Center, 4, 30f);
            
            // Reduced dust burst
            for (int i = 0; i < 12; i++)
            {
                Dust explosion = Dust.NewDustDirect(target.position, target.width, target.height,
                    DustID.RedTorch, 0f, 0f, 100, default, 2f);
                explosion.noGravity = true;
                explosion.velocity = Main.rand.NextVector2Circular(6f, 6f);
            }
            
            // Music notes on hit
            CustomParticles.EroicaMusicNotes(target.Center, 2, 18f);
            
            // Spawn seeking crystals on every hit - Eroica melee power
            if (Main.rand.NextBool(3)) // 33% chance
            {
                SeekingCrystalHelper.SpawnEroicaCrystals(
                    player.GetSource_ItemUse(Item),
                    target.Center,
                    (Main.MouseWorld - target.Center).SafeNormalize(Vector2.UnitX) * 8f,
                    (int)(damageDone * 0.2f),
                    Item.knockBack * 0.4f,
                    player.whoAmI,
                    4);
            }
        }

        public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Swings create spectral copies that seek enemies"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Hold right-click to charge a devastating sakura storm"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Petals fall, heroes rise'") { OverrideColor = new Color(200, 50, 50) });
        }

        // Recipe removed - drops from Eroica, God of Valor
        // public override void AddRecipes()
        // {
        //     CreateRecipe()
        //         .AddIngredient(ModContent.ItemType<ResonantCoreOfEroica>(), 30)
        //         .AddIngredient(ModContent.ItemType<EroicasResonantEnergy>(), 25)
        //         .AddIngredient(ItemID.LunarBar, 18)
        //         .AddTile(TileID.LunarCraftingStation)
        //         .Register();
        // }
    }
}
