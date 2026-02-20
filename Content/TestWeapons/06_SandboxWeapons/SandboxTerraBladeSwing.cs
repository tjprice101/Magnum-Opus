using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Shaders;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Trails;
using MagnumOpus.Content.TestWeapons.SandboxWeapons.Shaders;
using static MagnumOpus.Common.Systems.Particles.Particle;

namespace MagnumOpus.Content.TestWeapons.SandboxWeapons
{
    /// <summary>
    /// Held-projectile swing for the Sandbox Terra Blade — shader &amp; VFX showcase.
    /// Implements an Exoblade-style held swing with a 7-layer rendering pipeline:
    ///
    ///   Layer 1: Swept Trail — Custom vertex mesh (base+tip) + NatureTechnique noise shader
    ///   Layer 2: Smear — 3-layer additive smear overlay (shader/manual fallback)
    ///   Layer 3: Blade — Normal lit sprite draw (anchor layer)
    ///   Layer 4: Afterimages — Ghost blade copies at previous rotations
    ///   Layer 5: Glow+Shimmer+Flare — Blade glow + ShimmerOverlay + lens flare at tip
    ///   Layer 6: Motion Blur — MotionBlurBloomRenderer.DrawMeleeSwing (AFTER blade)
    ///   Layer 7: Lighting — Dynamic palette-colored lighting
    ///
    /// Uses squash-and-stretch via SwordDirection (range 1.072→0.88).
    /// </summary>
    public class SandboxTerraBladeSwing : ModProjectile
    {
        #region Constants & Animation Curve

        /// <summary>
        /// Number of tip positions to store for the arc trail.
        /// </summary>
        private const int TrailLength = 60;

        /// <summary>
        /// The swing animation curve:
        /// Segment 1 (0-30%): Slow wind-up at the top (SineIn easing — gentle start)
        /// Segment 2 (30-82%): Fast acceleration downward (PolyIn cubic — snappy flick)
        /// Segment 3 (82-100%): Slight overshoot/deceleration behind player (PolyOut — follow-through)
        /// </summary>
        private static readonly CurveSegment[] SwingCurve = new CurveSegment[]
        {
            new CurveSegment(EasingType.SineIn,  0.00f, -1.0f,   0.15f, 2),  // Slow start at top
            new CurveSegment(EasingType.PolyIn,  0.30f, -0.85f,  1.95f, 3),  // Fast cubic flick down
            new CurveSegment(EasingType.PolyOut, 0.82f,  1.10f, -0.10f, 2),  // Slight settle behind
        };

        /// <summary>
        /// Total arc of the swing in radians. ~200° sweep: starts above, ends behind/below.
        /// </summary>
        private const float MaxSwingAngle = MathHelper.PiOver2 * 2.2f;

        /// <summary>
        /// Duration of the swing in game frames (before attack speed scaling).
        /// </summary>
        private const int BaseDuration = 38;

        /// <summary>
        /// Visual length of the blade in pixels (how far the tip reaches from the player center).
        /// </summary>
        private const float BladeLength = 160f;

        /// <summary>
        /// How much the blade squishes at the extremes of the swing (1.0 = no squish).
        /// Lower values = more squash-and-stretch feel.
        /// </summary>
        private const float SquishRange = 0.88f;

        #endregion

        #region Properties

        private Player Owner => Main.player[Projectile.owner];

        /// <summary>Total time for this swing in update ticks.</summary>
        private int SwingTime
        {
            get => (int)Projectile.localAI[0];
            set => Projectile.localAI[0] = value;
        }

        /// <summary>Current squish factor for squash-and-stretch.</summary>
        private float SquishFactor
        {
            get => Projectile.localAI[1];
            set => Projectile.localAI[1] = value;
        }

        /// <summary>Frames elapsed since swing start.</summary>
        private int Timer => SwingTime - Projectile.timeLeft;

        /// <summary>Normalized progress through the swing (0 → 1).</summary>
        private float Progression => SwingTime > 0 ? (float)Timer / SwingTime : 0f;

        /// <summary>Swing direction: 1 = right-to-left, -1 = left-to-right.</summary>
        private int Direction => Math.Sign(Projectile.velocity.X) <= 0 ? -1 : 1;

        /// <summary>Base rotation angle derived from the initial velocity (toward cursor).</summary>
        private float BaseRotation => Projectile.velocity.ToRotation();

        #endregion

        #region Animation Helpers

        /// <summary>
        /// Gets the angular offset from the base rotation at a given progress point.
        /// </summary>
        private float SwingAngleShiftAtProgress(float progress)
        {
            return MaxSwingAngle * PiecewiseAnimation(progress, SwingCurve);
        }

        /// <summary>
        /// Gets the absolute sword rotation at a given progress point.
        /// </summary>
        private float SwordRotationAtProgress(float progress)
        {
            return BaseRotation + SwingAngleShiftAtProgress(progress) * Direction;
        }

        /// <summary>
        /// Gets the squish factor at a given progress point for squash-and-stretch.
        /// </summary>
        private float SquishAtProgress(float progress)
        {
            float angleShift = Math.Abs(SwingAngleShiftAtProgress(progress));
            return MathHelper.Lerp(1f + (1f - SquishRange) * 0.6f, SquishRange,
                (float)Math.Abs(Math.Sin(angleShift)));
        }

        /// <summary>Current sword rotation accounting for direction and animation.</summary>
        private float SwordRotation => SwordRotationAtProgress(Progression);

        /// <summary>Unit direction vector from player center toward blade tip, with squish applied.</summary>
        private Vector2 SwordDirection
        {
            get
            {
                float rot = SwordRotation;
                Vector2 dir = rot.ToRotationVector2();
                float squish = SquishAtProgress(Progression);
                SquishFactor = squish;
                return dir * squish;
            }
        }

        #endregion

        #region Trail Tracking

        private Vector2[] basePositions = new Vector2[TrailLength];
        private Vector2[] tipPositions = new Vector2[TrailLength];
        private float[] tipRotations = new float[TrailLength];
        private int trailIndex = 0;
        private int shardSpawnCount = 0;

        // Pre-allocated vertex/index buffers for swept-area trail rendering
        private VertexPositionColorTexture[] _sweptVerts = new VertexPositionColorTexture[TrailLength * 2];
        private static short[] _sweptIndices;

        /// <summary>
        /// Builds a vertex mesh from the base/tip circular buffers representing the
        /// full swept area of the blade. Each frame contributes a (base, tip) vertex pair;
        /// consecutive pairs form quads covering the blade's swept silhouette.
        /// UV: U = trail age (0=newest, 1=oldest), V = blade position (0=base, 1=tip).
        /// </summary>
        private int BuildSweptAreaMesh()
        {
            // Lazy-init static index buffer
            if (_sweptIndices == null)
            {
                _sweptIndices = new short[(TrailLength - 1) * 6];
                for (int i = 0; i < TrailLength - 1; i++)
                {
                    int sv = i * 2;
                    int idx = i * 6;
                    _sweptIndices[idx + 0] = (short)sv;
                    _sweptIndices[idx + 1] = (short)(sv + 1);
                    _sweptIndices[idx + 2] = (short)(sv + 2);
                    _sweptIndices[idx + 3] = (short)(sv + 2);
                    _sweptIndices[idx + 4] = (short)(sv + 1);
                    _sweptIndices[idx + 5] = (short)(sv + 3);
                }
            }

            int count = Math.Min(trailIndex, TrailLength);
            if (count < 2) return 0;

            int vertexCount = 0;
            for (int i = 0; i < count; i++)
            {
                int bufIdx = ((trailIndex - 1 - i) % TrailLength + TrailLength) % TrailLength;
                float progress = (float)i / (count - 1);
                float alpha = 1f - progress;
                alpha *= alpha; // quadratic falloff for smoother tail fade

                Color color = TerraBladeShaderManager.GetPaletteColor(0.3f + progress * 0.4f);
                color *= alpha;

                Vector2 baseScreen = basePositions[bufIdx] - Main.screenPosition;
                Vector2 tipScreen = tipPositions[bufIdx] - Main.screenPosition;

                _sweptVerts[vertexCount++] = new VertexPositionColorTexture(
                    new Vector3(baseScreen, 0), color, new Vector2(progress, 0f));
                _sweptVerts[vertexCount++] = new VertexPositionColorTexture(
                    new Vector3(tipScreen, 0), color, new Vector2(progress, 1f));
            }
            return vertexCount;
        }

        #endregion

        #region Setup

        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.None;

        public override void SetStaticDefaults()
        {
            // Store trail positions for arc trail rendering
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = TrailLength;
        }

        public override void SetDefaults()
        {
            Projectile.width = 80;
            Projectile.height = 80;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 9999;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 24;

            // Multiple updates per frame for smoother animation
            Projectile.extraUpdates = 2;
        }

        #endregion

        #region Collision

        public override bool ShouldUpdatePosition() => false;

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            // Line collision from player center to blade tip
            Vector2 start = Owner.MountedCenter;
            Vector2 end = start + SwordDirection * BladeLength;
            float collisionPoint = 0f;

            return Collision.CheckAABBvLineCollision(
                targetHitbox.TopLeft(), targetHitbox.Size(),
                start, end, 40f, ref collisionPoint);
        }

        public override void ModifyDamageHitbox(ref Rectangle hitbox)
        {
            // Center hitbox on the midpoint of the blade
            Vector2 tipPos = Owner.MountedCenter + SwordDirection * BladeLength * 0.5f;
            hitbox.X = (int)tipPos.X - hitbox.Width / 2;
            hitbox.Y = (int)tipPos.Y - hitbox.Height / 2;
        }

        #endregion

        #region AI

        private bool initialized = false;

        public override void AI()
        {
            if (!initialized)
            {
                InitializeSwing();
                initialized = true;
            }

            if (Owner.dead || !Owner.active)
            {
                Projectile.Kill();
                return;
            }

            DoBehavior_Swinging();
        }

        private void InitializeSwing()
        {
            // Calculate swing time based on attack speed, accounting for extra updates
            int maxUpdates = 1 + Projectile.extraUpdates; // 3 total (1 base + 2 extra)
            int totalTime = (int)(BaseDuration / Owner.GetAttackSpeed(DamageClass.MeleeNoSpeed));
            SwingTime = Math.Max(totalTime * maxUpdates, 12);
            Projectile.timeLeft = SwingTime;

            shardSpawnCount = 0;

            // Swing sound
            SoundEngine.PlaySound(SoundID.Item1 with { Pitch = 0.2f, Volume = 0.8f }, Owner.Center);
        }

        private void DoBehavior_Swinging()
        {
            // Lock projectile to owner
            Projectile.Center = Owner.MountedCenter;

            // Keep as held projectile
            Owner.heldProj = Projectile.whoAmI;
            Owner.itemTime = 2;
            Owner.itemAnimation = 2;

            // Rotate player arm to follow the sword
            float armRotation = SwordRotation - MathHelper.PiOver2;
            Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, armRotation);

            // Face the swing direction
            Owner.direction = Direction;

            // Track blade base and tip positions for swept-area trail rendering
            Vector2 baseWorld = Owner.MountedCenter;
            Vector2 tipWorld = baseWorld + SwordDirection * BladeLength;
            basePositions[trailIndex % TrailLength] = baseWorld;
            tipPositions[trailIndex % TrailLength] = tipWorld;
            tipRotations[trailIndex % TrailLength] = SwordRotation;
            trailIndex++;

            // Dense dust trail from blade during active swing
            if (Progression > 0.05f && Progression < 0.95f)
            {
                for (int i = 0; i < 2; i++)
                {
                    Vector2 dustPos = Owner.MountedCenter + SwordDirection * BladeLength * Main.rand.NextFloat(0.4f, 1f);
                    Dust d = Dust.NewDustPerfect(dustPos, DustID.GreenTorch,
                        -SwordDirection * Main.rand.NextFloat(1f, 3f), 0,
                        TerraBladeShaderManager.GetPaletteColor(Main.rand.NextFloat(0.2f, 0.8f)), 1.5f);
                    d.noGravity = true;
                }

                // Light shard spawning at progression thresholds
                float[] shardThresholds = { 0.30f, 0.50f, 0.70f };
                for (int s = 0; s < shardThresholds.Length; s++)
                {
                    if (Progression >= shardThresholds[s] && shardSpawnCount <= s)
                    {
                        if (Main.myPlayer == Projectile.owner)
                        {
                            Vector2 shardPos = Owner.MountedCenter + SwordDirection * BladeLength * Main.rand.NextFloat(0.6f, 0.9f);
                            Vector2 shardVel = -SwordDirection.RotatedBy(Main.rand.NextFloat(-0.3f, 0.3f)) * Main.rand.NextFloat(3f, 5f);
                            shardVel.Y -= 2f;
                            Projectile.NewProjectile(Projectile.GetSource_FromThis(), shardPos, shardVel,
                                ModContent.ProjectileType<LightShardProjectile>(), Projectile.damage / 3, 0f, Projectile.owner);
                        }
                        shardSpawnCount = s + 1;
                    }
                }
            }
        }

        #endregion

        #region Draw Context

        /// <summary>Shared draw locals for the 6-layer rendering pipeline.</summary>
        private struct DrawContext
        {
            public Texture2D BladeTex;
            public Vector2 Origin;
            public Vector2 DrawPos;
            public Vector2 Scale;
            public float Rotation;
            public SpriteEffects Effects;
            public float Time;
            public Vector2 TipScreen;
            public Vector2 TipWorld;
            public float SwingSpeed;
        }

        /// <summary>Safe texture request — returns null instead of throwing if asset is missing.</summary>
        private static Texture2D SafeRequest(string path)
        {
            try
            {
                if (ModContent.HasAsset(path))
                    return ModContent.Request<Texture2D>(path).Value;
            }
            catch { }
            return null;
        }

        #endregion

        #region Rendering

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            PrepareDrawLocals(out var ctx);

            try { DrawSweptTrail(sb, in ctx); } catch (Exception ex) { Mod?.Logger?.Warn($"DrawSweptTrail failed: {ex.Message}"); }
            try { DrawSmearOverlay(sb, in ctx); } catch (Exception ex) { Mod?.Logger?.Warn($"DrawSmearOverlay failed: {ex.Message}"); }
            DrawBladeSprite(sb, in ctx, lightColor);
            try { DrawBladeAfterimages(sb, in ctx); } catch (Exception ex) { Mod?.Logger?.Warn($"DrawBladeAfterimages failed: {ex.Message}"); }
            try { DrawGlowAndFlare(sb, in ctx); } catch (Exception ex) { Mod?.Logger?.Warn($"DrawGlowAndFlare failed: {ex.Message}"); }
            try { DrawMotionBlur(sb, in ctx); } catch (Exception ex) { Mod?.Logger?.Warn($"DrawMotionBlur failed: {ex.Message}"); }
            DrawDynamicLighting(in ctx);

            return false;
        }

        private void PrepareDrawLocals(out DrawContext ctx)
        {
            Texture2D bladeTex = Terraria.GameContent.TextureAssets.Item[ItemID.TerraBlade].Value;
            Vector2 tipWorld = Owner.MountedCenter + SwordRotation.ToRotationVector2() * BladeLength;

            float dp = 0.02f;
            float a0 = SwingAngleShiftAtProgress(Progression);
            float a1 = SwingAngleShiftAtProgress(Math.Min(Progression + dp, 1f));
            float swingSpeed = MathHelper.Clamp(Math.Abs(a1 - a0) / dp * 0.3f, 0f, 1f);

            float baseScale = BladeLength / bladeTex.Height;
            float squish = SquishAtProgress(Progression);

            ctx = new DrawContext
            {
                BladeTex = bladeTex,
                Origin = new Vector2(0, bladeTex.Height),
                DrawPos = Owner.MountedCenter - Main.screenPosition,
                Scale = new Vector2(baseScale * (1f + (1f - squish) * 0.6f), baseScale * squish),
                Rotation = SwordRotation + (Direction == -1 ? MathHelper.Pi : 0),
                Effects = Direction == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
                Time = Main.GlobalTimeWrappedHourly,
                TipWorld = tipWorld,
                TipScreen = tipWorld - Main.screenPosition,
                SwingSpeed = swingSpeed
            };
        }

        // ═══════════════════════════════════════════════════════════════
        // LAYER 1: Swept-Area Noise Trail (custom vertex mesh + NatureTechnique)
        // ═══════════════════════════════════════════════════════════════
        private void DrawSweptTrail(SpriteBatch sb, in DrawContext ctx)
        {
            int vertexCount = BuildSweptAreaMesh();
            if (vertexCount < 4) return;

            var device = Main.instance.GraphicsDevice;

            Effect trailShader = ShaderLoader.Trail;
            if (trailShader == null) return;

            // Safe SpriteBatch End (matches CalamityStyleTrailRenderer)
            try { sb.End(); } catch { }

            try
            {
                // Bind noise texture for organic energy flow
                Texture2D noise = ShaderLoader.GetNoiseTexture("TileableFBMNoise");
                if (noise != null)
                {
                    device.Textures[1] = noise;
                    device.SamplerStates[1] = SamplerState.LinearWrap;
                }

                // Set render states
                device.BlendState = BlendState.Additive;
                device.DepthStencilState = DepthStencilState.None;
                device.RasterizerState = RasterizerState.CullNone;
                device.SamplerStates[0] = SamplerState.LinearWrap;

                // Bind white pixel to slot 0 so shader's tex2D(uImage0, coords) returns (1,1,1,1)
                device.Textures[0] = Terraria.GameContent.TextureAssets.MagicPixel.Value;

                // Shader parameters
                trailShader.CurrentTechnique = trailShader.Techniques["NatureTechnique"];
                trailShader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
                trailShader.Parameters["uColor"]?.SetValue(TerraBladeShaderManager.EnergyGreen.ToVector3());
                trailShader.Parameters["uSecondaryColor"]?.SetValue(TerraBladeShaderManager.BrightCyan.ToVector3());
                trailShader.Parameters["uOpacity"]?.SetValue(1f);
                trailShader.Parameters["uProgress"]?.SetValue(0f);
                trailShader.Parameters["uOverbrightMult"]?.SetValue(3f);
                trailShader.Parameters["uGlowThreshold"]?.SetValue(0.5f);
                trailShader.Parameters["uGlowIntensity"]?.SetValue(1.5f);
                trailShader.Parameters["uHasSecondaryTex"]?.SetValue(noise != null ? 1f : 0f);
                trailShader.Parameters["uSecondaryTexScale"]?.SetValue(1.2f);
                trailShader.Parameters["uSecondaryTexScroll"]?.SetValue(0.5f);

                int primitiveCount = (vertexCount / 2 - 1) * 2;
                float[] intensityMultipliers = { 0.15f, 0.25f, 0.5f, 1.0f };

                for (int pass = 0; pass < 4; pass++)
                {
                    trailShader.Parameters["uIntensity"]?.SetValue(1.2f * intensityMultipliers[pass]);

                    foreach (var p in trailShader.CurrentTechnique.Passes)
                    {
                        p.Apply();
                        device.DrawUserIndexedPrimitives(
                            PrimitiveType.TriangleList,
                            _sweptVerts, 0, vertexCount,
                            _sweptIndices, 0, primitiveCount);
                    }
                }
            }
            finally
            {
                // Clean up noise texture + restore SpriteBatch (ALWAYS runs)
                device.Textures[1] = null;

                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                    DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // LAYER 2: Smear Overlay (shader/manual fallback)
        // ═══════════════════════════════════════════════════════════════
        private void DrawSmearOverlay(SpriteBatch sb, in DrawContext ctx)
        {
            if (Progression < 0.10f || Progression > 0.92f) return;

            float smearIn = MathHelper.Clamp((Progression - 0.10f) / 0.15f, 0f, 1f);
            float smearOut = MathHelper.Clamp((0.92f - Progression) / 0.15f, 0f, 1f);
            float smearAlpha = smearIn * smearOut;

            Texture2D smearTex = SafeRequest("MagnumOpus/Assets/VFX/Smears/Wide Crescent Arc Slash")
                              ?? ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SwordArc2").Value;

            Vector2 smearOrigin = new Vector2(smearTex.Width * 0.5f, smearTex.Height);
            float maxDim = Math.Max(smearTex.Width, smearTex.Height);
            float baseSmearScale = (BladeLength * 2.2f) / maxDim;
            SpriteEffects smearFlip = Direction == -1 ? SpriteEffects.FlipVertically : SpriteEffects.None;

            Effect shader = TerraBladeShaderManager.GetShader();
            if (shader != null && TerraBladeShaderManager.IsAvailable)
            {
                try
                {
                    TerraBladeShaderManager.BindNoiseTexture(Main.instance.GraphicsDevice);
                    TerraBladeShaderManager.BeginShaderAdditive(sb);
                    TerraBladeShaderManager.ApplySlashSmear(shader, Progression, ctx.SwingSpeed, Direction, ctx.Time);

                    Color smearColor = TerraBladeShaderManager.GetPaletteColor(0.5f) * smearAlpha;
                    sb.Draw(smearTex, ctx.DrawPos, null, smearColor, ctx.Rotation, smearOrigin, baseSmearScale, smearFlip, 0f);

                    TerraBladeShaderManager.RestoreSpriteBatch(sb);
                }
                catch (Exception ex)
                {
                    Mod?.Logger?.Warn($"SmearOverlay shader failed: {ex.Message}");
                    try { sb.End(); } catch { }
                    sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                        DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                }
            }
            else
            {
                try
                {
                    TerraBladeShaderManager.BeginAdditive(sb);

                    Color outerSmear = TerraBladeShaderManager.GetPaletteColor(0.2f);
                    sb.Draw(smearTex, ctx.DrawPos, null, outerSmear * smearAlpha * 0.35f,
                        ctx.Rotation, smearOrigin, baseSmearScale * 1.1f, smearFlip, 0f);

                    Color mainSmear = TerraBladeShaderManager.GetPaletteColor(0.5f);
                    sb.Draw(smearTex, ctx.DrawPos, null, mainSmear * smearAlpha * 0.65f,
                        ctx.Rotation, smearOrigin, baseSmearScale, smearFlip, 0f);

                    Color coreSmear = TerraBladeShaderManager.GetPaletteColor(0.8f);
                    sb.Draw(smearTex, ctx.DrawPos, null, coreSmear * smearAlpha * 0.45f,
                        ctx.Rotation, smearOrigin, baseSmearScale * 0.9f, smearFlip, 0f);

                    TerraBladeShaderManager.RestoreSpriteBatch(sb);
                }
                catch (Exception ex)
                {
                    Mod?.Logger?.Warn($"SmearOverlay fallback failed: {ex.Message}");
                    try { sb.End(); } catch { }
                    sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                        DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                }
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // LAYER 3: Normal Blade Sprite (anchor layer)
        // ═══════════════════════════════════════════════════════════════
        private void DrawBladeSprite(SpriteBatch sb, in DrawContext ctx, Color lightColor)
        {
            sb.Draw(ctx.BladeTex, ctx.DrawPos, null, lightColor,
                ctx.Rotation, ctx.Origin, ctx.Scale, ctx.Effects, 0f);
        }

        // ═══════════════════════════════════════════════════════════════
        // LAYER 4: Blade Afterimages (ghost copies at previous rotations)
        // ═══════════════════════════════════════════════════════════════
        private void DrawBladeAfterimages(SpriteBatch sb, in DrawContext ctx)
        {
            if (Progression < 0.05f || Progression > 0.95f) return;

            try
            {
                TerraBladeShaderManager.BeginAdditive(sb);

                int afterimageCount = 5;
                float stepSize = 0.02f;

                for (int i = 1; i <= afterimageCount; i++)
                {
                    float pastProgress = Progression - i * stepSize;
                    if (pastProgress < 0f) break;

                    float pastRotation = SwordRotationAtProgress(pastProgress);
                    float t = (float)i / afterimageCount;
                    float afterimageAlpha = (1f - t) * 0.25f;
                    float rotation = pastRotation + (Direction == -1 ? MathHelper.Pi : 0);

                    Color afterColor = TerraBladeShaderManager.GetPaletteColor(0.4f + t * 0.4f)
                        * afterimageAlpha;

                    sb.Draw(ctx.BladeTex, ctx.DrawPos, null, afterColor,
                        rotation, ctx.Origin, ctx.Scale, ctx.Effects, 0f);
                }

                TerraBladeShaderManager.RestoreSpriteBatch(sb);
            }
            catch (Exception ex)
            {
                Mod?.Logger?.Warn($"BladeAfterimages failed: {ex.Message}");
                try { sb.End(); } catch { }
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                    DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // LAYER 5: Glow + Shimmer + Lens Flare (single additive batch)
        // ═══════════════════════════════════════════════════════════════
        private void DrawGlowAndFlare(SpriteBatch sb, in DrawContext ctx)
        {
            try
            {
                TerraBladeShaderManager.BeginAdditive(sb);

                // Blade glow (2 layers)
                Color glowColor = TerraBladeShaderManager.GetPaletteColor(0.4f + Progression * 0.5f) * 0.3f;
                sb.Draw(ctx.BladeTex, ctx.DrawPos, null, glowColor,
                    ctx.Rotation, ctx.Origin, ctx.Scale * 1.05f, ctx.Effects, 0f);
                sb.Draw(ctx.BladeTex, ctx.DrawPos, null, glowColor * 0.5f,
                    ctx.Rotation, ctx.Origin, ctx.Scale * 1.12f, ctx.Effects, 0f);

                // Shimmer overlay on blade (iridescent scrolling noise)
                Effect shader = TerraBladeShaderManager.GetShader();
                if (shader != null && TerraBladeShaderManager.IsAvailable)
                {
                    try
                    {
                        TerraBladeShaderManager.RestoreSpriteBatch(sb);
                        TerraBladeShaderManager.BindShimmerTexture(Main.instance.GraphicsDevice);
                        TerraBladeShaderManager.BeginShaderAdditive(sb);
                        TerraBladeShaderManager.ApplyShimmerOverlay(
                            shader, Progression, ctx.SwingSpeed, Direction, ctx.Time,
                            afterimageOffset: 0f, intensity: 1.8f, overbright: 2.5f);

                        Color shimmerColor = Color.White * 0.3f;
                        sb.Draw(ctx.BladeTex, ctx.DrawPos, null, shimmerColor,
                            ctx.Rotation, ctx.Origin, ctx.Scale, ctx.Effects, 0f);

                        TerraBladeShaderManager.RestoreSpriteBatch(sb);
                        TerraBladeShaderManager.BeginAdditive(sb);
                    }
                    catch (Exception ex)
                    {
                        Mod?.Logger?.Warn($"ShimmerOverlay failed: {ex.Message}");
                        try { sb.End(); } catch { }
                        sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                            DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                    }
                }

                // Lens flare at tip (3 layers)
                Texture2D flareTex = Terraria.GameContent.TextureAssets.Extra[98].Value;
                Vector2 flareOrigin = flareTex.Size() * 0.5f;
                float pulse = 1f + MathF.Sin(ctx.Time * 10f) * 0.15f;
                float baseScale = 0.25f * pulse;
                Color flareColor = TerraBladeShaderManager.GetPaletteColor(0.6f + Progression * 0.3f);

                sb.Draw(flareTex, ctx.TipScreen, null, flareColor * 0.7f,
                    0f, flareOrigin, baseScale, SpriteEffects.None, 0f);
                sb.Draw(flareTex, ctx.TipScreen, null, flareColor * 0.4f,
                    MathHelper.PiOver4, flareOrigin, baseScale * 0.7f, SpriteEffects.None, 0f);
                sb.Draw(flareTex, ctx.TipScreen, null, Color.White * 0.3f,
                    0f, flareOrigin, baseScale * 0.35f, SpriteEffects.None, 0f);

                TerraBladeShaderManager.RestoreSpriteBatch(sb);
            }
            catch (Exception ex)
            {
                Mod?.Logger?.Warn($"GlowAndFlare failed: {ex.Message}");
                try { sb.End(); } catch { }
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                    DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // LAYER 6: Motion Blur (drawn AFTER blade for visibility)
        // ═══════════════════════════════════════════════════════════════
        private void DrawMotionBlur(SpriteBatch sb, in DrawContext ctx)
        {
            if (Progression < 0.08f || Progression > 0.95f) return;

            float swingSpeed = Progression < 0.5f ? Progression * 2f : 1f;
            float blurStrength = 0.08f * swingSpeed;

            MotionBlurBloomRenderer.DrawMeleeSwing(
                sb, ctx.BladeTex, ctx.DrawPos, SwordDirection,
                TerraBladeShaderManager.GetPaletteColor(0.5f),
                TerraBladeShaderManager.GetPaletteColor(0.8f),
                Math.Max(ctx.Scale.X, ctx.Scale.Y), ctx.Rotation, blurStrength, 1.0f,
                origin: ctx.Origin);
        }

        // ═══════════════════════════════════════════════════════════════
        // LAYER 7: Dynamic Lighting
        // ═══════════════════════════════════════════════════════════════
        private void DrawDynamicLighting(in DrawContext ctx)
        {
            Color tipLight = TerraBladeShaderManager.GetPaletteColor(0.5f);
            Lighting.AddLight(ctx.TipWorld, tipLight.ToVector3() * 0.4f * ctx.SwingSpeed);

            Vector2 midWorld = Owner.MountedCenter + SwordDirection * BladeLength * 0.5f;
            Lighting.AddLight(midWorld, tipLight.ToVector3() * 0.2f);
        }

        #endregion

        #region Hit Effects

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Vector2 hitPos = target.Center;

            // Green torch dust burst — palette gradient
            for (int i = 0; i < 8; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(6f, 6f);
                Dust d = Dust.NewDustPerfect(hitPos, DustID.GreenTorch, vel, 0,
                    TerraBladeShaderManager.GetPaletteColor(Main.rand.NextFloat()), 1.3f);
                d.noGravity = true;
            }

            // Contrasting white/gold sparks
            for (int i = 0; i < 4; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(4f, 4f);
                Dust d = Dust.NewDustPerfect(hitPos, DustID.Enchanted_Gold, vel, 0, Color.White, 0.9f);
                d.noGravity = true;
            }

            // Visible music notes — THIS IS A MUSIC MOD (scale 0.7f+)
            for (int i = 0; i < 3; i++)
            {
                Vector2 noteVel = Main.rand.NextVector2Circular(3f, 3f) + new Vector2(0f, -1.5f);
                Color noteColor = TerraBladeShaderManager.GetPaletteColor(0.3f + Main.rand.NextFloat() * 0.5f);
                float noteScale = Main.rand.NextFloat(0.7f, 1.0f);
                ThemedParticles.MusicNote(hitPos, noteVel, noteColor, noteScale, 35);
            }

            // Bright lighting flash
            Lighting.AddLight(hitPos, 0.5f, 1f, 0.6f);
        }

        #endregion

        #region Networking

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(SwingTime);
            writer.Write(SquishFactor);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            SwingTime = reader.ReadInt32();
            SquishFactor = reader.ReadSingle();
        }

        #endregion
    }
}
