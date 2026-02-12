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
    /// Vernal Scepter - Spring-themed magic weapon (Post-WoF tier)
    /// Fires splitting bolts of spring energy that bloom on impact.
    /// - Splitting Bloom: Main bolt splits into 4 smaller petals after 0.5 seconds
    /// - Passive Regeneration: Holding grants +2 life regen
    /// - Bloom Burst: Bolts explode into healing particles on crit
    /// - Nature's Blessing: Every 6th cast triggers a homing flower volley
    /// </summary>
    public class VernalScepter : ModItem
    {
        private int castCounter = 0;
        
        private static readonly Color SpringPink = new Color(255, 183, 197);
        private static readonly Color SpringWhite = new Color(255, 250, 250);
        private static readonly Color SpringGreen = new Color(144, 238, 144);
        private static readonly Color SpringLavender = new Color(200, 180, 220);

        public override void SetDefaults()
        {
            Item.width = 40;
            Item.height = 40;
            Item.damage = 58;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 12;
            Item.useTime = 28;
            Item.useAnimation = 28;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 4f;
            Item.value = Item.buyPrice(gold: 8);
            Item.rare = ItemRarityID.LightRed;
            Item.UseSound = SoundID.Item43;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<VernalBolt>();
            Item.shootSpeed = 9f;
            Item.noMelee = true;
            Item.staff[Type] = true;
        }

        public override void HoldItem(Player player)
        {
            // Passive Regeneration: +2 life regen while holding
            player.lifeRegen += 2;

            // ========== CALAMITY-STANDARD VFX PATTERN ==========
            // HEAVY DUST TRAILS - 2+ per frame with fadeIn (lavender/green magic)
            for (int d = 0; d < 2; d++)
            {
                Vector2 dustPos = player.Center + Main.rand.NextVector2Circular(26f, 26f);
                int dustType = Main.rand.NextBool() ? DustID.PurpleTorch : DustID.GreenTorch;
                Color dustColor = Main.rand.NextBool() ? SpringLavender : SpringGreen;
                Dust dust = Dust.NewDustPerfect(dustPos, dustType, new Vector2(0, -Main.rand.NextFloat(0.4f, 1.1f)), 0, dustColor, Main.rand.NextFloat(1.0f, 1.4f));
                dust.noGravity = true;
                dust.fadeIn = 1.4f;
            }
            
            // CONTRASTING SPARKLES - white/pink contrast
            if (Main.rand.NextBool(2))
            {
                Vector2 sparklePos = player.Center + Main.rand.NextVector2Circular(30f, 30f);
                Color sparkleColor = Main.rand.NextBool() ? SpringWhite : SpringPink;
                CustomParticles.PrismaticSparkle(sparklePos, sparkleColor, Main.rand.NextFloat(0.33f, 0.5f));
            }
            
            // SHIMMER TRAILS - floating vernal motes with color cycling
            if (Main.rand.NextBool(3))
            {
                float hue = 0.28f + Main.rand.NextFloat(0.08f); // Green-lavender range
                Color shimmerColor = Main.hslToRgb(hue, 0.6f, 0.75f);
                Vector2 shimmerPos = player.Center + Main.rand.NextVector2Circular(28f, 28f);
                Vector2 shimmerVel = new Vector2(Main.rand.NextFloat(-0.4f, 0.4f), -Main.rand.NextFloat(0.5f, 1.2f));
                var shimmer = new GenericGlowParticle(shimmerPos, shimmerVel, shimmerColor * 0.58f, 0.26f, 24, true);
                MagnumParticleHandler.SpawnParticle(shimmer);
            }
            
            // MUSIC NOTES - visible scale with vernal theme
            if (Main.rand.NextBool(5))
            {
                Vector2 notePos = player.Center + Main.rand.NextVector2Circular(34f, 34f);
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -Main.rand.NextFloat(0.4f, 1.0f));
                Color noteColor = Color.Lerp(SpringLavender, SpringPink, Main.rand.NextFloat());
                ThemedParticles.MusicNote(notePos, noteVel, noteColor, Main.rand.NextFloat(0.85f, 1.0f), 28);
            }
            
            // ORBITING MAGIC MOTES - vernal wisdom
            if (Main.rand.NextBool(4))
            {
                float orbitAngle = Main.GameUpdateCount * 0.045f + Main.rand.NextFloat(MathHelper.TwoPi);
                float orbitRadius = 40f + Main.rand.NextFloat(14f);
                Vector2 orbitPos = player.Center + orbitAngle.ToRotationVector2() * orbitRadius;
                Color orbitColor = Color.Lerp(SpringLavender, SpringGreen, Main.rand.NextFloat()) * 0.5f;
                var mote = new GenericGlowParticle(orbitPos, Vector2.Zero, orbitColor, 0.2f, 14, true);
                MagnumParticleHandler.SpawnParticle(mote);
            }

            // Enhanced dynamic lighting
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.05f) * 0.12f + 0.58f;
            Lighting.AddLight(player.Center, SpringLavender.ToVector3() * pulse);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            castCounter++;

            // ========== ENHANCED VERNAL CAST VFX ==========
            // MULTI-LAYER CAST FLASH
            CustomParticles.GenericFlare(position, Color.White * 0.7f, 0.45f, 10);
            CustomParticles.GenericFlare(position, SpringLavender, 0.38f, 12);
            
            // MAGIC SPARKLE BURST - directional
            for (int i = 0; i < 4; i++)
            {
                Vector2 sparklePos = position + Main.rand.NextVector2Circular(12f, 12f);
                Vector2 sparkleVel = velocity * 0.1f + Main.rand.NextVector2Circular(1.5f, 1.5f);
                Color sparkleColor = Color.Lerp(SpringPink, SpringLavender, Main.rand.NextFloat());
                var sparkle = new GenericGlowParticle(sparklePos, sparkleVel, sparkleColor * 0.6f, 0.24f, 17, true);
                MagnumParticleHandler.SpawnParticle(sparkle);
                
                // Heavy magic dust
                Dust dust = Dust.NewDustPerfect(sparklePos, DustID.PurpleTorch, sparkleVel * 1.5f, 0, SpringLavender, 1.1f);
                dust.noGravity = true;
                dust.fadeIn = 1.2f;
            }
            
            // OCCASIONAL SPARKLE ACCENT
            if (Main.rand.NextBool(2))
            {
                CustomParticles.PrismaticSparkle(position, SpringWhite, 0.36f);
            }

            // Nature's Blessing: Every 6th cast fires homing flower volley
            if (castCounter >= 6)
            {
                castCounter = 0;
                
                // ========== SPECTACULAR NATURE'S BLESSING VFX ==========
                // CENTRAL BLESSING FLARE
                CustomParticles.GenericFlare(position, Color.White, 0.85f, 18);
                CustomParticles.GenericFlare(position, SpringGreen, 0.7f, 20);
                
                // 5-LAYER GRADIENT HALO CASCADE - green to lavender blessing
                for (int ring = 0; ring < 5; ring++)
                {
                    float progress = ring / 5f;
                    Color ringColor = Color.Lerp(SpringGreen, SpringLavender, progress);
                    float ringScale = 0.35f + ring * 0.11f;
                    int ringLife = 14 + ring * 3;
                    CustomParticles.HaloRing(position, ringColor * (0.68f - progress * 0.25f), ringScale, ringLife);
                }
                
                // RADIAL MAGIC DUST BURST
                for (int i = 0; i < 10; i++)
                {
                    float angle = MathHelper.TwoPi * i / 10f;
                    Vector2 dustVel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 7f);
                    int dustType = Main.rand.NextBool() ? DustID.GreenTorch : DustID.PurpleTorch;
                    Dust magic = Dust.NewDustPerfect(position, dustType, dustVel, 0, SpringGreen, 1.3f);
                    magic.noGravity = true;
                    magic.fadeIn = 1.3f;
                }
                
                // MUSIC NOTE BLOOM - vernal chorus
                for (int i = 0; i < 4; i++)
                {
                    float angle = MathHelper.TwoPi * i / 4f + Main.rand.NextFloat(0.2f);
                    Vector2 noteVel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 4f);
                    Color noteColor = Color.Lerp(SpringLavender, SpringPink, Main.rand.NextFloat(0.4f));
                    ThemedParticles.MusicNote(position, noteVel, noteColor, 0.9f, 26);
                }
                
                // SPARKLE CORONA
                for (int i = 0; i < 6; i++)
                {
                    Vector2 sparklePos = position + Main.rand.NextVector2Circular(35f, 35f);
                    CustomParticles.PrismaticSparkle(sparklePos, SpringWhite, 0.42f);
                }
                
                // Fire 5 homing flower projectiles
                for (int i = -2; i <= 2; i++)
                {
                    Vector2 spreadVel = velocity.RotatedBy(MathHelper.ToRadians(i * 20));
                    Projectile.NewProjectile(source, position, spreadVel * 0.8f, ModContent.ProjectileType<HomingFlowerBolt>(), damage * 2 / 3, knockback * 0.5f, player.whoAmI);
                }
            }

            // Normal bolt
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            
            return false;
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D texture = TextureAssets.Item[Item.type].Value;
            Vector2 position = Item.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;

            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.045f) * 0.12f + 1f;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            spriteBatch.Draw(texture, position, null, SpringLavender * 0.4f, rotation, origin, scale * pulse * 1.35f, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, position, null, SpringPink * 0.3f, rotation, origin, scale * pulse * 1.18f, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, position, null, SpringGreen * 0.25f, rotation, origin, scale * 1.05f, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Lighting.AddLight(Item.Center, SpringLavender.ToVector3() * 0.45f);

            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "SplittingBloom", "Bolts split into 4 homing petals after a short flight") { OverrideColor = SpringPink });
            tooltips.Add(new TooltipLine(Mod, "PassiveRegen", "Holding grants +2 life regeneration") { OverrideColor = SpringGreen });
            tooltips.Add(new TooltipLine(Mod, "BloomBurst", "Critical hits spawn healing particles") { OverrideColor = SpringGreen });
            tooltips.Add(new TooltipLine(Mod, "NaturesBlessing", "Every 6th cast fires a volley of 5 homing flowers") { OverrideColor = SpringLavender });
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Channel the eternal spring through arcane bloom'") { OverrideColor = Color.Lerp(SpringPink, SpringLavender, 0.5f) });
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
