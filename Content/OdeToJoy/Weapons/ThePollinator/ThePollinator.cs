using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Content.OdeToJoy.ResonanceEnergies;
using MagnumOpus.Content.OdeToJoy.HarmonicCores;
using MagnumOpus.Content.Fate.CraftingStations;
using MagnumOpus.Content.OdeToJoy.Weapons.ThePollinator.Projectiles;
using MagnumOpus.Content.OdeToJoy.Weapons.ThePollinator.Particles;
using MagnumOpus.Content.OdeToJoy.Weapons.ThePollinator.Utilities;

namespace MagnumOpus.Content.OdeToJoy.Weapons.ThePollinator
{
    /// <summary>
    /// The Pollinator — Botanical gun of Ode to Joy.
    /// Converts bullets into pollen seeds that burst into homing petals.
    /// Every 4th shot fires a larger pollen burst that explodes into 6 radial homing petals.
    /// Post-endgame Ode to Joy tier ranged weapon.
    /// </summary>
    public class ThePollinator : ModItem
    {
        /// <summary>
        /// Shot counter for tracking burst shot cadence. Resets per player on world load.
        /// </summary>
        private int shotCounter = 0;

        public override void SetDefaults()
        {
            Item.width = 52;
            Item.height = 28;
            Item.damage = 3200;
            Item.DamageType = DamageClass.Ranged;
            Item.useTime = 18;
            Item.useAnimation = 18;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 5f;
            Item.value = Item.sellPrice(platinum: 3, gold: 50);
            Item.rare = ModContent.RarityType<global::MagnumOpus.Common.OdeToJoyRarity>();
            Item.UseSound = SoundID.Item11;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.crit = 15;
            Item.shoot = ProjectileID.Bullet;
            Item.shootSpeed = 14f;
            Item.useAmmo = AmmoID.Bullet;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            shotCounter++;

            // Muzzle position at gun barrel
            Vector2 muzzle = position + velocity.SafeNormalize(Vector2.UnitX) * 30f;

            // Muzzle flash VFX
            if (!Main.dedServ)
            {
                var bloom = new MuzzleBloomParticle(
                    muzzle,
                    velocity.SafeNormalize(Vector2.UnitX) * 1.5f,
                    0.6f,
                    10);
                PollinatorParticleHandler.SpawnParticle(bloom);

                // Scatter pollen dust from barrel
                for (int i = 0; i < 3; i++)
                {
                    var dust = new PollenDustParticle(
                        muzzle + Main.rand.NextVector2Circular(6f, 6f),
                        velocity.SafeNormalize(Vector2.UnitX).RotatedByRandom(0.5f) * Main.rand.NextFloat(1f, 3f),
                        Main.rand.NextFloat(0.12f, 0.22f),
                        Main.rand.Next(10, 20));
                    PollinatorParticleHandler.SpawnParticle(dust);
                }
            }

            // Every 4th shot: fire PollenBurstProjectile instead
            if (shotCounter % 4 == 0)
            {
                Projectile.NewProjectile(source, muzzle, velocity,
                    ModContent.ProjectileType<PollenBurstProjectile>(), damage, knockback, player.whoAmI);
            }
            else
            {
                // Normal shot: fire PollenSeedProjectile
                Projectile.NewProjectile(source, muzzle, velocity,
                    ModContent.ProjectileType<PollenSeedProjectile>(), damage, knockback, player.whoAmI);
            }

            return false; // We manually spawned the projectile
        }

        // ── WORLD DROP RENDERING ──

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D texture = TextureAssets.Item[Item.type].Value;
            Vector2 position = Item.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;

            float time = Main.GameUpdateCount * 0.06f;
            float pulse = 1f + (float)Math.Sin(time * 2f) * 0.1f;
            float flicker = Main.rand.NextFloat(0.9f, 1f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // Golden pollen glow
            spriteBatch.Draw(texture, position, null, PollinatorUtils.Additive(PollinatorUtils.PollenGold, 0.35f * flicker),
                rotation, origin, scale * pulse * 1.3f, SpriteEffects.None, 0f);
            // Green leaf glow
            spriteBatch.Draw(texture, position, null, PollinatorUtils.Additive(PollinatorUtils.LeafGreen, 0.25f * flicker),
                rotation, origin, scale * pulse * 1.15f, SpriteEffects.None, 0f);

            float shimmer = (float)Math.Sin(time * 3f) * 0.5f + 0.5f;
            spriteBatch.Draw(texture, position, null, PollinatorUtils.Additive(PollinatorUtils.PureLight, 0.2f * shimmer),
                rotation, origin, scale * pulse * 1.05f, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Lighting.AddLight(Item.Center, 0.5f, 0.45f, 0.12f);
            return true;
        }

        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            Texture2D texture = TextureAssets.Item[Item.type].Value;
            float time = Main.GameUpdateCount * 0.05f;
            float pulse = 1f + (float)Math.Sin(time * 2.2f) * 0.08f;
            float flicker = Main.rand.NextFloat(0.9f, 1f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);

            Color glowColor = Color.Lerp(PollinatorUtils.LeafGreen, PollinatorUtils.PollenGold,
                (float)Math.Sin(time * 0.8f) * 0.5f + 0.5f);
            spriteBatch.Draw(texture, position, frame, PollinatorUtils.Additive(glowColor, 0.3f * flicker),
                0f, origin, scale * pulse * 1.12f, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);

            spriteBatch.Draw(texture, position, frame, drawColor, 0f, origin, scale, SpriteEffects.None, 0f);
            return false;
        }

        // ── TOOLTIPS ──

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Converts bullets into pollen seeds that home toward enemies"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Pollen seeds burst into homing rose petals on hit"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Every 4th shot fires a larger pollen burst that explodes into a radial shower of petals"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "All projectiles inflict Poisoned"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Where pollen drifts, the garden of joy blooms eternal — each seed a note in nature's jubilant hymn'")
            {
                OverrideColor = new Color(255, 200, 50)
            });
        }

        // ── RECIPE ──

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ResonantCoreOfOdeToJoy>(), 20)
                .AddIngredient(ModContent.ItemType<OdeToJoyResonantEnergy>(), 15)
                .AddIngredient(ModContent.ItemType<HarmonicCoreOfOdeToJoy>(), 2)
                .AddIngredient(ItemID.LunarBar, 15)
                .AddTile(ModContent.TileType<FatesCosmicAnvilTile>())
                .Register();
        }
    }
}
