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
    /// SymphonicBellfireAnnihilator — Ranged Rocket Launcher, 494dmg, useTime 35, knockBack 6.
    /// 5+5 volley system: First 5 shots fire normal rockets, next 5 fire enhanced flaming rockets.
    /// Completing a volley grants BellfireCrescendoBuff (+10% dmg/speed 30s).
    /// Every 3rd volley triggers Grand Crescendo — massive AoE wave + GrandCrescendoBuff (+20% dmg, +15% speed 15s).
    /// </summary>
    public class SymphonicBellfireAnnihilatorItem : ModItem
    {
        public override string Texture => "MagnumOpus/Content/LaCampanella/ResonantWeapons/SymphonicBellfireAnnihilator/SymphonicBellfireAnnihilator";
        public override string Name => "SymphonicBellfireAnnihilator";

        public override void SetDefaults()
        {
            Item.damage = 494;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 70;
            Item.height = 30;
            Item.useTime = 35;
            Item.useAnimation = 35;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 6f;
            Item.value = Item.sellPrice(gold: 50);
            Item.rare = ModContent.RarityType<LaCampanellaRainbowRarity>();
            Item.UseSound = SoundID.Item11;
            Item.shoot = ProjectileID.RocketI;
            Item.useAmmo = AmmoID.Rocket;
            Item.shootSpeed = 12f;
            Item.noMelee = true;
            Item.autoReuse = true;
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            // Slight aim wobble
            velocity = velocity.RotatedByRandom(MathHelper.ToRadians(1.5f));
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            var modPlayer = player.GetModPlayer<SymphonicBellfirePlayer>();
            int shotResult = modPlayer.RegisterShot();

            Vector2 muzzlePos = position + Vector2.Normalize(velocity) * 50f;

            // Muzzle flash
            SymphonicBellfireParticleHandler.SpawnParticle(new RocketExhaustParticle(
                muzzlePos, -velocity * 0.1f, 3f, 10));

            switch (shotResult)
            {
                case 0: // Normal rocket (shots 1-5)
                    Projectile.NewProjectile(source, muzzlePos, velocity,
                        ModContent.ProjectileType<BellfireRocketProj>(), damage, knockback, player.whoAmI, ai0: 0);
                    break;

                case 1: // Enhanced rocket (shots 6-10)
                    Projectile.NewProjectile(source, muzzlePos, velocity,
                        ModContent.ProjectileType<BellfireRocketProj>(), (int)(damage * 1.3f), knockback * 1.2f, player.whoAmI, ai0: 1);

                    // Volley complete — grant small buff
                    player.AddBuff(ModContent.BuffType<BellfireCrescendoBuff>(), 1800); // 30s
                    break;

                case 2: // Grand Crescendo
                    // Fire the enhanced rocket
                    Projectile.NewProjectile(source, muzzlePos, velocity,
                        ModContent.ProjectileType<BellfireRocketProj>(), (int)(damage * 1.5f), knockback * 1.5f, player.whoAmI, ai0: 2);

                    // Spawn Grand Crescendo wave
                    Projectile.NewProjectile(source, player.Center, Vector2.Zero,
                        ModContent.ProjectileType<GrandCrescendoWaveProj>(), damage * 2, 10f, player.whoAmI);

                    // Grant Grand Crescendo buff
                    player.AddBuff(ModContent.BuffType<GrandCrescendoBuff>(), 900); // 15s

                    // Also refresh the smaller buff
                    player.AddBuff(ModContent.BuffType<BellfireCrescendoBuff>(), 1800);

                    // Big musical note burst
                    for (int i = 0; i < 10; i++)
                    {
                        Vector2 noteVel = Main.rand.NextVector2CircularEdge(4f, 4f);
                        SymphonicBellfireParticleHandler.SpawnParticle(new SymphonicNoteParticle(
                            player.Center, noteVel, Main.rand.Next(50, 80)));
                    }
                    break;
            }

            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Fires devastating bellfire rockets"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Shots 6-10 in each volley deal 30% more damage"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Completing a volley grants Bellfire Crescendo: +10% damage and speed for 30 seconds"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Every 3rd volley triggers a Grand Crescendo: massive wave + +20% damage for 15 seconds"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The symphony reaches its crescendo, and the world trembles before the final note'")
            {
                OverrideColor = new Color(255, 140, 40)
            });
        }
    }
}
