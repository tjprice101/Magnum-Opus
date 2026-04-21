using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Content.DiesIrae;
using MagnumOpus.Content.DiesIrae.Systems;

namespace MagnumOpus.Content.DiesIrae.Weapons.GrimoireOfCondemnation.Projectiles
{
    /// <summary>
    /// Blazing Shard — Grimoire of Condemnation projectile.
    /// Fast-moving orb with themed rendering and trail.
    /// ai[0] = IsPageTurn (0 or 1) — +30% scale/damage
    /// ai[1] = KillBonus multiplier (0-0.5)
    /// </summary>
    public class BlazingShardProjectile : ModProjectile
    {
        private VertexStrip _strip;

        private bool IsPageTurn => Projectile.ai[0] > 0;
        private float KillBonus => Projectile.ai[1];

        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 12;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 2;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();

            // Scale up for Page Turn
            if (IsPageTurn && Projectile.scale < 1.3f)
                Projectile.scale = 1.3f;

            // Mild homing
            NPC target = FindClosestNPC(400f);
            if (target != null)
            {
                Vector2 idealDir = (target.Center - Projectile.Center).SafeNormalize(Projectile.velocity.SafeNormalize(Vector2.UnitX));
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, idealDir * Projectile.velocity.Length(), 0.03f);
            }

            // Dust trail
            int dustChance = IsPageTurn ? 2 : 3;
            if (Main.rand.NextBool(dustChance))
            {
                Color dustCol = IsPageTurn
                    ? Color.Lerp(DiesIraePalette.JudgmentGold, DiesIraePalette.WrathWhite, Main.rand.NextFloat())
                    : Color.Lerp(DiesIraePalette.InfernalRed, DiesIraePalette.EmberOrange, Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(4f, 4f),
                    DustID.Torch, -Projectile.velocity * 0.12f, 0, dustCol, 0.7f + (IsPageTurn ? 0.3f : 0f));
                d.noGravity = true;
            }

            // Page Turn extra sparks
            if (IsPageTurn && Main.rand.NextBool(3))
            {
                Dust spark = Dust.NewDustPerfect(Projectile.Center, DustID.SolarFlare,
                    Main.rand.NextVector2Circular(1.5f, 1.5f), 0, default, 0.6f);
                spark.noGravity = true;
            }

            // Lighting
            float intensity = IsPageTurn ? 0.4f : 0.3f;
            intensity *= (1f + KillBonus);
            Color lightCol = IsPageTurn ? DiesIraePalette.JudgmentGold : DiesIraePalette.InfernalRed;
            Lighting.AddLight(Projectile.Center, lightCol.ToVector3() * intensity);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire3, 90 + (IsPageTurn ? 60 : 0));

            // Track kills for bonus
            if (target.life <= 0)
            {
                Player owner = Main.player[Projectile.owner];
                if (owner.whoAmI == Main.myPlayer)
                {
                    owner.GetModPlayer<DiesIraeCombatPlayer>().AddGrimoireKillBonus();
                }
            }

            // Impact VFX
            int burstCount = IsPageTurn ? 8 : 5;
            for (int i = 0; i < burstCount; i++)
            {
                Color col = IsPageTurn ? DiesIraePalette.JudgmentGold : DiesIraePalette.InfernalRed;
                Dust d = Dust.NewDustPerfect(target.Center, DustID.Torch,
                    Main.rand.NextVector2CircularEdge(4f, 4f), 0, col, 0.8f + (IsPageTurn ? 0.3f : 0f));
                d.noGravity = true;
            }

            if (IsPageTurn)
            {
                DiesIraeVFXLibrary.SpawnWrathBurst(target.Center, 4, 0.5f);
            }
        }

        public override void OnKill(int timeLeft)
        {
            if (Main.dedServ) return;

            int burstCount = IsPageTurn ? 6 : 4;
            for (int i = 0; i < burstCount; i++)
            {
                Color col = Color.Lerp(DiesIraePalette.InfernalRed, DiesIraePalette.EmberOrange, Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.Torch,
                    Main.rand.NextVector2Circular(2.5f, 2.5f), 0, col, 0.6f);
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
                // Use IncisorOrbRenderer for consistent themed rendering
                var config = IncisorOrbRenderer.DiesIrae;
                IncisorOrbRenderer.DrawOrbVisuals(sb, Projectile, config, ref _strip);

                // Additional Page Turn glow
                if (IsPageTurn)
                {
                    Vector2 drawPos = Projectile.Center - Main.screenPosition;
                    sb.End();
                    sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                        Main.DefaultSamplerState, DepthStencilState.None,
                        RasterizerState.CullCounterClockwise, null,
                        Main.GameViewMatrix.TransformationMatrix);

                    Texture2D glow = ModContent.Request<Texture2D>(
                        "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftGlow",
                        ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
                    Vector2 origin = glow.Size() / 2f;

                    float pulse = 0.9f + 0.1f * MathF.Sin((float)Main.timeForVisualEffects * 0.15f);
                    sb.Draw(glow, drawPos, null, (DiesIraePalette.JudgmentGold with { A = 0 }) * 0.35f,
                        Projectile.rotation, origin, 0.25f * Projectile.scale * pulse, SpriteEffects.None, 0f);
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
