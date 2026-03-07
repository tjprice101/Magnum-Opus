using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.Fate.Debuffs;
using MagnumOpus.Content.Fate.ResonantWeapons.LightOfTheFuture.Utilities;
using MagnumOpus.Content.Fate.ResonantWeapons.LightOfTheFuture.Particles;
using MagnumOpus.Content.Fate.ResonantWeapons.LightOfTheFuture.Primitives;

namespace MagnumOpus.Content.Fate.ResonantWeapons.LightOfTheFuture.Projectiles
{
    /// <summary>
    /// LightAcceleratingBullet — The primary projectile of Light of the Future.
    /// Starts at shootSpeed 6, accelerates to 40+ over ~0.5s.
    /// Pierces enemies, applies DestinyCollapse debuff.
    /// Trail intensifies with speed: void → violet → cyan → plasma white.
    /// Constellation-line trails connect impact points.
    /// </summary>
    public class LightAcceleratingBullet : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/SandboxLastPrism/Flare/flare_16";

        private float _currentSpeed = 6f;
        private const float MaxSpeed = 42f;
        private const float Acceleration = 1.2f;

        // Trail position cache
        private Vector2[] _trailPositions;
        private const int TrailLength = 20;

        private static Asset<Texture2D> _glowTex;

        // ─── Bloom Textures (Foundation-tier) ─────────────────────
        private static Asset<Texture2D> _pointBloomTex;
        private static Asset<Texture2D> _softRadialBloomTex;
        private static Asset<Texture2D> _starFlareTex;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = TrailLength;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 4;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 8;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();

            // Initialize trail array
            _trailPositions ??= new Vector2[TrailLength];

            // Accelerate — starts slow, ramps up fast
            _currentSpeed = Math.Min(_currentSpeed + Acceleration, MaxSpeed);
            Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.UnitX) * _currentSpeed;

            float speedRatio = MathHelper.Clamp((_currentSpeed - 6f) / (MaxSpeed - 6f), 0f, 1f);

            // === TRAIL VFX — scales with speed ===
            if (!Main.dedServ)
            {
                SpawnTrailParticles(speedRatio);
            }

            // Dynamic light
            Vector3 lightCol = Vector3.Lerp(
                LightUtils.TrailViolet.ToVector3(),
                LightUtils.LaserCyan.ToVector3(),
                speedRatio);
            Lighting.AddLight(Projectile.Center, lightCol * (0.3f + speedRatio * 0.5f));
        }

        private void SpawnTrailParticles(float speedRatio)
        {
            Vector2 awayDir = -Projectile.velocity.SafeNormalize(Vector2.Zero);

            // Primary glow motes — intensity scales with speed
            int moteCount = 1 + (int)(speedRatio * 2f);
            for (int i = 0; i < moteCount; i++)
            {
                Color trailCol = LightUtils.BulletGradient(speedRatio);
                float scale = 0.12f + speedRatio * 0.1f;
                var mote = new LightMote(
                    Projectile.Center + Main.rand.NextVector2Circular(4f, 4f),
                    awayDir * Main.rand.NextFloat(0.5f, 1.5f) + Main.rand.NextVector2Circular(0.5f, 0.5f),
                    trailCol * 0.6f, scale, 14);
                LightParticleHandler.SpawnParticle(mote);
            }

            // Speed line tracers at high speed
            if (speedRatio > 0.3f && Main.rand.NextBool(3))
            {
                Color tracerCol = Color.Lerp(LightUtils.LaserCyan, LightUtils.PlasmaWhite, speedRatio);
                var tracer = new LightTracer(
                    Projectile.Center + Main.rand.NextVector2Circular(3f, 3f),
                    awayDir * (3f + speedRatio * 6f),
                    tracerCol * 0.7f, 0.15f, 8);
                LightParticleHandler.SpawnParticle(tracer);
            }

            // Star sparks at high velocity
            if (speedRatio > 0.5f && Main.rand.NextBool(4))
            {
                Color sparkCol = Main.rand.NextBool(3) ? LightUtils.MuzzleGold : LightUtils.PlasmaWhite;
                var spark = new LightSpark(
                    Projectile.Center + Main.rand.NextVector2Circular(5f, 5f),
                    Main.rand.NextVector2Circular(1f, 1f),
                    sparkCol * 0.5f, 0.12f, 10);
                LightParticleHandler.SpawnParticle(spark);
            }

            // Dust trail
            if (Main.rand.NextBool(2))
            {
                Color dustCol = LightUtils.BulletGradient(speedRatio);
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.BlueTorch,
                    awayDir * Main.rand.NextFloat(1f, 2f), 0, dustCol, 0.8f + speedRatio * 0.4f);
                d.noGravity = true;
            }

            // Smoke wisps at high speed
            if (speedRatio > 0.6f && Main.rand.NextBool(5))
            {
                var smoke = new LightSmoke(
                    Projectile.Center,
                    awayDir * 0.5f + Main.rand.NextVector2Circular(0.3f, 0.3f),
                    LightUtils.DeepViolet * 0.4f, 0.2f, 25);
                LightParticleHandler.SpawnParticle(smoke);
            }
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            // Speed-based damage scaling: faster bullets deal more damage
            float speedRatio = MathHelper.Clamp((_currentSpeed - 6f) / (MaxSpeed - 6f), 0f, 1f);
            if (speedRatio > 0.3f)
            {
                // Up to +100% damage at max speed
                float bonus = speedRatio * 1.0f;
                modifiers.FinalDamage *= 1f + bonus;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Apply DestinyCollapse debuff
            target.AddBuff(ModContent.BuffType<DestinyCollapse>(), 300);

            float speedRatio = MathHelper.Clamp((_currentSpeed - 6f) / (MaxSpeed - 6f), 0f, 1f);

            // Cascade: peak-speed kills spawn 2 child bullets that continue at full speed
            if (target.life <= 0 && speedRatio > 0.8f && Main.myPlayer == Projectile.owner)
            {
                Player owner = Main.player[Projectile.owner];
                var lp = owner.LightOfFuture();
                if (lp.CascadeCooldown <= 0)
                {
                    lp.CascadeChain++;
                    lp.CascadeCooldown = 10; // Brief cooldown to prevent infinite loop
                    float baseAngle = Projectile.velocity.ToRotation();
                    for (int c = 0; c < 2; c++)
                    {
                        float fanAngle = baseAngle + (c == 0 ? -0.15f : 0.15f);
                        Vector2 childVel = fanAngle.ToRotationVector2() * _currentSpeed;
                        int childProj = Projectile.NewProjectile(
                            Projectile.GetSource_FromThis(),
                            target.Center, childVel,
                            ModContent.ProjectileType<LightAcceleratingBullet>(),
                            (int)(Projectile.damage * 0.6f),
                            Projectile.knockBack * 0.5f,
                            Projectile.owner);
                        // Child bullets start at current speed (already fast)
                        if (childProj >= 0 && childProj < Main.maxProjectiles)
                        {
                            Main.projectile[childProj].penetrate = 2; // Fewer pierces
                            Main.projectile[childProj].timeLeft = 90; // Shorter life
                        }
                    }

                    // Cascade VFX
                    if (!Main.dedServ)
                    {
                        LightParticleHandler.SpawnParticle(new LightBloomFlare(
                            target.Center, LightUtils.MuzzleGold, 0.5f, 12));
                    }
                }
            }

            // Impact VFX
            if (!Main.dedServ)
            {
                SpawnImpactVFX(target.Center);
            }

            // Sound
            SoundEngine.PlaySound(SoundID.Item62 with { Pitch = 0.3f, Volume = 0.6f }, target.Center);
        }

        public override void OnKill(int timeLeft)
        {
            if (!Main.dedServ)
            {
                SpawnImpactVFX(Projectile.Center);
            }
            SoundEngine.PlaySound(SoundID.Item14 with { Pitch = 0.5f, Volume = 0.5f }, Projectile.Center);
        }

        private void SpawnImpactVFX(Vector2 pos)
        {
            float speedRatio = MathHelper.Clamp((_currentSpeed - 6f) / (MaxSpeed - 6f), 0f, 1f);

            // NOTE: SpriteBatch bloom draws removed — SpawnImpactVFX is called from
            // OnHitNPC/OnKill (Update phase) where no SpriteBatch is active.
            // Impact visuals are handled by particles and dust below, which are
            // queued and rendered properly during the Draw phase.

            // ═══ ENHANCED PARTICLES ═══
            // Central bloom flares (original, enhanced)
            LightParticleHandler.SpawnParticle(new LightBloomFlare(pos, LightUtils.LaserCyan, 0.7f, 18));
            LightParticleHandler.SpawnParticle(new LightBloomFlare(pos, LightUtils.PlasmaWhite, 0.4f, 14));
            LightParticleHandler.SpawnParticle(new LightBloomFlare(pos, LightUtils.ImpactCrimson, 0.3f, 12));

            // 14 radial spark burst (up from 8)
            for (int i = 0; i < 14; i++)
            {
                float angle = MathHelper.TwoPi * i / 14f + Main.rand.NextFloat(-0.15f, 0.15f);
                Vector2 sparkVel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 8f + speedRatio * 4f);
                Color sparkCol = LightUtils.BulletGradient(speedRatio * ((float)i / 14f * 0.5f + 0.5f));
                LightParticleHandler.SpawnParticle(new LightSpark(pos, sparkVel, sparkCol * 0.8f, 0.2f, 14));
            }

            // 6 directional velocity-aligned sparks
            Vector2 hitDir = Projectile.velocity.SafeNormalize(Vector2.UnitX);
            Vector2 hitPerp = new Vector2(-hitDir.Y, hitDir.X);
            for (int i = 0; i < 6; i++)
            {
                float spread = (i - 2.5f) / 2.5f;
                Vector2 dirVel = (hitDir * 5f + hitPerp * spread * 6f) * Main.rand.NextFloat(0.8f, 1.2f);
                Color col = Color.Lerp(LightUtils.LaserCyan, LightUtils.PlasmaWhite, MathF.Abs(spread));
                LightParticleHandler.SpawnParticle(new LightSpark(pos, dirVel, col * 0.7f, 0.15f, 12));
            }

            // Dust ring (original, enhanced count)
            for (int i = 0; i < 14; i++)
            {
                float angle = MathHelper.TwoPi * i / 14f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 7f);
                Dust d = Dust.NewDustPerfect(pos, DustID.BlueTorch, vel, 0, LightUtils.LaserCyan, 1.2f);
                d.noGravity = true;
            }

            // Glyph accent (original)
            LightParticleHandler.SpawnParticle(new LightGlyph(pos, LightUtils.TrailViolet * 0.6f, 0.25f, 22));

            // Dual lighting
            Lighting.AddLight(pos, LightUtils.LaserCyan.ToVector3() * 1.0f);
            Lighting.AddLight(pos + hitDir * 16f, LightUtils.ImpactCrimson.ToVector3() * 0.6f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;

            try
            {
                // Draw GPU trail (requires SpriteBatch to be ended)
                if (Projectile.oldPos.Length >= 2)
                {
                    float speedRatio = MathHelper.Clamp((_currentSpeed - 6f) / (MaxSpeed - 6f), 0f, 1f);

                    var settings = new LightTrailSettings(
                        width: (progress, idx) =>
                        {
                            float baseWidth = 6f + speedRatio * 10f;
                            return baseWidth * (1f - progress * 0.8f);
                        },
                        color: (progress) =>
                        {
                            Color c = LightUtils.BulletGradient(speedRatio * (1f - progress * 0.5f));
                            float fade = 1f - MathF.Pow(progress, 1.3f);
                            return c * fade * 0.8f;
                        }
                    );

                    // Use oldPos for trail points
                    Vector2[] points = new Vector2[TrailLength];
                    int count = 0;
                    for (int i = 0; i < Projectile.oldPos.Length && i < TrailLength; i++)
                    {
                        if (Projectile.oldPos[i] == Vector2.Zero) break;
                        points[i] = Projectile.oldPos[i] + Projectile.Size / 2f;
                        count++;
                    }

                    if (count >= 2)
                    {
                        sb.End();
                        LightTrailRenderer.RenderTrail(points, settings, count);
                        sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                            DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                    }
                }

                // Draw bullet core sprite
                _glowTex ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/SandboxLastPrism/Orbs/SoftGlow");
                _pointBloomTex ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom");
                _softRadialBloomTex ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom");
                _starFlareTex ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/StarFlare");

                float sr = MathHelper.Clamp((_currentSpeed - 6f) / (MaxSpeed - 6f), 0f, 1f);
                Vector2 drawPos = Projectile.Center - Main.screenPosition;
                float time = (float)Main.timeForVisualEffects;
                float pulse = 1f + MathF.Sin(time * 0.1f) * 0.1f;
                float speedGlow = 1f + sr * 0.5f;

                // ═══ FOUNDATION-TIER GRADUATED BLOOM BODY ═══
                // Switch to additive for bloom layers
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                    DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

                // Layer 1: Outer void-violet haze (SoftRadialBloom) — reduced to avoid dark borders
                if (_softRadialBloomTex?.IsLoaded == true)
                {
                    var radTex = _softRadialBloomTex.Value;
                    var radOrigin = radTex.Size() * 0.5f;
                    sb.Draw(radTex, drawPos, null,
                        LightUtils.Additive(LightUtils.TrailViolet, 0.12f * speedGlow),
                        0f, radOrigin, MathHelper.Min((0.45f + sr * 0.25f) * pulse, 0.139f), SpriteEffects.None, 0f);
                }

                // Layer 2: Trail violet resonance field (SoftRadialBloom)
                if (_softRadialBloomTex?.IsLoaded == true)
                {
                    var radTex = _softRadialBloomTex.Value;
                    var radOrigin = radTex.Size() * 0.5f;
                    sb.Draw(radTex, drawPos, null,
                        LightUtils.Additive(LightUtils.TrailViolet, 0.3f * speedGlow),
                        0f, radOrigin, MathHelper.Min((0.35f + sr * 0.2f) * pulse, 0.139f), SpriteEffects.None, 0f);
                }

                // Layer 3: Cyan laser core (PointBloom)
                if (_pointBloomTex?.IsLoaded == true)
                {
                    var ptTex = _pointBloomTex.Value;
                    var ptOrigin = ptTex.Size() * 0.5f;
                    sb.Draw(ptTex, drawPos, null,
                        LightUtils.Additive(LightUtils.LaserCyan, 0.4f * speedGlow),
                        0f, ptOrigin, MathHelper.Min((0.2f + sr * 0.12f) * pulse, 0.139f), SpriteEffects.None, 0f);
                }

                // Layer 4: Plasma white hot center (PointBloom)
                if (_pointBloomTex?.IsLoaded == true)
                {
                    var ptTex = _pointBloomTex.Value;
                    var ptOrigin = ptTex.Size() * 0.5f;
                    sb.Draw(ptTex, drawPos, null,
                        LightUtils.Additive(LightUtils.PlasmaWhite, 0.5f * speedGlow),
                        0f, ptOrigin, MathHelper.Min((0.1f + sr * 0.08f) * pulse, 0.139f), SpriteEffects.None, 0f);
                }

                // Layer 5: StarFlare at bullet head (visible at higher speeds)
                if (sr > 0.3f && _starFlareTex?.IsLoaded == true)
                {
                    var starTex = _starFlareTex.Value;
                    var starOrigin = starTex.Size() * 0.5f;
                    sb.Draw(starTex, drawPos, null,
                        LightUtils.Additive(LightUtils.LaserCyan, 0.15f * sr),
                        time * 0.05f, starOrigin, (0.12f + sr * 0.1f) * pulse, SpriteEffects.None, 0f);
                    sb.Draw(starTex, drawPos, null,
                        LightUtils.Additive(LightUtils.MuzzleGold, 0.1f * sr),
                        -time * 0.035f, starOrigin, (0.08f + sr * 0.06f) * pulse, SpriteEffects.None, 0f);
                }

                // Original soft glow on top
                if (_glowTex?.IsLoaded == true)
                {
                    Color coreCol = Color.Lerp(LightUtils.LaserCyan, LightUtils.PlasmaWhite, sr) * 0.9f;
                    float coreScale = 0.15f + sr * 0.12f;
                    sb.Draw(_glowTex.Value, drawPos, null, coreCol with { A = 0 },
                        Projectile.rotation, _glowTex.Value.Size() / 2f, coreScale, SpriteEffects.None, 0f);

                    // Outer bloom layer
                    Color bloomCol = LightUtils.TrailViolet * 0.4f;
                    sb.Draw(_glowTex.Value, drawPos, null, bloomCol with { A = 0 },
                        0f, _glowTex.Value.Size() / 2f, MathHelper.Min(coreScale * 2.5f, 0.586f), SpriteEffects.None, 0f);
                }

                // ═══ LEADING-EDGE BLOOM ═══
                Vector2 leadDir = Projectile.velocity.SafeNormalize(Vector2.UnitX);
                Vector2 tipPos = drawPos + leadDir * 8f;
                float leadPulse = 1f + MathF.Sin(time * 0.15f) * 0.12f;

                if (_pointBloomTex?.IsLoaded == true)
                {
                    var ptTex = _pointBloomTex.Value;
                    sb.Draw(ptTex, tipPos, null,
                        LightUtils.Additive(LightUtils.LaserCyan, 0.3f * speedGlow),
                        0f, ptTex.Size() * 0.5f, MathHelper.Min((0.12f + sr * 0.08f) * leadPulse, 0.139f), SpriteEffects.None, 0f);
                    sb.Draw(ptTex, tipPos, null,
                        LightUtils.Additive(LightUtils.PlasmaWhite, 0.4f * speedGlow),
                        0f, ptTex.Size() * 0.5f, (0.06f + sr * 0.04f) * leadPulse, SpriteEffects.None, 0f);
                }

                sb.End();
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                    DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }
            catch
            {
                try
                {
                    sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                        DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                }
                catch { }
            }

            return false;
        }
    }
}
