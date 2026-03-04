using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.SwanLake.ResonantWeapons.ChromaticSwanSong.Utilities;
using MagnumOpus.Content.SwanLake.Debuffs;
using MagnumOpus.Content.SwanLake;
using MagnumOpus.Common.Systems.VFX;

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

        #region Rendering (Foundation Pattern)

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            float lifetime = IsOpusDetonation ? 30f : 20f;
            float progress = (float)_ticksAlive / lifetime;
            float alpha = (1f - progress) * (1f - progress);

            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float baseScale = MathHelper.Lerp(0.3f, MaxRadius / 80f, progress);

            int scalePos = ScalePosition;
            Color midColor = ChromaticSwanPlayer.GetScaleColor(scalePos);
            Color outerColor = ChromaticSwanPlayer.GetComplementaryColor(scalePos);

            Texture2D radial = MagnumTextureRegistry.GetRadialBloom();
            Texture2D point = MagnumTextureRegistry.GetPointBloom();
            Texture2D star = MagnumTextureRegistry.GetStarThin();

            try
            {
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                if (radial != null)
                {
                    Vector2 srOrigin = radial.Size() * 0.5f;

                    if (IsOpusDetonation)
                    {
                        // All 7 chromatic rings as stacked bloom layers
                        for (int note = 0; note < 7; note++)
                        {
                            Color noteCol = ChromaticSwanPlayer.GetScaleColor(note);
                            float ringScale = baseScale * (0.4f + note * 0.1f);
                            float ringAlpha = alpha * (0.35f - note * 0.03f);
                            sb.Draw(radial, drawPos, null,
                                new Color(noteCol.R, noteCol.G, noteCol.B, 0) * ringAlpha,
                                0f, srOrigin, ringScale, SpriteEffects.None, 0f);
                        }
                    }
                    else
                    {
                        // 3 distinct ring layers
                        sb.Draw(radial, drawPos, null,
                            new Color(outerColor.R, outerColor.G, outerColor.B, 0) * alpha * 0.35f,
                            0f, srOrigin, baseScale * 1.2f, SpriteEffects.None, 0f);

                        sb.Draw(radial, drawPos, null,
                            new Color(midColor.R, midColor.G, midColor.B, 0) * alpha * 0.4f,
                            0f, srOrigin, baseScale * 0.7f, SpriteEffects.None, 0f);

                        sb.Draw(radial, drawPos, null,
                            new Color(255, 255, 255, 0) * alpha * 0.5f,
                            0f, srOrigin, baseScale * 0.35f, SpriteEffects.None, 0f);
                    }
                }

                // White-hot core flash
                if (point != null)
                {
                    Vector2 pbOrigin = point.Size() * 0.5f;
                    float coreScale = IsOpusDetonation ? 0.35f : 0.25f;
                    sb.Draw(point, drawPos, null, new Color(255, 255, 255, 0) * alpha * 0.7f,
                        0f, pbOrigin, baseScale * coreScale, SpriteEffects.None, 0f);
                }

                // Radiating star at detonation apex
                if (star != null && progress < 0.5f)
                {
                    Vector2 starOrigin = star.Size() * 0.5f;
                    float starAlpha = 1f - progress * 2f;
                    float starBaseScale = IsOpusDetonation ? 0.6f : (IsHarmonicRelease ? 0.5f : 0.3f);

                    sb.Draw(star, drawPos, null,
                        new Color(midColor.R, midColor.G, midColor.B, 0) * starAlpha * 0.6f,
                        progress * MathHelper.TwoPi, starOrigin,
                        starBaseScale * (1f + progress), SpriteEffects.None, 0f);

                    sb.Draw(star, drawPos, null,
                        new Color(255, 255, 255, 0) * starAlpha * 0.4f,
                        progress * MathHelper.TwoPi + MathHelper.PiOver4, starOrigin,
                        (starBaseScale - 0.05f) * (1f + progress), SpriteEffects.None, 0f);
                }
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

        #endregion
    }
}
