using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Bloom;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.ClairDeLune.Weapons.Summon
{
    // =============================================================================
    //  LUNAR PHYLACTERY -- VFX
    //  Identity: Crystal soul vessel storing moonlit souls to empower attacks.
    //  Ethereal, accumulating, luminous. Pearl-blue crystal shimmer, dreamy mist,
    //  soul absorption arcs, empowerment aura. Every kill empowers the vessel.
    // =============================================================================

    public static class LunarPhylacteryVFX
    {
        /// <summary>
        /// Ambient VFX when holding the Lunar Phylactery item.
        /// Crystal shimmer, pearl sparkle, moonlit mist drift, soft blue ambient light.
        /// </summary>
        public static void HoldItemVFX(Player player, Item item)
        {
            if (Main.dedServ) return;

            Vector2 center = player.MountedCenter;
            float time = (float)Main.timeForVisualEffects;

            // Crystal shimmer dust — alternating IceTorch and GemDiamond
            if (Main.rand.NextBool(5))
            {
                Vector2 pos = center + Main.rand.NextVector2Circular(18f, 18f);
                int dustType = Main.rand.NextBool() ? DustID.IceTorch : DustID.GemDiamond;
                Color col = ClairDeLunePalette.GetPearlGradient(Main.rand.NextFloat(0.3f, 0.8f));
                Dust d = Dust.NewDustPerfect(pos, dustType, new Vector2(0, -0.4f), 0, col, 0.6f);
                d.noGravity = true;
            }

            // Pearl sparkle accent
            if (Main.rand.NextBool(8))
                ClairDeLuneVFXLibrary.SpawnPearlSparkle(
                    center + Main.rand.NextVector2Circular(15f, 15f), Vector2.UnitY * -0.5f);

            // Moonlit mist drift
            if (Main.rand.NextBool(12))
                ClairDeLuneVFXLibrary.SpawnMoonlitMist(center, 1, 25f, 0.3f);

            // Occasional music note
            if (Main.rand.NextBool(30))
                ClairDeLuneVFXLibrary.SpawnMusicNotes(center, 1, 18f, 0.7f, 0.85f, 30);

            // Soft blue ambient light with gentle pulse
            float pulse = 0.2f + MathF.Sin(time * 0.05f) * 0.05f;
            Lighting.AddLight(center, ClairDeLunePalette.SoftBlue.ToVector3() * pulse);
        }

        /// <summary>
        /// 3-layer bloom with pearl-blue and crystal tints, gentle pulsing.
        /// </summary>
        public static void PreDrawInWorldBloom(
            Microsoft.Xna.Framework.Graphics.SpriteBatch sb,
            Microsoft.Xna.Framework.Graphics.Texture2D tex,
            Vector2 pos, Vector2 origin, float rotation, float scale)
        {
            float time = (float)Main.timeForVisualEffects;
            float pulse = 1f + MathF.Sin(time * 0.04f) * 0.03f;

            // Layer 1: Outer midnight blue aura
            sb.Draw(tex, pos, null, ClairDeLunePalette.Additive(ClairDeLunePalette.MidnightBlue, 0.35f),
                rotation, origin, scale * 1.08f * pulse, Microsoft.Xna.Framework.Graphics.SpriteEffects.None, 0f);
            // Layer 2: Middle pearl blue crystal glow
            sb.Draw(tex, pos, null, ClairDeLunePalette.Additive(ClairDeLunePalette.PearlBlue, 0.28f),
                rotation, origin, scale * 1.04f * pulse, Microsoft.Xna.Framework.Graphics.SpriteEffects.None, 0f);
            // Layer 3: Inner pearl white crystal core
            sb.Draw(tex, pos, null, ClairDeLunePalette.Additive(ClairDeLunePalette.PearlWhite, 0.22f),
                rotation, origin, scale * 1.01f * pulse, Microsoft.Xna.Framework.Graphics.SpriteEffects.None, 0f);
        }

        /// <summary>
        /// Crystal soul vessel materialization -- pearl burst, converging mist,
        /// crystal radial burst, music notes.
        /// </summary>
        public static void SummonVFX(Vector2 spawnPos)
        {
            if (Main.dedServ) return;

            // Pearl burst radiating outward
            ClairDeLuneVFXLibrary.SpawnPearlBurst(spawnPos, 10, 5f, 0.3f);

            // Converging moonlit mist spiraling inward
            ClairDeLuneVFXLibrary.SpawnConvergingMist(spawnPos, 8, 60f, 0.55f);

            // Crystal radial dust burst (IceTorch + GemDiamond alternating)
            for (int i = 0; i < 14; i++)
            {
                float angle = MathHelper.TwoPi * i / 14f;
                float progress = (float)i / 14f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 5f);
                int dustType = i % 2 == 0 ? DustID.IceTorch : DustID.GemDiamond;
                Color col = ClairDeLunePalette.GetPearlGradient(progress);
                Dust d = Dust.NewDustPerfect(spawnPos, dustType, vel, 0, col, 1.2f);
                d.noGravity = true;
            }

            // Music note shower
            ClairDeLuneVFXLibrary.SpawnMusicNotes(spawnPos, 6, 25f, 0.8f, 1.0f, 35);

            // Bloom flash
            ClairDeLuneVFXLibrary.DrawBloom(spawnPos, 0.5f);

            Lighting.AddLight(spawnPos, ClairDeLunePalette.PearlBlue.ToVector3() * 0.9f);
        }

        /// <summary>
        /// Per-frame ambient VFX for the crystal soul vessel minion.
        /// Crystal soul glow with orbiting IceTorch and GemDiamond dust, pearl shimmer,
        /// occasional music notes, moonlit mist.
        /// </summary>
        public static void MinionAmbientVFX(Vector2 center)
        {
            if (Main.dedServ) return;

            float time = (float)Main.timeForVisualEffects;

            // Orbiting crystal dust ring (IceTorch + GemDiamond)
            if (Main.rand.NextBool(3))
            {
                float orbitAngle = time * 0.04f + Main.rand.NextFloat(MathHelper.TwoPi);
                float orbitRadius = 14f + MathF.Sin(time * 0.06f) * 3f;
                Vector2 orbitPos = center + orbitAngle.ToRotationVector2() * orbitRadius;
                int dustType = Main.rand.NextBool() ? DustID.IceTorch : DustID.GemDiamond;
                Color col = ClairDeLunePalette.GetPearlGradient(
                    0.3f + MathF.Sin(time * 0.03f) * 0.3f + 0.3f);
                Dust d = Dust.NewDustPerfect(orbitPos, dustType, Vector2.Zero, 0, col, 0.7f);
                d.noGravity = true;
            }

            // Pearl shimmer sparkle
            if (Main.rand.NextBool(8))
                ClairDeLuneVFXLibrary.SpawnPearlShimmer(center, 1, 18f, 0.2f);

            // Occasional music note
            if (Main.rand.NextBool(20))
                ClairDeLuneVFXLibrary.SpawnMusicNotes(center, 1, 12f, 0.7f, 0.85f, 25);

            // Sparse moonlit mist
            if (Main.rand.NextBool(15))
                ClairDeLuneVFXLibrary.SpawnMoonlitMist(center, 1, 20f, 0.3f);

            // Pulsing crystal soul glow
            float pulse = 0.4f + MathF.Sin(time * 0.06f) * 0.1f;
            Lighting.AddLight(center, ClairDeLunePalette.PearlBlue.ToVector3() * pulse);
        }

        /// <summary>
        /// Temporal beam launch VFX -- crystal dust spray, bloom, music notes.
        /// </summary>
        public static void SoulBeamFireVFX(Vector2 origin, Vector2 direction)
        {
            if (Main.dedServ) return;

            // Crystal dust spray in firing direction
            for (int i = 0; i < 5; i++)
            {
                Vector2 vel = direction * Main.rand.NextFloat(3f, 6f)
                    + Main.rand.NextVector2Circular(0.8f, 0.8f);
                float progress = (float)i / 5f;
                Color col = ClairDeLunePalette.GetPearlGradient(progress);
                int dustType = Main.rand.NextBool() ? DustID.IceTorch : DustID.GemDiamond;
                Dust d = Dust.NewDustPerfect(origin, dustType, vel, 0, col, 1.1f);
                d.noGravity = true;
            }

            // Bloom at launch point
            ClairDeLuneVFXLibrary.DrawBloom(origin, 0.35f);

            // Music notes
            ClairDeLuneVFXLibrary.SpawnMusicNotes(origin, 2, 12f, 0.7f, 0.9f, 22);

            Lighting.AddLight(origin, ClairDeLunePalette.PearlBlue.ToVector3() * 0.7f);
        }

        /// <summary>
        /// Pearl-blue crystal dust trail with starlight sparkle for the soul beam projectile.
        /// </summary>
        public static void SoulBeamTrailVFX(Vector2 pos, Vector2 velocity)
        {
            if (Main.dedServ) return;

            // Trailing crystal dust
            Vector2 vel = -velocity.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(0.5f, 1.5f)
                + Main.rand.NextVector2Circular(0.3f, 0.3f);
            Color col = ClairDeLunePalette.GetPearlGradient(Main.rand.NextFloat());
            Dust d = Dust.NewDustPerfect(pos, DustID.IceTorch, vel, 0, col, 0.8f);
            d.noGravity = true;

            // Orbiting GemDiamond sparkles
            if (Main.rand.NextBool(3))
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 orbPos = pos + angle.ToRotationVector2() * 6f;
                Color orbCol = ClairDeLunePalette.GetPearlGradient(Main.rand.NextFloat(0.5f, 1f));
                Dust orb = Dust.NewDustPerfect(orbPos, DustID.GemDiamond, Vector2.Zero, 0, orbCol, 0.5f);
                orb.noGravity = true;
            }

            // Starlight sparkle accent
            if (Main.rand.NextBool(6))
                ClairDeLuneVFXLibrary.SpawnStarlitSparkles(pos, 1, 8f, 0.15f);

            Lighting.AddLight(pos, ClairDeLunePalette.PearlBlue.ToVector3() * 0.4f);
        }

        /// <summary>
        /// Crystal impact VFX with pearl burst and music notes.
        /// </summary>
        public static void SoulBeamImpactVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            ClairDeLuneVFXLibrary.ProjectileImpact(pos, 0.6f);
            ClairDeLuneVFXLibrary.SpawnPearlBurst(pos, 6, 3.5f, 0.25f);
            ClairDeLuneVFXLibrary.SpawnMusicNotes(pos, 4, 18f, 0.75f, 1.0f, 28);

            // Crystal dust scatter
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                float progress = (float)i / 8f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 4f);
                Color col = ClairDeLunePalette.GetPearlGradient(progress);
                Dust d = Dust.NewDustPerfect(pos, DustID.GemDiamond, vel, 0, col, 0.9f);
                d.noGravity = true;
            }

            Lighting.AddLight(pos, ClairDeLunePalette.PearlWhite.ToVector3() * 0.8f);
        }

        /// <summary>
        /// Dust-based soul absorption arc -- soul pulled toward the minion
        /// with pearl shimmer at the destination.
        /// </summary>
        public static void SoulAbsorbVFX(Vector2 minionCenter, Vector2 soulSource)
        {
            if (Main.dedServ) return;

            // Arc of dust from source toward minion
            Vector2 toMinion = (minionCenter - soulSource).SafeNormalize(Vector2.Zero);
            float arcDist = Vector2.Distance(minionCenter, soulSource);
            int dustCount = Math.Max(4, (int)(arcDist / 15f));

            for (int i = 0; i < dustCount; i++)
            {
                float progress = (float)i / dustCount;
                Vector2 arcPos = Vector2.Lerp(soulSource, minionCenter, progress);
                // Slight perpendicular wave for arc shape
                Vector2 perp = new Vector2(-toMinion.Y, toMinion.X);
                arcPos += perp * MathF.Sin(progress * MathHelper.Pi) * 12f;

                Vector2 vel = toMinion * (1f + progress * 2f);
                Color col = ClairDeLunePalette.GetPearlGradient(progress);
                Dust d = Dust.NewDustPerfect(arcPos, DustID.IceTorch, vel, 0, col, 0.6f + progress * 0.4f);
                d.noGravity = true;
            }

            // Pearl shimmer at destination
            ClairDeLuneVFXLibrary.SpawnPearlShimmer(minionCenter, 2, 12f, 0.2f);

            Lighting.AddLight(minionCenter, ClairDeLunePalette.PearlBlue.ToVector3() * 0.5f);
            Lighting.AddLight(soulSource, ClairDeLunePalette.SoftBlue.ToVector3() * 0.3f);
        }

        /// <summary>
        /// Expanding pearl shimmer ring proportional to soul count.
        /// Visual feedback for empowerment level.
        /// </summary>
        public static void EmpowermentVFX(Vector2 center, int soulCount)
        {
            if (Main.dedServ) return;

            float time = (float)Main.timeForVisualEffects;
            float scaleFactor = 1f + soulCount * 0.15f;
            float ringRadius = 20f * scaleFactor;

            // Expanding pearl shimmer ring
            int ringDustCount = 6 + soulCount * 2;
            for (int i = 0; i < ringDustCount; i++)
            {
                float angle = MathHelper.TwoPi * i / ringDustCount + time * 0.02f;
                float progress = (float)i / ringDustCount;
                Vector2 ringPos = center + angle.ToRotationVector2() * ringRadius;
                Color col = ClairDeLunePalette.GetPearlGradient(progress);
                Dust d = Dust.NewDustPerfect(ringPos, DustID.GemDiamond, Vector2.Zero, 0,
                    col, 0.5f + soulCount * 0.05f);
                d.noGravity = true;
            }

            // Pearl shimmer burst
            ClairDeLuneVFXLibrary.SpawnPearlShimmer(center, 2 + soulCount, ringRadius, 0.22f);

            // Empowerment light scales with souls
            float intensity = 0.5f + soulCount * 0.08f;
            Color lightCol = ClairDeLunePalette.GetPearlGradient(
                MathHelper.Clamp(soulCount * 0.15f, 0f, 1f));
            Lighting.AddLight(center, lightCol.ToVector3() * intensity);
        }
    }

    // =============================================================================
    //  GEAR-DRIVEN ARBITER -- VFX
    //  Identity: Clockwork judge minion marking and judging targets.
    //  Judicial, mechanical, precise. Brass gear orbits, temporal crimson accents,
    //  clockwork impacts, verdict explosions. Every mark is a temporal verdict.
    // =============================================================================

    public static class GearDrivenArbiterVFX
    {
        /// <summary>
        /// Ambient VFX when holding the Gear-Driven Arbiter item.
        /// Clockwork brass ambiance, gear dust accents, pearl shimmer.
        /// </summary>
        public static void HoldItemVFX(Player player, Item item)
        {
            if (Main.dedServ) return;

            Vector2 center = player.MountedCenter;
            float time = (float)Main.timeForVisualEffects;

            // Clockwork brass shimmer
            if (Main.rand.NextBool(5))
            {
                Vector2 pos = center + Main.rand.NextVector2Circular(20f, 20f);
                Color col = ClairDeLunePalette.GetClockworkGradient(Main.rand.NextFloat(0.3f, 0.8f));
                Dust d = Dust.NewDustPerfect(pos, DustID.Electric, new Vector2(0, -0.3f), 0, col, 0.6f);
                d.noGravity = true;
            }

            // Gear dust accent
            if (Main.rand.NextBool(10))
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 gearPos = center + angle.ToRotationVector2() * 16f;
                Color brassCol = ClairDeLunePalette.GetClockworkGradient(Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(gearPos, DustID.IceTorch,
                    angle.ToRotationVector2() * 0.3f, 0, brassCol, 0.5f);
                d.noGravity = true;
            }

            // Pearl shimmer accent
            if (Main.rand.NextBool(12))
                ClairDeLuneVFXLibrary.SpawnPearlShimmer(center, 1, 18f, 0.18f);

            // Occasional music note
            if (Main.rand.NextBool(28))
                ClairDeLuneVFXLibrary.SpawnMusicNotes(center, 1, 18f, 0.7f, 0.85f, 30);

            // Clockwork-tinted ambient light
            float pulse = 0.2f + MathF.Sin(time * 0.05f) * 0.05f;
            Lighting.AddLight(center, ClairDeLunePalette.ClockworkBrass.ToVector3() * pulse);
        }

        /// <summary>
        /// 3-layer bloom with brass-gold tints for the clockwork judge.
        /// </summary>
        public static void PreDrawInWorldBloom(
            Microsoft.Xna.Framework.Graphics.SpriteBatch sb,
            Microsoft.Xna.Framework.Graphics.Texture2D tex,
            Vector2 pos, Vector2 origin, float rotation, float scale)
        {
            float time = (float)Main.timeForVisualEffects;
            float pulse = 1f + MathF.Sin(time * 0.04f) * 0.03f;

            // Layer 1: Outer night mist with brass warmth
            Color outerCol = Color.Lerp(ClairDeLunePalette.NightMist, ClairDeLunePalette.ClockworkBrass, 0.3f);
            sb.Draw(tex, pos, null, ClairDeLunePalette.Additive(outerCol, 0.38f),
                rotation, origin, scale * 1.08f * pulse, Microsoft.Xna.Framework.Graphics.SpriteEffects.None, 0f);
            // Layer 2: Middle clockwork brass glow
            sb.Draw(tex, pos, null, ClairDeLunePalette.Additive(ClairDeLunePalette.ClockworkBrass, 0.30f),
                rotation, origin, scale * 1.04f * pulse, Microsoft.Xna.Framework.Graphics.SpriteEffects.None, 0f);
            // Layer 3: Inner moonbeam gold core
            sb.Draw(tex, pos, null, ClairDeLunePalette.Additive(ClairDeLunePalette.MoonbeamGold, 0.22f),
                rotation, origin, scale * 1.01f * pulse, Microsoft.Xna.Framework.Graphics.SpriteEffects.None, 0f);
        }

        /// <summary>
        /// Clockwork judge entrance -- brass radial burst, gear cascade dust,
        /// music notes, screen shake.
        /// </summary>
        public static void SummonVFX(Vector2 spawnPos)
        {
            if (Main.dedServ) return;

            // Brass radial dust burst
            for (int i = 0; i < 16; i++)
            {
                float angle = MathHelper.TwoPi * i / 16f;
                float progress = (float)i / 16f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f);
                Color col = ClairDeLunePalette.GetClockworkGradient(progress);
                int dustType = i % 3 == 0 ? DustID.Electric : DustID.IceTorch;
                Dust d = Dust.NewDustPerfect(spawnPos, dustType, vel, 0, col, 1.3f);
                d.noGravity = true;
            }

            // Gear cascade dust — slower orbiting particles
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 gearPos = spawnPos + angle.ToRotationVector2() * 20f;
                Vector2 vel = new Vector2(0, -1.5f) + Main.rand.NextVector2Circular(0.5f, 0.5f);
                Color col = ClairDeLunePalette.GetClockworkGradient((float)i / 8f);
                Dust d = Dust.NewDustPerfect(gearPos, DustID.Electric, vel, 0, col, 0.9f);
                d.noGravity = true;
            }

            // Pearl shimmer accent
            ClairDeLuneVFXLibrary.SpawnPearlBurst(spawnPos, 6, 3.5f, 0.25f);

            // Music notes
            ClairDeLuneVFXLibrary.SpawnMusicNotes(spawnPos, 5, 25f, 0.8f, 1.0f, 35);

            // Bloom flash
            ClairDeLuneVFXLibrary.DrawBloom(spawnPos, 0.5f);

            MagnumScreenEffects.AddScreenShake(4f);
            Lighting.AddLight(spawnPos, ClairDeLunePalette.ClockworkBrass.ToVector3() * 0.9f);
        }

        /// <summary>
        /// Per-frame ambient VFX for the clockwork judge minion.
        /// Brass gear orbit dust (rotating ring), clockwork sparkle, ambient smoke-like mist.
        /// </summary>
        public static void MinionAmbientVFX(Vector2 center)
        {
            if (Main.dedServ) return;

            float time = (float)Main.timeForVisualEffects;

            // Rotating brass gear orbit ring
            if (Main.rand.NextBool(3))
            {
                float orbitAngle = time * 0.05f + Main.rand.NextFloat(MathHelper.TwoPi);
                float orbitRadius = 16f + MathF.Sin(time * 0.04f) * 2f;
                Vector2 orbitPos = center + orbitAngle.ToRotationVector2() * orbitRadius;
                Color col = ClairDeLunePalette.GetClockworkGradient(
                    0.4f + MathF.Sin(time * 0.03f) * 0.3f + 0.3f);
                Dust d = Dust.NewDustPerfect(orbitPos, DustID.Electric, Vector2.Zero, 0, col, 0.6f);
                d.noGravity = true;
            }

            // Clockwork sparkle accent
            if (Main.rand.NextBool(8))
            {
                Vector2 sparkPos = center + Main.rand.NextVector2Circular(10f, 10f);
                Color brassCol = ClairDeLunePalette.GetClockworkGradient(Main.rand.NextFloat(0.4f, 0.9f));
                Dust d = Dust.NewDustPerfect(sparkPos, DustID.IceTorch,
                    new Vector2(0, -0.2f), 0, brassCol, 0.4f);
                d.noGravity = true;
            }

            // Ambient smoke-like mist
            if (Main.rand.NextBool(12))
                ClairDeLuneVFXLibrary.SpawnMoonlitMist(center, 1, 22f, 0.25f);

            // Occasional music note
            if (Main.rand.NextBool(22))
                ClairDeLuneVFXLibrary.SpawnMusicNotes(center, 1, 12f, 0.7f, 0.85f, 25);

            // Pulsing clockwork glow
            float pulse = 0.4f + MathF.Sin(time * 0.06f) * 0.1f;
            Color lightCol = Color.Lerp(ClairDeLunePalette.ClockworkBrass, ClairDeLunePalette.MoonbeamGold,
                MathF.Sin(time * 0.04f) * 0.5f + 0.5f);
            Lighting.AddLight(center, lightCol.ToVector3() * pulse);
        }

        /// <summary>
        /// Judgment dash attack VFX -- brass trail dust with temporal crimson flare.
        /// </summary>
        public static void JudgmentStrikeVFX(Vector2 pos, Vector2 direction)
        {
            if (Main.dedServ) return;

            // Brass trail dust along dash path
            for (int i = 0; i < 4; i++)
            {
                Vector2 vel = direction * Main.rand.NextFloat(2f, 5f)
                    + Main.rand.NextVector2Circular(0.6f, 0.6f);
                float progress = (float)i / 4f;
                Color col = ClairDeLunePalette.GetClockworkGradient(progress);
                Dust d = Dust.NewDustPerfect(pos + Main.rand.NextVector2Circular(5f, 5f),
                    DustID.Electric, vel, 0, col, 1.1f);
                d.noGravity = true;
            }

            // Temporal crimson flare accent
            if (Main.rand.NextBool(2))
            {
                Vector2 vel = direction * Main.rand.NextFloat(1.5f, 3f);
                Color crimsonCol = ClairDeLunePalette.GetTemporalGradient(Main.rand.NextFloat(0.4f, 0.7f));
                Dust d = Dust.NewDustPerfect(pos, DustID.IceTorch, vel, 0, crimsonCol, 0.9f);
                d.noGravity = true;
            }

            // Music note on strike
            if (Main.rand.NextBool(3))
                ClairDeLuneVFXLibrary.SpawnMusicNotes(pos, 1, 8f, 0.7f, 0.9f, 20);

            Lighting.AddLight(pos, ClairDeLunePalette.ClockworkBrass.ToVector3() * 0.5f);
        }

        /// <summary>
        /// Clockwork impact VFX -- gear burst, pearl shimmer,
        /// judgment-themed dust (brass + crimson).
        /// </summary>
        public static void JudgmentImpactVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            // Gear burst — alternating brass and crimson dust ring
            for (int i = 0; i < 10; i++)
            {
                float angle = MathHelper.TwoPi * i / 10f;
                float progress = (float)i / 10f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 4f);
                Color col = i % 2 == 0
                    ? ClairDeLunePalette.GetClockworkGradient(progress)
                    : ClairDeLunePalette.GetTemporalGradient(progress);
                int dustType = i % 2 == 0 ? DustID.Electric : DustID.IceTorch;
                Dust d = Dust.NewDustPerfect(pos, dustType, vel, 0, col, 1.0f);
                d.noGravity = true;
            }

            // Pearl shimmer accent
            ClairDeLuneVFXLibrary.SpawnPearlShimmer(pos, 3, 20f, 0.22f);

            // Music notes at impact
            ClairDeLuneVFXLibrary.SpawnMusicNotes(pos, 3, 15f, 0.75f, 1.0f, 25);

            // Bloom flash
            ClairDeLuneVFXLibrary.DrawBloom(pos, 0.35f);

            Lighting.AddLight(pos, ClairDeLunePalette.MoonbeamGold.ToVector3() * 0.7f);
        }

        /// <summary>
        /// Target marking VFX -- orbiting clockwork dust ring at target position,
        /// temporal crimson shimmer to indicate judgment pending.
        /// </summary>
        public static void MarkTargetVFX(Vector2 targetCenter)
        {
            if (Main.dedServ) return;

            float time = (float)Main.timeForVisualEffects;

            // Orbiting clockwork dust ring around the marked target
            int ringCount = 8;
            for (int i = 0; i < ringCount; i++)
            {
                float angle = MathHelper.TwoPi * i / ringCount + time * 0.06f;
                float progress = (float)i / ringCount;
                Vector2 ringPos = targetCenter + angle.ToRotationVector2() * 28f;
                Color col = ClairDeLunePalette.GetClockworkGradient(progress);
                Dust d = Dust.NewDustPerfect(ringPos, DustID.Electric, Vector2.Zero, 0, col, 0.7f);
                d.noGravity = true;
            }

            // Temporal crimson shimmer accent
            if (Main.rand.NextBool(3))
            {
                Vector2 shimmerPos = targetCenter + Main.rand.NextVector2Circular(20f, 20f);
                Color crimsonCol = ClairDeLunePalette.GetTemporalGradient(Main.rand.NextFloat(0.3f, 0.7f));
                Dust d = Dust.NewDustPerfect(shimmerPos, DustID.IceTorch,
                    new Vector2(0, -0.4f), 0, crimsonCol, 0.6f);
                d.noGravity = true;
            }

            // Judgment mark light
            float pulse = 0.3f + MathF.Sin(time * 0.08f) * 0.1f;
            Color lightCol = Color.Lerp(ClairDeLunePalette.ClockworkBrass, ClairDeLunePalette.TemporalCrimson,
                MathF.Sin(time * 0.06f) * 0.5f + 0.5f);
            Lighting.AddLight(targetCenter, lightCol.ToVector3() * pulse);
        }

        /// <summary>
        /// Verdict explosion VFX when a marked enemy dies -- massive clockwork
        /// explosion with pearl burst, music notes, screen shake.
        /// </summary>
        public static void VerdictExplosionVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            // Massive clockwork radial burst — alternating brass, crimson, pearl
            for (int i = 0; i < 20; i++)
            {
                float angle = MathHelper.TwoPi * i / 20f;
                float progress = (float)i / 20f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 8f);
                Color col;
                int dustType;
                if (i % 3 == 0)
                {
                    col = ClairDeLunePalette.GetTemporalGradient(progress);
                    dustType = DustID.IceTorch;
                }
                else if (i % 3 == 1)
                {
                    col = ClairDeLunePalette.GetClockworkGradient(progress);
                    dustType = DustID.Electric;
                }
                else
                {
                    col = ClairDeLunePalette.GetPearlGradient(progress);
                    dustType = DustID.GemDiamond;
                }
                Dust d = Dust.NewDustPerfect(pos, dustType, vel, 0, col, 1.4f);
                d.noGravity = true;
            }

            // Pearl burst
            ClairDeLuneVFXLibrary.SpawnPearlBurst(pos, 10, 6f, 0.35f);

            // Music note shower
            ClairDeLuneVFXLibrary.SpawnMusicNotes(pos, 8, 35f, 0.8f, 1.1f, 35);

            // Halo rings
            ClairDeLuneVFXLibrary.SpawnGradientHaloRings(pos, 5, 0.35f);

            // Heavy bloom
            ClairDeLuneVFXLibrary.DrawBloom(pos, 0.7f);

            MagnumScreenEffects.AddScreenShake(6f);
            Lighting.AddLight(pos, ClairDeLunePalette.MoonbeamGold.ToVector3() * 1.2f);
        }
    }

    // =============================================================================
    //  AUTOMATON'S TUNING FORK -- VFX
    //  Identity: Resonance support minion projecting a harmonic buff field.
    //  Harmonic, supportive, amplifying. Dream haze mist, pearl resonance rings,
    //  heavy music notes, pulsing sound-wave attacks. Every resonance strengthens allies.
    // =============================================================================

    public static class AutomatonsTuningForkVFX
    {
        /// <summary>
        /// Ambient VFX when holding the Automaton's Tuning Fork item.
        /// Dream haze ambient, pearl shimmer, harmonics-themed dust, gentle glow.
        /// </summary>
        public static void HoldItemVFX(Player player, Item item)
        {
            if (Main.dedServ) return;

            Vector2 center = player.MountedCenter;
            float time = (float)Main.timeForVisualEffects;

            // Dream haze ambient dust
            if (Main.rand.NextBool(5))
            {
                Vector2 pos = center + Main.rand.NextVector2Circular(18f, 18f);
                Color col = ClairDeLunePalette.GetMoonlitGradient(Main.rand.NextFloat(0.3f, 0.8f));
                Dust d = Dust.NewDustPerfect(pos, DustID.IceTorch,
                    new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), -0.4f), 0, col, 0.6f);
                d.noGravity = true;
            }

            // Harmonics-themed GemDiamond dust — musical shimmer
            if (Main.rand.NextBool(8))
            {
                Vector2 pos = center + Main.rand.NextVector2Circular(15f, 15f);
                Color col = ClairDeLunePalette.GetPearlGradient(Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(pos, DustID.GemDiamond,
                    new Vector2(0, -0.3f), 0, col, 0.5f);
                d.noGravity = true;
            }

            // Pearl shimmer accent
            if (Main.rand.NextBool(10))
                ClairDeLuneVFXLibrary.SpawnPearlShimmer(center, 1, 16f, 0.18f);

            // Occasional music note (more frequent than other summons — harmonic identity)
            if (Main.rand.NextBool(18))
                ClairDeLuneVFXLibrary.SpawnMusicNotes(center, 1, 18f, 0.7f, 0.9f, 30);

            // Gentle dream haze glow
            float pulse = 0.2f + MathF.Sin(time * 0.05f) * 0.05f;
            Lighting.AddLight(center, ClairDeLunePalette.DreamHaze.ToVector3() * pulse);
        }

        /// <summary>
        /// 3-layer bloom with dream haze and pearl shimmer tints.
        /// </summary>
        public static void PreDrawInWorldBloom(
            Microsoft.Xna.Framework.Graphics.SpriteBatch sb,
            Microsoft.Xna.Framework.Graphics.Texture2D tex,
            Vector2 pos, Vector2 origin, float rotation, float scale)
        {
            float time = (float)Main.timeForVisualEffects;
            float pulse = 1f + MathF.Sin(time * 0.04f) * 0.03f;

            // Layer 1: Outer midnight blue with dream haze tint
            Color outerCol = Color.Lerp(ClairDeLunePalette.MidnightBlue, ClairDeLunePalette.DreamHaze, 0.25f);
            sb.Draw(tex, pos, null, ClairDeLunePalette.Additive(outerCol, 0.36f),
                rotation, origin, scale * 1.08f * pulse, Microsoft.Xna.Framework.Graphics.SpriteEffects.None, 0f);
            // Layer 2: Middle dream haze glow
            sb.Draw(tex, pos, null, ClairDeLunePalette.Additive(ClairDeLunePalette.DreamHaze, 0.28f),
                rotation, origin, scale * 1.04f * pulse, Microsoft.Xna.Framework.Graphics.SpriteEffects.None, 0f);
            // Layer 3: Inner pearl shimmer core
            sb.Draw(tex, pos, null, ClairDeLunePalette.Additive(ClairDeLunePalette.PearlShimmer, 0.22f),
                rotation, origin, scale * 1.01f * pulse, Microsoft.Xna.Framework.Graphics.SpriteEffects.None, 0f);
        }

        /// <summary>
        /// Harmonic entrance VFX -- pearl shimmer cascade, dream haze mist,
        /// starlight sparkles, music notes.
        /// </summary>
        public static void SummonVFX(Vector2 spawnPos)
        {
            if (Main.dedServ) return;

            // Pearl shimmer cascade
            ClairDeLuneVFXLibrary.SpawnPearlShimmer(spawnPos, 8, 35f, 0.3f);
            ClairDeLuneVFXLibrary.SpawnPearlBurst(spawnPos, 8, 4f, 0.28f);

            // Dream haze mist
            ClairDeLuneVFXLibrary.SpawnMoonlitMist(spawnPos, 6, 45f, 0.5f);

            // Starlight sparkles
            ClairDeLuneVFXLibrary.SpawnStarlitSparkles(spawnPos, 8, 30f, 0.22f);

            // Harmonic dust ring
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                float progress = (float)i / 12f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 4f);
                Color col = ClairDeLunePalette.GetMoonlitGradient(progress);
                int dustType = i % 2 == 0 ? DustID.IceTorch : DustID.GemDiamond;
                Dust d = Dust.NewDustPerfect(spawnPos, dustType, vel, 0, col, 1.0f);
                d.noGravity = true;
            }

            // Music note shower (heavy — resonance theme)
            ClairDeLuneVFXLibrary.SpawnMusicNotes(spawnPos, 8, 30f, 0.8f, 1.1f, 35);

            // Bloom flash
            ClairDeLuneVFXLibrary.DrawBloom(spawnPos, 0.45f);

            Lighting.AddLight(spawnPos, ClairDeLunePalette.PearlShimmer.ToVector3() * 0.8f);
        }

        /// <summary>
        /// Per-frame ambient VFX for the resonance support minion.
        /// Pulsing pearl ring (6-point dust ring), dream haze mist,
        /// HEAVY music notes (resonance theme).
        /// </summary>
        public static void MinionAmbientVFX(Vector2 center)
        {
            if (Main.dedServ) return;

            float time = (float)Main.timeForVisualEffects;

            // Pulsing pearl ring — 6-point dust ring (harmonic symmetry)
            if (Main.rand.NextBool(3))
            {
                float pulseRadius = 16f + MathF.Sin(time * 0.07f) * 4f;
                for (int i = 0; i < 6; i++)
                {
                    float angle = MathHelper.TwoPi * i / 6f + time * 0.03f;
                    float progress = (float)i / 6f;
                    Vector2 ringPos = center + angle.ToRotationVector2() * pulseRadius;
                    Color col = ClairDeLunePalette.GetPearlGradient(progress);
                    Dust d = Dust.NewDustPerfect(ringPos, DustID.GemDiamond, Vector2.Zero, 0, col, 0.5f);
                    d.noGravity = true;
                }
            }

            // Dream haze mist
            if (Main.rand.NextBool(10))
                ClairDeLuneVFXLibrary.SpawnMoonlitMist(center, 1, 22f, 0.3f);

            // HEAVY music notes — the tuning fork resonates constantly
            if (Main.rand.NextBool(8))
                ClairDeLuneVFXLibrary.SpawnMusicNotes(center, 1, 15f, 0.7f, 0.9f, 28);

            // Pearl shimmer accent
            if (Main.rand.NextBool(12))
                ClairDeLuneVFXLibrary.SpawnPearlShimmer(center, 1, 14f, 0.18f);

            // Ambient resonance glow
            float pulse = 0.4f + MathF.Sin(time * 0.07f) * 0.12f;
            Color lightCol = Color.Lerp(ClairDeLunePalette.DreamHaze, ClairDeLunePalette.PearlShimmer,
                MathF.Sin(time * 0.05f) * 0.5f + 0.5f);
            Lighting.AddLight(center, lightCol.ToVector3() * pulse);
        }

        /// <summary>
        /// Outward expanding pearl shimmer ring (buff pulse).
        /// Music notes spawned at the ring edge for harmonic emphasis.
        /// </summary>
        public static void ResonancePulseVFX(Vector2 center, float radius)
        {
            if (Main.dedServ) return;

            float time = (float)Main.timeForVisualEffects;

            // Expanding pearl shimmer ring
            int ringCount = 12 + (int)(radius / 10f);
            for (int i = 0; i < ringCount; i++)
            {
                float angle = MathHelper.TwoPi * i / ringCount;
                float progress = (float)i / ringCount;
                Vector2 ringPos = center + angle.ToRotationVector2() * radius;
                Vector2 vel = angle.ToRotationVector2() * 1.5f;
                Color col = ClairDeLunePalette.GetPearlGradient(progress);
                Dust d = Dust.NewDustPerfect(ringPos, DustID.GemDiamond, vel, 0, col, 0.8f);
                d.noGravity = true;
            }

            // Music notes at ring edge (3 evenly spaced)
            for (int i = 0; i < 3; i++)
            {
                float angle = MathHelper.TwoPi * i / 3f + time * 0.01f;
                Vector2 notePos = center + angle.ToRotationVector2() * radius;
                ClairDeLuneVFXLibrary.SpawnMusicNotes(notePos, 1, 8f, 0.8f, 1.0f, 25);
            }

            // Pearl shimmer at center
            ClairDeLuneVFXLibrary.SpawnPearlShimmer(center, 3, radius * 0.5f, 0.22f);

            Lighting.AddLight(center, ClairDeLunePalette.PearlShimmer.ToVector3() * 0.6f);
        }

        /// <summary>
        /// Ambient buff field VFX -- sparse pearl sparkle and dream haze
        /// at the field boundary for continuous resonance visualization.
        /// </summary>
        public static void ResonanceFieldVFX(Vector2 center, float radius)
        {
            if (Main.dedServ) return;

            float time = (float)Main.timeForVisualEffects;

            // Sparse pearl sparkle at field boundary
            if (Main.rand.NextBool(4))
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float dist = radius * (0.85f + Main.rand.NextFloat(0.15f));
                Vector2 sparklePos = center + angle.ToRotationVector2() * dist;
                Color col = ClairDeLunePalette.GetPearlGradient(Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(sparklePos, DustID.GemDiamond,
                    new Vector2(0, -0.2f), 0, col, 0.5f);
                d.noGravity = true;
            }

            // Dream haze at field boundary
            if (Main.rand.NextBool(8))
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float dist = radius * Main.rand.NextFloat(0.7f, 1.0f);
                Vector2 hazePos = center + angle.ToRotationVector2() * dist;
                Color col = ClairDeLunePalette.GetMoonlitGradient(Main.rand.NextFloat(0.3f, 0.7f));
                Dust d = Dust.NewDustPerfect(hazePos, DustID.IceTorch,
                    Main.rand.NextVector2Circular(0.3f, 0.3f), 0, col, 0.4f);
                d.noGravity = true;
            }

            // Gentle field boundary light
            float pulse = 0.15f + MathF.Sin(time * 0.04f) * 0.05f;
            Lighting.AddLight(center, ClairDeLunePalette.DreamHaze.ToVector3() * pulse);
        }

        /// <summary>
        /// Sound-wave style attack VFX -- pearl-blue dust arc with music notes.
        /// </summary>
        public static void HarmonicStrikeVFX(Vector2 pos, Vector2 direction)
        {
            if (Main.dedServ) return;

            // Pearl-blue dust arc in attack direction
            Vector2 perp = new Vector2(-direction.Y, direction.X);
            for (int i = 0; i < 6; i++)
            {
                float progress = (float)i / 6f;
                float arcOffset = (progress - 0.5f) * 2f;
                Vector2 arcPos = pos + perp * arcOffset * 15f;
                Vector2 vel = direction * Main.rand.NextFloat(3f, 6f)
                    + Main.rand.NextVector2Circular(0.4f, 0.4f);
                Color col = ClairDeLunePalette.GetPearlGradient(progress);
                int dustType = i % 2 == 0 ? DustID.IceTorch : DustID.GemDiamond;
                Dust d = Dust.NewDustPerfect(arcPos, dustType, vel, 0, col, 1.0f);
                d.noGravity = true;
            }

            // Music notes — harmonic attack signature
            ClairDeLuneVFXLibrary.SpawnMusicNotes(pos, 3, 12f, 0.75f, 1.0f, 22);

            Lighting.AddLight(pos, ClairDeLunePalette.PearlShimmer.ToVector3() * 0.5f);
        }

        /// <summary>
        /// Impact VFX -- pearl burst with cascading music note shower.
        /// </summary>
        public static void HarmonicImpactVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            // Pearl burst
            ClairDeLuneVFXLibrary.SpawnPearlBurst(pos, 8, 4f, 0.28f);

            // Impact dust ring
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                float progress = (float)i / 8f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 3.5f);
                Color col = ClairDeLunePalette.GetMoonlitGradient(progress);
                Dust d = Dust.NewDustPerfect(pos, DustID.GemDiamond, vel, 0, col, 0.9f);
                d.noGravity = true;
            }

            // Cascading music note shower (heavy)
            ClairDeLuneVFXLibrary.SpawnMusicNotes(pos, 6, 25f, 0.8f, 1.1f, 30);

            // Pearl shimmer accent
            ClairDeLuneVFXLibrary.SpawnPearlShimmer(pos, 3, 18f, 0.22f);

            // Bloom flash
            ClairDeLuneVFXLibrary.DrawBloom(pos, 0.35f);

            Lighting.AddLight(pos, ClairDeLunePalette.PearlWhite.ToVector3() * 0.7f);
        }
    }
}
