using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Core;
using MagnumOpus.Content.ClairDeLune;
using MagnumOpus.Content.ClairDeLune.Weapons.AutomatonsTuningFork;
using MagnumOpus.Content.ClairDeLune.Weapons.AutomatonsTuningFork.Utilities;

namespace MagnumOpus.Content.ClairDeLune.Weapons.AutomatonsTuningFork.Projectiles
{
    /// <summary>
    /// Automaton Minion — Summoned companion from Automaton's Tuning Fork.
    /// Hovers near player, attacks nearby enemies. Clair de Lune moonlit theme.
    /// Foundation-pattern rendering: safe SpriteBatch, IncisorOrbRenderer visuals.
    /// </summary>
    public class AutomatonMinionProjectile : ModProjectile
    {
        #region Properties

        private const float DetectionRange = 600f;

        private Player Owner => Main.player[Projectile.owner];
        private bool _initialized;

        private VertexStrip _strip;

        // Orb firing
        private int _fireTimer;
        private const int FireCooldown = 50;

        #endregion

        public override string Texture => "MagnumOpus/Content/ClairDeLune/Weapons/AutomatonsTuningFork/AutomatonsTuningForkItem";

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
            Projectile.width = 24;
            Projectile.height = 24;
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
                Owner.ClearBuff(ModContent.BuffType<AutomatonsTuningForkBuff>());
                Projectile.Kill();
                return;
            }

            if (Owner.HasBuff(ModContent.BuffType<AutomatonsTuningForkBuff>()))
                Projectile.timeLeft = 2;

            if (!_initialized)
            {
                _initialized = true;
                Projectile.rotation = Projectile.velocity.ToRotation();
            }

            // Hover near player
            Vector2 targetPos = Owner.Center + new Vector2(Owner.direction * 40f, -50f);
            Vector2 toTarget = targetPos - Projectile.Center;
            Projectile.velocity = toTarget * 0.1f;

            // Gentle bob
            Projectile.position.Y += MathF.Sin(Main.GameUpdateCount * 0.05f) * 0.3f;

            // Move toward nearest enemy
            NPC target = AutomatonsTuningForkUtils.ClosestNPCAt(Projectile.Center, DetectionRange);
            if (target != null)
            {
                Vector2 dir = target.Center - Projectile.Center;
                Projectile.velocity += dir.SafeNormalize(Vector2.Zero) * 0.5f;

                // Fire frequency orbs
                if (Main.myPlayer == Projectile.owner)
                {
                    _fireTimer++;
                    if (_fireTimer >= FireCooldown)
                    {
                        _fireTimer = 0;
                        FireFrequencyOrb(target);
                    }
                }
            }
            else
            {
                _fireTimer = 0;
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
        /// Fires a GenericHomingOrbChild whose behavior depends on the current frequency mode.
        /// Freq 0 (A): Pierce, reduced speed, gentle homing.
        /// Freq 1 (C): Fast, no homing.
        /// Freq 2 (E): Two orbs at 60% damage each.
        /// Freq 3 (G): Decelerate, spawn damage zone on kill.
        /// Perfect Resonance: all properties active with enhanced stats for 5s.
        /// </summary>
        private void FireFrequencyOrb(NPC target)
        {
            int freq = AutomatonsTuningForkItem.CurrentFrequency;
            bool resonance = AutomatonsTuningForkItem.IsResonanceActive;

            Vector2 dir = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
            int baseDamage = Projectile.damage;
            float baseSpeed = 10f;
            float homing = 0.06f;
            int flags = 0;
            int orbCount = 1;
            int orbDamage = baseDamage;

            if (resonance)
            {
                // Perfect Resonance — all properties active with enhanced stats
                flags = GenericHomingOrbChild.FLAG_PIERCE;
                homing = 0.08f;
                baseSpeed = 14f;
                orbCount = 2;
                orbDamage = (int)(baseDamage * 0.8f);
            }
            else
            {
                switch (freq)
                {
                    case 0: // A — Piercing, reduced speed, gentle homing
                        flags = GenericHomingOrbChild.FLAG_PIERCE;
                        homing = 0.06f;
                        baseSpeed *= 0.8f;
                        break;
                    case 1: // C — Fast, no homing
                        homing = 0f;
                        baseSpeed *= 1.4f;
                        break;
                    case 2: // E — Double orbs at 60% damage
                        orbCount = 2;
                        orbDamage = (int)(baseDamage * 0.6f);
                        break;
                    case 3: // G — Decelerate, spawn damage zone on kill
                        flags = GenericHomingOrbChild.FLAG_DECELERATE | GenericHomingOrbChild.FLAG_ZONE_ON_KILL;
                        break;
                }
            }

            for (int i = 0; i < orbCount; i++)
            {
                Vector2 orbVel = dir * baseSpeed;
                if (orbCount > 1)
                    orbVel = orbVel.RotatedBy((i - 0.5f) * 0.15f);

                GenericHomingOrbChild.SpawnChild(
                    Projectile.GetSource_FromThis(),
                    Projectile.Center, orbVel,
                    orbDamage, Projectile.knockBack, Projectile.owner,
                    homing, flags, GenericHomingOrbChild.THEME_CLAIRDELUNE);
            }
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
