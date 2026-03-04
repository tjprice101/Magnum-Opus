using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.Shaders;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.ClairDeLune.Weapons.StarfallWhisper.Projectiles
{
    /// <summary>
    /// Temporal Fracture — Persistent crack in time at arrow impact point.
    /// 1s duration. Replays hit at 0.5s dealing 40% damage.
    /// 3 render passes: (1) StarfallTrail.fx StarfallBolt frozen crack,
    /// (2) ClairDeLunePearlGlow.fx PearlShimmer replay burst, (3) Multi-scale bloom + crack lines.
    /// </summary>
    public class TemporalFractureProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

        private int _timer;
        private const int Duration = 60;
        private const int ReplayFrame = 30;
        private const float ResonanceCheckRadius = 80f;
        private bool _hasReplayed;
        private bool _hasCheckedResonance;

        // --- Shader + texture caching ---
        private static Effect _starfallShader;
        private static Effect _pearlGlowShader;
        private static Asset<Texture2D> _softCircle;
        private static Asset<Texture2D> _softRadialBloom;
        private static Asset<Texture2D> _starFlare;

        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
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

            if (_timer == ReplayFrame && !_hasReplayed)
            {
                _hasReplayed = true;
                Projectile.friendly = true;

                SoundEngine.PlaySound(SoundID.Item29 with { Pitch = 0.5f, Volume = 0.3f }, Projectile.Center);

                var replayFlash = new BloomParticle(Projectile.Center, Vector2.Zero,
                    ClairDeLunePalette.PearlFrost with { A = 0 } * 0.7f, 0.4f, 8);
                MagnumParticleHandler.SpawnParticle(replayFlash);

                for (int i = 0; i < 8; i++)
                {
                    float angle = MathHelper.TwoPi * i / 8f;
                    Vector2 vel = angle.ToRotationVector2() * 4f;
                    Color crackCol = Color.Lerp(ClairDeLunePalette.NightMist, ClairDeLunePalette.PearlFrost, Main.rand.NextFloat());
                    var crack = new GenericGlowParticle(Projectile.Center, vel,
                        crackCol with { A = 0 } * 0.5f, 0.1f, 10, true);
                    MagnumParticleHandler.SpawnParticle(crack);
                }
            }
            else if (_timer > ReplayFrame + 3)
            {
                Projectile.friendly = false;
            }

            if (_timer == ReplayFrame + 5 && !_hasCheckedResonance)
            {
                _hasCheckedResonance = true;
                CheckFractureResonance();
            }

            if (_timer % 6 == 0)
            {
                float randomAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 sparkPos = Projectile.Center + randomAngle.ToRotationVector2() * Main.rand.NextFloat(8f, 16f);
                var shimmer = new GenericGlowParticle(sparkPos, Vector2.Zero,
                    ClairDeLunePalette.PearlFrost with { A = 0 } * 0.3f, 0.06f, 8, true);
                MagnumParticleHandler.SpawnParticle(shimmer);
            }

            Lighting.AddLight(Projectile.Center, ClairDeLunePalette.PearlFrost.ToVector3() * 0.3f);
        }

        private void CheckFractureResonance()
        {
            List<int> nearbyFractures = new();
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile other = Main.projectile[i];
                if (!other.active || other.whoAmI == Projectile.whoAmI) continue;
                if (other.type != Projectile.type || other.owner != Projectile.owner) continue;

                float dist = Vector2.Distance(Projectile.Center, other.Center);
                if (dist < ResonanceCheckRadius)
                    nearbyFractures.Add(i);
            }

            if (nearbyFractures.Count >= 2)
            {
                SpawnResonanceBurst();

                foreach (int idx in nearbyFractures)
                {
                    Vector2 otherPos = Main.projectile[idx].Center;
                    int arcPoints = 6;
                    for (int p = 0; p < arcPoints; p++)
                    {
                        float t = (float)p / arcPoints;
                        Vector2 arcPos = Vector2.Lerp(Projectile.Center, otherPos, t);
                        arcPos += Main.rand.NextVector2Circular(4f, 4f);
                        var arc = new GenericGlowParticle(arcPos,
                            (otherPos - Projectile.Center).SafeNormalize(Vector2.UnitX) * 2f,
                            ClairDeLunePalette.ClockworkBrass with { A = 0 } * 0.4f, 0.08f, 8, true);
                        MagnumParticleHandler.SpawnParticle(arc);
                    }
                }
            }
        }

        private void SpawnResonanceBurst()
        {
            SoundEngine.PlaySound(SoundID.Item14 with { Pitch = 0.3f, Volume = 0.5f }, Projectile.Center);

            for (int ring = 0; ring < 4; ring++)
            {
                Color ringColor = Color.Lerp(ClairDeLunePalette.NightMist, ClairDeLunePalette.ClockworkBrass, ring / 3f);
                int numPts = 16 + ring * 4;
                for (int p = 0; p < numPts; p++)
                {
                    float angle = MathHelper.TwoPi * p / numPts + ring * 0.2f;
                    Vector2 vel = angle.ToRotationVector2() * (3f + ring * 2f);
                    var ringDot = new GenericGlowParticle(Projectile.Center, vel,
                        ringColor with { A = 0 } * 0.5f, 0.08f, 10 + ring, true);
                    MagnumParticleHandler.SpawnParticle(ringDot);
                }
            }

            for (int n = 0; n < Main.maxNPCs; n++)
            {
                NPC npc = Main.npc[n];
                if (!npc.active || npc.friendly || npc.dontTakeDamage) continue;
                float dist = Vector2.Distance(Projectile.Center, npc.Center);
                if (dist < ResonanceCheckRadius)
                {
                    int resonanceDmg = (int)(Projectile.damage * 1.5f);
                    Player player = Main.player[Projectile.owner];
                    player.ApplyDamageToNPC(npc, resonanceDmg, 4f, (npc.Center - Projectile.Center).X > 0 ? 1 : -1, false);
                }
            }
        }

        private void LoadTextures()
        {
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

            float lifeProgress = (float)_timer / Duration;
            float fade = _timer > Duration - 15 ? (1f - (float)(_timer - (Duration - 15)) / 15f) : 1f;
            bool isReplaying = _timer >= ReplayFrame - 2 && _timer <= ReplayFrame + 5;

            DrawStarfallCrack(sb, matrix, fade, isReplaying);    // Pass 1: StarfallBolt frozen crack
            DrawPearlReplayBurst(sb, matrix, fade, isReplaying); // Pass 2: PearlShimmer replay overlay
            DrawBloomCrackLines(sb, matrix, fade, isReplaying);  // Pass 3: Bloom core + crack lines
            return false;
        }

        // ---- PASS 1: StarfallTrail.fx StarfallBolt frozen crack ----
        private void DrawStarfallCrack(SpriteBatch sb, Matrix matrix, float fade, bool isReplaying)
        {
            _starfallShader ??= ShaderLoader.StarfallTrail;
            if (_starfallShader == null) return;

            float intensity = isReplaying ? 2f : 0.8f;

            sb.End();

            _starfallShader.Parameters["uColor"]?.SetValue(ClairDeLunePalette.NightMist.ToVector4());
            _starfallShader.Parameters["uSecondaryColor"]?.SetValue(ClairDeLunePalette.PearlFrost.ToVector4());
            _starfallShader.Parameters["uOpacity"]?.SetValue(0.5f * fade);
            _starfallShader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
            _starfallShader.Parameters["uIntensity"]?.SetValue(intensity);
            _starfallShader.Parameters["uOverbrightMult"]?.SetValue(1.1f);
            _starfallShader.Parameters["uScrollSpeed"]?.SetValue(0.3f);
            _starfallShader.Parameters["uDistortionAmt"]?.SetValue(0.02f);
            _starfallShader.Parameters["uHasSecondaryTex"]?.SetValue(false);

            _starfallShader.CurrentTechnique = _starfallShader.Techniques["StarfallBolt"];

            sb.Begin(SpriteSortMode.Immediate, BlendState.Additive,
                SamplerState.LinearWrap, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, _starfallShader, matrix);

            Texture2D sc = _softCircle.Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float crackScale = (isReplaying ? 40f : 28f) / sc.Width;
            sb.Draw(sc, drawPos, null, Color.White, 0f, sc.Size() * 0.5f, crackScale, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                SamplerState.LinearClamp, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null, matrix);
        }

        // ---- PASS 2: PearlShimmer replay burst overlay ----
        private void DrawPearlReplayBurst(SpriteBatch sb, Matrix matrix, float fade, bool isReplaying)
        {
            if (!isReplaying) return;

            _pearlGlowShader ??= ShaderLoader.ClairDeLunePearlGlow;
            if (_pearlGlowShader == null) return;

            sb.End();

            _pearlGlowShader.Parameters["uColor"]?.SetValue(ClairDeLunePalette.PearlFrost.ToVector4());
            _pearlGlowShader.Parameters["uSecondaryColor"]?.SetValue(ClairDeLunePalette.PearlWhite.ToVector4());
            _pearlGlowShader.Parameters["uOpacity"]?.SetValue(0.6f * fade);
            _pearlGlowShader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
            _pearlGlowShader.Parameters["uIntensity"]?.SetValue(2f);
            _pearlGlowShader.Parameters["uOverbrightMult"]?.SetValue(1.3f);
            _pearlGlowShader.Parameters["uScrollSpeed"]?.SetValue(3f);
            _pearlGlowShader.Parameters["uDistortionAmt"]?.SetValue(0.03f);
            _pearlGlowShader.Parameters["uHasSecondaryTex"]?.SetValue(false);

            _pearlGlowShader.CurrentTechnique = _pearlGlowShader.Techniques["PearlShimmer"];

            sb.Begin(SpriteSortMode.Immediate, BlendState.Additive,
                SamplerState.LinearWrap, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, _pearlGlowShader, matrix);

            Texture2D sc = _softCircle.Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            sb.Draw(sc, drawPos, null, Color.White, 0f, sc.Size() * 0.5f, 50f / sc.Width, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                SamplerState.LinearClamp, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null, matrix);
        }

        // ---- PASS 3: Multi-scale bloom + crack lines ----
        private void DrawBloomCrackLines(SpriteBatch sb, Matrix matrix, float fade, bool isReplaying)
        {
            Vector2 pos = Projectile.Center - Main.screenPosition;
            Texture2D srb = _softRadialBloom.Value;
            Texture2D sf = _starFlare.Value;
            float intensity = isReplaying ? 3f : 1.2f;
            float scale = isReplaying ? 0.6f : 0.35f;

            // NightMist outer
            sb.Draw(srb, pos, null,
                ClairDeLunePalette.NightMist with { A = 0 } * 0.2f * fade, 0f, srb.Size() * 0.5f,
                scale * 60f / srb.Width, SpriteEffects.None, 0f);

            // PearlFrost mid
            sb.Draw(srb, pos, null,
                ClairDeLunePalette.PearlFrost with { A = 0 } * 0.4f * fade * (intensity / 3f), 0f, srb.Size() * 0.5f,
                scale * 30f / srb.Width, SpriteEffects.None, 0f);

            // WhiteHot core
            sb.Draw(srb, pos, null,
                ClairDeLunePalette.PearlWhite with { A = 0 } * 0.3f * fade, 0f, srb.Size() * 0.5f,
                scale * 16f / srb.Width, SpriteEffects.None, 0f);

            // Crack lines — radial bloom dots
            float crackPulse = 0.5f + 0.5f * MathF.Sin(_timer * 0.15f);
            for (int i = 0; i < 4; i++)
            {
                float angle = (MathHelper.TwoPi * i / 4f) + crackPulse * 0.3f;
                Vector2 crackEnd = pos + angle.ToRotationVector2() * 18f * fade;
                sb.Draw(srb, crackEnd, null,
                    ClairDeLunePalette.PearlFrost with { A = 0 } * 0.25f * fade, 0f, srb.Size() * 0.5f,
                    4f / srb.Width, SpriteEffects.None, 0f);
            }

            // Star flare on replay
            if (isReplaying)
            {
                float flareRot = Main.GlobalTimeWrappedHourly * 5f;
                sb.Draw(sf, pos, null,
                    ClairDeLunePalette.PearlFrost with { A = 0 } * 0.3f * fade,
                    flareRot, sf.Size() * 0.5f, 20f / sf.Width, SpriteEffects.None, 0f);
            }

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null, matrix);
        }
    }
}
