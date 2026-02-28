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
using MagnumOpus.Content.OdeToJoy.Weapons.TheStandingOvation.Projectiles;
using MagnumOpus.Content.OdeToJoy.Weapons.TheStandingOvation.Buffs;
using MagnumOpus.Content.OdeToJoy.Weapons.TheStandingOvation.Particles;
using MagnumOpus.Content.OdeToJoy.Weapons.TheStandingOvation.Utilities;

namespace MagnumOpus.Content.OdeToJoy.Weapons.TheStandingOvation
{
    /// <summary>
    /// The Standing Ovation — Ode to Joy summoner staff.
    /// Summons spirit minions that fire joy waves.
    /// +20% damage per additional spirit minion.
    /// Self-contained weapon following the SandboxLastPrism pattern.
    /// </summary>
    public class TheStandingOvation : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 46;
            Item.height = 46;
            Item.damage = 2600;
            Item.DamageType = DamageClass.Summon;
            Item.mana = 20;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 3f;
            Item.crit = 4;
            Item.value = Item.sellPrice(platinum: 4);
            Item.rare = ModContent.RarityType<global::MagnumOpus.Common.OdeToJoyRarity>();
            Item.UseSound = SoundID.Item44;
            Item.autoReuse = false;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<StandingOvationMinion>();
            Item.shootSpeed = 10f;
            Item.buffType = ModContent.BuffType<StandingOvationBuff>();
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            position = Main.MouseWorld;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            player.AddBuff(Item.buffType, 2);

            Vector2 spawnPos = position; // already set to cursor by ModifyShootStats

            // Entrance VFX — golden celebration burst
            for (int i = 0; i < 10; i++)
            {
                float angle = MathHelper.TwoPi * i / 10f;
                Vector2 burstVel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f);
                Color burstColor = Color.Lerp(OvationUtils.SpotlightGold, OvationUtils.RoseApplause, Main.rand.NextFloat());
                OvationParticleHandler.SpawnParticle(new ApplauseSparkParticle(
                    spawnPos, burstVel, burstColor, Main.rand.NextFloat(0.25f, 0.4f), 22, false));
            }

            // Music notes rising from summon point
            for (int i = 0; i < 5; i++)
            {
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-1f, 1f), -2f) * Main.rand.NextFloat(0.8f, 1.5f);
                OvationParticleHandler.SpawnParticle(new OvationNoteParticle(
                    spawnPos + Main.rand.NextVector2Circular(12f, 12f),
                    noteVel, Main.rand.NextFloat(0.3f, 0.5f), 40));
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
                OvationParticleHandler.SpawnParticle(new OvationGlowParticle(
                    particlePos, Main.rand.NextVector2Circular(0.3f, 0.3f),
                    OvationUtils.SpotlightGold, Main.rand.NextFloat(0.1f, 0.2f), 25));
            }

            Lighting.AddLight(player.Center, OvationUtils.SpotlightGold.ToVector3() * 0.2f);
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Summon", "Summons an applauding spirit to fight for you"));
            tooltips.Add(new TooltipLine(Mod, "Behavior", "Spirits hover and release waves of joyful energy"));
            tooltips.Add(new TooltipLine(Mod, "Synergy", "Multiple spirits synchronize for +20% damage per spirit"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The crowd rises — a symphony of spirit, unbroken and glorious'")
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
            OvationUtils.BeginAdditive(spriteBatch);

            Color glowColor = OvationUtils.Additive(OvationUtils.SpotlightGold, 0.35f);
            spriteBatch.Draw(texture, drawPos, null, glowColor, rotation, origin, scale * pulse * 1.15f, SpriteEffects.None, 0f);

            spriteBatch.End();
            OvationUtils.BeginDefault(spriteBatch);
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
