using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.Nachtmusik.Debuffs;
using MagnumOpus.Common;

namespace MagnumOpus.Content.Nachtmusik.Weapons.StarweaversGrimoire.Projectiles
{
    /// <summary>
    /// Starweaver Node — starts as homing orb, becomes stationary node on hit or timeout.
    /// Nodes tether to other nodes within 300px, damaging enemies crossing the tether.
    /// ai[0]: 0 = moving, 1 = stationary node
    /// ai[1]: stationary timer (counts up)
    /// </summary>
    public class StarweaverNodeProjectile : ModProjectile
    {
        private const float TetherRange = 300f;
        private const int MaxNodeDuration = 180; // 3 seconds as node
        private const int MaxNodes = 8;

        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom";

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 30;
        }

        public override void AI()
        {
            bool isNode = Projectile.ai[0] == 1f;

            if (!isNode)
            {
                // Moving phase: gentle homing
                NPC target = FindClosestNPC(600f);
                if (target != null)
                {
                    Vector2 idealDir = (target.Center - Projectile.Center).SafeNormalize(Projectile.velocity.SafeNormalize(Vector2.UnitX));
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, idealDir * Projectile.velocity.Length(), 0.08f);
                }

                Projectile.rotation = Projectile.velocity.ToRotation();

                // Become node when close to timeout
                if (Projectile.timeLeft < 270)
                {
                    BecomeNode();
                }

                // Trail dust
                if (Main.rand.NextBool(3))
                {
                    Color dustCol = NachtmusikPalette.RadianceGold;
                    Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.WhiteTorch,
                        -Projectile.velocity * 0.1f, 0, dustCol, 0.6f);
                    d.noGravity = true;
                }
            }
            else
            {
                // Stationary node phase
                Projectile.velocity = Vector2.Zero;
                Projectile.ai[1]++;

                if (Projectile.ai[1] >= MaxNodeDuration)
                {
                    Projectile.Kill();
                    return;
                }

                // Tether damage: check enemies crossing lines to other nodes
                if ((int)Projectile.ai[1] % 10 == 0)
                {
                    DealTetherDamage();
                }

                // Node idle dust — golden glow
                if (Main.rand.NextBool(4))
                {
                    float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                    Vector2 dustPos = Projectile.Center + angle.ToRotationVector2() * Main.rand.NextFloat(6f);
                    Dust d = Dust.NewDustPerfect(dustPos, DustID.WhiteTorch,
                        Vector2.Zero, 0, NachtmusikPalette.StarGold, 0.4f);
                    d.noGravity = true;
                }

                // Pulsing light
                float pulse = 0.6f + 0.4f * MathF.Sin((float)Main.timeForVisualEffects * 0.06f);
                Lighting.AddLight(Projectile.Center, new Vector3(0.35f, 0.28f, 0.1f) * pulse);
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Apply CelestialHarmony
            target.AddBuff(ModContent.BuffType<CelestialHarmony>(), 600);
            target.GetGlobalNPC<CelestialHarmonyNPC>().AddStack(target, 1);

            // Become stationary node if still moving
            if (Projectile.ai[0] == 0f)
                BecomeNode();
        }

        private void BecomeNode()
        {
            Projectile.ai[0] = 1f;
            Projectile.ai[1] = 0f;
            Projectile.velocity = Vector2.Zero;
            Projectile.timeLeft = MaxNodeDuration + 10;
            Projectile.tileCollide = false;

            // Enforce max nodes
            int nodeCount = 0;
            int oldestIdx = -1;
            float oldestTime = 0;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (p.active && p.owner == Projectile.owner && p.type == Projectile.type && p.ai[0] == 1f && p.whoAmI != Projectile.whoAmI)
                {
                    nodeCount++;
                    if (p.ai[1] > oldestTime)
                    {
                        oldestTime = p.ai[1];
                        oldestIdx = i;
                    }
                }
            }
            if (nodeCount >= MaxNodes && oldestIdx >= 0)
                Main.projectile[oldestIdx].Kill();

            // Node birth VFX
            for (int i = 0; i < 6; i++)
            {
                Color col = i % 2 == 0 ? NachtmusikPalette.RadianceGold : NachtmusikPalette.StarlitBlue;
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.WhiteTorch,
                    Main.rand.NextVector2CircularEdge(3f, 3f), 0, col, 0.7f);
                d.noGravity = true;
            }
        }

        private void DealTetherDamage()
        {
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile other = Main.projectile[i];
                if (!other.active || other.owner != Projectile.owner || other.type != Projectile.type
                    || other.ai[0] != 1f || other.whoAmI == Projectile.whoAmI)
                    continue;

                float dist = Vector2.Distance(Projectile.Center, other.Center);
                if (dist > TetherRange || dist < 10f) continue;

                // Check each NPC against the tether line
                for (int n = 0; n < Main.maxNPCs; n++)
                {
                    NPC npc = Main.npc[n];
                    if (!npc.active || npc.friendly || npc.dontTakeDamage) continue;

                    float lineDist = DistanceToLine(npc.Center, Projectile.Center, other.Center);
                    if (lineDist < 20f + npc.width * 0.5f)
                    {
                        npc.SimpleStrikeNPC(Projectile.damage / 3, 0, false, 0f, null, false, 0, false);
                        npc.AddBuff(ModContent.BuffType<CelestialHarmony>(), 300);
                        npc.GetGlobalNPC<CelestialHarmonyNPC>().AddStack(npc, 1);
                    }
                }
            }
        }

        private static float DistanceToLine(Vector2 point, Vector2 lineA, Vector2 lineB)
        {
            Vector2 ab = lineB - lineA;
            float t = MathHelper.Clamp(Vector2.Dot(point - lineA, ab) / ab.LengthSquared(), 0f, 1f);
            Vector2 closest = lineA + ab * t;
            return Vector2.Distance(point, closest);
        }

        private NPC FindClosestNPC(float maxDist)
        {
            NPC closest = null;
            float closestDist = maxDist;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.CanBeChasedBy()) continue;
                float dist = Vector2.Distance(Projectile.Center, npc.Center);
                if (dist < closestDist) { closestDist = dist; closest = npc; }
            }
            return closest;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (Projectile.ai[0] != 1f) return true; // Moving: use normal sprite

            SpriteBatch sb = Main.spriteBatch;
            try
            {
                Vector2 drawPos = Projectile.Center - Main.screenPosition;
                float pulse = 0.8f + 0.2f * MathF.Sin((float)Main.timeForVisualEffects * 0.05f);
                float nodeAlpha = Projectile.ai[1] < MaxNodeDuration - 30 ? 1f : (MaxNodeDuration - Projectile.ai[1]) / 30f;

                sb.End();
                sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                    Main.DefaultSamplerState, DepthStencilState.None,
                    RasterizerState.CullCounterClockwise, null,
                    Main.GameViewMatrix.TransformationMatrix);

                Texture2D glow = ModContent.Request<Texture2D>(
                    "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom",
                    ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
                Vector2 origin = glow.Size() / 2f;

                // Core golden glow
                sb.Draw(glow, drawPos, null, NachtmusikPalette.RadianceGold * (0.6f * nodeAlpha), 0f, origin, 0.3f * pulse, SpriteEffects.None, 0f);
                // Outer purple halo
                sb.Draw(glow, drawPos, null, NachtmusikPalette.CosmicPurple * (0.2f * nodeAlpha), 0f, origin, 0.8f * pulse, SpriteEffects.None, 0f);

                // Draw tethers to nearby nodes
                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    Projectile other = Main.projectile[i];
                    if (!other.active || other.owner != Projectile.owner || other.type != Projectile.type
                        || other.ai[0] != 1f || other.whoAmI == Projectile.whoAmI)
                        continue;

                    float dist = Vector2.Distance(Projectile.Center, other.Center);
                    if (dist > TetherRange) continue;

                    // Simple tether line using glow texture
                    Vector2 otherDraw = other.Center - Main.screenPosition;
                    int steps = (int)(dist / 10f);
                    for (int s = 0; s < steps; s++)
                    {
                        float t = s / (float)steps;
                        Vector2 linePos = Vector2.Lerp(drawPos, otherDraw, t);
                        sb.Draw(glow, linePos, null, NachtmusikPalette.StarGold * (0.15f * nodeAlpha * pulse),
                            0f, origin, 0.08f, SpriteEffects.None, 0f);
                    }
                }
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
            if (Main.dedServ) return;
            for (int i = 0; i < 6; i++)
            {
                Color col = i % 2 == 0 ? NachtmusikPalette.RadianceGold : NachtmusikPalette.CosmicPurple;
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.WhiteTorch,
                    Main.rand.NextVector2CircularEdge(3f, 3f), 0, col, 0.5f);
                d.noGravity = true;
            }
        }
    }
}
