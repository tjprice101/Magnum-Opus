using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Graphics.Shaders;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.PiercingBellsResonance.Utilities;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.PiercingBellsResonance.Particles;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.PiercingBellsResonance.Shaders;
using MagnumOpus.Content.LaCampanella.Debuffs;
using MagnumOpus.Content.FoundationWeapons.ImpactFoundation;
using MagnumOpus.Common.Systems.VFX;
using ReLogic.Content;

namespace MagnumOpus.Content.LaCampanella.ResonantWeapons.PiercingBellsResonance.Projectiles
{
    /// <summary>
    /// Seeking crystal sub-projectile fired every 4th shot.
    /// Homes toward enemies that have Resonant Markers. Applies markers on hit.
    /// </summary>
    public class SeekingCrystalProj : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles Asset Library/Stars/4PointedStarSoft";

        private const float HomingRange = 600f;
        private const float HomingStrength = 0.08f;
        private float crystalRotation;
        private VertexStrip _strip;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 16;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 150;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
        }

        public override void AI()
        {
            crystalRotation += 0.12f;

            // Aggressive homing
            int target = FindClosestEnemy();
            if (target >= 0)
            {
                NPC npc = Main.npc[target];
                Vector2 toTarget = Vector2.Normalize(npc.Center - Projectile.Center);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * 14f, HomingStrength);
            }
            else
            {
                // Drift if no target
                Projectile.velocity *= 0.98f;
            }

            Projectile.rotation = Projectile.velocity.ToRotation();

            // Crystal ember trail
            if (Main.rand.NextBool(3))
            {
                PiercingBellsParticleHandler.SpawnParticle(new BulletTracerParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(5, 5),
                    -Projectile.velocity * 0.04f + Main.rand.NextVector2Circular(0.8f, 0.8f),
                    Main.rand.Next(10, 20)));
            }

            Lighting.AddLight(Projectile.Center, PiercingBellsResonanceUtils.CrystalPalette[1].ToVector3() * 0.5f);
        }

        private int FindClosestEnemy()
        {
            // Prioritize enemies with Resonant Markers
            int closestMarked = -1;
            float closestMarkedDist = HomingRange;
            int closestAny = -1;
            float closestAnyDist = HomingRange;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.CanBeChasedBy()) continue;
                float dist = Vector2.Distance(Projectile.Center, npc.Center);
                if (dist >= HomingRange) continue;

                var markers = npc.GetGlobalNPC<ResonantMarkerNPC>();
                if (markers.MarkerCount > 0 && dist < closestMarkedDist)
                {
                    closestMarkedDist = dist;
                    closestMarked = i;
                }
                if (dist < closestAnyDist)
                {
                    closestAnyDist = dist;
                    closestAny = i;
                }
            }
            return closestMarked >= 0 ? closestMarked : closestAny;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.GetGlobalNPC<ResonantTollNPC>().AddStacks(target, 1);
            target.GetGlobalNPC<ResonantMarkerNPC>().AddMarker(target);

            // === FOUNDATION: RippleEffectProjectile — Crystal impact ring ===
            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(), target.Center, Vector2.Zero,
                ModContent.ProjectileType<RippleEffectProjectile>(),
                0, 0f, Projectile.owner, ai0: 1f);
        }

        public override void OnKill(int timeLeft)
        {
            // Crystal shatter
            for (int i = 0; i < 6; i++)
            {
                PiercingBellsParticleHandler.SpawnParticle(new BulletTracerParticle(
                    Projectile.Center, Main.rand.NextVector2Circular(4f, 4f),
                    Main.rand.Next(10, 20)));
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            try
            {
            IncisorOrbRenderer.DrawOrbVisuals(Main.spriteBatch, Projectile, IncisorOrbRenderer.LaCampanella, ref _strip);
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
