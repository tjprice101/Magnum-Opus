using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Content.ClairDeLune;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.ClairDeLune.Weapons.OrreryOfDreams.Projectiles
{
    /// <summary>
    /// Orrery of Dreams Dream Sphere — Triple Orbit Engine.
    /// 3 orbs orbit at 60px/120px/180px radii firing children.
    /// Every 12s: all 3 align fire simultaneously (triple convergence).
    /// </summary>
    public class DreamSphereProjectile : ModProjectile
    {
        private int OrbitPhase { get; set; } = 0;
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
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 600;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            if (owner.dead || !owner.active)
            {
                Projectile.Kill();
                return;
            }

            // Hover near owner
            Vector2 idealPos = owner.Center + new Vector2(0, -60);
            Projectile.Center = Vector2.Lerp(Projectile.Center, idealPos, 0.05f);
            Projectile.velocity *= 0.95f;

            // Rotational orbit phase
            OrbitPhase++;

            // Every 720 frames (12 seconds): convergence burst
            if (OrbitPhase % 720 == 0 && Main.myPlayer == Projectile.owner)
            {
                ClairDeLuneVFXLibrary.SpawnLunarSwirl(Projectile.Center, 8, 60f);
                ClairDeLuneVFXLibrary.FinisherSlam(Projectile.Center, 1.5f);
            }

            // Periodic orbital child fire
            if (OrbitPhase % 30 == 0 && Main.myPlayer == Projectile.owner)
            {
                // Fire from orbit rings
                for (int i = 0; i < 3; i++)
                {
                    float radius = 60f + i * 60f;
                    float angle = (OrbitPhase * (0.05f + i * 0.02f)) + i * MathHelper.TwoPi / 3f;
                    Vector2 spawnPos = Projectile.Center + angle.ToRotationVector2() * radius;
                    Vector2 towardTarget = (owner.Center - spawnPos).SafeNormalize(Vector2.UnitX) * (12f + i * 2f);

                    GenericHomingOrbChild.SpawnChild(
                        Projectile.GetSource_FromThis(),
                        spawnPos, towardTarget,
                        (int)(Projectile.damage * 0.6f), Projectile.knockBack * 0.5f, Projectile.owner,
                        homingStrength: 0.06f + i * 0.02f,
                        behaviorFlags: 0,
                        themeIndex: GenericHomingOrbChild.THEME_CLAIRDELUNE,
                        scaleMult: 0.8f);
                }
            }

            ClairDeLuneVFXLibrary.AddMoonbeamLight(Projectile.Center, OrbitPhase * 0.02f, 0.6f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // IncisorOrb shader trail (shows approach path) + 5-layer palette-cycling bloom head
            IncisorOrbRenderer.DrawOrbVisuals(Main.spriteBatch, Projectile, IncisorOrbRenderer.ClairDeLune, ref _strip);

            // Triple orbit ring indicator — visualizes the three active orbit radii
            SpriteBatch sb = Main.spriteBatch;
            try
            {
                Vector2 drawPos = Projectile.Center - Main.screenPosition;
                float t = (float)Main.timeForVisualEffects;
                float pulse = 0.85f + 0.15f * MathF.Sin(t * 0.08f);

                sb.End();
                sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                    Main.DefaultSamplerState, DepthStencilState.None,
                    RasterizerState.CullCounterClockwise, null,
                    Main.GameViewMatrix.TransformationMatrix);

                Texture2D bloom = ModContent.Request<Texture2D>(
                    "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom",
                    ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
                Vector2 origin = bloom.Size() / 2f;

                float[] radii = { 60f, 120f, 180f };
                float[] speeds = { 0.022f, 0.016f, 0.010f };
                Color[] ringColors = {
                    ClairDeLunePalette.PearlBlue,
                    ClairDeLunePalette.SoftBlue,
                    ClairDeLunePalette.MidnightBlue
                };
                float[] dotSizes = { 0.52f, 0.42f, 0.34f };

                for (int ring = 0; ring < 3; ring++)
                {
                    float angle = t * speeds[ring] * 60f + ring * MathHelper.TwoPi / 3f;
                    Vector2 orbPos = drawPos + new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * radii[ring];
                    float ringAlpha = (0.55f - ring * 0.10f) * pulse;
                    sb.Draw(bloom, orbPos, null,
                        (ringColors[ring] with { A = 0 }) * ringAlpha, 0f, origin,
                        dotSizes[ring], SpriteEffects.None, 0f);
                    Vector2 trailPos = drawPos + new Vector2(
                        MathF.Cos(angle - 0.35f), MathF.Sin(angle - 0.35f)) * radii[ring];
                    sb.Draw(bloom, trailPos, null,
                        (ringColors[ring] with { A = 0 }) * (ringAlpha * 0.35f), 0f, origin,
                        dotSizes[ring] * 0.55f, SpriteEffects.None, 0f);
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

        public override void OnKill(int timeLeft)
        {
            if (Main.dedServ) return;
            ClairDeLuneVFXLibrary.SpawnPearlExplosion(Projectile.Center, 3.0f);
        }
    }
}
