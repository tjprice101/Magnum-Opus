using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.Shaders;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.ClairDeLune.Weapons.ClockworkGrimoire.Projectiles
{
    /// <summary>
    /// Second Bolt — rapid-fire small piercing bolt for Second Mode (20/s).
    /// 6px, Pearl Frost core → Soft Moonblue trail, pierces 1 enemy.
    /// 2 render passes: (1) ClairDeLuneMoonlit MoonlitFlow shimmer body,
    /// (2) Elongated bloom trail + core stacking.
    /// Kept lightweight due to 20/s spawn rate.
    /// </summary>
    public class SecondBoltProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

        private const int TrailLen = 6;
        private Vector2[] _oldPos = new Vector2[TrailLen];

        // --- Texture + shader caching ---
        private static Effect _moonlitShader;
        private static Asset<Texture2D> _softCircle;
        private static Asset<Texture2D> _softRadialBloom;
        private static Asset<Texture2D> _pointBloom;

        public override void SetDefaults()
        {
            Projectile.width = 6;
            Projectile.height = 6;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 60;
            Projectile.tileCollide = true;
            Projectile.extraUpdates = 2;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();

            for (int i = TrailLen - 1; i > 0; i--)
                _oldPos[i] = _oldPos[i - 1];
            _oldPos[0] = Projectile.Center;

            Lighting.AddLight(Projectile.Center, ClairDeLunePalette.PearlFrost.ToVector3() * 0.15f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            var flash = new GenericGlowParticle(target.Center, Vector2.Zero,
                ClairDeLunePalette.PearlFrost with { A = 0 } * 0.3f, 0.06f, 4, true);
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

            DrawMoonlitBody(sb, matrix);       // Pass 1: MoonlitFlow shimmer body
            DrawBloomTrailAndCore(sb, matrix); // Pass 2: Bloom trail + core
            return false;
        }

        // ---- PASS 1: ClairDeLuneMoonlit MoonlitFlow shimmer body ----
        private void DrawMoonlitBody(SpriteBatch sb, Matrix matrix)
        {
            _moonlitShader ??= ShaderLoader.ClairDeLuneMoonlit;
            if (_moonlitShader == null) return;

            sb.End();

            _moonlitShader.Parameters["uColor"]?.SetValue(ClairDeLunePalette.PearlFrost.ToVector4());
            _moonlitShader.Parameters["uSecondaryColor"]?.SetValue(ClairDeLunePalette.SoftBlue.ToVector4());
            _moonlitShader.Parameters["uOpacity"]?.SetValue(0.35f);
            _moonlitShader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
            _moonlitShader.Parameters["uIntensity"]?.SetValue(1.0f);
            _moonlitShader.Parameters["uOverbrightMult"]?.SetValue(1.0f);
            _moonlitShader.Parameters["uScrollSpeed"]?.SetValue(6f);
            _moonlitShader.Parameters["uDistortionAmt"]?.SetValue(0.01f);
            _moonlitShader.Parameters["uHasSecondaryTex"]?.SetValue(false);

            _moonlitShader.CurrentTechnique = _moonlitShader.Techniques["MoonlitFlow"];

            sb.Begin(SpriteSortMode.Immediate, MagnumBlendStates.ShaderAdditive,
                SamplerState.LinearWrap, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, _moonlitShader, matrix);

            Texture2D sc = _softCircle.Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float bodyScale = 8f / sc.Width;
            sb.Draw(sc, drawPos, null, Color.White, Projectile.rotation,
                sc.Size() * 0.5f, new Vector2(bodyScale * 1.5f, bodyScale), SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                SamplerState.LinearClamp, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null, matrix);
        }

        // ---- PASS 2: Elongated bloom trail + core stacking ----
        private void DrawBloomTrailAndCore(SpriteBatch sb, Matrix matrix)
        {
            Texture2D srb = _softRadialBloom.Value;
            Texture2D pb = _pointBloom.Value;
            Vector2 srbOrigin = srb.Size() * 0.5f;
            Vector2 pbOrigin = pb.Size() * 0.5f;

            // Trail afterimages
            for (int i = 0; i < TrailLen; i++)
            {
                if (_oldPos[i] == Vector2.Zero) continue;
                float fade = 1f - (float)i / TrailLen;
                Vector2 tDraw = _oldPos[i] - Main.screenPosition;
                Color tCol = Color.Lerp(ClairDeLunePalette.PearlFrost, ClairDeLunePalette.SoftBlue,
                    (float)i / TrailLen) with { A = 0 } * fade * 0.2f;
                float trailScale = 6f / srb.Width * fade;
                sb.Draw(srb, tDraw, null, tCol, Projectile.rotation, srbOrigin,
                    new Vector2(trailScale * 2f, trailScale), SpriteEffects.None, 0f);
            }

            // Core bloom layers
            Vector2 pos = Projectile.Center - Main.screenPosition;

            // Outer soft haze
            sb.Draw(srb, pos, null,
                ClairDeLunePalette.SoftBlue with { A = 0 } * 0.2f, Projectile.rotation, srbOrigin,
                new Vector2(12f / srb.Width, 6f / srb.Width), SpriteEffects.None, 0f);

            // Inner bright core
            sb.Draw(pb, pos, null,
                ClairDeLunePalette.PearlFrost with { A = 0 } * 0.3f, Projectile.rotation, pbOrigin,
                new Vector2(6f / pb.Width, 3f / pb.Width), SpriteEffects.None, 0f);

            // Restore AlphaBlend
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null, matrix);
        }
    }
}
