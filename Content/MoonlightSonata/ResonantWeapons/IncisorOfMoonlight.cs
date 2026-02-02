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
            Vector2 hitCenter = hitbox.Center.ToVector2();
            
            // === SPECTACULAR SWING SYSTEM - ENDGAME TIER (7-8 layered arcs + lunar effects) ===
            SpectacularMeleeSwing.OnSwing(player, hitbox, UnifiedVFX.MoonlightSonata.DarkPurple, UnifiedVFX.MoonlightSonata.LightBlue, 
                SpectacularMeleeSwing.SwingTier.Endgame, SpectacularMeleeSwing.WeaponTheme.MoonlightSonata);
            
            // === IRIDESCENT WINGSPAN-STYLE HEAVY DUST TRAILS ===
            // Heavy purple dust trail #1
            float trailProgress1 = Main.rand.NextFloat();
            Color purpleGradient1 = Color.Lerp(UnifiedVFX.MoonlightSonata.DarkPurple, UnifiedVFX.MoonlightSonata.MediumPurple, trailProgress1);
            Dust heavyPurple = Dust.NewDustDirect(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height, 
                DustID.PurpleTorch, player.velocity.X * 0.3f, player.velocity.Y * 0.3f, 100, purpleGradient1, 1.5f);
            heavyPurple.noGravity = true;
            heavyPurple.fadeIn = 1.4f;
            heavyPurple.velocity = heavyPurple.velocity.RotatedByRandom(0.3f) * Main.rand.NextFloat(1.2f, 1.8f);
            
            // Heavy blue dust trail #2
            float trailProgress2 = Main.rand.NextFloat();
            Color blueGradient = Color.Lerp(UnifiedVFX.MoonlightSonata.MediumPurple, UnifiedVFX.MoonlightSonata.LightBlue, trailProgress2);
            Dust heavyBlue = Dust.NewDustDirect(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height, 
                DustID.BlueTorch, player.velocity.X * 0.25f, player.velocity.Y * 0.25f, 80, blueGradient, 1.4f);
            heavyBlue.noGravity = true;
            heavyBlue.fadeIn = 1.3f;
            heavyBlue.velocity = heavyBlue.velocity.RotatedByRandom(0.4f) * Main.rand.NextFloat(1.0f, 1.6f);
            
            // === CONTRASTING SILVER SPARKLES (1-in-2) ===
            if (Main.rand.NextBool(2))
            {
                Vector2 sparklePos = new Vector2(hitbox.X + Main.rand.Next(hitbox.Width), hitbox.Y + Main.rand.Next(hitbox.Height));
                CustomParticles.PrismaticSparkle(sparklePos, UnifiedVFX.MoonlightSonata.Silver, 0.35f);
                
                Dust silverDust = Dust.NewDustDirect(sparklePos, 1, 1, DustID.SilverCoin, 0f, 0f, 100, default, 0.9f);
                silverDust.noGravity = true;
                silverDust.velocity = Main.rand.NextVector2Circular(2f, 2f);
            }
            
            // === LUNAR SHIMMER TRAIL (Main.hslToRgb rainbow in purple-blue range) (1-in-3) ===
            if (Main.rand.NextBool(3))
            {
                // Lunar shimmer - cycling through purple to blue hues (0.7-0.85 hue range)
                float lunarHue = 0.7f + (Main.GameUpdateCount * 0.015f % 0.15f);
                Color shimmerColor = Main.hslToRgb(lunarHue, 0.9f, 0.75f);
                Vector2 shimmerPos = new Vector2(hitbox.X + Main.rand.Next(hitbox.Width), hitbox.Y + Main.rand.Next(hitbox.Height));
                CustomParticles.GenericFlare(shimmerPos, shimmerColor, 0.4f, 12);
            }
            
            // === PEARLESCENT MOONSTONE EFFECTS (1-in-4) ===
            if (Main.rand.NextBool(4))
            {
                Vector2 pearlPos = new Vector2(hitbox.X + Main.rand.Next(hitbox.Width), hitbox.Y + Main.rand.Next(hitbox.Height));
                // Pearlescent color shift - iridescent blue-purple-white
                float pearlShift = (Main.GameUpdateCount * 0.02f) % 1f;
                Color pearlColor = Color.Lerp(Color.Lerp(UnifiedVFX.MoonlightSonata.LightBlue, Color.White, pearlShift), 
                    UnifiedVFX.MoonlightSonata.MediumPurple, (float)Math.Sin(pearlShift * MathHelper.TwoPi) * 0.3f + 0.3f);
                CustomParticles.GenericFlare(pearlPos, pearlColor * 0.8f, 0.3f, 15);
                
                Dust pearlDust = Dust.NewDustDirect(pearlPos, 1, 1, DustID.PinkFairy, 0f, 0f, 50, default, 0.7f);
                pearlDust.noGravity = true;
            }
            
            // === FREQUENT FLARES (1-in-2) ===
            if (Main.rand.NextBool(2))
            {
                Vector2 flarePos = new Vector2(hitbox.X + Main.rand.Next(hitbox.Width), hitbox.Y + Main.rand.Next(hitbox.Height));
                Color flareColor = Color.Lerp(UnifiedVFX.MoonlightSonata.DarkPurple, UnifiedVFX.MoonlightSonata.LightBlue, Main.rand.NextFloat());
                CustomParticles.GenericFlare(flarePos, flareColor, 0.35f, 10);
            }
            
            // === MOONLIGHT SWORD ARC SLASH (1-in-3) ===
            if (Main.rand.NextBool(3))
            {
                Vector2 slashVel = (player.direction * Vector2.UnitX).RotatedByRandom(0.3f) * Main.rand.NextFloat(2f, 4f);
                CustomParticles.SwordArcCrescent(hitCenter, slashVel, UnifiedVFX.MoonlightSonata.LightBlue * 0.8f, 0.5f);
            }
            
            // Crystal accents - less frequent, more impactful
            if (Main.rand.NextBool(4))
            {
                Dust dust2 = Dust.NewDustDirect(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height, 
                    DustID.PurpleCrystalShard, 0f, 0f, 100, default, 1.0f);
                dust2.noGravity = true;
                dust2.velocity = Main.rand.NextVector2Circular(2.5f, 2.5f);
            }
            
            // === MUSIC NOTES (1-in-6) ===
            if (Main.rand.NextBool(6))
            {
                Vector2 noteVel = (player.direction * Vector2.UnitX).RotatedByRandom(0.5f) * Main.rand.NextFloat(1.5f, 3f);
                Color noteColor = Color.Lerp(UnifiedVFX.MoonlightSonata.MediumPurple, UnifiedVFX.MoonlightSonata.Silver, Main.rand.NextFloat());
                ThemedParticles.MusicNote(hitCenter, noteVel, noteColor, 0.85f, 32);
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
            
            // === MOONLIGHT BURST VFX (Swan Lake benchmark: trust UnifiedVFX + minimal extras) ===
            UnifiedVFX.MoonlightSonata.SwingAura(position, direction, 1.0f);
            
            // Lunar crescent slash
            CustomParticles.SwordArcCrescent(position, velocity * 0.6f, UnifiedVFX.MoonlightSonata.LightBlue, 0.7f);
            
            // Single halo ring
            CustomParticles.HaloRing(position, UnifiedVFX.MoonlightSonata.MediumPurple, 0.4f, 18);
            
            // Gentle sparkle accents
            for (int i = 0; i < 4; i++)
            {
                CustomParticles.PrismaticSparkle(position + Main.rand.NextVector2Circular(15f, 15f), 
                    UnifiedVFX.MoonlightSonata.Silver, 0.25f);
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
            
            // === IRIDESCENT WINGSPAN-STYLE GRADIENT HALO RINGS (4 stacked) ===
            CustomParticles.HaloRing(target.Center, UnifiedVFX.MoonlightSonata.DarkPurple, 0.55f, 16);
            CustomParticles.HaloRing(target.Center, UnifiedVFX.MoonlightSonata.MediumPurple, 0.45f, 14);
            CustomParticles.HaloRing(target.Center, UnifiedVFX.MoonlightSonata.LightBlue, 0.35f, 12);
            CustomParticles.HaloRing(target.Center, Color.White * 0.9f, 0.25f, 10);
            
            // === MUSIC NOTES BURST (8 notes) ===
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 noteVel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 5f);
                Color noteColor = Color.Lerp(UnifiedVFX.MoonlightSonata.MediumPurple, UnifiedVFX.MoonlightSonata.Silver, Main.rand.NextFloat());
                ThemedParticles.MusicNote(target.Center, noteVel, noteColor, 0.9f, 35);
            }
            
            // === LUNAR SHIMMER FLARE BURST (10 flares) ===
            for (int i = 0; i < 10; i++)
            {
                float lunarHue = 0.7f + Main.rand.NextFloat(0.15f);
                Color shimmerColor = Main.hslToRgb(lunarHue, 0.9f, 0.8f);
                Vector2 flarePos = target.Center + Main.rand.NextVector2Circular(25f, 25f);
                CustomParticles.GenericFlare(flarePos, shimmerColor, 0.45f, 18);
            }
            
            // === RADIAL DUST EXPLOSION (16 dust particles) ===
            for (int i = 0; i < 16; i++)
            {
                float angle = MathHelper.TwoPi * i / 16f;
                Vector2 dustVel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 8f);
                
                Dust purpleDust = Dust.NewDustDirect(target.Center, 1, 1, DustID.PurpleTorch, dustVel.X, dustVel.Y, 100, default, 1.4f);
                purpleDust.noGravity = true;
                purpleDust.fadeIn = 1.2f;
            }
            
            // === SILVER CONTRASTING SPARKLES (6 sparkles) ===
            for (int i = 0; i < 6; i++)
            {
                Vector2 sparklePos = target.Center + Main.rand.NextVector2Circular(30f, 30f);
                CustomParticles.PrismaticSparkle(sparklePos, UnifiedVFX.MoonlightSonata.Silver, 0.4f);
                
                Dust silverDust = Dust.NewDustDirect(sparklePos, 1, 1, DustID.SilverCoin, 0f, 0f, 80, default, 1.0f);
                silverDust.noGravity = true;
                silverDust.velocity = Main.rand.NextVector2Circular(3f, 3f);
            }
            
            // === CRYSTAL SHARD BURST (6 crystals) ===
            for (int i = 0; i < 6; i++)
            {
                Vector2 crystalVel = Main.rand.NextVector2Circular(4f, 4f);
                Dust crystal = Dust.NewDustDirect(target.Center, 1, 1, DustID.PurpleCrystalShard, crystalVel.X, crystalVel.Y, 100, default, 1.1f);
                crystal.noGravity = true;
            }
            
            // === SPAWN SEEKING CRYSTALS - LUNAR SHARDS ===
            // On hit, release 3 homing lunar crystal shards (5 on crit)
            int crystalCount = hit.Crit ? 5 : 3;
            Vector2 crystalDir = (target.Center - player.Center).SafeNormalize(Vector2.UnitX);
            SeekingCrystalHelper.SpawnMoonlightCrystals(
                player.GetSource_ItemUse(player.HeldItem),
                target.Center,
                crystalDir * 7f,
                (int)(damageDone * 0.35f),
                2.5f,
                player.whoAmI,
                crystalCount
            );
        }

        public override void HoldItem(Player player)
        {
            // === UnifiedVFX MOONLIGHT SONATA AURA ===
            UnifiedVFX.MoonlightSonata.Aura(player.Center, 32f, 0.28f);
            
            // Subtle ambient sparkle (reduced frequency for cleaner look)
            if (Main.rand.NextBool(15))
            {
                Vector2 offset = Main.rand.NextVector2Circular(25f, 25f);
                CustomParticles.PrismaticSparkle(player.Center + offset, UnifiedVFX.MoonlightSonata.Silver * 0.6f, 0.2f);
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

        public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Swings fire sweeping lunar crescent beams"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Hold right-click to charge a lunar storm attack"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Ethereal moonlight aura while held"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'A blade forged from crystallized moonlight'")
            {
                OverrideColor = new Microsoft.Xna.Framework.Color(140, 100, 200)
            });
        }
    }
}
