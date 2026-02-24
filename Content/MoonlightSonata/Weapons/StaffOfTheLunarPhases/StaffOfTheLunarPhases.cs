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
            // === ENHANCED LUNAR SUMMONER AURA ===
            
            // Orbiting lunar motes (1-in-6)
            if (Main.rand.NextBool(6))
            {
                float orbitAngle = Main.GameUpdateCount * 0.04f;
                for (int i = 0; i < 3; i++)
                {
                    float angle = orbitAngle + MathHelper.TwoPi * i / 3f;
                    float radius = 30f + (float)Math.Sin(Main.GameUpdateCount * 0.05f + i * 0.7f) * 6f;
                    Vector2 orbitPos = player.Center + angle.ToRotationVector2() * radius;
                    float progress = (float)i / 3f;
                    Color orbitColor = Color.Lerp(UnifiedVFX.MoonlightSonata.DarkPurple, UnifiedVFX.MoonlightSonata.LightBlue, progress);
                    CustomParticles.GenericFlare(orbitPos, orbitColor * 0.6f, 0.25f, 12);
                }
            }
            
            // Prismatic sparkle aura (1-in-4)
            if (Main.rand.NextBool(4))
            {
                Vector2 offset = Main.rand.NextVector2Circular(28f, 28f);
                Color gradientColor = Color.Lerp(UnifiedVFX.MoonlightSonata.MediumPurple, UnifiedVFX.MoonlightSonata.LightBlue, Main.rand.NextFloat());
                CustomParticles.PrismaticSparkle(player.Center + offset, gradientColor * 0.6f, 0.22f);
                
                // Sparkle companion
                var sparkle = new SparkleParticle(player.Center + offset, Main.rand.NextVector2Circular(0.5f, 0.5f), 
                    UnifiedVFX.MoonlightSonata.Silver * 0.4f, 0.18f, 20);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            // VISIBLE ORBITING MUSIC NOTES (1-in-8, scale 0.75f+)
            if (Main.rand.NextBool(8))
            {
                float noteOrbit = Main.GameUpdateCount * 0.06f;
                float shimmer = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.15f) * 0.1f;
                
                for (int i = 0; i < 2; i++)
                {
                    float noteAngle = noteOrbit + MathHelper.Pi * i;
                    Vector2 notePos = player.Center + noteAngle.ToRotationVector2() * 22f;
                    Vector2 noteVel = new Vector2(0, -0.5f);
                    Color noteColor = Color.Lerp(UnifiedVFX.MoonlightSonata.MediumPurple, UnifiedVFX.MoonlightSonata.LightBlue, (float)i / 2f);
                    ThemedParticles.MusicNote(notePos, noteVel, noteColor * 0.85f, 0.75f * shimmer, 35);
                    
                    // Sparkle companion for visibility
                    CustomParticles.PrismaticSparkle(notePos + Main.rand.NextVector2Circular(4f, 4f), 
                        UnifiedVFX.MoonlightSonata.Silver * 0.4f, 0.15f);
                }
            }
            
            // Dense lunar dust (1-in-3)
            if (Main.rand.NextBool(3))
            {
                Vector2 dustPos = player.Center + Main.rand.NextVector2Circular(35f, 35f);
                Dust dust = Dust.NewDustPerfect(dustPos, DustID.PurpleTorch, 
                    Main.rand.NextVector2Circular(0.8f, 0.8f), 80, default, 1.1f);
                dust.noGravity = true;
                dust.fadeIn = 1.2f;
            }
            
            // Soft mystical glow - pulsing (brighter)
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.05f) * 0.15f + 0.95f;
            Lighting.AddLight(player.Center, 0.45f * pulse, 0.4f * pulse, 0.7f * pulse);
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
            
            // === GRAND SUMMONING RITUAL (Summons are infrequent - keep impactful but cleaner) ===
            
            // Central flash
            CustomParticles.GenericFlare(position, Color.White, 1.0f, 22);
            CustomParticles.GenericFlare(position, UnifiedVFX.MoonlightSonata.Silver, 0.8f, 20);
            
            // Magic circle - single ring of glyphs
            float magicCircleAngle = Main.GameUpdateCount * 0.05f;
            for (int i = 0; i < 6; i++)
            {
                float glyphAngle = magicCircleAngle + MathHelper.TwoPi * i / 6f;
                Vector2 glyphPos = position + glyphAngle.ToRotationVector2() * 50f;
                Color glyphColor = Color.Lerp(UnifiedVFX.MoonlightSonata.DarkPurple, UnifiedVFX.MoonlightSonata.LightBlue, (float)i / 6f);
                CustomParticles.Glyph(glyphPos, glyphColor, 0.4f, -1);
            }
            
            // Halo rings (reduced)
            for (int ring = 0; ring < 2; ring++)
            {
                Color ringColor = Color.Lerp(UnifiedVFX.MoonlightSonata.DarkPurple, UnifiedVFX.MoonlightSonata.Silver, ring * 0.5f);
                CustomParticles.HaloRing(position, ringColor, 0.5f + ring * 0.2f, 20 + ring * 5);
            }
            
            // Spark burst (reduced)
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                Vector2 sparkVel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 8f);
                Color sparkColor = Color.Lerp(UnifiedVFX.MoonlightSonata.MediumPurple, UnifiedVFX.MoonlightSonata.LightBlue, (float)i / 12f);
                
                var spark = new GenericGlowParticle(position, sparkVel, sparkColor, 0.3f, 20, true);
                MagnumParticleHandler.SpawnParticle(spark);
            }
            
            // Music notes - the summoning song
            ThemedParticles.MoonlightMusicNotes(position, 8, 50f);
            ThemedParticles.MoonlightClef(position, true, 1.5f);
            
            // Shockwave
            ThemedParticles.MoonlightShockwave(position, 1.2f);
            
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
