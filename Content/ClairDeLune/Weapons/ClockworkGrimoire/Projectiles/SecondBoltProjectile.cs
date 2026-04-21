using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Content.ClairDeLune;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.ClairDeLune.Weapons.ClockworkGrimoire.Projectiles
{
    /// <summary>
    /// Clockwork Grimoire Second Bolt — Pendulum-inspired magic bolt.
    /// Arcane-themed pearl-blue projectile with oscillating trajectory.
    /// </summary>
    public class SecondBoltProjectile : ModProjectile
    {
        private float OscillationTimer { get; set; } = 0f;
        private VertexStrip _strip;

        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 16;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 150;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 12;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            OscillationTimer += 0.12f;

            // Gentle sine-wave oscillation (pendulum effect)
            Vector2 perpendicular = new Vector2(-Projectile.velocity.Y, Projectile.velocity.X).SafeNormalize(Vector2.Zero);
            Projectile.Center += perpendicular * MathF.Sin(OscillationTimer) * 0.8f;

            // Arcane dust trail
            if (Main.rand.NextBool(2))
            {
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.WhiteTorch,
                    -Projectile.velocity * 0.1f, 0,
                    Color.Lerp(ClairDeLunePalette.PearlBlue, ClairDeLunePalette.SoftBlue, Main.rand.NextFloat()), 0.8f);
                d.noGravity = true;
            }

            ClairDeLuneVFXLibrary.AddPulsingLight(Projectile.Center, OscillationTimer, 0.5f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // IncisorOrb shader beam trail (captures wavy path via oldPos) + 5-layer bloom head
            IncisorOrbRenderer.DrawOrbVisuals(Main.spriteBatch, Projectile, IncisorOrbRenderer.ClairDeLune, ref _strip);

            // Pendulum ghost: bright dot offset perpendicular showing swing direction at extremes
            float oscPhase = MathF.Sin(OscillationTimer);
            if (MathF.Abs(oscPhase) > 0.25f)
            {
                SpriteBatch sb = Main.spriteBatch;
                try
                {
                    Vector2 drawPos = Projectile.Center - Main.screenPosition;
                    Vector2 perp = new Vector2(-Projectile.velocity.Y, Projectile.velocity.X).SafeNormalize(Vector2.Zero);
                    Vector2 ghostPos = drawPos + perp * oscPhase * 16f;
                    float ghostAlpha = (MathF.Abs(oscPhase) - 0.25f) / 0.75f;

                    sb.End();
                    sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                        Main.DefaultSamplerState, DepthStencilState.None,
                        RasterizerState.CullCounterClockwise, null,
                        Main.GameViewMatrix.TransformationMatrix);

                    Texture2D bloom = ModContent.Request<Texture2D>(
                        "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom",
                        ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
                    Vector2 origin = bloom.Size() / 2f;

                    Color peakColor = oscPhase < 0f ? ClairDeLunePalette.TemporalCrimson : ClairDeLunePalette.PearlWhite;
                    sb.Draw(bloom, ghostPos, null,
                        (peakColor with { A = 0 }) * (0.38f * ghostAlpha), 0f, origin,
                        0.30f, SpriteEffects.None, 0f);
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

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            ClairDeLuneVFXLibrary.ProjectileImpact(target.Center, 0.9f);
        }

        public override void OnKill(int timeLeft)
        {
            if (Main.dedServ) return;
            ClairDeLuneVFXLibrary.SpawnRadialDustBurst(Projectile.Center, 6, 3f);
        }
    }
}
