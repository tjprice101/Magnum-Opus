using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Content.OdeToJoy.ResonanceEnergies;
using MagnumOpus.Content.OdeToJoy.HarmonicCores;
using MagnumOpus.Content.Fate.CraftingStations;
using MagnumOpus.Content.OdeToJoy.Weapons.ThornboundReckoning.Projectiles;
using MagnumOpus.Content.OdeToJoy.Weapons.ThornboundReckoning.Particles;
using MagnumOpus.Content.OdeToJoy.Weapons.ThornboundReckoning.Utilities;

namespace MagnumOpus.Content.OdeToJoy.Weapons.ThornboundReckoning
{
    /// <summary>
    /// Thornbound Reckoning — Massive thorned greatsword of Ode to Joy.
    /// Fires vine waves on every swing, bloom explosion every 4th swing.
    /// Applies Poisoned + Venom on direct hit.
    /// Post-endgame Ode to Joy tier melee weapon.
    /// </summary>
    public class ThornboundReckoning : ModItem
    {
        /// <summary>
        /// Combo counter tracking consecutive swings. Resets if player stops swinging.
        /// </summary>
        private static int comboCounter = 0;

        /// <summary>
        /// Tracks the last game tick the player swung, for combo reset logic.
        /// </summary>
        private static int lastSwingTick = 0;

        /// <summary>
        /// Max ticks of inactivity before combo resets (about 1.5 seconds).
        /// </summary>
        private const int ComboResetThreshold = 90;

        public override void SetDefaults()
        {
            Item.width = 70;
            Item.height = 70;
            Item.damage = 4200;
            Item.DamageType = DamageClass.Melee;
            Item.useTime = 18;
            Item.useAnimation = 18;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 8f;
            Item.value = Item.sellPrice(platinum: 3, gold: 50);
            Item.rare = ModContent.RarityType<global::MagnumOpus.Common.OdeToJoyRarity>();
            Item.UseSound = SoundID.Item71;
            Item.autoReuse = true;
            Item.scale = 1.5f;
            Item.crit = 18;
            Item.shoot = ModContent.ProjectileType<VineWaveProjectile>();
            Item.shootSpeed = 14f;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Reset combo if player stopped swinging for too long
            int currentTick = (int)Main.GameUpdateCount;
            if (currentTick - lastSwingTick > ComboResetThreshold)
                comboCounter = 0;
            lastSwingTick = currentTick;

            comboCounter++;

            // Fire VineWaveProjectile on every swing
            Vector2 mouseDir = (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX);
            Projectile.NewProjectile(source, player.Center + mouseDir * 30f, mouseDir * Item.shootSpeed,
                ModContent.ProjectileType<VineWaveProjectile>(), damage, knockback, player.whoAmI);

            // Every 4th swing: fire BloomExplosionProjectile at cursor
            if (comboCounter % 4 == 0)
            {
                SoundEngine.PlaySound(SoundID.Item105 with { Pitch = 0.2f, Volume = 0.8f }, player.Center);

                // Spawn bloom explosion at cursor position
                Projectile.NewProjectile(source, Main.MouseWorld, Vector2.Zero,
                    ModContent.ProjectileType<BloomExplosionProjectile>(),
                    damage * 2, knockback * 2, player.whoAmI);

                // Spawn celebratory VFX burst at player
                if (!Main.dedServ)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        var note = new ReckoningNoteParticle(
                            player.Center + Main.rand.NextVector2Circular(20f, 20f),
                            new Vector2(Main.rand.NextFloat(-1.5f, 1.5f), Main.rand.NextFloat(-3f, -1f)),
                            Main.rand.NextFloat(0.3f, 0.5f),
                            Main.rand.Next(35, 55));
                        ReckoningParticleHandler.SpawnParticle(note);
                    }

                    for (int i = 0; i < 6; i++)
                    {
                        var spark = new VineSparkParticle(
                            player.Center,
                            mouseDir.RotatedByRandom(0.8f) * Main.rand.NextFloat(3f, 7f),
                            ReckoningUtils.JubilantGold,
                            Main.rand.NextFloat(0.4f, 0.7f),
                            Main.rand.Next(15, 25));
                        ReckoningParticleHandler.SpawnParticle(spark);
                    }
                }
            }

            return false; // We manually spawned the projectile
        }

        public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Poisoned, 300);
            target.AddBuff(BuffID.Venom, 180);

            // On-hit VFX
            if (!Main.dedServ)
            {
                for (int i = 0; i < 3; i++)
                {
                    var spark = new VineSparkParticle(
                        target.Center + Main.rand.NextVector2Circular(10f, 10f),
                        Main.rand.NextVector2Circular(3f, 3f),
                        Color.Lerp(ReckoningUtils.ForestGreen, ReckoningUtils.JubilantGold, Main.rand.NextFloat()),
                        Main.rand.NextFloat(0.3f, 0.5f),
                        Main.rand.Next(12, 22));
                    ReckoningParticleHandler.SpawnParticle(spark);
                }

                var mist = new VerdantMistParticle(
                    target.Center,
                    new Vector2(0f, -0.5f),
                    Main.rand.NextFloat(0.5f, 0.8f),
                    Main.rand.Next(20, 35));
                ReckoningParticleHandler.SpawnParticle(mist);
            }
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

            // Golden outer glow
            spriteBatch.Draw(texture, position, null, ReckoningUtils.Additive(ReckoningUtils.JubilantGold, 0.35f * flicker),
                rotation, origin, scale * pulse * 1.3f, SpriteEffects.None, 0f);
            // Green inner glow
            spriteBatch.Draw(texture, position, null, ReckoningUtils.Additive(ReckoningUtils.ForestGreen, 0.3f * flicker),
                rotation, origin, scale * pulse * 1.15f, SpriteEffects.None, 0f);

            float shimmer = (float)Math.Sin(time * 3f) * 0.5f + 0.5f;
            spriteBatch.Draw(texture, position, null, ReckoningUtils.Additive(ReckoningUtils.WhiteBloom, 0.2f * shimmer),
                rotation, origin, scale * pulse * 1.05f, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Lighting.AddLight(Item.Center, 0.6f, 0.5f, 0.15f);
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

            Color glowColor = Color.Lerp(ReckoningUtils.ForestGreen, ReckoningUtils.JubilantGold,
                (float)Math.Sin(time * 0.8f) * 0.5f + 0.5f);
            spriteBatch.Draw(texture, position, frame, ReckoningUtils.Additive(glowColor, 0.3f * flicker),
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
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Every swing releases a rolling wave of thorny golden vines"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Every 4th swing creates a massive jubilant bloom explosion at the cursor"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Direct hits inflict Poisoned and Venom"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Where thorns take root, jubilant vine eruptions herald the triumph of spring'")
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
