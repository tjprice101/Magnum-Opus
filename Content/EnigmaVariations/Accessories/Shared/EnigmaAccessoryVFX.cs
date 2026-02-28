using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace MagnumOpus.Content.EnigmaVariations.Accessories.Shared
{
    /// <summary>
    /// Shared VFX helper for ALL Enigma Variations accessories.
    /// VFX GUTTED — all particle/glow/bloom effects removed.
    /// Only Lighting.AddLight calls remain where applicable.
    /// Methods kept as empty stubs so callers don't break.
    /// </summary>
    public static class EnigmaAccessoryVFX
    {
        // Alchemical Paradox
        public static void AlchemicalBubblesVFX(Player player) { }
        public static void ParadoxBoltSplitVFX(Vector2 pos)
        {
            Lighting.AddLight(pos, new Vector3(0.2f, 0.86f, 0.39f) * 0.5f);
        }
        public static void ParadoxDeathExplosionVFX(Vector2 pos)
        {
            Lighting.AddLight(pos, new Vector3(0.2f, 0.86f, 0.39f) * 0.8f);
        }

        // Ignition of Mystery
        public static void MysteryFlameAuraVFX(Player player, int mysteryStacks)
        {
            float intensity = 0.2f + mysteryStacks * 0.03f;
            Lighting.AddLight(player.Center, new Vector3(0.55f, 0.24f, 0.78f) * intensity);
        }
        public static void MysteryUnveilingVFX(Vector2 pos)
        {
            Lighting.AddLight(pos, new Vector3(0.2f, 0.86f, 0.39f) * 1.0f);
        }

        // Pendant of a Thousand Puzzles
        public static void PendantGlowVFX(Player player) { }
        public static void PuzzleFragmentOrbitVFX(Player player, int fragmentCount) { }
        public static void PuzzleMasteryActivateVFX(Vector2 pos)
        {
            Lighting.AddLight(pos, new Vector3(0.2f, 0.86f, 0.39f) * 0.8f);
        }
        public static void PuzzleMasteryActiveVFX(Player player)
        {
            Lighting.AddLight(player.Center, new Vector3(0.2f, 0.86f, 0.39f) * 0.4f);
        }

        // Riddlemaster's Cauldron
        public static void CauldronBubblesVFX(Player player) { }
        public static void MysteryVaporReleaseVFX(Vector2 minionPos)
        {
            Lighting.AddLight(minionPos, new Vector3(0.2f, 0.86f, 0.39f) * 0.3f);
        }
        public static void RiddleBlessingVFX(Vector2 minionPos)
        {
            Lighting.AddLight(minionPos, new Vector3(0.2f, 0.86f, 0.39f) * 0.5f);
        }

        // Shared
        public static void GenericEnigmaAuraVFX(Player player, float intensity = 0.3f)
        {
            Lighting.AddLight(player.Center, new Vector3(0.55f, 0.24f, 0.78f) * intensity * 0.5f);
        }
        public static void DrawWorldItemBloom(SpriteBatch sb, Texture2D texture,
            Vector2 position, Vector2 origin, float rotation, float scale)
        {
            // VFX gutted — no bloom rendering
        }
    }
}
