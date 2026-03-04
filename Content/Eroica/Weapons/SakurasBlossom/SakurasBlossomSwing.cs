using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Bloom;
using MagnumOpus.Common.Systems.Particles;
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
                    ribbonPositions[i] = player.Center;
                    ribbonRotations[i] = swingRotation;
                }
            }

            Projectile.Center = player.Center;

            // ── Meditation check (no nearby enemies + holding) ──
            if (!meditationCharged)
            {
                bool enemiesNearby = false;
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    if (Main.npc[i].active && !Main.npc[i].friendly && Main.npc[i].CanBeChasedBy()
                        && Vector2.Distance(player.Center, Main.npc[i].Center) < 400f)
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
                        EroicaVFXLibrary.HaloBurst(player.Center, 0.8f);
                        SoundEngine.PlaySound(SoundID.Item29 with { Pitch = 0.4f, Volume = 0.5f }, player.Center);
                    }
                }
                else
                {
                    meditationTimer = 0f;
                }
            }

            // ── Swing interpolation ──
            int phaseIdx = (int)MathHelper.Clamp(ComboPhase, 0, 2);
            float duration = PhaseDuration[phaseIdx];
            float progress = MathHelper.Clamp(PhaseTimer / duration, 0f, 1f);
            float eased = EaseFlowing(progress);

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
            Vector2 tipPos = player.Center + tipDir * BladeLength;

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
                    ribbonPositions[i] = player.Center;
                ribbonCount = 0;
            }

            // ── Player lock ──
            player.direction = Main.MouseWorld.X >= player.Center.X ? 1 : -1;
            player.heldProj = Projectile.whoAmI;
            player.itemAnimation = 2;
            player.itemTime = 2;
        }

        private static float EaseFlowing(float t)
        {
            // Smooth flowing ease — gentler than standard for sakura grace
            return MathF.Sin(t * MathHelper.PiOver2);
        }

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
                var ring = new BloomRingParticle(player.Center, Vector2.Zero, EroicaPalette.Sakura, 0.4f, 25, 0.06f);
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
                SoundEngine.PlaySound(SoundID.Item43 with { Pitch = 0.3f, Volume = 0.6f }, player.Center);
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
            Vector2 playerDraw = player.Center - Main.screenPosition;
            float drawBaseRot = swingRotation + (swingDirection > 0 ? -MathHelper.PiOver4 : MathHelper.PiOver4 + MathHelper.Pi);

            // ── Layer 1: SmearDistort arc overlay (SwordSmearFoundation) ──
            DrawSmearArc(sb, phase, playerDraw, drawBaseRot, bladeScale, flip);

            // ── Layer 2: Harmonic Wave ribbon trail (RibbonFoundation Mode 4) ──
            DrawHarmonicRibbon(sb, phase);

            // ── Layer 3: Sakura petal afterimage trail ──
            DrawAfterimages(sb, bladeTex, bladeOrigin, bladeScale, flip, playerDraw, phase);

            // ── Layer 4: Main blade sprite + inner glow ──
            float paletteT = 0.4f + phase * 0.1f;
            Color bladeTint = Color.Lerp(lightColor, EroicaPalette.PaletteLerp(EroicaPalette.SakurasBlossomBlade, paletteT), 0.3f);
            sb.Draw(bladeTex, playerDraw, null, bladeTint, drawBaseRot, bladeOrigin, bladeScale, flip, 0f);

            Color bladeGlow = EroicaPalette.Sakura with { A = 0 };
            sb.Draw(bladeTex, playerDraw, null, bladeGlow * 0.25f, drawBaseRot, bladeOrigin, bladeScale * 1.03f, flip, 0f);

            // ── Layer 5: Bloom at blade tip ──
            DrawBladeTipBloom(sb, player, phase);

            // ── Layer 6: Meditation aura ──
            if (meditationCharged)
                DrawMeditationGlow(sb, player);

            // Eroica theme accent
            EroicaVFXLibrary.BeginEroicaAdditive(sb);
            EroicaVFXLibrary.DrawThemeSakuraAccent(sb, Projectile.Center, 1f, 0.3f);
            EroicaVFXLibrary.EndEroicaAdditive(sb);

            return false;
        }

        // ═══════════════════════════════════════════════════════════════════
        // LAYER 1: SmearDistort shader arc (gentler than Valor — katana grace)
        // ═══════════════════════════════════════════════════════════════════

        private void DrawSmearArc(SpriteBatch sb, int phase, Vector2 playerDraw, float drawBaseRot,
            float bladeScale, SpriteEffects flip)
        {
            // Use FlamingSwordArc from SwordSmearFoundation (fiery arc for katana)
            Texture2D smearTex = SMFTextures.FlamingSwordArc.Value;
            if (smearTex == null) return;

            Vector2 smearOrigin = new Vector2(0, smearTex.Height * 0.5f);
            float baseDistort = SmearDistortPerPhase[phase];
            float baseFlow = SmearFlowPerPhase[phase];
            float time = (float)Main.timeForVisualEffects * 0.008f;

            // Try shader path
            Effect smearShader = SMFTextures.SmearDistortShader;
            Texture2D noiseTex = SMFTextures.FBMNoise.Value;
            Texture2D gradientTex = EroicaThemeTextures.ERGradientLUT;

            if (smearShader != null && noiseTex != null && gradientTex != null)
            {
                sb.End();

                // Sub-layer A: Outer sakura haze (very gentle distortion)
                sb.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearWrap,
                    DepthStencilState.None, RasterizerState.CullCounterClockwise, null,
                    Main.GameViewMatrix.TransformationMatrix);
                try
                {
                smearShader.Parameters["uTime"]?.SetValue(time);
                smearShader.Parameters["fadeAlpha"]?.SetValue(0.2f);
                smearShader.Parameters["distortStrength"]?.SetValue(baseDistort * 1.2f);
                smearShader.Parameters["flowSpeed"]?.SetValue(baseFlow * 0.8f);
                smearShader.Parameters["noiseScale"]?.SetValue(1.8f);
                smearShader.GraphicsDevice.Textures[1] = noiseTex;
                smearShader.GraphicsDevice.SamplerStates[1] = SamplerState.LinearWrap;
                smearShader.GraphicsDevice.Textures[2] = gradientTex;
                smearShader.GraphicsDevice.SamplerStates[2] = SamplerState.LinearClamp;
                smearShader.CurrentTechnique.Passes[0].Apply();

                Color outerCol = EroicaPalette.Sakura with { A = 0 };
                sb.Draw(smearTex, playerDraw, null, outerCol * 0.25f, drawBaseRot, smearOrigin,
                    bladeScale * 1.12f, flip, 0f);
                sb.End();

                // Sub-layer B: Main sakura smear
                sb.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearWrap,
                    DepthStencilState.None, RasterizerState.CullCounterClockwise, null,
                    Main.GameViewMatrix.TransformationMatrix);

                smearShader.Parameters["fadeAlpha"]?.SetValue(0.5f);
                smearShader.Parameters["distortStrength"]?.SetValue(baseDistort);
                smearShader.Parameters["flowSpeed"]?.SetValue(baseFlow);
                smearShader.Parameters["noiseScale"]?.SetValue(2.2f);
                smearShader.CurrentTechnique.Passes[0].Apply();

                Color mainCol = Color.Lerp(EroicaPalette.Sakura, Color.White, 0.3f) with { A = 0 };
                sb.Draw(smearTex, playerDraw, null, mainCol * 0.45f, drawBaseRot, smearOrigin,
                    bladeScale * 1.04f, flip, 0f);
                sb.End();

                // Sub-layer C: Bright petal-white core (very subtle)
                sb.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearWrap,
                    DepthStencilState.None, RasterizerState.CullCounterClockwise, null,
                    Main.GameViewMatrix.TransformationMatrix);

                smearShader.Parameters["fadeAlpha"]?.SetValue(0.35f);
                smearShader.Parameters["distortStrength"]?.SetValue(baseDistort * 0.4f);
                smearShader.Parameters["flowSpeed"]?.SetValue(baseFlow * 1.2f);
                smearShader.Parameters["noiseScale"]?.SetValue(2.8f);
                smearShader.CurrentTechnique.Passes[0].Apply();

                Color coreCol = Color.Lerp(Color.White, EroicaPalette.Sakura, 0.15f) with { A = 0 };
                sb.Draw(smearTex, playerDraw, null, coreCol * 0.3f, drawBaseRot, smearOrigin,
                    bladeScale * 0.96f, flip, 0f);
                }
                finally
                {
                    sb.End();
                    sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                        Main.DefaultSamplerState, DepthStencilState.None,
                        RasterizerState.CullCounterClockwise, null,
                        Main.GameViewMatrix.TransformationMatrix);
                }
            }
            else
            {
                // Fallback: static sakura-tinted smear layers
                Color fallbackOuter = EroicaPalette.Sakura with { A = 0 };
                Color fallbackCore = Color.Lerp(EroicaPalette.Sakura, Color.White, 0.5f) with { A = 0 };

                sb.Draw(smearTex, playerDraw, null, fallbackOuter * 0.2f, drawBaseRot, smearOrigin,
                    bladeScale * 1.10f, flip, 0f);
                sb.Draw(smearTex, playerDraw, null, fallbackCore * 0.35f, drawBaseRot, smearOrigin,
                    bladeScale * 1.02f, flip, 0f);
            }
        }

        // ═══════════════════════════════════════════════════════════════════
        // LAYER 2: Harmonic Wave ribbon trail (RibbonFoundation Mode 4)
        // ═══════════════════════════════════════════════════════════════════

        private void DrawHarmonicRibbon(SpriteBatch sb, int phase)
        {
            if (ribbonCount < 3) return;

            // Build ordered position array from ring buffer (oldest → newest)
            int drawCount = Math.Min(ribbonCount, RibbonTrailLength);
            Vector2[] orderedPositions = new Vector2[drawCount];
            float[] orderedRotations = new float[drawCount];
            for (int i = 0; i < drawCount; i++)
            {
                int idx = (ribbonIndex - drawCount + i + RibbonTrailLength) % RibbonTrailLength;
                orderedPositions[i] = ribbonPositions[idx];
                orderedRotations[i] = ribbonRotations[idx];
            }

            // Use HarmonicWaveRibbon texture for standing wave pattern
            Texture2D stripTex = RBFTextures.HarmonicWaveRibbon.Value;
            if (stripTex == null) return;

            int texW = stripTex.Width;
            int texH = stripTex.Height;
            float time = (float)Main.timeForVisualEffects * 0.005f;
            float widthScale = RibbonWidthScale[phase];

            // Switch to additive for glowing ribbon
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                SamplerState.LinearWrap, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);
            try
            {
            int srcWidth = Math.Max(1, texW / drawCount);

            for (int i = 0; i < drawCount - 1; i++)
            {
                float progress = (float)i / drawCount; // 0 = oldest (tail), 1 = newest (head)
                float fade = progress * progress; // Cubic-ish — brighter near head
                if (fade < 0.01f) continue;

                // Width: narrow tail → wider head, scaled by phase
                float width = MathHelper.Lerp(RibbonWidthTail, RibbonWidthHead, progress) * widthScale;

                // Standing wave modulation (Mode 4 signature)
                float waveFreq = 4f + phase;
                float wave = 0.6f + 0.4f * MathF.Sin(progress * waveFreq * MathHelper.Pi + time * 3f);
                width *= wave;

                // Segment geometry
                Vector2 segDir = orderedPositions[i + 1] - orderedPositions[i];
                float segLength = segDir.Length();
                if (segLength < 0.5f) continue;
                float segAngle = segDir.ToRotation();

                // UV scrolling
                float uStart = (progress + time * 2f) % 1f;
                int srcX = (int)(uStart * texW) % texW;
                Rectangle srcRect = new Rectangle(srcX, 0, srcWidth, texH);

                float scaleX = segLength / (float)srcWidth;
                float scaleY = width / (float)texH;

                Vector2 pos = orderedPositions[i] - Main.screenPosition;
                Vector2 origin = new Vector2(0, texH / 2f);

                // Pink → white body color gradient
                Color bodyColor = Color.Lerp(EroicaPalette.Sakura, Color.White, progress * 0.5f) with { A = 0 };
                sb.Draw(stripTex, pos, srcRect, bodyColor * (fade * 0.7f), segAngle, origin,
                    new Vector2(scaleX, scaleY), SpriteEffects.None, 0f);

                // Bright inner core near head
                if (progress > 0.4f)
                {
                    float coreFade = (progress - 0.4f) / 0.6f;
                    Color coreColor = Color.Lerp(Color.White, EroicaPalette.PollenGold, 0.2f) with { A = 0 };
                    sb.Draw(stripTex, pos, srcRect, coreColor * (fade * coreFade * 0.35f), segAngle, origin,
                        new Vector2(scaleX * 0.5f, scaleY * 0.5f), SpriteEffects.None, 0f);
                }
            }

            // Bloom overlay along trail
            Texture2D bloomTex = MagnumTextureRegistry.GetBloom();
            if (bloomTex != null)
            {
                Vector2 bloomOrigin = bloomTex.Size() * 0.5f;
                int bloomStep = Math.Max(1, drawCount / 8);
                for (int i = 0; i < drawCount; i += bloomStep)
                {
                    float progress = (float)i / drawCount;
                    if (progress < 0.2f) continue;
                    float bloomFade = progress * progress * 0.2f;
                    Color bloomCol = EroicaPalette.Sakura with { A = 0 };
                    float bloomScale = MathHelper.Lerp(0.08f, 0.2f, progress) * widthScale;
                    sb.Draw(bloomTex, orderedPositions[i] - Main.screenPosition, null,
                        bloomCol * bloomFade, 0f, bloomOrigin, bloomScale, SpriteEffects.None, 0f);
                }
            }
            }
            finally
            {
                // Restore
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                    Main.DefaultSamplerState, DepthStencilState.None,
                    RasterizerState.CullCounterClockwise, null,
                    Main.GameViewMatrix.TransformationMatrix);
            }
        }

        // ═══════════════════════════════════════════════════════════════════
        // LAYER 3: Afterimage trail (sakura petal ghost)
        // ═══════════════════════════════════════════════════════════════════

        private void DrawAfterimages(SpriteBatch sb, Texture2D tex, Vector2 origin, float scale,
            SpriteEffects flip, Vector2 playerDraw, int phase)
        {
            int count = 5 + phase;

            for (int i = 0; i < count && i < MaxAfterimages; i++)
            {
                int idx = (afterimageHead - 2 - i + MaxAfterimages) % MaxAfterimages;
                float rot = afterimageRotations[idx];
                float drawRot = rot + (swingDirection > 0 ? -MathHelper.PiOver4 : MathHelper.PiOver4 + MathHelper.Pi);

                float fade = 1f - (float)(i + 1) / (count + 1);
                fade *= fade;

                Color col = Color.Lerp(EroicaPalette.Sakura, EroicaPalette.PollenGold, (float)i / count) * (fade * 0.3f);
                col.A = 0;

                sb.Draw(tex, playerDraw, null, col, drawRot, origin, scale * (1f - i * 0.02f), flip, 0f);
            }
        }

        // ═══════════════════════════════════════════════════════════════════
        // LAYER 5: Blade tip bloom
        // ═══════════════════════════════════════════════════════════════════

        private void DrawBladeTipBloom(SpriteBatch sb, Player player, int phase)
        {
            Vector2 tipPos = player.Center + swingRotation.ToRotationVector2() * BladeLength;

            EroicaVFXLibrary.DrawEroicaBloomStack(sb, tipPos,
                EroicaPalette.Sakura, EroicaPalette.PollenGold,
                0.25f + phase * 0.06f, 0.7f + phase * 0.1f);

            // ER Sakura Petal accent — floating sakura halo on blade tip
            Texture2D petalTex = EroicaThemeTextures.ERSakuraPetal;
            if (petalTex != null)
            {
                Vector2 petalOrigin = petalTex.Size() * 0.5f;
                Vector2 petalDrawPos = tipPos - Main.screenPosition;
                float petalRot = (float)Main.GameUpdateCount * 0.03f;
                float petalPulse = 0.7f + MathF.Sin((float)Main.GameUpdateCount * 0.07f) * 0.3f;
                Color petalCol = EroicaPalette.Sakura with { A = 0 };
                sb.Draw(petalTex, petalDrawPos, null, petalCol * (0.25f * petalPulse), petalRot, petalOrigin,
                    0.22f + phase * 0.04f, SpriteEffects.None, 0f);
            }
        }

        // ═══════════════════════════════════════════════════════════════════
        // LAYER 6: Meditation aura
        // ═══════════════════════════════════════════════════════════════════

        private void DrawMeditationGlow(SpriteBatch sb, Player player)
        {
            Texture2D bloom = MagnumTextureRegistry.GetBloom();
            if (bloom == null) return;

            Vector2 drawPos = player.Center - Main.screenPosition;
            Vector2 bloomOrigin = bloom.Size() * 0.5f;
            float pulse = 0.8f + MathF.Sin((float)Main.GameUpdateCount * 0.06f) * 0.2f;

            Color meditColor = EroicaPalette.Sakura with { A = 0 };
            sb.Draw(bloom, drawPos, null, meditColor * (0.18f * pulse), 0f, bloomOrigin,
                1.5f, SpriteEffects.None, 0f);

            Color innerColor = EroicaPalette.PollenGold with { A = 0 };
            sb.Draw(bloom, drawPos, null, innerColor * (0.12f * pulse), 0f, bloomOrigin,
                0.8f, SpriteEffects.None, 0f);

            // ER Power Effect Ring — concentric ring around meditation glow
            Texture2D ringTex = EroicaThemeTextures.ERPowerEffectRing;
            if (ringTex != null)
            {
                Vector2 ringOrigin = ringTex.Size() * 0.5f;
                float ringRot = (float)Main.GameUpdateCount * 0.015f;
                float ringPulse = 0.6f + MathF.Sin((float)Main.GameUpdateCount * 0.05f) * 0.4f;
                Color ringCol = Color.Lerp(EroicaPalette.Sakura, EroicaPalette.PollenGold, ringPulse) with { A = 0 };
                sb.Draw(ringTex, drawPos, null, ringCol * (0.12f * ringPulse), ringRot, ringOrigin,
                    0.4f, SpriteEffects.None, 0f);
            }
        }

        #endregion
    }
}