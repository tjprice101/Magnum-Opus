using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.FoundationWeapons.AttackFoundation
{
    /// <summary>
    /// ComboSwingProjectile — Mode 2: Three-part melee combo animation.
    ///
    /// Phase 0 (frames 0-14):   Downward slash — blade arcs from upper-right to lower-left
    /// Phase 1 (frames 15-29):  Upward slash — blade sweeps from lower-left to upper-right
    /// Phase 2 (frames 30-49):  Spinning throw — blade spins outward toward the cursor
    ///
    /// Each phase deals damage in its arc. The final spin launches the blade as a projectile
    /// that travels to the cursor position and returns.
    ///
    /// ai[0] = mode index (always 1 for ComboSwing)
    /// </summary>
    public class ComboSwingProjectile : ModProjectile
    {
        public override string Texture => "Terraria/Images/Item_" + ItemID.Katana;

        // ---- TIMING ----
        private const int DownSwingFrames = 15;
        private const int UpSwingFrames = 15;
        private const int SpinThrowFrames = 20;
        private const int TotalFrames = DownSwingFrames + UpSwingFrames + SpinThrowFrames;

        // ---- GEOMETRY ----
        private const float SwingRadius = 60f;
        private const float ThrowDistance = 300f;
        private const float SpinRate = 0.5f;

        // ---- STATE ----
        private int timer;
        private int phase; // 0=down, 1=up, 2=spin-throw
        private float swingAngle;
        private float spinRotation;
        private Vector2 throwTarget;
        private Vector2 throwStart;
        private bool returning;
        private bool[] phaseDamageDealt = new bool[3];

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DrawScreenCheckFluff[Projectile.type] = 600;
        }

        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.timeLeft = TotalFrames + 30; // Extra safety time
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
            Projectile.alpha = 0;
            Projectile.ownerHitCheck = true;
        }

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            if (!owner.active || owner.dead)
            {
                Projectile.Kill();
                return;
            }

            timer++;
            Color[] colors = AFTextures.GetModeColors(AttackMode.ComboSwing);

            if (timer <= DownSwingFrames)
            {
                phase = 0;
                PhaseDownSwing(owner, colors);
            }
            else if (timer <= DownSwingFrames + UpSwingFrames)
            {
                phase = 1;
                PhaseUpSwing(owner, colors);
            }
            else if (timer <= TotalFrames)
            {
                phase = 2;
                PhaseSpinThrow(owner, colors);
            }
            else
            {
                Projectile.Kill();
                return;
            }

            // Lighting
            Lighting.AddLight(Projectile.Center, colors[1].ToVector3() * 0.5f);

            // Trail dust
            if (Main.rand.NextBool(3))
            {
                Color dustColor = colors[Main.rand.Next(colors.Length)];
                Dust dust = Dust.NewDustPerfect(
                    Projectile.Center + Main.rand.NextVector2Circular(6f, 6f),
                    DustID.RainbowMk2,
                    Main.rand.NextVector2Circular(2f, 2f),
                    newColor: dustColor,
                    Scale: Main.rand.NextFloat(0.3f, 0.5f));
                dust.noGravity = true;
                dust.fadeIn = 0.3f;
            }
        }

        private void PhaseDownSwing(Player owner, Color[] colors)
        {
            float progress = (timer - 1) / (float)DownSwingFrames;
            float easedProgress = EaseInOut(progress);

            // Swing from upper-right to lower-left (relative to facing direction)
            float startAngle = -MathHelper.PiOver4 * 1.5f; // Upper area
            float endAngle = MathHelper.PiOver4 * 1.5f;     // Lower area

            swingAngle = MathHelper.Lerp(startAngle, endAngle, easedProgress);
            float actualAngle = swingAngle * owner.direction;

            // Position the sword relative to owner
            Vector2 offset = actualAngle.ToRotationVector2() * SwingRadius;
            Projectile.Center = owner.Center + offset;
            Projectile.rotation = actualAngle + (owner.direction == 1 ? MathHelper.PiOver4 : MathHelper.Pi - MathHelper.PiOver4);

            // Velocity for dust
            Projectile.velocity = (Projectile.Center - owner.Center).SafeNormalize(Vector2.UnitX) * 2f;

            // Deal damage near midpoint
            if (progress > 0.3f && progress < 0.7f && !phaseDamageDealt[0])
            {
                phaseDamageDealt[0] = true;
                SoundEngine.PlaySound(SoundID.Item1 with { Volume = 0.6f, Pitch = 0.1f }, Projectile.Center);
                SpawnSwingDust(owner.Center, actualAngle, colors, 6);
            }
        }

        private void PhaseUpSwing(Player owner, Color[] colors)
        {
            int frameInPhase = timer - DownSwingFrames;
            float progress = (frameInPhase - 1) / (float)UpSwingFrames;
            float easedProgress = EaseInOut(progress);

            // Swing from lower-left to upper-right (reverse of down swing)
            float startAngle = MathHelper.PiOver4 * 1.5f;
            float endAngle = -MathHelper.PiOver4 * 1.5f;

            swingAngle = MathHelper.Lerp(startAngle, endAngle, easedProgress);
            float actualAngle = swingAngle * owner.direction;

            Vector2 offset = actualAngle.ToRotationVector2() * SwingRadius;
            Projectile.Center = owner.Center + offset;
            Projectile.rotation = actualAngle + (owner.direction == 1 ? MathHelper.PiOver4 : MathHelper.Pi - MathHelper.PiOver4);

            Projectile.velocity = (Projectile.Center - owner.Center).SafeNormalize(Vector2.UnitX) * 2f;

            // Deal damage near midpoint
            if (progress > 0.3f && progress < 0.7f && !phaseDamageDealt[1])
            {
                phaseDamageDealt[1] = true;
                SoundEngine.PlaySound(SoundID.Item1 with { Volume = 0.6f, Pitch = 0.2f }, Projectile.Center);
                SpawnSwingDust(owner.Center, actualAngle, colors, 6);
            }
        }

        private void PhaseSpinThrow(Player owner, Color[] colors)
        {
            int frameInPhase = timer - DownSwingFrames - UpSwingFrames;
            float progress = frameInPhase / (float)SpinThrowFrames;

            if (frameInPhase == 1)
            {
                // Initialize throw toward cursor
                throwStart = owner.Center;
                throwTarget = Main.MouseWorld;
                Vector2 toMouse = (throwTarget - throwStart);
                if (toMouse.Length() > ThrowDistance)
                    throwTarget = throwStart + toMouse.SafeNormalize(Vector2.UnitX) * ThrowDistance;

                SoundEngine.PlaySound(SoundID.Item71 with { Volume = 0.7f, Pitch = 0.3f }, Projectile.Center);
                phaseDamageDealt[2] = false;
            }

            // Spin rapidly
            spinRotation += SpinRate * 2f;

            float easedProgress;
            if (progress < 0.5f)
            {
                // Fly out
                easedProgress = EaseOut(progress * 2f);
                Projectile.Center = Vector2.Lerp(throwStart, throwTarget, easedProgress);
                returning = false;
            }
            else
            {
                // Return
                float returnProgress = (progress - 0.5f) * 2f;
                easedProgress = EaseIn(returnProgress);
                Projectile.Center = Vector2.Lerp(throwTarget, owner.Center, easedProgress);
                returning = true;
            }

            Projectile.rotation = spinRotation;

            // Velocity for trail effect
            Vector2 dir = returning ? (owner.Center - Projectile.Center) : (throwTarget - Projectile.Center);
            Projectile.velocity = dir.SafeNormalize(Vector2.UnitX) * 4f;

            // Trail dust during spin
            if (Main.rand.NextBool(2))
            {
                float dustAngle = spinRotation + Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 dustPos = Projectile.Center + dustAngle.ToRotationVector2() * 15f;
                Dust dust = Dust.NewDustPerfect(dustPos, DustID.RainbowMk2,
                    dustAngle.ToRotationVector2() * 3f,
                    newColor: colors[Main.rand.Next(colors.Length)],
                    Scale: Main.rand.NextFloat(0.3f, 0.6f));
                dust.noGravity = true;
                dust.fadeIn = 0.3f;
            }
        }

        private void SpawnSwingDust(Vector2 center, float angle, Color[] colors, int count)
        {
            for (int i = 0; i < count; i++)
            {
                float dustAngle = angle + Main.rand.NextFloat(-0.5f, 0.5f);
                Vector2 vel = dustAngle.ToRotationVector2() * Main.rand.NextFloat(3f, 7f);
                Vector2 pos = center + dustAngle.ToRotationVector2() * Main.rand.NextFloat(20f, 50f);
                Color col = colors[Main.rand.Next(colors.Length)];
                Dust dust = Dust.NewDustPerfect(pos, DustID.RainbowMk2, vel,
                    newColor: col, Scale: Main.rand.NextFloat(0.4f, 0.7f));
                dust.noGravity = true;
                dust.fadeIn = 0.4f;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Color[] colors = AFTextures.GetModeColors(AttackMode.ComboSwing);

            for (int i = 0; i < 12; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(6f, 6f);
                Color col = colors[Main.rand.Next(colors.Length)];
                Dust dust = Dust.NewDustPerfect(target.Center, DustID.RainbowMk2, vel,
                    newColor: col, Scale: Main.rand.NextFloat(0.4f, 0.8f));
                dust.noGravity = true;
                dust.fadeIn = 0.4f;
            }
        }

        // =====================================================================
        // RENDERING
        // =====================================================================

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            try
            {
            Color[] colors = AFTextures.GetModeColors(AttackMode.ComboSwing);
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            float pulse = 0.9f + 0.1f * MathF.Sin((float)Main.timeForVisualEffects * 0.1f);
            float alpha = MathHelper.Clamp(timer / 5f, 0f, 1f);

            // ---- ADDITIVE BLOOM ----
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

            Texture2D softGlow = AFTextures.SoftGlow.Value;
            Vector2 glowOrigin = softGlow.Size() / 2f;

            // Outer glow (SoftGlow 1024px — target ~60px)
            sb.Draw(softGlow, drawPos, null, colors[0] * (0.3f * alpha * pulse),
                0f, glowOrigin, 0.06f * pulse, SpriteEffects.None, 0f);

            // Core glow (~25px)
            sb.Draw(softGlow, drawPos, null, colors[2] * (0.5f * alpha),
                0f, glowOrigin, 0.025f, SpriteEffects.None, 0f);

            // Phase-specific effects
            if (phase == 2)
            {
                // Spin throw — circular glow ring and velocity line
                Texture2D ringTex = AFTextures.PowerEffectRing.Value;
                Vector2 ringOrigin = ringTex.Size() / 2f;
                float ringScale = 30f / (ringTex.Width / 2f);
                sb.Draw(ringTex, drawPos, null, colors[1] * (0.3f * alpha),
                    spinRotation * 0.3f, ringOrigin, ringScale, SpriteEffects.None, 0f);

                // Velocity streak (~80×8px)
                float velAngle = Projectile.velocity.ToRotation();
                sb.Draw(softGlow, drawPos, null, colors[1] * (0.3f * alpha),
                    velAngle, glowOrigin, new Vector2(0.08f, 0.008f), SpriteEffects.None, 0f);
            }
            else
            {
                // Swing phases — directional arc glow (~60×12px)
                float arcAngle = Projectile.rotation;
                sb.Draw(softGlow, drawPos, null, colors[1] * (0.25f * alpha),
                    arcAngle, glowOrigin, new Vector2(0.06f, 0.012f), SpriteEffects.None, 0f);
            }

            // ---- RESTORE ----
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

            // Draw the sword sprite
            Texture2D swordTex = Terraria.GameContent.TextureAssets.Item[ItemID.Katana].Value;
            Vector2 swordOrigin = swordTex.Size() / 2f;
            sb.Draw(swordTex, drawPos, null, Color.White, Projectile.rotation,
                swordOrigin, 1f, SpriteEffects.None, 0f);

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

        // ---- EASING UTILITIES ----
        private static float EaseInOut(float t)
        {
            return t < 0.5f
                ? 4f * t * t * t
                : 1f - MathF.Pow(-2f * t + 2f, 3f) / 2f;
        }

        private static float EaseOut(float t) => 1f - MathF.Pow(1f - t, 3f);
        private static float EaseIn(float t) => t * t * t;

        public override Color? GetAlpha(Color lightColor) => Color.White;
    }
}
