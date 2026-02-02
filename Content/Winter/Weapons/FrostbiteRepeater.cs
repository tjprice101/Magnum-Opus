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
    /// Frostbite Repeater - Winter-themed ranged weapon (Post-Golem tier)
    /// A crystalline crossbow that fires volleys of ice bolts.
    /// - Icicle Volley: Fires 3 icicle bolts per shot (135 damage)
    /// - Crystalline Penetration: Icicles pierce through 3 enemies
    /// - Hypothermia: Stacking slow effect, at 5 stacks enemies freeze
    /// - Blizzard Barrage: Right-click to fire a spread of 7 homing ice shards
    /// </summary>
    public class FrostbiteRepeater : ModItem
    {
        private static readonly Color IceBlue = new Color(150, 220, 255);
        private static readonly Color FrostWhite = new Color(240, 250, 255);
        private static readonly Color DeepBlue = new Color(60, 100, 180);
        private static readonly Color CrystalCyan = new Color(100, 255, 255);

        public override void SetDefaults()
        {
            Item.width = 52;
            Item.height = 26;
            Item.damage = 135;
            Item.DamageType = DamageClass.Ranged;
            Item.useTime = 22;
            Item.useAnimation = 22;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 4f;
            Item.value = Item.buyPrice(gold: 40);
            Item.rare = ItemRarityID.Yellow;
            Item.UseSound = SoundID.Item75;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<IcicleBolt>();
            Item.shootSpeed = 18f;
            Item.useAmmo = AmmoID.Arrow;
        }

        public override void HoldItem(Player player)
        {
            // ========== IRIDESCENT WINGSPAN VFX PATTERN ==========
            // HEAVY DUST TRAILS - 2+ per frame with fadeIn (ice/frost dust)
            for (int d = 0; d < 2; d++)
            {
                Vector2 dustPos = player.Center + Main.rand.NextVector2Circular(26f, 26f);
                Dust dust = Dust.NewDustPerfect(dustPos, DustID.IceTorch, Main.rand.NextVector2Circular(0.4f, 0.4f) + new Vector2(0, -0.3f), 0, IceBlue, Main.rand.NextFloat(1.0f, 1.4f));
                dust.noGravity = true;
                dust.fadeIn = 1.4f;
            }
            
            // CONTRASTING SPARKLES - bright white/cyan crystalline
            if (Main.rand.NextBool(2))
            {
                Vector2 sparklePos = player.Center + Main.rand.NextVector2Circular(28f, 28f);
                Color sparkleColor = Main.rand.NextBool() ? FrostWhite : CrystalCyan;
                CustomParticles.PrismaticSparkle(sparklePos, sparkleColor, Main.rand.NextFloat(0.35f, 0.52f));
            }
            
            // SHIMMER TRAILS - drifting frost motes with color cycling
            if (Main.rand.NextBool(3))
            {
                float hue = 0.53f + Main.rand.NextFloat(0.06f); // Ice blue range
                Color shimmerColor = Main.hslToRgb(hue, 0.6f, 0.8f);
                Vector2 shimmerPos = player.Center + Main.rand.NextVector2Circular(30f, 30f);
                Vector2 shimmerVel = Main.rand.NextVector2Circular(0.5f, 0.5f);
                var shimmer = new GenericGlowParticle(shimmerPos, shimmerVel, shimmerColor * 0.55f, 0.26f, 22, true);
                MagnumParticleHandler.SpawnParticle(shimmer);
            }
            
            // MUSIC NOTES - visible scale with frost theme
            if (Main.rand.NextBool(5))
            {
                Vector2 notePos = player.Center + Main.rand.NextVector2Circular(34f, 34f);
                Vector2 noteVel = Main.rand.NextVector2Circular(0.6f, 0.6f) + new Vector2(0, -0.3f);
                Color noteColor = Color.Lerp(IceBlue, FrostWhite, Main.rand.NextFloat(0.4f));
                ThemedParticles.MusicNote(notePos, noteVel, noteColor, Main.rand.NextFloat(0.85f, 1.0f), 28);
            }
            
            // ORBITING ICE MOTES - crystalline aura
            if (Main.rand.NextBool(4))
            {
                float orbitAngle = Main.GameUpdateCount * 0.045f + Main.rand.NextFloat(MathHelper.TwoPi);
                float orbitRadius = 38f + Main.rand.NextFloat(14f);
                Vector2 orbitPos = player.Center + orbitAngle.ToRotationVector2() * orbitRadius;
                Color orbitColor = Color.Lerp(IceBlue, CrystalCyan, Main.rand.NextFloat()) * 0.5f;
                var mote = new GenericGlowParticle(orbitPos, Vector2.Zero, orbitColor, 0.2f, 14, true);
                MagnumParticleHandler.SpawnParticle(mote);
            }

            // Enhanced dynamic lighting
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.05f) * 0.12f + 0.55f;
            Lighting.AddLight(player.Center, IceBlue.ToVector3() * pulse);
        }

        public override bool AltFunctionUse(Player player)
        {
            return true;
        }

        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                // Blizzard Barrage - right click
                Item.useTime = 45;
                Item.useAnimation = 45;
                Item.UseSound = SoundID.Item30;
            }
            else
            {
                // Normal shot
                Item.useTime = 22;
                Item.useAnimation = 22;
                Item.UseSound = SoundID.Item75;
            }
            return base.CanUseItem(player);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Vector2 muzzlePos = position + velocity.SafeNormalize(Vector2.Zero) * 40f;

            if (player.altFunctionUse == 2)
            {
                // Blizzard Barrage - 7 homing shards in a spread
                for (int i = -3; i <= 3; i++)
                {
                    float spreadAngle = MathHelper.ToRadians(i * 8f);
                    Vector2 shardVel = velocity.RotatedBy(spreadAngle) * 0.85f;
                    Projectile.NewProjectile(source, muzzlePos, shardVel, ModContent.ProjectileType<BlizzardShardProjectile>(),
                        (int)(damage * 0.7f), knockback * 0.6f, player.whoAmI);
                }

                // ========== BLIZZARD BARRAGE SPECTACULAR VFX ==========
                // MULTI-LAYER FLARES - white core → ice → cyan
                CustomParticles.GenericFlare(muzzlePos, FrostWhite, 1.0f, 22);
                CustomParticles.GenericFlare(muzzlePos, IceBlue, 0.75f, 20);
                CustomParticles.GenericFlare(muzzlePos, CrystalCyan, 0.55f, 18);
                
                // GRADIENT HALO CASCADE - 6 layers IceBlue → DeepBlue
                for (int i = 0; i < 6; i++)
                {
                    float progress = i / 5f;
                    Color haloColor = Color.Lerp(IceBlue, DeepBlue, progress);
                    float haloScale = 0.32f + i * 0.1f;
                    int haloLife = 14 + i * 2;
                    CustomParticles.HaloRing(muzzlePos, haloColor * (0.7f - progress * 0.3f), haloScale, haloLife);
                }
                
                // RADIAL ICE DUST BURST
                for (int i = 0; i < 12; i++)
                {
                    float angle = MathHelper.TwoPi * i / 12f;
                    Vector2 burstVel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 8f);
                    Dust iceDust = Dust.NewDustPerfect(muzzlePos, DustID.IceTorch, burstVel, 0, IceBlue, Main.rand.NextFloat(1.1f, 1.5f));
                    iceDust.noGravity = true;
                    iceDust.fadeIn = 1.3f;
                }
                
                // DIRECTIONAL FROST STREAM
                for (int i = 0; i < 8; i++)
                {
                    float spreadAngle = velocity.ToRotation() + MathHelper.ToRadians(Main.rand.NextFloat(-30f, 30f));
                    Vector2 burstVel = spreadAngle.ToRotationVector2() * Main.rand.NextFloat(5f, 10f);
                    Color burstColor = Color.Lerp(IceBlue, CrystalCyan, Main.rand.NextFloat()) * 0.55f;
                    var burst = new GenericGlowParticle(muzzlePos, burstVel, burstColor, 0.28f, 18, true);
                    MagnumParticleHandler.SpawnParticle(burst);
                }
                
                // MUSIC NOTE STARBURST
                for (int i = 0; i < 4; i++)
                {
                    float noteAngle = MathHelper.TwoPi * i / 4f + Main.rand.NextFloat(0.3f);
                    Vector2 noteVel = noteAngle.ToRotationVector2() * Main.rand.NextFloat(1.5f, 3f);
                    Color noteColor = Color.Lerp(FrostWhite, IceBlue, Main.rand.NextFloat(0.4f));
                    ThemedParticles.MusicNote(muzzlePos, noteVel, noteColor, Main.rand.NextFloat(0.88f, 1.05f), 26);
                }
                
                // SPARKLE CORONA
                for (int i = 0; i < 6; i++)
                {
                    Vector2 sparklePos = muzzlePos + Main.rand.NextVector2Circular(24f, 24f);
                    Color sparkleColor = Main.rand.NextBool() ? FrostWhite : CrystalCyan;
                    CustomParticles.PrismaticSparkle(sparklePos, sparkleColor, Main.rand.NextFloat(0.4f, 0.6f));
                }

                return false;
            }

            // ========== NORMAL VOLLEY SPECTACULAR VFX ==========
            // Normal shot - 3 icicle volley
            for (int i = -1; i <= 1; i++)
            {
                float spreadAngle = MathHelper.ToRadians(i * 5f);
                Vector2 boltVel = velocity.RotatedBy(spreadAngle);
                Projectile.NewProjectile(source, muzzlePos, boltVel, ModContent.ProjectileType<IcicleBolt>(),
                    damage, knockback, player.whoAmI);
            }

            // MULTI-LAYER MUZZLE FLASH
            CustomParticles.GenericFlare(muzzlePos, FrostWhite, 0.7f, 16);
            CustomParticles.GenericFlare(muzzlePos, IceBlue, 0.5f, 14);
            
            // GRADIENT HALO RINGS - 4 layer cascade
            for (int i = 0; i < 4; i++)
            {
                float progress = i / 3f;
                Color haloColor = Color.Lerp(IceBlue, DeepBlue, progress);
                CustomParticles.HaloRing(muzzlePos, haloColor * (0.6f - progress * 0.2f), 0.22f + i * 0.08f, 12 + i * 2);
            }
            
            // DIRECTIONAL FROST SPARKS
            for (int i = 0; i < 6; i++)
            {
                Vector2 sparkVel = velocity.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(4f, 9f) + Main.rand.NextVector2Circular(2.5f, 2.5f);
                Color sparkColor = Color.Lerp(IceBlue, FrostWhite, Main.rand.NextFloat()) * 0.55f;
                var spark = new GenericGlowParticle(muzzlePos, sparkVel, sparkColor, 0.25f, 16, true);
                MagnumParticleHandler.SpawnParticle(spark);
            }
            
            // MUSIC NOTE ACCENT
            if (Main.rand.NextBool(2))
            {
                Vector2 noteVel = velocity.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(1f, 2f);
                Color noteColor = Color.Lerp(IceBlue, FrostWhite, Main.rand.NextFloat(0.4f));
                ThemedParticles.MusicNote(muzzlePos, noteVel, noteColor, Main.rand.NextFloat(0.85f, 0.95f), 22);
            }
            
            // CONTRASTING SPARKLE
            CustomParticles.PrismaticSparkle(muzzlePos + Main.rand.NextVector2Circular(12f, 12f), CrystalCyan, Main.rand.NextFloat(0.35f, 0.5f));

            return false;
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D texture = TextureAssets.Item[Item.type].Value;
            Vector2 position = Item.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;

            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.05f) * 0.08f + 1f;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            spriteBatch.Draw(texture, position, null, IceBlue * 0.3f, rotation, origin, scale * pulse * 1.25f, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, position, null, CrystalCyan * 0.2f, rotation, origin, scale * 1.1f, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Lighting.AddLight(Item.Center, IceBlue.ToVector3() * 0.4f);

            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "IcicleVolley", "Fires a volley of 3 piercing icicle bolts") { OverrideColor = IceBlue });
            tooltips.Add(new TooltipLine(Mod, "Hypothermia", "Hits inflict stacking Hypothermia - at 5 stacks, enemies freeze") { OverrideColor = CrystalCyan });
            tooltips.Add(new TooltipLine(Mod, "BlizzardBarrage", "Right-click to fire 7 homing frost shards") { OverrideColor = FrostWhite });
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The winds of the north made manifest'") { OverrideColor = Color.Lerp(IceBlue, FrostWhite, 0.5f) });
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-6f, 0f);
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<PermafrostBar>(), 18)
                .AddIngredient(ModContent.ItemType<WinterResonantEnergy>(), 1)
                .AddIngredient(ItemID.SoulofMight, 8)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}
