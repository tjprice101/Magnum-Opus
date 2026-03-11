using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.OdeToJoy;
using MagnumOpus.Content.OdeToJoy.Weapons.TheGardenersFury.Buffs;
using MagnumOpus.Content.OdeToJoy.Weapons.TheGardenersFury.Dusts;
using MagnumOpus.Content.OdeToJoy.Weapons.ThornboundReckoning.Buffs;

namespace MagnumOpus.Content.OdeToJoy.Weapons.TheGardenersFury.Projectiles
{
    /// <summary>
    /// Seed Pod — embeds in ground, grows over time, detonates in cascade.
    /// 3 pod types: Bloom (petal burst), Thorn (shrapnel), Pollen (slow cloud).
    /// Uses RadialNoiseMaskShader for organic pod body rendering.
    /// ai[0] = pod type (0=Bloom, 1=Thorn, 2=Pollen)
    /// ai[1] = rain pod flag (1 = detonate on landing)
    /// ai[2] = detonate flag (set by weapon's harvest phase)
    /// localAI[0] = cascade delay timer
    /// </summary>
    public class SeedPodProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

        private const int MaxLifetime = 600; // 10 seconds if not harvested
        private const int GrowthThreshold = 120; // 2 seconds for growth bonus
        private int timer;
        private float pulsePhase;
        private bool hasDetonated;
        private bool isEmbedded;
        private float growthScale = 1f;

        private int PodType => (int)Projectile.ai[0];
        private bool IsRainPod => Projectile.ai[1] >= 1f;
        private bool ShouldDetonate => Projectile.ai[2] >= 1f;

        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = MaxLifetime;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = MaxLifetime;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (!isEmbedded)
            {
                isEmbedded = true;
                Projectile.velocity = Vector2.Zero;
                Projectile.tileCollide = false;

                // Embed particles — PollenMistDust burst
                for (int i = 0; i < 6; i++)
                {
                    Vector2 vel = new Vector2(Main.rand.NextFloat(-2f, 2f), -Main.rand.NextFloat(1f, 3f));
                    Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<PollenMistDust>(), vel,
                        newColor: GardenerFuryTextures.GetPodColor(PodType),
                        Scale: Main.rand.NextFloat(0.5f, 1.0f));
                }

                // Rain pods detonate on landing
                if (IsRainPod)
                    Projectile.ai[2] = 1f;
            }
            return false;
        }

        public override void AI()
        {
            timer++;
            pulsePhase += 0.08f;

            // Gravity for non-embedded pods (rain pods fall)
            if (!isEmbedded)
            {
                Projectile.velocity.Y += 0.3f;
                Projectile.rotation += Projectile.velocity.X * 0.05f;
            }
            else
            {
                Projectile.velocity = Vector2.Zero;
            }

            // Growth over time
            if (isEmbedded && timer > GrowthThreshold)
            {
                growthScale = MathHelper.Lerp(growthScale, 1.3f, 0.01f);
            }

            // Check for detonation
            if (ShouldDetonate && !hasDetonated)
            {
                if (Projectile.localAI[0] > 0)
                {
                    Projectile.localAI[0]--;
                    return; // Wait for cascade delay
                }

                Detonate();
                return;
            }

            // Ambient glow
            float alpha = GetAlpha();
            Color podColor = GardenerFuryTextures.GetPodColor(PodType);
            Lighting.AddLight(Projectile.Center, podColor.ToVector3() * 0.2f * alpha);

            // Pulsing particles for embedded pods — PollenMistDust
            if (isEmbedded && timer % 8 == 0)
            {
                Vector2 vel = new Vector2(0, -Main.rand.NextFloat(0.3f, 0.8f));
                Dust.NewDustPerfect(
                    Projectile.Center + Main.rand.NextVector2Circular(8f * growthScale, 8f * growthScale),
                    ModContent.DustType<PollenMistDust>(), vel,
                    newColor: podColor, Scale: Main.rand.NextFloat(0.4f, 0.8f));
            }
        }

        private void Detonate()
        {
            hasDetonated = true;

            // Determine detonation properties based on pod type
            float detonationRadius = 64f * growthScale; // 4 tiles base
            float damageMult = timer > GrowthThreshold ? 1.2f : 1f;
            Color podColor = GardenerFuryTextures.GetPodColor(PodType);

            // Spawn detonation VFX projectile
            Player owner = Main.player[Projectile.owner];
            var source = owner.GetSource_FromThis();

            Projectile.NewProjectile(source, Projectile.Center, Vector2.Zero,
                ModContent.ProjectileType<PodDetonationProjectile>(),
                (int)(Projectile.damage * damageMult), Projectile.knockBack,
                Projectile.owner, ai0: PodType, ai1: detonationRadius);

            // Apply debuff based on pod type
            // (Done by PodDetonationProjectile on hit)

            SoundEngine.PlaySound(SoundID.Item14 with { Pitch = 0.3f + PodType * 0.15f, Volume = 0.5f },
                Projectile.Center);

            // Explosion particles — PetalFragmentDust + PollenMistDust FountainCascade
            int sparkCount = (int)(55 * growthScale);
            Color[] debrisColors = GetDebrisColors();

            for (int i = 0; i < sparkCount; i++)
            {
                float angle = MathHelper.TwoPi / sparkCount * i + Main.rand.NextFloat(-0.1f, 0.1f);
                float speed = Main.rand.NextFloat(2f, 7f);
                Vector2 vel = new Vector2(
                    (float)Math.Cos(angle) * speed,
                    -Math.Abs((float)Math.Sin(angle)) * speed * 1.5f - Main.rand.NextFloat(1f, 3f));

                Color col = debrisColors[i % debrisColors.Length];
                int dustType = i % 3 == 0 ? ModContent.DustType<PollenMistDust>() : ModContent.DustType<PetalFragmentDust>();
                Dust.NewDustPerfect(Projectile.Center, dustType, vel,
                    newColor: col, Scale: Main.rand.NextFloat(0.8f, 1.8f));
            }

            // Screen effects on detonation
            OdeToJoyVFXLibrary.ScreenShake(4f + growthScale, 8);
            OdeToJoyVFXLibrary.SpawnGardenSparkleExplosion(Projectile.Center, 8, 5f, 0.25f);
            OdeToJoyVFXLibrary.HarmonicPulseRing(Projectile.Center, detonationRadius, 10, podColor, 2.5f);

            Projectile.Kill();
        }

        private Color[] GetDebrisColors()
        {
            return PodType switch
            {
                0 => new[] { GardenerFuryTextures.PetalPink, GardenerFuryTextures.JubilantLight,
                             GardenerFuryTextures.PureJoyWhite },
                1 => new[] { GardenerFuryTextures.RoseShadow, GardenerFuryTextures.RadiantAmber,
                             GardenerFuryTextures.PetalPink },
                2 => new[] { GardenerFuryTextures.BloomGold, GardenerFuryTextures.PureJoyWhite,
                             GardenerFuryTextures.RadiantAmber },
                _ => new[] { GardenerFuryTextures.BloomGold }
            };
        }

        private float GetAlpha()
        {
            float fadeIn = MathHelper.Clamp(timer / 10f, 0f, 1f);
            float fadeOut = Projectile.timeLeft < 30 ? Projectile.timeLeft / 30f : 1f;
            return fadeIn * fadeOut;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            try
            {
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float alpha = GetAlpha();
            Color podColor = GardenerFuryTextures.GetPodColor(PodType);
            float pulse = 0.85f + 0.15f * (float)Math.Sin(pulsePhase);

            // ---- Layer 1: PollenDrift shader pod aura ----
            Effect pollenShader = OdeToJoyShaders.PollenDrift;
            if (pollenShader != null)
            {
                OdeToJoyShaders.SetPollenParams(pollenShader, (float)Main.gameTimeCache.TotalGameTime.TotalSeconds, podColor, GardenerFuryTextures.BloomGold, alpha * 0.5f * pulse, 0.8f * growthScale);
                OdeToJoyShaders.BeginShaderBatch(sb, pollenShader, "PollenTrailTechnique");

                Texture2D auraGlow = GardenerFuryTextures.SoftGlow.Value;
                Vector2 auraOrigin = auraGlow.Size() / 2f;
                sb.Draw(auraGlow, drawPos, null, podColor * alpha * 0.4f * pulse,
                    pulsePhase * 0.2f, auraOrigin, 0.08f * growthScale, SpriteEffects.None, 0f);

                OdeToJoyShaders.BeginAdditiveBatch(sb);
            }
            else
            {
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                    Main.DefaultSamplerState, DepthStencilState.None,
                    RasterizerState.CullCounterClockwise, null,
                    Main.GameViewMatrix.TransformationMatrix);
            }

            Texture2D softGlow = GardenerFuryTextures.SoftGlow.Value;
            Vector2 glowOrigin = softGlow.Size() / 2f;

            // ---- Layer 2: Pod body glow (orb-like) ----
            float bodyScale = 0.06f * growthScale * pulse;
            sb.Draw(softGlow, drawPos, null, podColor * alpha * 0.7f,
                0f, glowOrigin, bodyScale, SpriteEffects.None, 0f);

            // ---- Layer 3: Core bright spot ----
            float coreScale = bodyScale * 0.5f;
            sb.Draw(softGlow, drawPos, null,
                GardenerFuryTextures.PureJoyWhite * alpha * 0.5f * pulse,
                0f, glowOrigin, coreScale, SpriteEffects.None, 0f);

            // ---- Layer 4: Outer halo ----
            float haloScale = bodyScale * 2f;
            sb.Draw(softGlow, drawPos, null, podColor * alpha * 0.2f,
                0f, glowOrigin, haloScale, SpriteEffects.None, 0f);

            // ---- Layer 5: Blossom sparkle accent for grown pods ----
            if (timer > GrowthThreshold)
            {
                Texture2D sparkle = GardenerFuryTextures.OJBlossomSparkle.Value;
                Vector2 sparkleOrigin = sparkle.Size() / 2f;
                float sparkleScale = 0.35f * growthScale;

                sb.Draw(sparkle, drawPos, null,
                    GardenerFuryTextures.JubilantLight * alpha * 0.4f * pulse,
                    pulsePhase * 0.3f, sparkleOrigin, sparkleScale,
                    SpriteEffects.None, 0f);
            }

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
}
