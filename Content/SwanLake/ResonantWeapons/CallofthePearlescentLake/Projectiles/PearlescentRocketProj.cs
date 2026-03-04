using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using MagnumOpus.Content.SwanLake.ResonantWeapons.CallofthePearlescentLake.Utilities;
using MagnumOpus.Content.SwanLake.Debuffs;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;

namespace MagnumOpus.Content.SwanLake.ResonantWeapons.CallofthePearlescentLake.Projectiles
{
    /// <summary>
    /// Ranged rocket projectile for Call of the Pearlescent Lake.
    /// Sine-wave wobble flight, homing for tidal/still-waters variants.
    /// On-kill spawns SplashZoneProj + concentric ripple VFX.
    /// Foundation-pattern rendering: SpriteBatch bloom trail, no primitives, no custom particles.
    /// </summary>
    public class PearlescentRocketProj : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

        // --- AI Slots ---
        public ref float Timer => ref Projectile.ai[0];
        public ref float WobblePhase => ref Projectile.ai[1];
        public ref float HomingStrength => ref Projectile.ai[2];

        // --- Trail ---
        private const int TrailLength = 18;
        private Vector2[] oldPos = new Vector2[TrailLength];
        private float[] oldRot = new float[TrailLength];

        // --- Variant flags (set by weapon on spawn via Projectile.localAI) ---
        public bool IsTidalWaters => Projectile.localAI[0] == 1f;
        public bool IsStillWaters => Projectile.localAI[0] == 2f;

        private Player Owner => Main.player[Projectile.owner];
        private float LifeProgress => Timer / 300f;

        public override void SetStaticDefaults() { ProjectileID.Sets.TrailCacheLength[Projectile.type] = TrailLength; }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 1;
            Projectile.alpha = 0;
        }

        public override void AI()
        {
            Timer++;
            Projectile.rotation = Projectile.velocity.ToRotation();

            // --- Sine-wave wobble ---
            WobblePhase += 0.12f;
            float wobbleOffset = MathF.Sin(WobblePhase) * 3.5f;
            Vector2 perp = Projectile.velocity.SafeNormalize(Vector2.UnitY).RotatedBy(MathHelper.PiOver2);
            Projectile.position += perp * wobbleOffset;

            // --- Homing (tidal / still waters variants) ---
            if (IsTidalWaters || IsStillWaters)
            {
                float homingRange = IsTidalWaters ? 350f : 250f;
                float homingFactor = IsTidalWaters ? 0.04f : 0.025f;

                NPC target = FindClosestNPC(homingRange);
                if (target != null)
                {
                    Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitY);
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * Projectile.velocity.Length(), homingFactor);
                }
            }

            // --- Trail recording ---
            for (int i = TrailLength - 1; i > 0; i--)
            {
                oldPos[i] = oldPos[i - 1];
                oldRot[i] = oldRot[i - 1];
            }
            oldPos[0] = Projectile.Center;
            oldRot[0] = Projectile.rotation;

            // --- Ambient dust ---
            if (Timer % 2 == 0)
            {
                Color dustColor = PearlescentUtils.GetRainbow(Timer * 0.02f);
                Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(6, 6),
                    DustID.WhiteTorch, -Projectile.velocity * 0.3f, 0, dustColor, 0.6f);
                d.noGravity = true;
            }

            // --- Pearlescent shimmer dust (white core sparkle) ---
            if (Timer % 4 == 0)
            {
                Dust s = Dust.NewDustPerfect(Projectile.Center, DustID.TintableDustLighted,
                    Main.rand.NextVector2Circular(2, 2), 0, Color.White, 0.5f);
                s.noGravity = true;
            }

            // --- Lighting ---
            Lighting.AddLight(Projectile.Center, 0.8f, 0.8f, 0.9f);
        }

        private NPC FindClosestNPC(float range)
        {
            NPC closest = null;
            float bestDist = range;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage) continue;
                float dist = Vector2.Distance(Projectile.Center, npc.Center);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    closest = npc;
                }
            }
            return closest;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<SwansMark>(), 300);

            // Rainbow impact burst
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi / 8f * i;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f);
                Color c = PearlescentUtils.GetRainbow(i / 8f);
                Dust d = Dust.NewDustPerfect(target.Center, DustID.WhiteTorch, vel, 0, c, 1.0f);
                d.noGravity = true;
            }

            // VFXLibrary integration (safe)
            try { SwanLakeVFXLibrary.SpawnRainbowBurst(target.Center, 6, 4f); } catch { }
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);

            // Spawn SplashZone AoE
            if (Projectile.owner == Main.myPlayer)
            {
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center,
                    Vector2.Zero, ModContent.ProjectileType<SplashZoneProj>(),
                    Projectile.damage / 3, 0f, Projectile.owner);
            }

            // Concentric ripple dust burst (3 rings)
            for (int ring = 0; ring < 3; ring++)
            {
                int count = 10 + ring * 6;
                float ringSpeed = 2f + ring * 2.5f;
                for (int i = 0; i < count; i++)
                {
                    float angle = MathHelper.TwoPi / count * i;
                    Vector2 vel = angle.ToRotationVector2() * ringSpeed;
                    Color c = Color.Lerp(PearlescentUtils.PearlWhite, PearlescentUtils.GetRainbow(i / (float)count + ring * 0.33f), 0.4f);
                    Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.WhiteTorch, vel, 0, c, 0.8f - ring * 0.15f);
                    d.noGravity = true;
                }
            }

            // Feather drift
            try { SwanLakeVFXLibrary.SpawnFeatherDrift(Projectile.Center, 4, 25f); } catch { }

            // Screen impact
            for (int i = 0; i < 12; i++)
            {
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.Smoke,
                    Main.rand.NextVector2Circular(5, 5), 100, Color.LightGray, 1.2f);
                d.noGravity = false;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Vector2 screenPos = Main.screenPosition;

            try
            {
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                Texture2D bloom = MagnumTextureRegistry.GetSoftGlow();
                Texture2D point = MagnumTextureRegistry.GetPointBloom();
                Texture2D radial = MagnumTextureRegistry.GetRadialBloom();

                // --- Bloom trail from oldPos ---
                if (bloom != null)
                {
                    Vector2 bloomOrigin = bloom.Size() * 0.5f;
                    for (int i = TrailLength - 1; i >= 1; i--)
                    {
                        if (oldPos[i] == Vector2.Zero) continue;
                        float progress = 1f - i / (float)TrailLength;
                        float trailAlpha = progress * 0.6f;
                        float trailScale = 0.35f + progress * 0.25f;

                        Color trailColor = Color.Lerp(PearlescentUtils.LakeSilver, PearlescentUtils.PearlWhite, progress);
                        // Rainbow tint along trail
                        Color rainbow = PearlescentUtils.GetRainbow(i / (float)TrailLength + (float)Main.timeForVisualEffects * 0.01f);
                        trailColor = Color.Lerp(trailColor, rainbow, 0.25f);

                        sb.Draw(bloom, oldPos[i] - screenPos, null, trailColor * trailAlpha,
                            oldRot[i], bloomOrigin, trailScale, SpriteEffects.None, 0f);
                    }
                }

                // --- Rocket core bloom stack ---
                Vector2 drawPos = Projectile.Center - screenPos;
                float pulse = 0.9f + 0.1f * MathF.Sin((float)Main.timeForVisualEffects * 0.08f);

                // Outer pearl glow (wide, soft)
                if (radial != null)
                {
                    Vector2 radialOrigin = radial.Size() * 0.5f;
                    Color outerColor = Color.Lerp(PearlescentUtils.MistBlue, PearlescentUtils.PearlWhite, 0.5f);
                    sb.Draw(radial, drawPos, null, outerColor * 0.25f * pulse, 0f, radialOrigin, 0.7f * pulse, SpriteEffects.None, 0f);
                }

                // Mid glow (pearlescent)
                if (bloom != null)
                {
                    Vector2 bloomOrigin = bloom.Size() * 0.5f;
                    sb.Draw(bloom, drawPos, null, PearlescentUtils.PearlWhite * 0.5f * pulse, 0f, bloomOrigin, 0.4f * pulse, SpriteEffects.None, 0f);
                }

                // Hot core (bright)
                if (point != null)
                {
                    Vector2 pointOrigin = point.Size() * 0.5f;
                    sb.Draw(point, drawPos, null, Color.White * 0.8f, 0f, pointOrigin, 0.25f * pulse, SpriteEffects.None, 0f);
                }

                // --- Rainbow shimmer ring at projectile ---
                if (bloom != null)
                {
                    Vector2 bloomOrigin = bloom.Size() * 0.5f;
                    for (int i = 0; i < 6; i++)
                    {
                        float angle = MathHelper.TwoPi / 6f * i + (float)Main.timeForVisualEffects * 0.03f;
                        Vector2 offset = angle.ToRotationVector2() * 8f;
                        Color rc = PearlescentUtils.GetRainbow(i / 6f);
                        sb.Draw(bloom, drawPos + offset, null, rc * 0.2f, 0f, bloomOrigin, 0.15f, SpriteEffects.None, 0f);
                    }
                }

                // --- Draw rocket sprite on top ---
                DrawRocketSprite(sb, drawPos, lightColor);
            }
            catch { }
            finally
            {
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                    DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }

            // Theme accents (additive)
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);
            PearlescentUtils.DrawThemeAccents(sb, Projectile.Center, 1f, 0.6f);
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }

        private void DrawRocketSprite(SpriteBatch sb, Vector2 drawPos, Color lightColor)
        {
            Texture2D tex = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            if (tex == null) return;

            Vector2 origin = tex.Size() * 0.5f;
            float rot = Projectile.rotation + MathHelper.PiOver2;

            // Subtle white glow behind sprite
            Texture2D glow = MagnumTextureRegistry.GetSoftGlow();
            if (glow != null)
            {
                Vector2 glowOrigin = glow.Size() * 0.5f;
                sb.Draw(glow, drawPos, null, PearlescentUtils.PearlWhite * 0.3f, 0f, glowOrigin, 0.25f, SpriteEffects.None, 0f);
            }

            // Switch to alpha blend for sprite
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            sb.Draw(tex, drawPos, null, Projectile.GetAlpha(lightColor), rot, origin, Projectile.scale, SpriteEffects.None, 0f);

            // Back to additive for caller
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }
    }
}
