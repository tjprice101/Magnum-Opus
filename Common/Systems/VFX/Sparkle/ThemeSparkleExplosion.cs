using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Common.Systems.VFX.Sparkle
{
    /// <summary>
    /// ThemeSparkleExplosion — A dazzling 4-point star sparkle explosion that detonates
    /// on weapon/projectile contact with enemies or tiles. Each theme gets a unique,
    /// visually stunning burst of twinkling sparkles with theme-specific colors and behavior.
    ///
    /// Spawned automatically by ThemeSparkleGlobalProjectile for ALL theme weapon projectiles.
    ///
    /// Architecture mirrors Foundation4PointSparkle/SparkleStarExplosion:
    /// - Internally managed sparkle particle array (no Dust system overhead)
    /// - 3-layer rendering per sparkle: glow backdrop + star body + core flash
    /// - Center flash with starburst + expanding ring + cross flare
    /// - Secondary bloom motes for magical dust feel
    /// - All drawn in TrueAdditive blend mode (no black backgrounds)
    ///
    /// ai[0] = ThemeID (0=Moonlight, 1=Eroica, 2=SwanLake, 3=LaCampanella, 4=Enigma, 5=Fate)
    /// ai[1] = Intensity multiplier (0.5 = small, 1.0 = normal, 1.5 = large)
    /// </summary>
    public class ThemeSparkleExplosion : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.RainbowCrystalExplosion;

        // ---- CONFIGURATION ----
        private const int MaxLifetime = 80;
        private const float DamageRadius = 0f; // No damage — pure VFX

        // ---- STATE ----
        private int timer;
        private bool initialized;
        private ExplosionSparkle[] sparkles;
        private BloomMote[] motes;
        private float seed;
        private Color[] themeColors;
        private int sparkleCount;
        private int moteCount;
        private SparkleTheme theme;
        private float intensity;

        // ---- SPARKLE PARTICLE ----
        private struct ExplosionSparkle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Rotation;
            public float RotationSpeed;
            public float Scale;
            public float ScaleDecay;
            public float Alpha;
            public float AlphaDecay;
            public float FlashPhase;
            public float FlashSpeed;
            public float FlashPower;
            public int TextureType;
            public int ColorIndex;
            public float Friction;
            public float GravityMult;
            public bool Active;
        }

        // ---- BLOOM MOTE ----
        private struct BloomMote
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Scale;
            public float Alpha;
            public float AlphaDecay;
            public int ColorIndex;
            public bool Active;
        }

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DrawScreenCheckFluff[Projectile.type] = 600;
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
                seed = Main.rand.NextFloat(100f);
                theme = (SparkleTheme)(int)MathHelper.Clamp(Projectile.ai[0], 0, 5);
                intensity = MathHelper.Clamp(Projectile.ai[1], 0.3f, 2f);
                if (intensity == 0f) intensity = 1f;

                themeColors = ThemeSparkleColors.GetColors(theme);
                sparkleCount = (int)(45 * intensity);
                moteCount = (int)(20 * intensity);

                sparkles = new ExplosionSparkle[sparkleCount];
                motes = new BloomMote[moteCount];
                InitializeSparkles();
                InitializeMotes();
            }

            timer++;
            Projectile.velocity = Vector2.Zero;

            UpdateSparkles();
            UpdateMotes();

            // Center lighting
            if (themeColors != null && themeColors.Length > 2)
            {
                float centerAlpha = MathHelper.Clamp(1f - timer / 20f, 0f, 1f);
                Lighting.AddLight(Projectile.Center, themeColors[2].ToVector3() * centerAlpha * 0.5f);
            }

            if (timer >= MaxLifetime)
                Projectile.Kill();
        }

        // =====================================================================
        // SPARKLE INITIALIZATION
        // =====================================================================

        private void InitializeSparkles()
        {
            for (int i = 0; i < sparkleCount; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float speed = Main.rand.NextFloat(1.2f, 8f) * intensity;

                int texType;
                float roll = Main.rand.NextFloat();
                if (roll < 0.45f)
                    texType = 0; // 4PointStar
                else if (roll < 0.70f)
                    texType = 1; // BrightStar1
                else if (roll < 0.93f)
                    texType = 2; // BrightStar2
                else
                    texType = 3; // 8-Point Starburst

                float baseScale = texType switch
                {
                    3 => Main.rand.NextFloat(0.03f, 0.06f) * intensity,
                    1 => Main.rand.NextFloat(0.02f, 0.04f) * intensity,
                    0 => Main.rand.NextFloat(0.015f, 0.035f) * intensity,
                    _ => Main.rand.NextFloat(0.012f, 0.028f) * intensity,
                };

                float flashPow = texType switch
                {
                    3 => Main.rand.NextFloat(2f, 3f),
                    _ => Main.rand.NextFloat(3f, 6f),
                };

                // Theme-specific behavior tweaks
                float friction = theme switch
                {
                    SparkleTheme.MoonlightSonata => Main.rand.NextFloat(0.955f, 0.985f), // Slower, more lingering
                    SparkleTheme.Eroica => Main.rand.NextFloat(0.970f, 0.992f), // Faster, dramatic
                    SparkleTheme.SwanLake => Main.rand.NextFloat(0.960f, 0.990f), // Graceful drift
                    SparkleTheme.LaCampanella => Main.rand.NextFloat(0.975f, 0.995f), // Hot, persistent
                    SparkleTheme.EnigmaVariations => Main.rand.NextFloat(0.950f, 0.980f), // Chaotic scatter
                    SparkleTheme.Fate => Main.rand.NextFloat(0.965f, 0.990f), // Cosmic drift
                    SparkleTheme.Spring => Main.rand.NextFloat(0.962f, 0.988f), // Floating petals
                    SparkleTheme.Summer => Main.rand.NextFloat(0.972f, 0.994f), // Fast hot streaks
                    SparkleTheme.Autumn => Main.rand.NextFloat(0.958f, 0.986f), // Drifting leaves
                    SparkleTheme.Winter => Main.rand.NextFloat(0.948f, 0.978f), // Crisp, icy scatter
                    SparkleTheme.Seasons => Main.rand.NextFloat(0.960f, 0.990f), // Balanced four-season blend
                    _ => Main.rand.NextFloat(0.965f, 0.990f),
                };

                float gravity = theme switch
                {
                    SparkleTheme.MoonlightSonata => Main.rand.NextFloat(0.01f, 0.04f), // Light, floaty
                    SparkleTheme.Eroica => Main.rand.NextFloat(0.03f, 0.08f), // Falling petals
                    SparkleTheme.SwanLake => Main.rand.NextFloat(0.005f, 0.025f), // Feather-light
                    SparkleTheme.LaCampanella => Main.rand.NextFloat(0.04f, 0.10f), // Rising embers
                    SparkleTheme.EnigmaVariations => Main.rand.NextFloat(-0.03f, 0.05f), // Anti-gravity chaos
                    SparkleTheme.Fate => Main.rand.NextFloat(0.01f, 0.03f), // Zero-g cosmic
                    SparkleTheme.Spring => Main.rand.NextFloat(0.00f, 0.03f), // Petal-like float
                    SparkleTheme.Summer => Main.rand.NextFloat(0.03f, 0.09f), // Fiery falloff
                    SparkleTheme.Autumn => Main.rand.NextFloat(0.02f, 0.06f), // Leaf drop
                    SparkleTheme.Winter => Main.rand.NextFloat(-0.01f, 0.02f), // Snow-like hover
                    SparkleTheme.Seasons => Main.rand.NextFloat(0.00f, 0.05f), // Mixed seasonal motion
                    _ => Main.rand.NextFloat(0.02f, 0.06f),
                };

                sparkles[i] = new ExplosionSparkle
                {
                    Position = Projectile.Center + Main.rand.NextVector2Circular(6f, 6f),
                    Velocity = angle.ToRotationVector2() * speed,
                    Rotation = Main.rand.NextFloat(MathHelper.TwoPi),
                    RotationSpeed = Main.rand.NextFloat(-0.10f, 0.10f),
                    Scale = baseScale,
                    ScaleDecay = Main.rand.NextFloat(0.993f, 0.999f),
                    Alpha = 1f,
                    AlphaDecay = Main.rand.NextFloat(0.978f, 0.995f),
                    FlashPhase = Main.rand.NextFloat(MathHelper.TwoPi),
                    FlashSpeed = Main.rand.NextFloat(0.08f, 0.22f),
                    FlashPower = flashPow,
                    TextureType = texType,
                    ColorIndex = Main.rand.Next(themeColors.Length),
                    Friction = friction,
                    GravityMult = gravity,
                    Active = true,
                };
            }
        }

        private void InitializeMotes()
        {
            for (int i = 0; i < moteCount; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float speed = Main.rand.NextFloat(0.8f, 5f) * intensity;

                motes[i] = new BloomMote
                {
                    Position = Projectile.Center + Main.rand.NextVector2Circular(5f, 5f),
                    Velocity = angle.ToRotationVector2() * speed,
                    Scale = Main.rand.NextFloat(0.004f, 0.015f) * intensity,
                    Alpha = Main.rand.NextFloat(0.5f, 1f),
                    AlphaDecay = Main.rand.NextFloat(0.972f, 0.992f),
                    ColorIndex = Main.rand.Next(themeColors.Length),
                    Active = true,
                };
            }
        }

        // =====================================================================
        // UPDATE
        // =====================================================================

        private void UpdateSparkles()
        {
            for (int i = 0; i < sparkles.Length; i++)
            {
                if (!sparkles[i].Active) continue;
                ref ExplosionSparkle s = ref sparkles[i];

                // La Campanella: embers rise instead of fall
                if (theme == SparkleTheme.LaCampanella)
                    s.Velocity.Y -= s.GravityMult;
                else
                    s.Velocity.Y += s.GravityMult;

                s.Velocity *= s.Friction;
                s.Position += s.Velocity;
                s.Rotation += s.RotationSpeed;
                s.Alpha *= s.AlphaDecay;
                s.Scale *= s.ScaleDecay;

                // Enigma: slight chaotic jitter
                if (theme == SparkleTheme.EnigmaVariations && Main.rand.NextBool(3))
                    s.Velocity += Main.rand.NextVector2Circular(0.15f, 0.15f);

                // Per-sparkle lighting
                if (s.Alpha > 0.2f && themeColors != null)
                {
                    Color c = themeColors[Math.Min(s.ColorIndex, themeColors.Length - 1)];
                    float flashNow = MathF.Max(0f, MathF.Sin((float)Main.timeForVisualEffects * s.FlashSpeed + s.FlashPhase));
                    flashNow = MathF.Pow(flashNow, s.FlashPower);
                    Lighting.AddLight(s.Position, c.ToVector3() * s.Alpha * flashNow * 0.15f);
                }

                if (s.Alpha < 0.02f || s.Scale < 0.003f)
                    s.Active = false;
            }
        }

        private void UpdateMotes()
        {
            for (int i = 0; i < motes.Length; i++)
            {
                if (!motes[i].Active) continue;
                ref BloomMote m = ref motes[i];

                m.Velocity *= 0.97f;
                if (theme != SparkleTheme.LaCampanella)
                    m.Velocity.Y += 0.015f;
                else
                    m.Velocity.Y -= 0.02f; // Rising embers

                m.Position += m.Velocity;
                m.Alpha *= m.AlphaDecay;

                if (m.Alpha < 0.02f)
                    m.Active = false;
            }
        }

        // =====================================================================
        // RENDERING
        // =====================================================================

        public override bool PreDraw(ref Color lightColor)
        {
            if (sparkles == null || themeColors == null) return false;

            SpriteBatch sb = Main.spriteBatch;
            try
            {
            float time = (float)Main.timeForVisualEffects;

            // ---- ADDITIVE PASS ----
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                SamplerState.LinearClamp, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

            DrawCenterFlash(sb, time);
            DrawBloomMotes(sb);
            DrawSparkleField(sb, time);

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

        // =====================================================================
        // LAYER 1: CENTER FLASH
        // =====================================================================

        private void DrawCenterFlash(SpriteBatch sb, float time)
        {
            if (timer > 15) return;

            float flashProgress = timer / 15f;
            float flashAlpha = 1f - flashProgress * flashProgress;
            Vector2 center = Projectile.Center - Main.screenPosition;

            var softGlow = SparkleBloomHelper.SoftGlow;
            var starburst = SparkleBloomHelper.StarburstFlare;
            var ring = SparkleBloomHelper.PointBloom; // Use PointBloom for ring effect
            var starFlare = SparkleBloomHelper.StarFlare;

            Vector2 glowOrigin = softGlow.Size() / 2f;
            Vector2 burstOrigin = starburst.Size() / 2f;
            Vector2 ringOrigin = ring.Size() / 2f;
            Vector2 starOrigin = starFlare.Size() / 2f;

            // 1. Wide ambient glow (small, capped)
            float bloomScale = (0.06f + flashProgress * 0.10f) * intensity;
            bloomScale = MathHelper.Min(bloomScale, 0.15f);
            sb.Draw(softGlow, center, null, themeColors[0] * (flashAlpha * 0.4f),
                0f, glowOrigin, bloomScale, SpriteEffects.None, 0f);

            // 2. Warm inner glow
            sb.Draw(softGlow, center, null, themeColors[1] * (flashAlpha * 0.3f),
                0f, glowOrigin, bloomScale * 0.5f, SpriteEffects.None, 0f);

            // 3. 8-Point Starburst — the star of the show
            float burstScale = (0.04f + flashProgress * 0.03f) * intensity;
            burstScale = MathHelper.Min(burstScale, 0.07f);
            float burstRot = seed + timer * 0.04f;
            sb.Draw(starburst, center, null, themeColors[2] * (flashAlpha * 0.6f),
                burstRot, burstOrigin, burstScale, SpriteEffects.None, 0f);

            // Counter-rotated second starburst for depth
            sb.Draw(starburst, center, null, themeColors[themeColors.Length - 1] * (flashAlpha * 0.25f),
                -burstRot * 0.5f, burstOrigin, burstScale * 0.7f, SpriteEffects.None, 0f);

            // 4. Star flare cross
            float crossScale = (0.03f + flashProgress * 0.04f) * intensity;
            crossScale = MathHelper.Min(crossScale, 0.06f);
            sb.Draw(starFlare, center, null, themeColors[Math.Min(3, themeColors.Length - 1)] * (flashAlpha * 0.35f),
                burstRot * 0.3f, starOrigin, crossScale, SpriteEffects.None, 0f);

            // 5. Hot white core
            float coreScale = 0.015f * intensity * (1f - flashProgress * 0.5f);
            coreScale = MathHelper.Min(coreScale, 0.025f);
            var ptBloom = SparkleBloomHelper.PointBloom;
            Vector2 ptOrigin = ptBloom.Size() / 2f;
            sb.Draw(ptBloom, center, null, Color.White * (flashAlpha * 0.7f),
                0f, ptOrigin, coreScale, SpriteEffects.None, 0f);
        }

        // =====================================================================
        // LAYER 2: BLOOM MOTES
        // =====================================================================

        private void DrawBloomMotes(SpriteBatch sb)
        {
            var softGlow = SparkleBloomHelper.SoftGlow;
            Vector2 origin = softGlow.Size() / 2f;

            for (int i = 0; i < motes.Length; i++)
            {
                if (!motes[i].Active) continue;
                ref BloomMote m = ref motes[i];

                Vector2 pos = m.Position - Main.screenPosition;
                Color col = themeColors[Math.Min(m.ColorIndex, themeColors.Length - 1)];

                sb.Draw(softGlow, pos, null, col * (m.Alpha * 0.25f),
                    0f, origin, m.Scale, SpriteEffects.None, 0f);
            }
        }

        // =====================================================================
        // LAYER 3: SPARKLE FIELD
        // =====================================================================

        private void DrawSparkleField(SpriteBatch sb, float time)
        {
            var star4 = SparkleBloomHelper.Star4Point;
            var bright1 = SparkleBloomHelper.BrightStar1;
            var bright2 = SparkleBloomHelper.BrightStar2;
            var starburst = SparkleBloomHelper.StarburstFlare;
            var softGlow = SparkleBloomHelper.SoftGlow;
            var pointBloom = SparkleBloomHelper.PointBloom;

            Vector2 star4Origin = star4.Size() / 2f;
            Vector2 bright1Origin = bright1.Size() / 2f;
            Vector2 bright2Origin = bright2.Size() / 2f;
            Vector2 burstOrigin = starburst.Size() / 2f;
            Vector2 glowOrigin = softGlow.Size() / 2f;
            Vector2 ptOrigin = pointBloom.Size() / 2f;

            // PASS 1: Glow backdrops
            for (int i = 0; i < sparkles.Length; i++)
            {
                if (!sparkles[i].Active) continue;
                ref ExplosionSparkle s = ref sparkles[i];

                Vector2 pos = s.Position - Main.screenPosition;
                Color baseColor = themeColors[Math.Min(s.ColorIndex, themeColors.Length - 1)];

                float rawFlash = MathF.Sin(time * s.FlashSpeed + s.FlashPhase);
                float flash = MathF.Max(0f, rawFlash);
                flash = MathF.Pow(flash, s.FlashPower);

                float visAlpha = s.Alpha * (0.1f + flash * 0.9f);
                if (visAlpha < 0.015f) continue;

                float glowScale = s.Scale * 2.5f * (0.6f + flash * 0.4f);
                sb.Draw(softGlow, pos, null, baseColor * (visAlpha * 0.2f),
                    0f, glowOrigin, glowScale, SpriteEffects.None, 0f);
            }

            // PASS 2: Star bodies
            for (int i = 0; i < sparkles.Length; i++)
            {
                if (!sparkles[i].Active) continue;
                ref ExplosionSparkle s = ref sparkles[i];

                Vector2 pos = s.Position - Main.screenPosition;
                Color baseColor = themeColors[Math.Min(s.ColorIndex, themeColors.Length - 1)];

                float rawFlash = MathF.Sin(time * s.FlashSpeed + s.FlashPhase);
                float flash = MathF.Max(0f, rawFlash);
                flash = MathF.Pow(flash, s.FlashPower);

                float visAlpha = s.Alpha * (0.1f + flash * 0.9f);
                if (visAlpha < 0.015f) continue;

                Texture2D tex;
                Vector2 origin;
                switch (s.TextureType)
                {
                    case 0: tex = star4; origin = star4Origin; break;
                    case 1: tex = bright1; origin = bright1Origin; break;
                    case 2: tex = bright2; origin = bright2Origin; break;
                    default: tex = starburst; origin = burstOrigin; break;
                }

                sb.Draw(tex, pos, null, baseColor * visAlpha,
                    s.Rotation, origin, s.Scale, SpriteEffects.None, 0f);

                // Counter-rotated overlay for visual complexity
                if (s.TextureType <= 1 && s.Scale > 0.02f)
                {
                    Texture2D overlayTex = (s.TextureType == 0) ? bright2 : star4;
                    Vector2 overlayOrigin = (s.TextureType == 0) ? bright2Origin : star4Origin;
                    sb.Draw(overlayTex, pos, null, baseColor * (visAlpha * 0.35f),
                        -s.Rotation * 0.7f, overlayOrigin, s.Scale * 0.55f, SpriteEffects.None, 0f);
                }
            }

            // PASS 3: Core flash points at peaks
            for (int i = 0; i < sparkles.Length; i++)
            {
                if (!sparkles[i].Active) continue;
                ref ExplosionSparkle s = ref sparkles[i];

                Vector2 pos = s.Position - Main.screenPosition;

                float rawFlash = MathF.Sin(time * s.FlashSpeed + s.FlashPhase);
                float flash = MathF.Max(0f, rawFlash);
                flash = MathF.Pow(flash, s.FlashPower);

                float visAlpha = s.Alpha * (0.1f + flash * 0.9f);
                if (visAlpha < 0.015f) continue;

                // Core bright point at peak flash
                if (flash > 0.3f)
                {
                    float coreIntensity = (flash - 0.3f) / 0.7f;
                    float coreScale = s.Scale * 0.7f * coreIntensity;
                    sb.Draw(pointBloom, pos, null, Color.White * (coreIntensity * visAlpha * 0.4f),
                        0f, ptOrigin, coreScale, SpriteEffects.None, 0f);
                }

                // Star flare cross at high flash
                if (flash > 0.7f && s.TextureType != 3)
                {
                    var flareTex = SparkleBloomHelper.StarFlare;
                    Vector2 flareOrigin = flareTex.Size() / 2f;
                    float flarePow = (flash - 0.7f) / 0.3f;
                    float flareScale = s.Scale * 1.2f * flarePow;
                    sb.Draw(flareTex, pos, null, Color.White * (flarePow * visAlpha * 0.25f),
                        s.Rotation * 0.3f, flareOrigin, flareScale, SpriteEffects.None, 0f);
                }
            }
        }

        public override Color? GetAlpha(Color lightColor) => Color.Transparent;

        // =====================================================================
        // SPAWN HELPER
        // =====================================================================

        /// <summary>
        /// Spawns a themed sparkle explosion at the given world position.
        /// Call from OnHitNPC, OnTileCollide, or Kill.
        /// </summary>
        /// <param name="source">The projectile or entity source</param>
        /// <param name="position">World position for the explosion center</param>
        /// <param name="theme">Which theme's colors to use</param>
        /// <param name="intensity">Size multiplier (0.5=small, 1.0=normal, 1.5=large)</param>
        public static void Spawn(Terraria.DataStructures.IEntitySource source, Vector2 position,
            SparkleTheme theme, float intensity = 1f)
        {
            if (Main.netMode == NetmodeID.Server) return;

            int type = ModContent.ProjectileType<ThemeSparkleExplosion>();
            Projectile.NewProjectile(source, position, Vector2.Zero, type,
                0, 0f, Main.myPlayer, (float)theme, intensity);
        }
    }
}
