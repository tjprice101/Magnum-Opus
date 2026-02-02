using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria.GameContent;
using MagnumOpus.Content.Winter.Materials;
using MagnumOpus.Content.Winter.Projectiles;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using static MagnumOpus.Common.Systems.ThemedParticles;

namespace MagnumOpus.Content.Winter.Weapons
{
    /// <summary>
    /// Permafrost Codex - Winter-themed magic weapon (Post-Golem tier)
    /// An ancient tome of frost magic that channels winter's might.
    /// - Frost Barrage: Fires a spread of 5 frost bolts (165 damage)
    /// - Permafrost Barrier: Creates a defensive ice shield after channeling
    /// - Ice Storm: Charge attack summons a devastating blizzard
    /// - Frostbite: All attacks apply stacking frost damage over time
    /// </summary>
    public class PermafrostCodex : ModItem
    {
        private static readonly Color IceBlue = new Color(150, 220, 255);
        private static readonly Color FrostWhite = new Color(240, 250, 255);
        private static readonly Color DeepBlue = new Color(60, 100, 180);
        private static readonly Color CrystalCyan = new Color(100, 255, 255);
        private static readonly Color GlacialPurple = new Color(120, 130, 200);

        private int chargeTime = 0;
        private const int MaxCharge = 90;
        private bool isCharging = false;

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 36;
            Item.damage = 165;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 12;
            Item.useTime = 18;
            Item.useAnimation = 18;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 5f;
            Item.value = Item.buyPrice(gold: 42);
            Item.rare = ItemRarityID.Yellow;
            Item.UseSound = SoundID.Item28;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<PermafrostBolt>();
            Item.shootSpeed = 16f;
            Item.channel = true;
        }

        public override void HoldItem(Player player)
        {
            // ========== IRIDESCENT WINGSPAN VFX PATTERN ==========
            // HEAVY DUST TRAILS - 2+ per frame (glacial purple/ice dust)
            for (int d = 0; d < 2; d++)
            {
                Vector2 dustPos = player.Center + Main.rand.NextVector2Circular(28f, 28f);
                Dust dust = Dust.NewDustPerfect(dustPos, DustID.IceTorch, Main.rand.NextVector2Circular(0.5f, 0.5f) + new Vector2(0, -0.25f), 0, GlacialPurple, Main.rand.NextFloat(1.0f, 1.35f));
                dust.noGravity = true;
                dust.fadeIn = 1.4f;
            }
            
            // CONTRASTING SPARKLES - crystalline/white
            if (Main.rand.NextBool(2))
            {
                Vector2 sparklePos = player.Center + Main.rand.NextVector2Circular(30f, 30f);
                Color sparkleColor = Main.rand.NextBool() ? FrostWhite : CrystalCyan;
                CustomParticles.PrismaticSparkle(sparklePos, sparkleColor, Main.rand.NextFloat(0.35f, 0.5f));
            }
            
            // SHIMMER TRAILS - frost motes with subtle purple glow
            if (Main.rand.NextBool(3))
            {
                float hue = 0.68f + Main.rand.NextFloat(-0.04f, 0.04f); // Purple-blue range
                Color shimmerColor = Main.hslToRgb(hue, 0.55f, 0.75f);
                Vector2 shimmerPos = player.Center + Main.rand.NextVector2Circular(32f, 32f);
                Vector2 shimmerVel = Main.rand.NextVector2Circular(0.5f, 0.5f);
                var shimmer = new GenericGlowParticle(shimmerPos, shimmerVel, shimmerColor * 0.5f, 0.24f, 20, true);
                MagnumParticleHandler.SpawnParticle(shimmer);
            }
            
            // MUSIC NOTES - visible scale with glacial theme
            if (Main.rand.NextBool(5))
            {
                Vector2 notePos = player.Center + Main.rand.NextVector2Circular(36f, 36f);
                Vector2 noteVel = Main.rand.NextVector2Circular(0.6f, 0.6f) + new Vector2(0, -0.25f);
                Color noteColor = Color.Lerp(GlacialPurple, IceBlue, Main.rand.NextFloat(0.5f));
                ThemedParticles.MusicNote(notePos, noteVel, noteColor, Main.rand.NextFloat(0.85f, 1.0f), 26);
            }
            
            // ORBITING RUNE MOTES - frost runes circling
            if (Main.rand.NextBool(4))
            {
                float orbitAngle = Main.GameUpdateCount * 0.04f + Main.rand.NextFloat(MathHelper.TwoPi);
                float orbitRadius = 40f + Main.rand.NextFloat(12f);
                Vector2 orbitPos = player.Center + orbitAngle.ToRotationVector2() * orbitRadius;
                Color orbitColor = Color.Lerp(GlacialPurple, IceBlue, Main.rand.NextFloat()) * 0.45f;
                var mote = new GenericGlowParticle(orbitPos, Vector2.Zero, orbitColor, 0.2f, 14, true);
                MagnumParticleHandler.SpawnParticle(mote);
            }

            // ========== ENHANCED CHARGING EFFECTS ==========
            if (isCharging && chargeTime > 0)
            {
                float chargeProgress = (float)chargeTime / MaxCharge;
                
                // HEAVY converging frost particles (more frequent as charge builds)
                int chargeParticles = 1 + (int)(chargeProgress * 4f);
                for (int i = 0; i < chargeParticles; i++)
                {
                    float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                    float radius = 65f - chargeProgress * 35f;
                    Vector2 chargePos = player.Center + angle.ToRotationVector2() * radius;
                    Vector2 chargeVel = (player.Center - chargePos).SafeNormalize(Vector2.Zero) * (2.5f + chargeProgress * 4f);
                    Color chargeColor = Color.Lerp(IceBlue, FrostWhite, chargeProgress) * (0.4f + chargeProgress * 0.35f);
                    var charge = new GenericGlowParticle(chargePos, chargeVel, chargeColor, 0.22f + chargeProgress * 0.18f, 16, true);
                    MagnumParticleHandler.SpawnParticle(charge);
                }
                
                // Charging sparkles crescendo
                if (chargeProgress > 0.5f && Main.rand.NextBool(3))
                {
                    Vector2 sparklePos = player.Center + Main.rand.NextVector2Circular(20f, 20f);
                    CustomParticles.PrismaticSparkle(sparklePos, FrostWhite, Main.rand.NextFloat(0.35f, 0.55f) * chargeProgress);
                }
                
                // Full charge music note burst
                if (chargeTime == MaxCharge && Main.rand.NextBool(2))
                {
                    Vector2 notePos = player.Center + Main.rand.NextVector2Circular(15f, 15f);
                    Vector2 noteVel = Main.rand.NextVector2Circular(1f, 1f);
                    ThemedParticles.MusicNote(notePos, noteVel, CrystalCyan, Main.rand.NextFloat(0.9f, 1.1f), 24);
                }
            }

            // Enhanced dynamic lighting
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.05f) * 0.1f + 0.5f;
            float chargeBonus = isCharging ? (float)chargeTime / MaxCharge * 0.3f : 0f;
            Lighting.AddLight(player.Center, GlacialPurple.ToVector3() * (pulse + chargeBonus));
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Vector2 spawnPos = position + velocity.SafeNormalize(Vector2.Zero) * 30f;

            // Check if fully charged
            if (chargeTime >= MaxCharge)
            {
                // ========== ICE STORM SPECTACULAR VFX ==========
                chargeTime = 0;
                isCharging = false;

                // Spawn blizzard projectile
                Projectile.NewProjectile(source, spawnPos, velocity * 0.5f, ModContent.ProjectileType<IceStormProjectile>(),
                    (int)(damage * 2.5f), knockback * 1.5f, player.whoAmI);

                // MULTI-LAYER FLARES - white core → ice → purple
                CustomParticles.GenericFlare(spawnPos, FrostWhite, 1.2f, 26);
                CustomParticles.GenericFlare(spawnPos, IceBlue, 0.9f, 24);
                CustomParticles.GenericFlare(spawnPos, GlacialPurple, 0.65f, 22);
                
                // GRADIENT HALO CASCADE - 6 layers IceBlue → DeepBlue
                for (int i = 0; i < 6; i++)
                {
                    float progress = i / 5f;
                    Color haloColor = Color.Lerp(IceBlue, DeepBlue, progress);
                    float haloScale = 0.4f + i * 0.12f;
                    int haloLife = 16 + i * 2;
                    CustomParticles.HaloRing(spawnPos, haloColor * (0.75f - progress * 0.3f), haloScale, haloLife);
                }
                
                // RADIAL FROST DUST EXPLOSION
                for (int i = 0; i < 16; i++)
                {
                    float angle = MathHelper.TwoPi * i / 16f;
                    Vector2 burstVel = angle.ToRotationVector2() * Main.rand.NextFloat(5f, 10f);
                    Dust iceDust = Dust.NewDustPerfect(spawnPos, DustID.IceTorch, burstVel, 0, IceBlue, Main.rand.NextFloat(1.2f, 1.6f));
                    iceDust.noGravity = true;
                    iceDust.fadeIn = 1.4f;
                }
                
                // GLOW PARTICLE BURST
                for (int i = 0; i < 10; i++)
                {
                    Vector2 burstVel = Main.rand.NextVector2Circular(9f, 9f);
                    Color burstColor = Color.Lerp(IceBlue, FrostWhite, Main.rand.NextFloat()) * 0.6f;
                    var burst = new GenericGlowParticle(spawnPos, burstVel, burstColor, 0.3f, 22, true);
                    MagnumParticleHandler.SpawnParticle(burst);
                }
                
                // MUSIC NOTE STARBURST
                for (int i = 0; i < 5; i++)
                {
                    float noteAngle = MathHelper.TwoPi * i / 5f + Main.rand.NextFloat(0.3f);
                    Vector2 noteVel = noteAngle.ToRotationVector2() * Main.rand.NextFloat(1.5f, 3.5f);
                    Color noteColor = Color.Lerp(GlacialPurple, IceBlue, Main.rand.NextFloat(0.5f));
                    ThemedParticles.MusicNote(spawnPos, noteVel, noteColor, Main.rand.NextFloat(0.9f, 1.1f), 28);
                }
                
                // SPARKLE CORONA
                for (int i = 0; i < 8; i++)
                {
                    Vector2 sparklePos = spawnPos + Main.rand.NextVector2Circular(28f, 28f);
                    Color sparkleColor = Main.rand.NextBool() ? FrostWhite : CrystalCyan;
                    CustomParticles.PrismaticSparkle(sparklePos, sparkleColor, Main.rand.NextFloat(0.45f, 0.65f));
                }

                return false;
            }

            // ========== NORMAL BARRAGE SPECTACULAR VFX ==========
            // Normal attack - frost barrage (5 bolts)
            for (int i = -2; i <= 2; i++)
            {
                float spreadAngle = MathHelper.ToRadians(i * 7f);
                Vector2 boltVel = velocity.RotatedBy(spreadAngle);
                Projectile.NewProjectile(source, spawnPos, boltVel, type, damage, knockback, player.whoAmI);
            }

            // MULTI-LAYER CAST FLASH
            CustomParticles.GenericFlare(spawnPos, FrostWhite, 0.75f, 18);
            CustomParticles.GenericFlare(spawnPos, IceBlue, 0.55f, 16);
            
            // GRADIENT HALO RINGS - 4 layer cascade
            for (int i = 0; i < 4; i++)
            {
                float progress = i / 3f;
                Color haloColor = Color.Lerp(GlacialPurple, DeepBlue, progress);
                CustomParticles.HaloRing(spawnPos, haloColor * (0.6f - progress * 0.2f), 0.24f + i * 0.08f, 12 + i * 2);
            }
            
            // DIRECTIONAL FROST SPARKS
            for (int i = 0; i < 6; i++)
            {
                Vector2 sparkVel = velocity.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(4f, 8f) + Main.rand.NextVector2Circular(2.5f, 2.5f);
                Color sparkColor = Color.Lerp(IceBlue, GlacialPurple, Main.rand.NextFloat()) * 0.55f;
                var spark = new GenericGlowParticle(spawnPos, sparkVel, sparkColor, 0.25f, 16, true);
                MagnumParticleHandler.SpawnParticle(spark);
            }
            
            // MUSIC NOTE ACCENT
            if (Main.rand.NextBool(2))
            {
                Vector2 noteVel = velocity.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(1f, 2.5f);
                Color noteColor = Color.Lerp(GlacialPurple, IceBlue, Main.rand.NextFloat(0.5f));
                ThemedParticles.MusicNote(spawnPos, noteVel, noteColor, Main.rand.NextFloat(0.85f, 0.98f), 24);
            }
            
            // CONTRASTING SPARKLE
            CustomParticles.PrismaticSparkle(spawnPos + Main.rand.NextVector2Circular(14f, 14f), CrystalCyan, Main.rand.NextFloat(0.38f, 0.52f));

            // Start charging for next shot
            isCharging = true;

            return false;
        }

        public override void UpdateInventory(Player player)
        {
            // Update charge while channeling
            if (player.channel && player.HeldItem == Item)
            {
                if (chargeTime < MaxCharge)
                {
                    chargeTime++;
                }
            }
            else
            {
                chargeTime = Math.Max(0, chargeTime - 2);
                if (chargeTime == 0)
                    isCharging = false;
            }
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D texture = TextureAssets.Item[Item.type].Value;
            Vector2 position = Item.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;

            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.05f) * 0.1f + 1f;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            spriteBatch.Draw(texture, position, null, GlacialPurple * 0.35f, rotation, origin, scale * pulse * 1.3f, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, position, null, IceBlue * 0.25f, rotation, origin, scale * 1.15f, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, position, null, FrostWhite * 0.2f, rotation, origin, scale * 1.05f, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Lighting.AddLight(Item.Center, GlacialPurple.ToVector3() * 0.4f);

            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "FrostBarrage", "Fires a spread of 5 frost bolts") { OverrideColor = IceBlue });
            tooltips.Add(new TooltipLine(Mod, "IceStorm", "Hold to charge, release a devastating blizzard") { OverrideColor = CrystalCyan });
            tooltips.Add(new TooltipLine(Mod, "Frostbite", "All attacks apply stacking frost damage") { OverrideColor = GlacialPurple });
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Written in the tongue of eternal frost'") { OverrideColor = Color.Lerp(GlacialPurple, FrostWhite, 0.5f) });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<PermafrostBar>(), 18)
                .AddIngredient(ModContent.ItemType<WinterResonantEnergy>(), 1)
                .AddIngredient(ItemID.SoulofSight, 8)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}
