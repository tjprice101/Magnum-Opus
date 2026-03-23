using System;
using MagnumOpus.Common;
using MagnumOpus.Content.MoonlightSonata.Weapons.IncisorOfMoonlight.Buffs;
using MagnumOpus.Content.MoonlightSonata.Weapons.IncisorOfMoonlight.Particles;
using MagnumOpus.Content.MoonlightSonata.Weapons.IncisorOfMoonlight.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.MoonlightSonata.Weapons.IncisorOfMoonlight.Projectiles
{
    /// <summary>
    /// Super Lunar Orb — fired when right-clicking with full Lunar Charge.
    /// A large homing orb with 3 smaller orbiting sub-orbs.
    /// On impact, spawns a persistent LunarZoneProj.
    /// Rendering: InfernalBeamBodyShader VertexStrip trail + multi-layer bloom head + sub-orbs.
    /// </summary>
    public class SuperLunarOrbProj : ModProjectile
    {
        private const int NoHomeTime = 16;
        private const float MaxHomeTurn = 0.06f;
        private const float SubOrbRadius = 40f;
        private const float SubOrbSpeed = 0.08f;
        private const int MaxLifetime = 200;

        // Shader + texture cache (shared with LunarBeamProj pattern)
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
            ProjectileID.Sets.TrailCacheLength[Type] = 24;
            ProjectileID.Sets.DrawScreenCheckFluff[Projectile.type] = 600;
        }

        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.tileCollide = false;
            Projectile.penetrate = 1;
            Projectile.timeLeft = MaxLifetime;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
            Projectile.noEnchantmentVisuals = true;
        }

        public override void AI()
        {
            // Sub-orb rotation
            Projectile.localAI[0] += SubOrbSpeed;

            // Constellation spark trail
            if (Main.rand.NextBool(3) && !Main.dedServ)
            {
                Color sparkColor = IncisorUtils.MulticolorLerp(Main.rand.NextFloat(),
                    IncisorUtils.IncisorPalette);
                var spark = new ConstellationSparkParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(6f, 6f),
                    -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(2f, 2f),
                    false, Main.rand.Next(10, 20), Main.rand.NextFloat(0.1f, 0.22f),
                    sparkColor, new Vector2(0.5f, 1.3f), quickShrink: true);
                IncisorParticleHandler.SpawnParticle(spark);
            }

            // Lunar mote trail
            if (Main.rand.NextBool(6) && !Main.dedServ)
            {
                Color moteColor = IncisorUtils.MulticolorLerp(Main.rand.NextFloat(),
                    new Color(170, 140, 255), new Color(135, 206, 250), new Color(220, 230, 255));
                var mote = new LunarMoteParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(8f, 8f),
                    -Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(1.5f, 1.5f),
                    Main.rand.NextFloat(0.2f, 0.45f), moteColor,
                    Main.rand.Next(16, 26), 2.5f, 3.5f, hueShift: 0.012f);
                IncisorParticleHandler.SpawnParticle(mote);
            }

            // Lighting
            Lighting.AddLight(Projectile.Center, new Vector3(0.6f, 0.4f, 0.9f));

            // Homing after NoHomeTime frames
            int framesAlive = MaxLifetime - Projectile.timeLeft;
            if (framesAlive > NoHomeTime)
            {
                NPC target = Projectile.Center.ClosestNPCAt(1200f);
                if (target != null)
                {
                    Vector2 idealDir = (target.Center - Projectile.Center).SafeNormalize(
                        Projectile.velocity.SafeNormalize(Vector2.UnitX));
                    float homeStrength = MathHelper.Clamp(
                        (framesAlive - NoHomeTime) / 40f, 0f, 1f) * MaxHomeTurn;
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity,
                        idealDir * Projectile.velocity.Length(), homeStrength);
                }
            }

            Projectile.rotation = Projectile.velocity.ToRotation();
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<LunarResonanceDebuff>(), 300);

            // Spawn Lunar Zone at impact
            if (Main.myPlayer == Projectile.owner)
            {
                int zoneDmg = Projectile.damage / 3;
                Projectile.NewProjectile(Projectile.GetSource_FromAI(),
                    target.Center, Vector2.Zero,
                    ModContent.ProjectileType<LunarZoneProj>(),
                    zoneDmg, 0f, Projectile.owner);
            }
        }

        public override void OnKill(int timeLeft)
        {
            if (Main.dedServ) return;

            // Burst of constellation sparks
            for (int i = 0; i < 15; i++)
            {
                float angle = MathHelper.TwoPi * i / 15f + Main.rand.NextFloat(-0.1f, 0.1f);
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 8f);
                Color sparkColor = IncisorUtils.MulticolorLerp(Main.rand.NextFloat(),
                    IncisorUtils.IncisorPalette);
                var spark = new ConstellationSparkParticle(
                    Projectile.Center, vel, true,
                    Main.rand.Next(12, 22), Main.rand.NextFloat(0.1f, 0.28f),
                    sparkColor, new Vector2(0.6f, 1.4f));
                IncisorParticleHandler.SpawnParticle(spark);
            }

            // Music notes
            MoonlightVFXLibrary.SpawnMusicNotes(Projectile.Center, count: 4, spread: 25f,
                minScale: 0.6f, maxScale: 1.0f, lifetime: 40);

            SoundEngine.PlaySound(SoundID.Item125 with { Volume = 0.8f, Pitch = 0.1f }, Projectile.Center);
        }

        // =====================================================================
        // RENDERING — Same pipeline as LunarBeamProj but larger + sub-orbs
        // =====================================================================

        private void LoadTextures()
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
            try
            {
                LoadTextures();

                // Build VertexStrip from trail cache
                int count = 0;
                for (int i = 0; i < Projectile.oldPos.Length; i++)
                {
                    if (Projectile.oldPos[i] == Vector2.Zero) break;
                    count++;
                }

                sb.End();

                // === LAYER 1: InfernalBeamBodyShader VertexStrip trail ===
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
                        (float progress) => MathHelper.Lerp(36f, 3f, progress),
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
                        _beamShader.Parameters["totalMult"].SetValue(1.5f);
                        _beamShader.Parameters["uTime"].SetValue(time);

                        _beamShader.CurrentTechnique.Passes["MainPS"].Apply();
                        _strip.DrawTrail();
                        Main.pixelShader.CurrentTechnique.Passes[0].Apply();
                    }
                }

                // === LAYER 2: Multi-layer bloom head (1.5x scale vs LunarBeamProj) ===
                sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                    SamplerState.LinearClamp, DepthStencilState.None,
                    RasterizerState.CullCounterClockwise, null,
                    Main.GameViewMatrix.TransformationMatrix);

                Vector2 drawPos = Projectile.Center - Main.screenPosition;
                Texture2D glowTex = _softGlow?.Value;
                Texture2D bloomTex = _pointBloom?.Value;

                if (glowTex != null && bloomTex != null)
                {
                    float timeVFX = (float)Main.timeForVisualEffects;
                    float pulse = 0.9f + 0.1f * MathF.Sin(timeVFX * 0.05f);

                    Color lunarGlow = IncisorUtils.MulticolorLerp(
                        (Main.GlobalTimeWrappedHourly * 2f) % 1f,
                        new Color(170, 140, 255), new Color(220, 230, 255), new Color(135, 206, 250));
                    lunarGlow.A = 0;

                    // Wide soft outer glow (1.5x scale)
                    sb.Draw(glowTex, drawPos, null, lunarGlow * 0.4f * pulse, 0f,
                        glowTex.Size() / 2f, 0.375f, SpriteEffects.None, 0f);
                    // Mid bloom
                    sb.Draw(bloomTex, drawPos, null, lunarGlow * 0.55f * pulse, 0f,
                        bloomTex.Size() / 2f, 0.15f, SpriteEffects.None, 0f);
                    // White-hot center
                    sb.Draw(bloomTex, drawPos, null, (Color.White with { A = 0 }) * 0.75f, 0f,
                        bloomTex.Size() / 2f, 0.09f, SpriteEffects.None, 0f);

                    // === LAYER 3: 3 orbiting sub-orbs ===
                    float subAngle = Projectile.localAI[0];
                    for (int i = 0; i < 3; i++)
                    {
                        float angle = subAngle + MathHelper.TwoPi * i / 3f;
                        Vector2 subPos = drawPos + angle.ToRotationVector2() * SubOrbRadius * 0.5f;

                        Color subGlow = IncisorUtils.MulticolorLerp(
                            (Main.GlobalTimeWrappedHourly * 3f + i * 0.33f) % 1f,
                            new Color(170, 140, 255), new Color(220, 230, 255), new Color(135, 206, 250));
                        subGlow.A = 0;

                        // Outer glow
                        sb.Draw(glowTex, subPos, null, subGlow * 0.3f, 0f,
                            glowTex.Size() / 2f, 0.12f, SpriteEffects.None, 0f);
                        // Core
                        sb.Draw(bloomTex, subPos, null, subGlow * 0.5f, 0f,
                            bloomTex.Size() / 2f, 0.05f, SpriteEffects.None, 0f);
                        // White center
                        sb.Draw(bloomTex, subPos, null, (Color.White with { A = 0 }) * 0.4f, 0f,
                            bloomTex.Size() / 2f, 0.025f, SpriteEffects.None, 0f);
                    }
                }
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
    }
}
