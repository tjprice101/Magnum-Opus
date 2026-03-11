using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.Shaders;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.ClairDeLune.Weapons.RequiemOfTime.Projectiles
{
    /// <summary>
    /// Reverse Field  Etime-deceleration zone rendered via RadialNoiseMaskShader + TimeFreezeCrack overlay.
    /// 4 render passes: (1) RadialNoiseMask base (darker), (2) TimeFreezeCrack fracture overlay,
    /// (3) Counter-clockwise arrow indicators, (4) Center crimson bloom core.
    /// 40% slow applied to enemies inside the field.
    /// </summary>
    public class ReverseFieldProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

        private const int Duration = 360;
        private const float Radius = 192f;

        // --- Shader + texture caching ---
        private static Effect _radialNoiseShader;
        private static Effect _timeFreezeShader;
        private static Asset<Texture2D> _noiseTex;
        private static Asset<Texture2D> _gradientLUT;
        private static Asset<Texture2D> _softCircle;
        private static Asset<Texture2D> _softRadialBloom;
        private static Asset<Texture2D> _pointBloom;
        private static Asset<Texture2D> _starFlare;

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.timeLeft = Duration;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 60;
        }

        public override bool? CanHitNPC(NPC target)
        {
            return Vector2.Distance(Projectile.Center, target.Center) <= Radius ? null : false;
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            modifiers.FinalDamage *= 0.6f;
        }

        public override void AI()
        {
            Projectile.velocity = Vector2.Zero;

            float life = 1f - (Projectile.timeLeft / (float)Duration);
            float fadeIn = Math.Min(life * 5f, 1f);
            float fadeOut = Projectile.timeLeft < 30 ? Projectile.timeLeft / 30f : 1f;
            float alpha = fadeIn * fadeOut;

            // Slow all enemies in range
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage) continue;
                if (Vector2.Distance(Projectile.Center, npc.Center) <= Radius)
                {
                    npc.velocity *= 0.96f;
                }
            }

            // Reverse-flowing particles (counter-clockwise spiral inward)
            if (Main.rand.NextBool(3))
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float dist = Main.rand.NextFloat(Radius * 0.5f, Radius);
                Vector2 particlePos = Projectile.Center + angle.ToRotationVector2() * dist;

                Vector2 tangent = new Vector2(MathF.Sin(angle), -MathF.Cos(angle));
                Vector2 inward = -angle.ToRotationVector2();
                Vector2 vel = (tangent * 1.2f + inward * 0.8f) * alpha;

                Color color = Color.Lerp(ClairDeLunePalette.NightMist, ClairDeLunePalette.MidnightBlue, dist / Radius);
                var glow = new GenericGlowParticle(particlePos, vel,
                    color with { A = 0 } * 0.35f * alpha, 0.08f, 25);
                MagnumParticleHandler.SpawnParticle(glow);
            }

            // Occasional crimson sparks (HP cost visual echo)
            if (Main.rand.NextBool(12))
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 pos = Projectile.Center + angle.ToRotationVector2() * Main.rand.NextFloat(Radius * 0.5f);
                var crimson = new SparkleParticle(pos, Main.rand.NextVector2Circular(0.5f, 0.5f),
                    ClairDeLunePalette.TemporalCrimson with { A = 0 } * 0.3f * alpha, 0.06f, 18);
                MagnumParticleHandler.SpawnParticle(crimson);
            }

            Lighting.AddLight(Projectile.Center, ClairDeLunePalette.NightMist.ToVector3() * 0.2f * alpha);
        }

        private void LoadTextures()
        {
            _noiseTex ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/NoiseTextures/VornoiEdgeNoise", AssetRequestMode.ImmediateLoad);
            _gradientLUT ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/ColorGradients/ClairDeLuneGradientLUTandRAMP", AssetRequestMode.ImmediateLoad);
            _softCircle ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/MasksAndShapes/SoftCircle", AssetRequestMode.ImmediateLoad);
            _softRadialBloom ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom", AssetRequestMode.ImmediateLoad);
            _pointBloom ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom", AssetRequestMode.ImmediateLoad);
            _starFlare ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/StarFlare", AssetRequestMode.ImmediateLoad);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (Main.dedServ) return false;
            LoadTextures();

            SpriteBatch sb = Main.spriteBatch;
            try
            {
            Matrix matrix = Main.GameViewMatrix.TransformationMatrix;

            float life = 1f - (Projectile.timeLeft / (float)Duration);
            float fadeIn = Math.Min(life * 5f, 1f);
            float fadeOut = Projectile.timeLeft < 30 ? Projectile.timeLeft / 30f : 1f;
            float alpha = fadeIn * fadeOut;

            DrawRadialNoiseField(sb, matrix, alpha);    // Pass 1: Darker radial noise base
            DrawTimeFreezeCrack(sb, matrix, alpha);     // Pass 2: TimeFreezeCrack fracture overlay
            DrawOrbitingArrows(sb, matrix, alpha);       // Pass 3: Counter-clockwise arrows
            DrawCenterBloom(sb, matrix, alpha);          // Pass 4: Crimson-tinged center bloom
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

        // ---- PASS 1: RadialNoiseMaskShader  Edark, ominous deceleration field ----
        private void DrawRadialNoiseField(SpriteBatch sb, Matrix matrix, float alpha)
        {
            _radialNoiseShader ??= ModContent.Request<Effect>(
                "MagnumOpus/Content/FoundationWeapons/MaskFoundation/Shaders/RadialNoiseMaskShader",
                AssetRequestMode.ImmediateLoad).Value;

            if (_radialNoiseShader == null) return;

            sb.End();

            _radialNoiseShader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
            _radialNoiseShader.Parameters["scrollSpeed"]?.SetValue(-0.3f); // Reverse scroll direction
            _radialNoiseShader.Parameters["rotationSpeed"]?.SetValue(-0.25f); // Counter-clockwise
            _radialNoiseShader.Parameters["circleRadius"]?.SetValue(0.45f);
            _radialNoiseShader.Parameters["edgeSoftness"]?.SetValue(0.1f);
            _radialNoiseShader.Parameters["intensity"]?.SetValue(alpha * 0.7f);
            _radialNoiseShader.Parameters["primaryColor"]?.SetValue(ClairDeLunePalette.MidnightBlue.ToVector3());
            _radialNoiseShader.Parameters["coreColor"]?.SetValue(ClairDeLunePalette.NightMist.ToVector3());
            _radialNoiseShader.Parameters["noiseTex"]?.SetValue(_noiseTex.Value);
            _radialNoiseShader.Parameters["gradientTex"]?.SetValue(_gradientLUT.Value);

            _radialNoiseShader.CurrentTechnique = _radialNoiseShader.Techniques["Technique1"];

            sb.Begin(SpriteSortMode.Immediate, MagnumBlendStates.ShaderAdditive,
                SamplerState.LinearWrap, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, _radialNoiseShader, matrix);

            Texture2D sc = _softCircle.Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float fieldDiam = Radius * 2f;
            sb.Draw(sc, drawPos, null, Color.White, 0f, sc.Size() * 0.5f,
                fieldDiam / sc.Width, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                SamplerState.LinearClamp, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null, matrix);
        }

        // ---- PASS 2: TimeFreezeSlash.fx crack overlay via CDL shader ----
        private void DrawTimeFreezeCrack(SpriteBatch sb, Matrix matrix, float alpha)
        {
            _timeFreezeShader ??= ShaderLoader.TimeFreezeSlash;
            if (_timeFreezeShader == null) return;

            sb.End();

            _timeFreezeShader.Parameters["uColor"]?.SetValue(ClairDeLunePalette.NightMist.ToVector4());
            _timeFreezeShader.Parameters["uSecondaryColor"]?.SetValue(ClairDeLunePalette.TemporalCrimson.ToVector4());
            _timeFreezeShader.Parameters["uOpacity"]?.SetValue(alpha * 0.4f);
            _timeFreezeShader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
            _timeFreezeShader.Parameters["uIntensity"]?.SetValue(1.0f);
            _timeFreezeShader.Parameters["uOverbrightMult"]?.SetValue(0.8f);
            _timeFreezeShader.Parameters["uScrollSpeed"]?.SetValue(0.3f);
            _timeFreezeShader.Parameters["uDistortionAmt"]?.SetValue(0.02f);
            _timeFreezeShader.Parameters["uHasSecondaryTex"]?.SetValue(false);

            _timeFreezeShader.CurrentTechnique = _timeFreezeShader.Techniques["TimeFreezeCrack"];

            sb.Begin(SpriteSortMode.Immediate, MagnumBlendStates.ShaderAdditive,
                SamplerState.LinearWrap, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, _timeFreezeShader, matrix);

            Texture2D sc = _softCircle.Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float fieldDiam = Radius * 1.8f; // Slightly smaller than base for layered look
            sb.Draw(sc, drawPos, null, Color.White, 0f, sc.Size() * 0.5f,
                fieldDiam / sc.Width, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                SamplerState.LinearClamp, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null, matrix);
        }

        // ---- PASS 3: Counter-clockwise arrow indicators ----
        private void DrawOrbitingArrows(SpriteBatch sb, Matrix matrix, float alpha)
        {
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Texture2D sf = _starFlare.Value;
            Vector2 sfOrigin = sf.Size() * 0.5f;
            float rotation = -Main.GameUpdateCount * 0.02f; // Counter-clockwise

            for (int i = 0; i < 4; i++)
            {
                float arrowAngle = rotation + i * MathHelper.PiOver2;
                float arrowDist = Radius * 0.55f;
                Vector2 arrowPos = drawPos + arrowAngle.ToRotationVector2() * arrowDist;
                float arrowRot = arrowAngle - MathHelper.PiOver2; // Tangent counter-clockwise

                sb.Draw(sf, arrowPos, null, ClairDeLunePalette.NightMist with { A = 0 } * 0.2f * alpha,
                    arrowRot, sfOrigin, new Vector2(18f / sf.Width, 8f / sf.Height), SpriteEffects.None, 0f);
            }
        }

        // ---- PASS 4: Crimson-tinged center bloom ----
        private void DrawCenterBloom(SpriteBatch sb, Matrix matrix, float alpha)
        {
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float pulse = 0.85f + 0.15f * MathF.Sin(Main.GameUpdateCount * 0.05f);

            Texture2D srb = _softRadialBloom.Value;
            Texture2D pb = _pointBloom.Value;

            // Ambient dark halo
            sb.Draw(srb, drawPos, null, ClairDeLunePalette.MidnightBlue with { A = 0 } * 0.1f * alpha * pulse,
                0f, srb.Size() * 0.5f, 40f / srb.Width, SpriteEffects.None, 0f);
            // Purple mid
            sb.Draw(srb, drawPos, null, ClairDeLunePalette.NightMist with { A = 0 } * 0.18f * alpha,
                0f, srb.Size() * 0.5f, 22f / srb.Width, SpriteEffects.None, 0f);
            // Crimson tinge
            sb.Draw(pb, drawPos, null, ClairDeLunePalette.TemporalCrimson with { A = 0 } * 0.15f * alpha,
                0f, pb.Size() * 0.5f, 14f / pb.Width, SpriteEffects.None, 0f);
            // Core
            sb.Draw(pb, drawPos, null, ClairDeLunePalette.NightMist with { A = 0 } * 0.25f * alpha,
                0f, pb.Size() * 0.5f, 8f / pb.Width, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                SamplerState.PointClamp, DepthStencilState.None,
                RasterizerState.CullNone, null, matrix);
        }
    }
}
