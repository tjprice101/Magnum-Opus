using MagnumOpus.Common;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Content.ClairDeLune.Weapons.ClockworkGrimoire.Projectiles;
using MagnumOpus.Content.ClairDeLune.Weapons.ClockworkGrimoire.Utilities;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.ClairDeLune.Weapons.ClockworkGrimoire
{
    /// <summary>
    /// Clockwork Grimoire — Magic weapon with 4 modes cycling through aspects of timekeeping.
    /// Hour: sustained beam. Minute: 12 ticking orbs. Second: rapid bolts. Pendulum: AoE zone.
    /// Temporal Synergy (H→M→S→P sequence) = 50% enhanced next cast.
    /// "Hours of patience. Minutes of precision. Seconds of fury. And the pendulum swings eternal."
    /// </summary>
    public class ClockworkGrimoire : ModItem
    {
        private int _secondFireTimer;

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.damage = 3600; // Tier 10 (2800-4200 range), slow magic
            Item.DamageType = DamageClass.Magic;
            Item.mana = 15;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 4f;
            Item.value = Item.sellPrice(platinum: 5);
            Item.rare = ModContent.RarityType<ClairDeLuneRarity>();
            Item.UseSound = SoundID.Item43;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<SecondBoltProjectile>();
            Item.shootSpeed = 16f;
            Item.crit = 12;
        }

        public override bool AltFunctionUse(Player player) => true;

        public override bool CanUseItem(Player player)
        {
            var gp = player.GetModPlayer<ClockworkGrimoirePlayer>();

            if (player.altFunctionUse == 2)
            {
                // Alt = cycle mode
                Item.useTime = 15;
                Item.useAnimation = 15;
                Item.mana = 0;
                return true;
            }

            // Apply mode-specific use times
            switch (gp.CurrentMode)
            {
                case 0: // Hour — sustained beam (channel)
                    Item.useTime = 6;
                    Item.useAnimation = 6;
                    Item.mana = 8;
                    Item.channel = true;
                    break;
                case 1: // Minute — fires 12 orbs at once
                    Item.useTime = 45;
                    Item.useAnimation = 45;
                    Item.mana = 30;
                    Item.channel = false;
                    break;
                case 2: // Second — rapid bolts
                    Item.useTime = 3;
                    Item.useAnimation = 3;
                    Item.mana = 3;
                    Item.channel = false;
                    break;
                case 3: // Pendulum — creates zone
                    Item.useTime = 60;
                    Item.useAnimation = 60;
                    Item.mana = 40;
                    Item.channel = false;
                    break;
            }

            return base.CanUseItem(player);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            var gp = player.GetModPlayer<ClockworkGrimoirePlayer>();

            if (player.altFunctionUse == 2)
            {
                gp.CycleMode();
                SoundEngine.PlaySound(SoundID.Item4 with { Pitch = 0.3f, Volume = 0.5f }, position);

                // Mode change VFX
                var flash = new BloomParticle(position, Vector2.Zero,
                    gp.GetModeColor() with { A = 0 } * 0.4f, 0.3f, 6);
                MagnumParticleHandler.SpawnParticle(flash);

                // Synergy flash if activated
                if (gp.SynergyActive)
                {
                    var synFlash = new BloomParticle(position, Vector2.Zero,
                        ClairDeLunePalette.MoonbeamGold with { A = 0 } * 0.6f, 0.5f, 10);
                    MagnumParticleHandler.SpawnParticle(synFlash);
                }
                return false;
            }

            bool synergy = gp.ConsumeSynergy();
            float dmgMult = synergy ? 1.5f : 1f;
            int enhancedDmg = (int)(damage * dmgMult);

            switch (gp.CurrentMode)
            {
                case 0: // Hour Beam
                    // Check if beam already exists
                    bool beamExists = false;
                    for (int i = 0; i < Main.maxProjectiles; i++)
                    {
                        if (Main.projectile[i].active && Main.projectile[i].type == ModContent.ProjectileType<HourBeamProjectile>()
                            && Main.projectile[i].owner == player.whoAmI)
                        {
                            beamExists = true;
                            break;
                        }
                    }
                    if (!beamExists)
                    {
                        Projectile.NewProjectile(source, position, velocity.SafeNormalize(Vector2.UnitX),
                            ModContent.ProjectileType<HourBeamProjectile>(),
                            enhancedDmg, knockback, player.whoAmI, synergy ? 1 : 0);
                    }
                    break;

                case 1: // Minute — 12 orbs
                    for (int i = 0; i < 12; i++)
                    {
                        float spread = MathHelper.ToRadians(-30f + 60f * i / 11f);
                        Vector2 orbVel = velocity.RotatedBy(spread) * Main.rand.NextFloat(0.7f, 1.1f);
                        Projectile.NewProjectile(source, position, orbVel,
                            ModContent.ProjectileType<MinuteOrbProjectile>(),
                            (int)(enhancedDmg * 0.35f), knockback * 0.5f, player.whoAmI);
                    }
                    SoundEngine.PlaySound(SoundID.Item8 with { Pitch = 0.2f }, position);
                    break;

                case 2: // Second — rapid bolts
                    Vector2 boltVel = velocity.RotatedByRandom(0.05f);
                    Projectile.NewProjectile(source, position, boltVel,
                        ModContent.ProjectileType<SecondBoltProjectile>(),
                        (int)(enhancedDmg * 0.2f), knockback * 0.3f, player.whoAmI);
                    break;

                case 3: // Pendulum zone
                    Vector2 targetPos = Main.MouseWorld;
                    Projectile.NewProjectile(source, targetPos, Vector2.Zero,
                        ModContent.ProjectileType<PendulumZoneProjectile>(),
                        (int)(enhancedDmg * 0.8f), knockback, player.whoAmI, synergy ? 1 : 0);
                    SoundEngine.PlaySound(SoundID.Item29 with { Pitch = -0.3f }, targetPos);
                    break;
            }

            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Player player = Main.LocalPlayer;
            var gp = player.GetModPlayer<ClockworkGrimoirePlayer>();

            tooltips.Add(new TooltipLine(Mod, "Mode", $"Current mode: {gp.GetModeName()} — right click to cycle"));
            tooltips.Add(new TooltipLine(Mod, "Hour", "Hour Mode: sustained beam of temporal energy"));
            tooltips.Add(new TooltipLine(Mod, "Minute", "Minute Mode: launches 12 ticking orbs that detonate"));
            tooltips.Add(new TooltipLine(Mod, "Second", "Second Mode: rapid-fire piercing bolts"));
            tooltips.Add(new TooltipLine(Mod, "Pendulum", "Pendulum Mode: creates swinging temporal damage zone"));
            tooltips.Add(new TooltipLine(Mod, "Synergy", "Using all 4 modes in sequence enhances the next cast by 50%"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Hours of patience. Minutes of precision. Seconds of fury. And the pendulum swings eternal.'")
            {
                OverrideColor = ClairDeLunePalette.LoreText
            });
        }
    }
}
