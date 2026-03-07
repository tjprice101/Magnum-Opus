using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Bloom;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.Shaders;
using MagnumOpus.Content.Eroica;
using MagnumOpus.Content.Eroica.Projectiles;
using MagnumOpus.Content.Eroica.Weapons.SakurasBlossom.Buffs;
using MagnumOpus.Content.FoundationWeapons.SwordSmearFoundation;
using MagnumOpus.Content.FoundationWeapons.RibbonFoundation;
using MagnumOpus.Content.MoonlightSonata.Debuffs;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using ReLogic.Content;
using System;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;

namespace MagnumOpus.Content.Eroica.Weapons.SakurasBlossom
{
    /// <summary>
    /// Sakura's Blossom swing projectile — Petal Dance 3-phase flowing combo.
    /// Phase 0: First Petal — graceful horizontal sweep with petal trail.
    /// Phase 1: Scattered Petals — cross-slash spawns 8 homing petal projectiles.
    /// Phase 2: Final Bloom — upward flourish + downward finisher, 360° petal burst.
    /// Blossom Counter: perfect-timing reflects enemy projectiles as homing petals.
    /// Sakura Meditation: hold without target for 1.5s, next swing 2x arc range.
    /// </summary>
    public class SakurasBlossomSwing : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

        // ── AI state ──
        private ref float ComboPhase => ref Projectile.ai[0];
        private ref float PhaseTimer => ref Projectile.ai[1];

        private float swingRotation = 0f;
        private int swingDirection = 1;
        private bool initialized = false;
        private bool phaseProjectileSpawned = false;
        private float meditationTimer = 0f;
        private bool meditationCharged = false;

        // ── Afterimage tracking ──
        private const int MaxAfterimages = 10;
        private float[] afterimageRotations = new float[MaxAfterimages];
        private int afterimageHead = 0;

        // ── Ribbon trail buffer (Mode 4: Harmonic Wave) ──
        private const int RibbonTrailLength = 40;
        private const float RibbonWidthHead = 18f;
        private const float RibbonWidthTail = 2f;
        private const float BladeLength = 88f;
        private Vector2[] ribbonPositions = new Vector2[RibbonTrailLength];
        private float[] ribbonRotations = new float[RibbonTrailLength];
        private int ribbonIndex = 0;
        private int ribbonCount = 0;

        // ── Per-phase Foundation constants ──
        private static readonly float[] SmearDistortPerPhase = { 0.04f, 0.05f, 0.06f };
        private static readonly float[] SmearFlowPerPhase = { 0.5f, 0.6f, 0.75f };
        private static readonly float[] RibbonWidthScale = { 1.0f, 1.1f, 1.25f };

        // ── Piecewise CurveSegment (Incisor-style windup → main swing → settle) ──
        private struct CurveSegment
        {
            public float StartX, EndX, StartY, EndY;
            public Func<float, float> Easing;
            public CurveSegment(float sx, float ex, float sy, float ey, Func<float, float> e = null)
            { StartX = sx; EndX = ex; StartY = sy; EndY = ey; Easing = e ?? (t => t); }
        }

        private static float SineOut(float t) => MathF.Sin(t * MathHelper.PiOver2);
        private static float QuadIn(float t) => t * t;
        private static float QuadOut(float t) => 1f - (1f - t) * (1f - t);
        private static float CubicIn(float t) => t * t * t;

        private static float PiecewiseAnimation(float t, CurveSegment[] segments)
        {
            t = MathHelper.Clamp(t, 0f, 1f);
            foreach (var seg in segments)
            {
                if (t >= seg.StartX && t <= seg.EndX)
                {
                    float localT = (t - seg.StartX) / Math.Max(seg.EndX - seg.StartX, 0.0001f);
                    return MathHelper.Lerp(seg.StartY, seg.EndY, seg.Easing(localT));
                }
            }
            return segments.Length > 0 ? segments[^1].EndY : 0f;
        }

        // Per-phase swing curves: graceful sakura petal arcs
        private static readonly CurveSegment[][] SwingCurves = new[]
        {
            // Phase 0: First Petal — gentle flowing arc
            new[] {
                new CurveSegment(0f, 0.22f, 0f, 0.10f, SineOut),
                new CurveSegment(0.22f, 0.80f, 0.10f, 0.92f, QuadIn),
                new CurveSegment(0.80f, 1.0f, 0.92f, 1.0f, SineOut),
            },
            // Phase 1: Scattered Petals — slightly faster
            new[] {
                new CurveSegment(0f, 0.20f, 0f, 0.10f, SineOut),
                new CurveSegment(0.20f, 0.78f, 0.10f, 0.93f, QuadIn),
                new CurveSegment(0.78f, 1.0f, 0.93f, 1.0f, SineOut),
            },
            // Phase 2: Final Bloom — dramatic finisher
            new[] {
                new CurveSegment(0f, 0.25f, 0f, 0.14f, QuadOut),
                new CurveSegment(0.25f, 0.83f, 0.14f, 0.94f, CubicIn),
                new CurveSegment(0.83f, 1.0f, 0.94f, 1.0f, SineOut),
            },
        };

        // ── Phase definitions ──
        private static readonly int[] PhaseDuration = { 18, 22, 26 };
        private static readonly float[] ArcStart = { -1.8f, 1.2f, -2.4f };
        private static readonly float[] ArcEnd = { 1.2f, -1.5f, 2.0f };

        public override void SetDefaults()
        {
            Projectile.width = 110;
            Projectile.height = 110;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 9999;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
            Projectile.noEnchantmentVisuals = true;
            Projectile.ownerHitCheck = true;
        }

        public override bool ShouldUpdatePosition() => false;

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float _ = 0f;
            Player player = Main.player[Projectile.owner];
            Vector2 start = player.MountedCenter;
            Vector2 end = start + swingRotation.ToRotationVector2() * BladeLength * Projectile.scale;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, 28f, ref _);
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            if (!player.channel || player.dead || !player.active)
            {
                Projectile.Kill();
                return;
            }

            Projectile.timeLeft = 2;

            if (!initialized)
            {
                initialized = true;
                swingDirection = player.direction;
                Projectile.direction = swingDirection;
                swingRotation = ArcStart[0] * swingDirection;
                for (int i = 0; i < MaxAfterimages; i++)
                    afterimageRotations[i] = swingRotation;
                for (int i = 0; i < RibbonTrailLength; i++)
                {
                    ribbonPositions[i] = player.MountedCenter;
                    ribbonRotations[i] = swingRotation;
                }
            }

            Projectile.Center = player.MountedCenter;

            // ── Meditation check (no nearby enemies + holding) ──
            if (!meditationCharged)
            {
                bool enemiesNearby = false;
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    if (Main.npc[i].active && !Main.npc[i].friendly && Main.npc[i].CanBeChasedBy()
                        && Vector2.Distance(player.MountedCenter, Main.npc[i].Center) < 400f)
                    {
                        enemiesNearby = true;
                        break;
                    }
                }
                if (!enemiesNearby)
                {
                    meditationTimer++;
                    if (meditationTimer >= 90) // 1.5 seconds
                    {
                        meditationCharged = true;
                        EroicaVFXLibrary.HaloBurst(player.MountedCenter, 0.8f);
                        SoundEngine.PlaySound(SoundID.Item29 with { Pitch = 0.4f, Volume = 0.5f }, player.MountedCenter);
                    }
                }
                else
                {
                    meditationTimer = 0f;
                }
            }

            // ── Swing interpolation (piecewise CurveSegment) ──
            int phaseIdx = (int)MathHelper.Clamp(ComboPhase, 0, 2);
            float duration = PhaseDuration[phaseIdx];
            float progress = MathHelper.Clamp(PhaseTimer / duration, 0f, 1f);
            float eased = PiecewiseAnimation(progress, SwingCurves[phaseIdx]);

            float arcScale = meditationCharged ? 1.4f : 1f;
            float startAngle = ArcStart[phaseIdx] * swingDirection * arcScale;
            float endAngle = ArcEnd[phaseIdx] * swingDirection * arcScale;
            swingRotation = MathHelper.Lerp(startAngle, endAngle, eased);
            Projectile.rotation = swingRotation;

            // Afterimage
            afterimageRotations[afterimageHead] = swingRotation;
            afterimageHead = (afterimageHead + 1) % MaxAfterimages;

            // ── Blade tip & ribbon recording ──
            Vector2 tipDir = swingRotation.ToRotationVector2();
            Vector2 tipPos = player.MountedCenter + tipDir * BladeLength;

            ribbonPositions[ribbonIndex] = tipPos;
            ribbonRotations[ribbonIndex] = swingRotation;
            ribbonIndex = (ribbonIndex + 1) % RibbonTrailLength;
            if (ribbonCount < RibbonTrailLength) ribbonCount++;

            SpawnPerFrameVFX(tipPos, phaseIdx, player);

            // ── Phase projectiles at midpoint ──
            if (!phaseProjectileSpawned && progress > 0.5f)
            {
                phaseProjectileSpawned = true;
                SpawnPhaseProjectiles(player, tipPos, phaseIdx);
            }

            // ── Advance timer ──
            PhaseTimer++;

            if (PhaseTimer >= duration)
            {
                OnPhaseEnd(player, tipPos, phaseIdx);
                ComboPhase = (ComboPhase + 1) % 3;
                PhaseTimer = 0;
                swingDirection = -swingDirection;
                Projectile.direction = swingDirection;
                phaseProjectileSpawned = false;
                if (meditationCharged) meditationCharged = false;

                for (int i = 0; i < MaxAfterimages; i++)
                    afterimageRotations[i] = ArcStart[(int)ComboPhase] * swingDirection;
                for (int i = 0; i < RibbonTrailLength; i++)
                    ribbonPositions[i] = player.MountedCenter;
                ribbonCount = 0;
            }

            // ── Player lock ──
            player.direction = Main.MouseWorld.X >= player.MountedCenter.X ? 1 : -1;
            player.heldProj = Projectile.whoAmI;
            player.itemAnimation = 2;
            player.itemTime = 2;
        }

        // EaseFlowing replaced by PiecewiseAnimation SwingCurves above

        #region VFX Spawning

        private void SpawnPerFrameVFX(Vector2 tipPos, int phase, Player player)
        {
            // Sakura petals from blade
            if ((int)PhaseTimer % 3 == 0)
                EroicaVFXLibrary.SpawnSakuraPetals(tipPos, 1 + phase, 15f);

            // Sakura music notes
            if ((int)PhaseTimer % 7 == 0)
                EroicaVFXLibrary.SpawnSakuraMusicNotes(tipPos, 1, 12f);

            // Soft sparkles
            if ((int)PhaseTimer % 4 == 0)
                EroicaVFXLibrary.SpawnValorSparkles(tipPos, 1 + phase, 12f);

            // Meditation aura — glowing rings while charged
            if (meditationCharged && (int)PhaseTimer % 8 == 0)
            {
                var ring = new BloomRingParticle(player.MountedCenter, Vector2.Zero, EroicaPalette.Sakura, 0.4f, 25, 0.06f);
                MagnumParticleHandler.SpawnParticle(ring);
            }

            // Dynamic lighting
            EroicaVFXLibrary.AddPaletteLighting(tipPos, 0.6f + phase * 0.1f, 0.5f + phase * 0.1f);
        }

        private void SpawnPhaseProjectiles(Player player, Vector2 tipPos, int phase)
        {
            if (Main.myPlayer != Projectile.owner) return;

            if (phase == 1) // Scattered Petals: 8 homing spectral copies
            {
                for (int i = 0; i < 8; i++)
                {
                    float angle = MathHelper.TwoPi * i / 8f;
                    Vector2 dir = angle.ToRotationVector2() * 5f;
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), tipPos, dir,
                        ModContent.ProjectileType<SakurasBlossomSpectral>(),
                        (int)(Projectile.damage * 0.3f), Projectile.knockBack * 0.3f, Projectile.owner);
                }
                SoundEngine.PlaySound(SoundID.Item43 with { Pitch = 0.3f, Volume = 0.6f }, player.MountedCenter);
            }
        }

        private void OnPhaseEnd(Player player, Vector2 tipPos, int phase)
        {
            EroicaVFXLibrary.MeleeImpact(tipPos, phase);

            if (phase == 2) // Final Bloom — massive petal burst
            {
                EroicaVFXLibrary.FinisherSlam(tipPos, 0.8f);
                EroicaVFXLibrary.SpawnSakuraPetals(tipPos, 15, 80f);
                EroicaVFXLibrary.SpawnSakuraMusicNotes(tipPos, 5, 40f);
                EroicaVFXLibrary.HaloBurst(tipPos, 1f);
            }
            else
            {
                EroicaVFXLibrary.SpawnSakuraPetals(tipPos, 5 + phase * 3, 40f);
            }
        }

        #endregion

        #region On Hit

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<MusicsDissonance>(), 240);
            target.AddBuff(ModContent.BuffType<SakuraBlight>(), 180);
            target.AddBuff(ModContent.BuffType<PetalWound>(), 120);

            int phase = (int)MathHelper.Clamp(ComboPhase, 0, 2);
            EroicaVFXLibrary.MeleeImpact(target.Center, phase);
            EroicaVFXLibrary.SpawnSakuraPetals(target.Center, 4 + phase * 2, 30f);
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            // Meditation charged: 2x damage on next swing
            if (meditationCharged)
                modifiers.FinalDamage *= 2f;

            int phase = (int)MathHelper.Clamp(ComboPhase, 0, 2);
            modifiers.FinalDamage *= 1f + phase * 0.1f;
        }

        #endregion

        #region Rendering

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Player player = Main.player[Projectile.owner];

            Texture2D bladeTex = TextureAssets.Item[ModContent.ItemType<SakurasBlossom>()].Value;
            Vector2 bladeOrigin = new Vector2(0, bladeTex.Height);
            float bladeScale = 1.1f * (meditationCharged ? 1.15f : 1f);
            SpriteEffects flip = swingDirection > 0 ? SpriteEffects.None : SpriteEffects.FlipVertically;

            int phase = (int)MathHelper.Clamp(ComboPhase, 0, 2);
            float progress = MathHelper.Clamp(PhaseTimer / (float)PhaseDuration[phase], 0f, 1f);
            float comboNorm = phase / 2f;
            float time = (float)Main.gameTimeCache.TotalGameTime.TotalSeconds;
            Vector2 playerDraw = player.MountedCenter - Main.screenPosition;
            float drawBaseRot = swingRotation + (swingDirection > 0 ? -MathHelper.PiOver4 : MathHelper.PiOver4 + MathHelper.Pi);

            // ═══════════════════════════════════════════════════════════════════
            //  LAYER 1: SAKURA SWING TRAIL SMEAR — SakuraSwingTrail.fx
            //  Uses SakuraTrailFlow / SakuraTrailGlow for petal-shimmer distortion
            //  instead of generic SmearDistortShader. Gentle, graceful, katana-like.
            // ═══════════════════════════════════════════════════════════════════
            DrawSakuraSmearArc(sb, phase, progress, time, playerDraw, drawBaseRot, bladeScale, flip);

            // ═══════════════════════════════════════════════════════════════════
            //  LAYER 2: SHADER-DRIVEN PETAL RIBBON — SakuraSwingTrail.fx on strip
            //  Dual pass: SakuraTrailFlow (petal shimmer body) + SakuraTrailGlow.
            // ═══════════════════════════════════════════════════════════════════
            DrawShaderPetalRibbon(sb, phase, time);

            // ═══════════════════════════════════════════════════════════════════
            //  LAYER 3: SHADER AFTERIMAGES — SakuraTrailGlow pass
            // ═══════════════════════════════════════════════════════════════════
            DrawShaderAfterimages(sb, bladeTex, bladeOrigin, bladeScale, flip, playerDraw, phase, drawBaseRot, time);

            // ═══════════════════════════════════════════════════════════════════
            //  LAYER 4: MAIN BLADE + SAKURA GLOW
            // ═══════════════════════════════════════════════════════════════════
            float paletteT = 0.4f + phase * 0.1f;
            Color bladeTint = Color.Lerp(lightColor, EroicaPalette.PaletteLerp(EroicaPalette.SakurasBlossomBlade, paletteT), 0.3f);
            sb.Draw(bladeTex, playerDraw, null, bladeTint, drawBaseRot, bladeOrigin, bladeScale, flip, 0f);

            // Glow overlays need additive blend (A=0 colors are invisible under AlphaBlend)
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Color bladeGlow = EroicaPalette.Sakura with { A = 0 };
            sb.Draw(bladeTex, playerDraw, null, bladeGlow * 0.25f, drawBaseRot, bladeOrigin, bladeScale * 1.03f, flip, 0f);

            // Petal edge glow on phase 2 (Final Bloom)
            if (phase >= 2)
            {
                Color petalEdge = EroicaPalette.PollenGold with { A = 0 };
                sb.Draw(bladeTex, playerDraw, null, petalEdge * 0.12f, drawBaseRot, bladeOrigin, bladeScale * 1.06f, flip, 0f);
            }

            // Meditation charge overlay
            if (meditationCharged)
            {
                Color meditGlow = EroicaPalette.PollenGold with { A = 0 };
                float meditPulse = 0.7f + MathF.Sin(time * 4f) * 0.3f;
                sb.Draw(bladeTex, playerDraw, null, meditGlow * (0.15f * meditPulse), drawBaseRot, bladeOrigin, bladeScale * 1.05f, flip, 0f);
            }

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // ═══════════════════════════════════════════════════════════════════
            //  LAYER 5: BLADE TIP BLOOM + SakuraBloom shader overlay
            // ═══════════════════════════════════════════════════════════════════
            DrawShaderBladeTipBloom(sb, player, phase, time);

            // ═══════════════════════════════════════════════════════════════════
            //  LAYER 6: MEDITATION AURA — PetalDissolve.fx for mystic petal glow
            // ═══════════════════════════════════════════════════════════════════
            if (meditationCharged)
                DrawShaderMeditationGlow(sb, player, time);

            // ═══════════════════════════════════════════════════════════════════
            //  LAYER 7: SAKURA BLOOM HALO — SakuraBloom.fx on Final Bloom phase
            // ═══════════════════════════════════════════════════════════════════
            if (phase == 2)
                DrawSakuraBloomOverlay(sb, player, progress, time);

            // Eroica theme accent
            EroicaVFXLibrary.BeginEroicaAdditive(sb);
            EroicaVFXLibrary.DrawThemeSakuraAccent(sb, Projectile.Center, 1f, 0.3f);
            EroicaVFXLibrary.EndEroicaAdditive(sb);

            return false;
        }

        // ═══════════════════════════════════════════════════════════════════════
        //  LAYER 1: Sakura Smear Arc — SakuraSwingTrail.fx dual-pass
        //  Pass 1: SakuraTrailFlow (petal shimmer body with dual-frequency petal)
        //  Pass 2: SakuraTrailGlow (bright overbright sakura halo)
        // ═══════════════════════════════════════════════════════════════════════
        private void DrawSakuraSmearArc(SpriteBatch sb, int phase, float progress, float time,
            Vector2 playerDraw, float drawBaseRot, float bladeScale, SpriteEffects flip)
        {
            // Use the curved sword slash for graceful katana arcs
            Texture2D smearTex = EroicaTextures.CurvedSwordSlash?.Value;
            if (smearTex == null)
                smearTex = EroicaTextures.SimpleArcSwordSlash?.Value;
            if (smearTex == null) return;

            Vector2 smearOrigin = new Vector2(0, smearTex.Height * 0.5f);
            float comboNorm = phase / 2f;

            // Smear fade: ramp in → hold → ramp out
            float smearAlpha;
            if (progress < 0.1f)
                smearAlpha = progress / 0.1f;
            else if (progress > 0.85f)
                smearAlpha = (1f - progress) / 0.15f;
            else
                smearAlpha = 1f;

            // ── SHADER PATH: SakuraSwingTrail.fx ──
            if (EroicaShaderManager.HasSakuraSwingTrail)
            {
                EroicaShaderManager.BeginShaderAdditive(sb);
                try
                {
                    // Sub-layer A: Wide outer sakura haze — SakuraTrailGlow pass
                    EroicaShaderManager.ApplySakurasBlossomSwingTrail(time, comboNorm, glowPass: true);
                    sb.Draw(smearTex, playerDraw, null,
                        Color.White * smearAlpha * 0.35f, drawBaseRot, smearOrigin,
                        bladeScale * 1.15f, flip, 0f);

                    // Sub-layer B: Main sakura petal body — SakuraTrailFlow pass
                    EroicaShaderManager.ApplySakurasBlossomSwingTrail(time, comboNorm, glowPass: false);
                    sb.Draw(smearTex, playerDraw, null,
                        Color.White * smearAlpha * 0.6f, drawBaseRot, smearOrigin,
                        bladeScale * 1.04f, flip, 0f);

                    // Sub-layer C: Bright petal-white core
                    EroicaShaderManager.ApplySakuraSwingTrail(time,
                        Color.White, EroicaPalette.PollenGold,
                        glowPass: false,
                        scrollSpeed: 1.5f + comboNorm * 0.3f,
                        distortionAmt: 0.03f,
                        overbrightMult: 3.5f + comboNorm * 1.0f,
                        phase: comboNorm);
                    sb.Draw(smearTex, playerDraw, null,
                        Color.White * smearAlpha * 0.35f, drawBaseRot, smearOrigin,
                        bladeScale * 0.92f, flip, 0f);

                    // Sub-layer D: Meditation-charged extra glow
                    if (meditationCharged)
                    {
                        EroicaShaderManager.ApplySakurasBlossomSwingTrail(time, 1f, glowPass: true);
                        sb.Draw(smearTex, playerDraw, null,
                            Color.White * smearAlpha * 0.25f, drawBaseRot, smearOrigin,
                            bladeScale * 1.2f, flip, 0f);
                    }
                }
                finally
                {
                    EroicaShaderManager.RestoreSpriteBatch(sb);
                }
            }
            else
            {
                // ── FALLBACK: Static sakura-tinted layers ──
                EroicaShaderManager.BeginAdditive(sb);
                try
                {
                    Color outerCol = EroicaPalette.Sakura with { A = 0 };
                    Color mainCol = Color.Lerp(EroicaPalette.Sakura, Color.White, 0.3f) with { A = 0 };
                    Color coreCol = Color.Lerp(Color.White, EroicaPalette.Sakura, 0.15f) with { A = 0 };

                    sb.Draw(smearTex, playerDraw, null, outerCol * smearAlpha * 0.25f, drawBaseRot, smearOrigin, bladeScale * 1.12f, flip, 0f);
                    sb.Draw(smearTex, playerDraw, null, mainCol * smearAlpha * 0.45f, drawBaseRot, smearOrigin, bladeScale * 1.04f, flip, 0f);
                    sb.Draw(smearTex, playerDraw, null, coreCol * smearAlpha * 0.3f, drawBaseRot, smearOrigin, bladeScale * 0.96f, flip, 0f);
                }
                finally
                {
                    EroicaShaderManager.RestoreSpriteBatch(sb);
                }
            }
        }

        // ═══════════════════════════════════════════════════════════════════════
        //  LAYER 2: Shader Petal Ribbon — SakuraSwingTrail.fx on blade tip strip
        //  Dual pass: petal shimmer body + sakura glow halo with standing wave modulation.
        // ═══════════════════════════════════════════════════════════════════════
        private void DrawShaderPetalRibbon(SpriteBatch sb, int phase, float time)
        {
            if (ribbonCount < 3) return;

            int drawCount = Math.Min(ribbonCount, RibbonTrailLength);
            Vector2[] positions = new Vector2[drawCount];
            for (int i = 0; i < drawCount; i++)
            {
                int idx = (ribbonIndex - drawCount + i + RibbonTrailLength) % RibbonTrailLength;
                positions[i] = ribbonPositions[idx];
            }

            Texture2D stripTex = EroicaTextures.EnergyTrailUV?.Value ?? EroicaTextures.SparkleField?.Value;
            if (stripTex == null) return;

            int texW = stripTex.Width;
            int texH = stripTex.Height;
            float scrollTime = (float)Main.timeForVisualEffects * 0.005f;
            int srcWidth = Math.Max(1, texW / drawCount);
            float widthScale = RibbonWidthScale[phase];
            float comboNorm = phase / 2f;

            bool hasShader = EroicaShaderManager.HasSakuraSwingTrail;

            if (hasShader)
            {
                // ── PASS 1: Petal shimmer body (SakuraTrailFlow) ──
                EroicaShaderManager.BeginShaderAdditive(sb);
                try
                {
                    EroicaShaderManager.ApplySakurasBlossomSwingTrail(time, comboNorm, glowPass: false);

                    for (int i = 0; i < drawCount - 1; i++)
                    {
                        float progress = (float)i / drawCount;
                        float fade = progress * progress;
                        if (fade < 0.01f) continue;

                        // Smooth tip fade-in/out to prevent hard trail endpoints
                        float tipFade = MathHelper.Clamp(progress * 6f, 0f, 1f) * MathHelper.Clamp((1f - progress) * 4f, 0f, 1f);

                        float width = MathHelper.Lerp(RibbonWidthTail, RibbonWidthHead, progress) * widthScale;

                        // Gentler standing wave modulation to reduce seam visibility
                        float waveFreq = 4f + phase;
                        float wave = 0.8f + 0.2f * MathF.Sin(progress * waveFreq * MathHelper.Pi + time * 3f);
                        width *= wave;

                        Vector2 segDir = positions[i + 1] - positions[i];
                        float segLength = segDir.Length();
                        if (segLength < 0.5f) continue;
                        float segAngle = segDir.ToRotation();

                        float uStart = (progress + scrollTime * 2f) % 1f;
                        int srcX = (int)(uStart * texW) % texW;
                        Rectangle srcRect = new Rectangle(srcX, 0, srcWidth, texH);

                        float scaleX = segLength / (float)srcWidth;
                        float scaleY = width / (float)texH;
                        Vector2 pos = positions[i] - Main.screenPosition;
                        Vector2 origin = new Vector2(0, texH / 2f);

                        sb.Draw(stripTex, pos, srcRect, Color.White * (fade * tipFade * 0.55f), segAngle, origin,
                            new Vector2(scaleX, scaleY), SpriteEffects.None, 0f);
                    }
                }
                finally
                {
                    EroicaShaderManager.RestoreSpriteBatch(sb);
                }

                // ── PASS 2: Sakura glow halo (SakuraTrailGlow) — wider, softer ──
                EroicaShaderManager.BeginShaderAdditive(sb);
                try
                {
                    EroicaShaderManager.ApplySakurasBlossomSwingTrail(time, comboNorm, glowPass: true);

                    for (int i = 0; i < drawCount - 1; i++)
                    {
                        float progress = (float)i / drawCount;
                        float fade = progress * progress;
                        if (fade < 0.02f) continue;

                        // Smooth tip fade-in/out
                        float tipFade = MathHelper.Clamp(progress * 6f, 0f, 1f) * MathHelper.Clamp((1f - progress) * 4f, 0f, 1f);

                        float width = MathHelper.Lerp(RibbonWidthTail, RibbonWidthHead, progress) * widthScale * 1.5f;
                        float waveFreq = 4f + phase;
                        float wave = 0.8f + 0.2f * MathF.Sin(progress * waveFreq * MathHelper.Pi + time * 3f);
                        width *= wave;

                        Vector2 segDir = positions[i + 1] - positions[i];
                        float segLength = segDir.Length();
                        if (segLength < 0.5f) continue;
                        float segAngle = segDir.ToRotation();

                        float uStart = (progress + scrollTime * 2f) % 1f;
                        int srcX = (int)(uStart * texW) % texW;
                        Rectangle srcRect = new Rectangle(srcX, 0, srcWidth, texH);

                        float scaleX = segLength / (float)srcWidth;
                        float scaleY = width / (float)texH;
                        Vector2 pos = positions[i] - Main.screenPosition;
                        Vector2 origin = new Vector2(0, texH / 2f);

                        sb.Draw(stripTex, pos, srcRect, Color.White * (fade * tipFade * 0.3f), segAngle, origin,
                            new Vector2(scaleX, scaleY), SpriteEffects.None, 0f);
                    }
                }
                finally
                {
                    EroicaShaderManager.RestoreSpriteBatch(sb);
                }
            }
            else
            {
                // Fallback: basic additive sakura strip
                EroicaShaderManager.BeginAdditive(sb);
                try
                {
                    for (int i = 0; i < drawCount - 1; i++)
                    {
                        float progress = (float)i / drawCount;
                        float fade = progress * progress;
                        if (fade < 0.01f) continue;

                        // Smooth tip fade-in/out
                        float tipFade = MathHelper.Clamp(progress * 6f, 0f, 1f) * MathHelper.Clamp((1f - progress) * 4f, 0f, 1f);

                        float width = MathHelper.Lerp(RibbonWidthTail, RibbonWidthHead, progress) * widthScale;
                        float waveFreq = 4f + phase;
                        float wave = 0.8f + 0.2f * MathF.Sin(progress * waveFreq * MathHelper.Pi + scrollTime * 3f);
                        width *= wave;

                        Vector2 segDir = positions[i + 1] - positions[i];
                        float segLength = segDir.Length();
                        if (segLength < 0.5f) continue;
                        float segAngle = segDir.ToRotation();

                        float uStart = (progress + scrollTime * 2f) % 1f;
                        int srcX = (int)(uStart * texW) % texW;
                        Rectangle srcRect = new Rectangle(srcX, 0, srcWidth, texH);
                        float scaleX = segLength / (float)srcWidth;
                        float scaleY = width / (float)texH;
                        Vector2 pos = positions[i] - Main.screenPosition;
                        Vector2 origin = new Vector2(0, texH / 2f);

                        Color bodyColor = Color.Lerp(EroicaPalette.Sakura, Color.White, progress * 0.5f) with { A = 0 };
                        sb.Draw(stripTex, pos, srcRect, bodyColor * (fade * tipFade * 0.55f), segAngle, origin,
                            new Vector2(scaleX, scaleY), SpriteEffects.None, 0f);
                    }
                }
                finally
                {
                    EroicaShaderManager.RestoreSpriteBatch(sb);
                }
            }

            // ── Bloom points along trail ──
            Texture2D bloomTex = MagnumTextureRegistry.GetBloom();
            if (bloomTex != null)
            {
                EroicaShaderManager.BeginAdditive(sb);
                try
                {
                    Vector2 bloomOrigin = bloomTex.Size() * 0.5f;
                    int bloomStep = Math.Max(1, drawCount / 8);
                    for (int i = 0; i < drawCount; i += bloomStep)
                    {
                        float progress = (float)i / drawCount;
                        if (progress < 0.2f) continue;
                        float bloomFade = progress * progress * 0.25f;
                        Color bloomCol = EroicaPalette.Sakura with { A = 0 };
                        float bloomScale = MathHelper.Lerp(0.02f, 0.06f, progress) * widthScale;
                        sb.Draw(bloomTex, positions[i] - Main.screenPosition, null,
                            bloomCol * bloomFade, 0f, bloomOrigin, bloomScale, SpriteEffects.None, 0f);
                    }
                }
                finally
                {
                    EroicaShaderManager.RestoreSpriteBatch(sb);
                }
            }
        }

        // ═══════════════════════════════════════════════════════════════════════
        //  LAYER 3: Shader Afterimages — SakuraTrailGlow pass on blade sprites
        // ═══════════════════════════════════════════════════════════════════════
        private void DrawShaderAfterimages(SpriteBatch sb, Texture2D tex, Vector2 origin, float scale,
            SpriteEffects flip, Vector2 playerDraw, int phase, float drawBaseRot, float time)
        {
            int count = 5 + phase;
            float comboNorm = phase / 2f;

            if (EroicaShaderManager.HasSakuraSwingTrail)
            {
                EroicaShaderManager.BeginShaderAdditive(sb);
                try
                {
                    for (int i = 0; i < count && i < MaxAfterimages; i++)
                    {
                        int idx = (afterimageHead - 2 - i + MaxAfterimages) % MaxAfterimages;
                        float rot = afterimageRotations[idx];
                        float afterDrawRot = rot + (swingDirection > 0 ? -MathHelper.PiOver4 : MathHelper.PiOver4 + MathHelper.Pi);

                        float fade = 1f - (float)(i + 1) / (count + 1);
                        fade *= fade;

                        EroicaShaderManager.ApplySakuraSwingTrail(
                            time + i * 0.08f,
                            EroicaPalette.Sakura, EroicaPalette.PollenGold,
                            glowPass: true,
                            scrollSpeed: 0.8f + comboNorm * 0.3f,
                            distortionAmt: 0.02f + i * 0.008f,
                            overbrightMult: 2.0f * fade,
                            phase: comboNorm);

                        float afterScale = scale * (1f - i * 0.02f);
                        sb.Draw(tex, playerDraw, null, Color.White * (fade * 0.3f), afterDrawRot, origin, afterScale, flip, 0f);
                    }
                }
                finally
                {
                    EroicaShaderManager.RestoreSpriteBatch(sb);
                }
            }
            else
            {
                for (int i = 0; i < count && i < MaxAfterimages; i++)
                {
                    int idx = (afterimageHead - 2 - i + MaxAfterimages) % MaxAfterimages;
                    float rot = afterimageRotations[idx];
                    float afterDrawRot = rot + (swingDirection > 0 ? -MathHelper.PiOver4 : MathHelper.PiOver4 + MathHelper.Pi);

                    float fade = 1f - (float)(i + 1) / (count + 1);
                    fade *= fade;

                    Color col = Color.Lerp(EroicaPalette.Sakura, EroicaPalette.PollenGold, (float)i / count) * (fade * 0.3f);
                    col.A = 0;

                    sb.Draw(tex, playerDraw, null, col, afterDrawRot, origin, scale * (1f - i * 0.02f), flip, 0f);
                }
            }
        }

        // ═══════════════════════════════════════════════════════════════════════
        //  LAYER 5: Shader Blade Tip Bloom — SakuraBloom.fx petal bloom at tip
        // ═══════════════════════════════════════════════════════════════════════
        private void DrawShaderBladeTipBloom(SpriteBatch sb, Player player, int phase, float time)
        {
            Vector2 tipPos = player.MountedCenter + swingRotation.ToRotationVector2() * BladeLength;
            float comboNorm = phase / 2f;

            // Standard bloom stack
            EroicaVFXLibrary.DrawEroicaBloomStack(sb, tipPos,
                EroicaPalette.Sakura, EroicaPalette.PollenGold,
                0.25f + phase * 0.06f, 0.7f + phase * 0.1f);

            // ── SakuraBloom shader petal halo at blade tip ──
            Texture2D bloom = MagnumTextureRegistry.GetBloom();
            if (bloom != null && EroicaShaderManager.HasSakuraBloom)
            {
                Vector2 bloomOrigin = bloom.Size() * 0.5f;
                Vector2 bloomDrawPos = tipPos - Main.screenPosition;

                EroicaShaderManager.BeginShaderAdditive(sb);
                try
                {
                    EroicaShaderManager.ApplySakurasBlossomPetalBurst(time, comboNorm, glowPass: false);
                    sb.Draw(bloom, bloomDrawPos, null, Color.White * (0.2f + comboNorm * 0.1f),
                        0f, bloomOrigin, 0.04f + phase * 0.008f, SpriteEffects.None, 0f);

                    EroicaShaderManager.ApplySakurasBlossomPetalBurst(time, comboNorm, glowPass: true);
                    sb.Draw(bloom, bloomDrawPos, null, Color.White * (0.15f + comboNorm * 0.08f),
                        time * 0.5f, bloomOrigin, 0.06f + phase * 0.01f, SpriteEffects.None, 0f);
                }
                finally
                {
                    EroicaShaderManager.RestoreSpriteBatch(sb);
                }
            }

            // Sakura petal accent at tip
            Texture2D petalTex = EroicaThemeTextures.ERSakuraPetal;
            if (petalTex != null)
            {
                Vector2 petalOrigin = petalTex.Size() * 0.5f;
                Vector2 petalDrawPos = tipPos - Main.screenPosition;
                float petalRot = time * 1.5f;
                float petalPulse = 0.7f + MathF.Sin(time * 3.5f) * 0.3f;
                Color petalCol = EroicaPalette.Sakura with { A = 0 };
                sb.Draw(petalTex, petalDrawPos, null, petalCol * (0.25f * petalPulse), petalRot, petalOrigin,
                    0.22f + phase * 0.04f, SpriteEffects.None, 0f);
            }
        }

        // ═══════════════════════════════════════════════════════════════════════
        //  LAYER 6: Shader Meditation Glow — PetalDissolve.fx mystic petal aura
        // ═══════════════════════════════════════════════════════════════════════
        private void DrawShaderMeditationGlow(SpriteBatch sb, Player player, float time)
        {
            Vector2 drawPos = player.MountedCenter - Main.screenPosition;
            float pulse = 0.8f + MathF.Sin(time * 3f) * 0.2f;

            if (EroicaShaderManager.HasPetalDissolve)
            {
                Texture2D bloom = MagnumTextureRegistry.GetBloom();
                if (bloom != null)
                {
                    Vector2 bloomOrigin = bloom.Size() * 0.5f;

                    EroicaShaderManager.BeginShaderAdditive(sb);
                    try
                    {
                        // Inner dissolve aura — solid petal ring
                        EroicaShaderManager.ApplySakurasBlossomDissolve(time, 0.3f, glowPass: false);
                        sb.Draw(bloom, drawPos, null, Color.White * (0.2f * pulse),
                            0f, bloomOrigin, 0.06f, SpriteEffects.None, 0f);

                        // Outer dissolve haze — partially dissolved petals
                        EroicaShaderManager.ApplySakurasBlossomDissolve(time, 0.6f, glowPass: true);
                        sb.Draw(bloom, drawPos, null, Color.White * (0.12f * pulse),
                            time * 0.2f, bloomOrigin, 0.1f, SpriteEffects.None, 0f);
                    }
                    finally
                    {
                        EroicaShaderManager.RestoreSpriteBatch(sb);
                    }
                }
            }
            else
            {
                // Fallback
                Texture2D bloom = MagnumTextureRegistry.GetBloom();
                if (bloom != null)
                {
                    Vector2 bloomOrigin = bloom.Size() * 0.5f;
                    Color meditColor = EroicaPalette.Sakura with { A = 0 };
                    sb.Draw(bloom, drawPos, null, meditColor * (0.18f * pulse), 0f, bloomOrigin, 0.12f, SpriteEffects.None, 0f);

                    Color innerColor = EroicaPalette.PollenGold with { A = 0 };
                    sb.Draw(bloom, drawPos, null, innerColor * (0.12f * pulse), 0f, bloomOrigin, 0.08f, SpriteEffects.None, 0f);
                }
            }

            // Meditation ring overlay
            Texture2D ringTex = EroicaThemeTextures.ERPowerEffectRing;
            if (ringTex != null)
            {
                Vector2 ringOrigin = ringTex.Size() * 0.5f;
                float ringRot = time * 0.7f;
                float ringPulse = 0.6f + MathF.Sin(time * 2.5f) * 0.4f;
                Color ringCol = Color.Lerp(EroicaPalette.Sakura, EroicaPalette.PollenGold, ringPulse) with { A = 0 };
                sb.Draw(ringTex, drawPos, null, ringCol * (0.12f * ringPulse), ringRot, ringOrigin,
                    0.4f, SpriteEffects.None, 0f);
            }
        }

        // ═══════════════════════════════════════════════════════════════════════
        //  LAYER 7: Sakura Bloom Overlay — SakuraBloom.fx on Final Bloom phase
        // ═══════════════════════════════════════════════════════════════════════
        private void DrawSakuraBloomOverlay(SpriteBatch sb, Player player, float progress, float time)
        {
            Texture2D bloom = MagnumTextureRegistry.GetBloom();
            if (bloom == null) return;

            Vector2 drawPos = player.MountedCenter - Main.screenPosition;
            Vector2 bloomOrigin = bloom.Size() * 0.5f;

            if (EroicaShaderManager.HasSakuraBloom)
            {
                EroicaShaderManager.BeginShaderAdditive(sb);
                try
                {
                    EroicaShaderManager.ApplySakurasBlossomPetalBurst(time, progress, glowPass: false);
                    sb.Draw(bloom, drawPos, null, Color.White * (0.25f * progress),
                        0f, bloomOrigin, 0.1f * progress, SpriteEffects.None, 0f);

                    EroicaShaderManager.ApplySakurasBlossomPetalBurst(time, progress, glowPass: true);
                    sb.Draw(bloom, drawPos, null, Color.White * (0.15f * progress),
                        time * 0.3f, bloomOrigin, 0.139f * progress, SpriteEffects.None, 0f);
                }
                finally
                {
                    EroicaShaderManager.RestoreSpriteBatch(sb);
                }
            }
            else
            {
                Color sakuraGlow = EroicaPalette.Sakura with { A = 0 };
                EroicaShaderManager.BeginAdditive(sb);
                try
                {
                    sb.Draw(bloom, drawPos, null, sakuraGlow * (0.2f * progress), 0f, bloomOrigin,
                        MathHelper.Min(1.2f * progress, 0.139f), SpriteEffects.None, 0f);
                }
                finally
                {
                    EroicaShaderManager.RestoreSpriteBatch(sb);
                }
            }
        }

        #endregion
    }
}