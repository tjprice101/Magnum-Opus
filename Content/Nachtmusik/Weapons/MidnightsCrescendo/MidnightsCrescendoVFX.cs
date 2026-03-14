using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ReLogic.Content;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Content.Nachtmusik;

namespace MagnumOpus.Content.Nachtmusik.Weapons.MidnightsCrescendo
{
    /// <summary>
    /// VFX helper for Midnight's Crescendo — "A star being born."
    /// 5-tier visual scaling: Pianissimo (0-4) → Piano (5-7) → Mezzo (8-11) → Forte (12-14) → Sforzando (15)
    /// Music notes are integral to every effect — they ARE the VFX, not decoration.
    /// </summary>
    public static class MidnightsCrescendoVFX
    {
        private const int PianoThreshold = 5;
        private const int MezzoThreshold = 8;
        private const int ForteThreshold = 12;
        private const int SforzandoThreshold = 15;
        private const int MaxStacks = 15;

        private static Asset<Texture2D> _glowOrbTex;

        private static Texture2D GlowOrb
        {
            get
            {
                _glowOrbTex ??= ModContent.Request<Texture2D>(
                    "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/GlowOrb",
                    AssetRequestMode.ImmediateLoad);
                return _glowOrbTex.Value;
            }
        }

        #region Hold Item VFX — Constellation of Music Notes

        /// <summary>
        /// Held weapon ambient VFX: constellation of music notes orbiting the player.
        /// Count and brightness scale with crescendo stacks across all 5 tiers.
        /// </summary>
        public static void HoldItemVFX(Player player, float stackProgress, int stacks)
        {
            if (stacks < 1) return;

            float time = (float)Main.timeForVisualEffects * 0.03f;

            // Note count scales per tier
            int noteCount = stacks switch
            {
                < PianoThreshold => 1 + stacks / 2,
                < MezzoThreshold => 3 + (stacks - PianoThreshold),
                < ForteThreshold => 6 + (stacks - MezzoThreshold),
                _ => 10 + (stacks - ForteThreshold)
            };

            float baseRadius = 28f + stackProgress * 22f;

            for (int i = 0; i < noteCount; i++)
            {
                float angle = time * 1.2f + MathHelper.TwoPi * i / noteCount;
                float bob = MathF.Sin(time * 2f + i * 1.7f) * 6f;
                float radius = baseRadius + MathF.Sin(time * 1.5f + i * 2.3f) * 8f;

                Vector2 offset = new Vector2(MathF.Cos(angle) * radius, MathF.Sin(angle) * radius + bob);
                Vector2 pos = player.Center + offset;

                // Spawn music note particles periodically at orbit positions
                if ((int)Main.timeForVisualEffects % 8 == i % 8)
                {
                    Color noteColor = NachtmusikPalette.PaletteLerp(
                        NachtmusikPalette.MidnightsCrescendoBlade,
                        0.3f + stackProgress * 0.5f);
                    CustomParticles.GenericMusicNotes(pos, noteColor, 1, 4f);
                }

                // Continuous glow dust at constellation positions
                if (Main.rand.NextBool(4))
                {
                    Color dustColor = Color.Lerp(
                        NachtmusikPalette.DeepBlue,
                        NachtmusikPalette.StarlitBlue,
                        stackProgress);
                    Dust d = Dust.NewDustPerfect(pos, DustID.PurpleTorch, Vector2.Zero, 0, dustColor,
                        0.4f + stackProgress * 0.3f);
                    d.noGravity = true;
                }
            }

            // Forte+ glow intensification — pulsing starlit light
            if (stacks >= ForteThreshold)
            {
                float pulseIntensity = 0.5f + MathF.Sin(time * 4f) * 0.2f;
                Lighting.AddLight(player.Center,
                    NachtmusikPalette.StarlitBlue.ToVector3() * pulseIntensity * stackProgress);
            }

            // Sforzando — blade aura shimmer (golden accent sparks)
            if (stacks >= SforzandoThreshold && Main.rand.NextBool(2))
            {
                Vector2 sparkPos = player.Center + Main.rand.NextVector2Circular(20f, 20f);
                Dust g = Dust.NewDustPerfect(sparkPos, DustID.Enchanted_Gold,
                    new Vector2(0, -Main.rand.NextFloat(1f, 2.5f)), 0, default, 0.7f);
                g.noGravity = true;
            }
        }

        #endregion

        #region Swing Frame VFX — Per-Frame Dust + Notes Along Blade

        /// <summary>
        /// Called every frame during a swing. Spawns dust and music notes along the blade tip.
        /// Intensity scales with crescendo stacks.
        /// </summary>
        public static void SwingFrameVFX(Vector2 tipPos, Vector2 swordDirection, int comboStep, int stacks)
        {
            float stackProgress = stacks / (float)MaxStacks;

            // Base swing dust — always present (starlit blue sparks at blade tip)
            if (Main.rand.NextBool(2))
            {
                Vector2 dustVel = swordDirection.RotatedByRandom(0.3) * (1f + comboStep * 0.3f);
                Color c = NachtmusikPalette.PaletteLerp(
                    NachtmusikPalette.MidnightsCrescendoBlade,
                    0.2f + stackProgress * 0.6f);
                Dust d = Dust.NewDustPerfect(tipPos, DustID.PurpleTorch, dustVel, 0, c,
                    0.5f + stackProgress * 0.4f + comboStep * 0.05f);
                d.noGravity = true;
            }

            // Piano+ threshold: music notes trail the swing
            if (stacks >= PianoThreshold && Main.rand.NextBool(3))
            {
                Vector2 notePos = tipPos + Main.rand.NextVector2Circular(6f, 6f);
                Color noteColor = Color.Lerp(
                    NachtmusikPalette.StarlitBlue,
                    NachtmusikPalette.TwinklingWhite,
                    stackProgress);
                CustomParticles.GenericMusicNotes(notePos, noteColor, 1, 8f);
            }

            // Mezzo+ threshold: golden sparkle accent dust
            if (stacks >= MezzoThreshold && Main.rand.NextBool(2))
            {
                Vector2 sparkPos = tipPos + Main.rand.NextVector2Circular(10f, 10f);
                Dust s = Dust.NewDustPerfect(sparkPos, DustID.Enchanted_Gold,
                    swordDirection * 0.5f, 0, default, 0.5f + stackProgress * 0.3f);
                s.noGravity = true;
            }

            // Forte+ ascending star sparks (rising embers of stellar birth)
            if (stacks >= ForteThreshold)
            {
                Vector2 sparkVel = new Vector2(Main.rand.NextFloat(-1f, 1f), -Main.rand.NextFloat(1f, 3f));
                Dust star = Dust.NewDustPerfect(tipPos, DustID.SparksMech, sparkVel, 0,
                    NachtmusikPalette.StarGold, 0.6f);
                star.noGravity = true;
            }
        }

        #endregion

        #region Impact VFX — Explosive Musical Stellar Sparkle Explosions

        /// <summary>
        /// On-hit impact VFX. Multi-layered: bloom flash + note chord burst + sparkle explosion + halo ring + dust.
        /// Each hit is a musical chord being struck — notes explode outward from impact.
        /// </summary>
        public static void SwingImpactVFX(Vector2 pos, int comboStep, int stacks)
        {
            float stackProgress = stacks / (float)MaxStacks;
            float intensityMul = 0.6f + stackProgress * 0.6f + comboStep * 0.08f;

            // Core bloom flash
            CustomParticles.GenericFlare(pos,
                Color.Lerp(NachtmusikPalette.StarlitBlue, NachtmusikPalette.TwinklingWhite, stackProgress),
                0.4f * intensityMul, 15);

            // Music note chord burst — notes explode outward from impact point
            int noteCount = 2 + (int)(stacks * 0.5f) + comboStep;
            Color noteColor = NachtmusikPalette.PaletteLerp(
                NachtmusikPalette.MidnightsCrescendoBlade,
                0.4f + stackProgress * 0.4f);
            CustomParticles.GenericMusicNotes(pos, noteColor, noteCount, 15f + stacks);

            // Starlight sparkle explosion
            int sparkCount = 4 + (int)(stacks * 0.6f);
            Color sparkColor = Color.Lerp(NachtmusikPalette.DeepBlue, NachtmusikPalette.StarWhite, stackProgress);
            CustomParticles.ExplosionBurst(pos, sparkColor, sparkCount, 3f + stackProgress * 3f);

            // Halo ring on stronger hits (Mezzo+ or phase 4+)
            if (comboStep >= 4 || stacks >= MezzoThreshold)
            {
                CustomParticles.HaloRing(pos,
                    NachtmusikPalette.Additive(NachtmusikPalette.StarlitBlue),
                    0.3f + stackProgress * 0.3f, 20);
            }

            // Radial dust burst
            int dustCount = 6 + comboStep * 2 + stacks / 2;
            for (int i = 0; i < dustCount; i++)
            {
                float angle = MathHelper.TwoPi * i / dustCount;
                float speed = 2f + Main.rand.NextFloat(2f) + stackProgress * 2f;
                Vector2 vel = angle.ToRotationVector2() * speed;
                Color c = NachtmusikPalette.PaletteLerp(
                    NachtmusikPalette.MidnightsCrescendoBlade,
                    Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(pos, DustID.PurpleTorch, vel, 0, c,
                    0.7f + stackProgress * 0.5f);
                d.noGravity = true;
            }

            // Dynamic lighting on impact
            Lighting.AddLight(pos, NachtmusikPalette.StarWhite.ToVector3() * 0.4f * intensityMul);

            // Forte+ screen shake
            if (stacks >= ForteThreshold)
            {
                float shakeIntensity = 2f + (stacks - ForteThreshold) * 0.5f + (comboStep >= 5 ? 2f : 0f);
                try { MagnumScreenEffects.AddScreenShake(shakeIntensity); } catch { }
            }
        }

        #endregion

        #region Wave Release VFX — When Crescent Wave Fires

        /// <summary>
        /// VFX at wave release point when crescent wave projectile spawns (8+ stacks).
        /// Flash + musical note spray + expanding halo ring.
        /// </summary>
        public static void WaveReleaseVFX(Vector2 pos, float intensity)
        {
            // Flash at release point
            CustomParticles.GenericFlare(pos,
                NachtmusikPalette.Additive(NachtmusikPalette.StarWhite),
                0.5f * intensity, 12);

            // Music notes spray outward
            int noteCount = 3 + (int)(intensity * 4);
            CustomParticles.GenericMusicNotes(pos, NachtmusikPalette.StarlitBlue, noteCount, 20f);

            // Halo ring at release
            CustomParticles.HaloRing(pos,
                NachtmusikPalette.Additive(NachtmusikPalette.StarlitBlue),
                0.4f * intensity, 18);

            Lighting.AddLight(pos, NachtmusikPalette.StarWhite.ToVector3() * 0.5f * intensity);
        }

        #endregion

        #region Wave Trail VFX — Per-Frame Crescent Wave Particles

        /// <summary>
        /// Per-frame VFX for the crescent wave projectile as it travels.
        /// Music note dust spawns in its wake, trailing starlight.
        /// </summary>
        public static void WaveTrailVFX(Vector2 pos, Vector2 velocity, float intensity)
        {
            // Music notes in the wave's wake
            if (Main.rand.NextBool(2))
            {
                Vector2 notePos = pos + Main.rand.NextVector2Circular(12f, 12f);
                Color noteColor = Color.Lerp(NachtmusikPalette.StarlitBlue, NachtmusikPalette.TwinklingWhite, intensity);
                CustomParticles.GenericMusicNotes(notePos, noteColor, 1, 10f);
            }

            // Starlight dust particles trailing
            if (Main.rand.NextBool(2))
            {
                Vector2 dustVel = -velocity.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(1f, 3f)
                    + Main.rand.NextVector2Circular(1f, 1f);
                Dust d = Dust.NewDustPerfect(pos + Main.rand.NextVector2Circular(8f, 8f),
                    DustID.PurpleTorch, dustVel, 0,
                    Color.Lerp(NachtmusikPalette.DeepBlue, NachtmusikPalette.StarWhite, intensity),
                    0.5f + intensity * 0.4f);
                d.noGravity = true;
            }

            // Golden shimmer accents at higher intensity
            if (intensity > 0.6f && Main.rand.NextBool(3))
            {
                Dust g = Dust.NewDustPerfect(pos + Main.rand.NextVector2Circular(6f, 6f),
                    DustID.Enchanted_Gold, -velocity * 0.1f, 0, default, 0.4f);
                g.noGravity = true;
            }

            Lighting.AddLight(pos, NachtmusikPalette.StarlitBlue.ToVector3() * 0.3f * intensity);
        }

        #endregion

        #region Wave Impact VFX — When Crescent Wave Hits

        /// <summary>
        /// Impact VFX when the crescent wave hits an enemy.
        /// Burst of notes + starlight + expanding ring.
        /// </summary>
        public static void WaveImpactVFX(Vector2 pos, float intensity)
        {
            // Bloom flash
            CustomParticles.GenericFlare(pos,
                Color.Lerp(NachtmusikPalette.StarlitBlue, NachtmusikPalette.TwinklingWhite, intensity),
                0.5f * intensity, 12);

            // Music note explosion
            CustomParticles.GenericMusicNotes(pos,
                NachtmusikPalette.StarlitBlue, 4 + (int)(intensity * 6), 18f);

            // Sparkle burst
            CustomParticles.ExplosionBurst(pos,
                Color.Lerp(NachtmusikPalette.DeepBlue, NachtmusikPalette.StarWhite, intensity),
                8 + (int)(intensity * 8), 4f + intensity * 3f);

            // Expanding halo ring
            CustomParticles.HaloRing(pos,
                NachtmusikPalette.Additive(NachtmusikPalette.StarlitBlue),
                0.4f + intensity * 0.3f, 20);

            // Dust burst
            for (int i = 0; i < 10; i++)
            {
                float angle = MathHelper.TwoPi * i / 10;
                Vector2 vel = angle.ToRotationVector2() * (3f + intensity * 3f);
                Dust d = Dust.NewDustPerfect(pos, DustID.PurpleTorch, vel, 0,
                    NachtmusikPalette.StarWhite, 0.7f);
                d.noGravity = true;
            }

            Lighting.AddLight(pos, NachtmusikPalette.StarWhite.ToVector3() * 0.5f * intensity);
        }

        #endregion

        #region Supernova Ignition — The 15-Stack Moment

        /// <summary>
        /// SUPERNOVA IGNITION at max 15 stacks. The star has been born.
        /// Brief white flash + deep Nachtmusik blue screen tint + all orbiting notes burst outward
        /// + blade permanently blazes white-blue until stacks decay.
        /// </summary>
        public static void SupernovaIgnition(Player player)
        {
            Vector2 pos = player.Center;

            // Massive bloom flash — the star ignites
            CustomParticles.GenericFlare(pos, NachtmusikPalette.TwinklingWhite, 1.2f, 8);
            CustomParticles.GenericFlare(pos, NachtmusikPalette.StarlitBlue, 0.8f, 12);

            // Explosion of music notes outward — the constellation shatters into pure energy
            for (int i = 0; i < 20; i++)
            {
                float angle = MathHelper.TwoPi * i / 20f;
                Vector2 vel = angle.ToRotationVector2() * (6f + Main.rand.NextFloat(4f));
                Color c = Color.Lerp(NachtmusikPalette.StarlitBlue, NachtmusikPalette.TwinklingWhite,
                    Main.rand.NextFloat());
                CustomParticles.GenericMusicNotes(pos + vel * 2f, c, 1, 12f);
            }

            // Starburst explosion
            CustomParticles.ExplosionBurst(pos, NachtmusikPalette.StarWhite, 20, 8f);

            // Halo rings cascade — 3 expanding rings
            for (int i = 0; i < 3; i++)
            {
                CustomParticles.HaloRing(pos,
                    Color.Lerp(NachtmusikPalette.StarlitBlue, NachtmusikPalette.TwinklingWhite, i / 2f),
                    0.5f + i * 0.3f, 20 + i * 8);
            }

            // Screen shake — stellar birth
            try { MagnumScreenEffects.AddScreenShake(8f); } catch { }

            // Radial dust explosion — 30 particles outward
            for (int i = 0; i < 30; i++)
            {
                float angle = MathHelper.TwoPi * i / 30f;
                float speed = 4f + Main.rand.NextFloat(6f);
                Vector2 vel = angle.ToRotationVector2() * speed;
                Dust d = Dust.NewDustPerfect(pos, DustID.PurpleTorch, vel, 0,
                    NachtmusikPalette.TwinklingWhite, 1.2f);
                d.noGravity = true;
            }

            // Gold accent sparks
            for (int i = 0; i < 12; i++)
            {
                Vector2 vel = Main.rand.NextVector2CircularEdge(5f, 5f);
                Dust g = Dust.NewDustPerfect(pos, DustID.Enchanted_Gold, vel, 0, default, 1f);
                g.noGravity = true;
            }

            Lighting.AddLight(pos, NachtmusikPalette.TwinklingWhite.ToVector3() * 1.2f);
        }

        #endregion

        #region Draw Helpers — Bloom Stack for Custom VFX

        /// <summary>
        /// Draws a crescendo-themed bloom stack behind the blade tip during swings.
        /// Intensity scales with stack progress — dim at 0, blazing at 15.
        /// </summary>
        public static void DrawCrescendoBloom(SpriteBatch sb, Vector2 worldPos, float stackProgress, int comboStep)
        {
            if (GlowOrb == null) return;

            Vector2 screenPos = worldPos - Main.screenPosition;
            Vector2 origin = new Vector2(GlowOrb.Width, GlowOrb.Height) * 0.5f;

            float baseScale = 0.15f + stackProgress * 0.25f + comboStep * 0.02f;
            float pulse = 1f + MathF.Sin((float)Main.timeForVisualEffects * 0.08f) * 0.08f * (1f + stackProgress);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // Outer glow — deep blue
            Color outerColor = NachtmusikPalette.DeepBlue with { A = 0 };
            sb.Draw(GlowOrb, screenPos, null, outerColor * (0.15f + stackProgress * 0.2f),
                0f, origin, baseScale * pulse * 2.5f, SpriteEffects.None, 0f);

            // Mid glow — starlit blue
            Color midColor = NachtmusikPalette.StarlitBlue with { A = 0 };
            sb.Draw(GlowOrb, screenPos, null, midColor * (0.2f + stackProgress * 0.25f),
                0f, origin, baseScale * pulse * 1.5f, SpriteEffects.None, 0f);

            // Core — star white (only at Piano+ stacks)
            if (stackProgress > 0.3f)
            {
                Color coreColor = NachtmusikPalette.StarWhite with { A = 0 };
                sb.Draw(GlowOrb, screenPos, null, coreColor * (stackProgress - 0.2f) * 0.4f,
                    0f, origin, baseScale * pulse * 0.8f, SpriteEffects.None, 0f);
            }

            // Sforzando — twinkling white hot center
            if (stackProgress >= 1f)
            {
                Color hotCore = NachtmusikPalette.TwinklingWhite with { A = 0 };
                sb.Draw(GlowOrb, screenPos, null, hotCore * 0.3f,
                    0f, origin, baseScale * pulse * 0.4f, SpriteEffects.None, 0f);
            }

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }

        #endregion
    }
}
