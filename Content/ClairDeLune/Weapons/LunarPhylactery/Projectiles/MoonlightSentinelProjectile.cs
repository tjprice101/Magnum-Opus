using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Core;
using MagnumOpus.Content.ClairDeLune;
using MagnumOpus.Content.ClairDeLune.Weapons.LunarPhylactery.Utilities;

namespace MagnumOpus.Content.ClairDeLune.Weapons.LunarPhylactery.Projectiles
{
    /// <summary>
    /// Moonlight Sentinel — Summoned companion from Lunar Phylactery.
    /// Orbits player, fires homing orbs at enemies. Clair de Lune moonlit theme.
    /// Soul-Link: damage and homing scale inversely with player HP.
    /// Every 60 frames fires a 3-orb burst (5-frame gaps) instead of single orb.
    /// Foundation-pattern rendering: safe SpriteBatch, IncisorOrbRenderer visuals.
    /// </summary>
    public class MoonlightSentinelProjectile : ModProjectile
    {
        #region Properties

        private const float DetectionRange = 800f;

        private Player Owner => Main.player[Projectile.owner];
        private bool _initialized;
        private float _orbitAngle;

        // Fire timer for orb spawning
        private int _fireTimer;
        // Burst tracking: how many orbs left in current burst, and countdown between burst shots
        private int _burstRemaining;
        private int _burstDelay;
        private int _burstTargetIndex = -1; // NPC index of burst target

        private VertexStrip _strip;

        #endregion

        public override string Texture => "MagnumOpus/Content/ClairDeLune/Weapons/LunarPhylactery/LunarPhylacteryItem";

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
        public override bool MinionContactDamage() => false;

        public override void AI()
        {
            // Check active
            if (!Owner.active || Owner.dead)
            {
                Owner.ClearBuff(ModContent.BuffType<LunarPhylacteryBuff>());
                Projectile.Kill();
                return;
            }

            if (Owner.HasBuff(ModContent.BuffType<LunarPhylacteryBuff>()))
                Projectile.timeLeft = 2;

            if (!_initialized)
            {
                _initialized = true;
                Projectile.rotation = Projectile.velocity.ToRotation();
            }

            // Orbit around player (keep existing movement)
            _orbitAngle += MathHelper.ToRadians(2f);
            Vector2 targetPos = Owner.Center + new Vector2(
                MathF.Cos(_orbitAngle) * 60f,
                MathF.Sin(_orbitAngle) * 30f - 40f
            );

            Vector2 toTarget = targetPos - Projectile.Center;
            Projectile.velocity = toTarget * 0.15f;

            // Move toward nearest enemy
            NPC target = LunarPhylacteryUtils.ClosestNPCAt(Projectile.Center, DetectionRange);
            if (target != null)
            {
                Vector2 dir = target.Center - Projectile.Center;
                Projectile.velocity += dir.SafeNormalize(Vector2.Zero) * 0.5f;
            }

            Projectile.rotation = Projectile.velocity.ToRotation();

            // === Soul-Link: HP-scaled orb firing ===
            float hpRatio = Owner.statLife / (float)Owner.statLifeMax2;
            hpRatio = MathHelper.Clamp(hpRatio, 0f, 1f);

            // Process burst shots in progress
            if (_burstRemaining > 0)
            {
                _burstDelay--;
                if (_burstDelay <= 0)
                {
                    FireHomingOrb(hpRatio, _burstTargetIndex);
                    _burstRemaining--;
                    _burstDelay = 5; // 5-frame gap between burst shots
                }
            }
            else
            {
                // Fire timer: every 60 frames start a 3-orb burst
                _fireTimer++;
                if (_fireTimer >= 60)
                {
                    _fireTimer = 0;

                    // Find target for the burst
                    NPC fireTarget = LunarPhylacteryUtils.ClosestNPCAt(Projectile.Center, DetectionRange);
                    if (fireTarget != null)
                    {
                        _burstTargetIndex = fireTarget.whoAmI;
                        _burstRemaining = 3;
                        _burstDelay = 0; // Fire first orb immediately
                    }
                }
            }

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

        private void FireHomingOrb(float hpRatio, int targetIndex)
        {
            // Validate target
            if (targetIndex < 0 || targetIndex >= Main.maxNPCs) return;
            NPC npc = Main.npc[targetIndex];
            if (!npc.active || npc.friendly || npc.dontTakeDamage)
            {
                // Try to find a new target
                NPC fallback = LunarPhylacteryUtils.ClosestNPCAt(Projectile.Center, DetectionRange);
                if (fallback == null) return;
                npc = fallback;
            }

            // Damage scaling: 1x at full HP, 2x at 0% HP
            float damageMultiplier = 1f + (1f - hpRatio);

            // Homing scaling: 0.04 at full HP, 0.14 at 0% HP
            float homingStrength = 0.04f + (1f - hpRatio) * 0.10f;

            Vector2 fireVel = (npc.Center - Projectile.Center).SafeNormalize(Vector2.UnitX) * 10f;
            fireVel += Main.rand.NextVector2Circular(1.5f, 1.5f); // Slight spread for burst variety

            int scaledDamage = (int)(Projectile.damage * damageMultiplier);

            GenericHomingOrbChild.SpawnChild(
                Projectile.GetSource_FromThis(),
                Projectile.Center, fireVel,
                scaledDamage, Projectile.knockBack * 0.5f, Projectile.owner,
                homingStrength: homingStrength,
                behaviorFlags: GenericHomingOrbChild.FLAG_ACCELERATE,
                themeIndex: GenericHomingOrbChild.THEME_CLAIRDELUNE,
                scaleMult: 0.8f,
                timeLeft: 90);
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
