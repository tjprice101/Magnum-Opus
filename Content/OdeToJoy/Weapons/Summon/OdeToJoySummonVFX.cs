using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Bloom;
using MagnumOpus.Common.Systems.VFX.Screen;

namespace MagnumOpus.Content.OdeToJoy.Weapons.Summon
{
    /// <summary>
    /// Per-weapon VFX for all 3 Ode to Joy summon weapons.
    ///
    /// TheStandingOvation   — Applauding spirit minions, joy waves, 2600 damage.
    ///                        Identity: joyful celebration, golden aura, standing ovation energy.
    ///
    /// FountainOfJoyousHarmony — Stationary healing fountain minion, 2200 damage.
    ///                        Identity: healing water fountain, rising nature energy, garden harmony.
    ///
    /// TriumphantChorus     — Ultimate summon, choir spirits with harmonic blasts, 3400 damage, 2 slots.
    ///                        Identity: ultimate musical power, triumphant finale, grand chorus.
    ///
    /// All methods delegate to OdeToJoyPalette for colors and OdeToJoyVFXLibrary for
    /// shared impact/dust/note effects. No hardcoded colors.
    /// </summary>
    public static class OdeToJoySummonVFX
    {
        // =====================================================================
        //  THE STANDING OVATION
        //  Applauding spirit minions that hover and release joy waves.
        //  2600 damage. Joyful celebration, golden aura, standing ovation energy.
        // =====================================================================

        /// <summary>
        /// Per-frame held-item VFX: ambient golden music notes floating near player,
        /// spawned every 20 frames.
        /// </summary>
        public static void OvationHoldItemVFX(Player player)
        {
            if (Main.dedServ) return;

            Vector2 center = player.MountedCenter;

            // Ambient golden music notes every 20 frames
            if (Main.GameUpdateCount % 20 == 0)
            {
                Vector2 tipOffset = new Vector2(player.direction * 22f, -8f);
                Vector2 notePos = center + tipOffset + Main.rand.NextVector2Circular(8f, 8f);
                OdeToJoyVFXLibrary.SpawnMusicNotes(notePos, 1, 10f, 0.65f, 0.85f, 25);
            }

            // Subtle golden pollen motes
            if (Main.rand.NextBool(8))
            {
                Vector2 motePos = center + Main.rand.NextVector2Circular(14f, 14f);
                Vector2 vel = new Vector2(0f, -0.3f) + Main.rand.NextVector2Circular(0.3f, 0.3f);
                Color col = OdeToJoyPalette.GoldenPollen * 0.4f;
                var glow = new GenericGlowParticle(motePos, vel, col, 0.10f, 16, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }

            float pulse = 0.2f + MathF.Sin((float)Main.timeForVisualEffects * 0.04f) * 0.08f;
            Lighting.AddLight(center, OdeToJoyPalette.GoldenPollen.ToVector3() * pulse);
        }

        /// <summary>
        /// Entrance VFX when the ovation minion is summoned.
        /// GardenImpact at full scale, MusicNoteBurst, and golden pollen sparkles.
        /// </summary>
        public static void OvationSummonVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            OdeToJoyVFXLibrary.GardenImpact(pos, 1.0f);
            OdeToJoyVFXLibrary.MusicNoteBurst(pos, OdeToJoyPalette.GoldenPollen, 8, 4f);
            OdeToJoyVFXLibrary.SpawnPollenSparkles(pos, 10, 35f);

            Lighting.AddLight(pos, OdeToJoyPalette.GoldenPollen.ToVector3() * 1.0f);
        }

        /// <summary>
        /// Minion idle ambient particles: garden gradient GenericGlowParticle every 6 frames,
        /// golden Lighting.AddLight.
        /// </summary>
        public static void OvationMinionAmbientVFX(Vector2 center)
        {
            if (Main.dedServ) return;

            if (Main.GameUpdateCount % 6 == 0)
            {
                float gradientT = (Main.GameUpdateCount * 0.015f) % 1f;
                Color col = OdeToJoyPalette.GetGardenGradient(gradientT);
                Vector2 vel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -0.4f - Main.rand.NextFloat(0.3f));
                var glow = new GenericGlowParticle(
                    center + Main.rand.NextVector2Circular(8f, 8f),
                    vel, col * 0.5f, 0.14f, 18, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }

            Lighting.AddLight(center, OdeToJoyPalette.GoldenPollen.ToVector3() * 0.4f);
        }

        /// <summary>
        /// Glow behind minion: 1 additive bloom layer, GoldenPollen * 0.4f at scale * 1.3f.
        /// </summary>
        public static void OvationMinionPreDraw(SpriteBatch sb, Texture2D tex,
            Vector2 drawPos, Vector2 origin, float scale, SpriteEffects effects)
        {
            // Single additive bloom layer behind the minion
            sb.Draw(tex, drawPos, null,
                OdeToJoyPalette.GoldenPollen with { A = 0 } * 0.40f,
                0f, origin, scale * 1.3f, effects, 0f);
        }

        /// <summary>
        /// On attack fire: MusicalImpact at 0.5 intensity.
        /// </summary>
        public static void OvationMinionAttackVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            OdeToJoyVFXLibrary.MusicalImpact(pos, 0.5f);

            Lighting.AddLight(pos, OdeToJoyPalette.GoldenPollen.ToVector3() * 0.5f);
        }

        /// <summary>
        /// Joy wave projectile trail: garden gradient GenericGlowParticle,
        /// occasional music notes.
        /// </summary>
        public static void JoyWaveTrailVFX(Vector2 center, Vector2 velocity)
        {
            if (Main.dedServ) return;

            Vector2 awayDir = -velocity.SafeNormalize(Vector2.Zero);
            float gradientT = (Main.GameUpdateCount * 0.02f) % 1f;
            Color trailCol = OdeToJoyPalette.GetGardenGradient(gradientT);

            var trail = new GenericGlowParticle(
                center + Main.rand.NextVector2Circular(4f, 4f),
                awayDir * Main.rand.NextFloat(0.5f, 1.5f) + Main.rand.NextVector2Circular(0.3f, 0.3f),
                trailCol * 0.5f, 0.16f, 14, true);
            MagnumParticleHandler.SpawnParticle(trail);

            // Occasional music notes along the trail
            if (Main.rand.NextBool(5))
                OdeToJoyVFXLibrary.SpawnMusicNotes(center, 1, 6f, 0.65f, 0.85f, 20);

            Lighting.AddLight(center, OdeToJoyPalette.GoldenPollen.ToVector3() * 0.3f);
        }

        /// <summary>
        /// 2-layer PreDraw bloom for joy wave projectile: GoldenPollen outer, White core.
        /// </summary>
        public static void JoyWavePreDraw(SpriteBatch sb, Texture2D tex,
            Vector2 drawPos, Vector2 origin, float rotation, float scale)
        {
            float time = (float)Main.timeForVisualEffects;
            float pulse = 1f + MathF.Sin(time * 0.06f) * 0.05f;

            // Layer 1: Outer golden pollen aura
            sb.Draw(tex, drawPos, null,
                OdeToJoyPalette.GoldenPollen with { A = 0 } * 0.40f,
                rotation, origin, scale * 1.10f * pulse, SpriteEffects.None, 0f);

            // Layer 2: Inner white core
            sb.Draw(tex, drawPos, null,
                Color.White with { A = 0 } * 0.25f,
                rotation, origin, scale * 1.02f * pulse, SpriteEffects.None, 0f);
        }

        /// <summary>
        /// Joy wave hits NPC: MusicalImpact at 0.5 intensity.
        /// </summary>
        public static void JoyWaveImpactVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            OdeToJoyVFXLibrary.MusicalImpact(pos, 0.5f);

            Lighting.AddLight(pos, OdeToJoyPalette.GoldenPollen.ToVector3() * 0.5f);
        }

        /// <summary>
        /// Joy wave death: SpawnRosePetals and BloomBurst.
        /// </summary>
        public static void JoyWaveDeathVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            OdeToJoyVFXLibrary.SpawnRosePetals(pos);
            OdeToJoyVFXLibrary.BloomBurst(pos);

            // Fading sparkles on death
            for (int i = 0; i < 3; i++)
            {
                float angle = MathHelper.TwoPi * i / 3f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(1.5f, 3f);
                Color col = OdeToJoyPalette.GetGardenGradient((float)i / 3f);
                var glow = new GenericGlowParticle(pos, vel, col * 0.5f, 0.14f, 14, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }

            Lighting.AddLight(pos, OdeToJoyPalette.RosePink.ToVector3() * 0.5f);
        }

        // =====================================================================
        //  FOUNTAIN OF JOYOUS HARMONY
        //  Stationary healing fountain minion. 2200 damage.
        //  Identity: healing water fountain, rising nature energy, garden harmony.
        // =====================================================================

        /// <summary>
        /// Per-frame held-item VFX: subtle verdant glow particles near player.
        /// </summary>
        public static void FountainHoldItemVFX(Player player)
        {
            if (Main.dedServ) return;

            Vector2 center = player.MountedCenter;

            // Subtle verdant glow motes drifting upward
            if (Main.rand.NextBool(6))
            {
                Vector2 motePos = center + Main.rand.NextVector2Circular(12f, 12f);
                Vector2 vel = new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), -0.5f - Main.rand.NextFloat(0.3f));
                Color col = Color.Lerp(OdeToJoyPalette.VerdantGreen, OdeToJoyPalette.BudGreen,
                    Main.rand.NextFloat()) * 0.4f;
                var glow = new GenericGlowParticle(motePos, vel, col, 0.10f, 16, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }

            float pulse = 0.18f + MathF.Sin((float)Main.timeForVisualEffects * 0.04f) * 0.06f;
            Lighting.AddLight(center, OdeToJoyPalette.VerdantGreen.ToVector3() * pulse);
        }

        /// <summary>
        /// Entrance VFX when the fountain minion is summoned.
        /// TriumphantCelebration at 0.8f, 12 radial water burst particles (green-pink),
        /// 6 rising music notes.
        /// </summary>
        public static void FountainSummonVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            OdeToJoyVFXLibrary.TriumphantCelebration(pos, 0.8f);

            // 12 radial water burst particles (green-pink GenericGlowParticle)
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f);
                Color col = Color.Lerp(OdeToJoyPalette.VerdantGreen, OdeToJoyPalette.RosePink,
                    (float)i / 12f);
                var burst = new GenericGlowParticle(pos, vel, col * 0.6f, 0.22f, 18, true);
                MagnumParticleHandler.SpawnParticle(burst);
            }

            // 6 rising music notes
            for (int i = 0; i < 6; i++)
            {
                Vector2 offset = new Vector2(Main.rand.NextFloat(-20f, 20f), 0f);
                OdeToJoyVFXLibrary.SpawnMusicNotes(pos + offset, 1, 8f, 0.7f, 0.95f, 30);
            }

            Lighting.AddLight(pos, OdeToJoyPalette.VerdantGreen.ToVector3() * 1.0f);
        }

        /// <summary>
        /// Fountain minion ambient: rising verdant-pink GenericGlowParticle floating upward,
        /// golden music notes every 10 frames.
        /// </summary>
        public static void FountainMinionAmbientVFX(Vector2 center)
        {
            if (Main.dedServ) return;

            // Rising water/nature particles
            if (Main.GameUpdateCount % 4 == 0)
            {
                Color col = Color.Lerp(OdeToJoyPalette.VerdantGreen, OdeToJoyPalette.RosePink,
                    Main.rand.NextFloat());
                Vector2 vel = new Vector2(Main.rand.NextFloat(-0.4f, 0.4f), -1.0f - Main.rand.NextFloat(0.5f));
                var glow = new GenericGlowParticle(
                    center + Main.rand.NextVector2Circular(10f, 6f),
                    vel, col * 0.45f, 0.14f, 20, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }

            // Golden music notes every 10 frames
            if (Main.GameUpdateCount % 10 == 0)
            {
                OdeToJoyVFXLibrary.SpawnMusicNotes(center + new Vector2(0f, -12f), 1, 10f, 0.65f, 0.85f, 25);
            }

            Lighting.AddLight(center, OdeToJoyPalette.GoldenPollen.ToVector3() * 0.35f);
        }

        /// <summary>
        /// Glow behind fountain minion: VerdantGreen * 0.4f, pulsing with sin wave.
        /// </summary>
        public static void FountainMinionPreDraw(SpriteBatch sb, Texture2D tex,
            Vector2 drawPos, Vector2 origin, float scale)
        {
            float time = (float)Main.timeForVisualEffects;
            float pulse = 1f + MathF.Sin(time * 0.04f) * 0.08f;

            // Single glow layer with pulsation
            sb.Draw(tex, drawPos, null,
                OdeToJoyPalette.VerdantGreen with { A = 0 } * 0.40f * pulse,
                0f, origin, scale * 1.25f, SpriteEffects.None, 0f);
        }

        /// <summary>
        /// Healing effect: 3 ascending verdant music notes rising from the player,
        /// green Lighting.AddLight.
        /// </summary>
        public static void FountainHealVFX(Vector2 playerCenter)
        {
            if (Main.dedServ) return;

            // 3 ascending verdant music notes
            for (int i = 0; i < 3; i++)
            {
                Vector2 offset = new Vector2(Main.rand.NextFloat(-12f, 12f), 0f);
                Vector2 vel = new Vector2(Main.rand.NextFloat(-0.4f, 0.4f), -1.8f - Main.rand.NextFloat(0.8f));
                Color col = Color.Lerp(OdeToJoyPalette.VerdantGreen, OdeToJoyPalette.GoldenPollen,
                    Main.rand.NextFloat(0.3f));

                OdeToJoyVFXLibrary.SpawnMusicNotes(playerCenter + offset, 1, 5f, 0.7f, 0.9f, 28);

                // Ascending green glow
                var glow = new GenericGlowParticle(playerCenter + offset, vel,
                    col * 0.5f, 0.16f, 20, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }

            Lighting.AddLight(playerCenter, OdeToJoyPalette.VerdantGreen.ToVector3() * 0.6f);
        }

        /// <summary>
        /// Fountain shoots water projectile: SpawnVineTrailDust and a small BloomFlare.
        /// </summary>
        public static void FountainAttackVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            OdeToJoyVFXLibrary.SpawnVineTrailDust(pos, Main.rand.NextVector2Circular(2f, 2f));
            OdeToJoyVFXLibrary.BloomFlare(pos, OdeToJoyPalette.VerdantGreen, 0.3f);

            // Small verdant spark ring
            for (int i = 0; i < 4; i++)
            {
                float angle = MathHelper.TwoPi * i / 4f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(1f, 2.5f);
                Color col = Color.Lerp(OdeToJoyPalette.VerdantGreen, OdeToJoyPalette.RosePink,
                    (float)i / 4f);
                var spark = new GenericGlowParticle(pos, vel, col * 0.45f, 0.12f, 12, true);
                MagnumParticleHandler.SpawnParticle(spark);
            }

            Lighting.AddLight(pos, OdeToJoyPalette.VerdantGreen.ToVector3() * 0.5f);
        }

        /// <summary>
        /// Water arc projectile trail: verdant-pink GenericGlowParticle.
        /// </summary>
        public static void FountainWaterTrailVFX(Vector2 center, Vector2 velocity)
        {
            if (Main.dedServ) return;

            Vector2 awayDir = -velocity.SafeNormalize(Vector2.Zero);
            Color trailCol = Color.Lerp(OdeToJoyPalette.VerdantGreen, OdeToJoyPalette.RosePink,
                Main.rand.NextFloat());

            var trail = new GenericGlowParticle(
                center + Main.rand.NextVector2Circular(3f, 3f),
                awayDir * Main.rand.NextFloat(0.5f, 1.2f) + Main.rand.NextVector2Circular(0.3f, 0.3f),
                trailCol * 0.5f, 0.15f, 14, true);
            MagnumParticleHandler.SpawnParticle(trail);

            // Faint verdant dust accent
            if (Main.rand.NextBool(4))
            {
                Dust d = Dust.NewDustPerfect(center, DustID.GreenTorch,
                    awayDir * Main.rand.NextFloat(0.5f, 1f), 0,
                    OdeToJoyPalette.VerdantGreen, 0.7f);
                d.noGravity = true;
            }

            Lighting.AddLight(center, OdeToJoyPalette.VerdantGreen.ToVector3() * 0.25f);
        }

        /// <summary>
        /// 2-layer PreDraw bloom for water projectile: VerdantGreen outer, White core.
        /// </summary>
        public static void FountainWaterPreDraw(SpriteBatch sb, Texture2D tex,
            Vector2 drawPos, Vector2 origin, float rotation, float scale)
        {
            float time = (float)Main.timeForVisualEffects;
            float pulse = 1f + MathF.Sin(time * 0.06f) * 0.05f;

            // Layer 1: Outer verdant green aura
            sb.Draw(tex, drawPos, null,
                OdeToJoyPalette.VerdantGreen with { A = 0 } * 0.40f,
                rotation, origin, scale * 1.10f * pulse, SpriteEffects.None, 0f);

            // Layer 2: Inner white core
            sb.Draw(tex, drawPos, null,
                Color.White with { A = 0 } * 0.25f,
                rotation, origin, scale * 1.02f * pulse, SpriteEffects.None, 0f);
        }

        /// <summary>
        /// Water hit NPC: BlossomImpact at 0.4 intensity.
        /// </summary>
        public static void FountainWaterImpactVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            OdeToJoyVFXLibrary.BlossomImpact(pos, 0.4f);

            Lighting.AddLight(pos, OdeToJoyPalette.VerdantGreen.ToVector3() * 0.4f);
        }

        /// <summary>
        /// Water death: SpawnVineTrailDust and small BloomBurst.
        /// </summary>
        public static void FountainWaterDeathVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            OdeToJoyVFXLibrary.SpawnVineTrailDust(pos, Main.rand.NextVector2Circular(2f, 2f));
            OdeToJoyVFXLibrary.BloomBurst(pos, 0.4f);

            // Small verdant death sparkles
            for (int i = 0; i < 3; i++)
            {
                float angle = MathHelper.TwoPi * i / 3f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(1.5f, 3f);
                Color col = Color.Lerp(OdeToJoyPalette.VerdantGreen, OdeToJoyPalette.RosePink,
                    (float)i / 3f);
                var glow = new GenericGlowParticle(pos, vel, col * 0.45f, 0.14f, 14, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }

            Lighting.AddLight(pos, OdeToJoyPalette.VerdantGreen.ToVector3() * 0.5f);
        }

        // =====================================================================
        //  TRIUMPHANT CHORUS
        //  Ultimate summon -- choir spirits with coordinated harmonic blasts.
        //  3400 damage, 2 slots. Ultimate musical power, triumphant finale,
        //  grand chorus.
        // =====================================================================

        /// <summary>
        /// Per-frame held-item VFX: ambient garden gradient music notes,
        /// more frequent than others (every 15 frames).
        /// </summary>
        public static void ChorusHoldItemVFX(Player player)
        {
            if (Main.dedServ) return;

            Vector2 center = player.MountedCenter;

            // Garden gradient music notes every 15 frames -- more frequent than other summons
            if (Main.GameUpdateCount % 15 == 0)
            {
                Vector2 tipOffset = new Vector2(player.direction * 24f, -10f);
                Vector2 notePos = center + tipOffset + Main.rand.NextVector2Circular(8f, 8f);
                OdeToJoyVFXLibrary.SpawnMusicNotes(notePos, 1, 10f, 0.7f, 0.9f, 25);
            }

            // Garden gradient glow motes
            if (Main.rand.NextBool(6))
            {
                Vector2 motePos = center + Main.rand.NextVector2Circular(14f, 14f);
                Vector2 vel = new Vector2(0f, -0.35f) + Main.rand.NextVector2Circular(0.3f, 0.3f);
                Color col = OdeToJoyPalette.GetGardenGradient(Main.rand.NextFloat()) * 0.4f;
                var glow = new GenericGlowParticle(motePos, vel, col, 0.11f, 18, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }

            float pulse = 0.22f + MathF.Sin((float)Main.timeForVisualEffects * 0.045f) * 0.09f;
            Lighting.AddLight(center, OdeToJoyPalette.RosePink.ToVector3() * pulse);
        }

        /// <summary>
        /// Grand entrance VFX: TriumphantCelebration at 1.3 intensity.
        /// </summary>
        public static void ChorusSummonVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            OdeToJoyVFXLibrary.TriumphantCelebration(pos, 1.3f);

            Lighting.AddLight(pos, OdeToJoyPalette.GoldenPollen.ToVector3() * 1.3f);
        }

        /// <summary>
        /// Chorus minion ambient: more elaborate than others.
        /// Garden gradient GenericGlowParticle every 4 frames,
        /// music notes every 8 frames, rose pink Lighting.AddLight.
        /// </summary>
        public static void ChorusMinionAmbientVFX(Vector2 center)
        {
            if (Main.dedServ) return;

            // Garden gradient glow particles every 4 frames
            if (Main.GameUpdateCount % 4 == 0)
            {
                float gradientT = (Main.GameUpdateCount * 0.018f) % 1f;
                Color col = OdeToJoyPalette.GetGardenGradient(gradientT);
                Vector2 vel = new Vector2(Main.rand.NextFloat(-0.6f, 0.6f), -0.5f - Main.rand.NextFloat(0.4f));
                var glow = new GenericGlowParticle(
                    center + Main.rand.NextVector2Circular(10f, 10f),
                    vel, col * 0.55f, 0.16f, 20, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }

            // Music notes every 8 frames
            if (Main.GameUpdateCount % 8 == 0)
            {
                OdeToJoyVFXLibrary.SpawnMusicNotes(center, 1, 10f, 0.7f, 0.9f, 25);
            }

            Lighting.AddLight(center, OdeToJoyPalette.RosePink.ToVector3() * 0.45f);
        }

        /// <summary>
        /// 2-layer glow behind chorus minion:
        /// RosePink * 0.5f at scale * 1.4f, GoldenPollen * 0.3f at scale * 1.2f.
        /// </summary>
        public static void ChorusMinionPreDraw(SpriteBatch sb, Texture2D tex,
            Vector2 drawPos, Vector2 origin, float scale, SpriteEffects effects)
        {
            // Layer 1: Outer rose pink aura
            sb.Draw(tex, drawPos, null,
                OdeToJoyPalette.RosePink with { A = 0 } * 0.50f,
                0f, origin, scale * 1.4f, effects, 0f);

            // Layer 2: Inner golden pollen glow
            sb.Draw(tex, drawPos, null,
                OdeToJoyPalette.GoldenPollen with { A = 0 } * 0.30f,
                0f, origin, scale * 1.2f, effects, 0f);
        }

        /// <summary>
        /// Regular attack: MusicalImpact at 0.6 intensity, enhanced.
        /// </summary>
        public static void ChorusMinionAttackVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            OdeToJoyVFXLibrary.MusicalImpact(pos, 0.6f, true);

            Lighting.AddLight(pos, OdeToJoyPalette.GoldenPollen.ToVector3() * 0.6f);
        }

        /// <summary>
        /// Grand finale / ultimate VFX: TriumphantCelebration at 1.4,
        /// massive 12-note MusicNoteBurst with garden gradient.
        /// </summary>
        public static void ChorusUltimateVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            OdeToJoyVFXLibrary.TriumphantCelebration(pos, 1.4f);

            // Massive 12-note burst with garden gradient colors
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3.5f, 6f);
                Color noteCol = OdeToJoyPalette.GetGardenGradient((float)i / 12f);
                OdeToJoyVFXLibrary.SpawnMusicNote(pos, vel, noteCol, 0.85f, 35);
            }

            // Radial garden gradient glow ring
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 7f);
                Color col = OdeToJoyPalette.GetGardenGradient((float)i / 8f);
                var glow = new GenericGlowParticle(pos, vel, col * 0.6f, 0.25f, 20, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }

            // Bloom ring for the ultimate activation
            var ring = new BloomRingParticle(pos, Vector2.Zero,
                OdeToJoyPalette.GoldenPollen with { A = 0 }, 0.55f, 20);
            MagnumParticleHandler.SpawnParticle(ring);

            Lighting.AddLight(pos, OdeToJoyPalette.GoldenPollen.ToVector3() * 1.5f);
        }

        /// <summary>
        /// Harmonic blast projectile trail: garden gradient GenericGlowParticle.
        /// </summary>
        public static void HarmonicBlastTrailVFX(Vector2 center, Vector2 velocity)
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

            // Faint golden dust accent
            if (Main.rand.NextBool(3))
            {
                Dust d = Dust.NewDustPerfect(center, DustID.YellowTorch,
                    awayDir * Main.rand.NextFloat(0.5f, 1.5f), 0,
                    OdeToJoyPalette.GoldenPollen, 0.8f);
                d.noGravity = true;
            }

            Lighting.AddLight(center, OdeToJoyPalette.RosePink.ToVector3() * 0.35f);
        }

        /// <summary>
        /// 3-layer PreDraw bloom for harmonic blast: RosePink, GoldenPollen, White pulsing.
        /// </summary>
        public static void HarmonicBlastPreDraw(SpriteBatch sb, Texture2D tex,
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
        /// Harmonic blast hits NPC: BlossomImpact at 0.5 intensity.
        /// </summary>
        public static void HarmonicBlastImpactVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            OdeToJoyVFXLibrary.BlossomImpact(pos, 0.5f);

            Lighting.AddLight(pos, OdeToJoyPalette.RosePink.ToVector3() * 0.5f);
        }

        /// <summary>
        /// Harmonic blast death: MusicalImpact at 0.5 intensity.
        /// </summary>
        public static void HarmonicBlastDeathVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            OdeToJoyVFXLibrary.MusicalImpact(pos, 0.5f);

            // Fading glow sparkles on death
            for (int i = 0; i < 4; i++)
            {
                float angle = MathHelper.TwoPi * i / 4f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 3.5f);
                Color col = OdeToJoyPalette.GetGardenGradient((float)i / 4f);
                var glow = new GenericGlowParticle(pos, vel, col * 0.5f, 0.16f, 14, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }

            Lighting.AddLight(pos, OdeToJoyPalette.GoldenPollen.ToVector3() * 0.5f);
        }

        /// <summary>
        /// Grand finale projectile trail: heavy, 2 GenericGlowParticle + SparkleParticle
        /// + music notes.
        /// </summary>
        public static void GrandFinaleTrailVFX(Vector2 center, Vector2 velocity)
        {
            if (Main.dedServ) return;

            Vector2 awayDir = -velocity.SafeNormalize(Vector2.Zero);

            // Heavy trail: 2 glow particles per frame
            for (int i = 0; i < 2; i++)
            {
                float gradientT = ((Main.GameUpdateCount * 0.02f) + i * 0.4f) % 1f;
                Color col = OdeToJoyPalette.GetGardenGradient(gradientT);
                var trail = new GenericGlowParticle(
                    center + Main.rand.NextVector2Circular(5f, 5f),
                    awayDir * Main.rand.NextFloat(0.5f, 2f) + Main.rand.NextVector2Circular(0.5f, 0.5f),
                    col * 0.55f, 0.22f, 16, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }

            // SparkleParticle accent
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

            // Music notes along finale trail
            if (Main.rand.NextBool(4))
                OdeToJoyVFXLibrary.SpawnMusicNotes(center, 1, 8f, 0.7f, 0.9f, 20);

            Lighting.AddLight(center, OdeToJoyPalette.GoldenPollen.ToVector3() * 0.5f);
        }

        /// <summary>
        /// 3-layer PreDraw bloom for grand finale projectile:
        /// GoldenPollen, RosePink, White pulsing.
        /// </summary>
        public static void GrandFinalePreDraw(SpriteBatch sb, Texture2D tex,
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
        /// Grand finale hits NPC: GardenImpact at 0.8 intensity.
        /// </summary>
        public static void GrandFinaleImpactVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            OdeToJoyVFXLibrary.GardenImpact(pos, 0.8f);

            Lighting.AddLight(pos, OdeToJoyPalette.GoldenPollen.ToVector3() * 0.7f);
        }

        /// <summary>
        /// Grand finale death: GardenImpact at 0.9 intensity.
        /// </summary>
        public static void GrandFinaleDeathVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            OdeToJoyVFXLibrary.GardenImpact(pos, 0.9f);

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
