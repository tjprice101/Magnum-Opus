using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Graphics.Shaders;
using MagnumOpus.Content.SwanLake.ResonantWeapons.CallofthePearlescentLake.Utilities;
using MagnumOpus.Content.SwanLake.ResonantWeapons.CallofthePearlescentLake.Shaders;
using MagnumOpus.Content.SwanLake.ResonantWeapons.CallofthePearlescentLake.Particles;
using MagnumOpus.Content.SwanLake.ResonantWeapons.CallofthePearlescentLake.Primitives;
using MagnumOpus.Content.SwanLake.Debuffs;
using ReLogic.Content;

namespace MagnumOpus.Content.SwanLake.ResonantWeapons.CallofthePearlescentLake.Projectiles
{
    /// <summary>
    /// Pearlescent Rocket — the core projectile for Call of the Pearlescent Lake.
    /// 
    /// BEHAVIOR:
    /// • Travels in a straight line with subtle sine-wave wobble
    /// • Leaves a 3-pass shader-driven trail (bloom underlay → core → overbright halo)
    /// • Tidal rockets (ai[0] ≥ 1) are larger, seek enemies, have 2× explosion radius
    /// • Still Waters rockets (ai[1] ≥ 1) gently home toward nearest enemy
    /// • On impact: concentric water-ripple explosion, pearl droplets, mist clouds
    /// • Applies FlameOfTheSwan debuff for 5 seconds
    /// </summary>
    public class PearlescentRocketProj : ModProjectile
    {
        private const int TrailLength = 24;
        private Vector2[] _trailPositions = new Vector2[TrailLength];
        private float[] _trailRotations = new float[TrailLength];
        private PearlescentPrimitiveRenderer _renderer;

        private bool IsTidal => Projectile.ai[0] >= 1f;
        private bool IsStillWaters => Projectile.ai[1] >= 1f;

        public override string Texture => "MagnumOpus/Content/SwanLake/ResonantWeapons/CallofthePearlescentLake/CallofthePearlescentLake";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = TrailLength;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = IsTidal ? 18 : 12;
            Projectile.height = IsTidal ? 18 : 12;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 240;
            Projectile.extraUpdates = 1;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.alpha = 255;
        }

        public override void AI()
        {
            Projectile.alpha = Math.Max(Projectile.alpha - 25, 0);

            // Update trail
            for (int i = TrailLength - 1; i > 0; i--)
            {
                _trailPositions[i] = _trailPositions[i - 1];
                _trailRotations[i] = _trailRotations[i - 1];
            }
            _trailPositions[0] = Projectile.Center;
            _trailRotations[0] = Projectile.velocity.ToRotation();

            // Subtle sine wobble
            float wobble = (float)Math.Sin(Projectile.timeLeft * 0.15f) * 0.8f;
            Projectile.velocity = Projectile.velocity.RotatedBy(MathHelper.ToRadians(wobble * 0.3f));

            // Homing behavior for Tidal and Still Waters rockets
            if (IsTidal || IsStillWaters)
            {
                float homingRange = IsTidal ? 500f : 300f;
                float homingStrength = IsTidal ? 0.06f : 0.03f;

                NPC target = FindClosestNPC(homingRange);
                if (target != null)
                {
                    Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity.SafeNormalize(Vector2.Zero),
                        toTarget, homingStrength) * Projectile.velocity.Length();
                }
            }

            Projectile.rotation = Projectile.velocity.ToRotation();

            // Trail particles
            if (Main.rand.NextBool(3))
            {
                var droplet = new PearlDropletParticle();
                droplet.Initialize(
                    Projectile.Center + Main.rand.NextVector2Circular(4f, 4f),
                    -Projectile.velocity.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(0.5f, 2f)
                        + Main.rand.NextVector2Circular(1f, 1f),
                    Color.Lerp(PearlescentUtils.PearlWhite, PearlescentUtils.LakeSilver, Main.rand.NextFloat()),
                    Main.rand.NextFloat(0.5f, 1.0f)
                );
                PearlescentParticleHandler.Spawn(droplet);
            }

            // Tidal rocket extra glow
            if (IsTidal)
            {
                Lighting.AddLight(Projectile.Center, PearlescentUtils.PearlWhite.ToVector3() * 0.6f);

                if (Main.rand.NextBool(2))
                {
                    var mist = new LakeMistParticle();
                    mist.Initialize(
                        Projectile.Center + Main.rand.NextVector2Circular(6f, 6f),
                        Main.rand.NextVector2Circular(0.8f, 0.8f),
                        PearlescentUtils.MistBlue,
                        Main.rand.NextFloat(0.6f, 1.2f)
                    );
                    PearlescentParticleHandler.Spawn(mist);
                }
            }
            else
            {
                Lighting.AddLight(Projectile.Center, PearlescentUtils.LakeSilver.ToVector3() * 0.3f);
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<FlameOfTheSwan>(), 300); // 5 seconds

            // Pearlescent music notes — lake's melodic ripple
            SwanLakeVFXLibrary.SpawnMusicNotes(target.Center, IsTidal ? 3 : 1, 14f, 0.5f, 0.8f, 22);

            // Prismatic sparkle accents on impact
            SwanLakeVFXLibrary.SpawnPrismaticSparkles(target.Center, IsTidal ? 4 : 2, 10f);
        }

        public override void OnKill(int timeLeft)
        {
            // Sound
            SoundEngine.PlaySound(SoundID.Item14 with { Pitch = 0.5f, Volume = 0.6f }, Projectile.Center);

            float explosionRadius = IsTidal ? 80f : 45f;

            // Concentric ripple rings
            int ringCount = IsTidal ? 4 : 2;
            for (int r = 0; r < ringCount; r++)
            {
                var ripple = new RippleRingParticle();
                ripple.Initialize(
                    Projectile.Center,
                    Vector2.Zero,
                    Color.Lerp(PearlescentUtils.PearlWhite, PearlescentUtils.LakeSilver, (float)r / ringCount),
                    0.8f - r * 0.15f
                );
                ripple.Setup(explosionRadius * (0.6f + r * 0.3f));
                PearlescentParticleHandler.Spawn(ripple);
            }

            // Pearl droplet burst
            int dropletCount = IsTidal ? 16 : 8;
            for (int i = 0; i < dropletCount; i++)
            {
                float angle = MathHelper.TwoPi * i / dropletCount + Main.rand.NextFloat(-0.2f, 0.2f);
                float speed = Main.rand.NextFloat(2f, 6f) * (IsTidal ? 1.5f : 1f);
                var droplet = new PearlDropletParticle();
                droplet.Initialize(
                    Projectile.Center,
                    new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * speed,
                    PearlescentUtils.RipplePalette[Main.rand.Next(PearlescentUtils.RipplePalette.Length)],
                    Main.rand.NextFloat(0.7f, 1.3f)
                );
                PearlescentParticleHandler.Spawn(droplet);
            }

            // Mist clouds
            for (int i = 0; i < 5; i++)
            {
                var mist = new LakeMistParticle();
                mist.Initialize(
                    Projectile.Center + Main.rand.NextVector2Circular(15f, 15f),
                    Main.rand.NextVector2Circular(2f, 2f),
                    Color.Lerp(PearlescentUtils.MistBlue, PearlescentUtils.PearlWhite, Main.rand.NextFloat()),
                    Main.rand.NextFloat(0.8f, 1.5f)
                );
                PearlescentParticleHandler.Spawn(mist);
            }

            // Prismatic feather shards
            if (IsTidal)
            {
                for (int i = 0; i < 6; i++)
                {
                    var feather = new PrismaticFeatherParticle();
                    feather.Initialize(
                        Projectile.Center + Main.rand.NextVector2Circular(10f, 10f),
                        Main.rand.NextVector2Circular(3f, 3f) + new Vector2(0, -1f),
                        PearlescentUtils.PearlWhite,
                        Main.rand.NextFloat(0.6f, 1.1f)
                    );
                    feather.Setup();
                    PearlescentParticleHandler.Spawn(feather);
                }
            }

            // Vanilla dust for fullness
            for (int i = 0; i < 10; i++)
            {
                Dust d = Dust.NewDustPerfect(Projectile.Center,
                    DustID.WhiteTorch,
                    Main.rand.NextVector2Circular(4f, 4f), 0,
                    PearlescentUtils.PearlWhite, 1.2f);
                d.noGravity = true;
            }

            // VFX Library: Music notes cascading from the lake's shattered surface
            SwanLakeVFXLibrary.SpawnMusicNotes(Projectile.Center, IsTidal ? 5 : 3,
                explosionRadius * 0.5f, 0.6f, 1.0f, 28);

            // VFX Library: Feather burst — the lake weeps
            SwanLakeVFXLibrary.SpawnFeatherBurst(Projectile.Center, IsTidal ? 4 : 2, 0.35f);

            // VFX Library: Prismatic sparkles dancing across the ripples
            SwanLakeVFXLibrary.SpawnPrismaticSparkles(Projectile.Center, IsTidal ? 6 : 3,
                explosionRadius * 0.4f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;

            // === LAYER 1: Shader-driven primitive trail ===
            DrawShaderTrail(sb);

            // === LAYER 2: Bloom glow core ===
            DrawBloomCore(sb);

            // === LAYER 3: Projectile sprite ===
            DrawRocketSprite(sb, lightColor);

            // === LAYER 4: Per-weapon particles ===
            PearlescentParticleHandler.DrawAllParticles(sb);

            return false;
        }

        private void DrawShaderTrail(SpriteBatch sb)
        {
            // Build valid trail array
            var validPositions = new List<Vector2>();
            foreach (var pos in _trailPositions)
                if (pos != Vector2.Zero) validPositions.Add(pos);
            if (validPositions.Count < 3) return;

            _renderer ??= new PearlescentPrimitiveRenderer();

            float baseWidth = IsTidal ? 22f : 14f;

            // 3-pass rendering (like the shader comments suggest):
            // Pass 1 — soft bloom underlay at 3× width
            var bloomSettings = new PearlescentTrailSettings(
                t => baseWidth * 3f * (1f - t * 0.7f),
                t => Color.Lerp(PearlescentUtils.LakeSilver, PearlescentUtils.DeepLake, t) * 0.25f,
                PearlescentShaderLoader.HasRocketTrailShader ? GameShaders.Misc["MagnumOpus:PearlescentRocketTrail"] : null,
                smoothen: true
            );

            sb.End();
            sb.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.PointClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            if (PearlescentShaderLoader.HasRocketTrailShader)
            {
                var shader = GameShaders.Misc["MagnumOpus:PearlescentRocketTrail"];
                shader.UseColor(PearlescentUtils.PearlWhite);
                shader.UseSecondaryColor(PearlescentUtils.LakeSilver);
                shader.UseSaturation((float)Main.GameUpdateCount * 0.02f);
            }
            _renderer.RenderTrail(validPositions.ToArray(), bloomSettings, 20);

            // Pass 2 — core trail at 1× width
            var coreSettings = new PearlescentTrailSettings(
                t => baseWidth * (1f - t * 0.6f),
                t => Color.Lerp(PearlescentUtils.PearlWhite, PearlescentUtils.LakeSilver, t) * (1f - t * 0.5f),
                PearlescentShaderLoader.HasRocketTrailShader ? GameShaders.Misc["MagnumOpus:PearlescentRocketTrail"] : null
            );
            _renderer.RenderTrail(validPositions.ToArray(), coreSettings, 20);

            // Pass 3 — overbright halo at 1.5× width
            var haloSettings = new PearlescentTrailSettings(
                t => baseWidth * 1.5f * (1f - t * 0.8f),
                t => PearlescentUtils.PearlWhite * 0.15f * (1f - t),
                PearlescentShaderLoader.HasRocketTrailShader ? GameShaders.Misc["MagnumOpus:PearlescentRocketTrail"] : null
            );
            _renderer.RenderTrail(validPositions.ToArray(), haloSettings, 20);

            // Restore
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        private void DrawBloomCore(SpriteBatch sb)
        {
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.15f) * 0.1f + 0.9f;
            float baseScale = (IsTidal ? 0.6f : 0.35f) * pulse;
            float alpha = (255 - Projectile.alpha) / 255f;

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
                starAccent = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles Asset Library/Stars/4PointedStarSoft",
                    AssetRequestMode.ImmediateLoad)?.Value;
            }
            catch { }

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // Layer 1: Outer pearlescent halo (SoftRadialBloom)
            if (softRadial != null)
            {
                Vector2 srOrigin = new Vector2(softRadial.Width / 2f, softRadial.Height / 2f);
                sb.Draw(softRadial, drawPos, null, new Color(PearlescentUtils.LakeSilver.R, PearlescentUtils.LakeSilver.G,
                    PearlescentUtils.LakeSilver.B, 0) * 0.35f * alpha, 0f, srOrigin, baseScale * 1.8f, SpriteEffects.None, 0f);
            }

            // Layer 2: Mid pearl glow (SoftRadialBloom)
            if (softRadial != null)
            {
                Vector2 srOrigin = new Vector2(softRadial.Width / 2f, softRadial.Height / 2f);
                sb.Draw(softRadial, drawPos, null, new Color(PearlescentUtils.PearlWhite.R, PearlescentUtils.PearlWhite.G,
                    PearlescentUtils.PearlWhite.B, 0) * 0.30f * alpha, 0f, srOrigin, baseScale, SpriteEffects.None, 0f);
            }

            // Layer 3: Inner white-hot core (PointBloom)
            if (pointBloom != null)
            {
                Vector2 pbOrigin = new Vector2(pointBloom.Width / 2f, pointBloom.Height / 2f);
                sb.Draw(pointBloom, drawPos, null, new Color(255, 255, 255, 0) * 0.60f * alpha,
                    0f, pbOrigin, baseScale * 0.4f, SpriteEffects.None, 0f);
            }

            // Layer 4: Soft rotating star — lake reflection glimmer (4PointedStarSoft)
            if (starAccent != null)
            {
                Vector2 starOrigin = new Vector2(starAccent.Width / 2f, starAccent.Height / 2f);
                float starRot = Main.GameUpdateCount * 0.03f;
                Color shimmer = Color.Lerp(PearlescentUtils.PearlWhite, PearlescentUtils.MistBlue, pulse);
                sb.Draw(starAccent, drawPos, null, new Color(shimmer.R, shimmer.G, shimmer.B, 0) * 0.25f * alpha,
                    starRot, starOrigin, baseScale * 0.8f, SpriteEffects.None, 0f);
            }

            // Layer 5: Tidal extra bloom ring
            if (IsTidal && softRadial != null)
            {
                Vector2 srOrigin = new Vector2(softRadial.Width / 2f, softRadial.Height / 2f);
                sb.Draw(softRadial, drawPos, null, new Color(PearlescentUtils.PearlWhite.R, PearlescentUtils.PearlWhite.G,
                    PearlescentUtils.PearlWhite.B, 0) * 0.25f * alpha, 0f, srOrigin, baseScale * 2.5f, SpriteEffects.None, 0f);
            }

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        private void DrawRocketSprite(SpriteBatch sb, Color lightColor)
        {
            // Draw a small glowing orb as the rocket body (no separate texture needed)
            Texture2D tex = Terraria.GameContent.TextureAssets.Extra[174].Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = tex.Size() * 0.5f;
            float alpha = (255 - Projectile.alpha) / 255f;
            float scale = IsTidal ? 0.25f : 0.18f;

            Color body = Color.Lerp(PearlescentUtils.PearlWhite, PearlescentUtils.LakeSilver,
                (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.5f + 0.5f);

            sb.Draw(tex, drawPos, null, body * alpha, Projectile.rotation, origin, scale, SpriteEffects.None, 0f);
        }

        private NPC FindClosestNPC(float maxRange)
        {
            NPC closest = null;
            float bestDist = maxRange;
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
    }
}
