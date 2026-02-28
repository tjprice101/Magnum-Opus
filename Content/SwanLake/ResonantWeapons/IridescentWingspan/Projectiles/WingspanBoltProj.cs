using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Graphics.Shaders;
using MagnumOpus.Content.SwanLake.ResonantWeapons.IridescentWingspan.Utilities;
using MagnumOpus.Content.SwanLake.ResonantWeapons.IridescentWingspan.Shaders;
using MagnumOpus.Content.SwanLake.ResonantWeapons.IridescentWingspan.Particles;
using MagnumOpus.Content.SwanLake.ResonantWeapons.IridescentWingspan.Primitives;
using MagnumOpus.Content.SwanLake.Debuffs;
using ReLogic.Content;

namespace MagnumOpus.Content.SwanLake.ResonantWeapons.IridescentWingspan.Projectiles
{
    /// <summary>
    /// Wingspan Bolt — spectral wing-shaped projectile for Iridescent Wingspan.
    /// 
    /// BEHAVIOR:
    /// • Normal bolt: wing-shaped, penetrate=2, leaves ethereal trail with prismatic edge
    /// • Empowered bolt (ai[0] >= 1): 3× larger, penetrate=5, passes through tiles,
    ///   leaves lingering feathers, massive bloom
    /// • On hit: registers wing charge on owner's WingspanPlayer
    /// • Applies FlameOfTheSwan for 4 seconds
    /// </summary>
    public class WingspanBoltProj : ModProjectile
    {
        private const int TrailLength = 18;
        private Vector2[] _trail = new Vector2[TrailLength];
        private WingspanPrimitiveRenderer _renderer;

        private bool IsEmpowered => Projectile.ai[0] >= 1f;

        public override string Texture => "MagnumOpus/Content/SwanLake/ResonantWeapons/IridescentWingspan/IridescentWingspan";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = TrailLength;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 180;
            Projectile.extraUpdates = 1;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.alpha = 255;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
        }

        public override void AI()
        {
            // Apply empowered modifiers on first tick
            if (Projectile.localAI[0] == 0 && IsEmpowered)
            {
                Projectile.penetrate = 5;
                Projectile.tileCollide = false;
                Projectile.width = 30;
                Projectile.height = 30;
                Projectile.localAI[0] = 1;
            }

            Projectile.alpha = Math.Max(Projectile.alpha - 30, 0);

            // Trail update
            for (int i = TrailLength - 1; i > 0; i--) _trail[i] = _trail[i - 1];
            _trail[0] = Projectile.Center;

            Projectile.rotation = Projectile.velocity.ToRotation();

            // Trail particles
            if (Main.rand.NextBool(IsEmpowered ? 1 : 3))
            {
                var spark = new WingSparkParticle();
                spark.Initialize(
                    Projectile.Center + Main.rand.NextVector2Circular(4f, 4f),
                    -Projectile.velocity.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(0.5f, 2f)
                        + Main.rand.NextVector2Circular(1f, 1f),
                    WingspanUtils.GetWingGradient(Main.rand.NextFloat()),
                    Main.rand.NextFloat(0.4f, 0.8f) * (IsEmpowered ? 1.5f : 1f)
                );
                WingspanParticleHandler.Spawn(spark);
            }

            // Empowered: extra feathers floating off
            if (IsEmpowered && Main.rand.NextBool(3))
            {
                var feather = new EtherealFeatherParticle();
                feather.Initialize(
                    Projectile.Center + Main.rand.NextVector2Circular(10f, 10f),
                    Main.rand.NextVector2Circular(1f, 1f),
                    WingspanUtils.EtherealWhite,
                    Main.rand.NextFloat(0.3f, 0.6f)
                );
                WingspanParticleHandler.Spawn(feather);
            }

            // Light
            float lightMult = IsEmpowered ? 0.7f : 0.35f;
            Lighting.AddLight(Projectile.Center, WingspanUtils.EtherealWhite.ToVector3() * lightMult);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<FlameOfTheSwan>(), 240);

            Player owner = Main.player[Projectile.owner];
            owner.Wingspan().RegisterHit();

            // Impact sparks
            int sparkCount = IsEmpowered ? 10 : 5;
            for (int i = 0; i < sparkCount; i++)
            {
                var spark = new WingSparkParticle();
                spark.Initialize(
                    target.Center + Main.rand.NextVector2Circular(10f, 10f),
                    Main.rand.NextVector2Circular(4f, 4f),
                    WingspanUtils.GetPrismaticEdge(Main.rand.NextFloat(MathHelper.TwoPi)),
                    Main.rand.NextFloat(0.5f, 1.0f)
                );
                WingspanParticleHandler.Spawn(spark);
            }

            // Empowered: feather burst on hit
            if (IsEmpowered)
            {
                for (int i = 0; i < 4; i++)
                {
                    var feather = new EtherealFeatherParticle();
                    feather.Initialize(
                        target.Center + Main.rand.NextVector2Circular(15f, 15f),
                        Main.rand.NextVector2Circular(2f, 2f) + new Vector2(0, -1f),
                        WingspanUtils.EtherealWhite,
                        Main.rand.NextFloat(0.5f, 0.9f)
                    );
                    WingspanParticleHandler.Spawn(feather);
                }
            }

            // Music notes on wing bolt impact — ethereal chimes
            SwanLakeVFXLibrary.SpawnMusicNotes(target.Center, IsEmpowered ? 3 : 1, 15f, 0.5f, 0.9f, 25);

            // Prismatic sparkles for iridescent flair
            SwanLakeVFXLibrary.SpawnPrismaticSparkles(target.Center, IsEmpowered ? 5 : 2, 12f);
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 8; i++)
            {
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.WhiteTorch,
                    Main.rand.NextVector2Circular(3f, 3f), 0,
                    WingspanUtils.EtherealWhite, 0.9f);
                d.noGravity = true;
            }

            if (IsEmpowered)
            {
                var burst = new WingBurstParticle();
                burst.Initialize(Projectile.Center, Vector2.Zero, WingspanUtils.WingGold, 0.8f);
                burst.Rotation = Projectile.rotation;
                WingspanParticleHandler.Spawn(burst);
            }

            // Death VFX — music notes and feathers scatter into the ether
            SwanLakeVFXLibrary.SpawnMusicNotes(Projectile.Center, 2, 18f, 0.6f, 0.9f, 24);
            SwanLakeVFXLibrary.SpawnFeatherDrift(Projectile.Center, 3, 15f, 0.25f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;

            DrawWingTrail(sb);
            DrawBloom(sb);
            WingspanParticleHandler.DrawAllParticles(sb);

            return false;
        }

        private void DrawWingTrail(SpriteBatch sb)
        {
            var valid = new List<Vector2>();
            foreach (var p in _trail) if (p != Vector2.Zero) valid.Add(p);
            if (valid.Count < 3) return;

            _renderer ??= new WingspanPrimitiveRenderer();

            float baseWidth = IsEmpowered ? 28f : 14f;
            var settings = new WingspanTrailSettings(
                t => baseWidth * (1f - t * 0.7f),
                t => {
                    Color wing = WingspanUtils.GetWingGradient(t);
                    Color edge = WingspanUtils.GetPrismaticEdge(t * MathHelper.TwoPi);
                    return Color.Lerp(wing, edge, t * 0.5f) * (1f - t * 0.5f);
                },
                WingspanShaderLoader.HasWingspanFlareTrailShader ? GameShaders.Misc["MagnumOpus:WingspanFlareTrail"] : null
            );

            sb.End();
            sb.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.PointClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            _renderer.RenderTrail(valid.ToArray(), settings, 16);

            // Bloom underlayer
            var bloomSettings = new WingspanTrailSettings(
                t => baseWidth * 2.5f * (1f - t * 0.8f),
                t => WingspanUtils.EtherealWhite * 0.15f * (1f - t),
                WingspanShaderLoader.HasWingspanFlareTrailShader ? GameShaders.Misc["MagnumOpus:WingspanFlareTrail"] : null
            );
            _renderer.RenderTrail(valid.ToArray(), bloomSettings, 16);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        private void DrawBloom(SpriteBatch sb)
        {
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float alpha = (255 - Projectile.alpha) / 255f;
            float scale = IsEmpowered ? 0.5f : 0.25f;
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.15f) * 0.1f + 0.9f;

            // Load VFX Asset Library bloom textures
            Texture2D softRadial = null;
            Texture2D pointBloom = null;
            Texture2D starAccent = null;
            try
            {
                softRadial = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom",
                    AssetRequestMode.ImmediateLoad)?.Value;
                pointBloom = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom",
                    AssetRequestMode.ImmediateLoad)?.Value;
                starAccent = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles Asset Library/Stars/ThinTall4PointedStar",
                    AssetRequestMode.ImmediateLoad)?.Value;
            }
            catch { }

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // Layer 1: Wide ethereal halo (SoftRadialBloom)
            if (softRadial != null)
            {
                Vector2 srOrigin = new Vector2(softRadial.Width / 2f, softRadial.Height / 2f);
                sb.Draw(softRadial, drawPos, null, new Color(WingspanUtils.SpectralBlue.R, WingspanUtils.SpectralBlue.G, WingspanUtils.SpectralBlue.B, 0) * 0.25f * alpha * pulse,
                    0f, srOrigin, scale * 1.8f * pulse, SpriteEffects.None, 0f);

                // Layer 2: Mid glow ring
                Color midGlow = Color.Lerp(WingspanUtils.SpectralBlue, WingspanUtils.EtherealWhite, 0.4f);
                sb.Draw(softRadial, drawPos, null, new Color(midGlow.R, midGlow.G, midGlow.B, 0) * 0.40f * alpha * pulse,
                    0f, srOrigin, scale * 1.0f * pulse, SpriteEffects.None, 0f);
            }

            // Layer 3: Concentrated white-hot core (PointBloom)
            if (pointBloom != null)
            {
                Vector2 pbOrigin = new Vector2(pointBloom.Width / 2f, pointBloom.Height / 2f);
                sb.Draw(pointBloom, drawPos, null, new Color(255, 255, 255, 0) * 0.60f * alpha * pulse,
                    0f, pbOrigin, scale * 0.4f * pulse, SpriteEffects.None, 0f);
            }

            // Layer 4: Ethereal star accent — slowly rotating
            if (starAccent != null)
            {
                Vector2 starOrigin = new Vector2(starAccent.Width / 2f, starAccent.Height / 2f);
                Color prismatic = WingspanUtils.GetPrismaticEdge(Main.GameUpdateCount * 0.05f);
                sb.Draw(starAccent, drawPos, null, new Color(prismatic.R, prismatic.G, prismatic.B, 0) * 0.3f * alpha,
                    Main.GameUpdateCount * 0.06f, starOrigin, scale * 0.35f, SpriteEffects.None, 0f);
            }

            // Empowered: golden aura layer
            if (IsEmpowered && softRadial != null)
            {
                Vector2 srOrigin = new Vector2(softRadial.Width / 2f, softRadial.Height / 2f);
                sb.Draw(softRadial, drawPos, null, new Color(WingspanUtils.WingGold.R, WingspanUtils.WingGold.G, WingspanUtils.WingGold.B, 0) * 0.20f * alpha * pulse,
                    0f, srOrigin, scale * 2.5f * pulse, SpriteEffects.None, 0f);
            }

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }
    }
}
