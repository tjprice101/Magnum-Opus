using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.FoundationWeapons.AttackFoundation
{
    /// <summary>
    /// AstralgraphProjectile — Mode 3: Magic weapon that summons an astralgraph
    /// (a celestial star-connected graph) around the player.
    ///
    /// Behaviour:
    /// - Creates a ring of orbiting star nodes around the player
    /// - Nodes are connected by glowing lines forming a star polygon
    /// - The astralgraph expands outward, dealing damage to enemies it touches
    /// - After reaching full size, it pulses once then contracts and fades
    /// - Enemies within the astralgraph take continuous damage
    ///
    /// ai[0] = mode index (always 2 for Astralgraph)
    /// </summary>
    public class AstralgraphProjectile : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.RainbowCrystalExplosion;

        // ---- CONFIGURATION ----
        private const int NodeCount = 7;              // Number of star vertices
        private const int StarSkip = 3;               // Connect every Nth node to form star polygon
        private const float MaxRadius = 200f;         // Full expansion radius
        private const float MinRadius = 20f;          // Starting radius
        private const int ExpandFrames = 25;          // Frames to expand
        private const int HoldFrames = 40;            // Frames at full size
        private const int ContractFrames = 20;        // Frames to contract and fade
        private const int TotalFrames = ExpandFrames + HoldFrames + ContractFrames;
        private const float DamageRadius = 30f;       // Damage radius per node
        private const float OrbitSpeed = 0.015f;      // Rotation speed of the star

        // ---- STATE ----
        private int timer;
        private float currentRadius;
        private float orbitAngle;
        private float overallAlpha;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DrawScreenCheckFluff[Projectile.type] = 600;
        }

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.timeLeft = TotalFrames + 5;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
            Projectile.alpha = 255; // Invisible base sprite
        }

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            if (!owner.active || owner.dead)
            {
                Projectile.Kill();
                return;
            }

            timer++;

            // Follow the player
            Projectile.Center = owner.Center;
            Projectile.velocity = Vector2.Zero;

            // Orbit rotation
            orbitAngle += OrbitSpeed;

            Color[] colors = AFTextures.GetModeColors(AttackMode.Astralgraph);

            if (timer <= ExpandFrames)
            {
                // Expanding phase
                float progress = timer / (float)ExpandFrames;
                float eased = EaseOutBack(progress);
                currentRadius = MathHelper.Lerp(MinRadius, MaxRadius, eased);
                overallAlpha = MathHelper.Clamp(progress * 2f, 0f, 1f);
            }
            else if (timer <= ExpandFrames + HoldFrames)
            {
                // Hold phase — gentle radius pulse
                int holdTimer = timer - ExpandFrames;
                float pulse = MathF.Sin(holdTimer * 0.08f) * 10f;
                currentRadius = MaxRadius + pulse;
                overallAlpha = 1f;
            }
            else
            {
                // Contract phase
                int contractTimer = timer - ExpandFrames - HoldFrames;
                float progress = contractTimer / (float)ContractFrames;
                float eased = EaseInCubic(progress);
                currentRadius = MathHelper.Lerp(MaxRadius, 0f, eased);
                overallAlpha = 1f - eased;
            }

            // Deal damage at each node position
            if (timer % 8 == 0 && overallAlpha > 0.3f)
            {
                for (int n = 0; n < NodeCount; n++)
                {
                    float nodeAngle = orbitAngle + (MathHelper.TwoPi / NodeCount) * n;
                    Vector2 nodePos = owner.Center + nodeAngle.ToRotationVector2() * currentRadius;
                    DealNodeDamage(nodePos, owner);
                }
            }

            // Spawn dust particles at node positions
            if (Main.rand.NextBool(3) && overallAlpha > 0.1f)
            {
                int randomNode = Main.rand.Next(NodeCount);
                float nodeAngle = orbitAngle + (MathHelper.TwoPi / NodeCount) * randomNode;
                Vector2 nodePos = owner.Center + nodeAngle.ToRotationVector2() * currentRadius;

                Color dustColor = colors[Main.rand.Next(colors.Length)];
                Dust dust = Dust.NewDustPerfect(
                    nodePos + Main.rand.NextVector2Circular(8f, 8f),
                    DustID.RainbowMk2,
                    Main.rand.NextVector2Circular(1.5f, 1.5f),
                    newColor: dustColor,
                    Scale: Main.rand.NextFloat(0.3f, 0.6f));
                dust.noGravity = true;
                dust.fadeIn = 0.4f;
            }

            // Lighting from center
            Lighting.AddLight(owner.Center, colors[1].ToVector3() * 0.4f * overallAlpha);

            if (timer >= TotalFrames)
                Projectile.Kill();
        }

        private void DealNodeDamage(Vector2 nodePos, Player owner)
        {
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage || npc.immortal)
                    continue;

                if (Vector2.Distance(npc.Center, nodePos) < DamageRadius)
                {
                    int dir = (npc.Center.X > owner.Center.X) ? 1 : -1;
                    npc.StrikeNPC(npc.CalculateHitInfo(Projectile.damage, dir, false,
                        Projectile.knockBack, Projectile.DamageType, true));

                    // Hit VFX
                    Color[] colors = AFTextures.GetModeColors(AttackMode.Astralgraph);
                    for (int d = 0; d < 6; d++)
                    {
                        Vector2 vel = Main.rand.NextVector2Circular(4f, 4f);
                        Dust dust = Dust.NewDustPerfect(npc.Center, DustID.RainbowMk2, vel,
                            newColor: colors[2], Scale: Main.rand.NextFloat(0.3f, 0.6f));
                        dust.noGravity = true;
                    }
                }
            }
        }

        // =====================================================================
        // RENDERING
        // =====================================================================

        public override bool PreDraw(ref Color lightColor)
        {
            if (overallAlpha < 0.01f)
                return false;

            SpriteBatch sb = Main.spriteBatch;
            try
            {
            Player owner = Main.player[Projectile.owner];
            Color[] colors = AFTextures.GetModeColors(AttackMode.Astralgraph);
            Vector2 center = owner.Center - Main.screenPosition;

            float time = (float)Main.timeForVisualEffects;

            // ---- ADDITIVE PASS ----
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

            // Compute node positions
            Vector2[] nodeScreenPos = new Vector2[NodeCount];
            for (int n = 0; n < NodeCount; n++)
            {
                float nodeAngle = orbitAngle + (MathHelper.TwoPi / NodeCount) * n;
                nodeScreenPos[n] = center + nodeAngle.ToRotationVector2() * currentRadius;
            }

            // Draw star polygon connections (connect every StarSkip-th node)
            Texture2D softGlow = AFTextures.SoftGlow.Value;
            Vector2 glowOrigin = softGlow.Size() / 2f;

            for (int n = 0; n < NodeCount; n++)
            {
                int target = (n + StarSkip) % NodeCount;
                Vector2 from = nodeScreenPos[n];
                Vector2 to = nodeScreenPos[target];
                DrawGlowLine(sb, softGlow, glowOrigin, from, to, colors[1] * (overallAlpha * 0.4f), 0.008f);
            }

            // Draw outer circle ring connections (adjacent nodes)
            for (int n = 0; n < NodeCount; n++)
            {
                int next = (n + 1) % NodeCount;
                Vector2 from = nodeScreenPos[n];
                Vector2 to = nodeScreenPos[next];
                DrawGlowLine(sb, softGlow, glowOrigin, from, to, colors[0] * (overallAlpha * 0.2f), 0.005f);
            }

            // Draw node blooms
            Texture2D pointBloom = AFTextures.PointBloom.Value;
            Vector2 pbOrigin = pointBloom.Size() / 2f;
            Texture2D starFlare = AFTextures.StarFlare.Value;
            Vector2 flareOrigin = starFlare.Size() / 2f;

            for (int n = 0; n < NodeCount; n++)
            {
                float nodePulse = 0.8f + 0.2f * MathF.Sin(time * 0.06f + n * 1.2f);

                // Outer node glow (SoftGlow 1024px — target ~30px)
                sb.Draw(softGlow, nodeScreenPos[n], null,
                    colors[0] * (overallAlpha * 0.3f * nodePulse),
                    0f, glowOrigin, 0.03f * nodePulse, SpriteEffects.None, 0f);

                // Node point bloom (PointBloom 2160px — target ~20px)
                sb.Draw(pointBloom, nodeScreenPos[n], null,
                    colors[2] * (overallAlpha * 0.6f * nodePulse),
                    0f, pbOrigin, 0.01f * nodePulse, SpriteEffects.None, 0f);

                // Node star flare (StarFlare 1024px — target ~20px)
                float flareRot = time * 0.02f + n * MathHelper.PiOver4;
                sb.Draw(starFlare, nodeScreenPos[n], null,
                    colors[1] * (overallAlpha * 0.2f),
                    flareRot, flareOrigin, 0.02f, SpriteEffects.None, 0f);

                // Light at each node
                Vector2 worldPos = owner.Center + (orbitAngle + (MathHelper.TwoPi / NodeCount) * n).ToRotationVector2() * currentRadius;
                Lighting.AddLight(worldPos, colors[1].ToVector3() * 0.3f * overallAlpha);
            }

            // Center glow (SoftGlow — target ~80px)
            sb.Draw(softGlow, center, null,
                colors[0] * (overallAlpha * 0.25f),
                0f, glowOrigin, 0.08f, SpriteEffects.None, 0f);

            // Center star flare (StarFlare — target ~40px)
            sb.Draw(starFlare, center, null,
                colors[2] * (overallAlpha * 0.15f),
                time * 0.01f, flareOrigin, 0.04f, SpriteEffects.None, 0f);

            // Lines from center to each node (radial spokes)
            for (int n = 0; n < NodeCount; n++)
            {
                DrawGlowLine(sb, softGlow, glowOrigin, center, nodeScreenPos[n],
                    colors[0] * (overallAlpha * 0.1f), 0.003f);
            }

            // ---- RESTORE ----
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

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

        /// <summary>
        /// Draws a glow line between two screen-space points using a stretched SoftGlow texture.
        /// </summary>
        private void DrawGlowLine(SpriteBatch sb, Texture2D tex, Vector2 origin,
            Vector2 from, Vector2 to, Color color, float width)
        {
            Vector2 diff = to - from;
            float length = diff.Length();
            float angle = diff.ToRotation();
            float scaleX = length / tex.Width;

            sb.Draw(tex, from, null, color, angle, new Vector2(0, tex.Height / 2f),
                new Vector2(scaleX, width), SpriteEffects.None, 0f);
        }

        // ---- EASING ----
        private static float EaseOutBack(float t)
        {
            float c1 = 1.70158f;
            float c3 = c1 + 1f;
            return 1f + c3 * MathF.Pow(t - 1f, 3f) + c1 * MathF.Pow(t - 1f, 2f);
        }

        private static float EaseInCubic(float t) => t * t * t;

        public override Color? GetAlpha(Color lightColor) => Color.Transparent;
    }
}
