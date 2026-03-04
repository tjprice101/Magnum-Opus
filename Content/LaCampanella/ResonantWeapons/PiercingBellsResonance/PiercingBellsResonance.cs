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
    /// Piercing Bell's Resonance - Precision ranged weapon.
    /// Primary: Staccato Bullets that embed Resonant Markers on hit.
    /// Every 4th shot also fires a Seeking Crystal homing to nearest marker'd enemy.
    /// Alt-fire: Resonant Detonation - detonates all markers on enemies with 3+ markers.
    /// Perfect Pitch: exactly 5 markers = 2x damage + Resonant Silence.
    /// </summary>
    public class PiercingBellsResonanceItem : ModItem
    {
        public override string Texture => "MagnumOpus/Content/LaCampanella/ResonantWeapons/PiercingBellsResonance/PiercingBellsResonance";
        public override string Name => "PiercingBellsResonance";

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.damage = 165;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 60;
            Item.height = 26;
            Item.useTime = 12;
            Item.useAnimation = 12;
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

        public override bool AltFunctionUse(Player player) => true;

        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                // Alt-fire: Resonant Detonation — check if any enemy has 3+ markers
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (!npc.CanBeChasedBy()) continue;
                    if (Vector2.Distance(player.Center, npc.Center) > 1200f) continue;
                    var markers = npc.GetGlobalNPC<ResonantMarkerNPC>();
                    if (markers.CanDetonate) return true;
                }
                return false; // No detonatable targets
            }
            return base.CanUseItem(player);
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            if (player.altFunctionUse == 2)
            {
                // Alt-fire uses faster animation
                Item.useTime = 25;
                Item.useAnimation = 25;
            }
            else
            {
                Item.useTime = 12;
                Item.useAnimation = 12;

                // Small precision spread
                velocity = velocity.RotatedByRandom(MathHelper.ToRadians(1.5f));
            }
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Vector2 muzzlePos = position + Vector2.Normalize(velocity) * 40f;

            if (player.altFunctionUse == 2)
            {
                // Alt-fire: Resonant Detonation — detonate markers on all enemies with 3+
                TriggerResonantDetonation(source, player, damage, knockback);
                return false;
            }

            var modPlayer = player.GetModPlayer<PiercingBellsResonancePlayer>();
            bool isSeekingCrystalShot = modPlayer.RegisterShot();

            // Muzzle flash
            float angle = velocity.ToRotation();
            PiercingBellsParticleHandler.SpawnParticle(new MuzzleFlashParticle(
                muzzlePos, angle, Main.rand.NextFloat(30f, 50f), Main.rand.Next(5, 10)));

            // Fire the staccato bullet (embeds marker on hit)
            Projectile.NewProjectile(source, muzzlePos, velocity,
                ModContent.ProjectileType<StaccatoBulletProj>(), damage, knockback, player.whoAmI);

            // Every 4th shot: also fire a Seeking Crystal
            if (isSeekingCrystalShot)
            {
                Vector2 crystalVel = velocity.RotatedByRandom(0.1f) * 0.8f;
                Projectile.NewProjectile(source, muzzlePos, crystalVel,
                    ModContent.ProjectileType<SeekingCrystalProj>(), (int)(damage * 1.2f), knockback * 1.5f, player.whoAmI);

                // Crystal launch VFX
                for (int i = 0; i < 4; i++)
                {
                    Vector2 sparkVel = velocity.SafeNormalize(Vector2.UnitX).RotatedByRandom(0.5f) * Main.rand.NextFloat(2f, 4f);
                    PiercingBellsParticleHandler.SpawnParticle(new BulletTracerParticle(
                        muzzlePos, sparkVel, Main.rand.Next(10, 18)));
                }
            }

            return false;
        }

        private void TriggerResonantDetonation(EntitySource_ItemUse_WithAmmo source, Player player, int damage, float knockback)
        {
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.CanBeChasedBy()) continue;
                if (Vector2.Distance(player.Center, npc.Center) > 1200f) continue;

                var markers = npc.GetGlobalNPC<ResonantMarkerNPC>();
                if (!markers.CanDetonate) continue;

                int markerCount = markers.ConsumeMarkers();
                bool perfectPitch = markerCount == 5;

                // Scale damage with marker count; Perfect Pitch = 2x
                float dmgMult = 1f + (markerCount - 3) * 0.25f; // 3=1x, 4=1.25x, 5=1.5x, 6=1.75x, etc.
                if (perfectPitch) dmgMult = 2f;

                int detonationDmg = (int)(damage * dmgMult * markerCount * 0.5f);

                // Spawn Resonant Blast at enemy position
                int proj = Projectile.NewProjectile(source, npc.Center, Vector2.Zero,
                    ModContent.ProjectileType<ResonantBlastProj>(), detonationDmg, knockback * 2f, player.whoAmI,
                    ai0: markerCount, ai1: perfectPitch ? 1f : 0f);

                // Spawn Resonant Note landmines around detonation
                int noteCount = Math.Min(markerCount, 6);
                for (int n = 0; n < noteCount; n++)
                {
                    float noteAngle = MathHelper.TwoPi / noteCount * n + Main.rand.NextFloat(-0.2f, 0.2f);
                    Vector2 noteVel = new Vector2((float)Math.Cos(noteAngle), (float)Math.Sin(noteAngle)) * Main.rand.NextFloat(2f, 4f);
                    Projectile.NewProjectile(source, npc.Center, noteVel,
                        ModContent.ProjectileType<ResonantNoteProj>(), (int)(damage * 0.4f), 1f, player.whoAmI);
                }
            }
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Precision staccato bullets embed Resonant Markers on enemies"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Every 4th shot fires a Seeking Crystal that homes toward marked enemies"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Right click to trigger Resonant Detonation on enemies with 3+ markers"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Detonations spawn lingering Resonant Note landmines"));
            tooltips.Add(new TooltipLine(Mod, "Effect5", "Perfect Pitch: exactly 5 markers deals double detonation damage"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Count the tolls. Each one is a judgment.'")
            {
                OverrideColor = new Color(255, 140, 40)
            });
        }
    }
}
