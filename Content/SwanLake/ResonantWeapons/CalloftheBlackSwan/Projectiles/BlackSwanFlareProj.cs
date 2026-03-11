using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Graphics.Shaders;
using ReLogic.Content;
using MagnumOpus.Content.SwanLake.ResonantWeapons.CalloftheBlackSwan.Utilities;
using MagnumOpus.Content.SwanLake.ResonantWeapons.CalloftheBlackSwan.Primitives;
using MagnumOpus.Content.SwanLake.ResonantWeapons.CalloftheBlackSwan.Shaders;
using MagnumOpus.Content.SwanLake.Debuffs;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Core;

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

        // GPU primitive trail renderer for shader-driven trail
        private BlackSwanPrimitiveRenderer _trailRenderer;
        private VertexStrip _strip;

        #endregion

        public override string Texture => "MagnumOpus/Content/SwanLake/ResonantWeapons/CalloftheBlackSwan/CalloftheBlackSwan";

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

            // Swan Lake rainbow sparkles
            IncisorOrbRenderer.SpawnSwanLakeRainbowSparkles(Projectile);
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

            try { SwanLakeVFXLibrary.SpawnMusicNotes(hitPos, 1, 12f, 0.4f, 0.7f, 20); } catch { }
    try { SwanLakeVFXLibrary.SpawnMixedSparkleImpact(hitPos, 0.6f, 4, 4); } catch { }

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

        #region Rendering

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            try
            {
            IncisorOrbRenderer.DrawOrbVisuals(Main.spriteBatch, Projectile, IncisorOrbRenderer.SwanLake, ref _strip);
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

        #endregion

        public override void OnKill(int timeLeft)
        {
            // Dispose GPU trail renderer
            _trailRenderer?.Dispose();
            _trailRenderer = null;

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
            try { SwanLakeVFXLibrary.SpawnMixedSparkleImpact(Projectile.Center, 0.5f, 4, 4); } catch { }
            try { SwanLakeVFXLibrary.SpawnPrismaticSparkles(Projectile.Center, 3, 15f); } catch { }
        }
    }
}
