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

namespace MagnumOpus.Content.ClairDeLune.Weapons.ClockworkGrimoire.Projectiles
{
    /// <summary>
    /// Minute Orb — One of 12 ticking orbs from ClockworkGrimoire's minute hand sweep.
    /// VoronoiCell-style countdown with 3-tick detonation and 4-ring burst.
    /// 3 render passes: (1) ClairDeLuneMoonlit MoonlitGlow ticking body,
    /// (2) ClairDeLunePearlGlow PearlShimmer tick-pulse overlay, (3) Multi-scale bloom + Voronoi facets.
    /// </summary>
    public class MinuteOrbProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

        private int _tickCount;
        private int _timer;
        private const int TickInterval = 25;
        private const int TotalTicks = 3;
        private const float DetonationRadius = 48f;

        // --- Shader + texture caching ---
        private static Effect _moonlitShader;
        private static Effect _pearlGlowShader;
        private static Asset<Texture2D> _softCircle;
        private static Asset<Texture2D> _softRadialBloom;
        private static Asset<Texture2D> _pointBloom;
        private static Asset<Texture2D> _starFlare;

        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 100;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 100;
        }

        public override void AI()
        {
            _timer++;

            // Slow to a stop
            Projectile.velocity *= 0.97f;
            Projectile.rotation += 0.05f;

            // Tick countdown
            if (_timer % TickInterval == 0 && _tickCount < TotalTicks)
            {
                _tickCount++;
                SoundEngine.PlaySound(SoundID.Item11 with { Pitch = 0.3f + _tickCount * 0.15f, Volume = 0.3f },
                    Projectile.Center);

                // Tick burst particles
                Color tickCol = Color.Lerp(ClairDeLunePalette.SoftBlue, ClairDeLunePalette.MoonbeamGold,
                    _tickCount / (float)TotalTicks);
                for (int i = 0; i < 6; i++)
                {
                    float angle = MathHelper.TwoPi * i / 6f;
                    Vector2 vel = angle.ToRotationVector2() * (1.5f + _tickCount * 0.5f);
                    var spark = new GenericGlowParticle(Projectile.Center, vel,
                        tickCol with { A = 0 } * 0.4f, 0.04f, 6, true);
                    MagnumParticleHandler.SpawnParticle(spark);
                }
            }

            // Detonate after final tick
            if (_tickCount >= TotalTicks && _timer >= TotalTicks * TickInterval + 4)
            {
                Detonate();
                Projectile.Kill();
                return;
            }

            if (Projectile.velocity.Length() < 0.3f)
                Projectile.velocity = Vector2.Zero;

            float tickProgress = (float)_tickCount / TotalTicks;
            Lighting.AddLight(Projectile.Center, ClairDeLunePalette.SoftBlue.ToVector3() * (0.2f + tickProgress * 0.3f));
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Projectile.velocity = Vector2.Zero;
            return false;
        }

        private void Detonate()
        {
            SoundEngine.PlaySound(SoundID.Item14 with { Pitch = 0.4f, Volume = 0.5f }, Projectile.Center);

            // 4-ring detonation burst
            for (int ring = 0; ring < 4; ring++)
            {
                float ringProgress = ring / 3f;
                Color ringCol = Color.Lerp(ClairDeLunePalette.MoonbeamGold, ClairDeLunePalette.PearlFrost, ringProgress);
                int points = 10 + ring * 2;
                for (int p = 0; p < points; p++)
                {
                    float angle = MathHelper.TwoPi * p / points + ring * 0.15f;
                    Vector2 vel = angle.ToRotationVector2() * (1.5f + ring * 1.2f);
                    var dot = new GenericGlowParticle(Projectile.Center, vel,
                        ringCol with { A = 0 } * (0.4f - ring * 0.07f), 0.05f, 8 + ring, true);
                    MagnumParticleHandler.SpawnParticle(dot);
                }
            }

            var flash = new BloomParticle(Projectile.Center, Vector2.Zero,
                ClairDeLunePalette.MoonbeamGold with { A = 0 } * 0.5f, 0.3f, 8);
            MagnumParticleHandler.SpawnParticle(flash);

            // AoE damage
            for (int n = 0; n < Main.maxNPCs; n++)
            {
                NPC npc = Main.npc[n];
                if (!npc.active || npc.friendly || npc.dontTakeDamage) continue;
                if (Vector2.Distance(Projectile.Center, npc.Center) < DetonationRadius)
                {
                    Player player = Main.player[Projectile.owner];
                    player.ApplyDamageToNPC(npc, Projectile.damage, Projectile.knockBack,
                        (npc.Center - Projectile.Center).X > 0 ? 1 : -1, false);
                }
            }
        }

        private void LoadTextures()
        {
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

            DrawMoonlitBody(sb, matrix);      // Pass 1: MoonlitGlow ticking body
            DrawTickPulse(sb, matrix);         // Pass 2: PearlShimmer tick-pulse overlay
            DrawBloomVoronoi(sb, matrix);      // Pass 3: Multi-scale bloom + Voronoi facets
            return false;
        }

        // ---- PASS 1: ClairDeLuneMoonlit MoonlitGlow ticking body ----
        private void DrawMoonlitBody(SpriteBatch sb, Matrix matrix)
        {
            _moonlitShader ??= ShaderLoader.ClairDeLuneMoonlit;
            if (_moonlitShader == null) return;

            float tickProgress = (float)_tickCount / TotalTicks;
            float tickPulse = _timer % TickInterval < 6 ? 1.25f : 1f;

            sb.End();

            _moonlitShader.Parameters["uColor"]?.SetValue(
                Color.Lerp(ClairDeLunePalette.SoftBlue, ClairDeLunePalette.MoonbeamGold, tickProgress).ToVector4());
            _moonlitShader.Parameters["uSecondaryColor"]?.SetValue(ClairDeLunePalette.NightMist.ToVector4());
            _moonlitShader.Parameters["uOpacity"]?.SetValue(0.5f + tickProgress * 0.3f);
            _moonlitShader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
            _moonlitShader.Parameters["uIntensity"]?.SetValue(1f + tickProgress * 0.5f);
            _moonlitShader.Parameters["uOverbrightMult"]?.SetValue(1.0f);
            _moonlitShader.Parameters["uScrollSpeed"]?.SetValue(1.5f + tickProgress * 2f);
            _moonlitShader.Parameters["uDistortionAmt"]?.SetValue(0.015f);
            _moonlitShader.Parameters["uHasSecondaryTex"]?.SetValue(false);

            _moonlitShader.CurrentTechnique = _moonlitShader.Techniques["MoonlitGlow"];

            sb.Begin(SpriteSortMode.Immediate, BlendState.Additive,
                SamplerState.LinearWrap, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, _moonlitShader, matrix);

            Texture2D sc = _softCircle.Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float bodyScale = 16f * tickPulse / sc.Width;
            sb.Draw(sc, drawPos, null, Color.White, Projectile.rotation, sc.Size() * 0.5f, bodyScale, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                SamplerState.LinearClamp, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null, matrix);
        }

        // ---- PASS 2: PearlShimmer tick-pulse overlay ----
        private void DrawTickPulse(SpriteBatch sb, Matrix matrix)
        {
            if (_tickCount < 1) return;

            _pearlGlowShader ??= ShaderLoader.ClairDeLunePearlGlow;
            if (_pearlGlowShader == null) return;

            float tickProgress = (float)_tickCount / TotalTicks;
            float tickPulse = _timer % TickInterval < 6 ? 1.25f : 1f;

            sb.End();

            _pearlGlowShader.Parameters["uColor"]?.SetValue(ClairDeLunePalette.MoonbeamGold.ToVector4());
            _pearlGlowShader.Parameters["uSecondaryColor"]?.SetValue(ClairDeLunePalette.PearlFrost.ToVector4());
            _pearlGlowShader.Parameters["uOpacity"]?.SetValue(0.25f * tickProgress * tickPulse);
            _pearlGlowShader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
            _pearlGlowShader.Parameters["uIntensity"]?.SetValue(0.8f + tickProgress);
            _pearlGlowShader.Parameters["uOverbrightMult"]?.SetValue(1.1f);
            _pearlGlowShader.Parameters["uScrollSpeed"]?.SetValue(1.5f);
            _pearlGlowShader.Parameters["uDistortionAmt"]?.SetValue(0.01f);
            _pearlGlowShader.Parameters["uHasSecondaryTex"]?.SetValue(false);

            _pearlGlowShader.CurrentTechnique = _pearlGlowShader.Techniques["PearlShimmer"];

            sb.Begin(SpriteSortMode.Immediate, BlendState.Additive,
                SamplerState.LinearWrap, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, _pearlGlowShader, matrix);

            Texture2D sc = _softCircle.Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float overlayScale = 22f * tickPulse / sc.Width;
            sb.Draw(sc, drawPos, null, Color.White, 0f, sc.Size() * 0.5f, overlayScale, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                SamplerState.LinearClamp, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null, matrix);
        }

        // ---- PASS 3: Multi-scale bloom + Voronoi facets ----
        private void DrawBloomVoronoi(SpriteBatch sb, Matrix matrix)
        {
            Vector2 pos = Projectile.Center - Main.screenPosition;
            Texture2D srb = _softRadialBloom.Value;
            Texture2D pb = _pointBloom.Value;
            Texture2D sf = _starFlare.Value;

            float tickProgress = (float)_tickCount / TotalTicks;
            float tickPulse = _timer % TickInterval < 6 ? 1.25f : 1f;

            // Outer ambient glow
            sb.Draw(srb, pos, null,
                ClairDeLunePalette.NightMist with { A = 0 } * 0.12f, 0f, srb.Size() * 0.5f,
                20f * tickPulse / srb.Width, SpriteEffects.None, 0f);

            // Mid SoftBlue glow
            sb.Draw(srb, pos, null,
                ClairDeLunePalette.SoftBlue with { A = 0 } * (0.2f + tickProgress * 0.2f), 0f, srb.Size() * 0.5f,
                12f * tickPulse / srb.Width, SpriteEffects.None, 0f);

            // Core gold glow
            sb.Draw(pb, pos, null,
                ClairDeLunePalette.MoonbeamGold with { A = 0 } * (0.15f + tickProgress * 0.25f), 0f, pb.Size() * 0.5f,
                8f / pb.Width, SpriteEffects.None, 0f);

            // Voronoi facet dots — 6 orbiting points for VoronoiCell look
            float facetAngleBase = Main.GlobalTimeWrappedHourly * 2f + Projectile.whoAmI * 0.5f;
            int facetCount = 6;
            for (int i = 0; i < facetCount; i++)
            {
                float angle = facetAngleBase + i * MathHelper.TwoPi / facetCount;
                float dist = 6f + 2f * MathF.Sin(Main.GlobalTimeWrappedHourly * 3f + i);
                Vector2 facetPos = pos + angle.ToRotationVector2() * dist;
                Color facetCol = Color.Lerp(ClairDeLunePalette.SoftBlue, ClairDeLunePalette.MoonbeamGold, (float)i / facetCount);

                sb.Draw(pb, facetPos, null,
                    facetCol with { A = 0 } * 0.12f, 0f, pb.Size() * 0.5f,
                    3f / pb.Width, SpriteEffects.None, 0f);
            }

            // Star flare at max tick
            if (_tickCount >= 2)
            {
                float flareRot = Main.GlobalTimeWrappedHourly * 3f;
                sb.Draw(sf, pos, null,
                    ClairDeLunePalette.MoonbeamGold with { A = 0 } * 0.15f * tickProgress,
                    flareRot, sf.Size() * 0.5f, 10f / sf.Width, SpriteEffects.None, 0f);
            }

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null, matrix);
        }
    }
}
