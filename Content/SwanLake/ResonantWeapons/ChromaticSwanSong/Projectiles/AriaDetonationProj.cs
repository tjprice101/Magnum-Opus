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

        private float BaseMaxRadius => IsOpusDetonation ? 160f : (IsHarmonicRelease ? 110f : 70f);
        private float MaxRadius => BaseMaxRadius * (IsDyingBreath ? 1.3f : 1f);

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
                int shardCount = IsOpusDetonation ? 18 : (IsHarmonicRelease ? 12 : 8);
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
                int noteCount = IsOpusDetonation ? 8 : 5;

                // Mixed rainbow + B&W sparkle explosion (Swan Lake signature)
                float mixedIntensity = IsOpusDetonation ? 1.2f : (IsHarmonicRelease ? 0.9f : 0.6f);
                SwanLakeVFXLibrary.SpawnMixedSparkleImpact(Projectile.Center, mixedIntensity,
                    IsOpusDetonation ? 8 : 5, IsOpusDetonation ? 8 : 5);
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
                float sparkRadius = IsOpusDetonation ? 30f : (IsHarmonicRelease ? 22f : 15f);
                try { SwanLakeVFXLibrary.SpawnPrismaticSparkles(Projectile.Center, IsOpusDetonation ? 6 : (IsHarmonicRelease ? 4 : 2), sparkRadius); } catch { }

                float noteRadius = IsOpusDetonation ? 25f : (IsHarmonicRelease ? 18f : 12f);
                try { SwanLakeVFXLibrary.SpawnMusicNotes(Projectile.Center, IsOpusDetonation ? 4 : (IsHarmonicRelease ? 3 : 2), noteRadius, 0.6f, 0.9f, 25); } catch { }
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
            try
            {
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

            // Primary prismatic sparkle zone (chromatic identity) — reduced count
            SwanLakeVFXLibrary.DrawPrismaticSparkleImpact(sb, Projectile.Center, visualRadius,
                time, alpha * 0.6f, 6);

            // Harmonic/Opus modes: additional overlapping zone at different phase for richer look
            if (IsHarmonicRelease)
            {
                float secondaryRadius = visualRadius * 0.6f;
                SwanLakeVFXLibrary.DrawPrismaticSparkleImpact(sb, Projectile.Center, secondaryRadius,
                    time + 100f, alpha * 0.25f, 4);
            }

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                DepthStencilState.None, Main.Rasterizer, null,
                Main.GameViewMatrix.TransformationMatrix);
        }

        #endregion
    }
}
