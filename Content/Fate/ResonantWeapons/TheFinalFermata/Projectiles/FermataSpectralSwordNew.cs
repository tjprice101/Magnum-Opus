using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.Fate.Debuffs;
using MagnumOpus.Content.Fate.ResonantWeapons.TheFinalFermata.Utilities;
using MagnumOpus.Content.Fate.ResonantWeapons.TheFinalFermata.Particles;
using MagnumOpus.Content.Fate.ResonantWeapons.TheFinalFermata.Primitives;
using MagnumOpus.Common.Systems;

namespace MagnumOpus.Content.Fate.ResonantWeapons.TheFinalFermata.Projectiles
{
    /// <summary>
    /// FermataSpectralSwordNew — Orbiting spectral sword spawned by The Final Fermata.
    /// Orbits the player in triangle (3) or hexagonal (6) formation for 12 seconds.
    /// Deals contact damage continuously while orbiting.
    /// Every 90 frames, all swords synchronize and slash toward the nearest enemy.
    /// At max 6 swords, damage is multiplied by 1.5x.
    ///
    /// ai[0] = orbit offset angle (radians, fixed at spawn)
    /// ai[1] = unused
    /// </summary>
    public class FermataSpectralSwordNew : ModProjectile
    {
        // Use Coda of Annihilation texture for the spectral sword
        public override string Texture => "MagnumOpus/Content/Fate/ResonantWeapons/CodaOfAnnihilation";

        // === CONSTANTS ===
        private const int TotalLifetime = 720; // 12 seconds at 60fps
        private const float OrbitRadius = 60f;
        private const float OrbitSpeed = 0.035f; // radians per frame
        private const int FadeInFrames = 30;
        private const int FadeOutFrames = 40;

        // === LOCAL STATE ===
        private float _currentOrbitAngle;
        private float _spectralAlpha;
        private FermataTrailRenderer _trail;
        private int _frameCounter;

        // Frame guards for once-per-frame particle update/draw
        private static uint _lastUpdateFrame;
        private static uint _lastDrawFrame;

        /// <summary>The fixed orbit offset angle assigned at spawn.</summary>
        private float OrbitOffset
        {
            get => Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 12;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 50;
            Projectile.height = 50;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = TotalLifetime;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
            Projectile.alpha = 255; // starts invisible, fades in
        }

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            if (!owner.active || owner.dead)
            {
                Projectile.Kill();
                return;
            }

            _frameCounter++;

            // Ensure particle system is initialized and updated once per frame
            FermataParticleHandler.EnsureInitialized();
            if (_lastUpdateFrame != Main.GameUpdateCount)
            {
                _lastUpdateFrame = Main.GameUpdateCount;
                FermataParticleHandler.UpdateAll();
            }

            // Initialize trail on first frame
            if (_trail == null)
            {
                _trail = new FermataTrailRenderer(FermataTrailSettings.SwordOrbitTrail());
                _trail.Reset(Projectile.Center);
                _currentOrbitAngle = OrbitOffset;
            }

            // === FADE IN / OUT ===
            int framesAlive = TotalLifetime - Projectile.timeLeft;
            if (framesAlive < FadeInFrames)
            {
                _spectralAlpha = (float)framesAlive / FadeInFrames;
            }
            else if (Projectile.timeLeft < FadeOutFrames)
            {
                _spectralAlpha = (float)Projectile.timeLeft / FadeOutFrames;
            }
            else
            {
                _spectralAlpha = 1f;
            }
            Projectile.alpha = (int)(255 * (1f - _spectralAlpha));

            // === ORBIT ===
            _currentOrbitAngle += OrbitSpeed;
            float effectiveAngle = _currentOrbitAngle + OrbitOffset;

            // Oscillating radius for visual interest
            float radius = FermataUtils.OscillatingRadius(OrbitRadius, _frameCounter, 4f, 0.04f);

            Vector2 orbitOffset = FermataUtils.AngleToVector(effectiveAngle) * radius;
            Projectile.Center = owner.Center + orbitOffset;
            Projectile.velocity = Vector2.Zero;

            // Sword rotation: point tangentially along orbit path
            Projectile.rotation = effectiveAngle + MathHelper.PiOver2 + MathHelper.PiOver4;

            // Record trail
            if (_frameCounter % 2 == 0)
                _trail.RecordPosition(Projectile.Center, Projectile.rotation);

            // === SYNCHRONIZED SLASH (every 90 frames) ===
            var fermata = owner.Fermata();
            if (fermata.SyncSlashTriggered)
            {
                PerformSyncSlash(owner);
            }

            // === ORBIT VFX ===
            SpawnOrbitVFX(effectiveAngle);

            // Lighting
            float lightPulse = FermataUtils.SinePulse(_frameCounter, 0.06f);
            Lighting.AddLight(Projectile.Center,
                FermataUtils.FermataPurple.ToVector3() * 0.3f * _spectralAlpha * (0.8f + lightPulse * 0.2f));
        }

        private void PerformSyncSlash(Player owner)
        {
            // Find nearest enemy
            NPC target = FindNearestEnemy(900f);
            if (target == null) return;

            // Spawn FermataSlashWave toward the target
            Vector2 toTarget = (target.Center - Projectile.Center);
            if (toTarget == Vector2.Zero) return;
            toTarget.Normalize();
            Vector2 slashVel = toTarget * 18f;

            float dmgMult = owner.Fermata().DamageMultiplier * owner.Fermata().FermataPowerMultiplier;
            int slashDamage = (int)(Projectile.damage * 0.6f * dmgMult);

            Projectile.NewProjectile(
                Projectile.GetSource_FromAI(),
                Projectile.Center, slashVel,
                ModContent.ProjectileType<FermataSlashWave>(),
                slashDamage, Projectile.knockBack * 0.5f,
                Projectile.owner);

            // VFX: sync slash burst
            FermataParticleTypes.SpawnSparkBurst(Projectile.Center, 8, 4f, FermataUtils.FermataCrimson);
            FermataParticleTypes.SpawnBloomFlare(Projectile.Center, FermataUtils.FlashWhite, 0.35f, 10);
            FermataParticleTypes.SpawnTimeShard(Projectile.Center);

            // Sound
            SoundEngine.PlaySound(SoundID.Item71 with { Pitch = 0.3f + Main.rand.NextFloat(0.2f), Volume = 0.6f },
                Projectile.Center);
        }

        private void SpawnOrbitVFX(float orbitAngle)
        {
            if (Main.dedServ) return;

            // Gentle trailing motes
            if (_frameCounter % 3 == 0)
            {
                Color trailCol = FermataUtils.PaletteLerp(Main.rand.NextFloat(0.1f, 0.5f));
                Vector2 trailVel = FermataUtils.AngleToVector(orbitAngle + MathHelper.PiOver2) * 0.5f;
                FermataParticleTypes.SpawnMote(
                    Projectile.Center + Main.rand.NextVector2Circular(4f, 4f),
                    trailCol * _spectralAlpha, 0.15f, 20);
            }

            // Nebula wisps behind the sword
            if (_frameCounter % 5 == 0)
            {
                FermataParticleTypes.SpawnNebulaWisp(
                    Projectile.Center + Main.rand.NextVector2Circular(6f, 6f),
                    FermataUtils.AngleToVector(orbitAngle + MathHelper.Pi) * 0.4f,
                    FermataUtils.PaletteLerp(Main.rand.NextFloat(0f, 0.3f)) * _spectralAlpha);
            }

            // Occasional time shard sparkle
            if (Main.rand.NextBool(8))
            {
                FermataParticleTypes.SpawnTimeShard(
                    Projectile.Center + Main.rand.NextVector2Circular(12f, 12f),
                    Main.rand.NextVector2Circular(0.3f, 0.3f),
                    Color.Lerp(FermataUtils.GhostSilver, FermataUtils.TimeGold, Main.rand.NextFloat()) * _spectralAlpha,
                    0.12f, 25);
            }

            // Glyph pulses
            if (_frameCounter % 20 == 0)
            {
                FermataParticleTypes.SpawnGlyph(
                    Projectile.Center,
                    FermataUtils.FermataPurple * _spectralAlpha, 0.18f, 30);
            }

            // Vanilla dust for extra density
            if (Main.rand.NextBool(4))
            {
                Dust d = Dust.NewDustPerfect(
                    Projectile.Center + Main.rand.NextVector2Circular(6f, 6f),
                    DustID.PurpleTorch,
                    FermataUtils.AngleToVector(orbitAngle + MathHelper.Pi) * 0.5f,
                    0, FermataUtils.FermataPurple * 0.5f * _spectralAlpha, 0.8f);
                d.noGravity = true;
            }
        }

        private NPC FindNearestEnemy(float maxDist)
        {
            NPC closest = null;
            float closestDist = maxDist;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || !npc.CanBeChasedBy()) continue;
                float dist = Vector2.Distance(Projectile.Center, npc.Center);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = npc;
                }
            }
            return closest;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Apply DestinyCollapse debuff
            target.AddBuff(ModContent.BuffType<DestinyCollapse>(), 300);

            // Impact VFX
            FermataParticleTypes.SpawnSparkBurst(target.Center, 6, 3f);
            FermataParticleTypes.SpawnBloomFlare(target.Center, FermataUtils.FermataCrimson, 0.3f, 12);

            if (Main.rand.NextBool(2))
                FermataParticleTypes.SpawnTimeShard(target.Center);
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            Player owner = Main.player[Projectile.owner];
            var fermata = owner.Fermata();

            // Fermata Power: +10% per second held (max 5x at 5s)
            modifiers.FinalDamage *= fermata.FermataPowerMultiplier;

            // Harmonic Alignment bonus: 1.5x at max 6 swords
            if (fermata.AtMaxSwords)
                modifiers.FinalDamage *= 1.5f;

            // Harmonic Alignment convergence bonus: if aligned, extra 20%
            if (fermata.IsHarmonicallyAligned)
                modifiers.FinalDamage *= 1.2f;
        }

        public override void OnKill(int timeLeft)
        {
            // Final dissipation burst
            FermataParticleTypes.SpawnMoteBurst(Projectile.Center, 6, 15f, FermataUtils.FermataPurple * 0.5f);
            FermataParticleTypes.SpawnTimeShard(Projectile.Center);

            for (int i = 0; i < 4; i++)
            {
                Dust d = Dust.NewDustPerfect(
                    Projectile.Center + Main.rand.NextVector2Circular(8f, 8f),
                    DustID.PurpleTorch,
                    Main.rand.NextVector2Circular(2f, 2f), 0,
                    FermataUtils.FermataPurple * 0.6f, 0.9f);
                d.noGravity = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            try
            {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;

            try
            {
                // Draw particles once per frame (guard)
                if (_lastDrawFrame != Main.GameUpdateCount)
                {
                    _lastDrawFrame = Main.GameUpdateCount;
                    FermataParticleHandler.DrawAll(sb);
                }

                // Draw trail (SpriteBatch-based, handles its own additive state)
                _trail?.Draw(sb, _spectralAlpha);

                // === GLOW LAYERS (additive) ===
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null,
                    Main.GameViewMatrix.TransformationMatrix);

                float pulse = 1f + MathF.Sin(Main.GameUpdateCount * 0.12f + OrbitOffset * 3f) * 0.1f;

                // Graduated orb bloom head
                MagnumVFX.DrawGraduatedOrbHead(sb, drawPos, FermataUtils.FermataCrimson, FermataUtils.FermataPurple, 1.0f, _spectralAlpha);

                sb.End();
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null,
                    Main.GameViewMatrix.TransformationMatrix);

                // === MAIN SPECTRAL SWORD ===
                Color spectralColor = Color.White * _spectralAlpha * 0.9f;
                sb.Draw(texture, drawPos, null, spectralColor,
                    Projectile.rotation, origin, 0.95f, SpriteEffects.None, 0f);
            }
            catch
            {
                // Safety: ensure SpriteBatch is restored to Terraria's expected state
                try
                {
                    sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                        DepthStencilState.None, RasterizerState.CullNone, null,
                        Main.GameViewMatrix.TransformationMatrix);
                }
                catch { }
            }

            // Theme accents (additive pass)
            try
            {
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
                FermataUtils.DrawThemeAccents(sb, Projectile.Center, 1f, 0.6f);
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            }
            catch { }

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
