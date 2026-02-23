using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Shaders;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Effects;
using MagnumOpus.Content.TestWeapons.SandboxWeapons;
using MagnumOpus.Content.TestWeapons.SandboxWeapons.Shaders;

namespace MagnumOpus.Content.TestWeapons.SandboxWeapons
{
    /// <summary>
    /// Fast-moving 4-pointed star projectile spawned by the Sandbox Terra Blade swing.
    /// 3 of these are spawned per swing in a fan pattern toward the cursor.
    ///
    /// VFX Layers:
    ///   1. Chromatic afterimages (RGB-split shimmer)
    ///   2. Multi-pass vertex strip trail (bloom + main + core)
    ///   3. Motion blur (velocity-stretched directional blur)
    ///   4. Star body (multi-layer additive draws, velocity-stretched)
    ///   5. Light shimmer (pulsing bloom + FlareSparkle center)
    ///
    /// On hit: LingeringFlameZone + screen chromatic burst + impact light rays
    /// </summary>
    public class CrispStarProjectile : ModProjectile
    {
        #region Constants

        private const int TrailCacheSize = 30;
        private const int LaunchFrames = 4;
        private const float HomingRange = 500f;
        private const float HomingLerp = 0.04f;
        private const float HomingSpeed = 18f;
        private const float Deceleration = 0.94f;

        #endregion

        #region State

        private int cachedTargetWhoAmI = -1;
        private int timer = 0;
        private float spinRotation = 0f;

        #endregion

        #region Setup

        public override string Texture => "MagnumOpus/Assets/Particles/CrispStar4";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = TrailCacheSize;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.penetrate = 1;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 180;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        #endregion

        #region AI

        public override void AI()
        {
            timer++;

            // Fast spin — stars spin quicker than gears
            float spinSpeed = 0.25f + Projectile.velocity.Length() * 0.03f;
            spinRotation += spinSpeed;
            Projectile.rotation = spinRotation;

            if (timer <= LaunchFrames)
            {
                Projectile.velocity *= Deceleration;
            }
            else
            {
                if (cachedTargetWhoAmI < 0)
                    AcquireTarget();

                if (cachedTargetWhoAmI >= 0 && cachedTargetWhoAmI < Main.maxNPCs)
                {
                    NPC target = Main.npc[cachedTargetWhoAmI];
                    if (target.active && !target.friendly && !target.dontTakeDamage)
                    {
                        Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitY);
                        Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * HomingSpeed, HomingLerp);
                    }
                    else
                    {
                        cachedTargetWhoAmI = -1;
                    }
                }
                else
                {
                    Projectile.velocity *= 0.995f;
                }
            }

            // Ambient dust trail
            if (timer % 3 == 0)
            {
                Color dustColor = TerraBladeShaderManager.GetPaletteColor(Main.rand.NextFloat(0.3f, 0.7f));
                Dust d = Dust.NewDustPerfect(
                    Projectile.Center + Main.rand.NextVector2Circular(10f, 10f),
                    DustID.GreenTorch,
                    -Projectile.velocity * 0.08f,
                    0, dustColor, 1.0f);
                d.noGravity = true;
                d.fadeIn = 1.1f;
            }

            // GlowSpark particles
            if (timer % 5 == 0)
            {
                Vector2 sparkVel = -Projectile.velocity.SafeNormalize(Vector2.UnitY) * Main.rand.NextFloat(1f, 2.5f);
                sparkVel = sparkVel.RotatedBy(Main.rand.NextFloat(-0.5f, 0.5f));
                Color sparkColor = TerraBladeShaderManager.GetPaletteColor(Main.rand.NextFloat(0.4f, 0.8f));
                var spark = new GlowSparkParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(8f, 8f),
                    sparkVel, sparkColor, 0.18f, 14);
                MagnumParticleHandler.SpawnParticle(spark);
            }

            // Chromatic afterimages every 3 frames
            if (timer % 3 == 0 && timer > LaunchFrames)
            {
                Projectile.SpawnChromaticAfterimage(2.5f, 12);
            }

            // Dynamic lighting
            Color light = TerraBladeShaderManager.GetPaletteColor(0.5f);
            Lighting.AddLight(Projectile.Center, light.ToVector3() * 0.7f);
        }

        private void AcquireTarget()
        {
            float bestDist = HomingRange;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage || npc.immortal)
                    continue;
                float dist = Vector2.Distance(Projectile.Center, npc.Center);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    cachedTargetWhoAmI = i;
                }
            }
        }

        #endregion

        #region Hit Effects

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Spawn LingeringFlameZone at impact
            if (Main.myPlayer == Projectile.owner)
            {
                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(), target.Center, Vector2.Zero,
                    ModContent.ProjectileType<LingeringFlameZone>(),
                    Projectile.damage / 2, 0f, Projectile.owner);
            }

            // Screen chromatic burst distortion
            ScreenDistortionManager.TriggerChromaticBurst(target.Center, intensity: 0.6f, duration: 12);

            // Impact light rays
            Color rayPrimary = TerraBladeShaderManager.GetPaletteColor(0.5f);
            Color raySecondary = TerraBladeShaderManager.GetPaletteColor(0.8f);
            ImpactLightRays.SpawnImpactRays(target.Center, rayPrimary, raySecondary, rayCount: 6, scale: 0.8f);

            // Impact dust burst
            for (int i = 0; i < 12; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(7f, 7f);
                Color dustColor = TerraBladeShaderManager.GetPaletteColor(Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(target.Center, DustID.GreenTorch, vel, 0, dustColor, 1.4f);
                d.noGravity = true;
                d.fadeIn = 1.2f;
            }

            // Gold sparks
            for (int i = 0; i < 4; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(4f, 4f);
                Dust d = Dust.NewDustPerfect(target.Center, DustID.Enchanted_Gold, vel, 0, Color.White, 0.9f);
                d.noGravity = true;
            }

            // Bloom ring flash
            for (int i = 0; i < 2; i++)
            {
                float ringScale = 0.25f + i * 0.15f;
                Color ringColor = TerraBladeShaderManager.GetPaletteColor(0.4f + i * 0.2f);
                var ring = new BloomRingParticle(target.Center, Vector2.Zero, ringColor * 0.8f, ringScale, 16);
                MagnumParticleHandler.SpawnParticle(ring);
            }

            Lighting.AddLight(target.Center, 1.0f, 1.5f, 1.0f);
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 8; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(4f, 4f);
                Color dustColor = TerraBladeShaderManager.GetPaletteColor(Main.rand.NextFloat(0.3f, 0.7f));
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.GreenTorch, vel, 0, dustColor, 1.0f);
                d.noGravity = true;
                d.fadeIn = 1.1f;
            }

            Lighting.AddLight(Projectile.Center, 0.4f, 0.6f, 0.4f);
        }

        #endregion

        #region Rendering

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float time = Main.GlobalTimeWrappedHourly;

            // 1. Multi-pass vertex strip trail
            DrawMultiPassTrail();

            // 2. Motion blur on star body
            DrawMotionBlur(sb, time);

            // 3. Star body with velocity stretch
            DrawStarBody(sb, drawPos, time);

            // 4. Light shimmer overlays
            DrawLightShimmer(sb, drawPos, time);

            return false;
        }

        // =================================================================
        // LAYER 1: Multi-Pass Vertex Strip Trail
        // =================================================================

        private void DrawMultiPassTrail()
        {
            int validCount = 0;
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] != Vector2.Zero)
                    validCount++;
                else
                    break;
            }
            if (validCount < 3) return;

            Vector2[] positions = new Vector2[validCount + 1];
            positions[0] = Projectile.Center;
            for (int i = 0; i < validCount; i++)
            {
                positions[i + 1] = Projectile.oldPos[i] + Projectile.Size * 0.5f;
            }

            Color trailColor1 = TerraBladeShaderManager.GetPaletteColor(0.4f);
            Color trailColor2 = TerraBladeShaderManager.GetPaletteColor(0.7f);

            EnhancedTrailRenderer.RenderMultiPassTrail(
                positions,
                EnhancedTrailRenderer.LinearTaper(35f),
                EnhancedTrailRenderer.GradientColor(trailColor1, trailColor2, 0.9f),
                bloomMultiplier: 2.5f,
                coreMultiplier: 0.35f);
        }

        // =================================================================
        // LAYER 2: Motion Blur
        // =================================================================

        private void DrawMotionBlur(SpriteBatch sb, float time)
        {
            Texture2D starTex = ModContent.Request<Texture2D>(Texture).Value;
            if (starTex == null) return;

            Color blurColor = TerraBladeShaderManager.GetPaletteColor(0.5f);
            MotionBlurBloomRenderer.DrawProjectile(sb, starTex, Projectile,
                blurColor, secondaryColor: Color.White, intensityMult: 1.2f);
        }

        // =================================================================
        // LAYER 3: Star Body (velocity-stretched, multi-layer)
        // =================================================================

        private void DrawStarBody(SpriteBatch sb, Vector2 drawPos, float time)
        {
            Texture2D starTex = ModContent.Request<Texture2D>(Texture).Value;
            if (starTex == null) return;

            Vector2 origin = starTex.Size() * 0.5f;
            float pulse = 1f + MathF.Sin(time * 10f) * 0.1f;

            // Velocity stretch: elongate along travel direction
            float speed = Projectile.velocity.Length();
            float stretchFactor = 1f + speed * 0.02f;
            float velRotation = Projectile.velocity.ToRotation();

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);

            // Outer glow layer
            Color outerColor = TerraBladeShaderManager.GetPaletteColor(0.3f);
            sb.Draw(starTex, drawPos, null, outerColor with { A = 0 } * 0.3f,
                spinRotation, origin, new Vector2(1.5f * stretchFactor, 1.5f) * pulse, SpriteEffects.None, 0f);

            // Main star body
            Color mainColor = TerraBladeShaderManager.GetPaletteColor(0.5f);
            sb.Draw(starTex, drawPos, null, mainColor with { A = 0 } * 0.8f,
                spinRotation, origin, new Vector2(1.0f * stretchFactor, 1.0f) * pulse, SpriteEffects.None, 0f);

            // Velocity-aligned stretched layer (speed lines feel)
            Color stretchColor = TerraBladeShaderManager.GetPaletteColor(0.6f);
            sb.Draw(starTex, drawPos, null, stretchColor with { A = 0 } * 0.4f,
                velRotation, origin, new Vector2(1.2f * stretchFactor * 1.3f, 0.6f) * pulse, SpriteEffects.None, 0f);

            // White-hot core
            sb.Draw(starTex, drawPos, null, Color.White with { A = 0 } * 0.5f,
                spinRotation, origin, new Vector2(0.4f, 0.4f) * pulse, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                DepthStencilState.None, RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);
        }

        // =================================================================
        // LAYER 4: Light Shimmer
        // =================================================================

        private void DrawLightShimmer(SpriteBatch sb, Vector2 drawPos, float time)
        {
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);

            // Pulsing bloom halo — simulates light reflecting off the star
            Texture2D bloomTex = SandboxVFXHelper.SafeRequest("MagnumOpus/Assets/VFX/Blooms/Perfect Soft Color Bloom");
            if (bloomTex == null)
                bloomTex = Terraria.GameContent.TextureAssets.Extra[98].Value;
            Vector2 bloomOrigin = bloomTex.Size() * 0.5f;

            float shimmer = MathF.Sin(time * 12f + timer * 0.15f);
            float shimmerPulse = 0.8f + shimmer * 0.2f;

            Color shimmerColor = TerraBladeShaderManager.GetPaletteColor(0.5f + shimmer * 0.15f);
            sb.Draw(bloomTex, drawPos, null, shimmerColor * 0.25f * shimmerPulse,
                0f, bloomOrigin, 0.6f, SpriteEffects.None, 0f);

            // Tight core glow
            Color coreColor = TerraBladeShaderManager.GetPaletteColor(0.7f);
            sb.Draw(bloomTex, drawPos, null, coreColor * 0.35f,
                0f, bloomOrigin, 0.2f * shimmerPulse, SpriteEffects.None, 0f);

            // FlareSparkle center — rotating light reflection
            Texture2D sparkleTex = SandboxVFXHelper.SafeRequest("MagnumOpus/Assets/Particles/FlareSparkle");
            if (sparkleTex != null)
            {
                Vector2 sparkleOrigin = sparkleTex.Size() * 0.5f;
                Color sparkleColor = TerraBladeShaderManager.GetPaletteColor(0.6f);
                sb.Draw(sparkleTex, drawPos, null, sparkleColor * 0.4f,
                    -time * 3f, sparkleOrigin, 0.2f * shimmerPulse, SpriteEffects.None, 0f);
            }

            // Light beam cross — velocity-aligned glint
            Texture2D bloomLine = SandboxVFXHelper.SafeRequest("MagnumOpus/Assets/VFX/Blooms/Horizontal Anamorphic Streak");
            if (bloomLine != null)
            {
                Vector2 lineOrigin = bloomLine.Size() * 0.5f;
                Color lineColor = TerraBladeShaderManager.GetPaletteColor(0.7f);
                float velAngle = Projectile.velocity.ToRotation();

                // Horizontal beam along velocity
                float beamPulse = 0.7f + MathF.Sin(time * 8f + 2f) * 0.3f;
                sb.Draw(bloomLine, drawPos, null, lineColor with { A = 0 } * 0.25f * beamPulse,
                    velAngle, lineOrigin, new Vector2(0.4f, 0.08f), SpriteEffects.None, 0f);

                // Perpendicular beam (cross glint)
                sb.Draw(bloomLine, drawPos, null, lineColor with { A = 0 } * 0.15f * beamPulse,
                    velAngle + MathHelper.PiOver2, lineOrigin, new Vector2(0.25f, 0.06f), SpriteEffects.None, 0f);
            }

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                DepthStencilState.None, RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);
        }

        #endregion

        #region Networking

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(cachedTargetWhoAmI);
            writer.Write(timer);
            writer.Write(spinRotation);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            cachedTargetWhoAmI = reader.ReadInt32();
            timer = reader.ReadInt32();
            spinRotation = reader.ReadSingle();
        }

        #endregion
    }
}
