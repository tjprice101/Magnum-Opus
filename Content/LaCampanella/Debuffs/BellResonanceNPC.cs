using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.FoundationWeapons.ImpactFoundation;
using MagnumOpus.Content.FoundationWeapons.ExplosionParticlesFoundation;
using MagnumOpus.Content.FoundationWeapons.XSlashFoundation;

namespace MagnumOpus.Content.LaCampanella.Debuffs
{
    /// <summary>
    /// Bell Resonance — Per-NPC resonance ring tracking for Dual Fated Chime.
    /// Each hit adds a Resonance Ring (max 5). At 5 rings, next hit triggers Bell Shatter:
    /// massive AoE damage burst + all rings detonate as fire waves.
    /// Stacks decay after 3 seconds (180 frames) of no new rings.
    /// </summary>
    public class BellResonanceNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        /// <summary>Current number of resonance rings on this NPC (0-5).</summary>
        public int ResonanceRings { get; set; }

        /// <summary>Maximum resonance rings before Bell Shatter triggers.</summary>
        public const int MaxRings = 5;

        /// <summary>Frames before rings start decaying (3 seconds).</summary>
        public const int DecayDelay = 180;

        /// <summary>Timer that counts down to ring decay.</summary>
        public int DecayTimer { get; set; }

        public override void ResetEffects(NPC npc)
        {
            if (DecayTimer > 0)
            {
                DecayTimer--;
                if (DecayTimer <= 0 && ResonanceRings > 0)
                {
                    ResonanceRings = 0;
                }
            }
        }

        /// <summary>
        /// Add a resonance ring. If already at max, trigger Bell Shatter.
        /// </summary>
        public void AddResonanceRing(NPC npc, int playerOwner)
        {
            DecayTimer = DecayDelay;

            if (ResonanceRings >= MaxRings)
            {
                // Bell Shatter! Massive AoE burst
                TriggerBellShatter(npc, playerOwner);
                ResonanceRings = 0;
                return;
            }

            ResonanceRings++;

            // Visual feedback: gold ring dust burst proportional to stack count
            int dustCount = 4 + ResonanceRings * 2;
            float ringRadius = 20f + ResonanceRings * 8f;
            for (int i = 0; i < dustCount; i++)
            {
                float angle = MathHelper.TwoPi * i / dustCount;
                Vector2 offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * ringRadius;
                Vector2 vel = offset * 0.05f;
                Dust d = Dust.NewDustPerfect(npc.Center + offset, DustID.GoldFlame, vel, 0,
                    new Color(255, 210, 80), 0.8f + ResonanceRings * 0.15f);
                d.noGravity = true;
                d.fadeIn = 0.8f;
            }

            // Bell chime sound — pitch rises with stacks
            SoundEngine.PlaySound(SoundID.Item35 with
            {
                Pitch = -0.3f + ResonanceRings * 0.15f,
                Volume = 0.3f + ResonanceRings * 0.05f,
                MaxInstances = 3
            }, npc.Center);
        }

        /// <summary>
        /// Bell Shatter: Massive damage burst at max resonance stacks.
        /// Deals 200% of the weapon's base damage as AoE.
        /// </summary>
        private void TriggerBellShatter(NPC npc, int playerOwner)
        {
            Player player = Main.player[playerOwner];
            Vector2 center = npc.Center;
            float shatterRadius = 200f;

            // Damage all nearby NPCs
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC other = Main.npc[i];
                if (!other.active || other.friendly || other.dontTakeDamage)
                    continue;

                float dist = Vector2.Distance(center, other.Center);
                if (dist < shatterRadius)
                {
                    float falloff = 1f - (dist / shatterRadius) * 0.5f;
                    int shatterDamage = (int)(player.GetWeaponDamage(player.HeldItem) * 2f * falloff);

                    NPC.HitInfo hitInfo = new NPC.HitInfo
                    {
                        Damage = shatterDamage,
                        Knockback = 8f,
                        HitDirection = Math.Sign(other.Center.X - center.X),
                        Crit = false,
                        DamageType = DamageClass.MeleeNoSpeed
                    };
                    other.StrikeNPC(hitInfo);
                }
            }

            // === FOUNDATION: SparkExplosionProjectile — 12 directional Bell Flame Wave sparks (80+ sparks) ===
            Projectile.NewProjectile(
                player.GetSource_FromThis(), center, Vector2.Zero,
                ModContent.ProjectileType<SparkExplosionProjectile>(),
                0, 0f, playerOwner,
                ai0: (float)SparkMode.RadialScatter);

            // === FOUNDATION: XSlashEffect — Bell Shatter cross-detonation ===
            Projectile.NewProjectile(
                player.GetSource_FromThis(), center, Vector2.Zero,
                ModContent.ProjectileType<XSlashEffect>(),
                0, 0f, playerOwner,
                ai0: Main.rand.NextFloat(MathHelper.TwoPi), ai1: (float)XSlashStyle.LaCampanella);

            // === FOUNDATION: RippleEffectProjectile — Massive expanding bell shockwave ===
            Projectile.NewProjectile(
                player.GetSource_FromThis(), center, Vector2.Zero,
                ModContent.ProjectileType<RippleEffectProjectile>(),
                0, 0f, playerOwner, ai0: 1f);

            // Legacy VFX: expanding fire shockwave rings (layered on top of Foundation)
            for (int ring = 0; ring < 3; ring++)
            {
                int dustCount = 24 + ring * 12;
                float radius = 40f + ring * 60f;
                for (int i = 0; i < dustCount; i++)
                {
                    float angle = MathHelper.TwoPi * i / dustCount;
                    Vector2 dir = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
                    Vector2 vel = dir * (4f + ring * 2f);
                    Color col = Color.Lerp(new Color(255, 240, 200), new Color(255, 100, 0), ring / 2f);
                    Dust d = Dust.NewDustPerfect(center + dir * (10f + ring * 15f), DustID.Torch, vel, 0, col, 2f - ring * 0.3f);
                    d.noGravity = true;
                    d.fadeIn = 1.2f;
                }
            }

            // Black smoke cloud
            for (int i = 0; i < 15; i++)
            {
                Vector2 smokeVel = Main.rand.NextVector2Circular(5f, 5f) + new Vector2(0, -1f);
                Dust d = Dust.NewDustPerfect(center + Main.rand.NextVector2Circular(30f, 30f),
                    DustID.Smoke, smokeVel, 120, new Color(20, 15, 20), 2.5f);
                d.noGravity = true;
            }

            // Gold sparkles
            for (int i = 0; i < 20; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2CircularEdge(6f, 6f);
                Dust d = Dust.NewDustPerfect(center, DustID.GoldFlame, sparkVel, 0,
                    new Color(255, 210, 80), 1.5f);
                d.noGravity = true;
            }

            // Bell Shatter sound
            SoundEngine.PlaySound(SoundID.Item14 with { Pitch = 0.2f, Volume = 1.0f }, center);
            SoundEngine.PlaySound(SoundID.Item45 with { Pitch = 0.5f, Volume = 0.8f }, center);
        }

        public override void DrawEffects(NPC npc, ref Color drawColor)
        {
            if (ResonanceRings <= 0)
                return;

            // Draw resonance ring indicators around the NPC
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.1f + npc.whoAmI) * 0.3f + 0.7f;
            for (int ring = 0; ring < ResonanceRings; ring++)
            {
                float ringAngle = Main.GameUpdateCount * 0.03f * (1f + ring * 0.2f) + ring * MathHelper.TwoPi / MaxRings;
                float radius = 25f + ring * 10f;
                Vector2 offset = new Vector2((float)Math.Cos(ringAngle), (float)Math.Sin(ringAngle)) * radius;
                Color ringColor = Color.Lerp(new Color(255, 180, 40), new Color(255, 240, 200), ring / (float)MaxRings) * pulse;

                // Small orbiting gold dust
                if (Main.rand.NextBool(8))
                {
                    Dust d = Dust.NewDustPerfect(npc.Center + offset, DustID.GoldFlame,
                        Vector2.Zero, 0, ringColor, 0.5f + ring * 0.1f);
                    d.noGravity = true;
                    d.fadeIn = 0.5f;
                }
            }
        }
    }
}
