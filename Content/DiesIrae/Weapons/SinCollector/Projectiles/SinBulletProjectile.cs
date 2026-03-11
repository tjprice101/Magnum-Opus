using MagnumOpus.Common;
using MagnumOpus.Common.Systems.VFX.Core;
using MagnumOpus.Content.DiesIrae;
using MagnumOpus.Content.DiesIrae.Weapons.SinCollector.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems;

namespace MagnumOpus.Content.DiesIrae.Weapons.SinCollector.Projectiles
{
    /// <summary>
    /// Sin Bullet ? primary projectile for the Sin Collector.
    /// Fast precision shot with crimson-ember bloom trail.
    /// On hit: collects Sin Fragment + applies fire, with VFX burst.
    /// Trail intensity scales with the player's current sin count.
    /// </summary>
    public class SinBulletProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

        private const int TrailLength = 12;
        private Vector2[] trailPositions = new Vector2[TrailLength];
        private int trailCount;

        private ref float Timer => ref Projectile.ai[1];

        private VertexStrip _vertexStrip;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 16;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 2;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
            Timer++;
            Projectile.rotation = Projectile.velocity.ToRotation();

            // Record trail
            for (int i = TrailLength - 1; i > 0; i--)
                trailPositions[i] = trailPositions[i - 1];
            trailPositions[0] = Projectile.Center;
            if (trailCount < TrailLength) trailCount++;

            // Ember dust every few frames
            if (Main.rand.NextBool(4) && !Main.dedServ)
            {
                Dust d = Dust.NewDustPerfect(
                    Projectile.Center + Main.rand.NextVector2Circular(3, 3),
                    DustID.Torch,
                    Projectile.velocity * -0.05f,
                    0, DiesIraePalette.EmberOrange, 0.6f);
                d.noGravity = true;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire3, 180);

            // Collect sin
            Player owner = Main.player[Projectile.owner];
            var sinPlayer = owner.GetModPlayer<SinCollectorPlayer>();
            sinPlayer.CollectSin(1);

            // Sin fragment wisp VFX
            SinCollectorUtils.SpawnSinFragmentDust(target.Center, owner.Center);

            // Multi-layered impact
            DiesIraeVFXLibrary.MeleeImpact(target.Center, 0);
            SinCollectorUtils.DoBulletImpact(target.Center, Projectile.velocity.SafeNormalize(Vector2.Zero));

            // Music note on sin collection
            DiesIraeVFXLibrary.SpawnMusicNotes(target.Center, 1, 8f, 0.4f, 0.6f, 15);
        }

        public override void OnKill(int timeLeft)
        {
            if (Main.dedServ) return;
            // Small death puff
            for (int i = 0; i < 4; i++)
            {
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.Torch,
                    Main.rand.NextVector2Circular(2, 2), 0, DiesIraePalette.EmberOrange, 0.7f);
                d.noGravity = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            try
            {
                IncisorOrbRenderer.DrawOrbVisuals(sb, Projectile, IncisorOrbRenderer.DiesIrae, ref _vertexStrip);

                // Sin Bullet accent: ember directional streak
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                    SamplerState.LinearClamp, DepthStencilState.None,
                    RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                Vector2 drawPos = Projectile.Center - Main.screenPosition;
                Texture2D glow = MagnumTextureRegistry.GetSoftGlow();
                if (glow != null)
                {
                    Vector2 origin = glow.Size() / 2f;
                    float velRot = Projectile.velocity.ToRotation();

                    // Ember streak along trajectory
                    sb.Draw(glow, drawPos, null,
                        (DiesIraePalette.EmberOrange with { A = 0 }) * 0.22f,
                        velRot, origin, new Vector2(0.1f, 0.02f), SpriteEffects.None, 0f);
                }

                sb.End();
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

    /// <summary>
    /// Penance Shot ? enhanced sin expenditure projectile (Tier 1: 10-19 Sins).
    /// Piercing with stronger bloom and wrathfire.
    /// Also used for Absolution (Tier 2) with a larger explosion on impact.
    /// ai[0] = tier (1, 2, or 3).
    /// </summary>
    public class PenanceShotProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

        private const int TrailLength = 16;
        private Vector2[] trailPositions = new Vector2[TrailLength];
        private int trailCount;

        /// <summary>Expenditure tier 1-3.</summary>
        private int Tier => Math.Max(1, (int)Projectile.ai[0]);
        private ref float Timer => ref Projectile.ai[1];

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 150;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 8;
        }

        public override void AI()
        {
            Timer++;
            Projectile.rotation = Projectile.velocity.ToRotation();

            // Adjust stats based on tier
            if (Timer == 1)
            {
                if (Tier >= 2) Projectile.penetrate = 6;
                if (Tier >= 3) { Projectile.penetrate = -1; Projectile.tileCollide = false; }
            }

            // Trail
            for (int i = TrailLength - 1; i > 0; i--)
                trailPositions[i] = trailPositions[i - 1];
            trailPositions[0] = Projectile.Center;
            if (trailCount < TrailLength) trailCount++;

            // Intense ember dust
            if (!Main.dedServ)
            {
                int dustRate = Tier >= 2 ? 1 : 2;
                if (Main.rand.NextBool(dustRate))
                {
                    Color dustColor = Tier >= 3 ? DiesIraePalette.JudgmentGold : DiesIraePalette.EmberOrange;
                    Dust d = Dust.NewDustPerfect(
                        Projectile.Center + Main.rand.NextVector2Circular(5, 5),
                        DustID.Torch, Projectile.velocity * -0.08f + Main.rand.NextVector2Circular(1.5f, 1.5f),
                        0, dustColor, Tier * 0.4f + 0.6f);
                    d.noGravity = true;
                }
            }

            // Tier 3: light homing
            if (Tier >= 3)
            {
                float homingRange = 600f;
                float homingStr = 0.04f;
                int target = -1;
                float bestDist = homingRange;
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (!npc.active || npc.friendly || npc.dontTakeDamage) continue;
                    float d = Vector2.Distance(Projectile.Center, npc.Center);
                    if (d < bestDist) { bestDist = d; target = i; }
                }
                if (target >= 0)
                {
                    Vector2 desired = (Main.npc[target].Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, desired * Projectile.velocity.Length(), homingStr);
                }
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire3, 300);
            if (Tier >= 2)
                target.AddBuff(BuffID.Ichor, 180); // Armor-pierce for higher tiers

            // Explosion burst on Tier 2+
            if (Tier >= 2 && !Main.dedServ)
            {
                for (int i = 0; i < 15 + Tier * 5; i++)
                {
                    float angle = MathHelper.TwoPi * i / (15 + Tier * 5);
                    float speed = Main.rand.NextFloat(3f, 7f);
                    Vector2 vel = angle.ToRotationVector2() * speed;
                    Dust d = Dust.NewDustPerfect(target.Center, DustID.Torch, vel, 0,
                        Main.rand.NextBool() ? DiesIraePalette.EmberOrange : DiesIraePalette.JudgmentGold,
                        Main.rand.NextFloat(0.8f, 1.4f));
                    d.noGravity = true;
                }
            }

            SinCollectorUtils.DoBulletImpact(target.Center, Projectile.velocity.SafeNormalize(Vector2.Zero));

            // Tier-scaled VFX
            DiesIraeVFXLibrary.MeleeImpact(target.Center, Math.Min(Tier, 2));
            DiesIraeVFXLibrary.SpawnDirectionalSparkleExplosion(
                target.Center, Projectile.velocity.SafeNormalize(Vector2.Zero),
                4 + Tier * 3, 5f, 0.3f, 0.6f);

            // Tier 2+: judgment rings
            if (Tier >= 2)
                DiesIraeVFXLibrary.SpawnJudgmentRings(target.Center, Tier, 0.3f);

            // Tier 3: screen shake + starburst
            if (Tier >= 3)
            {
                MagnumScreenEffects.AddScreenShake(3f);
                DiesIraeVFXLibrary.SpawnHellfireStarburst(target.Center, 1.0f);
            }
        }

        public override void OnKill(int timeLeft)
        {
            if (Main.dedServ) return;
            int count = 6 + Tier * 4;
            for (int i = 0; i < count; i++)
            {
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.Torch,
                    Main.rand.NextVector2Circular(3, 3), 0, DiesIraePalette.EmberOrange, 0.8f + Tier * 0.2f);
                d.noGravity = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            try
            {
            Texture2D glow = MagnumTextureRegistry.GetSoftGlow();
            if (glow == null) return false;
            Vector2 origin = glow.Size() / 2f;

            float scaleMult = 1f + (Tier - 1) * 0.4f;
            float intensMult = 1f + (Tier - 1) * 0.3f;

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);

            // ���� Trail ����
            for (int i = 0; i < trailCount; i++)
            {
                float progress = i / (float)TrailLength;
                float fade = (1f - progress) * 0.85f * intensMult;
                float scale = MathHelper.Lerp(0.035f, 0.01f, progress) * scaleMult;
                Vector2 pos = trailPositions[i] - Main.screenPosition;

                sb.Draw(glow, pos, null, DiesIraePalette.BloodRed * fade * 0.35f,
                    0f, origin, scale * 1.4f, SpriteEffects.None, 0f);
                sb.Draw(glow, pos, null, DiesIraePalette.EmberOrange * fade * 0.55f,
                    0f, origin, scale, SpriteEffects.None, 0f);
                if (Tier >= 2)
                    sb.Draw(glow, pos, null, DiesIraePalette.JudgmentGold * fade * 0.3f,
                        0f, origin, scale * 0.6f, SpriteEffects.None, 0f);
            }

            // ���� Body bloom ����
            SinCollectorUtils.DrawExpendBloom(sb, Projectile.Center, 0.04f * scaleMult, intensMult);


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