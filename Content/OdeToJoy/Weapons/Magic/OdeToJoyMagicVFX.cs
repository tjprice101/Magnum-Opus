using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Bloom;
using MagnumOpus.Common.Systems.VFX.Screen;

namespace MagnumOpus.Content.OdeToJoy.Weapons.Magic
{
    /// <summary>
    /// Per-weapon VFX for the 3 Ode to Joy magic weapons.
    ///
    /// Weapons covered:
    ///   1. HymnOfTheVictorious -- Staff shooting 8 orbiting notes that launch outward, 3600 dmg.
    ///      Identity: musical hymn energy, glowing note constellations, symphonic power.
    ///   2. ElysianVerdict -- Cursor-tracking orb spawning vine missiles, 3200 dmg.
    ///      Identity: nature judgment, verdant orb of power, vine strike.
    ///   3. AnthemOfGlory -- Tome firing 3 petal shards that chain to enemies, 2800 dmg.
    ///      Identity: blazing glory petals, golden vine chains, anthemic power.
    ///
    /// All VFX use OdeToJoyPalette colors, OdeToJoyVFXLibrary methods,
    /// {A = 0} additive bloom, Lighting.AddLight, and MagnumParticleHandler.SpawnParticle.
    /// </summary>
    public static class OdeToJoyMagicVFX
    {
        // =====================================================================
        //  HYMN OF THE VICTORIOUS
        //  Staff -- 8 orbiting notes that launch outward, 3600 damage.
        //  Musical hymn energy, glowing note constellations, symphonic power.
        // =====================================================================

        /// <summary>
        /// Per-frame held-item VFX: ambient music notes drift near the staff tip,
        /// garden gradient colored, every 8 frames.
        /// </summary>
        public static void HymnHoldItemVFX(Player player)
        {
            if (Main.dedServ) return;

            Vector2 center = player.MountedCenter;
            float time = (float)Main.timeForVisualEffects;

            // Ambient music notes at staff tip every 8 frames
            if (Main.GameUpdateCount % 8 == 0)
            {
                Vector2 tipOffset = new Vector2(player.direction * 26f, -10f);
                Vector2 tipPos = center + tipOffset + Main.rand.NextVector2Circular(6f, 6f);
                OdeToJoyVFXLibrary.SpawnMusicNotes(tipPos, 1, 8f, 0.65f, 0.85f, 25);
            }

            // Subtle golden pollen motes drifting upward
            if (Main.rand.NextBool(7))
            {
                Vector2 tipOffset = new Vector2(player.direction * 24f, -8f);
                Vector2 motePos = center + tipOffset + Main.rand.NextVector2Circular(10f, 10f);
                Vector2 vel = new Vector2(0f, -0.4f) + Main.rand.NextVector2Circular(0.3f, 0.3f);
                Color col = OdeToJoyPalette.GoldenPollen * 0.5f;
                var glow = new GenericGlowParticle(motePos, vel, col, 0.12f, 18, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }

            float pulse = 0.25f + MathF.Sin(time * 0.05f) * 0.1f;
            Lighting.AddLight(center, OdeToJoyPalette.GoldenPollen.ToVector3() * pulse);
        }

        /// <summary>
        /// Standard 3-layer Ode to Joy item bloom for the Hymn staff sprite.
        /// </summary>
        public static void HymnPreDrawInWorldBloom(
            SpriteBatch sb, Texture2D tex,
            Vector2 pos, Vector2 origin, float rotation, float scale)
        {
            float time = (float)Main.timeForVisualEffects;
            float pulse = 1f + MathF.Sin(time * 0.04f) * 0.03f;
            OdeToJoyPalette.DrawItemBloom(sb, tex, pos, origin, rotation, scale, pulse);
        }

        /// <summary>
        /// Cast VFX when the staff fires its orbiting notes.
        /// Musical impact with music notes scattered around the cast position.
        /// </summary>
        public static void HymnCastVFX(Vector2 playerCenter, int noteCombo)
        {
            if (Main.dedServ) return;

            OdeToJoyVFXLibrary.MusicalImpact(playerCenter, 0.5f + noteCombo * 0.05f);

            // Scatter music notes around center proportional to combo
            int noteCount = 3 + noteCombo;
            OdeToJoyVFXLibrary.SpawnMusicNotes(playerCenter, noteCount, 25f, 0.75f, 1.0f, 30);

            // Garden gradient dust ring
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 4f);
                Color col = OdeToJoyPalette.GetGardenGradient((float)i / 6f);
                Dust d = Dust.NewDustPerfect(playerCenter, DustID.YellowTorch, vel, 0, col, 1.1f);
                d.noGravity = true;
            }

            Lighting.AddLight(playerCenter, OdeToJoyPalette.GoldenPollen.ToVector3() * 0.7f);
        }

        /// <summary>
        /// Individual note orbit spawn VFX: bloom flare at spawn position
        /// with garden gradient color based on note index.
        /// </summary>
        public static void HymnNoteSpawnVFX(Vector2 notePos, float gradientT)
        {
            if (Main.dedServ) return;

            Color col = OdeToJoyPalette.GetGardenGradient(gradientT);
            OdeToJoyVFXLibrary.BloomFlare(notePos, col, 0.4f);

            // Small spawn sparkle
            var sparkle = new SparkleParticle(notePos,
                Main.rand.NextVector2Circular(0.5f, 0.5f),
                col * 0.7f, 0.2f, 14);
            MagnumParticleHandler.SpawnParticle(sparkle);

            Lighting.AddLight(notePos, col.ToVector3() * 0.5f);
        }

        /// <summary>
        /// Every 5th cast: massive symphonic explosion with triumphant celebration,
        /// 20 music notes in a radial pattern.
        /// </summary>
        public static void HymnSymphonicExplosionVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            OdeToJoyVFXLibrary.TriumphantCelebration(pos, 1.4f);

            // Massive music note burst -- 20 notes in radial pattern
            OdeToJoyVFXLibrary.MusicNoteBurst(pos, OdeToJoyPalette.GoldenPollen, 20, 40f);

            // Radial garden gradient glow ring
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 7f);
                Color col = OdeToJoyPalette.GetGardenGradient((float)i / 12f);
                var glow = new GenericGlowParticle(pos, vel, col * 0.7f, 0.3f, 22, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }

            // Bloom ring particle for the symphonic explosion
            var ring = new BloomRingParticle(pos, Vector2.Zero,
                OdeToJoyPalette.GoldenPollen with { A = 0 }, 0.6f, 20);
            MagnumParticleHandler.SpawnParticle(ring);

            Lighting.AddLight(pos, OdeToJoyPalette.GoldenPollen.ToVector3() * 1.5f);
        }

        /// <summary>
        /// Per-frame trail VFX for orbiting / launched note projectiles.
        /// Garden gradient GenericGlowParticle trail.
        /// </summary>
        public static void VictoriousNoteTrailVFX(Vector2 center, Vector2 velocity)
        {
            if (Main.dedServ) return;

            Vector2 awayDir = -velocity.SafeNormalize(Vector2.Zero);
            float gradientT = (Main.GameUpdateCount * 0.02f) % 1f;
            Color trailCol = OdeToJoyPalette.GetGardenGradient(gradientT);

            var trail = new GenericGlowParticle(
                center + Main.rand.NextVector2Circular(4f, 4f),
                awayDir * Main.rand.NextFloat(0.5f, 1.5f) + Main.rand.NextVector2Circular(0.3f, 0.3f),
                trailCol * 0.55f, 0.18f, 14, true);
            MagnumParticleHandler.SpawnParticle(trail);

            // Faint golden dust
            if (Main.rand.NextBool(3))
            {
                Dust d = Dust.NewDustPerfect(center, DustID.YellowTorch,
                    awayDir * Main.rand.NextFloat(0.5f, 1.5f), 0,
                    OdeToJoyPalette.GoldenPollen, 0.8f);
                d.noGravity = true;
            }

            Lighting.AddLight(center, OdeToJoyPalette.GoldenPollen.ToVector3() * 0.35f);
        }

        /// <summary>
        /// 3-layer PreDraw bloom for note projectiles: GoldenPollen, RosePink, White core pulsing.
        /// </summary>
        public static void VictoriousNotePreDraw(
            SpriteBatch sb, Texture2D tex,
            Vector2 drawPos, Vector2 origin, float rotation, float scale)
        {
            float time = (float)Main.timeForVisualEffects;
            float pulse = 1f + MathF.Sin(time * 0.06f) * 0.05f;

            // Layer 1: Outer golden pollen aura
            sb.Draw(tex, drawPos, null,
                OdeToJoyPalette.GoldenPollen with { A = 0 } * 0.40f,
                rotation, origin, scale * 1.10f * pulse, SpriteEffects.None, 0f);

            // Layer 2: Middle rose pink glow
            sb.Draw(tex, drawPos, null,
                OdeToJoyPalette.RosePink with { A = 0 } * 0.30f,
                rotation, origin, scale * 1.05f * pulse, SpriteEffects.None, 0f);

            // Layer 3: Inner white core
            sb.Draw(tex, drawPos, null,
                Color.White with { A = 0 } * 0.22f,
                rotation, origin, scale * 1.01f * pulse, SpriteEffects.None, 0f);
        }

        /// <summary>
        /// Note hits NPC: musical impact at moderate intensity.
        /// </summary>
        public static void VictoriousNoteImpactVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            OdeToJoyVFXLibrary.MusicalImpact(pos, 0.7f);

            Lighting.AddLight(pos, OdeToJoyPalette.GoldenPollen.ToVector3() * 0.6f);
        }

        /// <summary>
        /// Note death: rose petals, music note burst, and bloom burst.
        /// </summary>
        public static void VictoriousNoteDeathVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            OdeToJoyVFXLibrary.SpawnRosePetals(pos);
            OdeToJoyVFXLibrary.MusicNoteBurst(pos, OdeToJoyPalette.GoldenPollen, 5, 15f);
            OdeToJoyVFXLibrary.BloomBurst(pos);

            // Death sparkles
            for (int i = 0; i < 4; i++)
            {
                float angle = MathHelper.TwoPi * i / 4f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 4f);
                Color col = OdeToJoyPalette.GetGardenGradient((float)i / 4f);
                var spark = new GenericGlowParticle(pos, vel, col * 0.6f, 0.2f, 16, true);
                MagnumParticleHandler.SpawnParticle(spark);
            }

            Lighting.AddLight(pos, OdeToJoyPalette.RosePink.ToVector3() * 0.7f);
        }

        /// <summary>
        /// Expanding symphonic explosion particles at radius scaled by progress.
        /// Called per frame during the expanding area effect.
        /// </summary>
        public static void SymphonicExplosionAreaVFX(Vector2 center, float progress)
        {
            if (Main.dedServ) return;

            float radius = 120f * progress;

            // Orbiting particles at the expanding edge
            if (Main.GameUpdateCount % 3 == 0)
            {
                for (int i = 0; i < 4; i++)
                {
                    float angle = MathHelper.TwoPi * i / 4f + Main.GameUpdateCount * 0.1f;
                    Vector2 particlePos = center + angle.ToRotationVector2() * radius;
                    Color col = OdeToJoyPalette.GetGardenGradient((float)i / 4f);
                    var glow = new GenericGlowParticle(particlePos,
                        angle.ToRotationVector2() * 1f,
                        col * 0.5f * (1f - progress), 0.22f, 14, true);
                    MagnumParticleHandler.SpawnParticle(glow);
                }
            }

            // Central golden pulse
            if (Main.GameUpdateCount % 6 == 0)
            {
                var bloom = new BloomParticle(center, Vector2.Zero,
                    OdeToJoyPalette.GoldenPollen with { A = 0 } * 0.4f * (1f - progress),
                    0.3f * progress, 12);
                MagnumParticleHandler.SpawnParticle(bloom);
            }

            Lighting.AddLight(center, OdeToJoyPalette.GoldenPollen.ToVector3() * 0.6f * (1f - progress));
        }

        /// <summary>
        /// Healing VFX: 5 ascending green music notes rising from the player.
        /// </summary>
        public static void SymphonicHealVFX(Vector2 playerCenter)
        {
            if (Main.dedServ) return;

            for (int i = 0; i < 5; i++)
            {
                Vector2 offset = new Vector2(Main.rand.NextFloat(-15f, 15f), 0f);
                Vector2 vel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -2f - Main.rand.NextFloat(1f));
                Color col = Color.Lerp(OdeToJoyPalette.VerdantGreen, OdeToJoyPalette.GoldenPollen,
                    Main.rand.NextFloat(0.3f));

                OdeToJoyVFXLibrary.SpawnMusicNotes(playerCenter + offset, 1, 5f, 0.7f, 0.9f, 30);

                // Ascending green glow particles
                var glow = new GenericGlowParticle(playerCenter + offset, vel,
                    col * 0.6f, 0.18f, 22, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }

            Lighting.AddLight(playerCenter, OdeToJoyPalette.VerdantGreen.ToVector3() * 0.6f);
        }

        // =====================================================================
        //  ELYSIAN VERDICT
        //  Cursor-tracking orb spawning vine missiles, 3200 damage.
        //  Nature judgment, verdant orb of power, vine strike.
        // =====================================================================

        /// <summary>
        /// Standard 3-layer Ode to Joy item bloom for the Verdict weapon sprite.
        /// </summary>
        public static void VerdictPreDrawInWorldBloom(
            SpriteBatch sb, Texture2D tex,
            Vector2 pos, Vector2 origin, float rotation, float scale)
        {
            float time = (float)Main.timeForVisualEffects;
            float pulse = 1f + MathF.Sin(time * 0.04f) * 0.03f;
            OdeToJoyPalette.DrawItemBloom(sb, tex, pos, origin, rotation, scale, pulse);
        }

        /// <summary>
        /// On-shoot VFX: garden impact, directional sparks, and 6 music notes
        /// launched in the firing direction.
        /// </summary>
        public static void VerdictLaunchVFX(Vector2 pos, Vector2 direction)
        {
            if (Main.dedServ) return;

            OdeToJoyVFXLibrary.GardenImpact(pos, 0.7f);
            OdeToJoyVFXLibrary.SpawnDirectionalSparks(pos, direction);

            // 6 music notes spreading in firing direction
            for (int i = 0; i < 6; i++)
            {
                float spread = MathHelper.Lerp(-0.4f, 0.4f, (float)i / 5f);
                Vector2 noteVel = direction.RotatedBy(spread) * Main.rand.NextFloat(2f, 4f);
                OdeToJoyVFXLibrary.SpawnMusicNotes(pos + noteVel * 2f, 1, 6f, 0.7f, 0.9f, 22);
            }

            Lighting.AddLight(pos, OdeToJoyPalette.VerdantGreen.ToVector3() * 0.7f);
        }

        /// <summary>
        /// Per-frame orb projectile trail: 2 green-gold GenericGlowParticle per frame.
        /// </summary>
        public static void ElysianOrbTrailVFX(Vector2 center, Vector2 velocity)
        {
            if (Main.dedServ) return;

            Vector2 awayDir = -velocity.SafeNormalize(Vector2.Zero);

            for (int i = 0; i < 2; i++)
            {
                Color col = Color.Lerp(OdeToJoyPalette.VerdantGreen, OdeToJoyPalette.GoldenPollen,
                    Main.rand.NextFloat());
                var trail = new GenericGlowParticle(
                    center + Main.rand.NextVector2Circular(5f, 5f),
                    awayDir * Main.rand.NextFloat(0.5f, 1.5f) + Main.rand.NextVector2Circular(0.4f, 0.4f),
                    col * 0.5f, 0.2f, 16, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }

            Lighting.AddLight(center, OdeToJoyPalette.VerdantGreen.ToVector3() * 0.4f);
        }

        /// <summary>
        /// 3-layer PreDraw bloom for the orb projectile: VerdantGreen, GoldenPollen, White core pulsing.
        /// </summary>
        public static void ElysianOrbPreDraw(
            SpriteBatch sb, Texture2D tex,
            Vector2 drawPos, Vector2 origin, float rotation, float scale)
        {
            float time = (float)Main.timeForVisualEffects;
            float pulse = 1f + MathF.Sin(time * 0.06f) * 0.05f;

            // Layer 1: Outer verdant green aura
            sb.Draw(tex, drawPos, null,
                OdeToJoyPalette.VerdantGreen with { A = 0 } * 0.40f,
                rotation, origin, scale * 1.10f * pulse, SpriteEffects.None, 0f);

            // Layer 2: Middle golden pollen glow
            sb.Draw(tex, drawPos, null,
                OdeToJoyPalette.GoldenPollen with { A = 0 } * 0.30f,
                rotation, origin, scale * 1.05f * pulse, SpriteEffects.None, 0f);

            // Layer 3: Inner white core
            sb.Draw(tex, drawPos, null,
                Color.White with { A = 0 } * 0.22f,
                rotation, origin, scale * 1.01f * pulse, SpriteEffects.None, 0f);
        }

        /// <summary>
        /// Vine missile spawns from orb: vine trail dust and small bloom flare.
        /// </summary>
        public static void ElysianOrbMissileLaunchVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            OdeToJoyVFXLibrary.SpawnVineTrailDust(pos, Vector2.Zero);
            OdeToJoyVFXLibrary.BloomFlare(pos, OdeToJoyPalette.VerdantGreen, 0.3f);

            // Small verdant spark ring
            for (int i = 0; i < 4; i++)
            {
                float angle = MathHelper.TwoPi * i / 4f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(1.5f, 3f);
                Color col = Color.Lerp(OdeToJoyPalette.VerdantGreen, OdeToJoyPalette.GoldenPollen,
                    (float)i / 4f);
                var spark = new GenericGlowParticle(pos, vel, col * 0.5f, 0.14f, 12, true);
                MagnumParticleHandler.SpawnParticle(spark);
            }

            Lighting.AddLight(pos, OdeToJoyPalette.VerdantGreen.ToVector3() * 0.5f);
        }

        /// <summary>
        /// Orb explosion on death: finisher slam at 1.2 intensity.
        /// </summary>
        public static void ElysianOrbExplodeVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            OdeToJoyVFXLibrary.FinisherSlam(pos, 1.2f);

            Lighting.AddLight(pos, OdeToJoyPalette.VerdantGreen.ToVector3() * 1.2f);
        }

        /// <summary>
        /// Per-frame vine missile trail: green GenericGlowParticle.
        /// </summary>
        public static void VineMissileTrailVFX(Vector2 center, Vector2 velocity)
        {
            if (Main.dedServ) return;

            Vector2 awayDir = -velocity.SafeNormalize(Vector2.Zero);

            Color col = OdeToJoyPalette.VerdantGreen * 0.5f;
            var trail = new GenericGlowParticle(
                center + Main.rand.NextVector2Circular(3f, 3f),
                awayDir * Main.rand.NextFloat(0.5f, 1.2f) + Main.rand.NextVector2Circular(0.3f, 0.3f),
                col, 0.16f, 14, true);
            MagnumParticleHandler.SpawnParticle(trail);

            // Faint vine dust
            if (Main.rand.NextBool(3))
            {
                Dust d = Dust.NewDustPerfect(center, DustID.GreenTorch,
                    awayDir * Main.rand.NextFloat(0.5f, 1f), 0,
                    OdeToJoyPalette.VerdantGreen, 0.7f);
                d.noGravity = true;
            }

            Lighting.AddLight(center, OdeToJoyPalette.VerdantGreen.ToVector3() * 0.25f);
        }

        /// <summary>
        /// Vine missile hit NPC: vine trail dust and blossom impact at 0.4 intensity.
        /// </summary>
        public static void VineMissileImpactVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            OdeToJoyVFXLibrary.SpawnVineTrailDust(pos, Vector2.Zero);
            OdeToJoyVFXLibrary.BlossomImpact(pos, 0.4f);

            Lighting.AddLight(pos, OdeToJoyPalette.VerdantGreen.ToVector3() * 0.5f);
        }

        /// <summary>
        /// Vine missile death: rose petals and bloom burst.
        /// </summary>
        public static void VineMissileDeathVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            OdeToJoyVFXLibrary.SpawnRosePetals(pos, 3);
            OdeToJoyVFXLibrary.BloomBurst(pos, 0.3f);

            // Small death sparkle
            for (int i = 0; i < 3; i++)
            {
                float angle = MathHelper.TwoPi * i / 3f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(1.5f, 3f);
                var glow = new GenericGlowParticle(pos, vel,
                    OdeToJoyPalette.VerdantGreen * 0.5f, 0.14f, 14, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }

            Lighting.AddLight(pos, OdeToJoyPalette.RosePink.ToVector3() * 0.5f);
        }

        /// <summary>
        /// Expanding area damage particles at radius scaled by progress.
        /// Called per frame during the orb's area explosion effect.
        /// </summary>
        public static void ElysianExplosionAreaVFX(Vector2 center, float progress)
        {
            if (Main.dedServ) return;

            float radius = 100f * progress;

            // Orbiting verdant particles at the expanding edge
            if (Main.GameUpdateCount % 3 == 0)
            {
                for (int i = 0; i < 3; i++)
                {
                    float angle = MathHelper.TwoPi * i / 3f + Main.GameUpdateCount * 0.12f;
                    Vector2 particlePos = center + angle.ToRotationVector2() * radius;
                    Color col = Color.Lerp(OdeToJoyPalette.VerdantGreen, OdeToJoyPalette.GoldenPollen,
                        (float)i / 3f);
                    var glow = new GenericGlowParticle(particlePos,
                        angle.ToRotationVector2() * 0.8f,
                        col * 0.5f * (1f - progress), 0.2f, 14, true);
                    MagnumParticleHandler.SpawnParticle(glow);
                }
            }

            // Vine dust at expansion edge
            if (Main.rand.NextBool(2))
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 dustPos = center + angle.ToRotationVector2() * radius;
                Dust d = Dust.NewDustPerfect(dustPos, DustID.GreenTorch,
                    Main.rand.NextVector2Circular(1f, 1f), 0,
                    OdeToJoyPalette.VerdantGreen * (1f - progress), 0.9f);
                d.noGravity = true;
            }

            Lighting.AddLight(center, OdeToJoyPalette.VerdantGreen.ToVector3() * 0.5f * (1f - progress));
        }

        // =====================================================================
        //  ANTHEM OF GLORY
        //  Tome -- 3 petal shards that chain to enemies, 2800 damage.
        //  Blazing glory petals, golden vine chains, anthemic power.
        // =====================================================================

        /// <summary>
        /// Standard 3-layer Ode to Joy item bloom for the Glory tome sprite.
        /// </summary>
        public static void GloryPreDrawInWorldBloom(
            SpriteBatch sb, Texture2D tex,
            Vector2 pos, Vector2 origin, float rotation, float scale)
        {
            float time = (float)Main.timeForVisualEffects;
            float pulse = 1f + MathF.Sin(time * 0.04f) * 0.03f;
            OdeToJoyPalette.DrawItemBloom(sb, tex, pos, origin, rotation, scale, pulse);
        }

        /// <summary>
        /// On-shoot VFX: blossom impact at muzzle and 3 music notes in firing direction.
        /// </summary>
        public static void GloryCastVFX(Vector2 muzzlePos, Vector2 direction)
        {
            if (Main.dedServ) return;

            OdeToJoyVFXLibrary.BlossomImpact(muzzlePos, 0.5f);

            // 3 music notes spreading in firing direction
            for (int i = 0; i < 3; i++)
            {
                float spread = MathHelper.Lerp(-0.3f, 0.3f, (float)i / 2f);
                Vector2 noteVel = direction.RotatedBy(spread) * Main.rand.NextFloat(2f, 3.5f);
                OdeToJoyVFXLibrary.SpawnMusicNotes(muzzlePos + noteVel * 2f, 1, 5f, 0.7f, 0.9f, 20);
            }

            // Petal dust in firing direction
            for (int i = 0; i < 4; i++)
            {
                Vector2 vel = direction * Main.rand.NextFloat(2f, 4f)
                    + Main.rand.NextVector2Circular(1f, 1f);
                Color col = OdeToJoyPalette.GetPetalGradient(Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(muzzlePos, DustID.YellowTorch, vel, 0, col, 1.0f);
                d.noGravity = true;
            }

            Lighting.AddLight(muzzlePos, OdeToJoyPalette.RosePink.ToVector3() * 0.6f);
        }

        /// <summary>
        /// Every 3rd cast glory beam: garden impact at 0.9 intensity with music note burst.
        /// </summary>
        public static void GloryBeamCastVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            OdeToJoyVFXLibrary.GardenImpact(pos, 0.9f);
            OdeToJoyVFXLibrary.MusicNoteBurst(pos, OdeToJoyPalette.GoldenPollen, 8, 25f);

            // Radial rose-gold dust ring
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 5f);
                Color col = OdeToJoyPalette.GetPetalGradient((float)i / 8f);
                Dust d = Dust.NewDustPerfect(pos, DustID.YellowTorch, vel, 0, col, 1.2f);
                d.noGravity = true;
            }

            // Bloom ring for glory beam activation
            var ring = new BloomRingParticle(pos, Vector2.Zero,
                OdeToJoyPalette.GoldenPollen with { A = 0 }, 0.45f, 16);
            MagnumParticleHandler.SpawnParticle(ring);

            Lighting.AddLight(pos, OdeToJoyPalette.GoldenPollen.ToVector3() * 0.9f);
        }

        /// <summary>
        /// Per-frame shard projectile trail: petal gradient GenericGlowParticle.
        /// </summary>
        public static void GloryShardTrailVFX(Vector2 center, Vector2 velocity)
        {
            if (Main.dedServ) return;

            Vector2 awayDir = -velocity.SafeNormalize(Vector2.Zero);
            Color trailCol = OdeToJoyPalette.GetPetalGradient(
                (Main.GameUpdateCount * 0.02f) % 1f);

            var trail = new GenericGlowParticle(
                center + Main.rand.NextVector2Circular(3f, 3f),
                awayDir * Main.rand.NextFloat(0.5f, 1.2f) + Main.rand.NextVector2Circular(0.3f, 0.3f),
                trailCol * 0.5f, 0.16f, 14, true);
            MagnumParticleHandler.SpawnParticle(trail);

            // Rose petal dust accent
            if (Main.rand.NextBool(4))
            {
                Dust d = Dust.NewDustPerfect(center, DustID.YellowTorch,
                    awayDir * Main.rand.NextFloat(0.5f, 1f), 0,
                    OdeToJoyPalette.RosePink, 0.7f);
                d.noGravity = true;
            }

            Lighting.AddLight(center, OdeToJoyPalette.RosePink.ToVector3() * 0.3f);
        }

        /// <summary>
        /// 3-layer PreDraw bloom for shard projectiles: RosePink, GoldenPollen, White pulsing.
        /// </summary>
        public static void GloryShardPreDraw(
            SpriteBatch sb, Texture2D tex,
            Vector2 drawPos, Vector2 origin, float rotation, float scale)
        {
            float time = (float)Main.timeForVisualEffects;
            float pulse = 1f + MathF.Sin(time * 0.06f) * 0.05f;

            // Layer 1: Outer rose pink aura
            sb.Draw(tex, drawPos, null,
                OdeToJoyPalette.RosePink with { A = 0 } * 0.40f,
                rotation, origin, scale * 1.10f * pulse, SpriteEffects.None, 0f);

            // Layer 2: Middle golden pollen glow
            sb.Draw(tex, drawPos, null,
                OdeToJoyPalette.GoldenPollen with { A = 0 } * 0.30f,
                rotation, origin, scale * 1.05f * pulse, SpriteEffects.None, 0f);

            // Layer 3: Inner white core
            sb.Draw(tex, drawPos, null,
                Color.White with { A = 0 } * 0.22f,
                rotation, origin, scale * 1.01f * pulse, SpriteEffects.None, 0f);
        }

        /// <summary>
        /// Shard hits NPC: blossom impact at 0.6 intensity, music notes at impact.
        /// </summary>
        public static void GloryShardImpactVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            OdeToJoyVFXLibrary.BlossomImpact(pos, 0.6f);
            OdeToJoyVFXLibrary.SpawnMusicNotes(pos, 2, 12f, 0.7f, 0.9f, 22);

            Lighting.AddLight(pos, OdeToJoyPalette.RosePink.ToVector3() * 0.5f);
        }

        /// <summary>
        /// Chain VFX between enemies: gradient particles along line from GoldenPollen to VerdantGreen.
        /// </summary>
        public static void GloryVineChainVFX(Vector2 from, Vector2 to)
        {
            if (Main.dedServ) return;

            float dist = Vector2.Distance(from, to);
            int segments = (int)(dist / 14f);
            Vector2 dir = (to - from).SafeNormalize(Vector2.UnitX);

            for (int i = 0; i < segments; i++)
            {
                float t = (float)i / segments;
                Vector2 pos = Vector2.Lerp(from, to, t) + Main.rand.NextVector2Circular(4f, 4f);
                Color col = Color.Lerp(OdeToJoyPalette.GoldenPollen, OdeToJoyPalette.VerdantGreen, t);

                var glow = new GenericGlowParticle(pos,
                    dir * 0.3f + Main.rand.NextVector2Circular(0.3f, 0.3f),
                    col * 0.55f, 0.14f, 12, true);
                MagnumParticleHandler.SpawnParticle(glow);

                // Vine dust along the chain
                if (Main.rand.NextBool(3))
                {
                    Dust d = Dust.NewDustPerfect(pos, DustID.GreenTorch,
                        dir * 0.2f, 0, col, 0.7f);
                    d.noGravity = true;
                }
            }

            // Flare at target end
            OdeToJoyVFXLibrary.BloomFlare(to, OdeToJoyPalette.VerdantGreen, 0.25f);

            Lighting.AddLight(to, OdeToJoyPalette.VerdantGreen.ToVector3() * 0.4f);
        }

        /// <summary>
        /// Shard death: rose petals and bloom burst.
        /// </summary>
        public static void GloryShardDeathVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            OdeToJoyVFXLibrary.SpawnRosePetals(pos);
            OdeToJoyVFXLibrary.BloomBurst(pos);

            // Fading petal sparkles
            for (int i = 0; i < 3; i++)
            {
                float angle = MathHelper.TwoPi * i / 3f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(1.5f, 3f);
                Color col = OdeToJoyPalette.GetPetalGradient((float)i / 3f);
                var glow = new GenericGlowParticle(pos, vel, col * 0.5f, 0.16f, 14, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }

            Lighting.AddLight(pos, OdeToJoyPalette.RosePink.ToVector3() * 0.5f);
        }

        /// <summary>
        /// Per-frame glory beam projectile trail: heavy, 3 GenericGlowParticle per frame,
        /// SparkleParticle accent, and music notes.
        /// </summary>
        public static void GloryBeamTrailVFX(Vector2 center, Vector2 velocity)
        {
            if (Main.dedServ) return;

            Vector2 awayDir = -velocity.SafeNormalize(Vector2.Zero);

            // Heavy trail: 3 glow particles per frame
            for (int i = 0; i < 3; i++)
            {
                float gradientT = ((Main.GameUpdateCount * 0.02f) + i * 0.3f) % 1f;
                Color col = OdeToJoyPalette.GetGardenGradient(gradientT);
                var trail = new GenericGlowParticle(
                    center + Main.rand.NextVector2Circular(5f, 5f),
                    awayDir * Main.rand.NextFloat(0.5f, 2f) + Main.rand.NextVector2Circular(0.5f, 0.5f),
                    col * 0.55f, 0.22f, 16, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }

            // Sparkle particle accent
            if (Main.rand.NextBool(3))
            {
                Color sparkleCol = Color.Lerp(OdeToJoyPalette.GoldenPollen, Color.White,
                    Main.rand.NextFloat(0.3f));
                var sparkle = new SparkleParticle(
                    center + Main.rand.NextVector2Circular(6f, 6f),
                    awayDir * 0.5f + Main.rand.NextVector2Circular(0.3f, 0.3f),
                    sparkleCol * 0.6f, 0.18f, 14);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            // Music notes along beam trail
            if (Main.rand.NextBool(5))
                OdeToJoyVFXLibrary.SpawnMusicNotes(center, 1, 8f, 0.7f, 0.9f, 20);

            Lighting.AddLight(center, OdeToJoyPalette.GoldenPollen.ToVector3() * 0.5f);
        }

        /// <summary>
        /// 3-layer PreDraw bloom for glory beam: GoldenPollen, RosePink, White pulsing.
        /// </summary>
        public static void GloryBeamPreDraw(
            SpriteBatch sb, Texture2D tex,
            Vector2 drawPos, Vector2 origin, float rotation, float scale)
        {
            float time = (float)Main.timeForVisualEffects;
            float pulse = 1f + MathF.Sin(time * 0.06f) * 0.05f;

            // Layer 1: Outer golden pollen aura
            sb.Draw(tex, drawPos, null,
                OdeToJoyPalette.GoldenPollen with { A = 0 } * 0.40f,
                rotation, origin, scale * 1.12f * pulse, SpriteEffects.None, 0f);

            // Layer 2: Middle rose pink glow
            sb.Draw(tex, drawPos, null,
                OdeToJoyPalette.RosePink with { A = 0 } * 0.30f,
                rotation, origin, scale * 1.06f * pulse, SpriteEffects.None, 0f);

            // Layer 3: Inner white core
            sb.Draw(tex, drawPos, null,
                Color.White with { A = 0 } * 0.22f,
                rotation, origin, scale * 1.01f * pulse, SpriteEffects.None, 0f);
        }

        /// <summary>
        /// Beam hits NPC: garden impact at 0.8 intensity.
        /// </summary>
        public static void GloryBeamImpactVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            OdeToJoyVFXLibrary.GardenImpact(pos, 0.8f);

            Lighting.AddLight(pos, OdeToJoyPalette.GoldenPollen.ToVector3() * 0.7f);
        }

        /// <summary>
        /// Beam death: garden impact at 0.85 intensity.
        /// </summary>
        public static void GloryBeamDeathVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            OdeToJoyVFXLibrary.GardenImpact(pos, 0.85f);

            // Death sparkles dispersing
            for (int i = 0; i < 5; i++)
            {
                float angle = MathHelper.TwoPi * i / 5f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 4f);
                Color col = OdeToJoyPalette.GetGardenGradient((float)i / 5f);
                var glow = new GenericGlowParticle(pos, vel, col * 0.5f, 0.18f, 16, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }

            Lighting.AddLight(pos, OdeToJoyPalette.GoldenPollen.ToVector3() * 0.8f);
        }
    }
}
