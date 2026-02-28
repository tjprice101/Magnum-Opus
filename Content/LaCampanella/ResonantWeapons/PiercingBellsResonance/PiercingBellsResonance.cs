using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.PiercingBellsResonance.Utilities;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.PiercingBellsResonance.Projectiles;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.PiercingBellsResonance.Particles;

namespace MagnumOpus.Content.LaCampanella.ResonantWeapons.PiercingBellsResonance
{
    /// <summary>
    /// PiercingBellsResonance — Ranged Gun, 165dmg, useTime 4 (rapid fire).
    /// Scorching Staccato: sustained fire accelerates up to 60% faster.
    /// Every 20th shot fires a Resonant Blast that spawns homing note and seeking crystal sub-projectiles.
    /// </summary>
    public class PiercingBellsResonanceItem : ModItem
    {
        public override string Texture => "MagnumOpus/Content/LaCampanella/ResonantWeapons/PiercingBellsResonance/PiercingBellsResonance";
        public override string Name => "PiercingBellsResonance";

        public override void SetDefaults()
        {
            Item.damage = 165;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 60;
            Item.height = 26;
            Item.useTime = 4;
            Item.useAnimation = 4;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 2.5f;
            Item.value = Item.sellPrice(gold: 50);
            Item.rare = ModContent.RarityType<LaCampanellaRainbowRarity>();
            Item.UseSound = SoundID.Item11;
            Item.shoot = ProjectileID.Bullet;
            Item.useAmmo = AmmoID.Bullet;
            Item.shootSpeed = 16f;
            Item.noMelee = true;
            Item.autoReuse = true;
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            // Apply Scorching Staccato acceleration to useTime/useAnimation via shoot speed
            var modPlayer = player.GetModPlayer<PiercingBellsResonancePlayer>();

            // Slight inaccuracy spread that decreases with staccato buildup (tighter at high speed)
            float spreadReduction = modPlayer.StaccatoSpeed * 0.5f; // 0 → 0.3 reduction
            float maxSpread = MathHelper.ToRadians(3f * (1f - spreadReduction));
            velocity = velocity.RotatedByRandom(maxSpread);

            // Speed bonus from staccato
            velocity *= 1f + modPlayer.StaccatoSpeed * 0.3f;
        }

        public override float UseSpeedMultiplier(Player player)
        {
            var modPlayer = player.GetModPlayer<PiercingBellsResonancePlayer>();
            return modPlayer.GetFireRateMultiplier();
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            var modPlayer = player.GetModPlayer<PiercingBellsResonancePlayer>();
            bool isResonantBlast = modPlayer.RegisterShot();

            // Muzzle position
            Vector2 muzzlePos = position + Vector2.Normalize(velocity) * 40f;

            // Muzzle flash particles
            float angle = velocity.ToRotation();
            PiercingBellsParticleHandler.SpawnParticle(new MuzzleFlashParticle(
                muzzlePos, angle, Main.rand.NextFloat(30f, 50f), Main.rand.Next(5, 10)));
            PiercingBellsParticleHandler.SpawnParticle(new BulletTracerParticle(
                muzzlePos + Main.rand.NextVector2Circular(4, 4),
                velocity * 0.1f + Main.rand.NextVector2Circular(1f, 1f),
                Main.rand.Next(10, 20)));

            if (isResonantBlast)
            {
                // 20th shot: Fire a resonant blast projectile instead of normal bullet
                Projectile.NewProjectile(source, muzzlePos, velocity * 1.5f,
                    ModContent.ProjectileType<ResonantBlastProj>(), damage * 3, knockback * 2f, player.whoAmI);

                // Big muzzle flash
                PiercingBellsParticleHandler.SpawnParticle(new ResonantBlastFlashParticle(
                    muzzlePos, 2f, 15));

                // Musical note burst from muzzle
                for (int i = 0; i < 6; i++)
                {
                    Vector2 noteVel = velocity.SafeNormalize(Vector2.UnitX).RotatedByRandom(0.8f) * Main.rand.NextFloat(2f, 5f);
                    PiercingBellsParticleHandler.SpawnParticle(new ResonantNoteParticle(
                        muzzlePos, noteVel, Main.rand.Next(40, 70)));
                }

                // Still fire the normal bullet too
                Projectile.NewProjectile(source, muzzlePos, velocity, type, damage, knockback, player.whoAmI);
                return false;
            }

            // Normal shot: Fire standard bullet with staccato trail projectile wrapper
            Projectile.NewProjectile(source, muzzlePos, velocity,
                ModContent.ProjectileType<StaccatoBulletProj>(), damage, knockback, player.whoAmI,
                ai0: type); // Store original bullet type in ai[0]

            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Sustained fire triggers Scorching Staccato, accelerating fire rate up to 60%"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Every 20th shot unleashes a resonant blast with homing notes and seeking crystals"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Each piercing note builds upon the last, until the bell's fury can no longer be contained'")
            {
                OverrideColor = new Color(255, 140, 40)
            });
        }
    }
}
