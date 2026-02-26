using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Terraria.GameContent;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Core;
using MagnumOpus.Common.Systems.VFX.Trails;
using MagnumOpus.Content.MoonlightSonata;
using MagnumOpus.Content.MoonlightSonata.Dusts;
using MagnumOpus.Content.MoonlightSonata.Weapons.ResurrectionOfTheMoon;

namespace MagnumOpus.Content.MoonlightSonata.Projectiles
{
    /// <summary>
    /// CometCore — Penetrating heavy round for Resurrection of the Moon's Chamber mechanic.
    ///
    /// Visual: A white-hot comet core that burns through multiple enemies without stopping.
    /// Does not ricochet — instead pierces through up to 5 enemies with increasing heat.
    /// Leaves a dense ember wake trail and detonates on tile collision or expiry.
    ///
    /// Chamber slot 2 ammo: High penetration, lower per-hit damage but hits everything in its path.
    /// Uses CometTrail.fx shader for GPU-rendered burning ember trail.
    /// </summary>
    public class CometCore : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow3";

        private int hitCount = 0;
        private const int MaxPenetrations = 5;

        /// <summary>Heat phase for shader — hitCount / MaxPenetrations (0 = cold, 1 = white-hot).</summary>
        private float HeatPhase => (float)hitCount / MaxPenetrations;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 28;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = MaxPenetrations + 1;
            Projectile.timeLeft = 240;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 3;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();

            // Dense comet ember trail
            ResurrectionVFX.CometTrailFrame(Projectile.Center, Projectile.velocity, hitCount);

            // Orbiting CometEmberDust ring (every 4 frames) — tighter orbit for penetrating round
            if ((int)Projectile.ai[0] % 4 == 0)
            {
                float orbitPhase = Projectile.ai[0] * 0.3f;
                for (int i = 0; i < 2; i++)
                {
                    float angle = orbitPhase + MathHelper.Pi * i;
                    float radius = 6f + MathF.Sin(orbitPhase + i) * 2f;
                    Vector2 orbitPos = Projectile.Center + angle.ToRotationVector2() * radius;
                    Color orbitColor = ResurrectionVFX.GetCometColor((float)i / 2f, hitCount + 2);
                    Dust ember = Dust.NewDustPerfect(orbitPos,
                        ModContent.DustType<CometEmberDust>(),
                        -Projectile.velocity * 0.05f, 0, orbitColor,
                        0.2f + hitCount * 0.03f);
                    ember.customData = new CometEmberBehavior
                    {
                        VelocityDecay = 0.94f,
                        RotationSpeed = 0.08f,
                        BaseScale = 0.2f + hitCount * 0.03f,
                        Lifetime = 18,
                        HasGravity = false
                    };
                }
            }

            // Dense StarPointDust sparks — more frequent than standard round
            if ((int)Projectile.ai[0] % 3 == 0)
            {
                Color sparkColor = Color.Lerp(ResurrectionVFX.LunarShine,
                    ResurrectionVFX.CometCore, 0.5f + hitCount * 0.1f);
                Dust star = Dust.NewDustPerfect(
                    Projectile.Center + Main.rand.NextVector2Circular(3f, 3f),
                    ModContent.DustType<StarPointDust>(),
                    -Projectile.velocity * 0.04f, 0, sparkColor, 0.16f);
                star.customData = new StarPointBehavior
                {
                    RotationSpeed = 0.15f,
                    TwinkleFrequency = 0.6f,
                    Lifetime = 14,
                    FadeStartTime = 4
                };
            }

            if ((int)Projectile.ai[0] % 6 == 0)
                MoonlightVFXLibrary.SpawnMusicNotes(Projectile.Center, 1, 4f, 0.7f, 0.85f, 22);

            Projectile.ai[0]++;

            // Intense comet lighting — brighter than standard round
            float lightIntensity = 0.8f + hitCount * 0.15f;
            ResurrectionVFX.AddCometLight(Projectile.Center, lightIntensity, hitCount + 2);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<Debuffs.MusicsDissonance>(), 240 + hitCount * 40);
            hitCount++;

            // Through-hit VFX — flash + ember spray without the full crater treatment
            float intensity = 0.7f + hitCount * 0.2f;
            CustomParticles.GenericFlare(target.Center, Color.White, 0.5f * intensity, 12);
            CustomParticles.GenericFlare(target.Center, ResurrectionVFX.LunarShine, 0.4f * intensity, 14);

            // CometEmberDust spray in direction of travel
            for (int i = 0; i < 4 + hitCount; i++)
            {
                Vector2 vel = Projectile.velocity.SafeNormalize(Vector2.Zero).RotatedByRandom(0.6f)
                    * Main.rand.NextFloat(3f, 7f);
                Dust ember = Dust.NewDustPerfect(target.Center,
                    ModContent.DustType<CometEmberDust>(),
                    vel, 0, ResurrectionVFX.CometCore, 0.25f * intensity);
                ember.customData = new CometEmberBehavior
                {
                    VelocityDecay = 0.93f,
                    RotationSpeed = 0.1f,
                    BaseScale = 0.25f * intensity,
                    Lifetime = 20,
                    HasGravity = true
                };
            }

            // ResonantPulseDust mini-ring
            Dust pulse = Dust.NewDustPerfect(target.Center,
                ModContent.DustType<ResonantPulseDust>(),
                Vector2.Zero, 0, ResurrectionVFX.ImpactCrater, 0.18f + hitCount * 0.03f);
            pulse.customData = new ResonantPulseBehavior(0.03f + hitCount * 0.005f, 12);

            CustomParticles.HaloRing(target.Center, ResurrectionVFX.ImpactCrater, 0.25f * intensity, 14);

            // Through-hit sound — higher pitch with more penetrations
            SoundEngine.PlaySound(SoundID.Item10 with
            {
                Volume = 0.4f + hitCount * 0.05f,
                Pitch = 0.1f + hitCount * 0.12f
            }, target.Center);

            Lighting.AddLight(target.Center, ResurrectionVFX.CometCore.ToVector3() * intensity);
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            // Final detonation on wall hit
            ResurrectionVFX.DeathVFX(Projectile.Center, hitCount + 3);
            SoundEngine.PlaySound(SoundID.Item14 with { Volume = 0.7f, Pitch = 0.2f }, Projectile.Center);
            return true;
        }

        public override void OnKill(int timeLeft)
        {
            // Death detonation
            ResurrectionVFX.DeathVFX(Projectile.Center, hitCount);

            for (int i = 0; i < 5; i++)
            {
                float angle = MathHelper.TwoPi * i / 5f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 5f);
                Color emberColor = ResurrectionVFX.GetCometColor(i / 5f, hitCount);
                Dust ember = Dust.NewDustPerfect(Projectile.Center,
                    ModContent.DustType<CometEmberDust>(),
                    vel, 0, emberColor, 0.25f);
                ember.customData = new CometEmberBehavior(0.25f, 22, true);
            }

            Dust deathPulse = Dust.NewDustPerfect(Projectile.Center,
                ModContent.DustType<ResonantPulseDust>(),
                Vector2.Zero, 0, ResurrectionVFX.CometTrail, 0.2f);
            deathPulse.customData = new ResonantPulseBehavior(0.035f, 16);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            float heatPhase = HeatPhase;

            // === BUILD TRAIL ARRAYS ===
            Vector2[] trailPositions = null;
            float[] trailRotations = null;
            int validCount = 0;

            if (Projectile.oldPos.Length > 1)
            {
                trailPositions = new Vector2[Projectile.oldPos.Length];
                trailRotations = new float[Projectile.oldPos.Length];

                for (int i = 0; i < Projectile.oldPos.Length; i++)
                {
                    if (Projectile.oldPos[i] == Vector2.Zero) break;
                    trailPositions[i] = Projectile.oldPos[i] + Projectile.Size / 2f;
                    trailRotations[i] = Projectile.oldRot[i];
                    validCount++;
                }

                if (validCount > 1 && validCount < trailPositions.Length)
                {
                    Array.Resize(ref trailPositions, validCount);
                    Array.Resize(ref trailRotations, validCount);
                }
            }

            // === LAYER 1: CALAMITY-STYLE BASE TRAIL — thick penetrating comet trail ===
            if (validCount > 1)
            {
                Color primaryColor = Color.Lerp(ResurrectionVFX.CometTrail,
                    ResurrectionVFX.CometCore, hitCount * 0.15f);
                Color secondaryColor = Color.Lerp(ResurrectionVFX.LunarShine,
                    Color.White, hitCount * 0.1f);

                CalamityStyleTrailRenderer.DrawTrailWithBloom(
                    trailPositions, trailRotations,
                    CalamityStyleTrailRenderer.TrailStyle.Flame,
                    baseWidth: 14f + hitCount * 2f,
                    primaryColor: primaryColor,
                    secondaryColor: secondaryColor,
                    intensity: 1.0f + hitCount * 0.15f,
                    bloomMultiplier: 2.8f + hitCount * 0.3f);
            }

            // === LAYER 2: COMET TRAIL SHADER OVERLAY ===
            if (MoonlightSonataShaderManager.HasCometTrail)
            {
                DrawCometShaderOverlay(sb, heatPhase);
            }

            // === LAYER 3: BLOOM STACK ===
            ResurrectionVFX.DrawProjectileBloom(sb, Projectile.Center,
                Projectile.velocity.Length(), hitCount + 2);

            // === LAYER 4: MOTION BLUR BLOOM ===
            if (Projectile.velocity.LengthSquared() > 4f)
            {
                Texture2D texture = TextureAssets.Projectile[Type].Value;
                Color motionPrimary = Color.Lerp(ResurrectionVFX.LunarShine,
                    ResurrectionVFX.CometCore, hitCount * 0.12f);
                MotionBlurBloomRenderer.DrawProjectile(sb, texture, Projectile,
                    motionPrimary, Color.White,
                    intensityMult: 0.7f + hitCount * 0.05f);
            }

            return false;
        }

        private void DrawCometShaderOverlay(SpriteBatch sb, float heatPhase)
        {
            var glowTex = MoonlightSonataTextures.BloomOrb?.Value
                       ?? MagnumTextureRegistry.GetSoftGlow();
            if (glowTex == null) return;

            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = glowTex.Size() * 0.5f;
            float baseScale = 0.18f + hitCount * 0.05f;
            float pulse = 1f + MathF.Sin(Main.GlobalTimeWrappedHourly * 7f + hitCount) * 0.08f;

            try
            {
                MoonlightSonataShaderManager.BeginShaderBatch(sb);

                MoonlightSonataShaderManager.ApplyResurrectionCometTrail(
                    Main.GlobalTimeWrappedHourly, heatPhase, glowPass: false);

                sb.Draw(glowTex, drawPos, null,
                    Color.White, Projectile.rotation, origin,
                    baseScale * pulse * 1.3f, SpriteEffects.None, 0f);

                MoonlightSonataShaderManager.ApplyResurrectionCometTrail(
                    Main.GlobalTimeWrappedHourly, heatPhase, glowPass: true);

                sb.Draw(glowTex, drawPos, null,
                    Color.White * 0.6f, Projectile.rotation, origin,
                    baseScale * pulse * 2.0f, SpriteEffects.None, 0f);

                MoonlightSonataShaderManager.RestoreDefaultBatch(sb);
            }
            catch
            {
                try { MoonlightSonataShaderManager.RestoreDefaultBatch(sb); } catch { }
                sb.Draw(glowTex, drawPos, null,
                    MoonlightSonataPalette.Additive(ResurrectionVFX.CometCore, 0.35f),
                    Projectile.rotation, origin, baseScale, SpriteEffects.None, 0f);
            }
        }
    }
}
