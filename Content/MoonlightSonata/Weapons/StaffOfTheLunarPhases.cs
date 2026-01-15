using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria.GameContent;
using MagnumOpus.Content.MoonlightSonata.ResonanceEnergies;
using MagnumOpus.Content.MoonlightSonata.Minions;
using MagnumOpus.Content.MoonlightSonata.CraftingStations;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.MoonlightSonata.Weapons
{
    /// <summary>
    /// Staff of the Lunar Phases - Summons a Goliath of Moonlight.
    /// A massive lunar guardian that fires healing beams at enemies.
    /// </summary>
    public class StaffOfTheLunarPhases : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 44;
            Item.height = 44;
            Item.damage = 280; // Balanced summon damage for Moonlight tier
            Item.DamageType = DamageClass.Summon;
            Item.mana = 15;
            Item.useTime = 36;
            Item.useAnimation = 36;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 4f;
            Item.value = Item.buyPrice(gold: 30);
            Item.rare = ModContent.RarityType<MoonlightSonataRarity>();
            Item.UseSound = SoundID.Item44;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<GoliathOfMoonlight>();
            Item.buffType = ModContent.BuffType<GoliathOfMoonlightBuff>();
            Item.maxStack = 1;
        }

        public override void HoldItem(Player player)
        {
            // === CALAMITY-INSPIRED SUMMONER STAFF AURA ===
            
            // Rotating glyph magic circle - mystical lunar runes orbit the player
            if (Main.rand.NextBool(6))
            {
                float baseAngle = Main.GameUpdateCount * 0.025f;
                for (int i = 0; i < 6; i++)
                {
                    float angle = baseAngle + MathHelper.TwoPi * i / 6f;
                    float radius = 32f + (float)Math.Sin(Main.GameUpdateCount * 0.04f + i * MathHelper.PiOver2) * 12f;
                    Vector2 flarePos = player.Center + angle.ToRotationVector2() * radius;
                    float progress = (float)i / 6f;
                    Color fractalColor = Color.Lerp(UnifiedVFX.MoonlightSonata.DarkPurple, UnifiedVFX.MoonlightSonata.LightBlue, progress);
                    CustomParticles.GenericFlare(flarePos, fractalColor, 0.28f + (float)Math.Sin(Main.GameUpdateCount * 0.06f) * 0.08f, 16);
                }
                
                // Occasional glyph particles orbiting
                if (Main.rand.NextBool(2))
                {
                    float glyphAngle = -baseAngle * 0.7f + Main.rand.NextFloat(MathHelper.TwoPi);
                    Vector2 glyphPos = player.Center + glyphAngle.ToRotationVector2() * 38f;
                    CustomParticles.Glyph(glyphPos, UnifiedVFX.MoonlightSonata.MediumPurple, 0.25f, -1);
                }
            }
            
            // UnifiedVFX themed aura
            if (Main.rand.NextBool(8))
            {
                UnifiedVFX.MoonlightSonata.Aura(player.Center, 30f, 0.28f);
            }
            
            // Magical moonlight particles with gradient - flowing glow particles
            if (Main.rand.NextBool(4))
            {
                Vector2 offset = Main.rand.NextVector2Circular(25f, 25f);
                float progress = Main.rand.NextFloat();
                Color gradientColor = Color.Lerp(UnifiedVFX.MoonlightSonata.MediumPurple, UnifiedVFX.MoonlightSonata.LightBlue, progress);
                
                var glow = new GenericGlowParticle(player.Center + offset, Main.rand.NextVector2Circular(0.6f, 0.6f),
                    gradientColor, 0.22f + progress * 0.1f, 20, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            // Music notes floating upward - the staff hums with power
            if (Main.rand.NextBool(10))
            {
                Vector2 notePos = player.Center + Main.rand.NextVector2Circular(25f, 25f);
                Vector2 noteVel = new Vector2(0, -1f).RotatedByRandom(0.3f);
                Color noteColor = Color.Lerp(UnifiedVFX.MoonlightSonata.MediumPurple, UnifiedVFX.MoonlightSonata.Silver, Main.rand.NextFloat());
                ThemedParticles.MusicNote(notePos, noteVel, noteColor, 0.2f, 35);
            }
            
            // Soft mystical glow - pulsing
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.05f) * 0.12f + 0.9f;
            Lighting.AddLight(player.Center, 0.32f * pulse, 0.28f * pulse, 0.55f * pulse);
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            // Draw glowing backlight effect when dropped in world
            Texture2D texture = TextureAssets.Item[Item.type].Value;
            Vector2 position = Item.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;
            
            // Calculate pulse - mystical and otherworldly
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.04f) * 0.12f + 1f;
            
            // Begin additive blending for glow
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Outer deep indigo aura - cosmic power
            spriteBatch.Draw(texture, position, null, new Color(50, 30, 100) * 0.45f, rotation, origin, scale * pulse * 1.35f, SpriteEffects.None, 0f);
            
            // Middle purple/blue glow - lunar phases
            spriteBatch.Draw(texture, position, null, new Color(120, 100, 220) * 0.35f, rotation, origin, scale * pulse * 1.18f, SpriteEffects.None, 0f);
            
            // Inner cyan/white glow - goliath's power
            spriteBatch.Draw(texture, position, null, new Color(180, 220, 255) * 0.25f, rotation, origin, scale * pulse * 1.06f, SpriteEffects.None, 0f);
            
            // Return to normal blending
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Add lighting
            Lighting.AddLight(Item.Center, 0.4f, 0.35f, 0.65f);
            
            return true;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Apply the buff
            player.AddBuff(Item.buffType, 18000);
            
            // Spawn position at mouse
            position = Main.MouseWorld;
            
            // === CALAMITY-INSPIRED GRAND SUMMONING RITUAL ===
            
            // Phase 1: Central white flash - the ritual begins
            CustomParticles.GenericFlare(position, Color.White, 1.2f, 25);
            CustomParticles.GenericFlare(position, UnifiedVFX.MoonlightSonata.Silver, 1.0f, 22);
            
            // Phase 2: Magic circle glyph formation - rotating arcane symbols
            float magicCircleAngle = Main.GameUpdateCount * 0.05f;
            for (int ring = 0; ring < 3; ring++)
            {
                float ringRadius = 40f + ring * 30f;
                int glyphCount = 4 + ring * 2;
                float ringAngle = magicCircleAngle * (ring % 2 == 0 ? 1f : -0.7f); // Alternate directions
                
                for (int i = 0; i < glyphCount; i++)
                {
                    float glyphAngle = ringAngle + MathHelper.TwoPi * i / glyphCount;
                    Vector2 glyphPos = position + glyphAngle.ToRotationVector2() * ringRadius;
                    float progress = (float)(ring * glyphCount + i) / (3 * glyphCount);
                    Color glyphColor = Color.Lerp(UnifiedVFX.MoonlightSonata.DarkPurple, UnifiedVFX.MoonlightSonata.LightBlue, progress);
                    CustomParticles.Glyph(glyphPos, glyphColor, 0.35f + ring * 0.1f, -1);
                }
            }
            
            // Phase 3: Fractal geometric burst - 10-point star pattern
            for (int i = 0; i < 10; i++)
            {
                float angle = MathHelper.TwoPi * i / 10f;
                Vector2 offset = angle.ToRotationVector2() * 45f;
                float progress = (float)i / 10f;
                Color gradientColor = Color.Lerp(UnifiedVFX.MoonlightSonata.DarkPurple, UnifiedVFX.MoonlightSonata.LightBlue, progress);
                CustomParticles.GenericFlare(position + offset, gradientColor, 0.6f, 24);
            }
            
            // Phase 4: Layered halo cascade - ascending rings
            for (int ring = 0; ring < 6; ring++)
            {
                float ringProgress = (float)ring / 6f;
                Color ringColor = Color.Lerp(UnifiedVFX.MoonlightSonata.DarkPurple, UnifiedVFX.MoonlightSonata.Silver, ringProgress);
                CustomParticles.HaloRing(position, ringColor, 0.4f + ring * 0.18f, 18 + ring * 5);
            }
            
            // Phase 5: Spiral galaxy energy burst
            for (int arm = 0; arm < 8; arm++)
            {
                float armAngle = MathHelper.TwoPi * arm / 8f;
                for (int point = 0; point < 6; point++)
                {
                    float spiralAngle = armAngle + point * 0.3f;
                    float spiralRadius = 20f + point * 15f;
                    Vector2 spiralPos = position + spiralAngle.ToRotationVector2() * spiralRadius;
                    float progress = (arm * 6 + point) / 48f;
                    Color galaxyColor = Color.Lerp(UnifiedVFX.MoonlightSonata.MediumPurple, UnifiedVFX.MoonlightSonata.LightBlue, progress);
                    
                    var galaxySpark = new GenericGlowParticle(spiralPos, spiralAngle.ToRotationVector2() * 2f,
                        galaxyColor, 0.35f + point * 0.05f, 22 + point * 2, true);
                    MagnumParticleHandler.SpawnParticle(galaxySpark);
                }
            }
            
            // Phase 6: Music notes explosion - the symphony of summoning
            ThemedParticles.MoonlightMusicNotes(position, 15, 70f);
            ThemedParticles.MoonlightClef(position, true, 2f);
            ThemedParticles.MoonlightClef(position, false, 1.8f);
            
            // Phase 7: Rising music notes ascending like a crescendo
            for (int i = 0; i < 10; i++)
            {
                float angle = MathHelper.TwoPi * i / 10f;
                Vector2 notePos = position + angle.ToRotationVector2() * 40f;
                Vector2 noteVel = new Vector2(0, -2f).RotatedByRandom(0.3f);
                float noteProgress = (float)i / 10f;
                Color noteColor = Color.Lerp(UnifiedVFX.MoonlightSonata.DarkPurple, UnifiedVFX.MoonlightSonata.Silver, noteProgress);
                ThemedParticles.MusicNote(notePos, noteVel, noteColor, 0.35f, 40);
            }
            
            // Phase 8: Vortex pull effect - particles spiral inward
            for (int i = 0; i < 25; i++)
            {
                float vortexAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                float vortexRadius = Main.rand.NextFloat(80f, 120f);
                Vector2 vortexStart = position + vortexAngle.ToRotationVector2() * vortexRadius;
                Vector2 vortexVel = (position - vortexStart).SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(4f, 8f);
                float progress = (float)i / 25f;
                Color vortexColor = Color.Lerp(UnifiedVFX.MoonlightSonata.DarkPurple, UnifiedVFX.MoonlightSonata.LightBlue, progress);
                
                var vortexParticle = new GenericGlowParticle(vortexStart, vortexVel, vortexColor, 0.3f, 25, true);
                MagnumParticleHandler.SpawnParticle(vortexParticle);
            }
            
            // Phase 9: UnifiedVFX explosion
            ThemedParticles.MoonlightShockwave(position, 1.6f);
            
            // Phase 10: Lightning fractals radiating outward
            for (int i = 0; i < 6; i++)
            {
                float lightningAngle = MathHelper.TwoPi * i / 6f + Main.rand.NextFloat(-0.2f, 0.2f);
                Vector2 lightningEnd = position + lightningAngle.ToRotationVector2() * 100f;
                MagnumVFX.DrawMoonlightLightning(position, lightningEnd, 10, 28f, 3, 0.5f);
            }
            
            // Powerful summoning sound
            Terraria.Audio.SoundEngine.PlaySound(SoundID.Item119 with { Volume = 1f, Pitch = -0.2f }, position);
            Terraria.Audio.SoundEngine.PlaySound(SoundID.Item82 with { Volume = 0.6f }, position);
            
            // Spawn the Goliath
            Projectile.NewProjectile(source, position, Vector2.Zero, type, damage, knockback, player.whoAmI);
            
            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<MoonlightsResonantEnergy>(), 20)
                .AddIngredient(ModContent.ItemType<ResonantCoreOfMoonlightSonata>(), 20)
                .AddIngredient(ModContent.ItemType<Enemies.ShardsOfMoonlitTempo>(), 20)
                .AddTile(ModContent.TileType<MoonlightAnvilTile>())
                .Register();
        }
        
        public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "MinionInfo", "Summons a Goliath of Moonlight")
            {
                OverrideColor = new Color(180, 150, 255)
            });
            
            tooltips.Add(new TooltipLine(Mod, "BeamInfo", "Fires explosive moonlight beams that heal you")
            {
                OverrideColor = new Color(150, 200, 255)
            });
            
            tooltips.Add(new TooltipLine(Mod, "HealInfo", "Each beam hit restores 10 health")
            {
                OverrideColor = new Color(100, 255, 150)
            });
        }
    }
}
