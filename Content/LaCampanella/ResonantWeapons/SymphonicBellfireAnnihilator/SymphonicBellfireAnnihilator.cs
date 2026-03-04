using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.SymphonicBellfireAnnihilator.Utilities;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.SymphonicBellfireAnnihilator.Particles;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.SymphonicBellfireAnnihilator.Projectiles;

namespace MagnumOpus.Content.LaCampanella.ResonantWeapons.SymphonicBellfireAnnihilator
{
    /// <summary>
    /// SymphonicBellfireAnnihilator — Ranged heavy launcher, 494dmg.
    /// Primary fire: Grand Crescendo Wave — slow-moving bell-shaped shockwave, pierces, expands.
    /// Alt fire: Bellfire Rockets — rapid arcing rockets that leave fire patches.
    /// Buff Stacking:
    ///   Grand Crescendo Buff (max 5, from wave kills): +10% wave size, +8% dmg per stack.
    ///   Bellfire Crescendo Buff (max 3, from rocket kills): rockets burst 2→3→4.
    /// Symphonic Overture: Both max stacks → massive full-width wave ignoring pierce slowdown.
    /// </summary>
    public class SymphonicBellfireAnnihilatorItem : ModItem
    {
        public override string Texture => "MagnumOpus/Content/LaCampanella/ResonantWeapons/SymphonicBellfireAnnihilator/SymphonicBellfireAnnihilator";
        public override string Name => "SymphonicBellfireAnnihilator";

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.damage = 494;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 70;
            Item.height = 30;
            Item.useTime = 40;
            Item.useAnimation = 40;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 6f;
            Item.value = Item.sellPrice(gold: 50);
            Item.rare = ModContent.RarityType<LaCampanellaRainbowRarity>();
            Item.UseSound = SoundID.Item11;
            Item.shoot = ModContent.ProjectileType<GrandCrescendoWaveProj>();
            Item.useAmmo = AmmoID.Rocket;
            Item.shootSpeed = 8f;
            Item.noMelee = true;
            Item.autoReuse = true;
        }

        public override bool AltFunctionUse(Player player) => true;

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            if (player.altFunctionUse == 2)
            {
                // Alt fire: rapid rockets
                type = ModContent.ProjectileType<BellfireRocketProj>();
                Item.useTime = 12;
                Item.useAnimation = 12;
            }
            else
            {
                // Primary fire: crescendo wave
                type = ModContent.ProjectileType<GrandCrescendoWaveProj>();
                Item.useTime = 40;
                Item.useAnimation = 40;
            }
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            var modPlayer = player.GetModPlayer<SymphonicBellfirePlayer>();
            Vector2 muzzlePos = position + Vector2.Normalize(velocity) * 50f;

            // Muzzle flash
            SymphonicBellfireParticleHandler.SpawnParticle(new RocketExhaustParticle(
                muzzlePos, -velocity * 0.1f, 3f, 10));

            if (player.altFunctionUse == 2)
            {
                // ALT FIRE: Bellfire Rockets
                int burstCount = modPlayer.GetRocketBurstCount(); // 1, 2, 3, or 4

                for (int i = 0; i < burstCount; i++)
                {
                    float spread = MathHelper.ToRadians(5f * (i - burstCount / 2f));
                    Vector2 rocketVel = velocity.RotatedBy(spread);
                    // Slight arc: add upward bias
                    rocketVel.Y -= 1.5f;

                    Projectile.NewProjectile(source, muzzlePos, rocketVel,
                        ModContent.ProjectileType<BellfireRocketProj>(),
                        damage, knockback, player.whoAmI);
                }
            }
            else
            {
                // PRIMARY FIRE: Grand Crescendo Wave
                bool isSymphonicOverture = modPlayer.IsSymphonicOvertureReady();

                if (isSymphonicOverture)
                {
                    modPlayer.ConsumeSymphonicOverture();

                    // Symphonic Overture: massive wave ignoring pierce slowdown
                    Projectile.NewProjectile(source, muzzlePos, velocity,
                        ModContent.ProjectileType<GrandCrescendoWaveProj>(),
                        (int)(damage * 2f), knockback * 2f, player.whoAmI,
                        ai0: 1f); // ai0 = 1 for Overture variant

                    // Big musical note burst
                    for (int i = 0; i < 12; i++)
                    {
                        Vector2 noteVel = Main.rand.NextVector2CircularEdge(5f, 5f);
                        SymphonicBellfireParticleHandler.SpawnParticle(new SymphonicNoteParticle(
                            player.Center, noteVel, Main.rand.Next(50, 80)));
                    }
                }
                else
                {
                    // Normal wave — apply crescendo buff scaling
                    float damageMult = 1f + modPlayer.GrandCrescendoStacks * 0.08f;
                    Projectile.NewProjectile(source, muzzlePos, velocity,
                        ModContent.ProjectileType<GrandCrescendoWaveProj>(),
                        (int)(damage * damageMult), knockback, player.whoAmI,
                        ai0: 0f, ai1: modPlayer.GrandCrescendoStacks);
                }
            }

            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Fires expanding bell-shaped crescendo waves that pierce enemies"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Right click to fire rapid arcing bellfire rockets that leave fire patches"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Wave kills grant Grand Crescendo stacks (max 5): +10% wave size and +8% damage per stack"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Rocket kills grant Bellfire Crescendo stacks (max 3): rockets fire in bursts of 2, 3, then 4"));
            tooltips.Add(new TooltipLine(Mod, "Effect5", "Both at maximum triggers Symphonic Overture: a massive wave ignoring all pierce slowdown"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Fortissimo. Always fortissimo.'")
            {
                OverrideColor = new Color(255, 140, 40)
            });
        }
    }
}
