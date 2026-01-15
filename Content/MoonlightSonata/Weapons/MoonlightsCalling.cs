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
            Item.damage = 200; // Balanced: ~1000 DPS (200 × 60/12)
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
            // === CALAMITY-INSPIRED AMBIENT AURA ===
            // Orbiting fractal flares with gradient
            if (Main.rand.NextBool(5))
            {
                float baseAngle = Main.GameUpdateCount * 0.03f;
                for (int i = 0; i < 6; i++)
                {
                    float angle = baseAngle + MathHelper.TwoPi * i / 6f;
                    float radius = 26f + (float)Math.Sin(Main.GameUpdateCount * 0.06f + i * 0.8f) * 10f;
                    Vector2 flarePos = player.Center + angle.ToRotationVector2() * radius;
                    float progress = (float)i / 6f;
                    Color fractalColor = Color.Lerp(UnifiedVFX.MoonlightSonata.DarkPurple, UnifiedVFX.MoonlightSonata.LightBlue, progress);
                    CustomParticles.GenericFlare(flarePos, fractalColor, 0.25f + (float)Math.Sin(Main.GameUpdateCount * 0.08f) * 0.08f, 14);
                }
            }
            
            // UnifiedVFX themed aura
            if (Main.rand.NextBool(8))
            {
                UnifiedVFX.MoonlightSonata.Aura(player.Center, 28f, 0.25f);
            }
            
            // Magical tome particles with gradient - flowing glow particles
            if (Main.rand.NextBool(4))
            {
                Vector2 offset = Main.rand.NextVector2Circular(22f, 22f);
                float progress = Main.rand.NextFloat();
                Color gradientColor = Color.Lerp(UnifiedVFX.MoonlightSonata.DarkPurple, UnifiedVFX.MoonlightSonata.LightBlue, progress);
                
                var glow = new GenericGlowParticle(player.Center + offset, Main.rand.NextVector2Circular(0.5f, 0.5f),
                    gradientColor, 0.22f + progress * 0.1f, 20, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            // Music notes floating around the player - the tome sings
            if (Main.rand.NextBool(12))
            {
                Vector2 notePos = player.Center + Main.rand.NextVector2Circular(30f, 30f);
                Vector2 noteVel = new Vector2(0, -0.8f).RotatedByRandom(0.5f);
                Color noteColor = Color.Lerp(UnifiedVFX.MoonlightSonata.MediumPurple, UnifiedVFX.MoonlightSonata.Silver, Main.rand.NextFloat());
                ThemedParticles.MusicNote(notePos, noteVel, noteColor, 0.22f, 35);
            }
            
            // Custom particle ethereal glow
            if (Main.rand.NextBool(8))
            {
                CustomParticles.MoonlightFlare(player.Center + Main.rand.NextVector2Circular(18f, 18f), 0.2f);
            }
            
            // Pulsing mystical glow
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.06f) * 0.1f + 0.9f;
            Lighting.AddLight(player.Center, 0.35f * pulse, 0.22f * pulse, 0.5f * pulse);
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
            
            // === CALAMITY-INSPIRED MUZZLE FLASH ===
            // Phase 1: Central white flash
            CustomParticles.GenericFlare(position, Color.White * 0.9f, 0.5f, 10);
            
            // Phase 2: UnifiedVFX swing aura (adapted for magic cast)
            UnifiedVFX.MoonlightSonata.SwingAura(position, direction, 0.6f);
            
            // Phase 3: Fractal geometric burst with gradient
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 flareOffset = angle.ToRotationVector2() * 18f;
                float progress = (float)i / 8f;
                Color flashColor = Color.Lerp(UnifiedVFX.MoonlightSonata.DarkPurple, UnifiedVFX.MoonlightSonata.LightBlue, progress);
                CustomParticles.GenericFlare(position + flareOffset, flashColor, 0.32f, 14);
            }
            
            // Phase 4: Gradient halo rings
            for (int ring = 0; ring < 3; ring++)
            {
                float ringProgress = (float)ring / 3f;
                Color ringColor = Color.Lerp(UnifiedVFX.MoonlightSonata.DarkPurple, UnifiedVFX.MoonlightSonata.LightBlue, ringProgress);
                CustomParticles.HaloRing(position, ringColor, 0.22f + ring * 0.1f, 10 + ring * 3);
            }
            
            // Phase 5: Directional sparks shooting forward
            for (int i = 0; i < 4; i++)
            {
                Vector2 sparkVel = direction.RotatedByRandom(0.4f) * Main.rand.NextFloat(4f, 8f);
                float progress = (float)i / 4f;
                Color sparkColor = Color.Lerp(UnifiedVFX.MoonlightSonata.MediumPurple, UnifiedVFX.MoonlightSonata.Silver, progress);
                
                var spark = new GenericGlowParticle(position, sparkVel, sparkColor, 0.28f, 16, true);
                MagnumParticleHandler.SpawnParticle(spark);
            }
            
            // Phase 6: Music notes on every cast - the tome sings with each spell
            ThemedParticles.MoonlightMusicNotes(position, 3, 22f);
            
            // Custom particle muzzle flash
            CustomParticles.MoonlightFlare(position, 0.28f);
            
            return false;
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
