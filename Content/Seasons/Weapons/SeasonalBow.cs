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
using MagnumOpus.Content.Summer.Materials;
using MagnumOpus.Content.Autumn.Materials;
using MagnumOpus.Content.Winter.Materials;
using MagnumOpus.Content.Seasons.Projectiles;
using MagnumOpus.Content.Spring.Weapons;
using MagnumOpus.Content.Summer.Weapons;
using MagnumOpus.Content.Autumn.Weapons;
using MagnumOpus.Content.Winter.Weapons;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using static MagnumOpus.Common.Systems.ThemedParticles;

namespace MagnumOpus.Content.Seasons.Weapons
{
    /// <summary>
    /// Seasonal Bow - The ultimate seasonal ranged weapon (Post-Moon Lord tier)
    /// A legendary bow that fires arrows imbued with seasonal power.
    /// - Seasonal Quiver: Each shot cycles through Spring ↁESummer ↁEAutumn ↁEWinter arrows
    /// - Spring Arrow: Splits into homing petal projectiles
    /// - Summer Arrow: Explodes into solar flares on impact
    /// - Autumn Arrow: Leaves a trail of decaying damage zones
    /// - Winter Arrow: Freezes and shatters enemies
    /// - Harmonized Volley: Right-click fires all four seasonal arrows at once
    /// </summary>
    public class SeasonalBow : ModItem
    {
        private static readonly Color SpringPink = new Color(255, 183, 197);
        private static readonly Color SpringGreen = new Color(144, 238, 144);
        private static readonly Color SummerGold = new Color(255, 215, 0);
        private static readonly Color SummerOrange = new Color(255, 140, 0);
        private static readonly Color AutumnOrange = new Color(255, 140, 50);
        private static readonly Color AutumnBrown = new Color(139, 90, 43);
        private static readonly Color WinterBlue = new Color(150, 220, 255);
        private static readonly Color WinterWhite = new Color(240, 250, 255);

        private int seasonIndex = 0;

        public override void SetDefaults()
        {
            Item.width = 42;
            Item.height = 70;
            Item.damage = 175;
            Item.DamageType = DamageClass.Ranged;
            Item.useTime = 16;
            Item.useAnimation = 16;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 4f;
            Item.value = Item.buyPrice(platinum: 1, gold: 40);
            Item.rare = ItemRarityID.Red;
            Item.UseSound = SoundID.Item5;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.shoot = ProjectileID.WoodenArrowFriendly;
            Item.shootSpeed = 20f;
            Item.useAmmo = AmmoID.Arrow;
        }

        private Color GetCurrentSeasonColor()
        {
            return seasonIndex switch
            {
                0 => SpringPink,
                1 => SummerGold,
                2 => AutumnOrange,
                _ => WinterBlue
            };
        }

        public override void HoldItem(Player player)
        {
            Color primaryColor = GetCurrentSeasonColor();

            if (Main.rand.NextBool(8))
            {
                Vector2 particlePos = player.Center + Main.rand.NextVector2Circular(35f, 35f);
                Vector2 particleVel = Main.rand.NextVector2Circular(1f, 1f);
                var particle = new GenericGlowParticle(particlePos, particleVel, primaryColor * 0.4f, 0.25f, 22, true);
                MagnumParticleHandler.SpawnParticle(particle);
                
                // ☁ESPARKLE accent
                var sparkle = new SparkleParticle(particlePos, particleVel * 0.5f, primaryColor * 0.5f, 0.2f, 16);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            // ☁EMUSICAL NOTATION - Ambient notes! - VISIBLE SCALE 0.68f+
            if (Main.rand.NextBool(15))
            {
                Vector2 notePos = player.Center + new Vector2(player.direction * 25f, -12f) + Main.rand.NextVector2Circular(8f, 8f);
                Vector2 noteVel = new Vector2(0, -1.2f);
                ThemedParticles.MusicNote(notePos, noteVel, primaryColor * 0.8f, 0.75f, 26);
            }

            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.05f) * 0.1f + 0.4f;
            Lighting.AddLight(player.Center, primaryColor.ToVector3() * pulse);
        }

        public override bool AltFunctionUse(Player player)
        {
            return true;
        }

        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                Item.useTime = 40;
                Item.useAnimation = 40;
            }
            else
            {
                Item.useTime = 16;
                Item.useAnimation = 16;
            }
            return base.CanUseItem(player);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Vector2 muzzlePos = position + velocity.SafeNormalize(Vector2.Zero) * 40f;

            if (player.altFunctionUse == 2)
            {
                // Harmonized Volley - fire all four seasons
                for (int i = 0; i < 4; i++)
                {
                    float spreadAngle = MathHelper.ToRadians(-15f + i * 10f);
                    Vector2 arrowVel = velocity.RotatedBy(spreadAngle);
                    Projectile.NewProjectile(source, muzzlePos, arrowVel, ModContent.ProjectileType<SeasonalArrow>(),
                        damage, knockback, player.whoAmI, i);
                }

                // Volley VFX - Layered bloom and music notes!
                CustomParticles.GenericFlare(muzzlePos, Color.White, 1.2f, 28);
                CustomParticles.GenericFlare(muzzlePos, Color.White * 0.8f, 0.9f, 24);
                CustomParticles.HaloRing(muzzlePos, SpringPink * 0.55f, 0.6f, 20);
                CustomParticles.HaloRing(muzzlePos, SummerGold * 0.5f, 0.5f, 18);
                CustomParticles.HaloRing(muzzlePos, AutumnOrange * 0.45f, 0.4f, 16);
                CustomParticles.HaloRing(muzzlePos, WinterBlue * 0.45f, 0.3f, 14);
                
                // ☁EMUSICAL NOTATION - Grand volley note burst! - VISIBLE SCALE 0.75f+
                for (int n = 0; n < 5; n++)
                {
                    float noteAngle = velocity.ToRotation() + MathHelper.ToRadians(-30f + n * 15f);
                    Vector2 noteVel = noteAngle.ToRotationVector2() * Main.rand.NextFloat(2f, 4f);
                    Color noteColor = (n % 4) switch { 0 => SpringPink, 1 => SummerGold, 2 => AutumnOrange, _ => WinterBlue };
                    ThemedParticles.MusicNote(muzzlePos, noteVel, noteColor, 0.75f, 32);
                }

                for (int i = 0; i < 12; i++)
                {
                    float angle = velocity.ToRotation() + MathHelper.ToRadians(Main.rand.NextFloat(-25f, 25f));
                    Vector2 burstVel = angle.ToRotationVector2() * Main.rand.NextFloat(5f, 10f);
                    Color burstColor = (i % 4) switch
                    {
                        0 => SpringPink,
                        1 => SummerGold,
                        2 => AutumnOrange,
                        _ => WinterBlue
                    };
                    var burst = new GenericGlowParticle(muzzlePos, burstVel, burstColor * 0.5f, 0.3f, 18, true);
                    MagnumParticleHandler.SpawnParticle(burst);
                    
                    // ☁ESPARKLE accents
                    if (i % 2 == 0)
                    {
                        var sparkle = new SparkleParticle(muzzlePos, burstVel * 0.7f, burstColor * 0.6f, 0.22f, 16);
                        MagnumParticleHandler.SpawnParticle(sparkle);
                    }
                }

                return false;
            }

            // Normal shot - single seasonal arrow
            Projectile.NewProjectile(source, muzzlePos, velocity, ModContent.ProjectileType<SeasonalArrow>(),
                damage, knockback, player.whoAmI, seasonIndex);

            // Muzzle VFX - Layered bloom with music note
            Color seasonColor = GetCurrentSeasonColor();
            CustomParticles.GenericFlare(muzzlePos, Color.White * 0.8f, 0.6f, 18);
            CustomParticles.GenericFlare(muzzlePos, seasonColor, 0.58f, 16);
            CustomParticles.HaloRing(muzzlePos, seasonColor * 0.5f, 0.3f, 14);
            
            // ☁EMUSICAL NOTATION - Single note on shot! - VISIBLE SCALE 0.7f+
            float singleNoteAngle = velocity.ToRotation() + MathHelper.ToRadians(Main.rand.NextFloat(-15f, 15f));
            Vector2 singleNoteVel = singleNoteAngle.ToRotationVector2() * Main.rand.NextFloat(1.5f, 3f);
            ThemedParticles.MusicNote(muzzlePos, singleNoteVel, seasonColor, 0.7f, 28);

            for (int i = 0; i < 5; i++)
            {
                Vector2 sparkVel = velocity.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(3f, 7f) + Main.rand.NextVector2Circular(2f, 2f);
                var spark = new GenericGlowParticle(muzzlePos, sparkVel, seasonColor * 0.5f, 0.25f, 14, true);
                MagnumParticleHandler.SpawnParticle(spark);
                
                // ☁ESPARKLE on some sparks
                if (i % 2 == 0)
                {
                    var sparkle = new SparkleParticle(muzzlePos, sparkVel * 0.6f, seasonColor * 0.55f, 0.18f, 12);
                    MagnumParticleHandler.SpawnParticle(sparkle);
                }
            }

            // Cycle season
            seasonIndex = (seasonIndex + 1) % 4;

            return false;
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D texture = TextureAssets.Item[Item.type].Value;
            Vector2 position = Item.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;

            Color primaryColor = GetCurrentSeasonColor();
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.05f) * 0.1f + 1f;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            spriteBatch.Draw(texture, position, null, SpringPink * 0.18f, rotation, origin, scale * pulse * 1.3f, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, position, null, SummerGold * 0.16f, rotation, origin, scale * 1.2f, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, position, null, AutumnOrange * 0.14f, rotation, origin, scale * 1.12f, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, position, null, WinterBlue * 0.12f, rotation, origin, scale * 1.05f, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Lighting.AddLight(Item.Center, primaryColor.ToVector3() * 0.5f);

            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "SeasonalQuiver", "Each shot cycles through the four seasons") { OverrideColor = Color.White });
            tooltips.Add(new TooltipLine(Mod, "Spring", "Spring: Arrows split into homing petal projectiles") { OverrideColor = SpringPink });
            tooltips.Add(new TooltipLine(Mod, "Summer", "Summer: Arrows explode into solar flares") { OverrideColor = SummerGold });
            tooltips.Add(new TooltipLine(Mod, "Autumn", "Autumn: Arrows leave decaying damage trails") { OverrideColor = AutumnOrange });
            tooltips.Add(new TooltipLine(Mod, "Winter", "Winter: Arrows freeze and shatter enemies") { OverrideColor = WinterBlue });
            tooltips.Add(new TooltipLine(Mod, "Volley", "Right-click to fire all four seasons at once") { OverrideColor = Color.Lerp(Color.White, Color.Gold, 0.5f) });
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Strung with the threads of time itself'") { OverrideColor = Color.Lerp(SpringPink, WinterBlue, 0.5f) });
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-4f, 0f);
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                // Combine the 4 lower-tier seasonal ranged weapons
                .AddIngredient(ModContent.ItemType<PetalStormBow>(), 1)
                .AddIngredient(ModContent.ItemType<SolarScorcher>(), 1)
                .AddIngredient(ModContent.ItemType<TwilightArbalest>(), 1)
                .AddIngredient(ModContent.ItemType<FrostbiteRepeater>(), 1)
                // Plus 10 of each Seasonal Resonant Energy
                .AddIngredient(ModContent.ItemType<SpringResonantEnergy>(), 10)
                .AddIngredient(ModContent.ItemType<SummerResonantEnergy>(), 10)
                .AddIngredient(ModContent.ItemType<AutumnResonantEnergy>(), 10)
                .AddIngredient(ModContent.ItemType<WinterResonantEnergy>(), 10)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}
