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
using MagnumOpus.Content.LaCampanella.Debuffs;

namespace MagnumOpus.Content.MoonlightSonata.Weapons.ResurrectionOfTheMoon.Projectiles
{
    /// <summary>
    /// Supernova Chamber — Arcing artillery shell that detonates in massive AoE.
    /// Gravity-affected arc trajectory.
    /// On first tile or enemy contact: massive radial explosion using SupernovaBlast shader.
    /// Spawns expanding shockwave ring + crater ring particles + screen shake.
    /// Spawns 6 secondary lunar fragment projectiles on explosion.
    /// </summary>
    public class SupernovaShell : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_1";

        // =================================================================
        // CONSTANTS
        // =================================================================

        public const float Gravity = 0.12f;
        public const float MaxFallSpeed = 14f;
        public const float ShellWidth = 22f;
        public const int TrailLength = 20;
        public const float ExplosionRadius = 280f;
        public const float ExplosionDamageMultiplier = 1.5f;
        public const int FragmentCount = 6;
        public const float FragmentDamageMultiplier = 0.3f;

        // =================================================================
        // AI FIELDS
        // =================================================================

        /// <summary>localAI[0] = alive time.</summary>
        public ref float AliveTime => ref Projectile.localAI[0];

        /// <summary>localAI[1] = has exploded flag.</summary>
        public ref float HasExploded => ref Projectile.localAI[1];

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
            Projectile.width = 18;
            Projectile.height = 18;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = -1; // explodes manually
            Projectile.timeLeft = 600;
            Projectile.extraUpdates = 1;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;
            Projectile.alpha = 0;
            Projectile.light = 0.5f;
        }

        // =================================================================
        // AI — Gravity Arc
        // =================================================================

        public override void AI()
        {
            AliveTime++;
            Projectile.rotation = Projectile.velocity.ToRotation();

            // Gravity — arcing trajectory
            if (Projectile.velocity.Y < MaxFallSpeed)
                Projectile.velocity.Y += Gravity;

            // Lighting
            Color lightCol = CometUtils.SupernovaColor;
            Lighting.AddLight(Projectile.Center, lightCol.ToVector3() * 0.5f);

            SpawnFlightParticles();
        }

        private void SpawnFlightParticles()
        {
            if (Main.dedServ) return;

            // Ember trail — every tick
            Vector2 offset = Main.rand.NextVector2Circular(4f, 4f);
            Vector2 emberVel = -Projectile.velocity.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(1f, 3f)
                + Main.rand.NextVector2Circular(1f, 1f);
            CometParticleHandler.Spawn(new EmberTrailParticle(
                Projectile.Center + offset, emberVel,
                0.35f + Main.rand.NextFloat(0.2f), 15 + Main.rand.Next(8)));

            // Dust — every 2 ticks
            if (AliveTime % 2 == 0)
            {
                int d = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height,
                    ModContent.DustType<CometDust>(), -Projectile.velocity.X * 0.1f, -Projectile.velocity.Y * 0.1f);
                Main.dust[d].noGravity = true;
                Main.dust[d].scale = 0.9f;
            }

            // Violet mist around shell
            if (AliveTime % 5 == 0)
            {
                CometParticleHandler.Spawn(new CometMistParticle(
                    Projectile.Center, Main.rand.NextVector2Circular(0.5f, 0.5f),
                    CometUtils.SupernovaColor * 0.4f, 0.4f, 20));
            }
        }

        // =================================================================
        // TILE COLLISION — Explode
        // =================================================================

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Explode();
            return true; // kill projectile
        }

        // =================================================================
        // ENEMY HIT — Explode
        // =================================================================

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (HasExploded == 0f)
                Explode();
        }

        // =================================================================
        // EXPLOSION
        // =================================================================

        private void Explode()
        {
            if (HasExploded != 0f) return;
            HasExploded = 1f;

            // Screen shake
            if (Projectile.owner == Main.myPlayer)
            {
                var shakePlayer = Main.LocalPlayer.GetModPlayer<ScreenShakePlayer>();
                shakePlayer.AddShake(10f, 30);
            }

            // AoE damage
            if (Projectile.owner == Main.myPlayer)
                ApplyExplosionDamage();

            // Spawn lunar fragments
            if (Projectile.owner == Main.myPlayer)
                SpawnFragments();

            // Sound
            SoundEngine.PlaySound(SoundID.Item14 with { Volume = 0.8f, Pitch = -0.4f }, Projectile.Center);

            // VFX
            SpawnExplosionVFX();

            // Add charges to player
            if (Projectile.owner == Main.myPlayer)
                Main.LocalPlayer.Resurrection().AddCharge(3);
        }

        private void ApplyExplosionDamage()
        {
            int explosionDamage = (int)(Projectile.damage * ExplosionDamageMultiplier);

            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (!npc.CanBeChasedBy(Projectile)) continue;
                float dist = Vector2.Distance(npc.Center, Projectile.Center);
                if (dist > ExplosionRadius) continue;

                // Distance-based damage falloff
                float falloff = 1f - (dist / ExplosionRadius) * 0.4f;
                int scaledDamage = (int)(explosionDamage * falloff);

                Player owner = Main.player[Projectile.owner];
                NPC.HitInfo hitInfo = npc.CalculateHitInfo(scaledDamage, Projectile.velocity.X > 0 ? 1 : -1,
                    false, Projectile.knockBack * 1.5f, Projectile.DamageType);
                npc.StrikeNPC(hitInfo, false, false);

                // Heavy Lunar Impact debuff
                npc.AddBuff(ModContent.BuffType<LunarImpact>(), 600); // 10 seconds
                var impactNpc = npc.GetGlobalNPC<LunarImpactNPC>();
                impactNpc.AddStack();
                impactNpc.AddStack();
                impactNpc.AddStack(); // Triple stack for Supernova
            }
        }

        private void SpawnFragments()
        {
            int fragmentDamage = (int)(Projectile.damage * FragmentDamageMultiplier);

            for (int i = 0; i < FragmentCount; i++)
            {
                float angle = MathHelper.TwoPi * i / FragmentCount + Main.rand.NextFloat(-0.2f, 0.2f);
                Vector2 vel = angle.ToRotationVector2() * (8f + Main.rand.NextFloat(4f));

                // Fragments use the Standard ricochet projectile with pre-set bounces
                int proj = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center,
                    vel, ModContent.ProjectileType<ResurrectionProjectile>(),
                    fragmentDamage, Projectile.knockBack * 0.5f, Projectile.owner);

                if (proj >= 0 && proj < Main.maxProjectiles)
                {
                    Main.projectile[proj].ai[0] = 5; // Start at bounce 5 (already heated up)
                    Main.projectile[proj].timeLeft = 300;
                    Main.projectile[proj].penetrate = 6;
                }
            }
        }

        private void SpawnExplosionVFX()
        {
            if (Main.dedServ) return;

            // Multi-layer crater bloom cascade (5 layers, staggered)
            for (int layer = 0; layer < 5; layer++)
            {
                float layerPhase = layer / 4f;
                Color layerColor = CometUtils.GetCometGradient(layerPhase);
                float layerScale = 2f + layer * 0.8f;
                int layerLife = 25 + layer * 5;
                CometParticleHandler.Spawn(new CraterBloomParticle(
                    Projectile.Center, layerColor, layerScale, layerLife));
            }

            // Core flash
            CometParticleHandler.Spawn(new CraterBloomParticle(
                Projectile.Center, CometUtils.FrigidImpact, 3f, 15));

            // Shockwave rings (3 expanding rings)
            for (int ring = 0; ring < 3; ring++)
            {
                float ringScale = 3f + ring * 2f;
                Color ringCol = Color.Lerp(CometUtils.CometCoreWhite, CometUtils.SupernovaColor, ring / 2f);
                CometParticleHandler.Spawn(new ShockwaveRingParticle(
                    Projectile.Center, ringCol, ringScale, 30 + ring * 8));
            }

            // Massive radial ember burst
            for (int i = 0; i < 40; i++)
            {
                float angle = MathHelper.TwoPi * i / 40f + Main.rand.NextFloat(-0.1f, 0.1f);
                float speed = 4f + Main.rand.NextFloat(8f);
                Vector2 vel = angle.ToRotationVector2() * speed;
                CometParticleHandler.Spawn(new EmberTrailParticle(
                    Projectile.Center, vel,
                    0.5f + Main.rand.NextFloat(0.4f),
                    25 + Main.rand.Next(20)));
            }

            // Lunar shards flung outward
            for (int i = 0; i < 15; i++)
            {
                Vector2 vel = Main.rand.NextVector2CircularEdge(6f, 6f) + Main.rand.NextVector2Circular(2f, 2f);
                CometParticleHandler.Spawn(new LunarShardParticle(
                    Projectile.Center, vel,
                    CometUtils.GetCometGradient(Main.rand.NextFloat()),
                    0.5f + Main.rand.NextFloat(0.4f), 30 + Main.rand.Next(15)));
            }

            // Comet mist cloud
            for (int i = 0; i < 12; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(2f, 2f);
                CometParticleHandler.Spawn(new CometMistParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(30f, 30f),
                    vel, CometUtils.GetCometGradient(Main.rand.NextFloat()) * 0.4f,
                    1.5f + Main.rand.NextFloat(1f), 40 + Main.rand.Next(20)));
            }

            // Heavy dust explosion
            for (int i = 0; i < 30; i++)
            {
                Vector2 dustVel = Main.rand.NextVector2CircularEdge(8f, 8f);
                int d = Dust.NewDust(Projectile.Center - new Vector2(8), 16, 16,
                    ModContent.DustType<CometDust>(), dustVel.X, dustVel.Y);
                Main.dust[d].noGravity = true;
                Main.dust[d].scale = 1.5f + Main.rand.NextFloat(0.8f);
            }
        }

        // =================================================================
        // RENDERING
        // =================================================================

        public override bool PreDraw(ref Color lightColor)
        {
            if (Main.dedServ) return false;

            // Pass 1: Glow trail
            DrawGlowTrail();

            // Pass 2: Main shell trail
            DrawMainTrail();

            // Pass 3: Head glow
            DrawHeadGlow();

            return false;
        }

        private void DrawGlowTrail()
        {
            MiscShaderData glowShader = null;
            if (GameShaders.Misc.TryGetValue("MagnumOpus:CometTrailGlow", out var shader))
            {
                glowShader = shader;
                glowShader.UseColor(CometUtils.SupernovaColor);
                glowShader.UseSecondaryColor(CometUtils.DeepSpaceViolet);
                glowShader.UseOpacity(0.5f);
                glowShader.UseSaturation(0.5f); // mid-phase (not yet detonated)
            }

            CometTrailRenderer.RenderTrail(Projectile.oldPos, new CometTrailSettings(
                widthFunction: (completion, _) =>
                {
                    float taper = 1f - completion;
                    return ShellWidth * 2f * taper * CometUtils.SineOut(1f - completion);
                },
                colorFunction: (completion, _) =>
                {
                    Color col = Color.Lerp(CometUtils.SupernovaColor, CometUtils.DeepSpaceViolet, completion);
                    return col * 0.35f * (1f - completion * 0.4f);
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
                mainShader.UseColor(CometUtils.CometCoreWhite);
                mainShader.UseSecondaryColor(CometUtils.SupernovaColor);
                mainShader.UseOpacity(0.7f);
                mainShader.UseSaturation(0.5f);
            }

            CometTrailRenderer.RenderTrail(Projectile.oldPos, new CometTrailSettings(
                widthFunction: (completion, _) =>
                {
                    float taper = 1f - completion;
                    return ShellWidth * taper * CometUtils.SineOut(1f - completion);
                },
                colorFunction: (completion, _) =>
                {
                    Color col = Color.Lerp(CometUtils.CometCoreWhite, CometUtils.SupernovaColor, completion);
                    return col * 0.7f * (1f - completion * 0.3f);
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

            // Pulsing violet glow
            float pulse = 0.8f + MathF.Sin(AliveTime * 0.15f) * 0.2f;
            sb.Draw(bloom, drawPos, null, CometUtils.SupernovaColor * 0.5f * pulse, 0f, origin, 0.7f, SpriteEffects.None, 0f);

            // Core glow
            sb.Draw(bloom, drawPos, null, CometUtils.CometCoreWhite * 0.5f, 0f, origin, 0.35f, SpriteEffects.None, 0f);
        }

        // =================================================================
        // DEATH VFX (in case of timeout without hitting anything)
        // =================================================================

        public override void OnKill(int timeLeft)
        {
            // If we haven't exploded yet (killed by timeout), do a smaller burst
            if (HasExploded == 0f && !Main.dedServ)
            {
                CometParticleHandler.Spawn(new CraterBloomParticle(
                    Projectile.Center, CometUtils.SupernovaColor, 1f, 20));

                for (int i = 0; i < 8; i++)
                {
                    Vector2 vel = Main.rand.NextVector2CircularEdge(4f, 4f);
                    CometParticleHandler.Spawn(new EmberTrailParticle(
                        Projectile.Center, vel, 0.4f, 15));
                }
            }
        }
    }
}
