using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;
using MagnumOpus.Content.MoonlightSonata.ResonanceEnergies;
using MagnumOpus.Content.MoonlightSonata.CraftingStations;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.MoonlightSonata.ResonantWeapons
{
    /// <summary>
    /// Incisor of Moonlight - A powerful sword with ethereal moonlight effects.
    /// Features Calamity-inspired visual effects with purple/silver glowing aura.
    /// Hold right-click to charge a devastating lunar storm attack!
    /// </summary>
    public class IncisorOfMoonlight : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            // Stronger than Zenith (190 damage)
            Item.damage = 280; // Balanced: Premium melee ~1400 DPS with projectile
            Item.DamageType = DamageClass.Melee;
            Item.width = 60;
            Item.height = 60;
            Item.useTime = 12; // Fast swing
            Item.useAnimation = 12;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 6.5f;
            Item.value = Item.sellPrice(gold: 25);
            Item.rare = ModContent.RarityType<EroicaRainbowRarity>();
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.useTurn = false;
            Item.shoot = ModContent.ProjectileType<MoonlightWaveProjectile>();
            Item.shootSpeed = 12f;
            Item.maxStack = 1;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<MoonlightsResonantEnergy>(), 20)
                .AddIngredient(ModContent.ItemType<ResonantCoreOfMoonlightSonata>(), 10)
                .AddIngredient(ModContent.ItemType<Enemies.ShardsOfMoonlitTempo>(), 25)
                .AddTile(ModContent.TileType<MoonlightAnvilTile>())
                .Register();
        }

        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            // Prismatic sparkle trail on swing - ethereal moonlight gems
            if (Main.rand.NextBool(3))
            {
                Vector2 sparklePos = new Vector2(hitbox.X + Main.rand.Next(hitbox.Width), hitbox.Y + Main.rand.Next(hitbox.Height));
                CustomParticles.PrismaticSparkle(sparklePos, CustomParticleSystem.MoonlightColors.Random(), 0.25f);
            }
            
            // Main purple dust with reduced frequency for cleaner look
            if (Main.rand.NextBool(3))
            {
                Dust dust = Dust.NewDustDirect(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height, 
                    DustID.PurpleTorch, player.velocity.X * 0.2f, player.velocity.Y * 0.2f, 150, default, 1.1f);
                dust.noGravity = true;
                dust.velocity *= 1.2f;
            }

            // Crystal accents - less frequent, more impactful
            if (Main.rand.NextBool(5))
            {
                Dust dust2 = Dust.NewDustDirect(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height, 
                    DustID.PurpleCrystalShard, 0f, 0f, 100, default, 0.8f);
                dust2.noGravity = true;
                dust2.velocity = Main.rand.NextVector2Circular(2f, 2f);
            }
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // === LUNAR CRESCENT BEAM ATTACK ===
            // Fires a large sweeping crescent moon beam that expands as it travels
            // Much more moon-like, beam-like, unique, and flashy!
            
            Vector2 direction = velocity.SafeNormalize(Vector2.UnitX);
            float baseAngle = direction.ToRotation();
            
            // Main crescent moon beam - large and sweeping
            int projDamage = (int)(damage * 0.8f);
            Projectile.NewProjectile(source, position, velocity * 1.2f, type, projDamage, knockback, player.whoAmI, 0f, 0f);
            
            // === MOONLIGHT BURST VFX ===
            UnifiedVFX.MoonlightSonata.SwingAura(position, direction, 1.2f);
            
            // Massive lunar crescent slash - sweeping arc of moonlight
            CustomParticles.SwordArcCrescent(position, velocity * 0.8f, UnifiedVFX.MoonlightSonata.LightBlue, 0.9f);
            
            // Radiating moon phases - crescents expanding outward
            for (int i = 0; i < 8; i++)
            {
                float angle = baseAngle + MathHelper.TwoPi * i / 8f;
                Vector2 crescentVel = angle.ToRotationVector2() * 3f;
                float progress = (float)i / 8f;
                Color crescentColor = Color.Lerp(UnifiedVFX.MoonlightSonata.DarkPurple, UnifiedVFX.MoonlightSonata.LightBlue, progress);
                CustomParticles.SwordArcCrescent(position + angle.ToRotationVector2() * 20f, crescentVel, crescentColor * 0.6f, 0.4f);
            }
            
            // Signature fractal flare burst - larger and more prominent
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 offset = angle.ToRotationVector2() * 35f;
                float progress = (float)i / 8f;
                Color flareColor = Color.Lerp(UnifiedVFX.MoonlightSonata.DarkPurple, UnifiedVFX.MoonlightSonata.LightBlue, progress);
                CustomParticles.GenericFlare(position + offset, flareColor, 0.65f, 22);
            }
            
            // Expanding lunar halo rings
            for (int ring = 0; ring < 5; ring++)
            {
                float progress = (float)ring / 5f;
                Color ringColor = Color.Lerp(UnifiedVFX.MoonlightSonata.DarkPurple, UnifiedVFX.MoonlightSonata.LightBlue, progress);
                float scale = 0.3f + ring * 0.15f;
                CustomParticles.HaloRing(position, ringColor, scale, 15 + ring * 3);
            }
            
            // Moonlight sparkle cascade
            for (int i = 0; i < 12; i++)
            {
                Vector2 sparkleVel = Main.rand.NextVector2Circular(4f, 4f);
                Color sparkleColor = Color.Lerp(UnifiedVFX.MoonlightSonata.MediumPurple, Color.White, Main.rand.NextFloat());
                CustomParticles.PrismaticSparkle(position + Main.rand.NextVector2Circular(20f, 20f), sparkleColor, 0.35f);
            }
            
            // Musical notes floating from swing
            CustomParticles.MoonlightMusicNotes(position, 6, 40f);
            
            // Themed moonlight sparks shooting forward in beam direction
            ThemedParticles.MoonlightSparks(position, velocity, 12, 8f);
            
            // Beam launch sound
            SoundEngine.PlaySound(SoundID.Item60 with { Volume = 0.7f, Pitch = -0.3f }, position);

            return false; // We already created the projectile
        }

        public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            // === UnifiedVFX MOONLIGHT SONATA IMPACT ===
            UnifiedVFX.MoonlightSonata.Impact(target.Center, 1.0f);
            
            // === FRACTAL IMPACT BURST - Moonlight crescent explosion ===
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 flareOffset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 24f;
                float progress = (float)i / 6f;
                Color fractalColor = Color.Lerp(UnifiedVFX.MoonlightSonata.DarkPurple, UnifiedVFX.MoonlightSonata.LightBlue, progress);
                CustomParticles.GenericFlare(target.Center + flareOffset, fractalColor, 0.5f, 17);
            }
            
            // Gradient halo rings
            for (int ring = 0; ring < 3; ring++)
            {
                float progress = (float)ring / 3f;
                Color ringColor = Color.Lerp(UnifiedVFX.MoonlightSonata.MediumPurple, UnifiedVFX.MoonlightSonata.Silver, progress);
                CustomParticles.HaloRing(target.Center, ringColor, 0.45f + ring * 0.1f, 13 + ring * 2);
            }
            
            // Music notes on impact
            CustomParticles.MoonlightMusicNotes(target.Center, 3, 22f);
        }

        public override void HoldItem(Player player)
        {
            // === UnifiedVFX MOONLIGHT SONATA AURA ===
            UnifiedVFX.MoonlightSonata.Aura(player.Center, 32f, 0.28f);
            
            // === AMBIENT FRACTAL FLARES - Moonlight crescent pattern ===
            if (Main.rand.NextBool(6))
            {
                // Crescent moon orbital pattern
                float baseAngle = Main.GameUpdateCount * 0.018f;
                for (int i = 0; i < 4; i++)
                {
                    float angle = baseAngle + MathHelper.TwoPi * i / 4f;
                    float radius = 30f + (float)Math.Sin(Main.GameUpdateCount * 0.04f + i) * 10f;
                    Vector2 flarePos = player.Center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius;
                    float progress = (float)i / 4f;
                    Color fractalColor = Color.Lerp(UnifiedVFX.MoonlightSonata.DarkPurple, UnifiedVFX.MoonlightSonata.LightBlue, progress);
                    CustomParticles.GenericFlare(flarePos, fractalColor, 0.3f, 17);
                }
            }
            
            // Magic sparkle field ambient aura - subtle enchantment glow
            if (Main.rand.NextBool(10))
            {
                Vector2 offset = Main.rand.NextVector2Circular(30f, 30f);
                CustomParticles.MagicSparkleFieldAura(player.Center + offset, UnifiedVFX.MoonlightSonata.Silver * 0.6f, 0.32f, 30);
            }
            
            // Occasional prismatic twinkle with fractal accents
            if (Main.rand.NextBool(12))
            {
                Vector2 offset = Main.rand.NextVector2Circular(25f, 25f);
                Color sparkleColor = Color.Lerp(UnifiedVFX.MoonlightSonata.MediumPurple, UnifiedVFX.MoonlightSonata.LightBlue, Main.rand.NextFloat());
                CustomParticles.PrismaticSparkle(player.Center + offset, sparkleColor, 0.22f);
            }
            
            // Soft gradient lighting with pulse
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.05f) * 0.08f + 0.92f;
            Vector3 lightColor = Color.Lerp(UnifiedVFX.MoonlightSonata.DarkPurple, UnifiedVFX.MoonlightSonata.LightBlue, 0.4f).ToVector3();
            Lighting.AddLight(player.Center, lightColor * pulse * 0.5f);
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            // Draw glowing backlight effect when dropped in world
            Texture2D texture = TextureAssets.Item[Item.type].Value;
            Vector2 position = Item.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;
            
            // Calculate pulse - slow and ethereal for moonlight theme
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.04f) * 0.12f + 1f;
            
            // Begin additive blending for glow
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Outer deep purple aura
            spriteBatch.Draw(texture, position, null, new Color(75, 0, 130) * 0.45f, rotation, origin, scale * pulse * 1.35f, SpriteEffects.None, 0f);
            
            // Middle blue-purple glow
            spriteBatch.Draw(texture, position, null, new Color(138, 43, 226) * 0.35f, rotation, origin, scale * pulse * 1.2f, SpriteEffects.None, 0f);
            
            // Inner silver/lavender glow
            spriteBatch.Draw(texture, position, null, new Color(200, 180, 255) * 0.25f, rotation, origin, scale * pulse * 1.08f, SpriteEffects.None, 0f);
            
            // Return to normal blending
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Add lighting
            Lighting.AddLight(Item.Center, 0.4f, 0.3f, 0.7f);
            
            return true; // Draw the normal sprite too
        }
    }
}
