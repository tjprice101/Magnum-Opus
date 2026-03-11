using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.FangOfTheInfiniteBell.Utilities;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.FangOfTheInfiniteBell.Particles;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.FangOfTheInfiniteBell.Primitives;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.FangOfTheInfiniteBell.Shaders;
using MagnumOpus.Content.LaCampanella;
using MagnumOpus.Content.LaCampanella.Debuffs;
using MagnumOpus.Content.FoundationWeapons.ImpactFoundation;
using MagnumOpus.Content.FoundationWeapons.ExplosionParticlesFoundation;
using Terraria.Graphics.Shaders;

namespace MagnumOpus.Content.LaCampanella.ResonantWeapons.FangOfTheInfiniteBell.Projectiles
{
    /// <summary>
    /// InfiniteBellOrbProj — Bouncing bell-shaped energy orb.
    /// ai[0] = max bounces remaining, ai[1] = 1 if Infinite Crescendo variant.
    /// 
    /// Normal: bounces between enemies 2 times, spawns echo orb each bounce (half dmg, 1 bounce).
    /// Crescendo: bounces 10 times, full damage, spawns 4 echo orbs per bounce.
    /// Each bounce registers a stack on the owning player's FangOfTheInfiniteBellPlayer.
    /// At 10+ stacks: spawns EmpoweredLightningProj arcs toward nearby airborne orbs.
    /// At 20 stacks (normal orbs only): explodes on final bounce.
    /// </summary>
    public class InfiniteBellOrbProj : ModProjectile
    {
        private const int MaxTrailPositions = 20;
        private Vector2[] _trailPositions = new Vector2[MaxTrailPositions];
        private int _trailIndex;
        private FangOfTheInfiniteBellPrimitiveRenderer _trailRenderer;
        private VertexStrip _strip;

        private int BouncesRemaining { get => (int)Projectile.ai[0]; set => Projectile.ai[0] = value; }
        private bool IsCrescendo => Projectile.ai[1] == 1f;
        private bool _hasHitAnEnemy;
        private int _seekTimer;
        private int _lightningCooldown;

        public override string Texture => "MagnumOpus/Content/LaCampanella/ResonantWeapons/FangOfTheInfiniteBell/FangOfTheInfiniteBell";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 16;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1; // Bouncing handles "hits"
            Projectile.timeLeft = 300;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 30;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            _trailRenderer ??= new FangOfTheInfiniteBellPrimitiveRenderer();

            // Record trail
            if (Main.GameUpdateCount % 2 == 0)
            {
                _trailPositions[_trailIndex % MaxTrailPositions] = Projectile.Center;
                _trailIndex++;
            }

            // Scale for Crescendo orbs
            if (IsCrescendo)
            {
                Projectile.width = 40;
                Projectile.height = 40;
                Projectile.scale = 1.6f;
            }

            // Seek nearest enemy after initial travel
            _seekTimer++;
            if (_seekTimer > 10)
            {
                NPC target = FangOfTheInfiniteBellUtils.ClosestNPCAt(Projectile.Center, 480f);
                if (target != null)
                {
                    Vector2 toTarget = target.Center - Projectile.Center;
                    float dist = toTarget.Length();
                    if (dist > 1f)
                    {
                        toTarget /= dist;
                        float turnSpeed = IsCrescendo ? 0.06f : 0.08f;
                        Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * Projectile.velocity.Length(), turnSpeed);
                    }
                }
            }

            // Lightning arcs at 10+ stacks
            Player owner = Main.player[Projectile.owner];
            if (owner.active)
            {
                var fbPlayer = owner.FangOfTheInfiniteBell();
                if (fbPlayer.HasLightningArcs && _lightningCooldown <= 0)
                {
                    TrySpawnLightningArc(owner);
                    _lightningCooldown = 30; // 0.5s cooldown
                }
            }
            if (_lightningCooldown > 0) _lightningCooldown--;

            // Particle trail — sparks + musical bell notes
            if (Main.rand.NextBool(3))
            {
                Vector2 dustVel = Main.rand.NextVector2Circular(1f, 1f);
                FangOfTheInfiniteBellParticleHandler.SpawnParticle(
                    new EmpoweredSparkParticle(Projectile.Center + Main.rand.NextVector2Circular(6f, 6f),
                        dustVel, 10, IsCrescendo ? 0.4f : 0.25f));
            }

            // Musical bell notes drifting from the orb — reinforces bell identity
            if (Main.rand.NextBool(IsCrescendo ? 6 : 10))
            {
                Vector2 noteVel = new Vector2(Main.rand.NextFloatDirection() * 0.8f, -Main.rand.NextFloat(0.3f, 0.8f));
                FangOfTheInfiniteBellParticleHandler.SpawnParticle(
                    new MusicalBellNoteParticle(Projectile.Center + Main.rand.NextVector2Circular(8f, 8f),
                        noteVel, Main.rand.Next(25, 45), IsCrescendo ? 0.35f : 0.25f, IsCrescendo));
            }

            // Gentle light
            float lightIntensity = IsCrescendo ? 0.8f : 0.5f;
            Lighting.AddLight(Projectile.Center, new Vector3(0.6f, 0.4f, 0.1f) * lightIntensity);

            // Rotation
            Projectile.rotation += 0.08f;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Apply Resonant Toll debuff
            target.GetGlobalNPC<ResonantTollNPC>().AddStacks(target, 1);

            _hasHitAnEnemy = true;

            if (BouncesRemaining > 0)
            {
                BouncesRemaining--;

                // Register bounce stack on player
                Player owner = Main.player[Projectile.owner];
                if (owner.active)
                    owner.FangOfTheInfiniteBell().RegisterBounce();

                // Spawn echo orbs
                SpawnEchoOrbs(target);

                // Spawn bounce impact particles
                SpawnBounceImpactParticles(target.Center);

                // Find next bounce target
                NPC nextTarget = FindNextBounceTarget(target);
                if (nextTarget != null)
                {
                    // Redirect toward next target
                    Vector2 toNext = (nextTarget.Center - Projectile.Center);
                    float len = toNext.Length();
                    if (len > 1f)
                        Projectile.velocity = (toNext / len) * Projectile.velocity.Length();

                    // Reset local immunity so we can hit the next target
                    for (int i = 0; i < Projectile.localNPCImmunity.Length; i++)
                        Projectile.localNPCImmunity[i] = 0;
                    // But keep this target immune
                    if (target.whoAmI >= 0 && target.whoAmI < Projectile.localNPCImmunity.Length)
                        Projectile.localNPCImmunity[target.whoAmI] = 60;

                    SoundEngine.PlaySound(SoundID.Item28, Projectile.Center);
                }
                else
                {
                    // No more targets — die
                    HandleFinalBounce(owner);
                }
            }
            else
            {
                // No bounces left
                Player owner = Main.player[Projectile.owner];
                HandleFinalBounce(owner);
            }
        }

        private void HandleFinalBounce(Player owner)
        {
            // At 20 stacks (non-Crescendo): explode on final bounce
            if (owner.active)
            {
                var fbPlayer = owner.FangOfTheInfiniteBell();
                if (fbPlayer.HasFinalBounceExplosion || IsCrescendo)
                {
                    // Explosion VFX
                    SpawnExplosion();
                }
            }
            Projectile.Kill();
        }

        private void SpawnEchoOrbs(NPC hitTarget)
        {
            int echoCount = IsCrescendo ? 4 : 1;
            int echoDamage = IsCrescendo ? Projectile.damage : Projectile.damage / 2;

            for (int i = 0; i < echoCount; i++)
            {
                float angle = MathHelper.TwoPi / echoCount * i + Main.rand.NextFloat(-0.3f, 0.3f);
                Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 6f;

                // Echo orb: ai[0] = 1 bounce, ai[1] = 0 (not Crescendo)
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, vel,
                    Type, echoDamage, Projectile.knockBack * 0.5f, Projectile.owner, 1f, 0f);
            }
        }

        private NPC FindNextBounceTarget(NPC exclude)
        {
            NPC best = null;
            float bestDist = 480f; // 30 tiles
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage || npc.whoAmI == exclude.whoAmI)
                    continue;
                float dist = Vector2.Distance(Projectile.Center, npc.Center);
                if (dist < bestDist) { bestDist = dist; best = npc; }
            }
            return best;
        }

        private void TrySpawnLightningArc(Player owner)
        {
            // Find another airborne InfiniteBellOrbProj nearby
            int myType = Projectile.type;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile other = Main.projectile[i];
                if (!other.active || other.whoAmI == Projectile.whoAmI || other.type != myType || other.owner != Projectile.owner)
                    continue;
                float dist = Vector2.Distance(Projectile.Center, other.Center);
                if (dist < 320f && dist > 32f) // Within 20 tiles but not too close
                {
                    // Spawn EmpoweredLightningProj between us
                    int lightDmg = (int)(Projectile.damage * 0.3f);
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero,
                        ModContent.ProjectileType<EmpoweredLightningProj>(),
                        lightDmg, 0f, Projectile.owner, other.Center.X, other.Center.Y);
                    break; // One arc per cooldown
                }
            }
        }

        private void SpawnBounceImpactParticles(Vector2 position)
        {
            for (int i = 0; i < 10; i++)
            {
                Vector2 vel = Main.rand.NextVector2CircularEdge(4f, 4f) * Main.rand.NextFloat(0.5f, 1f);
                FangOfTheInfiniteBellParticleHandler.SpawnParticle(
                    new EmpoweredSparkParticle(position, vel, 15, 0.3f));
            }
            FangOfTheInfiniteBellParticleHandler.SpawnParticle(
                new ArcaneFlashParticle(position, 10, 1.2f, false));

            // Musical bell notes cascade on bounce — 3-5 notes scatter upward
            int noteCount = Main.rand.Next(3, 6);
            for (int i = 0; i < noteCount; i++)
            {
                Vector2 noteVel = new Vector2(Main.rand.NextFloatDirection() * 2f, -Main.rand.NextFloat(1f, 2.5f));
                FangOfTheInfiniteBellParticleHandler.SpawnParticle(
                    new MusicalBellNoteParticle(position + Main.rand.NextVector2Circular(10f, 10f),
                        noteVel, Main.rand.Next(30, 55), Main.rand.NextFloat(0.3f, 0.5f), IsCrescendo));
            }

            // === FOUNDATION: RippleEffectProjectile — Bell chime ring on bounce impact ===
            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(), position, Vector2.Zero,
                ModContent.ProjectileType<RippleEffectProjectile>(),
                0, 0f, Projectile.owner, ai0: 1f);
        }

        private void SpawnExplosion()
        {
            // Large explosion VFX at death position
            float radius = IsCrescendo ? 1.8f : 1.2f;
            FangOfTheInfiniteBellParticleHandler.SpawnParticle(
                new ArcaneFlashParticle(Projectile.Center, 15, radius, true));

            for (int i = 0; i < 16; i++)
            {
                Vector2 vel = Main.rand.NextVector2CircularEdge(6f, 6f) * Main.rand.NextFloat(0.6f, 1.2f);
                FangOfTheInfiniteBellParticleHandler.SpawnParticle(
                    new EmpoweredSparkParticle(Projectile.Center, vel, 20, 0.4f));
            }

            // Splash damage in radius
            float aoeRadius = IsCrescendo ? 120f : 80f;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage) continue;
                if (Vector2.Distance(Projectile.Center, npc.Center) <= aoeRadius)
                {
                    Player owner = Main.player[Projectile.owner];
                    int splashDmg = (int)(Projectile.damage * 0.5f);
                    owner.ApplyDamageToNPC(npc, splashDmg, 0f, 0, false);
                }
            }

            SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);

            // === FOUNDATION: RippleEffectProjectile — Orb explosion bell shockwave ===
            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero,
                ModContent.ProjectileType<RippleEffectProjectile>(),
                0, 0f, Projectile.owner, ai0: 1f);

            // === FOUNDATION: SparkExplosionProjectile — Orb detonation spark burst ===
            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero,
                ModContent.ProjectileType<SparkExplosionProjectile>(),
                0, 0f, Projectile.owner,
                ai0: (float)SparkMode.RadialScatter);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            try
            {
            IncisorOrbRenderer.DrawOrbVisuals(Main.spriteBatch, Projectile, IncisorOrbRenderer.LaCampanella, ref _strip);
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

        public override void OnKill(int timeLeft)
        {
            _trailRenderer?.Dispose();

            // Death flash — arcane flash at death point
            FangOfTheInfiniteBellParticleHandler.SpawnParticle(
                new ArcaneFlashParticle(Projectile.Center, 12, IsCrescendo ? 1.0f : 0.7f, false));

            // Death sparks — radial burst
            int sparkCount = IsCrescendo ? 12 : 8;
            for (int i = 0; i < sparkCount; i++)
            {
                Vector2 vel = Main.rand.NextVector2CircularEdge(4f, 4f) * Main.rand.NextFloat(0.4f, 1f);
                FangOfTheInfiniteBellParticleHandler.SpawnParticle(
                    new EmpoweredSparkParticle(Projectile.Center, vel, 15, 0.25f));
            }

            // Musical bell notes scatter on death — the bell echoes
            int noteCount = IsCrescendo ? 5 : 3;
            for (int i = 0; i < noteCount; i++)
            {
                Vector2 noteVel = Main.rand.NextVector2Circular(2f, 2f) + new Vector2(0, -Main.rand.NextFloat(0.5f, 1.5f));
                FangOfTheInfiniteBellParticleHandler.SpawnParticle(
                    new MusicalBellNoteParticle(Projectile.Center + Main.rand.NextVector2Circular(8f, 8f),
                        noteVel, Main.rand.Next(30, 50), Main.rand.NextFloat(0.25f, 0.4f), IsCrescendo));
            }

            // Foundation ripple — bell chime shockwave on death
            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero,
                ModContent.ProjectileType<RippleEffectProjectile>(),
                0, 0f, Projectile.owner, ai0: 1f);
        }
    }
}
