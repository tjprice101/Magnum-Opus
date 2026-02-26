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
using MagnumOpus.Common.Systems.VFX.Bloom;
using MagnumOpus.Common.Systems.VFX.Trails;
using MagnumOpus.Common.Systems.VFX.Optimization;
using MagnumOpus.Content.MoonlightSonata;
using MagnumOpus.Content.MoonlightSonata.Dusts;
using MagnumOpus.Content.MoonlightSonata.Weapons.ResurrectionOfTheMoon;
using MagnumOpus.Content.SandboxLastPrism.Systems;

namespace MagnumOpus.Content.MoonlightSonata.Projectiles
{
    /// <summary>
    /// SupernovaShell — Massive AoE explosion round for Resurrection of the Moon's Chamber mechanic.
    ///
    /// Visual: A slow, heavy-gravity artillery shell that detonates on first enemy contact
    /// or tile collision, creating a massive radial supernova explosion that damages all
    /// enemies in a large area. Uses SupernovaBlast.fx for GPU-rendered crater explosion.
    ///
    /// Chamber slot 3 ammo: Maximum area damage, high single-hit damage, no penetration.
    /// The shell has a visible arcing trajectory with gravity.
    /// </summary>
    public class SupernovaShell : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow3";

        /// <summary>Explosion radius in world units for damage area.</summary>
        private const float ExplosionRadius = 200f;

        /// <summary>Animation timer for the explosion shader overlay (0-1 over lifetime).</summary>
        private float _explosionAge;
        private bool _hasDetonated;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 16;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = -1; // Doesn't penetrate — detonates on contact
            Projectile.timeLeft = 300;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1; // Hits each NPC only once
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();

            // Gravity — arcing trajectory
            Projectile.velocity.Y += 0.15f;

            // Dense comet trail — heavier visual than other ammo types
            ResurrectionVFX.CometTrailFrame(Projectile.Center, Projectile.velocity, 5);

            // Smoldering CometEmberDust — dense wake
            if (Main.rand.NextBool(2))
            {
                Vector2 dustVel = -Projectile.velocity * 0.08f + Main.rand.NextVector2Circular(2f, 2f);
                Color emberColor = Color.Lerp(ResurrectionVFX.CometCore, ResurrectionVFX.SupernovaWhite,
                    Main.rand.NextFloat(0.5f));
                Dust ember = Dust.NewDustPerfect(
                    Projectile.Center + Main.rand.NextVector2Circular(8f, 8f),
                    ModContent.DustType<CometEmberDust>(),
                    dustVel, 0, emberColor, 0.3f);
                ember.customData = new CometEmberBehavior
                {
                    VelocityDecay = 0.92f,
                    RotationSpeed = 0.1f,
                    BaseScale = 0.3f,
                    Lifetime = 25,
                    HasGravity = true
                };
            }

            // StarPointDust sparks — warning sparkles
            if (Main.rand.NextBool(3))
            {
                Color sparkColor = Color.Lerp(ResurrectionVFX.SupernovaWhite,
                    ResurrectionVFX.LunarShine, Main.rand.NextFloat());
                Dust star = Dust.NewDustPerfect(
                    Projectile.Center + Main.rand.NextVector2Circular(5f, 5f),
                    ModContent.DustType<StarPointDust>(),
                    -Projectile.velocity * 0.04f, 0, sparkColor, 0.2f);
                star.customData = new StarPointBehavior
                {
                    RotationSpeed = 0.15f,
                    TwinkleFrequency = 0.5f,
                    Lifetime = 16,
                    FadeStartTime = 4
                };
            }

            // LunarMote orbit — 1 crescent circling the shell
            if (Main.rand.NextBool(6))
            {
                float orbitAngle = Main.GameUpdateCount * 0.15f;
                Color moteColor = Color.Lerp(ResurrectionVFX.CometTrail,
                    ResurrectionVFX.LunarShine, Main.rand.NextFloat());
                Dust mote = Dust.NewDustPerfect(Projectile.Center,
                    ModContent.DustType<LunarMote>(),
                    -Projectile.velocity * 0.02f, 0, moteColor, 0.28f);
                mote.customData = new LunarMoteBehavior(Projectile.Center, orbitAngle)
                {
                    OrbitRadius = 12f,
                    OrbitSpeed = 0.16f,
                    Lifetime = 18,
                    FadePower = 0.9f
                };
            }

            if ((int)Projectile.ai[0] % 6 == 0)
                MoonlightVFXLibrary.SpawnMusicNotes(Projectile.Center, 1, 5f, 0.8f, 0.9f, 25);

            Projectile.ai[0]++;

            // Heavy comet lighting
            ResurrectionVFX.AddCometLight(Projectile.Center, 1.0f, 6);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<Debuffs.MusicsDissonance>(), 360);

            // Detonate on first hit — kill the projectile
            if (!_hasDetonated)
            {
                _hasDetonated = true;
                Detonate(target.Center);
                Projectile.Kill();
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (!_hasDetonated)
            {
                _hasDetonated = true;
                Detonate(Projectile.Center);
            }
            return true;
        }

        /// <summary>
        /// Massive supernova detonation — damages all enemies in radius,
        /// creates shader-driven crater explosion, heavy screen effects.
        /// </summary>
        private void Detonate(Vector2 center)
        {
            // === AREA DAMAGE ===
            if (Main.myPlayer == Projectile.owner)
            {
                foreach (NPC npc in Main.ActiveNPCs)
                {
                    if (npc.CanBeChasedBy(Projectile) && Vector2.Distance(center, npc.Center) < ExplosionRadius)
                    {
                        npc.SimpleStrikeNPC(Projectile.damage, Projectile.direction, true, Projectile.knockBack,
                            Projectile.DamageType);
                        npc.AddBuff(ModContent.BuffType<Debuffs.MusicsDissonance>(), 360);
                    }
                }
            }

            // === GRAND SUPERNOVA VFX ===
            ResurrectionVFX.DeathVFX(center, 10); // Max-intensity finale

            // Extra supernova flare layers
            CustomParticles.GenericFlare(center, Color.White, 1.2f, 26);
            CustomParticles.GenericFlare(center, ResurrectionVFX.SupernovaWhite, 1.0f, 24);
            CustomParticles.GenericFlare(center, ResurrectionVFX.LunarShine, 0.8f, 22);
            CustomParticles.GenericFlare(center, ResurrectionVFX.CometTrail, 0.6f, 20);
            CustomParticles.GenericFlare(center, ResurrectionVFX.DeepSpaceViolet, 0.4f, 18);

            // Massive CometEmberDust starburst — 20 radial lances
            for (int i = 0; i < 20; i++)
            {
                float angle = MathHelper.TwoPi * i / 20f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(5f, 12f);
                Color emberColor = ResurrectionVFX.GetCometColor((float)i / 20f, 10);
                Dust ember = Dust.NewDustPerfect(center,
                    ModContent.DustType<CometEmberDust>(),
                    vel, 0, emberColor, 0.4f);
                ember.customData = new CometEmberBehavior
                {
                    VelocityDecay = 0.94f,
                    RotationSpeed = 0.12f,
                    BaseScale = 0.4f,
                    Lifetime = 35,
                    HasGravity = true
                };
            }

            // ResonantPulseDust massive shockwave cascade — 6 expanding rings
            for (int i = 0; i < 6; i++)
            {
                Color ringColor = Color.Lerp(ResurrectionVFX.ImpactCrater,
                    ResurrectionVFX.CometTrail, (float)i / 6f);
                Dust pulse = Dust.NewDustPerfect(center,
                    ModContent.DustType<ResonantPulseDust>(),
                    Vector2.Zero, 0, ringColor,
                    0.3f + i * 0.08f);
                pulse.customData = new ResonantPulseBehavior
                {
                    ExpansionRate = 0.05f + i * 0.015f,
                    ExpansionDecay = 0.93f,
                    Lifetime = 22 + i * 4,
                    PulseFrequency = 0.2f + i * 0.05f
                };
            }

            // LunarMote supernova orbit — 8 crescents spiraling outward
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Color moteColor = Color.Lerp(ResurrectionVFX.LunarShine,
                    MoonlightVFXLibrary.MoonWhite, (float)i / 8f);
                Dust mote = Dust.NewDustPerfect(center + angle.ToRotationVector2() * 15f,
                    ModContent.DustType<LunarMote>(),
                    angle.ToRotationVector2() * 3f,
                    0, moteColor, 0.5f);
                mote.customData = new LunarMoteBehavior(center, angle)
                {
                    OrbitRadius = 30f + i * 6f,
                    OrbitSpeed = 0.05f,
                    Lifetime = 40,
                    FadePower = 0.93f
                };
            }

            // StarPointDust massive burst — 12 sharp sparkles
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 7f);
                Color starColor = Color.Lerp(ResurrectionVFX.CometCore,
                    ResurrectionVFX.SupernovaWhite, Main.rand.NextFloat());
                Dust star = Dust.NewDustPerfect(center,
                    ModContent.DustType<StarPointDust>(),
                    vel, 0, starColor, 0.35f);
                star.customData = new StarPointBehavior
                {
                    RotationSpeed = 0.15f,
                    TwinkleFrequency = 0.5f,
                    Lifetime = 30,
                    FadeStartTime = 8
                };
            }

            // Moonlight lightning fractals — 6 directions
            for (int i = 0; i < 6; i++)
            {
                float lightningAngle = MathHelper.TwoPi * i / 6f + Main.rand.NextFloat(-0.3f, 0.3f);
                Vector2 lightningEnd = center + lightningAngle.ToRotationVector2() * 90f;
                MagnumVFX.DrawMoonlightLightning(center, lightningEnd, 6, 22f, 2, 0.4f);
            }

            // Halo ring cascade — 5 expanding spectral rings
            for (int i = 0; i < 5; i++)
            {
                Color haloColor = Color.Lerp(ResurrectionVFX.ImpactCrater,
                    ResurrectionVFX.LunarShine, (float)i / 5f);
                CustomParticles.HaloRing(center, haloColor, 0.4f + i * 0.12f, 18 + i * 5);
            }

            // Music note supernova cascade
            for (int i = 0; i < 10; i++)
            {
                float angle = MathHelper.TwoPi * i / 10f;
                Vector2 noteOffset = angle.ToRotationVector2() * 10f;
                MoonlightVFXLibrary.SpawnMusicNotes(center + noteOffset, 1, 10f, 0.9f, 1.1f, 40);
            }

            // GOD RAY BURST — maximum intensity
            GodRaySystem.CreateBurst(center, ResurrectionVFX.LunarShine, 10, 100f, 30,
                GodRaySystem.GodRayStyle.Explosion, ResurrectionVFX.CometTrail);

            // Supernova arc burst
            CustomParticles.SwordArcBurst(center, ResurrectionVFX.LunarShine, 10, 0.6f);

            // SCREEN EFFECTS — maximum drama
            if (AdaptiveQualityManager.Instance?.CurrentQuality >= AdaptiveQualityManager.QualityLevel.Medium)
            {
                ScreenDistortionManager.TriggerRipple(center, ResurrectionVFX.CometTrail, 0.7f, 25);
                MagnumScreenEffects.AddScreenShake(6f);
            }

            // CHROMATIC ABERRATION
            try
            {
                SLPFlashSystem.SetCAFlashEffect(
                    intensity: 0.25f,
                    lifetime: 18,
                    whiteIntensity: 0.6f,
                    distanceMult: 0.5f,
                    moveIn: true);
            }
            catch { }

            // Explosion sounds — layered for impact
            SoundEngine.PlaySound(SoundID.Item14 with { Volume = 1.2f, Pitch = -0.4f }, center);
            SoundEngine.PlaySound(SoundID.Item122 with { Volume = 0.8f, Pitch = -0.2f }, center);

            Lighting.AddLight(center, ResurrectionVFX.SupernovaWhite.ToVector3() * 2f);
        }

        public override void OnKill(int timeLeft)
        {
            // If somehow killed without detonating, detonate now
            if (!_hasDetonated)
            {
                _hasDetonated = true;
                Detonate(Projectile.Center);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;

            // === BUILD TRAIL ARRAYS ===
            if (Projectile.oldPos.Length > 1)
            {
                Vector2[] trailPositions = new Vector2[Projectile.oldPos.Length];
                float[] trailRotations = new float[Projectile.oldPos.Length];
                int validCount = 0;

                for (int i = 0; i < Projectile.oldPos.Length; i++)
                {
                    if (Projectile.oldPos[i] == Vector2.Zero) break;
                    trailPositions[i] = Projectile.oldPos[i] + Projectile.Size / 2f;
                    trailRotations[i] = Projectile.oldRot[i];
                    validCount++;
                }

                if (validCount > 1)
                {
                    if (validCount < trailPositions.Length)
                    {
                        Array.Resize(ref trailPositions, validCount);
                        Array.Resize(ref trailRotations, validCount);
                    }

                    // Heavy comet trail — Cosmic + Flame layered
                    CalamityStyleTrailRenderer.DrawTrailWithBloom(
                        trailPositions, trailRotations,
                        CalamityStyleTrailRenderer.TrailStyle.Cosmic,
                        baseWidth: 22f,
                        primaryColor: ResurrectionVFX.CometTrail,
                        secondaryColor: ResurrectionVFX.LunarShine,
                        intensity: 1.2f,
                        bloomMultiplier: 3.0f);

                    CalamityStyleTrailRenderer.DrawTrailWithBloom(
                        trailPositions, trailRotations,
                        CalamityStyleTrailRenderer.TrailStyle.Flame,
                        baseWidth: 12f,
                        primaryColor: ResurrectionVFX.LunarShine,
                        secondaryColor: ResurrectionVFX.CometCore,
                        intensity: 0.6f,
                        bloomMultiplier: 1.8f);
                }
            }

            // === SHADER GLOW OVERLAY ===
            if (MoonlightSonataShaderManager.HasCometTrail)
            {
                DrawSupernovaShaderOverlay(sb);
            }

            // === BLOOM STACK ===
            ResurrectionVFX.DrawProjectileBloom(sb, Projectile.Center,
                Projectile.velocity.Length(), 8);

            // === MOTION BLUR BLOOM ===
            if (Projectile.velocity.LengthSquared() > 4f)
            {
                Texture2D texture = TextureAssets.Projectile[Type].Value;
                MotionBlurBloomRenderer.DrawProjectile(sb, texture, Projectile,
                    ResurrectionVFX.CometTrail, ResurrectionVFX.SupernovaWhite,
                    intensityMult: 0.8f);
            }

            return false;
        }

        private void DrawSupernovaShaderOverlay(SpriteBatch sb)
        {
            var glowTex = MoonlightSonataTextures.BloomOrb?.Value
                       ?? MagnumTextureRegistry.GetSoftGlow();
            if (glowTex == null) return;

            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = glowTex.Size() * 0.5f;
            float pulse = 1f + MathF.Sin(Main.GlobalTimeWrappedHourly * 5f) * 0.12f;

            try
            {
                MoonlightSonataShaderManager.BeginShaderBatch(sb);

                // Comet trail shader at high intensity — the shell is always "hot"
                MoonlightSonataShaderManager.ApplyResurrectionCometTrail(
                    Main.GlobalTimeWrappedHourly, 0.8f, glowPass: false);

                sb.Draw(glowTex, drawPos, null,
                    Color.White, Projectile.rotation, origin,
                    0.3f * pulse, SpriteEffects.None, 0f);

                MoonlightSonataShaderManager.ApplyResurrectionCometTrail(
                    Main.GlobalTimeWrappedHourly, 0.8f, glowPass: true);

                sb.Draw(glowTex, drawPos, null,
                    Color.White * 0.5f, Projectile.rotation, origin,
                    0.5f * pulse, SpriteEffects.None, 0f);

                MoonlightSonataShaderManager.RestoreDefaultBatch(sb);
            }
            catch
            {
                try { MoonlightSonataShaderManager.RestoreDefaultBatch(sb); } catch { }
                sb.Draw(glowTex, drawPos, null,
                    MoonlightSonataPalette.Additive(ResurrectionVFX.CometCore, 0.4f),
                    Projectile.rotation, origin, 0.25f, SpriteEffects.None, 0f);
            }
        }
    }
}
