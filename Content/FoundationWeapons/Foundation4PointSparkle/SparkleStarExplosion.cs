using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.FoundationWeapons.Foundation4PointSparkle
{
    /// <summary>
    /// SparkleStarExplosion — The dazzling impact explosion that creates a field of
    /// twinkling 4-point star sparkles of various sizes, rotations, and flash timings.
    ///
    /// Spawned when SparkleStarProjectile hits a tile or enemy. The projectile itself
    /// is invisible — all visuals are the sparkle particles it internally manages.
    ///
    /// SPARKLE PARTICLE TYPES (using the 4 star assets):
    /// - Type 0: 4PointStarShiningProjectile — Medium sparkles, the primary display
    /// - Type 1: BrightStarProjectile1 — Large accent sparkles, slower flash
    /// - Type 2: BrightStarProjectile2 — Small fast-twinkling sparkles
    /// - Type 3: 8-Point Starburst Flare — Rare large starburst accents (1-3 per explosion)
    ///
    /// RENDERING ARCHITECTURE:
    /// 1. CENTER FLASH — Large 8-Point Starburst Flare at impact center (first 15 frames)
    ///    with expanding ring, cross flare, and bloom stacking
    /// 2. SPARKLE FIELD — 60+ managed sparkle particles, each with:
    ///    - Independent sin-wave flash timing with cubic/quartic peak sharpening
    ///    - Own rotation, rotation speed, scale, color tint
    ///    - Outward drift velocity with friction and slight gravity
    ///    - 3-layer rendering: glow backdrop + star body + core flash at peak
    /// 3. SECONDARY BLOOM MOTES — Tiny SoftGlow dots scattered between sparkles
    ///    for a sense of magical dust filling the space
    ///
    /// Deals area damage at the center on the first frame.
    /// </summary>
    public class SparkleStarExplosion : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.RainbowCrystalExplosion;

        // ---- CONFIGURATION ----
        private const int SparkleCount = 65;
        private const int MoteCount = 30;
        private const int MaxLifetime = 100;
        private const float DamageRadius = 120f;

        // ---- STATE ----
        private int timer;
        private bool initialized;
        private bool damageDone;
        private ExplosionSparkle[] sparkles;
        private BloomMote[] motes;
        private float seed;

        // ---- SHADER ----
        private Effect sparkleShader;

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
            public float FlashPower;   // Exponent for peak sharpening (3-6)
            public int TextureType;    // 0=4Point, 1=BrightStar1, 2=BrightStar2, 3=8PointStarburst
            public int ColorIndex;
            public float Friction;
            public float GravityMult;
            public bool Active;
        }

        // ---- BLOOM MOTE (tiny ambient glow dot) ----
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
                seed = Main.rand.NextFloat(100f);
                sparkles = new ExplosionSparkle[SparkleCount];
                motes = new BloomMote[MoteCount];
                InitializeSparkles();
                InitializeMotes();
            }

            timer++;
            Projectile.velocity = Vector2.Zero;

            // AoE damage on first frame
            if (!damageDone)
            {
                damageDone = true;
                DealAreaDamage();
            }

            UpdateSparkles();
            UpdateMotes();

            // Center lighting (fading)
            Color[] colors = F4PSTextures.GetSparkleColors();
            float centerAlpha = MathHelper.Clamp(1f - timer / 25f, 0f, 1f);
            Lighting.AddLight(Projectile.Center, colors[2].ToVector3() * centerAlpha * 0.7f);

            if (timer >= MaxLifetime)
                Projectile.Kill();
        }

        // =====================================================================
        // SPARKLE INITIALIZATION
        // =====================================================================

        private void InitializeSparkles()
        {
            for (int i = 0; i < SparkleCount; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float speed = Main.rand.NextFloat(1.5f, 10f);

                // Determine texture type — mostly 4-point stars, some bright stars, rare starburst
                int texType;
                float roll = Main.rand.NextFloat();
                if (roll < 0.45f)
                    texType = 0; // 4PointStarShiningProjectile — most common
                else if (roll < 0.70f)
                    texType = 1; // BrightStarProjectile1
                else if (roll < 0.93f)
                    texType = 2; // BrightStarProjectile2
                else
                    texType = 3; // 8-Point Starburst (rare, large)

                // Scale depends on texture type — starbursts are larger
                float baseScale = texType switch
                {
                    3 => Main.rand.NextFloat(0.04f, 0.08f),  // Starburst — big
                    1 => Main.rand.NextFloat(0.025f, 0.05f),  // BrightStar1 — medium-large
                    0 => Main.rand.NextFloat(0.02f, 0.045f),  // 4Point — medium
                    _ => Main.rand.NextFloat(0.015f, 0.035f),  // BrightStar2 — small-medium
                };

                // Flash power — higher = sharper peaks = more twinkle-like
                float flashPow = texType switch
                {
                    3 => Main.rand.NextFloat(2f, 3f),   // Starburst flashes more broadly
                    _ => Main.rand.NextFloat(3f, 6f),    // Stars twinkle sharply
                };

                sparkles[i] = new ExplosionSparkle
                {
                    Position = Projectile.Center + Main.rand.NextVector2Circular(8f, 8f),
                    Velocity = angle.ToRotationVector2() * speed,
                    Rotation = Main.rand.NextFloat(MathHelper.TwoPi),
                    RotationSpeed = Main.rand.NextFloat(-0.12f, 0.12f),
                    Scale = baseScale,
                    ScaleDecay = Main.rand.NextFloat(0.992f, 0.999f),
                    Alpha = 1f,
                    AlphaDecay = Main.rand.NextFloat(0.975f, 0.994f),
                    FlashPhase = Main.rand.NextFloat(MathHelper.TwoPi),
                    FlashSpeed = Main.rand.NextFloat(0.08f, 0.25f),
                    FlashPower = flashPow,
                    TextureType = texType,
                    ColorIndex = Main.rand.Next(5),
                    Friction = Main.rand.NextFloat(0.965f, 0.990f),
                    GravityMult = Main.rand.NextFloat(0.02f, 0.08f),
                    Active = true,
                };
            }
        }

        private void InitializeMotes()
        {
            for (int i = 0; i < MoteCount; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float speed = Main.rand.NextFloat(1f, 6f);

                motes[i] = new BloomMote
                {
                    Position = Projectile.Center + Main.rand.NextVector2Circular(6f, 6f),
                    Velocity = angle.ToRotationVector2() * speed,
                    Scale = Main.rand.NextFloat(0.005f, 0.02f),
                    Alpha = Main.rand.NextFloat(0.5f, 1f),
                    AlphaDecay = Main.rand.NextFloat(0.970f, 0.990f),
                    ColorIndex = Main.rand.Next(5),
                    Active = true,
                };
            }
        }

        // =====================================================================
        // UPDATE
        // =====================================================================

        private void UpdateSparkles()
        {
            Color[] colors = F4PSTextures.GetSparkleColors();

            for (int i = 0; i < sparkles.Length; i++)
            {
                if (!sparkles[i].Active) continue;
                ref ExplosionSparkle s = ref sparkles[i];

                s.Velocity.Y += s.GravityMult;
                s.Velocity *= s.Friction;
                s.Position += s.Velocity;
                s.Rotation += s.RotationSpeed;
                s.Alpha *= s.AlphaDecay;
                s.Scale *= s.ScaleDecay;

                // Per-sparkle lighting
                if (s.Alpha > 0.2f)
                {
                    Color c = colors[Math.Min(s.ColorIndex, colors.Length - 1)];
                    float flashNow = MathF.Max(0f, MathF.Sin((float)Main.timeForVisualEffects * s.FlashSpeed + s.FlashPhase));
                    flashNow = MathF.Pow(flashNow, s.FlashPower);
                    Lighting.AddLight(s.Position, c.ToVector3() * s.Alpha * flashNow * 0.2f);
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
                m.Velocity.Y += 0.02f;
                m.Position += m.Velocity;
                m.Alpha *= m.AlphaDecay;

                if (m.Alpha < 0.02f)
                    m.Active = false;
            }
        }

        // =====================================================================
        // AREA DAMAGE
        // =====================================================================

        private void DealAreaDamage()
        {
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage || npc.immortal)
                    continue;

                float dist = Vector2.Distance(npc.Center, Projectile.Center);
                if (dist < DamageRadius)
                {
                    float falloff = 1f - (dist / DamageRadius) * 0.5f;
                    int damage = (int)(Projectile.damage * falloff);
                    int dir = (npc.Center.X > Projectile.Center.X) ? 1 : -1;

                    npc.StrikeNPC(npc.CalculateHitInfo(damage, dir, false,
                        Projectile.knockBack, DamageClass.Ranged, true));
                }
            }
        }

        // =====================================================================
        // RENDERING
        // =====================================================================

        public override bool PreDraw(ref Color lightColor)
        {
            if (sparkles == null) return false;

            SpriteBatch sb = Main.spriteBatch;
            Color[] colors = F4PSTextures.GetSparkleColors();
            float time = (float)Main.timeForVisualEffects;

            // ---- ADDITIVE PASS ----
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                SamplerState.LinearClamp, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

            // LAYER 1: Center flash (first 15 frames)
            DrawCenterFlash(sb, colors, time);

            // LAYER 2: Bloom motes
            DrawBloomMotes(sb, colors);

            // LAYER 3: Sparkle field
            DrawSparkleField(sb, colors, time);

            // ---- RESTORE ----
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

            return false;
        }

        // =====================================================================
        // LAYER 1: CENTER FLASH
        // =====================================================================

        /// <summary>
        /// Draws a dramatic center flash using the 8-Point Starburst Flare
        /// as the primary shape, with expanding ring, cross flare, and bloom.
        ///
        /// SUB-LAYERS:
        /// 1. Wide SoftGlow bloom (expanding, fading)
        /// 2. Power Effect Ring (expanding outward ring)
        /// 3. 8-Point Starburst Flare (large rotating starburst)
        /// 4. X-shaped impact cross flare
        /// 5. Lens flare at center
        /// 6. Hot white core point
        /// </summary>
        private void DrawCenterFlash(SpriteBatch sb, Color[] colors, float time)
        {
            if (timer > 18) return;

            float flashProgress = timer / 18f;
            float flashAlpha = 1f - flashProgress * flashProgress; // Quadratic fade
            Vector2 center = Projectile.Center - Main.screenPosition;

            Texture2D softGlow = F4PSTextures.SoftGlow.Value;
            Texture2D starburst = F4PSTextures.StarburstFlare.Value;
            Texture2D ring = F4PSTextures.PowerEffectRing.Value;
            Texture2D cross = F4PSTextures.ImpactCross.Value;
            Texture2D lens = F4PSTextures.LensFlare.Value;
            Texture2D pointBloom = F4PSTextures.PointBloom.Value;

            Vector2 glowOrigin = softGlow.Size() / 2f;
            Vector2 burstOrigin = starburst.Size() / 2f;
            Vector2 ringOrigin = ring.Size() / 2f;
            Vector2 crossOrigin = cross.Size() / 2f;
            Vector2 lensOrigin = lens.Size() / 2f;
            Vector2 ptOrigin = pointBloom.Size() / 2f;

            // 1. Wide bloom (expanding outward)
            float bloomScale = (0.08f + flashProgress * 0.15f);
            sb.Draw(softGlow, center, null, colors[0] * (flashAlpha * 0.5f),
                0f, glowOrigin, bloomScale, SpriteEffects.None, 0f);

            // Warm inner bloom
            sb.Draw(softGlow, center, null, colors[1] * (flashAlpha * 0.4f),
                0f, glowOrigin, bloomScale * 0.5f, SpriteEffects.None, 0f);

            // 2. Expanding ring
            float ringScale = (0.02f + flashProgress * 0.18f);
            float ringAlpha = flashAlpha * (1f - flashProgress * 0.5f);
            sb.Draw(ring, center, null, colors[0] * (ringAlpha * 0.5f),
                0f, ringOrigin, ringScale, SpriteEffects.None, 0f);

            // 3. 8-Point Starburst — the star of the show
            float burstScale = (0.06f + flashProgress * 0.04f);
            float burstRot = seed + timer * 0.04f; // Slow rotation
            sb.Draw(starburst, center, null, colors[2] * (flashAlpha * 0.7f),
                burstRot, burstOrigin, burstScale, SpriteEffects.None, 0f);

            // Counter-rotated second layer for depth
            sb.Draw(starburst, center, null, colors[4] * (flashAlpha * 0.3f),
                -burstRot * 0.5f, burstOrigin, burstScale * 0.7f, SpriteEffects.None, 0f);

            // 4. X-shaped cross flare
            float crossScale = (0.04f + flashProgress * 0.06f);
            sb.Draw(cross, center, null, colors[3] * (flashAlpha * 0.4f),
                burstRot * 0.3f, crossOrigin, crossScale, SpriteEffects.None, 0f);

            // 5. Lens flare (first few frames only)
            if (timer < 8)
            {
                float lensAlpha = (1f - timer / 8f);
                float lensScale = 0.03f + (timer / 8f) * 0.02f;
                sb.Draw(lens, center, null, colors[2] * (lensAlpha * 0.5f),
                    0f, lensOrigin, lensScale, SpriteEffects.None, 0f);
            }

            // 6. Hot white core
            float coreScale = 0.02f * (1f - flashProgress * 0.5f);
            sb.Draw(pointBloom, center, null, Color.White * (flashAlpha * 0.8f),
                0f, ptOrigin, coreScale, SpriteEffects.None, 0f);
        }

        // =====================================================================
        // LAYER 2: BLOOM MOTES
        // =====================================================================

        /// <summary>
        /// Tiny ambient glow dots scattered between sparkles — creates a sense
        /// of magical dust filling the explosion volume.
        /// </summary>
        private void DrawBloomMotes(SpriteBatch sb, Color[] colors)
        {
            Texture2D softGlow = F4PSTextures.SoftGlow.Value;
            Vector2 origin = softGlow.Size() / 2f;

            for (int i = 0; i < motes.Length; i++)
            {
                if (!motes[i].Active) continue;
                ref BloomMote m = ref motes[i];

                Vector2 pos = m.Position - Main.screenPosition;
                Color col = colors[Math.Min(m.ColorIndex, colors.Length - 1)];

                sb.Draw(softGlow, pos, null, col * (m.Alpha * 0.3f),
                    0f, origin, m.Scale, SpriteEffects.None, 0f);
            }
        }

        // =====================================================================
        // LAYER 3: SPARKLE FIELD — The main dazzling display
        // =====================================================================

        /// <summary>
        /// Draws all active explosion sparkles as dazzling, shader-enhanced twinkling stars.
        ///
        /// Uses the StarburstFlashShader in SpriteSortMode.Immediate mode to apply
        /// per-sparkle shader parameters — each sparkle gets its own flash timing,
        /// shimmer intensity, and color configuration for maximum visual variety.
        ///
        /// The shader adds:
        /// - Multi-facet angular shimmer (4/8/3-point patterns) for crystalline light play
        /// - Prismatic HSV edge refraction for rainbow sparkle edges
        /// - Sharp dazzle flash points at facet intersections
        /// - Radial bloom falloff for soft glowing halos
        /// - Pulsing inner glow for a breathing, living quality
        ///
        /// Each sparkle still has its 3 sub-layers for maximum dazzle:
        /// 1. Soft glow backdrop — ambient light halo (non-shader, additive)
        /// 2. Star body — the shader-enhanced star texture
        /// 3. Core bright point — tiny intense white bloom at peak flash moments (non-shader)
        /// </summary>
        private void DrawSparkleField(SpriteBatch sb, Color[] colors, float time)
        {
            Texture2D star4 = F4PSTextures.Star4Point.Value;
            Texture2D bright1 = F4PSTextures.BrightStar1.Value;
            Texture2D bright2 = F4PSTextures.BrightStar2.Value;
            Texture2D starburst = F4PSTextures.StarburstFlare.Value;
            Texture2D softGlow = F4PSTextures.SoftGlow.Value;
            Texture2D pointBloom = F4PSTextures.PointBloom.Value;

            Vector2 star4Origin = star4.Size() / 2f;
            Vector2 bright1Origin = bright1.Size() / 2f;
            Vector2 bright2Origin = bright2.Size() / 2f;
            Vector2 burstOrigin = starburst.Size() / 2f;
            Vector2 glowOrigin = softGlow.Size() / 2f;
            Vector2 ptOrigin = pointBloom.Size() / 2f;

            // ---- FIRST PASS: Non-shader glow backdrops (Deferred additive) ----
            for (int i = 0; i < sparkles.Length; i++)
            {
                if (!sparkles[i].Active) continue;
                ref ExplosionSparkle s = ref sparkles[i];

                Vector2 pos = s.Position - Main.screenPosition;
                Color baseColor = colors[Math.Min(s.ColorIndex, colors.Length - 1)];

                float rawFlash = MathF.Sin(time * s.FlashSpeed + s.FlashPhase);
                float flash = MathF.Max(0f, rawFlash);
                flash = MathF.Pow(flash, s.FlashPower);

                float visAlpha = s.Alpha * (0.1f + flash * 0.9f);
                if (visAlpha < 0.015f) continue;

                float drawScale = s.Scale;

                // SUB-LAYER 1: Soft glow backdrop
                float glowScale = drawScale * 3f * (0.6f + flash * 0.4f);
                sb.Draw(softGlow, pos, null, baseColor * (visAlpha * 0.25f),
                    0f, glowOrigin, glowScale, SpriteEffects.None, 0f);
            }

            // ---- SECOND PASS: Shader-enhanced star bodies (Immediate mode) ----
            // Load shader lazily
            if (sparkleShader == null)
            {
                sparkleShader = ModContent.Request<Effect>(
                    "MagnumOpus/Content/FoundationWeapons/Foundation4PointSparkle/Shaders/StarburstFlashShader",
                    AssetRequestMode.ImmediateLoad).Value;
            }

            // Switch to Immediate mode so we can set per-sparkle shader params
            sb.End();
            sb.Begin(SpriteSortMode.Immediate, MagnumBlendStates.TrueAdditive,
                SamplerState.LinearClamp, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

            // Set shared shader uniforms once
            float shaderTime = time * 0.02f;
            sparkleShader.Parameters["uTime"]?.SetValue(shaderTime);
            sparkleShader.Parameters["highlightColor"]?.SetValue(Color.White.ToVector3());

            for (int i = 0; i < sparkles.Length; i++)
            {
                if (!sparkles[i].Active) continue;
                ref ExplosionSparkle s = ref sparkles[i];

                Vector2 pos = s.Position - Main.screenPosition;
                Color baseColor = colors[Math.Min(s.ColorIndex, colors.Length - 1)];

                float rawFlash = MathF.Sin(time * s.FlashSpeed + s.FlashPhase);
                float flash = MathF.Max(0f, rawFlash);
                flash = MathF.Pow(flash, s.FlashPower);

                float visAlpha = s.Alpha * (0.1f + flash * 0.9f);
                if (visAlpha < 0.015f) continue;

                // Select texture and origin
                Texture2D tex;
                Vector2 origin;
                switch (s.TextureType)
                {
                    case 0: tex = star4; origin = star4Origin; break;
                    case 1: tex = bright1; origin = bright1Origin; break;
                    case 2: tex = bright2; origin = bright2Origin; break;
                    default: tex = starburst; origin = burstOrigin; break;
                }

                float drawScale = s.Scale;

                // Set per-sparkle shader parameters
                sparkleShader.Parameters["flashPhase"]?.SetValue(s.FlashPhase);
                sparkleShader.Parameters["flashSpeed"]?.SetValue(s.FlashSpeed * 50f);
                sparkleShader.Parameters["flashPower"]?.SetValue(s.FlashPower);
                sparkleShader.Parameters["baseAlpha"]?.SetValue(visAlpha);
                sparkleShader.Parameters["shimmerIntensity"]?.SetValue(0.7f + flash * 0.3f);
                sparkleShader.Parameters["primaryColor"]?.SetValue(baseColor.ToVector3());

                // Accent color — use a complementary sparkle color for variety
                int accentIdx = (s.ColorIndex + 2) % colors.Length;
                sparkleShader.Parameters["accentColor"]?.SetValue(colors[accentIdx].ToVector3());

                // Apply pass with updated parameters
                sparkleShader.CurrentTechnique.Passes["StarburstPass"].Apply();

                // SUB-LAYER 2: Shader-enhanced star body
                sb.Draw(tex, pos, null, Color.White,
                    s.Rotation, origin, drawScale, SpriteEffects.None, 0f);

                // Counter-rotated faint overlay for visual complexity on larger sparkles
                if (s.TextureType <= 1 && drawScale > 0.025f)
                {
                    Texture2D overlayTex = (s.TextureType == 0) ? bright2 : star4;
                    Vector2 overlayOrigin = (s.TextureType == 0) ? bright2Origin : star4Origin;
                    sb.Draw(overlayTex, pos, null, Color.White * 0.4f,
                        -s.Rotation * 0.7f, overlayOrigin, drawScale * 0.6f, SpriteEffects.None, 0f);
                }
            }

            // ---- THIRD PASS: Non-shader core flash points (back to Deferred additive) ----
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                SamplerState.LinearClamp, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

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

                float drawScale = s.Scale;

                // SUB-LAYER 3: Core bright point at peak flash
                if (flash > 0.3f)
                {
                    float coreIntensity = (flash - 0.3f) / 0.7f;
                    float coreScale = drawScale * 0.8f * coreIntensity;
                    sb.Draw(pointBloom, pos, null, Color.White * (coreIntensity * visAlpha * 0.5f),
                        0f, ptOrigin, coreScale, SpriteEffects.None, 0f);
                }

                // Extra: star flare cross for maximum dazzle at high flash
                if (flash > 0.7f && s.TextureType != 3)
                {
                    float flarePow = (flash - 0.7f) / 0.3f;
                    Texture2D flareTex = F4PSTextures.StarFlare.Value;
                    Vector2 flareOrigin = flareTex.Size() / 2f;
                    float flareScale = drawScale * 1.5f * flarePow;
                    sb.Draw(flareTex, pos, null, Color.White * (flarePow * visAlpha * 0.3f),
                        s.Rotation * 0.3f, flareOrigin, flareScale, SpriteEffects.None, 0f);
                }
            }
        }

        public override Color? GetAlpha(Color lightColor) => Color.Transparent;
    }
}
