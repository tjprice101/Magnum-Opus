using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.DiesIrae.Weapons.StaffOfFinalJudgement.Particles;
using MagnumOpus.Content.DiesIrae.Weapons.StaffOfFinalJudgement.Utilities;

namespace MagnumOpus.Content.DiesIrae.Weapons.StaffOfFinalJudgement
{
    /// <summary>
    /// Staff of Final Judgement — Summons 5 floating ignition orbs that orbit the cursor,
    /// then converge on enemies for devastating delayed detonations.
    ///
    /// Stats: 1950 damage, 22 mana, 30 useTime, 5 KB, crit 18, Magic.
    /// Theme: Dies Irae — the final judgment upon sinners.
    /// </summary>
    public class StaffOfFinalJudgement : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 48;
            Item.height = 48;
            Item.damage = 1950;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 22;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 5f;
            Item.value = Item.sellPrice(platinum: 2);
            Item.rare = ModContent.RarityType<global::MagnumOpus.Common.DiesIraeRarity>();
            Item.UseSound = SoundID.Item117;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.staff[Item.type] = true;
            Item.shoot = ModContent.ProjectileType<Projectiles.FloatingIgnitionProjectile>();
            Item.shootSpeed = 4f;
            Item.crit = 18;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Spawn 5 floating ignitions in a burst
            for (int i = 0; i < 5; i++)
            {
                float angle = MathHelper.TwoPi * i / 5f;
                Vector2 offset = angle.ToRotationVector2() * 30f;
                Projectile.NewProjectile(source, position + offset, velocity.RotatedBy(angle * 0.1f), type,
                    damage, knockback, player.whoAmI, ai0: 0f, ai1: i);
            }

            // Staff casting VFX
            Vector2 staffTip = position + velocity.SafeNormalize(Vector2.UnitX) * 50f;
            JudgementParticleHandler.Spawn(new JudgmentDetonationParticle(staffTip, JudgementUtils.JudgmentFlame, 0.8f, 10));
            for (int i = 0; i < 6; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2Circular(3f, 3f);
                JudgementParticleHandler.Spawn(new JudgmentEmberParticle(staffTip, sparkVel,
                    JudgementUtils.GetJudgmentColor(Main.rand.NextFloat()), 0.12f, 12));
            }

            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Summons 5 floating ignition orbs that orbit the cursor"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "After a delay, orbs seek enemies and detonate on contact"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The final judgment cannot be appealed, only endured'")
            {
                OverrideColor = new Color(200, 50, 30)
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<Content.DiesIrae.ResonanceEnergies.ResonantCoreOfDiesIrae>(), 25)
                .AddIngredient(ModContent.ItemType<Content.DiesIrae.ResonanceEnergies.DiesIraeResonantEnergy>(), 20)
                .AddIngredient(ModContent.ItemType<Content.DiesIrae.HarmonicCores.HarmonicCoreOfDiesIrae>(), 3)
                .AddIngredient(ItemID.LunarBar, 20)
                .AddTile(ModContent.TileType<Content.Fate.CraftingStations.FatesCosmicAnvilTile>())
                .Register();
        }
    }
}
