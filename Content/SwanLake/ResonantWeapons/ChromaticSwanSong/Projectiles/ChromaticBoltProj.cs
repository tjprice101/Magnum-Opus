using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Graphics.Shaders;
using MagnumOpus.Content.SwanLake.ResonantWeapons.ChromaticSwanSong.Utilities;
using MagnumOpus.Content.SwanLake.ResonantWeapons.ChromaticSwanSong.Shaders;
using MagnumOpus.Content.SwanLake.ResonantWeapons.ChromaticSwanSong.Particles;
using MagnumOpus.Content.SwanLake.ResonantWeapons.ChromaticSwanSong.Primitives;
using MagnumOpus.Content.SwanLake.Debuffs;
using ReLogic.Content;

namespace MagnumOpus.Content.SwanLake.ResonantWeapons.ChromaticSwanSong.Projectiles
{
    /// <summary>
    /// Chromatic Bolt — main projectile for Chromatic Swan Song.
    /// 
    /// BEHAVIOR:
    /// • Travels in a straight line with a gentle spiral wobble
    /// • Leaves a rainbow-shifting shader trail
    /// • On hit: registers hit on player's ChromaticSwanPlayer
    /// • If 3rd consecutive hit on same target → spawn AriaDetonationProj
    /// • Applies FlameOfTheSwan for 4 seconds
    /// </summary>
    public class ChromaticBoltProj : ModProjectile
    {
        private const int TrailLength = 20;
        private Vector2[] _trail = new Vector2[TrailLength];
        private ChromaticPrimitiveRenderer _renderer;
        private float _hueOffset;

        public override string Texture => "MagnumOpus/Content/SwanLake/ResonantWeapons/ChromaticSwanSong/ChromaticSwanSong";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = TrailLength;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 180;
            Projectile.extraUpdates = 1;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.alpha = 255;
        }

        public override void AI()
        {
            Projectile.alpha = Math.Max(Projectile.alpha - 30, 0);
            _hueOffset += 0.02f;

            // Update trail
            for (int i = TrailLength - 1; i > 0; i--)
                _trail[i] = _trail[i - 1];
            _trail[0] = Projectile.Center;

            // Gentle spiral
            float spiral = (float)Math.Sin(Projectile.timeLeft * 0.2f) * 1.2f;
            Projectile.velocity = Projectile.velocity.RotatedBy(MathHelper.ToRadians(spiral * 0.2f));
            Projectile.rotation = Projectile.velocity.ToRotation();

            // Chromatic trail sparks
            if (Main.rand.NextBool(3))
            {
                var spark = new ChromaticSparkParticle();
                spark.Initialize(
                    Projectile.Center + Main.rand.NextVector2Circular(3f, 3f),
                    -Projectile.velocity.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(0.5f, 2f)
                        + Main.rand.NextVector2Circular(1f, 1f),
                    ChromaticSwanUtils.GetChromatic(Main.rand.NextFloat()),
                    Main.rand.NextFloat(0.6f, 1.0f)
                );
                ChromaticParticleHandler.Spawn(spark);
            }

            // Light
            Color lightCol = ChromaticSwanUtils.GetChromatic(0f);
            Lighting.AddLight(Projectile.Center, lightCol.ToVector3() * 0.4f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<FlameOfTheSwan>(), 240); // 4 seconds

            Player owner = Main.player[Projectile.owner];
            var csp = owner.ChromaticSwan();
            csp.RegisterHit(target.whoAmI);

            // Aria Detonation trigger
            if (csp.AriaReady)
            {
                csp.ConsumeAria();

                // Spawn aria detonation at target
                int extraDmg = csp.HarmonicStack >= 5 ? 2 : 1; // Bonus from harmonic stack
                int ariaDmg = (int)(Projectile.damage * 2.5f * extraDmg);
                Projectile.NewProjectile(Projectile.GetSource_OnHit(target, "AriaDetonation"),
                    target.Center, Vector2.Zero, ModContent.ProjectileType<AriaDetonationProj>(),
                    ariaDmg, 10f, Projectile.owner,
                    ai0: csp.HarmonicStack >= 5 ? 1f : 0f);

                if (csp.HarmonicStack >= 5)
                    csp.ConsumeHarmonicStack();

                SoundEngine.PlaySound(SoundID.Item29 with { Pitch = 0.6f, Volume = 0.9f }, target.Center);
            }

            // Hit sparks
            for (int i = 0; i < 6; i++)
            {
                var shard = new PrismaticShardParticle();
                shard.Initialize(
                    target.Center + Main.rand.NextVector2Circular(10f, 10f),
                    Main.rand.NextVector2Circular(5f, 5f),
                    ChromaticSwanUtils.GetChromatic(Main.rand.NextFloat()),
                    Main.rand.NextFloat(0.5f, 1.0f)
                );
                ChromaticParticleHandler.Spawn(shard);
            }

            // Chromatic music notes — harmonic resonance on strike
            SwanLakeVFXLibrary.SpawnMusicNotes(target.Center, 2, 14f, 0.5f, 0.9f, 24);

            // Prismatic sparkle accents
            SwanLakeVFXLibrary.SpawnPrismaticSparkles(target.Center, 3, 12f);
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 8; i++)
            {
                var spark = new ChromaticSparkParticle();
                spark.Initialize(
                    Projectile.Center,
                    Main.rand.NextVector2Circular(4f, 4f),
                    ChromaticSwanUtils.GetChromatic(Main.rand.NextFloat()),
                    Main.rand.NextFloat(0.4f, 0.8f)
                );
                ChromaticParticleHandler.Spawn(spark);
            }

            for (int i = 0; i < 6; i++)
            {
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.RainbowTorch,
                    Main.rand.NextVector2Circular(3f, 3f), 0,
                    ChromaticSwanUtils.GetChromatic(Main.rand.NextFloat()), 1f);
                d.noGravity = true;
            }

            // Chromatic dissolution music notes
            SwanLakeVFXLibrary.SpawnMusicNotes(Projectile.Center, 3, 18f, 0.6f, 0.9f, 26);

            // Feather drift at death — the swan's final whisper
            SwanLakeVFXLibrary.SpawnFeatherDrift(Projectile.Center, 2, 0.35f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;

            // === Shader trail ===
            DrawChromaticTrail(sb);

            // === Bloom core ===
            DrawBloom(sb);

            // === Particles ===
            ChromaticParticleHandler.DrawAllParticles(sb);

            return false;
        }

        private void DrawChromaticTrail(SpriteBatch sb)
        {
            var valid = new List<Vector2>();
            foreach (var p in _trail) if (p != Vector2.Zero) valid.Add(p);
            if (valid.Count < 3) return;

            _renderer ??= new ChromaticPrimitiveRenderer();

            var settings = new ChromaticTrailSettings(
                t => MathHelper.Lerp(12f, 2f, t),
                t => ChromaticSwanUtils.GetSpectrumColor(t + _hueOffset) * (1f - t * 0.6f),
                ChromaticShaderLoader.HasChromaticTrailShader ? GameShaders.Misc["MagnumOpus:ChromaticTrail"] : null
            );

            sb.End();
            sb.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.PointClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            if (ChromaticShaderLoader.HasChromaticTrailShader)
            {
                var shader = GameShaders.Misc["MagnumOpus:ChromaticTrail"];
                shader.UseColor(Color.White);
                shader.UseSaturation(_hueOffset);
            }
            _renderer.RenderTrail(valid.ToArray(), settings, 18);

            // Bloom underlay trail
            var bloomSettings = new ChromaticTrailSettings(
                t => MathHelper.Lerp(24f, 4f, t),
                t => ChromaticSwanUtils.GetSpectrumColor(t + _hueOffset) * 0.2f * (1f - t),
                ChromaticShaderLoader.HasChromaticTrailShader ? GameShaders.Misc["MagnumOpus:ChromaticTrail"] : null
            );
            _renderer.RenderTrail(valid.ToArray(), bloomSettings, 18);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        private void DrawBloom(SpriteBatch sb)
        {
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float alpha = (255 - Projectile.alpha) / 255f;
            Color col = ChromaticSwanUtils.GetChromatic(0f);
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.2f) * 0.1f + 0.9f;

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
                starAccent = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles Asset Library/Stars/4PointedStarHard",
                    AssetRequestMode.ImmediateLoad)?.Value;
            }
            catch { }

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // Layer 1: Outer chromatic halo (SoftRadialBloom)
            if (softRadial != null)
            {
                Vector2 srOrigin = new Vector2(softRadial.Width / 2f, softRadial.Height / 2f);
                sb.Draw(softRadial, drawPos, null, new Color(col.R, col.G, col.B, 0) * 0.35f * alpha,
                    0f, srOrigin, 0.40f * pulse, SpriteEffects.None, 0f);
            }

            // Layer 2: Mid-spectrum glow (SoftRadialBloom, shifted hue)
            if (softRadial != null)
            {
                Vector2 srOrigin = new Vector2(softRadial.Width / 2f, softRadial.Height / 2f);
                Color shifted = ChromaticSwanUtils.GetChromatic(_hueOffset + 0.33f);
                sb.Draw(softRadial, drawPos, null, new Color(shifted.R, shifted.G, shifted.B, 0) * 0.25f * alpha,
                    0f, srOrigin, 0.25f * pulse, SpriteEffects.None, 0f);
            }

            // Layer 3: Concentrated white-hot core (PointBloom)
            if (pointBloom != null)
            {
                Vector2 pbOrigin = new Vector2(pointBloom.Width / 2f, pointBloom.Height / 2f);
                sb.Draw(pointBloom, drawPos, null, new Color(255, 255, 255, 0) * 0.55f * alpha,
                    0f, pbOrigin, 0.12f * pulse, SpriteEffects.None, 0f);
            }

            // Layer 4: Rotating rainbow star accent (4PointedStarHard)
            if (starAccent != null)
            {
                Vector2 starOrigin = new Vector2(starAccent.Width / 2f, starAccent.Height / 2f);
                Color rainbow = ChromaticSwanUtils.GetSpectrumColor(_hueOffset);
                sb.Draw(starAccent, drawPos, null, new Color(rainbow.R, rainbow.G, rainbow.B, 0) * 0.30f * alpha,
                    _hueOffset * 3f, starOrigin, 0.18f * pulse, SpriteEffects.None, 0f);
            }

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }
    }
}
