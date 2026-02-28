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
using MagnumOpus.Content.OdeToJoy.Weapons.FountainOfJoyousHarmony.Projectiles;
using MagnumOpus.Content.OdeToJoy.Weapons.FountainOfJoyousHarmony.Buffs;
using MagnumOpus.Content.OdeToJoy.Weapons.FountainOfJoyousHarmony.Particles;
using MagnumOpus.Content.OdeToJoy.Weapons.FountainOfJoyousHarmony.Utilities;

namespace MagnumOpus.Content.OdeToJoy.Weapons.FountainOfJoyousHarmony
{
    /// <summary>
    /// Fountain of Joyous Harmony — Ode to Joy summoner staff.
    /// Places a stationary fountain that heals the player
    /// and attacks with water/petal projectiles.
    /// Self-contained weapon following the SandboxLastPrism pattern.
    /// </summary>
    public class FountainOfJoyousHarmony : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 46;
            Item.height = 46;
            Item.damage = 2200;
            Item.DamageType = DamageClass.Summon;
            Item.mana = 25;
            Item.useTime = 35;
            Item.useAnimation = 35;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 2f;
            Item.crit = 4;
            Item.value = Item.sellPrice(platinum: 4);
            Item.rare = ModContent.RarityType<global::MagnumOpus.Common.OdeToJoyRarity>();
            Item.UseSound = SoundID.Item44;
            Item.autoReuse = false;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<JoyousFountainMinion>();
            Item.shootSpeed = 0.01f;
            Item.buffType = ModContent.BuffType<JoyousFountainBuff>();
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            position = Main.MouseWorld;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            player.AddBuff(Item.buffType, 2);

            Vector2 spawnPos = position; // already set to cursor by ModifyShootStats

            // Entrance VFX — water burst fountain activation
            if (!Main.dedServ)
            {
                for (int i = 0; i < 12; i++)
                {
                    float angle = MathHelper.TwoPi * i / 12f;
                    Vector2 burstVel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 5f);
                    FountainParticleHandler.SpawnParticle(new FountainSprayParticle(
                        spawnPos, burstVel, Main.rand.NextFloat(0.2f, 0.35f), 25));
                }

                // Expanding healing ring
                FountainParticleHandler.SpawnParticle(new HealingAuraParticle(
                    spawnPos, 0.2f, 25));

                // Music notes rising from summon point
                for (int i = 0; i < 5; i++)
                {
                    Vector2 noteVel = new Vector2(Main.rand.NextFloat(-1f, 1f), -2f) * Main.rand.NextFloat(0.8f, 1.5f);
                    FountainParticleHandler.SpawnParticle(new FountainNoteParticle(
                        spawnPos + Main.rand.NextVector2Circular(12f, 12f),
                        noteVel, Main.rand.NextFloat(0.3f, 0.5f), 40));
                }

                // Petal scatter
                for (int i = 0; i < 8; i++)
                {
                    Vector2 petalVel = Main.rand.NextVector2CircularEdge(3f, 3f);
                    FountainParticleHandler.SpawnParticle(new PetalSplashParticle(
                        spawnPos, petalVel, Main.rand.NextFloat(0.15f, 0.3f), 30));
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
                FountainParticleHandler.SpawnParticle(new FountainSprayParticle(
                    particlePos, Main.rand.NextVector2Circular(0.3f, 0.3f),
                    Main.rand.NextFloat(0.08f, 0.15f), 20));
            }

            Lighting.AddLight(player.Center, FountainUtils.GoldenSpray.ToVector3() * 0.2f);
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Summon", "Places a stationary fountain at the cursor position"));
            tooltips.Add(new TooltipLine(Mod, "Heal", "Heals 3 HP every second when within range"));
            tooltips.Add(new TooltipLine(Mod, "Attack", "Fires homing water bolts that burst into rose petals"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Where waters rise and petals fall, joy sings its endless song for all'")
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
            FountainUtils.BeginAdditive(spriteBatch);

            Color glowColor = FountainUtils.Additive(FountainUtils.GoldenSpray, 0.35f);
            spriteBatch.Draw(texture, drawPos, null, glowColor, rotation, origin, scale * pulse * 1.15f, SpriteEffects.None, 0f);

            spriteBatch.End();
            FountainUtils.BeginDefault(spriteBatch);
        }

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
