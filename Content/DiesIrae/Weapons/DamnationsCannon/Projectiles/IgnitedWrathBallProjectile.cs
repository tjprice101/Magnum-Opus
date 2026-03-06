using MagnumOpus.Common.Systems.VFX.Core;
using MagnumOpus.Content.DiesIrae;
using MagnumOpus.Content.DiesIrae.Weapons.DamnationsCannon.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.VFX;

namespace MagnumOpus.Content.DiesIrae.Weapons.DamnationsCannon.Projectiles
{
    /// <summary>
    /// Ignited Wrath Ball ? arcing mortar shot with massive fire orb rendering.
    /// Gravity-affected lobbed projectile. On impact: explosion + hellfire zone + shrapnel.
    /// Three-layer bloom body with dense ember trail.
    /// </summary>
    public class IgnitedWrathBallProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

        private const int TrailLength = 20;
        private Vector2[] trailPositions = new Vector2[TrailLength];
        private int trailCount;

        private ref float Timer => ref Projectile.ai[0];

        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
            Timer++;

            // Gravity for mortar arc
            Projectile.velocity.Y += 0.18f;
            if (Projectile.velocity.Y > 16f) Projectile.velocity.Y = 16f;

            Projectile.rotation = Projectile.velocity.ToRotation();

            // Record trail
            for (int i = TrailLength - 1; i > 0; i--)
                trailPositions[i] = trailPositions[i - 1];
            trailPositions[0] = Projectile.Center;
            if (trailCount < TrailLength) trailCount++;

            // Dense fire dust ? the ball IS fire
            if (!Main.dedServ)
            {
                for (int d = 0; d < 2; d++)
                {
                    Dust dust = Dust.NewDustPerfect(
                        Projectile.Center + Main.rand.NextVector2Circular(10, 10),
                        DustID.Torch,
                        Projectile.velocity * -0.15f + Main.rand.NextVector2Circular(2f, 2f),
                        0, Main.rand.NextBool() ? DiesIraePalette.EmberOrange : DamnationsCannonUtils.WrathRed,
                        Main.rand.NextFloat(1.0f, 1.5f));
                    dust.noGravity = true;
                }

                // Smoke wisps
                if (Main.rand.NextBool(3))
                {
                    Dust s = Dust.NewDustPerfect(
                        Projectile.Center + Main.rand.NextVector2Circular(12, 12),
                        DustID.Smoke, Projectile.velocity * -0.1f, 120,
                        new Color(40, 30, 30), 1.4f);
                    s.noGravity = true;
                }
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire3, 300);
            Explode();
        }

        public override void OnKill(int timeLeft)
        {
            Explode();
        }

        private bool hasExploded;
        private void Explode()
        {
            if (hasExploded) return;
            hasExploded = true;

            // Massive explosion VFX
            DamnationsCannonUtils.DoExplosion(Projectile.Center, 50);

            // Sound
            SoundEngine.PlaySound(SoundID.Item14 with { Volume = 1.3f, Pitch = -0.4f }, Projectile.Center);

            // Spawn hellfire zone
            if (Projectile.owner == Main.myPlayer)
            {
                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    Projectile.Center, Vector2.Zero,
                    ModContent.ProjectileType<HellfireZoneProjectile>(),
                    (int)(Projectile.damage * 0.3f), 0f,
                    Projectile.owner);
            }

            // Spawn shrapnel (5 pieces)
            if (Projectile.owner == Main.myPlayer)
            {
                for (int i = 0; i < 5; i++)
                {
                    float angle = MathHelper.TwoPi * i / 5f + Main.rand.NextFloat(-0.3f, 0.3f);
                    Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(6f, 10f);
                    vel.Y -= 4f; // upward bias

                    Projectile.NewProjectile(
                        Projectile.GetSource_FromThis(),
                        Projectile.Center, vel,
                        ModContent.ProjectileType<WrathShrapnelProjectile>(),
                        (int)(Projectile.damage * 0.25f), 3f,
                        Projectile.owner);
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D glow = MagnumTextureRegistry.GetSoftGlow();
            if (glow == null) return false;
            Vector2 origin = glow.Size() / 2f;

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);

            // ���� Trail ����
            for (int i = 0; i < trailCount; i++)
            {
                float progress = i / (float)TrailLength;
                float fade = (1f - progress) * 0.75f;
                float scale = MathHelper.Lerp(0.06f, 0.015f, progress);
                Vector2 pos = trailPositions[i] - Main.screenPosition;

                sb.Draw(glow, pos, null, DamnationsCannonUtils.WrathRed * fade * 0.3f,
                    0f, origin, scale * 1.3f, SpriteEffects.None, 0f);
                sb.Draw(glow, pos, null, DiesIraePalette.EmberOrange * fade * 0.5f,
                    0f, origin, scale, SpriteEffects.None, 0f);
                sb.Draw(glow, pos, null, DiesIraePalette.CharcoalBlack * fade * 0.2f,
                    0f, origin, scale * 1.5f, SpriteEffects.None, 0f);
            }

            // ���� Wrath ball body ����
            DamnationsCannonUtils.DrawWrathBallBody(sb, Projectile.Center, Timer);

            // Dies Irae theme accent layer
            DamnationsCannonUtils.DrawThemeAccents(sb, Projectile.Center, 1f, 0.6f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);

            return false;
        }
    }

    /// <summary>
    /// Wrath Shrapnel ? fast homing ember fragments spawned by the wrath ball explosion.
    /// Seek nearby enemies, deal fire damage, leave short ember trails.
    /// </summary>
    public class WrathShrapnelProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

        private const int TrailLength = 8;
        private Vector2[] trailPositions = new Vector2[TrailLength];
        private int trailCount;

        private ref float Timer => ref Projectile.ai[0];

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
            Timer++;
            Projectile.rotation = Projectile.velocity.ToRotation();

            // Gravity (shrapnel rains down)
            Projectile.velocity.Y += 0.08f;

            // Light homing after initial burst
            if (Timer > 15)
            {
                float homingRange = 400f;
                float homingStr = 0.06f;
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

            // Trail
            for (int i = TrailLength - 1; i > 0; i--)
                trailPositions[i] = trailPositions[i - 1];
            trailPositions[0] = Projectile.Center;
            if (trailCount < TrailLength) trailCount++;

            // Ember dust
            if (!Main.dedServ && Main.rand.NextBool(2))
            {
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.Torch,
                    Projectile.velocity * -0.05f, 0, DiesIraePalette.EmberOrange, 0.6f);
                d.noGravity = true;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire3, 180);

            // Impact sparks
            if (!Main.dedServ)
            {
                for (int i = 0; i < 6; i++)
                {
                    Dust d = Dust.NewDustPerfect(target.Center, DustID.Torch,
                        Main.rand.NextVector2Circular(3, 3), 0,
                        DiesIraePalette.EmberOrange, 0.8f);
                    d.noGravity = true;
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D glow = MagnumTextureRegistry.GetSoftGlow();
            if (glow == null) return false;
            Vector2 origin = glow.Size() / 2f;

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);

            // Trail
            for (int i = 0; i < trailCount; i++)
            {
                float progress = i / (float)TrailLength;
                float fade = (1f - progress) * 0.6f;
                float scale = MathHelper.Lerp(0.02f, 0.006f, progress);
                Vector2 pos = trailPositions[i] - Main.screenPosition;

                sb.Draw(glow, pos, null, DiesIraePalette.EmberOrange * fade * 0.5f,
                    0f, origin, scale, SpriteEffects.None, 0f);
            }

            // Body
            DamnationsCannonUtils.DrawShrapnelBloom(sb, Projectile.Center, Timer);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);

            return false;
        }
    }

    /// <summary>
    /// Hellfire Zone ? persistent ground fire AoE spawned on wrath ball impact.
    /// Lasts 5 seconds, deals escalating fire damage. Renders as ground fire dust circle.
    /// </summary>
    public class HellfireZoneProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

        private const int Duration = 300; // 5 seconds
        private const float BaseRadius = 8f * 16f; // 8 tiles

        private ref float Timer => ref Projectile.ai[0];

        public override void SetDefaults()
        {
            Projectile.width = (int)(BaseRadius * 2);
            Projectile.height = (int)(BaseRadius * 2);
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = -1;
            Projectile.timeLeft = Duration;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 30;
        }

        public override void AI()
        {
            Timer++;
            Projectile.velocity = Vector2.Zero;

            // Escalating damage (20% per second)
            float elapsed = Timer / 60f;
            // Note: damage escalation would be handled in ModifyHitNPC

            // Spawn ground fire dust
            DamnationsCannonUtils.SpawnHellfireZoneDust(Projectile.Center, BaseRadius);

            // Render zone edge ember ring
            if (!Main.dedServ && Main.rand.NextBool(2))
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 edgePos = Projectile.Center + angle.ToRotationVector2() * BaseRadius;
                Dust d = Dust.NewDustPerfect(edgePos, DustID.Torch,
                    new Vector2(0, Main.rand.NextFloat(-1.5f, -0.3f)), 0,
                    DiesIraePalette.JudgmentGold, 0.8f);
                d.noGravity = true;
            }
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            // Damage escalates 20% per second
            float elapsed = Timer / 60f;
            float escalation = 1f + elapsed * 0.2f;
            modifiers.FinalDamage *= escalation;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            // Circular collision
            float dist = Vector2.Distance(Projectile.Center, targetHitbox.Center.ToVector2());
            return dist < BaseRadius;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D glow = MagnumTextureRegistry.GetSoftGlow();
            if (glow == null) return false;
            Vector2 origin = glow.Size() / 2f;

            float fadeIn = MathHelper.Clamp(Timer / 20f, 0f, 1f);
            float fadeOut = MathHelper.Clamp((Duration - Projectile.timeLeft) > Duration - 40 ?
                Projectile.timeLeft / 40f : 1f, 0f, 1f);
            float alpha = fadeIn * fadeOut;

            float pulse = 0.8f + 0.2f * (float)Math.Sin(Timer * 0.1f);
            float elapsed = Timer / 60f;
            float intensityScale = 1f + elapsed * 0.15f; // brightens over time

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);

            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            // Wide ground fire glow
            float glowScale = BaseRadius / (glow.Width * 0.5f) * 1.2f;
            sb.Draw(glow, drawPos, null, DamnationsCannonUtils.WrathRed * 0.15f * alpha * pulse * intensityScale,
                0f, origin, glowScale, SpriteEffects.None, 0f);
            sb.Draw(glow, drawPos, null, DiesIraePalette.EmberOrange * 0.2f * alpha * pulse * intensityScale,
                0f, origin, glowScale * 0.7f, SpriteEffects.None, 0f);
            sb.Draw(glow, drawPos, null, DiesIraePalette.JudgmentGold * 0.1f * alpha * intensityScale,
                0f, origin, glowScale * 0.4f, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);

            return false;
        }
    }
}