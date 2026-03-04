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
    /// Chromatic Bolt — main projectile for Chromatic Swan Song.
    /// Gentle spiral wobble, rainbow-shifting trail. Color based on Chromatic Scale position.
    /// On impact: triggers Aria Detonation. Foundation-pattern rendering.
    /// ai[0]: 0=normal, 1=harmonic-charged, 2=Opus Detonation.
    /// ai[1]: scale position 0-6 (C-D-E-F-G-A-B).
    /// </summary>
    public class ChromaticBoltProj : ModProjectile
    {
        private float _hueOffset;

        public override string Texture => "MagnumOpus/Content/SwanLake/ResonantWeapons/ChromaticSwanSong/ChromaticSwanSong";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 20;
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

            int scalePos = (int)Projectile.ai[1];

            // Gentle spiral
            float spiral = (float)Math.Sin(Projectile.timeLeft * 0.2f) * 1.2f;
            Projectile.velocity = Projectile.velocity.RotatedBy(MathHelper.ToRadians(spiral * 0.2f));
            Projectile.rotation = Projectile.velocity.ToRotation();

            // Chromatic trail sparks — vanilla Dust colored by scale note
            if (Main.rand.NextBool(3))
            {
                Color sparkCol = ChromaticSwanPlayer.GetScaleColor(scalePos);
                Dust d = Dust.NewDustPerfect(
                    Projectile.Center + Main.rand.NextVector2Circular(3f, 3f),
                    DustID.RainbowTorch,
                    -Projectile.velocity.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(0.5f, 2f)
                        + Main.rand.NextVector2Circular(1f, 1f),
                    0, sparkCol, Main.rand.NextFloat(0.6f, 1.0f));
                d.noGravity = true;
            }

            // Dying Breath: black feather particles
            Player owner = Main.player[Projectile.owner];
            try
            {
                if (owner.active && owner.ChromaticSwan().DyingBreathActive && Main.rand.NextBool(4))
                {
                    Dust d = Dust.NewDustPerfect(
                        Projectile.Center + Main.rand.NextVector2Circular(6f, 6f),
                        DustID.Smoke,
                        -Projectile.velocity.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(0.3f, 1.5f)
                            + Main.rand.NextVector2Circular(0.8f, 0.8f),
                        180, Color.Black, 0.7f);
                    d.noGravity = true;
                }
            }
            catch { }

            // Light — colored by scale note
            Color lightCol = ChromaticSwanPlayer.GetScaleColor(scalePos);
            Lighting.AddLight(Projectile.Center, lightCol.ToVector3() * 0.4f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<SwansMark>(), 300);

            Player owner = Main.player[Projectile.owner];
            ChromaticSwanPlayer csp = null;
            try { csp = owner.ChromaticSwan(); } catch { }
            csp?.RegisterHit(target.whoAmI);

            int scalePos = (int)Projectile.ai[1];
            bool isOpus = Projectile.ai[0] >= 2f;
            bool isHarmonic = Projectile.ai[0] >= 1f;
            bool dyingBreath = csp?.DyingBreathActive ?? false;

            // Aria Detonation on EVERY hit
            float ariaMode = isOpus ? 2f : (isHarmonic ? 1f : 0f);
            float ariaDmgMult = isOpus ? 3f : (isHarmonic ? 2f : 0.5f);
            int ariaDmg = (int)(Projectile.damage * ariaDmgMult);
            float ariaAi1 = scalePos + (dyingBreath ? 100f : 0f);

            Projectile.NewProjectile(Projectile.GetSource_OnHit(target, "AriaDetonation"),
                target.Center, Vector2.Zero, ModContent.ProjectileType<AriaDetonationProj>(),
                ariaDmg, 10f, Projectile.owner, ai0: ariaMode, ai1: ariaAi1);

            if (isHarmonic && !isOpus && csp != null && csp.HarmonicStack >= 5)
                csp.ConsumeHarmonicStack();

            if (isOpus)
                SoundEngine.PlaySound(SoundID.Item29 with { Pitch = 0.2f, Volume = 1.0f }, target.Center);
            else
                SoundEngine.PlaySound(SoundID.Item29 with { Pitch = 0.6f, Volume = 0.7f }, target.Center);

            // Hit sparks — vanilla Dust colored by scale note
            Color noteColor = ChromaticSwanPlayer.GetScaleColor(scalePos);
            for (int i = 0; i < 6; i++)
            {
                Dust d = Dust.NewDustPerfect(
                    target.Center + Main.rand.NextVector2Circular(10f, 10f),
                    DustID.RainbowTorch,
                    Main.rand.NextVector2Circular(5f, 5f),
                    0, noteColor, Main.rand.NextFloat(0.5f, 1.0f));
                d.noGravity = true;
            }

            try { SwanLakeVFXLibrary.SpawnMusicNotes(target.Center, 2, 14f, 0.5f, 0.9f, 24); } catch { }
            try { SwanLakeVFXLibrary.SpawnPrismaticSparkles(target.Center, 3, 12f); } catch { }
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 8; i++)
            {
                Color col = ChromaticSwanUtils.GetChromatic(Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.RainbowTorch,
                    Main.rand.NextVector2Circular(4f, 4f), 0, col, Main.rand.NextFloat(0.4f, 0.8f));
                d.noGravity = true;
            }

            try { SwanLakeVFXLibrary.SpawnMusicNotes(Projectile.Center, 3, 18f, 0.6f, 0.9f, 26); } catch { }
            try { SwanLakeVFXLibrary.SpawnFeatherDrift(Projectile.Center, 2, 0.35f); } catch { }
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

                DrawChromaticTrail(sb);
                DrawBloom(sb);
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

            // Theme accents (additive)
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);
            ChromaticSwanUtils.DrawThemeAccents(sb, Projectile.Center, 1f, 0.6f);
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }

        private void DrawChromaticTrail(SpriteBatch sb)
        {
            Texture2D bloom = MagnumTextureRegistry.GetSoftGlow();
            if (bloom == null) return;

            Vector2 origin = bloom.Size() * 0.5f;

            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;

                float t = (float)i / Projectile.oldPos.Length;
                Vector2 drawPos = Projectile.oldPos[i] + Projectile.Size * 0.5f - Main.screenPosition;
                float trailAlpha = (1f - t) * 0.7f * ((255 - Projectile.alpha) / 255f);
                float trailScale = MathHelper.Lerp(0.18f, 0.04f, t);

                // Chrome spectrum color shifting along trail
                Color spectrumCol = ChromaticSwanUtils.GetSpectrumColor(t + _hueOffset);
                sb.Draw(bloom, drawPos, null, new Color(spectrumCol.R, spectrumCol.G, spectrumCol.B, 0) * trailAlpha,
                    0f, origin, trailScale * 2f, SpriteEffects.None, 0f);

                // White core
                sb.Draw(bloom, drawPos, null, new Color(255, 255, 255, 0) * trailAlpha * 0.4f,
                    0f, origin, trailScale * 0.6f, SpriteEffects.None, 0f);
            }
        }

        private void DrawBloom(SpriteBatch sb)
        {
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float alpha = (255 - Projectile.alpha) / 255f;
            Color col = ChromaticSwanUtils.GetChromatic(_hueOffset);
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.2f) * 0.1f + 0.9f;

            Texture2D radial = MagnumTextureRegistry.GetRadialBloom();
            Texture2D point = MagnumTextureRegistry.GetPointBloom();
            Texture2D star = MagnumTextureRegistry.GetStar4Hard();

            // Layer 1: Outer chromatic halo
            if (radial != null)
            {
                Vector2 srOrigin = radial.Size() * 0.5f;
                sb.Draw(radial, drawPos, null, new Color(col.R, col.G, col.B, 0) * 0.35f * alpha,
                    0f, srOrigin, 0.40f * pulse, SpriteEffects.None, 0f);

                // Layer 2: Mid-spectrum glow shifted hue
                Color shifted = ChromaticSwanUtils.GetChromatic(_hueOffset + 0.33f);
                sb.Draw(radial, drawPos, null, new Color(shifted.R, shifted.G, shifted.B, 0) * 0.25f * alpha,
                    0f, srOrigin, 0.25f * pulse, SpriteEffects.None, 0f);
            }

            // Layer 3: White-hot core
            if (point != null)
            {
                Vector2 pbOrigin = point.Size() * 0.5f;
                sb.Draw(point, drawPos, null, new Color(255, 255, 255, 0) * 0.55f * alpha,
                    0f, pbOrigin, 0.12f * pulse, SpriteEffects.None, 0f);
            }

            // Layer 4: Rotating rainbow star accent
            if (star != null)
            {
                Vector2 starOrigin = star.Size() * 0.5f;
                Color rainbow = ChromaticSwanUtils.GetSpectrumColor(_hueOffset);
                sb.Draw(star, drawPos, null, new Color(rainbow.R, rainbow.G, rainbow.B, 0) * 0.30f * alpha,
                    _hueOffset * 3f, starOrigin, 0.18f * pulse, SpriteEffects.None, 0f);
            }
        }

        #endregion
    }
}
