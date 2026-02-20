using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Core;
using MagnumOpus.Common.Systems.VFX.Optimization;
using MagnumOpus.Content.MoonlightSonata;

namespace MagnumOpus.Content.MoonlightSonata.VFX.EternalMoon
{
    /// <summary>
    /// Unique VFX for Eternal Moon — the premier Moonlight Sonata melee sword.
    /// Theme: Crescent arcs, lunar phase cycling, constellation connections.
    /// Every swing paints crescent moons across the sky.
    /// </summary>
    public static class EternalMoonVFX
    {
        // === UNIQUE COLOR ACCENTS (layered on top of MoonlightVFXLibrary palette) ===
        private static readonly Color CrescentGold = new Color(255, 240, 180);
        private static readonly Color LunarEclipse = new Color(60, 20, 80);
        private static readonly Color PhaseShimmer = new Color(200, 180, 255);

        /// <summary>
        /// Crescent bloom at the blade tip — draws a crescent-shaped multi-layer glow.
        /// Called every frame during active swing in PreDraw.
        /// </summary>
        public static void DrawCrescentTipBloom(SpriteBatch sb, Vector2 tipWorldPos, float swordRotation, int comboStep, float progression)
        {
            if (sb == null) return;

            Vector2 drawPos = tipWorldPos - Main.screenPosition;
            float pulse = 1f + MathF.Sin(Main.GlobalTimeWrappedHourly * 8f + comboStep) * 0.12f;
            float intensityRamp = MathHelper.Clamp(progression * 3f, 0f, 1f) * MathHelper.Clamp((1f - progression) * 4f, 0f, 1f);
            float baseScale = (0.35f + comboStep * 0.08f) * pulse * intensityRamp;

            if (baseScale < 0.05f) return;

            // Get bloom texture
            var bloomTex = MagnumTextureRegistry.GetSoftGlow();
            if (bloomTex == null) return;
            Vector2 origin = bloomTex.Size() * 0.5f;

            // Crescent offset — slightly offset from center for crescent shape
            Vector2 crescentOffset = new Vector2(MathF.Cos(swordRotation + MathHelper.PiOver2), MathF.Sin(swordRotation + MathHelper.PiOver2)) * 4f;

            // Switch to additive
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // Layer 1: Outer crescent glow (deep purple, large)
            Color outerColor = MoonlightVFXLibrary.DarkPurple with { A = 0 };
            sb.Draw(bloomTex, drawPos + crescentOffset, null, outerColor * 0.25f * intensityRamp, 0f, origin, baseScale * 2.2f, SpriteEffects.None, 0f);

            // Layer 2: Mid crescent (violet)
            Color midColor = MoonlightVFXLibrary.Violet with { A = 0 };
            sb.Draw(bloomTex, drawPos + crescentOffset * 0.5f, null, midColor * 0.40f * intensityRamp, 0f, origin, baseScale * 1.5f, SpriteEffects.None, 0f);

            // Layer 3: Inner glow (ice blue)
            Color innerColor = MoonlightVFXLibrary.IceBlue with { A = 0 };
            sb.Draw(bloomTex, drawPos, null, innerColor * 0.55f * intensityRamp, 0f, origin, baseScale * 1.0f, SpriteEffects.None, 0f);

            // Layer 4: White-hot core with gold tint
            Color coreColor = CrescentGold with { A = 0 };
            sb.Draw(bloomTex, drawPos, null, coreColor * 0.70f * intensityRamp, 0f, origin, baseScale * 0.45f, SpriteEffects.None, 0f);

            // Restore
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        /// <summary>
        /// Lunar phase color cycling — returns a color that cycles through moon phases.
        /// Use this for trail colors to make EternalMoon's trail unique from other Moonlight weapons.
        /// </summary>
        public static Color GetLunarPhaseColor(float progress, int comboStep)
        {
            // Each combo step shifts the hue band slightly
            float phaseOffset = comboStep * 0.08f;
            float t = progress + phaseOffset;

            // Cycle: Deep purple → Violet → Ice blue → Silver → Gold shimmer → back
            if (t < 0.25f)
                return Color.Lerp(MoonlightVFXLibrary.DarkPurple, MoonlightVFXLibrary.Violet, t * 4f);
            else if (t < 0.5f)
                return Color.Lerp(MoonlightVFXLibrary.Violet, MoonlightVFXLibrary.IceBlue, (t - 0.25f) * 4f);
            else if (t < 0.75f)
                return Color.Lerp(MoonlightVFXLibrary.IceBlue, PhaseShimmer, (t - 0.5f) * 4f);
            else
                return Color.Lerp(PhaseShimmer, CrescentGold, (t - 0.75f) * 4f);
        }

        /// <summary>
        /// Per-frame swing VFX unique to EternalMoon.
        /// Spawns crescent dust arcs + orbiting lunar motes along the blade.
        /// </summary>
        public static void SwingFrameEffects(Vector2 ownerCenter, Vector2 tipPos, Vector2 swordDirection, int comboStep, int timer)
        {
            // Dense crescent dust along blade (2 per frame)
            for (int i = 0; i < 2; i++)
            {
                float bladeProgress = Main.rand.NextFloat(0.4f, 1f);
                Vector2 dustPos = Vector2.Lerp(ownerCenter, tipPos, bladeProgress);
                Vector2 perpendicular = new Vector2(-swordDirection.Y, swordDirection.X);
                dustPos += perpendicular * Main.rand.NextFloat(-6f, 6f);

                Color dustColor = GetLunarPhaseColor(bladeProgress, comboStep);
                Dust d = Dust.NewDustPerfect(dustPos, DustID.PurpleTorch, -swordDirection * Main.rand.NextFloat(1f, 3f), 0, dustColor, 1.6f);
                d.noGravity = true;
                d.fadeIn = 1.1f;
            }

            // Contrasting silver sparkle every other frame
            if (timer % 2 == 0)
            {
                Vector2 sparkPos = Vector2.Lerp(ownerCenter, tipPos, Main.rand.NextFloat(0.6f, 1f));
                Dust s = Dust.NewDustPerfect(sparkPos, DustID.Enchanted_Gold, -swordDirection * Main.rand.NextFloat(0.5f, 2f), 0, MoonlightVFXLibrary.Silver, 1.2f);
                s.noGravity = true;
            }

            // Orbiting lunar mote at blade midpoint every 4 frames
            if (timer % 4 == 0)
            {
                float orbitAngle = timer * 0.12f;
                Vector2 midBlade = Vector2.Lerp(ownerCenter, tipPos, 0.6f);
                Vector2 motePos = midBlade + new Vector2(MathF.Cos(orbitAngle), MathF.Sin(orbitAngle)) * 12f;
                MoonlightVFXLibrary.SpawnMusicNotes(motePos, 1, 5f, 0.7f, 0.9f, 30);
            }
        }

        /// <summary>
        /// Impact VFX when EternalMoon hits an enemy — crescent shockwave + constellation burst.
        /// </summary>
        public static void OnHitImpact(Vector2 hitPos, int comboStep, bool isCrit)
        {
            // Base moonlight impact (shared library)
            MoonlightVFXLibrary.MeleeImpact(hitPos, comboStep);

            // UNIQUE: Crescent arc ring — offset halo that looks like a crescent
            float arcScale = 0.4f + comboStep * 0.1f;
            CustomParticles.MoonlightHalo(hitPos + Main.rand.NextVector2Circular(5f, 5f), arcScale);

            // UNIQUE: Phase-colored radial sparkles (not generic flares)
            int sparkCount = 4 + comboStep * 2;
            for (int i = 0; i < sparkCount; i++)
            {
                float angle = MathHelper.TwoPi * i / sparkCount;
                Vector2 sparkVel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f);
                Color sparkColor = GetLunarPhaseColor((float)i / sparkCount, comboStep) with { A = 0 };

                Dust d = Dust.NewDustPerfect(hitPos, DustID.MagicMirror, sparkVel, 0, sparkColor, 1.4f);
                d.noGravity = true;
            }

            // Crit bonus: constellation burst — star points connected by faint lines
            if (isCrit)
            {
                int starCount = 5;
                for (int i = 0; i < starCount; i++)
                {
                    float angle = MathHelper.TwoPi * i / starCount + Main.rand.NextFloat(-0.2f, 0.2f);
                    float dist = Main.rand.NextFloat(20f, 40f);
                    Vector2 starPos = hitPos + angle.ToRotationVector2() * dist;
                    CustomParticles.GenericFlare(starPos, PhaseShimmer, 0.4f, 20);
                }
                // Bright center flash
                CustomParticles.GenericFlare(hitPos, CrescentGold, 0.7f, 15);
            }
        }

        /// <summary>
        /// Finisher slam VFX unique to EternalMoon Phase 3 (CrescendoFinale).
        /// Massive crescent explosion with expanding lunar rings.
        /// </summary>
        public static void CrescendoFinaleVFX(Vector2 pos)
        {
            // Use shared finisher as base
            MoonlightVFXLibrary.FinisherSlam(pos, 1.5f);

            // UNIQUE: Triple expanding crescent rings
            for (int ring = 0; ring < 3; ring++)
            {
                float delay = ring * 0.15f;
                Color ringColor = Color.Lerp(MoonlightVFXLibrary.IceBlue, CrescentGold, ring / 3f);
                CustomParticles.HaloRing(pos, ringColor, 0.5f + ring * 0.2f, 20 + ring * 5);
            }

            // UNIQUE: Lunar phase music note cascade (8 visible notes spiraling outward)
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 noteVel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 5f);
                noteVel.Y -= 1f; // Slight upward drift
                MoonlightVFXLibrary.SpawnMusicNotes(pos, 1, 8f, 0.8f, 1.1f, 40);
            }

            // Screen distortion ripple
            if (AdaptiveQualityManager.Instance?.CurrentQuality >= AdaptiveQualityManager.QualityLevel.Medium)
            {
                MagnumScreenEffects.AddScreenShake(6f);
            }
        }

        /// <summary>
        /// Projectile trail color function for EternalMoon's wave/beam sub-projectiles.
        /// Returns lunar-phase cycling color unique to this weapon.
        /// </summary>
        public static Color WaveTrailColor(float progress)
        {
            Color c = GetLunarPhaseColor(progress, 0);
            return c with { A = 0 } * (1f - progress * 0.7f);
        }

        /// <summary>
        /// Projectile trail width function for EternalMoon sub-projectiles.
        /// Crescent-shaped taper: thin start, wide middle, thin end.
        /// </summary>
        public static float WaveTrailWidth(float progress)
        {
            // Crescent shape: rises quickly, holds, then tapers
            float ramp = MathHelper.Clamp(progress * 5f, 0f, 1f);
            float fade = MathHelper.Clamp((1f - progress) * 3f, 0f, 1f);
            return ramp * fade * 22f;
        }

        /// <summary>
        /// Dynamic lighting for EternalMoon effects.
        /// </summary>
        public static void AddCrescentLight(Vector2 worldPos, float intensity = 0.8f)
        {
            float pulse = 0.85f + MathF.Sin(Main.GlobalTimeWrappedHourly * 6f) * 0.15f;
            Vector3 lightColor = MoonlightVFXLibrary.Violet.ToVector3() * intensity * pulse;
            Lighting.AddLight(worldPos, lightColor);
        }
    }
}
