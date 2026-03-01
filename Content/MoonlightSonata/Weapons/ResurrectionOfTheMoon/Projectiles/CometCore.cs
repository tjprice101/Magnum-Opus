using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.MoonlightSonata.Weapons.ResurrectionOfTheMoon.Utilities;
using MagnumOpus.Content.MoonlightSonata.Weapons.ResurrectionOfTheMoon.Particles;
using MagnumOpus.Content.MoonlightSonata.Weapons.ResurrectionOfTheMoon.Primitives;
using MagnumOpus.Content.MoonlightSonata.Weapons.ResurrectionOfTheMoon.Buffs;
using MagnumOpus.Content.MoonlightSonata.Weapons.ResurrectionOfTheMoon.Dusts;

namespace MagnumOpus.Content.MoonlightSonata.Weapons.ResurrectionOfTheMoon.Projectiles
{
    /// <summary>
    /// Comet Core Chamber — Piercing comet with burning ember wake.
    /// Passes through 5 enemies, leaving a searing ember trail.
    /// Each pierce applies stacking Lunar Impact and spawns fire particles.
    /// Trail at maximum CometTrail intensity (white-hot).
    /// Destroys on tile contact.
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
            }

            SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode with { Volume = 0.5f, Pitch = 0.2f },
                target.Center);
        }

        // =================================================================
        // RENDERING
        // =================================================================

        public override bool PreDraw(ref Color lightColor)
        {
            if (Main.dedServ) return false;

            // Pass 1: Wide burning glow trail
            DrawGlowTrail();

            // Pass 2: Main comet body trail
            DrawMainTrail();

            // Pass 3: Burning head orb
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

            // Outer burning glow
            sb.Draw(bloom, drawPos, null, CometUtils.CometCoreColor * 0.5f, 0f, origin, 0.8f, SpriteEffects.None, 0f);

            // White-hot core
            sb.Draw(bloom, drawPos, null, CometUtils.FrigidImpact * 0.7f, 0f, origin, 0.4f, SpriteEffects.None, 0f);
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
