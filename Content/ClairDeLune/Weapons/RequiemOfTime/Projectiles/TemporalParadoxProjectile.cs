using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.Shaders;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.ClairDeLune.Weapons.RequiemOfTime.Projectiles
{
    /// <summary>
    /// Temporal Paradox — created when Forward and Reverse fields overlap.
    /// 3-phase expanding detonation dealing heavy damage.
    /// 3 render passes: (1) RadialNoiseMaskShader expanding zone,
    /// (2) TimeFreezeSlash.fx TimeFreezeCrack overlay, (3) Multi-scale bloom rings.
    /// </summary>
    public class TemporalParadoxProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

        private int _phase;
        private int _phaseTimer;
        private const int PhaseFrames = 12;
        private const float Phase1Radius = 64f;
        private const float Phase2Radius = 128f;
        private const float Phase3Radius = 224f;
        private bool _dealtDamage;

        // --- Shader + texture caching ---
        private static Effect _radialMaskShader;
        private static Effect _timeFreezeShader;
        private static Asset<Texture2D> _noiseTex;
        private static Asset<Texture2D> _gradientLUT;
        private static Asset<Texture2D> _softCircle;
        private static Asset<Texture2D> _softRadialBloom;
        private static Asset<Texture2D> _starFlare;

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.timeLeft = PhaseFrames * 3 + 30;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
            Projectile.alpha = 0;
        }

        public override void AI()
        {
            Projectile.velocity = Vector2.Zero;
            _phaseTimer++;

            if (_phaseTimer == 1)
            {
                _phase = 1;
                SoundEngine.PlaySound(SoundID.Item14 with { Pitch = 0.6f, Volume = 0.7f }, Projectile.Center);
                SpawnCrackParticles(Phase1Radius, ClairDeLunePalette.PearlWhite, 8);
                DealAreaDamage(Phase1Radius, 1f);
            }
            else if (_phaseTimer == PhaseFrames + 1)
            {
                _phase = 2;
                SoundEngine.PlaySound(SoundID.Item14 with { Pitch = 0.2f, Volume = 0.85f }, Projectile.Center);
                SpawnCrackParticles(Phase2Radius, ClairDeLunePalette.SoftBlue, 12);
                DealAreaDamage(Phase2Radius, 1.5f);
            }
            else if (_phaseTimer == PhaseFrames * 2 + 1)
            {
                _phase = 3;
                SoundEngine.PlaySound(SoundID.Item14 with { Pitch = -0.3f, Volume = 1f }, Projectile.Center);
                SpawnShatterParticles();
                DealAreaDamage(Phase3Radius, 2f);

                Player owner = Main.player[Projectile.owner];
                if (owner.active)
                {
                    for (int i = 0; i < 6; i++)
                    {
                        var shake = new BloomParticle(
                            owner.Center + Main.rand.NextVector2Circular(3, 3),
                            Vector2.Zero,
                            ClairDeLunePalette.WhiteHot with { A = 0 } * 0.1f,
                            0.05f, 3);
                        MagnumParticleHandler.SpawnParticle(shake);
                    }
                }
            }

            float intensity = _phase switch
            {
                1 => 0.3f,
                2 => 0.5f,
                3 => 0.7f,
                _ => 0.1f
            };
            Lighting.AddLight(Projectile.Center, ClairDeLunePalette.PearlWhite.ToVector3() * intensity);
        }

        private void DealAreaDamage(float radius, float multiplier)
        {
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage) continue;
                if (Vector2.Distance(Projectile.Center, npc.Center) <= radius)
                {
                    int dmg = (int)(Projectile.damage * multiplier);
                    Player owner = Main.player[Projectile.owner];
                    owner.ApplyDamageToNPC(npc, dmg, Projectile.knockBack * multiplier, 0, false);
                }
            }
        }

        private void SpawnCrackParticles(float radius, Color color, int count)
        {
            for (int i = 0; i < count; i++)
            {
                float angle = MathHelper.TwoPi * i / count + Main.rand.NextFloat(-0.15f, 0.15f);
                float dist = radius * Main.rand.NextFloat(0.2f, 1f);
                Vector2 vel = angle.ToRotationVector2() * (dist * 0.03f);

                var crack = new GlowSparkParticle(
                    Projectile.Center + angle.ToRotationVector2() * dist * 0.3f,
                    vel, color with { A = 0 } * 0.6f,
                    0.06f + dist * 0.0003f, 20);
                MagnumParticleHandler.SpawnParticle(crack);
            }

            var ring = new BloomParticle(Projectile.Center, Vector2.Zero,
                color with { A = 0 } * 0.4f, radius / 200f, 15);
            MagnumParticleHandler.SpawnParticle(ring);
        }

        private void SpawnShatterParticles()
        {
            for (int i = 0; i < 24; i++)
            {
                float angle = MathHelper.TwoPi * i / 24f;
                float speed = Main.rand.NextFloat(3f, 7f);
                Vector2 vel = angle.ToRotationVector2() * speed;

                Color color = Main.rand.NextBool()
                    ? ClairDeLunePalette.PearlWhite
                    : ClairDeLunePalette.SoftBlue;

                var fragment = new SparkleParticle(
                    Projectile.Center + angle.ToRotationVector2() * Main.rand.NextFloat(5, 20),
                    vel, color with { A = 0 } * 0.7f,
                    0.1f + Main.rand.NextFloat(0.05f), 25);
                MagnumParticleHandler.SpawnParticle(fragment);
            }

            var blueFlash = new BloomParticle(Projectile.Center, Vector2.Zero,
                ClairDeLunePalette.SoftBlue with { A = 0 } * 0.5f, 0.8f, 15);
            MagnumParticleHandler.SpawnParticle(blueFlash);

            var purpleFlash = new BloomParticle(Projectile.Center, Vector2.Zero,
                ClairDeLunePalette.NightMist with { A = 0 } * 0.4f, 1f, 18);
            MagnumParticleHandler.SpawnParticle(purpleFlash);

            var coreFlash = new BloomParticle(Projectile.Center, Vector2.Zero,
                ClairDeLunePalette.WhiteHot with { A = 0 } * 0.7f, 0.4f, 10);
            MagnumParticleHandler.SpawnParticle(coreFlash);

            for (int i = 0; i < 6; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(4f, 4f);
                var crimson = new GlowSparkParticle(
                    Projectile.Center, vel,
                    ClairDeLunePalette.TemporalCrimson with { A = 0 } * 0.4f,
                    0.08f, 18);
                MagnumParticleHandler.SpawnParticle(crimson);
            }
        }

        private void LoadTextures()
        {
            _noiseTex ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/NoiseAndDistortion/VoronoiCrackNoise", AssetRequestMode.ImmediateLoad);
            _gradientLUT ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/ColorGradients/ClairDeLuneGradientLUTandRAMP", AssetRequestMode.ImmediateLoad);
            _softCircle ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/MasksAndShapes/SoftCircle", AssetRequestMode.ImmediateLoad);
            _softRadialBloom ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom", AssetRequestMode.ImmediateLoad);
            _starFlare ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/StarFlare", AssetRequestMode.ImmediateLoad);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (Main.dedServ) return false;
            LoadTextures();

            SpriteBatch sb = Main.spriteBatch;
            Matrix matrix = Main.GameViewMatrix.TransformationMatrix;

            DrawRadialMaskZone(sb, matrix);      // Pass 1: RadialNoiseMask expanding zone
            DrawTimeFreezeOverlay(sb, matrix);   // Pass 2: TimeFreezeCrack shader overlay
            DrawBloomRings(sb, matrix);          // Pass 3: Multi-scale bloom rings + core
            ClairDeLuneVFXLibrary.DrawThemeAccents(sb, Projectile.Center, 0.5f, 0.3f);
            return false;
        }

        // ---- PASS 1: RadialNoiseMaskShader expanding fracture zone ----
        private void DrawRadialMaskZone(SpriteBatch sb, Matrix matrix)
        {
            _radialMaskShader ??= ModContent.Request<Effect>(
                "MagnumOpus/Content/FoundationWeapons/MaskFoundation/Shaders/RadialNoiseMaskShader",
                AssetRequestMode.ImmediateLoad).Value;
            if (_radialMaskShader == null) return;

            float currentRadius = _phase switch
            {
                1 => Phase1Radius * Math.Min(_phaseTimer / (float)PhaseFrames, 1f),
                2 => Phase2Radius * Math.Min((_phaseTimer - PhaseFrames) / (float)PhaseFrames, 1f),
                3 => Phase3Radius * Math.Min((_phaseTimer - PhaseFrames * 2) / (float)PhaseFrames, 1f),
                _ => 0f
            };
            if (currentRadius <= 0f) return;

            float fadeOut = Projectile.timeLeft < 20 ? Projectile.timeLeft / 20f : 1f;

            sb.End();

            _radialMaskShader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
            _radialMaskShader.Parameters["scrollSpeed"]?.SetValue(0.3f);
            _radialMaskShader.Parameters["rotationSpeed"]?.SetValue(-0.8f); // counter-clockwise fracture
            _radialMaskShader.Parameters["circleRadius"]?.SetValue(0.45f);
            _radialMaskShader.Parameters["edgeSoftness"]?.SetValue(0.12f);
            _radialMaskShader.Parameters["intensity"]?.SetValue(1.2f * fadeOut);
            _radialMaskShader.Parameters["primaryColor"]?.SetValue(ClairDeLunePalette.SoftBlue.ToVector3());
            _radialMaskShader.Parameters["coreColor"]?.SetValue(ClairDeLunePalette.PearlWhite.ToVector3());
            _radialMaskShader.Parameters["noiseTex"]?.SetValue(_noiseTex.Value);
            _radialMaskShader.Parameters["gradientTex"]?.SetValue(_gradientLUT.Value);

            _radialMaskShader.CurrentTechnique.Passes["RadialNoiseMaskPass"].Apply();

            sb.Begin(SpriteSortMode.Immediate, BlendState.Additive,
                SamplerState.LinearWrap, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, _radialMaskShader, matrix);

            Texture2D sc = _softCircle.Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float zoneScale = currentRadius * 2f / sc.Width;
            sb.Draw(sc, drawPos, null, Color.White, 0f, sc.Size() * 0.5f, zoneScale, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                SamplerState.LinearClamp, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null, matrix);
        }

        // ---- PASS 2: TimeFreezeSlash.fx TimeFreezeCrack overlay ----
        private void DrawTimeFreezeOverlay(SpriteBatch sb, Matrix matrix)
        {
            if (_phase < 2) return; // Only phase 2+ gets the crack overlay

            _timeFreezeShader ??= ShaderLoader.TimeFreezeSlash;
            if (_timeFreezeShader == null) return;

            float fadeOut = Projectile.timeLeft < 20 ? Projectile.timeLeft / 20f : 1f;
            float currentRadius = _phase >= 3 ? Phase3Radius : Phase2Radius;
            float phaseProgress = _phase >= 3
                ? Math.Min((_phaseTimer - PhaseFrames * 2) / (float)PhaseFrames, 1f)
                : Math.Min((_phaseTimer - PhaseFrames) / (float)PhaseFrames, 1f);

            sb.End();

            _timeFreezeShader.Parameters["uColor"]?.SetValue(ClairDeLunePalette.NightMist.ToVector4());
            _timeFreezeShader.Parameters["uSecondaryColor"]?.SetValue(ClairDeLunePalette.TemporalCrimson.ToVector4());
            _timeFreezeShader.Parameters["uOpacity"]?.SetValue(0.5f * fadeOut * phaseProgress);
            _timeFreezeShader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
            _timeFreezeShader.Parameters["uIntensity"]?.SetValue(1.5f + _phase * 0.3f);
            _timeFreezeShader.Parameters["uOverbrightMult"]?.SetValue(1.2f);
            _timeFreezeShader.Parameters["uScrollSpeed"]?.SetValue(1f);
            _timeFreezeShader.Parameters["uDistortionAmt"]?.SetValue(0.02f * _phase);
            _timeFreezeShader.Parameters["uHasSecondaryTex"]?.SetValue(false);

            _timeFreezeShader.CurrentTechnique = _timeFreezeShader.Techniques["TimeFreezeCrack"];

            sb.Begin(SpriteSortMode.Immediate, BlendState.Additive,
                SamplerState.LinearWrap, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, _timeFreezeShader, matrix);

            Texture2D sc = _softCircle.Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float overlayScale = currentRadius * 2.2f / sc.Width;
            sb.Draw(sc, drawPos, null, Color.White, 0f, sc.Size() * 0.5f, overlayScale, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                SamplerState.LinearClamp, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null, matrix);
        }

        // ---- PASS 3: Multi-scale bloom phase rings + flickering core ----
        private void DrawBloomRings(SpriteBatch sb, Matrix matrix)
        {
            Texture2D srb = _softRadialBloom.Value;
            Texture2D sf = _starFlare.Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            float fadeOut = Projectile.timeLeft < 20 ? Projectile.timeLeft / 20f : 1f;
            float phaseProgress = (_phaseTimer % PhaseFrames) / (float)PhaseFrames;

            // Phase ring blooms (expanding per-phase)
            if (_phase >= 1)
            {
                float r1 = _phase > 1 ? Phase1Radius : Phase1Radius * phaseProgress;
                float s1 = r1 * 2f / srb.Width;
                float a1 = _phase > 1 ? 0.3f : (1f - phaseProgress) * 0.5f;
                sb.Draw(srb, drawPos, null, ClairDeLunePalette.PearlWhite with { A = 0 } * a1 * fadeOut,
                    0f, srb.Size() * 0.5f, s1, SpriteEffects.None, 0f);
            }

            if (_phase >= 2)
            {
                float r2 = _phase > 2 ? Phase2Radius : Phase2Radius * phaseProgress;
                float s2 = r2 * 2f / srb.Width;
                float a2 = _phase > 2 ? 0.25f : (1f - phaseProgress) * 0.4f;
                sb.Draw(srb, drawPos, null, ClairDeLunePalette.SoftBlue with { A = 0 } * a2 * fadeOut,
                    0f, srb.Size() * 0.5f, s2, SpriteEffects.None, 0f);
            }

            if (_phase >= 3)
            {
                float r3 = Phase3Radius * phaseProgress;
                float s3 = r3 * 2f / srb.Width;
                float a3 = (1f - phaseProgress) * 0.35f;
                sb.Draw(srb, drawPos, null, ClairDeLunePalette.NightMist with { A = 0 } * a3 * fadeOut,
                    0f, srb.Size() * 0.5f, s3, SpriteEffects.None, 0f);
            }

            // Dual-color flickering core
            float flicker = 0.6f + 0.4f * MathF.Sin(Main.GameUpdateCount * 0.3f);
            sb.Draw(srb, drawPos, null, ClairDeLunePalette.SoftBlue with { A = 0 } * 0.3f * flicker * fadeOut,
                0f, srb.Size() * 0.5f, 16f / srb.Width, SpriteEffects.None, 0f);
            sb.Draw(srb, drawPos, null, ClairDeLunePalette.NightMist with { A = 0 } * 0.3f * (1f - flicker) * fadeOut,
                0f, srb.Size() * 0.5f, 20f / srb.Width, SpriteEffects.None, 0f);

            // White-hot center
            sb.Draw(srb, drawPos, null, ClairDeLunePalette.WhiteHot with { A = 0 } * 0.5f * fadeOut,
                0f, srb.Size() * 0.5f, 6f / srb.Width, SpriteEffects.None, 0f);

            // Star flare accent
            float flareRot = Main.GlobalTimeWrappedHourly * 1.5f;
            sb.Draw(sf, drawPos, null, ClairDeLunePalette.PearlWhite with { A = 0 } * 0.25f * fadeOut,
                flareRot, sf.Size() * 0.5f, 12f / sf.Width, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                SamplerState.PointClamp, DepthStencilState.None,
                RasterizerState.CullNone, null, matrix);
        }
    }
}
