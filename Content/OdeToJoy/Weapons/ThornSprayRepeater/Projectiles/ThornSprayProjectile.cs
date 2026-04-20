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
using MagnumOpus.Content.OdeToJoy.Weapons.ThornSprayRepeater.Utilities;

namespace MagnumOpus.Content.OdeToJoy.Weapons.ThornSprayRepeater.Projectiles
{
    /// <summary>
    /// Thorn bullet projectile for ThornSprayRepeater.
    /// BlackSwanFlareProj scaffold — homing sub-projectile with IncisorOrb rendering.
    /// </summary>
    public class ThornSprayProjectile : ModProjectile
    {
        private const float HomingRange = 350f;
        private const float MaxSpeed = 16f;
        private Player Owner => Main.player[Projectile.owner];
        private bool _initialized;
        private VertexStrip _strip;

        /// <summary>Per-NPC thorn stack tracking for detonation mechanic. Keyed by npc.whoAmI.</summary>
        private static readonly Dictionary<int, int> _thornStacks = new Dictionary<int, int>();

        /// <summary>Clear stacks when entering/leaving world to avoid stale data.</summary>
        public static void ClearThornStacks() => _thornStacks.Clear();

        public override string Texture => "MagnumOpus/Content/OdeToJoy/Weapons/ThornSprayRepeater/ThornSprayRepeater";

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
            bool isBloomReload = Projectile.ai[1] == 1f;

            if (!_initialized)
            {
                _initialized = true;
                Projectile.rotation = Projectile.velocity.ToRotation();

                // Bloom Reload: scale up
                if (isBloomReload)
                    Projectile.scale = 1.3f;
            }

            // Bloom Reload: apply homing
            if (isBloomReload)
            {
                NPC target = Projectile.Center.ClosestNPCAt(HomingRange);
                if (target != null)
                {
                    Vector2 desiredDir = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, desiredDir * Projectile.velocity.Length(), 0.08f);
                }
                if (Projectile.velocity.Length() > MaxSpeed)
                    Projectile.velocity = Vector2.Normalize(Projectile.velocity) * MaxSpeed;
            }

            // Straight Shot: no homing for normal shots
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
            // Thorn stack tracking — increment per hit on this NPC
            int npcId = target.whoAmI;
            if (!_thornStacks.ContainsKey(npcId))
                _thornStacks[npcId] = 0;
            _thornStacks[npcId]++;

            // At 25 stacks: detonate all thorns on that NPC
            if (_thornStacks[npcId] >= 25)
            {
                _thornStacks[npcId] = 0;

                // Spawn AoE damage zone at target
                try
                {
                    GenericDamageZone.SpawnZone(
                        Projectile.GetSource_FromThis(),
                        target.Center, Projectile.damage, Projectile.knockBack, Projectile.owner,
                        GenericDamageZone.FLAG_SLOW, 120f,
                        GenericHomingOrbChild.THEME_ODETOJOY,
                        durationFrames: 90);
                }
                catch { }

                // Splash damage to nearby enemies within 80px of detonation
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (!npc.active || npc.friendly || npc.dontTakeDamage || npc.whoAmI == npcId) continue;
                    float dist = Vector2.Distance(target.Center, npc.Center);
                    if (dist < 80f)
                    {
                        // Apply splash hit
                        Player owner = Main.player[Projectile.owner];
                        owner.ApplyDamageToNPC(npc, Projectile.damage / 2, 0f, 0, false);
                    }
                }

                // Detonation VFX burst
                if (!Main.dedServ)
                {
                    for (int i = 0; i < 12; i++)
                    {
                        Vector2 sparkVel = Main.rand.NextVector2CircularEdge(6f, 6f);
                        Color col = i % 2 == 0 ? new Color(90, 200, 60) : new Color(255, 210, 60);
                        Dust d = Dust.NewDustPerfect(target.Center, DustID.GreenTorch, sparkVel, 0, col, 1.0f);
                        d.noGravity = true;
                    }
                    try { OdeToJoyVFXLibrary.SpawnMusicNotes(target.Center, 3, 20f, 0.6f, 1.2f, 30); } catch { }
                    try { OdeToJoyVFXLibrary.SpawnMixedSparkleImpact(target.Center, 1.0f, 8, 6); } catch { }
                    try { OdeToJoyVFXLibrary.SpawnJoyousSparkles(target.Center, 6, 20f); } catch { }
                }
            }

            // Normal hit VFX
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
