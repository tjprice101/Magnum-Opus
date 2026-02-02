using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria.GameContent;
using MagnumOpus.Content.Summer.Materials;
using MagnumOpus.Content.Summer.Projectiles;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using static MagnumOpus.Common.Systems.ThemedParticles;

namespace MagnumOpus.Content.Summer.Weapons
{
    /// <summary>
    /// Solstice Tome - Summer-themed magic tome (Post-Mechs tier)
    /// Channels the power of the summer solstice to devastate foes.
    /// - Solar Barrage: Fires rapid solar orbs that explode on impact
    /// - Sunbeam Charge: Hold to charge a devastating beam attack
    /// - Solstice Blessing: Every 10 kills grants temporary damage boost
    /// - Radiant Aura: Provides light and minor health regen while held
    /// </summary>
    public class SolsticeTome : ModItem
    {
        private int chargeTime = 0;
        private int killCount = 0;
        private int blessingTimer = 0;
        private bool isCharging = false;

        private static readonly Color SunGold = new Color(255, 215, 0);
        private static readonly Color SunOrange = new Color(255, 140, 0);
        private static readonly Color SunWhite = new Color(255, 250, 240);
        private static readonly Color SunRed = new Color(255, 100, 50);

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 34;
            Item.damage = 95;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 8;
            Item.useTime = 14;
            Item.useAnimation = 14;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 4f;
            Item.value = Item.buyPrice(gold: 25);
            Item.rare = ItemRarityID.Pink;
            Item.UseSound = SoundID.Item43;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<SolarOrbProjectile>();
            Item.shootSpeed = 14f;
            Item.noMelee = true;
            Item.channel = true;
            Item.staff[Type] = true;
        }

        public override void HoldItem(Player player)
        {
            // Radiant Aura: Light and minor health regen
            player.lifeRegen += 2;
            
            // Blessing timer decay
            if (blessingTimer > 0)
            {
                blessingTimer--;
                player.GetDamage(DamageClass.Magic) += 0.15f; // +15% damage during blessing
            }

            // ========== IRIDESCENT WINGSPAN VFX PATTERN ==========
            // HEAVY DUST TRAILS - 2+ per frame with fadeIn
            for (int d = 0; d < 2; d++)
            {
                Vector2 dustPos = player.Center + Main.rand.NextVector2Circular(24f, 24f);
                Dust dust = Dust.NewDustPerfect(dustPos, DustID.GoldFlame, new Vector2(0, -Main.rand.NextFloat(0.5f, 1.3f)), 0, SunGold, Main.rand.NextFloat(1.0f, 1.4f));
                dust.noGravity = true;
                dust.fadeIn = 1.4f;
            }
            
            // CONTRASTING SPARKLES - white brilliance
            if (Main.rand.NextBool(2))
            {
                Vector2 sparklePos = player.Center + Main.rand.NextVector2Circular(30f, 30f);
                CustomParticles.PrismaticSparkle(sparklePos, SunWhite, Main.rand.NextFloat(0.35f, 0.5f));
            }
            
            // SHIMMER TRAILS - solar motes with color cycling
            if (Main.rand.NextBool(3))
            {
                float hue = 0.1f + Main.rand.NextFloat(0.06f); // Gold range
                Color shimmerColor = Main.hslToRgb(hue, 1f, 0.75f);
                Vector2 shimmerPos = player.Center + Main.rand.NextVector2Circular(28f, 28f);
                Vector2 shimmerVel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -Main.rand.NextFloat(0.6f, 1.4f));
                var shimmer = new GenericGlowParticle(shimmerPos, shimmerVel, shimmerColor * 0.6f, 0.26f, 24, true);
                MagnumParticleHandler.SpawnParticle(shimmer);
            }
            
            // MUSIC NOTES - visible scale with solstice theme
            if (Main.rand.NextBool(5))
            {
                Vector2 notePos = player.Center + Main.rand.NextVector2Circular(32f, 32f);
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-0.6f, 0.6f), -Main.rand.NextFloat(0.5f, 1.2f));
                Color noteColor = Color.Lerp(SunGold, SunWhite, Main.rand.NextFloat(0.3f));
                ThemedParticles.MusicNote(notePos, noteVel, noteColor, Main.rand.NextFloat(0.85f, 1.05f), 30);
            }
            
            // ORBITING LIGHT MOTES - solar wisdom
            if (Main.rand.NextBool(4))
            {
                float orbitAngle = Main.GameUpdateCount * 0.05f + Main.rand.NextFloat(MathHelper.TwoPi);
                float orbitRadius = 38f + Main.rand.NextFloat(12f);
                Vector2 orbitPos = player.Center + orbitAngle.ToRotationVector2() * orbitRadius;
                var mote = new GenericGlowParticle(orbitPos, Vector2.Zero, SunGold * 0.5f, 0.22f, 15, true);
                MagnumParticleHandler.SpawnParticle(mote);
            }

            // Charging effect
            if (player.channel && player.CheckMana(Item.mana, false))
            {
                if (!isCharging)
                {
                    isCharging = true;
                    chargeTime = 0;
                }
                chargeTime++;
                
                // ========== ENHANCED CHARGE PARTICLES ==========
                if (chargeTime > 20 && chargeTime % 5 == 0)
                {
                    float chargeProgress = Math.Min(1f, (chargeTime - 20) / 60f);
                    int particleCount = (int)(6 + chargeProgress * 10);
                    
                    for (int i = 0; i < particleCount; i++)
                    {
                        float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                        float dist = 100f * (1f - chargeProgress * 0.6f);
                        Vector2 particlePos = player.Center + angle.ToRotationVector2() * dist;
                        Vector2 particleVel = (player.Center - particlePos).SafeNormalize(Vector2.Zero) * (5f + chargeProgress * 4f);
                        Color particleColor = Color.Lerp(SunGold, SunWhite, chargeProgress);
                        var particle = new GenericGlowParticle(particlePos, particleVel, particleColor * 0.75f, 0.32f + chargeProgress * 0.2f, 15, true);
                        MagnumParticleHandler.SpawnParticle(particle);
                        
                        // Heavy dust trails during charge
                        Dust dust = Dust.NewDustPerfect(particlePos, DustID.GoldFlame, particleVel * 0.6f, 0, SunGold, 1.2f);
                        dust.noGravity = true;
                        dust.fadeIn = 1.3f;
                    }
                    
                    // Core glow intensifying
                    CustomParticles.GenericFlare(player.Center, SunGold * (0.4f + chargeProgress * 0.5f), 0.45f + chargeProgress * 0.4f, 10);
                    
                    // Sparkle ring at full charge
                    if (chargeProgress > 0.8f)
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            Vector2 sparklePos = player.Center + Main.rand.NextVector2Circular(20f, 20f);
                            CustomParticles.PrismaticSparkle(sparklePos, SunWhite, 0.4f);
                        }
                    }
                }
            }
            else if (isCharging)
            {
                // Release charged attack
                if (chargeTime >= 80) // Fully charged
                {
                    ReleaseChargedBeam(player);
                }
                isCharging = false;
                chargeTime = 0;
            }

            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.05f) * 0.12f + 0.6f;
            Lighting.AddLight(player.Center, SunGold.ToVector3() * pulse);
        }

        private void ReleaseChargedBeam(Player player)
        {
            if (Main.myPlayer != player.whoAmI) return;
            
            Vector2 direction = (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX);
            Vector2 spawnPos = player.Center + direction * 30f;
            
            // Spawn sunbeam
            Projectile.NewProjectile(player.GetSource_ItemUse(Item), spawnPos, direction * 20f,
                ModContent.ProjectileType<SunbeamProjectile>(), Item.damage * 3, Item.knockBack * 2f, player.whoAmI);
            
            // ========== SPECTACULAR SUNBEAM RELEASE VFX ==========
            // MULTI-LAYER CENTRAL FLARE - blinding solar burst
            CustomParticles.GenericFlare(spawnPos, Color.White, 1.0f, 20);
            CustomParticles.GenericFlare(spawnPos, SunGold, 0.8f, 22);
            CustomParticles.GenericFlare(spawnPos, SunOrange * 0.9f, 0.6f, 24);
            
            // 6-LAYER GRADIENT HALO CASCADE - white to gold to orange
            for (int ring = 0; ring < 6; ring++)
            {
                float progress = ring / 6f;
                Color ringColor = Color.Lerp(SunWhite, SunOrange, progress);
                float ringScale = 0.4f + ring * 0.14f;
                int ringLife = 16 + ring * 3;
                CustomParticles.HaloRing(spawnPos, ringColor * (0.75f - progress * 0.3f), ringScale, ringLife);
            }
            
            // RADIAL SOLAR DUST BURST
            for (int i = 0; i < 14; i++)
            {
                float angle = MathHelper.TwoPi * i / 14f;
                Vector2 dustVel = angle.ToRotationVector2() * Main.rand.NextFloat(5f, 10f);
                Dust sun = Dust.NewDustPerfect(spawnPos, DustID.GoldFlame, dustVel, 0, SunGold, 1.5f);
                sun.noGravity = true;
                sun.fadeIn = 1.4f;
            }
            
            // DIRECTIONAL BEAM SPARKS - along firing direction
            for (int i = 0; i < 6; i++)
            {
                Vector2 beamSparkVel = direction.RotatedByRandom(0.3f) * Main.rand.NextFloat(8f, 14f);
                var beamSpark = new GenericGlowParticle(spawnPos, beamSparkVel, SunWhite * 0.8f, 0.35f, 20, true);
                MagnumParticleHandler.SpawnParticle(beamSpark);
            }
            
            // MUSIC NOTE STARBURST - solar symphony release
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f + Main.rand.NextFloat(0.15f);
                Vector2 noteVel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 5f);
                Color noteColor = Color.Lerp(SunGold, SunWhite, Main.rand.NextFloat(0.4f));
                ThemedParticles.MusicNote(spawnPos, noteVel, noteColor, 0.95f, 30);
            }
            
            // SPARKLE CORONA
            for (int i = 0; i < 8; i++)
            {
                Vector2 sparklePos = spawnPos + Main.rand.NextVector2Circular(45f, 45f);
                CustomParticles.PrismaticSparkle(sparklePos, SunWhite, 0.5f);
            }
            
            // Mana cost
            player.CheckMana(Item.mana * 5, true);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (isCharging && chargeTime > 20) return false; // Don't fire while charging

            // Solar Barrage: Rapid solar orbs
            Vector2 offset = Main.rand.NextVector2Circular(3f, 3f);
            Projectile.NewProjectile(source, position, velocity + offset, type, damage, knockback, player.whoAmI);

            // ========== ENHANCED SOLAR BARRAGE CAST VFX ==========
            // Multi-layer cast flash
            CustomParticles.GenericFlare(position, Color.White * 0.8f, 0.4f, 10);
            CustomParticles.GenericFlare(position, SunGold, 0.35f, 12);
            
            // Directional solar sparks
            for (int i = 0; i < 3; i++)
            {
                Vector2 sparkVel = velocity.SafeNormalize(Vector2.UnitX).RotatedByRandom(0.5f) * Main.rand.NextFloat(3f, 6f);
                Dust spark = Dust.NewDustPerfect(position, DustID.GoldFlame, sparkVel, 0, SunGold, 1.1f);
                spark.noGravity = true;
                spark.fadeIn = 1.2f;
            }
            
            // Occasional sparkle accent
            if (Main.rand.NextBool(3))
            {
                CustomParticles.PrismaticSparkle(position, SunWhite, 0.35f);
            }

            return false;
        }

        public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Solstice Blessing: Track kills
            if (target.life <= 0)
            {
                killCount++;
                if (killCount >= 10)
                {
                    killCount = 0;
                    blessingTimer = 300; // 5 seconds of bonus damage
                    
                    // ========== SPECTACULAR SOLSTICE BLESSING VFX ==========
                    // CENTRAL BLESSING FLARE - divine radiance
                    CustomParticles.GenericFlare(player.Center, Color.White, 0.9f, 20);
                    CustomParticles.GenericFlare(player.Center, SunGold, 0.7f, 22);
                    
                    // 5-LAYER GRADIENT HALO CASCADE - blessing aura
                    for (int ring = 0; ring < 5; ring++)
                    {
                        float progress = ring / 5f;
                        Color ringColor = Color.Lerp(SunWhite, SunGold, progress);
                        float ringScale = 0.35f + ring * 0.12f;
                        int ringLife = 14 + ring * 3;
                        CustomParticles.HaloRing(player.Center, ringColor * (0.7f - progress * 0.25f), ringScale, ringLife);
                    }
                    
                    // RISING BLESSING DUST
                    for (int i = 0; i < 10; i++)
                    {
                        Vector2 dustVel = new Vector2(Main.rand.NextFloat(-2f, 2f), -Main.rand.NextFloat(3f, 6f));
                        Dust blessing = Dust.NewDustPerfect(player.Center + Main.rand.NextVector2Circular(30f, 15f), DustID.GoldFlame, dustVel, 0, SunGold, 1.3f);
                        blessing.noGravity = true;
                        blessing.fadeIn = 1.4f;
                    }
                    
                    // MUSIC NOTE ASCENSION - blessing song
                    for (int i = 0; i < 4; i++)
                    {
                        Vector2 noteVel = new Vector2(Main.rand.NextFloat(-1.5f, 1.5f), -Main.rand.NextFloat(2f, 4f));
                        ThemedParticles.MusicNote(player.Center + Main.rand.NextVector2Circular(25f, 10f), noteVel, SunGold, 0.9f, 32);
                    }
                    
                    // SPARKLE CORONA
                    for (int i = 0; i < 6; i++)
                    {
                        Vector2 sparklePos = player.Center + Main.rand.NextVector2Circular(45f, 45f);
                        CustomParticles.PrismaticSparkle(sparklePos, SunWhite, 0.45f);
                    }
                }
            }
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D texture = TextureAssets.Item[Item.type].Value;
            Vector2 position = Item.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;

            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.05f) * 0.12f + 1f;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            spriteBatch.Draw(texture, position, null, SunGold * 0.35f, rotation, origin, scale * pulse * 1.35f, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, position, null, SunOrange * 0.28f, rotation, origin, scale * pulse * 1.18f, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, position, null, SunWhite * 0.22f, rotation, origin, scale * 1.05f, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Lighting.AddLight(Item.Center, SunGold.ToVector3() * 0.5f);

            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "SolarBarrage", "Fires rapid solar orbs that explode on impact") { OverrideColor = SunOrange });
            tooltips.Add(new TooltipLine(Mod, "SunbeamCharge", "Hold to charge a devastating sunbeam attack") { OverrideColor = SunGold });
            tooltips.Add(new TooltipLine(Mod, "SolsticeBlessing", "Every 10 kills grants +15% damage for 5 seconds") { OverrideColor = SunWhite });
            tooltips.Add(new TooltipLine(Mod, "RadiantAura", "Provides +2 life regeneration while held") { OverrideColor = Color.Lerp(SunGold, SunWhite, 0.5f) });
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The wisdom of endless summer days bound within pages of light'") { OverrideColor = Color.Lerp(SunGold, SunOrange, 0.5f) });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<SolsticeBar>(), 16)
                .AddIngredient(ModContent.ItemType<SummerResonantEnergy>(), 1)
                .AddIngredient(ItemID.SoulofMight, 8)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}
