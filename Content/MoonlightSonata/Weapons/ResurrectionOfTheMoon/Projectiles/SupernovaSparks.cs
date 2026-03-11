using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.FoundationWeapons.ExplosionParticlesFoundation;
using MagnumOpus.Common.Systems.VFX.Sparkle;

namespace MagnumOpus.Content.MoonlightSonata.Weapons.ResurrectionOfTheMoon.Projectiles
{
    /// <summary>
    /// SupernovaSparks — Foundation-based ExplosionParticlesFoundation (RadialScatter mode)
    /// adapted for the Resurrection of the Moon's supernova detonation.
    ///
    /// Spawns 55 structurally-managed spark particles in a radial burst with lunar colors:
    ///   Deep purple core → ice blue arcs → white-hot tips
    ///
    /// Three spark types (line/star/dot) with gravity, friction, rotation.
    /// Additive center flash bloom with SoftGlow + StarFlare + LensFlare stacking.
    /// VFX-only projectile: friendly=false, 0 damage.
    ///
    /// ai[0] = lunar phase multiplier (0.7–1.3) scaling spark count and speed
    /// </summary>
    public class SupernovaSparks : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.RainbowCrystalExplosion;

        // ---- CONFIGURATION ----
        private const int BaseSparkCount = 55;
        private const int MaxLifetime = 90;

        // ---- STATE ----
        private int timer;
        private bool initialized;
        private Spark[] sparks;
        private int actualSparkCount;

        // ---- LUNAR PALETTE ----
        private static readonly Color[] LunarSparkColors = new[]
        {
            new Color(50, 20, 100),   // Deep Space Violet (pianissimo)
            new Color(100, 80, 200),  // Impact Crater (piano)
            new Color(180, 120, 255), // Comet Trail (mezzo)
            new Color(120, 190, 255), // Lunar Shine (forte)
            new Color(210, 225, 255), // Comet Core White (fortissimo)
        };

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
            public float Length;
            public int ColorIndex;
            public float GravityMult;
            public bool Active;
            public int SparkType; // 0=line, 1=star, 2=dot
            public float Friction;
        }

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DrawScreenCheckFluff[Projectile.type] = 800;
        }

        public override void SetDefaults()
        {
            Projectile.width = 2;
            Projectile.height = 2;
            Projectile.friendly = false;
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
                float lunarMult = MathHelper.Clamp(Projectile.ai[0], 0.5f, 2f);
                actualSparkCount = (int)(BaseSparkCount * lunarMult);
                sparks = new Spark[actualSparkCount];
                InitRadialScatter(lunarMult);
            }

            timer++;
            Projectile.velocity = Vector2.Zero;

            UpdateSparks();

            // Center glow light
            float centerAlpha = MathHelper.Clamp(1f - timer / 20f, 0f, 1f);
            Lighting.AddLight(Projectile.Center, LunarSparkColors[3].ToVector3() * centerAlpha * 0.6f);

            if (timer >= MaxLifetime)
                Projectile.Kill();
        }

        private void InitRadialScatter(float lunarMult)
        {
            for (int i = 0; i < actualSparkCount; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float speed = Main.rand.NextFloat(3f, 14f) * MathHelper.Lerp(0.8f, 1.2f, lunarMult - 0.5f);
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
                    ColorIndex = Main.rand.Next(LunarSparkColors.Length),
                    GravityMult = Main.rand.NextFloat(0.06f, 0.18f),
                    Active = true,
                    SparkType = isBigChunk ? 1 : (Main.rand.NextBool(3) ? 2 : 0),
                    Friction = Main.rand.NextFloat(0.97f, 0.995f),
                };
            }
        }

        private void UpdateSparks()
        {
            for (int i = 0; i < sparks.Length; i++)
            {
                if (!sparks[i].Active) continue;
                ref Spark s = ref sparks[i];

                s.Velocity.Y += s.GravityMult;
                s.Velocity *= s.Friction;
                s.Position += s.Velocity;
                s.Rotation += s.RotationSpeed;

                // Line sparks align to velocity
                if (s.SparkType == 0 && s.Velocity.LengthSquared() > 1f)
                {
                    float targetRot = s.Velocity.ToRotation();
                    s.Rotation = MathHelper.Lerp(s.Rotation, targetRot, 0.3f);
                }

                s.Alpha *= s.AlphaDecay;
                s.Scale *= s.ScaleDecay;

                if (s.Alpha > 0.15f)
                {
                    Color c = LunarSparkColors[Math.Min(s.ColorIndex, LunarSparkColors.Length - 1)];
                    Lighting.AddLight(s.Position, c.ToVector3() * s.Alpha * 0.12f);
                }

                if (s.Alpha < 0.02f || s.Scale < 0.05f)
                    s.Active = false;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (sparks == null) return false;

            SpriteBatch sb = Main.spriteBatch;
            try
            {

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                SamplerState.LinearClamp, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

            DrawCenterFlash(sb);
            DrawSparks(sb);

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

        private void DrawCenterFlash(SpriteBatch sb)
        {
            if (timer > 12) return;

            float flashProgress = timer / 12f;
            float flashAlpha = 1f - flashProgress * flashProgress;
            float time = (float)Main.timeForVisualEffects;

            // Lunar sparkle impact - replaces 4-layer SoftGlow/StarFlare/LensFlare stacking
            Color[] lunarColors = new Color[] {
                new Color(40, 10, 60),      // NightPurple
                new Color(75, 0, 130),      // DarkPurple
                new Color(138, 43, 226),    // Violet
                new Color(135, 206, 250),   // IceBlue
                new Color(240, 235, 255),   // MoonWhite
            };
            float impactRadius = 45f * (1f + flashProgress * 0.5f);
            int impactCount = 10 + (int)(flashProgress * 4f);
            SparkleBloomHelper.DrawSparkleImpact(sb, Projectile.Center, SparkleTheme.MoonlightSonata,
                lunarColors, flashAlpha, impactRadius, impactCount, time,
                seed: Projectile.identity * 0.91f, sparkleScale: 0.04f);
        }

        private void DrawSparks(SpriteBatch sb)
        {
            Texture2D lineTex = EPFTextures.SolidWhiteLine.Value;
            Texture2D starTex = EPFTextures.Star4Hard.Value;
            Texture2D dotTex = EPFTextures.GlowOrb.Value;
            Vector2 lineOrigin = new Vector2(lineTex.Width / 2f, lineTex.Height / 2f);
            Vector2 starOrigin = starTex.Size() / 2f;
            Vector2 dotOrigin = dotTex.Size() / 2f;
            float time = (float)Main.timeForVisualEffects;

            for (int i = 0; i < sparks.Length; i++)
            {
                if (!sparks[i].Active) continue;
                ref Spark s = ref sparks[i];

                Vector2 drawPos = s.Position - Main.screenPosition;
                Color baseColor = LunarSparkColors[Math.Min(s.ColorIndex, LunarSparkColors.Length - 1)];
                Color sparkColor = baseColor * s.Alpha;
                Color coreColor = LunarSparkColors[4] * (s.Alpha * 0.7f);

                float speedFactor = MathHelper.Clamp(s.Velocity.Length() / 6f, 0.2f, 1f);

                switch (s.SparkType)
                {
                    case 0: // Elongated line spark
                    {
                        float targetLenPx = s.Scale * s.Length * speedFactor * 6f;
                        float targetWidPx = MathHelper.Max(1f, s.Scale * 1.5f);
                        float stretchX = targetLenPx / lineTex.Width;
                        float stretchY = targetWidPx / lineTex.Height;

                        sb.Draw(lineTex, drawPos, null, sparkColor,
                            s.Rotation, lineOrigin, new Vector2(stretchX, stretchY),
                            SpriteEffects.None, 0f);
                        sb.Draw(lineTex, drawPos, null, coreColor,
                            s.Rotation, lineOrigin, new Vector2(stretchX * 0.7f, stretchY * 0.4f),
                            SpriteEffects.None, 0f);

                        // Sparkle twinkle at head instead of SoftGlow blob
                        Color[] lunarSpkCols = new Color[] { baseColor, LunarSparkColors[4] };
                        SparkleBloomHelper.DrawSparkleBloom(sb, s.Position, SparkleTheme.MoonlightSonata,
                            lunarSpkCols, s.Alpha * 0.5f, s.Scale * 4f, 2, time,
                            seed: i * 0.37f, sparkleScale: 0.015f);
                        break;
                    }
                    case 1: // Star chunk
                    {
                        float starScale = s.Scale * 0.15f;
                        sb.Draw(starTex, drawPos, null, sparkColor,
                            s.Rotation, starOrigin, starScale, SpriteEffects.None, 0f);

                        // Sparkle twinkle at star instead of SoftGlow×2 stack
                        Color[] starSpkCols = new Color[] { baseColor, LunarSparkColors[4], new Color(135, 206, 250) };
                        SparkleBloomHelper.DrawSparkleBloom(sb, s.Position, SparkleTheme.MoonlightSonata,
                            starSpkCols, s.Alpha * 0.5f, s.Scale * 6f, 3, time,
                            seed: i * 0.59f + 0.5f, sparkleScale: 0.018f);
                        break;
                    }
                    case 2: // Glowing dot
                    {
                        float dotPx = MathHelper.Max(3f, s.Scale * 4f);
                        float dotScale = dotPx / dotTex.Width;
                        sb.Draw(dotTex, drawPos, null, sparkColor,
                            0f, dotOrigin, dotScale, SpriteEffects.None, 0f);

                        // Single small sparkle instead of SoftGlow blob
                        Color[] dotSpkCols = new Color[] { baseColor, LunarSparkColors[4] };
                        SparkleBloomHelper.DrawSparkleBloom(sb, s.Position, SparkleTheme.MoonlightSonata,
                            dotSpkCols, s.Alpha * 0.3f, s.Scale * 4f, 1, time,
                            seed: i * 0.83f + 1f, sparkleScale: 0.012f);
                        break;
                    }
                }
            }
        }

        public override Color? GetAlpha(Color lightColor) => Color.Transparent;
    }
}
