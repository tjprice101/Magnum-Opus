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
using MagnumOpus.Content.OdeToJoy.Weapons.HymnOfTheVictorious.Projectiles;
using MagnumOpus.Content.OdeToJoy.Weapons.HymnOfTheVictorious.Particles;
using MagnumOpus.Content.OdeToJoy.Weapons.HymnOfTheVictorious.Utilities;

namespace MagnumOpus.Content.OdeToJoy.Weapons.HymnOfTheVictorious
{
    /// <summary>
    /// Hymn of the Victorious — Grand scepter of Ode to Joy.
    /// Summons 8 orbiting note projectiles that gather energy then launch
    /// toward the cursor simultaneously. Every 5th cast triggers a massive
    /// Symphonic Explosion at the cursor that heals the player and poisons enemies.
    /// Post-endgame Ode to Joy tier magic weapon.
    /// </summary>
    public class HymnOfTheVictorious : ModItem
    {
        /// <summary>
        /// Shot counter for tracking symphonic explosion cadence.
        /// </summary>
        private int shotCounter = 0;

        public override void SetDefaults()
        {
            Item.width = 44;
            Item.height = 44;
            Item.damage = 3600;
            Item.DamageType = DamageClass.Magic;
            Item.useTime = 25;
            Item.useAnimation = 25;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 6f;
            Item.value = Item.sellPrice(platinum: 4);
            Item.rare = ModContent.RarityType<global::MagnumOpus.Common.OdeToJoyRarity>();
            Item.UseSound = SoundID.Item43;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.crit = 15;
            Item.mana = 25;
            Item.shoot = ModContent.ProjectileType<OrbitalNoteProjectile>();
            Item.shootSpeed = 0.01f;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            shotCounter++;

            Vector2 cursorWorld = Main.MouseWorld;
            float launchAngle = (cursorWorld - player.Center).ToRotation();

            // Spawn 8 OrbitalNoteProjectile in a ring around the player
            if (player.whoAmI == Main.myPlayer)
            {
                for (int i = 0; i < 8; i++)
                {
                    float orbitAngle = MathHelper.TwoPi / 8f * i;
                    Vector2 spawnPos = player.Center + orbitAngle.ToRotationVector2() * 80f;

                    int proj = Projectile.NewProjectile(source, spawnPos, Vector2.Zero,
                        ModContent.ProjectileType<OrbitalNoteProjectile>(), damage, knockback, player.whoAmI,
                        ai0: 0f, ai1: i, ai2: launchAngle);
                }
            }

            // Every 5th shot: symphonic explosion at cursor
            if (shotCounter % 5 == 0)
            {
                if (player.whoAmI == Main.myPlayer)
                {
                    Projectile.NewProjectile(source, cursorWorld, Vector2.Zero,
                        ModContent.ProjectileType<SymphonicExplosionProjectile>(),
                        damage, knockback, player.whoAmI);
                }

                // Cast VFX burst at player
                if (!Main.dedServ)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        float angle = MathHelper.TwoPi / 8f * i;
                        var glow = new OrbitalNoteGlowParticle(
                            player.Center + angle.ToRotationVector2() * 30f,
                            angle.ToRotationVector2() * 2f,
                            Main.rand.NextFloat(0.25f, 0.45f),
                            Main.rand.Next(15, 25));
                        HymnParticleHandler.SpawnParticle(glow);
                    }

                    var bloom = new HymnBloomParticle(player.Center, 1.2f, 12);
                    HymnParticleHandler.SpawnParticle(bloom);
                }
            }

            return false; // We manually spawned the projectiles
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

            // Warm amber glow
            spriteBatch.Draw(texture, position, null, HymnUtils.Additive(HymnUtils.WarmAmber, 0.35f * flicker),
                rotation, origin, scale * pulse * 1.3f, SpriteEffects.None, 0f);
            // Brilliant gold inner glow
            spriteBatch.Draw(texture, position, null, HymnUtils.Additive(HymnUtils.BrilliantGold, 0.25f * flicker),
                rotation, origin, scale * pulse * 1.15f, SpriteEffects.None, 0f);

            float shimmer = (float)Math.Sin(time * 3f) * 0.5f + 0.5f;
            spriteBatch.Draw(texture, position, null, HymnUtils.Additive(HymnUtils.DivineLight, 0.2f * shimmer),
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

            Color glowColor = Color.Lerp(HymnUtils.WarmAmber, HymnUtils.BrilliantGold,
                (float)Math.Sin(time * 0.8f) * 0.5f + 0.5f);
            spriteBatch.Draw(texture, position, frame, HymnUtils.Additive(glowColor, 0.3f * flicker),
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
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Summons 8 orbiting music notes that gather energy then launch toward the cursor"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Launched notes home toward enemies and inflict Poisoned"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Every 5th cast triggers a massive Symphonic Explosion at the cursor"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Symphonic Explosions heal 30 HP, inflict Poisoned and Venom, and deal 1.5x damage"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Let every voice rise as one — for in triumph of spirit, the hymn of the victorious echoes through eternity'")
            {
                OverrideColor = new Color(255, 200, 50)
            });
        }

        // ── RECIPE ──

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ResonantCoreOfOdeToJoy>(), 25)
                .AddIngredient(ModContent.ItemType<OdeToJoyResonantEnergy>(), 20)
                .AddIngredient(ModContent.ItemType<HarmonicCoreOfOdeToJoy>(), 3)
                .AddIngredient(ItemID.LunarBar, 20)
                .AddTile(ModContent.TileType<FatesCosmicAnvilTile>())
                .Register();
        }
    }
}
