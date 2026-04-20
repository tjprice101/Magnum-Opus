using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Core;
using MagnumOpus.Content.ClairDeLune;
using MagnumOpus.Content.ClairDeLune.Weapons.GearDrivenArbiter.Utilities;

namespace MagnumOpus.Content.ClairDeLune.Weapons.GearDrivenArbiter.Projectiles
{
    /// <summary>
    /// Arbiter Minion — Summoned companion from Gear-Driven Arbiter.
    /// Orbits player, attacks nearby enemies. Clair de Lune moonlit theme.
    /// Foundation-pattern rendering: safe SpriteBatch, IncisorOrbRenderer visuals.
    /// </summary>
    public class ArbiterMinionProjectile : ModProjectile
    {
        #region Properties

        private const float OrbitRadius = 70f;
        private const float DetectionRange = 700f;

        private Player Owner => Main.player[Projectile.owner];
        private bool _initialized;
        private float _orbitAngle;

        private VertexStrip _strip;

        // Orb firing
        private int _fireTimer;
        private const int FireCooldown = 45;

        // === Verdict Stacking System ===
        /// <summary>Per-NPC verdict stacks: (stacks, decayTimer in frames).</summary>
        public static Dictionary<int, (int stacks, int decayTimer)> VerdictStacks = new();
        private static uint _lastDecayFrame;

        #endregion

        public override string Texture => "MagnumOpus/Content/ClairDeLune/Weapons/GearDrivenArbiter/GearDrivenArbiterItem";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.MinionSacrificable[Type] = true;
            ProjectileID.Sets.MinionTargettingFeature[Type] = true;
            Main.projPet[Type] = true;
            ProjectileID.Sets.TrailCacheLength[Type] = 16;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 28;
            Projectile.height = 28;
            Projectile.friendly = true;
            Projectile.minion = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.minionSlots = 1f;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }

        public override bool? CanCutTiles() => false;
        public override bool MinionContactDamage() => true;

        public override void AI()
        {
            // Check active
            if (!Owner.active || Owner.dead)
            {
                Owner.ClearBuff(ModContent.BuffType<GearDrivenArbiterBuff>());
                Projectile.Kill();
                return;
            }

            if (Owner.HasBuff(ModContent.BuffType<GearDrivenArbiterBuff>()))
                Projectile.timeLeft = 2;

            if (!_initialized)
            {
                _initialized = true;
                Projectile.rotation = Projectile.velocity.ToRotation();
            }

            // Count and index for orbit spacing
            int arbiterCount = 0;
            float orbiterIndex = 0;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                if (Main.projectile[i].active && Main.projectile[i].owner == Projectile.owner &&
                    Main.projectile[i].type == Projectile.type)
                {
                    if (i < Projectile.whoAmI) orbiterIndex++;
                    arbiterCount++;
                }
            }

            float angleOffset = arbiterCount > 1 ? orbiterIndex * MathHelper.TwoPi / arbiterCount : 0f;
            _orbitAngle += MathHelper.ToRadians(1.8f);

            Vector2 targetPos = Owner.Center + new Vector2(
                MathF.Cos(_orbitAngle + angleOffset) * OrbitRadius,
                MathF.Sin(_orbitAngle + angleOffset) * OrbitRadius * 0.6f - 30f
            );

            Vector2 toTarget = targetPos - Projectile.Center;
            Projectile.velocity = toTarget * 0.12f;

            // Move toward nearest enemy
            NPC target = GearDrivenArbiterUtils.ClosestNPCAt(Projectile.Center, DetectionRange);
            if (target != null)
            {
                Vector2 dir = target.Center - Projectile.Center;
                Projectile.velocity += dir.SafeNormalize(Vector2.Zero) * 0.5f;

                // Fire verdict orbs
                if (Main.myPlayer == Projectile.owner)
                {
                    _fireTimer++;
                    if (_fireTimer >= FireCooldown)
                    {
                        _fireTimer = 0;
                        FireVerdictOrb(target);
                    }
                }
            }
            else
            {
                _fireTimer = 0;
            }

            // Decay verdict stacks (once per frame, not per minion)
            if (Main.GameUpdateCount != _lastDecayFrame)
            {
                _lastDecayFrame = Main.GameUpdateCount;
                DecayVerdictStacks();
            }

            Projectile.rotation = Projectile.velocity.ToRotation();

            // Trail dust — moonlit theme
            if (Main.rand.NextBool(3))
            {
                int dustType = Main.rand.NextBool() ? DustID.IceTorch : DustID.WhiteTorch;
                Color dustColor = Main.rand.NextBool() ? new Color(150, 200, 255) : new Color(240, 240, 255);
                Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(6f, 6f),
                    dustType, -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(0.5f, 0.5f),
                    0, dustColor, 0.8f);
                d.noGravity = true;
                d.fadeIn = 0.6f;
            }

            // Pulsing light
            float pulse = 1f + 0.15f * (float)Math.Sin(Main.GameUpdateCount * 0.1f);
            Lighting.AddLight(Projectile.Center, new Vector3(0.35f, 0.45f, 0.6f) * 0.35f * pulse);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Vector2 hitPos = target.Center;

            // Impact sparks — moonlit dual tone
            for (int i = 0; i < 6; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2CircularEdge(4f, 4f);
                Color col = i % 2 == 0 ? new Color(150, 200, 255) : new Color(240, 240, 255);
                Dust d = Dust.NewDustPerfect(hitPos, DustID.IceTorch, sparkVel, 0, col, 0.5f);
                d.noGravity = true;
            }

            // Pearl accent on impact
            for (int i = 0; i < 2; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(2f, 2f) + new Vector2(0, -1f);
                Dust d = Dust.NewDustPerfect(hitPos + Main.rand.NextVector2Circular(8f, 8f),
                    DustID.WhiteTorch, vel, 0, new Color(240, 240, 255), 0.5f);
                d.noGravity = true;
            }

            try { ClairDeLuneVFXLibrary.SpawnMusicNotes(hitPos, 1, 12f, 0.4f, 0.7f, 20); } catch { }
            try { ClairDeLuneVFXLibrary.SpawnMixedSparkleImpact(hitPos, 0.6f, 4, 4); } catch { }
        }

        /// <summary>
        /// Fires a GenericHomingOrbChild with properties based on the target's verdict stacks.
        /// 2+ stacks: homing 0.10. 4+ stacks: pierce. 6+ stacks: +30% speed.
        /// 8 stacks: Arbiter's Verdict — 5x damage, reset stacks.
        /// Increments target stacks on each fire.
        /// </summary>
        private void FireVerdictOrb(NPC target)
        {
            int npcId = target.whoAmI;
            int stacks = 0;
            if (VerdictStacks.TryGetValue(npcId, out var data))
                stacks = data.stacks;

            // Build orb parameters based on current stacks
            float homing = 0.06f;
            int flags = 0;
            float speed = 10f;
            int damage = Projectile.damage;

            if (stacks >= 2) homing = 0.10f;
            if (stacks >= 4) flags |= GenericHomingOrbChild.FLAG_PIERCE;
            if (stacks >= 6) speed *= 1.3f;

            if (stacks >= 8)
            {
                // Arbiter's Verdict — 5x damage, reset stacks
                damage *= 5;
                VerdictStacks.Remove(npcId);

                // Verdict VFX burst
                for (int i = 0; i < 12; i++)
                {
                    Vector2 sparkVel = Main.rand.NextVector2CircularEdge(6f, 6f);
                    Color col = i % 2 == 0 ? new Color(150, 200, 255) : ClairDeLunePalette.MoonbeamGold;
                    Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.IceTorch, sparkVel, 0, col, 0.8f);
                    d.noGravity = true;
                }
            }
            else
            {
                // Increment stacks, reset decay timer to 180 frames (3s)
                VerdictStacks[npcId] = (Math.Min(stacks + 1, 8), 180);
            }

            Vector2 vel = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX) * speed;
            GenericHomingOrbChild.SpawnChild(
                Projectile.GetSource_FromThis(),
                Projectile.Center, vel,
                damage, Projectile.knockBack, Projectile.owner,
                homing, flags, GenericHomingOrbChild.THEME_CLAIRDELUNE);
        }

        /// <summary>
        /// Decays verdict stacks: timer counts down each frame,
        /// removes 1 stack when timer hits 0, resets timer to 180.
        /// Cleans up dead NPCs.
        /// </summary>
        private static void DecayVerdictStacks()
        {
            if (VerdictStacks.Count == 0) return;

            var toRemove = new List<int>();
            var toUpdate = new List<(int key, int stacks, int timer)>();

            foreach (var kvp in VerdictStacks)
            {
                int npcId = kvp.Key;
                var (stacks, timer) = kvp.Value;

                // Clean up dead NPCs
                if (npcId < 0 || npcId >= Main.maxNPCs || !Main.npc[npcId].active)
                {
                    toRemove.Add(npcId);
                    continue;
                }

                timer--;
                if (timer <= 0)
                {
                    stacks--;
                    if (stacks <= 0)
                    {
                        toRemove.Add(npcId);
                        continue;
                    }
                    timer = 180; // 3s until next stack decay
                }
                toUpdate.Add((npcId, stacks, timer));
            }

            foreach (var k in toRemove) VerdictStacks.Remove(k);
            foreach (var u in toUpdate) VerdictStacks[u.key] = (u.stacks, u.timer);
        }

        #region Rendering

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            try
            {
                IncisorOrbRenderer.DrawOrbVisuals(Main.spriteBatch, Projectile, IncisorOrbRenderer.ClairDeLune, ref _strip);
            }
            catch { }
            finally
            {
                try { sb.End(); } catch { }
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                    DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }

            return false;
        }

        #endregion

        public override void OnKill(int timeLeft)
        {
            // Death VFX — moonlit spark burst
            for (int i = 0; i < 4; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2CircularEdge(3f, 3f);
                Color col = Main.rand.NextBool() ? new Color(150, 200, 255) : new Color(240, 240, 255);
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.IceTorch, sparkVel, 0, col, 0.3f);
                d.noGravity = true;
            }

            try { ClairDeLuneVFXLibrary.SpawnMusicNotes(Projectile.Center, 1, 12f, 0.5f, 0.7f, 20); } catch { }
            try { ClairDeLuneVFXLibrary.SpawnMixedSparkleImpact(Projectile.Center, 0.5f, 4, 4); } catch { }
            try { ClairDeLuneVFXLibrary.SpawnLunarSparkles(Projectile.Center, 3, 15f); } catch { }
        }
    }
}
