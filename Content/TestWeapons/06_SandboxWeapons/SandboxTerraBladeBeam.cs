using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Effects;
using MagnumOpus.Common.Systems.VFX.Screen;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.Shaders;
using MagnumOpus.Content.TestWeapons.SandboxWeapons.Shaders;

namespace MagnumOpus.Content.TestWeapons.SandboxWeapons
{
    /// <summary>
    /// Main beam projectile for the Sandbox TerraBlade right-click attack.
    /// Charges at the blade tip, then fires a beam toward the cursor.
    /// At the impact point, spawns 5 TerraBladeStarburstBeam projectiles in a star pattern.
    /// </summary>
    public class SandboxTerraBladeBeam : BaseBeamProjectile
    {
        #region Constants

        private const float BladeLength = 160f;
        private const int ChargeFrames = 20;
        private const int FireFrames = 40;
        private const int BeamFadeDuration = 15;
        private const int TotalLifetime = ChargeFrames + FireFrames + BeamFadeDuration;
        private const int StarburstSpawnFrame = 25;
        private const float CursorLerpSpeed = 0.03f;

        #endregion

        #region BaseBeamProjectile Overrides

        public override string ThemeName => "TerraBlade";
        public override float BeamWidth => 55f;
        public override CalamityBeamSystem.WidthStyle WidthStyle => CalamityBeamSystem.WidthStyle.SourceTaper;
        public override float BloomMultiplier => 3.0f;
        public override float CoreMultiplier => 0.25f;
        public override float TextureScrollSpeed => 3.5f;
        public override int SegmentCount => 60;
        public override bool EmitParticles => true;
        public override float ParticleDensity => 1.5f;
        public override float MaxBeamLength => 1200f;
        public override bool FadeOnDeath => true;
        public override int FadeDuration => BeamFadeDuration;

        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.LastPrismLaser;

        #endregion

        #region State

        private int Phase
        {
            get => (int)Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }

        private float PhaseTimer
        {
            get => Projectile.ai[1];
            set => Projectile.ai[1] = value;
        }

        private float TargetX
        {
            get => Projectile.localAI[0];
            set => Projectile.localAI[0] = value;
        }

        private float TargetY
        {
            get => Projectile.localAI[1];
            set => Projectile.localAI[1] = value;
        }

        private Player Owner => Main.player[Projectile.owner];
        private bool starburstSpawned = false;

        #endregion

        #region Lifecycle

        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.timeLeft = TotalLifetime;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 8;
            Projectile.ignoreWater = true;
        }

        public override bool ShouldUpdatePosition() => false;

        #endregion

        #region Beam Endpoint

        protected override Vector2 GetBeamEndPoint()
        {
            if (Phase == 0)
            {
                // During charge, keep beam negligible length
                Vector2 toTarget = (new Vector2(TargetX, TargetY) - Projectile.Center).SafeNormalize(Vector2.UnitX);
                return Projectile.Center + toTarget * 10f;
            }

            return new Vector2(TargetX, TargetY);
        }

        #endregion

        #region AI

        protected override void OnBeamAI()
        {
            // Validate owner
            if (Owner.dead || !Owner.active)
            {
                Projectile.Kill();
                return;
            }

            // Lock player
            Owner.heldProj = Projectile.whoAmI;
            Owner.itemTime = 2;
            Owner.itemAnimation = 2;

            // Face toward target
            Owner.direction = TargetX > Owner.Center.X ? 1 : -1;

            // Anchor to blade tip
            Vector2 toTarget = (new Vector2(TargetX, TargetY) - Owner.MountedCenter).SafeNormalize(Vector2.UnitX);
            Projectile.Center = Owner.MountedCenter + toTarget * BladeLength;

            // Slow cursor tracking (owner only)
            if (Main.myPlayer == Projectile.owner)
            {
                TargetX = MathHelper.Lerp(TargetX, Main.MouseWorld.X, CursorLerpSpeed);
                TargetY = MathHelper.Lerp(TargetY, Main.MouseWorld.Y, CursorLerpSpeed);
            }

            // Arm rotation
            float armRotation = toTarget.ToRotation() - MathHelper.PiOver2;
            Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, armRotation);

            PhaseTimer++;

            switch (Phase)
            {
                case 0: AI_Charge(); break;
                case 1: AI_Fire(); break;
            }
        }

        private void AI_Charge()
        {
            float chargeProgress = PhaseTimer / (float)ChargeFrames;

            // Charge sound at start
            if (PhaseTimer == 1)
                SoundEngine.PlaySound(SoundID.Item15 with { Pitch = 0.8f, Volume = 0.6f }, Projectile.Center);

            // Converging dust
            int dustCount = 4 + (int)(chargeProgress * 8);
            for (int i = 0; i < dustCount; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float dist = MathHelper.Lerp(80f, 20f, chargeProgress);
                Vector2 dustStart = Projectile.Center + angle.ToRotationVector2() * dist;
                Vector2 dustVel = (Projectile.Center - dustStart).SafeNormalize(Vector2.Zero)
                                  * Main.rand.NextFloat(3f, 6f) * (0.5f + chargeProgress);
                Color dustColor = TerraBladeShaderManager.GetPaletteColor(Main.rand.NextFloat(0.3f, 0.8f));
                Dust d = Dust.NewDustPerfect(dustStart, DustID.GreenTorch, dustVel, 0, dustColor,
                    0.8f + chargeProgress * 0.8f);
                d.noGravity = true;
                d.fadeIn = 1.2f;
            }

            // GlowSpark converging particles
            if ((int)PhaseTimer % 2 == 0)
            {
                for (int i = 0; i < 2; i++)
                {
                    float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                    Vector2 sparkStart = Projectile.Center + angle.ToRotationVector2() * 50f;
                    Vector2 sparkVel = (Projectile.Center - sparkStart).SafeNormalize(Vector2.Zero) * 4f;
                    Color sparkColor = TerraBladeShaderManager.GetPaletteColor(Main.rand.NextFloat(0.5f, 0.9f));
                    var spark = new GlowSparkParticle(sparkStart, sparkVel, sparkColor, 0.2f, 10);
                    MagnumParticleHandler.SpawnParticle(spark);
                }
            }

            // BloomRing pulse
            if ((int)PhaseTimer % 8 == 0)
            {
                Color ringColor = TerraBladeShaderManager.GetPaletteColor(0.5f + chargeProgress * 0.3f);
                var ring = new BloomRingParticle(Projectile.Center, Vector2.Zero, ringColor * 0.6f,
                    0.1f + chargeProgress * 0.15f, 10);
                MagnumParticleHandler.SpawnParticle(ring);
            }

            // Subtle trauma buildup
            Projectile.AddTrauma(0.02f * chargeProgress);

            // Lighting
            Color lightColor = TerraBladeShaderManager.GetPaletteColor(0.5f);
            Lighting.AddLight(Projectile.Center, lightColor.ToVector3() * (0.3f + chargeProgress * 0.7f));

            // Transition to fire
            if (PhaseTimer >= ChargeFrames)
            {
                Phase = 1;
                PhaseTimer = 0;

                CalamityBeamSystem.CreateStartupEffect(Projectile.Center, "TerraBlade", 1.2f);
                ScreenFlashSystem.Instance?.ImpactFlash(0.4f);
                Projectile.ShakeScreen(0.5f);
                ScreenDistortionManager.TriggerChromaticBurst(Projectile.Center, intensity: 0.6f, duration: 12);

                SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.2f, Volume = 1.0f }, Projectile.Center);
            }
        }

        private void AI_Fire()
        {
            float fireProgress = PhaseTimer / (float)FireFrames;

            // Impact dust at endpoint
            if ((int)PhaseTimer % 3 == 0)
            {
                Vector2 impactPoint = new Vector2(TargetX, TargetY);
                Color impactColor = TerraBladeShaderManager.GetPaletteColor(Main.rand.NextFloat(0.4f, 0.8f));
                Dust d = Dust.NewDustPerfect(impactPoint + Main.rand.NextVector2Circular(15f, 15f),
                    DustID.GreenTorch, Main.rand.NextVector2Circular(3f, 3f), 0, impactColor, 1.5f);
                d.noGravity = true;
            }

            // Spawn starburst beams
            if (!starburstSpawned && PhaseTimer >= StarburstSpawnFrame && Main.myPlayer == Projectile.owner)
            {
                starburstSpawned = true;
                SpawnStarburstBeams();
            }

            // Continuous trauma
            Projectile.AddTrauma(0.03f);

            // Impact lighting
            Color light = TerraBladeShaderManager.GetPaletteColor(0.6f);
            Vector2 impactPos = new Vector2(TargetX, TargetY);
            Lighting.AddLight(impactPos, light.ToVector3() * 1.2f);

            // Screen ripple at endpoint
            if ((int)PhaseTimer % 10 == 0)
            {
                Color rippleColor = TerraBladeShaderManager.GetPaletteColor(0.5f);
                ScreenDistortionManager.TriggerRipple(impactPos, rippleColor, 0.3f, 10);
            }
        }

        private void SpawnStarburstBeams()
        {
            Vector2 impactPoint = new Vector2(TargetX, TargetY);

            // Impact VFX
            CalamityBeamSystem.CreateImpactEffect(impactPoint, "TerraBlade", 1.5f);
            ScreenFlashSystem.Instance?.ImpactFlash(0.5f);
            Projectile.ShakeScreen(0.6f);
            ScreenDistortionManager.TriggerChromaticBurst(impactPoint, intensity: 0.8f, duration: 15);

            SoundEngine.PlaySound(SoundID.Item14 with { Pitch = 0.4f, Volume = 0.9f }, impactPoint);

            // 5 beams, 72 degrees apart
            float baseAngle = Main.rand.NextFloat(MathHelper.TwoPi);
            for (int i = 0; i < 5; i++)
            {
                float angle = baseAngle + (MathHelper.TwoPi / 5f) * i;

                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    impactPoint,
                    Vector2.Zero,
                    ModContent.ProjectileType<TerraBladeStarburstBeam>(),
                    (int)(Projectile.damage * 0.4f),
                    Projectile.knockBack * 0.3f,
                    Projectile.owner,
                    ai0: angle);
            }
        }

        #endregion

        #region Rendering

        public override bool PreDraw(ref Color lightColor)
        {
            if (Phase == 0)
            {
                DrawChargeVFX();
                return false;
            }

            // Fire phase: render beam via base class
            base.PreDraw(ref lightColor);

            // Additional VFX layers
            DrawMuzzleFlare();
            DrawImpactSplash();

            return false;
        }

        private void DrawChargeVFX()
        {
            SpriteBatch sb = Main.spriteBatch;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float time = Main.GlobalTimeWrappedHourly;
            float chargeProgress = PhaseTimer / (float)ChargeFrames;
            float pulse = 1f + MathF.Sin(time * 12f) * 0.15f;

            // Switch to additive blending
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Texture2D bloomTex = SandboxVFXHelper.SafeRequest("MagnumOpus/Assets/VFX/Blooms/Perfect Soft Color Bloom")
                                ?? Terraria.GameContent.TextureAssets.Extra[98].Value;
            Vector2 bloomOrigin = bloomTex.Size() * 0.5f;

            // Outer bloom
            Color outerColor = TerraBladeShaderManager.GetPaletteColor(0.3f) with { A = 0 };
            float outerScale = 0.15f + chargeProgress * 0.35f;
            sb.Draw(bloomTex, drawPos, null, outerColor * 0.4f * chargeProgress,
                0f, bloomOrigin, outerScale * pulse, SpriteEffects.None, 0f);

            // Inner bright bloom
            Color innerColor = TerraBladeShaderManager.GetPaletteColor(0.6f) with { A = 0 };
            float innerScale = 0.05f + chargeProgress * 0.15f;
            sb.Draw(bloomTex, drawPos, null, innerColor * 0.6f * chargeProgress,
                0f, bloomOrigin, innerScale * pulse, SpriteEffects.None, 0f);

            // White-hot core
            sb.Draw(bloomTex, drawPos, null, (Color.White with { A = 0 }) * 0.5f * chargeProgress,
                0f, bloomOrigin, innerScale * 0.3f * pulse, SpriteEffects.None, 0f);

            // Anamorphic streak along beam direction
            Texture2D streakTex = SandboxVFXHelper.SafeRequest("MagnumOpus/Assets/VFX/Blooms/Horizontal Anamorphic Streak");
            if (streakTex != null)
            {
                Vector2 streakOrigin = streakTex.Size() * 0.5f;
                Vector2 toTarget = (new Vector2(TargetX, TargetY) - Owner.MountedCenter).SafeNormalize(Vector2.UnitX);
                float beamAngle = toTarget.ToRotation();

                Color streakColor = TerraBladeShaderManager.GetPaletteColor(0.7f) with { A = 0 };
                float streakScale = chargeProgress * 0.4f;

                // Along beam direction
                sb.Draw(streakTex, drawPos, null, streakColor * 0.5f * chargeProgress,
                    beamAngle, streakOrigin, new Vector2(streakScale, 0.04f) * pulse,
                    SpriteEffects.None, 0f);

                // Perpendicular
                sb.Draw(streakTex, drawPos, null, streakColor * 0.3f * chargeProgress,
                    beamAngle + MathHelper.PiOver2, streakOrigin, new Vector2(streakScale * 0.6f, 0.03f) * pulse,
                    SpriteEffects.None, 0f);
            }

            // Restore blending
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        private void DrawMuzzleFlare()
        {
            SpriteBatch sb = Main.spriteBatch;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float time = Main.GlobalTimeWrappedHourly;
            float pulse = 1f + MathF.Sin(time * 12f) * 0.06f;

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Texture2D muzzleTex = SandboxVFXHelper.SafeRequest("MagnumOpus/Assets/VFX/Beams/Beam Muzzle Flare Origin")
                                 ?? Terraria.GameContent.TextureAssets.Extra[98].Value;
            Vector2 muzzleOrigin = muzzleTex.Size() * 0.5f;
            Color muzzleColor = TerraBladeShaderManager.GetPaletteColor(0.7f);

            sb.Draw(muzzleTex, drawPos, null, muzzleColor * 0.7f * FadeMultiplier,
                time * 4f, muzzleOrigin, 0.5f * pulse, SpriteEffects.None, 0f);
            sb.Draw(muzzleTex, drawPos, null, (Color.White with { A = 0 }) * 0.5f * FadeMultiplier,
                0f, muzzleOrigin, 0.2f * pulse, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        private void DrawImpactSplash()
        {
            SpriteBatch sb = Main.spriteBatch;
            Vector2 impactScreen = new Vector2(TargetX, TargetY) - Main.screenPosition;
            float time = Main.GlobalTimeWrappedHourly;
            float pulse = 1f + MathF.Sin(time * 10f) * 0.08f;

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Texture2D impactTex = SandboxVFXHelper.SafeRequest("MagnumOpus/Assets/VFX/Beams/Beam Impact Splash")
                                 ?? Terraria.GameContent.TextureAssets.Extra[98].Value;
            Vector2 impactOrigin = impactTex.Size() * 0.5f;
            Color impactColor = TerraBladeShaderManager.GetPaletteColor(0.6f);

            sb.Draw(impactTex, impactScreen, null, impactColor * 0.6f * FadeMultiplier,
                -time * 3f, impactOrigin, 0.6f * pulse, SpriteEffects.None, 0f);
            sb.Draw(impactTex, impactScreen, null, (Color.White with { A = 0 }) * 0.4f * FadeMultiplier,
                0f, impactOrigin, 0.25f * pulse, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        #endregion

        #region Shader Beam Pipeline

        // Static pooled buffers for shader-based beam rendering (shared with TerraBladeStarburstBeam)
        internal static VertexPositionColorTexture[] ShaderVertices;
        internal static short[] ShaderIndices;
        private static bool _shaderPoolReady;
        private const int ShaderVertexCap = 256;
        private const int ShaderIndexCap = 768;

        internal static void EnsureShaderPool()
        {
            if (_shaderPoolReady || Main.dedServ) return;
            ShaderVertices = new VertexPositionColorTexture[ShaderVertexCap];
            ShaderIndices = new short[ShaderIndexCap];
            _shaderPoolReady = true;
        }

        protected override void RenderBeam()
        {
            Effect shader = ShaderLoader.BeamGradientFlow;
            if (shader == null) { base.RenderBeam(); return; }

            EnsureShaderPool();
            if (ShaderVertices == null) { base.RenderBeam(); return; }

            RenderShaderBeam(shader, BeamStart, BeamEnd, FadeMultiplier,
                BeamWidth, WidthStyle, TextureScrollSpeed, SegmentCount);

            if (EmitParticles)
                EmitTerraBladeDust(BeamStart, BeamEnd, FadeMultiplier * ParticleDensity);
        }

        /// <summary>
        /// Renders a flowing energy beam using the BeamGradientFlow shader.
        /// 3-pass pipeline: wide bloom → main flowing beam → white-hot core.
        /// </summary>
        internal static void RenderShaderBeam(Effect shader, Vector2 start, Vector2 end,
            float fade, float beamWidth, CalamityBeamSystem.WidthStyle widthStyle,
            float scrollSpeed, int segments)
        {
            float time = Main.GlobalTimeWrappedHourly;
            float uvScroll = time * scrollSpeed;

            Texture2D noise1 = ShaderLoader.GetNoiseTexture("SoftCircularCaustics");
            Texture2D noise2 = ShaderLoader.GetNoiseTexture("TileableFBMNoise");

            Color primary = TerraBladeShaderManager.GetPaletteColor(0.40f);
            Color secondary = TerraBladeShaderManager.GetPaletteColor(0.60f);

            Matrix viewProj = Matrix.CreateLookAt(Vector3.Backward, Vector3.Zero, Vector3.Up)
                            * Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

            // Generate control points along beam
            Vector2[] points = new Vector2[segments];
            for (int i = 0; i < segments; i++)
            {
                float t = (float)i / (segments - 1);
                points[i] = Vector2.Lerp(start, end, t);
            }

            // Add wave-like wobble perpendicular to beam for organic water-like motion
            Vector2 beamDir = (end - start).SafeNormalize(Vector2.UnitX);
            Vector2 beamPerp = new Vector2(-beamDir.Y, beamDir.X);
            for (int i = 1; i < segments - 1; i++)
            {
                float t = (float)i / (segments - 1);
                // Multi-frequency sine waves for organic wobble
                float wobble = MathF.Sin(t * MathHelper.TwoPi * 2f + time * 3.0f) * 5f
                             + MathF.Sin(t * MathHelper.TwoPi * 3.5f - time * 4.5f) * 2.5f;
                // Taper at endpoints so beam stays anchored at source and tip
                float taper = MathF.Sin(t * MathHelper.Pi);
                points[i] += beamPerp * wobble * taper;
            }

            try { Main.spriteBatch.End(); } catch { }

            try
            {
                var device = Main.instance.GraphicsDevice;
                var prevBlend = device.BlendState;
                var prevRaster = device.RasterizerState;
                var prevDepth = device.DepthStencilState;

                device.RasterizerState = RasterizerState.CullNone;
                device.DepthStencilState = DepthStencilState.None;

                // Shared shader parameters
                shader.Parameters["uWorldViewProjection"]?.SetValue(viewProj);
                shader.Parameters["uTime"]?.SetValue(time);
                shader.Parameters["uNoiseSpeed1"]?.SetValue(-1.5f);
                shader.Parameters["uNoiseSpeed2"]?.SetValue(0.8f);
                shader.Parameters["uNoiseScale1"]?.SetValue(2.0f);
                shader.Parameters["uNoiseScale2"]?.SetValue(3.0f);
                shader.Parameters["uPulseSpeed"]?.SetValue(4.0f);

                // Bind noise textures to sampler slots
                if (noise1 != null) device.Textures[0] = noise1;
                if (noise2 != null) device.Textures[1] = noise2;
                device.SamplerStates[0] = SamplerState.LinearWrap;
                device.SamplerStates[1] = SamplerState.LinearWrap;

                int vc, tc;

                // Pass 1: Wide bloom glow (additive, soft edges, wide)
                FillBeamMesh(points, uvScroll, beamWidth, widthStyle, 3.0f, fade, out vc, out tc);
                if (vc > 0)
                {
                    SetShaderPass(shader, primary, secondary, 0.2f * fade, 1.0f, 0.4f, 2.5f);
                    device.BlendState = BlendState.Additive;
                    ApplyAndDraw(shader, device, vc, tc);
                }

                // Pass 2: Main flowing beam (alpha blend, medium edges)
                FillBeamMesh(points, uvScroll, beamWidth, widthStyle, 1.0f, fade, out vc, out tc);
                if (vc > 0)
                {
                    SetShaderPass(shader, primary, secondary, 0.85f * fade, 1.2f, 0.2f, 1.5f);
                    device.BlendState = BlendState.AlphaBlend;
                    ApplyAndDraw(shader, device, vc, tc);
                }

                // Pass 3: White-hot core (additive, sharp edges, narrow)
                FillBeamMesh(points, uvScroll, beamWidth, widthStyle, 0.25f, fade, out vc, out tc);
                if (vc > 0)
                {
                    SetShaderPass(shader, Color.White, new Color(200, 255, 230), 1.0f * fade, 1.5f, 0.1f, 3.0f);
                    device.BlendState = BlendState.Additive;
                    ApplyAndDraw(shader, device, vc, tc);
                }

                device.BlendState = prevBlend;
                device.RasterizerState = prevRaster;
                device.DepthStencilState = prevDepth;
            }
            finally
            {
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                    SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone,
                    null, Main.GameViewMatrix.TransformationMatrix);
            }
        }

        private static void SetShaderPass(Effect shader, Color primary, Color secondary,
            float opacity, float intensity, float edgeSoftness, float overbrightMult)
        {
            shader.Parameters["uColor"]?.SetValue(primary.ToVector3());
            shader.Parameters["uSecondaryColor"]?.SetValue(secondary.ToVector3());
            shader.Parameters["uOpacity"]?.SetValue(opacity);
            shader.Parameters["uIntensity"]?.SetValue(intensity);
            shader.Parameters["uEdgeSoftness"]?.SetValue(edgeSoftness);
            shader.Parameters["uOverbrightMult"]?.SetValue(overbrightMult);
        }

        private static void ApplyAndDraw(Effect shader, GraphicsDevice device, int vertexCount, int triangleCount)
        {
            foreach (var pass in shader.CurrentTechnique.Passes)
                pass.Apply();
            device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList,
                ShaderVertices, 0, vertexCount, ShaderIndices, 0, triangleCount);
        }

        /// <summary>
        /// Fills the static vertex/index buffers with a beam triangle strip.
        /// </summary>
        private static void FillBeamMesh(Vector2[] points, float uvScroll,
            float baseWidth, CalamityBeamSystem.WidthStyle widthStyle,
            float widthMult, float alphaFade, out int vertexCount, out int triangleCount)
        {
            vertexCount = points.Length * 2;
            triangleCount = (points.Length - 1) * 2;

            if (vertexCount > ShaderVertexCap || triangleCount * 3 > ShaderIndexCap)
            {
                vertexCount = 0;
                triangleCount = 0;
                return;
            }

            for (int i = 0; i < points.Length; i++)
            {
                float ratio = (float)i / (points.Length - 1);

                // Width profile matching BaseBeamProjectile.CalculateWidth
                float width = widthStyle switch
                {
                    CalamityBeamSystem.WidthStyle.SourceTaper =>
                        MathHelper.Lerp(baseWidth, baseWidth * 0.08f, ratio * ratio * (3f - 2f * ratio)),
                    CalamityBeamSystem.WidthStyle.QuadraticBump =>
                        baseWidth * MathF.Sin(ratio * MathHelper.Pi),
                    CalamityBeamSystem.WidthStyle.Constant => baseWidth,
                    _ => baseWidth * MathF.Sin(ratio * MathHelper.Pi)
                };
                width *= widthMult;

                // Sine envelope for smooth beam endpoint falloff (vertex alpha)
                float envelope = MathF.Sin(ratio * MathHelper.Pi);
                Color vertColor = Color.White * (envelope * alphaFade);

                // Direction and perpendicular
                Vector2 dir;
                if (i == 0)
                    dir = (points[1] - points[0]).SafeNormalize(Vector2.UnitY);
                else if (i == points.Length - 1)
                    dir = (points[i] - points[i - 1]).SafeNormalize(Vector2.UnitY);
                else
                    dir = (points[i + 1] - points[i - 1]).SafeNormalize(Vector2.UnitY);

                Vector2 perp = new Vector2(-dir.Y, dir.X);
                Vector2 screenPos = points[i] - Main.screenPosition;
                float u = ratio + uvScroll;

                Vector2 topPos = screenPos + perp * width * 0.5f;
                Vector2 bottomPos = screenPos - perp * width * 0.5f;

                ShaderVertices[i * 2] = new VertexPositionColorTexture(
                    new Vector3(topPos.X, topPos.Y, 0), vertColor, new Vector2(u, 0));
                ShaderVertices[i * 2 + 1] = new VertexPositionColorTexture(
                    new Vector3(bottomPos.X, bottomPos.Y, 0), vertColor, new Vector2(u, 1));
            }

            int idx = 0;
            for (int i = 0; i < points.Length - 1; i++)
            {
                int b = i * 2;
                ShaderIndices[idx++] = (short)b;
                ShaderIndices[idx++] = (short)(b + 1);
                ShaderIndices[idx++] = (short)(b + 2);
                ShaderIndices[idx++] = (short)(b + 1);
                ShaderIndices[idx++] = (short)(b + 3);
                ShaderIndices[idx++] = (short)(b + 2);
            }
        }

        internal static void EmitTerraBladeDust(Vector2 start, Vector2 end, float density)
        {
            if (density <= 0 || Main.rand.NextFloat() > density * 0.25f) return;

            Vector2 dir = (end - start).SafeNormalize(Vector2.UnitX);
            float len = (end - start).Length();
            int count = Math.Max(1, (int)(len / 80f * density));

            for (int i = 0; i < count; i++)
            {
                if (!Main.rand.NextBool(4)) continue;
                float t = Main.rand.NextFloat();
                Vector2 pos = Vector2.Lerp(start, end, t);
                Vector2 offset = new Vector2(-dir.Y, dir.X) * Main.rand.NextFloat(-12f, 12f);
                Color dustColor = TerraBladeShaderManager.GetPaletteColor(Main.rand.NextFloat(0.3f, 0.8f));
                Dust d = Dust.NewDustPerfect(pos + offset, DustID.GreenTorch,
                    dir * Main.rand.NextFloat(0.3f, 1.5f), 0, dustColor, 1.2f);
                d.noGravity = true;
            }
        }

        #endregion

        #region Collision

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (Phase == 0) return false; // No damage during charge
            return base.Colliding(projHitbox, targetHitbox);
        }

        #endregion

        #region Hit Effects

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            ScreenFlashSystem.Instance?.ImpactFlash(0.2f);
            Projectile.ShakeScreen(0.2f);

            for (int i = 0; i < 6; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(5f, 5f);
                Color dustColor = TerraBladeShaderManager.GetPaletteColor(Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(target.Center, DustID.GreenTorch, vel, 0, dustColor, 1.3f);
                d.noGravity = true;
            }

            Color ringColor = TerraBladeShaderManager.GetPaletteColor(0.5f);
            var ring = new BloomRingParticle(target.Center, Vector2.Zero, ringColor * 0.6f, 0.15f, 12);
            MagnumParticleHandler.SpawnParticle(ring);

            Lighting.AddLight(target.Center, 0.5f, 1.0f, 0.6f);
        }

        #endregion

        #region Kill Effects

        protected override void OnBeamKill()
        {
            Vector2 impactPoint = new Vector2(TargetX, TargetY);

            for (int i = 0; i < 12; i++)
            {
                float angle = i / 12f * MathHelper.TwoPi;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f);
                Color dustColor = TerraBladeShaderManager.GetPaletteColor(Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(impactPoint, DustID.GreenTorch, vel, 0, dustColor, 1.4f);
                d.noGravity = true;
            }

            Lighting.AddLight(impactPoint, 0.8f, 1.2f, 0.8f);
        }

        #endregion

        #region Networking

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(Phase);
            writer.Write(PhaseTimer);
            writer.Write(TargetX);
            writer.Write(TargetY);
            writer.Write(starburstSpawned);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            Phase = reader.ReadInt32();
            PhaseTimer = reader.ReadSingle();
            TargetX = reader.ReadSingle();
            TargetY = reader.ReadSingle();
            starburstSpawned = reader.ReadBoolean();
        }

        #endregion
    }

    /// <summary>
    /// Starburst beam that radiates outward from the main beam's impact point.
    /// 5 of these spawn in a star pattern (72 degrees apart).
    /// Grows outward over 8 frames with cubic ease-out.
    /// </summary>
    public class TerraBladeStarburstBeam : BaseBeamProjectile
    {
        #region Constants

        private const int GrowthFrames = 8;
        private const int ActiveFrames = 20;
        private const int StarburstFadeDuration = 10;
        private const int StarburstTotalLifetime = ActiveFrames + StarburstFadeDuration;

        #endregion

        #region BaseBeamProjectile Overrides

        public override string ThemeName => "TerraBlade";
        public override float BeamWidth => 30f;
        public override CalamityBeamSystem.WidthStyle WidthStyle => CalamityBeamSystem.WidthStyle.QuadraticBump;
        public override float BloomMultiplier => 2.0f;
        public override float CoreMultiplier => 0.3f;
        public override float TextureScrollSpeed => 5.0f;
        public override int SegmentCount => 30;
        public override bool EmitParticles => true;
        public override float ParticleDensity => 0.8f;
        public override float MaxBeamLength => 450f;
        public override bool FadeOnDeath => true;
        public override int FadeDuration => StarburstFadeDuration;

        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.LastPrismLaser;

        #endregion

        #region State

        private float BeamAngle => Projectile.ai[0];

        #endregion

        #region Lifecycle

        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.timeLeft = StarburstTotalLifetime;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
            Projectile.ignoreWater = true;
        }

        public override bool ShouldUpdatePosition() => false;

        #endregion

        #region Beam Endpoint

        protected override Vector2 GetBeamEndPoint()
        {
            int age = StarburstTotalLifetime - Projectile.timeLeft;
            float growthProgress = MathHelper.Clamp((float)age / GrowthFrames, 0f, 1f);

            // Cubic ease-out for satisfying extension
            float eased = 1f - (1f - growthProgress) * (1f - growthProgress) * (1f - growthProgress);

            float currentLength = MaxBeamLength * eased;
            Vector2 direction = BeamAngle.ToRotationVector2();

            return Projectile.Center + direction * currentLength;
        }

        #endregion

        #region AI

        protected override void OnBeamAI()
        {
            int age = StarburstTotalLifetime - Projectile.timeLeft;

            // Spawn-frame effects
            if (age == 0)
            {
                Projectile.ShakeScreen(0.15f);

                for (int i = 0; i < 4; i++)
                {
                    Vector2 dustVel = Main.rand.NextVector2Circular(3f, 3f);
                    Color dustColor = TerraBladeShaderManager.GetPaletteColor(Main.rand.NextFloat(0.3f, 0.7f));
                    Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.GreenTorch, dustVel, 0, dustColor, 1.2f);
                    d.noGravity = true;
                }
            }

            // Tip spark during growth
            if (age < GrowthFrames && age % 2 == 0)
            {
                Vector2 currentEnd = GetBeamEndPoint();
                Color sparkColor = TerraBladeShaderManager.GetPaletteColor(Main.rand.NextFloat(0.5f, 0.9f));
                var spark = new GlowSparkParticle(currentEnd, Main.rand.NextVector2Circular(2f, 2f), sparkColor, 0.15f, 8);
                MagnumParticleHandler.SpawnParticle(spark);
            }

            // Lighting at beam tip
            Vector2 tipPos = GetBeamEndPoint();
            Color light = TerraBladeShaderManager.GetPaletteColor(0.5f);
            Lighting.AddLight(tipPos, light.ToVector3() * 0.5f * FadeMultiplier);
        }

        #endregion

        #region Shader Beam Override

        protected override void RenderBeam()
        {
            Effect shader = ShaderLoader.BeamGradientFlow;
            if (shader == null) { base.RenderBeam(); return; }

            SandboxTerraBladeBeam.EnsureShaderPool();
            if (SandboxTerraBladeBeam.ShaderVertices == null) { base.RenderBeam(); return; }

            SandboxTerraBladeBeam.RenderShaderBeam(shader, BeamStart, BeamEnd, FadeMultiplier,
                BeamWidth, WidthStyle, TextureScrollSpeed, SegmentCount);

            if (EmitParticles)
                SandboxTerraBladeBeam.EmitTerraBladeDust(BeamStart, BeamEnd, FadeMultiplier * ParticleDensity);
        }

        #endregion

        #region Hit Effects

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            for (int i = 0; i < 4; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(4f, 4f);
                Color dustColor = TerraBladeShaderManager.GetPaletteColor(Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(target.Center, DustID.GreenTorch, vel, 0, dustColor, 1.1f);
                d.noGravity = true;
            }
            Lighting.AddLight(target.Center, 0.3f, 0.6f, 0.3f);
        }

        #endregion
    }
}
