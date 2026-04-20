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
using MagnumOpus.Content.OdeToJoy;
using MagnumOpus.Content.OdeToJoy.Weapons.ElysianVerdict.Utilities;

namespace MagnumOpus.Content.OdeToJoy.Weapons.ElysianVerdict.Projectiles
{
    /// <summary>
    /// Golden judgment swing projectile for ElysianVerdict.
    /// Homing sub-projectile with IncisorOrb rendering and Elysian Mark system.
    /// ai[0] = Paradise Lost flag (1 = active, 0 = normal)
    ///
    /// Mark System:
    ///   - Tier 1 (1 mark): +10% damage taken (visual cue)
    ///   - Tier 2 (2 marks): escalated damage visuals
    ///   - Tier 3 (3+ marks): Elysian Verdict detonation — AoE explosion, marks reset
    ///
    /// Paradise Lost (player HP < 25%): 2 marks per hit, aggressive homing, 2x scale.
    /// </summary>
    public class ElysianVerdictSwing : ModProjectile
    {
        private const float HomingRange = 350f;
        private const float HomingStrength = 0.06f;
        private const float ParadiseLostHomingStrength = 0.14f;
        private const float MaxSpeed = 16f;
        private Player Owner => Main.player[Projectile.owner];
        private bool _initialized;
        private VertexStrip _strip;

        // Static mark tracking: key = npc.whoAmI, value = (markCount, lastHitTime)
        private static readonly Dictionary<int, (int marks, double lastHit)> _markStacks = new();
        private const double MarkExpirySeconds = 10.0;

        public override string Texture => "MagnumOpus/Content/OdeToJoy/Weapons/ElysianVerdict/ElysianVerdict";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 16;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            if (!_initialized)
            {
                _initialized = true;
                Projectile.rotation = Projectile.velocity.ToRotation();
            }

            // Paradise Lost check: if owner HP < 25%, enhance
            bool paradiseLost = Projectile.ai[0] >= 1f;
            if (!paradiseLost && Owner.active && !Owner.dead)
            {
                float hpRatio = (float)Owner.statLife / Owner.statLifeMax2;
                if (hpRatio < 0.25f)
                {
                    paradiseLost = true;
                    Projectile.ai[0] = 1f;
                }
            }

            // Apply Paradise Lost enhancements
            float currentHoming = paradiseLost ? ParadiseLostHomingStrength : HomingStrength;
            if (paradiseLost)
                Projectile.scale = 2f;

            // Homing
            NPC target = Projectile.Center.ClosestNPCAt(HomingRange);
            if (target != null)
            {
                Vector2 desiredDir = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, desiredDir * Projectile.velocity.Length(), currentHoming);
            }
            if (Projectile.velocity.Length() > MaxSpeed)
                Projectile.velocity = Vector2.Normalize(Projectile.velocity) * MaxSpeed;

            Projectile.rotation = Projectile.velocity.ToRotation();

            // Dust trail
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

            // Paradise Lost extra particles
            if (paradiseLost && Main.rand.NextBool(2))
            {
                Color plColor = new Color(255, 200, 60);
                Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(10f, 10f),
                    DustID.GoldFlame, Main.rand.NextVector2Circular(1f, 1f), 0, plColor, 1.0f);
                d.noGravity = true;
            }

            float pulse = 1f + 0.15f * (float)Math.Sin(Projectile.timeLeft * 0.2f);
            float lightMult = paradiseLost ? 0.55f : 0.35f;
            Lighting.AddLight(Projectile.Center, new Vector3(0.4f, 0.55f, 0.2f) * lightMult * pulse);

            // Clean expired marks periodically
            if (Main.GameUpdateCount % 60 == 0)
                CleanExpiredMarks();
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Vector2 hitPos = target.Center;
            bool paradiseLost = Projectile.ai[0] >= 1f;
            int marksToAdd = paradiseLost ? 2 : 1;

            // Apply marks
            int npcKey = target.whoAmI;
            int currentMarks = 0;
            if (_markStacks.TryGetValue(npcKey, out var existing))
                currentMarks = existing.marks;

            currentMarks += marksToAdd;
            _markStacks[npcKey] = (currentMarks, Main.gameTimeCache.TotalGameTime.TotalSeconds);

            // Tier-based VFX scaling
            float vfxIntensity = 0.5f + currentMarks * 0.2f;

            // Base spark VFX (scales with marks)
            int sparkCount = 4 + currentMarks * 2;
            for (int i = 0; i < sparkCount; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2CircularEdge(4f + currentMarks, 4f + currentMarks);
                Color col = i % 2 == 0 ? new Color(90, 200, 60) : new Color(255, 210, 60);
                Dust d = Dust.NewDustPerfect(hitPos, DustID.GreenTorch, sparkVel, 0, col, 0.5f + currentMarks * 0.1f);
                d.noGravity = true;
            }
            for (int i = 0; i < 2; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(2f, 2f) + new Vector2(0, -1f);
                Dust d = Dust.NewDustPerfect(hitPos + Main.rand.NextVector2Circular(8f, 8f),
                    DustID.GoldFlame, vel, 0, new Color(255, 210, 60), 0.5f);
                d.noGravity = true;
            }

            try { OdeToJoyVFXLibrary.SpawnMusicNotes(hitPos, 1 + (currentMarks / 2), 12f, 0.4f, 0.7f, 20); } catch { }
            try { OdeToJoyVFXLibrary.SpawnMixedSparkleImpact(hitPos, vfxIntensity, 4 + currentMarks, 4 + currentMarks); } catch { }

            // Tier 3 detonation: 3+ marks triggers AoE explosion
            if (currentMarks >= 3)
            {
                // Reset marks
                _markStacks.Remove(npcKey);

                // Spawn AoE damage zone — pull + slow combo
                GenericDamageZone.SpawnZone(
                    Projectile.GetSource_FromThis(),
                    hitPos, Projectile.damage, Projectile.knockBack, Projectile.owner,
                    GenericDamageZone.FLAG_PULL | GenericDamageZone.FLAG_SLOW,
                    radius: 120f,
                    themeIndex: GenericHomingOrbChild.THEME_ODETOJOY,
                    durationFrames: 60);

                // Big detonation VFX
                try { OdeToJoyVFXLibrary.SpawnGardenExplosion(hitPos, 1.2f); } catch { }
                try { OdeToJoyVFXLibrary.SpawnJoyousSparkles(hitPos, 8, 30f); } catch { }
                try { OdeToJoyVFXLibrary.SpawnGardenBurst(hitPos, 12, 7f); } catch { }

                // Extra detonation dust
                for (int i = 0; i < 12; i++)
                {
                    float angle = MathHelper.TwoPi * i / 12f;
                    Vector2 burstVel = angle.ToRotationVector2() * Main.rand.NextFloat(5f, 9f);
                    Color col = i % 3 == 0 ? new Color(90, 200, 60) :
                                i % 3 == 1 ? new Color(255, 210, 60) :
                                new Color(255, 255, 255);
                    Dust d = Dust.NewDustPerfect(hitPos, DustID.GoldFlame, burstVel, 0, col, 1.2f);
                    d.noGravity = true;
                }
            }
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            // Bonus damage based on existing marks
            int npcKey = target.whoAmI;
            if (_markStacks.TryGetValue(npcKey, out var existing))
            {
                int marks = existing.marks;
                // +10% per mark tier (pre-detonation)
                float damageBonus = 1f + marks * 0.10f;
                modifiers.FinalDamage *= damageBonus;
            }
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

        /// <summary>Remove mark entries older than 10 seconds.</summary>
        private static void CleanExpiredMarks()
        {
            double now = Main.gameTimeCache.TotalGameTime.TotalSeconds;
            List<int> expired = null;
            foreach (var kvp in _markStacks)
            {
                if (now - kvp.Value.lastHit > MarkExpirySeconds)
                {
                    expired ??= new List<int>();
                    expired.Add(kvp.Key);
                }
            }
            if (expired != null)
            {
                foreach (int key in expired)
                    _markStacks.Remove(key);
            }
        }
    }
}
