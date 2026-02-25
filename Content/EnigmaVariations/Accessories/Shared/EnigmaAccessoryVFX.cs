using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Bloom;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.EnigmaVariations.Accessories.Shared
{
    /// <summary>
    /// Shared VFX helper for ALL Enigma Variations accessories.
    /// Provides accessory-specific visuals: alchemical bubbles, mystery flames,
    /// puzzle fragment orbits, cauldron effects, and buff state VFX.
    /// Call from individual accessory files and EnigmaAccessoryPlayer.
    /// </summary>
    public static class EnigmaAccessoryVFX
    {
        // =====================================================================
        //  ALCHEMICAL PARADOX — Ranger accessory VFX
        // =====================================================================

        /// <summary>
        /// Alchemical bubble particles rising from the player.
        /// Call every 40 frames from HoldItem.
        /// </summary>
        public static void AlchemicalBubblesVFX(Player player)
        {
            if (Main.dedServ) return;

            for (int i = 0; i < 2; i++)
            {
                Vector2 bubblePos = player.Center + Main.rand.NextVector2Circular(20f, 20f);
                Vector2 vel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -Main.rand.NextFloat(1f, 2f));
                Color col = EnigmaPalette.GetEnigmaGradient(Main.rand.NextFloat()) * 0.5f;
                var glow = new GenericGlowParticle(bubblePos, vel, col, 0.2f, 20, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
        }

        /// <summary>
        /// VFX when a paradox bolt splits on hit.
        /// </summary>
        public static void ParadoxBoltSplitVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            CustomParticles.GenericFlare(pos, EnigmaPalette.GreenFlame, 0.5f, 14);
            CustomParticles.HaloRing(pos, EnigmaPalette.Purple, 0.3f, 12);
            EnigmaVFXLibrary.SpawnGlyphBurst(pos, 3, 3f);
            EnigmaVFXLibrary.SpawnMusicNotes(pos, 2, 12f, 0.7f, 0.9f, 20);

            // Watching eye at split
            CustomParticles.EnigmaEyeGaze(pos, EnigmaPalette.EyeGreen * 0.5f, 0.25f);

            Lighting.AddLight(pos, EnigmaPalette.GreenFlame.ToVector3() * 0.5f);
        }

        /// <summary>
        /// VFX when a paradoxed enemy explodes on death.
        /// </summary>
        public static void ParadoxDeathExplosionVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            EnigmaVFXLibrary.ProjectileImpact(pos, 0.8f);
            EnigmaVFXLibrary.SpawnVoidSwirl(pos, 4, 40f);
            Lighting.AddLight(pos, EnigmaPalette.GreenFlame.ToVector3() * 0.8f);
        }

        // =====================================================================
        //  IGNITION OF MYSTERY — Melee accessory VFX
        // =====================================================================

        /// <summary>
        /// Mysterious flame particles around the player while equipped.
        /// Call every 30 frames from HoldItem.
        /// </summary>
        public static void MysteryFlameAuraVFX(Player player, int mysteryStacks)
        {
            if (Main.dedServ) return;

            float stackProgress = mysteryStacks / 10f;

            // Rising mystery flames
            Vector2 flamePos = player.Center + Main.rand.NextVector2Circular(18f, 10f);
            Vector2 vel = new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), -Main.rand.NextFloat(1.5f, 2.5f));
            Color flameCol = Color.Lerp(EnigmaPalette.Purple, EnigmaPalette.GreenFlame, stackProgress) * 0.5f;
            var flame = new GenericGlowParticle(flamePos, vel, flameCol, 0.2f + stackProgress * 0.1f, 18, true);
            MagnumParticleHandler.SpawnParticle(flame);

            // Eye watching at high stacks
            if (mysteryStacks >= 6 && Main.rand.NextBool(20))
            {
                Vector2 eyePos = player.Center + Main.rand.NextVector2Circular(30f, 30f);
                CustomParticles.EnigmaEyeGaze(eyePos, EnigmaPalette.EyeGreen * 0.4f, 0.25f);
            }

            float intensity = 0.2f + mysteryStacks * 0.03f;
            Lighting.AddLight(player.Center, EnigmaPalette.Purple.ToVector3() * intensity);
        }

        /// <summary>
        /// VFX when mystery stacks reach max and the eye burst triggers.
        /// </summary>
        public static void MysteryUnveilingVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            // Central flash
            CustomParticles.GenericFlare(pos, Color.White, 0.8f, 18);
            CustomParticles.GenericFlare(pos, EnigmaPalette.GreenFlame, 0.6f, 16);

            // Eye burst — mysteries unveiled
            EnigmaVFXLibrary.SpawnEyeImpactBurst(pos, 6, 5f);
            EnigmaVFXLibrary.SpawnWatchingEyes(pos, 4, 40f, 0.35f);

            // Glyph circle
            EnigmaVFXLibrary.SpawnGlyphCircle(pos, 6, 50f, 0.07f);

            // Halo rings
            EnigmaVFXLibrary.SpawnGradientHaloRings(pos, 4, 0.3f);

            // Music notes
            EnigmaVFXLibrary.SpawnMusicNotes(pos, 5, 30f, 0.8f, 1.1f, 30);

            // Bloom
            EnigmaVFXLibrary.DrawBloom(pos, 0.6f);

            MagnumScreenEffects.AddScreenShake(3f);
            Lighting.AddLight(pos, EnigmaPalette.GreenFlame.ToVector3() * 1.0f);
        }

        // =====================================================================
        //  PENDANT OF A THOUSAND PUZZLES — Mage accessory VFX
        // =====================================================================

        /// <summary>
        /// Gentle arcane glow around pendant position (neck area).
        /// Call every 40 frames from HoldItem.
        /// </summary>
        public static void PendantGlowVFX(Player player)
        {
            if (Main.dedServ) return;

            Vector2 neckPos = player.Center + new Vector2(0, -12f);
            Color col = Color.Lerp(EnigmaPalette.Purple, EnigmaPalette.GreenFlame, Main.rand.NextFloat()) * 0.35f;
            var glow = new GenericGlowParticle(neckPos + Main.rand.NextVector2Circular(8f, 8f),
                Main.rand.NextVector2Circular(0.2f, 0.2f), col, 0.15f, 16, true);
            MagnumParticleHandler.SpawnParticle(glow);
        }

        /// <summary>
        /// Fragment counter visual — orbiting indicators.
        /// </summary>
        public static void PuzzleFragmentOrbitVFX(Player player, int fragmentCount)
        {
            if (Main.dedServ || fragmentCount <= 0) return;

            float time = Main.GameUpdateCount * 0.04f;
            for (int i = 0; i < fragmentCount; i++)
            {
                float angle = time + MathHelper.TwoPi * i / fragmentCount;
                float radius = 25f + MathF.Sin(Main.GameUpdateCount * 0.05f + i * 0.8f) * 4f;
                Vector2 fragPos = player.Center + angle.ToRotationVector2() * radius;
                Color fragColor = EnigmaPalette.GetEnigmaGradient((float)i / 5f);
                CustomParticles.Glyph(fragPos, fragColor, 0.18f);
            }
        }

        /// <summary>
        /// VFX when Puzzle Mastery buff activates (5 fragments collected).
        /// </summary>
        public static void PuzzleMasteryActivateVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            CustomParticles.GenericFlare(pos, EnigmaPalette.GreenFlame, 0.7f, 16);
            EnigmaVFXLibrary.SpawnGlyphCircle(pos, 5, 35f, 0.06f);
            EnigmaVFXLibrary.SpawnGlyphBurst(pos, 5, 4f);
            EnigmaVFXLibrary.SpawnMusicNotes(pos, 4, 25f, 0.8f, 1.0f, 28);
            EnigmaVFXLibrary.SpawnWatchingEyes(pos, 2, 30f, 0.3f);
            EnigmaVFXLibrary.DrawBloom(pos, 0.5f);

            Lighting.AddLight(pos, EnigmaPalette.GreenFlame.ToVector3() * 0.8f);
        }

        /// <summary>
        /// Per-frame VFX while Puzzle Mastery is active — glowing glyph trails.
        /// </summary>
        public static void PuzzleMasteryActiveVFX(Player player)
        {
            if (Main.dedServ) return;

            if (Main.rand.NextBool(4))
            {
                Vector2 trailPos = player.Center + Main.rand.NextVector2Circular(20f, 20f);
                CustomParticles.Glyph(trailPos, EnigmaPalette.GetEnigmaGradient(Main.rand.NextFloat()), 0.2f);
            }

            if (Main.rand.NextBool(8))
                EnigmaVFXLibrary.SpawnMusicNotes(player.Center, 1, 15f, 0.6f, 0.85f, 22);

            Lighting.AddLight(player.Center, EnigmaPalette.GreenFlame.ToVector3() * 0.4f);
        }

        // =====================================================================
        //  RIDDLEMASTER'S CAULDRON — Summoner accessory VFX
        // =====================================================================

        /// <summary>
        /// Cauldron bubble particles around the player.
        /// Call every 30 frames from HoldItem.
        /// </summary>
        public static void CauldronBubblesVFX(Player player)
        {
            if (Main.dedServ) return;

            for (int i = 0; i < 2; i++)
            {
                Vector2 bubblePos = player.Center + new Vector2(Main.rand.NextFloat(-15f, 15f), 5f);
                Vector2 vel = new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), -Main.rand.NextFloat(0.8f, 1.5f));
                Color col = Color.Lerp(EnigmaPalette.DeepPurple, EnigmaPalette.GreenFlame, Main.rand.NextFloat()) * 0.4f;
                var bubble = new GenericGlowParticle(bubblePos, vel, col, 0.15f, 22, true);
                MagnumParticleHandler.SpawnParticle(bubble);
            }
        }

        /// <summary>
        /// VFX when Mystery Vapors are released from minions.
        /// </summary>
        public static void MysteryVaporReleaseVFX(Vector2 minionPos)
        {
            if (Main.dedServ) return;

            // Vapor cloud burst
            for (int i = 0; i < 4; i++)
            {
                Vector2 vaporVel = Main.rand.NextVector2Circular(3f, 3f);
                Color vaporCol = EnigmaPalette.GetVoidGradient(Main.rand.NextFloat()) * 0.4f;
                var vapor = new GenericGlowParticle(minionPos, vaporVel, vaporCol, 0.25f, 25, true);
                MagnumParticleHandler.SpawnParticle(vapor);
            }

            CustomParticles.GenericFlare(minionPos, EnigmaPalette.GreenFlame * 0.5f, 0.3f, 12);
            Lighting.AddLight(minionPos, EnigmaPalette.GreenFlame.ToVector3() * 0.3f);
        }

        /// <summary>
        /// VFX when Riddle's Blessing activates on a minion.
        /// </summary>
        public static void RiddleBlessingVFX(Vector2 minionPos)
        {
            if (Main.dedServ) return;

            CustomParticles.GenericFlare(minionPos, EnigmaPalette.GreenFlame, 0.5f, 14);
            CustomParticles.HaloRing(minionPos, EnigmaPalette.Purple, 0.3f, 12);
            EnigmaVFXLibrary.SpawnGlyphAccent(minionPos, 0.25f);
            EnigmaVFXLibrary.SpawnMusicNotes(minionPos, 2, 12f, 0.7f, 0.9f, 22);
            CustomParticles.EnigmaEyeGaze(minionPos, EnigmaPalette.EyeGreen * 0.5f, 0.25f);

            Lighting.AddLight(minionPos, EnigmaPalette.GreenFlame.ToVector3() * 0.5f);
        }

        // =====================================================================
        //  SHARED ACCESSORY VFX — generic effects for all Enigma accessories
        // =====================================================================

        /// <summary>
        /// Generic Enigma accessory ambient aura.
        /// Call periodically from any Enigma accessory's UpdateAccessory.
        /// </summary>
        public static void GenericEnigmaAuraVFX(Player player, float intensity = 0.3f)
        {
            if (Main.dedServ) return;

            if (Main.rand.NextBool(12))
            {
                Vector2 auraPos = player.Center + Main.rand.NextVector2Circular(25f, 25f);
                Color col = EnigmaPalette.GetEnigmaGradient(Main.rand.NextFloat()) * intensity;
                var glow = new GenericGlowParticle(auraPos, Main.rand.NextVector2Circular(0.5f, 0.5f),
                    col, 0.15f, 15, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }

            Lighting.AddLight(player.Center, EnigmaPalette.Purple.ToVector3() * intensity * 0.5f);
        }

        /// <summary>
        /// PreDrawInWorld bloom for any Enigma accessory item lying in the world.
        /// </summary>
        public static void DrawWorldItemBloom(SpriteBatch sb, Texture2D texture,
            Vector2 position, Vector2 origin, float rotation, float scale)
        {
            float pulse = 1f + MathF.Sin(Main.GameUpdateCount * 0.04f) * 0.08f;
            EnigmaPalette.DrawItemBloom(sb, texture, position, origin, rotation, scale, pulse);
        }
    }
}
