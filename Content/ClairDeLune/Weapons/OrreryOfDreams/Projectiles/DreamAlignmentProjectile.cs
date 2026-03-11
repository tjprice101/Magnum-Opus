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

namespace MagnumOpus.Content.ClairDeLune.Weapons.OrreryOfDreams.Projectiles
{
    /// <summary>
    /// Dream Alignment  Ecombined sphere blast triggered every 12s.
    /// 3-phase chain detonation representing Inner, Middle, Outer alignment.
    /// 3 render passes: (1) CelestialOrbit.fx CelestialOrbitPath expanding rings,
    /// (2) RadialNoiseMaskShader detonation zone, (3) Multi-scale bloom + phase rings.
    /// </summary>
    public class DreamAlignmentProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

        private int _phase;
        private int _phaseTimer;
        private const int PhaseDuration = 15;
        private const float InnerRadius = 48f;
        private const float MiddleRadius = 96f;
        private const float OuterRadius = 160f;

        // --- Shader + texture caching ---
        private static Effect _celestialShader;
        private static Effect _radialNoiseShader;
        private static Asset<Texture2D> _softCircle;
        private static Asset<Texture2D> _softRadialBloom;
        private static Asset<Texture2D> _starFlare;
        private static Asset<Texture2D> _noiseTex;

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
            Projectile.alpha = 0;
        }

        public override void AI()
        {
            if (_phase == 0)
            {
                Projectile.velocity *= 0.96f;

                if (Projectile.velocity.Length() < 2f || Projectile.timeLeft < 90)
                {
                    _phase = 1;
                    _phaseTimer = 0;
                    Projectile.velocity = Vector2.Zero;
                    Projectile.timeLeft = PhaseDuration * 3 + 10;

                    SoundEngine.PlaySound(SoundID.Item29 with { Pitch = -0.4f, Volume = 0.7f }, Projectile.Center);
                    TriggerBurst(InnerRadius, ClairDeLunePalette.PearlWhite, 0.3f);
                }
            }
            else
            {
                _phaseTimer++;

                if (_phase == 1 && _phaseTimer >= PhaseDuration)
                {
                    _phase = 2;
                    _phaseTimer = 0;
                    SoundEngine.PlaySound(SoundID.Item29 with { Pitch = -0.1f, Volume = 0.8f }, Projectile.Center);
                    TriggerBurst(MiddleRadius, ClairDeLunePalette.SoftBlue, 0.5f);
                    DealAreaDamage(MiddleRadius, 1.0f);
                }
                else if (_phase == 2 && _phaseTimer >= PhaseDuration)
                {
                    _phase = 3;
                    _phaseTimer = 0;
                    SoundEngine.PlaySound(SoundID.Item29 with { Pitch = 0.2f, Volume = 0.9f }, Projectile.Center);
                    TriggerBurst(OuterRadius, ClairDeLunePalette.NightMist, 0.8f);
                    DealAreaDamage(OuterRadius, 1.5f);
                }
                else if (_phase == 3 && _phaseTimer >= PhaseDuration)
                {
                    Projectile.Kill();
                }
            }

            Lighting.AddLight(Projectile.Center, ClairDeLunePalette.PearlBlue.ToVector3() * 0.6f);
        }

        private void TriggerBurst(float radius, Color color, float intensity)
        {
            int ringCount = (int)(radius / 16f);
            for (int i = 0; i < ringCount; i++)
            {
                float angle = MathHelper.TwoPi * i / ringCount;
                Vector2 vel = angle.ToRotationVector2() * (radius * 0.05f);

                var bloom = new BloomParticle(
                    Projectile.Center + angle.ToRotationVector2() * radius * 0.2f,
                    vel, color with { A = 0 } * (0.3f + intensity * 0.4f),
                    0.15f + intensity * 0.1f, 25);
                MagnumParticleHandler.SpawnParticle(bloom);
            }

            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f + Main.rand.NextFloat(-0.2f, 0.2f);
                Vector2 vel = angle.ToRotationVector2() * (3f + intensity * 3f);

                var sparkle = new SparkleParticle(
                    Projectile.Center, vel,
                    ClairDeLunePalette.PearlWhite with { A = 0 } * 0.5f,
                    0.1f + intensity * 0.08f, 20);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            var flash = new BloomParticle(Projectile.Center, Vector2.Zero,
                ClairDeLunePalette.WhiteHot with { A = 0 } * (0.4f + intensity * 0.3f),
                0.3f + intensity * 0.3f, 10);
            MagnumParticleHandler.SpawnParticle(flash);
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
                    owner.ApplyDamageToNPC(npc, dmg, Projectile.knockBack, 0, false);
                }
            }
        }

        private void LoadTextures()
        {
            _softCircle ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/MasksAndShapes/SoftCircle", AssetRequestMode.ImmediateLoad);
            _softRadialBloom ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom", AssetRequestMode.ImmediateLoad);
            _starFlare ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/StarFlare", AssetRequestMode.ImmediateLoad);
            _noiseTex ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/NoiseTextures/CosmicEnergyVortex", AssetRequestMode.ImmediateLoad);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (Main.dedServ) return false;
            LoadTextures();

            SpriteBatch sb = Main.spriteBatch;
            try
            {
            Matrix matrix = Main.GameViewMatrix.TransformationMatrix;

            if (_phase == 0)
            {
                DrawTravelingOrb(sb, matrix);
            }
            else
            {
                float phaseProgress = _phaseTimer / (float)PhaseDuration;
                DrawCelestialRings(sb, matrix, phaseProgress);     // Pass 1: CelestialOrbitPath rings
                DrawRadialNoiseZone(sb, matrix, phaseProgress);    // Pass 2: RadialNoiseMask detonation
                DrawBloomPhaseRings(sb, matrix, phaseProgress);    // Pass 3: Bloom + phase rings
            }
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

        // ---- Traveling orb (phase 0) ----
        private void DrawTravelingOrb(SpriteBatch sb, Matrix matrix)
        {
            Vector2 pos = Projectile.Center - Main.screenPosition;
            Texture2D srb = _softRadialBloom.Value;
            float pulse = 0.85f + 0.15f * MathF.Sin(Main.GameUpdateCount * 0.12f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                SamplerState.LinearClamp, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null, matrix);

            sb.Draw(srb, pos, null,
                ClairDeLunePalette.PearlBlue with { A = 0 } * 0.4f * pulse, 0f, srb.Size() * 0.5f,
                16f / srb.Width, SpriteEffects.None, 0f);
            sb.Draw(srb, pos, null,
                ClairDeLunePalette.WhiteHot with { A = 0 } * 0.5f, 0f, srb.Size() * 0.5f,
                8f / srb.Width, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null, matrix);
        }

        // ---- PASS 1: CelestialOrbit.fx CelestialOrbitPath expanding rings ----
        private void DrawCelestialRings(SpriteBatch sb, Matrix matrix, float phaseProgress)
        {
            _celestialShader ??= ShaderLoader.CelestialOrbit;
            if (_celestialShader == null) return;

            float currentRadius = 0f;
            Color ringColor = ClairDeLunePalette.PearlWhite;

            if (_phase == 1) { currentRadius = InnerRadius * phaseProgress; ringColor = ClairDeLunePalette.PearlWhite; }
            else if (_phase == 2) { currentRadius = MiddleRadius * phaseProgress; ringColor = ClairDeLunePalette.SoftBlue; }
            else if (_phase == 3) { currentRadius = OuterRadius * phaseProgress; ringColor = ClairDeLunePalette.NightMist; }

            sb.End();

            _celestialShader.Parameters["uColor"]?.SetValue(ringColor.ToVector4());
            _celestialShader.Parameters["uSecondaryColor"]?.SetValue(ClairDeLunePalette.PearlBlue.ToVector4());
            _celestialShader.Parameters["uOpacity"]?.SetValue(0.4f * (1f - phaseProgress * 0.5f));
            _celestialShader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
            _celestialShader.Parameters["uIntensity"]?.SetValue(1.5f);
            _celestialShader.Parameters["uOverbrightMult"]?.SetValue(1.2f);
            _celestialShader.Parameters["uScrollSpeed"]?.SetValue(2f);
            _celestialShader.Parameters["uDistortionAmt"]?.SetValue(0.02f);
            _celestialShader.Parameters["uHasSecondaryTex"]?.SetValue(false);

            _celestialShader.CurrentTechnique = _celestialShader.Techniques["CelestialOrbitPath"];

            sb.Begin(SpriteSortMode.Immediate, MagnumBlendStates.ShaderAdditive,
                SamplerState.LinearWrap, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, _celestialShader, matrix);

            Texture2D sc = _softCircle.Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float ringScale = currentRadius * 2f / sc.Width;
            sb.Draw(sc, drawPos, null, Color.White, 0f, sc.Size() * 0.5f, ringScale, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                SamplerState.LinearClamp, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null, matrix);
        }

        // ---- PASS 2: RadialNoiseMaskShader detonation zone ----
        private void DrawRadialNoiseZone(SpriteBatch sb, Matrix matrix, float phaseProgress)
        {
            _radialNoiseShader ??= ModContent.Request<Effect>(
                "MagnumOpus/Content/FoundationWeapons/MaskFoundation/Shaders/RadialNoiseMaskShader",
                AssetRequestMode.ImmediateLoad).Value;
            if (_radialNoiseShader == null) return;

            float activeRadius = _phase == 1 ? InnerRadius : _phase == 2 ? MiddleRadius : OuterRadius;
            float opacity = 0.3f * (1f - phaseProgress * 0.3f);

            sb.End();

            _radialNoiseShader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
            _radialNoiseShader.Parameters["scrollSpeed"]?.SetValue(2f + _phase);
            _radialNoiseShader.Parameters["rotationSpeed"]?.SetValue(3f);
            _radialNoiseShader.Parameters["circleRadius"]?.SetValue(0.4f);
            _radialNoiseShader.Parameters["edgeSoftness"]?.SetValue(0.15f);
            _radialNoiseShader.Parameters["intensity"]?.SetValue(1.5f);
            _radialNoiseShader.Parameters["primaryColor"]?.SetValue(ClairDeLunePalette.SoftBlue.ToVector3());
            _radialNoiseShader.Parameters["coreColor"]?.SetValue(ClairDeLunePalette.PearlWhite.ToVector3());

            Main.graphics.GraphicsDevice.Textures[1] = _noiseTex.Value;
            _radialNoiseShader.CurrentTechnique = _radialNoiseShader.Techniques["Technique1"];

            sb.Begin(SpriteSortMode.Immediate, MagnumBlendStates.ShaderAdditive,
                SamplerState.LinearWrap, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, _radialNoiseShader, matrix);

            Texture2D sc = _softCircle.Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float zoneScale = activeRadius * 2.5f * phaseProgress / sc.Width;
            sb.Draw(sc, drawPos, null, Color.White * opacity, 0f, sc.Size() * 0.5f, zoneScale, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                SamplerState.LinearClamp, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null, matrix);
        }

        // ---- PASS 3: Multi-scale bloom + phase rings ----
        private void DrawBloomPhaseRings(SpriteBatch sb, Matrix matrix, float phaseProgress)
        {
            Vector2 pos = Projectile.Center - Main.screenPosition;
            Texture2D srb = _softRadialBloom.Value;
            Texture2D sf = _starFlare.Value;

            // Completed ring halos
            if (_phase >= 1)
                DrawRingHalo(sb, srb, pos, InnerRadius, ClairDeLunePalette.PearlWhite, _phase == 1 ? phaseProgress : 1f);
            if (_phase >= 2)
                DrawRingHalo(sb, srb, pos, MiddleRadius, ClairDeLunePalette.SoftBlue, _phase == 2 ? phaseProgress : 1f);
            if (_phase >= 3)
                DrawRingHalo(sb, srb, pos, OuterRadius, ClairDeLunePalette.NightMist, _phase == 3 ? phaseProgress : 1f);

            // Center glow
            float centerPulse = 0.8f + 0.2f * MathF.Sin(Main.GameUpdateCount * 0.2f);
            sb.Draw(srb, pos, null,
                ClairDeLunePalette.WhiteHot with { A = 0 } * 0.4f * centerPulse, 0f, srb.Size() * 0.5f,
                12f / srb.Width, SpriteEffects.None, 0f);

            // Star flare at center during detonation
            float flareRot = Main.GlobalTimeWrappedHourly * 3f;
            sb.Draw(sf, pos, null,
                ClairDeLunePalette.PearlBlue with { A = 0 } * 0.2f,
                flareRot, sf.Size() * 0.5f, 16f / sf.Width, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null, matrix);
        }

        private void DrawRingHalo(SpriteBatch sb, Texture2D srb, Vector2 center, float radius, Color color, float progress)
        {
            float currentRadius = radius * progress;
            float fade = 1f - progress * 0.5f;

            // Ring rendered as haze at radius (capped 300px max)
            float hazeScale = MathHelper.Min(currentRadius * 2f / srb.Width, 0.139f);
            sb.Draw(srb, center, null,
                color with { A = 0 } * 0.06f * fade, 0f, srb.Size() * 0.5f,
                hazeScale, SpriteEffects.None, 0f);

            // Ring dots
            int points = 16;
            for (int i = 0; i < points; i++)
            {
                float angle = MathHelper.TwoPi * i / points;
                Vector2 ringPos = center + angle.ToRotationVector2() * currentRadius;
                sb.Draw(srb, ringPos, null,
                    color with { A = 0 } * 0.12f * fade, 0f, srb.Size() * 0.5f,
                    4f / srb.Width, SpriteEffects.None, 0f);
            }
        }
    }
}
