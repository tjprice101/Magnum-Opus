using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.FoundationWeapons.MagicOrbFoundation
{
    /// <summary>
    /// MagicOrb — A floating noise-textured orb that fires shiny bloom bolts at enemies.
    /// 
    /// VISUAL ARCHITECTURE:
    /// 1. RADIAL NOISE SHADER (reuses MaskFoundation/RadialNoiseMaskShader.fx)
    ///    — Renders a radially scrolling noise texture masked to a circle.
    /// 2. BLOOM HALO — Multi-scale additive glow behind the orb.
    /// 3. CORE BLOOM — Bright center point glow on top.
    /// 
    /// BEHAVIOUR:
    /// Normal mode (ai[1] = 0): Moves slowly (speed 4), long lifetime (480 frames),
    ///   fires OrbBolt every 30 frames at nearest enemy in 400px radius.
    /// Burst mode (ai[1] = 1): Moves faster (speed 7), short lifetime (120 frames),
    ///   fires OrbBolt every 20 frames, explodes on death with area damage.
    /// 
    /// ai[0] = OrbNoiseStyle index (determines noise texture + colors)
    /// ai[1] = 0 (normal) or 1 (burst, from right-click)
    /// </summary>
    public class MagicOrb : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.CrystalBullet;

        // ---- CONSTANTS ----
        private const int NormalLifetime = 480;
        private const int BurstLifetime = 120;
        private const int NormalFireRate = 30;
        private const int BurstFireRate = 20;
        private const float DetectionRadius = 400f;
        private const float OrbDrawScale = 0.15f;
        private const int FadeOutFrames = 30;
        private const int FadeInFrames = 12;

        // ---- STATE ----
        private int timer;
        private float seed;
        private int fireTimer;
        private Effect orbShader;

        private OrbNoiseStyle CurrentStyle => (OrbNoiseStyle)(int)Projectile.ai[0];
        private bool IsBurstMode => Projectile.ai[1] >= 1f;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DrawScreenCheckFluff[Projectile.type] = 500;
        }

        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.timeLeft = NormalLifetime;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 30;
            Projectile.hide = false;
            Projectile.alpha = 0;
        }

        public override void AI()
        {
            // ---- INIT ----
            if (timer == 0)
            {
                seed = Main.rand.NextFloat(100f);
                if (IsBurstMode)
                    Projectile.timeLeft = BurstLifetime;
            }

            timer++;
            fireTimer++;

            // ---- SLOW DRIFT ----
            // Orbs decelerate slightly over time for a floating feel
            if (!IsBurstMode)
            {
                if (Projectile.velocity.Length() > 2f)
                    Projectile.velocity *= 0.985f;
            }
            else
            {
                // Burst orbs maintain speed longer, slight decel
                if (Projectile.velocity.Length() > 3.5f)
                    Projectile.velocity *= 0.992f;
            }

            // Gentle sine wave bob perpendicular to velocity
            float bobAmplitude = IsBurstMode ? 0.3f : 0.5f;
            float bobFreq = IsBurstMode ? 0.08f : 0.05f;
            Vector2 perpDir = new Vector2(-Projectile.velocity.Y, Projectile.velocity.X).SafeNormalize(Vector2.UnitY);
            Projectile.position += perpDir * MathF.Sin(timer * bobFreq + seed) * bobAmplitude;

            // Visual spin
            Projectile.rotation += 0.02f;

            // ---- FIRE SUB-PROJECTILES ----
            int fireRate = IsBurstMode ? BurstFireRate : NormalFireRate;
            if (fireTimer >= fireRate && timer > FadeInFrames)
            {
                fireTimer = 0;
                TryFireBolt();
            }

            // ---- LIGHTING ----
            Color[] colors = MOFTextures.GetStyleColors(CurrentStyle);
            float lightIntensity = IsBurstMode ? 0.7f : 0.5f;
            Lighting.AddLight(Projectile.Center, colors[0].ToVector3() * lightIntensity);

            // ---- AMBIENT DUST ----
            if (Main.rand.NextBool(5))
            {
                Color dustColor = colors[Main.rand.Next(colors.Length)];
                Vector2 dustVel = Main.rand.NextVector2Circular(1.5f, 1.5f);
                Dust dust = Dust.NewDustPerfect(
                    Projectile.Center + Main.rand.NextVector2Circular(16f, 16f),
                    DustID.RainbowMk2, dustVel, newColor: dustColor,
                    Scale: Main.rand.NextFloat(0.2f, 0.45f));
                dust.noGravity = true;
                dust.fadeIn = 0.4f;
            }
        }

        /// <summary>
        /// Finds the nearest enemy in detection radius and fires an OrbBolt at it.
        /// </summary>
        private void TryFireBolt()
        {
            NPC target = FindClosestTarget();
            if (target == null)
                return;

            Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
            float boltSpeed = 10f;
            Vector2 boltVel = toTarget * boltSpeed;

            // Pass the orb's noise style as ai[0] for color matching
            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(), Projectile.Center, boltVel,
                ModContent.ProjectileType<OrbBolt>(),
                Projectile.damage / 2, Projectile.knockBack * 0.3f, Projectile.owner,
                ai0: (float)CurrentStyle);

            // Subtle firing sound
            SoundEngine.PlaySound(SoundID.Item12 with { Volume = 0.3f, Pitch = 0.6f }, Projectile.Center);
        }

        private NPC FindClosestTarget()
        {
            NPC closest = null;
            float closestDist = DetectionRadius;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage || npc.immortal)
                    continue;

                float dist = Vector2.Distance(Projectile.Center, npc.Center);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = npc;
                }
            }

            return closest;
        }

        // =====================================================================
        // EXPLOSION ON DEATH (Burst mode)
        // =====================================================================

        public override void OnKill(int timeLeft)
        {
            Color[] colors = MOFTextures.GetStyleColors(CurrentStyle);

            if (IsBurstMode)
            {
                // Burst orbs explode: area damage via NewProjectile with 0 timeLeft
                // Spawn dust explosion
                for (int i = 0; i < 25; i++)
                {
                    Vector2 vel = Main.rand.NextVector2Circular(8f, 8f);
                    Color col = colors[Main.rand.Next(colors.Length)];
                    Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.RainbowMk2, vel,
                        newColor: col, Scale: Main.rand.NextFloat(0.5f, 0.9f));
                    dust.noGravity = true;
                    dust.fadeIn = 0.6f;
                }

                // Fire 6 bolts in all directions for the explosion
                for (int i = 0; i < 6; i++)
                {
                    float angle = MathHelper.TwoPi / 6f * i + Main.rand.NextFloat(-0.15f, 0.15f);
                    Vector2 boltVel = angle.ToRotationVector2() * Main.rand.NextFloat(7f, 11f);
                    Projectile.NewProjectile(
                        Projectile.GetSource_FromThis(), Projectile.Center, boltVel,
                        ModContent.ProjectileType<OrbBolt>(),
                        Projectile.damage, Projectile.knockBack * 0.5f, Projectile.owner,
                        ai0: (float)CurrentStyle);
                }

                SoundEngine.PlaySound(SoundID.Item14 with { Volume = 0.7f, Pitch = 0.2f }, Projectile.Center);
            }
            else
            {
                // Normal orbs: gentle dissipation
                for (int i = 0; i < 12; i++)
                {
                    Vector2 vel = Main.rand.NextVector2Circular(4f, 4f);
                    Color col = colors[Main.rand.Next(colors.Length)];
                    Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.RainbowMk2, vel,
                        newColor: col, Scale: Main.rand.NextFloat(0.3f, 0.6f));
                    dust.noGravity = true;
                    dust.fadeIn = 0.5f;
                }
            }
        }

        // =====================================================================
        // RENDERING
        // =====================================================================

        private float GetAlpha()
        {
            int maxLife = IsBurstMode ? BurstLifetime : NormalLifetime;
            float fadeIn = MathHelper.Clamp(timer / (float)FadeInFrames, 0f, 1f);
            float fadeOut = Projectile.timeLeft < FadeOutFrames
                ? Projectile.timeLeft / (float)FadeOutFrames
                : 1f;
            return fadeIn * fadeOut;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            try
            {
            Color[] colors = MOFTextures.GetStyleColors(CurrentStyle);
            float alpha = GetAlpha();

            // ---- LAYER 1: BLOOM HALO ----
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

            DrawBloomHalo(sb, colors, alpha);

            // ---- LAYER 2: NOISE ORB VIA SHADER ----
            DrawShaderOrb(sb, colors, alpha);

            // ---- LAYER 3: CORE BLOOM ----
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

            DrawCoreBloom(sb, colors, alpha);

            // ---- RESTORE ----
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

            }
            catch { }
            finally
            {
                try { sb.End(); } catch { }
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                    DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }

            return false;
        }

        private void DrawBloomHalo(SpriteBatch sb, Color[] colors, float alpha)
        {
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Texture2D softGlow = MOFTextures.SoftGlow.Value;
            Vector2 glowOrigin = softGlow.Size() / 2f;

            float pulse = 0.85f + 0.15f * MathF.Sin((float)Main.timeForVisualEffects * 0.06f + seed);
            float burstScale = IsBurstMode ? 1.1f : 1f;

            // Outer glow
            sb.Draw(softGlow, drawPos, null, colors[0] * (0.2f * alpha * pulse), 0f,
                glowOrigin, 0.2f * pulse * burstScale, SpriteEffects.None, 0f);

            // Mid glow
            sb.Draw(softGlow, drawPos, null, colors[1] * (0.3f * alpha * pulse), 0f,
                glowOrigin, 0.12f * pulse * burstScale, SpriteEffects.None, 0f);

            // Inner glow
            sb.Draw(softGlow, drawPos, null, colors[2] * (0.4f * alpha * pulse), 0f,
                glowOrigin, 0.07f * pulse * burstScale, SpriteEffects.None, 0f);
        }

        private void DrawShaderOrb(SpriteBatch sb, Color[] colors, float alpha)
        {
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float time = (float)Main.timeForVisualEffects;

            // Load RadialNoiseMaskShader from MaskFoundation
            if (orbShader == null)
            {
                orbShader = ModContent.Request<Effect>(
                    "MagnumOpus/Content/FoundationWeapons/MaskFoundation/Shaders/RadialNoiseMaskShader",
                    AssetRequestMode.ImmediateLoad).Value;
            }

            // Configure shader
            orbShader.Parameters["uTime"]?.SetValue(time * 0.015f + seed);
            orbShader.Parameters["scrollSpeed"]?.SetValue(0.35f);
            orbShader.Parameters["rotationSpeed"]?.SetValue(0.2f);
            orbShader.Parameters["circleRadius"]?.SetValue(0.43f);
            orbShader.Parameters["edgeSoftness"]?.SetValue(0.07f);
            orbShader.Parameters["intensity"]?.SetValue(2.2f);
            orbShader.Parameters["primaryColor"]?.SetValue(colors[0].ToVector3());
            orbShader.Parameters["coreColor"]?.SetValue(colors[2].ToVector3());
            orbShader.Parameters["noiseTex"]?.SetValue(MOFTextures.GetNoiseForStyle(CurrentStyle));
            orbShader.Parameters["gradientTex"]?.SetValue(MOFTextures.GetGradientForStyle(CurrentStyle));

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                SamplerState.LinearWrap, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, orbShader,
                Main.GameViewMatrix.TransformationMatrix);

            Texture2D orbTex = MOFTextures.SoftCircle.Value;
            Vector2 orbOrigin = orbTex.Size() / 2f;

            sb.Draw(orbTex, drawPos, null, Color.White * alpha, 0f,
                orbOrigin, OrbDrawScale, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);
        }

        private void DrawCoreBloom(SpriteBatch sb, Color[] colors, float alpha)
        {
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Texture2D glowOrb = MOFTextures.GlowOrb.Value;
            Vector2 orbOrigin = glowOrb.Size() / 2f;

            float pulse = 0.9f + 0.1f * MathF.Sin((float)Main.timeForVisualEffects * 0.1f + seed * 2f);

            sb.Draw(glowOrb, drawPos, null, colors[2] * (0.35f * alpha * pulse), 0f,
                orbOrigin, 0.06f * pulse, SpriteEffects.None, 0f);

            sb.Draw(glowOrb, drawPos, null, colors[0] * (0.18f * alpha * pulse), 0f,
                orbOrigin, 0.1f * pulse, SpriteEffects.None, 0f);
        }

        // The orb itself doesn't deal direct contact damage — it fires bolts
        public override bool? CanDamage() => false;

        public override Color? GetAlpha(Color lightColor) => Color.White;
    }
}
