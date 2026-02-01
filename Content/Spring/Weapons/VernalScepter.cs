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

            // Ethereal spring aura
            if (Main.rand.NextBool(6))
            {
                float angle = Main.GameUpdateCount * 0.025f;
                for (int i = 0; i < 2; i++)
                {
                    float orbAngle = angle + MathHelper.Pi * i;
                    float radius = 32f + (float)Math.Sin(Main.GameUpdateCount * 0.04f + i) * 8f;
                    Vector2 orbPos = player.Center + orbAngle.ToRotationVector2() * radius;
                    Color orbColor = Color.Lerp(SpringPink, SpringLavender, Main.rand.NextFloat());
                    CustomParticles.GenericFlare(orbPos, orbColor * 0.6f, 0.28f, 15);
                }
            }

            // Floating motes
            if (Main.rand.NextBool(10))
            {
                Vector2 motePos = player.Center + Main.rand.NextVector2Circular(40f, 40f);
                Vector2 moteVel = new Vector2(0, -Main.rand.NextFloat(0.3f, 0.8f));
                var mote = new GenericGlowParticle(motePos, moteVel, SpringGreen * 0.5f, 0.22f, 35, true);
                MagnumParticleHandler.SpawnParticle(mote);
            }
            
            // Floating spring melody notes
            if (Main.rand.NextBool(12))
            {
                Vector2 notePos = player.Center + Main.rand.NextVector2Circular(38f, 38f);
                Vector2 noteVel = new Vector2(0, -Main.rand.NextFloat(0.3f, 0.7f));
                Color noteColor = Color.Lerp(SpringLavender, SpringGreen, Main.rand.NextFloat()) * 0.65f;
                ThemedParticles.MusicNote(notePos, noteVel, noteColor, 0.75f, 42);
            }

            // Soft lighting
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.05f) * 0.1f + 0.5f;
            Lighting.AddLight(player.Center, SpringLavender.ToVector3() * pulse);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            castCounter++;

            // Cast VFX
            CustomParticles.GenericFlare(position, SpringLavender, 0.6f, 18);
            
            // Music note on cast
            ThemedParticles.MusicNote(position, velocity * 0.12f, SpringLavender * 0.8f, 0.7f, 28);
            
            // Magic sparkles on cast
            for (int i = 0; i < 5; i++)
            {
                Vector2 sparklePos = position + Main.rand.NextVector2Circular(15f, 15f);
                Vector2 sparkleVel = velocity * 0.1f + Main.rand.NextVector2Circular(2f, 2f);
                Color sparkleColor = Color.Lerp(SpringPink, SpringLavender, Main.rand.NextFloat());
                var sparkle = new GenericGlowParticle(sparklePos, sparkleVel, sparkleColor * 0.7f, 0.3f, 20, true);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            // Nature's Blessing: Every 6th cast fires homing flower volley
            if (castCounter >= 6)
            {
                castCounter = 0;
                
                // Big burst VFX - layered flares instead of halo
                CustomParticles.GenericFlare(position, Color.White, 0.8f, 20);
                CustomParticles.GenericFlare(position, SpringGreen, 0.6f, 18);
                CustomParticles.GenericFlare(position, SpringGreen * 0.6f, 0.45f, 15);
                
                // Music note ring and burst for Nature's Blessing
                ThemedParticles.MusicNoteRing(position, SpringGreen, 35f, 6);
                ThemedParticles.MusicNoteBurst(position, SpringLavender, 5, 4f);
                
                // Sparkle accents
                for (int i = 0; i < 4; i++)
                {
                    var sparkle = new SparkleParticle(position + Main.rand.NextVector2Circular(12f, 12f),
                        velocity * 0.05f + Main.rand.NextVector2Circular(2f, 2f), SpringWhite * 0.6f, 0.2f, 16);
                    MagnumParticleHandler.SpawnParticle(sparkle);
                }
                
                // Nature sparkle burst
                for (int s = 0; s < 8; s++)
                {
                    float sparkAngle = MathHelper.TwoPi * s / 8f;
                    Vector2 sparkPos = position + sparkAngle.ToRotationVector2() * 22f;
                    Color sparkColor = Color.Lerp(SpringPink, SpringLavender, (float)s / 8f);
                    CustomParticles.GenericFlare(sparkPos, sparkColor * 0.7f, 0.22f, 13);
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
