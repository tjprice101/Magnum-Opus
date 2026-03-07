using System;
using MagnumOpus.Content.MoonlightSonata.Weapons.IncisorOfMoonlight.Buffs;
using MagnumOpus.Content.MoonlightSonata.Weapons.IncisorOfMoonlight.Particles;
using MagnumOpus.Content.MoonlightSonata.Weapons.IncisorOfMoonlight.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.Graphics;
using Terraria.ModLoader;
using ReLogic.Content;

namespace MagnumOpus.Content.MoonlightSonata.Weapons.IncisorOfMoonlight.Projectiles
{
    /// <summary>
    /// Homing lunar beam projectile 遯ｶ繝ｻfired during swing.
    /// Flies straight initially, then homes on the nearest NPC.
    /// Draws itself with a sprite + bloom circle + layered sprite-based trail.
    /// Trail uses overlapping stretched BeamStreak1 segments rotated along velocity.
    /// </summary>
    public class LunarBeamProj : ModProjectile
    {
        public static int NoHomeTime = 24;

        // --- InfernalBeamFoundation scaffolding: shader + texture caching ---
        private static Effect _beamShader;
        private static Asset<Texture2D> _beamAlphaMask;
        private static Asset<Texture2D> _gradientLUT;
        private static Asset<Texture2D> _bodyTex;
        private static Asset<Texture2D> _detailTex1;
        private static Asset<Texture2D> _detailTex2;
        private static Asset<Texture2D> _noiseTex;
        private static Asset<Texture2D> _softGlow;
        private static Asset<Texture2D> _pointBloom;
        private VertexStrip _strip;

        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailingMode[Type] = 2;
            ProjectileID.Sets.TrailCacheLength[Type] = 16;
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.tileCollide = false;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 140;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
            Projectile.noEnchantmentVisuals = true;
        }

        public override void AI()
        {
            // Custom constellation spark trail (replaces generic dust)
            if (Main.rand.NextBool(5))
            {
                Color sparkColor = IncisorUtils.MulticolorLerp(Main.rand.NextFloat(),
                    IncisorUtils.IncisorPalette);
                var spark = new ConstellationSparkParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(4f, 4f),
                    -Projectile.velocity * 0.2f + Main.rand.NextVector2Circular(1.5f, 1.5f),
                    false, Main.rand.Next(10, 18), Main.rand.NextFloat(0.08f, 0.18f),
                    sparkColor, new Vector2(0.5f, 1.3f), quickShrink: true);
                IncisorParticleHandler.SpawnParticle(spark);
            }

            // Lighting
            Lighting.AddLight(Projectile.Center, new Vector3(0.4f, 0.3f, 0.7f));

            // Homing after NoHomeTime frames
            if (Projectile.timeLeft < 140 - NoHomeTime)
            {
                NPC target = Projectile.Center.ClosestNPCAt(1200f);
                if (target != null)
                {
                    Vector2 idealDir = (target.Center - Projectile.Center).SafeNormalize(Projectile.velocity.SafeNormalize(Vector2.UnitX));
                    float homeStrength = MathHelper.Clamp((140 - Projectile.timeLeft - NoHomeTime) / 40f, 0f, 1f) * 0.08f;
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, idealDir * Projectile.velocity.Length(), homeStrength);
                }
            }

            Projectile.rotation = Projectile.velocity.ToRotation();
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<LunarResonanceDebuff>(), 180);

            // Spawn a constellation slash creator at target
            if (Main.myPlayer == Projectile.owner)
            {
                Projectile.NewProjectile(Projectile.GetSource_FromAI(), target.Center,
                    Projectile.velocity * 0.1f, ModContent.ProjectileType<ConstellationSlashCreator>(),
                    (int)(Projectile.damage * 0.6f), 0f, Projectile.owner, target.whoAmI, 25);
            }
        }

        private void LoadBeamTextures()
        {
            const string ThemeBeams = "MagnumOpus/Assets/VFX Asset Library/Theme Specific/Moonlight Sonata/Beam Textures/";
            const string Beams = "MagnumOpus/Assets/VFX Asset Library/BeamTextures/";
            const string Trails = "MagnumOpus/Assets/VFX Asset Library/TrailsAndRibbons/";
            const string Bloom = "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/";
            const string Noise = "MagnumOpus/Assets/VFX Asset Library/NoiseTextures/";
            const string Gradients = "MagnumOpus/Assets/VFX Asset Library/ColorGradients/";

            _beamAlphaMask ??= ModContent.Request<Texture2D>(Trails + "BasicTrail", AssetRequestMode.ImmediateLoad);
            _gradientLUT ??= ModContent.Request<Texture2D>(Gradients + "MoonlightSonataGradientLUTandRAMP", AssetRequestMode.ImmediateLoad);
            _bodyTex ??= ModContent.Request<Texture2D>(Beams + "SoundWaveBeam", AssetRequestMode.ImmediateLoad);
            _detailTex1 ??= ModContent.Request<Texture2D>(ThemeBeams + "MS Energy Motion Beam", AssetRequestMode.ImmediateLoad);
            _detailTex2 ??= ModContent.Request<Texture2D>(ThemeBeams + "MS Energy Surge Beam", AssetRequestMode.ImmediateLoad);
            _noiseTex ??= ModContent.Request<Texture2D>(Noise + "TileableFBMNoise", AssetRequestMode.ImmediateLoad);
            _softGlow ??= ModContent.Request<Texture2D>(Bloom + "SoftGlow", AssetRequestMode.ImmediateLoad);
            _pointBloom ??= ModContent.Request<Texture2D>(Bloom + "PointBloom", AssetRequestMode.ImmediateLoad);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (Main.dedServ) return false;

            SpriteBatch sb = Main.spriteBatch;
            LoadBeamTextures();

            // Build VertexStrip from trail cache (oldPos[0] = newest/head)
            int count = 0;
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) break;
                count++;
            }

            sb.End(); // End current SpriteBatch for raw vertex drawing

            // === LAYER 1: Shader-driven beam body via VertexStrip (InfernalBeamFoundation) ===
            if (count >= 2)
            {
                Vector2[] positions = new Vector2[count];
                float[] rotations = new float[count];
                float totalLength = 0f;

                for (int i = 0; i < count; i++)
                {
                    positions[i] = Projectile.oldPos[i] + Projectile.Size / 2f;
                    rotations[i] = Projectile.oldRot[i];
                    if (i > 0) totalLength += Vector2.Distance(positions[i - 1], positions[i]);
                }

                _strip ??= new VertexStrip();
                _strip.PrepareStrip(positions, rotations,
                    (float progress) => Color.White * (1f - progress * 0.85f),
                    (float progress) => MathHelper.Lerp(24f, 2f, progress),
                    -Main.screenPosition, includeBacksides: true);

                _beamShader ??= ModContent.Request<Effect>(
                    "MagnumOpus/Content/FoundationWeapons/InfernalBeamFoundation/Shaders/InfernalBeamBodyShader",
                    AssetRequestMode.ImmediateLoad).Value;

                if (_beamShader != null)
                {
                    float repVal = MathHelper.Max(totalLength / 800f, 0.3f);
                    float time = (float)Main.timeForVisualEffects * -0.024f;

                    _beamShader.Parameters["WorldViewProjection"].SetValue(
                        Main.GameViewMatrix.NormalizedTransformationmatrix);
                    _beamShader.Parameters["onTex"].SetValue(_beamAlphaMask.Value);
                    _beamShader.Parameters["gradientTex"].SetValue(_gradientLUT.Value);
                    _beamShader.Parameters["bodyTex"].SetValue(_bodyTex.Value);
                    _beamShader.Parameters["detailTex1"].SetValue(_detailTex1.Value);
                    _beamShader.Parameters["detailTex2"].SetValue(_detailTex2.Value);
                    _beamShader.Parameters["noiseTex"].SetValue(_noiseTex.Value);

                    _beamShader.Parameters["bodyReps"].SetValue(1.5f * repVal);
                    _beamShader.Parameters["detail1Reps"].SetValue(2.0f * repVal);
                    _beamShader.Parameters["detail2Reps"].SetValue(1.2f * repVal);
                    _beamShader.Parameters["gradientReps"].SetValue(0.75f * repVal);
                    _beamShader.Parameters["bodyScrollSpeed"].SetValue(0.8f);
                    _beamShader.Parameters["detail1ScrollSpeed"].SetValue(1.2f);
                    _beamShader.Parameters["detail2ScrollSpeed"].SetValue(-0.6f);
                    _beamShader.Parameters["noiseDistortion"].SetValue(0.025f);
                    _beamShader.Parameters["totalMult"].SetValue(1.3f);
                    _beamShader.Parameters["uTime"].SetValue(time);

                    _beamShader.CurrentTechnique.Passes["MainPS"].Apply();
                    _strip.DrawTrail();
                    Main.pixelShader.CurrentTechnique.Passes[0].Apply();
                }
            }

            // === LAYER 2: Multi-layer bloom head (Moonlight Sonata palette) ===
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                SamplerState.LinearClamp, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Texture2D glowTex = _softGlow?.Value;
            Texture2D bloomTex = _pointBloom?.Value;

            if (glowTex != null && bloomTex != null)
            {
                Color lunarGlow = IncisorUtils.MulticolorLerp(
                    (Main.GlobalTimeWrappedHourly * 2f) % 1f,
                    new Color(170, 140, 255), new Color(220, 230, 255), new Color(135, 206, 250));
                lunarGlow.A = 0;

                // Wide soft outer glow
                sb.Draw(glowTex, drawPos, null, lunarGlow * 0.35f, 0f,
                    glowTex.Size() / 2f, 0.25f, SpriteEffects.None, 0f);
                // Mid bloom
                sb.Draw(bloomTex, drawPos, null, lunarGlow * 0.5f, 0f,
                    bloomTex.Size() / 2f, 0.1f, SpriteEffects.None, 0f);
                // White-hot center point
                sb.Draw(bloomTex, drawPos, null, Color.White * 0.7f, 0f,
                    bloomTex.Size() / 2f, 0.06f, SpriteEffects.None, 0f);
            }

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None,
                Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }
    }
}