using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Content.OdeToJoy.Weapons.TheGardenersFury.Dusts;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.OdeToJoy;

namespace MagnumOpus.Content.OdeToJoy.Weapons.TheGardenersFury.Projectiles
{
    /// <summary>
    /// Gardener Swing Projectile — the main melee swing arc.
    /// 3 combo phases with escalating visual intensity.
    /// Uses SmearDistortShader for organic botanical swing arcs.
    /// </summary>
    public class GardenerSwingProj : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

        private const int SwingDuration = 20;
        private int timer;

        // ai[0] = combo phase (0=Sow, 1=Cultivate, 2=Harvest)
        private int ComboPhase => (int)Projectile.ai[0];

        private float SwingDirection => Projectile.ai[1] != 0 ? Projectile.ai[1] : 1f;

        // Phase-specific arc angles
        private float GetArcAngle() => ComboPhase switch
        {
            0 => MathHelper.ToRadians(160), // Wide horizontal sweep
            1 => MathHelper.ToRadians(140), // Diagonal arc
            2 => MathHelper.ToRadians(200), // Massive overhead slam
            _ => MathHelper.ToRadians(160)
        };

        // Phase-specific visual scale
        private float GetPhaseScale() => ComboPhase switch
        {
            0 => 0.7f,   // Restrained sow
            1 => 0.9f,   // Growing cultivate
            2 => 1.3f,   // Devastating harvest
            _ => 0.7f
        };

        public override void SetDefaults()
        {
            Projectile.width = 120;
            Projectile.height = 120;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = SwingDuration;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.ownerHitCheck = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = SwingDuration;
        }

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            timer++;

            // Lock to player
            Projectile.Center = owner.MountedCenter;

            // Calculate swing angle
            float progress = timer / (float)SwingDuration;
            float arcAngle = GetArcAngle();
            float baseAngle = Projectile.velocity.ToRotation();

            // CurveSegment-style swing: accelerate through middle, decel at ends
            float swingT = EaseSwing(progress);
            float currentAngle = baseAngle + (-arcAngle / 2f + arcAngle * swingT) * SwingDirection;

            Projectile.rotation = currentAngle;
            owner.heldProj = Projectile.whoAmI;

            // Player direction
            if (Math.Cos(currentAngle) > 0)
                owner.direction = 1;
            else
                owner.direction = -1;

            // Dust particles along arc — PetalFragmentDust flutter
            float alpha = GetAlpha();
            if (timer % 2 == 0 && alpha > 0.3f)
            {
                float dustDist = Main.rand.NextFloat(40f, 80f) * GetPhaseScale();
                Vector2 dustPos = owner.MountedCenter + currentAngle.ToRotationVector2() * dustDist;
                Vector2 vel = currentAngle.ToRotationVector2() * Main.rand.NextFloat(1f, 3f);

                Color col = GardenerFuryTextures.GetBotanicalGradient(Main.rand.NextFloat());
                Dust.NewDustPerfect(dustPos, ModContent.DustType<PetalFragmentDust>(), vel,
                    newColor: col, Scale: Main.rand.NextFloat(0.8f, 1.5f));
            }

            // Tip sparkles during swing — GlowSparkParticle twinkle
            if (timer % 3 == 0 && alpha > 0.3f)
            {
                float tipDist = 70f * GetPhaseScale();
                Vector2 tipPos = owner.MountedCenter + currentAngle.ToRotationVector2() * tipDist;
                Vector2 sparkVel = Main.rand.NextVector2Circular(2f, 2f);
                Color sparkCol = OdeToJoyPalette.GetGardenGradient(Main.rand.NextFloat());
                try
                {
                    var glow = new GlowSparkParticle(
                        tipPos + Main.rand.NextVector2Circular(6f, 6f),
                        sparkVel, sparkCol with { A = 0 },
                        0.2f, Main.rand.Next(12, 22));
                    MagnumParticleHandler.SpawnParticle(glow);
                }
                catch { }
            }

            // Phase 2 Harvest: PollenMistDust rising from ground
            if (ComboPhase == 2 && progress > 0.6f && timer % 3 == 0)
            {
                Vector2 groundPos = owner.MountedCenter + new Vector2(
                    Main.rand.NextFloat(-60f, 60f), 40f);
                Dust.NewDustPerfect(groundPos, ModContent.DustType<PollenMistDust>(),
                    new Vector2(0, -Main.rand.NextFloat(1f, 3f)),
                    newColor: GardenerFuryTextures.RadiantAmber,
                    Scale: Main.rand.NextFloat(0.8f, 1.4f));
            }

            // Screen shake on Harvest phase peak
            if (ComboPhase == 2 && progress > 0.65f && progress < 0.68f && Projectile.owner == Main.myPlayer)
            {
                OdeToJoyVFXLibrary.ScreenShake(5f, 10);
                OdeToJoyVFXLibrary.RhythmicPulse(owner.MountedCenter, 0.6f, OdeToJoyPalette.GoldenPollen);
            }

            // Lighting
            Lighting.AddLight(Projectile.Center,
                GardenerFuryTextures.BloomGold.ToVector3() * 0.3f * alpha);
        }

        private float EaseSwing(float t)
        {
            // Eased swing curve: fast middle, slow ends
            return t < 0.5f
                ? 2f * t * t
                : 1f - (float)Math.Pow(-2f * t + 2f, 2f) / 2f;
        }

        private float GetAlpha()
        {
            float progress = timer / (float)SwingDuration;
            float fadeIn = MathHelper.SmoothStep(0f, 1f, progress / 0.2f);
            float fadeOut = MathHelper.SmoothStep(0f, 1f, (1f - progress) / 0.25f);
            return Math.Min(fadeIn, fadeOut);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            OdeToJoyVFXLibrary.SpawnGardenSparkleExplosion(target.Center, 4 + ComboPhase * 2, 4f, 0.2f + ComboPhase * 0.1f);

            if (ComboPhase >= 2)
                OdeToJoyVFXLibrary.SpawnTriumphantStarburst(target.Center, 0.3f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            try
            {
            Player owner = Main.player[Projectile.owner];
            Vector2 drawPos = owner.MountedCenter - Main.screenPosition;
            float alpha = GetAlpha();
            float phaseScale = GetPhaseScale();

            sb.End();
            // NOTE: BlendState.Additive (SourceAlpha) for alpha-transparent arc textures.
            // TrueAdditive ignores alpha → full rectangle drawn as bright square.
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

            // ---- Layer 1: Smear Arc with shader ----
            Texture2D arcTex = GardenerFuryTextures.SwordArcSmear.Value;
            Vector2 arcOrigin = new Vector2(
                SwingDirection > 0 ? 0 : arcTex.Width,
                arcTex.Height / 2f);

            Effect shader = GardenerFuryTextures.SmearDistortShader;
            if (shader != null)
            {
                try
                {
                    shader.Parameters["uTime"]?.SetValue((float)Main.gameTimeCache.TotalGameTime.TotalSeconds);
                    shader.Parameters["fadeAlpha"]?.SetValue(alpha);
                    shader.Parameters["distortStrength"]?.SetValue(0.05f * phaseScale);
                    shader.Parameters["flowSpeed"]?.SetValue(0.4f);
                    shader.Parameters["noiseScale"]?.SetValue(2.8f);

                    if (GardenerFuryTextures.FBMNoise?.Value != null)
                        Main.graphics.GraphicsDevice.Textures[1] = GardenerFuryTextures.FBMNoise.Value;
                    if (GardenerFuryTextures.GradOdeToJoy?.Value != null)
                        Main.graphics.GraphicsDevice.Textures[2] = GardenerFuryTextures.GradOdeToJoy.Value;

                    sb.End();
                    sb.Begin(SpriteSortMode.Immediate, BlendState.Additive,
                        Main.DefaultSamplerState, DepthStencilState.None,
                        RasterizerState.CullCounterClockwise,
                        shader, Main.GameViewMatrix.TransformationMatrix);
                }
                catch
                {
                    sb.End();
                    sb.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                        Main.DefaultSamplerState, DepthStencilState.None,
                        RasterizerState.CullCounterClockwise, null,
                        Main.GameViewMatrix.TransformationMatrix);
                }
            }

            // 3-layer smear arc (outer, main, core)
            Color outerCol = GardenerFuryTextures.RoseShadow;
            Color mainCol = GardenerFuryTextures.BloomGold;
            Color coreCol = GardenerFuryTextures.RadiantAmber;

            if (ComboPhase == 2)
            {
                outerCol = GardenerFuryTextures.RadiantAmber;
                mainCol = GardenerFuryTextures.JubilantLight;
                coreCol = GardenerFuryTextures.PureJoyWhite;
            }

            SpriteEffects flip = SwingDirection > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            sb.Draw(arcTex, drawPos, null, outerCol * alpha * 0.5f,
                Projectile.rotation, arcOrigin, phaseScale * 1.15f, flip, 0f);
            sb.Draw(arcTex, drawPos, null, mainCol * alpha * 0.7f,
                Projectile.rotation, arcOrigin, phaseScale, flip, 0f);
            sb.Draw(arcTex, drawPos, null, coreCol * alpha * 0.5f,
                Projectile.rotation, arcOrigin, phaseScale * 0.85f, flip, 0f);

            // Return to additive (if used shader, it was immediate)
            if (shader != null)
            {
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                    Main.DefaultSamplerState, DepthStencilState.None,
                    RasterizerState.CullCounterClockwise, null,
                    Main.GameViewMatrix.TransformationMatrix);
            }

            // ---- Layer 1.5: VerdantSlash botanical vine aura overlay ----
            Effect verdantShader = OdeToJoyShaders.VerdantSlash;
            if (verdantShader != null)
            {
                OdeToJoyShaders.SetSlashParams(verdantShader, (float)Main.gameTimeCache.TotalGameTime.TotalSeconds, mainCol, outerCol, alpha * 0.35f, 0.7f * phaseScale, (float)ComboPhase / 2f);
                // Inline BeginShaderBatch with BlendState.Additive for alpha-transparent arc texture
                if (verdantShader.Techniques["VerdantSlashTechnique"] != null)
                    verdantShader.CurrentTechnique = verdantShader.Techniques["VerdantSlashTechnique"];
                sb.End();
                sb.Begin(SpriteSortMode.Immediate, BlendState.Additive,
                    SamplerState.LinearWrap, DepthStencilState.None,
                    RasterizerState.CullCounterClockwise, null,
                    Main.GameViewMatrix.TransformationMatrix);

                sb.Draw(arcTex, drawPos, null, mainCol * alpha * 0.3f,
                    Projectile.rotation + 0.04f, arcOrigin, phaseScale * 1.08f, flip, 0f);

                OdeToJoyShaders.BeginAdditiveBatch(sb);
            }

            // ---- Layer 2: Enhanced tip glow with GardenBloom ----
            Texture2D softGlow = GardenerFuryTextures.SoftGlow.Value;
            Vector2 glowOrigin = softGlow.Size() / 2f;
            Vector2 tipPos = drawPos + Projectile.rotation.ToRotationVector2() * 70f * phaseScale;

            Effect gardenShader = OdeToJoyShaders.GardenBloom;
            if (gardenShader != null)
            {
                OdeToJoyShaders.SetBloomParams(gardenShader, (float)Main.gameTimeCache.TotalGameTime.TotalSeconds, GardenerFuryTextures.BloomGold, GardenerFuryTextures.PetalPink, alpha * 0.6f, 1.2f * phaseScale, 0.5f);
                OdeToJoyShaders.BeginShaderBatch(sb, gardenShader, "JubilantPulseTechnique");

                sb.Draw(softGlow, tipPos, null,
                    GardenerFuryTextures.BloomGold * alpha * 0.6f,
                    0f, glowOrigin, 0.14f * phaseScale, SpriteEffects.None, 0f);

                OdeToJoyShaders.BeginAdditiveBatch(sb);
            }

            // Additive tip bloom overlays
            sb.Draw(softGlow, tipPos, null,
                GardenerFuryTextures.BloomGold * alpha * 0.5f,
                0f, glowOrigin, 0.12f * phaseScale, SpriteEffects.None, 0f);
            sb.Draw(softGlow, tipPos, null,
                coreCol * alpha * 0.3f,
                0f, glowOrigin, 0.08f * phaseScale, SpriteEffects.None, 0f);

            // ---- Layer 3: Root glow (subtle) ----
            sb.Draw(softGlow, drawPos, null,
                GardenerFuryTextures.PetalPink * alpha * 0.25f,
                0f, glowOrigin, 0.06f, SpriteEffects.None, 0f);

            // Theme blossom sparkle accent
            OdeToJoyVFXLibrary.DrawThemeBlossomSparkle(sb, Projectile.Center, 1f, 0.5f);

            OdeToJoyShaders.RestoreSpriteBatch(sb);

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

    /// <summary>
    /// Small Petal — homing petals released during harvest phase hits.
    /// Petal Pink trailing particles with rose petal texture.
    /// </summary>
    public class SmallPetalProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

        private VertexStrip _vertexStrip;
        private int timer;
        private const int MaxLifetime = 120;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 16;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 1;
            Projectile.timeLeft = MaxLifetime;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
            timer++;
            Projectile.rotation += 0.15f;

            // Gentle homing after 15 frames
            if (timer > 15)
            {
                NPC target = FindClosestNPC(400f);
                if (target != null)
                {
                    Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity,
                        toTarget * 10f, 0.06f);
                }
            }

            // Trailing petal particles — PetalFragmentDust
            if (timer % 3 == 0)
            {
                Vector2 vel = -Projectile.velocity * 0.05f + Main.rand.NextVector2Circular(0.5f, 0.5f);
                Color col = Color.Lerp(GardenerFuryTextures.PetalPink,
                    GardenerFuryTextures.BloomGold, Main.rand.NextFloat());
                Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<PetalFragmentDust>(), vel,
                    newColor: col, Scale: Main.rand.NextFloat(0.6f, 1.2f));
            }

            float a = GetAlpha();
            Lighting.AddLight(Projectile.Center,
                GardenerFuryTextures.PetalPink.ToVector3() * 0.2f * a);
        }

        private NPC FindClosestNPC(float maxDist)
        {
            NPC closest = null;
            float closestDist = maxDist * maxDist;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage) continue;
                float distSq = Vector2.DistanceSquared(Projectile.Center, npc.Center);
                if (distSq < closestDist)
                {
                    closestDist = distSq;
                    closest = npc;
                }
            }
            return closest;
        }

        private float GetAlpha()
        {
            float fadeIn = MathHelper.Clamp(timer / 8f, 0f, 1f);
            float fadeOut = Projectile.timeLeft < 20 ? Projectile.timeLeft / 20f : 1f;
            return fadeIn * fadeOut;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            try
            {
                IncisorOrbRenderer.DrawOrbVisuals(sb, Projectile, IncisorOrbRenderer.OdeToJoy, ref _vertexStrip);
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

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Impact burst — PetalFragmentDust
            for (int i = 0; i < 5; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(3f, 3f);
                Dust.NewDustPerfect(target.Center, ModContent.DustType<PetalFragmentDust>(), vel,
                    newColor: GardenerFuryTextures.PetalPink,
                    Scale: Main.rand.NextFloat(0.8f, 1.4f));
            }

            OdeToJoyVFXLibrary.SpawnGardenSparkleExplosion(target.Center, 4, 3f, 0.15f);
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 4; i++)
            {
                Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<PetalFragmentDust>(),
                    Main.rand.NextVector2Circular(2f, 2f),
                    newColor: GardenerFuryTextures.PetalPink,
                    Scale: Main.rand.NextFloat(0.6f, 1.0f));
            }
        }
    }

    /// <summary>
    /// Jubilant Petal — larger petal from Botanical Barrage, more damage + debuffs.
    /// </summary>
    public class JubilantPetalProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

        private VertexStrip _vertexStrip;
        private int timer;
        private const int MaxLifetime = 90;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 16;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = MaxLifetime;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 12;
        }

        public override void AI()
        {
            timer++;
            Projectile.rotation += 0.1f;

            // Spiral motion
            float spiralSpeed = 0.04f;
            Projectile.velocity = Projectile.velocity.RotatedBy(spiralSpeed);

            // Jubilant sparkle trail — PollenMistDust
            if (timer % 2 == 0)
            {
                Vector2 vel = -Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(1f, 1f);
                Color col = Color.Lerp(GardenerFuryTextures.JubilantLight,
                    GardenerFuryTextures.BloomGold, Main.rand.NextFloat());
                Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<PollenMistDust>(), vel,
                    newColor: col, Scale: Main.rand.NextFloat(0.6f, 1.0f));
            }

            float a = GetAlpha();
            Lighting.AddLight(Projectile.Center,
                GardenerFuryTextures.JubilantLight.ToVector3() * 0.3f * a);
        }

        private float GetAlpha()
        {
            float fadeIn = MathHelper.Clamp(timer / 6f, 0f, 1f);
            float fadeOut = Projectile.timeLeft < 15 ? Projectile.timeLeft / 15f : 1f;
            return fadeIn * fadeOut;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            try
            {
                IncisorOrbRenderer.DrawOrbVisuals(sb, Projectile, IncisorOrbRenderer.OdeToJoy, ref _vertexStrip);
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

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Confused, 60);

            for (int i = 0; i < 6; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(4f, 4f);
                Color col = Color.Lerp(GardenerFuryTextures.JubilantLight,
                    GardenerFuryTextures.PureJoyWhite, Main.rand.NextFloat());
                Dust.NewDustPerfect(target.Center, ModContent.DustType<PetalFragmentDust>(), vel,
                    newColor: col, Scale: Main.rand.NextFloat(1.0f, 1.8f));
            }

            OdeToJoyVFXLibrary.SpawnGardenSparkleExplosion(target.Center, 6, 4f, 0.2f);
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 6; i++)
            {
                Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<PollenMistDust>(),
                    Main.rand.NextVector2Circular(2f, 2f),
                    newColor: GardenerFuryTextures.BloomGold,
                    Scale: Main.rand.NextFloat(0.6f, 1.0f));
            }

            OdeToJoyVFXLibrary.RhythmicPulse(Projectile.Center, 0.3f, OdeToJoyPalette.GoldenPollen);
        }
    }
}
