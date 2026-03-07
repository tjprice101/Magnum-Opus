using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.OdeToJoy.Weapons.ThornboundReckoning.Buffs;
using MagnumOpus.Content.OdeToJoy.Weapons.ThornboundReckoning.Utilities;

namespace MagnumOpus.Content.OdeToJoy.Weapons.ThornboundReckoning.Projectiles
{
    /// <summary>
    /// ThornboundSwingProj — The visual swing projectile for Thornbound Reckoning.
    /// Based on SwordSmearFoundation architecture with 3-phase botanical combo rendering.
    ///
    /// Visual layers:
    ///  1. SMEAR ARC — FBM-distorted vine smear arc via SmearDistortShader
    ///  2. BLADE SPRITE — The weapon texture drawn along the swing angle
    ///  3. TIP GLOW — Additive bloom at blade tip (botanical themed)
    ///  4. ROOT GLOW — Soft bloom at swing origin
    ///  5. DUST — Phase-colored botanical dust particles
    ///
    /// ai[0] = Combo phase (0=Vine Wave, 1=Thorn Lash, 2=Botanical Burst)
    /// ai[1] = Empowered (0=normal, 1=full Reckoning Charge)
    /// </summary>
    public class ThornboundSwingProj : ModProjectile
    {
        public override string Texture => "MagnumOpus/Content/OdeToJoy/Weapons/ThornboundReckoning/ThornboundReckoning";

        // ---- SWING CONSTANTS ----
        private const float BladeLength = 95f;
        private const int SwingDuration = 22;

        // ---- PHASE-SPECIFIC ARC ANGLES ----
        private static readonly float[] PhaseArcDeg = { 160f, 140f, 180f };
        private static readonly float[] PhaseWidthScale = { 0.8f, 1.0f, 1.4f };

        // ---- STATE ----
        private int timer;
        private float startAngle;
        private int swingDirection;
        private int ComboPhase => (int)Projectile.ai[0];
        private bool IsEmpowered => Projectile.ai[1] >= 1f;

        private int CurrentSwingDuration
        {
            get
            {
                return ComboPhase switch
                {
                    0 => 22,
                    1 => 18,
                    2 => 26,
                    _ => 22
                };
            }
        }

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
            }

            timer++;

            float arcDegCurrent = PhaseArcDeg[Math.Clamp(ComboPhase, 0, 2)];
            float progress = MathHelper.Clamp((float)timer / swingDur, 0f, 1f);
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

            // Phase-colored dust along the swing arc
            if (timer % 2 == 0 && progress < 0.9f)
            {
                SpawnSwingDust(owner.MountedCenter, currentAngle, progress);
            }

            // Additive bloom particles for empowered state
            if (IsEmpowered && timer % 3 == 0)
            {
                SpawnEmpoweredDust(tipPos);
            }

            if (timer >= swingDur)
            {
                Projectile.Kill();
            }
        }

        private void SpawnSwingDust(Vector2 origin, float angle, float progress)
        {
            Color[] colors = GetPhaseColors();
            float dustDist = BladeLength * Main.rand.NextFloat(0.4f, 1.0f);
            Vector2 pos = origin + angle.ToRotationVector2() * dustDist;
            Vector2 vel = angle.ToRotationVector2().RotatedBy(MathHelper.PiOver2 * swingDirection) * Main.rand.NextFloat(1f, 3f);
            Color col = colors[Main.rand.Next(colors.Length)];

            Dust dust = Dust.NewDustPerfect(pos, DustID.RainbowMk2, vel, newColor: col,
                Scale: Main.rand.NextFloat(0.3f, 0.7f));
            dust.noGravity = true;
            dust.fadeIn = 0.6f;
        }

        private void SpawnEmpoweredDust(Vector2 tipPos)
        {
            Vector2 vel = Main.rand.NextVector2Circular(3f, 3f);
            Dust dust = Dust.NewDustPerfect(tipPos + Main.rand.NextVector2Circular(8f, 8f),
                DustID.GoldCoin, vel, Scale: Main.rand.NextFloat(0.5f, 0.9f));
            dust.noGravity = true;
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

            // Impact dust burst
            Color[] colors = GetPhaseColors();
            for (int i = 0; i < 8; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(7f, 7f);
                Color col = colors[Main.rand.Next(colors.Length)];
                Dust dust = Dust.NewDustPerfect(target.Center, DustID.RainbowMk2, vel,
                    newColor: col, Scale: Main.rand.NextFloat(0.5f, 0.9f));
                dust.noGravity = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Player owner = Main.player[Projectile.owner];
            SpriteBatch sb = Main.spriteBatch;
            Vector2 drawOrigin = owner.MountedCenter - Main.screenPosition;

            int swingDur = CurrentSwingDuration;
            float progress = MathHelper.Clamp((float)timer / swingDur, 0f, 1f);
            float currentAngle = Projectile.rotation;
            Color[] phaseColors = GetPhaseColors();
            float widthScale = PhaseWidthScale[Math.Clamp(ComboPhase, 0, 2)];

            // ---- FADE ENVELOPE ----
            float smearAlpha;
            if (progress < 0.1f)
                smearAlpha = progress / 0.1f;
            else if (progress > 0.85f)
                smearAlpha = (1f - progress) / 0.15f;
            else
                smearAlpha = 1f;

            if (IsEmpowered)
                smearAlpha *= 1.3f;

            // ==================================================================
            //  LAYER 1: SMEAR ARC OVERLAY (shader-driven distortion + flow)
            // ==================================================================
            Texture2D smearTex = ThornboundTextures.FlamingSwordArc.Value;
            Vector2 smearOrigin = smearTex.Size() / 2f;
            float maxDim = MathF.Max(smearTex.Width, smearTex.Height);
            float smearScale = (BladeLength * 2.4f) / maxDim * widthScale;

            Effect shader = ThornboundTextures.SmearDistortShader;

            if (shader != null)
            {
                sb.End();
                // BlendState.Additive (SourceAlpha) for alpha-transparent arc textures
                sb.Begin(SpriteSortMode.Immediate, BlendState.Additive,
                    SamplerState.LinearWrap, DepthStencilState.None,
                    RasterizerState.CullCounterClockwise, null,
                    Main.GameViewMatrix.EffectMatrix);

                float time = (float)Main.gameTimeCache.TotalGameTime.TotalSeconds;

                // Organic vine-wave warping per planning doc
                float distortStr = ComboPhase switch
                {
                    0 => 0.06f,  // Vine Wave — organic flow
                    1 => 0.04f,  // Thorn Lash — sharper, less distortion
                    2 => 0.08f,  // Botanical Burst — maximum intensity
                    _ => 0.06f,
                };

                shader.Parameters["uTime"]?.SetValue(time);
                shader.Parameters["fadeAlpha"]?.SetValue(MathHelper.Clamp(smearAlpha, 0f, 1f));
                shader.Parameters["distortStrength"]?.SetValue(distortStr);
                shader.Parameters["flowSpeed"]?.SetValue(0.35f);
                shader.Parameters["noiseScale"]?.SetValue(3.0f);
                shader.Parameters["noiseTex"]?.SetValue(ThornboundTextures.FBMNoise.Value);
                shader.Parameters["gradientTex"]?.SetValue(ThornboundTextures.GradOdeToJoy.Value);

                // Sub-layer A: Wide outer glow (stronger distortion for vine feel)
                shader.Parameters["distortStrength"]?.SetValue(distortStr * 1.3f);
                shader.CurrentTechnique.Passes[0].Apply();
                sb.Draw(smearTex, drawOrigin, null,
                    Color.White * MathHelper.Clamp(smearAlpha * 0.5f, 0f, 1f),
                    currentAngle, smearOrigin,
                    smearScale * 1.15f, SpriteEffects.None, 0f);

                // Sub-layer B: Main smear
                shader.Parameters["distortStrength"]?.SetValue(distortStr);
                shader.CurrentTechnique.Passes[0].Apply();
                sb.Draw(smearTex, drawOrigin, null,
                    Color.White * MathHelper.Clamp(smearAlpha * 0.8f, 0f, 1f),
                    currentAngle, smearOrigin,
                    smearScale, SpriteEffects.None, 0f);

                // Sub-layer C: Bright core
                shader.Parameters["distortStrength"]?.SetValue(distortStr * 0.4f);
                shader.CurrentTechnique.Passes[0].Apply();
                sb.Draw(smearTex, drawOrigin, null,
                    Color.White * MathHelper.Clamp(smearAlpha * 0.65f, 0f, 1f),
                    currentAngle, smearOrigin,
                    smearScale * 0.85f, SpriteEffects.None, 0f);

                sb.End();
            }
            else
            {
                // --- FALLBACK: static colored layers (no shader) ---
                sb.End();
                // BlendState.Additive (SourceAlpha) for alpha-transparent arc textures
                sb.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                    Main.DefaultSamplerState, DepthStencilState.None,
                    RasterizerState.CullCounterClockwise, null,
                    Main.GameViewMatrix.EffectMatrix);

                sb.Draw(smearTex, drawOrigin, null,
                    phaseColors[0] * smearAlpha * 0.4f,
                    currentAngle, smearOrigin,
                    smearScale * 1.15f, SpriteEffects.None, 0f);

                sb.Draw(smearTex, drawOrigin, null,
                    phaseColors[1] * smearAlpha * 0.7f,
                    currentAngle, smearOrigin,
                    smearScale, SpriteEffects.None, 0f);

                sb.Draw(smearTex, drawOrigin, null,
                    phaseColors[2] * smearAlpha * 0.55f,
                    currentAngle, smearOrigin,
                    smearScale * 0.85f, SpriteEffects.None, 0f);

                sb.End();
            }

            // ==================================================================
            //  LAYER 2: TIP GLOW (botanical bloom)
            // ==================================================================
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.EffectMatrix);

            Vector2 tipDrawPos = drawOrigin + currentAngle.ToRotationVector2() * BladeLength;
            Texture2D softGlow = ThornboundTextures.SoftGlow.Value;
            Texture2D starFlare = ThornboundTextures.StarFlare.Value;

            // Golden bloom at tip
            float tipPulse = 0.85f + 0.15f * MathF.Sin((float)Main.timeForVisualEffects * 0.08f);
            sb.Draw(softGlow, tipDrawPos, null,
                ThornboundTextures.BloomGold * smearAlpha * 0.6f * tipPulse, 0f,
                softGlow.Size() / 2f, 0.20f * widthScale, SpriteEffects.None, 0f);

            sb.Draw(starFlare, tipDrawPos, null,
                ThornboundTextures.JubilantLight * smearAlpha * 0.4f, currentAngle * 0.5f,
                starFlare.Size() / 2f, 0.14f * widthScale, SpriteEffects.None, 0f);

            // Empowered: extra flare
            if (IsEmpowered)
            {
                sb.Draw(softGlow, tipDrawPos, null,
                    ThornboundTextures.PureJoyWhite * smearAlpha * 0.4f, 0f,
                    softGlow.Size() / 2f, 0.20f * widthScale, SpriteEffects.None, 0f);
            }

            // ==================================================================
            //  LAYER 3: ROOT GLOW
            // ==================================================================
            sb.Draw(softGlow, drawOrigin, null,
                ThornboundTextures.RoseShadow * smearAlpha * 0.3f, 0f,
                softGlow.Size() / 2f, 0.15f, SpriteEffects.None, 0f);

            // Theme blossom sparkle accent
            OdeToJoyVFXLibrary.DrawThemeBlossomSparkle(sb, Projectile.Center, 1f, 0.5f);

            sb.End();

            // ==================================================================
            //  LAYER 4: BLADE SPRITE
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

            return false;
        }
    }
}
