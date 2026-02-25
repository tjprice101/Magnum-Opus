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
    /// Last Prism-style channeled beam for the Sandbox TerraBlade right-click.
    /// Architecture follows VFX+'s LastPrismHeldProjectileOverride / LastPrismLaserOverride:
    /// - Charge phase: 6 converging thin beams with easeOutCirc width ramp
    /// - Merge burst: width expansion + white flash + screen shake
    /// - Sustained beam: shader-rendered beam while mouse held, slow cursor tracking
    /// - Release: 15-frame fade out
    /// </summary>
    public class SandboxTerraBladePrismBeam : ModProjectile
    {
        private const int ChargeFrames = 60;
        private const int FadeFrames = 15;
        private const float BeamBaseWidth = 80f;
        private const float MergeBurstExtra = 120f;
        private const float MaxBeamLength = 1400f;
        private const float CursorLerpSpeed = 0.025f;
        private const int ConvergingBeamCount = 6;
        private const float MaxConvergeAngle = 0.21f; // ~12 degrees

        // ai[0] = TargetX, ai[1] = TargetY (set from Shoot, networked automatically)
        private float TargetX { get => Projectile.ai[0]; set => Projectile.ai[0] = value; }
        private float TargetY { get => Projectile.ai[1]; set => Projectile.ai[1] = value; }

        private Player Owner => Main.player[Projectile.owner];
        private int chargeTimer;
        private bool isFiring;
        private bool isFading;
        private int fadeTimer;
        private float mergeBurstPower;

        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.LastPrism;

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 9999;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 6;
        }

        public override bool ShouldUpdatePosition() => false;

        #region AI

        public override void AI()
        {
            Player player = Owner;
            if (player.dead || !player.active) { Projectile.Kill(); return; }

            // Fade phase runs independently
            if (isFading)
            {
                fadeTimer++;
                if (fadeTimer >= FadeFrames)
                    Projectile.Kill();
                return;
            }

            // Detect release (channel goes false when mouse button released)
            if (Main.myPlayer == Projectile.owner && !player.channel)
            {
                StartFade();
                return;
            }

            // Lock player to this projectile
            player.heldProj = Projectile.whoAmI;
            player.itemTime = 2;
            player.itemAnimation = 2;

            // Anchor to player center
            Projectile.Center = player.MountedCenter;

            // Slow cursor tracking
            if (Main.myPlayer == Projectile.owner)
            {
                TargetX = MathHelper.Lerp(TargetX, Main.MouseWorld.X, CursorLerpSpeed);
                TargetY = MathHelper.Lerp(TargetY, Main.MouseWorld.Y, CursorLerpSpeed);
            }

            // Face target + arm rotation
            Vector2 toTarget = (new Vector2(TargetX, TargetY) - player.MountedCenter).SafeNormalize(Vector2.UnitX);
            player.direction = toTarget.X >= 0 ? 1 : -1;
            player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full,
                toTarget.ToRotation() - MathHelper.PiOver2);

            if (!isFiring)
                AI_Charge();
            else
                AI_Fire();
        }

        private void AI_Charge()
        {
            chargeTimer++;
            float progress = (float)chargeTimer / ChargeFrames;
            Vector2 muzzle = GetMuzzlePosition();

            // Charge sound
            if (chargeTimer == 1)
                SoundEngine.PlaySound(SoundID.Item15 with { Pitch = 0.5f, Volume = 0.5f }, muzzle);
            if (chargeTimer % 20 == 0 && chargeTimer < ChargeFrames)
                SoundEngine.PlaySound(SoundID.Item15 with { Pitch = 0.5f + progress * 0.5f, Volume = 0.3f + progress * 0.3f }, muzzle);

            // VFX+ pattern: dust converges from outer radius toward muzzle
            int dustCount = 2 + (int)(progress * 8);
            for (int i = 0; i < dustCount; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float dist = MathHelper.Lerp(90f, 15f, progress);
                Vector2 dustStart = muzzle + angle.ToRotationVector2() * dist;
                Vector2 dustVel = (muzzle - dustStart).SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(3f, 6f) * (0.5f + progress);
                Color dustColor = TerraBladeShaderManager.GetPaletteColor(Main.rand.NextFloat(0.3f, 0.9f));
                Dust d = Dust.NewDustPerfect(dustStart, DustID.GreenTorch, dustVel, 0, dustColor, 0.8f + progress);
                d.noGravity = true;
                d.fadeIn = 1.2f;
            }

            // BloomRing pulses
            if (chargeTimer % 12 == 0)
            {
                Color ringColor = TerraBladeShaderManager.GetPaletteColor(0.5f + progress * 0.3f);
                var ring = new BloomRingParticle(muzzle, Vector2.Zero, ringColor * 0.5f, 0.08f + progress * 0.12f, 12);
                MagnumParticleHandler.SpawnParticle(ring);
            }

            Projectile.AddTrauma(0.01f + 0.02f * progress);
            Color lightColor = TerraBladeShaderManager.GetPaletteColor(0.5f);
            Lighting.AddLight(muzzle, lightColor.ToVector3() * (0.3f + progress * 0.8f));

            // Merge transition
            if (chargeTimer >= ChargeFrames)
            {
                isFiring = true;
                mergeBurstPower = 1f;

                // VFX+ merge burst: flash, shake, chromatic aberration, layered sounds
                CalamityBeamSystem.CreateStartupEffect(muzzle, "TerraBlade", 1.5f);
                ScreenFlashSystem.Instance?.ImpactFlash(0.5f);
                Projectile.ShakeScreen(0.7f);
                ScreenDistortionManager.TriggerChromaticBurst(muzzle, intensity: 0.7f, duration: 15);

                SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.2f, Volume = 1.0f }, muzzle);
                SoundEngine.PlaySound(SoundID.Item68 with { Pitch = -0.3f, Volume = 0.6f }, muzzle);
            }
        }

        private void AI_Fire()
        {
            // VFX+ pattern: mergeBurstPower decays via Lerp toward -0.5, clamped to 0+
            mergeBurstPower = MathHelper.Clamp(MathHelper.Lerp(mergeBurstPower, -0.5f, 0.06f), 0f, 10f);

            Vector2 endPoint = GetBeamEndPoint();

            // Impact dust at endpoint
            if (Main.GameUpdateCount % 3 == 0)
            {
                Color impactColor = TerraBladeShaderManager.GetPaletteColor(Main.rand.NextFloat(0.4f, 0.8f));
                Dust d = Dust.NewDustPerfect(endPoint + Main.rand.NextVector2Circular(15f, 15f),
                    DustID.GreenTorch, Main.rand.NextVector2Circular(3f, 3f), 0, impactColor, 1.5f);
                d.noGravity = true;
            }

            Projectile.AddTrauma(0.02f);

            Color light = TerraBladeShaderManager.GetPaletteColor(0.6f);
            Vector2 muzzle = GetMuzzlePosition();
            Lighting.AddLight(muzzle, light.ToVector3() * 1.1f);
            Lighting.AddLight(endPoint, light.ToVector3() * 1.1f);
        }

        private void StartFade()
        {
            isFading = true;
            fadeTimer = 0;

            if (isFiring)
            {
                Vector2 endPoint = GetBeamEndPoint();
                CalamityBeamSystem.CreateImpactEffect(endPoint, "TerraBlade", 1.0f);
            }
        }

        #endregion

        #region Position Helpers

        private Vector2 GetMuzzlePosition()
        {
            Vector2 toTarget = (new Vector2(TargetX, TargetY) - Owner.MountedCenter).SafeNormalize(Vector2.UnitX);
            return Owner.MountedCenter + toTarget * 40f;
        }

        private Vector2 GetBeamEndPoint()
        {
            Vector2 muzzle = GetMuzzlePosition();
            Vector2 toTarget = (new Vector2(TargetX, TargetY) - muzzle).SafeNormalize(Vector2.UnitX);
            return muzzle + toTarget * MaxBeamLength;
        }

        #endregion

        #region Collision

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (!isFiring || isFading) return false;

            Vector2 start = GetMuzzlePosition();
            Vector2 end = GetBeamEndPoint();
            float _ = 0f;
            return Collision.CheckAABBvLineCollision(
                targetHitbox.TopLeft(), targetHitbox.Size(),
                start, end, BeamBaseWidth * 0.5f, ref _);
        }

        #endregion

        #region Rendering

        public override bool PreDraw(ref Color lightColor)
        {
            if (isFiring || isFading)
                DrawSustainedBeam();
            else
                DrawConvergingBeams();

            DrawMuzzleFlare();

            if (isFiring || isFading)
                DrawEndpointFlare();

            return false;
        }

        /// <summary>
        /// VFX+ charge phase: 6 thin beams at angular offsets that converge as charge progresses.
        /// Opacity ramp: 0→0.4 over first 67%, then 0.4→1.0.
        /// Width scales with easeOutCirc(opacity) per VFX+ LastPrismLaserOverride.
        /// </summary>
        private void DrawConvergingBeams()
        {
            float progress = MathHelper.Clamp((float)chargeTimer / ChargeFrames, 0f, 1f);
            if (progress <= 0) return;

            SpriteBatch sb = Main.spriteBatch;
            Vector2 muzzle = GetMuzzlePosition();
            Vector2 muzzleScreen = muzzle - Main.screenPosition;
            Vector2 baseDir = (new Vector2(TargetX, TargetY) - muzzle).SafeNormalize(Vector2.UnitX);

            // VFX+ opacity ramp: slow then fast
            float opacity = progress < 0.67f
                ? progress / 0.67f * 0.4f
                : 0.4f + (progress - 0.67f) / 0.33f * 0.6f;

            // VFX+ easeOutCirc for width
            float widthEase = MathF.Sqrt(1f - MathF.Pow(opacity - 1f, 2f));
            float beamPx = 8f * widthEase;

            // VFX+ converge: Lerp(maxAngle, 0, easeOutQuad(progress))
            float easeQuad = 1f - (1f - progress) * (1f - progress);
            float angleSpread = MathHelper.Lerp(MaxConvergeAngle, 0f, easeQuad);
            float beamLen = MathHelper.Lerp(200f, MaxBeamLength * 0.5f, easeQuad);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Texture2D bloomTex = SandboxVFXHelper.SafeRequest("MagnumOpus/Assets/VFX/Blooms/Perfect Soft Color Bloom")
                                ?? Terraria.GameContent.TextureAssets.Extra[98].Value;

            for (int i = 0; i < ConvergingBeamCount; i++)
            {
                // Distribute beams symmetrically around center axis
                float t = (i - (ConvergingBeamCount - 1) * 0.5f) / ((ConvergingBeamCount - 1) * 0.5f);
                Vector2 beamDir = baseDir.RotatedBy(t * angleSpread);
                Vector2 endScreen = (muzzle + beamDir * beamLen) - Main.screenPosition;

                Color beamColor = TerraBladeShaderManager.GetPaletteColor((float)i / ConvergingBeamCount) with { A = 0 };

                // Stretched bloom sprite as beam line
                Vector2 midPoint = (muzzleScreen + endScreen) * 0.5f;
                float drawLen = (endScreen - muzzleScreen).Length();
                float rotation = beamDir.ToRotation();
                float scaleX = drawLen / bloomTex.Width;
                float scaleY = beamPx / bloomTex.Height;

                sb.Draw(bloomTex, midPoint, null, beamColor * opacity * 0.6f,
                    rotation, bloomTex.Size() * 0.5f, new Vector2(scaleX, scaleY),
                    SpriteEffects.None, 0f);

                // VFX+ bloom orb at each beam endpoint
                sb.Draw(bloomTex, endScreen, null, beamColor * opacity * 0.4f,
                    0f, bloomTex.Size() * 0.5f, 0.04f * widthEase, SpriteEffects.None, 0f);
            }

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        /// <summary>
        /// VFX+ merged beam: wide beam via shader pipeline with merge burst width decay.
        /// Width = base + mergeBurstPower * extra (starts inflated, decays to base).
        /// </summary>
        private void DrawSustainedBeam()
        {
            float fade = isFading ? 1f - (float)fadeTimer / FadeFrames : 1f;
            if (fade <= 0) return;

            Vector2 start = GetMuzzlePosition();
            Vector2 end = GetBeamEndPoint();
            float effectiveWidth = BeamBaseWidth + mergeBurstPower * MergeBurstExtra;

            Effect shader = ShaderLoader.BeamGradientFlow;
            if (shader != null)
            {
                SandboxTerraBladeBeam.EnsureShaderPool();
                if (SandboxTerraBladeBeam.ShaderVertices != null)
                {
                    SandboxTerraBladeBeam.RenderShaderBeam(shader, start, end, fade,
                        effectiveWidth, CalamityBeamSystem.WidthStyle.SourceTaper, 3.5f, 60);
                    SandboxTerraBladeBeam.EmitTerraBladeDust(start, end, fade * 1.5f);
                    return;
                }
            }

            // Fallback
            CalamityBeamSystem.RenderBeam(start, end, "TerraBlade", effectiveWidth);
        }

        private void DrawMuzzleFlare()
        {
            float intensity;
            if (!isFiring && !isFading)
                intensity = MathHelper.Clamp((float)chargeTimer / ChargeFrames, 0f, 1f);
            else if (isFading)
                intensity = 1f - (float)fadeTimer / FadeFrames;
            else
                intensity = 1f;

            if (intensity <= 0) return;

            SpriteBatch sb = Main.spriteBatch;
            Vector2 muzzle = GetMuzzlePosition() - Main.screenPosition;
            float pulse = 1f + MathF.Sin(Main.GlobalTimeWrappedHourly * 12f) * 0.1f;

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Texture2D bloomTex = SandboxVFXHelper.SafeRequest("MagnumOpus/Assets/VFX/Blooms/Perfect Soft Color Bloom")
                                ?? Terraria.GameContent.TextureAssets.Extra[98].Value;
            Vector2 origin = bloomTex.Size() * 0.5f;

            Color outerColor = TerraBladeShaderManager.GetPaletteColor(0.3f) with { A = 0 };
            float outerScale = (0.15f + intensity * 0.4f + mergeBurstPower * 0.3f) * pulse;
            sb.Draw(bloomTex, muzzle, null, outerColor * 0.5f * intensity, 0f, origin, outerScale, SpriteEffects.None, 0f);

            Color innerColor = TerraBladeShaderManager.GetPaletteColor(0.7f) with { A = 0 };
            float innerScale = (0.05f + intensity * 0.15f + mergeBurstPower * 0.15f) * pulse;
            sb.Draw(bloomTex, muzzle, null, innerColor * 0.7f * intensity, 0f, origin, innerScale, SpriteEffects.None, 0f);

            sb.Draw(bloomTex, muzzle, null, (Color.White with { A = 0 }) * 0.6f * intensity,
                0f, origin, innerScale * 0.3f * pulse, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        private void DrawEndpointFlare()
        {
            float fade = isFading ? 1f - (float)fadeTimer / FadeFrames : 1f;
            if (fade <= 0) return;

            SpriteBatch sb = Main.spriteBatch;
            Vector2 endpoint = GetBeamEndPoint() - Main.screenPosition;
            float pulse = 1f + MathF.Sin(Main.GlobalTimeWrappedHourly * 10f) * 0.08f;

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Texture2D bloomTex = SandboxVFXHelper.SafeRequest("MagnumOpus/Assets/VFX/Blooms/Perfect Soft Color Bloom")
                                ?? Terraria.GameContent.TextureAssets.Extra[98].Value;
            Vector2 origin = bloomTex.Size() * 0.5f;

            // VFX+ pattern: endpoint glow boosted during merge burst
            float boostScale = 0.3f + mergeBurstPower * 0.2f;
            Color endColor = TerraBladeShaderManager.GetPaletteColor(0.5f) with { A = 0 };
            sb.Draw(bloomTex, endpoint, null, endColor * 0.6f * fade, 0f, origin, boostScale * pulse, SpriteEffects.None, 0f);
            sb.Draw(bloomTex, endpoint, null, (Color.White with { A = 0 }) * 0.4f * fade,
                0f, origin, boostScale * 0.3f * pulse, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        #endregion

        #region Hit Effects

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            ScreenFlashSystem.Instance?.ImpactFlash(0.15f);

            for (int i = 0; i < 4; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(4f, 4f);
                Color dustColor = TerraBladeShaderManager.GetPaletteColor(Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(target.Center, DustID.GreenTorch, vel, 0, dustColor, 1.2f);
                d.noGravity = true;
            }

            Lighting.AddLight(target.Center, 0.5f, 1.0f, 0.6f);
        }

        #endregion

        #region Networking

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(chargeTimer);
            writer.Write(isFiring);
            writer.Write(isFading);
            writer.Write(fadeTimer);
            writer.Write(mergeBurstPower);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            chargeTimer = reader.ReadInt32();
            isFiring = reader.ReadBoolean();
            isFading = reader.ReadBoolean();
            fadeTimer = reader.ReadInt32();
            mergeBurstPower = reader.ReadSingle();
        }

        #endregion
    }
}
