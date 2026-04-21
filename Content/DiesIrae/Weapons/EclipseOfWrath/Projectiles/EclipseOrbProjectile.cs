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

namespace MagnumOpus.Content.DiesIrae.Weapons.EclipseOfWrath.Projectiles
{
    /// <summary>
    /// Eclipse Orb — slow-moving eclipse with dark core and fire corona.
    /// Cursor tracking (homes toward cursor, not enemies).
    /// Splits into 6 wrath shards on hit (12 on crit + fire nova).
    /// On death: Eclipse Field zone (+15% damage taken debuff).
    /// </summary>
    public class EclipseOrbProjectile : ModProjectile
    {
        private const int SplitCount = 6;
        private const int CritSplitCount = 12;
        private const float CursorHomingStrength = 0.04f;

        private float pulseTimer;
        private float coronaRotation;
        private bool wasCrit;
        private VertexStrip _strip;

        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 20;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 28;
            Projectile.height = 28;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 240;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.scale = 1.2f;
        }

        public override void AI()
        {
            pulseTimer += 0.08f;
            coronaRotation += 0.04f;

            // Cursor tracking
            Player owner = Main.player[Projectile.owner];
            if (owner.whoAmI == Main.myPlayer)
            {
                Vector2 toCursor = (Main.MouseWorld - Projectile.Center).SafeNormalize(Projectile.velocity.SafeNormalize(Vector2.UnitX));
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, toCursor * Projectile.velocity.Length(), CursorHomingStrength);
            }

            // Slow acceleration
            if (Projectile.velocity.Length() < 10f)
                Projectile.velocity *= 1.01f;

            Projectile.rotation = Projectile.velocity.ToRotation();

            // Corona fire dust
            if (Main.rand.NextBool(2))
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 offset = angle.ToRotationVector2() * Main.rand.NextFloat(12f, 20f);
                Color fireCol = Color.Lerp(DiesIraePalette.InfernalRed, DiesIraePalette.EmberOrange, Main.rand.NextFloat());
                Dust fire = Dust.NewDustPerfect(Projectile.Center + offset,
                    DustID.Torch, -Projectile.velocity * 0.15f + offset * 0.05f, 0, fireCol, Main.rand.NextFloat(0.8f, 1.3f));
                fire.noGravity = true;
            }

            // Dark core particles (drifting inward)
            if (Main.rand.NextBool(4))
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 offset = angle.ToRotationVector2() * Main.rand.NextFloat(25f, 35f);
                Dust dark = Dust.NewDustPerfect(Projectile.Center + offset,
                    DustID.Wraith, -offset * 0.08f, 0, DiesIraePalette.CharcoalBlack, 0.6f);
                dark.noGravity = true;
            }

            // Solar flare sparks
            if (Main.rand.NextBool(5))
            {
                Dust spark = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(15f, 15f),
                    DustID.SolarFlare, Main.rand.NextVector2Circular(2f, 2f), 0, default, 0.7f);
                spark.noGravity = true;
            }

            // Lighting
            float pulse = 0.7f + 0.3f * MathF.Sin(pulseTimer * 2f);
            Lighting.AddLight(Projectile.Center, DiesIraePalette.InfernalRed.ToVector3() * 0.5f * pulse);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            wasCrit = hit.Crit;
            target.AddBuff(BuffID.OnFire3, 240);

            // Split into wrath shards
            if (Main.myPlayer == Projectile.owner)
            {
                int count = wasCrit ? CritSplitCount : SplitCount;
                for (int i = 0; i < count; i++)
                {
                    float angle = MathHelper.TwoPi * i / count + Main.rand.NextFloat(-0.1f, 0.1f);
                    Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(10f, 14f);

                    GenericHomingOrbChild.SpawnChild(
                        Projectile.GetSource_FromThis(),
                        target.Center, vel,
                        (int)(Projectile.damage * 0.6f), Projectile.knockBack * 0.5f, Projectile.owner,
                        homingStrength: 0.07f,
                        behaviorFlags: GenericHomingOrbChild.FLAG_ACCELERATE,
                        themeIndex: GenericHomingOrbChild.THEME_DIESIRAE,
                        scaleMult: wasCrit ? 1f : 0.85f,
                        timeLeft: 80);
                }
            }

            // Impact VFX
            DiesIraeVFXLibrary.FinisherSlam(target.Center, wasCrit ? 1.2f : 0.8f);

            // Corona Flare on crit - fire nova
            if (wasCrit)
            {
                SoundEngine.PlaySound(SoundID.Item119 with { Pitch = -0.2f, Volume = 0.9f }, target.Center);

                // Massive fire burst
                for (int i = 0; i < 24; i++)
                {
                    float angle = MathHelper.TwoPi * i / 24;
                    Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(8f, 14f);
                    Color fireCol = Color.Lerp(DiesIraePalette.InfernalRed, DiesIraePalette.WrathWhite, Main.rand.NextFloat(0.3f));
                    Dust fire = Dust.NewDustPerfect(target.Center, DustID.Torch, vel, 0, fireCol, Main.rand.NextFloat(1.4f, 2.2f));
                    fire.noGravity = true;
                }

                // Solar flare nova
                for (int i = 0; i < 12; i++)
                {
                    Dust spark = Dust.NewDustPerfect(target.Center, DustID.SolarFlare,
                        Main.rand.NextVector2Circular(8f, 8f), 0, default, 1.2f);
                    spark.noGravity = true;
                }
            }
        }

        public override void OnKill(int timeLeft)
        {
            if (Main.dedServ) return;

            // Spawn Eclipse Field zone (increased damage taken)
            if (Main.myPlayer == Projectile.owner)
            {
                GenericDamageZone.SpawnZone(
                    Projectile.GetSource_FromThis(),
                    Projectile.Center, Projectile.damage / 4, Projectile.knockBack * 0.3f, Projectile.owner,
                    GenericDamageZone.FLAG_SLOW, // Using slow as proxy for "increased damage taken"
                    120f, GenericHomingOrbChild.THEME_DIESIRAE,
                    durationFrames: 300);
            }

            // Death burst VFX
            DiesIraeVFXLibrary.SpawnWrathBurst(Projectile.Center, 8, 0.9f);

            // Radial fire
            for (int i = 0; i < 16; i++)
            {
                float angle = MathHelper.TwoPi * i / 16;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 8f);
                Color fireCol = Color.Lerp(DiesIraePalette.InfernalRed, DiesIraePalette.BloodRed, Main.rand.NextFloat());
                Dust fire = Dust.NewDustPerfect(Projectile.Center, DustID.Torch, vel, 0, fireCol, Main.rand.NextFloat(1f, 1.5f));
                fire.noGravity = true;
            }

            // Dark void collapse
            for (int i = 0; i < 8; i++)
            {
                Dust dark = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(30f, 30f),
                    DustID.Wraith, Vector2.Zero, 0, DiesIraePalette.CharcoalBlack, 0.8f);
                dark.noGravity = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            IncisorOrbRenderer.DrawOrbVisuals(Main.spriteBatch, Projectile, IncisorOrbRenderer.DiesIrae, ref _strip);

            // Eclipse overlays: dark void core + cursor tracking indicator + pre-split telegraph
            SpriteBatch sb = Main.spriteBatch;
            try
            {
                float pulse = 0.9f + 0.1f * MathF.Sin(pulseTimer * 2f);
                Vector2 drawPos = Projectile.Center - Main.screenPosition;

                Texture2D glow = ModContent.Request<Texture2D>(
                    "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftGlow",
                    ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
                Vector2 origin = glow.Size() / 2f;

                // Dark void core (AlphaBlend — occlude like a real eclipse)
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                    Main.DefaultSamplerState, DepthStencilState.None,
                    RasterizerState.CullCounterClockwise, null,
                    Main.GameViewMatrix.TransformationMatrix);
                float coreScale = 0.15f * pulse * Projectile.scale;
                sb.Draw(glow, drawPos, null, DiesIraePalette.CharcoalBlack * 0.8f,
                    -coronaRotation * 0.5f, origin, coreScale, SpriteEffects.None, 0f);

                // Back to additive for cursor indicator + telegraph
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                    Main.DefaultSamplerState, DepthStencilState.None,
                    RasterizerState.CullCounterClockwise, null,
                    Main.GameViewMatrix.TransformationMatrix);

                // Cursor tracking directional indicator
                Vector2 toCursor = Main.MouseWorld - Projectile.Center;
                if (toCursor.Length() > 40f)
                {
                    Vector2 cursorDir = toCursor.SafeNormalize(Vector2.Zero);
                    float indAlpha = 0.30f * pulse;
                    Vector2 arrowPos = drawPos + cursorDir * 28f * Projectile.scale;
                    sb.Draw(glow, arrowPos, null, (DiesIraePalette.JudgmentGold with { A = 0 }) * indAlpha,
                        toCursor.ToRotation(), origin, 0.14f * Projectile.scale, SpriteEffects.None, 0f);
                    sb.Draw(glow, drawPos + cursorDir * 18f * Projectile.scale, null,
                        (DiesIraePalette.EmberOrange with { A = 0 }) * indAlpha * 0.55f,
                        0f, origin, 0.08f * Projectile.scale, SpriteEffects.None, 0f);
                }

                // Pre-split telegraph
                if (Projectile.timeLeft < 60)
                {
                    float telegraphFrac = 1f - Projectile.timeLeft / 60f;
                    float urgentPulse = MathF.Sin(pulseTimer * (5f + telegraphFrac * 15f));
                    float telegraphScale = (0.65f + 0.25f * telegraphFrac + 0.08f * urgentPulse) * Projectile.scale;
                    float telegraphAlpha = (0.20f + 0.45f * telegraphFrac) * (0.8f + 0.2f * urgentPulse);
                    Color telegraphCol = Color.Lerp(DiesIraePalette.JudgmentGold, DiesIraePalette.WrathWhite, telegraphFrac);
                    sb.Draw(glow, drawPos, null, (telegraphCol with { A = 0 }) * telegraphAlpha,
                        -coronaRotation, origin, telegraphScale, SpriteEffects.None, 0f);
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
