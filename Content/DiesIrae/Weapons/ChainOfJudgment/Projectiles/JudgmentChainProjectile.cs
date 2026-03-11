using MagnumOpus.Common;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Core;
using MagnumOpus.Common.Systems;
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

namespace MagnumOpus.Content.DiesIrae.Weapons.ChainOfJudgment.Projectiles
{
    /// <summary>
    /// Judgment Chain - the core chain-whip projectile.
    ///
    /// Behaviour:
    ///   Phase 0 (Extend): Launches outward, decelerating over distance.
    ///   Phase 1 (Retract): Returns to the player, pulled by increasing force.
    ///   Hit enemies receive Chain Link stacks; at 5 stacks -> Fully Bound.
    ///   Every 5 cumulative hits spawns a ChainLightningArc.
    ///
    /// Rendering: 5-layer composited chain VFX:
    ///   L1: Shader-driven chain body via DiesIraeShaderManager
    ///   L2: Segmented glow links (ChainOfJudgmentUtils.DrawChainBody)
    ///   L3: Tip bloom with radial flare
    ///   L4: Theme accent flares (star flare, impact rings)
    ///   L5: Retract phase ember intensification + judgment spark trail
    /// </summary>
    public class JudgmentChainProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

        // --- Constants ---
        private const float MaxRange = 12f * 16f; // 12 tiles in pixels
        private const float ExtendDecel = 0.94f;
        private const float RetractAccel = 0.18f;
        private const float RetractMaxSpeed = 24f;
        private const float KillRadius = 36f;
        private const int MaxExtendTime = 35;

        // --- AI slots ---
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

        // --- AI ---

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

                // Retract initiation VFX: judgment spark burst at chain tip
                DiesIraeVFXLibrary.SpawnColorRampedSparkleExplosion(Projectile.Center, 6, 4f, 0.25f);
                DiesIraeVFXLibrary.SpawnEmberScatter(Projectile.Center, 5, 3f);
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

            // Standard ember trail
            if (Main.rand.NextBool(3))
            {
                Dust d = Dust.NewDustPerfect(
                    Projectile.Center + Main.rand.NextVector2Circular(8, 8),
                    DustID.Torch,
                    Projectile.velocity * -0.1f + Main.rand.NextVector2Circular(1f, 1f),
                    0, DiesIraePalette.EmberOrange, 0.8f);
                d.noGravity = true;
            }

            // Retract phase: intensified dust + gold sparks
            if (Phase == 1 && Timer > 5)
            {
                if (Main.rand.NextBool(2))
                {
                    Dust g = Dust.NewDustPerfect(
                        Projectile.Center + Main.rand.NextVector2Circular(6, 6),
                        DustID.GoldFlame,
                        Projectile.velocity * -0.15f,
                        0, DiesIraePalette.JudgmentGold, 1.0f);
                    g.noGravity = true;
                    g.fadeIn = 0.7f;
                }
            }

            // Music notes every 12 frames during retract (chains of judgment sing)
            if (Phase == 1 && Main.GameUpdateCount % 12 == 0)
            {
                DiesIraeVFXLibrary.SpawnMusicNotes(Projectile.Center, 1, 8f, 0.4f, 0.6f, 18);
            }
        }

        // --- Hit ---

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Fire debuff
            target.AddBuff(BuffID.OnFire3, 300);

            // Chain Link stacking
            target.AddBuff(ModContent.BuffType<ChainLinkMark>(), 300);
            var global = target.GetGlobalNPC<ChainOfJudgmentGlobalNPC>();
            int stacks = global.IncrementChainLink(target);

            // Multi-layered impact VFX via DiesIraeVFXLibrary
            Vector2 hitDir = Projectile.velocity.SafeNormalize(Vector2.Zero);
            DiesIraeVFXLibrary.MeleeImpact(target.Center, Math.Min(stacks, 2));

            // Directional chain sparks on impact
            DiesIraeVFXLibrary.SpawnDirectionalSparkleExplosion(
                target.Center, hitDir, 6 + stacks * 2, 5f, 0.3f, 0.6f);

            // Chain impact (metallic sparks + binding ring)
            ChainOfJudgmentUtils.DoChainImpact(target.Center, hitDir);

            // Fully Bound conversion flash
            if (stacks == 0) // just triggered Fully Bound (stacks reset to 0)
            {
                DiesIraeVFXLibrary.SpawnHellfireStarburst(target.Center, 1.3f);
                DiesIraeVFXLibrary.SpawnJudgmentRings(target.Center, 3, 0.4f);
                MagnumScreenEffects.AddScreenShake(4f);
                SoundEngine.PlaySound(SoundID.Item73 with { Pitch = -0.3f, Volume = 0.8f }, target.Center);
            }

            // Combo tracking -> chain lightning
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

        // --- Drawing: 5-layer composited chain VFX ---

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            try
            {
                Player owner = Main.player[Projectile.owner];
                float time = (float)Main.GameUpdateCount;
                float dist = Vector2.Distance(owner.Center, Projectile.Center);
                float chainIntensity = Phase == 1 ? MathHelper.Clamp(Timer / 15f, 0.5f, 1f) : 0.7f;

                // -- LAYER 1: Shader-driven chain aura --
                bool hasShader = false;
                try { hasShader = DiesIraeShaderManager.HasFlameTrail; }
                catch { }

                sb.End();
                sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);

                if (hasShader)
                {
                    try
                    {
                        DiesIraeShaderManager.BeginShaderAdditive(sb);

                        Texture2D glow = MagnumTextureRegistry.GetSoftGlow();
                        if (glow != null)
                        {
                            Vector2 glowOrigin = glow.Size() * 0.5f;
                            // Shader-enhanced bloom at chain midpoint
                            Vector2 midWorld = Vector2.Lerp(owner.Center, Projectile.Center, 0.5f);
                            Vector2 midScreen = midWorld - Main.screenPosition;
                            Color shaderColor = DiesIraePalette.Additive(DiesIraePalette.BloodRed, 0.2f * chainIntensity);
                            sb.Draw(glow, midScreen, null, shaderColor, 0f, glowOrigin, 0.06f, SpriteEffects.None, 0f);
                        }

                        DiesIraeShaderManager.RestoreSpriteBatch(sb);

                        // Re-enter additive after shader restore
                        sb.End();
                        sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                            DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);
                    }
                    catch
                    {
                        try { sb.End(); } catch { }
                        sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                            DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);
                    }
                }

                // -- LAYER 2: Segmented glow chain body --
                ChainOfJudgmentUtils.DrawChainBody(sb, owner.Center, Projectile.Center, Timer);

                // -- LAYER 3: Tip bloom with radial flare --
                ChainOfJudgmentUtils.DrawTipBloom(sb, Projectile.Center, Timer, chainIntensity);

                // -- LAYER 4: Theme accent flares --
                DiesIraeVFXLibrary.DrawThemeStarFlare(sb, Projectile.Center, 0.9f, chainIntensity * 0.5f);

                // Judgment ring at tip during retract
                if (Phase == 1 && Timer > 8)
                {
                    float ringRot = time * 0.03f;
                    DiesIraeVFXLibrary.DrawThemeImpactRing(sb, Projectile.Center, 0.8f, chainIntensity * 0.4f, ringRot);
                }

                // -- LAYER 5: Retract intensification --
                if (Phase == 1 && Timer > 5)
                {
                    float retractGlow = MathHelper.Clamp(Timer / 20f, 0f, 1f);
                    Texture2D softGlow = MagnumTextureRegistry.GetSoftGlow();
                    if (softGlow != null)
                    {
                        Vector2 origin = softGlow.Size() * 0.5f;
                        Vector2 drawPos = Projectile.Center - Main.screenPosition;

                        // Outer ember haze
                        Color outerColor = DiesIraePalette.Additive(DiesIraePalette.EmberOrange, 0.25f * retractGlow);
                        sb.Draw(softGlow, drawPos, null, outerColor, 0f, origin, 0.14f, SpriteEffects.None, 0f);

                        // Inner gold core
                        Color coreColor = DiesIraePalette.Additive(DiesIraePalette.JudgmentGold, 0.4f * retractGlow);
                        sb.Draw(softGlow, drawPos, null, coreColor, 0f, origin, 0.06f, SpriteEffects.None, 0f);

                        // Wrath-white hotspot
                        Color hotColor = DiesIraePalette.Additive(DiesIraePalette.WrathWhite, 0.2f * retractGlow);
                        sb.Draw(softGlow, drawPos, null, hotColor, 0f, origin, 0.03f, SpriteEffects.None, 0f);
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
