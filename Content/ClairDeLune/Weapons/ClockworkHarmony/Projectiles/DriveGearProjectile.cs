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
    /// Drive Gear — Heavy slow gear (48px, 5°/frame spin, 2x damage).
    /// The main driving gear of ClockworkHarmony's gear mesh system.
    /// 3 render passes: (1) GearSwing GearSwingArc heavy gear body,
    /// (2) ClairDeLuneMoonlit MoonlitGlow ambient aura, (3) Multi-scale bloom + 16 gear teeth.
    /// </summary>
    public class DriveGearProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

        private const float GearRadius = 24f; // 48px diameter
        private const float SpinRate = MathHelper.Pi / 36f; // 5°/frame
        private const int ToothCount = 16;

        // --- Shader + texture caching ---
        private static Effect _gearSwingShader;
        private static Effect _moonlitShader;
        private static Asset<Texture2D> _softCircle;
        private static Asset<Texture2D> _softRadialBloom;
        private static Asset<Texture2D> _pointBloom;
        private static Asset<Texture2D> _starFlare;

        public override void SetDefaults()
        {
            Projectile.width = 48;
            Projectile.height = 48;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 12;
        }

        public override void AI()
        {
            Projectile.rotation += SpinRate;
            Projectile.velocity.Y += 0.06f; // Subtle gravity — it's heavy
            Projectile.velocity *= 0.995f;

            // Grinding sparks
            if (Main.GameUpdateCount % 4 == 0)
            {
                float sparkAngle = Projectile.rotation + Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 sparkPos = Projectile.Center + sparkAngle.ToRotationVector2() * GearRadius;
                var spark = new GenericGlowParticle(sparkPos,
                    sparkAngle.ToRotationVector2() * 1.5f,
                    ClairDeLunePalette.ClockworkBrass with { A = 0 } * 0.3f, 0.04f, 6, true);
                MagnumParticleHandler.SpawnParticle(spark);
            }

            Lighting.AddLight(Projectile.Center, ClairDeLunePalette.ClockworkBrass.ToVector3() * 0.25f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            SoundEngine.PlaySound(SoundID.Item10 with { Pitch = -0.3f, Volume = 0.4f }, Projectile.Center);

            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 vel = angle.ToRotationVector2() * 2.5f;
                var spark = new GenericGlowParticle(target.Center, vel,
                    ClairDeLunePalette.MoonbeamGold with { A = 0 } * 0.4f, 0.05f, 8, true);
                MagnumParticleHandler.SpawnParticle(spark);
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

            DrawGearSwingBody(sb, matrix);     // Pass 1: GearSwingArc heavy gear body
            DrawMoonlitAura(sb, matrix);       // Pass 2: MoonlitGlow ambient aura
            DrawBloomTeeth(sb, matrix);        // Pass 3: Bloom stacking + gear teeth
            return false;
        }

        // ---- PASS 1: GearSwing GearSwingArc heavy gear body ----
        private void DrawGearSwingBody(SpriteBatch sb, Matrix matrix)
        {
            _gearSwingShader ??= ShaderLoader.GearSwing;
            if (_gearSwingShader == null) return;

            sb.End();

            _gearSwingShader.Parameters["uColor"]?.SetValue(ClairDeLunePalette.ClockworkBrass.ToVector4());
            _gearSwingShader.Parameters["uSecondaryColor"]?.SetValue(ClairDeLunePalette.MoonbeamGold.ToVector4());
            _gearSwingShader.Parameters["uOpacity"]?.SetValue(0.7f);
            _gearSwingShader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
            _gearSwingShader.Parameters["uIntensity"]?.SetValue(1.2f);
            _gearSwingShader.Parameters["uOverbrightMult"]?.SetValue(1.1f);
            _gearSwingShader.Parameters["uScrollSpeed"]?.SetValue(1.5f);
            _gearSwingShader.Parameters["uDistortionAmt"]?.SetValue(0.02f);
            _gearSwingShader.Parameters["uHasSecondaryTex"]?.SetValue(false);
            _gearSwingShader.Parameters["uPhase"]?.SetValue(Projectile.rotation);

            _gearSwingShader.CurrentTechnique = _gearSwingShader.Techniques["GearSwingArc"];

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

        // ---- PASS 2: ClairDeLuneMoonlit MoonlitGlow ambient ----
        private void DrawMoonlitAura(SpriteBatch sb, Matrix matrix)
        {
            _moonlitShader ??= ShaderLoader.ClairDeLuneMoonlit;
            if (_moonlitShader == null) return;

            sb.End();

            _moonlitShader.Parameters["uColor"]?.SetValue(ClairDeLunePalette.NightMist.ToVector4());
            _moonlitShader.Parameters["uSecondaryColor"]?.SetValue(ClairDeLunePalette.MidnightBlue.ToVector4());
            _moonlitShader.Parameters["uOpacity"]?.SetValue(0.2f);
            _moonlitShader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
            _moonlitShader.Parameters["uIntensity"]?.SetValue(0.7f);
            _moonlitShader.Parameters["uOverbrightMult"]?.SetValue(1.0f);
            _moonlitShader.Parameters["uScrollSpeed"]?.SetValue(0.8f);
            _moonlitShader.Parameters["uDistortionAmt"]?.SetValue(0.015f);
            _moonlitShader.Parameters["uHasSecondaryTex"]?.SetValue(false);

            _moonlitShader.CurrentTechnique = _moonlitShader.Techniques["MoonlitGlow"];

            sb.Begin(SpriteSortMode.Immediate, BlendState.Additive,
                SamplerState.LinearWrap, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, _moonlitShader, matrix);

            Texture2D sc = _softCircle.Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float auraScale = GearRadius * 2.5f / sc.Width;
            sb.Draw(sc, drawPos, null, Color.White, 0f, sc.Size() * 0.5f, auraScale, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                SamplerState.LinearClamp, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null, matrix);
        }

        // ---- PASS 3: Multi-scale bloom + 16 gear teeth ----
        private void DrawBloomTeeth(SpriteBatch sb, Matrix matrix)
        {
            Vector2 pos = Projectile.Center - Main.screenPosition;
            Texture2D srb = _softRadialBloom.Value;
            Texture2D pb = _pointBloom.Value;
            Texture2D sf = _starFlare.Value;

            // Outer brass ambient
            sb.Draw(srb, pos, null,
                ClairDeLunePalette.NightMist with { A = 0 } * 0.15f, 0f, srb.Size() * 0.5f,
                GearRadius * 2.2f / srb.Width, SpriteEffects.None, 0f);

            // Brass glow
            sb.Draw(srb, pos, null,
                ClairDeLunePalette.ClockworkBrass with { A = 0 } * 0.25f, 0f, srb.Size() * 0.5f,
                GearRadius * 1.3f / srb.Width, SpriteEffects.None, 0f);

            // Core gold
            sb.Draw(pb, pos, null,
                ClairDeLunePalette.MoonbeamGold with { A = 0 } * 0.2f, 0f, pb.Size() * 0.5f,
                GearRadius * 0.6f / pb.Width, SpriteEffects.None, 0f);

            // 16 gear teeth as bloom dots
            for (int t = 0; t < ToothCount; t++)
            {
                float toothAngle = Projectile.rotation + t * MathHelper.TwoPi / ToothCount;
                Vector2 toothPos = pos + toothAngle.ToRotationVector2() * GearRadius;
                float toothPulse = 0.8f + 0.2f * MathF.Sin(Main.GlobalTimeWrappedHourly * 4f + t * 0.5f);

                sb.Draw(pb, toothPos, null,
                    ClairDeLunePalette.ClockworkBrass with { A = 0 } * 0.15f * toothPulse, toothAngle, pb.Size() * 0.5f,
                    5f / pb.Width, SpriteEffects.None, 0f);
            }

            // Central star flare
            float flareRot = Main.GlobalTimeWrappedHourly * 2f;
            sb.Draw(sf, pos, null,
                ClairDeLunePalette.MoonbeamGold with { A = 0 } * 0.12f, flareRot, sf.Size() * 0.5f,
                GearRadius * 0.5f / sf.Width, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null, matrix);
        }
    }
}
