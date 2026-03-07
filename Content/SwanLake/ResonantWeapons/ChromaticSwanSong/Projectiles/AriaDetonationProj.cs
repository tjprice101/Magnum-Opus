using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Graphics.Shaders;
using ReLogic.Content;
using MagnumOpus.Content.SwanLake.ResonantWeapons.ChromaticSwanSong.Utilities;
using MagnumOpus.Content.SwanLake.ResonantWeapons.ChromaticSwanSong.Shaders;
using MagnumOpus.Content.SwanLake.Debuffs;
using MagnumOpus.Content.SwanLake;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Core;

namespace MagnumOpus.Content.SwanLake.ResonantWeapons.ChromaticSwanSong.Projectiles
{
    /// <summary>
    /// Aria Detonation — structured 3-ring explosion triggered on every Chromatic Bolt impact.
    /// Modes: 0=Normal Aria, 1=Harmonic Release, 2=Opus Detonation (all 7 chromatic colors).
    /// ai[1] encoding: scalePosition + (dyingBreath ? 100 : 0).
    /// Foundation-pattern rendering: safe SpriteBatch, MagnumTextureRegistry textures.
    /// </summary>
    public class AriaDetonationProj : ModProjectile
    {
        private float AriaMode => Projectile.ai[0];
        private bool IsHarmonicRelease => AriaMode >= 1f;
        private bool IsOpusDetonation => AriaMode >= 2f;
        private int ScalePosition => (int)(Projectile.ai[1] % 100f);
        private bool IsDyingBreath => Projectile.ai[1] >= 100f;
        private int _ticksAlive;

        private float BaseMaxRadius => IsOpusDetonation ? 300f : (IsHarmonicRelease ? 200f : 120f);
        private float MaxRadius => BaseMaxRadius * (IsDyingBreath ? 1.5f : 1f);

        public override string Texture => "MagnumOpus/Content/SwanLake/ResonantWeapons/ChromaticSwanSong/ChromaticSwanSong";

        public override void SetDefaults()
        {
            Projectile.width = 120;
            Projectile.height = 120;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.timeLeft = IsOpusDetonation ? 30 : 20;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 6;
            Projectile.alpha = 0;
        }

        public override void AI()
        {
            _ticksAlive++;
            float lifetime = IsOpusDetonation ? 30f : 20f;

            if (_ticksAlive == 1)
            {
                if (IsOpusDetonation)
                    SoundEngine.PlaySound(SoundID.Item14 with { Pitch = 0.3f, Volume = 1.0f }, Projectile.Center);
                else
                    SoundEngine.PlaySound(SoundID.Item14 with { Pitch = 0.8f, Volume = 0.8f }, Projectile.Center);

                int scalePos = ScalePosition;
                Color midColor = ChromaticSwanPlayer.GetScaleColor(scalePos);

                // Radial shard burst — vanilla Dust
                int shardCount = IsOpusDetonation ? 36 : (IsHarmonicRelease ? 24 : 12);
                for (int i = 0; i < shardCount; i++)
                {
                    float angle = MathHelper.TwoPi * i / shardCount;
                    float speed = Main.rand.NextFloat(3f, 8f) * (IsOpusDetonation ? 2f : (IsHarmonicRelease ? 1.5f : 1f));
                    Color shardCol = IsOpusDetonation
                        ? ChromaticSwanPlayer.GetScaleColor(i % 7)
                        : (i % 3 == 0 ? Color.White : midColor);

                    Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * speed;
                    Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.RainbowTorch, vel, 0, shardCol, Main.rand.NextFloat(0.6f, 1.2f));
                    d.noGravity = true;
                }

                // Harmonic notes — vanilla Dust rising
                int noteCount = IsOpusDetonation ? 14 : 8;
                for (int i = 0; i < noteCount; i++)
                {
                    Color noteCol = IsOpusDetonation
                        ? ChromaticSwanPlayer.GetScaleColor(i % 7)
                        : ChromaticSwanUtils.GetChromatic(Main.rand.NextFloat());
                    Vector2 noteVel = new Vector2(Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-3f, -1f));
                    Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(20f, 20f),
                        DustID.RainbowTorch, noteVel, 0, noteCol, Main.rand.NextFloat(0.5f, 1.0f));
                    d.noGravity = true;
                }

                // Dying Breath: black feather burst
                if (IsDyingBreath)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(15f, 15f),
                            DustID.Smoke, Main.rand.NextVector2Circular(4f, 4f), 180, Color.Black, 1.0f);
                        d.noGravity = true;
                    }
                }

                // VFX Library calls
                int featherCount = IsOpusDetonation ? 8 : (IsHarmonicRelease ? 6 : 3);
                try { SwanLakeVFXLibrary.SpawnFeatherBurst(Projectile.Center, featherCount, 0.4f); } catch { }

                float sparkRadius = IsOpusDetonation ? 50f : (IsHarmonicRelease ? 40f : 25f);
                try { SwanLakeVFXLibrary.SpawnPrismaticSparkles(Projectile.Center, IsOpusDetonation ? 12 : (IsHarmonicRelease ? 8 : 4), sparkRadius); } catch { }

                float noteRadius = IsOpusDetonation ? 45f : (IsHarmonicRelease ? 35f : 20f);
                try { SwanLakeVFXLibrary.SpawnMusicNotes(Projectile.Center, IsOpusDetonation ? 8 : (IsHarmonicRelease ? 6 : 3), noteRadius, 0.7f, 1.1f, 30); } catch { }
            }

            // Expanding hitbox
            float expansion = (float)_ticksAlive / lifetime;
            int newSize = (int)MathHelper.Lerp(60f, MaxRadius * 2f, expansion);
            Projectile.Resize(newSize, newSize);

            // Light
            Color lightCol = ChromaticSwanPlayer.GetScaleColor(ScalePosition);
            float lightIntensity = 1f - (float)_ticksAlive / lifetime;
            Lighting.AddLight(Projectile.Center, lightCol.ToVector3() * lightIntensity);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<SwansMark>(), 480);
        }

        #region Rendering (Shader-Driven Spectral Explosion — AriaExplosion.fx Pipeline)

        /// <summary>
        /// OVERHAULED RENDERING PIPELINE:
        /// Pass 1: Noise UV-scrolled rainbow zone (replaces shader radial)
        /// Pass 2: Bloom stacking — core flash + spectral star accents
        /// Visuals scaled down ~65% from original to reduce zone bloat.
        /// </summary>
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            float lifetime = IsOpusDetonation ? 30f : 20f;
            float progress = (float)_ticksAlive / lifetime;
            float alpha = (1f - progress) * (1f - progress);
            if (alpha <= 0.01f) return false;

            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            // Visual radius scaled down by ~65% from original MaxRadius
            float visualRadius = MaxRadius * 0.35f * MathHelper.Lerp(0.3f, 1f, progress);

            int scalePos = ScalePosition;
            Color midColor = ChromaticSwanPlayer.GetScaleColor(scalePos);
            Color outerColor = ChromaticSwanPlayer.GetComplementaryColor(scalePos);

            try
            {
                // ===== NOISE UV-SCROLLED RAINBOW ZONE =====
                DrawNoiseExplosionZone(sb, visualRadius, progress, alpha, midColor);

                // ===== BLOOM STACKING (core flash + star accents) =====
                DrawExplosionBloomStack(sb, drawPos, visualRadius, progress, alpha, midColor, outerColor);
            }
            catch
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

        /// <summary>
        /// Prismatic sparkle impact zone replacing the old noise-scrolled approach.
        /// Uses SwanLakeVFXLibrary.DrawPrismaticSparkleImpact for clean, sparkle-based zones.
        /// </summary>
        private void DrawNoiseExplosionZone(SpriteBatch sb, float visualRadius,
            float progress, float alpha, Color midColor)
        {
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);

            float time = (float)Main.timeForVisualEffects;

            // Primary prismatic sparkle zone (chromatic identity)
            SwanLakeVFXLibrary.DrawPrismaticSparkleImpact(sb, Projectile.Center, visualRadius,
                time, alpha * 0.85f, 12);

            // Harmonic/Opus modes: additional overlapping zone at different phase for richer look
            if (IsHarmonicRelease)
            {
                float secondaryRadius = visualRadius * 0.7f;
                SwanLakeVFXLibrary.DrawPrismaticSparkleImpact(sb, Projectile.Center, secondaryRadius,
                    time + 100f, alpha * 0.4f, 8);
            }

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                DepthStencilState.None, Main.Rasterizer, null,
                Main.GameViewMatrix.TransformationMatrix);
        }

        /// <summary>
        /// Bloom stacking — white-hot core flash + spectral star accents + rainbow orbit dots.
        /// Layered on top of the noise zone for extra visual depth. Uses pixel-radius sizing.
        /// </summary>
        private void DrawExplosionBloomStack(SpriteBatch sb, Vector2 drawPos, float visualRadius,
            float progress, float alpha, Color midColor, Color outerColor)
        {
            Texture2D pointBloom = MagnumTextureRegistry.PointBloom?.Value;
            Texture2D bloom = MagnumTextureRegistry.SoftGlow?.Value;
            Texture2D star = MagnumTextureRegistry.GetStarThin();
            if (bloom == null && pointBloom == null) return;

            // Convert pixel radius to a scale factor for bloom textures
            float baseScale = bloom != null ? visualRadius / (bloom.Width * 0.5f) : visualRadius / 32f;

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);

            // White-hot core flash (strongest at early progress)
            if (pointBloom != null)
            {
                Vector2 pbOrigin = pointBloom.Size() * 0.5f;
                float coreIntensity = Math.Max(0f, 1f - progress * 2.5f);
                float coreScale = IsOpusDetonation ? 0.4f : 0.25f;
                sb.Draw(pointBloom, drawPos, null, new Color(255, 255, 255, 0) * alpha * 0.8f * coreIntensity,
                    0f, pbOrigin, baseScale * coreScale, SpriteEffects.None, 0f);
            }

            // Spectral star accents (rotating, visible in first half)
            if (star != null && progress < 0.5f)
            {
                Vector2 starOrigin = star.Size() * 0.5f;
                float starAlpha = 1f - progress * 2f;
                float starBaseScale = IsOpusDetonation ? 0.65f : (IsHarmonicRelease ? 0.5f : 0.3f);

                // Primary star (note-colored)
                sb.Draw(star, drawPos, null,
                    new Color(midColor.R, midColor.G, midColor.B, 0) * starAlpha * 0.6f * alpha,
                    progress * MathHelper.TwoPi, starOrigin,
                    starBaseScale * (1f + progress), SpriteEffects.None, 0f);

                // Counter-rotating white secondary star
                sb.Draw(star, drawPos, null,
                    new Color(255, 255, 255, 0) * starAlpha * 0.4f * alpha,
                    progress * MathHelper.TwoPi + MathHelper.PiOver4, starOrigin,
                    (starBaseScale - 0.05f) * (1f + progress), SpriteEffects.None, 0f);
            }

            // Opus: 7 chromatic orbiting bloom dots
            if (IsOpusDetonation && bloom != null && progress < 0.7f)
            {
                Vector2 bloomOrigin = bloom.Size() * 0.5f;
                float orbFade = 1f - progress / 0.7f;
                float orbitRadius = baseScale * 50f;
                for (int n = 0; n < 7; n++)
                {
                    Color c = ChromaticSwanPlayer.GetScaleColor(n);
                    float angle = MathHelper.TwoPi * n / 7f + progress * MathHelper.TwoPi * 2f;
                    Vector2 offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * orbitRadius;
                    sb.Draw(bloom, drawPos + offset, null,
                        new Color(c.R, c.G, c.B, 0) * 0.35f * orbFade * alpha,
                        0f, bloomOrigin, 0.12f * baseScale, SpriteEffects.None, 0f);
                }
            }

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                DepthStencilState.None, Main.Rasterizer, null,
                Main.GameViewMatrix.TransformationMatrix);
        }

        #endregion
    }
}
