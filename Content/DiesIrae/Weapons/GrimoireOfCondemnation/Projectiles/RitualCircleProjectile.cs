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

namespace MagnumOpus.Content.DiesIrae.Weapons.GrimoireOfCondemnation.Projectiles
{
    /// <summary>
    /// Dark Sermon ritual circle — Grimoire of Condemnation right-click.
    /// Expands over 3 seconds then detonates with massive damage + zone.
    /// </summary>
    public class RitualCircleProjectile : ModProjectile
    {
        private const int DetonationTime = 180; // 3 seconds
        private const float MaxRadius = 200f;
        private const float BaseRadius = 40f;

        private float CurrentRadius => BaseRadius + (MaxRadius - BaseRadius) * ((float)(DetonationTime - Projectile.timeLeft) / DetonationTime);
        private float Progress => 1f - (float)Projectile.timeLeft / DetonationTime;

        private float pulseTimer;

        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom";

        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.timeLeft = DetonationTime;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 30; // Tick damage every half second
        }

        public override void AI()
        {
            // Stay in place
            Projectile.velocity = Vector2.Zero;
            pulseTimer += 0.1f;

            // Update hitbox to match radius
            int hitboxSize = (int)(CurrentRadius * 2);
            Projectile.width = hitboxSize;
            Projectile.height = hitboxSize;

            // Ambient dust around edge
            if (Main.rand.NextBool(3))
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 pos = Projectile.Center + angle.ToRotationVector2() * CurrentRadius * Main.rand.NextFloat(0.8f, 1f);
                Color dustCol = Color.Lerp(DiesIraePalette.InfernalRed, DiesIraePalette.CharcoalBlack, Main.rand.NextFloat(0.3f));
                Dust d = Dust.NewDustPerfect(pos, DustID.Torch, -angle.ToRotationVector2() * 0.5f, 0, dustCol, 0.7f);
                d.noGravity = true;
            }

            // Rising embers inside circle
            if (Main.rand.NextBool(4))
            {
                Vector2 pos = Projectile.Center + Main.rand.NextVector2Circular(CurrentRadius * 0.7f, CurrentRadius * 0.7f);
                Color emberCol = Color.Lerp(DiesIraePalette.EmberOrange, DiesIraePalette.JudgmentGold, Main.rand.NextFloat());
                Dust ember = Dust.NewDustPerfect(pos, DustID.Torch, new Vector2(0, -Main.rand.NextFloat(1f, 2.5f)), 0, emberCol, 0.6f);
                ember.noGravity = true;
            }

            // Periodic condemnation sound
            if (Projectile.timeLeft % 60 == 0 && Projectile.timeLeft > 0)
            {
                float pitch = 0.3f * Progress - 0.2f; // Gets higher as it approaches detonation
                SoundEngine.PlaySound(SoundID.Item104 with { Pitch = pitch, Volume = 0.4f + 0.3f * Progress }, Projectile.Center);
            }

            // Lighting
            float intensity = 0.3f + 0.4f * Progress;
            Lighting.AddLight(Projectile.Center, DiesIraePalette.InfernalRed.ToVector3() * intensity);

            // Detonation VFX preparation
            if (Projectile.timeLeft <= 10)
            {
                // Flash before explosion
                for (int i = 0; i < 4; i++)
                {
                    float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                    Vector2 pos = Projectile.Center + angle.ToRotationVector2() * CurrentRadius * 0.5f;
                    Dust flash = Dust.NewDustPerfect(pos, DustID.SolarFlare,
                        angle.ToRotationVector2() * Main.rand.NextFloat(3f, 5f), 0, default, 0.9f);
                    flash.noGravity = true;
                }
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire3, 180);

            // Visual feedback on tick
            for (int i = 0; i < 3; i++)
            {
                Color col = DiesIraePalette.InfernalRed;
                Dust d = Dust.NewDustPerfect(target.Center, DustID.Torch,
                    Main.rand.NextVector2Circular(2f, 2f), 0, col, 0.5f);
                d.noGravity = true;
            }
        }

        public override void OnKill(int timeLeft)
        {
            if (Main.dedServ) return;

            // Massive detonation VFX
            DiesIraeVFXLibrary.FinisherSlam(Projectile.Center, 1.5f);
            SoundEngine.PlaySound(SoundID.Item119 with { Pitch = -0.4f, Volume = 1.0f }, Projectile.Center);

            // Radial fire explosion
            int burstCount = 30;
            for (int i = 0; i < burstCount; i++)
            {
                float angle = MathHelper.TwoPi * i / burstCount;
                float speed = Main.rand.NextFloat(8f, 14f);
                Vector2 vel = angle.ToRotationVector2() * speed;
                Color fireCol = Color.Lerp(DiesIraePalette.InfernalRed, DiesIraePalette.BloodRed, Main.rand.NextFloat());
                Dust fire = Dust.NewDustPerfect(Projectile.Center, DustID.Torch, vel, 30, fireCol, Main.rand.NextFloat(1.4f, 2.2f));
                fire.noGravity = true;
            }

            // Inner golden explosion
            for (int i = 0; i < 20; i++)
            {
                float angle = MathHelper.TwoPi * i / 20;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(5f, 9f);
                Color goldCol = Color.Lerp(DiesIraePalette.JudgmentGold, DiesIraePalette.HellfireGold, Main.rand.NextFloat());
                Dust gold = Dust.NewDustPerfect(Projectile.Center, DustID.GoldFlame, vel, 0, goldCol, Main.rand.NextFloat(1.2f, 1.8f));
                gold.noGravity = true;
            }

            // Solar flare sparks
            for (int i = 0; i < 15; i++)
            {
                Dust spark = Dust.NewDustPerfect(Projectile.Center, DustID.SolarFlare,
                    Main.rand.NextVector2Circular(8f, 8f), 0, default, 1.2f);
                spark.noGravity = true;
            }

            // Spawn damage zone at detonation point
            if (Main.myPlayer == Projectile.owner)
            {
                GenericDamageZone.SpawnZone(
                    Projectile.GetSource_FromThis(),
                    Projectile.Center, Projectile.damage / 3, Projectile.knockBack, Projectile.owner,
                    GenericDamageZone.FLAG_SLOW,
                    MaxRadius * 0.8f, GenericHomingOrbChild.THEME_DIESIRAE,
                    durationFrames: 240);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            try
            {
                Vector2 drawPos = Projectile.Center - Main.screenPosition;
                float pulse = 0.9f + 0.1f * MathF.Sin(pulseTimer * 3f);

                sb.End();
                sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                    Main.DefaultSamplerState, DepthStencilState.None,
                    RasterizerState.CullCounterClockwise, null,
                    Main.GameViewMatrix.TransformationMatrix);

                Texture2D glow = ModContent.Request<Texture2D>(
                    "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftGlow",
                    ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
                Vector2 origin = glow.Size() / 2f;

                float texScale = CurrentRadius / (glow.Width * 0.5f);

                // Outer dark ring
                sb.Draw(glow, drawPos, null, (DiesIraePalette.CharcoalBlack with { A = 0 }) * 0.4f,
                    pulseTimer * 0.3f, origin, texScale * 1.1f * pulse, SpriteEffects.None, 0f);

                // Middle infernal ring
                sb.Draw(glow, drawPos, null, (DiesIraePalette.InfernalRed with { A = 0 }) * (0.3f + 0.2f * Progress),
                    -pulseTimer * 0.5f, origin, texScale * pulse, SpriteEffects.None, 0f);

                // Inner ember glow
                sb.Draw(glow, drawPos, null, (DiesIraePalette.EmberOrange with { A = 0 }) * (0.25f + 0.25f * Progress),
                    pulseTimer * 0.7f, origin, texScale * 0.7f * pulse, SpriteEffects.None, 0f);

                // Core judgment gold (builds as detonation approaches)
                if (Progress > 0.5f)
                {
                    float coreIntensity = (Progress - 0.5f) * 2f; // 0 to 1 in final 1.5 seconds
                    sb.Draw(glow, drawPos, null, (DiesIraePalette.JudgmentGold with { A = 0 }) * 0.4f * coreIntensity,
                        -pulseTimer, origin, texScale * 0.4f * pulse, SpriteEffects.None, 0f);
                }

                // Ritual ring edge (pulsing ring)
                Texture2D point = ModContent.Request<Texture2D>(
                    "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom",
                    ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
                Vector2 pointOrigin = point.Size() / 2f;

                int ringPoints = 24;
                for (int i = 0; i < ringPoints; i++)
                {
                    float angle = MathHelper.TwoPi * i / ringPoints + pulseTimer;
                    Vector2 offset = angle.ToRotationVector2() * CurrentRadius * 0.95f;
                    float pointPulse = 0.8f + 0.2f * MathF.Sin(pulseTimer * 4f + i * 0.3f);
                    Color ringCol = Color.Lerp(DiesIraePalette.BloodRed, DiesIraePalette.InfernalRed, MathF.Sin(pulseTimer + i) * 0.5f + 0.5f);
                    sb.Draw(point, drawPos + offset, null, (ringCol with { A = 0 }) * 0.5f * pointPulse,
                        0f, pointOrigin, 0.06f * pointPulse, SpriteEffects.None, 0f);
                }
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
