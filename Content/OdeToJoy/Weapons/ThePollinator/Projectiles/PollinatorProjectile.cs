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
using MagnumOpus.Content.OdeToJoy.Weapons.ThePollinator.Utilities;

namespace MagnumOpus.Content.OdeToJoy.Weapons.ThePollinator.Projectiles
{
    /// <summary>
    /// Pollen cloud projectile for ThePollinator.
    /// BlackSwanFlareProj scaffold — homing sub-projectile with IncisorOrb rendering.
    /// </summary>
    public class PollinatorProjectile : ModProjectile
    {
        private const float HomingRange = 350f;
        private const float HomingStrength = 0.08f;
        private const float MaxSpeed = 16f;
        private Player Owner => Main.player[Projectile.owner];
        private bool _initialized;
        private VertexStrip _strip;

        // Spreading Contagion — per-NPC pollination tracking
        private static Dictionary<int, int> _pollinatedStacks = new Dictionary<int, int>();
        private static int _pollinatedTimer;

        // Harvest Season: 5 bloom kills → next 10 orbs get buffed
        private static int _bloomKillCount;
        private static int _harvestShotsRemaining;
        private bool _isHarvestOrb;

        public override string Texture => "MagnumOpus/Content/OdeToJoy/Weapons/ThePollinator/ThePollinator";

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
            Projectile.DamageType = DamageClass.Ranged;
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

                // Check if this orb spawns during Harvest Season
                if (_harvestShotsRemaining > 0)
                {
                    _isHarvestOrb = true;
                    _harvestShotsRemaining--;
                    Projectile.damage *= 2;
                    Projectile.scale *= 1.5f;
                }
            }

            // Homing strength: aggressive during Harvest Season
            float homingStr = _isHarvestOrb ? 0.12f : 0.04f;
            NPC target = Projectile.Center.ClosestNPCAt(HomingRange);
            if (target != null)
            {
                Vector2 desiredDir = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, desiredDir * Projectile.velocity.Length(), homingStr);
            }
            if (Projectile.velocity.Length() > MaxSpeed)
                Projectile.velocity = Vector2.Normalize(Projectile.velocity) * MaxSpeed;

            // Perpendicular pollen drift
            Projectile.ai[0] += 0.10f;
            Vector2 perp = new Vector2(-Projectile.velocity.Y, Projectile.velocity.X).SafeNormalize(Vector2.Zero);
            Projectile.position += perp * (float)Math.Sin(Projectile.ai[0]) * 2f;

            // Update pollination spreading and DoT
            UpdatePollinationSpread();

            Projectile.rotation = Projectile.velocity.ToRotation();

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

            float pulse = 1f + 0.15f * (float)Math.Sin(Projectile.timeLeft * 0.2f);
            Lighting.AddLight(Projectile.Center, new Vector3(0.4f, 0.55f, 0.2f) * 0.35f * pulse);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Apply Pollinated: 5 seconds (300 frames)
            _pollinatedStacks[target.whoAmI] = 300;

            // On-death spawn: if target is about to die, spawn 3 homing children toward other pollinated enemies
            if (target.life - damageDone <= 0)
            {
                // Track bloom kills for Harvest Season
                _bloomKillCount++;
                if (_bloomKillCount >= 5)
                {
                    _bloomKillCount = 0;
                    _harvestShotsRemaining = 10;
                }

                try
                {
                    for (int i = 0; i < 3; i++)
                    {
                        // Find a pollinated target to aim at
                        NPC childTarget = FindPollinatedTarget(target.whoAmI);
                        Vector2 vel;
                        if (childTarget != null)
                            vel = (childTarget.Center - target.Center).SafeNormalize(Vector2.UnitX) * 10f + Main.rand.NextVector2Circular(2f, 2f);
                        else
                            vel = Main.rand.NextVector2CircularEdge(8f, 8f);

                        GenericHomingOrbChild.SpawnChild(
                            Projectile.GetSource_FromThis(),
                            target.Center, vel,
                            Projectile.damage / 2, Projectile.knockBack, Projectile.owner,
                            homingStrength: 0.10f, behaviorFlags: 0,
                            themeIndex: GenericHomingOrbChild.THEME_ODETOJOY,
                            scaleMult: 0.8f, timeLeft: 120);
                    }
                }
                catch { }
            }

            // Existing VFX
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

        /// <summary>Find a pollinated NPC target (excluding the dying one).</summary>
        private NPC FindPollinatedTarget(int excludeWhoAmI)
        {
            NPC best = null;
            float bestDist = 600f;
            foreach (var kvp in _pollinatedStacks)
            {
                if (kvp.Key == excludeWhoAmI || kvp.Value <= 0) continue;
                if (kvp.Key < 0 || kvp.Key >= Main.maxNPCs) continue;
                NPC npc = Main.npc[kvp.Key];
                if (!npc.active || npc.life <= 0) continue;
                float dist = Vector2.Distance(Main.npc[excludeWhoAmI].Center, npc.Center);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    best = npc;
                }
            }
            return best;
        }

        /// <summary>Static pollination spreading + DoT. Called from AI.</summary>
        private static void UpdatePollinationSpread()
        {
            if (Main.GameUpdateCount % 60 != 0) return;

            // Decrement timers and collect active pollinated NPCs
            List<int> toRemove = new List<int>();
            List<int> activeKeys = new List<int>();
            foreach (var kvp in _pollinatedStacks)
            {
                int remaining = kvp.Value - 60;
                if (remaining <= 0)
                    toRemove.Add(kvp.Key);
                else
                    activeKeys.Add(kvp.Key);
            }
            foreach (int key in toRemove)
                _pollinatedStacks.Remove(key);
            foreach (int key in activeKeys)
                _pollinatedStacks[key] = _pollinatedStacks[key] - 60;

            // Spread + DoT for active pollinated NPCs
            List<int> newPollinated = new List<int>();
            foreach (int whoAmI in activeKeys)
            {
                if (whoAmI < 0 || whoAmI >= Main.maxNPCs) continue;
                NPC npc = Main.npc[whoAmI];
                if (!npc.active || npc.life <= 0) continue;

                // DoT: 1% max HP per second
                int dot = Math.Max(1, npc.lifeMax / 100);
                npc.life -= dot;
                if (npc.life <= 0)
                {
                    npc.life = 1;
                    npc.StrikeInstantKill();
                }
                npc.HitEffect();

                // Spread to nearby non-pollinated enemies within 100px
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC other = Main.npc[i];
                    if (!other.active || other.friendly || other.dontTakeDamage) continue;
                    if (_pollinatedStacks.ContainsKey(i)) continue;
                    if (newPollinated.Contains(i)) continue;
                    float dist = Vector2.Distance(npc.Center, other.Center);
                    if (dist < 100f)
                        newPollinated.Add(i);
                }
            }

            // Apply new pollinations
            foreach (int newKey in newPollinated)
                _pollinatedStacks[newKey] = 300; // 5 seconds
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
