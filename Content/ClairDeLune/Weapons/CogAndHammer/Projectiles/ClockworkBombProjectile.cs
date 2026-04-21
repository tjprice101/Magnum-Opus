using System;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Content.ClairDeLune;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.ClairDeLune.Weapons.CogAndHammer.Projectiles
{
    /// <summary>
    /// Cog and Hammer Clockwork Bomb — Ranged launcher projectile.
    /// Explodes on impact creating a stationary damage zone with gravity and knockback.
    /// </summary>
    public class ClockworkBombProjectile : ModProjectile
    {
        private int BounceCount { get; set; } = 0;
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
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;
        }

        public override void AI()
        {
            // Gravity
            Projectile.velocity.Y += 0.12f;
            if (Projectile.velocity.Y > 16f)
                Projectile.velocity.Y = 16f;

            Projectile.rotation = Projectile.velocity.ToRotation();

            // Dust trail
            if (Main.rand.NextBool(3))
            {
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.WhiteTorch,
                    -Projectile.velocity * 0.1f, 0, ClairDeLunePalette.ClockworkBrass, 0.7f);
                d.noGravity = true;
            }

            ClairDeLuneVFXLibrary.AddClairDeLuneLight(Projectile.Center, 0.6f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // IncisorOrb shader trail (naturally follows arc trajectory via oldPos) + 5-layer bloom head
            IncisorOrbRenderer.DrawOrbVisuals(Main.spriteBatch, Projectile, IncisorOrbRenderer.ClairDeLune, ref _strip);

            // Spinning gear danger indicator: brass → crimson after first bounce
            SpriteBatch sb = Main.spriteBatch;
            try
            {
                Vector2 drawPos = Projectile.Center - Main.screenPosition;
                float t = (float)Main.timeForVisualEffects;
                float dangerFrac = BounceCount >= 1 ? 0.75f : 0f;
                Color gearColor = Color.Lerp(ClairDeLunePalette.ClockworkBrass, ClairDeLunePalette.TemporalCrimson, dangerFrac);
                float pulse = 0.82f + 0.18f * MathF.Sin(t * (0.10f + dangerFrac * 0.22f));

                sb.End();
                sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                    Main.DefaultSamplerState, DepthStencilState.None,
                    RasterizerState.CullCounterClockwise, null,
                    Main.GameViewMatrix.TransformationMatrix);

                Texture2D bloom = ModContent.Request<Texture2D>(
                    "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom",
                    ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
                Vector2 origin = bloom.Size() / 2f;

                // 4 spinning gear dots
                float gearSpeed = 0.07f + dangerFrac * 0.10f;
                for (int g = 0; g < 4; g++)
                {
                    float gearAngle = t * gearSpeed * 60f + g * MathHelper.PiOver2;
                    Vector2 gearPos = drawPos + new Vector2(MathF.Cos(gearAngle), MathF.Sin(gearAngle)) * 13f;
                    sb.Draw(bloom, gearPos, null,
                        (gearColor with { A = 0 }) * (0.48f + dangerFrac * 0.30f) * pulse, 0f, origin,
                        0.24f, SpriteEffects.None, 0f);
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

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            BounceCount++;
            if (BounceCount < 2)
            {
                if (System.Math.Abs(Projectile.velocity.X - oldVelocity.X) > float.Epsilon)
                    Projectile.velocity.X = -oldVelocity.X * 0.8f;
                if (System.Math.Abs(Projectile.velocity.Y - oldVelocity.Y) > float.Epsilon)
                    Projectile.velocity.Y = -oldVelocity.Y * 0.8f;
                return false;
            }
            return true;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            ClairDeLuneVFXLibrary.FinisherSlam(target.Center, 1.5f);
            Projectile.Kill();
        }

        public override void OnKill(int timeLeft)
        {
            if (Main.dedServ) return;

            // Explosion at impact
            ClairDeLuneVFXLibrary.SpawnPearlExplosion(Projectile.Center, 2.0f);

            // Spawn damage zone (slow effect, 120px radius, 120 frames)
            if (Main.myPlayer == Projectile.owner)
            {
                GenericDamageZone.SpawnZone(
                    Projectile.GetSource_FromThis(),
                    Projectile.Center, Projectile.damage, Projectile.knockBack, Projectile.owner,
                    GenericDamageZone.FLAG_SLOW, 120f, 8, durationFrames: 120);
            }
        }
    }
}
