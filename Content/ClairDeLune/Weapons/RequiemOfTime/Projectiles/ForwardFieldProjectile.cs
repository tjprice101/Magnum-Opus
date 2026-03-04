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
    /// Forward Field — time-acceleration zone rendered via RadialNoiseMaskShader + ResonanceField overlay.
    /// 4 render passes: (1) RadialNoiseMask base, (2) ResonanceFieldHarmonic ring overlay,
    /// (3) Orbiting arrow indicators, (4) Center bloom core.
    /// 30% movement speed boost to allies inside the field.
    /// </summary>
    public class ForwardFieldProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

        private const int Duration = 360;
        private const float Radius = 192f;

        // --- Shader + texture caching ---
        private static Effect _radialNoiseShader;
        private static Effect _resonanceShader;
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
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.timeLeft = Duration;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            Projectile.velocity = Vector2.Zero;

            float life = 1f - (Projectile.timeLeft / (float)Duration);
            float fadeIn = Math.Min(life * 5f, 1f);
            float fadeOut = Projectile.timeLeft < 30 ? Projectile.timeLeft / 30f : 1f;
            float alpha = fadeIn * fadeOut;

            // Speed buff all players in range
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player player = Main.player[i];
                if (!player.active || player.dead) continue;
                if (Vector2.Distance(Projectile.Center, player.Center) <= Radius)
                {
                    player.moveSpeed += 0.3f;
                    player.maxRunSpeed *= 1.3f;
                }
            }

            // Check overlap with Reverse Fields for Temporal Paradox
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile other = Main.projectile[i];
                if (!other.active || other.owner != Projectile.owner) continue;
                if (other.type != ModContent.ProjectileType<ReverseFieldProjectile>()) continue;

                float dist = Vector2.Distance(Projectile.Center, other.Center);
                if (dist < Radius + 192f)
                {
                    Vector2 paradoxPos = (Projectile.Center + other.Center) * 0.5f;

                    bool paradoxExists = false;
                    for (int j = 0; j < Main.maxProjectiles; j++)
                    {
                        if (Main.projectile[j].active &&
                            Main.projectile[j].type == ModContent.ProjectileType<TemporalParadoxProjectile>() &&
                            Vector2.Distance(Main.projectile[j].Center, paradoxPos) < 64f)
                        {
                            paradoxExists = true;
                            break;
                        }
                    }

                    if (!paradoxExists)
                    {
                        int dmg = (int)(Projectile.damage * 2.5f);
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), paradoxPos, Vector2.Zero,
                            ModContent.ProjectileType<TemporalParadoxProjectile>(),
                            dmg, 8f, Projectile.owner);
                        Projectile.Kill();
                        other.Kill();
                        return;
                    }
                }
            }

            // Forward-flowing particles (clockwise spiral outward)
            if (Main.rand.NextBool(3))
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float dist = Main.rand.NextFloat(Radius * 0.3f, Radius * 0.9f);
                Vector2 particlePos = Projectile.Center + angle.ToRotationVector2() * dist;

                Vector2 tangent = new Vector2(-MathF.Sin(angle), MathF.Cos(angle));
                Vector2 outward = angle.ToRotationVector2();
                Vector2 vel = (tangent * 1.5f + outward * 0.5f) * alpha;

                Color color = Color.Lerp(ClairDeLunePalette.PearlBlue, ClairDeLunePalette.SoftBlue, dist / Radius);
                var glow = new GenericGlowParticle(particlePos, vel,
                    color with { A = 0 } * 0.3f * alpha, 0.08f, 25);
                MagnumParticleHandler.SpawnParticle(glow);
            }

            Lighting.AddLight(Projectile.Center, ClairDeLunePalette.SoftBlue.ToVector3() * 0.25f * alpha);
        }

        private void LoadTextures()
        {
            _noiseTex ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/NoiseTextures/PerlinNoise", AssetRequestMode.ImmediateLoad);
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
            Matrix matrix = Main.GameViewMatrix.TransformationMatrix;

            float life = 1f - (Projectile.timeLeft / (float)Duration);
            float fadeIn = Math.Min(life * 5f, 1f);
            float fadeOut = Projectile.timeLeft < 30 ? Projectile.timeLeft / 30f : 1f;
            float alpha = fadeIn * fadeOut;

            DrawRadialNoiseField(sb, matrix, alpha);       // Pass 1: RadialNoiseMask base
            DrawResonanceHarmonicRings(sb, matrix, alpha); // Pass 2: ResonanceField overlay
            DrawOrbitingArrows(sb, matrix, alpha);          // Pass 3: Clockwise arrow indicators
            DrawCenterBloom(sb, matrix, alpha);             // Pass 4: Center bloom core
            return false;
        }

        // ---- PASS 1: RadialNoiseMaskShader base field (MaskFoundation pattern) ----
        private void DrawRadialNoiseField(SpriteBatch sb, Matrix matrix, float alpha)
        {
            _radialNoiseShader ??= ModContent.Request<Effect>(
                "MagnumOpus/Content/FoundationWeapons/MaskFoundation/Shaders/RadialNoiseMaskShader",
                AssetRequestMode.ImmediateLoad).Value;

            if (_radialNoiseShader == null) return;

            sb.End();

            // Configure shader
            _radialNoiseShader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
            _radialNoiseShader.Parameters["scrollSpeed"]?.SetValue(0.4f);
            _radialNoiseShader.Parameters["rotationSpeed"]?.SetValue(0.3f);
            _radialNoiseShader.Parameters["circleRadius"]?.SetValue(0.45f);
            _radialNoiseShader.Parameters["edgeSoftness"]?.SetValue(0.12f);
            _radialNoiseShader.Parameters["intensity"]?.SetValue(alpha * 0.8f);
            _radialNoiseShader.Parameters["primaryColor"]?.SetValue(ClairDeLunePalette.SoftBlue.ToVector3());
            _radialNoiseShader.Parameters["coreColor"]?.SetValue(ClairDeLunePalette.PearlWhite.ToVector3());
            _radialNoiseShader.Parameters["noiseTex"]?.SetValue(_noiseTex.Value);
            _radialNoiseShader.Parameters["gradientTex"]?.SetValue(_gradientLUT.Value);

            _radialNoiseShader.CurrentTechnique = _radialNoiseShader.Techniques["Technique1"];

            sb.Begin(SpriteSortMode.Immediate, BlendState.Additive,
                SamplerState.LinearWrap, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, _radialNoiseShader, matrix);

            // Draw SoftCircle quad scaled to field radius
            Texture2D sc = _softCircle.Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float fieldDiam = Radius * 2f;
            sb.Draw(sc, drawPos, null, Color.White, 0f, sc.Size() * 0.5f,
                fieldDiam / sc.Width, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                SamplerState.LinearClamp, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null, matrix);
        }

        // ---- PASS 2: ResonanceField harmonic ring overlay via CDL shader ----
        private void DrawResonanceHarmonicRings(SpriteBatch sb, Matrix matrix, float alpha)
        {
            _resonanceShader ??= ShaderLoader.ResonanceField;
            if (_resonanceShader == null) return;

            sb.End();

            _resonanceShader.Parameters["uColor"]?.SetValue(ClairDeLunePalette.PearlBlue.ToVector4());
            _resonanceShader.Parameters["uSecondaryColor"]?.SetValue(ClairDeLunePalette.MoonbeamGold.ToVector4());
            _resonanceShader.Parameters["uOpacity"]?.SetValue(alpha * 0.5f);
            _resonanceShader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
            _resonanceShader.Parameters["uIntensity"]?.SetValue(1.2f);
            _resonanceShader.Parameters["uOverbrightMult"]?.SetValue(1.0f);
            _resonanceShader.Parameters["uScrollSpeed"]?.SetValue(1.5f);
            _resonanceShader.Parameters["uDistortionAmt"]?.SetValue(0.01f);
            _resonanceShader.Parameters["uHasSecondaryTex"]?.SetValue(false);

            _resonanceShader.CurrentTechnique = _resonanceShader.Techniques["ResonanceFieldHarmonic"];

            sb.Begin(SpriteSortMode.Immediate, BlendState.Additive,
                SamplerState.LinearWrap, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, _resonanceShader, matrix);

            Texture2D sc = _softCircle.Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float fieldDiam = Radius * 2f;
            sb.Draw(sc, drawPos, null, Color.White, 0f, sc.Size() * 0.5f,
                fieldDiam / sc.Width, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                SamplerState.LinearClamp, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null, matrix);
        }

        // ---- PASS 3: Orbiting clockwise arrow indicators ----
        private void DrawOrbitingArrows(SpriteBatch sb, Matrix matrix, float alpha)
        {
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Texture2D sf = _starFlare.Value;
            Vector2 sfOrigin = sf.Size() * 0.5f;
            float rotation = Main.GameUpdateCount * 0.02f;

            for (int i = 0; i < 4; i++)
            {
                float arrowAngle = rotation + i * MathHelper.PiOver2;
                float arrowDist = Radius * 0.55f;
                Vector2 arrowPos = drawPos + arrowAngle.ToRotationVector2() * arrowDist;
                float arrowRot = arrowAngle + MathHelper.PiOver2; // Tangent direction (clockwise)

                // Stretched star as arrow indicator
                sb.Draw(sf, arrowPos, null, ClairDeLunePalette.PearlWhite with { A = 0 } * 0.2f * alpha,
                    arrowRot, sfOrigin, new Vector2(18f / sf.Width, 8f / sf.Height), SpriteEffects.None, 0f);
            }
        }

        // ---- PASS 4: Center bloom core ----
        private void DrawCenterBloom(SpriteBatch sb, Matrix matrix, float alpha)
        {
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float pulse = 0.85f + 0.15f * MathF.Sin(Main.GameUpdateCount * 0.06f);

            Texture2D srb = _softRadialBloom.Value;
            Texture2D pb = _pointBloom.Value;

            // Ambient halo
            sb.Draw(srb, drawPos, null, ClairDeLunePalette.SoftBlue with { A = 0 } * 0.12f * alpha * pulse,
                0f, srb.Size() * 0.5f, 40f / srb.Width, SpriteEffects.None, 0f);
            // Mid glow
            sb.Draw(srb, drawPos, null, ClairDeLunePalette.PearlBlue with { A = 0 } * 0.2f * alpha,
                0f, srb.Size() * 0.5f, 22f / srb.Width, SpriteEffects.None, 0f);
            // Core
            sb.Draw(pb, drawPos, null, ClairDeLunePalette.PearlWhite with { A = 0 } * 0.35f * alpha,
                0f, pb.Size() * 0.5f, 10f / pb.Width, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                SamplerState.PointClamp, DepthStencilState.None,
                RasterizerState.CullNone, null, matrix);
        }
    }
}
