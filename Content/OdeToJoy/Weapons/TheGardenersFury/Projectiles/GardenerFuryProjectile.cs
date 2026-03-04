using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
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

            // Dust particles along arc
            float alpha = GetAlpha();
            if (timer % 2 == 0 && alpha > 0.3f)
            {
                float dustDist = Main.rand.NextFloat(40f, 80f) * GetPhaseScale();
                Vector2 dustPos = owner.MountedCenter + currentAngle.ToRotationVector2() * dustDist;
                Vector2 vel = currentAngle.ToRotationVector2() * Main.rand.NextFloat(1f, 3f);

                Color col = GardenerFuryTextures.GetBotanicalGradient(Main.rand.NextFloat());
                Dust dust = Dust.NewDustPerfect(dustPos, DustID.RainbowMk2, vel,
                    newColor: col, Scale: Main.rand.NextFloat(0.3f, 0.6f));
                dust.noGravity = true;
                dust.fadeIn = 0.3f;
            }

            // Phase 2 Harvest: extra ground-shake dust
            if (ComboPhase == 2 && progress > 0.6f && timer % 3 == 0)
            {
                Vector2 groundPos = owner.MountedCenter + new Vector2(
                    Main.rand.NextFloat(-60f, 60f), 40f);
                Dust dust = Dust.NewDustPerfect(groundPos, DustID.RainbowMk2,
                    new Vector2(0, -Main.rand.NextFloat(1f, 3f)),
                    newColor: GardenerFuryTextures.RadiantAmber,
                    Scale: Main.rand.NextFloat(0.4f, 0.7f));
                dust.noGravity = true;
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

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Player owner = Main.player[Projectile.owner];
            Vector2 drawPos = owner.MountedCenter - Main.screenPosition;
            float alpha = GetAlpha();
            float phaseScale = GetPhaseScale();

            sb.End();
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
                sb.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                    Main.DefaultSamplerState, DepthStencilState.None,
                    RasterizerState.CullCounterClockwise, null,
                    Main.GameViewMatrix.TransformationMatrix);
            }

            // ---- Layer 1.5: VerdantSlash botanical vine aura overlay ----
            Effect verdantShader = OdeToJoyShaders.VerdantSlash;
            if (verdantShader != null)
            {
                OdeToJoyShaders.SetSlashParams(verdantShader, (float)Main.gameTimeCache.TotalGameTime.TotalSeconds, mainCol, outerCol, alpha * 0.35f, 0.7f * phaseScale, (float)ComboPhase / 2f);
                OdeToJoyShaders.BeginShaderBatch(sb, verdantShader, "VerdantSlashTechnique");

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

        private int timer;
        private const int MaxLifetime = 120;

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

            // Trailing petal particles
            if (timer % 3 == 0)
            {
                Vector2 vel = -Projectile.velocity * 0.05f + Main.rand.NextVector2Circular(0.5f, 0.5f);
                Color col = Color.Lerp(GardenerFuryTextures.PetalPink,
                    GardenerFuryTextures.BloomGold, Main.rand.NextFloat());
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.RainbowMk2, vel,
                    newColor: col, Scale: Main.rand.NextFloat(0.2f, 0.4f));
                dust.noGravity = true;
                dust.fadeIn = 0.3f;
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
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float alpha = GetAlpha();

            // ---- Layer 1: GardenBloom shader petal accent ----
            Effect gardenShader = OdeToJoyShaders.GardenBloom;
            if (gardenShader != null)
            {
                OdeToJoyShaders.SetBloomParams(gardenShader, (float)Main.gameTimeCache.TotalGameTime.TotalSeconds, GardenerFuryTextures.PetalPink, GardenerFuryTextures.BloomGold, alpha * 0.7f, 0.8f, 0.4f);
                OdeToJoyShaders.BeginShaderBatch(sb, gardenShader, "GardenBloomTechnique");

                Texture2D petalShader = GardenerFuryTextures.OJRosePetal.Value;
                Vector2 petalShaderOrigin = petalShader.Size() / 2f;
                sb.Draw(petalShader, drawPos, null,
                    GardenerFuryTextures.PetalPink * alpha * 0.6f,
                    Projectile.rotation, petalShaderOrigin, 0.55f, SpriteEffects.None, 0f);

                OdeToJoyShaders.BeginAdditiveBatch(sb);
            }
            else
            {
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                    Main.DefaultSamplerState, DepthStencilState.None,
                    RasterizerState.CullCounterClockwise, null,
                    Main.GameViewMatrix.TransformationMatrix);
            }

            // ---- Layer 2: Rose petal body ----
            Texture2D petalTex = GardenerFuryTextures.OJRosePetal.Value;
            Vector2 petalOrigin = petalTex.Size() / 2f;
            sb.Draw(petalTex, drawPos, null,
                GardenerFuryTextures.PetalPink * alpha * 0.8f,
                Projectile.rotation, petalOrigin, 0.5f, SpriteEffects.None, 0f);

            // ---- Layer 3: Soft bloom halo ----
            Texture2D glow = GardenerFuryTextures.SoftGlow.Value;
            Vector2 glowOrigin = glow.Size() / 2f;
            sb.Draw(glow, drawPos, null,
                GardenerFuryTextures.PetalPink * alpha * 0.35f,
                0f, glowOrigin, 0.07f, SpriteEffects.None, 0f);
            sb.Draw(glow, drawPos, null,
                GardenerFuryTextures.BloomGold * alpha * 0.2f,
                0f, glowOrigin, 0.04f, SpriteEffects.None, 0f);

            // Theme blossom sparkle accent
            OdeToJoyVFXLibrary.DrawThemeBlossomSparkle(sb, Projectile.Center, 1f, 0.5f);

            OdeToJoyShaders.RestoreSpriteBatch(sb);

            return false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Impact burst
            for (int i = 0; i < 5; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(3f, 3f);
                Dust dust = Dust.NewDustPerfect(target.Center, DustID.RainbowMk2, vel,
                    newColor: GardenerFuryTextures.PetalPink,
                    Scale: Main.rand.NextFloat(0.3f, 0.6f));
                dust.noGravity = true;
            }
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 4; i++)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.RainbowMk2,
                    Main.rand.NextVector2Circular(2f, 2f),
                    newColor: GardenerFuryTextures.PetalPink,
                    Scale: Main.rand.NextFloat(0.2f, 0.4f));
                dust.noGravity = true;
            }
        }
    }

    /// <summary>
    /// Jubilant Petal — larger petal from Botanical Barrage, more damage + debuffs.
    /// </summary>
    public class JubilantPetalProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

        private int timer;
        private const int MaxLifetime = 90;

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

            // Jubilant sparkle trail
            if (timer % 2 == 0)
            {
                Vector2 vel = -Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(1f, 1f);
                Color col = Color.Lerp(GardenerFuryTextures.JubilantLight,
                    GardenerFuryTextures.BloomGold, Main.rand.NextFloat());
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.RainbowMk2, vel,
                    newColor: col, Scale: Main.rand.NextFloat(0.3f, 0.6f));
                dust.noGravity = true;
                dust.fadeIn = 0.4f;
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
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float alpha = GetAlpha();

            // ---- Layer 1: GardenBloom shader jubilant shimmer ----
            Effect gardenShader = OdeToJoyShaders.GardenBloom;
            if (gardenShader != null)
            {
                OdeToJoyShaders.SetBloomParams(gardenShader, (float)Main.gameTimeCache.TotalGameTime.TotalSeconds, GardenerFuryTextures.JubilantLight, GardenerFuryTextures.BloomGold, alpha * 0.75f, 1.1f, 0.5f);
                OdeToJoyShaders.BeginShaderBatch(sb, gardenShader, "JubilantPulseTechnique");

                Texture2D petalShader = GardenerFuryTextures.OJRosePetal.Value;
                Vector2 petalShaderOrigin = petalShader.Size() / 2f;
                sb.Draw(petalShader, drawPos, null,
                    GardenerFuryTextures.JubilantLight * alpha * 0.65f,
                    Projectile.rotation, petalShaderOrigin, 0.75f, SpriteEffects.None, 0f);

                OdeToJoyShaders.BeginAdditiveBatch(sb);
            }
            else
            {
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                    Main.DefaultSamplerState, DepthStencilState.None,
                    RasterizerState.CullCounterClockwise, null,
                    Main.GameViewMatrix.TransformationMatrix);
            }

            // ---- Layer 2: Jubilant petal body ----
            Texture2D petalTex = GardenerFuryTextures.OJRosePetal.Value;
            Vector2 petalOrigin = petalTex.Size() / 2f;
            sb.Draw(petalTex, drawPos, null,
                GardenerFuryTextures.JubilantLight * alpha * 0.85f,
                Projectile.rotation, petalOrigin, 0.7f, SpriteEffects.None, 0f);

            // ---- Layer 3: Golden bloom overlays ----
            Texture2D glow = GardenerFuryTextures.SoftGlow.Value;
            Vector2 glowOrigin = glow.Size() / 2f;
            sb.Draw(glow, drawPos, null,
                GardenerFuryTextures.BloomGold * alpha * 0.45f,
                0f, glowOrigin, 0.12f, SpriteEffects.None, 0f);
            sb.Draw(glow, drawPos, null,
                GardenerFuryTextures.PureJoyWhite * alpha * 0.25f,
                0f, glowOrigin, 0.06f, SpriteEffects.None, 0f);

            // Theme blossom sparkle accent
            OdeToJoyVFXLibrary.DrawThemeBlossomSparkle(sb, Projectile.Center, 1f, 0.5f);

            OdeToJoyShaders.RestoreSpriteBatch(sb);

            return false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Confused, 60);

            for (int i = 0; i < 8; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(4f, 4f);
                Color col = Color.Lerp(GardenerFuryTextures.JubilantLight,
                    GardenerFuryTextures.PureJoyWhite, Main.rand.NextFloat());
                Dust dust = Dust.NewDustPerfect(target.Center, DustID.RainbowMk2, vel,
                    newColor: col, Scale: Main.rand.NextFloat(0.4f, 0.8f));
                dust.noGravity = true;
            }
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 6; i++)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.RainbowMk2,
                    Main.rand.NextVector2Circular(2f, 2f),
                    newColor: GardenerFuryTextures.BloomGold,
                    Scale: Main.rand.NextFloat(0.3f, 0.5f));
                dust.noGravity = true;
            }
        }
    }
}