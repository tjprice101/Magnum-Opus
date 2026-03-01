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
using MagnumOpus.Content.OdeToJoy.Weapons.ThornSprayRepeater.Projectiles;
using MagnumOpus.Content.OdeToJoy.Weapons.ThornSprayRepeater.Particles;
using MagnumOpus.Content.OdeToJoy.Weapons.ThornSprayRepeater.Utilities;

namespace MagnumOpus.Content.OdeToJoy.Weapons.ThornSprayRepeater
{
    /// <summary>
    /// Thorn Spray Repeater — Rapid-fire botanical repeater of Ode to Joy.
    /// Fires sticky thorns that embed in enemies and chain-explode after 60 ticks.
    /// Converts any arrow ammo into verdant thorn bolts. Up to 8 thorns per NPC;
    /// each additional thorn adds +10% explosion damage for devastating chain detonations.
    /// Post-endgame Ode to Joy tier ranged weapon.
    /// </summary>
    public class ThornSprayRepeater : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 52;
            Item.height = 24;
            Item.damage = 2400;
            Item.DamageType = DamageClass.Ranged;
            Item.useTime = 6;
            Item.useAnimation = 6;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 2f;
            Item.value = Item.sellPrice(platinum: 3, gold: 50);
            Item.rare = ModContent.RarityType<global::MagnumOpus.Common.OdeToJoyRarity>();
            Item.UseSound = SoundID.Item5;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.crit = 12;
            Item.shoot = ProjectileID.WoodenArrowFriendly;
            Item.shootSpeed = 16f;
            Item.useAmmo = AmmoID.Arrow;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Muzzle position at barrel tip
            Vector2 muzzle = position + velocity.SafeNormalize(Vector2.UnitX) * 32f;

            // Slight spread for repeater feel
            float spread = Main.rand.NextFloat(-0.04f, 0.04f);
            Vector2 shotVel = velocity.RotatedBy(spread);

            // Muzzle flash VFX
            if (!Main.dedServ)
            {
                for (int i = 0; i < 3; i++)
                {
                    var spark = new ThornSparkParticle(
                        muzzle + Main.rand.NextVector2Circular(4f, 4f),
                        shotVel.SafeNormalize(Vector2.UnitX).RotatedByRandom(0.4f) * Main.rand.NextFloat(2f, 5f),
                        Main.rand.NextFloat(0.1f, 0.2f),
                        Main.rand.Next(6, 12));
                    ThornSprayParticleHandler.SpawnParticle(spark);
                }
            }

            // Convert any arrow into ThornBoltProjectile
            Projectile.NewProjectile(source, muzzle, shotVel,
                ModContent.ProjectileType<ThornBoltProjectile>(), damage, knockback, player.whoAmI);

            return false; // We manually spawned the projectile
        }

        // ── WORLD DROP RENDERING ──

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D texture = TextureAssets.Item[Item.type].Value;
            Vector2 position = Item.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;

            // Sharp angular flicker: stepped square-wave-like oscillation with thorny edge
            float time = Main.GameUpdateCount * 0.05f;
            float rawSine = (float)Math.Sin(time * 3.5f);
            float squarePulse = rawSine > 0 ? 1f : 0.6f; // Abrupt on/off stepped feel
            float thornEdge = (float)Math.Abs(Math.Sin(time * 5f)); // Sharp angular spikes
            float bangFlash = (float)Math.Pow(Math.Max(0f, (float)Math.Sin(time * 7f)), 8f); // Narrow bright spikes

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // Base verdant — abrupt intensity stepping
            spriteBatch.Draw(texture, position, null, ThornSprayUtils.Additive(ThornSprayUtils.VerdantBolt, 0.25f * squarePulse),
                rotation, origin, scale * (1.15f + thornEdge * 0.12f), SpriteEffects.None, 0f);

            // Amber warning — pulses on off-phase for contrast
            spriteBatch.Draw(texture, position, null, ThornSprayUtils.Additive(ThornSprayUtils.AmberWarn, 0.2f * (1f - squarePulse + 0.4f)),
                rotation, origin, scale * (1.05f + thornEdge * 0.06f), SpriteEffects.None, 0f);

            // Muzzle flash spike — sharp narrow white bursts
            spriteBatch.Draw(texture, position, null, ThornSprayUtils.Additive(ThornSprayUtils.FlashWhite, 0.45f * bangFlash),
                rotation, origin, scale * (1f + bangFlash * 0.15f), SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Lighting.AddLight(Item.Center, 0.2f + bangFlash * 0.3f, 0.4f * squarePulse, 0.08f);
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

            Color glowColor = Color.Lerp(ThornSprayUtils.ThornGreen, ThornSprayUtils.ExplosionGold,
                (float)Math.Sin(time * 0.7f) * 0.5f + 0.5f);
            spriteBatch.Draw(texture, position, frame, ThornSprayUtils.Additive(glowColor, 0.25f * flicker),
                0f, origin, scale * pulse * 1.1f, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);

            spriteBatch.Draw(texture, position, frame, drawColor, 0f, origin, scale, SpriteEffects.None, 0f);
            return false;
        }

        // ── TOOLTIPS ──

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Converts any arrow into rapid-fire thorn bolts that embed in enemies"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Embedded thorns detonate after 1 second in a chain explosion of splinters and poison"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Each additional thorn on the same enemy adds 10% explosion damage, up to 8 thorns"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Explosions scatter homing splinters and inflict Poisoned"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Let every barb be a verse of jubilation — a thousand thorns sing the hymn of triumphant bloom'")
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
