using System;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems.VFX.Sparkle;
using MagnumOpus.Content.Eroica.Weapons.CelestialValor.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Eroica.Weapons.CelestialValor.Projectiles
{
    /// <summary>
    /// CelestialValorFlyingBlade -- 4-phase state machine projectile.
    /// Phase 0 (Ascend): Fly upward, decelerate.
    /// Phase 1 (Float): Hover above owner with gentle bobbing, await hurl signal.
    /// Phase 2 (Ignite): Scale up with fire dust, then target nearest NPC.
    /// Phase 3 (Hurl): Aggressive homing toward target. On hit: spawn noise zone + sparkle explosion.
    ///
    /// ai[0] = current phase (0-3)
    /// ai[1] = phase timer (counts up each tick)
    /// </summary>
    public class CelestialValorFlyingBlade : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

        // Phase constants
        private const int PhaseAscend = 0;
        private const int PhaseFloat = 1;
        private const int PhaseIgnite = 2;
        private const int PhaseHurl = 3;

        private const int AscendDuration = 30;
        private const int IgniteDuration = 15;

        /// <summary>Base draw scale for the oversized blade sprite (source texture is very large).</summary>
        private const float BladeDrawScale = 0.10f;

        // Cached textures
        private static Asset<Texture2D> _bladeTexture;
        private static Asset<Texture2D> _softGlow;
        private static Asset<Texture2D> _pointBloom;

        private int Phase
        {
            get => (int)Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }

        private float PhaseTimer
        {
            get => Projectile.ai[1];
            set => Projectile.ai[1] = value;
        }

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
            ProjectileID.Sets.DrawScreenCheckFluff[Projectile.type] = 400;
        }

        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.friendly = false; // Only friendly during Phase 3
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 600;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void OnSpawn(IEntitySource source)
        {
            Player owner = Main.player[Projectile.owner];
            if (owner != null && owner.active)
                owner.CelestialValor().HasActiveFlyingBlade = true;
        }

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            if (owner == null || !owner.active || owner.dead)
            {
                Projectile.Kill();
                return;
            }

            PhaseTimer++;

            switch (Phase)
            {
                case PhaseAscend:
                    AIPhaseAscend(owner);
                    break;
                case PhaseFloat:
                    AIPhaseFloat(owner);
                    break;
                case PhaseIgnite:
                    AIPhaseIgnite(owner);
                    break;
                case PhaseHurl:
                    AIPhaseHurl(owner);
                    break;
            }

            // Lighting
            float lightIntensity = Phase >= PhaseIgnite ? 0.8f : 0.4f;
            Lighting.AddLight(Projectile.Center, 0.7f * lightIntensity, 0.3f * lightIntensity, 0.1f * lightIntensity);
        }

        private void AIPhaseAscend(Player owner)
        {
            Projectile.friendly = false;
            Projectile.scale = BladeDrawScale;
            Projectile.velocity *= 0.92f;
            Projectile.velocity.Y -= 0.1f;
            Projectile.rotation += 0.02f;

            if (PhaseTimer >= AscendDuration)
            {
                Phase = PhaseFloat;
                PhaseTimer = 0;
            }
        }

        private void AIPhaseFloat(Player owner)
        {
            Projectile.friendly = false;
            Projectile.scale = BladeDrawScale;
            Vector2 targetPos = owner.MountedCenter + new Vector2(0, -80);
            Projectile.Center = Vector2.Lerp(Projectile.Center, targetPos, 0.1f);

            // Gentle sine bob
            float bob = MathF.Sin((float)Main.timeForVisualEffects * 0.05f) * 3f;
            Projectile.Center += new Vector2(0, bob * 0.1f);
            Projectile.rotation += 0.02f;

            // Check for hurl trigger
            CelestialValorPlayer valorPlayer = owner.CelestialValor();
            if (valorPlayer.TriggerBladeHurl)
            {
                valorPlayer.TriggerBladeHurl = false;
                Phase = PhaseIgnite;
                PhaseTimer = 0;
            }
        }

        private void AIPhaseIgnite(Player owner)
        {
            Projectile.friendly = false;
            float igniteProgress = PhaseTimer / (float)IgniteDuration;
            Projectile.scale = MathHelper.Lerp(BladeDrawScale, BladeDrawScale * 2f, igniteProgress);
            Projectile.rotation += 0.08f;

            // Spawn fire dust
            if (!Main.dedServ)
            {
                int dustCount = Main.rand.Next(4, 7);
                for (int i = 0; i < dustCount; i++)
                {
                    Vector2 dustVel = Main.rand.NextVector2Circular(3f, 3f);
                    Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(20f, 20f),
                        DustID.Torch, dustVel, 0, default, Main.rand.NextFloat(1.0f, 1.8f));
                    d.noGravity = true;
                }
            }

            if (PhaseTimer >= IgniteDuration)
            {
                // Find nearest NPC and launch toward it
                NPC target = FindNearestNPC(800f);
                if (target != null)
                {
                    Vector2 dir = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitY);
                    Projectile.velocity = dir * 30f;
                }
                else
                {
                    // No target: launch in the direction the owner is facing
                    Projectile.velocity = new Vector2(owner.direction * 30f, -5f);
                }

                Projectile.friendly = true;
                Phase = PhaseHurl;
                PhaseTimer = 0;
            }
        }

        private void AIPhaseHurl(Player owner)
        {
            Projectile.friendly = true;

            // Aggressive homing
            NPC target = FindNearestNPC(800f);
            if (target != null)
            {
                Vector2 dirToTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                float targetSpeed = 30f;
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, dirToTarget * targetSpeed, 0.12f);
            }

            // Face velocity direction
            if (Projectile.velocity.LengthSquared() > 1f)
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            // Dust trail
            if (!Main.dedServ && PhaseTimer % 2 == 0)
            {
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.Torch,
                    -Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(1f, 1f),
                    0, default, Main.rand.NextFloat(0.8f, 1.4f));
                d.noGravity = true;
            }

            // Safety: kill if hurl phase lasts too long
            if (PhaseTimer > 180)
                Projectile.Kill();
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Phase != PhaseHurl) return;

            // Spawn CelestialValorNoiseZone at target center
            if (Projectile.owner == Main.myPlayer)
            {
                Projectile.NewProjectile(
                    Projectile.GetSource_OnHit(target, "ValorNoiseZone"),
                    target.Center, Vector2.Zero,
                    ModContent.ProjectileType<CelestialValorNoiseZone>(),
                    Projectile.damage / 3, 0f, Projectile.owner);
            }

            // Spawn sparkle explosion (Eroica theme, 1.2f intensity)
            ThemeSparkleExplosion.Spawn(
                Projectile.GetSource_OnHit(target, "ValorSparkle"),
                target.Center, SparkleTheme.Eroica, 1.2f);

            Projectile.Kill();
        }

        public override void OnKill(int timeLeft)
        {
            Player owner = Main.player[Projectile.owner];
            if (owner != null && owner.active)
            {
                CelestialValorPlayer valorPlayer = owner.CelestialValor();
                valorPlayer.HasActiveFlyingBlade = false;
                valorPlayer.TriggerBladeHurl = false;
            }

            // Death burst VFX
            if (!Main.dedServ)
            {
                for (int i = 0; i < 8; i++)
                {
                    float angle = MathHelper.TwoPi * i / 8f;
                    Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f);
                    Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.GoldFlame, vel, 0,
                        EroicaPalette.Gold, Main.rand.NextFloat(0.6f, 1.2f));
                    d.noGravity = true;
                }
                for (int i = 0; i < 6; i++)
                {
                    float angle = MathHelper.TwoPi * i / 6f + 0.3f;
                    Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 5f);
                    Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.Torch, vel, 0,
                        default, Main.rand.NextFloat(0.8f, 1.4f));
                    d.noGravity = true;
                }
            }
        }

        private NPC FindNearestNPC(float maxRange)
        {
            NPC closest = null;
            float closestDist = maxRange * maxRange;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage || npc.immortal)
                    continue;

                float distSq = Vector2.DistanceSquared(Projectile.Center, npc.Center);
                if (distSq < closestDist)
                {
                    closestDist = distSq;
                    closest = npc;
                }
            }

            return closest;
        }

        // =====================================================================
        // RENDERING
        // =====================================================================

        public override bool PreDraw(ref Color lightColor)
        {
            if (Main.dedServ) return false;

            SpriteBatch sb = Main.spriteBatch;
            try
            {
                LoadTextures();

                Texture2D bladeTex = _bladeTexture?.Value;
                Texture2D softGlow = _softGlow?.Value;
                Texture2D pointBloom = _pointBloom?.Value;
                if (bladeTex == null || softGlow == null || pointBloom == null) return false;

                Vector2 drawPos = Projectile.Center - Main.screenPosition;
                Vector2 bladeOrigin = bladeTex.Size() / 2f;
                Vector2 glowOrigin = softGlow.Size() / 2f;
                Vector2 bloomOrigin = pointBloom.Size() / 2f;

                // -- ADDITIVE PASS --
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                    SamplerState.LinearClamp, DepthStencilState.None,
                    RasterizerState.CullCounterClockwise, null,
                    Main.GameViewMatrix.TransformationMatrix);

                if (Phase >= PhaseIgnite)
                {
                    // Phase 2-3: Fire bloom layers + afterimage trail
                    DrawFireBloom(sb, softGlow, pointBloom, drawPos, glowOrigin, bloomOrigin);
                    DrawAfterimageTrail(sb, softGlow, glowOrigin);
                }
                else
                {
                    // Phase 0-1: Subtle gold underlayer glow
                    sb.Draw(softGlow, drawPos, null,
                        new Color(255, 220, 100) * 0.3f,
                        0f, glowOrigin, 0.15f, SpriteEffects.None, 0f);
                }

                // -- RESTORE TO ALPHA BLEND FOR BLADE SPRITE --
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                    Main.DefaultSamplerState, DepthStencilState.None,
                    Main.Rasterizer, null,
                    Main.GameViewMatrix.TransformationMatrix);

                // Draw the blade sprite
                sb.Draw(bladeTex, drawPos, null, Color.White,
                    Projectile.rotation, bladeOrigin, Projectile.scale, SpriteEffects.None, 0f);
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

        private void DrawFireBloom(SpriteBatch sb, Texture2D softGlow, Texture2D pointBloom,
            Vector2 drawPos, Vector2 glowOrigin, Vector2 bloomOrigin)
        {
            float pulse = 0.9f + 0.1f * MathF.Sin((float)Main.timeForVisualEffects * 0.1f);

            // Outer scarlet glow
            sb.Draw(softGlow, drawPos, null,
                (EroicaPalette.Scarlet with { A = 0 }) * (0.25f * pulse),
                0f, glowOrigin, 0.2f * Projectile.scale, SpriteEffects.None, 0f);

            // Mid flame glow
            sb.Draw(softGlow, drawPos, null,
                (EroicaPalette.Flame with { A = 0 }) * (0.35f * pulse),
                0f, glowOrigin, 0.12f * Projectile.scale, SpriteEffects.None, 0f);

            // White-hot core
            sb.Draw(pointBloom, drawPos, null,
                (Color.White with { A = 0 }) * (0.5f * pulse),
                0f, bloomOrigin, 0.06f * Projectile.scale, SpriteEffects.None, 0f);
        }

        private void DrawAfterimageTrail(SpriteBatch sb, Texture2D softGlow, Vector2 glowOrigin)
        {
            if (Phase != PhaseHurl) return;

            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) break;

                float progress = (float)i / Projectile.oldPos.Length;
                float alpha = (1f - progress) * 0.4f;
                Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;

                Color trailColor = Color.Lerp(EroicaPalette.Gold, EroicaPalette.Scarlet, progress);
                sb.Draw(softGlow, trailPos, null,
                    (trailColor with { A = 0 }) * alpha,
                    0f, glowOrigin, 0.08f * (1f - progress * 0.5f), SpriteEffects.None, 0f);
            }
        }

        private static void LoadTextures()
        {
            _bladeTexture ??= ModContent.Request<Texture2D>(
                "MagnumOpus/Content/Eroica/Weapons/CelestialValor/CelestialValor", AssetRequestMode.ImmediateLoad);
            _softGlow ??= ModContent.Request<Texture2D>(
                "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftGlow", AssetRequestMode.ImmediateLoad);
            _pointBloom ??= ModContent.Request<Texture2D>(
                "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom", AssetRequestMode.ImmediateLoad);
        }
    }
}
