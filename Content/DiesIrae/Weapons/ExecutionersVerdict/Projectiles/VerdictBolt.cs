using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.DiesIrae.Weapons.ExecutionersVerdict.Utilities;
using MagnumOpus.Content.DiesIrae.Weapons.ExecutionersVerdict.Particles;
using MagnumOpus.Content.DiesIrae.Weapons.ExecutionersVerdict.Primitives;
using MagnumOpus.Content.DiesIrae.Weapons.ExecutionersVerdict.Buffs;

namespace MagnumOpus.Content.DiesIrae.Weapons.ExecutionersVerdict.Projectiles
{
    /// <summary>
    /// Verdict Bolt — dark flaming bolts launched during Executioner's Verdict swings.
    /// These track enemies, leave heavy smoke trails, and spawn spectral sword strikes on hit.
    /// Phase 1: Spiral outward briefly. Phase 2: Aggressive homing. Phase 3: Detonate.
    /// </summary>
    public class VerdictBolt : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom";

        private enum BoltState { Launch, Home, Detonate }

        private BoltState State
        {
            get => (BoltState)(int)Projectile.ai[0];
            set => Projectile.ai[0] = (float)value;
        }
        private ref float StateTimer => ref Projectile.ai[1];

        private readonly List<Vector2> trailPoints = new List<Vector2>(16);
        private static Asset<Texture2D> bloomTex;
        private static Asset<Texture2D> glowTex;

        public override void SetStaticDefaults() { }

        public override void SetDefaults()
        {
            Projectile.width = 18;
            Projectile.height = 18;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 180;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void AI()
        {
            // Trail tracking
            trailPoints.Add(Projectile.Center);
            if (trailPoints.Count > 14)
                trailPoints.RemoveAt(0);

            StateTimer++;

            switch (State)
            {
                case BoltState.Launch:
                    // Spiral outward for 15 ticks
                    Projectile.velocity = Projectile.velocity.RotatedBy(0.08f * (Projectile.whoAmI % 2 == 0 ? 1 : -1));
                    Projectile.velocity *= 1.01f;

                    if (StateTimer >= 15)
                    {
                        State = BoltState.Home;
                        StateTimer = 0;
                    }
                    break;

                case BoltState.Home:
                    // Aggressive homing toward nearest enemy
                    NPC target = ExecutionersVerdictUtils.ClosestNPCAt(Projectile.Center, 800f);
                    if (target != null)
                    {
                        Vector2 desired = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX) * 14f;
                        Projectile.velocity = Vector2.Lerp(Projectile.velocity, desired, 0.12f);
                    }
                    else
                    {
                        Projectile.velocity *= 0.99f;
                    }
                    break;

                case BoltState.Detonate:
                    Projectile.Kill();
                    return;
            }

            // Rotation
            Projectile.rotation = Projectile.velocity.ToRotation();

            // Spawn particles
            if (Main.rand.NextBool(2))
            {
                VerdictParticleHandler.SpawnParticle(new JudgmentSmokeParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(5f, 5f),
                    -Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(1f, 1f),
                    Main.rand.NextFloat(0.3f, 0.6f), Main.rand.Next(20, 35)));
            }

            if (Main.rand.NextBool(3))
            {
                VerdictParticleHandler.SpawnParticle(new EmberShardParticle(
                    Projectile.Center,
                    -Projectile.velocity * 0.3f + Main.rand.NextVector2Circular(2f, 2f),
                    Main.rand.NextFloat(0.2f, 0.4f), Main.rand.Next(12, 22)));
            }

            // Lighting
            Lighting.AddLight(Projectile.Center, ExecutionersVerdictUtils.BurningCrimson.ToVector3() * 0.6f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<ExecutionBrand>(), 180);
            target.AddBuff(ModContent.BuffType<PyreImmolation>(), 120);

            // Spawn spectral sword strike on hit location
            SpawnSpectralStrike(target.Center);

            // Explosion VFX
            VerdictParticleHandler.SpawnParticle(new ExecutionBloomParticle(target.Center, 1.5f, 20));

            for (int i = 0; i < 8; i++)
            {
                VerdictParticleHandler.SpawnParticle(new EmberShardParticle(
                    target.Center, Main.rand.NextVector2Circular(5f, 5f),
                    Main.rand.NextFloat(0.3f, 0.7f), Main.rand.Next(18, 30)));
            }

            for (int i = 0; i < 3; i++)
            {
                VerdictParticleHandler.SpawnParticle(new JudgmentNoteParticle(
                    target.Center + Main.rand.NextVector2Circular(15f, 15f),
                    new Vector2(Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-3f, -1f)),
                    ExecutionersVerdictUtils.BloodRed,
                    Main.rand.NextFloat(0.5f, 0.8f), Main.rand.Next(35, 50)));
            }

            SoundEngine.PlaySound(SoundID.DD2_BetsyFireballImpact with { Pitch = -0.3f, Volume = 0.7f }, target.Center);
        }

        private void SpawnSpectralStrike(Vector2 pos)
        {
            // Spawn 3 spectral sword projectiles radiating outward
            Player owner = Main.player[Projectile.owner];
            for (int i = 0; i < 3; i++)
            {
                float angle = MathHelper.TwoPi / 3f * i + Main.rand.NextFloat(-0.3f, 0.3f);
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(8f, 12f);

                Projectile.NewProjectile(
                    owner.GetSource_FromThis(),
                    pos,
                    vel,
                    ModContent.ProjectileType<SpectralVerdictSlash>(),
                    Projectile.damage / 2,
                    Projectile.knockBack / 3,
                    Projectile.owner);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            bloomTex ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom");
            glowTex ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom");

            SpriteBatch sb = Main.spriteBatch;

            // Draw trail via primitive renderer
            if (trailPoints.Count >= 3)
            {
                try
                {
                    sb.End();

                    var settings = new VerdictTrailSettings(
                        widthFunc: p => 18f * (float)Math.Sin(p * MathHelper.Pi) * (1f - p * 0.4f),
                        colorFunc: p =>
                        {
                            Color c = ExecutionersVerdictUtils.MulticolorLerp(p,
                                ExecutionersVerdictUtils.AshWhite,
                                ExecutionersVerdictUtils.EmberGlow,
                                ExecutionersVerdictUtils.BurningCrimson,
                                ExecutionersVerdictUtils.DarkCrimson);
                            return c * (1f - p * 0.6f);
                        },
                        smoothing: 3,
                        shaderSetup: () =>
                        {
                            var device = Main.graphics.GraphicsDevice;
                            device.BlendState = BlendState.Additive;
                            device.RasterizerState = RasterizerState.CullNone;
                        });

                    VerdictTrailRenderer.RenderTrail(trailPoints, settings);
                    Main.graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;
                }
                catch { }
                finally
                {
                    sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                        DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
                }
            }

            // Draw bolt core glow
            if (bloomTex.IsLoaded && glowTex.IsLoaded)
            {
                sb.End();
                ExecutionersVerdictUtils.BeginAdditive(sb);

                var bloom = bloomTex.Value;
                var glow = glowTex.Value;
                Vector2 drawPos = Projectile.Center - Main.screenPosition;

                // Outer ember glow
                sb.Draw(bloom, drawPos, null,
                    ExecutionersVerdictUtils.Additive(ExecutionersVerdictUtils.EmberGlow, 0.5f),
                    0f, bloom.Size() / 2f, 0.4f, SpriteEffects.None, 0f);

                // Inner blood-red core
                sb.Draw(glow, drawPos, null,
                    ExecutionersVerdictUtils.Additive(ExecutionersVerdictUtils.BloodRed, 0.7f),
                    Projectile.rotation, glow.Size() / 2f, new Vector2(0.5f, 0.2f), SpriteEffects.None, 0f);

                // Hot white center
                sb.Draw(glow, drawPos, null,
                    ExecutionersVerdictUtils.Additive(ExecutionersVerdictUtils.AshWhite, 0.4f),
                    0f, glow.Size() / 2f, 0.15f, SpriteEffects.None, 0f);

                sb.End();
                ExecutionersVerdictUtils.ResetSpriteBatch(sb);
            }

            return false;
        }
    }

    /// <summary>
    /// Spectral Verdict Slash — phantom sword strikes spawned by VerdictBolt on hit.
    /// Short-lived, passes through enemies, with dark crimson afterimages.
    /// </summary>
    public class SpectralVerdictSlash : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom";

        private static Asset<Texture2D> bloomTex;

        public override void SetStaticDefaults() { }

        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 3;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 40;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
            Projectile.alpha = 100;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
            Projectile.velocity *= 0.97f;
            Projectile.alpha += 3;

            if (Projectile.alpha >= 255)
                Projectile.Kill();

            // Trailing ember sparks
            if (Main.rand.NextBool(3))
            {
                VerdictParticleHandler.SpawnParticle(new EmberShardParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(10f, 10f),
                    -Projectile.velocity * 0.2f,
                    Main.rand.NextFloat(0.2f, 0.4f), Main.rand.Next(10, 20)));
            }

            Lighting.AddLight(Projectile.Center, ExecutionersVerdictUtils.DarkCrimson.ToVector3() * 0.3f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<ExecutionBrand>(), 120);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            bloomTex ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom");
            if (!bloomTex.IsLoaded) return true;

            SpriteBatch sb = Main.spriteBatch;
            sb.End();
            ExecutionersVerdictUtils.BeginAdditive(sb);

            var tex = bloomTex.Value;
            float alpha = 1f - Projectile.alpha / 255f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            // Dark crimson spectral glow
            sb.Draw(tex, drawPos, null,
                ExecutionersVerdictUtils.Additive(ExecutionersVerdictUtils.DarkCrimson, alpha * 0.6f),
                Projectile.rotation, tex.Size() / 2f, new Vector2(0.8f, 0.3f), SpriteEffects.None, 0f);

            // Ash-white edge highlight
            sb.Draw(tex, drawPos, null,
                ExecutionersVerdictUtils.Additive(ExecutionersVerdictUtils.AshWhite, alpha * 0.3f),
                Projectile.rotation, tex.Size() / 2f, new Vector2(0.4f, 0.15f), SpriteEffects.None, 0f);

            sb.End();
            ExecutionersVerdictUtils.ResetSpriteBatch(sb);

            return false;
        }
    }
}
