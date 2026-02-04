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
    /// Moonlight's Calling - A magic tome that casts rapid moonlight beams.
    /// Dark purple center gradient to light purple, sparkly beams.
    /// </summary>
    public class MoonlightsCalling : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 30;
            Item.damage = 200; // Balanced: ~1000 DPS (200 ÁE60/12)
            Item.DamageType = DamageClass.Magic;
            Item.mana = 8;
            Item.useTime = 12; // Fast fire rate
            Item.useAnimation = 12;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 3f;
            Item.value = Item.buyPrice(gold: 25);
            Item.rare = ModContent.RarityType<MoonlightSonataRarity>();
            Item.UseSound = SoundID.Item72;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<MoonlightBeam>();
            Item.shootSpeed = 16f;
            Item.noMelee = true;
            Item.staff[Item.type] = true;
            Item.maxStack = 1;
        }

        public override void HoldItem(Player player)
        {
            float time = Main.GameUpdateCount * 0.04f;
            float pulse = (float)Math.Sin(time * 1.5f) * 0.12f + 0.88f;
            
            // === ORBITING LUNAR MOTES - Like tiny moons around the player ===
            if (Main.rand.NextBool(6))
            {
                float orbitAngle = time + Main.rand.NextFloat(MathHelper.TwoPi);
                float radius = 25f + (float)Math.Sin(time * 2f + Main.rand.NextFloat()) * 8f;
                Vector2 orbitPos = player.Center + orbitAngle.ToRotationVector2() * radius;
                
                // Gradient from dark purple core to light blue edge
                float hueShift = (time * 0.02f) % 0.15f;
                Color moteColor = Color.Lerp(UnifiedVFX.MoonlightSonata.DarkPurple, UnifiedVFX.MoonlightSonata.LightBlue, 
                    ((float)Math.Sin(orbitAngle * 2f) + 1f) * 0.5f);
                
                CustomParticles.GenericFlare(orbitPos, moteColor * 0.7f, 0.25f * pulse, 12);
            }
            
            // === PRISMATIC SPARKLE TRAIL ===
            if (Main.rand.NextBool(8))
            {
                Vector2 offset = Main.rand.NextVector2Circular(22f, 22f);
                Color gradientColor = Color.Lerp(UnifiedVFX.MoonlightSonata.DarkPurple, UnifiedVFX.MoonlightSonata.LightBlue, Main.rand.NextFloat());
                CustomParticles.PrismaticSparkle(player.Center + offset, gradientColor * 0.6f, 0.22f);
                
                // Accompanying sparkle
                var sparkle = new SparkleParticle(player.Center + offset, Vector2.Zero, UnifiedVFX.MoonlightSonata.Silver * 0.4f, 0.15f, 18);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            // === VISIBLE ORBITING MUSIC NOTES - Scale 0.75f+ ===
            if (Main.rand.NextBool(10))
            {
                float noteOrbitAngle = time * 0.8f;
                for (int i = 0; i < 2; i++)
                {
                    float noteAngle = noteOrbitAngle + MathHelper.TwoPi * i / 2f;
                    Vector2 notePos = player.Center + noteAngle.ToRotationVector2() * 30f;
                    Vector2 noteVel = new Vector2(0, -Main.rand.NextFloat(0.4f, 0.8f));
                    
                    // VISIBLE scale with shimmer
                    float shimmer = 1f + (float)Math.Sin(time * 3f + i) * 0.12f;
                    Color noteColor = Color.Lerp(UnifiedVFX.MoonlightSonata.MediumPurple, UnifiedVFX.MoonlightSonata.Silver, i * 0.4f);
                    ThemedParticles.MusicNote(notePos, noteVel, noteColor * 0.85f, 0.75f * shimmer, 40);
                    
                    // Sparkle companion for visibility
                    CustomParticles.PrismaticSparkle(notePos + Main.rand.NextVector2Circular(5f, 5f), 
                        UnifiedVFX.MoonlightSonata.Silver * 0.5f, 0.18f);
                }
            }
            
            // === DENSE DUST AURA - Iridescent Wingspan style ===
            if (Main.rand.NextBool(3))
            {
                Vector2 dustPos = player.Center + Main.rand.NextVector2Circular(20f, 20f);
                Color dustColor = Color.Lerp(UnifiedVFX.MoonlightSonata.DarkPurple, UnifiedVFX.MoonlightSonata.LightBlue, Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(dustPos, DustID.PurpleTorch, Main.rand.NextVector2Circular(0.8f, 0.8f), 100, dustColor, 1.1f);
                d.noGravity = true;
                d.fadeIn = 1.2f;
            }
            
            // Pulsing mystical glow - brighter
            Lighting.AddLight(player.Center, 0.45f * pulse, 0.28f * pulse, 0.65f * pulse);
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            // Draw glowing backlight effect when dropped in world
            Texture2D texture = TextureAssets.Item[Item.type].Value;
            Vector2 position = Item.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;
            
            // Calculate pulse - mystical like a calling
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.05f) * 0.1f + 1f;
            
            // Begin additive blending for glow
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Outer deep purple aura
            spriteBatch.Draw(texture, position, null, new Color(70, 30, 110) * 0.45f, rotation, origin, scale * pulse * 1.35f, SpriteEffects.None, 0f);
            
            // Middle violet glow
            spriteBatch.Draw(texture, position, null, new Color(140, 90, 200) * 0.32f, rotation, origin, scale * pulse * 1.18f, SpriteEffects.None, 0f);
            
            // Inner silver/light purple glow
            spriteBatch.Draw(texture, position, null, new Color(200, 180, 255) * 0.22f, rotation, origin, scale * pulse * 1.06f, SpriteEffects.None, 0f);
            
            // Return to normal blending
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Add lighting
            Lighting.AddLight(Item.Center, 0.4f, 0.3f, 0.55f);
            
            return true;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Add slight spread for rapid fire feel
            float spread = MathHelper.ToRadians(5f);
            velocity = velocity.RotatedByRandom(spread);
            
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            
            Vector2 direction = velocity.SafeNormalize(Vector2.UnitX);
            
            // === PHASE 10 ENHANCED MUZZLE FLASH ===
            // Layered central flash (Iridescent Wingspan style)
            CustomParticles.GenericFlare(position, Color.White * 0.6f, 0.5f, 15);
            CustomParticles.GenericFlare(position, UnifiedVFX.MoonlightSonata.LightBlue * 0.8f, 0.42f, 14);
            CustomParticles.GenericFlare(position, UnifiedVFX.MoonlightSonata.DarkPurple * 0.6f, 0.35f, 12);
            
            // Cascading halo rings with gradient
            for (int i = 0; i < 3; i++)
            {
                Color ringColor = Color.Lerp(UnifiedVFX.MoonlightSonata.DarkPurple, UnifiedVFX.MoonlightSonata.LightBlue, i / 3f);
                CustomParticles.HaloRing(position, ringColor * (0.5f - i * 0.1f), 0.22f + i * 0.08f, 10 + i * 2);
            }
            
            // === VISIBLE MUSIC NOTES - Scale 0.8f ===
            for (int i = 0; i < 3; i++)
            {
                float angle = direction.ToRotation() + Main.rand.NextFloat(-0.5f, 0.5f);
                Vector2 noteVel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 4f);
                Color noteColor = Color.Lerp(UnifiedVFX.MoonlightSonata.MediumPurple, UnifiedVFX.MoonlightSonata.Silver, i / 3f);
                ThemedParticles.MusicNote(position + direction * 10f, noteVel, noteColor * 0.9f, 0.8f, 35);
            }
            
            // Directional spark burst
            for (int i = 0; i < 5; i++)
            {
                float sparkAngle = direction.ToRotation() + Main.rand.NextFloat(-0.4f, 0.4f);
                Vector2 sparkVel = sparkAngle.ToRotationVector2() * Main.rand.NextFloat(4f, 8f);
                Color sparkColor = Color.Lerp(UnifiedVFX.MoonlightSonata.LightBlue, Color.White, Main.rand.NextFloat(0.3f));
                var spark = new SparkleParticle(position, sparkVel, sparkColor, 0.3f, 18);
                MagnumParticleHandler.SpawnParticle(spark);
            }
            
            // Dense dust burst
            for (int i = 0; i < 4; i++)
            {
                Vector2 dustVel = direction * 3f + Main.rand.NextVector2Circular(2f, 2f);
                Dust d = Dust.NewDustPerfect(position, DustID.PurpleTorch, dustVel, 80, default, 1.4f);
                d.noGravity = true;
                d.fadeIn = 1.2f;
            }
            
            // Phase10 dramatic impact for shot firing
            Phase10Integration.Universal.DramaticImpact(position, UnifiedVFX.MoonlightSonata.DarkPurple, 
                UnifiedVFX.MoonlightSonata.LightBlue, 0.4f);
            
            return false;
        }

        public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Fires rapid moonlight beams that pierce enemies"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Beams leave sparkle trails and create prismatic impacts"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The moon whispers secrets to those who listen'")
            {
                OverrideColor = new Microsoft.Xna.Framework.Color(140, 100, 200)
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<MoonlightsResonantEnergy>(), 15)
                .AddIngredient(ModContent.ItemType<ResonantCoreOfMoonlightSonata>(), 5)
                .AddIngredient(ModContent.ItemType<Enemies.ShardsOfMoonlitTempo>(), 10)
                .AddTile(ModContent.TileType<MoonlightAnvilTile>())
                .Register();
        }
    }
}
