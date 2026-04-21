using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Content.DiesIrae;

namespace MagnumOpus.Content.DiesIrae.Weapons.DamnationsCannon.Projectiles
{
    /// <summary>
    /// Ignited Wrath Ball — Damnation's Cannon arcing explosive.
    /// Arcs with gravity, explodes on impact.
    /// Spawns 5 homing shrapnel fragments and a hellfire zone.
    /// </summary>
    public class IgnitedWrathBallProjectile : ModProjectile
    {
        private const float Gravity = 0.12f;
        private const int ShrapnelCount = 5;
        private const int ZoneDuration = 300; // 5 seconds

        private float pulseTimer;
        private float spinRotation;
        private VertexStrip _strip;

        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom";

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
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.scale = 1.3f;
        }

        public override void AI()
        {
            // Gravity
            Projectile.velocity.Y += Gravity;

            pulseTimer += 0.1f;
            spinRotation += 0.08f;
            Projectile.rotation = Projectile.velocity.ToRotation();

            // Fire trail dust
            if (Main.rand.NextBool(2))
            {
                Color fireCol = Color.Lerp(DiesIraePalette.InfernalRed, DiesIraePalette.EmberOrange, Main.rand.NextFloat());
                Dust fire = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(8f, 8f),
                    DustID.Torch, -Projectile.velocity * 0.15f, 0, fireCol, Main.rand.NextFloat(0.9f, 1.4f));
                fire.noGravity = true;
            }

            // Smoke trail
            if (Main.rand.NextBool(3))
            {
                Dust smoke = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(6f, 6f),
                    DustID.Smoke, -Projectile.velocity * 0.08f, 150, DiesIraePalette.CharcoalBlack, 0.8f);
                smoke.noGravity = true;
            }

            // Solar flare sparks
            if (Main.rand.NextBool(4))
            {
                Dust spark = Dust.NewDustPerfect(Projectile.Center, DustID.SolarFlare,
                    Main.rand.NextVector2Circular(2f, 2f), 0, default, 0.6f);
                spark.noGravity = true;
            }

            // Lighting
            float pulse = 0.7f + 0.3f * MathF.Sin(pulseTimer * 2f);
            Lighting.AddLight(Projectile.Center, DiesIraePalette.InfernalRed.ToVector3() * 0.5f * pulse);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Explode(target.Center);
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Explode(Projectile.Center);
            return true;
        }

        public override void OnKill(int timeLeft)
        {
            // Fallback explosion if killed without hitting
            if (timeLeft > 0)
                Explode(Projectile.Center);
        }

        private void Explode(Vector2 position)
        {
            if (Main.dedServ) return;

            // Spawn homing shrapnel fragments
            if (Main.myPlayer == Projectile.owner)
            {
                for (int i = 0; i < ShrapnelCount; i++)
                {
                    float angle = MathHelper.TwoPi * i / ShrapnelCount + Main.rand.NextFloat(-0.2f, 0.2f);
                    Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(10f, 14f);

                    GenericHomingOrbChild.SpawnChild(
                        Projectile.GetSource_FromThis(),
                        position, vel,
                        (int)(Projectile.damage * 0.5f), Projectile.knockBack * 0.5f, Projectile.owner,
                        homingStrength: 0.08f,
                        behaviorFlags: GenericHomingOrbChild.FLAG_ACCELERATE,
                        themeIndex: GenericHomingOrbChild.THEME_DIESIRAE,
                        scaleMult: 0.9f,
                        timeLeft: 90);
                }

                // Spawn hellfire zone
                GenericDamageZone.SpawnZone(
                    Projectile.GetSource_FromThis(),
                    position, Projectile.damage / 4, Projectile.knockBack * 0.3f, Projectile.owner,
                    GenericDamageZone.FLAG_SLOW,
                    100f, GenericHomingOrbChild.THEME_DIESIRAE,
                    durationFrames: ZoneDuration);
            }

            // Explosion VFX
            DiesIraeVFXLibrary.SpawnInfernalExplosion(position, 1.2f);
            SoundEngine.PlaySound(SoundID.Item14 with { Pitch = -0.3f, Volume = 1.0f }, position);

            // Radial fire burst
            for (int i = 0; i < 25; i++)
            {
                float angle = MathHelper.TwoPi * i / 25;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(8f, 14f);
                Color fireCol = Color.Lerp(DiesIraePalette.InfernalRed, DiesIraePalette.EmberOrange, Main.rand.NextFloat());
                Dust fire = Dust.NewDustPerfect(position, DustID.Torch, vel, 0, fireCol, Main.rand.NextFloat(1.4f, 2.2f));
                fire.noGravity = true;
            }

            // Golden judgment sparks
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(6f, 10f);
                Color goldCol = Color.Lerp(DiesIraePalette.JudgmentGold, DiesIraePalette.HellfireGold, Main.rand.NextFloat());
                Dust gold = Dust.NewDustPerfect(position, DustID.GoldFlame, vel, 0, goldCol, Main.rand.NextFloat(1.1f, 1.6f));
                gold.noGravity = true;
            }

            // Solar flare burst
            for (int i = 0; i < 10; i++)
            {
                Dust spark = Dust.NewDustPerfect(position, DustID.SolarFlare,
                    Main.rand.NextVector2Circular(8f, 8f), 0, default, 1.1f);
                spark.noGravity = true;
            }

            // Smoke ring
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 6f);
                Dust smoke = Dust.NewDustPerfect(position, DustID.Smoke, vel, 180, DiesIraePalette.CharcoalBlack, 1.2f);
                smoke.noGravity = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            IncisorOrbRenderer.DrawOrbVisuals(Main.spriteBatch, Projectile, IncisorOrbRenderer.DiesIrae, ref _strip);

            // Detonation warning ring — flashes urgently in final 25 frames
            if (Projectile.timeLeft < 25)
            {
                SpriteBatch sb = Main.spriteBatch;
                try
                {
                    float warnFrac = 1f - Projectile.timeLeft / 25f;
                    float warnPulse = MathF.Sin(pulseTimer * (8f + warnFrac * 20f));
                    float warnScale = (0.7f + 0.3f * warnFrac + 0.1f * warnPulse) * Projectile.scale;
                    float warnAlpha = (0.25f + 0.5f * warnFrac) * (0.7f + 0.3f * warnPulse);

                    sb.End();
                    sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                        Main.DefaultSamplerState, DepthStencilState.None,
                        RasterizerState.CullCounterClockwise, null,
                        Main.GameViewMatrix.TransformationMatrix);

                    Texture2D ring = ModContent.Request<Texture2D>(
                        "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftGlow",
                        ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
                    Vector2 origin = ring.Size() / 2f;
                    Vector2 drawPos = Projectile.Center - Main.screenPosition;

                    sb.Draw(ring, drawPos, null, (DiesIraePalette.WrathWhite with { A = 0 }) * warnAlpha,
                        0f, origin, warnScale, SpriteEffects.None, 0f);
                }
                catch { }
                finally
                {
                    try { sb.End(); } catch { }
                    sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                        DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                }
            }

            return false;
        }
    }
}
