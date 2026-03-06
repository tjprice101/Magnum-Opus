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

namespace MagnumOpus.Content.ClairDeLune.Weapons.GearDrivenArbiter.Projectiles
{
    /// <summary>
    /// Arbiter Verdict — 2-phase countdown-then-verdict zone projectile.
    /// 3 render passes: (1) JudgmentMark.fx JudgmentMarkSigil countdown clock,
    /// (2) RadialNoiseMaskShader zone build-up, (3) Multi-scale bloom + flare verdict.
    /// </summary>
    public class ArbiterVerdictProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

        private const int CountdownDuration = 60;
        private const float VerdictRadius = 96f;
        private int _timer;
        private bool _verdictDelivered;

        // --- Shader + texture caching ---
        private static Effect _judgmentShader;
        private static Effect _radialNoiseShader;
        private static Asset<Texture2D> _softCircle;
        private static Asset<Texture2D> _softRadialBloom;
        private static Asset<Texture2D> _starFlare;
        private static Asset<Texture2D> _noiseTex;

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = false;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 90;
            Projectile.tileCollide = false;
            Projectile.localNPCHitCooldown = 60;
            Projectile.usesLocalNPCImmunity = true;
        }

        public override void AI()
        {
            _timer++;
            Projectile.velocity *= 0.92f;

            float countProg = Math.Clamp((float)_timer / CountdownDuration, 0f, 1f);

            // Countdown phase — rising tension
            if (_timer <= CountdownDuration)
            {
                // Tick mark particles appear every quarter
                int quarterTick = CountdownDuration / 4;
                if (_timer % quarterTick == 0 && _timer > 0)
                {
                    int tickNum = _timer / quarterTick;
                    SoundEngine.PlaySound(SoundID.Item12 with { Pitch = -0.3f + tickNum * 0.15f, Volume = 0.35f }, Projectile.Center);

                    for (int i = 0; i < 8; i++)
                    {
                        float angle = MathHelper.TwoPi * i / 8f;
                        float rad = 20f + countProg * 30f;
                        Vector2 pos = Projectile.Center + angle.ToRotationVector2() * rad;
                        Vector2 vel = (Projectile.Center - pos).SafeNormalize(Vector2.Zero) * 1.5f;
                        var tick = new GenericGlowParticle(pos, vel,
                            ClairDeLunePalette.MoonbeamGold with { A = 0 } * 0.4f, 0.06f, 12, true);
                        MagnumParticleHandler.SpawnParticle(tick);
                    }
                }

                // Spiral inward particles during countdown
                if (_timer > CountdownDuration / 2 && Main.rand.NextBool(2))
                {
                    float spiralAngle = Main.GlobalTimeWrappedHourly * 6f + Main.rand.NextFloat(MathHelper.TwoPi);
                    float spiralRadius = VerdictRadius * 0.6f * (1f - countProg);
                    Vector2 spiralPos = Projectile.Center + spiralAngle.ToRotationVector2() * spiralRadius;
                    Vector2 spiralVel = (Projectile.Center - spiralPos).SafeNormalize(Vector2.Zero) * 2.5f;
                    var spiral = new GenericGlowParticle(spiralPos, spiralVel,
                        ClairDeLunePalette.SilverLining with { A = 0 } * 0.3f, 0.04f, 10, true);
                    MagnumParticleHandler.SpawnParticle(spiral);
                }
            }

            // Verdict delivery
            if (_timer == CountdownDuration && !_verdictDelivered)
            {
                _verdictDelivered = true;
                SoundEngine.PlaySound(SoundID.Item14 with { Pitch = -0.5f, Volume = 0.9f }, Projectile.Center);

                // AoE damage
                for (int n = 0; n < Main.maxNPCs; n++)
                {
                    NPC npc = Main.npc[n];
                    if (!npc.active || npc.friendly || npc.dontTakeDamage) continue;
                    if (Vector2.Distance(Projectile.Center, npc.Center) < VerdictRadius)
                    {
                        Player player = Main.player[Projectile.owner];
                        player.ApplyDamageToNPC(npc, Projectile.damage, 12f, (npc.Center - Projectile.Center).X > 0 ? 1 : -1, false);
                    }
                }

                // Verdict burst — 3 expanding gear rings
                for (int ring = 0; ring < 3; ring++)
                {
                    int pointsInRing = 12 + ring * 4;
                    float speed = 3f + ring * 2f;
                    Color ringCol = Color.Lerp(ClairDeLunePalette.MoonbeamGold, ClairDeLunePalette.PearlFrost, ring / 2f);
                    for (int p = 0; p < pointsInRing; p++)
                    {
                        float angle = MathHelper.TwoPi * p / pointsInRing + ring * 0.15f;
                        Vector2 vel = angle.ToRotationVector2() * speed;
                        var burst = new GenericGlowParticle(Projectile.Center, vel,
                            ringCol with { A = 0 } * (0.5f - ring * 0.1f), 0.07f, 12 + ring * 2, true);
                        MagnumParticleHandler.SpawnParticle(burst);
                    }
                }

                // Gavel-strike central flash
                var flash = new BloomParticle(Projectile.Center, Vector2.Zero,
                    ClairDeLunePalette.MoonbeamGold with { A = 0 } * 0.7f, 0.6f, 12);
                MagnumParticleHandler.SpawnParticle(flash);
            }

            // Post-verdict fade
            if (_timer > CountdownDuration + 20)
                Projectile.Kill();

            float lightIntensity = _verdictDelivered ? 0.8f : 0.3f + countProg * 0.3f;
            Lighting.AddLight(Projectile.Center, ClairDeLunePalette.MoonbeamGold.ToVector3() * lightIntensity);
        }

        private void LoadTextures()
        {
            _softCircle ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/MasksAndShapes/SoftCircle", AssetRequestMode.ImmediateLoad);
            _softRadialBloom ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom", AssetRequestMode.ImmediateLoad);
            _starFlare ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/StarFlare", AssetRequestMode.ImmediateLoad);
            _noiseTex ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX/Noise/CosmicEnergyNoise", AssetRequestMode.ImmediateLoad);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (Main.dedServ) return false;
            LoadTextures();

            SpriteBatch sb = Main.spriteBatch;
            Matrix matrix = Main.GameViewMatrix.TransformationMatrix;

            float countProg = Math.Clamp((float)_timer / CountdownDuration, 0f, 1f);
            float postProg = _verdictDelivered ? Math.Clamp((float)(_timer - CountdownDuration) / 20f, 0f, 1f) : 0f;

            DrawJudgmentSigil(sb, matrix, countProg, postProg);     // Pass 1: JudgmentMarkSigil countdown
            DrawRadialNoiseZone(sb, matrix, countProg, postProg);   // Pass 2: RadialNoiseMask build-up
            DrawBloomVerdict(sb, matrix, countProg, postProg);      // Pass 3: Bloom + flare
            return false;
        }

        // ---- PASS 1: JudgmentMark.fx JudgmentMarkSigil countdown clock ----
        private void DrawJudgmentSigil(SpriteBatch sb, Matrix matrix, float countProg, float postProg)
        {
            _judgmentShader ??= ShaderLoader.JudgmentMark;
            if (_judgmentShader == null) return;

            float opacity = _verdictDelivered ? (1f - postProg) * 0.8f : countProg * 0.6f;
            if (opacity <= 0.01f) return;

            sb.End();

            _judgmentShader.Parameters["uColor"]?.SetValue(ClairDeLunePalette.MoonbeamGold.ToVector4());
            _judgmentShader.Parameters["uSecondaryColor"]?.SetValue(ClairDeLunePalette.SilverLining.ToVector4());
            _judgmentShader.Parameters["uOpacity"]?.SetValue(opacity);
            _judgmentShader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
            _judgmentShader.Parameters["uIntensity"]?.SetValue(_verdictDelivered ? 2f : 0.8f + countProg);
            _judgmentShader.Parameters["uOverbrightMult"]?.SetValue(1.2f);
            _judgmentShader.Parameters["uScrollSpeed"]?.SetValue(1f + countProg * 2f);
            _judgmentShader.Parameters["uDistortionAmt"]?.SetValue(0.015f);
            _judgmentShader.Parameters["uHasSecondaryTex"]?.SetValue(false);

            string technique = _verdictDelivered ? "JudgmentMarkDetonate" : "JudgmentMarkSigil";
            _judgmentShader.CurrentTechnique = _judgmentShader.Techniques[technique];

            sb.Begin(SpriteSortMode.Immediate, MagnumBlendStates.ShaderAdditive,
                SamplerState.LinearWrap, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, _judgmentShader, matrix);

            Texture2D sc = _softCircle.Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float sigilScale = (30f + countProg * 50f) / sc.Width;
            if (_verdictDelivered)
                sigilScale *= 1f + postProg * 0.5f;

            float sigilRot = _verdictDelivered ? 0f : Main.GlobalTimeWrappedHourly * -1.5f;
            sb.Draw(sc, drawPos, null, Color.White, sigilRot, sc.Size() * 0.5f, sigilScale, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                SamplerState.LinearClamp, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null, matrix);
        }

        // ---- PASS 2: RadialNoiseMaskShader zone build-up ----
        private void DrawRadialNoiseZone(SpriteBatch sb, Matrix matrix, float countProg, float postProg)
        {
            if (countProg < 0.3f) return;

            _radialNoiseShader ??= ModContent.Request<Effect>(
                "MagnumOpus/Content/FoundationWeapons/MaskFoundation/Shaders/RadialNoiseMaskShader",
                AssetRequestMode.ImmediateLoad).Value;
            if (_radialNoiseShader == null) return;

            float opacity = _verdictDelivered ? (1f - postProg) * 0.5f : (countProg - 0.3f) / 0.7f * 0.35f;
            if (opacity <= 0.01f) return;

            sb.End();

            _radialNoiseShader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
            _radialNoiseShader.Parameters["scrollSpeed"]?.SetValue(1f + countProg * 2f);
            _radialNoiseShader.Parameters["rotationSpeed"]?.SetValue(_verdictDelivered ? 5f : 2f);
            _radialNoiseShader.Parameters["circleRadius"]?.SetValue(_verdictDelivered ? 0.52f : 0.35f + countProg * 0.1f);
            _radialNoiseShader.Parameters["edgeSoftness"]?.SetValue(0.2f);
            _radialNoiseShader.Parameters["intensity"]?.SetValue(_verdictDelivered ? 2f : 0.8f + countProg);
            _radialNoiseShader.Parameters["primaryColor"]?.SetValue(ClairDeLunePalette.SilverLining.ToVector3());
            _radialNoiseShader.Parameters["coreColor"]?.SetValue(ClairDeLunePalette.MoonbeamGold.ToVector3());

            Main.graphics.GraphicsDevice.Textures[1] = _noiseTex.Value;
            _radialNoiseShader.CurrentTechnique = _radialNoiseShader.Techniques["Technique1"];

            sb.Begin(SpriteSortMode.Immediate, MagnumBlendStates.ShaderAdditive,
                SamplerState.LinearWrap, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, _radialNoiseShader, matrix);

            Texture2D sc = _softCircle.Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float zoneScale = (60f + countProg * 80f) / sc.Width;
            if (_verdictDelivered)
                zoneScale *= 1f + postProg * 0.6f;

            sb.Draw(sc, drawPos, null, Color.White * opacity, 0f, sc.Size() * 0.5f, zoneScale, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                SamplerState.LinearClamp, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null, matrix);
        }

        // ---- PASS 3: Multi-scale bloom + flare verdict ----
        private void DrawBloomVerdict(SpriteBatch sb, Matrix matrix, float countProg, float postProg)
        {
            Texture2D srb = _softRadialBloom.Value;
            Texture2D sf = _starFlare.Value;
            Vector2 pos = Projectile.Center - Main.screenPosition;

            float baseIntensity = _verdictDelivered ? (1f - postProg * 0.5f) : 0.2f + countProg * 0.3f;

            // NightMist outer halo
            sb.Draw(srb, pos, null,
                ClairDeLunePalette.NightMist with { A = 0 } * 0.1f * baseIntensity, 0f, srb.Size() * 0.5f,
                (40f + countProg * 40f) / srb.Width, SpriteEffects.None, 0f);

            // Gold mid
            sb.Draw(srb, pos, null,
                ClairDeLunePalette.MoonbeamGold with { A = 0 } * 0.3f * baseIntensity, 0f, srb.Size() * 0.5f,
                (20f + countProg * 20f) / srb.Width, SpriteEffects.None, 0f);

            // SilverLining core
            sb.Draw(srb, pos, null,
                ClairDeLunePalette.SilverLining with { A = 0 } * 0.2f * baseIntensity, 0f, srb.Size() * 0.5f,
                10f / srb.Width, SpriteEffects.None, 0f);

            // Verdict flash burst
            if (_verdictDelivered && postProg < 0.5f)
            {
                float flashMul = 1f - postProg * 2f;
                float flashScale = (60f + postProg * 120f) / srb.Width;
                sb.Draw(srb, pos, null,
                    Color.White with { A = 0 } * 0.5f * flashMul, 0f, srb.Size() * 0.5f,
                    flashScale, SpriteEffects.None, 0f);

                // Star flare during verdict
                float flareRot = Main.GlobalTimeWrappedHourly * 4f;
                sb.Draw(sf, pos, null,
                    ClairDeLunePalette.MoonbeamGold with { A = 0 } * 0.4f * flashMul,
                    flareRot, sf.Size() * 0.5f, 30f / sf.Width, SpriteEffects.None, 0f);
                sb.Draw(sf, pos, null,
                    ClairDeLunePalette.MoonbeamGold with { A = 0 } * 0.3f * flashMul,
                    -flareRot * 0.7f, sf.Size() * 0.5f, 20f / sf.Width, SpriteEffects.None, 0f);
            }

            // Countdown tick ring (8 evenly-spaced dots)
            if (!_verdictDelivered)
            {
                int visibleTicks = (int)(countProg * 8);
                for (int i = 0; i < visibleTicks; i++)
                {
                    float angle = MathHelper.TwoPi * i / 8f - MathHelper.PiOver2;
                    float tickRad = 32f + countProg * 20f;
                    Vector2 tickPos = pos + angle.ToRotationVector2() * tickRad;
                    sb.Draw(srb, tickPos, null,
                        ClairDeLunePalette.MoonbeamGold with { A = 0 } * 0.3f, 0f, srb.Size() * 0.5f,
                        5f / srb.Width, SpriteEffects.None, 0f);
                }
            }

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null, matrix);
        }
    }
}
