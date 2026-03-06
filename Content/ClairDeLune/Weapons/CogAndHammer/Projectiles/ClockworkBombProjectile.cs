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

namespace MagnumOpus.Content.ClairDeLune.Weapons.CogAndHammer.Projectiles
{
    /// <summary>
    /// Clockwork Bomb — lobbed arcing explosive with 3-tick countdown.
    /// 3 render passes: (1) SingularityPull.fx SingularityCore bomb body,
    /// (2) ClairDeLunePearlGlow.fx PearlShimmer tick-pulse overlay, (3) Multi-scale bloom core.
    /// </summary>
    public class ClockworkBombProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

        private int _tickCount;
        private int _timer;
        private const int TickInterval = 30;
        private const int TotalTicks = 3;
        private const float DetonationRadius = 64f;
        private const float ChainCheckRadius = 96f;

        // --- Shader + texture caching ---
        private static Effect _singularityShader;
        private static Effect _pearlGlowShader;
        private static Asset<Texture2D> _softCircle;
        private static Asset<Texture2D> _softRadialBloom;
        private static Asset<Texture2D> _starFlare;

        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.friendly = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = true;
        }

        public override void AI()
        {
            _timer++;
            Projectile.rotation += 0.08f;
            Projectile.velocity.Y += 0.12f;
            Projectile.velocity *= 0.995f;

            if (_timer % TickInterval == 0 && _tickCount < TotalTicks)
            {
                _tickCount++;
                SoundEngine.PlaySound(SoundID.Item11 with { Pitch = 0.2f + _tickCount * 0.2f, Volume = 0.4f }, Projectile.Center);

                float pulseScale = 0.15f + _tickCount * 0.1f;
                Color tickCol = Color.Lerp(ClairDeLunePalette.ClockworkBrass, ClairDeLunePalette.MoonbeamGold, _tickCount / 3f);
                var tickPulse = new BloomParticle(Projectile.Center, Vector2.Zero,
                    tickCol with { A = 0 } * 0.5f, pulseScale, 8);
                MagnumParticleHandler.SpawnParticle(tickPulse);

                for (int i = 0; i < 6; i++)
                {
                    float angle = MathHelper.TwoPi * i / 6f;
                    Vector2 vel = angle.ToRotationVector2() * 2f;
                    var spark = new GenericGlowParticle(Projectile.Center, vel,
                        tickCol with { A = 0 } * 0.3f, 0.05f, 6, true);
                    MagnumParticleHandler.SpawnParticle(spark);
                }
            }

            if (_tickCount >= TotalTicks && _timer >= TotalTicks * TickInterval + 5)
            {
                Detonate();
                Projectile.Kill();
                return;
            }

            if (Projectile.velocity.Length() < 0.5f)
                Projectile.velocity = Vector2.Zero;

            if (Main.rand.NextBool(3))
            {
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.Enchanted_Gold,
                    Main.rand.NextVector2Circular(1f, 1f),
                    0, ClairDeLunePalette.ClockworkBrass, 0.5f);
                d.noGravity = true;
            }

            float tickProgress = (float)_tickCount / TotalTicks;
            Lighting.AddLight(Projectile.Center, ClairDeLunePalette.MoonbeamGold.ToVector3() * (0.3f + tickProgress * 0.4f));
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Projectile.velocity = Vector2.Zero;
            return false;
        }

        protected virtual void Detonate()
        {
            SoundEngine.PlaySound(SoundID.Item14 with { Pitch = 0.1f, Volume = 0.8f }, Projectile.Center);

            for (int ring = 0; ring < 6; ring++)
            {
                float ringProgress = ring / 5f;
                Color ringCol = Color.Lerp(ClairDeLunePalette.MoonbeamGold, ClairDeLunePalette.PearlFrost, ringProgress);
                int points = 14 + ring * 2;
                for (int p = 0; p < points; p++)
                {
                    float angle = MathHelper.TwoPi * p / points + ring * 0.1f;
                    Vector2 vel = angle.ToRotationVector2() * (2f + ring * 1.5f);
                    var dot = new GenericGlowParticle(Projectile.Center, vel,
                        ringCol with { A = 0 } * (0.5f - ring * 0.06f), 0.06f, 10 + ring, true);
                    MagnumParticleHandler.SpawnParticle(dot);
                }
            }

            var flash = new BloomParticle(Projectile.Center, Vector2.Zero,
                ClairDeLunePalette.MoonbeamGold with { A = 0 } * 0.6f, 0.5f, 10);
            MagnumParticleHandler.SpawnParticle(flash);

            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f + Main.rand.NextFloat(-0.2f, 0.2f);
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(5f, 10f);
                Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, vel,
                    ModContent.ProjectileType<GearShrapnelProjectile>(),
                    (int)(Projectile.damage * 0.3f), 2f, Projectile.owner);
            }

            for (int n = 0; n < Main.maxNPCs; n++)
            {
                NPC npc = Main.npc[n];
                if (!npc.active || npc.friendly || npc.dontTakeDamage) continue;
                if (Vector2.Distance(Projectile.Center, npc.Center) < DetonationRadius)
                {
                    Player player = Main.player[Projectile.owner];
                    player.ApplyDamageToNPC(npc, Projectile.damage, 8f, (npc.Center - Projectile.Center).X > 0 ? 1 : -1, false);
                }
            }

            CheckChainDetonation();
        }

        private void CheckChainDetonation()
        {
            List<int> nearbyBombs = new();
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile other = Main.projectile[i];
                if (!other.active || other.whoAmI == Projectile.whoAmI) continue;
                if (other.owner != Projectile.owner) continue;
                if (other.type != Projectile.type &&
                    other.type != ModContent.ProjectileType<StickyBombProjectile>() &&
                    other.type != ModContent.ProjectileType<MasterMechanismBombProjectile>()) continue;

                float dist = Vector2.Distance(Projectile.Center, other.Center);
                if (dist < ChainCheckRadius)
                    nearbyBombs.Add(i);
            }

            if (nearbyBombs.Count >= 2)
            {
                int delay = 0;
                foreach (int idx in nearbyBombs)
                {
                    Main.projectile[idx].timeLeft = Math.Min(Main.projectile[idx].timeLeft, 5 + delay);
                    delay += 12;

                    Vector2 otherPos = Main.projectile[idx].Center;
                    int arcPts = 5;
                    for (int p = 0; p < arcPts; p++)
                    {
                        float t = (float)p / arcPts;
                        Vector2 arcPos = Vector2.Lerp(Projectile.Center, otherPos, t);
                        arcPos += Main.rand.NextVector2Circular(3f, 3f);
                        var arc = new GenericGlowParticle(arcPos,
                            (otherPos - Projectile.Center).SafeNormalize(Vector2.UnitX) * 2f,
                            ClairDeLunePalette.MoonbeamGold with { A = 0 } * 0.5f, 0.08f, 8, true);
                        MagnumParticleHandler.SpawnParticle(arc);
                    }
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

            DrawSingularityBody(sb, matrix);     // Pass 1: SingularityCore shader on bomb body
            DrawTickPulseOverlay(sb, matrix);    // Pass 2: PearlShimmer tick-pulse overlay
            DrawBloomCore(sb, matrix);           // Pass 3: Multi-scale bloom core
            ClairDeLuneVFXLibrary.DrawThemeAccents(sb, Projectile.Center, 0.5f, 0.3f);
            return false;
        }

        // ---- PASS 1: SingularityPull.fx SingularityCore bomb body ----
        private void DrawSingularityBody(SpriteBatch sb, Matrix matrix)
        {
            _singularityShader ??= ShaderLoader.SingularityPull;
            if (_singularityShader == null) return;

            float tickProgress = (float)_tickCount / TotalTicks;
            float tickPulse = _timer % TickInterval < 8 ? 1.3f : 1f;

            sb.End();

            _singularityShader.Parameters["uColor"]?.SetValue(ClairDeLunePalette.ClockworkBrass.ToVector4());
            _singularityShader.Parameters["uSecondaryColor"]?.SetValue(ClairDeLunePalette.MoonbeamGold.ToVector4());
            _singularityShader.Parameters["uOpacity"]?.SetValue(0.5f + tickProgress * 0.3f);
            _singularityShader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
            _singularityShader.Parameters["uIntensity"]?.SetValue(1f + tickProgress * 0.8f);
            _singularityShader.Parameters["uOverbrightMult"]?.SetValue(1.1f);
            _singularityShader.Parameters["uScrollSpeed"]?.SetValue(2f + tickProgress * 3f);
            _singularityShader.Parameters["uDistortionAmt"]?.SetValue(0.02f);
            _singularityShader.Parameters["uHasSecondaryTex"]?.SetValue(false);

            _singularityShader.CurrentTechnique = _singularityShader.Techniques["SingularityCore"];

            sb.Begin(SpriteSortMode.Immediate, MagnumBlendStates.ShaderAdditive,
                SamplerState.LinearWrap, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, _singularityShader, matrix);

            Texture2D sc = _softCircle.Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float bodyScale = 28f * tickPulse / sc.Width;
            sb.Draw(sc, drawPos, null, Color.White, Projectile.rotation, sc.Size() * 0.5f, bodyScale, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                SamplerState.LinearClamp, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null, matrix);
        }

        // ---- PASS 2: PearlShimmer tick-pulse overlay ----
        private void DrawTickPulseOverlay(SpriteBatch sb, Matrix matrix)
        {
            if (_tickCount < 1) return;

            _pearlGlowShader ??= ShaderLoader.ClairDeLunePearlGlow;
            if (_pearlGlowShader == null) return;

            float tickProgress = (float)_tickCount / TotalTicks;
            float tickPulse = _timer % TickInterval < 8 ? 1.3f : 1f;

            sb.End();

            _pearlGlowShader.Parameters["uColor"]?.SetValue(ClairDeLunePalette.MoonbeamGold.ToVector4());
            _pearlGlowShader.Parameters["uSecondaryColor"]?.SetValue(ClairDeLunePalette.PearlFrost.ToVector4());
            _pearlGlowShader.Parameters["uOpacity"]?.SetValue(0.3f * tickProgress * tickPulse);
            _pearlGlowShader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
            _pearlGlowShader.Parameters["uIntensity"]?.SetValue(1f + tickProgress);
            _pearlGlowShader.Parameters["uOverbrightMult"]?.SetValue(1.1f);
            _pearlGlowShader.Parameters["uScrollSpeed"]?.SetValue(1.5f);
            _pearlGlowShader.Parameters["uDistortionAmt"]?.SetValue(0.01f);
            _pearlGlowShader.Parameters["uHasSecondaryTex"]?.SetValue(false);

            _pearlGlowShader.CurrentTechnique = _pearlGlowShader.Techniques["PearlShimmer"];

            sb.Begin(SpriteSortMode.Immediate, MagnumBlendStates.ShaderAdditive,
                SamplerState.LinearWrap, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, _pearlGlowShader, matrix);

            Texture2D sc = _softCircle.Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float overlayScale = 36f * tickPulse / sc.Width;
            sb.Draw(sc, drawPos, null, Color.White, 0f, sc.Size() * 0.5f, overlayScale, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                SamplerState.LinearClamp, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null, matrix);
        }

        // ---- PASS 3: Multi-scale bloom core ----
        private void DrawBloomCore(SpriteBatch sb, Matrix matrix)
        {
            Vector2 pos = Projectile.Center - Main.screenPosition;
            Texture2D srb = _softRadialBloom.Value;
            Texture2D sf = _starFlare.Value;
            float tickProgress = (float)_tickCount / TotalTicks;
            float tickPulse = _timer % TickInterval < 8 ? 1.3f : 1f;
            float intensity = 0.3f + tickProgress * 0.4f;

            // NightMist outer
            sb.Draw(srb, pos, null,
                ClairDeLunePalette.NightMist with { A = 0 } * 0.15f, 0f, srb.Size() * 0.5f,
                24f * tickPulse / srb.Width, SpriteEffects.None, 0f);

            // Gold core
            sb.Draw(srb, pos, null,
                ClairDeLunePalette.MoonbeamGold with { A = 0 } * intensity, 0f, srb.Size() * 0.5f,
                14f * tickPulse / srb.Width, SpriteEffects.None, 0f);

            // Pearl frost inner
            sb.Draw(srb, pos, null,
                ClairDeLunePalette.PearlFrost with { A = 0 } * intensity * 0.5f, 0f, srb.Size() * 0.5f,
                8f * tickPulse / srb.Width, SpriteEffects.None, 0f);

            // Star flare at max tick
            if (_tickCount >= 2)
            {
                float flareRot = Main.GlobalTimeWrappedHourly * 3f;
                sb.Draw(sf, pos, null,
                    ClairDeLunePalette.MoonbeamGold with { A = 0 } * 0.2f * tickProgress,
                    flareRot, sf.Size() * 0.5f, 12f / sf.Width, SpriteEffects.None, 0f);
            }

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null, matrix);
        }
    }
}
