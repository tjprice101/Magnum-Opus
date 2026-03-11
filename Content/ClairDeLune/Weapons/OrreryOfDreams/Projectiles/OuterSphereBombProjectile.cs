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
    /// Outer Sphere Bomb — Slow AoE dream bomb from OrreryOfDreams.
    /// Detonates on contact or timer, creating large misty explosion.
    /// 3 render passes: (1) CelestialOrbit CelestialOrbitPath orbit body,
    /// (2) RadialNoiseMaskShader detonation zone (on explosion), (3) Multi-scale bloom + expanding mist.
    /// </summary>
    public class OuterSphereBombProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

        private const int DetonationTime = 90;
        private const float ExplosionRadius = 80f;
        private bool _detonated;
        private int _detonateTimer;

        // --- Shader + texture caching ---
        private static Effect _celestialShader;
        private static Effect _radialNoiseShader;
        private static Asset<Texture2D> _softCircle;
        private static Asset<Texture2D> _softRadialBloom;
        private static Asset<Texture2D> _pointBloom;
        private static Asset<Texture2D> _noiseTex;
        private static Asset<Texture2D> _gradientLUT;

        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.timeLeft = DetonationTime;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = DetonationTime;
        }

        public override void AI()
        {
            if (_detonated)
            {
                _detonateTimer++;
                Projectile.velocity = Vector2.Zero;

                if (_detonateTimer > 30)
                {
                    Projectile.Kill();
                    return;
                }

                Lighting.AddLight(Projectile.Center,
                    ClairDeLunePalette.PearlFrost.ToVector3() * 0.4f * (1f - _detonateTimer / 30f));
                return;
            }

            // Slow drift with dream wobble
            Projectile.velocity *= 0.98f;
            float wobble = MathF.Sin(Main.GameUpdateCount * 0.06f + Projectile.whoAmI * 2f) * 0.15f;
            Projectile.velocity = Projectile.velocity.RotatedBy(wobble * 0.02f);
            Projectile.rotation += 0.02f;

            // Dream mist particles
            if (Main.GameUpdateCount % 4 == 0)
            {
                var mist = new GenericGlowParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(8f, 8f),
                    Main.rand.NextVector2Circular(0.5f, 0.5f),
                    ClairDeLunePalette.NightMist with { A = 0 } * 0.15f, 0.06f, 12, true);
                MagnumParticleHandler.SpawnParticle(mist);
            }

            float life = 1f - (Projectile.timeLeft / (float)DetonationTime);
            Lighting.AddLight(Projectile.Center, ClairDeLunePalette.SoftBlue.ToVector3() * (0.15f + life * 0.2f));
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!_detonated)
                Detonate();
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (!_detonated)
                Detonate();
            return false;
        }

        public override void OnKill(int timeLeft)
        {
            if (!_detonated)
                Detonate();
        }

        private void Detonate()
        {
            _detonated = true;
            Projectile.velocity = Vector2.Zero;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 35;
            Projectile.friendly = false;

            SoundEngine.PlaySound(SoundID.Item29 with { Pitch = -0.2f, Volume = 0.6f }, Projectile.Center);

            // AoE damage
            for (int n = 0; n < Main.maxNPCs; n++)
            {
                NPC npc = Main.npc[n];
                if (!npc.active || npc.friendly || npc.dontTakeDamage) continue;
                if (Vector2.Distance(Projectile.Center, npc.Center) <= ExplosionRadius)
                {
                    Player owner = Main.player[Projectile.owner];
                    owner.ApplyDamageToNPC(npc, Projectile.damage, 0f, 0, false);
                }
            }

            // Expanding mist ring particles
            for (int ring = 0; ring < 5; ring++)
            {
                float ringRadius = 10f + ring * 15f;
                for (int p = 0; p < 8; p++)
                {
                    float angle = MathHelper.TwoPi * p / 8f + ring * 0.3f;
                    Vector2 vel = angle.ToRotationVector2() * (2f + ring * 0.8f);
                    Color ringCol = Color.Lerp(ClairDeLunePalette.PearlWhite, ClairDeLunePalette.NightMist, ring / 5f);
                    var bloom = new BloomParticle(
                        Projectile.Center + angle.ToRotationVector2() * ringRadius * 0.3f,
                        vel, ringCol with { A = 0 } * 0.4f, 0.12f + ring * 0.04f, 18 + ring * 4);
                    MagnumParticleHandler.SpawnParticle(bloom);
                }
            }

            var flash = new BloomParticle(Projectile.Center, Vector2.Zero,
                ClairDeLunePalette.WhiteHot with { A = 0 } * 0.5f, 0.4f, 10);
            MagnumParticleHandler.SpawnParticle(flash);
        }

        private void LoadTextures()
        {
            _softCircle ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/MasksAndShapes/SoftCircle", AssetRequestMode.ImmediateLoad);
            _softRadialBloom ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom", AssetRequestMode.ImmediateLoad);
            _pointBloom ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom", AssetRequestMode.ImmediateLoad);
            _noiseTex ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/NoiseTextures/PerlinNoise", AssetRequestMode.ImmediateLoad);
            _gradientLUT ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/ColorGradients/ClairDeLuneGradientLUTandRAMP", AssetRequestMode.ImmediateLoad);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (Main.dedServ) return false;
            LoadTextures();

            SpriteBatch sb = Main.spriteBatch;
            try
            {
            Matrix matrix = Main.GameViewMatrix.TransformationMatrix;

            if (_detonated)
            {
                DrawDetonationZone(sb, matrix);   // Detonation: RadialNoiseMask expanding zone
                DrawDetonationBloom(sb, matrix);  // Detonation bloom
            }
            else
            {
                DrawCelestialBody(sb, matrix);    // Pass 1: CelestialOrbitPath orbit body
                DrawBloomHalo(sb, matrix);        // Pass 2: Multi-scale bloom halo
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

        // ---- PRE-DETONATION: CelestialOrbit CelestialOrbitPath body ----
        private void DrawCelestialBody(SpriteBatch sb, Matrix matrix)
        {
            _celestialShader ??= ShaderLoader.CelestialOrbit;
            if (_celestialShader == null) return;

            float life = 1f - (Projectile.timeLeft / (float)DetonationTime);
            float pulse = 0.8f + 0.2f * MathF.Sin(Main.GameUpdateCount * 0.15f + life * 8f);

            sb.End();

            _celestialShader.Parameters["uColor"]?.SetValue(ClairDeLunePalette.NightMist.ToVector4());
            _celestialShader.Parameters["uSecondaryColor"]?.SetValue(ClairDeLunePalette.SoftBlue.ToVector4());
            _celestialShader.Parameters["uOpacity"]?.SetValue(0.5f + life * 0.3f);
            _celestialShader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
            _celestialShader.Parameters["uIntensity"]?.SetValue(0.8f + life * 0.8f);
            _celestialShader.Parameters["uOverbrightMult"]?.SetValue(1.0f);
            _celestialShader.Parameters["uScrollSpeed"]?.SetValue(1f + life * 2f);
            _celestialShader.Parameters["uDistortionAmt"]?.SetValue(0.02f);
            _celestialShader.Parameters["uHasSecondaryTex"]?.SetValue(false);

            _celestialShader.CurrentTechnique = _celestialShader.Techniques["CelestialOrbitPath"];

            sb.Begin(SpriteSortMode.Immediate, MagnumBlendStates.ShaderAdditive,
                SamplerState.LinearWrap, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, _celestialShader, matrix);

            Texture2D sc = _softCircle.Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float bodyScale = (24f + life * 8f) * pulse / sc.Width;
            sb.Draw(sc, drawPos, null, Color.White, Projectile.rotation, sc.Size() * 0.5f, bodyScale, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                SamplerState.LinearClamp, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null, matrix);
        }

        // ---- PRE-DETONATION: Multi-scale bloom halo ----
        private void DrawBloomHalo(SpriteBatch sb, Matrix matrix)
        {
            Vector2 pos = Projectile.Center - Main.screenPosition;
            Texture2D srb = _softRadialBloom.Value;
            Texture2D pb = _pointBloom.Value;
            float life = 1f - (Projectile.timeLeft / (float)DetonationTime);
            float pulse = 0.8f + 0.2f * MathF.Sin(Main.GameUpdateCount * 0.15f + life * 8f);

            // Outer mist
            sb.Draw(srb, pos, null,
                ClairDeLunePalette.NightMist with { A = 0 } * (0.1f + life * 0.15f), 0f, srb.Size() * 0.5f,
                (28f + life * 12f) * pulse / srb.Width, SpriteEffects.None, 0f);

            // Mid blue
            sb.Draw(srb, pos, null,
                ClairDeLunePalette.MidnightBlue with { A = 0 } * (0.15f + life * 0.2f), 0f, srb.Size() * 0.5f,
                (18f + life * 6f) * pulse / srb.Width, SpriteEffects.None, 0f);

            // Core
            sb.Draw(pb, pos, null,
                ClairDeLunePalette.PearlFrost with { A = 0 } * (0.12f + life * 0.2f), 0f, pb.Size() * 0.5f,
                8f * pulse / pb.Width, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null, matrix);
        }

        // ---- DETONATION: RadialNoiseMask expanding zone ----
        private void DrawDetonationZone(SpriteBatch sb, Matrix matrix)
        {
            _radialNoiseShader ??= ModContent.Request<Effect>(
                "MagnumOpus/Content/FoundationWeapons/MaskFoundation/Shaders/RadialNoiseMaskShader",
                AssetRequestMode.ImmediateLoad).Value;
            if (_radialNoiseShader == null) return;

            float detonateProgress = _detonateTimer / 30f;
            float expandRadius = ExplosionRadius * MathF.Min(detonateProgress * 2f, 1f);
            float fadeAlpha = 1f - MathF.Max(0f, detonateProgress - 0.5f) * 2f;

            sb.End();

            _radialNoiseShader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 2f);
            _radialNoiseShader.Parameters["scrollSpeed"]?.SetValue(3f);
            _radialNoiseShader.Parameters["rotationSpeed"]?.SetValue(2f);
            _radialNoiseShader.Parameters["circleRadius"]?.SetValue(0.45f);
            _radialNoiseShader.Parameters["edgeSoftness"]?.SetValue(0.15f);
            _radialNoiseShader.Parameters["intensity"]?.SetValue(1.2f * fadeAlpha);
            _radialNoiseShader.Parameters["primaryColor"]?.SetValue(ClairDeLunePalette.NightMist.ToVector3());
            _radialNoiseShader.Parameters["coreColor"]?.SetValue(ClairDeLunePalette.PearlFrost.ToVector3());

            Main.graphics.GraphicsDevice.Textures[1] = _noiseTex.Value;
            Main.graphics.GraphicsDevice.Textures[2] = _gradientLUT.Value;

            _radialNoiseShader.CurrentTechnique = _radialNoiseShader.Techniques["Technique1"];

            sb.Begin(SpriteSortMode.Immediate, MagnumBlendStates.ShaderAdditive,
                SamplerState.LinearWrap, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, _radialNoiseShader, matrix);

            Texture2D sc = _softCircle.Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float zoneScale = expandRadius * 2f / sc.Width;
            sb.Draw(sc, drawPos, null, Color.White, 0f, sc.Size() * 0.5f, zoneScale, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                SamplerState.LinearClamp, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null, matrix);
        }

        // ---- DETONATION: Bloom ----
        private void DrawDetonationBloom(SpriteBatch sb, Matrix matrix)
        {
            Vector2 pos = Projectile.Center - Main.screenPosition;
            Texture2D srb = _softRadialBloom.Value;
            Texture2D pb = _pointBloom.Value;
            float detonateProgress = _detonateTimer / 30f;
            float fadeAlpha = 1f - detonateProgress;

            // Expanding mist haze
            sb.Draw(srb, pos, null,
                ClairDeLunePalette.NightMist with { A = 0 } * 0.2f * fadeAlpha, 0f, srb.Size() * 0.5f,
                ExplosionRadius * detonateProgress * 2f / srb.Width, SpriteEffects.None, 0f);

            // Core flash (bright early, fades fast)
            float coreAlpha = MathF.Max(0f, 1f - detonateProgress * 3f) * 0.4f;
            sb.Draw(pb, pos, null,
                ClairDeLunePalette.WhiteHot with { A = 0 } * coreAlpha, 0f, pb.Size() * 0.5f,
                20f / pb.Width, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null, matrix);
        }
    }
}
