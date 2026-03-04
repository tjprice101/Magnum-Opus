using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.Shaders;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.ClairDeLune.Weapons.CogAndHammer.Projectiles
{
    /// <summary>
    /// Gear Shrapnel — brass gear fragment ejected from bomb detonations.
    /// Deals 30% bomb damage, short lifetime, spinning.
    /// 2 render passes: (1) GearSwing GearSwingTrail for spinning gear body,
    /// (2) Multi-scale bloom with gear teeth + core.
    /// Kept lightweight — many spawned from simultaneous detonations.
    /// </summary>
    public class GearShrapnelProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

        // --- Texture + shader caching ---
        private static Effect _gearSwingShader;
        private static Asset<Texture2D> _softCircle;
        private static Asset<Texture2D> _softRadialBloom;
        private static Asset<Texture2D> _pointBloom;

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 40;
            Projectile.tileCollide = true;
        }

        public override void AI()
        {
            Projectile.rotation += 0.4f;
            Projectile.velocity *= 0.96f;
            Projectile.velocity.Y += 0.2f;

            // Spark trail
            if (Main.rand.NextBool(2))
            {
                Color sparkCol = Main.rand.NextBool(5)
                    ? ClairDeLunePalette.NightMist
                    : ClairDeLunePalette.MoonbeamGold;
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.GoldFlame,
                    -Projectile.velocity * 0.05f, 0, sparkCol, 0.4f);
                d.noGravity = true;
            }

            Lighting.AddLight(Projectile.Center, ClairDeLunePalette.MoonbeamGold.ToVector3() * 0.15f);
        }

        public override void OnKill(int timeLeft)
        {
            var flash = new GenericGlowParticle(Projectile.Center, Vector2.Zero,
                ClairDeLunePalette.MoonbeamGold with { A = 0 } * 0.3f, 0.08f, 5, true);
            MagnumParticleHandler.SpawnParticle(flash);
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

            DrawGearShader(sb, matrix);    // Pass 1: GearSwingTrail spinning body
            DrawBloomAndTeeth(sb, matrix); // Pass 2: Bloom teeth + core
            return false;
        }

        // ---- PASS 1: GearSwing GearSwingTrail spinning gear body ----
        private void DrawGearShader(SpriteBatch sb, Matrix matrix)
        {
            _gearSwingShader ??= ShaderLoader.GearSwing;
            if (_gearSwingShader == null) return;

            sb.End();

            _gearSwingShader.Parameters["uColor"]?.SetValue(ClairDeLunePalette.ClockworkBrass.ToVector4());
            _gearSwingShader.Parameters["uSecondaryColor"]?.SetValue(ClairDeLunePalette.MoonbeamGold.ToVector4());
            _gearSwingShader.Parameters["uOpacity"]?.SetValue(0.4f);
            _gearSwingShader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
            _gearSwingShader.Parameters["uIntensity"]?.SetValue(0.8f);
            _gearSwingShader.Parameters["uOverbrightMult"]?.SetValue(1.0f);
            _gearSwingShader.Parameters["uScrollSpeed"]?.SetValue(4f);
            _gearSwingShader.Parameters["uDistortionAmt"]?.SetValue(0.015f);
            _gearSwingShader.Parameters["uHasSecondaryTex"]?.SetValue(false);

            _gearSwingShader.CurrentTechnique = _gearSwingShader.Techniques["GearSwingTrail"];

            sb.Begin(SpriteSortMode.Immediate, BlendState.Additive,
                SamplerState.LinearWrap, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, _gearSwingShader, matrix);

            Texture2D sc = _softCircle.Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float gearScale = 14f / sc.Width;
            sb.Draw(sc, drawPos, null, Color.White, Projectile.rotation,
                sc.Size() * 0.5f, gearScale, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                SamplerState.LinearClamp, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null, matrix);
        }

        // ---- PASS 2: Multi-scale bloom with gear teeth + core ----
        private void DrawBloomAndTeeth(SpriteBatch sb, Matrix matrix)
        {
            Vector2 pos = Projectile.Center - Main.screenPosition;
            float life = (float)Projectile.timeLeft / 40f;
            Texture2D srb = _softRadialBloom.Value;
            Texture2D pb = _pointBloom.Value;

            // Gear teeth — 4 small bloom dots orbiting
            int teeth = 4;
            float radius = 5f;
            for (int t = 0; t < teeth; t++)
            {
                float angle = Projectile.rotation + MathHelper.TwoPi * t / teeth;
                Vector2 toothPos = pos + angle.ToRotationVector2() * radius;
                sb.Draw(pb, toothPos, null,
                    ClairDeLunePalette.ClockworkBrass with { A = 0 } * 0.3f * life, 0f,
                    pb.Size() * 0.5f, 3f / pb.Width, SpriteEffects.None, 0f);
            }

            // Outer glow haze
            sb.Draw(srb, pos, null,
                ClairDeLunePalette.MoonbeamGold with { A = 0 } * 0.15f * life, 0f,
                srb.Size() * 0.5f, 18f / srb.Width, SpriteEffects.None, 0f);

            // Core
            sb.Draw(pb, pos, null,
                ClairDeLunePalette.MoonbeamGold with { A = 0 } * 0.35f * life, 0f,
                pb.Size() * 0.5f, 6f / pb.Width, SpriteEffects.None, 0f);

            // Restore AlphaBlend
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null, matrix);
        }
    }
}
