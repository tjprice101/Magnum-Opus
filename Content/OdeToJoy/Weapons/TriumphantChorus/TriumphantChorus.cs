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
using MagnumOpus.Content.OdeToJoy.ResonanceEnergies;
using MagnumOpus.Content.OdeToJoy.HarmonicCores;
using MagnumOpus.Content.Fate.CraftingStations;
using MagnumOpus.Content.OdeToJoy.Weapons.TriumphantChorus.Projectiles;
using MagnumOpus.Content.OdeToJoy.Weapons.TriumphantChorus.Buffs;
using MagnumOpus.Content.OdeToJoy.Weapons.TriumphantChorus.Particles;
using MagnumOpus.Content.OdeToJoy.Weapons.TriumphantChorus.Utilities;

namespace MagnumOpus.Content.OdeToJoy.Weapons.TriumphantChorus
{
    /// <summary>
    /// Triumphant Chorus — Ode to Joy 2-slot summoner staff.
    /// Summons a radiant chorus entity that fires harmonic blasts
    /// and performs a grand finale burst every 5 seconds.
    /// Self-contained weapon following the SandboxLastPrism pattern.
    /// </summary>
    public class TriumphantChorus : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 46;
            Item.height = 46;
            Item.damage = 3400;
            Item.DamageType = DamageClass.Summon;
            Item.mana = 35;
            Item.useTime = 35;
            Item.useAnimation = 35;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 5f;
            Item.crit = 4;
            Item.value = Item.sellPrice(platinum: 5);
            Item.rare = ModContent.RarityType<global::MagnumOpus.Common.OdeToJoyRarity>();
            Item.UseSound = SoundID.Item44;
            Item.autoReuse = false;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<TriumphantChorusMinion>();
            Item.shootSpeed = 10f;
            Item.buffType = ModContent.BuffType<TriumphantChorusBuff>();
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            position = Main.MouseWorld;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            player.AddBuff(Item.buffType, 2);

            Vector2 spawnPos = position;

            // Entrance VFX — triumphant golden burst
            if (!Main.dedServ)
            {
                // Expanding golden bloom
                ChorusParticleHandler.SpawnParticle(new GrandFinaleBloomParticle(
                    spawnPos, 0.3f, 20));

                // Radial spark burst
                for (int i = 0; i < 14; i++)
                {
                    float angle = MathHelper.TwoPi * i / 14f;
                    Vector2 burstVel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f);
                    ChorusParticleHandler.SpawnParticle(new ChorusSparkParticle(
                        spawnPos, burstVel, Main.rand.NextFloat(0.2f, 0.4f), 20));
                }

                // Music notes rising from summon point
                for (int i = 0; i < 8; i++)
                {
                    Vector2 noteVel = new Vector2(Main.rand.NextFloat(-1.5f, 1.5f), -2.5f) * Main.rand.NextFloat(0.8f, 1.5f);
                    ChorusParticleHandler.SpawnParticle(new FinaleNoteParticle(
                        spawnPos + Main.rand.NextVector2Circular(15f, 15f),
                        noteVel, Main.rand.NextFloat(0.3f, 0.5f), 45));
                }

                // Golden glow motes
                for (int i = 0; i < 10; i++)
                {
                    Vector2 glowVel = Main.rand.NextVector2CircularEdge(3f, 3f);
                    ChorusParticleHandler.SpawnParticle(new ChorusGlowParticle(
                        spawnPos, glowVel, Main.rand.NextFloat(0.15f, 0.3f), 25));
                }
            }

            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.3f, Volume = 0.8f }, spawnPos);

            Projectile.NewProjectile(source, spawnPos, Vector2.Zero, type, damage, knockback, player.whoAmI);

            return false;
        }

        public override void HoldItem(Player player)
        {
            // Soft ambient glow particles while holding
            if (Main.rand.NextBool(20))
            {
                Vector2 particlePos = player.Center + Main.rand.NextVector2Circular(35f, 35f);
                ChorusParticleHandler.SpawnParticle(new ChorusGlowParticle(
                    particlePos, Main.rand.NextVector2Circular(0.3f, 0.3f),
                    Main.rand.NextFloat(0.06f, 0.12f), 18));
            }

            Lighting.AddLight(player.Center, ChorusUtils.TriumphGold.ToVector3() * 0.2f);
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Summon", "Summons a triumphant chorus entity that orbits you"));
            tooltips.Add(new TooltipLine(Mod, "Slots", "Requires 2 minion slots"));
            tooltips.Add(new TooltipLine(Mod, "Attack", "Fires homing harmonic blasts at nearby enemies"));
            tooltips.Add(new TooltipLine(Mod, "Finale", "Every 5 seconds, performs a grand finale — an 8-way radial burst at double damage"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'When every voice rings true, the world itself sings back in jubilation'")
            {
                OverrideColor = new Color(255, 200, 50)
            });
        }

        public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            Texture2D texture = TextureAssets.Item[Item.type].Value;
            Vector2 drawPos = Item.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;

            float pulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.08f) * 0.08f;

            // Additive golden glow behind item in world
            spriteBatch.End();
            ChorusUtils.BeginAdditive(spriteBatch);

            Color glowColor = ChorusUtils.Additive(ChorusUtils.TriumphGold, 0.35f);
            spriteBatch.Draw(texture, drawPos, null, glowColor, rotation, origin, scale * pulse * 1.15f, SpriteEffects.None, 0f);

            spriteBatch.End();
            ChorusUtils.BeginDefault(spriteBatch);
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ResonantCoreOfOdeToJoy>(), 30)
                .AddIngredient(ModContent.ItemType<OdeToJoyResonantEnergy>(), 25)
                .AddIngredient(ModContent.ItemType<HarmonicCoreOfOdeToJoy>(), 4)
                .AddIngredient(ItemID.LunarBar, 25)
                .AddTile(ModContent.TileType<FatesCosmicAnvilTile>())
                .Register();
        }
    }
}
