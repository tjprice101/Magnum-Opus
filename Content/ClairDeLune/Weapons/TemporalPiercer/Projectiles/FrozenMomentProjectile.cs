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

namespace MagnumOpus.Content.ClairDeLune.Weapons.TemporalPiercer.Projectiles
{
    /// <summary>
    /// Frozen Moment — Triggered at 5 Temporal Puncture stacks.
    /// 3 render passes: (1) RadialNoiseMaskShader frozen burst disc,
    /// (2) CrystalLance.fx CrystalLanceShatter overlay, (3) Multi-scale expanding bloom.
    /// </summary>
    public class FrozenMomentProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

        private int _timer;
        private const int Duration = 30;
        private const float ExpansionRadius = 120f;

        private int TargetNPC => (int)Projectile.ai[0];

        // --- Shader + texture caching ---
        private static Effect _radialMaskShader;
        private static Effect _crystalLanceShader;
        private static Asset<Texture2D> _noiseTex;
        private static Asset<Texture2D> _gradientLUT;
        private static Asset<Texture2D> _softCircle;
        private static Asset<Texture2D> _softRadialBloom;
        private static Asset<Texture2D> _starFlare;

        public override void SetDefaults()
        {
            Projectile.width = 200;
            Projectile.height = 200;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = Duration;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void AI()
        {
            _timer++;

            if (_timer == 1)
            {
                SoundEngine.PlaySound(SoundID.Item27 with { Pitch = -0.4f, Volume = 0.8f }, Projectile.Center);

                if (TargetNPC >= 0 && TargetNPC < Main.maxNPCs && Main.npc[TargetNPC].active)
                    Main.npc[TargetNPC].AddBuff(BuffID.Frozen, 90);

                var flash = new BloomParticle(Projectile.Center, Vector2.Zero,
                    ClairDeLunePalette.PearlWhite with { A = 0 }, 1.2f, 15);
                MagnumParticleHandler.SpawnParticle(flash);

                for (int i = 0; i < 5; i++)
                {
                    float slashAngle = MathHelper.TwoPi * i / 5f + Main.rand.NextFloat(-0.1f, 0.1f);
                    Vector2 slashDir = slashAngle.ToRotationVector2();

                    for (int p = 0; p < 8; p++)
                    {
                        float dist = 15f + p * 12f;
                        Vector2 pos = Projectile.Center + slashDir * dist;
                        Vector2 vel = slashDir * (4f + p * 0.5f);
                        Color slashCol = Color.Lerp(ClairDeLunePalette.PearlFrost, ClairDeLunePalette.ClockworkBrass, p / 7f) with { A = 0 };
                        var slash = new GenericGlowParticle(pos, vel, slashCol * 0.6f, 0.1f, 12, true);
                        MagnumParticleHandler.SpawnParticle(slash);
                    }
                }

                for (int ring = 0; ring < 6; ring++)
                {
                    float ringRadius = 20f + ring * 18f;
                    Color ringColor = ring switch
                    {
                        0 or 1 => ClairDeLunePalette.PearlFrost,
                        2 or 3 => ClairDeLunePalette.ClockworkBrass,
                        _ => ClairDeLunePalette.PearlWhite
                    };

                    int numPoints = 20 + ring * 4;
                    for (int p = 0; p < numPoints; p++)
                    {
                        float angle = MathHelper.TwoPi * p / numPoints + ring * 0.2f;
                        Vector2 vel = angle.ToRotationVector2() * (ringRadius / 8f);
                        var ringDot = new GenericGlowParticle(
                            Projectile.Center, vel, ringColor with { A = 0 } * 0.5f,
                            0.08f, 12 + ring, true);
                        MagnumParticleHandler.SpawnParticle(ringDot);
                    }
                }

                for (int i = 0; i < 10; i++)
                {
                    float angle = MathHelper.TwoPi * i / 10f;
                    Vector2 vel = angle.ToRotationVector2() * (5f + Main.rand.NextFloat() * 3f);
                    Color fragColor = Main.rand.NextBool()
                        ? ClairDeLunePalette.ClockworkBrass
                        : ClairDeLunePalette.PearlFrost;
                    var frag = new SparkleParticle(Projectile.Center, vel,
                        fragColor with { A = 0 } * 0.7f, 0.12f, 20);
                    MagnumParticleHandler.SpawnParticle(frag);
                }
            }

            if (_timer <= 10)
            {
                float freezeIntensity = 1f - (float)_timer / 10f;
                Lighting.AddLight(Projectile.Center, ClairDeLunePalette.PearlFrost.ToVector3() * freezeIntensity * 2f);
            }
        }

        private void LoadTextures()
        {
            _noiseTex ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/NoiseAndDistortion/FrostCrystalNoise", AssetRequestMode.ImmediateLoad);
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

            DrawRadialFrostZone(sb, matrix);     // Pass 1: RadialNoiseMask frozen burst
            DrawCrystalShatter(sb, matrix);      // Pass 2: CrystalLanceShatter overlay
            DrawExpandingBloom(sb, matrix);       // Pass 3: Multi-scale expanding bloom
            return false;
        }

        // ---- PASS 1: RadialNoiseMaskShader frozen burst disc ----
        private void DrawRadialFrostZone(SpriteBatch sb, Matrix matrix)
        {
            _radialMaskShader ??= ModContent.Request<Effect>(
                "MagnumOpus/Content/FoundationWeapons/MaskFoundation/Shaders/RadialNoiseMaskShader",
                AssetRequestMode.ImmediateLoad).Value;
            if (_radialMaskShader == null) return;

            float progress = (float)_timer / Duration;
            float expandMult = 0.3f + progress * 3f;
            float fadeOut = 1f - progress;

            sb.End();

            _radialMaskShader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
            _radialMaskShader.Parameters["scrollSpeed"]?.SetValue(0.1f);
            _radialMaskShader.Parameters["rotationSpeed"]?.SetValue(0.3f);
            _radialMaskShader.Parameters["circleRadius"]?.SetValue(0.42f);
            _radialMaskShader.Parameters["edgeSoftness"]?.SetValue(0.15f);
            _radialMaskShader.Parameters["intensity"]?.SetValue(1.5f * fadeOut);
            _radialMaskShader.Parameters["primaryColor"]?.SetValue(ClairDeLunePalette.PearlFrost.ToVector3());
            _radialMaskShader.Parameters["coreColor"]?.SetValue(ClairDeLunePalette.PearlWhite.ToVector3());
            _radialMaskShader.Parameters["noiseTex"]?.SetValue(_noiseTex.Value);
            _radialMaskShader.Parameters["gradientTex"]?.SetValue(_gradientLUT.Value);

            _radialMaskShader.CurrentTechnique.Passes["RadialNoiseMaskPass"].Apply();

            sb.Begin(SpriteSortMode.Immediate, BlendState.Additive,
                SamplerState.LinearWrap, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, _radialMaskShader, matrix);

            Texture2D sc = _softCircle.Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float zoneScale = ExpansionRadius * expandMult / sc.Width;
            sb.Draw(sc, drawPos, null, Color.White, 0f, sc.Size() * 0.5f, zoneScale, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                SamplerState.LinearClamp, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null, matrix);
        }

        // ---- PASS 2: CrystalLance.fx CrystalLanceShatter overlay ----
        private void DrawCrystalShatter(SpriteBatch sb, Matrix matrix)
        {
            _crystalLanceShader ??= ShaderLoader.CrystalLance;
            if (_crystalLanceShader == null) return;

            float progress = (float)_timer / Duration;
            float fadeOut = 1f - progress;
            if (fadeOut <= 0f) return;

            sb.End();

            _crystalLanceShader.Parameters["uColor"]?.SetValue(ClairDeLunePalette.PearlFrost.ToVector4());
            _crystalLanceShader.Parameters["uSecondaryColor"]?.SetValue(ClairDeLunePalette.ClockworkBrass.ToVector4());
            _crystalLanceShader.Parameters["uOpacity"]?.SetValue(0.6f * fadeOut);
            _crystalLanceShader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
            _crystalLanceShader.Parameters["uIntensity"]?.SetValue(2f);
            _crystalLanceShader.Parameters["uOverbrightMult"]?.SetValue(1.3f);
            _crystalLanceShader.Parameters["uScrollSpeed"]?.SetValue(3f);
            _crystalLanceShader.Parameters["uDistortionAmt"]?.SetValue(0.04f);
            _crystalLanceShader.Parameters["uHasSecondaryTex"]?.SetValue(false);

            _crystalLanceShader.CurrentTechnique = _crystalLanceShader.Techniques["CrystalLanceShatter"];

            sb.Begin(SpriteSortMode.Immediate, BlendState.Additive,
                SamplerState.LinearWrap, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, _crystalLanceShader, matrix);

            Texture2D sc = _softCircle.Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float expandMult = 0.3f + progress * 3f;
            float overlayScale = ExpansionRadius * expandMult * 1.1f / sc.Width;
            sb.Draw(sc, drawPos, null, Color.White, 0f, sc.Size() * 0.5f, overlayScale, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                SamplerState.LinearClamp, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null, matrix);
        }

        // ---- PASS 3: Multi-scale expanding bloom ----
        private void DrawExpandingBloom(SpriteBatch sb, Matrix matrix)
        {
            Texture2D srb = _softRadialBloom.Value;
            Texture2D sf = _starFlare.Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float progress = (float)_timer / Duration;
            float fadeOut = 1f - progress;
            float expandMult = 0.3f + progress * 3f;

            // Outer NightMist haze
            sb.Draw(srb, drawPos, null,
                ClairDeLunePalette.NightMist with { A = 0 } * 0.2f * fadeOut, 0f, srb.Size() * 0.5f,
                expandMult * ExpansionRadius * 2f / srb.Width, SpriteEffects.None, 0f);

            // Mid SoftBlue
            sb.Draw(srb, drawPos, null,
                ClairDeLunePalette.SoftBlue with { A = 0 } * 0.3f * fadeOut, 0f, srb.Size() * 0.5f,
                expandMult * ExpansionRadius * 1.2f / srb.Width, SpriteEffects.None, 0f);

            // Inner PearlFrost
            sb.Draw(srb, drawPos, null,
                ClairDeLunePalette.PearlFrost with { A = 0 } * 0.45f * fadeOut, 0f, srb.Size() * 0.5f,
                expandMult * ExpansionRadius * 0.6f / srb.Width, SpriteEffects.None, 0f);

            // Hot core
            sb.Draw(srb, drawPos, null,
                ClairDeLunePalette.PearlWhite with { A = 0 } * 0.6f * fadeOut * fadeOut, 0f, srb.Size() * 0.5f,
                expandMult * 20f / srb.Width, SpriteEffects.None, 0f);

            // Brass accent ring
            float ringPhase = MathF.Sin(progress * MathHelper.Pi);
            sb.Draw(srb, drawPos, null,
                ClairDeLunePalette.ClockworkBrass with { A = 0 } * 0.25f * ringPhase, 0f, srb.Size() * 0.5f,
                expandMult * ExpansionRadius * 1.5f / srb.Width, SpriteEffects.None, 0f);

            // Star flare at center
            if (_timer < 10)
            {
                float flareAlpha = 1f - _timer / 10f;
                sb.Draw(sf, drawPos, null, ClairDeLunePalette.PearlWhite with { A = 0 } * 0.4f * flareAlpha,
                    Main.GlobalTimeWrappedHourly * 3f, sf.Size() * 0.5f, 24f / sf.Width, SpriteEffects.None, 0f);
            }

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null, matrix);
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (_timer > 3) return false;
            float dist = Vector2.Distance(Projectile.Center, targetHitbox.Center.ToVector2());
            return dist < ExpansionRadius;
        }
    }
}
