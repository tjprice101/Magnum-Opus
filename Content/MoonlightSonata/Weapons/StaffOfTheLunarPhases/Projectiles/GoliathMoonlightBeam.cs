using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.MoonlightSonata.Weapons.StaffOfTheLunarPhases.Utilities;
using MagnumOpus.Content.MoonlightSonata.Weapons.StaffOfTheLunarPhases.Particles;
using MagnumOpus.Content.MoonlightSonata.Weapons.StaffOfTheLunarPhases.Primitives;
using MagnumOpus.Content.MoonlightSonata.Weapons.StaffOfTheLunarPhases.Dusts;
using MagnumOpus.Content.MoonlightSonata.Weapons.MoonlightsCalling.Buffs;
using MagnumOpus.Content.MoonlightSonata;
using ReLogic.Content;

namespace MagnumOpus.Content.MoonlightSonata.Weapons.StaffOfTheLunarPhases.Projectiles
{
    /// <summary>
    /// Goliath Moonlight Beam — ricocheting beam fired by the Goliath of Moonlight minion.
    /// Bounces between enemies up to 5 times, healing the owner 10 HP per hit
    /// and inflicting Musical Dissonance. Primitive trail with cosmic gradient.
    /// </summary>
    public class GoliathMoonlightBeam : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_1";

        // =================================================================
        // CONSTANTS
        // =================================================================

        public const int MaxBounces = 5;
        public const float BounceRange = 800f;
        public const float BeamSpeed = 22f;
        public const float BeamWidth = 16f;
        public const int TrailLength = 20;
        public const int HealAmount = 10;

        // =================================================================
        // AI FIELDS
        // =================================================================

        /// <summary>ai[0] = remaining bounces.</summary>
        public ref float BouncesRemaining => ref Projectile.ai[0];

        /// <summary>ai[1] = last hit NPC whoAmI (to avoid re-hitting same target immediately).</summary>
        public ref float LastHitNPC => ref Projectile.ai[1];

        /// <summary>localAI[0] = alive time counter.</summary>
        public ref float AliveTime => ref Projectile.localAI[0];

        /// <summary>localAI[1] = beam intensity (0..1).</summary>
        public ref float BeamIntensity => ref Projectile.localAI[1];

        // =================================================================
        // SETUP
        // =================================================================

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailingMode[Type] = 2;
            ProjectileID.Sets.TrailCacheLength[Type] = TrailLength;
            ProjectileID.Sets.MinionShot[Type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = MaxBounces + 2;
            Projectile.timeLeft = 300;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 0;
            Projectile.light = 0.5f;
        }

        public override bool? CanCutTiles() => false;

        // =================================================================
        // AI
        // =================================================================

        public override void AI()
        {
            AliveTime++;
            BeamIntensity = MathHelper.Clamp(1f - (BouncesRemaining / MaxBounces) * 0.3f, 0.5f, 1f);

            Projectile.rotation = Projectile.velocity.ToRotation();

            // Soft homing toward nearest enemy
            int target = GoliathUtils.ClosestNPCAt(Projectile.Center, BounceRange, (int)LastHitNPC);
            if (target != -1)
            {
                Vector2 toTarget = Main.npc[target].Center - Projectile.Center;
                toTarget.Normalize();
                float homingStr = 0.06f;
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * Projectile.velocity.Length(), homingStr);
            }

            // Lighting
            Color lightCol = GoliathUtils.GetCosmicGradient(0.6f + BeamIntensity * 0.3f);
            Lighting.AddLight(Projectile.Center, lightCol.ToVector3() * 0.4f);

            // Flight particles
            SpawnFlightParticles();
        }

        private void SpawnFlightParticles()
        {
            if (Main.dedServ) return;

            // Beam sparks — every 2 ticks
            if (AliveTime % 2 == 0)
            {
                Vector2 offset = Main.rand.NextVector2Circular(6f, 6f);
                Vector2 sparkVel = -Projectile.velocity.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(1f, 2f)
                    + Main.rand.NextVector2Circular(0.8f, 0.8f);
                GoliathParticleHandler.Spawn(new BeamSparkParticle(
                    Projectile.Center + offset, sparkVel,
                    0.3f + BeamIntensity * 0.2f, 12 + Main.rand.Next(8)));
            }

            // Cosmic dust — every 3 ticks
            if (AliveTime % 3 == 0)
            {
                int d = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height,
                    ModContent.DustType<GoliathDust>(), -Projectile.velocity.X * 0.08f, -Projectile.velocity.Y * 0.08f);
                Main.dust[d].noGravity = true;
                Main.dust[d].scale = 0.7f + BeamIntensity * 0.4f;
            }
        }

        // =================================================================
        // ON HIT — RICOCHET + HEAL + DEBUFF
        // =================================================================

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Heal owner 10 HP
            if (Projectile.owner == Main.myPlayer)
            {
                Player owner = Main.player[Projectile.owner];
                owner.Heal(HealAmount);
            }

            // Inflict Musical Dissonance
            target.AddBuff(ModContent.BuffType<MusicalDissonance>(), 300);

            // Impact VFX
            SpawnImpactVFX(target.Center);

            // Ricochet to next target
            if (BouncesRemaining > 0)
            {
                BouncesRemaining--;
                int nextTarget = GoliathUtils.ClosestNPCAt(target.Center, BounceRange, target.whoAmI);
                if (nextTarget != -1)
                {
                    Vector2 toNext = Main.npc[nextTarget].Center - Projectile.Center;
                    toNext.Normalize();
                    Projectile.velocity = toNext * BeamSpeed;
                    LastHitNPC = target.whoAmI;

                    // Ricochet sound
                    SoundEngine.PlaySound(SoundID.Item12 with { Volume = 0.4f, Pitch = 0.3f + (MaxBounces - BouncesRemaining) * 0.1f },
                        Projectile.Center);
                }
            }
        }

        private void SpawnImpactVFX(Vector2 impactPos)
        {
            if (Main.dedServ) return;

            // Impact bloom
            GoliathParticleHandler.Spawn(new ImpactBloomParticle(
                impactPos, GoliathUtils.ImpactFlash, 0.8f + BeamIntensity * 0.4f, 15));

            // Radial spark burst
            for (int i = 0; i < 8; i++)
            {
                Vector2 vel = Main.rand.NextVector2CircularEdge(4f, 4f);
                GoliathParticleHandler.Spawn(new BeamSparkParticle(
                    impactPos, vel, 0.3f + BeamIntensity * 0.2f, 15 + Main.rand.Next(8)));
            }

            // Music notes on impact
            for (int i = 0; i < 3; i++)
            {
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-3f, -1f));
                Color noteColor = GoliathUtils.GetCosmicGradient(Main.rand.NextFloat(0.4f, 0.9f));
                GoliathParticleHandler.Spawn(new MusicNoteParticle(
                    impactPos + Main.rand.NextVector2Circular(10f, 10f), noteVel,
                    noteColor, 0.4f + Main.rand.NextFloat(0.3f), 40 + Main.rand.Next(20)));
            }

            // Hue-shifting music notes — the Conductor's gravitational command echoes
            MoonlightVFXLibrary.SpawnMusicNotes(impactPos, count: 3 + (int)(BeamIntensity * 3),
                spread: 20f, minScale: 0.5f, maxScale: 0.9f, lifetime: 40);

            // Dust burst
            for (int i = 0; i < 6; i++)
            {
                Vector2 dustVel = Main.rand.NextVector2CircularEdge(4f, 4f);
                int d = Dust.NewDust(impactPos - new Vector2(4), 8, 8,
                    ModContent.DustType<GoliathDust>(), dustVel.X, dustVel.Y);
                Main.dust[d].noGravity = true;
                Main.dust[d].scale = 1.2f;
            }
        }

        // =================================================================
        // RENDERING
        // =================================================================

        public override bool PreDraw(ref Color lightColor)
        {
            if (Main.dedServ) return false;

            DrawGlowTrail();
            DrawMainTrail();
            DrawHeadGlow();

            return false;
        }

        private void DrawGlowTrail()
        {
            MiscShaderData glowShader = null;
            if (GameShaders.Misc.TryGetValue("MagnumOpus:GoliathBeamGlow", out var shader))
            {
                glowShader = shader;
                glowShader.UseColor(GoliathUtils.GetCosmicGradient(0.5f));
                glowShader.UseSecondaryColor(GoliathUtils.CosmicVoid);
                glowShader.UseOpacity(0.35f + BeamIntensity * 0.25f);
                glowShader.UseSaturation(BeamIntensity);

                // Cosmic nebula clouds — gravitational field distortion texture
                glowShader.UseImage1(ModContent.Request<Texture2D>(
                    "MagnumOpus/Assets/VFX Asset Library/NoiseTextures/CosmicNebulaClouds"));
            }

            GoliathTrailRenderer.RenderTrail(Projectile.oldPos, new GoliathTrailSettings(
                widthFunction: (completion, _) =>
                {
                    float taper = 1f - completion;
                    return (BeamWidth * 2.5f + BeamIntensity * 15f) * taper * GoliathUtils.SineOut(1f - completion);
                },
                colorFunction: (completion, _) =>
                {
                    Color col = GoliathUtils.GetCosmicGradient(completion * 0.5f + 0.3f);
                    return col * (0.3f + BeamIntensity * 0.2f) * (1f - completion * 0.5f);
                },
                offsetFunction: (_, _) => Projectile.Size * 0.5f,
                smoothen: true,
                shader: glowShader
            ), TrailLength);
        }

        private void DrawMainTrail()
        {
            MiscShaderData mainShader = null;
            if (GameShaders.Misc.TryGetValue("MagnumOpus:GoliathBeamMain", out var shader))
            {
                mainShader = shader;
                mainShader.UseColor(GoliathUtils.StarCore);
                mainShader.UseSecondaryColor(GoliathUtils.NebulaPurple);
                mainShader.UseOpacity(0.7f + BeamIntensity * 0.3f);
                mainShader.UseSaturation(BeamIntensity);

                // Cosmic nebula clouds — inner cosmic vein structure
                mainShader.UseImage1(ModContent.Request<Texture2D>(
                    "MagnumOpus/Assets/VFX Asset Library/NoiseTextures/CosmicNebulaClouds"));
            }

            GoliathTrailRenderer.RenderTrail(Projectile.oldPos, new GoliathTrailSettings(
                widthFunction: (completion, _) =>
                {
                    float taper = 1f - completion;
                    return (BeamWidth + BeamIntensity * 6f) * taper * GoliathUtils.SineOut(1f - completion);
                },
                colorFunction: (completion, _) =>
                {
                    Color col = GoliathUtils.GetCosmicGradient(0.5f + completion * 0.4f);
                    return col * (0.8f + BeamIntensity * 0.2f) * (1f - completion * 0.3f);
                },
                offsetFunction: (_, _) => Projectile.Size * 0.5f,
                smoothen: true,
                shader: mainShader
            ), TrailLength);
        }

        /// <summary>Gravitational beam head glow with cross-star flares —
        /// the Conductor's cosmic authority radiates through intersecting star points.</summary>
        private void DrawHeadGlow()
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D bloom = GoliathTextures.SoftRadialBloom;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            // Layer 1: Wide atmospheric halo — cosmic nebula presence
            Color outerColor = GoliathUtils.NebulaPurple with { A = 0 };
            float outerScale = 0.5f + BeamIntensity * 0.3f;
            sb.Draw(bloom, drawPos, null, outerColor * (0.4f + BeamIntensity * 0.3f),
                0f, bloom.Size() * 0.5f, outerScale * 1.5f, SpriteEffects.None, 0f);

            // Layer 2: Mid glow — ice blue cosmic ring
            Color midColor = GoliathUtils.IceBlueBrilliance with { A = 0 };
            sb.Draw(bloom, drawPos, null, midColor * 0.3f,
                0f, bloom.Size() * 0.5f, outerScale * 0.8f, SpriteEffects.None, 0f);

            // Layer 3: Cross-star flares — counter-rotating 4-pointed star pair
            var starTex = ModContent.Request<Texture2D>(
                "MagnumOpus/Assets/Particles Asset Library/Stars/4PointedStarHard").Value;
            float baseRot = AliveTime * 0.04f;
            Color starColor = GoliathUtils.GetCosmicGradient(
                0.6f + BeamIntensity * 0.3f) with { A = 0 };

            // Primary star rotation
            sb.Draw(starTex, drawPos, null, starColor * (0.5f + BeamIntensity * 0.3f),
                baseRot, starTex.Size() * 0.5f,
                0.25f + BeamIntensity * 0.15f, SpriteEffects.None, 0f);

            // Counter-rotating secondary star
            sb.Draw(starTex, drawPos, null, starColor * (0.3f + BeamIntensity * 0.2f),
                -baseRot + MathHelper.PiOver4, starTex.Size() * 0.5f,
                0.18f + BeamIntensity * 0.12f, SpriteEffects.None, 0f);

            // Layer 4: Bright inner core — star core white
            Color coreColor = GoliathUtils.StarCore with { A = 0 };
            float coreScale = 0.25f + BeamIntensity * 0.15f;
            sb.Draw(bloom, drawPos, null, coreColor * (0.6f + BeamIntensity * 0.4f),
                0f, bloom.Size() * 0.5f, coreScale, SpriteEffects.None, 0f);
        }

        // =================================================================
        // DEATH VFX
        // =================================================================

        public override void OnKill(int timeLeft)
        {
            if (Main.dedServ) return;

            // Final dissipation bloom
            GoliathParticleHandler.Spawn(new ImpactBloomParticle(
                Projectile.Center, GoliathUtils.IceBlueBrilliance, 0.6f, 20));

            for (int i = 0; i < 8; i++)
            {
                Vector2 vel = Main.rand.NextVector2CircularEdge(3f, 3f);
                GoliathParticleHandler.Spawn(new BeamSparkParticle(
                    Projectile.Center, vel, 0.3f, 15 + Main.rand.Next(8)));
            }
        }
    }
}
