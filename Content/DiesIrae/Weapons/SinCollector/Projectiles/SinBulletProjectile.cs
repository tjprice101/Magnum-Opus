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

namespace MagnumOpus.Content.DiesIrae.Weapons.SinCollector.Projectiles
{
    /// <summary>
    /// Standard Sin Collector bullet. Fast, straight, adds sin on hit.
    /// ai[0] = Tier (0=normal, 1=Penance, 2=Absolution, 3=Damnation)
    /// </summary>
    public class SinBulletProjectile : ModProjectile
    {
        private VertexStrip _strip;

        private int Tier => (int)Projectile.ai[0];

        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 10;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 2;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();

            // Configure based on tier
            switch (Tier)
            {
                case 1: // Penance — visibly larger, gold-tinted orbit
                    if (Projectile.scale < 1.2f) Projectile.scale = 1.2f;
                    if (Projectile.penetrate == 1) Projectile.penetrate = 3;
                    ApplyMildHoming(0.04f);
                    break;
                case 2: // Absolution — noticeably large, bright gold ring of sparks
                    if (Projectile.scale < 1.5f) Projectile.scale = 1.5f;
                    if (Projectile.penetrate == 1) Projectile.penetrate = -1;
                    if (Projectile.velocity.Length() < 35f)
                        Projectile.velocity *= 1.03f;
                    break;
                case 3: // Damnation — massive, crimson halo + white core
                    if (Projectile.scale < 1.8f) Projectile.scale = 1.8f;
                    ApplyAggressiveHoming(0.10f);
                    break;
            }

            // Dust trail (intensity scales with tier)
            int dustChance = 4 - Tier;
            if (Main.rand.NextBool(Math.Max(1, dustChance)))
            {
                Color dustCol = GetTierColor();
                Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(4f, 4f),
                    DustID.Torch, -Projectile.velocity * 0.1f, 0, dustCol, 0.6f + Tier * 0.2f);
                d.noGravity = true;
            }

            // Extra sparks for higher tiers
            if (Tier >= 2 && Main.rand.NextBool(3))
            {
                Dust spark = Dust.NewDustPerfect(Projectile.Center, DustID.SolarFlare,
                    Main.rand.NextVector2Circular(1.5f, 1.5f), 0, default, 0.5f);
                spark.noGravity = true;
            }

            // Lighting
            float intensity = 0.25f + Tier * 0.1f;
            Lighting.AddLight(Projectile.Center, GetTierColor().ToVector3() * intensity);
        }

        private void ApplyMildHoming(float strength)
        {
            NPC target = FindClosestNPC(400f);
            if (target != null)
            {
                Vector2 idealDir = (target.Center - Projectile.Center).SafeNormalize(Projectile.velocity.SafeNormalize(Vector2.UnitX));
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, idealDir * Projectile.velocity.Length(), strength);
            }
        }

        private void ApplyAggressiveHoming(float strength)
        {
            NPC target = FindClosestNPC(600f);
            if (target != null)
            {
                Vector2 idealDir = (target.Center - Projectile.Center).SafeNormalize(Projectile.velocity.SafeNormalize(Vector2.UnitX));
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, idealDir * Projectile.velocity.Length(), strength);
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Player owner = Main.player[Projectile.owner];
            if (owner.whoAmI == Main.myPlayer)
            {
                // Add sin on normal hits
                if (Tier == 0)
                {
                    owner.GetModPlayer<DiesIraeCombatPlayer>().AddSin(1);
                }

                // Damnation spawns a zone
                if (Tier == 3)
                {
                    GenericDamageZone.SpawnZone(
                        Projectile.GetSource_FromThis(),
                        target.Center, Projectile.damage / 2, Projectile.knockBack, Projectile.owner,
                        GenericDamageZone.FLAG_SLOW,
                        150f, GenericHomingOrbChild.THEME_DIESIRAE,
                        durationFrames: 180);
                }
            }

            // Impact VFX scales with tier
            int burstCount = 4 + Tier * 2;
            for (int i = 0; i < burstCount; i++)
            {
                Color col = GetTierColor();
                Dust d = Dust.NewDustPerfect(target.Center, DustID.Torch,
                    Main.rand.NextVector2CircularEdge(3f + Tier, 3f + Tier), 0, col, 0.8f + Tier * 0.2f);
                d.noGravity = true;
            }

            // Apply debuff
            target.AddBuff(BuffID.OnFire3, 60 + Tier * 60);
        }

        public override void OnKill(int timeLeft)
        {
            if (Main.dedServ) return;

            int burstCount = 3 + Tier;
            for (int i = 0; i < burstCount; i++)
            {
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.Torch,
                    Main.rand.NextVector2Circular(2f, 2f), 0, GetTierColor(), 0.5f);
                d.noGravity = true;
            }
        }

        private Color GetTierColor()
        {
            return Tier switch
            {
                0 => DiesIraePalette.InfernalRed,
                1 => Color.Lerp(DiesIraePalette.InfernalRed, DiesIraePalette.JudgmentGold, 0.3f),
                2 => DiesIraePalette.JudgmentGold,
                3 => DiesIraePalette.WrathWhite,
                _ => DiesIraePalette.InfernalRed
            };
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
                // Base rendering via IncisorOrbRenderer (trail strip + bloom head)
                var config = IncisorOrbRenderer.DiesIrae;
                IncisorOrbRenderer.DrawOrbVisuals(sb, Projectile, config, ref _strip);

                if (Tier == 0) return false; // T0 uses base renderer only

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

                float t = (float)Main.timeForVisualEffects;
                float pulse = 0.85f + 0.15f * MathF.Sin(t * 0.12f);

                if (Tier == 1)
                {
                    // Penance: gold-tinted aura + 2 orbiting embers — player sees "this bullet is powered"
                    Color goldTint = Color.Lerp(DiesIraePalette.InfernalRed, DiesIraePalette.JudgmentGold, 0.5f);
                    sb.Draw(glow, drawPos, null, (goldTint with { A = 0 }) * 0.35f,
                        0f, origin, 0.20f * Projectile.scale * pulse, SpriteEffects.None, 0f);

                    for (int i = 0; i < 2; i++)
                    {
                        float angle = t * 0.07f + i * MathHelper.Pi;
                        Vector2 orbitPos = drawPos + new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * 8f;
                        sb.Draw(glow, orbitPos, null, (DiesIraePalette.JudgmentGold with { A = 0 }) * 0.7f,
                            0f, origin, 0.07f * Projectile.scale, SpriteEffects.None, 0f);
                    }
                }
                else if (Tier == 2)
                {
                    // Absolution: bright gold aura + 4 fast-spinning sparks — clearly transcending normal
                    sb.Draw(glow, drawPos, null, (DiesIraePalette.JudgmentGold with { A = 0 }) * 0.5f,
                        0f, origin, 0.35f * Projectile.scale * pulse, SpriteEffects.None, 0f);
                    sb.Draw(glow, drawPos, null, (DiesIraePalette.WrathWhite with { A = 0 }) * 0.40f,
                        0f, origin, 0.16f * Projectile.scale * pulse, SpriteEffects.None, 0f);

                    for (int i = 0; i < 4; i++)
                    {
                        float angle = t * 0.12f + i * MathHelper.TwoPi / 4f;
                        Vector2 orbitPos = drawPos + new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * 12f * Projectile.scale;
                        sb.Draw(glow, orbitPos, null, (DiesIraePalette.JudgmentGold with { A = 0 }) * 0.9f,
                            0f, origin, 0.08f * Projectile.scale, SpriteEffects.None, 0f);
                    }
                }
                else if (Tier == 3)
                {
                    // Damnation: crimson halo + white-hot core + 5 high-speed orbiting sparks
                    sb.Draw(glow, drawPos, null, (DiesIraePalette.BloodRed with { A = 0 }) * 0.55f,
                        0f, origin, 0.60f * Projectile.scale * pulse, SpriteEffects.None, 0f);
                    sb.Draw(glow, drawPos, null, (DiesIraePalette.InfernalRed with { A = 0 }) * 0.50f,
                        0f, origin, 0.38f * Projectile.scale * pulse, SpriteEffects.None, 0f);
                    sb.Draw(glow, drawPos, null, (DiesIraePalette.WrathWhite with { A = 0 }) * 0.75f,
                        0f, origin, 0.17f * Projectile.scale, SpriteEffects.None, 0f);

                    for (int i = 0; i < 5; i++)
                    {
                        float angle = t * 0.17f + i * MathHelper.TwoPi / 5f;
                        Vector2 orbitPos = drawPos + new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * 16f * Projectile.scale;
                        sb.Draw(glow, orbitPos, null, (DiesIraePalette.WrathWhite with { A = 0 }) * 0.95f,
                            0f, origin, 0.09f * Projectile.scale, SpriteEffects.None, 0f);
                    }
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
