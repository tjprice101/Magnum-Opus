using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Core;
using MagnumOpus.Content.MoonlightSonata;

namespace MagnumOpus.Content.MoonlightSonata.VFX.ResurrectionOfTheMoon
{
    /// <summary>
    /// Unique VFX for Resurrection of the Moon — the ranged sniper rifle.
    /// Theme: Heavy astronomical impact, comet trails, crater detonations, moonrise flash.
    /// Every shot is a falling star; every ricochet burns brighter.
    /// </summary>
    public static class ResurrectionVFX
    {
        // === UNIQUE COLOR ACCENTS ===
        private static readonly Color CometCore = new Color(255, 230, 200);
        private static readonly Color CometTrail = new Color(180, 120, 255);
        private static readonly Color ImpactCrater = new Color(100, 80, 200);
        private static readonly Color MoonriseGold = new Color(255, 210, 150);
        private static readonly Color DeepSpaceViolet = new Color(50, 20, 100);

        /// <summary>
        /// Massive muzzle flash — the gun firing should light up the area.
        /// Called from Shoot() in the weapon item.
        /// </summary>
        public static void MuzzleFlash(Vector2 firePos, Vector2 direction)
        {
            // Flash cascade — 3 layers outward
            CustomParticles.GenericFlare(firePos, Color.White, 0.8f, 12);
            CustomParticles.GenericFlare(firePos, MoonriseGold, 0.65f, 15);
            CustomParticles.GenericFlare(firePos, CometTrail, 0.5f, 18);

            // Directional blast cone
            for (int i = 0; i < 10; i++)
            {
                Vector2 blastVel = direction.RotatedByRandom(0.35f) * Main.rand.NextFloat(5f, 12f);
                Color blastColor = Color.Lerp(CometCore, MoonlightVFXLibrary.IceBlue, Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(firePos, DustID.MagicMirror, blastVel, 0, blastColor, 1.5f);
                d.noGravity = true;
                d.fadeIn = 1.2f;
            }

            // Perpendicular "barrel flash" sparks
            Vector2 perp = new Vector2(-direction.Y, direction.X);
            for (int side = -1; side <= 1; side += 2)
            {
                for (int j = 0; j < 3; j++)
                {
                    Vector2 sparkVel = perp * side * Main.rand.NextFloat(2f, 5f) + direction * Main.rand.NextFloat(1f, 3f);
                    Dust s = Dust.NewDustPerfect(firePos, DustID.Enchanted_Gold, sparkVel, 0, MoonriseGold, 1.1f);
                    s.noGravity = true;
                }
            }

            // Expanding halo at muzzle
            CustomParticles.MoonlightHalo(firePos, 0.5f);

            // Lighting flash
            Lighting.AddLight(firePos, CometCore.ToVector3() * 1.5f);
        }

        /// <summary>
        /// Comet trail — dense, heavy trail befitting a sniper round.
        /// Called every frame in projectile AI().
        /// </summary>
        public static void CometTrailFrame(Vector2 projCenter, Vector2 velocity, int ricochetCount)
        {
            float bounceIntensity = 1f + ricochetCount * 0.35f;

            // Dense comet dust (2 per frame) — heavy smoky trail
            for (int i = 0; i < 2; i++)
            {
                Vector2 dustVel = -velocity * 0.12f + Main.rand.NextVector2Circular(1f, 1f);
                Color trailColor = Color.Lerp(CometTrail, MoonlightVFXLibrary.IceBlue, Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(projCenter + Main.rand.NextVector2Circular(4f, 4f), DustID.PurpleTorch, dustVel, 0, trailColor, 1.8f * bounceIntensity);
                d.noGravity = true;
                d.fadeIn = 1.3f;
            }

            // Comet ember sparks — golden hot particles
            if (Main.rand.NextBool(2))
            {
                Vector2 emberVel = -velocity * 0.2f + Main.rand.NextVector2Circular(2f, 2f);
                Dust e = Dust.NewDustPerfect(projCenter, DustID.Enchanted_Gold, emberVel, 0, MoonriseGold, 1.2f * bounceIntensity);
                e.noGravity = true;
            }

            // Orbiting music notes locked to projectile (every 8 frames)
            if (Main.rand.NextBool(8))
            {
                float orbitAngle = Main.GameUpdateCount * 0.1f;
                for (int i = 0; i < 2; i++)
                {
                    float noteAngle = orbitAngle + MathHelper.Pi * i;
                    Vector2 notePos = projCenter + noteAngle.ToRotationVector2() * 12f;
                    MoonlightVFXLibrary.SpawnMusicNotes(notePos, 1, 3f, 0.7f, 0.85f, 25);
                }
            }

            // Dynamic lighting — comet glow
            Lighting.AddLight(projCenter, Color.Lerp(CometTrail, CometCore, 0.3f).ToVector3() * (0.7f + ricochetCount * 0.15f));
        }

        /// <summary>
        /// Ricochet VFX — each bounce is a mini-explosion that burns brighter.
        /// </summary>
        public static void OnRicochetVFX(Vector2 bouncePos, Vector2 inVel, Vector2 outVel, int ricochetCount)
        {
            float intensity = 0.8f + ricochetCount * 0.4f;

            // Central crater flash
            CustomParticles.GenericFlare(bouncePos, Color.White, 0.6f * intensity, 12);
            CustomParticles.GenericFlare(bouncePos, MoonriseGold, 0.5f * intensity, 15);

            // Crater ring
            CustomParticles.MoonlightHalo(bouncePos, 0.4f * intensity);
            CustomParticles.HaloRing(bouncePos, ImpactCrater, 0.3f * intensity, 18);

            // Ricochet spark spray — directional toward outgoing velocity
            int sparkCount = 6 + ricochetCount * 3;
            for (int i = 0; i < sparkCount; i++)
            {
                Vector2 sparkVel = outVel.SafeNormalize(Vector2.Zero).RotatedByRandom(0.8f) * Main.rand.NextFloat(3f, 8f) * intensity;
                Color sparkColor = Color.Lerp(CometCore, CometTrail, Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(bouncePos, DustID.MagicMirror, sparkVel, 0, sparkColor, 1.3f);
                d.noGravity = true;
            }

            // Music notes on bounce
            MoonlightVFXLibrary.SpawnMusicNotes(bouncePos, 2 + ricochetCount, 12f, 0.75f, 1.0f, 30);

            Lighting.AddLight(bouncePos, CometCore.ToVector3() * intensity);
        }

        /// <summary>
        /// Final impact — crater detonation when the bullet hits an enemy or dies.
        /// </summary>
        public static void CraterDetonation(Vector2 impactPos, int totalRicochets)
        {
            float intensity = 1f + totalRicochets * 0.25f;

            // Use shared projectile impact as base
            MoonlightVFXLibrary.ProjectileImpact(impactPos, intensity);

            // UNIQUE: Radial moonbeam lances — 6-point star of light beams
            int lanceCount = 6;
            for (int i = 0; i < lanceCount; i++)
            {
                float angle = MathHelper.TwoPi * i / lanceCount + Main.rand.NextFloat(-0.1f, 0.1f);
                // Multiple dust per lance for beam look
                for (int j = 0; j < 4; j++)
                {
                    float dist = 8f + j * 10f;
                    Vector2 lancePos = impactPos + angle.ToRotationVector2() * dist;
                    Color lanceColor = Color.Lerp(CometCore, MoonlightVFXLibrary.IceBlue, (float)j / 4f);
                    Dust d = Dust.NewDustPerfect(lancePos, DustID.Enchanted_Gold, angle.ToRotationVector2() * (3f + j * 2f), 0, lanceColor, 1.4f - j * 0.15f);
                    d.noGravity = true;
                }
            }

            // UNIQUE: Crater ring cascade (3 rings expanding)
            for (int ring = 0; ring < 3; ring++)
            {
                Color craterColor = Color.Lerp(ImpactCrater, DeepSpaceViolet, ring / 3f);
                CustomParticles.HaloRing(impactPos, craterColor, 0.4f + ring * 0.2f, 20 + ring * 5);
            }

            // Music note cascade from crater
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 noteVel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f);
                MoonlightVFXLibrary.SpawnMusicNotes(impactPos, 1, 8f, 0.85f, 1.1f, 35);
            }
        }

        /// <summary>
        /// PreDraw bloom for the sniper projectile — comet-core 4-layer bloom.
        /// </summary>
        public static void DrawProjectileBloom(SpriteBatch sb, Vector2 projWorldPos, float velocityMagnitude, int ricochetCount)
        {
            if (sb == null) return;

            Vector2 drawPos = projWorldPos - Main.screenPosition;
            float speed = MathHelper.Clamp(velocityMagnitude / 20f, 0.5f, 1.5f);
            float pulse = 1f + MathF.Sin(Main.GlobalTimeWrappedHourly * 8f) * 0.08f;
            float bounceScale = 1f + ricochetCount * 0.2f;

            var bloomTex = MagnumTextureRegistry.GetSoftGlow();
            if (bloomTex == null) return;
            Vector2 origin = bloomTex.Size() * 0.5f;

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // Layer 1: Outer comet tail glow
            Color outer = CometTrail with { A = 0 };
            sb.Draw(bloomTex, drawPos, null, outer * 0.25f, 0f, origin, 0.7f * speed * bounceScale * pulse, SpriteEffects.None, 0f);

            // Layer 2: Mid purple
            Color mid = MoonlightVFXLibrary.Violet with { A = 0 };
            sb.Draw(bloomTex, drawPos, null, mid * 0.40f, 0f, origin, 0.45f * speed * bounceScale * pulse, SpriteEffects.None, 0f);

            // Layer 3: Inner gold
            Color inner = MoonriseGold with { A = 0 };
            sb.Draw(bloomTex, drawPos, null, inner * 0.55f, 0f, origin, 0.25f * speed * bounceScale * pulse, SpriteEffects.None, 0f);

            // Layer 4: White-hot core
            sb.Draw(bloomTex, drawPos, null, Color.White with { A = 0 } * 0.75f, 0f, origin, 0.12f * speed * bounceScale * pulse, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }
    }
}
