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
            Item.damage = 155; // Balanced: ~1163 DPS (155 × 60/8)
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
            
            // GRADIENT COLORS: Scarlet → Crimson → Gold
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
                
                // Special firing effect with gradient burst
                SoundEngine.PlaySound(SoundID.Item125 with { Pitch = 0.3f, Volume = 0.8f }, position);
                
                // Fractal geometric burst with gradient
                for (int i = 0; i < 8; i++)
                {
                    float angle = MathHelper.TwoPi * i / 8f;
                    Vector2 offset = angle.ToRotationVector2() * 25f;
                    float progress = (float)i / 8f;
                    Color gradientColor = Color.Lerp(eroicaScarlet, eroicaGold, progress);
                    CustomParticles.GenericFlare(position + offset, gradientColor, 0.5f, 18);
                }
                
                // Gradient explosion burst
                for (int i = 0; i < 12; i++)
                {
                    float progress = (float)i / 12f;
                    Color burstColor = Color.Lerp(eroicaCrimson, eroicaGold, progress);
                    CustomParticles.GenericGlow(position, burstColor, 0.4f, 20);
                }
                
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
            // === UnifiedVFX EROICA AMBIENT AURA ===
            UnifiedVFX.Eroica.Aura(player.Center, 32f, 0.28f);
            
            // Ambient fractal orbit pattern with gradient
            if (Main.rand.NextBool(5))
            {
                float baseAngle = Main.GameUpdateCount * 0.025f;
                for (int i = 0; i < 5; i++)
                {
                    float angle = baseAngle + MathHelper.TwoPi * i / 5f;
                    float radius = 30f + (float)Math.Sin(Main.GameUpdateCount * 0.05f + i * 0.7f) * 10f;
                    Vector2 flarePos = player.Center + angle.ToRotationVector2() * radius;
                    float progress = (float)i / 5f;
                    Color fractalColor = Color.Lerp(UnifiedVFX.Eroica.Scarlet, UnifiedVFX.Eroica.Gold, progress);
                    CustomParticles.GenericFlare(flarePos, fractalColor, 0.32f, 16);
                }
            }
            
            // Gradient particles while holding
            if (Main.rand.NextBool(3))
            {
                Vector2 offset = Main.rand.NextVector2Circular(20f, 20f);
                float progress = Main.rand.NextFloat();
                Color particleColor = Color.Lerp(UnifiedVFX.Eroica.Scarlet, UnifiedVFX.Eroica.Gold, progress);
                CustomParticles.GenericGlow(player.Center + offset, particleColor, 0.28f, 15);
            }
            
            // Sakura petals
            if (Main.rand.NextBool(8))
            {
                ThemedParticles.SakuraPetals(player.Center + Main.rand.NextVector2Circular(25f, 25f), 2, 20f);
            }
            
            // Custom particle lightning energy
            if (Main.rand.NextBool(6))
            {
                CustomParticles.EroicaTrailFlare(player.Center + Main.rand.NextVector2Circular(18f, 18f), player.velocity);
            }
            
            // Lightning-style energy aura while holding
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.06f) * 0.1f + 0.9f;
            Vector3 lightColor = Color.Lerp(UnifiedVFX.Eroica.Scarlet, UnifiedVFX.Eroica.Gold, 0.5f).ToVector3();
            Lighting.AddLight(player.Center, lightColor * pulse * 0.5f);
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
            
            // Outer black/shadow aura - darkness before lightning
            spriteBatch.Draw(texture, position, null, new Color(40, 20, 60) * 0.4f * flicker, rotation, origin, scale * pulse * 1.4f, SpriteEffects.None, 0f);
            
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
