using MagnumOpus.Common;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Bloom;
using MagnumOpus.Common.Systems.VFX.Core;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Content.Eroica;
using MagnumOpus.Content.Eroica.Weapons.CelestialValor.Buffs;
using MagnumOpus.Content.Eroica.Weapons.CelestialValor.Projectiles;
using MagnumOpus.Content.FoundationWeapons.SwordSmearFoundation;
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

namespace MagnumOpus.Content.Eroica.Weapons.CelestialValor
{
    /// <summary>
    /// Celestial Valor swing projectile - Heroic Crescendo 4-phase combo.
    /// 
    /// ARCHITECTURE: Built on SwordSmearFoundation scaffolding.
    /// - Swing: Aim-based start angle, smoothstep easing, sensible arc widths
    /// - Smear: SMFTextures.SmearDistortShader with Eroica gradient LUT + FBM noise
    /// - Bloom: Foundation-style tip glow + root glow (SMFTextures.SoftGlow / StarFlare)
    /// - Blade: Item sprite drawn along swing angle with proper origin
    /// - Afterimages: Palette-colored ghost blades fading behind the swing
    /// 
    /// Phase 0: Resolute Strike - 140deg overhead slash
    /// Phase 1: Ascending Valor - 160deg upward sweep, fires 1 ValorBeam
    /// Phase 2: Crimson Legion - 150deg triple-speed slash, fires 3 ValorBeams
    /// Phase 3: Finale Fortissimo - 180deg heroic cleave, spawns ValorBoom AoE
    /// 
    /// Valor Gauge builds on successive hits; at max the Finale becomes Gloria.
    /// Hero's Resolve: below 30% HP, blades burn brighter and deal +25% damage.
    /// </summary>
    public class CelestialValorSwing : ModProjectile
    {
        // -- AI state --
        private ref float ComboPhase => ref Projectile.ai[0];
        private ref float PhaseTimer => ref Projectile.ai[1];

        private float startAngle;
        private int swingDirection = 1;
        private bool initialized = false;
        private float valorGauge = 0f;
        private bool phaseProjectileSpawned = false;

        // -- Afterimage tracking --
        private const int MaxAfterimages = 8;
        private float[] afterimageRotations = new float[MaxAfterimages];
        private int afterimageHead = 0;

        // -- Ribbon trail buffer (heroic flame trail) --
        private const int RibbonTrailLength = 32;
        private const float RibbonWidthHead = 16f;
        private const float RibbonWidthTail = 1f;
        private Vector2[] ribbonPositions = new Vector2[RibbonTrailLength];
        private float[] ribbonRotations = new float[RibbonTrailLength];
        private int ribbonIndex = 0;
        private int ribbonCount = 0;

        // -- Swing constants (Foundation-calibrated) --
        private const float BladeLength = 100f;
        private const float SmearScaleMult = 2.4f;

        // -- Combo phase definitions --
        private static readonly int[] PhaseDuration = { 22, 24, 18, 28 };
        private static readonly float[] ArcDegrees = { 140f, 160f, 150f, 180f };

        // -- Eroica color shortcuts for smear fallback --
        private static readonly Color[] EroicaSmearColors = new Color[]
        {
            new Color(200, 50, 50),
            new Color(230, 120, 40),
            new Color(255, 200, 80),
        };

        public override void SetDefaults()
        {
            Projectile.width = 120;
            Projectile.height = 120;
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

            // -- Initialize on first tick (Foundation pattern: aim-based start angle) --
            if (!initialized)
            {
                initialized = true;
                float aimAngle = Projectile.velocity.ToRotation();
                swingDirection = player.direction;
                Projectile.direction = swingDirection;

                float halfArc = MathHelper.ToRadians(ArcDegrees[0] / 2f);
                startAngle = aimAngle - halfArc * swingDirection;

                for (int i = 0; i < MaxAfterimages; i++)
                    afterimageRotations[i] = startAngle;

                for (int i = 0; i < RibbonTrailLength; i++)
                {
                    ribbonPositions[i] = player.MountedCenter;
                    ribbonRotations[i] = startAngle;
                }
            }

            Projectile.Center = player.Center;

            // -- Swing interpolation (smoothstep easing) --
            int phaseIdx = (int)MathHelper.Clamp(ComboPhase, 0, 3);
            float duration = PhaseDuration[phaseIdx];
            float progress = MathHelper.Clamp(PhaseTimer / duration, 0f, 1f);
            float eased = progress * progress * (3f - 2f * progress);

            float arcRad = MathHelper.ToRadians(ArcDegrees[phaseIdx]);
            float currentAngle = startAngle + arcRad * eased * swingDirection;
            Projectile.rotation = currentAngle;

            // -- Record afterimage --
            afterimageRotations[afterimageHead] = currentAngle;
            afterimageHead = (afterimageHead + 1) % MaxAfterimages;

            // -- Blade tip for collision --
            Vector2 tipPos = player.MountedCenter + currentAngle.ToRotationVector2() * BladeLength;
            Projectile.position = tipPos - Projectile.Size / 2f;

            // -- Record ribbon trail position --
            ribbonPositions[ribbonIndex] = tipPos;
            ribbonRotations[ribbonIndex] = currentAngle;
            ribbonIndex = (ribbonIndex + 1) % RibbonTrailLength;
            if (ribbonCount < RibbonTrailLength) ribbonCount++;

            // -- Player anim (Foundation pattern) --
            player.ChangeDir(Projectile.velocity.X > 0 ? 1 : -1);
            player.heldProj = Projectile.whoAmI;
            player.itemTime = 2;
            player.itemAnimation = 2;
            player.itemRotation = (float)Math.Atan2(
                MathF.Sin(currentAngle) * player.direction,
                MathF.Cos(currentAngle) * player.direction);

            // -- Per-frame VFX --
            bool heroResolve = player.statLife < player.statLifeMax2 * 0.3f;
            SpawnPerFrameVFX(tipPos, phaseIdx, heroResolve, player, progress);

            // -- Phase-specific sub-projectile spawning at midpoint --
            if (!phaseProjectileSpawned && progress > 0.5f)
            {
                phaseProjectileSpawned = true;
                SpawnPhaseProjectiles(player, tipPos, phaseIdx);
            }

            // -- Advance timer --
            PhaseTimer++;

            if (PhaseTimer >= duration)
            {
                OnPhaseEnd(player, tipPos, phaseIdx);

                // Advance to next phase and recalculate start angle
                ComboPhase = (ComboPhase + 1) % 4;
                PhaseTimer = 0;
                swingDirection = -swingDirection;
                Projectile.direction = swingDirection;
                phaseProjectileSpawned = false;

                // New start angle = current end angle (seamless transition)
                int nextPhase = (int)ComboPhase;
                float nextArc = MathHelper.ToRadians(ArcDegrees[nextPhase]);
                startAngle = currentAngle;

                for (int i = 0; i < MaxAfterimages; i++)
                    afterimageRotations[i] = startAngle;

                // Reset ribbon trail for new phase
                for (int i = 0; i < RibbonTrailLength; i++)
                    ribbonPositions[i] = player.MountedCenter;
                ribbonCount = 0;
            }
            valorGauge = Math.Max(0, valorGauge - 0.08f);
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

        #region VFX Spawning

        private void SpawnPerFrameVFX(Vector2 tipPos, int phase, bool heroResolve, Player player, float progress)
        {
            // Foundation-style colored swing dust along the blade
            if ((int)PhaseTimer % 2 == 0 && progress < 0.9f)
            {
                float dustDist = BladeLength * Main.rand.NextFloat(0.4f, 1.0f);
                Vector2 pos = player.MountedCenter + Projectile.rotation.ToRotationVector2() * dustDist;
                Vector2 vel = Projectile.rotation.ToRotationVector2().RotatedBy(MathHelper.PiOver2 * swingDirection) * Main.rand.NextFloat(1f, 3f);
                Color col = EroicaSmearColors[Main.rand.Next(EroicaSmearColors.Length)];
                Dust dust = Dust.NewDustPerfect(pos, DustID.RainbowMk2, vel, newColor: col, Scale: Main.rand.NextFloat(0.3f, 0.6f));
                dust.noGravity = true;
                dust.fadeIn = 0.6f;
            }

            // Blade tip dust
            EroicaVFXLibrary.SpawnSwingDust(tipPos, -Projectile.rotation.ToRotationVector2());
            EroicaVFXLibrary.SpawnContrastSparkle(tipPos, -Projectile.rotation.ToRotationVector2());

            // Music notes periodically
            if ((int)PhaseTimer % 6 == 0)
                EroicaVFXLibrary.SpawnMusicNotes(tipPos, 1 + phase / 2, 12f, 0.7f, 1.0f, 28);

            // Valor sparkles on higher phases
            if (phase >= 1 && (int)PhaseTimer % 4 == 0)
                EroicaVFXLibrary.SpawnValorSparkles(tipPos, 2 + phase, 18f);

            // Dense sparks on phase 2+
            if (phase >= 2)
                EroicaVFXLibrary.SpawnDirectionalSparks(tipPos, Projectile.rotation.ToRotationVector2(), 2, 4f);

            // Hero's Resolve: extra rising embers
            if (heroResolve && Main.rand.NextBool(2))
                EroicaVFXLibrary.SpawnHeroicAura(player.Center, 35f);

            // Dynamic lighting
            EroicaVFXLibrary.AddPaletteLighting(tipPos, 0.3f + phase * 0.15f, 0.7f + phase * 0.1f);
        }

        private void SpawnPhaseProjectiles(Player player, Vector2 tipPos, int phase)
        {
            if (Main.myPlayer != Projectile.owner) return;

            Vector2 aimDir = (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX);

            switch (phase)
            {
                case 1:
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), tipPos, aimDir * 14f,
                        ModContent.ProjectileType<ValorBeam>(), (int)(Projectile.damage * 0.5f),
                        Projectile.knockBack * 0.5f, Projectile.owner);
                    SoundEngine.PlaySound(SoundID.Item60 with { Pitch = 0.1f, Volume = 0.6f }, player.Center);
                    break;

                case 2:
                    for (int i = -1; i <= 1; i++)
                    {
                        Vector2 dir = aimDir.RotatedBy(i * 0.18f) * 15f;
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), tipPos, dir,
                            ModContent.ProjectileType<ValorBeam>(), (int)(Projectile.damage * 0.35f),
                            Projectile.knockBack * 0.3f, Projectile.owner);
                    }
                    SoundEngine.PlaySound(SoundID.Item71 with { Pitch = 0.2f, Volume = 0.5f }, player.Center);
                    break;

                case 3:
                    float boomScale = valorGauge >= 95f ? 2f : 1f;
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), tipPos, Vector2.Zero,
                        ModContent.ProjectileType<ValorBoom>(), (int)(Projectile.damage * 0.7f * boomScale),
                        Projectile.knockBack, Projectile.owner);
                    SoundEngine.PlaySound(SoundID.Item62 with { Pitch = -0.3f }, player.Center);
                    break;
            }
        }

        private void OnPhaseEnd(Player player, Vector2 tipPos, int phase)
        {
            EroicaVFXLibrary.MeleeImpact(tipPos, phase);

            if (phase == 3)
            {
                float slamIntensity = valorGauge >= 95f ? 1.5f : 1f;
                EroicaVFXLibrary.FinisherSlam(tipPos, slamIntensity);

                if (valorGauge >= 95f)
                {
                    EroicaVFXLibrary.DeathHeroicFlash(tipPos, 1.2f);
                    EroicaVFXLibrary.SpawnSakuraPetals(tipPos, 18, 90f);
                    EroicaVFXLibrary.MusicNoteBurst(tipPos, EroicaPalette.Gold, 12, 6f);
                    valorGauge = 0f;
                }
            }
            else if (phase == 1)
            {
                EroicaVFXLibrary.Shockwave(tipPos, 0.5f);
            }
        }

        #endregion

        #region On Hit

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<MusicsDissonance>(), 240);
            target.AddBuff(ModContent.BuffType<HeroicBurn>(), 180);
            target.AddBuff(ModContent.BuffType<ValorStagger>(), 90);

            valorGauge = Math.Min(100f, valorGauge + 14f);

            int phase = (int)MathHelper.Clamp(ComboPhase, 0, 3);
            EroicaVFXLibrary.MeleeImpact(target.Center, phase);

            // Foundation-style hit sparks
            for (int i = 0; i < 5 + phase * 2; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(6f, 6f);
                Color col = EroicaSmearColors[Main.rand.Next(EroicaSmearColors.Length)];
                Dust dust = Dust.NewDustPerfect(target.Center, DustID.RainbowMk2, vel, newColor: col, Scale: Main.rand.NextFloat(0.5f, 0.8f));
                dust.noGravity = true;
            }

            Player player = Main.player[Projectile.owner];
            if (player.statLife < player.statLifeMax2 * 0.3f)
            {
                EroicaVFXLibrary.SpawnDirectionalSparks(target.Center,
                    (target.Center - player.Center).SafeNormalize(Vector2.UnitX), 5, 7f);
            }
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            Player player = Main.player[Projectile.owner];
            if (player.statLife < player.statLifeMax2 * 0.3f)
                modifiers.FinalDamage *= 1.25f;

            int phase = (int)MathHelper.Clamp(ComboPhase, 0, 3);
            modifiers.FinalDamage *= 1f + phase * 0.08f;
        }

        #endregion

        #region Rendering

        public override bool PreDraw(ref Color lightColor)
        {
            Player owner = Main.player[Projectile.owner];
            SpriteBatch sb = Main.spriteBatch;
            Vector2 drawOrigin = owner.MountedCenter - Main.screenPosition;

            int phase = (int)MathHelper.Clamp(ComboPhase, 0, 3);
            float progress = MathHelper.Clamp(PhaseTimer / (float)PhaseDuration[phase], 0f, 1f);
            float currentAngle = Projectile.rotation;
            bool heroResolve = owner.statLife < owner.statLifeMax2 * 0.3f;

            // -- FADE ENVELOPE (Foundation pattern) --
            float smearAlpha;
            if (progress < 0.1f)
                smearAlpha = progress / 0.1f;
            else if (progress > 0.85f)
                smearAlpha = (1f - progress) / 0.15f;
            else
                smearAlpha = 1f;

            // ==================================================================
            //  LAYER 1: SMEAR ARC OVERLAY (SwordSmearFoundation scaffolding)
            //  SMFTextures.SmearDistortShader with Eroica gradient LUT + FBM noise.
            //  3 sub-layers: outer haze, main body, hot core.
            // ==================================================================
            DrawSmearArc(sb, drawOrigin, currentAngle, smearAlpha, phase, heroResolve);

            // ==================================================================
            //  LAYER 1.5: HEROIC TRAIL RIBBON (flame trail along blade tip path)
            //  Shader-driven ribbon using CelestialValorTrail.fx with tip fade.
            // ==================================================================
            float time = (float)Main.gameTimeCache.TotalGameTime.TotalSeconds;
            DrawHeroicTrailRibbon(sb, phase, time, smearAlpha);

            // ==================================================================
            //  LAYER 2: TIP GLOW (Foundation pattern)
            // ==================================================================
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.EffectMatrix);

            Vector2 tipDrawPos = drawOrigin + currentAngle.ToRotationVector2() * BladeLength;
            Texture2D starFlare = SMFTextures.StarFlare.Value;
            Texture2D softGlow = SMFTextures.SoftGlow.Value;

            float glowScale = 0.2f + phase * 0.04f;
            sb.Draw(softGlow, tipDrawPos, null,
                EroicaSmearColors[1] * (smearAlpha * 0.5f), 0f,
                softGlow.Size() / 2f, glowScale, SpriteEffects.None, 0f);

            sb.Draw(starFlare, tipDrawPos, null,
                EroicaSmearColors[2] * (smearAlpha * 0.4f), currentAngle * 0.5f,
                starFlare.Size() / 2f, 0.12f + phase * 0.03f, SpriteEffects.None, 0f);

            if (phase >= 2)
            {
                sb.Draw(starFlare, tipDrawPos, null,
                    (EroicaPalette.Gold with { A = 0 }) * (smearAlpha * 0.25f), -currentAngle * 0.3f,
                    starFlare.Size() / 2f, 0.08f + phase * 0.02f, SpriteEffects.None, 0f);
            }

            // ==================================================================
            //  LAYER 3: ROOT GLOW (Foundation pattern)
            // ==================================================================
            sb.Draw(softGlow, drawOrigin, null,
                EroicaSmearColors[0] * (smearAlpha * 0.3f), 0f,
                softGlow.Size() / 2f, 0.15f + phase * 0.02f, SpriteEffects.None, 0f);

            sb.End();

            // Restore to AlphaBlend for blade sprite
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

            // ==================================================================
            //  LAYER 4: BLADE SPRITE (Foundation pattern)
            // ==================================================================
            Texture2D bladeTex = TextureAssets.Item[ModContent.ItemType<CelestialValor>()].Value;
            Vector2 bladeOrigin = new Vector2(0, bladeTex.Height);
            SpriteEffects flip = swingDirection < 0 ? SpriteEffects.FlipVertically : SpriteEffects.None;
            float bladeScale = 2.2f;
            float drawRot = currentAngle + MathHelper.PiOver4;

            // Afterimage ghost blades first (behind main blade)
            int afterCount = 4 + phase;
            for (int i = 0; i < afterCount && i < MaxAfterimages; i++)
            {
                int idx = (afterimageHead - 2 - i + MaxAfterimages) % MaxAfterimages;
                float rot = afterimageRotations[idx] + MathHelper.PiOver4;

                float fade = 1f - (float)(i + 1) / (afterCount + 1);
                fade *= fade;

                Color col = Color.Lerp(EroicaPalette.Scarlet, EroicaPalette.Gold, (float)i / afterCount) * (fade * 0.35f);
                col.A = 0;

                sb.Draw(bladeTex, drawOrigin, null, col, rot, bladeOrigin, bladeScale * (1f - i * 0.025f), flip, 0f);
            }

            // Main blade
            Color bladeTint = Color.Lerp(lightColor, EroicaVFXLibrary.GetPaletteColor(0.35f + phase * 0.12f), 0.35f);
            sb.Draw(bladeTex, drawOrigin, null, bladeTint, drawRot, bladeOrigin, bladeScale, flip, 0f);

            // Additive glow pass
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

            Color bladeGlow = EroicaVFXLibrary.GetPaletteColor(0.5f + phase * 0.1f) with { A = 0 };
            sb.Draw(bladeTex, drawOrigin, null, bladeGlow * (0.3f + phase * 0.05f), drawRot, bladeOrigin, bladeScale * 1.03f, flip, 0f);

            if (heroResolve)
            {
                float resolveTime = (float)Main.gameTimeCache.TotalGameTime.TotalSeconds;
                Color resolveGlow = EroicaPalette.HotCore with { A = 0 };
                float resolvePulse = 0.7f + MathF.Sin(resolveTime * 6f) * 0.3f;
                sb.Draw(bladeTex, drawOrigin, null, resolveGlow * (0.2f * resolvePulse), drawRot, bladeOrigin, bladeScale * 1.06f, flip, 0f);
            }

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

            return false;
        }

        /// <summary>
        /// LAYER 1: Smear arc overlay - SwordSmearFoundation scaffolding.
        /// Uses Foundation SmearDistortShader with Eroica gradient + noise,
        /// fallback to static Eroica-colored layers if shader unavailable.
        /// </summary>
        private void DrawSmearArc(SpriteBatch sb, Vector2 drawOrigin, float currentAngle,
            float smearAlpha, int phase, bool heroResolve)
        {
            Texture2D smearTex = SMFTextures.FlamingSwordArc.Value;
            Vector2 smearOrigin = smearTex.Size() / 2f;

            float maxDim = MathF.Max(smearTex.Width, smearTex.Height);
            float smearScale = (BladeLength * SmearScaleMult) / maxDim;
            smearScale *= 1f + phase * 0.06f;
            if (heroResolve) smearScale *= 1.08f;

            Effect shader = SMFTextures.SmearDistortShader;

            if (shader != null)
            {
                sb.End();
                sb.Begin(SpriteSortMode.Immediate, BlendState.Additive,
                    SamplerState.LinearWrap, DepthStencilState.None,
                    RasterizerState.CullCounterClockwise, null,
                    Main.GameViewMatrix.EffectMatrix);

                float time = (float)Main.gameTimeCache.TotalGameTime.TotalSeconds;

                shader.Parameters["uTime"]?.SetValue(time);
                shader.Parameters["fadeAlpha"]?.SetValue(smearAlpha);
                shader.Parameters["noiseScale"]?.SetValue(2.5f);
                shader.Parameters["noiseTex"]?.SetValue(SMFTextures.FBMNoise.Value);
                shader.Parameters["gradientTex"]?.SetValue(SMFTextures.GradEroica.Value);

                // Sub-layer A: Wide outer glow
                shader.Parameters["distortStrength"]?.SetValue(0.06f + phase * 0.015f);
                shader.Parameters["flowSpeed"]?.SetValue(0.4f + phase * 0.1f);
                shader.CurrentTechnique.Passes[0].Apply();
                sb.Draw(smearTex, drawOrigin, null,
                    Color.White * smearAlpha * 0.5f,
                    currentAngle, smearOrigin,
                    smearScale * 1.15f, SpriteEffects.None, 0f);

                // Sub-layer B: Main smear body
                shader.Parameters["distortStrength"]?.SetValue(0.04f + phase * 0.01f);
                shader.Parameters["flowSpeed"]?.SetValue(0.5f + phase * 0.08f);
                shader.CurrentTechnique.Passes[0].Apply();
                sb.Draw(smearTex, drawOrigin, null,
                    Color.White * smearAlpha * 0.8f,
                    currentAngle, smearOrigin,
                    smearScale, SpriteEffects.None, 0f);

                // Sub-layer C: Bright hot core
                shader.Parameters["distortStrength"]?.SetValue(0.02f);
                shader.Parameters["flowSpeed"]?.SetValue(0.6f + phase * 0.05f);
                shader.CurrentTechnique.Passes[0].Apply();
                sb.Draw(smearTex, drawOrigin, null,
                    Color.White * smearAlpha * 0.65f,
                    currentAngle, smearOrigin,
                    smearScale * 0.85f, SpriteEffects.None, 0f);

                // Sub-layer D: Hero's Resolve fire intensity
                if (heroResolve)
                {
                    shader.Parameters["distortStrength"]?.SetValue(0.08f);
                    shader.Parameters["flowSpeed"]?.SetValue(0.8f);
                    shader.CurrentTechnique.Passes[0].Apply();
                    sb.Draw(smearTex, drawOrigin, null,
                        Color.White * smearAlpha * 0.3f,
                        currentAngle, smearOrigin,
                        smearScale * 1.2f, SpriteEffects.None, 0f);
                }

                sb.End();
            }
            else
            {
                // -- FALLBACK: Static Eroica-colored layers --
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                    Main.DefaultSamplerState, DepthStencilState.None,
                    RasterizerState.CullCounterClockwise, null,
                    Main.GameViewMatrix.EffectMatrix);

                sb.Draw(smearTex, drawOrigin, null,
                    EroicaSmearColors[0] * smearAlpha * 0.4f,
                    currentAngle, smearOrigin,
                    smearScale * 1.15f, SpriteEffects.None, 0f);

                sb.Draw(smearTex, drawOrigin, null,
                    EroicaSmearColors[1] * smearAlpha * 0.7f,
                    currentAngle, smearOrigin,
                    smearScale, SpriteEffects.None, 0f);

                sb.Draw(smearTex, drawOrigin, null,
                    EroicaSmearColors[2] * smearAlpha * 0.55f,
                    currentAngle, smearOrigin,
                    smearScale * 0.85f, SpriteEffects.None, 0f);

                sb.End();
            }
        }

        // ═══════════════════════════════════════════════════════════════════════
        //  LAYER 1.5: Heroic Trail Ribbon — flame trail along blade tip path
        //  Dual-pass: CelestialValorTrail body + glow overlay with tip fade.
        // ═══════════════════════════════════════════════════════════════════════
        private void DrawHeroicTrailRibbon(SpriteBatch sb, int phase, float time, float smearAlpha)
        {
            if (ribbonCount < 3) return;

            int drawCount = Math.Min(ribbonCount, RibbonTrailLength);
            Vector2[] positions = new Vector2[drawCount];
            for (int i = 0; i < drawCount; i++)
            {
                int idx = (ribbonIndex - drawCount + i + RibbonTrailLength) % RibbonTrailLength;
                positions[i] = ribbonPositions[idx];
            }

            Texture2D stripTex = EroicaTextures.EnergyTrailUV?.Value ?? EroicaTextures.EmberScatter?.Value;
            if (stripTex == null) return;

            int texW = stripTex.Width;
            int texH = stripTex.Height;
            float scrollTime = (float)Main.timeForVisualEffects * 0.005f;
            int srcWidth = Math.Max(1, texW / drawCount);
            float comboNorm = phase / 3f;

            bool hasShader = EroicaShaderManager.HasCelestialValorTrail;

            if (hasShader)
            {
                // ── PASS 1: Heroic flame body (HeroicTrail technique) ──
                EroicaShaderManager.BeginShaderAdditive(sb);
                try
                {
                    EroicaShaderManager.ApplyCelestialValorSwingTrail(time, comboNorm, glowPass: false);

                    for (int i = 0; i < drawCount - 1; i++)
                    {
                        float progress = (float)i / drawCount;
                        float fade = progress * progress;
                        if (fade < 0.01f) continue;

                        float tipFade = MathHelper.Clamp(progress * 6f, 0f, 1f) * MathHelper.Clamp((1f - progress) * 4f, 0f, 1f);
                        float width = MathHelper.Lerp(RibbonWidthTail, RibbonWidthHead, progress);

                        Vector2 segDir = positions[i + 1] - positions[i];
                        float segLength = segDir.Length();
                        if (segLength < 0.5f) continue;
                        float segAngle = segDir.ToRotation();

                        float uStart = (progress + scrollTime * 2.5f) % 1f;
                        int srcX = (int)(uStart * texW) % texW;
                        Rectangle srcRect = new Rectangle(srcX, 0, srcWidth, texH);

                        float scaleX = segLength / (float)srcWidth;
                        float scaleY = width / (float)texH;
                        Vector2 pos = positions[i] - Main.screenPosition;
                        Vector2 origin = new Vector2(0, texH / 2f);

                        sb.Draw(stripTex, pos, srcRect, Color.White * (fade * tipFade * smearAlpha * 0.6f), segAngle, origin,
                            new Vector2(scaleX, scaleY), SpriteEffects.None, 0f);
                    }
                }
                finally
                {
                    EroicaShaderManager.RestoreSpriteBatch(sb);
                }

                // ── PASS 2: Valor glow overlay (wider, softer) ──
                EroicaShaderManager.BeginShaderAdditive(sb);
                try
                {
                    EroicaShaderManager.ApplyCelestialValorSwingTrail(time, comboNorm, glowPass: true);

                    for (int i = 0; i < drawCount - 1; i++)
                    {
                        float progress = (float)i / drawCount;
                        float fade = progress * progress;
                        if (fade < 0.02f) continue;

                        float tipFade = MathHelper.Clamp(progress * 6f, 0f, 1f) * MathHelper.Clamp((1f - progress) * 4f, 0f, 1f);
                        float width = MathHelper.Lerp(RibbonWidthTail, RibbonWidthHead, progress) * 1.4f;

                        Vector2 segDir = positions[i + 1] - positions[i];
                        float segLength = segDir.Length();
                        if (segLength < 0.5f) continue;
                        float segAngle = segDir.ToRotation();

                        float uStart = (progress + scrollTime * 2.5f) % 1f;
                        int srcX = (int)(uStart * texW) % texW;
                        Rectangle srcRect = new Rectangle(srcX, 0, srcWidth, texH);

                        float scaleX = segLength / (float)srcWidth;
                        float scaleY = width / (float)texH;
                        Vector2 pos = positions[i] - Main.screenPosition;
                        Vector2 origin = new Vector2(0, texH / 2f);

                        sb.Draw(stripTex, pos, srcRect, Color.White * (fade * tipFade * smearAlpha * 0.3f), segAngle, origin,
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
                // Fallback: basic additive Eroica-colored flame trail
                EroicaShaderManager.BeginAdditive(sb);
                try
                {
                    for (int i = 0; i < drawCount - 1; i++)
                    {
                        float progress = (float)i / drawCount;
                        float fade = progress * progress;
                        if (fade < 0.01f) continue;

                        float tipFade = MathHelper.Clamp(progress * 6f, 0f, 1f) * MathHelper.Clamp((1f - progress) * 4f, 0f, 1f);
                        float width = MathHelper.Lerp(RibbonWidthTail, RibbonWidthHead, progress);

                        Vector2 segDir = positions[i + 1] - positions[i];
                        float segLength = segDir.Length();
                        if (segLength < 0.5f) continue;
                        float segAngle = segDir.ToRotation();

                        float uStart = (progress + scrollTime * 2.5f) % 1f;
                        int srcX = (int)(uStart * texW) % texW;
                        Rectangle srcRect = new Rectangle(srcX, 0, srcWidth, texH);
                        float scaleX = segLength / (float)srcWidth;
                        float scaleY = width / (float)texH;
                        Vector2 pos = positions[i] - Main.screenPosition;
                        Vector2 origin = new Vector2(0, texH / 2f);

                        Color bodyColor = Color.Lerp(EroicaPalette.Scarlet, EroicaPalette.Gold, progress * 0.5f) with { A = 0 };
                        sb.Draw(stripTex, pos, srcRect, bodyColor * (fade * tipFade * smearAlpha * 0.55f), segAngle, origin,
                            new Vector2(scaleX, scaleY), SpriteEffects.None, 0f);
                    }
                }
                finally
                {
                    EroicaShaderManager.RestoreSpriteBatch(sb);
                }
            }

            // Bloom accent points along trail
            Texture2D bloomTex = MagnumTextureRegistry.GetBloom();
            if (bloomTex != null)
            {
                EroicaShaderManager.BeginAdditive(sb);
                try
                {
                    Vector2 bloomOrigin = bloomTex.Size() * 0.5f;
                    int bloomStep = Math.Max(1, drawCount / 6);
                    for (int i = 0; i < drawCount; i += bloomStep)
                    {
                        float progress = (float)i / drawCount;
                        if (progress < 0.15f) continue;
                        float bloomFade = progress * progress * 0.2f * smearAlpha;
                        Color bloomCol = EroicaPalette.Gold with { A = 0 };
                        float bloomScale = MathHelper.Lerp(0.06f, 0.18f, progress);
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

        #endregion
    }
}
