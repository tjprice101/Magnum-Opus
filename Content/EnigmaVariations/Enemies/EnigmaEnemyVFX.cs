using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.EnigmaVariations.Enemies
{
    /// <summary>
    /// VFX helper for ALL Enigma Variations enemies.
    /// Provides attack VFX, ambient aura, death effects, and boss-phase VFX
    /// for Mystery's End and any future Enigma enemies.
    /// Self-contained — no longer depends on deleted EnigmaVFXLibrary.
    /// </summary>
    public static class EnigmaEnemyVFX
    {
        // =====================================================================
        //  AMBIENT AURA — per-frame NPC ambient VFX
        // =====================================================================

        public static void AmbientAuraVFX(NPC npc, float intensity = 1f)
        {
            if (Main.dedServ) return;

            if (Main.rand.NextBool(5))
            {
                Vector2 mistPos = npc.Center + Main.rand.NextVector2Circular(npc.width * 0.6f, npc.height * 0.6f);
                Color mistColor = EnigmaPalette.GetVoidGradient(Main.rand.NextFloat()) * 0.35f * intensity;
                var mist = new GenericGlowParticle(mistPos, Main.rand.NextVector2Circular(0.5f, 0.5f),
                    mistColor, 0.2f, 20, true);
                MagnumParticleHandler.SpawnParticle(mist);
            }

            if (Main.rand.NextBool(15))
            {
                float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                float radius = Math.Max(npc.width, npc.height) * 0.5f + 15f;
                Vector2 eyePos = npc.Center + angle.ToRotationVector2() * radius;
                CustomParticles.EnigmaEyeGaze(eyePos, EnigmaPalette.EyeGreen * 0.5f * intensity, 0.25f);
            }

            if (Main.rand.NextBool(25))
            {
                Vector2 glyphPos = npc.Center + Main.rand.NextVector2Circular(npc.width * 0.4f, npc.height * 0.4f);
                CustomParticles.Glyph(glyphPos, EnigmaPalette.GlyphPurple * intensity, 0.2f);
            }

            float pulse = MathF.Sin(Main.GameUpdateCount * 0.06f) * 0.1f + 0.9f;
            Lighting.AddLight(npc.Center, EnigmaPalette.Purple.ToVector3() * 0.4f * intensity * pulse);
        }

        // =====================================================================
        //  ATTACK VFX — per-attack type visual effects
        // =====================================================================

        public static void ParadoxGazeVFX(Vector2 targetCenter, int eyeCount = 6)
        {
            if (Main.dedServ) return;

            SpawnWatchingEyes(targetCenter, eyeCount, 50f, 0.4f);
            CustomParticles.GenericFlare(targetCenter, EnigmaPalette.EyeGreen * 0.6f, 0.4f, 14);
            SpawnGlyphCircle(targetCenter, 4, 45f);
            Lighting.AddLight(targetCenter, EnigmaPalette.EyeGreen.ToVector3() * 0.5f);
        }

        public static void GlyphCascadeVFX(Vector2 sourcePos, int glyphCount = 8)
        {
            if (Main.dedServ) return;

            for (int i = 0; i < glyphCount; i++)
            {
                float xOffset = Main.rand.NextFloat(-80f, 80f);
                Vector2 glyphPos = sourcePos + new Vector2(xOffset, -60f);
                Color col = EnigmaPalette.GetEnigmaGradient(Main.rand.NextFloat());
                CustomParticles.Glyph(glyphPos, col, 0.25f + Main.rand.NextFloat(0.1f));
            }

            CustomParticles.GenericFlare(sourcePos, EnigmaPalette.Purple, 0.5f, 14);
            SpawnMusicNotes(sourcePos, 3, 30f);
        }

        public static void WatchingVolleyLaunchVFX(Vector2 launchPos, Vector2 direction)
        {
            if (Main.dedServ) return;

            CustomParticles.GenericFlare(launchPos, EnigmaPalette.GreenFlame, 0.5f, 12);
            CustomParticles.HaloRing(launchPos, EnigmaPalette.Purple, 0.25f, 10);

            for (int i = 0; i < 2; i++)
            {
                Vector2 eyePos = launchPos + direction * (10f + i * 12f);
                CustomParticles.EnigmaEyeGaze(eyePos, EnigmaPalette.EyeGreen * 0.6f, 0.3f,
                    launchPos + direction * 100f);
            }
        }

        public static void MysteryVortexVFX(Vector2 vortexCenter, float radius)
        {
            if (Main.dedServ) return;

            SpawnVoidSwirl(vortexCenter, 8, radius * 1.5f);
            SpawnGlyphCircle(vortexCenter, 8, radius);
            CustomParticles.EnigmaEyeGaze(vortexCenter, EnigmaPalette.EyeGreen * 0.7f, 0.4f);
            BloomRenderer.DrawBloomStackAdditive(vortexCenter, EnigmaPalette.DeepPurple, EnigmaPalette.GreenFlame,
                0.4f, 0.6f);
            Lighting.AddLight(vortexCenter, EnigmaPalette.Purple.ToVector3() * 0.6f);
        }

        public static void EnigmaRevelationVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            CustomParticles.GenericFlare(pos, Color.White, 1.0f, 22);
            CustomParticles.GenericFlare(pos, EnigmaPalette.WhiteGreenFlash, 0.8f, 20);
            CustomParticles.GenericFlare(pos, EnigmaPalette.GreenFlame, 0.6f, 18);

            CustomParticles.EnigmaEyeExplosion(pos, EnigmaPalette.Purple, 10, 6f);
            SpawnGlyphCircle(pos, 10, 80f);
            SpawnGlyphBurst(pos, 16, 8f);
            SpawnVoidSwirl(pos, 10, 80f);
            SpawnGradientHaloRings(pos, 7, 0.35f);
            SpawnRadialDustBurst(pos, 20, 8f);
            DrawBloom(pos, 0.9f);
            SpawnMusicNotes(pos, 8, 50f);

            MagnumScreenEffects.AddScreenShake(8f);
            Lighting.AddLight(pos, EnigmaPalette.WhiteGreenFlash.ToVector3() * 1.5f);
        }

        // =====================================================================
        //  DEATH VFX — enemy death effects
        // =====================================================================

        public static void DeathVFX(Vector2 pos, float intensity = 1f)
        {
            if (Main.dedServ) return;

            // Impact flash + radial dust
            CustomParticles.GenericFlare(pos, EnigmaPalette.GreenFlame, 0.6f * intensity, 16);
            CustomParticles.HaloRing(pos, EnigmaPalette.Purple * intensity, 0.4f, 14);
            SpawnRadialDustBurst(pos, 12, 5f * intensity);

            SpawnWatchingEyes(pos, 5, 40f * intensity, 0.3f);
            SpawnVoidSwirl(pos, 6, 50f * intensity);

            Lighting.AddLight(pos, EnigmaPalette.GreenFlame.ToVector3() * 0.8f * intensity);
        }

        public static void BossDeathVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            DeathVFX(pos, 1.5f);
            CustomParticles.EnigmaEyeExplosion(pos, EnigmaPalette.Purple, 12, 7f);
            SpawnGlyphCircle(pos, 10, 80f);
            SpawnGlyphBurst(pos, 20, 10f);
            DrawBloom(pos, 1.0f);
            MagnumScreenEffects.AddScreenShake(10f);
        }

        // =====================================================================
        //  PROJECTILE TRAIL VFX — for enemy projectiles
        // =====================================================================

        public static void EnemyProjectileTrailVFX(Vector2 pos, Vector2 velocity)
        {
            if (Main.dedServ) return;

            if (Main.rand.NextBool(2))
            {
                int dustType = Main.rand.NextBool() ? DustID.PurpleTorch : DustID.GreenTorch;
                Dust d = Dust.NewDustPerfect(pos, dustType,
                    -velocity * 0.1f + Main.rand.NextVector2Circular(0.5f, 0.5f), 0, default, 1.0f);
                d.noGravity = true;
            }

            if (Main.rand.NextBool(10))
                CustomParticles.EnigmaEyeGaze(pos + Main.rand.NextVector2Circular(5f, 5f),
                    EnigmaPalette.EyeGreen * 0.4f, 0.2f);

            Lighting.AddLight(pos, EnigmaPalette.Purple.ToVector3() * 0.25f);
        }

        public static void EnemyProjectileImpactVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            CustomParticles.GenericFlare(pos, EnigmaPalette.GreenFlame, 0.4f, 12);
            CustomParticles.HaloRing(pos, EnigmaPalette.Purple, 0.25f, 10);
            SpawnRadialDustBurst(pos, 8, 4f);
            SpawnMusicNotes(pos, 1, 10f);

            Lighting.AddLight(pos, EnigmaPalette.GreenFlame.ToVector3() * 0.5f);
        }

        // =====================================================================
        //  EYE GLOW EFFECT — pulsing eye per-frame
        // =====================================================================

        public static float UpdateEyeGlow(float currentGlow, float time, float baseIntensity = 0.6f)
        {
            float target = baseIntensity + MathF.Sin(time * 0.05f) * 0.2f;
            return MathHelper.Lerp(currentGlow, target, 0.05f);
        }

        public static float UpdateAuraPulse(float currentPulse, float time)
        {
            float target = MathF.Sin(time * 0.04f) * 0.5f + 0.5f;
            return MathHelper.Lerp(currentPulse, target, 0.03f);
        }

        // =====================================================================
        //  INLINE VFX HELPERS (replaces deleted EnigmaVFXLibrary)
        // =====================================================================

        private static void SpawnWatchingEyes(Vector2 center, int count, float radius, float scale)
        {
            for (int i = 0; i < count; i++)
            {
                float angle = MathHelper.TwoPi * i / count + Main.rand.NextFloat(-0.2f, 0.2f);
                Vector2 pos = center + angle.ToRotationVector2() * (radius + Main.rand.NextFloat(-10f, 10f));
                CustomParticles.EnigmaEyeGaze(pos, EnigmaPalette.EyeGreen * 0.6f, scale, center);
            }
        }

        private static void SpawnGlyphCircle(Vector2 center, int count, float radius)
        {
            for (int i = 0; i < count; i++)
            {
                float angle = MathHelper.TwoPi * i / count;
                Vector2 pos = center + angle.ToRotationVector2() * radius;
                Color col = EnigmaPalette.GetEnigmaGradient((float)i / count);
                CustomParticles.Glyph(pos, col, 0.3f);
            }
        }

        private static void SpawnGlyphBurst(Vector2 center, int count, float speed)
        {
            for (int i = 0; i < count; i++)
            {
                float angle = MathHelper.TwoPi * i / count;
                Vector2 pos = center + angle.ToRotationVector2() * 10f;
                Color col = EnigmaPalette.GetEnigmaGradient((float)i / count);
                CustomParticles.Glyph(pos, col, 0.25f + Main.rand.NextFloat(0.1f));
            }
        }

        private static void SpawnVoidSwirl(Vector2 center, int count, float radius)
        {
            for (int i = 0; i < count; i++)
            {
                float angle = MathHelper.TwoPi * i / count + Main.rand.NextFloat(-0.3f, 0.3f);
                float dist = radius * Main.rand.NextFloat(0.4f, 1f);
                Vector2 pos = center + angle.ToRotationVector2() * dist;
                Vector2 vel = (center - pos).SafeNormalize(Vector2.Zero) * 2f + Main.rand.NextVector2Circular(0.5f, 0.5f);
                Color col = EnigmaPalette.GetVoidGradient(Main.rand.NextFloat()) * 0.6f;
                var glow = new GenericGlowParticle(pos, vel, col, Main.rand.NextFloat(0.2f, 0.4f),
                    Main.rand.Next(25, 45), true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
        }

        private static void SpawnGradientHaloRings(Vector2 center, int count, float scale)
        {
            for (int i = 0; i < count; i++)
            {
                Color col = EnigmaPalette.GetEnigmaGradient((float)i / count);
                CustomParticles.HaloRing(center, col, scale + i * 0.05f, 18 + i * 2);
            }
        }

        private static void SpawnRadialDustBurst(Vector2 center, int count, float speed)
        {
            for (int i = 0; i < count; i++)
            {
                float angle = MathHelper.TwoPi * i / count;
                Vector2 vel = angle.ToRotationVector2() * speed * Main.rand.NextFloat(0.7f, 1.3f);
                int dustType = Main.rand.NextBool() ? DustID.PurpleTorch : DustID.GreenTorch;
                Dust d = Dust.NewDustPerfect(center, dustType, vel, 0, default, 1.2f);
                d.noGravity = true;
            }
        }

        private static void SpawnMusicNotes(Vector2 center, int count, float radius)
        {
            for (int i = 0; i < count; i++)
            {
                float angle = MathHelper.TwoPi * i / count + Main.rand.NextFloat(-0.3f, 0.3f);
                Vector2 pos = center + angle.ToRotationVector2() * Main.rand.NextFloat(5f, radius);
                Color col = EnigmaPalette.GetEnigmaGradient(Main.rand.NextFloat());
                CustomParticles.Glyph(pos, col, 0.2f);
            }
        }

        private static void DrawBloom(Vector2 center, float intensity)
        {
            BloomRenderer.DrawBloomStackAdditive(center, EnigmaPalette.DeepPurple * intensity,
                EnigmaPalette.GreenFlame * intensity, 0.5f * intensity, 0.8f * intensity);
        }

        private static NPC FindNearestPlayer(Vector2 center)
        {
            return null;
        }
    }
}
