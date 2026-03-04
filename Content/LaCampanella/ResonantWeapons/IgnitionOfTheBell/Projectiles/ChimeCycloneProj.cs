using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.IgnitionOfTheBell.Utilities;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.IgnitionOfTheBell.Particles;
using MagnumOpus.Content.LaCampanella;
using MagnumOpus.Content.LaCampanella.Debuffs;
using MagnumOpus.Content.FoundationWeapons.ImpactFoundation;
using MagnumOpus.Content.FoundationWeapons.ExplosionParticlesFoundation;
using MagnumOpus.Content.FoundationWeapons.XSlashFoundation;
using MagnumOpus.Content.FoundationWeapons.SmokeFoundation;

namespace MagnumOpus.Content.LaCampanella.ResonantWeapons.IgnitionOfTheBell.Projectiles
{
    /// <summary>
    /// ChimeCycloneProj - Fire cyclone vortex from Chime Cyclone (Phase 3).
    /// Pulls enemies inward for 2 seconds, then detonates.
    /// Every 3rd Cyclone detonation triggers Chimequake via IgnitionOfTheBellPlayer.
    /// Chimequake creates persistent ground fire in large area for 3 seconds.
    /// </summary>
    public class ChimeCycloneProj : ModProjectile
    {
        // Phase timing: 120 ticks (2s) pull + instantaneous detonation
        private const int PullDuration = 120;
        private const float PullRadius = 200f;
        private const float PullForce = 3.5f;
        private const float DetonationRadius = 160f;

        // Chimequake: large AoE fire zone
        private const float ChimequakeRadius = 280f;
        private const int ChimequakeDuration = 180; // 3 seconds

        private float _spinAngle;
        private bool _initialized;
        private int _timer;
        private bool _detonated;

        public override string Texture => "MagnumOpus/Content/LaCampanella/ResonantWeapons/IgnitionOfTheBell/IgnitionOfTheBell";

        public override void SetDefaults()
        {
            Projectile.width = 200;
            Projectile.height = 200;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 30; // Hit during pull phase periodically
            Projectile.timeLeft = PullDuration + 10;
        }

        public override void AI()
        {
            if (!_initialized)
            {
                _initialized = true;
                _spinAngle = Main.rand.NextFloat() * MathHelper.TwoPi;
                _timer = 0;
                _detonated = false;

                // Initial vortex formation flash
                IgnitionOfTheBellParticleHandler.SpawnParticle(
                    new BellIgnitionFlashParticle(Projectile.Center, 12, 2f));

                SoundEngine.PlaySound(SoundID.Item45 with { Pitch = -0.3f, Volume = 0.7f }, Projectile.Center);
            }

            _timer++;

            if (_timer >= PullDuration && !_detonated)
            {
                // DETONATION
                _detonated = true;
                Detonate();
                Projectile.Kill();
                return;
            }

            float progress = (float)_timer / PullDuration;
            _spinAngle += 0.2f + progress * 0.35f; // Accelerating spin

            // Pull enemies inward
            PullEnemies(progress);

            // Spiraling cyclone flame particles
            SpawnCycloneParticles(progress);

            // Pulsing light - intensifies as detonation approaches
            float intensity = 0.4f + 0.4f * progress;
            Lighting.AddLight(Projectile.Center, new Vector3(0.5f, 0.15f, 0.02f) * intensity);
        }

        private void PullEnemies(float progress)
        {
            float pullStrength = PullForce * (0.3f + 0.7f * progress); // Stronger near end

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage)
                    continue;

                float dist = Vector2.Distance(Projectile.Center, npc.Center);
                if (dist > PullRadius || dist < 10f)
                    continue;

                // Pull toward center with inverse-distance scaling
                Vector2 pullDir = (Projectile.Center - npc.Center).SafeNormalize(Vector2.Zero);
                float distFactor = 1f - (dist / PullRadius);
                npc.velocity += pullDir * pullStrength * distFactor * 0.5f;

                // Cap pull velocity so enemies don't teleport
                if (npc.velocity.Length() > 12f)
                    npc.velocity = npc.velocity.SafeNormalize(Vector2.Zero) * 12f;
            }
        }

        private void SpawnCycloneParticles(float progress)
        {
            // Spiraling flame ring
            int particleCount = (int)(3 + progress * 5);
            for (int i = 0; i < particleCount; i++)
            {
                float angle = _spinAngle + i * MathHelper.TwoPi / particleCount;
                float currentRadius = PullRadius * (0.2f + 0.6f * (1f - progress)); // Contracts as it pulls

                IgnitionOfTheBellParticleHandler.SpawnParticle(
                    new CycloneFlameParticle(
                        Projectile.Center,
                        angle,
                        0.12f + Main.rand.NextFloat(0.05f),
                        currentRadius * Main.rand.NextFloat(0.5f, 1f),
                        Main.rand.NextFloat(0.5f, 2f),
                        Main.rand.Next(12, 22),
                        Main.rand.NextFloat(0.4f, 0.8f)));
            }

            // Inward-spiraling embers
            if (Main.rand.NextBool(2))
            {
                float emberAngle = Main.rand.NextFloat() * MathHelper.TwoPi;
                float spawnRadius = PullRadius * Main.rand.NextFloat(0.5f, 1f);
                Vector2 emberPos = Projectile.Center + emberAngle.ToRotationVector2() * spawnRadius;
                Vector2 inwardVel = (Projectile.Center - emberPos).SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(2f, 4f);
                inwardVel += (emberAngle + MathHelper.PiOver2).ToRotationVector2() * Main.rand.NextFloat(1f, 3f);

                IgnitionOfTheBellParticleHandler.SpawnParticle(
                    new ThrustEmberParticle(emberPos, inwardVel,
                        Main.rand.NextFloat(0.5f, 1f), 20, 0.35f));
            }

            // Vanilla dust ring
            for (int i = 0; i < 2; i++)
            {
                float dustAngle = _spinAngle + Main.rand.NextFloat() * MathHelper.TwoPi;
                float dustRadius = PullRadius * (1f - progress) * Main.rand.NextFloat(0.3f, 1f);
                Vector2 dustPos = Projectile.Center + dustAngle.ToRotationVector2() * dustRadius;

                Dust d = Dust.NewDustPerfect(dustPos, DustID.Torch,
                    (dustAngle + MathHelper.PiOver2).ToRotationVector2() * 2f,
                    0, IgnitionOfTheBellUtils.GetCycloneGradient(Main.rand.NextFloat()), 1.1f);
                d.noGravity = true;
            }
        }

        private void Detonate()
        {
            Player owner = Main.player[Projectile.owner];

            // Massive detonation damage to all enemies in radius
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage)
                    continue;

                float dist = Vector2.Distance(Projectile.Center, npc.Center);
                if (dist < DetonationRadius)
                {
                    float falloff = 1f - (dist / DetonationRadius) * 0.4f;
                    int detDamage = (int)(Projectile.damage * 1.5f * falloff);

                    NPC.HitInfo hitInfo = new NPC.HitInfo
                    {
                        Damage = detDamage,
                        Knockback = 10f,
                        HitDirection = Math.Sign(npc.Center.X - Projectile.Center.X),
                        Crit = false,
                        DamageType = DamageClass.MeleeNoSpeed
                    };
                    npc.StrikeNPC(hitInfo);

                    // Apply heavy Resonant Toll
                    npc.GetGlobalNPC<ResonantTollNPC>().AddStacks(npc, 3);
                }
            }

            // Check for Chimequake
            var tracker = owner.IgnitionOfTheBell();
            bool chimequake = tracker.RegisterCycloneDetonation();

            // Detonation VFX - massive radial burst
            for (int ring = 0; ring < 3; ring++)
            {
                int dustCount = 20 + ring * 10;
                float radius = 30f + ring * 50f;
                for (int i = 0; i < dustCount; i++)
                {
                    float angle = MathHelper.TwoPi * i / dustCount;
                    Vector2 dir = angle.ToRotationVector2();
                    Vector2 vel = dir * (5f + ring * 2f);
                    Dust d = Dust.NewDustPerfect(Projectile.Center + dir * radius * 0.3f,
                        DustID.Torch, vel, 0,
                        IgnitionOfTheBellUtils.GetMagmaFlicker(Main.rand.NextFloat()), 1.5f - ring * 0.2f);
                    d.noGravity = true;
                }
            }

            // Ember shower
            for (int i = 0; i < 16; i++)
            {
                Vector2 burstVel = Main.rand.NextVector2CircularEdge(6f, 6f);
                IgnitionOfTheBellParticleHandler.SpawnParticle(
                    new ThrustEmberParticle(Projectile.Center, burstVel,
                        Main.rand.NextFloat(0.6f, 1f), 25, 0.45f));
            }

            // Big flash
            IgnitionOfTheBellParticleHandler.SpawnParticle(
                new BellIgnitionFlashParticle(Projectile.Center, 14, 2.5f));

            // === FOUNDATION: RippleEffectProjectile — Cyclone detonation shockwave ===
            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero,
                ModContent.ProjectileType<RippleEffectProjectile>(),
                0, 0f, Projectile.owner);

            // === FOUNDATION: SparkExplosionProjectile — Cyclone detonation radial scatter ===
            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero,
                ModContent.ProjectileType<SparkExplosionProjectile>(),
                0, 0f, Projectile.owner,
                ai0: (float)SparkMode.RadialScatter);

            // Detonation sound
            SoundEngine.PlaySound(SoundID.Item14 with { Pitch = -0.2f, Volume = 1.0f }, Projectile.Center);

            // CHIMEQUAKE if threshold met
            if (chimequake)
            {
                SpawnChimequake(owner);
            }
        }

        private void SpawnChimequake(Player owner)
        {
            Vector2 center = Projectile.Center;

            // Screen shake effect via camera bump
            SoundEngine.PlaySound(SoundID.Item14 with { Pitch = -0.5f, Volume = 1.2f }, center);
            SoundEngine.PlaySound(SoundID.Item45 with { Pitch = -0.4f, Volume = 1.0f }, center);

            // Massive ground fire crack effect
            // Spawn lingering damage in large area
            for (int ring = 0; ring < 5; ring++)
            {
                int dustCount = 30 + ring * 8;
                float radius = 40f + ring * 50f;
                for (int i = 0; i < dustCount; i++)
                {
                    float angle = MathHelper.TwoPi * i / dustCount + Main.rand.NextFloat(-0.1f, 0.1f);
                    Vector2 dir = angle.ToRotationVector2();
                    Vector2 pos = center + dir * radius;
                    Vector2 vel = dir * Main.rand.NextFloat(1f, 3f) + new Vector2(0, -Main.rand.NextFloat(0.5f, 2f));

                    Dust d = Dust.NewDustPerfect(pos, DustID.Torch, vel, 0,
                        Color.Lerp(new Color(255, 100, 0), new Color(255, 240, 200), Main.rand.NextFloat()),
                        1.8f - ring * 0.2f);
                    d.noGravity = true;
                    d.fadeIn = 1.5f;
                }
            }

            // Black smoke columns
            for (int i = 0; i < 20; i++)
            {
                Vector2 smokePos = center + Main.rand.NextVector2Circular(ChimequakeRadius * 0.6f, 30f);
                Vector2 smokeVel = new Vector2(Main.rand.NextFloat(-1f, 1f), -Main.rand.NextFloat(2f, 5f));
                Dust d = Dust.NewDustPerfect(smokePos, DustID.Smoke, smokeVel, 150,
                    new Color(20, 15, 10), 3f);
                d.noGravity = true;
            }

            // Massive flash
            IgnitionOfTheBellParticleHandler.SpawnParticle(
                new BellIgnitionFlashParticle(center, 18, 3.5f));

            // === FOUNDATION: XSlashEffect — Chimequake massive ground X-cross detonation ===
            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(), center, Vector2.Zero,
                ModContent.ProjectileType<XSlashEffect>(),
                0, 0f, Projectile.owner,
                ai0: Main.rand.NextFloat(MathHelper.TwoPi), ai1: (float)XSlashStyle.LaCampanella);

            // === FOUNDATION: DamageZoneProjectile — Persistent chimequake flame zone ===
            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(), center, Vector2.Zero,
                ModContent.ProjectileType<DamageZoneProjectile>(),
                (int)(Projectile.damage * 0.5f), 0f, Projectile.owner);

            // === FOUNDATION: SparkExplosionProjectile — Chimequake massive spark burst ===
            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(), center, Vector2.Zero,
                ModContent.ProjectileType<SparkExplosionProjectile>(),
                0, 0f, Projectile.owner,
                ai0: (float)SparkMode.SpiralShrapnel);

            // Chimequake damage: hurt all enemies in ChimequakeRadius
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage)
                    continue;

                float dist = Vector2.Distance(center, npc.Center);
                if (dist < ChimequakeRadius)
                {
                    float falloff = 1f - (dist / ChimequakeRadius) * 0.3f;
                    int quakeDmg = (int)(Projectile.damage * 2f * falloff);

                    NPC.HitInfo hitInfo = new NPC.HitInfo
                    {
                        Damage = quakeDmg,
                        Knockback = 12f,
                        HitDirection = Math.Sign(npc.Center.X - center.X),
                        Crit = false,
                        DamageType = DamageClass.MeleeNoSpeed
                    };
                    npc.StrikeNPC(hitInfo);

                    // Heavy toll stacks
                    npc.GetGlobalNPC<ResonantTollNPC>().AddStacks(npc, 4);
                }
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Pull-phase periodic damage applies Resonant Toll
            target.GetGlobalNPC<ResonantTollNPC>().AddStacks(target, 1);

            for (int i = 0; i < 3; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2CircularEdge(3f, 3f);
                IgnitionOfTheBellParticleHandler.SpawnParticle(
                    new ThrustEmberParticle(target.Center, sparkVel,
                        Main.rand.NextFloat(0.5f, 1f), 12, 0.3f));
            }
        }

        private static int _lastParticleDrawFrame;

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            try
            {
                DrawCycloneAura(sb);
                DrawCycloneParticles(sb);
            }
            catch
            {
                try
                {
                    sb.End();
                    sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                        DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
                }
                catch { }
            }
            return false;
        }

        private void DrawCycloneAura(SpriteBatch sb)
        {
            Texture2D bloomTex = null;
            try
            {
                bloomTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftGlow",
                    ReLogic.Content.AssetRequestMode.ImmediateLoad)?.Value;
            }
            catch { }
            if (bloomTex == null) return;

            float progress = (float)_timer / PullDuration;
            Vector2 screenPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = new Vector2(bloomTex.Width / 2f, bloomTex.Height / 2f);

            // Intensifying pulse as detonation approaches
            float pulse = 0.6f + 0.4f * (float)Math.Sin(_timer * 0.15f + progress * 3f);
            float intensity = 0.5f + 0.5f * progress;

            try { sb.End(); } catch { }
            try
            {
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // Outer contracting vortex glow
            float outerScale = (3f - progress * 1.5f) * intensity;
            sb.Draw(bloomTex, screenPos, null,
                IgnitionOfTheBellUtils.Additive(new Color(140, 20, 0), 0.2f * pulse * intensity),
                _spinAngle, origin, outerScale, SpriteEffects.None, 0f);

            // Mid rotating ring
            float midScale = (1.5f - progress * 0.5f) * intensity;
            sb.Draw(bloomTex, screenPos, null,
                IgnitionOfTheBellUtils.Additive(new Color(255, 100, 10), 0.3f * pulse * intensity),
                -_spinAngle * 0.7f, origin, midScale, SpriteEffects.None, 0f);

            // Intensifying hot core
            float coreIntensity = 0.3f + progress * 0.5f;
            sb.Draw(bloomTex, screenPos, null,
                IgnitionOfTheBellUtils.Additive(new Color(255, 210, 80), coreIntensity),
                0f, origin, 0.5f + progress * 0.3f, SpriteEffects.None, 0f);

            // --- LC Impact Ellipse — expanding infernal shockwave ring in vortex ---
            {
                float ellipseScale = (1.8f - progress * 0.8f) * pulse;
                LaCampanellaVFXLibrary.DrawImpactEllipse(sb, screenPos,
                    ellipseScale * 0.3f, _spinAngle * 0.3f,
                    0.18f * pulse * intensity, LaCampanellaPalette.InfernalOrange);
            }

            // --- LC Power Effect Ring — contracting ring as vortex intensifies ---
            if (progress > 0.3f)
            {
                float ringIntensity = (progress - 0.3f) / 0.7f;
                LaCampanellaVFXLibrary.DrawPowerEffectRing(sb, screenPos,
                    (1.2f - ringIntensity * 0.5f) * pulse * 0.3f,
                    -_spinAngle * 0.5f,
                    0.2f * ringIntensity * pulse, LaCampanellaPalette.FlameYellow);
            }
            }
            catch { }
            finally
            {
                try { sb.End(); } catch { }
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            }
        }

        private void DrawCycloneParticles(SpriteBatch sb)
        {
            int currentFrame = (int)Main.GameUpdateCount;
            if (_lastParticleDrawFrame != currentFrame)
            {
                _lastParticleDrawFrame = currentFrame;
                IgnitionOfTheBellParticleHandler handler = ModContent.GetInstance<IgnitionOfTheBellParticleHandler>();
                handler?.DrawAllParticles(sb);
            }
        }
    }
}
