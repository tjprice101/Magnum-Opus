using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.Shaders;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.ClairDeLune.Weapons.Chronologicality.Projectiles
{
    /// <summary>
    /// Temporal Echo — Ghost replay of the player's previous melee swing,
    /// dealing 30% damage after a 0.5s delay. Plays as a crystalline time-ghost.
    /// 3 render passes: (1) TimeFreezeSlash TimeFreezeCrack crystalline ghost body,
    /// (2) ClairDeLuneMoonlit MoonlitGlow ambient aura, (3) Multi-scale bloom with expanding rings.
    /// </summary>
    public class TemporalEchoProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

        private int _timer;
        private const int DelayFrames = 30; // 0.5s delay
        private const int SwingDuration = 20;
        private bool _hasDealt;

        // ai[0] = swing direction (+1 or -1), ai[1] = swing arc angle
        private float SwingDirection => Projectile.ai[0];
        private float SwingArc => Projectile.ai[1] == 0f ? MathHelper.PiOver2 : Projectile.ai[1];

        // --- Shader + texture caching ---
        private static Effect _timeFreezeShader;
        private static Effect _moonlitShader;
        private static Asset<Texture2D> _softCircle;
        private static Asset<Texture2D> _softRadialBloom;
        private static Asset<Texture2D> _pointBloom;

        public override void SetDefaults()
        {
            Projectile.width = 60;
            Projectile.height = 60;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = DelayFrames + SwingDuration + 10;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = DelayFrames + SwingDuration;
        }

        public override void AI()
        {
            _timer++;

            // During delay phase, the echo is ghostly and still forming
            // During swing phase, it replays and deals damage
            if (_timer > DelayFrames + SwingDuration)
            {
                Projectile.Kill();
                return;
            }

            Projectile.velocity = Vector2.Zero;

            // Slowly advance the swing rotation during active phase
            if (_timer > DelayFrames)
            {
                float swingProgress = (_timer - DelayFrames) / (float)SwingDuration;
                float swingAngle = MathHelper.Lerp(-SwingArc * 0.5f, SwingArc * 0.5f, swingProgress) * SwingDirection;
                Projectile.rotation = Projectile.ai[1] + swingAngle; // Use initial angle + sweep
            }

            // Ghost shimmer particles during delay
            if (_timer <= DelayFrames && Main.GameUpdateCount % 3 == 0)
            {
                var shimmer = new SparkleParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(20f, 20f),
                    Main.rand.NextVector2Circular(0.5f, 0.5f),
                    ClairDeLunePalette.PearlFrost with { A = 0 } * 0.3f, 0.06f, 8);
                MagnumParticleHandler.SpawnParticle(shimmer);
            }

            float intensity = _timer <= DelayFrames ? 0.15f : 0.3f;
            Lighting.AddLight(Projectile.Center, ClairDeLunePalette.SoftBlue.ToVector3() * intensity);
        }

        public override bool? CanHitNPC(NPC target)
        {
            // Only deal damage during swing phase, once
            if (_timer <= DelayFrames || _hasDealt)
                return false;
            return null;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            _hasDealt = true;
        }

        private void LoadTextures()
        {
            _softCircle ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/MasksAndShapes/SoftCircle", AssetRequestMode.ImmediateLoad);
            _softRadialBloom ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom", AssetRequestMode.ImmediateLoad);
            _pointBloom ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom", AssetRequestMode.ImmediateLoad);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (Main.dedServ) return false;
            LoadTextures();

            SpriteBatch sb = Main.spriteBatch;
            Matrix matrix = Main.GameViewMatrix.TransformationMatrix;

            // Compute ghost phase
            float ghostAlpha;
            if (_timer <= DelayFrames)
                ghostAlpha = 0.15f + 0.15f * MathF.Sin(_timer * 0.2f); // Forming, pulsing faintly
            else
            {
                float swingProgress = (_timer - DelayFrames) / (float)SwingDuration;
                ghostAlpha = 0.5f * (1f - swingProgress); // Fades as swing completes
            }

            DrawTimeFreezeGhost(sb, matrix, ghostAlpha);   // Pass 1: TimeFreezeCrack crystalline ghost
            DrawMoonlitAura(sb, matrix, ghostAlpha);       // Pass 2: MoonlitGlow ambient aura
            DrawBloomRings(sb, matrix, ghostAlpha);        // Pass 3: Multi-scale bloom + rings
            return false;
        }

        // ---- PASS 1: TimeFreezeSlash TimeFreezeCrack crystalline ghost ----
        private void DrawTimeFreezeGhost(SpriteBatch sb, Matrix matrix, float ghostAlpha)
        {
            _timeFreezeShader ??= ShaderLoader.TimeFreezeSlash;
            if (_timeFreezeShader == null) return;

            bool isSwinging = _timer > DelayFrames;
            float crackIntensity = isSwinging ? 1.5f : 0.6f;

            sb.End();

            _timeFreezeShader.Parameters["uColor"]?.SetValue(
                (ClairDeLunePalette.PearlFrost with { A = 200 }).ToVector4());
            _timeFreezeShader.Parameters["uSecondaryColor"]?.SetValue(ClairDeLunePalette.SoftBlue.ToVector4());
            _timeFreezeShader.Parameters["uOpacity"]?.SetValue(ghostAlpha);
            _timeFreezeShader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
            _timeFreezeShader.Parameters["uIntensity"]?.SetValue(crackIntensity);
            _timeFreezeShader.Parameters["uOverbrightMult"]?.SetValue(1.0f);
            _timeFreezeShader.Parameters["uScrollSpeed"]?.SetValue(isSwinging ? 3f : 1f);
            _timeFreezeShader.Parameters["uDistortionAmt"]?.SetValue(0.03f);
            _timeFreezeShader.Parameters["uHasSecondaryTex"]?.SetValue(false);

            _timeFreezeShader.CurrentTechnique = _timeFreezeShader.Techniques["TimeFreezeCrack"];

            sb.Begin(SpriteSortMode.Immediate, BlendState.Additive,
                SamplerState.LinearWrap, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, _timeFreezeShader, matrix);

            Texture2D sc = _softCircle.Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float bodyScale = 50f / sc.Width;
            sb.Draw(sc, drawPos, null, Color.White, Projectile.rotation, sc.Size() * 0.5f, bodyScale, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                SamplerState.LinearClamp, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null, matrix);
        }

        // ---- PASS 2: ClairDeLuneMoonlit MoonlitGlow ambient aura ----
        private void DrawMoonlitAura(SpriteBatch sb, Matrix matrix, float ghostAlpha)
        {
            _moonlitShader ??= ShaderLoader.ClairDeLuneMoonlit;
            if (_moonlitShader == null) return;

            sb.End();

            _moonlitShader.Parameters["uColor"]?.SetValue(ClairDeLunePalette.NightMist.ToVector4());
            _moonlitShader.Parameters["uSecondaryColor"]?.SetValue(ClairDeLunePalette.MidnightBlue.ToVector4());
            _moonlitShader.Parameters["uOpacity"]?.SetValue(ghostAlpha * 0.6f);
            _moonlitShader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
            _moonlitShader.Parameters["uIntensity"]?.SetValue(0.8f);
            _moonlitShader.Parameters["uOverbrightMult"]?.SetValue(1.0f);
            _moonlitShader.Parameters["uScrollSpeed"]?.SetValue(0.8f);
            _moonlitShader.Parameters["uDistortionAmt"]?.SetValue(0.02f);
            _moonlitShader.Parameters["uHasSecondaryTex"]?.SetValue(false);

            _moonlitShader.CurrentTechnique = _moonlitShader.Techniques["MoonlitGlow"];

            sb.Begin(SpriteSortMode.Immediate, BlendState.Additive,
                SamplerState.LinearWrap, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, _moonlitShader, matrix);

            Texture2D sc = _softCircle.Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float auraScale = 65f / sc.Width;
            sb.Draw(sc, drawPos, null, Color.White, 0f, sc.Size() * 0.5f, auraScale, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                SamplerState.LinearClamp, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null, matrix);
        }

        // ---- PASS 3: Multi-scale bloom + expanding time rings ----
        private void DrawBloomRings(SpriteBatch sb, Matrix matrix, float ghostAlpha)
        {
            Vector2 pos = Projectile.Center - Main.screenPosition;
            Texture2D srb = _softRadialBloom.Value;
            Texture2D pb = _pointBloom.Value;

            bool isSwinging = _timer > DelayFrames;

            // Outer ghost haze
            sb.Draw(srb, pos, null,
                ClairDeLunePalette.NightMist with { A = 0 } * ghostAlpha * 0.3f, 0f, srb.Size() * 0.5f,
                50f / srb.Width, SpriteEffects.None, 0f);

            // Mid frost glow
            sb.Draw(srb, pos, null,
                ClairDeLunePalette.PearlFrost with { A = 0 } * ghostAlpha * 0.25f, 0f, srb.Size() * 0.5f,
                30f / srb.Width, SpriteEffects.None, 0f);

            // Center core
            sb.Draw(pb, pos, null,
                ClairDeLunePalette.SoftBlue with { A = 0 } * ghostAlpha * 0.3f, 0f, pb.Size() * 0.5f,
                12f / pb.Width, SpriteEffects.None, 0f);

            // Time-crack expanding rings during swing phase
            if (isSwinging)
            {
                float swingProgress = (_timer - DelayFrames) / (float)SwingDuration;
                int ringCount = 3;
                for (int r = 0; r < ringCount; r++)
                {
                    float ringPhase = (swingProgress + r * 0.15f) % 1f;
                    float ringRadius = 20f + ringPhase * 30f;
                    float ringAlpha = (1f - ringPhase) * 0.15f;

                    int ringPts = 8;
                    for (int p = 0; p < ringPts; p++)
                    {
                        float angle = MathHelper.TwoPi * p / ringPts + r * 0.3f;
                        Vector2 ringPos = pos + angle.ToRotationVector2() * ringRadius;
                        sb.Draw(pb, ringPos, null,
                            ClairDeLunePalette.PearlFrost with { A = 0 } * ringAlpha, 0f, pb.Size() * 0.5f,
                            3f / pb.Width, SpriteEffects.None, 0f);
                    }
                }
            }

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null, matrix);
        }
    }
}
