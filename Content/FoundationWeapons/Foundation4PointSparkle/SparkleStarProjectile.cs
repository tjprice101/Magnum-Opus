using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.FoundationWeapons.Foundation4PointSparkle
{
    /// <summary>
    /// SparkleStarProjectile — The ball projectile fired by Foundation4PointSparkle.
    ///
    /// A glowing 4-point star ball that travels toward the cursor. On impact with
    /// any tile or enemy, it spawns a SparkleStarExplosion — a dazzling field of
    /// twinkling 4-point sparkles of various sizes.
    ///
    /// When SparkleTrail mode is active (right-click toggle), the projectile also
    /// spawns twinkling sparkle accents behind it as it flies, creating a brilliant
    /// ribbon of flashing stars in its wake.
    ///
    /// RENDERING LAYERS:
    /// 1. BLOOM TRAIL — Velocity-stretched SoftGlow blooms at past positions (always)
    /// 2. SPARKLE TRAIL — Managed array of twinkling star sprites behind the ball
    ///    (SparkleTrail mode only). Each trail sparkle has its own sin-wave flash
    ///    timing, rotation, scale, and random star texture (4Point, BrightStar1, BrightStar2).
    /// 3. PROJECTILE BODY — 4PointStarShiningProjectile drawn with multi-scale bloom
    ///    stacking: outer glow → mid glow → star body → counter-rotated overlay → core flash
    /// 4. SPARKLE ACCENTS — 6 orbiting twinkle points around the projectile body
    ///
    /// ai[0] = SparkleFireMode index (0 = Normal, 1 = SparkleTrail)
    /// </summary>
    public class SparkleStarProjectile : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.RainbowCrystalExplosion;

        // ---- CONSTANTS ----
        private const int MaxLifetime = 240;
        private const int TrailLength = 16;
        private const int MaxTrailSparkles = 40;
        private const float ProjectileBodyScale = 0.06f;

        // ---- STATE ----
        private int timer;
        private float seed;
        private SparkleFireMode mode;
        private Vector2[] trailPositions = new Vector2[TrailLength];
        private float[] trailRotations = new float[TrailLength];
        private int trailIndex;

        // ---- SHADER ----
        private Effect trailShader;
        private const float TrailWidthHead = 22f;
        private const float TrailWidthTail = 2f;

        // ---- TRAIL SPARKLE PARTICLES (SparkleTrail mode only) ----
        private TrailSparkle[] trailSparkles;
        private int sparkleSpawnTimer;

        private struct TrailSparkle
        {
            public Vector2 Position;
            public float Rotation;
            public float RotationSpeed;
            public float Scale;
            public float Alpha;
            public float AlphaDecay;
            public float FlashPhase;     // Phase offset for sin-wave flash
            public float FlashSpeed;     // Speed of flash oscillation
            public int TextureType;      // 0=4PointStar, 1=BrightStar1, 2=BrightStar2
            public bool Active;
            public Vector2 Drift;        // Gentle drift velocity
            public Color Tint;           // Per-sparkle color tint
        }

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DrawScreenCheckFluff[Projectile.type] = 600;
        }

        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.friendly = true;
            Projectile.penetrate = 1;
            Projectile.tileCollide = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.timeLeft = MaxLifetime;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
            Projectile.alpha = 0;
            Projectile.extraUpdates = 1; // Smoother movement
        }

        public override void AI()
        {
            if (timer == 0)
            {
                seed = Main.rand.NextFloat(100f);
                mode = (SparkleFireMode)(int)Projectile.ai[0];

                for (int i = 0; i < TrailLength; i++)
                {
                    trailPositions[i] = Projectile.Center;
                    trailRotations[i] = Projectile.velocity.ToRotation();
                }

                if (mode == SparkleFireMode.SparkleTrail)
                    trailSparkles = new TrailSparkle[MaxTrailSparkles];
            }

            timer++;

            // Store trail position and rotation
            trailPositions[trailIndex % TrailLength] = Projectile.Center;
            trailRotations[trailIndex % TrailLength] = Projectile.velocity.ToRotation();
            trailIndex++;

            // Slight gravity
            Projectile.velocity.Y += 0.05f;

            // Rotation follows velocity with a spin
            Projectile.rotation += 0.08f;

            // Lighting
            Color[] colors = F4PSTextures.GetSparkleColors();
            float lightPulse = 0.4f + 0.1f * MathF.Sin((float)Main.timeForVisualEffects * 0.1f + seed);
            Lighting.AddLight(Projectile.Center, colors[0].ToVector3() * lightPulse);

            // Ambient dust
            if (Main.rand.NextBool(4))
            {
                Color dustColor = colors[Main.rand.Next(colors.Length)];
                Dust dust = Dust.NewDustPerfect(
                    Projectile.Center + Main.rand.NextVector2Circular(6f, 6f),
                    DustID.RainbowMk2,
                    -Projectile.velocity * 0.05f + Main.rand.NextVector2Circular(0.5f, 0.5f),
                    newColor: dustColor,
                    Scale: Main.rand.NextFloat(0.15f, 0.3f));
                dust.noGravity = true;
                dust.fadeIn = 0.3f;
            }

            // ---- SPARKLE TRAIL MODE: Spawn twinkling sparkles behind ----
            if (mode == SparkleFireMode.SparkleTrail)
            {
                sparkleSpawnTimer++;

                // Spawn a new sparkle every 2 AI ticks (every frame due to extraUpdates)
                if (sparkleSpawnTimer >= 2)
                {
                    sparkleSpawnTimer = 0;
                    SpawnTrailSparkle();
                }

                // Update existing trail sparkles
                UpdateTrailSparkles();
            }
        }

        // =====================================================================
        // TRAIL SPARKLE MANAGEMENT
        // =====================================================================

        private void SpawnTrailSparkle()
        {
            Color[] colors = F4PSTextures.GetSparkleColors();

            // Find an inactive slot
            for (int i = 0; i < MaxTrailSparkles; i++)
            {
                if (!trailSparkles[i].Active)
                {
                    // Offset slightly behind and perpendicular to velocity
                    Vector2 perpendicular = new Vector2(-Projectile.velocity.Y, Projectile.velocity.X);
                    perpendicular = perpendicular.SafeNormalize(Vector2.UnitY);
                    Vector2 spawnPos = Projectile.Center
                        - Projectile.velocity.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(4f, 12f)
                        + perpendicular * Main.rand.NextFloat(-8f, 8f);

                    trailSparkles[i] = new TrailSparkle
                    {
                        Position = spawnPos,
                        Rotation = Main.rand.NextFloat(MathHelper.TwoPi),
                        RotationSpeed = Main.rand.NextFloat(-0.08f, 0.08f),
                        Scale = Main.rand.NextFloat(0.015f, 0.06f),
                        Alpha = 1f,
                        AlphaDecay = Main.rand.NextFloat(0.965f, 0.985f),
                        FlashPhase = Main.rand.NextFloat(MathHelper.TwoPi),
                        FlashSpeed = Main.rand.NextFloat(0.15f, 0.35f),
                        TextureType = Main.rand.Next(3),
                        Active = true,
                        Drift = Main.rand.NextVector2Circular(0.3f, 0.3f) - Projectile.velocity * 0.02f,
                        Tint = colors[Main.rand.Next(colors.Length)],
                    };
                    break;
                }
            }
        }

        private void UpdateTrailSparkles()
        {
            for (int i = 0; i < MaxTrailSparkles; i++)
            {
                if (!trailSparkles[i].Active) continue;
                ref TrailSparkle s = ref trailSparkles[i];

                s.Position += s.Drift;
                s.Rotation += s.RotationSpeed;
                s.Alpha *= s.AlphaDecay;

                // Gentle upward float
                s.Drift.Y -= 0.005f;

                if (s.Alpha < 0.03f)
                    s.Active = false;
            }
        }

        // =====================================================================
        // IMPACT — Spawn the sparkle explosion
        // =====================================================================

        private void SpawnExplosion(Vector2 position)
        {
            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(), position, Vector2.Zero,
                ModContent.ProjectileType<SparkleStarExplosion>(),
                Projectile.damage / 2, Projectile.knockBack * 0.5f,
                Projectile.owner);

            SoundEngine.PlaySound(SoundID.Item25 with { Volume = 0.8f, Pitch = 0.3f }, position);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            SpawnExplosion(target.Center);
        }

        public override void OnKill(int timeLeft)
        {
            SpawnExplosion(Projectile.Center);

            // Death burst of dust
            Color[] colors = F4PSTextures.GetSparkleColors();
            for (int i = 0; i < 10; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(5f, 5f);
                Color col = colors[Main.rand.Next(colors.Length)];
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.RainbowMk2, vel,
                    newColor: col, Scale: Main.rand.NextFloat(0.3f, 0.5f));
                dust.noGravity = true;
                dust.fadeIn = 0.3f;
            }
        }

        // =====================================================================
        // RENDERING
        // =====================================================================

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            try
            {
            Color[] colors = F4PSTextures.GetSparkleColors();
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float fadeIn = MathHelper.Clamp(timer / 6f, 0f, 1f);
            float time = (float)Main.timeForVisualEffects;
            float pulse = 0.85f + 0.15f * MathF.Sin(time * 0.12f + seed);

            // ---- SHADER TRAIL (VertexStrip) — draws outside SpriteBatch ----
            DrawShaderTrail(sb, colors, fadeIn);

            // ---- ADDITIVE PASS ----
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

            // LAYER 1: BLOOM TRAIL — fading glow blooms at past positions
            DrawBloomTrail(sb, colors, fadeIn);

            // LAYER 2: SPARKLE TRAIL — twinkling stars behind the ball (SparkleTrail mode)
            if (mode == SparkleFireMode.SparkleTrail && trailSparkles != null)
                DrawTrailSparkles(sb, colors, time);

            // LAYER 3: PROJECTILE BODY — multi-scale bloom + star body
            DrawProjectileBody(sb, colors, drawPos, fadeIn, pulse, time);

            // LAYER 4: SPARKLE ACCENTS — orbiting twinkle points
            DrawSparkleAccents(sb, colors, drawPos, fadeIn, time);

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
        // SHADER TRAIL — DazzleTrailShader on VertexStrip
        // =====================================================================

        /// <summary>
        /// Renders the projectile's trail as a dazzling sparkle ribbon using
        /// DazzleTrailShader on a VertexStrip mesh. The shader creates:
        /// - Dual counter-scrolling star texture sampling for shimmer interference
        /// - 3-layer procedural glitter at different spatial frequencies
        /// - Prismatic HSV hue shifting along the trail
        /// - Standing-wave brightness pulses
        /// - Hot white core at sparkle peaks with softer colored outer glow
        /// </summary>
        private void DrawShaderTrail(SpriteBatch sb, Color[] colors, float fadeIn)
        {
            int count = Math.Min(trailIndex, TrailLength);
            if (count < 3) return;

            // ---- END SPRITEBATCH (can't mix SpriteBatch and raw vertex drawing) ----
            sb.End();

            // ---- BUILD ORDERED POSITION/ROTATION ARRAYS ----
            Vector2[] positions = new Vector2[count];
            float[] rotations = new float[count];
            for (int i = 0; i < count; i++)
            {
                int bufIdx = (trailIndex - count + i + TrailLength * 2) % TrailLength;
                positions[i] = trailPositions[bufIdx];
                rotations[i] = trailRotations[bufIdx];
            }

            // ---- CONFIGURE VERTEX STRIP ----
            Color StripColor(float progress)
            {
                float alpha = progress * progress * fadeIn;
                return Color.White * alpha;
            }

            float StripWidth(float progress)
            {
                return MathHelper.Lerp(TrailWidthTail, TrailWidthHead, progress);
            }

            VertexStrip strip = new VertexStrip();
            strip.PrepareStrip(positions, rotations, StripColor, StripWidth,
                -Main.screenPosition, includeBacksides: true);

            // ---- LOAD SHADER (lazy, once) ----
            if (trailShader == null)
            {
                trailShader = ModContent.Request<Effect>(
                    "MagnumOpus/Content/FoundationWeapons/Foundation4PointSparkle/Shaders/DazzleTrailShader",
                    AssetRequestMode.ImmediateLoad).Value;
            }

            // ---- SET SHADER UNIFORMS ----
            float time = (float)Main.timeForVisualEffects * 0.02f;

            trailShader.Parameters["WorldViewProjection"]?.SetValue(
                Main.GameViewMatrix.NormalizedTransformationmatrix);
            trailShader.Parameters["uTime"]?.SetValue(time);

            // Trail behavior parameters
            trailShader.Parameters["trailIntensity"]?.SetValue(1.4f);
            trailShader.Parameters["sparkleSpeed"]?.SetValue(1.8f);
            trailShader.Parameters["sparkleScale"]?.SetValue(3.0f);
            trailShader.Parameters["glitterDensity"]?.SetValue(2.5f);
            trailShader.Parameters["tipFadeStart"]?.SetValue(0.65f);
            trailShader.Parameters["edgeSoftness"]?.SetValue(0.35f);
            trailShader.Parameters["pulseRate"]?.SetValue(2.0f);
            trailShader.Parameters["prismaticShift"]?.SetValue(0.08f);

            // Colors
            trailShader.Parameters["coreColor"]?.SetValue(colors[2].ToVector3()); // Pure white
            trailShader.Parameters["outerColor"]?.SetValue(colors[0].ToVector3()); // Cool white-blue
            trailShader.Parameters["accentColor"]?.SetValue(colors[1].ToVector3()); // Warm gold

            // Textures — dual star textures + glow mask
            GraphicsDevice gd = Main.graphics.GraphicsDevice;
            gd.Textures[1] = F4PSTextures.Star4Point.Value;    // sparkleTex
            gd.SamplerStates[1] = SamplerState.LinearWrap;
            gd.Textures[2] = F4PSTextures.BrightStar1.Value;   // sparkleTexB
            gd.SamplerStates[2] = SamplerState.LinearWrap;
            gd.Textures[3] = F4PSTextures.SoftCircle.Value;    // glowMaskTex
            gd.SamplerStates[3] = SamplerState.LinearClamp;

            trailShader.Parameters["sparkleTex"]?.SetValue(F4PSTextures.Star4Point.Value);
            trailShader.Parameters["sparkleTexB"]?.SetValue(F4PSTextures.BrightStar1.Value);
            trailShader.Parameters["glowMaskTex"]?.SetValue(F4PSTextures.SoftCircle.Value);

            // ---- APPLY SHADER PASS AND DRAW ----
            trailShader.CurrentTechnique.Passes["DazzleTrailPass"].Apply();
            strip.DrawTrail();

            // ---- RESET PIXEL SHADER TO TERRARIA DEFAULT ----
            Main.pixelShader.CurrentTechnique.Passes[0].Apply();

            // ---- RESTART SPRITEBATCH ----
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);
        }

        // =====================================================================
        // LAYER 1: BLOOM TRAIL
        // =====================================================================

        private void DrawBloomTrail(SpriteBatch sb, Color[] colors, float fadeIn)
        {
            Texture2D softGlow = F4PSTextures.SoftGlow.Value;
            Vector2 glowOrigin = softGlow.Size() / 2f;
            int count = Math.Min(trailIndex, TrailLength);

            for (int i = 0; i < count; i++)
            {
                int idx = (trailIndex - 1 - i) % TrailLength;
                if (idx < 0) idx += TrailLength;
                Vector2 pos = trailPositions[idx] - Main.screenPosition;

                float progress = (float)i / count;
                float alpha = (1f - progress) * 0.35f * fadeIn;
                float scale = (1f - progress * 0.6f) * 0.05f;

                if (alpha < 0.01f) break;

                // Outer glow — warm gold tint
                sb.Draw(softGlow, pos, null, colors[1] * (alpha * 0.5f),
                    0f, glowOrigin, scale * 1.5f, SpriteEffects.None, 0f);

                // Core glow — white-hot
                sb.Draw(softGlow, pos, null, colors[2] * alpha,
                    0f, glowOrigin, scale * 0.6f, SpriteEffects.None, 0f);
            }
        }

        // =====================================================================
        // LAYER 2: SPARKLE TRAIL — Twinkling stars behind the ball
        // =====================================================================

        /// <summary>
        /// Draws all active trail sparkles as twinkling, flashing 4-point stars.
        /// Each sparkle has independent flash timing (sin-wave with cubic peak sharpening)
        /// creating a field of stars that twinkle asynchronously — like a real star field.
        ///
        /// THREE layers per sparkle for dazzle:
        /// 1. Soft glow backdrop — ambient light from the sparkle
        /// 2. Main star body — the selected star texture with flash-modulated alpha
        /// 3. Core bright flash — tiny intense point at peak flash moments
        /// </summary>
        private void DrawTrailSparkles(SpriteBatch sb, Color[] colors, float time)
        {
            Texture2D softGlow = F4PSTextures.SoftGlow.Value;
            Vector2 glowOrigin = softGlow.Size() / 2f;

            Texture2D star4 = F4PSTextures.Star4Point.Value;
            Texture2D bright1 = F4PSTextures.BrightStar1.Value;
            Texture2D bright2 = F4PSTextures.BrightStar2.Value;

            for (int i = 0; i < MaxTrailSparkles; i++)
            {
                if (!trailSparkles[i].Active) continue;
                ref TrailSparkle s = ref trailSparkles[i];

                Vector2 pos = s.Position - Main.screenPosition;

                // Flash timing — cubic sin peak for sharp twinkle
                float rawFlash = MathF.Sin(time * s.FlashSpeed + s.FlashPhase);
                float flash = MathF.Max(0f, rawFlash);
                flash = flash * flash * flash; // Cubic sharpening — brief bright peaks
                float visAlpha = s.Alpha * (0.15f + flash * 0.85f); // Always slightly visible, flashes bright

                if (visAlpha < 0.02f) continue;

                // Select star texture
                Texture2D starTex = s.TextureType switch
                {
                    0 => star4,
                    1 => bright1,
                    _ => bright2,
                };
                Vector2 starOrigin = starTex.Size() / 2f;

                // LAYER 1: Soft glow backdrop
                float glowScale = s.Scale * 2.5f * (0.7f + flash * 0.3f);
                sb.Draw(softGlow, pos, null, s.Tint * (visAlpha * 0.3f),
                    0f, glowOrigin, glowScale, SpriteEffects.None, 0f);

                // LAYER 2: Star body
                sb.Draw(starTex, pos, null, s.Tint * visAlpha,
                    s.Rotation, starOrigin, s.Scale, SpriteEffects.None, 0f);

                // LAYER 3: Core flash at peak moments
                if (flash > 0.4f)
                {
                    float coreAlpha = (flash - 0.4f) / 0.6f * visAlpha;
                    sb.Draw(softGlow, pos, null, Color.White * (coreAlpha * 0.5f),
                        0f, glowOrigin, s.Scale * 0.8f, SpriteEffects.None, 0f);
                }
            }
        }

        // =====================================================================
        // LAYER 3: PROJECTILE BODY — Multi-scale bloom + star
        // =====================================================================

        /// <summary>
        /// Draws the projectile body as a dazzling rotating star with layered bloom.
        ///
        /// FIVE sub-layers for visual richness:
        /// 1. Wide outer glow — gentle ambient light (SoftRadialBloom)
        /// 2. Mid glow — brighter fill (SoftGlow)
        /// 3. Star body — 4PointStarShiningProjectile, theme-tinted
        /// 4. Counter-rotated overlay — BrightStarProjectile2 at offset rotation for depth
        /// 5. Core point — small intense white center
        /// </summary>
        private void DrawProjectileBody(SpriteBatch sb, Color[] colors, Vector2 drawPos,
            float fadeIn, float pulse, float time)
        {
            Texture2D softGlow = F4PSTextures.SoftGlow.Value;
            Texture2D softRadial = F4PSTextures.SoftRadialBloom.Value;
            Texture2D star4 = F4PSTextures.Star4Point.Value;
            Texture2D bright2 = F4PSTextures.BrightStar2.Value;
            Texture2D pointBloom = F4PSTextures.PointBloom.Value;

            Vector2 glowOrigin = softGlow.Size() / 2f;
            Vector2 radialOrigin = softRadial.Size() / 2f;
            Vector2 starOrigin = star4.Size() / 2f;
            Vector2 bright2Origin = bright2.Size() / 2f;
            Vector2 ptOrigin = pointBloom.Size() / 2f;

            float bodyAlpha = fadeIn * pulse;

            // 1. Wide outer ambient glow
            sb.Draw(softRadial, drawPos, null, colors[3] * (bodyAlpha * 0.25f),
                0f, radialOrigin, 0.15f * pulse, SpriteEffects.None, 0f);

            // 2. Mid glow — warm gold
            sb.Draw(softGlow, drawPos, null, colors[1] * (bodyAlpha * 0.4f),
                0f, glowOrigin, 0.08f * pulse, SpriteEffects.None, 0f);

            // 3. Star body — 4PointStarShiningProjectile
            sb.Draw(star4, drawPos, null, colors[0] * (fadeIn * 0.8f),
                Projectile.rotation, starOrigin, ProjectileBodyScale * pulse, SpriteEffects.None, 0f);

            // White-hot core version
            sb.Draw(star4, drawPos, null, Color.White * (fadeIn * 0.5f),
                Projectile.rotation, starOrigin, ProjectileBodyScale * 0.55f * pulse, SpriteEffects.None, 0f);

            // 4. Counter-rotated overlay — BrightStar2 for depth shimmer
            float overlayRot = -Projectile.rotation * 0.6f + time * 0.02f;
            sb.Draw(bright2, drawPos, null, colors[4] * (fadeIn * 0.3f),
                overlayRot, bright2Origin, ProjectileBodyScale * 0.5f * pulse, SpriteEffects.None, 0f);

            // 5. Core point bloom
            sb.Draw(pointBloom, drawPos, null, Color.White * (fadeIn * 0.6f * pulse),
                0f, ptOrigin, 0.02f, SpriteEffects.None, 0f);
        }

        // =====================================================================
        // LAYER 4: SPARKLE ACCENTS — Orbiting twinkle points
        // =====================================================================

        /// <summary>
        /// 6 orbiting sparkle points with cubic sin-wave flash timing.
        /// Uses alternating 4PointStar and BrightStar1 textures.
        /// </summary>
        private void DrawSparkleAccents(SpriteBatch sb, Color[] colors, Vector2 drawPos,
            float fadeIn, float time)
        {
            Texture2D star4 = F4PSTextures.Star4Point.Value;
            Texture2D bright1 = F4PSTextures.BrightStar1.Value;
            Texture2D softGlow = F4PSTextures.SoftGlow.Value;
            Vector2 star4Origin = star4.Size() / 2f;
            Vector2 bright1Origin = bright1.Size() / 2f;
            Vector2 glowOrigin = softGlow.Size() / 2f;

            for (int i = 0; i < 6; i++)
            {
                float angle = time * 0.05f + i * (MathHelper.TwoPi / 6f) + seed;
                float radius = 10f + 4f * MathF.Sin(time * 0.08f + i * 1.7f);
                Vector2 offset = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * radius;

                // Flash timing — cubic peak sharpening
                float flash = MathF.Max(0f, MathF.Sin(time * 0.1f + i * 2.1f + seed));
                flash = flash * flash * flash;

                if (flash < 0.05f) continue;

                float alpha = flash * fadeIn;
                bool useAlt = i % 2 == 0;
                Texture2D tex = useAlt ? star4 : bright1;
                Vector2 origin = useAlt ? star4Origin : bright1Origin;
                float scale = (useAlt ? 0.025f : 0.02f) * (0.5f + flash * 0.5f);

                // Glow behind sparkle
                sb.Draw(softGlow, drawPos + offset, null, colors[i % colors.Length] * (alpha * 0.25f),
                    0f, glowOrigin, scale * 2f, SpriteEffects.None, 0f);

                // Star sparkle
                float rot = time * 0.12f + i * 1.3f;
                sb.Draw(tex, drawPos + offset, null, colors[i % colors.Length] * (alpha * 0.7f),
                    rot, origin, scale, SpriteEffects.None, 0f);

                // White core at peak
                if (flash > 0.5f)
                {
                    sb.Draw(softGlow, drawPos + offset, null, Color.White * ((flash - 0.5f) * fadeIn * 0.4f),
                        0f, glowOrigin, scale * 0.5f, SpriteEffects.None, 0f);
                }
            }
        }

        public override Color? GetAlpha(Color lightColor) => Color.White;
    }
}
