using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Content.TestWeapons.SandboxWeapons.Shaders;

namespace MagnumOpus.Content.TestWeapons.SandboxWeapons
{
    /// <summary>
    /// Crystalline energy shard spawned by the Sandbox Terra Blade during swings.
    /// 3-phase AI:
    ///   Phase 0 (Launch)  – Rise and decelerate for 20 ticks.
    ///   Phase 1 (Hover)   – Lock in place, acquire target, build charge flare for 25 ticks.
    ///   Phase 2 (Fire+Fade) – Spawn LightShardBeam toward target, then fade out over 20 ticks.
    /// </summary>
    public class LightShardProjectile : ModProjectile
    {
        // =====================================================================
        //  Constants
        // =====================================================================

        private const int LaunchDuration = 20;
        private const int HoverDuration = 25;
        private const int FadeDuration = 20;
        private const float TargetSearchRange = 600f;
        private const int TrailCacheSize = 16;

        // =====================================================================
        //  AI Slot Accessors
        // =====================================================================

        /// <summary>Current phase: 0 = Launch, 1 = Hover, 2 = Fire+Fade.</summary>
        private ref float Phase => ref Projectile.ai[0];

        /// <summary>Frame counter within the current phase.</summary>
        private ref float PhaseTimer => ref Projectile.ai[1];

        // =====================================================================
        //  State
        // =====================================================================

        private int cachedTargetWhoAmI = -1;

        // =====================================================================
        //  Setup
        // =====================================================================

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = TrailCacheSize;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.extraUpdates = 1;
            Projectile.timeLeft = (LaunchDuration + HoverDuration + FadeDuration) * 2 + 10; // +10 buffer
            Projectile.alpha = 0;
        }

        public override bool? CanDamage() => false;

        public override string Texture => "MagnumOpus/Assets/Particles/SmallCrystalShard";

        // =====================================================================
        //  AI
        // =====================================================================

        public override void AI()
        {
            PhaseTimer++;

            switch ((int)Phase)
            {
                case 0:
                    AI_Launch();
                    break;
                case 1:
                    AI_Hover();
                    break;
                case 2:
                    AI_FireAndFade();
                    break;
            }

            // Dynamic lighting in theme color
            Color light = TerraBladeShaderManager.GetPaletteColor(0.5f);
            Lighting.AddLight(Projectile.Center, light.ToVector3() * 0.9f);
        }

        // ----- Phase 0: Launch -----

        private void AI_Launch()
        {
            // Decelerate with friction
            Projectile.velocity *= 0.94f;

            // Gentle upward drift
            Projectile.velocity.Y -= 0.15f;

            // Spin
            Projectile.rotation += Projectile.velocity.Length() * 0.06f * Projectile.direction;

            // Sparkle trail
            if (Main.rand.NextBool(2))
            {
                Color sparkColor = TerraBladeShaderManager.GetPaletteColor(Main.rand.NextFloat(0.3f, 0.7f));
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.GreenTorch,
                    -Projectile.velocity * 0.3f + Main.rand.NextVector2Circular(1f, 1f),
                    0, sparkColor, 1.2f);
                d.noGravity = true;
            }

            if (PhaseTimer >= LaunchDuration)
            {
                Phase = 1;
                PhaseTimer = 0;
                Projectile.velocity = Vector2.Zero;
            }
        }

        // ----- Phase 1: Hover -----

        private void AI_Hover()
        {
            // Hold position with gentle sine bob
            Projectile.velocity = Vector2.Zero;
            Projectile.position.Y += MathF.Sin(PhaseTimer * 0.15f) * 0.3f;

            // Slow spin
            Projectile.rotation += 0.04f;

            // Target acquisition
            if (cachedTargetWhoAmI < 0)
            {
                float bestDist = TargetSearchRange;
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (!npc.active || npc.friendly || npc.dontTakeDamage || npc.immortal)
                        continue;
                    float dist = Vector2.Distance(Projectile.Center, npc.Center);
                    if (dist < bestDist)
                    {
                        bestDist = dist;
                        cachedTargetWhoAmI = i;
                    }
                }
            }

            // Charge flare: converging particles every 3 ticks
            if (PhaseTimer % 3 == 0)
            {
                float chargeProgress = PhaseTimer / (float)HoverDuration;
                int count = 2 + (int)(chargeProgress * 4);
                for (int i = 0; i < count; i++)
                {
                    float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                    float radius = 35f * (1f - chargeProgress * 0.5f);
                    Vector2 offset = angle.ToRotationVector2() * radius;
                    Vector2 vel = -offset * 0.08f;
                    Color c = TerraBladeShaderManager.GetPaletteColor(0.4f + chargeProgress * 0.4f);

                    var glow = new GenericGlowParticle(
                        Projectile.Center + offset, vel,
                        c * 0.7f, 0.2f + chargeProgress * 0.15f, 12, true);
                    MagnumParticleHandler.SpawnParticle(glow);
                }

                // Occasional music note during charge (ramps up with progress)
                if (Main.rand.NextFloat() < chargeProgress * 0.6f)
                {
                    Vector2 noteVel = Main.rand.NextVector2Circular(2f, 2f) + new Vector2(0f, -1f);
                    Color noteColor = TerraBladeShaderManager.GetPaletteColor(0.3f + Main.rand.NextFloat() * 0.5f);
                    float noteScale = Main.rand.NextFloat(0.7f, 0.9f);
                    ThemedParticles.MusicNote(Projectile.Center, noteVel, noteColor, noteScale, 30);
                }
            }

            if (PhaseTimer >= HoverDuration)
            {
                Phase = 2;
                PhaseTimer = 0;
                FireBeam();
            }
        }

        // ----- Phase 2: Fire + Fade -----

        private void AI_FireAndFade()
        {
            // Fade out
            float fadeProgress = PhaseTimer / (float)FadeDuration;
            Projectile.alpha = (int)(fadeProgress * 255f);

            // Slow shrink
            Projectile.scale = 1f - fadeProgress * 0.5f;

            // Gentle upward float while fading
            Projectile.velocity.Y -= 0.05f;

            if (PhaseTimer >= FadeDuration)
            {
                Projectile.Kill();
            }
        }

        // =====================================================================
        //  Beam Spawning
        // =====================================================================

        private void FireBeam()
        {
            if (Main.myPlayer != Projectile.owner) return;

            // Validate target
            if (cachedTargetWhoAmI >= 0 && cachedTargetWhoAmI < Main.maxNPCs)
            {
                NPC target = Main.npc[cachedTargetWhoAmI];
                if (target.active && !target.friendly && !target.dontTakeDamage)
                {
                    Projectile.NewProjectile(
                        Projectile.GetSource_FromThis(),
                        Projectile.Center,
                        (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitY),
                        ModContent.ProjectileType<LightShardBeam>(),
                        Projectile.damage,
                        Projectile.knockBack,
                        Projectile.owner,
                        ai0: cachedTargetWhoAmI);
                    return;
                }
            }

            // No valid target -- particle burst then let Phase 2 fade handle death
            for (int i = 0; i < 6; i++)
            {
                Vector2 burstVel = Main.rand.NextVector2Circular(3f, 3f);
                Color burstColor = TerraBladeShaderManager.GetPaletteColor(Main.rand.NextFloat(0.4f, 0.8f));
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.GreenTorch, burstVel, 0, burstColor, 1.4f);
                d.noGravity = true;
            }
        }

        // =====================================================================
        //  Rendering
        // =====================================================================

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D shardTex = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = shardTex.Size() * 0.5f;

            float alphaFactor = 1f - Projectile.alpha / 255f;

            // --- Afterimage trail (fading copies at old positions) ---
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                float progress = (float)i / Projectile.oldPos.Length;
                float trailAlpha = (1f - progress) * 0.5f * alphaFactor;
                float trailScale = Projectile.scale * (1f - progress * 0.3f);
                Color trailColor = TerraBladeShaderManager.GetPaletteColor(0.3f + progress * 0.5f) * trailAlpha;
                Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size * 0.5f - Main.screenPosition;
                sb.Draw(shardTex, trailPos, null, trailColor, Projectile.oldRot[i],
                    origin, trailScale, SpriteEffects.None, 0f);
            }

            // --- Crystal shard sprite (main draw) ---
            Color shardColor = TerraBladeShaderManager.GetPaletteColor(0.5f) * alphaFactor;
            sb.Draw(shardTex, drawPos, null, shardColor, Projectile.rotation,
                origin, Projectile.scale, SpriteEffects.None, 0f);

            // --- Small bloom glow behind shard ---
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);

            Color glowColor = TerraBladeShaderManager.GetPaletteColor(0.4f);
            sb.Draw(shardTex, drawPos, null, glowColor * 0.4f * alphaFactor, Projectile.rotation,
                origin, Projectile.scale * 1.3f, SpriteEffects.None, 0f);
            sb.Draw(shardTex, drawPos, null, glowColor * 0.2f * alphaFactor, Projectile.rotation,
                origin, Projectile.scale * 1.6f, SpriteEffects.None, 0f);

            // Charge flare during hover phase
            if ((int)Phase == 1)
            {
                float chargeProgress = PhaseTimer / (float)HoverDuration;
                Texture2D flareTex = Terraria.GameContent.TextureAssets.Extra[98].Value;
                Vector2 flareOrigin = flareTex.Size() * 0.5f;
                float flareScale = 0.1f + chargeProgress * 0.3f;
                Color flareColor = TerraBladeShaderManager.GetPaletteColor(0.7f + chargeProgress * 0.3f);
                sb.Draw(flareTex, drawPos, null, flareColor * (0.3f + chargeProgress * 0.4f),
                    Main.GlobalTimeWrappedHourly * 3f, flareOrigin, flareScale, SpriteEffects.None, 0f);
            }

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);

            return false;
        }

        // =====================================================================
        //  Networking
        // =====================================================================

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(cachedTargetWhoAmI);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            cachedTargetWhoAmI = reader.ReadInt32();
        }
    }
}
