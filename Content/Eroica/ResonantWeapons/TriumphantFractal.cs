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
    /// Triumphant Fractal - Magic staff that fires three fractal projectiles with massive explosions.
    /// Rainbow rarity, higher tier than Moonlight weapons.
    /// Features Calamity-inspired visual effects with chromatic aberration and pulsing glow.
    /// </summary>
    public class TriumphantFractal : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
            Item.staff[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.damage = 518; // Buffed: ~1244 DPS (518 × 60/25), 15% increase
            Item.DamageType = DamageClass.Magic;
            Item.width = 56;
            Item.height = 56;
            Item.useTime = 25;
            Item.useAnimation = 25;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 8f;
            Item.value = Item.sellPrice(gold: 45);
            Item.rare = ModContent.RarityType<EroicaRainbowRarity>();
            Item.UseSound = SoundID.Item43;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<TriumphantFractalProjectile>();
            Item.shootSpeed = 14f;
            Item.mana = 19; // Reduced from 20 (5% reduction)
            Item.noMelee = true;            Item.maxStack = 1;        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Fire 3 fractals in a tight spread
            int numberOfProjectiles = 3;
            float spreadAngle = MathHelper.ToRadians(15);

            for (int i = 0; i < numberOfProjectiles; i++)
            {
                float angle = spreadAngle * ((float)i / (numberOfProjectiles - 1) - 0.5f);
                Vector2 perturbedVelocity = velocity.RotatedBy(angle);

                Projectile.NewProjectile(source, position, perturbedVelocity, type, (int)(damage * 1.15f), knockback, player.whoAmI);
            }

            // === TRUE FRACTAL GEOMETRY - RECURSIVE BRANCHING PATTERN ===
            // This is TRIUMPHANT FRACTAL - it MUST have actual fractal geometry!
            
            // Layer 1: Primary hexagonal burst (6 points)
            float baseRotation = Main.GameUpdateCount * 0.02f;
            for (int i = 0; i < 6; i++)
            {
                float angle1 = MathHelper.TwoPi * i / 6f + baseRotation;
                Vector2 point1 = position + angle1.ToRotationVector2() * 50f;
                CustomParticles.GenericFlare(point1, UnifiedVFX.Eroica.Gold, 0.7f, 25);
                
                // Layer 2: Secondary branches from each primary point (3 sub-branches each)
                for (int j = 0; j < 3; j++)
                {
                    float angle2 = angle1 + MathHelper.ToRadians(-40 + j * 40);
                    Vector2 point2 = point1 + angle2.ToRotationVector2() * 30f;
                    Color branchColor = Color.Lerp(UnifiedVFX.Eroica.Gold, UnifiedVFX.Eroica.Scarlet, 0.4f);
                    CustomParticles.GenericFlare(point2, branchColor, 0.5f, 20);
                    
                    // Layer 3: Tertiary micro-branches (2 per secondary)
                    for (int k = 0; k < 2; k++)
                    {
                        float angle3 = angle2 + MathHelper.ToRadians(-25 + k * 50);
                        Vector2 point3 = point2 + angle3.ToRotationVector2() * 18f;
                        Color microColor = Color.Lerp(UnifiedVFX.Eroica.Scarlet, UnifiedVFX.Eroica.Crimson, 0.5f);
                        CustomParticles.GenericFlare(point3, microColor, 0.3f, 15);
                    }
                }
            }
            
            // Connecting lines between primary points (hexagonal web)
            for (int i = 0; i < 6; i++)
            {
                float angle1 = MathHelper.TwoPi * i / 6f + baseRotation;
                float angle2 = MathHelper.TwoPi * ((i + 1) % 6) / 6f + baseRotation;
                Vector2 point1 = position + angle1.ToRotationVector2() * 50f;
                Vector2 point2 = position + angle2.ToRotationVector2() * 50f;
                
                // Draw connecting line as series of flares
                for (int seg = 0; seg < 5; seg++)
                {
                    float lerp = seg / 5f;
                    Vector2 linePos = Vector2.Lerp(point1, point2, lerp);
                    Color lineColor = Color.Lerp(UnifiedVFX.Eroica.Gold, UnifiedVFX.Eroica.Scarlet, lerp) * 0.6f;
                    CustomParticles.GenericFlare(linePos, lineColor, 0.25f, 12);
                }
            }
            
            // Inner triangular pattern (sacred geometry)
            for (int i = 0; i < 3; i++)
            {
                float triAngle = MathHelper.TwoPi * i / 3f + baseRotation + MathHelper.Pi / 6f;
                Vector2 triPoint = position + triAngle.ToRotationVector2() * 25f;
                CustomParticles.GenericFlare(triPoint, Color.White, 0.5f, 18);
                CustomParticles.HaloRing(triPoint, UnifiedVFX.Eroica.Gold * 0.5f, 0.2f, 10);
            }
            
            // Central burst - the triumphant core
            CustomParticles.GenericFlare(position, Color.White, 1.0f, 28);
            CustomParticles.HaloRing(position, UnifiedVFX.Eroica.Gold, 0.8f, 22);
            CustomParticles.HaloRing(position, UnifiedVFX.Eroica.Scarlet, 0.6f, 18);
            
            // Sakura petals spiral outward following fractal paths
            ThemedParticles.SakuraPetals(position, 8, 60f);
            
            // Musical notes at each primary fractal point
            for (int i = 0; i < 6; i++)
            {
                float noteAngle = MathHelper.TwoPi * i / 6f + baseRotation;
                Vector2 notePos = position + noteAngle.ToRotationVector2() * 40f;
                ThemedParticles.EroicaMusicNotes(notePos, 2, 15f);
            }
            
            // Central music burst
            ThemedParticles.MusicNoteBurst(position, UnifiedVFX.Eroica.Gold, 8, 4f);

            return false;
        }

        public override void HoldItem(Player player)
        {
            // === ROTATING FRACTAL MANDALA - Unique to Triumphant Fractal ===
            // Unlike other weapons, this creates a constantly evolving geometric pattern
            
            float time = Main.GameUpdateCount * 0.03f;
            
            // Outer ring - 8-point star rotating clockwise
            if (Main.rand.NextBool(4))
            {
                for (int i = 0; i < 8; i++)
                {
                    float angle = MathHelper.TwoPi * i / 8f + time;
                    float radius = 45f + (float)Math.Sin(time * 2f + i) * 8f;
                    Vector2 pos = player.Center + angle.ToRotationVector2() * radius;
                    Color color = Color.Lerp(UnifiedVFX.Eroica.Gold, UnifiedVFX.Eroica.Scarlet, (float)i / 8f);
                    CustomParticles.GenericFlare(pos, color, 0.28f, 12);
                }
            }
            
            // Middle ring - 6-point hexagon rotating counter-clockwise
            if (Main.rand.NextBool(5))
            {
                for (int i = 0; i < 6; i++)
                {
                    float angle = MathHelper.TwoPi * i / 6f - time * 0.7f;
                    float radius = 28f;
                    Vector2 pos = player.Center + angle.ToRotationVector2() * radius;
                    CustomParticles.GenericFlare(pos, UnifiedVFX.Eroica.Crimson * 0.8f, 0.22f, 10);
                }
            }
            
            // Inner ring - 3-point triangle (sacred geometry)
            if (Main.rand.NextBool(6))
            {
                for (int i = 0; i < 3; i++)
                {
                    float angle = MathHelper.TwoPi * i / 3f + time * 1.5f;
                    Vector2 pos = player.Center + angle.ToRotationVector2() * 15f;
                    CustomParticles.GenericFlare(pos, Color.White * 0.7f, 0.18f, 8);
                }
            }
            
            // Occasional fractal branch extension
            if (Main.rand.NextBool(12))
            {
                float branchAngle = Main.rand.NextFloat() * MathHelper.TwoPi;
                Vector2 branchStart = player.Center + branchAngle.ToRotationVector2() * 20f;
                
                // Draw a quick fractal branch
                for (int depth = 0; depth < 3; depth++)
                {
                    Vector2 branchEnd = branchStart + branchAngle.ToRotationVector2() * (15f - depth * 4f);
                    CustomParticles.GenericFlare(branchEnd, UnifiedVFX.Eroica.Gold * (1f - depth * 0.25f), 0.2f - depth * 0.05f, 10);
                    branchStart = branchEnd;
                    branchAngle += Main.rand.NextFloat(-0.5f, 0.5f);
                }
            }
            
            // Sakura petals drifting
            if (Main.rand.NextBool(18))
                ThemedParticles.SakuraPetals(player.Center, 1, 50f);
            
            // Golden sparkles with prismatic effect
            if (Main.rand.NextBool(5))
            {
                ThemedParticles.EroicaSparkles(player.Center + Main.rand.NextVector2Circular(25f, 25f), 1, 5f);
                CustomParticles.PrismaticSparkle(player.Center + Main.rand.NextVector2Circular(20f, 20f), UnifiedVFX.Eroica.Gold, 0.28f);
            }
            
            // Heroic halo pulse
            if (Main.rand.NextBool(25))
                CustomParticles.HaloRing(player.Center, UnifiedVFX.Eroica.Gold * 0.5f, 0.35f, 22);
            
            // Lighting aura with pulse
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.08f) * 0.1f + 0.9f;
            Lighting.AddLight(player.Center, 0.6f * pulse, 0.3f * pulse, 0.2f * pulse);
        }
        
        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            // Draw glowing backlight effect when dropped in world
            Texture2D texture = TextureAssets.Item[Item.type].Value;
            Vector2 position = Item.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;
            
            // Calculate pulse - faster for magic staff
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.08f) * 0.2f + 1f;
            
            // Begin additive blending for glow
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Outer crimson/scarlet glow
            spriteBatch.Draw(texture, position, null, new Color(200, 50, 50) * 0.5f, rotation, origin, scale * pulse * 1.4f, SpriteEffects.None, 0f);
            
            // Middle orange glow
            spriteBatch.Draw(texture, position, null, new Color(255, 120, 60) * 0.4f, rotation, origin, scale * pulse * 1.2f, SpriteEffects.None, 0f);
            
            // Inner golden-white core
            spriteBatch.Draw(texture, position, null, new Color(255, 220, 150) * 0.3f, rotation, origin, scale * pulse * 1.05f, SpriteEffects.None, 0f);
            
            // Return to normal blending
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Add lighting
            Lighting.AddLight(Item.Center, 1f, 0.5f, 0.3f);
            
            return true; // Draw the normal sprite too
        }

        // Recipe removed - drops from Eroica, God of Valor
        // public override void AddRecipes()
        // {
        //     CreateRecipe()
        //         .AddIngredient(ModContent.ItemType<ResonantCoreOfEroica>(), 28)
        //         .AddIngredient(ModContent.ItemType<EroicasResonantEnergy>(), 22)
        //         .AddIngredient(ItemID.LunarBar, 16)
        //         .AddTile(TileID.LunarCraftingStation)
        //         .Register();
        // }
    }
}
