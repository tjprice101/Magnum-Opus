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
using MagnumOpus.Common.Systems.VFX;

namespace MagnumOpus.Content.ClairDeLune.Weapons.ClockworkHarmony.Projectiles
{
    /// <summary>
    /// Gear Mesh Zone - shader-driven persistent spinning AoE from gear collisions.
    /// 3 render passes: (1) GearSwingArc zone body with spinning gear pattern,
    /// (2) ClairDeLuneMoonlit ambient glow, (3) Bloom stacking for core + gear teeth + harmony corona.
    /// 3s duration, sustained damage. Harmony Mesh (3 gears) = 1.5x AoE + 50% damage.
    /// ai[0] = gear count (2 or 3).
    /// </summary>
    public class GearMeshZoneProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

        private const int Duration = 180;
        private int _timer;
        private float _spinAngle;

        private int GearCount => Math.Max(2, (int)Projectile.ai[0]);
        private float RadiusMultiplier => GearCount >= 3 ? 1.5f : 1f;
        private float BaseRadius => 48f * RadiusMultiplier;

        // --- Shader + texture caching ---
        private static Effect _gearSwingShader;
        private static Effect _moonlitShader;
        private static Asset<Texture2D> _softCircle;
        private static Asset<Texture2D> _softRadialBloom;
        private static Asset<Texture2D> _pointBloom;
        private static Asset<Texture2D> _starFlare;
        private static Asset<Texture2D> _noiseTex;

        public override void SetDefaults()
        {
            Projectile.width = 96;
            Projectile.height = 96;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = -1;
            Projectile.timeLeft = Duration;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
        }

        public override void AI()
        {
            _timer++;
            _spinAngle += 0.08f * (GearCount >= 3 ? 1.3f : 1f);

            if (_timer == 1)
            {
                SoundEngine.PlaySound(SoundID.Item37 with { Pitch = 0.1f, Volume = 0.6f }, Projectile.Center);

                var flash = new BloomParticle(Projectile.Center, Vector2.Zero,
                    ClairDeLunePalette.ClockworkBrass with { A = 0 } * 0.7f, 0.6f, 10);
                MagnumParticleHandler.SpawnParticle(flash);

                for (int i = 0; i < 16; i++)
                {
                    float angle = MathHelper.TwoPi * i / 16f;
                    Vector2 vel = angle.ToRotationVector2() * 5f;
                    var spark = new GenericGlowParticle(Projectile.Center, vel,
                        ClairDeLunePalette.PearlFrost with { A = 0 } * 0.5f, 0.1f, 10, true);
                    MagnumParticleHandler.SpawnParticle(spark);
                }
            }

            // Sustained gear-teeth sparks around zone edge
            if (_timer % 5 == 0)
            {
                int numTeeth = GearCount >= 3 ? 8 : 5;
                for (int i = 0; i < numTeeth; i++)
                {
                    float angle = _spinAngle + MathHelper.TwoPi * i / numTeeth;
                    Vector2 edgePos = Projectile.Center + angle.ToRotationVector2() * BaseRadius;
                    var gearSpark = new SparkleParticle(edgePos,
                        angle.ToRotationVector2() * 2f,
                        ClairDeLunePalette.ClockworkBrass with { A = 0 } * 0.4f, 0.08f, 8);
                    MagnumParticleHandler.SpawnParticle(gearSpark);
                }
            }

            float lifeFrac = 1f - (float)_timer / Duration;
            float fadeAlpha = _timer > Duration - 30 ? lifeFrac : 1f;
            Lighting.AddLight(Projectile.Center, ClairDeLunePalette.ClockworkBrass.ToVector3() * 0.6f * fadeAlpha);
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float dist = Vector2.Distance(Projectile.Center, targetHitbox.Center.ToVector2());
            return dist < BaseRadius;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            for (int i = 0; i < 3; i++)
            {
                Vector2 dir = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
                Vector2 vel = dir.RotatedBy(Main.rand.NextFloat(-0.5f, 0.5f)) * 3f;
                var grind = new GenericGlowParticle(target.Center, vel,
                    ClairDeLunePalette.MoonbeamGold with { A = 0 } * 0.5f, 0.08f, 8, true);
                MagnumParticleHandler.SpawnParticle(grind);
            }
        }

        private void LoadTextures()
        {
            _softCircle ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/MasksAndShapes/SoftCircle", AssetRequestMode.ImmediateLoad);
            _softRadialBloom ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom", AssetRequestMode.ImmediateLoad);
            _pointBloom ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom", AssetRequestMode.ImmediateLoad);
            _starFlare ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/StarFlare", AssetRequestMode.ImmediateLoad);
            _noiseTex ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/NoiseTextures/PerlinNoise", AssetRequestMode.ImmediateLoad);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (Main.dedServ) return false;
            LoadTextures();

            SpriteBatch sb = Main.spriteBatch;
            Matrix matrix = Main.GameViewMatrix.TransformationMatrix;

            float lifeFrac = 1f - (float)_timer / Duration;
            float fadeAlpha = _timer > Duration - 30 ? lifeFrac : 1f;
            float corePulse = 0.7f + 0.3f * MathF.Sin(_spinAngle * 2f);

            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            DrawGearSwingZone(sb, matrix, drawPos, fadeAlpha);             // Pass 1
            DrawMoonlitAmbient(sb, matrix, drawPos, fadeAlpha, corePulse); // Pass 2
            DrawBloomComposite(sb, matrix, drawPos, fadeAlpha, corePulse); // Pass 3
            return false;
        }

        // ---- PASS 1: GearSwingArc for zone body with spinning gear-tooth pattern ----
        private void DrawGearSwingZone(SpriteBatch sb, Matrix matrix, Vector2 drawPos, float fadeAlpha)
        {
            _gearSwingShader ??= ShaderLoader.GearSwing;
            if (_gearSwingShader == null) return;

            sb.End();

            _gearSwingShader.Parameters["uColor"]?.SetValue(ClairDeLunePalette.ClockworkBrass.ToVector4());
            _gearSwingShader.Parameters["uSecondaryColor"]?.SetValue(ClairDeLunePalette.MoonbeamGold.ToVector4());
            _gearSwingShader.Parameters["uOpacity"]?.SetValue(fadeAlpha * 0.5f);
            _gearSwingShader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
            _gearSwingShader.Parameters["uIntensity"]?.SetValue(1.1f);
            _gearSwingShader.Parameters["uOverbrightMult"]?.SetValue(1.2f);
            _gearSwingShader.Parameters["uScrollSpeed"]?.SetValue(2.0f * (GearCount >= 3 ? 1.3f : 1f));
            _gearSwingShader.Parameters["uDistortionAmt"]?.SetValue(0.01f);
            _gearSwingShader.Parameters["uHasSecondaryTex"]?.SetValue(_noiseTex != null);
            _gearSwingShader.Parameters["uPhase"]?.SetValue((_spinAngle % MathHelper.TwoPi) / MathHelper.TwoPi);

            if (_noiseTex != null)
            {
                Main.graphics.GraphicsDevice.Textures[1] = _noiseTex.Value;
                _gearSwingShader.Parameters["uSecondaryTexScale"]?.SetValue(2.0f);
            }

            _gearSwingShader.CurrentTechnique = _gearSwingShader.Techniques["GearSwingArc"];

            sb.Begin(SpriteSortMode.Immediate, BlendState.Additive,
                SamplerState.LinearWrap, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, _gearSwingShader, matrix);

            Texture2D sc = _softCircle.Value;
            float diam = BaseRadius * 2f;
            sb.Draw(sc, drawPos, null, Color.White, _spinAngle, sc.Size() * 0.5f,
                diam / sc.Width, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                SamplerState.LinearClamp, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null, matrix);
        }

        // ---- PASS 2: ClairDeLuneMoonlit ambient glow for environment ----
        private void DrawMoonlitAmbient(SpriteBatch sb, Matrix matrix, Vector2 drawPos, float fadeAlpha, float corePulse)
        {
            _moonlitShader ??= ShaderLoader.ClairDeLuneMoonlit;
            if (_moonlitShader == null) return;

            sb.End();

            _moonlitShader.Parameters["uColor"]?.SetValue(ClairDeLunePalette.NightMist.ToVector4());
            _moonlitShader.Parameters["uSecondaryColor"]?.SetValue(ClairDeLunePalette.DreamHaze.ToVector4());
            _moonlitShader.Parameters["uOpacity"]?.SetValue(fadeAlpha * 0.18f * corePulse);
            _moonlitShader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
            _moonlitShader.Parameters["uIntensity"]?.SetValue(0.6f);
            _moonlitShader.Parameters["uOverbrightMult"]?.SetValue(1.0f);
            _moonlitShader.Parameters["uScrollSpeed"]?.SetValue(0.3f);
            _moonlitShader.Parameters["uHasSecondaryTex"]?.SetValue(false);

            _moonlitShader.CurrentTechnique = _moonlitShader.Techniques["MoonlitGlow"];

            sb.Begin(SpriteSortMode.Immediate, BlendState.Additive,
                SamplerState.LinearWrap, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, _moonlitShader, matrix);

            Texture2D sc = _softCircle.Value;
            float ambientDiam = BaseRadius * 2.6f;
            sb.Draw(sc, drawPos, null, Color.White, 0f, sc.Size() * 0.5f,
                ambientDiam / sc.Width, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                SamplerState.LinearClamp, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null, matrix);
        }

        // ---- PASS 3: Bloom composite (core, spinning gear teeth, harmony corona) ----
        private void DrawBloomComposite(SpriteBatch sb, Matrix matrix, Vector2 drawPos, float fadeAlpha, float corePulse)
        {
            Texture2D srb = _softRadialBloom.Value;
            Texture2D pb = _pointBloom.Value;
            Texture2D sf = _starFlare.Value;
            float zoneScale = BaseRadius * 2f;

            // Zone background halo
            sb.Draw(srb, drawPos, null,
                ClairDeLunePalette.NightMist with { A = 0 } * 0.1f * fadeAlpha,
                0f, srb.Size() * 0.5f, zoneScale * 1.2f / srb.Width, SpriteEffects.None, 0f);

            // Mid glow
            sb.Draw(srb, drawPos, null,
                ClairDeLunePalette.SoftBlue with { A = 0 } * 0.15f * fadeAlpha,
                0f, srb.Size() * 0.5f, zoneScale * 0.8f / srb.Width, SpriteEffects.None, 0f);

            // Center core — clockwork gold spinning
            sb.Draw(pb, drawPos, null,
                ClairDeLunePalette.ClockworkBrass with { A = 0 } * 0.35f * fadeAlpha * corePulse,
                _spinAngle, pb.Size() * 0.5f, 12f / pb.Width, SpriteEffects.None, 0f);

            // Spinning gear teeth around edge (star flares)
            int teethCount = GearCount >= 3 ? 16 : 10;
            for (int i = 0; i < teethCount; i++)
            {
                float angle = _spinAngle + MathHelper.TwoPi * i / teethCount;
                Vector2 toothPos = drawPos + angle.ToRotationVector2() * (BaseRadius * 0.8f);
                float toothRot = angle + MathHelper.PiOver2;

                sb.Draw(sf, toothPos, null,
                    ClairDeLunePalette.ClockworkBrass with { A = 0 } * 0.2f * fadeAlpha,
                    toothRot, sf.Size() * 0.5f, 6f / sf.Width, SpriteEffects.None, 0f);
            }

            // Harmony Mesh extra corona (3-gear golden aura)
            if (GearCount >= 3)
            {
                sb.Draw(srb, drawPos, null,
                    ClairDeLunePalette.MoonbeamGold with { A = 0 } * 0.12f * fadeAlpha,
                    0f, srb.Size() * 0.5f, zoneScale * 1.5f / srb.Width, SpriteEffects.None, 0f);

                // Extra ring of star flares for harmony emphasis
                for (int i = 0; i < 8; i++)
                {
                    float angle = -_spinAngle * 0.7f + MathHelper.TwoPi * i / 8f;
                    Vector2 coronaPos = drawPos + angle.ToRotationVector2() * (BaseRadius * 1.15f);
                    sb.Draw(sf, coronaPos, null,
                        ClairDeLunePalette.MoonbeamGold with { A = 0 } * 0.1f * fadeAlpha,
                        angle, sf.Size() * 0.5f, 8f / sf.Width, SpriteEffects.None, 0f);
                }
            }

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullNone, null, matrix);
        }
    }
}
