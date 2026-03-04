using MagnumOpus.Common.Systems.VFX.Core;
using MagnumOpus.Content.DiesIrae;
using MagnumOpus.Content.DiesIrae.Weapons.ChainOfJudgment.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.VFX;

namespace MagnumOpus.Content.DiesIrae.Weapons.ChainOfJudgment.Projectiles
{
    /// <summary>
    /// Chain Lightning Arc  Efast electrical arc that bounces between enemies.
    /// Spawned when the Chain of Judgment reaches 5 hit combo.
    /// Arcs to up to 3 targets, each bounce dimmer. 25-frame lifetime.
    /// Rendered as a segmented lightning bolt with ember-gold coloring.
    /// </summary>
    public class ChainLightningArc : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

        private const int MaxBounces = 3;
        private const float BounceRange = 400f;
        private const int Lifetime = 25;

        /// <summary>Target NPC whoAmI, set via ai[0].</summary>
        private int TargetNPC => (int)Projectile.ai[0];

        /// <summary>Bounce count, incremented per arc.</summary>
        private ref float BounceCount => ref Projectile.ai[1];

        private List<Vector2> arcPoints = new List<Vector2>();
        private bool arcBuilt;
        private HashSet<int> hitNPCs = new HashSet<int>();
        private float alphaFade = 1f;

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = Lifetime;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1; // hit each NPC once
        }

        public override void AI()
        {
            if (!arcBuilt)
            {
                BuildArcPath();
                arcBuilt = true;
            }

            // Fade out over lifetime
            float progress = 1f - (Projectile.timeLeft / (float)Lifetime);
            alphaFade = MathHelper.Lerp(1f, 0f, progress);

            // Spawn ember dust along arc
            if (arcPoints.Count >= 2 && Main.rand.NextBool(2))
            {
                int idx = Main.rand.Next(arcPoints.Count);
                Vector2 dustPos = arcPoints[idx] + Main.rand.NextVector2Circular(4, 4);
                Dust d = Dust.NewDustPerfect(dustPos, DustID.Torch,
                    Main.rand.NextVector2Circular(1f, 1f), 0,
                    DiesIraePalette.EmberOrange, 0.7f);
                d.noGravity = true;
            }
        }

        private void BuildArcPath()
        {
            arcPoints.Clear();
            arcPoints.Add(Projectile.Center);

            Vector2 currentPos = Projectile.Center;
            int targetIdx = TargetNPC;
            hitNPCs.Add(targetIdx); // don't re-hit the source

            for (int bounce = 0; bounce < MaxBounces; bounce++)
            {
                // Find next closest enemy
                int nextTarget = FindNextTarget(currentPos);
                if (nextTarget < 0) break;

                Vector2 targetPos = Main.npc[nextTarget].Center;
                // Add intermediate jagged points for lightning look
                AddLightningSegments(currentPos, targetPos);
                arcPoints.Add(targetPos);

                hitNPCs.Add(nextTarget);
                currentPos = targetPos;

                // Deal damage to the target
                if (Projectile.owner == Main.myPlayer)
                {
                    NPC npc = Main.npc[nextTarget];
                    int dmg = (int)(Projectile.damage * Math.Pow(0.85, bounce));
                    Player owner = Main.player[Projectile.owner];
                    owner.ApplyDamageToNPC(npc, dmg, 0f, Projectile.Center.X < npc.Center.X ? 1 : -1, false);
                }

                // Impact spark
                ChainOfJudgmentUtils.DoChainImpact(Main.npc[nextTarget].Center, Vector2.Zero);
            }
        }

        private int FindNextTarget(Vector2 from)
        {
            int best = -1;
            float bestDist = BounceRange;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage) continue;
                if (hitNPCs.Contains(i)) continue;

                float dist = Vector2.Distance(from, npc.Center);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    best = i;
                }
            }
            return best;
        }

        private void AddLightningSegments(Vector2 from, Vector2 to)
        {
            float dist = Vector2.Distance(from, to);
            int segments = Math.Max(2, (int)(dist / 30f));

            for (int i = 1; i < segments; i++)
            {
                float t = i / (float)segments;
                Vector2 mid = Vector2.Lerp(from, to, t);

                // Perpendicular offset for jagged lightning
                Vector2 dir = to - from;
                dir.Normalize();
                Vector2 perp = new Vector2(-dir.Y, dir.X);
                float offset = Main.rand.NextFloat(-12f, 12f) * (1f - Math.Abs(t - 0.5f) * 2f);
                mid += perp * offset;

                arcPoints.Add(mid);
            }
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => false;

        public override bool PreDraw(ref Color lightColor)
        {
            if (arcPoints.Count < 2) return false;

            SpriteBatch sb = Main.spriteBatch;
            Texture2D glow = MagnumTextureRegistry.GetSoftGlow();
            if (glow == null) return false;
            Vector2 glowOrigin = glow.Size() / 2f;

            // Begin additive
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);

            // Draw line segments between arc points
            for (int i = 0; i < arcPoints.Count - 1; i++)
            {
                Vector2 a = arcPoints[i];
                Vector2 b = arcPoints[i + 1];
                float segDist = Vector2.Distance(a, b);
                int pointCount = Math.Max(2, (int)(segDist / 6f));

                for (int j = 0; j <= pointCount; j++)
                {
                    float t = j / (float)pointCount;
                    Vector2 pos = Vector2.Lerp(a, b, t) - Main.screenPosition;

                    float fadeMult = alphaFade * (1f - (float)i / arcPoints.Count * 0.5f);

                    // Wide outer glow  Eember
                    sb.Draw(glow, pos, null, DiesIraePalette.EmberOrange * 0.3f * fadeMult,
                        0f, glowOrigin, 0.025f, SpriteEffects.None, 0f);
                    // Mid  Ewhite-hot
                    sb.Draw(glow, pos, null, DiesIraePalette.WrathWhite * 0.5f * fadeMult,
                        0f, glowOrigin, 0.012f, SpriteEffects.None, 0f);
                    // Core  Egold
                    sb.Draw(glow, pos, null, DiesIraePalette.JudgmentGold * 0.7f * fadeMult,
                        0f, glowOrigin, 0.006f, SpriteEffects.None, 0f);
                }
            }

            // Junction flares at each arc point
            for (int i = 0; i < arcPoints.Count; i++)
            {
                Vector2 pos = arcPoints[i] - Main.screenPosition;
                float brightness = (i == 0 || i == arcPoints.Count - 1) ? 0.6f : 0.4f;
                brightness *= alphaFade;

                sb.Draw(glow, pos, null, DiesIraePalette.JudgmentGold * brightness,
                    0f, glowOrigin, 0.04f, SpriteEffects.None, 0f);
                sb.Draw(glow, pos, null, DiesIraePalette.WrathWhite * brightness * 0.6f,
                    0f, glowOrigin, 0.02f, SpriteEffects.None, 0f);
            }

            // Restore
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);

            return false;
        }
    }
}