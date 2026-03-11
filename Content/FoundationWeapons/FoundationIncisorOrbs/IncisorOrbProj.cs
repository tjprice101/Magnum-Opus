using System;
using MagnumOpus.Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.FoundationWeapons.FoundationIncisorOrbs
{
    /// <summary>
    /// IncisorOrbProj — 1-to-1 skeleton of LunarBeamProj (the homing orb projectile
    /// fired during the Incisor of Moonlight's swing).
    ///
    /// VISUAL ARCHITECTURE (identical to LunarBeamProj):
    /// LAYER 1: Shader-driven beam body via VertexStrip (InfernalBeamBodyShader)
    ///   — Builds a triangle strip from trail cache positions
    ///   — Uses InfernalBeamBodyShader with scrolling noise, gradient LUT, body+detail textures
    ///   — Width tapers from 24px (head) to 2px (tail)
    ///   — Color fades from full opacity to 15% along the trail
    ///
    /// LAYER 2: Multi-layer bloom head
    ///   — Wide soft outer glow (SoftGlow texture, 0.25 scale, 35% opacity)
    ///   — Mid bloom (PointBloom texture, 0.1 scale, 50% opacity)
    ///   — White-hot center point (PointBloom texture, 0.06 scale, 70% opacity)
    ///   — Colors cycle through the palette using MulticolorLerp
    ///
    /// BEHAVIOUR (identical to LunarBeamProj):
    /// — Flies straight for NoHomeTime frames (24)
    /// — After that, homes on nearest NPC within 1200px
    /// — Homing strength ramps up over 40 frames (0 → 8% turn rate)
    /// — 1 penetration, 140 frame lifetime, 1 extra update
    /// — Constellation spark particle trail (every 5 frames)
    /// </summary>
    public class IncisorOrbProj : ModProjectile
    {
        // --- Homing constants — identical to LunarBeamProj ---
        public static int NoHomeTime = 24;

        // --- Shader + texture caching — identical to LunarBeamProj ---
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

        // =====================================================================
        // SETUP — identical to LunarBeamProj
        // =====================================================================

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

        // =====================================================================
        // AI — identical to LunarBeamProj
        // =====================================================================

        public override void AI()
        {
            // Constellation spark trail — identical to LunarBeamProj
            // Uses vanilla dust as a stand-in for ConstellationSparkParticle
            if (Main.rand.NextBool(5))
            {
                Color sparkColor = IOFUtils.MulticolorLerp(Main.rand.NextFloat(), IOFUtils.FoundationPalette);
                Vector2 dustVel = -Projectile.velocity * 0.2f + Main.rand.NextVector2Circular(1.5f, 1.5f);
                Dust spark = Dust.NewDustPerfect(
                    Projectile.Center + Main.rand.NextVector2Circular(4f, 4f),
                    DustID.PurpleTorch, dustVel,
                    newColor: sparkColor, Scale: Main.rand.NextFloat(0.3f, 0.6f));
                spark.noGravity = true;
                spark.fadeIn = 0.3f;
            }

            // Lighting — identical to LunarBeamProj
            Lighting.AddLight(Projectile.Center, new Vector3(0.4f, 0.3f, 0.7f));

            // Homing after NoHomeTime frames — identical to LunarBeamProj
            if (Projectile.timeLeft < 140 - NoHomeTime)
            {
                NPC target = FindClosestNPC(1200f);
                if (target != null)
                {
                    Vector2 idealDir = (target.Center - Projectile.Center).SafeNormalize(
                        Projectile.velocity.SafeNormalize(Vector2.UnitX));
                    float homeStrength = MathHelper.Clamp(
                        (140 - Projectile.timeLeft - NoHomeTime) / 40f, 0f, 1f) * 0.08f;
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity,
                        idealDir * Projectile.velocity.Length(), homeStrength);
                }
            }

            Projectile.rotation = Projectile.velocity.ToRotation();
        }

        /// <summary>Finds the closest hostile NPC within maxDist.</summary>
        private NPC FindClosestNPC(float maxDist)
        {
            NPC closest = null;
            float closestDist = maxDist;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.CanBeChasedBy()) continue;
                float dist = Vector2.Distance(Projectile.Center, npc.Center);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = npc;
                }
            }
            return closest;
        }

        // =====================================================================
        // ON HIT — simple dust burst (skeleton version)
        // =====================================================================

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Impact dust burst
            if (!Main.dedServ)
            {
                for (int i = 0; i < 6; i++)
                {
                    Color sparkColor = IOFUtils.MulticolorLerp(Main.rand.NextFloat(), IOFUtils.FoundationPalette);
                    Vector2 dustVel = Main.rand.NextVector2Circular(5f, 5f);
                    Dust spark = Dust.NewDustPerfect(target.Center, DustID.PurpleTorch, dustVel,
                        newColor: sparkColor, Scale: Main.rand.NextFloat(0.5f, 0.9f));
                    spark.noGravity = true;
                    spark.fadeIn = 0.4f;
                }
            }
        }

        // =====================================================================
        // TEXTURE LOADING — identical to LunarBeamProj.LoadBeamTextures()
        // =====================================================================

        private void LoadBeamTextures()
        {
            _beamAlphaMask ??= IOFTextures.BasicTrail;
            _gradientLUT ??= IOFTextures.MoonlightGradient;
            _bodyTex ??= IOFTextures.SoundWaveBeam;
            _detailTex1 ??= IOFTextures.EnergyMotionBeam;
            _detailTex2 ??= IOFTextures.EnergySurgeBeam;
            _noiseTex ??= IOFTextures.FBMNoise;
            _softGlow ??= IOFTextures.SoftGlow;
            _pointBloom ??= IOFTextures.PointBloom;
        }

        // =====================================================================
        // RENDERING — identical to LunarBeamProj.PreDraw()
        // Two layers: shader-driven VertexStrip trail + multi-layer bloom head
        // =====================================================================

        public override bool PreDraw(ref Color lightColor)
        {
            if (Main.dedServ) return false;

            SpriteBatch sb = Main.spriteBatch;
            try
            {
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
            // Identical to LunarBeamProj's Layer 1
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

                    // Shader uniforms — identical to LunarBeamProj
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

            // === LAYER 2: Multi-layer bloom head (identical to LunarBeamProj) ===
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                SamplerState.LinearClamp, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Texture2D glowTex = _softGlow?.Value;
            Texture2D bloomTex = _pointBloom?.Value;

            if (glowTex != null && bloomTex != null)
            {
                // Color cycling through palette — identical to LunarBeamProj
                Color lunarGlow = IOFUtils.MulticolorLerp(
                    (Main.GlobalTimeWrappedHourly * 2f) % 1f,
                    IOFUtils.FoundationPalette);
                lunarGlow.A = 0;

                // Wide soft outer glow — identical scale/opacity to LunarBeamProj
                sb.Draw(glowTex, drawPos, null, lunarGlow * 0.35f, 0f,
                    glowTex.Size() / 2f, 0.25f, SpriteEffects.None, 0f);

                // Mid bloom — identical to LunarBeamProj
                sb.Draw(bloomTex, drawPos, null, lunarGlow * 0.5f, 0f,
                    bloomTex.Size() / 2f, 0.1f, SpriteEffects.None, 0f);

                // White-hot center point — identical to LunarBeamProj
                sb.Draw(bloomTex, drawPos, null, Color.White * 0.7f, 0f,
                    bloomTex.Size() / 2f, 0.06f, SpriteEffects.None, 0f);
            }

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None,
                Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

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

        // =====================================================================
        // ON KILL — dust burst
        // =====================================================================

        public override void OnKill(int timeLeft)
        {
            if (Main.dedServ) return;
            for (int i = 0; i < 4; i++)
            {
                Color sparkColor = IOFUtils.MulticolorLerp(Main.rand.NextFloat(), IOFUtils.FoundationPalette);
                Vector2 dustVel = Main.rand.NextVector2Circular(3f, 3f);
                Dust spark = Dust.NewDustPerfect(Projectile.Center, DustID.PurpleTorch, dustVel,
                    newColor: sparkColor, Scale: Main.rand.NextFloat(0.4f, 0.7f));
                spark.noGravity = true;
                spark.fadeIn = 0.4f;
            }
        }
    }
}
