using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria.Audio;
using Terraria.GameContent;
using MagnumOpus.Content.Eroica.Minions;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.Eroica.ResonantWeapons
{
    /// <summary>
    /// Finality of the Sakura - A powerful summoner weapon with rainbow rarity.
    /// Summons the Sakura of Fate, a spectral guardian that fires black and red flames.
    /// The staff itself is lit aflame in black and deep scarlet particles.
    /// </summary>
    public class FinalityOfTheSakura : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 48;
            Item.height = 48;
            Item.damage = 320; // Balanced: Strong summon damage for Eroica tier
            Item.DamageType = DamageClass.Summon;
            Item.mana = 20;
            Item.useTime = 36;
            Item.useAnimation = 36;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 5f;
            Item.value = Item.buyPrice(platinum: 1);
            Item.rare = ModContent.RarityType<EroicaRainbowRarity>();
            Item.UseSound = null; // Custom sound
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<SakuraOfFate>();
            Item.buffType = ModContent.BuffType<SakuraOfFateBuff>();
            Item.maxStack = 1;
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            // Draw glowing backlight effect when dropped in world
            Texture2D texture = TextureAssets.Item[Item.type].Value;
            Vector2 position = Item.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;
            
            // Calculate pulse - ominous and dark
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.05f) * 0.1f + 1f;
            float flicker = Main.rand.NextBool(12) ? 1.15f : 1f;
            
            // Begin additive blending for glow
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Outer dark shadow aura - finality's darkness
            spriteBatch.Draw(texture, position, null, new Color(30, 10, 20) * 0.6f * flicker, rotation, origin, scale * pulse * 1.4f, SpriteEffects.None, 0f);
            
            // Middle deep crimson glow - sakura's blood
            spriteBatch.Draw(texture, position, null, new Color(150, 30, 30) * 0.4f * flicker, rotation, origin, scale * pulse * 1.2f, SpriteEffects.None, 0f);
            
            // Inner scarlet/orange glow - burning fate
            spriteBatch.Draw(texture, position, null, new Color(255, 120, 80) * 0.3f * flicker, rotation, origin, scale * pulse * 1.08f, SpriteEffects.None, 0f);
            
            // Return to normal blending
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Add lighting
            Lighting.AddLight(Item.Center, 0.5f, 0.2f, 0.15f);
            
            return true;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Apply the buff
            player.AddBuff(Item.buffType, 18000);
            
            // Spawn position at mouse
            position = Main.MouseWorld;
            
            // GRADIENT COLORS: Black → Deep Scarlet → Crimson → Gold
            Color eroicaBlack = new Color(30, 20, 25);
            Color eroicaScarlet = new Color(139, 0, 0);
            Color eroicaCrimson = new Color(220, 50, 50);
            Color eroicaGold = new Color(255, 215, 0);
            
            // === CUSTOM PARTICLE EFFECTS WITH GRADIENT FADING ===
            
            // === UnifiedVFX EROICA SUMMON EXPLOSION ===
            UnifiedVFX.Eroica.Explosion(position, 1.4f);
            
            // Central fractal burst - geometric flares with gradient
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 offset = angle.ToRotationVector2() * 35f;
                float progress = (float)i / 8f;
                Color gradientColor = Color.Lerp(UnifiedVFX.Eroica.Scarlet, UnifiedVFX.Eroica.Gold, progress);
                CustomParticles.GenericFlare(position + offset, gradientColor, 0.65f, 22);
            }
            
            // Gradient halo rings - dark to bright
            for (int ring = 0; ring < 5; ring++)
            {
                float progress = (float)ring / 5f;
                Color ringColor = Color.Lerp(eroicaBlack, eroicaCrimson, progress);
                CustomParticles.HaloRing(position, ringColor, 0.4f + ring * 0.15f, 18 + ring * 3);
            }
            
            // Explosion burst with gradient
            for (int i = 0; i < 16; i++)
            {
                float progress = (float)i / 16f;
                Color burstColor = Color.Lerp(eroicaScarlet, eroicaGold, progress);
                CustomParticles.GenericGlow(position, burstColor, 0.4f, 25);
            }
            
            // Central white flash
            CustomParticles.GenericFlare(position, Color.White, 1.0f, 15);
            
            // Themed impact
            ThemedParticles.EroicaImpact(position, 1.5f);
            ThemedParticles.EroicaHaloBurst(position, 1.2f);
            
            // === VANILLA DUST WITH GRADIENT COLORS ===
            
            // Outer ring - gradient from black to crimson
            for (int i = 0; i < 40; i++)
            {
                float angle = MathHelper.TwoPi * i / 40f;
                float progress = (float)i / 40f;
                Vector2 dustPos = position + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 70f;
                Vector2 dustVel = (position - dustPos).SafeNormalize(Vector2.Zero) * 5f;
                Color dustColor = Color.Lerp(eroicaBlack, eroicaCrimson, progress);
                Dust dust = Dust.NewDustPerfect(dustPos, DustID.Smoke, dustVel, 200, dustColor, 2.5f);
                dust.noGravity = true;
                dust.fadeIn = 1.5f;
            }
            
            // Inner ring - gradient from scarlet to gold
            for (int i = 0; i < 35; i++)
            {
                float angle = MathHelper.TwoPi * i / 35f;
                float progress = (float)i / 35f;
                Vector2 dustPos = position + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 50f;
                Vector2 dustVel = (position - dustPos).SafeNormalize(Vector2.Zero) * 4f;
                Color dustColor = Color.Lerp(eroicaScarlet, eroicaGold, progress);
                Dust dust = Dust.NewDustPerfect(dustPos, DustID.CrimsonTorch, dustVel, 100, dustColor, 2f);
                dust.noGravity = true;
                dust.fadeIn = 1.3f;
            }
            
            // Central burst - gradient particles
            for (int i = 0; i < 30; i++)
            {
                float progress = (float)i / 30f;
                Vector2 dustVel = Main.rand.NextVector2Circular(8f, 8f);
                Color dustColor = Color.Lerp(eroicaBlack, eroicaCrimson, progress);
                Dust dust = Dust.NewDustDirect(position, 1, 1, DustID.Smoke, dustVel.X, dustVel.Y, 200, dustColor, 2.5f);
                dust.noGravity = true;
                dust.fadeIn = 1.3f;
            }
            
            // Dark, ominous summoning sounds
            SoundEngine.PlaySound(SoundID.Item119 with { Volume = 0.9f, Pitch = -0.4f }, position);
            SoundEngine.PlaySound(SoundID.Item73 with { Volume = 0.7f, Pitch = -0.3f }, position);
            SoundEngine.PlaySound(SoundID.NPCDeath6 with { Volume = 0.4f, Pitch = 0.5f }, position);
            
            // Spawn the Sakura of Fate
            Projectile.NewProjectile(source, position, Vector2.Zero, type, damage, knockback, player.whoAmI);
            
            return false;
        }

        public override void HoldItem(Player player)
        {
            // === UnifiedVFX EROICA AMBIENT AURA ===
            UnifiedVFX.Eroica.Aura(player.Center, 30f, 0.3f);
            
            // Position around the held item
            Vector2 itemCenter = player.itemLocation + new Vector2(Item.width * 0.5f * player.direction, -Item.height * 0.3f);
            
            // Ambient fractal orbit pattern with gradient
            if (Main.rand.NextBool(5))
            {
                float baseAngle = Main.GameUpdateCount * 0.03f;
                for (int i = 0; i < 6; i++)
                {
                    float angle = baseAngle + MathHelper.TwoPi * i / 6f;
                    float radius = 25f + (float)Math.Sin(Main.GameUpdateCount * 0.06f + i * 0.5f) * 8f;
                    Vector2 flarePos = itemCenter + angle.ToRotationVector2() * radius;
                    float progress = (float)i / 6f;
                    Color fractalColor = Color.Lerp(UnifiedVFX.Eroica.Scarlet, UnifiedVFX.Eroica.Gold, progress);
                    CustomParticles.GenericFlare(flarePos, fractalColor, 0.3f, 15);
                }
            }
            
            // Staff is constantly lit aflame - gradient particles
            if (Main.rand.NextBool(2))
            {
                // Deep crimson glow with gradient to gold
                if (Main.rand.NextBool(2))
                {
                    float progress = Main.rand.NextFloat();
                    Color crimsonColor = Color.Lerp(UnifiedVFX.Eroica.Scarlet, UnifiedVFX.Eroica.Gold, progress * 0.6f);
                    CustomParticles.GenericGlow(itemCenter + Main.rand.NextVector2Circular(8f, 8f), crimsonColor, 0.32f, 13);
                }
            }
            
            // Sakura petals
            if (Main.rand.NextBool(8))
            {
                ThemedParticles.SakuraPetals(itemCenter, 2, 18f);
            }
            
            // Occasional ember with gradient
            if (Main.rand.NextBool(5))
            {
                float progress = Main.rand.NextFloat();
                Color emberColor = Color.Lerp(UnifiedVFX.Eroica.Crimson, UnifiedVFX.Eroica.Gold, progress);
                CustomParticles.GenericFlare(itemCenter, emberColor, 0.38f, 12);
            }
        }
        
        public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "MinionInfo", "Summons a Sakura of Fate to fight for you")
            {
                OverrideColor = new Color(180, 60, 80)
            });
            
            tooltips.Add(new TooltipLine(Mod, "FlameInfo", "The Sakura fires black and scarlet flames at enemies")
            {
                OverrideColor = new Color(120, 40, 60)
            });
            
            tooltips.Add(new TooltipLine(Mod, "FateInfo", "'A final blossom before eternal night'")
            {
                OverrideColor = new Color(100, 100, 100)
            });
        }
    }
}
