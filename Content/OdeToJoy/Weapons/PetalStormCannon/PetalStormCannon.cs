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
using MagnumOpus.Content.OdeToJoy.Weapons.PetalStormCannon.Projectiles;
using MagnumOpus.Content.OdeToJoy.Weapons.PetalStormCannon.Particles;
using MagnumOpus.Content.OdeToJoy.Weapons.PetalStormCannon.Utilities;

namespace MagnumOpus.Content.OdeToJoy.Weapons.PetalStormCannon
{
    /// <summary>
    /// Petal Storm Cannon — Massive botanical artillery of Ode to Joy.
    /// Fires explosive petal bombs that create lingering AoE petal storms.
    /// Converts any rocket ammo into golden petal bombs that arc, explode into
    /// 8 homing shrapnel petals and a devastating petal storm vortex zone.
    /// Post-endgame Ode to Joy tier ranged weapon.
    /// </summary>
    public class PetalStormCannon : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 62;
            Item.height = 32;
            Item.damage = 4800;
            Item.DamageType = DamageClass.Ranged;
            Item.useTime = 50;
            Item.useAnimation = 50;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 12f;
            Item.value = Item.sellPrice(platinum: 3, gold: 50);
            Item.rare = ModContent.RarityType<global::MagnumOpus.Common.OdeToJoyRarity>();
            Item.UseSound = SoundID.Item62;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.crit = 20;
            Item.shoot = ProjectileID.RocketI;
            Item.shootSpeed = 8f;
            Item.useAmmo = AmmoID.Rocket;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Muzzle position at cannon barrel tip
            Vector2 muzzle = position + velocity.SafeNormalize(Vector2.UnitX) * 40f;

            // Muzzle flash VFX
            if (!Main.dedServ)
            {
                var bloom = new ExplosionBloomParticle(
                    muzzle,
                    velocity.SafeNormalize(Vector2.UnitX) * 2f,
                    0.8f,
                    12);
                PetalStormParticleHandler.SpawnParticle(bloom);

                // Scatter cannon smoke from barrel
                for (int i = 0; i < 5; i++)
                {
                    var smoke = new CannonSmokeParticle(
                        muzzle + Main.rand.NextVector2Circular(8f, 8f),
                        velocity.SafeNormalize(Vector2.UnitX).RotatedByRandom(0.6f) * Main.rand.NextFloat(1.5f, 4f),
                        Main.rand.NextFloat(0.2f, 0.4f),
                        Main.rand.Next(15, 30));
                    PetalStormParticleHandler.SpawnParticle(smoke);
                }
            }

            // Convert any rocket ammo into PetalBombProjectile
            Projectile.NewProjectile(source, muzzle, velocity,
                ModContent.ProjectileType<PetalBombProjectile>(), damage, knockback, player.whoAmI);

            return false; // We manually spawned the projectile
        }

        // ── WORLD DROP RENDERING ──

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D texture = TextureAssets.Item[Item.type].Value;
            Vector2 position = Item.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;

            float time = Main.GameUpdateCount * 0.05f;
            float pulse = 1f + (float)Math.Sin(time * 1.8f) * 0.1f;
            float flicker = Main.rand.NextFloat(0.9f, 1f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // Golden amber glow
            spriteBatch.Draw(texture, position, null, PetalStormUtils.Additive(PetalStormUtils.AmberFlame, 0.35f * flicker),
                rotation, origin, scale * pulse * 1.3f, SpriteEffects.None, 0f);
            // Rose burst accent
            spriteBatch.Draw(texture, position, null, PetalStormUtils.Additive(PetalStormUtils.RoseBurst, 0.2f * flicker),
                rotation, origin, scale * pulse * 1.15f, SpriteEffects.None, 0f);

            float shimmer = (float)Math.Sin(time * 2.5f) * 0.5f + 0.5f;
            spriteBatch.Draw(texture, position, null, PetalStormUtils.Additive(PetalStormUtils.WhiteFlash, 0.2f * shimmer),
                rotation, origin, scale * pulse * 1.05f, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Lighting.AddLight(Item.Center, 0.55f, 0.42f, 0.1f);
            return true;
        }

        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            Texture2D texture = TextureAssets.Item[Item.type].Value;
            float time = Main.GameUpdateCount * 0.04f;
            float pulse = 1f + (float)Math.Sin(time * 2f) * 0.07f;
            float flicker = Main.rand.NextFloat(0.9f, 1f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);

            Color glowColor = Color.Lerp(PetalStormUtils.AmberFlame, PetalStormUtils.GoldenExplosion,
                (float)Math.Sin(time * 0.7f) * 0.5f + 0.5f);
            spriteBatch.Draw(texture, position, frame, PetalStormUtils.Additive(glowColor, 0.3f * flicker),
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
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Converts any rocket into explosive petal bombs that arc through the air"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Petal bombs detonate into 8 homing shrapnel petals and a lingering petal storm vortex"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "The petal storm lasts 5 seconds, damaging all enemies caught in the whirling bloom"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "All impacts inflict Poisoned and Venom"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Where the cannon roars, a garden erupts — every detonation a verse in the jubilant anthem of creation'")
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
