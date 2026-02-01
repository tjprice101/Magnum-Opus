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
            // Gentle petal aura
            if (Main.rand.NextBool(10))
            {
                Vector2 pos = player.Center + Main.rand.NextVector2Circular(30f, 30f);
                Vector2 vel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(-0.8f, -0.2f));
                Color petalColor = Color.Lerp(SpringPink, SpringWhite, Main.rand.NextFloat());
                
                var petal = new GenericGlowParticle(pos, vel, petalColor * 0.6f, 0.25f, 35, true);
                MagnumParticleHandler.SpawnParticle(petal);
            }
            
            // Floating spring melody notes
            if (Main.rand.NextBool(14))
            {
                Vector2 notePos = player.Center + Main.rand.NextVector2Circular(35f, 35f);
                Vector2 noteVel = new Vector2(0, -Main.rand.NextFloat(0.3f, 0.7f));
                Color noteColor = Color.Lerp(SpringPink, SpringGreen, Main.rand.NextFloat()) * 0.65f;
                ThemedParticles.MusicNote(notePos, noteVel, noteColor, 0.75f, 40);
            }

            // Soft lighting
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.05f) * 0.1f + 0.4f;
            Lighting.AddLight(player.Center, SpringPink.ToVector3() * pulse);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            shotCounter++;

            // Muzzle bloom VFX
            CustomParticles.GenericFlare(position, SpringPink, 0.5f, 15);
            
            // Music note on shot
            ThemedParticles.MusicNote(position, velocity * 0.1f, SpringPink * 0.8f, 0.75f, 25);
            
            // Petal particles on shot
            for (int i = 0; i < 4; i++)
            {
                Vector2 petalVel = velocity.RotatedByRandom(MathHelper.ToRadians(30)) * Main.rand.NextFloat(0.1f, 0.3f);
                var petal = new GenericGlowParticle(position, petalVel, SpringPink * 0.7f, 0.3f, 25, true);
                MagnumParticleHandler.SpawnParticle(petal);
            }

            // Spring Showers: Every 8th shot fires a fan of 5 petals
            if (shotCounter >= 8)
            {
                shotCounter = 0;
                
                // Big bloom VFX - layered flares instead of halo
                CustomParticles.GenericFlare(position, Color.White, 0.7f, 18);
                CustomParticles.GenericFlare(position, SpringPink, 0.55f, 15);
                CustomParticles.GenericFlare(position, SpringPink * 0.6f, 0.4f, 12);
                
                // Music note burst for Spring Showers
                ThemedParticles.MusicNoteBurst(position, SpringPink, 5, 4f);
                
                // Sparkle accents
                for (int i = 0; i < 4; i++)
                {
                    var sparkle = new SparkleParticle(position + Main.rand.NextVector2Circular(15f, 15f),
                        velocity * 0.05f + Main.rand.NextVector2Circular(2f, 2f), SpringWhite * 0.6f, 0.22f, 18);
                    MagnumParticleHandler.SpawnParticle(sparkle);
                }
                
                // Petal sparkle burst
                for (int s = 0; s < 6; s++)
                {
                    float sparkAngle = MathHelper.TwoPi * s / 6f;
                    Vector2 sparkPos = position + sparkAngle.ToRotationVector2() * 18f;
                    CustomParticles.GenericFlare(sparkPos, SpringWhite * 0.8f, 0.25f, 12);
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
