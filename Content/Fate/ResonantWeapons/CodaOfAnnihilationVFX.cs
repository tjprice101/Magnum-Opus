/*
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.Fate.ResonantWeapons
{
    /// <summary>
    /// VFX helper for the Coda of Annihilation — the ultimate Zenith-class weapon.
    /// Cycles through 14 weapon types with spectral sword projectiles.
    /// Handles hold-item ambient, item bloom, sword spawn/trail/impact effects,
    /// and per-weapon color mapping across all five scores.
    /// Call from CodaOfAnnihilation and CodaZenithSwordProjectile.
    /// </summary>
    public static class CodaOfAnnihilationVFX
    {
        // ===== HOLD ITEM VFX =====

        /// <summary>
        /// Per-frame held-item VFX: cosmic pulse lighting, faint constellation orbit,
        /// ambient star sparkles around the wielder. The cosmos answers the conductor.
        /// </summary>
        public static void HoldItemVFX(Player player)
        {
            if (Main.dedServ) return;

            Vector2 center = player.MountedCenter;
            float time = (float)Main.timeForVisualEffects;

            // Faint constellation orbit — spectral star points circling the wielder
            if (Main.rand.NextBool(8))
            {
                float orbitAngle = time * 0.03f + Main.rand.NextFloat(MathHelper.TwoPi);
                float orbitRadius = 40f + MathF.Sin(time * 0.02f) * 10f;
                Vector2 orbitPos = center + orbitAngle.ToRotationVector2() * orbitRadius;
                Color starCol = FatePalette.PaletteLerp(FatePalette.CodaAnnihilation, Main.rand.NextFloat());
                var star = new GenericGlowParticle(orbitPos, Main.rand.NextVector2Circular(0.3f, 0.3f),
                    starCol * 0.5f, 0.18f, 18, true);
                MagnumParticleHandler.SpawnParticle(star);
            }

            // Ambient star sparkles — tiny twinkling motes
            if (Main.rand.NextBool(8))
            {
                Vector2 sparklePos = center + Main.rand.NextVector2Circular(35f, 35f);
                Color sparkleCol = Main.rand.NextBool(3) ? FatePalette.StarGold : FatePalette.WhiteCelestial;
                var sparkle = new GenericGlowParticle(sparklePos, Main.rand.NextVector2Circular(0.5f, 0.5f),
                    sparkleCol * 0.4f, 0.15f, 16, true);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            // Occasional glyph accent — fate's rune shimmers
            if (Main.rand.NextBool(15))
                FateVFXLibrary.SpawnGlyphAccent(center + Main.rand.NextVector2Circular(30f, 30f), 0.22f);

            // Music notes — the coda's overture whispers
            if (Main.rand.NextBool(12))
                FateVFXLibrary.SpawnMusicNotes(center, 1, 15f, 0.65f, 0.85f, 25);

            // Cosmic pulse lighting — pulsing fate purple glow
            float pulse = 0.3f + MathF.Sin(time * 0.05f) * 0.12f;
            Lighting.AddLight(center, FatePalette.FatePurple.ToVector3() * pulse);
        }

        // ===== PREDRAW IN WORLD BLOOM =====

        /// <summary>
        /// Enhanced 3-layer additive bloom with color-shifting for the ultimate weapon.
        /// Uses DrawItemBloomEnhanced with time parameter for dynamic crimson-gold oscillation.
        /// </summary>
        public static void PreDrawInWorldBloom(
            SpriteBatch sb,
            Texture2D tex,
            Vector2 pos, Vector2 origin, float rotation, float scale)
        {
            float time = Main.GameUpdateCount * 0.03f;
            float pulse = 1f + MathF.Sin(time * 1.2f) * 0.08f;

            // Switch to additive blending for bloom layers
            FateVFXLibrary.BeginFateAdditive(sb);

            // 3-layer bloom with enhanced color shift
            FatePalette.DrawItemBloomEnhanced(sb, tex, pos, origin, rotation, scale, pulse, time);

            // Restore standard blending
            FateVFXLibrary.EndFateAdditive(sb);
        }

        // ===== SWORD SPAWN VFX =====

        /// <summary>
        /// One-shot VFX when a zenith sword projectile spawns. Flash of cosmic energy,
        /// halo ring, glow sparks radiating outward, and a music note flourish.
        /// </summary>
        public static void SwordSpawnVFX(Vector2 spawnPos, Color weaponColor)
        {
            if (Main.dedServ) return;

            // Central flare in the weapon's theme color
            CustomParticles.GenericFlare(spawnPos, weaponColor, 0.5f, 14);

            // Halo ring at spawn point
            CustomParticles.HaloRing(spawnPos, weaponColor * 0.7f, 0.3f, 12);

            // 4 glow sparks radiating outward in weapon color
            for (int i = 0; i < 4; i++)
            {
                float angle = MathHelper.TwoPi * i / 4f + Main.rand.NextFloat(-0.3f, 0.3f);
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 4f);
                var spark = new GenericGlowParticle(spawnPos, vel,
                    weaponColor * 0.7f, 0.25f, 16, true);
                MagnumParticleHandler.SpawnParticle(spark);
            }

            // Music note flourish — the coda announces each blade
            FateVFXLibrary.SpawnMusicNotes(spawnPos, 1, 10f, 0.7f, 0.9f, 22);

            Lighting.AddLight(spawnPos, weaponColor.ToVector3() * 0.6f);
        }

        // ===== SWORD TRAIL VFX =====

        /// <summary>
        /// Per-frame trail VFX for flying zenith sword projectiles.
        /// GenericGlowParticle trail in weapon color, occasional star sparkle,
        /// and cosmic cloud trail for nebula depth.
        /// </summary>
        public static void SwordTrailVFX(Vector2 pos, Vector2 velocity, Color trailColor)
        {
            if (Main.dedServ) return;

            Vector2 awayDir = -velocity.SafeNormalize(Vector2.Zero);

            // Primary glow trail in weapon color
            if (Main.rand.NextBool(2))
            {
                var trail = new GenericGlowParticle(pos + Main.rand.NextVector2Circular(4f, 4f),
                    awayDir * Main.rand.NextFloat(0.5f, 1.5f) + Main.rand.NextVector2Circular(0.5f, 0.5f),
                    trailColor * 0.6f, 0.22f, 14, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }

            // Star sparkle accent — cosmic twinkling along flight path
            if (Main.rand.NextBool(5))
            {
                Color starCol = Main.rand.NextBool(3) ? FatePalette.StarGold : FatePalette.WhiteCelestial;
                var star = new GenericGlowParticle(pos + Main.rand.NextVector2Circular(8f, 8f),
                    Main.rand.NextVector2Circular(0.5f, 0.5f),
                    starCol * 0.5f, 0.15f, 12, true);
                MagnumParticleHandler.SpawnParticle(star);
            }

            // Cosmic cloud trail — nebula wisps in the sword's wake (1-in-3)
            if (Main.rand.NextBool(3))
                FateVFXLibrary.SpawnCosmicCloudTrail(pos, velocity, 0.4f);

            // Dust trail in weapon color
            Dust d = Dust.NewDustPerfect(pos, DustID.PinkTorch,
                awayDir * Main.rand.NextFloat(1f, 2f), 0, trailColor, 1.0f);
            d.noGravity = true;

            // Music note (1-in-8)
            if (Main.rand.NextBool(8))
                FateVFXLibrary.SpawnMusicNotes(pos, 1, 8f, 0.65f, 0.8f, 18);

            Lighting.AddLight(pos, trailColor.ToVector3() * 0.35f);
        }

        // ===== SWORD IMPACT VFX =====

        /// <summary>
        /// On-hit impact VFX when a zenith sword strikes an enemy.
        /// Combines shared melee impact with weapon-colored flare, glyph burst,
        /// and 6 star particles radiating outward.
        /// </summary>
        public static void SwordImpactVFX(Vector2 pos, Color weaponColor)
        {
            if (Main.dedServ) return;

            // Shared Fate melee impact as base
            FateVFXLibrary.MeleeImpact(pos, 0);

            // Weapon-colored central flare
            CustomParticles.GenericFlare(pos, weaponColor, 0.5f, 14);

            // Glyph burst radiating outward
            FateVFXLibrary.SpawnGlyphBurst(pos, 6, 4f);

            // 6 star particles in a ring around impact
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 5f);
                Color starCol = Color.Lerp(weaponColor, FatePalette.StarGold, (float)i / 6f);
                var star = new GenericGlowParticle(pos, vel,
                    starCol * 0.7f, 0.25f, 18, true);
                MagnumParticleHandler.SpawnParticle(star);
            }

            // Music notes burst
            FateVFXLibrary.SpawnMusicNotes(pos, 2, 15f, 0.8f, 1.0f, 24);

            // Weapon-colored dust burst
            for (int i = 0; i < 4; i++)
            {
                Vector2 dustVel = Main.rand.NextVector2Circular(4f, 4f);
                Dust d = Dust.NewDustPerfect(pos, DustID.PinkTorch, dustVel, 0, weaponColor, 1.2f);
                d.noGravity = true;
            }

            Lighting.AddLight(pos, weaponColor.ToVector3() * 0.8f);
        }

        // ===== GET WEAPON COLOR =====

        /// <summary>
        /// Returns the theme color for a specific zenith weapon index (0-13).
        /// Maps each index to its source score's palette:
        /// 0-1: Moonlight (violet, light blue)
        /// 2-3: Eroica (red, gold)
        /// 4-5: La Campanella (orange, amber)
        /// 6-7: Enigma (purple, green)
        /// 8: Swan Lake (white)
        /// 9-13: Fate (DarkPink, BrightCrimson, FatePurple, CosmicRose, DestinyFlame)
        /// </summary>
        public static Color GetWeaponColor(int index)
        {
            return index switch
            {
                // Moonlight Sonata — lunar violet and celestial blue
                0 => new Color(138, 43, 226),
                1 => new Color(135, 206, 250),

                // Eroica — heroic scarlet and triumphant gold
                2 => new Color(255, 100, 100),
                3 => new Color(255, 200, 80),

                // La Campanella — bell fire orange and warm amber
                4 => new Color(255, 140, 40),
                5 => new Color(255, 180, 60),

                // Enigma Variations — arcane purple and mystery green
                6 => new Color(140, 60, 200),
                7 => new Color(50, 180, 100),

                // Swan Lake — pristine white
                8 => Color.White,

                // Fate — the cosmic palette of destiny
                9 => FatePalette.DarkPink,
                10 => FatePalette.BrightCrimson,
                11 => FatePalette.FatePurple,
                12 => FatePalette.CosmicRose,
                13 => FatePalette.DestinyFlame,

                // Fallback to bright crimson for any out-of-range
                _ => FatePalette.BrightCrimson,
            };
        }

        // ===== SWING AMBIENT VFX =====

        /// <summary>
        /// Per-frame VFX while the held swing projectile is active.
        /// Cosmic sparks along the blade, star particles, glyph accents, and pulsing light.
        /// </summary>
        public static void SwingFrameVFX(Vector2 tipPos, Vector2 swordDirection, int comboStep)
        {
            if (Main.dedServ) return;

            // Dense cosmic sparks at blade tip (1-in-2)
            if (Main.rand.NextBool(2))
            {
                Color sparkCol = FatePalette.GetCosmicGradient(Main.rand.NextFloat());
                Vector2 sparkVel = -swordDirection * Main.rand.NextFloat(1f, 3f) + Main.rand.NextVector2Circular(0.8f, 0.8f);
                var spark = new GenericGlowParticle(tipPos, sparkVel,
                    sparkCol * 0.6f, 0.22f, 14, true);
                MagnumParticleHandler.SpawnParticle(spark);
            }

            // Star particles along blade (1-in-4)
            if (Main.rand.NextBool(4))
                FateVFXLibrary.SpawnStarSparkles(tipPos, 1, 12f, 0.18f);

            // Glyph trail accent (1-in-6)
            if (Main.rand.NextBool(6))
                FateVFXLibrary.SpawnGlyphAccent(tipPos, 0.2f);

            // Fate dual-color dust
            FateVFXLibrary.SpawnFateSwingDust(tipPos, -swordDirection);

            // Music notes (periodic)
            if (Main.GameUpdateCount % 8 == 0)
                FateVFXLibrary.SpawnMusicNotes(tipPos, 1, 10f, 0.7f, 0.9f, 22);

            Lighting.AddLight(tipPos, FatePalette.GetCosmicGradient(0.4f + comboStep * 0.1f).ToVector3() * 0.5f);
        }

        // ===== ANNIHILATION FINISHER VFX =====

        /// <summary>
        /// Ultimate finisher explosion VFX — the coda's ultimate annihilation statement.
        /// Used when all 14 swords converge or on massive kills.
        /// </summary>
        public static void AnnihilationFinisherVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            // Supernova base explosion
            FateVFXLibrary.SupernovaExplosion(pos, 1.2f);

            // Additional annihilation-specific effects
            // 14-point star burst — one for each weapon
            for (int i = 0; i < 14; i++)
            {
                float angle = MathHelper.TwoPi * i / 14f;
                Vector2 starPos = pos + angle.ToRotationVector2() * 60f;
                Color weaponCol = GetWeaponColor(i);
                CustomParticles.GenericFlare(starPos, weaponCol, 0.4f, 16);
            }

            // Glyph circle — the coda's seal
            FateVFXLibrary.SpawnGlyphCircle(pos, 14, 80f, 0.06f);

            // Music note cascade
            FateVFXLibrary.SpawnMusicNotes(pos, 8, 50f, 0.85f, 1.2f, 35);

            // Constellation burst — the final star map
            FateVFXLibrary.SpawnConstellationBurst(pos, 10, 90f, 1.2f);

            Lighting.AddLight(pos, FatePalette.SupernovaWhite.ToVector3() * 2.0f);
        }

        // ===== TRAIL RENDERING FUNCTIONS =====

        /// <summary>
        /// Trail color function for zenith sword projectile trails.
        /// Uses CodaAnnihilation palette with additive-safe output.
        /// </summary>
        public static Color CodaTrailColor(float completionRatio)
        {
            Color c = FatePalette.PaletteLerp(FatePalette.CodaAnnihilation,
                0.2f + completionRatio * 0.6f);
            float fade = 1f - MathF.Pow(completionRatio, 1.3f);
            return (c * fade) with { A = 0 };
        }

        /// <summary>
        /// Trail width function for zenith sword projectiles.
        /// Wide at head, elegant taper to point.
        /// </summary>
        public static float CodaTrailWidth(float completionRatio)
            => FateVFXLibrary.FateTrailWidth(completionRatio, 18f);
    }
}
*/