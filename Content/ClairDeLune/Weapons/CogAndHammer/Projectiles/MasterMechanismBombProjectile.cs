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

namespace MagnumOpus.Content.ClairDeLune.Weapons.CogAndHammer.Projectiles
{
    /// <summary>
    /// Master Mechanism Bomb — every 8th shot, 2x explosion radius,
    /// spawns 4 regular ClockworkBombs on detonation, 10-ring detonation + 16 shrapnel.
    /// 3 render passes: (1) SingularityPull.fx SingularityVortex mega body,
    /// (2) ClairDeLunePearlGlow.fx PearlBloom tick overlay, (3) Massive bloom core.
    /// </summary>
    public class MasterMechanismBombProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

        private int _tickCount;
        private int _timer;
        private const int TickInterval = 30;
        private const int TotalTicks = 3;
        private const float DetonationRadius = 128f;

        // --- Shader + texture caching ---
        private static Effect _singularityShader;
        private static Effect _pearlGlowShader;
        private static Asset<Texture2D> _softCircle;
        private static Asset<Texture2D> _softRadialBloom;
        private static Asset<Texture2D> _starFlare;

        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.friendly = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = true;
        }

        public override void AI()
        {
            _timer++;
            Projectile.rotation += 0.05f;
            Projectile.velocity.Y += 0.15f;
            Projectile.velocity *= 0.993f;

            if (_timer % TickInterval == 0 && _tickCount < TotalTicks)
            {
                _tickCount++;
                SoundEngine.PlaySound(SoundID.Item14 with { Pitch = -0.2f + _tickCount * 0.15f, Volume = 0.5f }, Projectile.Center);

                float pulseScale = 0.25f + _tickCount * 0.15f;
                Color tickCol = Color.Lerp(ClairDeLunePalette.MoonbeamGold, ClairDeLunePalette.PearlWhite, _tickCount / 3f);
                var pulse = new BloomParticle(Projectile.Center, Vector2.Zero,
                    tickCol with { A = 0 } * 0.6f, pulseScale, 10);
                MagnumParticleHandler.SpawnParticle(pulse);

                for (int i = 0; i < 10; i++)
                {
                    float angle = MathHelper.TwoPi * i / 10f;
                    Vector2 vel = angle.ToRotationVector2() * 3f;
                    var spark = new GenericGlowParticle(Projectile.Center, vel,
                        tickCol with { A = 0 } * 0.4f, 0.07f, 8, true);
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

            if (Main.rand.NextBool(2))
            {
                Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(6f, 6f),
                    DustID.GoldFlame, Main.rand.NextVector2Circular(1.5f, 1.5f),
                    0, ClairDeLunePalette.MoonbeamGold, 0.8f);
                d.noGravity = true;
            }

            float tickProgress = (float)_tickCount / TotalTicks;
            Lighting.AddLight(Projectile.Center, ClairDeLunePalette.MoonbeamGold.ToVector3() * (0.5f + tickProgress * 0.5f));
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Projectile.velocity = Vector2.Zero;
            return false;
        }

        private void Detonate()
        {
            SoundEngine.PlaySound(SoundID.Item14 with { Pitch = -0.5f, Volume = 1.2f }, Projectile.Center);

            for (int ring = 0; ring < 10; ring++)
            {
                float ringProgress = ring / 9f;
                Color ringCol = ring < 4
                    ? Color.Lerp(ClairDeLunePalette.MoonbeamGold, ClairDeLunePalette.PearlFrost, ringProgress)
                    : Color.Lerp(ClairDeLunePalette.PearlFrost, ClairDeLunePalette.PearlWhite, (ringProgress - 0.4f) / 0.6f);
                int points = 18 + ring * 3;
                for (int p = 0; p < points; p++)
                {
                    float angle = MathHelper.TwoPi * p / points + ring * 0.12f;
                    Vector2 vel = angle.ToRotationVector2() * (3f + ring * 2f);
                    var dot = new GenericGlowParticle(Projectile.Center, vel,
                        ringCol with { A = 0 } * (0.55f - ring * 0.04f), 0.07f, 12 + ring, true);
                    MagnumParticleHandler.SpawnParticle(dot);
                }
            }

            var flash = new BloomParticle(Projectile.Center, Vector2.Zero,
                ClairDeLunePalette.PearlWhite with { A = 0 } * 0.8f, 1.0f, 12);
            MagnumParticleHandler.SpawnParticle(flash);

            for (int i = 0; i < 16; i++)
            {
                float angle = MathHelper.TwoPi * i / 16f + Main.rand.NextFloat(-0.15f, 0.15f);
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(6f, 14f);
                Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, vel,
                    ModContent.ProjectileType<GearShrapnelProjectile>(),
                    (int)(Projectile.damage * 0.3f), 3f, Projectile.owner);
            }

            for (int i = 0; i < 4; i++)
            {
                float angle = MathHelper.PiOver2 * i + MathHelper.PiOver4;
                Vector2 vel = angle.ToRotationVector2() * 6f;
                Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, vel,
                    ModContent.ProjectileType<ClockworkBombProjectile>(),
                    (int)(Projectile.damage * 0.7f), 6f, Projectile.owner);
            }

            for (int n = 0; n < Main.maxNPCs; n++)
            {
                NPC npc = Main.npc[n];
                if (!npc.active || npc.friendly || npc.dontTakeDamage) continue;
                if (Vector2.Distance(Projectile.Center, npc.Center) < DetonationRadius)
                {
                    Player player = Main.player[Projectile.owner];
                    player.ApplyDamageToNPC(npc, (int)(Projectile.damage * 1.5f), 12f, (npc.Center - Projectile.Center).X > 0 ? 1 : -1, false);
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
            try
            {
            Matrix matrix = Main.GameViewMatrix.TransformationMatrix;

            DrawSingularityBody(sb, matrix);     // Pass 1: SingularityVortex mega body
            DrawPearlTickOverlay(sb, matrix);    // Pass 2: PearlBloom tick overlay
            DrawMassiveBloomCore(sb, matrix);    // Pass 3: Massive bloom
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

        // ---- PASS 1: SingularityPull.fx SingularityVortex mega body ----
        private void DrawSingularityBody(SpriteBatch sb, Matrix matrix)
        {
            _singularityShader ??= ShaderLoader.SingularityPull;
            if (_singularityShader == null) return;

            float tickProgress = (float)_tickCount / TotalTicks;
            float tickPulse = _timer % TickInterval < 10 ? 1.4f : 1f;

            sb.End();

            _singularityShader.Parameters["uColor"]?.SetValue(ClairDeLunePalette.MoonbeamGold.ToVector4());
            _singularityShader.Parameters["uSecondaryColor"]?.SetValue(ClairDeLunePalette.PearlWhite.ToVector4());
            _singularityShader.Parameters["uOpacity"]?.SetValue(0.6f + tickProgress * 0.3f);
            _singularityShader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
            _singularityShader.Parameters["uIntensity"]?.SetValue(1.2f + tickProgress);
            _singularityShader.Parameters["uOverbrightMult"]?.SetValue(1.2f);
            _singularityShader.Parameters["uScrollSpeed"]?.SetValue(3f + tickProgress * 4f);
            _singularityShader.Parameters["uDistortionAmt"]?.SetValue(0.03f);
            _singularityShader.Parameters["uHasSecondaryTex"]?.SetValue(false);

            _singularityShader.CurrentTechnique = _singularityShader.Techniques["SingularityVortex"];

            sb.Begin(SpriteSortMode.Immediate, MagnumBlendStates.ShaderAdditive,
                SamplerState.LinearWrap, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, _singularityShader, matrix);

            Texture2D sc = _softCircle.Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float bodyScale = 44f * tickPulse / sc.Width;
            sb.Draw(sc, drawPos, null, Color.White, Projectile.rotation, sc.Size() * 0.5f, bodyScale, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                SamplerState.LinearClamp, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null, matrix);
        }

        // ---- PASS 2: PearlBloom tick overlay ----
        private void DrawPearlTickOverlay(SpriteBatch sb, Matrix matrix)
        {
            if (_tickCount < 1) return;

            _pearlGlowShader ??= ShaderLoader.ClairDeLunePearlGlow;
            if (_pearlGlowShader == null) return;

            float tickProgress = (float)_tickCount / TotalTicks;
            float tickPulse = _timer % TickInterval < 10 ? 1.4f : 1f;

            sb.End();

            _pearlGlowShader.Parameters["uColor"]?.SetValue(ClairDeLunePalette.PearlWhite.ToVector4());
            _pearlGlowShader.Parameters["uSecondaryColor"]?.SetValue(ClairDeLunePalette.MoonbeamGold.ToVector4());
            _pearlGlowShader.Parameters["uOpacity"]?.SetValue(0.35f * tickProgress * tickPulse);
            _pearlGlowShader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
            _pearlGlowShader.Parameters["uIntensity"]?.SetValue(1.2f + tickProgress);
            _pearlGlowShader.Parameters["uOverbrightMult"]?.SetValue(1.1f);
            _pearlGlowShader.Parameters["uScrollSpeed"]?.SetValue(2f);
            _pearlGlowShader.Parameters["uDistortionAmt"]?.SetValue(0.01f);
            _pearlGlowShader.Parameters["uHasSecondaryTex"]?.SetValue(false);

            _pearlGlowShader.CurrentTechnique = _pearlGlowShader.Techniques["PearlBloom"];

            sb.Begin(SpriteSortMode.Immediate, MagnumBlendStates.ShaderAdditive,
                SamplerState.LinearWrap, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, _pearlGlowShader, matrix);

            Texture2D sc = _softCircle.Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float overlayScale = 52f * tickPulse / sc.Width;
            sb.Draw(sc, drawPos, null, Color.White, 0f, sc.Size() * 0.5f, overlayScale, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                SamplerState.LinearClamp, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null, matrix);
        }

        // ---- PASS 3: Massive bloom core ----
        private void DrawMassiveBloomCore(SpriteBatch sb, Matrix matrix)
        {
            Vector2 pos = Projectile.Center - Main.screenPosition;
            Texture2D srb = _softRadialBloom.Value;
            Texture2D sf = _starFlare.Value;
            float tickProgress = (float)_tickCount / TotalTicks;
            float tickPulse = _timer % TickInterval < 10 ? 1.4f : 1f;
            float intensity = 0.4f + tickProgress * 0.5f;

            // 12 gear teeth blooms
            for (int t = 0; t < 12; t++)
            {
                float angle = Projectile.rotation + MathHelper.TwoPi * t / 12f;
                Vector2 toothPos = pos + angle.ToRotationVector2() * 20f;
                sb.Draw(srb, toothPos, null,
                    ClairDeLunePalette.ClockworkBrass with { A = 0 } * 0.25f, 0f, srb.Size() * 0.5f,
                    6f * tickPulse / srb.Width, SpriteEffects.None, 0f);
            }

            // NightMist outer rim
            sb.Draw(srb, pos, null,
                ClairDeLunePalette.NightMist with { A = 0 } * 0.12f, 0f, srb.Size() * 0.5f,
                36f / srb.Width, SpriteEffects.None, 0f);

            // Gold core
            sb.Draw(srb, pos, null,
                ClairDeLunePalette.MoonbeamGold with { A = 0 } * intensity * 0.6f, 0f, srb.Size() * 0.5f,
                22f * tickPulse / srb.Width, SpriteEffects.None, 0f);

            // PearlFrost inner
            sb.Draw(srb, pos, null,
                ClairDeLunePalette.PearlFrost with { A = 0 } * intensity * 0.4f, 0f, srb.Size() * 0.5f,
                14f * tickPulse / srb.Width, SpriteEffects.None, 0f);

            // PearlWhite hot core
            sb.Draw(srb, pos, null,
                ClairDeLunePalette.PearlWhite with { A = 0 } * intensity * 0.3f, 0f, srb.Size() * 0.5f,
                8f * tickPulse / srb.Width, SpriteEffects.None, 0f);

            // Star flare at tick 2+
            if (_tickCount >= 2)
            {
                float flareRot = Main.GlobalTimeWrappedHourly * 2f;
                sb.Draw(sf, pos, null,
                    ClairDeLunePalette.MoonbeamGold with { A = 0 } * 0.25f * tickProgress,
                    flareRot, sf.Size() * 0.5f, 18f / sf.Width, SpriteEffects.None, 0f);
            }

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null, matrix);
        }
    }
}
