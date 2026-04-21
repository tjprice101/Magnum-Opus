using System;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Content.ClairDeLune;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.ClairDeLune.Weapons.MidnightMechanism.Projectiles
{
    /// <summary>
    /// Midnight Mechanism Bullet — Spin-Up Gatling phase-based projectile.
    /// ai[0] = phase (1-5)
    /// ai[1] = isMidnightStrike (1 = true, 0 = false)
    /// </summary>
    public class MechanismBulletProjectile : ModProjectile
    {
        private int Phase => (int)Projectile.ai[0];
        private bool IsMidnightStrike => Projectile.ai[1] > 0.5f;
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
            Projectile.penetrate = 1;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 8;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();

            // Phase 5 with homing behavior
            if (Phase >= 5)
            {
                NPC target = FindClosestNPC(600f);
                if (target != null)
                {
                    Vector2 idealDir = (target.Center - Projectile.Center).SafeNormalize(Projectile.velocity.SafeNormalize(Vector2.UnitX));
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, idealDir * Projectile.velocity.Length(), 0.08f);
                }
            }

            // Dust trail — clockwork-brass colored
            if (Main.rand.NextBool(2))
            {
                Color trailColor = IsMidnightStrike
                    ? Color.Lerp(ClairDeLunePalette.TemporalCrimson, ClairDeLunePalette.WhiteHot, Main.rand.NextFloat())
                    : Color.Lerp(ClairDeLunePalette.ClockworkBrass, ClairDeLunePalette.PearlBlue, Main.rand.NextFloat());

                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.WhiteTorch,
                    -Projectile.velocity * 0.15f, 0, trailColor, IsMidnightStrike ? 1.2f : 0.7f);
                d.noGravity = true;
            }

            // Lighting
            Color lightColor = IsMidnightStrike ? ClairDeLunePalette.TemporalCrimson : ClairDeLunePalette.ClockworkBrass;
            Lighting.AddLight(Projectile.Center, lightColor.ToVector3() * (IsMidnightStrike ? 0.8f : 0.5f));
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // IncisorOrb shader beam trail + 5-layer palette-cycling bloom head
            IncisorOrbRenderer.DrawOrbVisuals(Main.spriteBatch, Projectile, IncisorOrbRenderer.ClairDeLune, ref _strip);

            // Phase/MidnightStrike indicator overlay on top of base bloom
            float phaseProgress = MathHelper.Clamp((Phase - 1) / 4f, 0f, 1f);
            bool needsOverlay = phaseProgress > 0.1f || IsMidnightStrike;
            if (!needsOverlay) return false;

            SpriteBatch sb = Main.spriteBatch;
            try
            {
                Vector2 drawPos = Projectile.Center - Main.screenPosition;
                float t = (float)Main.timeForVisualEffects;

                sb.End();
                sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                    Main.DefaultSamplerState, DepthStencilState.None,
                    RasterizerState.CullCounterClockwise, null,
                    Main.GameViewMatrix.TransformationMatrix);

                Texture2D bloom = ModContent.Request<Texture2D>(
                    "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom",
                    ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
                Vector2 origin = bloom.Size() / 2f;

                if (IsMidnightStrike)
                {
                    // Blazing crimson ring for Midnight Strike
                    float pulse = 0.8f + 0.2f * MathF.Sin(t * 0.20f);
                    sb.Draw(bloom, drawPos, null,
                        (ClairDeLunePalette.TemporalCrimson with { A = 0 }) * 0.55f * pulse, 0f, origin,
                        Projectile.scale * 2.0f * pulse, SpriteEffects.None, 0f);
                }
                else if (Phase >= 5)
                {
                    // Phase 5 homing indicator — pulsing outer ring
                    float ringPulse = 0.5f + 0.5f * MathF.Sin(t * 0.25f);
                    sb.Draw(bloom, drawPos, null,
                        (ClairDeLunePalette.PearlBlue with { A = 0 }) * 0.32f * ringPulse, 0f, origin,
                        Projectile.scale * 2.5f * ringPulse, SpriteEffects.None, 0f);
                }
                else
                {
                    // Phase progress: dim brass ring growing brighter with each phase
                    sb.Draw(bloom, drawPos, null,
                        (ClairDeLunePalette.ClockworkBrass with { A = 0 }) * (0.25f * phaseProgress), 0f, origin,
                        Projectile.scale * (1.4f + phaseProgress * 0.8f), SpriteEffects.None, 0f);
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

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (IsMidnightStrike)
            {
                ClairDeLuneVFXLibrary.FinisherSlam(target.Center, 1.0f);
            }
            else
            {
                ClairDeLuneVFXLibrary.ProjectileImpact(target.Center, 0.8f);
            }
        }

        public override void OnKill(int timeLeft)
        {
            if (Main.dedServ) return;

            if (IsMidnightStrike)
            {
                ClairDeLuneVFXLibrary.SpawnMixedSparkleImpact(Projectile.Center, 2.0f, 8, 8);
            }
            else
            {
                ClairDeLuneVFXLibrary.SpawnRadialDustBurst(Projectile.Center, 4, 3f);
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
    }
}
