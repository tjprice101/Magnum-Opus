using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.Shaders;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.ClairDeLune.Weapons.ClockworkGrimoire.Projectiles
{
    /// <summary>
    /// Hour Beam — sustained clockwork beam rendered via ConvergenceBeamShader (LaserFoundation).
    /// 3 rendering passes: (1) VertexStrip beam body with scrolling detail textures,
    /// (2) Multi-layer endpoint/origin flares, (3) Edge particle accents.
    /// ai[0] = synergy flag (1=enhanced width/intensity).
    /// </summary>
    public class HourBeamProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

        private const float MaxLength = 2400f;
        private const float BeamWidth = 40f;

        // --- Shader + texture caching ---
        private static Effect _beamShader;
        private static Asset<Texture2D> _beamAlphaMask;
        private static Asset<Texture2D> _gradientLUT;
        private static Asset<Texture2D> _detailTex1; // EnergyMotion
        private static Asset<Texture2D> _detailTex2; // Oscillating Frequency Wave
        private static Asset<Texture2D> _detailTex3; // SoundWaveBeam
        private static Asset<Texture2D> _detailTex4; // ThinLinearGlow
        private static Asset<Texture2D> _softGlow;
        private static Asset<Texture2D> _softRadialBloom;
        private static Asset<Texture2D> _pointBloom;
        private static Asset<Texture2D> _starFlare;
        private static Asset<Texture2D> _lensFlare;
        private VertexStrip _strip;

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 180;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 8;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            if (player.channel && player.active && !player.dead)
            {
                Projectile.timeLeft = 10;
                Vector2 direction = (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX);
                Projectile.velocity = direction;
                Projectile.Center = player.Center;
                player.ChangeDir(direction.X > 0 ? 1 : -1);
                player.heldProj = Projectile.whoAmI;
                player.itemTime = 2;
                player.itemAnimation = 2;
            }

            Projectile.rotation = Projectile.velocity.ToRotation();

            float beamLen = GetBeamLength();

            // Beam edge particles
            if (Main.rand.NextBool(3))
            {
                float t = Main.rand.NextFloat();
                Vector2 dir = Projectile.velocity.SafeNormalize(Vector2.UnitX);
                Vector2 perp = new Vector2(-dir.Y, dir.X);
                Vector2 beamPos = Projectile.Center + dir * beamLen * t + perp * Main.rand.NextFloat(-BeamWidth * 0.4f, BeamWidth * 0.4f);
                Color pCol = Color.Lerp(ClairDeLunePalette.NightMist, ClairDeLunePalette.MoonbeamGold, t);
                var p = new GenericGlowParticle(beamPos, perp * Main.rand.NextFloat(-0.5f, 0.5f),
                    pCol with { A = 0 } * 0.25f, 0.06f, 8, true);
                MagnumParticleHandler.SpawnParticle(p);
            }

            Vector2 endPos = Projectile.Center + Projectile.velocity * beamLen;
            Lighting.AddLight(endPos, ClairDeLunePalette.MoonbeamGold.ToVector3() * 0.5f);
            Lighting.AddLight(Projectile.Center, ClairDeLunePalette.SoftBlue.ToVector3() * 0.4f);

            // Light along beam
            for (float d = 0; d < beamLen; d += 80f)
            {
                Vector2 lightPos = Projectile.Center + Projectile.velocity * d;
                Lighting.AddLight(lightPos, ClairDeLunePalette.SoftBlue.ToVector3() * 0.2f);
            }
        }

        private float GetBeamLength()
        {
            float len = MaxLength;
            Vector2 start = Projectile.Center;
            Vector2 dir = Projectile.velocity.SafeNormalize(Vector2.UnitX);

            for (float d = 0; d < MaxLength; d += 16f)
            {
                Vector2 check = start + dir * d;
                Point tilePos = check.ToTileCoordinates();
                if (tilePos.X >= 0 && tilePos.X < Main.maxTilesX && tilePos.Y >= 0 && tilePos.Y < Main.maxTilesY)
                {
                    Tile tile = Framing.GetTileSafely(tilePos);
                    if (tile.HasTile && Main.tileSolid[tile.TileType])
                    {
                        len = d;
                        break;
                    }
                }
            }

            return len;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float beamLen = GetBeamLength();
            Vector2 start = Projectile.Center;
            Vector2 end = Projectile.Center + Projectile.velocity * beamLen;
            float point = 0f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, BeamWidth, ref point);
        }

        private void LoadTextures()
        {
            _beamAlphaMask ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/BeamTextures/ThinLinearGlow", AssetRequestMode.ImmediateLoad);
            _gradientLUT ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/ColorGradients/ClairDeLuneGradientLUTandRAMP", AssetRequestMode.ImmediateLoad);
            _detailTex1 ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/Theme Specific/Clair de Lune/Beam Textures/CL Energy Motion Beam", AssetRequestMode.ImmediateLoad);
            _detailTex2 ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/Theme Specific/Clair de Lune/Beam Textures/CL Energy Surge Beam", AssetRequestMode.ImmediateLoad);
            _detailTex3 ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/Theme Specific/Clair de Lune/Beam Textures/CL Braided Energy Helix Beam", AssetRequestMode.ImmediateLoad);
            _detailTex4 ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/BeamTextures/ThinLinearGlow", AssetRequestMode.ImmediateLoad);
            _softGlow ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftGlow", AssetRequestMode.ImmediateLoad);
            _softRadialBloom ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom", AssetRequestMode.ImmediateLoad);
            _pointBloom ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom", AssetRequestMode.ImmediateLoad);
            _starFlare ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/StarFlare", AssetRequestMode.ImmediateLoad);
            _lensFlare ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/LensFlare", AssetRequestMode.ImmediateLoad);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (Main.dedServ) return false;
            LoadTextures();

            SpriteBatch sb = Main.spriteBatch;
            Matrix matrix = Main.GameViewMatrix.TransformationMatrix;

            DrawBeamBody(sb, matrix);     // Pass 1: VertexStrip beam with ConvergenceBeamShader
            DrawEndpointFlares(sb, matrix); // Pass 2: Multi-layer flares at both ends
            ClairDeLuneVFXLibrary.DrawThemeAccents(sb, Projectile.Center, 0.5f, 0.3f);
            return false;
        }

        // ---- PASS 1: VertexStrip beam body (LaserFoundation pattern) ----
        private void DrawBeamBody(SpriteBatch sb, Matrix matrix)
        {
            float beamLen = GetBeamLength();
            if (beamLen < 16f) return;

            bool synergy = Projectile.ai[0] == 1;
            float widthMult = synergy ? 1.5f : 1f;
            float time = Main.GlobalTimeWrappedHourly;

            // Build 2-position strip (start → end) — the shader handles all visuals
            Vector2 dir = Projectile.velocity.SafeNormalize(Vector2.UnitX);
            Vector2[] positions = { Projectile.Center, Projectile.Center + dir * beamLen };
            float[] rotations = { Projectile.rotation, Projectile.rotation };

            _strip ??= new VertexStrip();
            _strip.PrepareStrip(positions, rotations,
                (float progress) => ClairDeLunePalette.PearlWhite with { A = 0 },
                (float progress) => BeamWidth * widthMult * (1f - MathF.Pow(progress, 4f) * 0.3f),
                -Main.screenPosition, 2, includeBacksides: true);

            // Load ConvergenceBeamShader from LaserFoundation
            _beamShader ??= ModContent.Request<Effect>(
                "MagnumOpus/Content/FoundationWeapons/LaserFoundation/Shaders/ConvergenceBeamShader",
                AssetRequestMode.ImmediateLoad).Value;

            sb.End(); // End SpriteBatch for raw vertex drawing

            if (_beamShader != null)
            {
                // UV repetitions scale with beam length so patterns don't stretch
                float uvReps = beamLen / 200f;

                _beamShader.Parameters["WorldViewProjection"]?.SetValue(
                    Main.GameViewMatrix.NormalizedTransformationmatrix);
                _beamShader.Parameters["uTime"]?.SetValue(time);
                _beamShader.Parameters["totalMult"]?.SetValue(1.4f);
                _beamShader.Parameters["satPower"]?.SetValue(2.5f);
                _beamShader.Parameters["gradientReps"]?.SetValue(uvReps);
                _beamShader.Parameters["baseColor"]?.SetValue(ClairDeLunePalette.SoftBlue.ToVector3());

                // Texture bindings
                _beamShader.Parameters["onTex"]?.SetValue(_beamAlphaMask.Value);
                _beamShader.Parameters["gradientTex"]?.SetValue(_gradientLUT.Value);

                // 4 scrolling detail layers — each adds visual complexity
                _beamShader.Parameters["sampleTexture1"]?.SetValue(_detailTex1.Value);
                _beamShader.Parameters["grad1Speed"]?.SetValue(1.2f);
                _beamShader.Parameters["tex1reps"]?.SetValue(uvReps * 2f);
                _beamShader.Parameters["tex1Mult"]?.SetValue(0.8f);

                _beamShader.Parameters["sampleTexture2"]?.SetValue(_detailTex2.Value);
                _beamShader.Parameters["grad2Speed"]?.SetValue(-0.8f);
                _beamShader.Parameters["tex2reps"]?.SetValue(uvReps * 1.5f);
                _beamShader.Parameters["tex2Mult"]?.SetValue(0.6f);

                _beamShader.Parameters["sampleTexture3"]?.SetValue(_detailTex3.Value);
                _beamShader.Parameters["grad3Speed"]?.SetValue(0.5f);
                _beamShader.Parameters["tex3reps"]?.SetValue(uvReps);
                _beamShader.Parameters["tex3Mult"]?.SetValue(0.4f);

                _beamShader.Parameters["sampleTexture4"]?.SetValue(_detailTex4.Value);
                _beamShader.Parameters["grad4Speed"]?.SetValue(2.0f);
                _beamShader.Parameters["tex4reps"]?.SetValue(uvReps * 3f);
                _beamShader.Parameters["tex4Mult"]?.SetValue(0.3f);

                _beamShader.CurrentTechnique.Passes[0].Apply();
                _strip.DrawTrail();
                Main.pixelShader.CurrentTechnique.Passes[0].Apply(); // Reset pixel shader
            }

            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                SamplerState.LinearClamp, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null, matrix);
        }

        // ---- PASS 2: Multi-layer endpoint flares (LaserFoundation flare pattern) ----
        private void DrawEndpointFlares(SpriteBatch sb, Matrix matrix)
        {
            float beamLen = GetBeamLength();
            Vector2 dir = Projectile.velocity.SafeNormalize(Vector2.UnitX);
            bool synergy = Projectile.ai[0] == 1;
            float intensityMult = synergy ? 1.4f : 1f;
            float time = Main.GlobalTimeWrappedHourly;

            Vector2 originDraw = Projectile.Center - Main.screenPosition;
            Vector2 endDraw = Projectile.Center + dir * beamLen - Main.screenPosition;

            Texture2D sg = _softGlow.Value;
            Texture2D srb = _softRadialBloom.Value;
            Texture2D pb = _pointBloom.Value;
            Texture2D sf = _starFlare.Value;
            Texture2D lf = _lensFlare.Value;

            // -- Origin flares --
            // Wide ambient halo
            sb.Draw(srb, originDraw, null, ClairDeLunePalette.NightMist with { A = 0 } * 0.2f * intensityMult,
                0f, srb.Size() * 0.5f, 80f / srb.Width, SpriteEffects.None, 0f);
            // Mid glow
            sb.Draw(srb, originDraw, null, ClairDeLunePalette.SoftBlue with { A = 0 } * 0.35f * intensityMult,
                0f, srb.Size() * 0.5f, 50f / srb.Width, SpriteEffects.None, 0f);
            // Bright core
            sb.Draw(pb, originDraw, null, ClairDeLunePalette.PearlWhite with { A = 0 } * 0.5f * intensityMult,
                0f, pb.Size() * 0.5f, 25f / pb.Width, SpriteEffects.None, 0f);
            // Spinning star flare
            sb.Draw(sf, originDraw, null, ClairDeLunePalette.MoonbeamGold with { A = 0 } * 0.3f * intensityMult,
                time * 1.5f, sf.Size() * 0.5f, 40f / sf.Width, SpriteEffects.None, 0f);

            // -- Endpoint flares --
            float endPulse = 0.8f + 0.2f * MathF.Sin(time * 6f);
            // Wide ambient
            sb.Draw(srb, endDraw, null, ClairDeLunePalette.MoonbeamGold with { A = 0 } * 0.3f * intensityMult * endPulse,
                0f, srb.Size() * 0.5f, 100f / srb.Width, SpriteEffects.None, 0f);
            // Mid glow
            sb.Draw(srb, endDraw, null, ClairDeLunePalette.PearlFrost with { A = 0 } * 0.4f * intensityMult,
                0f, srb.Size() * 0.5f, 55f / srb.Width, SpriteEffects.None, 0f);
            // Hot core
            sb.Draw(pb, endDraw, null, ClairDeLunePalette.WhiteHot with { A = 0 } * 0.6f * intensityMult,
                0f, pb.Size() * 0.5f, 30f / pb.Width, SpriteEffects.None, 0f);
            // Cross-star flares (counter-rotating pair)
            sb.Draw(sf, endDraw, null, ClairDeLunePalette.MoonbeamGold with { A = 0 } * 0.35f * intensityMult,
                time * 2f, sf.Size() * 0.5f, 50f / sf.Width, SpriteEffects.None, 0f);
            sb.Draw(sf, endDraw, null, ClairDeLunePalette.SoftBlue with { A = 0 } * 0.2f * intensityMult,
                -time * 1.5f + MathHelper.PiOver4, sf.Size() * 0.5f, 35f / sf.Width, SpriteEffects.None, 0f);
            // Lens flare accent
            sb.Draw(lf, endDraw, null, ClairDeLunePalette.PearlWhite with { A = 0 } * 0.15f * intensityMult * endPulse,
                time * 0.3f, lf.Size() * 0.5f, 70f / lf.Width, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null, matrix);
        }
    }
}
