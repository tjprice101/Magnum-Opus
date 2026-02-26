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
    /// Resurrection Projectile — "The Comet Round".
    /// Devastating bullet that ricochets 10 times to nearby enemies.
    /// Each hit creates crater detonations with escalating god ray bursts.
    ///
    /// Overhaul: CometTrail.fx shader-driven burning ember trail rendering,
    /// SupernovaBlast.fx shader overlay at death, custom dust types
    /// (CometEmberDust, StarPointDust, LunarMote, ResonantPulseDust),
    /// enhanced ricochet chain VFX, grand finale supernova.
    ///
    /// Rendering pipeline (PreDraw):
    ///   1. CalamityStyleTrailRenderer.Cosmic — base trail geometry
    ///   2. CometTrail.fx CometTrailMain — shader burning ember overlay
    ///   3. CometTrail.fx CometTrailGlow — shader bloom pass
    ///   4. DrawProjectileBloom — 5-layer additive bloom stack
    ///   5. MotionBlurBloomRenderer — velocity stretch
    ///   6. Flame inner trail (ricochets 5+) — heat shimmer layer
    ///
    /// Ricochet behavior:
    ///   Ricochet 1-4:  Normal chain + crater VFX + shader trail intensifies
    ///   Ricochet 5-7:  Chain + GodRaySystem bursts + Flame inner trail
    ///   Ricochet 8-9:  Chain + god rays + screen shake
    ///   Ricochet 10:   Final hit — supernova detonation + SupernovaBlast shader burst
    /// </summary>
    public class ResurrectionProjectile : ModProjectile
    {
        private int ricochetCount = 0;
        private const int MaxRicochets = 10;

        /// <summary>Ricochet phase for shader — ricochetCount / MaxRicochets (0 = first shot, 1 = max ricochets).</summary>
        private float RicochetPhase => (float)ricochetCount / MaxRicochets;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 24;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = MaxRicochets + 1;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 2;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 8;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();

            // Comet trail VFX — dense, heavy trail using custom dusts
            ResurrectionVFX.CometTrailFrame(Projectile.Center, Projectile.velocity, ricochetCount);

            // Orbiting CometEmberDust ring (every 6 frames) — comet tail effect
            if ((int)Projectile.ai[0] % 6 == 0)
            {
                float orbitPhase = Projectile.ai[0] * 0.25f;
                for (int i = 0; i < 2; i++)
                {
                    float angle = orbitPhase + MathHelper.Pi * i;
                    float radius = 8f + MathF.Sin(orbitPhase + i) * 3f;
                    Vector2 orbitPos = Projectile.Center + angle.ToRotationVector2() * radius;
                    Color orbitColor = ResurrectionVFX.GetCometColor((float)i / 2f, ricochetCount);
                    Dust ember = Dust.NewDustPerfect(orbitPos,
                        ModContent.DustType<CometEmberDust>(),
                        -Projectile.velocity * 0.04f, 0, orbitColor,
                        0.18f + ricochetCount * 0.02f);
                    ember.customData = new CometEmberBehavior
                    {
                        VelocityDecay = 0.95f,
                        RotationSpeed = 0.06f,
                        BaseScale = 0.18f + ricochetCount * 0.02f,
                        Lifetime = 16,
                        HasGravity = false
                    };
                }
            }

            // StarPointDust sharp sparks along path (every 8 frames)
            if ((int)Projectile.ai[0] % 8 == 0)
            {
                float orbitPhase = Projectile.ai[0] * 0.2f;
                for (int i = 0; i < 2; i++)
                {
                    float angle = orbitPhase + MathHelper.Pi * i;
                    float radius = 6f + MathF.Sin(orbitPhase + i) * 3f;
                    Vector2 starPos = Projectile.Center + angle.ToRotationVector2() * radius;
                    Color starColor = Color.Lerp(ResurrectionVFX.LunarShine,
                        ResurrectionVFX.CometCore, (float)i / 2f);
                    Dust star = Dust.NewDustPerfect(starPos,
                        ModContent.DustType<StarPointDust>(),
                        -Projectile.velocity * 0.03f, 0, starColor, 0.14f);
                    star.customData = new StarPointBehavior
                    {
                        RotationSpeed = 0.15f,
                        TwinkleFrequency = 0.5f,
                        Lifetime = 14,
                        FadeStartTime = 4
                    };
                }
            }

            // Music notes dense in trail (every 5 frames)
            if ((int)Projectile.ai[0] % 5 == 0)
            {
                MoonlightVFXLibrary.SpawnMusicNotes(Projectile.Center, 1, 5f, 0.7f, 0.85f, 25);
            }
            Projectile.ai[0]++;

            // Pulsing comet lighting
            ResurrectionVFX.AddCometLight(Projectile.Center, 0.7f + ricochetCount * 0.1f, ricochetCount);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Apply Musical Dissonance debuff — longer with more ricochets
            target.AddBuff(ModContent.BuffType<Debuffs.MusicsDissonance>(), 300 + ricochetCount * 30);

            // Seeking crystals — 33% chance on hit
            if (Main.rand.NextBool(3))
            {
                SeekingCrystalHelper.SpawnMoonlightCrystals(
                    Projectile.GetSource_FromThis(),
                    target.Center,
                    Projectile.velocity,
                    (int)(damageDone * 0.18f),
                    Projectile.knockBack,
                    Projectile.owner,
                    4
                );
            }

            // Crater detonation with moonbeam lances
            ResurrectionVFX.OnHitExplosion(target.Center, ricochetCount, hit.Crit);

            // Explosion sound — pitch escalates with ricochets
            SoundEngine.PlaySound(SoundID.Item14 with
            {
                Volume = 0.5f + ricochetCount * 0.05f,
                Pitch = 0.3f + ricochetCount * 0.06f
            }, target.Center);

            // Attempt to ricochet to another enemy
            if (ricochetCount < MaxRicochets)
            {
                NPC newTarget = FindNearestEnemy(target.Center, 800f, target.whoAmI);
                if (newTarget != null)
                {
                    ricochetCount++;

                    // Redirect toward new target
                    Vector2 direction = (newTarget.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                    float speed = Projectile.velocity.Length();
                    if (speed < 20f) speed = 20f;
                    Projectile.velocity = direction * speed;

                    // Reset time left for continued flight
                    Projectile.timeLeft = Math.Max(Projectile.timeLeft, 60);

                    // Ricochet sound — escalating pitch
                    float pitch = -0.3f + (ricochetCount * 0.1f);
                    SoundEngine.PlaySound(SoundID.Item10 with { Volume = 0.5f, Pitch = pitch }, Projectile.Center);

                    // Lightning chain between ricochet points
                    MagnumVFX.DrawMoonlightLightning(Projectile.Center, newTarget.Center, 10, 30f, 3, 0.4f);

                    // Ricochet VFX — escalating crater impact
                    ResurrectionVFX.OnRicochetVFX(Projectile.Center, Projectile.velocity, ricochetCount);

                    // ResonantPulseDust chain link ring at ricochet origin
                    Dust chainPulse = Dust.NewDustPerfect(Projectile.Center,
                        ModContent.DustType<ResonantPulseDust>(),
                        Vector2.Zero, 0, ResurrectionVFX.ImpactCrater, 0.2f);
                    chainPulse.customData = new ResonantPulseBehavior(0.04f, 14);
                }
            }
        }

        private NPC FindNearestEnemy(Vector2 position, float range, int excludeWhoAmI)
        {
            NPC closest = null;
            float closestDist = range;

            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (npc.whoAmI != excludeWhoAmI && npc.CanBeChasedBy(Projectile) &&
                    Collision.CanHitLine(position, 1, 1, npc.position, npc.width, npc.height))
                {
                    float dist = Vector2.Distance(position, npc.Center);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closest = npc;
                    }
                }
            }

            return closest;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            ResurrectionVFX.WallHitVFX(Projectile.Center);
            SoundEngine.PlaySound(SoundID.Item10 with { Volume = 0.7f, Pitch = 0.2f }, Projectile.Center);
            return true;
        }

        public override void OnKill(int timeLeft)
        {
            // Final comet detonation — grand finale scales with ricochets achieved
            ResurrectionVFX.DeathVFX(Projectile.Center, ricochetCount);

            // CometEmberDust death scatter
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 5f);
                Color emberColor = ResurrectionVFX.GetCometColor(i / 6f, ricochetCount);
                Dust ember = Dust.NewDustPerfect(Projectile.Center,
                    ModContent.DustType<CometEmberDust>(),
                    vel, 0, emberColor, 0.25f);
                ember.customData = new CometEmberBehavior(0.25f, 22, true);
            }

            // ResonantPulseDust death ring
            Dust deathPulse = Dust.NewDustPerfect(Projectile.Center,
                ModContent.DustType<ResonantPulseDust>(),
                Vector2.Zero, 0, ResurrectionVFX.CometTrail, 0.2f);
            deathPulse.customData = new ResonantPulseBehavior(0.035f, 16);

            // Small halo ring cascade
            for (int i = 0; i < 3; i++)
            {
                Color ringColor = Color.Lerp(ResurrectionVFX.CometTrail,
                    ResurrectionVFX.LunarShine, i / 3f);
                CustomParticles.HaloRing(Projectile.Center, ringColor, 0.2f + i * 0.08f, 12 + i * 3);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            float ricochetPhase = RicochetPhase;

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

            // === LAYER 1: CALAMITY-STYLE BASE TRAIL — thick comet trail ===
            if (validCount > 1)
            {
                float bounceIntensity = 1f + ricochetCount * 0.1f;
                Color primaryColor = Color.Lerp(ResurrectionVFX.CometTrail,
                    ResurrectionVFX.LunarShine, ricochetCount * 0.08f);
                Color secondaryColor = Color.Lerp(ResurrectionVFX.LunarShine,
                    ResurrectionVFX.CometCore, ricochetCount * 0.08f);

                CalamityStyleTrailRenderer.DrawTrailWithBloom(
                    trailPositions, trailRotations,
                    CalamityStyleTrailRenderer.TrailStyle.Cosmic,
                    baseWidth: 16f + ricochetCount * 1.5f,
                    primaryColor: primaryColor,
                    secondaryColor: secondaryColor,
                    intensity: 0.9f * bounceIntensity,
                    bloomMultiplier: 2.5f + ricochetCount * 0.25f);

                // Secondary inner Flame trail for heat shimmer (ricochets 5+)
                if (ricochetCount >= 5)
                {
                    CalamityStyleTrailRenderer.DrawTrailWithBloom(
                        trailPositions, trailRotations,
                        CalamityStyleTrailRenderer.TrailStyle.Flame,
                        baseWidth: 8f + (ricochetCount - 5) * 1f,
                        primaryColor: ResurrectionVFX.LunarShine,
                        secondaryColor: ResurrectionVFX.CometCore,
                        intensity: 0.4f + (ricochetCount - 5) * 0.08f,
                        bloomMultiplier: 1.5f);
                }
            }

            // === LAYER 2: COMET TRAIL SHADER OVERLAY (GPU-rendered burning ember effect) ===
            if (MoonlightSonataShaderManager.HasCometTrail)
            {
                DrawCometShaderOverlay(sb, ricochetPhase);
            }

            // === LAYER 3: 5-LAYER {A=0} BLOOM STACK BODY ===
            ResurrectionVFX.DrawProjectileBloom(sb, Projectile.Center, Projectile.velocity.Length(), ricochetCount);

            // === LAYER 4: MOTION BLUR BLOOM (velocity-based stretch) ===
            if (Projectile.velocity.LengthSquared() > 4f)
            {
                Texture2D texture = TextureAssets.Projectile[Type].Value;
                Color motionPrimary = Color.Lerp(ResurrectionVFX.CometTrail,
                    ResurrectionVFX.LunarShine, ricochetCount * 0.08f);
                MotionBlurBloomRenderer.DrawProjectile(sb, texture, Projectile,
                    motionPrimary, ResurrectionVFX.CometCore,
                    intensityMult: 0.6f + ricochetCount * 0.04f);
            }

            return false;
        }

        /// <summary>
        /// Draws the CometTrail.fx shader glow overlay at the projectile position.
        /// Creates a burning ember effect that intensifies with each ricochet.
        /// Two passes: main burning ember body + soft glow bloom.
        /// </summary>
        private void DrawCometShaderOverlay(SpriteBatch sb, float ricochetPhase)
        {
            var glowTex = MoonlightSonataTextures.BloomOrb?.Value
                       ?? MagnumTextureRegistry.GetSoftGlow();
            if (glowTex == null) return;

            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = glowTex.Size() * 0.5f;

            float baseScale = 0.15f + ricochetCount * 0.05f;
            float pulse = 1f + MathF.Sin(Main.GlobalTimeWrappedHourly * 6f + ricochetCount) * 0.08f;
            float glowScale = baseScale * pulse;

            try
            {
                // Pass 1: Main comet ember overlay
                MoonlightSonataShaderManager.BeginShaderBatch(sb);

                MoonlightSonataShaderManager.ApplyResurrectionCometTrail(
                    Main.GlobalTimeWrappedHourly, ricochetPhase, glowPass: false);

                sb.Draw(glowTex, drawPos, null,
                    Color.White, Projectile.rotation, origin,
                    glowScale * 1.2f, SpriteEffects.None, 0f);

                // Pass 2: Soft glow bloom
                MoonlightSonataShaderManager.ApplyResurrectionCometTrail(
                    Main.GlobalTimeWrappedHourly, ricochetPhase, glowPass: true);

                sb.Draw(glowTex, drawPos, null,
                    Color.White * 0.7f, Projectile.rotation, origin,
                    glowScale * 1.8f, SpriteEffects.None, 0f);

                MoonlightSonataShaderManager.RestoreDefaultBatch(sb);
            }
            catch
            {
                // Fallback: plain additive bloom if shader fails
                try { MoonlightSonataShaderManager.RestoreDefaultBatch(sb); } catch { }
                sb.Draw(glowTex, drawPos, null,
                    MoonlightSonataPalette.Additive(ResurrectionVFX.CometTrail, 0.3f),
                    Projectile.rotation, origin, glowScale, SpriteEffects.None, 0f);
            }
        }
    }
}
