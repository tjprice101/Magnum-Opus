using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Core;
using MagnumOpus.Content.OdeToJoy;
using MagnumOpus.Content.OdeToJoy.Weapons.TriumphantChorus.Utilities;

namespace MagnumOpus.Content.OdeToJoy.Weapons.TriumphantChorus.Projectiles
{
    /// <summary>
    /// Triumphant Chorus minion for TriumphantChorus.
    /// BlackSwanFlareProj scaffold — minion with IncisorOrb rendering + homing AI.
    /// </summary>
    public class TriumphantChorusMinion : ModProjectile
    {
        private const float HomingRange = 700f;
        private Player Owner => Main.player[Projectile.owner];
        private bool _initialized;
        private VertexStrip _strip;

        // Four-Voice Ensemble attack system
        private int _attackTimer;
        private int _syncTimer;

        public override string Texture => "MagnumOpus/Content/OdeToJoy/Weapons/TriumphantChorus/TriumphantChorus";

        public override void SetStaticDefaults()
        {
            Main.projPet[Type] = true;
            ProjectileID.Sets.MinionSacrificable[Type] = true;
            ProjectileID.Sets.MinionTargettingFeature[Type] = true;
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
            Projectile.minionSlots = 2f;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }

        public override bool? CanCutTiles() => false;
        public override bool MinionContactDamage() => false;

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];

            if (!CheckActive(owner))
                return;

            if (!_initialized)
            {
                _initialized = true;
            }

            SearchForTargets(owner, out bool foundTarget, out float distanceFromTarget, out Vector2 targetCenter);
            Movement(foundTarget, distanceFromTarget, targetCenter, owner);

            // Four-Voice attack system
            _attackTimer++;
            _syncTimer++;

            int voiceType = (int)Projectile.ai[0]; // 0=Soprano, 1=Alto, 2=Tenor, 3=Bass
            bool harmonyBonus = CheckHarmonyBonus(owner);
            float damageMultiplier = harmonyBonus ? 1.15f : 1f;

            if (foundTarget)
            {
                int fireInterval = GetFireInterval(voiceType);
                bool syncFire = _syncTimer >= 600; // Every 10 seconds, synchronized fire

                if (_attackTimer >= fireInterval || syncFire)
                {
                    _attackTimer = 0;
                    if (syncFire)
                        _syncTimer = 0;

                    try
                    {
                        FireVoiceOrb(voiceType, targetCenter, damageMultiplier);
                    }
                    catch { }
                }
            }

            Projectile.rotation = Projectile.velocity.ToRotation();

            // Trail dust
            if (Main.rand.NextBool(3))
            {
                int dustType = Main.rand.NextBool() ? DustID.GreenTorch : DustID.GoldFlame;
                Color dustColor = Main.rand.NextBool() ? new Color(90, 200, 60) : new Color(255, 210, 60);
                Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(6f, 6f),
                    dustType, -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(0.5f, 0.5f),
                    0, dustColor, 0.8f);
                d.noGravity = true;
                d.fadeIn = 0.6f;
            }

            float pulse = 1f + 0.15f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * 3f);
            Lighting.AddLight(Projectile.Center, new Vector3(0.4f, 0.55f, 0.2f) * 0.35f * pulse);
        }

        /// <summary>Get fire interval for each voice type.</summary>
        private int GetFireInterval(int voiceType)
        {
            return voiceType switch
            {
                0 => 60,  // Soprano: every 60 frames
                1 => 50,  // Alto: every 50 frames
                2 => 40,  // Tenor: every 40 frames
                3 => 70,  // Bass: every 70 frames
                _ => 60,
            };
        }

        /// <summary>Fire a voice-specific orb toward the target.</summary>
        private void FireVoiceOrb(int voiceType, Vector2 targetCenter, float damageMultiplier)
        {
            Vector2 toTarget = (targetCenter - Projectile.Center).SafeNormalize(Vector2.UnitX);
            int damage = (int)(Projectile.damage * damageMultiplier);

            switch (voiceType)
            {
                case 0: // Soprano: high arc with gravity
                {
                    Vector2 vel = toTarget * 12f + new Vector2(0, -6f);
                    GenericHomingOrbChild.SpawnChild(
                        Projectile.GetSource_FromThis(), Projectile.Center, vel,
                        damage, Projectile.knockBack, Projectile.owner,
                        homingStrength: 0.04f, behaviorFlags: GenericHomingOrbChild.FLAG_GRAVITY,
                        themeIndex: GenericHomingOrbChild.THEME_ODETOJOY,
                        scaleMult: 0.9f, timeLeft: 120);
                    break;
                }
                case 1: // Alto: sine-wave wobble
                {
                    Vector2 vel = toTarget * 12f;
                    GenericHomingOrbChild.SpawnChild(
                        Projectile.GetSource_FromThis(), Projectile.Center, vel,
                        damage, Projectile.knockBack, Projectile.owner,
                        homingStrength: 0.06f, behaviorFlags: GenericHomingOrbChild.FLAG_SINEWAVE,
                        themeIndex: GenericHomingOrbChild.THEME_ODETOJOY,
                        scaleMult: 0.85f, timeLeft: 110);
                    break;
                }
                case 2: // Tenor: fast straight shot, no homing
                {
                    Vector2 vel = toTarget * 18f;
                    GenericHomingOrbChild.SpawnChild(
                        Projectile.GetSource_FromThis(), Projectile.Center, vel,
                        damage, Projectile.knockBack, Projectile.owner,
                        homingStrength: 0f, behaviorFlags: 0,
                        themeIndex: GenericHomingOrbChild.THEME_ODETOJOY,
                        scaleMult: 0.75f, timeLeft: 80);
                    break;
                }
                case 3: // Bass: slow, heavy homing, piercing
                {
                    Vector2 vel = toTarget * 8f;
                    GenericHomingOrbChild.SpawnChild(
                        Projectile.GetSource_FromThis(), Projectile.Center, vel,
                        damage, Projectile.knockBack, Projectile.owner,
                        homingStrength: 0.12f, behaviorFlags: GenericHomingOrbChild.FLAG_PIERCE,
                        themeIndex: GenericHomingOrbChild.THEME_ODETOJOY,
                        scaleMult: 1.1f, timeLeft: 150);
                    break;
                }
            }
        }

        /// <summary>Check if all 4 voice types are active for Harmony Bonus.</summary>
        private bool CheckHarmonyBonus(Player owner)
        {
            bool[] voices = new bool[4];
            int minionType = Projectile.type;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (p.active && p.owner == owner.whoAmI && p.type == minionType)
                {
                    int v = (int)p.ai[0];
                    if (v >= 0 && v < 4)
                        voices[v] = true;
                }
            }
            return voices[0] && voices[1] && voices[2] && voices[3];
        }

        private bool CheckActive(Player owner)
        {
            if (owner.dead || !owner.active)
            {
                owner.ClearBuff(ModContent.BuffType<Buffs.TriumphantChorusBuff>());
                Projectile.Kill();
                return false;
            }

            if (owner.HasBuff(ModContent.BuffType<Buffs.TriumphantChorusBuff>()))
                Projectile.timeLeft = 2;

            return true;
        }

        private void SearchForTargets(Player owner, out bool foundTarget, out float distanceFromTarget, out Vector2 targetCenter)
        {
            distanceFromTarget = HomingRange;
            targetCenter = Projectile.position;
            foundTarget = false;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.CanBeChasedBy())
                {
                    float dist = Vector2.Distance(npc.Center, Projectile.Center);
                    if (dist < distanceFromTarget)
                    {
                        distanceFromTarget = dist;
                        targetCenter = npc.Center;
                        foundTarget = true;
                    }
                }
            }
        }

        private void Movement(bool foundTarget, float distanceFromTarget, Vector2 targetCenter, Player owner)
        {
            float speed = 8f;
            float inertia = 20f;

            if (foundTarget)
            {
                Vector2 direction = targetCenter - Projectile.Center;
                direction.Normalize();
                direction *= speed;
                Projectile.velocity = (Projectile.velocity * (inertia - 1) + direction) / inertia;
            }
            else
            {
                float distToOwner = Vector2.Distance(owner.Center, Projectile.Center);
                if (distToOwner > 600f)
                {
                    Projectile.Center = owner.Center;
                }
                else if (distToOwner > 200f)
                {
                    Vector2 direction = owner.Center - Projectile.Center;
                    direction.Normalize();
                    direction *= speed;
                    Projectile.velocity = (Projectile.velocity * (inertia - 1) + direction) / inertia;
                }
                else
                {
                    Projectile.velocity *= 0.95f;
                }
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Vector2 hitPos = target.Center;
            for (int i = 0; i < 6; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2CircularEdge(4f, 4f);
                Color col = i % 2 == 0 ? new Color(90, 200, 60) : new Color(255, 210, 60);
                Dust d = Dust.NewDustPerfect(hitPos, DustID.GreenTorch, sparkVel, 0, col, 0.5f);
                d.noGravity = true;
            }
            for (int i = 0; i < 2; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(2f, 2f) + new Vector2(0, -1f);
                Dust d = Dust.NewDustPerfect(hitPos + Main.rand.NextVector2Circular(8f, 8f),
                    DustID.GoldFlame, vel, 0, new Color(255, 210, 60), 0.5f);
                d.noGravity = true;
            }
            try { OdeToJoyVFXLibrary.SpawnMusicNotes(hitPos, 1, 12f, 0.4f, 0.7f, 20); } catch { }
            try { OdeToJoyVFXLibrary.SpawnMixedSparkleImpact(hitPos, 0.6f, 4, 4); } catch { }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            try
            {
                IncisorOrbRenderer.DrawOrbVisuals(sb, Projectile, IncisorOrbRenderer.OdeToJoy, ref _strip);
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

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 4; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2CircularEdge(3f, 3f);
                Color col = Main.rand.NextBool() ? new Color(90, 200, 60) : new Color(255, 210, 60);
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.GreenTorch, sparkVel, 0, col, 0.3f);
                d.noGravity = true;
            }
            try { OdeToJoyVFXLibrary.SpawnMusicNotes(Projectile.Center, 1, 12f, 0.5f, 0.7f, 20); } catch { }
            try { OdeToJoyVFXLibrary.SpawnMixedSparkleImpact(Projectile.Center, 0.5f, 4, 4); } catch { }
            try { OdeToJoyVFXLibrary.SpawnJoyousSparkles(Projectile.Center, 3, 15f); } catch { }
        }
    }
}
