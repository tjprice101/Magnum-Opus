using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.FoundationWeapons.ExplosionParticlesFoundation
{
    /// <summary>
    /// SparkExplosionProjectile — The visual explosion that spawns a field of scattering
    /// elongated spark particles. Each spark is an internally-managed struct with its own
    /// position, velocity, rotation, scale, color, and lifetime.
    ///
    /// Three distinct spark patterns:
    ///   - Radial Scatter: Uniform outward burst of hot sparks, slight gravity, random sizes
    ///   - Fountain Cascade: Sparks erupt upward then arc down under gravity like a firework
    ///   - Spiral Shrapnel: Sparks spin outward with angular momentum in a spiral shape
    ///
    /// The projectile itself is invisible — all visuals are the spark particles it manages.
    /// It deals area damage to enemies at the impact center on the first frame.
    ///
    /// ai[0] = SparkMode index
    /// </summary>
    public class SparkExplosionProjectile : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.RainbowCrystalExplosion;

        // ---- CONFIGURATION ----
        private const int SparkCount = 55;            // Number of individual spark particles
        private const int MaxLifetime = 90;           // Total frames the explosion projectile lives
        private const float DamageRadius = 100f;      // Initial AoE damage radius

        // ---- STATE ----
        private int timer;
        private bool initialized;
        private bool damageDone;
        private SparkMode mode;
        private Spark[] sparks;

        // ---- SPARK PARTICLE STRUCT ----
        private struct Spark
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Rotation;
            public float RotationSpeed;
            public float Scale;
            public float ScaleDecay;
            public float Alpha;
            public float AlphaDecay;
            public float Length;        // Stretch factor for elongation
            public int ColorIndex;      // Which theme color to use
            public float GravityMult;   // Per-spark gravity multiplier
            public bool Active;
            public int SparkType;       // 0=line, 1=star, 2=dot (visual variety)
            public float Friction;      // Velocity decay per frame
        }

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DrawScreenCheckFluff[Projectile.type] = 800;
        }

        public override void SetDefaults()
        {
            Projectile.width = 2;
            Projectile.height = 2;
            Projectile.friendly = false; // We deal damage manually
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.timeLeft = MaxLifetime + 5;
            Projectile.alpha = 255;
        }

        public override void AI()
        {
            if (!initialized)
            {
                initialized = true;
                mode = (SparkMode)(int)Projectile.ai[0];
                sparks = new Spark[SparkCount];
                InitializeSparks();
            }

            timer++;
            Projectile.velocity = Vector2.Zero;

            // Deal AoE damage on first frame only
            if (!damageDone)
            {
                damageDone = true;
                DealAreaDamage();
            }

            // Update all spark particles
            UpdateSparks();

            // Lighting at the center (fading)
            Color[] colors = EPFTextures.GetModeColors(mode);
            float centerAlpha = MathHelper.Clamp(1f - timer / 20f, 0f, 1f);
            Lighting.AddLight(Projectile.Center, colors[1].ToVector3() * centerAlpha * 0.6f);

            // Kill when all sparks are dead or time runs out
            if (timer >= MaxLifetime)
                Projectile.Kill();
        }

        // =====================================================================
        // SPARK INITIALIZATION — Different patterns per mode
        // =====================================================================

        private void InitializeSparks()
        {
            switch (mode)
            {
                case SparkMode.RadialScatter:
                    InitRadialScatter();
                    break;
                case SparkMode.FountainCascade:
                    InitFountainCascade();
                    break;
                case SparkMode.SpiralShrapnel:
                    InitSpiralShrapnel();
                    break;
            }
        }

        /// <summary>
        /// Radial Scatter — Sparks burst outward uniformly in all directions.
        /// High speed, moderate gravity, random sizes. Like hot metal debris.
        /// </summary>
        private void InitRadialScatter()
        {
            for (int i = 0; i < SparkCount; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float speed = Main.rand.NextFloat(3f, 14f);

                // Some sparks are fast and small, some are slow and large
                bool isBigChunk = Main.rand.NextBool(5);

                sparks[i] = new Spark
                {
                    Position = Projectile.Center + Main.rand.NextVector2Circular(6f, 6f),
                    Velocity = angle.ToRotationVector2() * speed,
                    Rotation = angle,
                    RotationSpeed = Main.rand.NextFloat(-0.15f, 0.15f),
                    Scale = isBigChunk ? Main.rand.NextFloat(1.5f, 2.5f) : Main.rand.NextFloat(0.4f, 1.2f),
                    ScaleDecay = Main.rand.NextFloat(0.985f, 0.995f),
                    Alpha = 1f,
                    AlphaDecay = Main.rand.NextFloat(0.975f, 0.993f),
                    Length = isBigChunk ? Main.rand.NextFloat(1f, 2f) : Main.rand.NextFloat(1.5f, 4f),
                    ColorIndex = Main.rand.Next(4),
                    GravityMult = Main.rand.NextFloat(0.06f, 0.18f),
                    Active = true,
                    SparkType = isBigChunk ? 1 : (Main.rand.NextBool(3) ? 2 : 0),
                    Friction = Main.rand.NextFloat(0.97f, 0.995f),
                };
            }
        }

        /// <summary>
        /// Fountain Cascade — Sparks erupt upward in a cone then arc down
        /// under strong gravity, like a pyrotechnic fountain.
        /// </summary>
        private void InitFountainCascade()
        {
            for (int i = 0; i < SparkCount; i++)
            {
                // Upward cone with some randomness
                float angle = -MathHelper.PiOver2 + Main.rand.NextFloat(-0.8f, 0.8f);
                float speed = Main.rand.NextFloat(4f, 16f);

                bool isTrailer = Main.rand.NextBool(4);

                sparks[i] = new Spark
                {
                    Position = Projectile.Center + new Vector2(Main.rand.NextFloat(-15f, 15f), 0f),
                    Velocity = angle.ToRotationVector2() * speed,
                    Rotation = angle,
                    RotationSpeed = Main.rand.NextFloat(-0.1f, 0.1f),
                    Scale = isTrailer ? Main.rand.NextFloat(0.3f, 0.7f) : Main.rand.NextFloat(0.5f, 1.5f),
                    ScaleDecay = Main.rand.NextFloat(0.988f, 0.997f),
                    Alpha = 1f,
                    AlphaDecay = Main.rand.NextFloat(0.980f, 0.995f),
                    Length = Main.rand.NextFloat(1.5f, 3.5f),
                    ColorIndex = Main.rand.Next(4),
                    GravityMult = Main.rand.NextFloat(0.12f, 0.28f), // Strong gravity
                    Active = true,
                    SparkType = isTrailer ? 2 : (Main.rand.NextBool(4) ? 1 : 0),
                    Friction = Main.rand.NextFloat(0.985f, 0.998f),
                };
            }
        }

        /// <summary>
        /// Spiral Shrapnel — Sparks spin outward with angular momentum,
        /// creating a spiral pattern. Less gravity, more rotational movement.
        /// </summary>
        private void InitSpiralShrapnel()
        {
            for (int i = 0; i < SparkCount; i++)
            {
                // Distribute evenly around a spiral with perturbation
                float baseAngle = (MathHelper.TwoPi / SparkCount) * i;
                float angle = baseAngle + Main.rand.NextFloat(-0.3f, 0.3f);
                float speed = Main.rand.NextFloat(2f, 10f);

                // Tangential velocity component (perpendicular to radial)
                float tangentialSpeed = Main.rand.NextFloat(1f, 5f);
                float tangentialAngle = angle + MathHelper.PiOver2; // perpendicular
                Vector2 radialVel = angle.ToRotationVector2() * speed;
                Vector2 tangentialVel = tangentialAngle.ToRotationVector2() * tangentialSpeed;

                bool isCore = Main.rand.NextBool(6);

                sparks[i] = new Spark
                {
                    Position = Projectile.Center + Main.rand.NextVector2Circular(4f, 4f),
                    Velocity = radialVel + tangentialVel,
                    Rotation = (radialVel + tangentialVel).ToRotation(),
                    RotationSpeed = Main.rand.NextFloat(-0.25f, 0.25f),
                    Scale = isCore ? Main.rand.NextFloat(1.2f, 2.0f) : Main.rand.NextFloat(0.4f, 1.0f),
                    ScaleDecay = Main.rand.NextFloat(0.990f, 0.998f),
                    Alpha = 1f,
                    AlphaDecay = Main.rand.NextFloat(0.982f, 0.996f),
                    Length = Main.rand.NextFloat(1.5f, 3f),
                    ColorIndex = Main.rand.Next(4),
                    GravityMult = Main.rand.NextFloat(0.01f, 0.06f), // Very light gravity
                    Active = true,
                    SparkType = isCore ? 1 : (Main.rand.NextBool(3) ? 2 : 0),
                    Friction = Main.rand.NextFloat(0.980f, 0.995f),
                };
            }
        }

        // =====================================================================
        // SPARK UPDATE
        // =====================================================================

        private void UpdateSparks()
        {
            for (int i = 0; i < sparks.Length; i++)
            {
                if (!sparks[i].Active) continue;

                ref Spark s = ref sparks[i];

                // Apply gravity
                s.Velocity.Y += s.GravityMult;

                // Apply friction
                s.Velocity *= s.Friction;

                // Move
                s.Position += s.Velocity;

                // Rotate
                s.Rotation += s.RotationSpeed;

                // For line-type sparks, align rotation with velocity for natural look
                if (s.SparkType == 0 && s.Velocity.LengthSquared() > 1f)
                {
                    float targetRot = s.Velocity.ToRotation();
                    s.Rotation = MathHelper.Lerp(s.Rotation, targetRot, 0.3f);
                }

                // Fade and shrink
                s.Alpha *= s.AlphaDecay;
                s.Scale *= s.ScaleDecay;

                // Elongation decreases as spark slows
                float speedFactor = MathHelper.Clamp(s.Velocity.Length() / 8f, 0.3f, 1f);
                // (length stays the same, but rendering uses speedFactor to modulate stretch)

                // Light per spark
                if (s.Alpha > 0.15f)
                {
                    Color[] colors = EPFTextures.GetModeColors(mode);
                    Color c = colors[Math.Min(s.ColorIndex, colors.Length - 1)];
                    Lighting.AddLight(s.Position, c.ToVector3() * s.Alpha * 0.15f);
                }

                // Kill faded sparks
                if (s.Alpha < 0.02f || s.Scale < 0.05f)
                    s.Active = false;
            }
        }

        // =====================================================================
        // AREA DAMAGE
        // =====================================================================

        private void DealAreaDamage()
        {
            Player owner = Main.player[Projectile.owner];
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage || npc.immortal)
                    continue;

                float dist = Vector2.Distance(npc.Center, Projectile.Center);
                if (dist < DamageRadius)
                {
                    // Damage falloff by distance
                    float falloff = 1f - (dist / DamageRadius) * 0.5f;
                    int damage = (int)(Projectile.damage * falloff);
                    int dir = (npc.Center.X > Projectile.Center.X) ? 1 : -1;

                    npc.StrikeNPC(npc.CalculateHitInfo(damage, dir, false,
                        Projectile.knockBack, DamageClass.Melee, true));
                }
            }
        }

        // =====================================================================
        // RENDERING
        // =====================================================================

        public override bool PreDraw(ref Color lightColor)
        {
            if (sparks == null) return false;

            SpriteBatch sb = Main.spriteBatch;
            Color[] colors = EPFTextures.GetModeColors(mode);

            // ---- ADDITIVE PASS (all spark rendering) ----
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                SamplerState.LinearClamp, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

            // Draw central flash (first few frames)
            DrawCenterFlash(sb, colors);

            // Draw each spark
            DrawSparks(sb, colors);

            // ---- RESTORE ----
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

            return false;
        }

        /// <summary>
        /// Draws a bright flash at the explosion center for the first few frames.
        /// </summary>
        private void DrawCenterFlash(SpriteBatch sb, Color[] colors)
        {
            if (timer > 12) return;

            float flashProgress = timer / 12f;
            float flashAlpha = (1f - flashProgress * flashProgress);
            Vector2 center = Projectile.Center - Main.screenPosition;

            Texture2D softGlow = EPFTextures.SoftGlow.Value;
            Vector2 glowOrigin = softGlow.Size() / 2f;

            // Wide flash (SoftGlow 1024px — target ~100px)
            float wideFlashScale = 100f * (1f + flashProgress * 0.5f) / softGlow.Width;
            sb.Draw(softGlow, center, null, colors[2] * (flashAlpha * 0.6f),
                0f, glowOrigin, wideFlashScale, SpriteEffects.None, 0f);

            // Mid glow (~50px)
            float midGlowScale = 50f / softGlow.Width;
            sb.Draw(softGlow, center, null, colors[1] * (flashAlpha * 0.4f),
                0f, glowOrigin, midGlowScale, SpriteEffects.None, 0f);

            // Star burst (StarFlare 1024px — target ~60px)
            Texture2D starFlare = EPFTextures.StarFlare.Value;
            Vector2 flareOrigin = starFlare.Size() / 2f;
            float starBurstScale = 60f * (1f + flashProgress) / starFlare.Width;
            sb.Draw(starFlare, center, null, colors[2] * (flashAlpha * 0.5f),
                timer * 0.1f, flareOrigin, starBurstScale, SpriteEffects.None, 0f);

            // Lens flare at very start (LensFlare 1024px — target ~40px)
            if (timer < 4)
            {
                Texture2D lens = EPFTextures.LensFlare.Value;
                Vector2 lensOrigin = lens.Size() / 2f;
                float lensScale = 40f / lens.Width;
                sb.Draw(lens, center, null, colors[2] * (flashAlpha * 0.3f),
                    0f, lensOrigin, lensScale, SpriteEffects.None, 0f);
            }
        }

        /// <summary>
        /// Draws all active spark particles as elongated shapes.
        /// </summary>
        private void DrawSparks(SpriteBatch sb, Color[] colors)
        {
            Texture2D lineTex = EPFTextures.SolidWhiteLine.Value;
            Texture2D starTex = EPFTextures.Star4Hard.Value;
            Texture2D dotTex = EPFTextures.GlowOrb.Value;
            Texture2D softGlow = EPFTextures.SoftGlow.Value;
            Vector2 lineOrigin = new Vector2(lineTex.Width / 2f, lineTex.Height / 2f);
            Vector2 starOrigin = starTex.Size() / 2f;
            Vector2 dotOrigin = dotTex.Size() / 2f;
            Vector2 glowOrigin = softGlow.Size() / 2f;

            for (int i = 0; i < sparks.Length; i++)
            {
                if (!sparks[i].Active) continue;
                ref Spark s = ref sparks[i];

                Vector2 drawPos = s.Position - Main.screenPosition;
                Color baseColor = colors[Math.Min(s.ColorIndex, colors.Length - 1)];
                Color sparkColor = baseColor * s.Alpha;
                Color coreColor = colors[2] * (s.Alpha * 0.7f);

                // Speed factor affects elongation
                float speedFactor = MathHelper.Clamp(s.Velocity.Length() / 6f, 0.2f, 1f);

                switch (s.SparkType)
                {
                    case 0: // Elongated line spark (the primary scattered debris look)
                        {
                            // SolidWhiteLine is 2160px — use pixel-based sizing
                            // Target: max ~60px long, ~4px wide
                            float targetLenPx = s.Scale * s.Length * speedFactor * 6f;
                            float targetWidPx = MathHelper.Max(1f, s.Scale * 1.5f);
                            float stretchX = targetLenPx / lineTex.Width;
                            float stretchY = targetWidPx / lineTex.Height;

                            // Main spark body
                            sb.Draw(lineTex, drawPos, null, sparkColor,
                                s.Rotation, lineOrigin,
                                new Vector2(stretchX, stretchY),
                                SpriteEffects.None, 0f);

                            // Brighter core line (thinner)
                            sb.Draw(lineTex, drawPos, null, coreColor,
                                s.Rotation, lineOrigin,
                                new Vector2(stretchX * 0.7f, stretchY * 0.4f),
                                SpriteEffects.None, 0f);

                            // Small glow at spark head (SoftGlow 1024px — target ~12px)
                            float headGlowPx = s.Scale * 5f;
                            sb.Draw(softGlow, drawPos, null, sparkColor * 0.3f,
                                0f, glowOrigin, headGlowPx / softGlow.Width, SpriteEffects.None, 0f);
                        }
                        break;

                    case 1: // Star/chunk spark (bright 4-pointed star)
                        {
                            // Star4Hard is 32px — target ~10px on screen
                            float starScale = s.Scale * 0.15f;

                            // Star shape
                            sb.Draw(starTex, drawPos, null, sparkColor,
                                s.Rotation, starOrigin, starScale, SpriteEffects.None, 0f);

                            // Glow behind star (SoftGlow 1024px — target ~20px)
                            float starGlowPx = s.Scale * 8f;
                            sb.Draw(softGlow, drawPos, null, sparkColor * 0.4f,
                                0f, glowOrigin, starGlowPx / softGlow.Width, SpriteEffects.None, 0f);

                            // Core bloom (~10px)
                            float coreGlowPx = s.Scale * 4f;
                            sb.Draw(softGlow, drawPos, null, coreColor * 0.3f,
                                0f, glowOrigin, coreGlowPx / softGlow.Width, SpriteEffects.None, 0f);
                        }
                        break;

                    case 2: // Tiny dot spark (small glowing ember)
                        {
                            // GlowOrb is 1024px — target ~10px
                            float dotPx = MathHelper.Max(3f, s.Scale * 4f);
                            float dotScale = dotPx / dotTex.Width;

                            // Dot body
                            sb.Draw(dotTex, drawPos, null, sparkColor,
                                0f, dotOrigin, dotScale, SpriteEffects.None, 0f);

                            // Tiny glow (SoftGlow 1024px — target ~20px)
                            float dotGlowPx = MathHelper.Max(5f, s.Scale * 8f);
                            sb.Draw(softGlow, drawPos, null, sparkColor * 0.25f,
                                0f, glowOrigin, dotGlowPx / softGlow.Width, SpriteEffects.None, 0f);
                        }
                        break;
                }
            }
        }

        public override Color? GetAlpha(Color lightColor) => Color.Transparent;
    }
}
