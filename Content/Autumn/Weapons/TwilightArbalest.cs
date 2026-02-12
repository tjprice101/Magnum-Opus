using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria.GameContent;
using MagnumOpus.Content.Autumn.Materials;
using MagnumOpus.Content.Autumn.Projectiles;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using static MagnumOpus.Common.Systems.ThemedParticles;

namespace MagnumOpus.Content.Autumn.Weapons
{
    /// <summary>
    /// Twilight Arbalest - Autumn-themed ranged weapon (Post-Plantera tier)
    /// A heavy crossbow that fires bolts infused with autumn's fading light.
    /// - Twilight Bolt: Fires piercing bolts that leave decay trails (98 damage)
    /// - Fading Light: Bolts gain damage as they travel, up to +50%
    /// - Harvest Moon: Every 6th shot fires a large seeking moon bolt
    /// - Dusk's Embrace: Critical hits spawn homing leaf shards
    /// </summary>
    public class TwilightArbalest : ModItem
    {
        private static readonly Color TwilightPurple = new Color(120, 60, 140);
        private static readonly Color TwilightOrange = new Color(255, 120, 60);
        private static readonly Color AutumnGold = new Color(218, 165, 32);
        private static readonly Color MoonSilver = new Color(200, 200, 220);

        private int shotCount = 0;

        public override void SetDefaults()
        {
            Item.width = 56;
            Item.height = 28;
            Item.damage = 98;
            Item.DamageType = DamageClass.Ranged;
            Item.useTime = 28;
            Item.useAnimation = 28;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 5f;
            Item.value = Item.buyPrice(gold: 32);
            Item.rare = ItemRarityID.Lime;
            Item.UseSound = SoundID.Item102;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<TwilightBolt>();
            Item.shootSpeed = 16f;
            Item.useAmmo = AmmoID.Arrow;
        }

        public override void HoldItem(Player player)
        {
            // === CALAMITY-STANDARD HEAVY DUST TRAILS ===
            // Heavy twilight dust trail #1
            float trailProgress1 = Main.rand.NextFloat();
            Color purpleGradient = Color.Lerp(TwilightPurple, MoonSilver, trailProgress1);
            Dust heavyPurple = Dust.NewDustDirect(player.position, player.width, player.height, 
                DustID.PurpleTorch, player.velocity.X * 0.3f, player.velocity.Y * 0.3f, 100, purpleGradient, 1.5f);
            heavyPurple.noGravity = true;
            heavyPurple.fadeIn = 1.4f;
            heavyPurple.velocity = heavyPurple.velocity.RotatedByRandom(0.3f) * Main.rand.NextFloat(1.2f, 1.8f);
            
            // Heavy orange dust trail #2
            float trailProgress2 = Main.rand.NextFloat();
            Color orangeGradient = Color.Lerp(TwilightOrange, AutumnGold, trailProgress2);
            Dust heavyOrange = Dust.NewDustDirect(player.position, player.width, player.height, 
                DustID.OrangeTorch, player.velocity.X * 0.25f, player.velocity.Y * 0.25f, 80, orangeGradient, 1.4f);
            heavyOrange.noGravity = true;
            heavyOrange.fadeIn = 1.3f;
            heavyOrange.velocity = heavyOrange.velocity.RotatedByRandom(0.25f) * Main.rand.NextFloat(1.1f, 1.6f);
            
            // === CONTRASTING SPARKLES (every 1-in-2 frames) ===
            if (Main.rand.NextBool(2))
            {
                Vector2 sparklePos = player.Center + Main.rand.NextVector2Circular(25f, 25f);
                float sparkleHue = (Main.GameUpdateCount * 0.015f + Main.rand.NextFloat() * 0.3f) % 1f;
                Color sparkleColor = Color.Lerp(TwilightPurple, Main.hslToRgb(sparkleHue, 0.6f, 0.75f), 0.4f);
                CustomParticles.PrismaticSparkle(sparklePos, sparkleColor, 0.28f);
            }
            
            // === ORBITING TWILIGHT PARTICLES ===
            if (Main.rand.NextBool(6))
            {
                float orbitAngle = Main.GameUpdateCount * 0.04f;
                for (int i = 0; i < 3; i++)
                {
                    float angle = orbitAngle + MathHelper.TwoPi * i / 3f;
                    Vector2 orbitPos = player.Center + angle.ToRotationVector2() * (28f + Main.rand.NextFloat(8f));
                    Color orbitColor = i % 2 == 0 ? TwilightPurple : TwilightOrange;
                    CustomParticles.GenericFlare(orbitPos, orbitColor * 0.6f, 0.22f, 15);
                }
            }
            
            // === MUSIC NOTES - the twilight melody ===
            if (Main.rand.NextBool(15))
            {
                Vector2 notePos = player.Center + Main.rand.NextVector2Circular(20f, 20f);
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -1.2f);
                Color noteColor = Color.Lerp(TwilightPurple, TwilightOrange, Main.rand.NextFloat());
                ThemedParticles.MusicNote(notePos, noteVel, noteColor, 0.85f, 28);
            }

            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.035f) * 0.12f + 0.55f;
            Lighting.AddLight(player.Center, TwilightPurple.ToVector3() * pulse);
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            // Convert all arrows to twilight bolts
            type = ModContent.ProjectileType<TwilightBolt>();
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            shotCount++;
            Vector2 direction = velocity.SafeNormalize(Vector2.UnitX);

            // === CALAMITY-STANDARD MUZZLE FLASH ===
            // Multi-layer flare burst
            CustomParticles.GenericFlare(position, Color.White, 0.55f, 15);
            CustomParticles.GenericFlare(position, TwilightOrange, 0.45f, 18);
            CustomParticles.GenericFlare(position, TwilightPurple, 0.35f, 20);
            
            // === GRADIENT HALO RINGS ===
            for (int ring = 0; ring < 4; ring++)
            {
                float progress = (float)ring / 4f;
                Color ringColor = Color.Lerp(TwilightPurple, TwilightOrange, progress);
                CustomParticles.HaloRing(position, ringColor * 0.7f, 0.25f + ring * 0.1f, 12 + ring * 3);
            }
            
            // === HEAVY DUST BURST ===
            for (int i = 0; i < 8; i++)
            {
                float progress = (float)i / 8f;
                Vector2 burstVel = direction.RotatedBy(Main.rand.NextFloat(-0.4f, 0.4f)) * Main.rand.NextFloat(3f, 7f);
                Color burstColor = Color.Lerp(TwilightPurple, TwilightOrange, progress);
                Dust burst = Dust.NewDustPerfect(position, DustID.PurpleTorch, burstVel, 100, burstColor, 1.3f);
                burst.noGravity = true;
                burst.fadeIn = 1.2f;
            }
            
            // === CONTRASTING SPARKLE RING ===
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 sparklePos = position + angle.ToRotationVector2() * 15f;
                Color sparkleColor = i % 2 == 0 ? TwilightPurple : TwilightOrange;
                CustomParticles.PrismaticSparkle(sparklePos, sparkleColor, 0.3f);
            }
            
            // === MUSIC NOTES ON FIRE ===
            for (int i = 0; i < 3; i++)
            {
                Vector2 noteVel = direction.RotatedBy(Main.rand.NextFloat(-0.5f, 0.5f)) * Main.rand.NextFloat(1f, 3f);
                Color noteColor = Color.Lerp(TwilightPurple, AutumnGold, Main.rand.NextFloat());
                ThemedParticles.MusicNote(position, noteVel, noteColor, 0.85f, 25);
            }

            // Harvest Moon - every 6th shot
            if (shotCount >= 6)
            {
                shotCount = 0;

                // Spawn large moon bolt
                Projectile.NewProjectile(source, position, velocity * 0.9f, ModContent.ProjectileType<HarvestMoonBolt>(), (int)(damage * 1.75f), knockback * 1.5f, player.whoAmI);

                // === SPECTACULAR HARVEST MOON VFX ===
                CustomParticles.GenericFlare(position, Color.White, 0.8f, 20);
                CustomParticles.GenericFlare(position, MoonSilver, 0.65f, 22);
                CustomParticles.GenericFlare(position, AutumnGold, 0.5f, 25);
                
                // Moon halo cascade
                for (int ring = 0; ring < 6; ring++)
                {
                    float progress = (float)ring / 6f;
                    Color moonColor = Color.Lerp(MoonSilver, AutumnGold, progress);
                    CustomParticles.HaloRing(position, moonColor * 0.8f, 0.35f + ring * 0.12f, 15 + ring * 4);
                }
                
                // Starburst music notes
                for (int i = 0; i < 6; i++)
                {
                    float angle = MathHelper.TwoPi * i / 6f;
                    Vector2 noteVel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 4f);
                    ThemedParticles.MusicNote(position, noteVel, MoonSilver, 0.9f, 30);
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

            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.04f) * 0.08f + 1f;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            spriteBatch.Draw(texture, position, null, TwilightPurple * 0.25f, rotation, origin, scale * pulse * 1.2f, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, position, null, TwilightOrange * 0.2f, rotation, origin, scale * 1.1f, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Lighting.AddLight(Item.Center, TwilightPurple.ToVector3() * 0.35f);

            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "TwilightBolt", "Fires piercing twilight bolts that leave decay trails") { OverrideColor = TwilightPurple });
            tooltips.Add(new TooltipLine(Mod, "FadingLight", "Bolts gain up to 50% bonus damage as they travel") { OverrideColor = TwilightOrange });
            tooltips.Add(new TooltipLine(Mod, "HarvestMoon", "Every 6th shot fires a large seeking harvest moon bolt") { OverrideColor = MoonSilver });
            tooltips.Add(new TooltipLine(Mod, "DusksEmbrace", "Critical hits spawn homing leaf shards") { OverrideColor = AutumnGold });
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The last light before the long night'") { OverrideColor = Color.Lerp(TwilightPurple, TwilightOrange, 0.5f) });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<HarvestBar>(), 16)
                .AddIngredient(ModContent.ItemType<AutumnResonantEnergy>(), 1)
                .AddIngredient(ItemID.SoulofFright, 8)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}
