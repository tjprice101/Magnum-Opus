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
using MagnumOpus.Content.DiesIrae.ResonanceEnergies;
using MagnumOpus.Content.DiesIrae.HarmonicCores;
using MagnumOpus.Content.DiesIrae.Weapons.WrathsCleaver.Projectiles;
using MagnumOpus.Content.DiesIrae.Weapons.WrathsCleaver.Particles;
using MagnumOpus.Content.DiesIrae.Weapons.WrathsCleaver.Utilities;
using MagnumOpus.Content.Fate.CraftingStations;

namespace MagnumOpus.Content.DiesIrae.Weapons.WrathsCleaver
{
    /// <summary>
    /// Wrath's Cleaver — Massive hellfire cleaver. Post-Nachtmusik tier melee.
    /// 5-phase combo system with CurveSegment animation.
    /// Every 3rd swing spawns 5 homing crystallized flame projectiles.
    /// Wrath meter builds with hits — at max, triggers Infernal Eruption (AoE mark).
    /// Combo finisher (step 4) triggers an Infernal Lunge dash.
    /// Stats preserved from original implementation.
    /// </summary>
    public class WrathsCleaver : ModItem
    {
        private int swingCounter = 0;

        public override void SetDefaults()
        {
            Item.width = 70;
            Item.height = 70;
            Item.damage = 2800;
            Item.DamageType = DamageClass.Melee;
            Item.useTime = 16;
            Item.useAnimation = 16;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 9f;
            Item.value = Item.sellPrice(platinum: 2, gold: 50);
            Item.rare = ModContent.RarityType<DiesIraeRarity>();
            Item.UseSound = SoundID.Item71;
            Item.autoReuse = true;
            Item.useTurn = false;
            Item.scale = 1.6f;
            Item.crit = 20;
            Item.shoot = ModContent.ProjectileType<WrathsCleaverSwing>();
            Item.shootSpeed = 12f;
            Item.noMelee = true;
            Item.noUseGraphic = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "5-phase combo with escalating intensity"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Every 3rd swing spawns 5 homing crystallized flame projectiles"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Hits build Wrath — at maximum, triggers Infernal Eruption"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Eruption marks all nearby enemies, increasing damage taken by 25%"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Forged in the flames of final judgment'")
            {
                OverrideColor = new Color(200, 50, 30)
            });
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            var wrathPlayer = player.WrathsCleaver();
            int comboStep = wrathPlayer.AdvanceCombo();
            swingCounter++;

            // Create swing projectile with combo state
            Vector2 mouseDir = (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX);
            int proj = Projectile.NewProjectile(source, player.Center, mouseDir * Item.shootSpeed,
                ModContent.ProjectileType<WrathsCleaverSwing>(), damage, knockback, player.whoAmI,
                ai0: 0f, ai1: 0f, ai2: comboStep);

            // Every 3rd swing: crystallized flame burst
            if (swingCounter >= 3)
            {
                swingCounter = 0;
                SoundEngine.PlaySound(SoundID.DD2_BetsyFireballImpact with { Pitch = 0.3f }, player.Center);

                for (int i = 0; i < 5; i++)
                {
                    float angle = mouseDir.ToRotation() + MathHelper.ToRadians(-30f + i * 15f);
                    Vector2 crystalVel = angle.ToRotationVector2() * 8f;
                    Projectile.NewProjectile(source, player.Center + mouseDir * 30f, crystalVel,
                        ModContent.ProjectileType<WrathCrystallizedFlame>(), damage * 3 / 4, knockback / 2, player.whoAmI);
                }

                // Crystal burst VFX
                for (int i = 0; i < 8; i++)
                {
                    var note = new HellfireNote(player.Center + mouseDir * 40f,
                        Main.rand.NextVector2Circular(4f, 4f),
                        WrathsCleaverUtils.HellfireGold, 0.5f, 35);
                    WrathParticleHandler.SpawnParticle(note);
                }
            }

            return false;
        }

        public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire3, 300);
            if (hit.Crit)
                target.AddBuff(BuffID.Daybreak, 180);
        }

        // ── WORLD DROP RENDERING ──

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D texture = TextureAssets.Item[Item.type].Value;
            Vector2 position = Item.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;

            float time = Main.GameUpdateCount * 0.07f;
            float pulse = 1f + (float)Math.Sin(time * 2f) * 0.12f;
            float flicker = Main.rand.NextFloat(0.9f, 1f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            spriteBatch.Draw(texture, position, null, WrathsCleaverUtils.Additive(WrathsCleaverUtils.BloodRed, 0.4f * flicker),
                rotation, origin, scale * pulse * 1.35f, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, position, null, WrathsCleaverUtils.Additive(WrathsCleaverUtils.EmberOrange, 0.35f * flicker),
                rotation, origin, scale * pulse * 1.2f, SpriteEffects.None, 0f);

            float shimmer = (float)Math.Sin(time * 3f) * 0.5f + 0.5f;
            spriteBatch.Draw(texture, position, null, WrathsCleaverUtils.Additive(Color.Yellow, 0.25f * shimmer),
                rotation, origin, scale * pulse * 1.08f, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Lighting.AddLight(Item.Center, 0.8f, 0.4f, 0.1f);
            return true;
        }

        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            Texture2D texture = TextureAssets.Item[Item.type].Value;
            float time = Main.GameUpdateCount * 0.06f;
            float pulse = 1f + (float)Math.Sin(time * 2.2f) * 0.1f;
            float flicker = Main.rand.NextFloat(0.85f, 1f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);

            Color glowColor = Color.Lerp(WrathsCleaverUtils.BloodRed, WrathsCleaverUtils.EmberOrange,
                (float)Math.Sin(time * 0.8f) * 0.5f + 0.5f);
            spriteBatch.Draw(texture, position, frame, WrathsCleaverUtils.Additive(glowColor, 0.35f * flicker),
                0f, origin, scale * pulse * 1.15f, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);

            spriteBatch.Draw(texture, position, frame, drawColor, 0f, origin, scale, SpriteEffects.None, 0f);
            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ResonantCoreOfDiesIrae>(), 20)
                .AddIngredient(ModContent.ItemType<DiesIraeResonantEnergy>(), 15)
                .AddIngredient(ModContent.ItemType<HarmonicCoreOfDiesIrae>(), 2)
                .AddIngredient(ItemID.LunarBar, 15)
                .AddTile(ModContent.TileType<FatesCosmicAnvilTile>())
                .Register();
        }
    }
}
