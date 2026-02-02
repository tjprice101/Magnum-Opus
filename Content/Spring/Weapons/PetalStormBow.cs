using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria.GameContent;
using MagnumOpus.Content.Spring.Materials;
using MagnumOpus.Content.Spring.Projectiles;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using static MagnumOpus.Common.Systems.ThemedParticles;

namespace MagnumOpus.Content.Spring.Weapons
{
    /// <summary>
    /// Petal Storm Bow - Spring-themed ranged bow (Post-WoF tier)
    /// Fires arrows that split into homing petal projectiles.
    /// - Petal Conversion: Any arrow becomes a bloom arrow that splits into 3 homing petals
    /// - Pollination: Petal hits have 15% chance to spawn a healing flower at the hit location
    /// - Spring Showers: Every 8th shot fires a spread of 5 petal arrows in a fan
    /// - Life Leech: Kills restore 3 HP
    /// </summary>
    public class PetalStormBow : ModItem
    {
        private int shotCounter = 0;
        
        private static readonly Color SpringPink = new Color(255, 183, 197);
        private static readonly Color SpringWhite = new Color(255, 250, 250);
        private static readonly Color SpringGreen = new Color(144, 238, 144);

        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 38;
            Item.damage = 48;
            Item.DamageType = DamageClass.Ranged;
            Item.useTime = 24;
            Item.useAnimation = 24;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 2.5f;
            Item.value = Item.buyPrice(gold: 8);
            Item.rare = ItemRarityID.LightRed;
            Item.UseSound = SoundID.Item5;
            Item.autoReuse = true;
            Item.shoot = ProjectileID.WoodenArrowFriendly;
            Item.shootSpeed = 11f;
            Item.useAmmo = AmmoID.Arrow;
            Item.noMelee = true;
        }

        public override void HoldItem(Player player)
        {
            // ========== IRIDESCENT WINGSPAN VFX PATTERN ==========
            // HEAVY DUST TRAILS - 2+ per frame with fadeIn (pink petal dust)
            for (int d = 0; d < 2; d++)
            {
                Vector2 dustPos = player.Center + Main.rand.NextVector2Circular(24f, 24f);
                Dust dust = Dust.NewDustPerfect(dustPos, DustID.PinkTorch, new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), -Main.rand.NextFloat(0.4f, 1.0f)), 0, SpringPink, Main.rand.NextFloat(1.0f, 1.4f));
                dust.noGravity = true;
                dust.fadeIn = 1.4f;
            }
            
            // CONTRASTING SPARKLES - bright white/green contrast
            if (Main.rand.NextBool(2))
            {
                Vector2 sparklePos = player.Center + Main.rand.NextVector2Circular(28f, 28f);
                Color sparkleColor = Main.rand.NextBool() ? SpringWhite : SpringGreen;
                CustomParticles.PrismaticSparkle(sparklePos, sparkleColor, Main.rand.NextFloat(0.32f, 0.48f));
            }
            
            // SHIMMER TRAILS - drifting petal motes with color cycling
            if (Main.rand.NextBool(3))
            {
                float hue = 0.92f + Main.rand.NextFloat(0.06f); // Pink range
                Color shimmerColor = Main.hslToRgb(hue, 0.7f, 0.8f);
                Vector2 shimmerPos = player.Center + Main.rand.NextVector2Circular(28f, 28f);
                Vector2 shimmerVel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -Main.rand.NextFloat(0.5f, 1.2f));
                var shimmer = new GenericGlowParticle(shimmerPos, shimmerVel, shimmerColor * 0.6f, 0.25f, 24, true);
                MagnumParticleHandler.SpawnParticle(shimmer);
            }
            
            // MUSIC NOTES - visible scale with spring theme
            if (Main.rand.NextBool(5))
            {
                Vector2 notePos = player.Center + Main.rand.NextVector2Circular(32f, 32f);
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-0.6f, 0.6f), -Main.rand.NextFloat(0.5f, 1.1f));
                Color noteColor = Color.Lerp(SpringPink, SpringWhite, Main.rand.NextFloat(0.4f));
                ThemedParticles.MusicNote(notePos, noteVel, noteColor, Main.rand.NextFloat(0.85f, 1.0f), 30);
            }
            
            // ORBITING PETAL MOTES - springtime bloom
            if (Main.rand.NextBool(4))
            {
                float orbitAngle = Main.GameUpdateCount * 0.05f + Main.rand.NextFloat(MathHelper.TwoPi);
                float orbitRadius = 38f + Main.rand.NextFloat(12f);
                Vector2 orbitPos = player.Center + orbitAngle.ToRotationVector2() * orbitRadius;
                Color orbitColor = Color.Lerp(SpringPink, SpringGreen, Main.rand.NextFloat()) * 0.5f;
                var mote = new GenericGlowParticle(orbitPos, Vector2.Zero, orbitColor, 0.2f, 14, true);
                MagnumParticleHandler.SpawnParticle(mote);
            }

            // Enhanced dynamic lighting
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.05f) * 0.12f + 0.55f;
            Lighting.AddLight(player.Center, SpringPink.ToVector3() * pulse);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            shotCounter++;

            // ========== ENHANCED PETAL STORM MUZZLE VFX ==========
            // MULTI-LAYER FLARE - bloom burst
            CustomParticles.GenericFlare(position, Color.White * 0.7f, 0.4f, 10);
            CustomParticles.GenericFlare(position, SpringPink, 0.35f, 12);
            
            // DIRECTIONAL PETAL SPARKS - along firing direction
            for (int i = 0; i < 4; i++)
            {
                Vector2 sparkVel = velocity.SafeNormalize(Vector2.UnitX).RotatedByRandom(0.4f) * Main.rand.NextFloat(3f, 6f);
                Color sparkColor = Color.Lerp(SpringPink, SpringWhite, Main.rand.NextFloat());
                Dust spark = Dust.NewDustPerfect(position, DustID.PinkTorch, sparkVel, 0, sparkColor, 1.2f);
                spark.noGravity = true;
                spark.fadeIn = 1.2f;
            }
            
            // PETAL TRAIL PARTICLES - enhanced
            for (int i = 0; i < 3; i++)
            {
                Vector2 petalVel = velocity.RotatedByRandom(MathHelper.ToRadians(30)) * Main.rand.NextFloat(0.12f, 0.25f);
                Color petalColor = Color.Lerp(SpringPink, SpringGreen, Main.rand.NextFloat(0.3f));
                var petal = new GenericGlowParticle(position, petalVel, petalColor * 0.6f, 0.25f, 20, true);
                MagnumParticleHandler.SpawnParticle(petal);
            }
            
            // OCCASIONAL SPARKLE ACCENT
            if (Main.rand.NextBool(2))
            {
                CustomParticles.PrismaticSparkle(position, SpringWhite, 0.38f);
            }

            // Spring Showers: Every 8th shot fires a fan of 5 petals
            if (shotCounter >= 8)
            {
                shotCounter = 0;
                
                // ========== SPECTACULAR SPRING SHOWERS VFX ==========
                // CENTRAL BLOOM BURST
                CustomParticles.GenericFlare(position, Color.White, 0.8f, 16);
                CustomParticles.GenericFlare(position, SpringPink, 0.65f, 18);
                
                // 5-LAYER GRADIENT HALO CASCADE - pink to green spring gradient
                for (int ring = 0; ring < 5; ring++)
                {
                    float progress = ring / 5f;
                    Color ringColor = Color.Lerp(SpringPink, SpringGreen, progress);
                    float ringScale = 0.32f + ring * 0.1f;
                    int ringLife = 13 + ring * 3;
                    CustomParticles.HaloRing(position, ringColor * (0.65f - progress * 0.25f), ringScale, ringLife);
                }
                
                // RADIAL PETAL DUST BURST
                for (int i = 0; i < 10; i++)
                {
                    float angle = MathHelper.TwoPi * i / 10f;
                    Vector2 dustVel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 7f);
                    Dust petal = Dust.NewDustPerfect(position, DustID.PinkTorch, dustVel, 0, SpringPink, 1.3f);
                    petal.noGravity = true;
                    petal.fadeIn = 1.3f;
                }
                
                // MUSIC NOTE BLOOM - spring symphony
                for (int i = 0; i < 4; i++)
                {
                    float angle = MathHelper.TwoPi * i / 4f + Main.rand.NextFloat(0.2f);
                    Vector2 noteVel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 4f);
                    Color noteColor = Color.Lerp(SpringPink, SpringWhite, Main.rand.NextFloat(0.3f));
                    ThemedParticles.MusicNote(position, noteVel, noteColor, 0.9f, 26);
                }
                
                // SPARKLE RING
                for (int i = 0; i < 5; i++)
                {
                    float angle = MathHelper.TwoPi * i / 5f;
                    Vector2 sparklePos = position + angle.ToRotationVector2() * 25f;
                    CustomParticles.PrismaticSparkle(sparklePos, Color.Lerp(SpringWhite, SpringGreen, Main.rand.NextFloat(0.4f)), 0.42f);
                }
                
                // Fire 5 petal arrows in a fan
                for (int i = -2; i <= 2; i++)
                {
                    Vector2 spreadVel = velocity.RotatedBy(MathHelper.ToRadians(i * 12));
                    Projectile.NewProjectile(source, position, spreadVel, ModContent.ProjectileType<BloomArrow>(), damage, knockback, player.whoAmI);
                }
                
                return false;
            }
            
            // Normal shot - fire bloom arrow instead of regular arrow
            Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<BloomArrow>(), damage, knockback, player.whoAmI);
            
            return false;
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-2, 0);
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D texture = TextureAssets.Item[Item.type].Value;
            Vector2 position = Item.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;

            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.04f) * 0.1f + 1f;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            spriteBatch.Draw(texture, position, null, SpringPink * 0.35f, rotation, origin, scale * pulse * 1.3f, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, position, null, SpringWhite * 0.25f, rotation, origin, scale * pulse * 1.15f, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Lighting.AddLight(Item.Center, SpringPink.ToVector3() * 0.4f);

            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "PetalConversion", "Arrows transform into bloom arrows that split into 3 homing petals") { OverrideColor = SpringPink });
            tooltips.Add(new TooltipLine(Mod, "Pollination", "Petal hits have 15% chance to spawn a healing flower") { OverrideColor = SpringGreen });
            tooltips.Add(new TooltipLine(Mod, "SpringShowers", "Every 8th shot fires a fan of 5 petal arrows") { OverrideColor = SpringPink });
            tooltips.Add(new TooltipLine(Mod, "LifeLeech", "Kills restore 3 HP") { OverrideColor = SpringGreen });
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Nocked from the eternal garden, arrows bloom mid-flight'") { OverrideColor = Color.Lerp(SpringPink, SpringGreen, 0.5f) });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<VernalBar>(), 12)
                .AddIngredient(ModContent.ItemType<SpringResonantEnergy>(), 1)
                .AddIngredient(ItemID.SoulofLight, 8)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}
