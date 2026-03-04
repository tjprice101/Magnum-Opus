using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.Graphics;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.MoonlightSonata.Weapons.ResurrectionOfTheMoon.Utilities;
using MagnumOpus.Content.MoonlightSonata.Weapons.ResurrectionOfTheMoon.Particles;
using MagnumOpus.Content.MoonlightSonata.Weapons.ResurrectionOfTheMoon.Primitives;
using MagnumOpus.Content.MoonlightSonata.Weapons.ResurrectionOfTheMoon.Buffs;
using MagnumOpus.Content.MoonlightSonata.Weapons.ResurrectionOfTheMoon.Dusts;
using MagnumOpus.Content.MoonlightSonata;
using MagnumOpus.Content.FoundationWeapons.SparkleProjectileFoundation;
using ReLogic.Content;

namespace MagnumOpus.Content.MoonlightSonata.Weapons.ResurrectionOfTheMoon.Projectiles
{
    /// <summary>
    /// Comet Core Chamber — Piercing comet with burning ember wake.
    /// Passes through 5 enemies, leaving a searing ember trail.
    /// Each pierce applies stacking Lunar Impact and spawns fire particles.
    /// Trail at maximum CometTrail intensity (white-hot).
    /// Destroys on tile contact.
    ///
    /// FOUNDATION VFX INTEGRATION:
    /// - SPARKLE TRAIL OVERLAY (SparkleProjectileFoundation): SparkleTrailShader VertexStrip
    ///   layered over CometTrail for crystalline comet glitter effect
    /// - SPARKLE ACCENTS: 4PointedStarHard sprites at trail points for discrete glitter flashes
    /// - ENHANCED BLOOM: Multi-scale bloom stacking with lunar phase colors
    /// </summary>
    public class CometCore : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_1";

        // =================================================================
        // CONSTANTS
        // =================================================================

        public const int MaxPierces = 5;
        public const float CometWidth = 28f;
        public const int TrailLength = 30;
        public const float DamageMultiplier = 1.2f;

        // =================================================================
        // AI FIELDS
        // =================================================================

        /// <summary>ai[0] = pierce count.</summary>
        public ref float PierceCount => ref Projectile.ai[0];

        /// <summary>localAI[0] = alive time.</summary>
        public ref float AliveTime => ref Projectile.localAI[0];

        // =================================================================
        // SETUP
        // =================================================================

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailingMode[Type] = 2;
            ProjectileID.Sets.TrailCacheLength[Type] = TrailLength;
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = MaxPierces + 1;
            Projectile.timeLeft = 480;
            Projectile.extraUpdates = 3;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 6;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.alpha = 0;
            Projectile.light = 0.8f;
        }

        // =================================================================
        // AI
        // =================================================================

        public override void AI()
        {
            AliveTime++;
            Projectile.rotation = Projectile.velocity.ToRotation();

            // Intense lighting — this is a burning comet
            Color lightCol = Color.Lerp(CometUtils.CometCoreWhite, CometUtils.CometCoreColor, 0.5f);
            Lighting.AddLight(Projectile.Center, lightCol.ToVector3() * 0.8f);

            SpawnFlightParticles();
        }

        private void SpawnFlightParticles()
        {
            if (Main.dedServ) return;

            // Dense ember trail — every tick, multiple embers for the burning wake
            for (int i = 0; i < 2; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(6f, 6f);
                Vector2 emberVel = -Projectile.velocity.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(2f, 5f)
                    + Main.rand.NextVector2Circular(2f, 2f);
                CometParticleHandler.Spawn(new EmberTrailParticle(
                    Projectile.Center + offset, emberVel,
                    0.5f + Main.rand.NextFloat(0.3f), 20 + Main.rand.Next(15)));
            }

            // Dust trail — heavy
            if (AliveTime % 1 == 0)
            {
                int d = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height,
                    ModContent.DustType<CometDust>(), -Projectile.velocity.X * 0.15f, -Projectile.velocity.Y * 0.15f);
                Main.dust[d].noGravity = true;
                Main.dust[d].scale = 1.2f + Main.rand.NextFloat(0.4f);
            }

            // Head mist — every 3 ticks
            if (AliveTime % 3 == 0)
            {
                CometParticleHandler.Spawn(new CometMistParticle(
                    Projectile.Center, Main.rand.NextVector2Circular(0.8f, 0.8f),
                    CometUtils.CometCoreColor * 0.5f,
                    0.5f + Main.rand.NextFloat(0.3f), 18));
            }
        }

        // =================================================================
        // ON HIT — Pierce VFX
        // =================================================================

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            PierceCount++;

            // Lunar Impact debuff — stacking
            target.AddBuff(ModContent.BuffType<LunarImpact>(), 360);
            var impactNpc = target.GetGlobalNPC<LunarImpactNPC>();
            impactNpc.AddStack();
            impactNpc.AddStack(); // Double stack for Comet Core's piercing heat

            // Add charge to player
            if (Projectile.owner == Main.myPlayer)
                Main.LocalPlayer.Resurrection().AddCharge(2);

            // Pierce VFX
            if (!Main.dedServ)
            {
                // Burning impact bloom
                CometParticleHandler.Spawn(new CraterBloomParticle(
                    target.Center, CometUtils.CometCoreColor, 1.2f, 18));

                // Lunar cycle ring at impact
                Color lunarTint = CometUtils.CometCoreColor;
                if (Projectile.owner >= 0 && Projectile.owner < Main.maxPlayers && Main.player[Projectile.owner].active)
                    lunarTint = Main.player[Projectile.owner].Resurrection().CurrentLunarColor;
                CometParticleHandler.Spawn(new LunarCycleRingParticle(
                    target.Center, lunarTint, 1f, 15));

                // Searing spark burst
                for (int i = 0; i < 10; i++)
                {
                    Vector2 vel = Main.rand.NextVector2CircularEdge(6f, 6f);
                    CometParticleHandler.Spawn(new EmberTrailParticle(
                        target.Center, vel, 0.4f + Main.rand.NextFloat(0.2f),
                        15 + Main.rand.Next(10)));
                }

                // Lunar shards from pierced enemy
                for (int i = 0; i < 4; i++)
                {
                    Vector2 vel = Projectile.velocity.SafeNormalize(Vector2.UnitX) * Main.rand.NextFloat(2f, 5f)
                        + Main.rand.NextVector2Circular(3f, 3f);
                    CometParticleHandler.Spawn(new LunarShardParticle(
                        target.Center, vel,
                        CometUtils.GetCometGradient(Main.rand.NextFloat(0.5f, 1f)),
                        0.4f + Main.rand.NextFloat(0.3f), 20 + Main.rand.Next(10)));
                }

                // Music notes on pierce — escalating with pierce count
                int noteCount = 1 + (int)PierceCount;
                MoonlightVFXLibrary.SpawnMusicNotes(target.Center, count: Math.Min(noteCount, 5),
                    spread: 15f + PierceCount * 3f, minScale: 0.4f, maxScale: 0.7f,
                    lifetime: 25 + (int)(PierceCount * 5));
            }

            SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode with { Volume = 0.5f, Pitch = 0.2f },
                target.Center);
        }

        // =================================================================
        // RENDERING
        // =================================================================

        // Foundation sparkle shader + texture cache
        private Effect _sparkleTrailShader;
        private static Asset<Texture2D> _gradientLUT;

        public override bool PreDraw(ref Color lightColor)
        {
            if (Main.dedServ) return false;

            // Pass 1: Wide burning glow trail (CometTrailGlow shader)
            DrawGlowTrail();

            // Pass 2: Main comet body trail (CometTrailMain shader)
            DrawMainTrail();

            // Pass 3: Sparkle Trail Overlay (SparkleProjectileFoundation)
            DrawSparkleTrailOverlay();

            // Pass 4: Sparkle Accents (additive star sprites)
            DrawSparkleAccents();

            // Pass 5: Burning head orb + enhanced bloom
            DrawHeadGlow();

            return false;
        }

        private void DrawGlowTrail()
        {
            MiscShaderData glowShader = null;
            if (GameShaders.Misc.TryGetValue("MagnumOpus:CometTrailGlow", out var shader))
            {
                glowShader = shader;
                glowShader.UseColor(CometUtils.CometCoreColor);
                glowShader.UseSecondaryColor(CometUtils.CometTrail);
                glowShader.UseOpacity(0.6f);
                glowShader.UseSaturation(0.9f); // uPhase near-max (burning hot)
            }

            CometTrailRenderer.RenderTrail(Projectile.oldPos, new CometTrailSettings(
                widthFunction: (completion, _) =>
                {
                    float taper = 1f - completion;
                    return CometWidth * 3f * taper * CometUtils.SineOut(1f - completion);
                },
                colorFunction: (completion, _) =>
                {
                    Color col = Color.Lerp(CometUtils.CometCoreColor, CometUtils.DeepSpaceViolet, completion);
                    return col * 0.4f * (1f - completion * 0.4f);
                },
                offsetFunction: (_, _) => Projectile.Size * 0.5f,
                smoothen: true,
                shader: glowShader
            ), TrailLength);
        }

        private void DrawMainTrail()
        {
            MiscShaderData mainShader = null;
            if (GameShaders.Misc.TryGetValue("MagnumOpus:CometTrailMain", out var shader))
            {
                mainShader = shader;
                mainShader.UseColor(CometUtils.FrigidImpact);
                mainShader.UseSecondaryColor(CometUtils.CometCoreColor);
                mainShader.UseOpacity(0.9f);
                mainShader.UseSaturation(0.95f); // near-max phase
            }

            CometTrailRenderer.RenderTrail(Projectile.oldPos, new CometTrailSettings(
                widthFunction: (completion, _) =>
                {
                    float taper = 1f - completion;
                    return CometWidth * 1.5f * taper * CometUtils.SineOut(1f - completion);
                },
                colorFunction: (completion, _) =>
                {
                    Color col = Color.Lerp(CometUtils.FrigidImpact, CometUtils.CometTrail, completion);
                    return col * 0.9f * (1f - completion * 0.2f);
                },
                offsetFunction: (_, _) => Projectile.Size * 0.5f,
                smoothen: true,
                shader: mainShader
            ), TrailLength);
        }

        private void DrawHeadGlow()
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D bloom = CometTextures.SoftRadialBloom;
            Vector2 origin = bloom.Size() * 0.5f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            // Lunar phase influence on head glow
            Color lunarTint = CometUtils.CometCoreColor;
            if (Projectile.owner >= 0 && Projectile.owner < Main.maxPlayers && Main.player[Projectile.owner].active)
                lunarTint = Color.Lerp(CometUtils.CometCoreColor, Main.player[Projectile.owner].Resurrection().CurrentLunarColor, 0.25f);

            // Switch to Additive for bloom rendering
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

            // Outer burning glow with lunar tinting
            sb.Draw(bloom, drawPos, null, lunarTint with { A = 0 } * 0.5f, 0f, origin, 0.6f, SpriteEffects.None, 0f);

            // Mid glow layer
            sb.Draw(bloom, drawPos, null, CometUtils.CometCoreColor with { A = 0 } * 0.3f, 0f, origin, 0.4f, SpriteEffects.None, 0f);

            // White-hot core
            sb.Draw(bloom, drawPos, null, CometUtils.FrigidImpact with { A = 0 } * 0.7f, 0f, origin, 0.3f, SpriteEffects.None, 0f);

            // Foundation-enhanced: additional multi-scale bloom stacking
            Texture2D softGlow = SPFTextures.SoftGlow.Value;
            Vector2 glowOrigin = softGlow.Size() / 2f;
            float pulse = 0.85f + 0.15f * MathF.Sin(AliveTime * 0.12f);

            // Wide ice blue outer halo
            sb.Draw(softGlow, drawPos, null, new Color(120, 190, 255) with { A = 0 } * (0.12f * pulse),
                0f, glowOrigin, 0.4f, SpriteEffects.None, 0f);

            // Tight violet accent
            sb.Draw(softGlow, drawPos, null, CometUtils.ImpactCrater with { A = 0 } * (0.08f * pulse),
                0f, glowOrigin, 0.2f, SpriteEffects.None, 0f);

            // Restore to AlphaBlend
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);
        }

        // =================================================================
        // SPARKLE TRAIL OVERLAY (Foundation: SparkleProjectileFoundation)
        // =================================================================

        /// <summary>
        /// Draws a SparkleTrailShader-driven VertexStrip overlay on top of the
        /// existing CometTrail rendering. This adds crystalline glitter sparkle
        /// to the comet's wake.
        ///
        /// Uses Projectile.oldPos/oldRot trail data to build a VertexStrip.
        /// Lunar palette: DeepSpaceViolet outer, CometCoreWhite core.
        /// sparkleSpeed=3.0, sparkleScale=0.6, glitterDensity=4.0 for dense
        /// comet-appropriate sparkle frequency.
        /// </summary>
        private void DrawSparkleTrailOverlay()
        {
            SpriteBatch sb = Main.spriteBatch;

            // Count valid trail positions
            int validCount = 0;
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] != Vector2.Zero) validCount++;
                else break;
            }
            if (validCount < 4) return;

            // Build ordered position/rotation arrays
            Vector2[] positions = new Vector2[validCount];
            float[] rotations = new float[validCount];
            for (int i = 0; i < validCount; i++)
            {
                positions[i] = Projectile.oldPos[i] + Projectile.Size * 0.5f;
                rotations[i] = Projectile.oldRot[i];
            }

            // End SpriteBatch for raw vertex drawing
            sb.End();

            // Build VertexStrip
            Color StripColor(float progress)
            {
                float alpha = progress * progress;
                return Color.White * alpha;
            }

            float StripWidth(float progress)
            {
                return MathHelper.Lerp(2f, 18f, progress);
            }

            VertexStrip strip = new VertexStrip();
            strip.PrepareStrip(positions, rotations, StripColor, StripWidth,
                -Main.screenPosition, includeBacksides: true);

            // Load sparkle trail shader
            if (_sparkleTrailShader == null)
            {
                _sparkleTrailShader = ModContent.Request<Effect>(
                    "MagnumOpus/Content/FoundationWeapons/SparkleProjectileFoundation/Shaders/SparkleTrailShader",
                    AssetRequestMode.ImmediateLoad).Value;
            }

            // Load gradient LUT
            _gradientLUT ??= ModContent.Request<Texture2D>(
                "MagnumOpus/Assets/VFX Asset Library/ColorGradients/MoonlightSonataGradientLUTandRAMP");

            // Configure shader with comet parameters
            _sparkleTrailShader.Parameters["WorldViewProjection"]?.SetValue(
                Main.GameViewMatrix.NormalizedTransformationmatrix);
            _sparkleTrailShader.Parameters["uTime"]?.SetValue(
                (float)Main.timeForVisualEffects * 0.02f + AliveTime * 0.1f);

            _sparkleTrailShader.Parameters["sparkleTex"]?.SetValue(SPFTextures.SparkleHard.Value);
            _sparkleTrailShader.Parameters["gradientTex"]?.SetValue(_gradientLUT.Value);
            _sparkleTrailShader.Parameters["glowMaskTex"]?.SetValue(SPFTextures.SoftCircle.Value);

            // Lunar comet colors
            _sparkleTrailShader.Parameters["coreColor"]?.SetValue(CometUtils.CometCoreWhite.ToVector3());
            _sparkleTrailShader.Parameters["outerColor"]?.SetValue(CometUtils.DeepSpaceViolet.ToVector3());

            // Dense comet-appropriate sparkle parameters
            _sparkleTrailShader.Parameters["trailIntensity"]?.SetValue(1.2f);
            _sparkleTrailShader.Parameters["sparkleSpeed"]?.SetValue(3.0f);
            _sparkleTrailShader.Parameters["sparkleScale"]?.SetValue(0.6f);
            _sparkleTrailShader.Parameters["glitterDensity"]?.SetValue(4.0f);
            _sparkleTrailShader.Parameters["tipFadeStart"]?.SetValue(0.65f);
            _sparkleTrailShader.Parameters["edgeSoftness"]?.SetValue(0.35f);

            // Draw with shader
            _sparkleTrailShader.CurrentTechnique.Passes["SparkleTrailPass"].Apply();
            strip.DrawTrail();

            // Reset pixel shader
            Main.pixelShader.CurrentTechnique.Passes[0].Apply();

            // Restart SpriteBatch
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);
        }

        // =================================================================
        // SPARKLE ACCENTS (Foundation: SparkleProjectileFoundation pattern)
        // =================================================================

        /// <summary>
        /// Draws discrete sparkle flash sprites at trail points.
        /// 4PointedStarHard + ThinTall4PointedStar with cubic sin-wave flash timing.
        /// </summary>
        private void DrawSparkleAccents()
        {
            SpriteBatch sb = Main.spriteBatch;
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

            Texture2D starTex = SPFTextures.SparkleHard.Value;
            Texture2D thinStarTex = SPFTextures.SparkleThin.Value;
            Vector2 starOrigin = starTex.Size() / 2f;
            Vector2 thinOrigin = thinStarTex.Size() / 2f;

            float time = (float)Main.timeForVisualEffects * 0.04f;
            Color iceBlue = new(120, 190, 255);

            // Sparkle at every 3rd trail point
            for (int i = 2; i < Projectile.oldPos.Length; i += 3)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) break;

                float progress = (float)i / TrailLength;
                float fade = (1f - progress) * (1f - progress);
                if (fade < 0.02f) continue;

                Vector2 pos = Projectile.oldPos[i] + Projectile.Size * 0.5f - Main.screenPosition;

                // Cubic sin flash — brief bright peaks
                float flash = MathF.Sin(time + i * 1.7f);
                flash = flash * flash * flash;
                flash = MathF.Max(flash, 0f);

                if (flash > 0.1f)
                {
                    float scale = 0.08f + flash * 0.12f;
                    float rotation = time * 0.5f + i * 0.4f;
                    Color col = Color.Lerp(iceBlue, CometUtils.CometCoreWhite, flash) * (fade * flash * 0.6f);

                    sb.Draw(starTex, pos, null, col, rotation, starOrigin, scale, SpriteEffects.None, 0f);
                    sb.Draw(thinStarTex, pos, null, col * 0.5f, rotation + 0.4f, thinOrigin, scale * 0.7f, SpriteEffects.None, 0f);
                }
            }

            // Restore
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);
        }

        // =================================================================
        // DEATH VFX
        // =================================================================

        public override void OnKill(int timeLeft)
        {
            if (Main.dedServ) return;

            // Comet extinction burst
            CometParticleHandler.Spawn(new CraterBloomParticle(
                Projectile.Center, CometUtils.CometCoreColor, 2f, 25));

            for (int i = 0; i < 16; i++)
            {
                Vector2 vel = Main.rand.NextVector2CircularEdge(6f, 6f);
                CometParticleHandler.Spawn(new EmberTrailParticle(
                    Projectile.Center, vel, 0.5f, 20 + Main.rand.Next(12)));
            }

            for (int i = 0; i < 6; i++)
            {
                Vector2 vel = Main.rand.NextVector2CircularEdge(4f, 4f);
                CometParticleHandler.Spawn(new LunarShardParticle(
                    Projectile.Center, vel,
                    CometUtils.GetCometGradient(Main.rand.NextFloat()),
                    0.5f, 25 + Main.rand.Next(10)));
            }

            SoundEngine.PlaySound(SoundID.Item62 with { Volume = 0.6f, Pitch = 0f }, Projectile.Center);
        }
    }
}
