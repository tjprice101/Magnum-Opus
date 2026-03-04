using MagnumOpus.Common.Systems.VFX.Core;
using MagnumOpus.Content.DiesIrae;
using MagnumOpus.Content.DiesIrae.Weapons.ChainOfJudgment.Buffs;
using MagnumOpus.Content.DiesIrae.Weapons.ChainOfJudgment.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.VFX;

namespace MagnumOpus.Content.DiesIrae.Weapons.ChainOfJudgment.Projectiles
{
    /// <summary>
    /// Judgment Chain ? the core chain-whip projectile.
    /// 
    /// Behaviour:
    ///   Phase 0 (Extend): Launches outward from the player, decelerating over distance.
    ///   Phase 1 (Retract): Returns to the player, pulled by increasing force.
    ///   Hit enemies receive Chain Link stacks; at 5 stacks �� Fully Bound.
    ///   Every 5 cumulative hits spawns a ChainLightningArc.
    /// 
    /// Rendering: Segmented chain body drawn between player and tip with jitter offsets.
    ///   Tip glow + ember dust trail. Additive bloom layering throughout.
    /// </summary>
    public class JudgmentChainProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

        // ������ Constants ����������������������������������������������������������������������������������������������������
        private const float MaxRange = 12f * 16f; // 12 tiles in pixels
        private const float ExtendDecel = 0.94f;
        private const float RetractAccel = 0.18f;
        private const float RetractMaxSpeed = 24f;
        private const float KillRadius = 36f;
        private const int MaxExtendTime = 35;

        // ������ AI slots ������������������������������������������������������������������������������������������������������
        /// <summary>0 = extending, 1 = retracting.</summary>
        private ref float Phase => ref Projectile.ai[0];
        /// <summary>Frame counter.</summary>
        private ref float Timer => ref Projectile.ai[1];

        private int hitCombo; // cumulative hits this throw

        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 12;
            Projectile.extraUpdates = 1;
        }

        // ������ AI ������������������������������������������������������������������������������������������������������������������

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            if (!owner.active || owner.dead)
            {
                Projectile.Kill();
                return;
            }

            Timer++;
            Projectile.rotation = Projectile.velocity.ToRotation();

            // Keep owner animation alive
            owner.ChangeDir(Projectile.Center.X > owner.Center.X ? 1 : -1);
            owner.itemAnimation = 10;
            owner.itemTime = 10;
            owner.heldProj = Projectile.whoAmI;

            if (Phase == 0)
                ExtendPhase(owner);
            else
                RetractPhase(owner);

            // Ember dust trail
            SpawnTrailDust();
        }

        private void ExtendPhase(Player owner)
        {
            float dist = Vector2.Distance(Projectile.Center, owner.Center);

            // Transition to retract on max range or max time
            if (dist > MaxRange || Timer > MaxExtendTime)
            {
                Phase = 1;
                Timer = 0;
                SoundEngine.PlaySound(SoundID.Item153 with { Volume = 0.4f, Pitch = 0.3f }, Projectile.Center);
                return;
            }

            // Decelerate as we extend
            Projectile.velocity *= ExtendDecel;
        }

        private void RetractPhase(Player owner)
        {
            Vector2 toOwner = owner.Center - Projectile.Center;
            float dist = toOwner.Length();

            if (dist < KillRadius)
            {
                Projectile.Kill();
                return;
            }

            // Accelerate toward owner
            toOwner.Normalize();
            Vector2 desired = toOwner * Math.Min(RetractMaxSpeed, dist * 0.1f + 8f);
            Projectile.velocity = Vector2.Lerp(Projectile.velocity, desired, RetractAccel);
        }

        private void SpawnTrailDust()
        {
            if (Main.dedServ) return;
            if (!Main.rand.NextBool(3)) return;

            Dust d = Dust.NewDustPerfect(
                Projectile.Center + Main.rand.NextVector2Circular(8, 8),
                DustID.Torch,
                Projectile.velocity * -0.1f + Main.rand.NextVector2Circular(1f, 1f),
                0, DiesIraePalette.EmberOrange, 0.8f);
            d.noGravity = true;
        }

        // ������ Hit ����������������������������������������������������������������������������������������������������������������

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Fire debuff
            target.AddBuff(BuffID.OnFire3, 300);

            // Chain Link stacking
            target.AddBuff(ModContent.BuffType<ChainLinkMark>(), 300);
            var global = target.GetGlobalNPC<ChainOfJudgmentGlobalNPC>();
            global.IncrementChainLink(target);

            // Impact VFX
            ChainOfJudgmentUtils.DoChainImpact(target.Center, Projectile.velocity.SafeNormalize(Vector2.Zero));

            // Combo tracking �� chain lightning
            hitCombo++;
            if (hitCombo >= 5 && Projectile.owner == Main.myPlayer)
            {
                SpawnChainLightning(target);
                hitCombo = 0;
            }
        }

        private void SpawnChainLightning(NPC source)
        {
            // Find nearest enemy to arc toward
            float bestDist = 400f;
            int bestIdx = -1;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage || npc.whoAmI == source.whoAmI) continue;
                float d = Vector2.Distance(npc.Center, source.Center);
                if (d < bestDist) { bestDist = d; bestIdx = i; }
            }

            if (bestIdx >= 0)
            {
                Vector2 dir = (Main.npc[bestIdx].Center - source.Center).SafeNormalize(Vector2.UnitX);
                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    source.Center, dir * 20f,
                    ModContent.ProjectileType<ChainLightningArc>(),
                    (int)(Projectile.damage * 0.5f), 2f,
                    Projectile.owner, bestIdx);

                SoundEngine.PlaySound(SoundID.DD2_LightningAuraZap with { Volume = 0.5f }, source.Center);
            }
        }

        // ������ Drawing ��������������������������������������������������������������������������������������������������������

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Player owner = Main.player[Projectile.owner];

            // ���� Layer 1: Chain body (additive) ����
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);

            ChainOfJudgmentUtils.DrawChainBody(sb, owner.Center, Projectile.Center, Timer);

            // ���� Layer 2: Tip glow ����
            ChainOfJudgmentUtils.DrawTipBloom(sb, Projectile.Center, Timer);

            // ���� Layer 3: Retract phase ? intensified trail glow ����
            if (Phase == 1 && Timer > 5)
            {
                float retractIntensity = MathHelper.Clamp(Timer / 20f, 0f, 1f);
                Texture2D glow = MagnumTextureRegistry.GetSoftGlow();
                if (glow != null)
                {
                    Vector2 origin = glow.Size() / 2f;
                    Vector2 drawPos = Projectile.Center - Main.screenPosition;
                    sb.Draw(glow, drawPos, null, DiesIraePalette.JudgmentGold * 0.3f * retractIntensity,
                        0f, origin, 0.12f, SpriteEffects.None, 0f);
                }
            }

            // Dies Irae theme accent layer
            ChainOfJudgmentUtils.DrawThemeAccents(sb, Projectile.Center, 1f, 0.6f);

            // ���� Restore blend state ����
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);

            return false;
        }
    }
}