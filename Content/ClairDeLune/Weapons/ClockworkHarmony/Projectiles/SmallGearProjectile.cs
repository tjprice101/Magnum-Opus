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

namespace MagnumOpus.Content.ClairDeLune.Weapons.ClockworkHarmony.Projectiles
{
    /// <summary>
    /// Small Gear — Fast direct-fire gear (20px, 15°/frame, bounces 1x).
    /// Lightest gear in ClockworkHarmony's mesh system, checks for gear meshing.
    /// 3 render passes: (1) GearSwing GearSwingTrail fast gear body,
    /// (2) ClairDeLuneMoonlit MoonlitFlow speed shimmer, (3) Multi-scale bloom + 8 teeth.
    /// Contains CheckMeshCollision() for gear mesh synergy detection.
    /// </summary>
    public class SmallGearProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

        private const float GearRadius = 10f; // 20px diameter
        private const float SpinRate = MathHelper.Pi / 12f; // 15°/frame
        private const int ToothCount = 8;
        private int _bounceCount;
        private const int MaxBounces = 1;
        private bool _hasMeshed;

        // --- Shader + texture caching ---
        private static Effect _gearSwingShader;
        private static Effect _moonlitShader;
        private static Asset<Texture2D> _softCircle;
        private static Asset<Texture2D> _softRadialBloom;
        private static Asset<Texture2D> _pointBloom;

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            Projectile.rotation += SpinRate;

            // Fast gear — minimal gravity
            Projectile.velocity.Y += 0.03f;

            // Check for gear mesh collision with other gears
            if (!_hasMeshed)
                CheckMeshCollision();

            // Speed sparks
            if (Main.GameUpdateCount % 2 == 0)
            {
                var trail = new GenericGlowParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(3f, 3f),
                    -Projectile.velocity * 0.05f,
                    ClairDeLunePalette.PearlFrost with { A = 0 } * 0.2f, 0.025f, 4, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }

            Lighting.AddLight(Projectile.Center, ClairDeLunePalette.PearlFrost.ToVector3() * 0.15f);
        }

        /// <summary>
        /// Checks for collision with other ClockworkHarmony gears.
        /// On contact, boosts both gears' damage and spawns a mesh-synergy burst.
        /// </summary>
        private void CheckMeshCollision()
        {
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile other = Main.projectile[i];
                if (!other.active || other.whoAmI == Projectile.whoAmI) continue;
                if (other.owner != Projectile.owner) continue;

                bool isGear = other.type == ModContent.ProjectileType<DriveGearProjectile>() ||
                              other.type == ModContent.ProjectileType<MediumGearProjectile>();

                if (!isGear) continue;

                float meshDist = GearRadius + (other.type == ModContent.ProjectileType<DriveGearProjectile>() ? 24f : 16f);
                if (Vector2.Distance(Projectile.Center, other.Center) <= meshDist + 6f)
                {
                    _hasMeshed = true;

                    // Mesh synergy: boost damage
                    Projectile.damage = (int)(Projectile.damage * 1.25f);

                    // Synergy spark burst
                    Vector2 meshPoint = (Projectile.Center + other.Center) * 0.5f;
                    SoundEngine.PlaySound(SoundID.Item10 with { Pitch = 0.5f, Volume = 0.3f }, meshPoint);

                    for (int s = 0; s < 8; s++)
                    {
                        float angle = MathHelper.TwoPi * s / 8f;
                        Vector2 vel = angle.ToRotationVector2() * 2f;
                        var meshSpark = new SparkleParticle(meshPoint, vel,
                            ClairDeLunePalette.MoonbeamGold with { A = 0 } * 0.5f, 0.06f, 8);
                        MagnumParticleHandler.SpawnParticle(meshSpark);
                    }
                    break;
                }
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (_bounceCount >= MaxBounces)
                return true;

            _bounceCount++;

            if (Math.Abs(Projectile.velocity.X - oldVelocity.X) > float.Epsilon)
                Projectile.velocity.X = -oldVelocity.X * 0.6f;
            if (Math.Abs(Projectile.velocity.Y - oldVelocity.Y) > float.Epsilon)
                Projectile.velocity.Y = -oldVelocity.Y * 0.6f;

            SoundEngine.PlaySound(SoundID.Item10 with { Pitch = 0.4f, Volume = 0.2f }, Projectile.Center);
            return false;
        }

        private void LoadTextures()
        {
            _softCircle ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/MasksAndShapes/SoftCircle", AssetRequestMode.ImmediateLoad);
            _softRadialBloom ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom", AssetRequestMode.ImmediateLoad);
            _pointBloom ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom", AssetRequestMode.ImmediateLoad);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (Main.dedServ) return false;
            LoadTextures();

            SpriteBatch sb = Main.spriteBatch;
            Matrix matrix = Main.GameViewMatrix.TransformationMatrix;

            DrawGearSwingBody(sb, matrix);     // Pass 1: GearSwingTrail fast gear
            DrawSpeedShimmer(sb, matrix);      // Pass 2: MoonlitFlow speed shimmer
            DrawBloomTeeth(sb, matrix);        // Pass 3: Bloom + 8 teeth
            ClairDeLuneVFXLibrary.DrawThemeAccents(sb, Projectile.Center, 0.5f, 0.3f);
            return false;
        }

        // ---- PASS 1: GearSwing GearSwingTrail fast gear body ----
        private void DrawGearSwingBody(SpriteBatch sb, Matrix matrix)
        {
            _gearSwingShader ??= ShaderLoader.GearSwing;
            if (_gearSwingShader == null) return;

            sb.End();

            _gearSwingShader.Parameters["uColor"]?.SetValue(ClairDeLunePalette.PearlFrost.ToVector4());
            _gearSwingShader.Parameters["uSecondaryColor"]?.SetValue(ClairDeLunePalette.SoftBlue.ToVector4());
            _gearSwingShader.Parameters["uOpacity"]?.SetValue(0.55f);
            _gearSwingShader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
            _gearSwingShader.Parameters["uIntensity"]?.SetValue(_hasMeshed ? 1.5f : 1f);
            _gearSwingShader.Parameters["uOverbrightMult"]?.SetValue(_hasMeshed ? 1.2f : 1.0f);
            _gearSwingShader.Parameters["uScrollSpeed"]?.SetValue(3f);
            _gearSwingShader.Parameters["uDistortionAmt"]?.SetValue(0.01f);
            _gearSwingShader.Parameters["uHasSecondaryTex"]?.SetValue(false);
            _gearSwingShader.Parameters["uPhase"]?.SetValue(Projectile.rotation);

            _gearSwingShader.CurrentTechnique = _gearSwingShader.Techniques["GearSwingTrail"];

            sb.Begin(SpriteSortMode.Immediate, BlendState.Additive,
                SamplerState.LinearWrap, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, _gearSwingShader, matrix);

            Texture2D sc = _softCircle.Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float bodyScale = GearRadius * 2f / sc.Width;
            sb.Draw(sc, drawPos, null, Color.White, Projectile.rotation, sc.Size() * 0.5f, bodyScale, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                SamplerState.LinearClamp, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null, matrix);
        }

        // ---- PASS 2: MoonlitFlow speed shimmer (enhanced when meshed) ----
        private void DrawSpeedShimmer(SpriteBatch sb, Matrix matrix)
        {
            _moonlitShader ??= ShaderLoader.ClairDeLuneMoonlit;
            if (_moonlitShader == null) return;

            float speedFactor = Projectile.velocity.Length() / 12f;
            float meshBoost = _hasMeshed ? 1.4f : 1f;

            sb.End();

            _moonlitShader.Parameters["uColor"]?.SetValue(ClairDeLunePalette.PearlFrost.ToVector4());
            _moonlitShader.Parameters["uSecondaryColor"]?.SetValue(ClairDeLunePalette.NightMist.ToVector4());
            _moonlitShader.Parameters["uOpacity"]?.SetValue(0.15f * speedFactor * meshBoost);
            _moonlitShader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
            _moonlitShader.Parameters["uIntensity"]?.SetValue(0.6f * meshBoost);
            _moonlitShader.Parameters["uOverbrightMult"]?.SetValue(1.0f);
            _moonlitShader.Parameters["uScrollSpeed"]?.SetValue(3f);
            _moonlitShader.Parameters["uDistortionAmt"]?.SetValue(0.01f);
            _moonlitShader.Parameters["uHasSecondaryTex"]?.SetValue(false);

            _moonlitShader.CurrentTechnique = _moonlitShader.Techniques["MoonlitFlow"];

            sb.Begin(SpriteSortMode.Immediate, BlendState.Additive,
                SamplerState.LinearWrap, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, _moonlitShader, matrix);

            Texture2D sc = _softCircle.Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float shimmerScale = GearRadius * 2.5f / sc.Width;
            sb.Draw(sc, drawPos, null, Color.White, Projectile.velocity.ToRotation(), sc.Size() * 0.5f, shimmerScale, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                SamplerState.LinearClamp, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null, matrix);
        }

        // ---- PASS 3: Multi-scale bloom + 8 teeth ----
        private void DrawBloomTeeth(SpriteBatch sb, Matrix matrix)
        {
            Vector2 pos = Projectile.Center - Main.screenPosition;
            Texture2D srb = _softRadialBloom.Value;
            Texture2D pb = _pointBloom.Value;
            float meshBoost = _hasMeshed ? 1.3f : 1f;

            // Ambient frost
            sb.Draw(srb, pos, null,
                ClairDeLunePalette.NightMist with { A = 0 } * 0.1f * meshBoost, 0f, srb.Size() * 0.5f,
                GearRadius * 1.8f / srb.Width, SpriteEffects.None, 0f);

            // Frost glow
            sb.Draw(srb, pos, null,
                ClairDeLunePalette.PearlFrost with { A = 0 } * 0.15f * meshBoost, 0f, srb.Size() * 0.5f,
                GearRadius / srb.Width, SpriteEffects.None, 0f);

            // Core
            sb.Draw(pb, pos, null,
                ClairDeLunePalette.SoftBlue with { A = 0 } * 0.12f * meshBoost, 0f, pb.Size() * 0.5f,
                GearRadius * 0.4f / pb.Width, SpriteEffects.None, 0f);

            // 8 gear teeth
            for (int t = 0; t < ToothCount; t++)
            {
                float toothAngle = Projectile.rotation + t * MathHelper.TwoPi / ToothCount;
                Vector2 toothPos = pos + toothAngle.ToRotationVector2() * GearRadius;

                sb.Draw(pb, toothPos, null,
                    ClairDeLunePalette.PearlFrost with { A = 0 } * 0.1f * meshBoost, toothAngle, pb.Size() * 0.5f,
                    3f / pb.Width, SpriteEffects.None, 0f);
            }

            // Mesh synergy indicator — golden ring
            if (_hasMeshed)
            {
                sb.Draw(srb, pos, null,
                    ClairDeLunePalette.MoonbeamGold with { A = 0 } * 0.1f, 0f, srb.Size() * 0.5f,
                    GearRadius * 1.5f / srb.Width, SpriteEffects.None, 0f);
            }

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null, matrix);
        }
    }
}
