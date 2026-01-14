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
            // GRADIENT COLORS
            Color darkPurple = new Color(75, 0, 130);
            Color violet = new Color(138, 43, 226);
            Color lightBlue = new Color(135, 206, 250);
            
            // Ambient fractal orbit pattern with GRADIENT
            if (Main.rand.NextBool(6))
            {
                float baseAngle = Main.GameUpdateCount * 0.02f;
                for (int i = 0; i < 6; i++)
                {
                    float angle = baseAngle + MathHelper.TwoPi * i / 6f;
                    float radius = 30f + (float)Math.Sin(Main.GameUpdateCount * 0.04f + i * 0.5f) * 12f;
                    Vector2 flarePos = player.Center + angle.ToRotationVector2() * radius;
                    float progress = (float)i / 6f;
                    Color fractalColor = Color.Lerp(darkPurple, lightBlue, progress);
                    CustomParticles.GenericFlare(flarePos, fractalColor, 0.3f, 16);
                }
            }
            
            // Magical moonlight particles with GRADIENT
            if (Main.rand.NextBool(4))
            {
                Vector2 offset = Main.rand.NextVector2Circular(22f, 22f);
                float progress = Main.rand.NextFloat();
                Color gradientColor = Color.Lerp(violet, lightBlue, progress);
                CustomParticles.GenericGlow(player.Center + offset, gradientColor, 0.25f, 18);
            }
            
            // Soft mystical glow
            Lighting.AddLight(player.Center, 0.3f, 0.25f, 0.5f);
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
            // GRADIENT COLORS: Dark Purple → Violet → Light Blue
            Color darkPurple = new Color(75, 0, 130);
            Color violet = new Color(138, 43, 226);
            Color lightBlue = new Color(135, 206, 250);
            Color silver = new Color(220, 220, 235);
            
            // Apply the buff
            player.AddBuff(Item.buffType, 18000);
            
            // Spawn position at mouse
            position = Main.MouseWorld;
            
            // === CUSTOM PARTICLE EFFECTS WITH GRADIENT FADING ===
            
            // Central fractal burst - geometric flares with gradient
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 offset = angle.ToRotationVector2() * 40f;
                float progress = (float)i / 8f;
                Color gradientColor = Color.Lerp(darkPurple, lightBlue, progress);
                CustomParticles.GenericFlare(position + offset, gradientColor, 0.6f, 22);
            }
            
            // Gradient halo rings - dark to bright
            for (int ring = 0; ring < 5; ring++)
            {
                float progress = (float)ring / 5f;
                Color ringColor = Color.Lerp(darkPurple, lightBlue, progress);
                CustomParticles.HaloRing(position, ringColor, 0.4f + ring * 0.18f, 18 + ring * 4);
            }
            
            // Explosion burst with gradient
            for (int i = 0; i < 20; i++)
            {
                float progress = (float)i / 20f;
                Color burstColor = Color.Lerp(violet, lightBlue, progress);
                CustomParticles.GenericGlow(position, burstColor, 0.4f, 25);
            }
            
            // Central silver flash
            CustomParticles.GenericFlare(position, silver, 1.0f, 18);
            
            // Themed impact
            ThemedParticles.MoonlightImpact(position, 1.8f);
            ThemedParticles.MoonlightHaloBurst(position, 1.5f);
            
            // === VANILLA DUST WITH GRADIENT COLORS ===
            
            // Epic summoning effects - massive moonlight vortex for Goliath
            for (int i = 0; i < 50; i++)
            {
                float angle = MathHelper.TwoPi * i / 50f;
                float progress = (float)i / 50f;
                Vector2 dustPos = position + new Vector2((float)System.Math.Cos(angle), (float)System.Math.Sin(angle)) * 80f;
                Vector2 dustVel = (position - dustPos).SafeNormalize(Vector2.Zero) * 6f;
                Color dustColor = Color.Lerp(darkPurple, lightBlue, progress);
                int dustType = progress < 0.5f ? DustID.PurpleTorch : DustID.IceTorch;
                Dust dust = Dust.NewDustPerfect(dustPos, dustType, dustVel, 100, dustColor, 2.2f);
                dust.noGravity = true;
            }
            
            // Inner burst with gradient
            for (int i = 0; i < 35; i++)
            {
                float progress = (float)i / 35f;
                Vector2 dustVel = Main.rand.NextVector2Circular(10f, 10f);
                Color dustColor = Color.Lerp(violet, lightBlue, progress);
                int dustType = progress < 0.5f ? DustID.PurpleTorch : DustID.IceTorch;
                Dust dust = Dust.NewDustDirect(position, 1, 1, dustType, dustVel.X, dustVel.Y, 100, dustColor, 2.2f);
                dust.noGravity = true;
            }
            
            // White/silver sparkles
            for (int i = 0; i < 20; i++)
            {
                CustomParticles.GenericGlow(position, silver, 0.35f, 15);
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
