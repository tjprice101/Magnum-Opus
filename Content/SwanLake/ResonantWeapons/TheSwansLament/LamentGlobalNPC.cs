using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.SwanLake.ResonantWeapons.TheSwansLament.Projectiles;
using MagnumOpus.Content.SwanLake.ResonantWeapons.TheSwansLament.Utilities;

namespace MagnumOpus.Content.SwanLake.ResonantWeapons.TheSwansLament
{
    /// <summary>
    /// Tracks Lament-related state per NPC (OVERHAUL):
    /// - Registers kills for the Lament's Echo fire-rate buff
    /// - Spawns Destruction Halo projectiles on enemy death when killed by Lament weapons
    /// - Lamentation stacks: consecutive hits on same target build stacks (max 5)
    ///   At 5 stacks: target begins "weeping" (cosmetic + -20% attack speed)
    /// - Finale Lament: if a Destruction Halo kills an enemy, all enemies within
    ///   nova radius receive 5 Lamentation stacks instantly
    /// </summary>
    public class LamentGlobalNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        /// <summary>Whether this NPC was most recently hit by a Swan's Lament projectile.</summary>
        public bool HitByLament { get; set; }
        public int LamentOwner { get; set; } = -1;

        /// <summary>Lamentation stacks on this NPC from consecutive hits. Max 5.</summary>
        public int LamentationStacks { get; set; }

        /// <summary>Max Lamentation stacks before weeping triggers.</summary>
        public const int MaxLamentationStacks = 5;

        /// <summary>Whether this NPC is weeping (5 stacks reached).</summary>
        public bool IsWeeping => LamentationStacks >= MaxLamentationStacks;

        /// <summary>Whether this NPC was killed by a Destruction Halo (for Finale Lament).</summary>
        public bool KilledByHalo { get; set; }

        /// <summary>Timer for Lamentation stack decay.</summary>
        public int LamentationDecayTimer;
        private const int LamentationDecayDelay = 180; // 3 seconds between hits before decay

        public override void ResetEffects(NPC npc)
        {
            // Decay timer
            if (LamentationDecayTimer > 0)
            {
                LamentationDecayTimer--;
            }
            else if (LamentationStacks > 0 && !HitByLament)
            {
                LamentationStacks = 0; // Reset if too long without a hit
            }
        }

        public override void OnHitByProjectile(NPC npc, Projectile projectile, NPC.HitInfo hit, int damageDone)
        {
            if (projectile.type == ModContent.ProjectileType<LamentBulletProj>() ||
                projectile.type == ModContent.ProjectileType<DestructionHaloProj>())
            {
                HitByLament = true;
                LamentOwner = projectile.owner;

                // Build Lamentation stacks on consecutive hits
                if (LamentationStacks < MaxLamentationStacks)
                    LamentationStacks++;
                LamentationDecayTimer = LamentationDecayDelay;

                // Track halo kills for Finale Lament
                if (projectile.type == ModContent.ProjectileType<DestructionHaloProj>())
                    KilledByHalo = true;
                else
                    KilledByHalo = false;
            }
        }

        public override void PostAI(NPC npc)
        {
            // Weeping effect at max stacks: -20% attack speed (slow NPC actions)
            if (IsWeeping)
            {
                // Visual: weeping teardrop particles
                if (Main.rand.NextBool(8))
                {
                    Vector2 tearPos = npc.Top + new Vector2(Main.rand.NextFloat(-6f, 6f), -4f);
                    Vector2 tearVel = new Vector2(0, 1.5f + Main.rand.NextFloat(0.5f));
                    Dust d = Dust.NewDustPerfect(tearPos, DustID.WhiteTorch, tearVel, 0,
                        new Color(200, 210, 240), 0.5f);
                    d.noGravity = false;
                    d.fadeIn = 0.3f;
                }
            }
        }

        public override void ModifyIncomingHit(NPC npc, ref NPC.HitModifiers modifiers)
        {
            // Weeping: bonus damage from all sources when at max Lamentation
            if (IsWeeping)
            {
                modifiers.FinalDamage += 0.1f; // +10% incoming damage while weeping
            }
        }

        public override void OnKill(NPC npc)
        {
            if (!HitByLament || LamentOwner < 0 || LamentOwner >= Main.maxPlayers) return;

            Player owner = Main.player[LamentOwner];
            if (!owner.active || owner.dead) return;

            // Register kill for Lament's Echo
            var echoPlayer = owner.GetModPlayer<LamentPlayer>();
            echoPlayer.RegisterKill();

            // Spawn Destruction Halo at the death location
            if (Main.myPlayer == LamentOwner)
            {
                int haloDamage = (int)(owner.GetTotalDamage(DamageClass.Ranged).ApplyTo(90));
                Projectile.NewProjectile(
                    owner.GetSource_FromThis(),
                    npc.Center,
                    Vector2.Zero,
                    ModContent.ProjectileType<DestructionHaloProj>(),
                    haloDamage, 4f, LamentOwner);
            }

            // Finale Lament: if killed by a Destruction Halo, apply 5 Lamentation stacks
            // to all nearby enemies (nova radius)
            if (KilledByHalo && Main.myPlayer == LamentOwner)
            {
                float novaRadius = 200f;
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC nearby = Main.npc[i];
                    if (nearby.active && !nearby.friendly && nearby.whoAmI != npc.whoAmI &&
                        Vector2.Distance(nearby.Center, npc.Center) <= novaRadius)
                    {
                        var nearbyLament = nearby.GetGlobalNPC<LamentGlobalNPC>();
                        nearbyLament.LamentationStacks = MaxLamentationStacks;
                        nearbyLament.LamentationDecayTimer = LamentationDecayDelay;
                        nearbyLament.HitByLament = true;
                        nearbyLament.LamentOwner = LamentOwner;
                    }
                }

                // Finale Lament VFX: white flash nova
                for (int i = 0; i < 16; i++)
                {
                    Vector2 vel = Main.rand.NextVector2CircularEdge(6f, 6f);
                    Dust d = Dust.NewDustPerfect(npc.Center, DustID.WhiteTorch, vel, 0,
                        new Color(240, 235, 250), 1.2f);
                    d.noGravity = true;
                }

                SwanLakeVFXLibrary.SpawnPrismaticSparkles(npc.Center, 8, novaRadius * 0.4f);
                SwanLakeVFXLibrary.SpawnMusicNotes(npc.Center, 5, novaRadius * 0.5f, 0.8f, 1.2f, 35);
            }
        }
    }
}
