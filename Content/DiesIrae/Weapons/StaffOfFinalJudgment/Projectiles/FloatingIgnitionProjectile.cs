using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Content.DiesIrae;

namespace MagnumOpus.Content.DiesIrae.Weapons.StaffOfFinalJudgment.Projectiles
{
    /// <summary>
    /// Sentencing Mine — Decelerates to hover, becomes a proximity mine.
    /// Arms after 20 frames, detection range 120px.
    /// On detonate: Spawn 3 homing child orbs (5 during Judgment Storm).
    /// Max 5 active mines.
    /// </summary>
    public class FloatingIgnitionProjectile : ModProjectile
    {
        private const int ArmTime = 20;
        private const float DetectionRange = 120f;
        private const float DecelerationRate = 0.93f;
        private const int MaxActiveMines = 5;
        private const int ChildCount = 3;
        private const int JudgmentStormChildCount = 5;
        private const int JudgmentStormWindow = 60;

        // Track recent detonations globally for Judgment Storm
        private static int recentDetonations = 0;
        private static int lastDetonationTime = 0;

        private int timer = 0;
        private bool armed = false;
        private float hoverBob = 0f;
        private float pulseTimer = 0f;
        // 0 = no nearby enemy; 1 = enemy at detection edge (about to detonate)
        private float _alertFraction = 0f;

        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom";

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 360;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 30;
        }

        public override void AI()
        {
            timer++;
            pulseTimer += 0.08f;

            // Decelerate to hover
            if (Projectile.velocity.Length() > 0.5f)
            {
                Projectile.velocity *= DecelerationRate;
            }
            else
            {
                Projectile.velocity = Vector2.Zero;
            }

            // Hover bob once stationary
            if (Projectile.velocity.Length() < 1f)
            {
                hoverBob += 0.05f;
                Projectile.position.Y += MathF.Sin(hoverBob) * 0.3f;
            }

            // Arm check
            if (timer >= ArmTime && !armed)
            {
                armed = true;
                SoundEngine.PlaySound(SoundID.Item4 with { Pitch = 0.3f, Volume = 0.5f }, Projectile.Center);
            }

            // Proximity detection when armed
            if (armed)
            {
                NPC nearestEnemy = FindClosestNPC(DetectionRange);
                if (nearestEnemy != null)
                {
                    Detonate();
                    return;
                }
            }

            // Proximity alert tracking — how close is the nearest enemy to the detonation range?
            if (armed)
            {
                const float AlertRange = DetectionRange * 3f;
                NPC alertEnemy = FindClosestNPC(AlertRange);
                if (alertEnemy != null)
                {
                    float dist = Vector2.Distance(Projectile.Center, alertEnemy.Center);
                    _alertFraction = MathHelper.Clamp(1f - (dist - DetectionRange) / (AlertRange - DetectionRange), 0f, 1f);

                    // Urgent sparks as enemy gets close
                    int urgentChance = Math.Max(1, (int)(5f - _alertFraction * 4f));
                    if (Main.rand.NextBool(urgentChance))
                    {
                        Dust spark = Dust.NewDustPerfect(
                            Projectile.Center + Main.rand.NextVector2Circular(14f, 14f),
                            DustID.SolarFlare, Main.rand.NextVector2Circular(2f, 2f), 0,
                            Color.Lerp(DiesIraePalette.JudgmentGold, DiesIraePalette.WrathWhite, _alertFraction),
                            0.4f + _alertFraction * 0.5f);
                        spark.noGravity = true;
                    }
                }
                else
                {
                    _alertFraction = MathHelper.Lerp(_alertFraction, 0f, 0.05f);
                }
            }

            // Ambient VFX
            if (Main.rand.NextBool(6))
            {
                Color dustCol = Color.Lerp(DiesIraePalette.InfernalRed, DiesIraePalette.JudgmentGold, Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(
                    Projectile.Center + Main.rand.NextVector2Circular(12f, 12f),
                    DustID.Torch, Vector2.Zero, 0, dustCol, 0.6f);
                d.noGravity = true;
                d.fadeIn = 0.3f;
            }

            // Lighting
            float armPulse = armed ? (0.6f + 0.3f * MathF.Sin(pulseTimer * 3f)) : 0.4f;
            Lighting.AddLight(Projectile.Center, DiesIraePalette.InfernalRed.ToVector3() * armPulse);

            // Rotation
            Projectile.rotation += armed ? 0.08f : 0.03f;
        }

        private void Detonate()
        {
            if (Main.myPlayer != Projectile.owner)
            {
                Projectile.Kill();
                return;
            }

            // Update Judgment Storm tracking
            int currentTime = (int)Main.GameUpdateCount;
            if (currentTime - lastDetonationTime > JudgmentStormWindow)
            {
                recentDetonations = 0;
            }
            recentDetonations++;
            lastDetonationTime = currentTime;

            // Determine child count (Judgment Storm = 3+ detonations in window)
            int children = recentDetonations >= 3 ? JudgmentStormChildCount : ChildCount;

            // Find targets for children
            for (int i = 0; i < children; i++)
            {
                float angle = MathHelper.TwoPi * i / children + Main.rand.NextFloat(-0.2f, 0.2f);
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(8f, 12f);

                NPC target = FindClosestNPC(600f);
                if (target != null)
                {
                    vel = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX) * 10f
                          + Main.rand.NextVector2Circular(2f, 2f);
                }

                GenericHomingOrbChild.SpawnChild(
                    Projectile.GetSource_FromThis(),
                    Projectile.Center, vel,
                    Projectile.damage, Projectile.knockBack, Projectile.owner,
                    homingStrength: 0.08f,
                    behaviorFlags: GenericHomingOrbChild.FLAG_ACCELERATE,
                    themeIndex: GenericHomingOrbChild.THEME_DIESIRAE,
                    scaleMult: 0.9f,
                    timeLeft: 75);
            }

            // Judgment Storm sound
            if (recentDetonations >= 3)
            {
                SoundEngine.PlaySound(SoundID.Item74 with { Pitch = -0.4f, Volume = 0.8f }, Projectile.Center);
            }
            else
            {
                SoundEngine.PlaySound(SoundID.Item62 with { Volume = 0.7f }, Projectile.Center);
            }

            Projectile.Kill();
        }

        public override void OnKill(int timeLeft)
        {
            if (Main.dedServ) return;

            // Detonation VFX
            DiesIraeVFXLibrary.SpawnInfernalExplosion(Projectile.Center, 0.8f);
            DiesIraeVFXLibrary.SpawnGradientHaloRings(Projectile.Center, 3, 0.25f);
            DiesIraeVFXLibrary.SpawnMusicNotes(Projectile.Center, 2, 20f, 0.7f, 1f, 30);

            // Additional fire dust burst
            for (int i = 0; i < 10; i++)
            {
                float angle = MathHelper.TwoPi * i / 10;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 8f);
                Color col = Color.Lerp(DiesIraePalette.InfernalRed, DiesIraePalette.HellfireGold, Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.Torch, vel, 0, col, 1.2f);
                d.noGravity = true;
            }
        }

        private NPC FindClosestNPC(float maxDist)
        {
            NPC closest = null;
            float closestDist = maxDist;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.CanBeChasedBy()) continue;
                float dist = Vector2.Distance(Projectile.Center, npc.Center);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = npc;
                }
            }
            return closest;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            try
            {
                Vector2 drawPos = Projectile.Center - Main.screenPosition;
                float pulse = 0.85f + 0.15f * MathF.Sin(pulseTimer * 2f);
                float armScale = armed ? 1.2f : 1f;

                sb.End();
                sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                    Main.DefaultSamplerState, DepthStencilState.None,
                    RasterizerState.CullCounterClockwise, null,
                    Main.GameViewMatrix.TransformationMatrix);

                Texture2D glow = ModContent.Request<Texture2D>(
                    "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftGlow",
                    ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
                Vector2 origin = glow.Size() / 2f;

                // Detection ring — brightens and pulses faster as enemy approaches
                if (armed)
                {
                    float ringScale = DetectionRange / (glow.Width * 0.5f);
                    float fastPulse = MathF.Sin(pulseTimer * (4f + _alertFraction * 12f));
                    float ringAlpha = 0.15f + 0.55f * _alertFraction + 0.08f * fastPulse;
                    Color ringCol = Color.Lerp(DiesIraePalette.JudgmentGold, DiesIraePalette.WrathWhite, _alertFraction);
                    sb.Draw(glow, drawPos, null, (ringCol with { A = 0 }) * ringAlpha, 0f, origin, ringScale, SpriteEffects.None, 0f);

                    // Second expanding ring at high alert (>40%) — danger indicator
                    if (_alertFraction > 0.4f)
                    {
                        float expandedScale = ringScale * (1f + 0.45f * _alertFraction);
                        float expandAlpha = (_alertFraction - 0.4f) / 0.6f * 0.45f * (0.7f + 0.3f * fastPulse);
                        sb.Draw(glow, drawPos, null, (DiesIraePalette.WrathWhite with { A = 0 }) * expandAlpha,
                            0f, origin, expandedScale, SpriteEffects.None, 0f);
                    }
                }

                // Outer aura
                float outerScale = 0.4f * pulse * armScale;
                sb.Draw(glow, drawPos, null, (DiesIraePalette.BloodRed with { A = 0 }) * 0.4f,
                    Projectile.rotation, origin, outerScale, SpriteEffects.None, 0f);

                // Inner glow
                float innerScale = 0.25f * pulse * armScale;
                sb.Draw(glow, drawPos, null, (DiesIraePalette.InfernalRed with { A = 0 }) * 0.6f,
                    Projectile.rotation * 0.5f, origin, innerScale, SpriteEffects.None, 0f);

                // Hot core
                float coreScale = 0.12f * pulse * armScale;
                Color coreCol = armed ? DiesIraePalette.WrathWhite : DiesIraePalette.JudgmentGold;
                sb.Draw(glow, drawPos, null, (coreCol with { A = 0 }) * 0.8f,
                    -Projectile.rotation, origin, coreScale, SpriteEffects.None, 0f);
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

        public override bool? CanDamage() => false; // Damage comes from children
    }
}
