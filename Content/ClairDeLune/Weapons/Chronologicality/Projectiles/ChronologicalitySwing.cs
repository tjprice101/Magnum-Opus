using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Content.ClairDeLune.Weapons.Chronologicality.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using static MagnumOpus.Content.ClairDeLune.Weapons.Chronologicality.Utilities.ChronologicalityUtils;
using MagnumOpus.Common.Systems.VFX;

namespace MagnumOpus.Content.ClairDeLune.Weapons.Chronologicality.Projectiles
{
    /// <summary>
    /// ChronologicalitySwing  EThe main swing projectile for the Chronologicality broadsword.
    ///
    /// 3-phase clock-hand combo with distinct swing arcs per phase:
    ///  - Hour Hand (Phase 0): 270° slow heavy cleave, 2x damage, screen shake
    ///  - Minute Hand (Phase 1): 180° mid sweep, 1.5x damage
    ///  - Second Hand (Phase 2): 3ÁErapid 90° flurry, 0.8x per strike
    ///
    /// Visual layers:
    ///  1. SMEAR ARC  EShader-driven distortion overlay from SwordSmearFoundation pattern
    ///  2. TEMPORAL TRAIL  EMulti-point bloom trail along swing arc
    ///  3. TIP GLOW  E4-layer bloom at blade tip with clockwork gold ticks
    ///  4. BLADE SPRITE  EWeapon texture drawn at swing angle
    ///  5. CLOCKWORK PARTICLES  ESparkle + gear dust along arc
    ///
    /// ai[0] = combo phase (0=Hour, 1=Minute, 2=Second)
    /// ai[1] = overflow flag (1 = Clockwork Overflow active)
    /// </summary>
    public class ChronologicalitySwing : ModProjectile
    {
        public override string Texture => "MagnumOpus/Content/ClairDeLune/Weapons/Chronologicality/Chronologicality";

        // ══════════════════════════════════════════╁E
        //  CONSTANTS
        // ══════════════════════════════════════════╁E

        private const int TrailLength = 24;

        // ══════════════════════════════════════════╁E
        //  STATE
        // ══════════════════════════════════════════╁E

        private int _timer;
        private int _comboPhase;
        private bool _overflowActive;
        private float _startAngle;
        private int _swingDirection;
        private int _secondHandStrike; // For phase 2's triple flurry

        // Trail rendering
        private Vector2[] _trailPoints = new Vector2[TrailLength];
        private float[] _trailAlphas = new float[TrailLength];
        private int _trailHead;

        // Smear shader
        private Effect _smearShader;

        // ══════════════════════════════════════════╁E
        //  PROPERTIES
        // ══════════════════════════════════════════╁E

        private Player Owner => Main.player[Projectile.owner];
        private (float arc, int duration, float reach, float dmgMult, float shake) PhaseParams
            => GetPhaseParams(_comboPhase);

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DrawScreenCheckFluff[Projectile.type] = 600;
        }

        public override void SetDefaults()
        {
            Projectile.width = 60;
            Projectile.height = 60;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.ownerHitCheck = true;
            Projectile.hide = true;
            Projectile.timeLeft = 2;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 12;
            Projectile.extraUpdates = 1;
        }

        // ══════════════════════════════════════════╁E
        //  AI  ESWING LOGIC
        // ══════════════════════════════════════════╁E

        public override void AI()
        {
            Player player = Owner;
            if (player.dead || !player.active)
            {
                Projectile.Kill();
                return;
            }

            // Initialize on first frame
            if (_timer == 0)
            {
                _comboPhase = (int)Projectile.ai[0];
                _overflowActive = Projectile.ai[1] > 0;

                float aimAngle = Projectile.velocity.ToRotation();
                _swingDirection = player.direction;
                var (arc, _, _, _, _) = PhaseParams;
                _startAngle = aimAngle - MathHelper.ToRadians(arc / 2f) * _swingDirection;

                // Play swing sound
                SoundEngine.PlaySound(SoundID.Item71 with { Pitch = _comboPhase * 0.15f }, player.Center);
            }

            // Keep projectile alive while channeling (for Hour/Minute) or for duration (Second)
            bool channeled = _comboPhase < 2 && player.channel;
            var (_, duration, reach, _, _) = PhaseParams;
            int effectiveDuration = _overflowActive ? (int)(duration * 0.6f) : duration;

            // For Second Hand, we chain 3 rapid flurries
            if (_comboPhase == 2)
                effectiveDuration = duration * SecondHandStrikes;

            Projectile.timeLeft = 2;
            _timer++;

            // Compute swing progress
            float totalProgress = MathHelper.Clamp((float)_timer / (effectiveDuration * 2), 0f, 1f); // extraUpdates=1 doubles ticks
            float currentAngle = GetSwingAngle(totalProgress, _comboPhase, _startAngle, _swingDirection);

            // For Second Hand Triple Flurry  Ealternate direction every sub-swing
            if (_comboPhase == 2)
            {
                int subDuration = duration * 2; // account for extraUpdates
                int subIndex = Math.Min(_timer / subDuration, SecondHandStrikes - 1);
                if (subIndex != _secondHandStrike)
                {
                    _secondHandStrike = subIndex;
                    _swingDirection = -_swingDirection;
                    _startAngle = currentAngle;
                }
                float subProgress = MathHelper.Clamp((float)(_timer - subIndex * subDuration) / subDuration, 0f, 1f);
                currentAngle = GetSwingAngle(subProgress, _comboPhase, _startAngle, _swingDirection);
            }

            Projectile.rotation = currentAngle;
            Projectile.Center = player.MountedCenter;

            // Player anchoring
            player.ChangeDir(Projectile.velocity.X > 0 ? 1 : -1);
            player.heldProj = Projectile.whoAmI;
            player.itemTime = 2;
            player.itemAnimation = 2;
            player.itemRotation = MathF.Atan2(
                MathF.Sin(currentAngle) * player.direction,
                MathF.Cos(currentAngle) * player.direction);

            // Blade tip position
            float effectiveReach = _overflowActive ? reach * 1.3f : reach;
            Vector2 tipPos = player.MountedCenter + currentAngle.ToRotationVector2() * effectiveReach;
            Projectile.position = tipPos - Projectile.Size / 2f;

            // Record trail
            _trailPoints[_trailHead % TrailLength] = tipPos;
            _trailAlphas[_trailHead % TrailLength] = 1f;
            _trailHead++;

            // Per-frame VFX
            SpawnSwingVFX(player, tipPos, currentAngle, totalProgress);

            // Dynamic lighting along blade
            float lightIntensity = _comboPhase == 0 ? 0.7f : (_comboPhase == 1 ? 0.5f : 0.4f);
            if (_overflowActive) lightIntensity *= 1.5f;
            Lighting.AddLight(tipPos, ClairDeLunePalette.SoftBlue.ToVector3() * lightIntensity);
            Vector2 midBlade = player.MountedCenter + currentAngle.ToRotationVector2() * effectiveReach * 0.5f;
            Lighting.AddLight(midBlade, ClairDeLunePalette.NightMist.ToVector3() * lightIntensity * 0.5f);

            // Kill when swing completes
            if (_timer >= effectiveDuration * 2) // extraUpdates doubles frame count
            {
                // Advance combo
                var mp = player.ChronologicalityState();
                mp.AdvanceCombo();

                if (_overflowActive)
                {
                    mp.ConsumeOverflow();
                    // Spawn Clockwork Overflow detonation
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), player.Center,
                        Vector2.Zero, ModContent.ProjectileType<ClockworkOverflowProjectile>(),
                        (int)(Projectile.damage * 4f), 0f, Projectile.owner);
                }

                Projectile.Kill();
            }
        }

        // ══════════════════════════════════════════╁E
        //  PER-FRAME VFX
        // ══════════════════════════════════════════╁E

        private void SpawnSwingVFX(Player player, Vector2 tipPos, float angle, float progress)
        {
            Color[] phaseColors = GetPhaseColors(_comboPhase);

            // Sparkle at blade tip every other frame
            if (_timer % 3 == 0 && progress < 0.95f)
            {
                Color sparkColor = phaseColors[1] with { A = 0 } * 0.7f;
                var spark = new SparkleParticle(
                    tipPos + Main.rand.NextVector2Circular(4f, 4f),
                    angle.ToRotationVector2() * 1.5f,
                    sparkColor, 0.3f, 15);
                MagnumParticleHandler.SpawnParticle(spark);
            }

            // Clockwork gold tick marks every ~30° of swing arc
            float arcRadians = MathHelper.ToRadians(PhaseParams.arc);
            float anglePerTick = MathHelper.Pi / 6f; // 30°
            float currentArc = progress * arcRadians;
            if (currentArc > 0 && currentArc % anglePerTick < (arcRadians / PhaseParams.duration * 2))
            {
                Color goldColor = ClairDeLunePalette.ClockworkBrass with { A = 0 } * 0.6f;
                var goldSpark = new GenericGlowParticle(tipPos, Vector2.Zero, goldColor, 0.2f, 12, true);
                MagnumParticleHandler.SpawnParticle(goldSpark);
            }

            // Temporal echo dust (blue fairy dust)
            if (Main.rand.NextBool(3))
            {
                Dust d = Dust.NewDustPerfect(tipPos, DustID.BlueFairy,
                    Main.rand.NextVector2Circular(2f, 2f), 0, ClairDeLunePalette.SoftBlue, 1.1f);
                d.noGravity = true;
            }

            // Overflow special: extra VFX intensity
            if (_overflowActive && _timer % 2 == 0)
            {
                Color overflowColor = ClairDeLunePalette.MoonbeamGold with { A = 0 } * 0.5f;
                var overflowGlow = new BloomParticle(
                    tipPos + Main.rand.NextVector2Circular(8f, 8f),
                    -angle.ToRotationVector2() * 0.5f,
                    overflowColor, 0.25f, 10);
                MagnumParticleHandler.SpawnParticle(overflowGlow);

                // Music notes during overflow
                if (_timer % 8 == 0)
                    ClairDeLuneVFXLibrary.SpawnMusicNotes(tipPos, 1, 3f, 0.2f, 0.4f, 20);
            }
        }

        // ══════════════════════════════════════════╁E
        //  COLLISION
        // ══════════════════════════════════════════╁E

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            Player player = Owner;
            Vector2 origin = player.MountedCenter;
            float effectiveReach = _overflowActive ? PhaseParams.reach * 1.3f : PhaseParams.reach;
            Vector2 tip = origin + Projectile.rotation.ToRotationVector2() * effectiveReach;
            float collisionWidth = _comboPhase == 0 ? 30f : (_comboPhase == 1 ? 24f : 18f);
            float _ = 0f;
            return Collision.CheckAABBvLineCollision(
                targetHitbox.TopLeft(), targetHitbox.Size(),
                origin, tip, collisionWidth, ref _);
        }

        public override void ModifyDamageHitbox(ref Rectangle hitbox)
        {
            // Expand hitbox for Hour Hand
            if (_comboPhase == 0)
                hitbox.Inflate(20, 20);
        }

        public override bool ShouldUpdatePosition() => false;

        // ══════════════════════════════════════════╁E
        //  ON HIT
        // ══════════════════════════════════════════╁E

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Apply slow
            target.AddBuff(BuffID.Slow, 180);

            // Temporal echo impact
            ClairDeLuneVFXLibrary.MeleeImpact(target.Center, _comboPhase);

            // Screen shake for Hour Hand
            if (_comboPhase == 0 && Main.myPlayer == Projectile.owner)
            {
                // Spawn Time Slow Field at impact
                Projectile.NewProjectile(Projectile.GetSource_OnHit(target), target.Center,
                    Vector2.Zero, ModContent.ProjectileType<TimeSlowFieldProjectile>(),
                    0, 0f, Projectile.owner);
            }

            // Spawn temporal echo (ghost replay) with delay
            if (Main.myPlayer == Projectile.owner)
            {
                Projectile.NewProjectile(Projectile.GetSource_OnHit(target), target.Center,
                    Vector2.Zero, ModContent.ProjectileType<TemporalEchoProjectile>(),
                    (int)(damageDone * 0.3f), 0f, Projectile.owner,
                    Projectile.rotation, _comboPhase);
            }
        }

        // ══════════════════════════════════════════╁E
        //  RENDERING
        // ══════════════════════════════════════════╁E

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Player player = Owner;
            Vector2 drawOrigin = player.MountedCenter - Main.screenPosition;

            var (arc, duration, reach, _, _) = PhaseParams;
            int effectiveDuration = _overflowActive ? (int)(duration * 0.6f) : duration;
            if (_comboPhase == 2) effectiveDuration = duration * SecondHandStrikes;

            float progress = MathHelper.Clamp((float)_timer / (effectiveDuration * 2), 0f, 1f);
            float currentAngle = Projectile.rotation;
            Color[] phaseColors = GetPhaseColors(_comboPhase);

            // Fade in/out
            float smearAlpha;
            if (progress < 0.08f) smearAlpha = progress / 0.08f;
            else if (progress > 0.88f) smearAlpha = (1f - progress) / 0.12f;
            else smearAlpha = 1f;
            if (_overflowActive) smearAlpha = Math.Min(smearAlpha * 1.3f, 1f);

            float effectiveReach = _overflowActive ? reach * 1.3f : reach;

            // === LAYER 1: SMEAR ARC OVERLAY (follow SmearSwingProjectile pattern) ===
            DrawSmearArc(sb, drawOrigin, currentAngle, smearAlpha, effectiveReach, phaseColors);

            // === LAYER 2: TEMPORAL TRAIL ===
            DrawTemporalTrail(sb, smearAlpha);

            // === LAYER 3: TIP GLOW ===
            DrawTipGlow(sb, drawOrigin, currentAngle, effectiveReach, smearAlpha);

            // === LAYER 4: BLADE SPRITE ===
            DrawBladeSprite(sb, drawOrigin, currentAngle, effectiveReach, lightColor);

            ClairDeLuneVFXLibrary.DrawThemeAccents(sb, Projectile.Center, 0.5f, 0.3f);
            return false;
        }

        private void DrawSmearArc(SpriteBatch sb, Vector2 drawOrigin, float angle, float alpha, float reach, Color[] colors)
        {
            // Load smear shader (SmearDistortShader from SwordSmearFoundation)
            if (_smearShader == null)
            {
                try
                {
                    _smearShader = ModContent.Request<Effect>(
                        "MagnumOpus/Content/FoundationWeapons/SwordSmearFoundation/Shaders/SmearDistortShader",
                        AssetRequestMode.ImmediateLoad).Value;
                }
                catch { _smearShader = null; }
            }

            // Load smear texture (use SwordArcSmear from VFX library)
            Texture2D smearTex;
            try
            {
                smearTex = ModContent.Request<Texture2D>(
                    "MagnumOpus/Assets/VFX Asset Library/SlashArcs/SwordArcSmear",
                    AssetRequestMode.ImmediateLoad).Value;
            }
            catch { return; }

            Vector2 smearOrigin = smearTex.Size() / 2f;
            float maxDim = MathF.Max(smearTex.Width, smearTex.Height);
            float smearScale = (reach * 2.4f) / maxDim;
            if (_overflowActive) smearScale *= 1.2f;

            // Phase-specific distortion intensity
            float distortBase = _comboPhase switch { 0 => 0.07f, 1 => 0.05f, _ => 0.03f };
            float flowSpeed = _comboPhase switch { 0 => 0.25f, 1 => 0.4f, _ => 0.6f };

            if (_smearShader != null)
            {
                sb.End();
                sb.Begin(SpriteSortMode.Immediate, BlendState.Additive,
                    SamplerState.LinearWrap, DepthStencilState.None,
                    RasterizerState.CullCounterClockwise, null,
                    Main.GameViewMatrix.EffectMatrix);

                float time = (float)Main.gameTimeCache.TotalGameTime.TotalSeconds;
                _smearShader.Parameters["uTime"]?.SetValue(time);
                _smearShader.Parameters["fadeAlpha"]?.SetValue(alpha);
                _smearShader.Parameters["flowSpeed"]?.SetValue(flowSpeed);
                _smearShader.Parameters["noiseScale"]?.SetValue(2.5f);

                // Load noise texture (TileableFBMNoise for organic distortion)
                Texture2D noiseTex;
                try
                {
                    noiseTex = ModContent.Request<Texture2D>(
                        "MagnumOpus/Assets/VFX Asset Library/NoiseTextures/TileableFBMNoise",
                        AssetRequestMode.ImmediateLoad).Value;
                    _smearShader.Parameters["noiseTex"]?.SetValue(noiseTex);
                }
                catch { }

                // Load gradient LUT for Clair de Lune theme coloring
                Texture2D gradientLUT;
                try
                {
                    gradientLUT = ModContent.Request<Texture2D>(
                        "MagnumOpus/Assets/VFX Asset Library/ColorGradients/ClairDeLuneGradientLUTandRAMP",
                        AssetRequestMode.ImmediateLoad).Value;
                    _smearShader.Parameters["gradientTex"]?.SetValue(gradientLUT);
                }
                catch { }

                // Sub-layer A: Wide outer glow
                _smearShader.Parameters["distortStrength"]?.SetValue(distortBase * 1.3f);
                _smearShader.CurrentTechnique.Passes[0].Apply();
                sb.Draw(smearTex, drawOrigin, null, colors[0] with { A = 0 } * alpha * 0.4f,
                    angle, smearOrigin, smearScale * 1.15f, SpriteEffects.None, 0f);

                // Sub-layer B: Main smear
                _smearShader.Parameters["distortStrength"]?.SetValue(distortBase);
                _smearShader.CurrentTechnique.Passes[0].Apply();
                sb.Draw(smearTex, drawOrigin, null, colors[1] with { A = 0 } * alpha * 0.7f,
                    angle, smearOrigin, smearScale, SpriteEffects.None, 0f);

                // Sub-layer C: Bright core
                _smearShader.Parameters["distortStrength"]?.SetValue(distortBase * 0.5f);
                _smearShader.CurrentTechnique.Passes[0].Apply();
                sb.Draw(smearTex, drawOrigin, null, colors[2] with { A = 0 } * alpha * 0.55f,
                    angle, smearOrigin, smearScale * 0.85f, SpriteEffects.None, 0f);

                sb.End();
            }
            else
            {
                // Fallback: static colored layers without shader
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                    Main.DefaultSamplerState, DepthStencilState.None,
                    RasterizerState.CullCounterClockwise, null,
                    Main.GameViewMatrix.EffectMatrix);

                sb.Draw(smearTex, drawOrigin, null, colors[0] with { A = 0 } * alpha * 0.4f,
                    angle, smearOrigin, smearScale * 1.15f, SpriteEffects.None, 0f);
                sb.Draw(smearTex, drawOrigin, null, colors[1] with { A = 0 } * alpha * 0.7f,
                    angle, smearOrigin, smearScale, SpriteEffects.None, 0f);
                sb.Draw(smearTex, drawOrigin, null, colors[2] with { A = 0 } * alpha * 0.55f,
                    angle, smearOrigin, smearScale * 0.85f, SpriteEffects.None, 0f);

                sb.End();
            }

            // Restore SpriteBatch
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);
        }

        private void DrawTemporalTrail(SpriteBatch sb, float overallAlpha)
        {
            Texture2D bloom = MagnumTextureRegistry.GetSoftGlow();
            if (bloom == null) return;
            Texture2D starTex = MagnumTextureRegistry.GetStar4Soft();

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

            Vector2 origin = bloom.Size() * 0.5f;
            float phaseIntensity = _comboPhase switch { 0 => 1.0f, 1 => 0.8f, _ => 0.6f };
            if (_overflowActive) phaseIntensity *= 1.4f;

            for (int i = 0; i < TrailLength; i++)
            {
                int idx = ((_trailHead - 1 - i) % TrailLength + TrailLength) % TrailLength;
                Vector2 pos = _trailPoints[idx];
                if (pos == Vector2.Zero) continue;

                float progress = (float)i / TrailLength;
                float fade = (1f - progress) * phaseIntensity * overallAlpha;
                Vector2 drawPos = pos - Main.screenPosition;

                // Outer glow — wide, themed (capped ~150px on SoftGlow)
                Color outerCol = ClairDeLunePalette.NightMist with { A = 0 } * fade * 0.35f;
                sb.Draw(bloom, drawPos, null, outerCol, 0f, origin,
                    0.29f * (1f - progress * 0.5f), SpriteEffects.None, 0f);

                // Mid glow
                Color midCol = ClairDeLunePalette.SoftBlue with { A = 0 } * fade * 0.4f;
                sb.Draw(bloom, drawPos, null, midCol, 0f, origin,
                    0.20f * (1f - progress * 0.3f), SpriteEffects.None, 0f);

                // Core
                Color coreCol = ClairDeLunePalette.PearlBlue with { A = 0 } * fade * 0.5f;
                sb.Draw(bloom, drawPos, null, coreCol, 0f, origin,
                    0.12f * (1f - progress * 0.3f), SpriteEffects.None, 0f);

                // Star4Soft sparkle accent every 3rd trail point
                if (i % 3 == 0 && starTex != null)
                {
                    Vector2 starOrigin = starTex.Size() / 2f;
                    float starRot = Main.GlobalTimeWrappedHourly * 2f + i * 0.5f;
                    float starScale = 0.08f * fade;
                    sb.Draw(starTex, drawPos, null,
                        ClairDeLunePalette.PearlWhite with { A = 0 } * fade * 0.4f, starRot,
                        starOrigin, starScale, SpriteEffects.None, 0f);
                }
            }

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);
        }

        private void DrawTipGlow(SpriteBatch sb, Vector2 drawOrigin, float angle, float reach, float alpha)
        {
            Texture2D bloom = MagnumTextureRegistry.GetSoftGlow();
            if (bloom == null) return;
            Texture2D starTex = MagnumTextureRegistry.GetStar4Soft();

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

            Vector2 origin = bloom.Size() * 0.5f;
            Vector2 tipDrawPos = drawOrigin + angle.ToRotationVector2() * reach;
            float pulse = 0.9f + 0.1f * MathF.Sin(Main.GameUpdateCount * 0.15f);

            // 4-layer bloom at tip — SoftGlow capped (max ~180px outer)
            sb.Draw(bloom, tipDrawPos, null,
                ClairDeLunePalette.NightMist with { A = 0 } * 0.3f * alpha, 0f, origin,
                0.35f * pulse, SpriteEffects.None, 0f);
            sb.Draw(bloom, tipDrawPos, null,
                ClairDeLunePalette.SoftBlue with { A = 0 } * 0.4f * alpha, 0f, origin,
                0.22f * pulse, SpriteEffects.None, 0f);
            sb.Draw(bloom, tipDrawPos, null,
                ClairDeLunePalette.PearlWhite with { A = 0 } * 0.5f * alpha, 0f, origin,
                0.14f * pulse, SpriteEffects.None, 0f);
            sb.Draw(bloom, tipDrawPos, null,
                ClairDeLunePalette.WhiteHot with { A = 0 } * 0.3f * alpha, 0f, origin,
                0.08f * pulse, SpriteEffects.None, 0f);

            // Star4Soft clockwork sparkle accent
            if (starTex != null)
            {
                Vector2 starOrigin = starTex.Size() / 2f;
                float starRot = Main.GlobalTimeWrappedHourly * 1.5f;
                sb.Draw(starTex, tipDrawPos, null,
                    ClairDeLunePalette.PearlWhite with { A = 0 } * 0.5f * alpha, starRot,
                    starOrigin, 0.12f * pulse, SpriteEffects.None, 0f);
                sb.Draw(starTex, tipDrawPos, null,
                    ClairDeLunePalette.SoftBlue with { A = 0 } * 0.35f * alpha, -starRot * 0.7f,
                    starOrigin, 0.07f * pulse, SpriteEffects.None, 0f);
            }

            // Clockwork gold accent for Hour Hand phase
            if (_comboPhase == 0 || _overflowActive)
            {
                sb.Draw(bloom, tipDrawPos, null,
                    ClairDeLunePalette.ClockworkBrass with { A = 0 } * 0.25f * alpha, 0f, origin,
                    0.28f * pulse, SpriteEffects.None, 0f);
            }

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);
        }

        private void DrawBladeSprite(SpriteBatch sb, Vector2 drawOrigin, float angle, float reach, Color lightColor)
        {
            Texture2D bladeTex;
            try
            {
                bladeTex = ModContent.Request<Texture2D>(Texture, AssetRequestMode.ImmediateLoad).Value;
            }
            catch { return; }

            Vector2 bladeOrigin = new Vector2(0, bladeTex.Height);
            SpriteEffects flip = _swingDirection < 0 ? SpriteEffects.FlipVertically : SpriteEffects.None;

            sb.Draw(bladeTex, drawOrigin, null, lightColor, angle + MathHelper.PiOver4,
                bladeOrigin, _overflowActive ? 1.3f : 1f, flip, 0f);
        }
    }
}