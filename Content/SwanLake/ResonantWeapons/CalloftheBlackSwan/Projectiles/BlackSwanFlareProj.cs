using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.SwanLake.ResonantWeapons.CalloftheBlackSwan.Utilities;
using MagnumOpus.Content.SwanLake.Debuffs;
using MagnumOpus.Common.Systems.VFX;

namespace MagnumOpus.Content.SwanLake.ResonantWeapons.CalloftheBlackSwan.Projectiles
{
    /// <summary>
    /// Black Swan Flare — Homing sub-projectile fired during swing phases.
    /// Dual-polarity: randomly black or white on spawn. Tracks enemies.
    /// ai[0] = 0: normal, 1: empowered (rainbow aura), 2: grand jeté shockwave seed.
    /// ai[1] = polarity (0 = white, 1 = black).
    /// Foundation-pattern rendering: safe SpriteBatch, MagnumTextureRegistry textures.
    /// </summary>
    public class BlackSwanFlareProj : ModProjectile
    {
        #region Properties

        public bool IsEmpowered => Projectile.ai[0] >= 1f;
        public bool IsGrandJete => Projectile.ai[0] >= 2f;
        public bool IsBlack => Projectile.ai[1] >= 1f;

        private const float HomingRange = 350f;
        private const float HomingStrength = 0.08f;
        private const float MaxSpeed = 16f;

        private Player Owner => Main.player[Projectile.owner];
        private bool _initialized;

        #endregion

        public override string Texture => "MagnumOpus/Content/SwanLake/ResonantWeapons/CalloftheBlackSwan/CalloftheBlackSwan";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 20;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            if (!_initialized)
            {
                _initialized = true;
                Projectile.ai[1] = Main.rand.NextBool() ? 1f : 0f;
                Projectile.rotation = Projectile.velocity.ToRotation();
            }

            // Homing AI
            NPC target = BlackSwanUtils.ClosestNPCAt(Projectile.Center, HomingRange);
            if (target != null)
            {
                Vector2 desiredDir = Projectile.Center.SafeDirectionTo(target.Center);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, desiredDir * Projectile.velocity.Length(), HomingStrength);
            }

            if (Projectile.velocity.Length() > MaxSpeed)
                Projectile.velocity = Vector2.Normalize(Projectile.velocity) * MaxSpeed;

            Projectile.rotation = Projectile.velocity.ToRotation();

            // Trail dust — dual polarity
            if (Main.rand.NextBool(3))
            {
                int dustType = IsBlack ? DustID.Shadowflame : DustID.WhiteTorch;
                Color dustColor = IsBlack ? new Color(30, 30, 45) : new Color(240, 240, 255);
                Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(6f, 6f),
                    dustType, -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(0.5f, 0.5f),
                    0, dustColor, 0.8f);
                d.noGravity = true;
                d.fadeIn = 0.6f;
            }

            // Empowered rainbow sparkle
            if (IsEmpowered && Main.rand.NextBool(4))
            {
                float hue = Main.rand.NextFloat();
                Color rainbow = Main.hslToRgb(hue, 0.85f, 0.8f);
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.RainbowTorch,
                    Main.rand.NextVector2Circular(1f, 1f), 0, rainbow, 0.5f);
                d.noGravity = true;
            }

            // Pulsing light
            float intensity = IsEmpowered ? 0.6f : 0.35f;
            float pulse = 1f + 0.15f * (float)Math.Sin(Projectile.timeLeft * 0.2f);
            Vector3 lightColor = IsBlack
                ? new Vector3(0.15f, 0.15f, 0.25f)
                : new Vector3(0.5f, 0.5f, 0.6f);
            Lighting.AddLight(Projectile.Center, lightColor * intensity * pulse);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            try { Owner.GetModPlayer<BlackSwanPlayer>().RegisterHit(); } catch { }
            try { Owner.GetModPlayer<BlackSwanPlayer>().RegisterFlareHit(); } catch { }

            target.AddBuff(ModContent.BuffType<SwansMark>(), 300);

            Vector2 hitPos = target.Center;

            // Impact sparks — dual polarity
            for (int i = 0; i < 6; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2CircularEdge(4f, 4f);
                bool isBlack = i % 2 == 0;
                Color col = isBlack ? new Color(30, 30, 45) : new Color(240, 240, 255);
                Dust d = Dust.NewDustPerfect(hitPos, DustID.RainbowTorch, sparkVel, 0, col, 0.5f);
                d.noGravity = true;
            }

            // Feather on impact
            for (int i = 0; i < 2; i++)
            {
                Vector2 featherVel = Main.rand.NextVector2Circular(2f, 2f) + new Vector2(0, -1f);
                bool isBlack = Main.rand.NextBool();
                Dust d = Dust.NewDustPerfect(hitPos + Main.rand.NextVector2Circular(8f, 8f),
                    isBlack ? DustID.Shadowflame : DustID.WhiteTorch, featherVel, 0,
                    isBlack ? new Color(30, 30, 40) : new Color(248, 245, 255), 0.5f);
                d.noGravity = true;
            }

            try { SwanLakeVFXLibrary.SpawnMusicNotes(hitPos, 2, 15f, 0.5f, 0.8f, 22); } catch { }

            // Empowered rainbow burst
            if (IsEmpowered)
            {
                for (int i = 0; i < 8; i++)
                {
                    float hue = (float)i / 8f;
                    Color rainbow = Main.hslToRgb(hue, 0.85f, 0.8f);
                    Vector2 burstVel = Main.rand.NextVector2CircularEdge(6f, 6f);
                    Dust d = Dust.NewDustPerfect(hitPos, DustID.RainbowTorch, burstVel, 0, rainbow, 0.7f);
                    d.noGravity = true;
                }
            }
        }

        #region Rendering (Foundation Pattern)

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;

            try
            {
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                DrawBloomTrail(sb);
                DrawFlareCore(sb);
            }
            catch { }
            finally
            {
                try { sb.End(); } catch { }
                try
                {
                    sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                        DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                }
                catch { }
            }

            return false;
        }

        private void DrawBloomTrail(SpriteBatch sb)
        {
            Texture2D bloom = MagnumTextureRegistry.GetSoftGlow();
            if (bloom == null) return;

            Vector2 origin = bloom.Size() * 0.5f;

            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;

                float t = (float)i / Projectile.oldPos.Length;
                Vector2 drawPos = Projectile.oldPos[i] + Projectile.Size * 0.5f - Main.screenPosition;
                float trailAlpha = (1f - t) * 0.6f;
                float trailScale = MathHelper.Lerp(0.15f, 0.03f, t) * (IsEmpowered ? 1.3f : 1f);

                // Dual-polarity trail color
                Color trailCol = IsBlack
                    ? new Color(40, 40, 60, 0)
                    : new Color(200, 200, 220, 0);

                sb.Draw(bloom, drawPos, null, trailCol * trailAlpha, 0f, origin, trailScale, SpriteEffects.None, 0f);

                // White core trail
                sb.Draw(bloom, drawPos, null, new Color(255, 255, 255, 0) * trailAlpha * 0.3f, 0f, origin, trailScale * 0.4f, SpriteEffects.None, 0f);

                // Rainbow accent on empowered
                if (IsEmpowered && i % 2 == 0)
                {
                    float hue = (t + Main.GameUpdateCount * 0.01f) % 1f;
                    Color rainbow = Main.hslToRgb(hue, 0.85f, 0.8f);
                    sb.Draw(bloom, drawPos, null, new Color(rainbow.R, rainbow.G, rainbow.B, 0) * trailAlpha * 0.25f,
                        0f, origin, trailScale * 1.8f, SpriteEffects.None, 0f);
                }
            }
        }

        private void DrawFlareCore(SpriteBatch sb)
        {
            Texture2D radial = MagnumTextureRegistry.GetRadialBloom();
            Texture2D point = MagnumTextureRegistry.GetPointBloom();
            Texture2D star = MagnumTextureRegistry.GetStar4Soft();

            Vector2 screenPos = Projectile.Center - Main.screenPosition;
            float pulse = 0.8f + 0.2f * (float)Math.Sin(Projectile.timeLeft * 0.15f);
            float baseScale = IsEmpowered ? 0.5f : 0.35f;

            Color outerColor = IsBlack ? new Color(40, 40, 60, 0) : new Color(220, 220, 240, 0);

            // Layer 1: Wide soft halo
            if (radial != null)
            {
                Vector2 srOrigin = radial.Size() * 0.5f;
                sb.Draw(radial, screenPos, null, outerColor * 0.35f * pulse, 0f, srOrigin, baseScale * 2.2f, SpriteEffects.None, 0f);

                // Layer 2: Mid polarity glow
                Color midColor = IsBlack ? new Color(60, 60, 85, 0) : new Color(200, 200, 230, 0);
                sb.Draw(radial, screenPos, null, midColor * 0.45f * pulse, 0f, srOrigin, baseScale * 1.2f, SpriteEffects.None, 0f);
            }

            // Layer 3: White-hot core
            if (point != null)
            {
                Vector2 pbOrigin = point.Size() * 0.5f;
                sb.Draw(point, screenPos, null, new Color(255, 255, 255, 0) * 0.7f * pulse, 0f, pbOrigin, baseScale * 0.6f, SpriteEffects.None, 0f);
            }

            // Layer 4: Rotating star accent
            if (star != null)
            {
                Vector2 starOrigin = star.Size() * 0.5f;
                Color starCol = IsBlack ? new Color(100, 100, 140, 0) : new Color(240, 240, 255, 0);
                float starRot = Projectile.timeLeft * 0.12f;
                sb.Draw(star, screenPos, null, starCol * 0.35f * pulse, starRot, starOrigin, baseScale * 0.4f, SpriteEffects.None, 0f);
            }

            // Empowered: rainbow outer ring
            if (IsEmpowered && radial != null)
            {
                float hue = (Projectile.timeLeft * 0.02f) % 1f;
                Color rainbow = Main.hslToRgb(hue, 0.85f, 0.8f);
                Vector2 srOrigin = radial.Size() * 0.5f;
                sb.Draw(radial, screenPos, null, new Color(rainbow.R, rainbow.G, rainbow.B, 0) * 0.3f, 0f, srOrigin, baseScale * 3.5f, SpriteEffects.None, 0f);
            }
        }

        #endregion

        public override void OnKill(int timeLeft)
        {
            // Death VFX — dual-polarity spark burst
            for (int i = 0; i < 4; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2CircularEdge(3f, 3f);
                bool isBlack = Main.rand.NextBool();
                Color col = isBlack ? new Color(30, 30, 45) : new Color(240, 240, 255);
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.RainbowTorch, sparkVel, 0, col, 0.3f);
                d.noGravity = true;
            }

            try { SwanLakeVFXLibrary.SpawnMusicNotes(Projectile.Center, 1, 12f, 0.5f, 0.7f, 20); } catch { }
            try { SwanLakeVFXLibrary.SpawnFeatherDrift(Projectile.Center, 2, 15f, 0.2f); } catch { }
        }
    }
}
