using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Content.OdeToJoy;
using MagnumOpus.Content.OdeToJoy.Systems;

namespace MagnumOpus.Content.OdeToJoy.Weapons.ElysianVerdict.Projectiles
{
    /// <summary>
    /// Elysian Verdict magic orb — applies Elysian Mark on hit, tracks tier progression.
    /// At 25% HP threshold: Paradise Lost mode activates (2x scale, aggressive homing).
    /// </summary>
    public class ElysianVerdictSwing : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom";

        private int _marksApplied = 0;
        private bool _paradiseLost = false;
        private VertexStrip _strip;

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
            Projectile.penetrate = 1;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 12;
        }

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];

            // Check if owner is below 25% HP for Paradise Lost mode
            if (owner.statLife < owner.statLifeMax * 0.25f && !_paradiseLost)
            {
                _paradiseLost = true;
                Projectile.scale = 2f;
            }

            Projectile.rotation = Projectile.velocity.ToRotation();

            // Gentle homing (0.06 base, 0.14 in Paradise Lost)
            float homingStrength = _paradiseLost ? 0.14f : 0.06f;
            NPC target = FindClosestNPC(600f);
            if (target != null)
            {
                Vector2 idealDir = (target.Center - Projectile.Center).SafeNormalize(Projectile.velocity.SafeNormalize(Vector2.UnitX));
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, idealDir * Projectile.velocity.Length(), homingStrength);
            }

            // VFX trail
            if (Main.rand.NextBool(3))
            {
                Color col = Color.Lerp(OdeToJoyPalette.LeafGreen, OdeToJoyPalette.GoldenPollen, 0.5f);
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.GreenTorch,
                    -Projectile.velocity * 0.1f, 0, col, 0.7f);
                d.noGravity = true;
            }

            // Bloom rendering
            OdeToJoyVFXLibrary.AddOdeToJoyLight(Projectile.Center, 0.6f * Projectile.scale);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Player owner = Main.player[Projectile.owner];
            var globalNPC = target.GetGlobalNPC<OdeToJoyGlobalNPC>();

            // Apply mark(s)
            int tierToAdd = _paradiseLost ? 2 : 1;
            globalNPC.AddElysianMark(owner.whoAmI, tierToAdd);
            _marksApplied++;

            // Impact VFX
            OdeToJoyVFXLibrary.SpawnMixedSparkleImpact(target.Center, 0.8f);

            // Check if tier 3 for detonation
            if (globalNPC.ElysianMarkTier >= 3)
            {
                int tier3Count = globalNPC.ConsumeMarks();

                // Detonate all tier 3 marks as AoE
                var source = Projectile.GetSource_FromThis();
                int aoeProj = Terraria.Projectile.NewProjectile(source, target.Center,
                    Vector2.Zero, ModContent.ProjectileType<ElysianDetonationZone>(),
                    hit.Damage, hit.Knockback, owner.whoAmI);

                if (aoeProj >= 0 && aoeProj < Main.maxProjectiles)
                {
                    Main.projectile[aoeProj].scale = 1.5f;
                }

                // Detonation VFX
                OdeToJoyVFXLibrary.SpawnGardenExplosion(target.Center, 1.2f);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            IncisorOrbRenderer.DrawOrbVisuals(Main.spriteBatch, Projectile, IncisorOrbRenderer.OdeToJoy, ref _strip);

            // Paradise Lost mode: golden outer ring to signal empowerment
            if (_paradiseLost)
            {
                SpriteBatch sb = Main.spriteBatch;
                try
                {
                    Vector2 drawPos = Projectile.Center - Main.screenPosition;
                    float pulse = 0.7f + 0.3f * MathF.Sin(Main.GlobalTimeWrappedHourly * 6f);

                    sb.End();
                    sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                        Main.DefaultSamplerState, DepthStencilState.None,
                        RasterizerState.CullCounterClockwise, null,
                        Main.GameViewMatrix.TransformationMatrix);

                    Texture2D glow = ModContent.Request<Texture2D>(
                        "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftGlow",
                        ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
                    Vector2 origin = glow.Size() / 2f;

                    sb.Draw(glow, drawPos, null, (OdeToJoyPalette.GoldenPollen with { A = 0 }) * 0.55f * pulse,
                        0f, origin, 0.55f, SpriteEffects.None, 0f);
                    sb.Draw(glow, drawPos, null, (OdeToJoyPalette.WhiteBloom with { A = 0 }) * 0.40f * pulse,
                        0f, origin, 0.20f, SpriteEffects.None, 0f);
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

        public override void OnKill(int timeLeft)
        {
            OdeToJoyVFXLibrary.ProjectileImpact(Projectile.Center, Projectile.scale);
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

    /// <summary>
    /// Detonation zone for Tier 3 Elysian Mark explosion.
    /// </summary>
    public class ElysianDetonationZone : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom";

        public override void SetDefaults()
        {
            Projectile.width = 150;
            Projectile.height = 150;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 12;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void AI()
        {
            Projectile.scale = System.Math.Min(Projectile.scale + 0.2f, 2f);
            OdeToJoyVFXLibrary.AddOdeToJoyLight(Projectile.Center, 0.8f * Projectile.scale);
        }

        public override void OnKill(int timeLeft)
        {
            OdeToJoyVFXLibrary.SpawnGardenExplosion(Projectile.Center, 1.5f);
        }
    }
}
