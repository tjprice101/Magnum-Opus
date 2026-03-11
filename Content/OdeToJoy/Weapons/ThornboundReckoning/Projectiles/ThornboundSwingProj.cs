using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX.Core;
using MagnumOpus.Common.Systems.VFX.Trails;
using MagnumOpus.Content.OdeToJoy.Weapons.ThornboundReckoning.Buffs;
using MagnumOpus.Content.OdeToJoy.Weapons.ThornboundReckoning.Dusts;
using MagnumOpus.Content.OdeToJoy.Weapons.ThornboundReckoning.Utilities;

namespace MagnumOpus.Content.OdeToJoy.Weapons.ThornboundReckoning.Projectiles
{
    /// <summary>
    /// ThornboundSwingProj — The visual swing projectile for Thornbound Reckoning.
    /// Full overhaul: 7-layer rendering pipeline with VertexStrip trail, shader-driven
    /// smear arcs, multi-layer bloom stacking, custom ModDust, screen effects,
    /// and musical harmonic pulses.
    ///
    /// Rendering layers (in draw order):
    ///  1. VERTEXSTRIP TRAIL — GPU ribbon mesh via CalamityStyleTrailRenderer
    ///  2. SMEAR ARC — FBM-distorted vine smear arc via SmearDistortShader
    ///  3. BLADE SPRITE — The weapon texture drawn along the swing angle
    ///  4. ADDITIVE BLOOM STACK — 4-layer bloom at blade tip + root glow
    ///  5. TIP LENS FLARE — Directional lens flare at blade tip
    ///  6. THEME IMPACT ACCENT — OJ-themed blossom sparkle + thorn accent
    ///  7. PHASE VFX — Phase-specific VFX (vine particles / thorn chips / burst ring)
    ///
    /// ai[0] = Combo phase (0=Vine Wave, 1=Thorn Lash, 2=Botanical Burst)
    /// ai[1] = Empowered (0=normal, 1=full Reckoning Charge)
    /// </summary>
    public class ThornboundSwingProj : ModProjectile
    {
        public override string Texture => "MagnumOpus/Content/OdeToJoy/Weapons/ThornboundReckoning/ThornboundReckoning";

        // ---- SWING CONSTANTS ----
        private const float BladeLength = 95f;
        private const int TrailPositionCount = 32;

        // ---- PHASE-SPECIFIC ARC ANGLES ----
        private static readonly float[] PhaseArcDeg = { 160f, 140f, 180f };
        private static readonly float[] PhaseWidthScale = { 0.8f, 1.0f, 1.4f };

        // ---- STATE ----
        private int timer;
        private float startAngle;
        private int swingDirection;
        private Vector2[] trailPositions = new Vector2[TrailPositionCount];
        private int trailIndex;
        private int ComboPhase => (int)Projectile.ai[0];
        private bool IsEmpowered => Projectile.ai[1] >= 1f;

        private int CurrentSwingDuration => ComboPhase switch
        {
            0 => 22,
            1 => 18,
            2 => 26,
            _ => 22
        };

        public override void SetDefaults()
        {
            Projectile.width = 50;
            Projectile.height = 50;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.timeLeft = 30;
            Projectile.ownerHitCheck = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            if (owner.dead || !owner.active)
            {
                Projectile.Kill();
                return;
            }

            int swingDur = CurrentSwingDuration;

            if (timer == 0)
            {
                float aimAngle = Projectile.velocity.ToRotation();
                float arcDeg = PhaseArcDeg[Math.Clamp(ComboPhase, 0, 2)];
                swingDirection = owner.direction;
                startAngle = aimAngle - MathHelper.ToRadians(arcDeg / 2f) * swingDirection;
                Projectile.rotation = startAngle;
                Projectile.timeLeft = swingDur + 2;

                // Initialize trail buffer
                for (int i = 0; i < TrailPositionCount; i++)
                    trailPositions[i] = Vector2.Zero;
                trailIndex = 0;

                // Musical harmonic pulse on combo start
                if (Projectile.owner == Main.myPlayer)
                {
                    Color pulseColor = ComboPhase switch
                    {
                        0 => OdeToJoyPalette.VerdantGreen,
                        1 => OdeToJoyPalette.GoldenPollen,
                        2 => OdeToJoyPalette.RosePink,
                        _ => OdeToJoyPalette.GoldenPollen
                    };
                    OdeToJoyVFXLibrary.RhythmicPulse(owner.MountedCenter, 0.5f + ComboPhase * 0.3f, pulseColor);
                }
            }

            timer++;

            float arcDegCurrent = PhaseArcDeg[Math.Clamp(ComboPhase, 0, 2)];
            float progress = MathHelper.Clamp((float)timer / swingDur, 0f, 1f);
            // Smooth hermite ease for natural swing feel
            float eased = progress * progress * (3f - 2f * progress);

            float currentAngle = startAngle + MathHelper.ToRadians(arcDegCurrent) * eased * swingDirection;
            Projectile.rotation = currentAngle;
            Projectile.Center = owner.MountedCenter;

            owner.ChangeDir(Projectile.velocity.X > 0 ? 1 : -1);
            owner.heldProj = Projectile.whoAmI;
            owner.itemTime = 2;
            owner.itemAnimation = 2;
            owner.itemRotation = (float)Math.Atan2(
                MathF.Sin(currentAngle) * owner.direction,
                MathF.Cos(currentAngle) * owner.direction);

            Vector2 tipPos = owner.MountedCenter + currentAngle.ToRotationVector2() * BladeLength;
            Projectile.position = tipPos - Projectile.Size / 2f;

            // Record trail position (ring buffer)
            if (progress > 0.05f && progress < 0.95f)
            {
                trailPositions[trailIndex % TrailPositionCount] = tipPos;
                trailIndex++;
            }

            // Custom dust along swing arc (ThornburstDust + VineSapDust)
            if (timer % 2 == 0 && progress > 0.1f && progress < 0.9f)
            {
                SpawnSwingDust(owner.MountedCenter, currentAngle, progress);
            }

            // Empowered golden particles at tip
            if (IsEmpowered && timer % 3 == 0)
            {
                SpawnEmpoweredDust(tipPos);
            }

            // Phase 3 finisher effects
            if (ComboPhase == 2 && progress > 0.7f && progress < 0.73f && Projectile.owner == Main.myPlayer)
            {
                OdeToJoyVFXLibrary.ScreenShake(IsEmpowered ? 6f : 3f, 12);
                if (IsEmpowered)
                {
                    OdeToJoyVFXLibrary.ScreenFlash(OdeToJoyPalette.GoldenPollen, 0.5f);
                    OdeToJoyVFXLibrary.HarmonicPulseRing(tipPos, 60f, 16, OdeToJoyPalette.GoldenPollen, 4f);
                }
            }

            if (timer >= swingDur)
                Projectile.Kill();
        }

        private void SpawnSwingDust(Vector2 origin, float angle, float progress)
        {
            float dustDist = BladeLength * Main.rand.NextFloat(0.4f, 1.0f);
            Vector2 pos = origin + angle.ToRotationVector2() * dustDist;
            Vector2 vel = angle.ToRotationVector2().RotatedBy(MathHelper.PiOver2 * swingDirection) * Main.rand.NextFloat(1f, 3f);

            // ThornburstDust for angular thorn debris
            Color thornCol = ThornboundTextures.GetBotanicalGradient(progress);
            Dust thorn = Dust.NewDustPerfect(pos, ModContent.DustType<ThornburstDust>(), vel,
                newColor: thornCol, Scale: Main.rand.NextFloat(1.2f, 2.0f));
            thorn.noGravity = true;

            // Occasional VineSapDust drips from blade
            if (Main.rand.NextBool(3))
            {
                Vector2 sapVel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(0.5f, 2f));
                Dust sap = Dust.NewDustPerfect(pos, ModContent.DustType<VineSapDust>(), sapVel,
                    newColor: OdeToJoyPalette.VerdantGreen, Scale: Main.rand.NextFloat(1.0f, 1.8f));
            }
        }

        private void SpawnEmpoweredDust(Vector2 tipPos)
        {
            Vector2 vel = Main.rand.NextVector2Circular(3f, 3f);
            try
            {
                var glow = new GlowSparkParticle(
                    tipPos + Main.rand.NextVector2Circular(8f, 8f),
                    vel, OdeToJoyPalette.GoldenPollen with { A = 0 },
                    0.25f, Main.rand.Next(15, 25));
                MagnumParticleHandler.SpawnParticle(glow);
            }
            catch
            {
                Dust d = Dust.NewDustPerfect(tipPos, DustID.GoldCoin, vel, Scale: 0.7f);
                d.noGravity = true;
            }
        }

        private Color[] GetPhaseColors()
        {
            return ComboPhase switch
            {
                0 => ThornboundTextures.VineSwingColors,
                1 => ThornboundTextures.ThornLashColors,
                2 => ThornboundTextures.BotanicalBurstColors,
                _ => ThornboundTextures.VineSwingColors,
            };
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            Player owner = Main.player[Projectile.owner];
            Vector2 origin = owner.MountedCenter;
            Vector2 tip = origin + Projectile.rotation.ToRotationVector2() * BladeLength;

            float _ = 0f;
            return Collision.CheckAABBvLineCollision(
                targetHitbox.TopLeft(), targetHitbox.Size(),
                origin, tip, 24f, ref _);
        }

        public override bool ShouldUpdatePosition() => false;

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Player owner = Main.player[Projectile.owner];
            var tbp = owner.GetModPlayer<ThornboundPlayer>();

            // Build Reckoning Charge based on phase
            if (ComboPhase == 0)
                tbp.AddVineWaveCharge();
            else if (ComboPhase == 1)
                tbp.AddThornEmbedCharge();
            else
                tbp.AddVineWaveCharge();

            // Apply Rose Thorn Bleed on Phase 1 and 2
            if (ComboPhase <= 1)
                target.AddBuff(ModContent.BuffType<RoseThornBleedDebuff>(), 240);

            // Custom impact VFX — layered thorn burst + sparkle explosion
            Color[] colors = GetPhaseColors();
            for (int i = 0; i < 6; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(7f, 7f);
                Color col = colors[Main.rand.Next(colors.Length)];
                Dust dust = Dust.NewDustPerfect(target.Center, ModContent.DustType<ThornburstDust>(), vel,
                    newColor: col, Scale: Main.rand.NextFloat(1.5f, 2.5f));
            }

            // Sparkle explosion on hit
            OdeToJoyVFXLibrary.SpawnGardenSparkleExplosion(target.Center, 6 + ComboPhase * 2,
                5f + ComboPhase * 1.5f, 0.25f + ComboPhase * 0.1f);

            // Starburst on Phase 2+ hits
            if (ComboPhase >= 2)
                OdeToJoyVFXLibrary.SpawnTriumphantStarburst(target.Center, 0.4f + (IsEmpowered ? 0.4f : 0f));
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Player owner = Main.player[Projectile.owner];
            SpriteBatch sb = Main.spriteBatch;

            try
            {
                Vector2 drawOrigin = owner.MountedCenter - Main.screenPosition;
                int swingDur = CurrentSwingDuration;
                float progress = MathHelper.Clamp((float)timer / swingDur, 0f, 1f);
                float currentAngle = Projectile.rotation;
                float widthScale = PhaseWidthScale[Math.Clamp(ComboPhase, 0, 2)];

                // Fade envelope — smooth in/out
                float fade;
                if (progress < 0.1f)
                    fade = progress / 0.1f;
                else if (progress > 0.85f)
                    fade = (1f - progress) / 0.15f;
                else
                    fade = 1f;

                if (IsEmpowered) fade = MathHelper.Clamp(fade * 1.3f, 0f, 1f);

                Vector2 tipDrawPos = drawOrigin + currentAngle.ToRotationVector2() * BladeLength;

                // ==================================================================
                //  LAYER 1: VERTEXSTRIP TRAIL (GPU ribbon mesh)
                // ==================================================================
                DrawVertexStripTrail(progress, fade);

                // ==================================================================
                //  LAYER 2: SMEAR ARC OVERLAY (shader-driven distortion + flow)
                // ==================================================================
                DrawSmearArc(sb, drawOrigin, currentAngle, widthScale, fade);

                // ==================================================================
                //  LAYER 3: BLADE SPRITE
                // ==================================================================
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                    Main.DefaultSamplerState, DepthStencilState.None,
                    RasterizerState.CullCounterClockwise, null,
                    Main.GameViewMatrix.TransformationMatrix);

                Texture2D bladeTex = ModContent.Request<Texture2D>(Texture,
                    ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
                Vector2 bladeOrigin = new Vector2(0, bladeTex.Height);
                SpriteEffects flip = swingDirection < 0 ? SpriteEffects.FlipVertically : SpriteEffects.None;

                sb.Draw(bladeTex, drawOrigin, null,
                    lightColor, currentAngle + MathHelper.PiOver4,
                    bladeOrigin, 1f, flip, 0f);

                sb.End();

                // ==================================================================
                //  LAYER 4: ADDITIVE BLOOM STACK (4-layer at tip + root glow)
                // ==================================================================
                sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                    SamplerState.LinearClamp, DepthStencilState.None,
                    RasterizerState.CullCounterClockwise, null,
                    Main.GameViewMatrix.TransformationMatrix);

                // Tip bloom stack — 4 layers from outer to core
                Color tipOuter = GetPhaseBloomColor(ComboPhase, 0.2f) with { A = 0 };
                Color tipCore = GetPhaseBloomColor(ComboPhase, 0.9f) with { A = 0 };
                float tipPulse = 0.85f + 0.15f * MathF.Sin((float)Main.timeForVisualEffects * 0.08f);
                OdeToJoyVFXLibrary.DrawBloomStack(sb,
                    owner.MountedCenter + currentAngle.ToRotationVector2() * BladeLength,
                    tipCore, tipOuter, 0.22f * widthScale * tipPulse, fade * 0.7f, 4);

                // Root glow — soft bloom at handle
                Texture2D softGlow = ThornboundTextures.SoftGlow.Value;
                sb.Draw(softGlow, drawOrigin, null,
                    (ThornboundTextures.RoseShadow with { A = 0 }) * fade * 0.25f, 0f,
                    softGlow.Size() / 2f, 0.15f, SpriteEffects.None, 0f);

                // ==================================================================
                //  LAYER 5: TIP LENS FLARE
                // ==================================================================
                float flareIntensity = fade * (0.4f + progress * 0.4f);
                Color flareColor = GetPhaseBloomColor(ComboPhase, 0.7f);
                OdeToJoyVFXLibrary.DrawLensFlare(sb,
                    owner.MountedCenter + currentAngle.ToRotationVector2() * BladeLength,
                    flareColor, 0.18f * widthScale, currentAngle, flareIntensity);

                // Empowered: extra white-hot flare
                if (IsEmpowered)
                {
                    OdeToJoyVFXLibrary.DrawLensFlare(sb,
                        owner.MountedCenter + currentAngle.ToRotationVector2() * BladeLength,
                        OdeToJoyPalette.WhiteBloom, 0.14f * widthScale,
                        currentAngle + MathHelper.PiOver4, fade * 0.5f);
                }

                // ==================================================================
                //  LAYER 6: THEME IMPACT ACCENTS
                // ==================================================================
                OdeToJoyVFXLibrary.DrawThemeBlossomSparkle(sb, Projectile.Center, 1f, fade * 0.5f);
                if (ComboPhase >= 1)
                    OdeToJoyVFXLibrary.DrawThemeThornAccent(sb, Projectile.Center, 0.8f, fade * 0.35f);

                sb.End();
            }
            catch { }
            finally
            {
                // Always restore SpriteBatch to standard state
                try { sb.End(); } catch { }
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                    DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }

            return false;
        }

        /// <summary>
        /// LAYER 1: Draws a VertexStrip GPU ribbon trail along recorded blade tip positions.
        /// Uses CalamityStyleTrailRenderer with Nature trail style + bloom multi-pass.
        /// </summary>
        private void DrawVertexStripTrail(float progress, float fade)
        {
            if (trailIndex < 4) return;

            // Build contiguous position array from ring buffer
            int count = Math.Min(trailIndex, TrailPositionCount);
            Vector2[] positions = new Vector2[count];
            for (int i = 0; i < count; i++)
            {
                int idx = (trailIndex - count + i) % TrailPositionCount;
                positions[i] = trailPositions[idx];
            }

            // Filter out zero positions
            int valid = 0;
            for (int i = 0; i < positions.Length; i++)
            {
                if (positions[i] != Vector2.Zero) valid++;
            }
            if (valid < 3) return;

            Color primary = ComboPhase switch
            {
                0 => OdeToJoyPalette.VerdantGreen,
                1 => OdeToJoyPalette.GoldenPollen,
                2 => OdeToJoyPalette.RosePink,
                _ => OdeToJoyPalette.VerdantGreen
            };
            Color secondary = Color.Lerp(primary, OdeToJoyPalette.WhiteBloom, 0.3f);

            float trailWidth = 28f * PhaseWidthScale[Math.Clamp(ComboPhase, 0, 2)] * fade;
            if (IsEmpowered) trailWidth *= 1.3f;

            // Multi-pass bloom trail: outer glow + body + core
            CalamityStyleTrailRenderer.DrawTrailWithBloom(
                positions, null, CalamityStyleTrailRenderer.TrailStyle.Nature,
                trailWidth, primary, secondary,
                fade * 0.8f, 2.2f);
        }

        /// <summary>
        /// LAYER 2: Draws the shader-driven smear arc overlay.
        /// 3 sub-layers: wide outer glow, main smear, bright core.
        /// </summary>
        private void DrawSmearArc(SpriteBatch sb, Vector2 drawOrigin, float currentAngle,
            float widthScale, float fade)
        {
            Texture2D smearTex = ThornboundTextures.FlamingSwordArc.Value;
            Vector2 smearOrigin = smearTex.Size() / 2f;
            float maxDim = MathF.Max(smearTex.Width, smearTex.Height);
            float smearScale = (BladeLength * 2.4f) / maxDim * widthScale;

            Effect shader = ThornboundTextures.SmearDistortShader;

            if (shader != null)
            {
                sb.End();
                sb.Begin(SpriteSortMode.Immediate, BlendState.Additive,
                    SamplerState.LinearWrap, DepthStencilState.None,
                    RasterizerState.CullCounterClockwise, null,
                    Main.GameViewMatrix.EffectMatrix);

                float time = (float)Main.gameTimeCache.TotalGameTime.TotalSeconds;

                float distortStr = ComboPhase switch
                {
                    0 => 0.06f,
                    1 => 0.04f,
                    2 => 0.08f,
                    _ => 0.06f,
                };

                shader.Parameters["uTime"]?.SetValue(time);
                shader.Parameters["fadeAlpha"]?.SetValue(MathHelper.Clamp(fade, 0f, 1f));
                shader.Parameters["flowSpeed"]?.SetValue(0.35f);
                shader.Parameters["noiseScale"]?.SetValue(3.0f);
                shader.Parameters["noiseTex"]?.SetValue(ThornboundTextures.FBMNoise.Value);
                shader.Parameters["gradientTex"]?.SetValue(ThornboundTextures.GradOdeToJoy.Value);

                // Sub-layer A: Wide outer glow
                shader.Parameters["distortStrength"]?.SetValue(distortStr * 1.3f);
                shader.CurrentTechnique.Passes[0].Apply();
                sb.Draw(smearTex, drawOrigin, null,
                    Color.White * MathHelper.Clamp(fade * 0.45f, 0f, 1f),
                    currentAngle, smearOrigin,
                    smearScale * 1.15f, SpriteEffects.None, 0f);

                // Sub-layer B: Main smear
                shader.Parameters["distortStrength"]?.SetValue(distortStr);
                shader.CurrentTechnique.Passes[0].Apply();
                sb.Draw(smearTex, drawOrigin, null,
                    Color.White * MathHelper.Clamp(fade * 0.75f, 0f, 1f),
                    currentAngle, smearOrigin,
                    smearScale, SpriteEffects.None, 0f);

                // Sub-layer C: Bright core
                shader.Parameters["distortStrength"]?.SetValue(distortStr * 0.4f);
                shader.CurrentTechnique.Passes[0].Apply();
                sb.Draw(smearTex, drawOrigin, null,
                    Color.White * MathHelper.Clamp(fade * 0.6f, 0f, 1f),
                    currentAngle, smearOrigin,
                    smearScale * 0.85f, SpriteEffects.None, 0f);

                sb.End();
            }
            else
            {
                // Fallback without shader — colored arc layers
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                    Main.DefaultSamplerState, DepthStencilState.None,
                    RasterizerState.CullCounterClockwise, null,
                    Main.GameViewMatrix.EffectMatrix);

                Color fallbackColor = GetPhaseBloomColor(ComboPhase, 0.5f);
                sb.Draw(smearTex, drawOrigin, null, fallbackColor * fade * 0.4f,
                    currentAngle, smearOrigin, smearScale * 1.15f, SpriteEffects.None, 0f);
                sb.Draw(smearTex, drawOrigin, null, fallbackColor * fade * 0.7f,
                    currentAngle, smearOrigin, smearScale, SpriteEffects.None, 0f);
                sb.Draw(smearTex, drawOrigin, null, fallbackColor * fade * 0.55f,
                    currentAngle, smearOrigin, smearScale * 0.85f, SpriteEffects.None, 0f);

                sb.End();
            }
        }

        /// <summary>Gets a phase-appropriate bloom color at the given gradient position t.</summary>
        private Color GetPhaseBloomColor(int phase, float t)
        {
            return phase switch
            {
                0 => Color.Lerp(OdeToJoyPalette.DeepForest, OdeToJoyPalette.GoldenPollen, t),
                1 => Color.Lerp(OdeToJoyPalette.WarmAmber, OdeToJoyPalette.SunlightYellow, t),
                2 => Color.Lerp(OdeToJoyPalette.RosePink, OdeToJoyPalette.WhiteBloom, t),
                _ => OdeToJoyPalette.GetGradient(t)
            };
        }
    }
}
