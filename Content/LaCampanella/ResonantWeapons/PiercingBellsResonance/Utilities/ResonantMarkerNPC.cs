using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.LaCampanella.ResonantWeapons.PiercingBellsResonance.Utilities
{
    /// <summary>
    /// Per-NPC tracking for Resonant Markers from Piercing Bell's Resonance.
    /// Staccato bullets embed golden bell markers on enemies.
    /// At 3+ markers, alt-fire triggers Resonant Detonation.
    /// At exactly 5: Perfect Pitch (2x damage + Resonant Silence).
    /// Markers decay after 300 frames (5s) without new markers.
    /// </summary>
    public class ResonantMarkerNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        public int MarkerCount;
        public int DecayTimer;
        private const int DecayDelay = 300; // 5 seconds

        /// <summary>Add a Resonant Marker. Resets decay timer.</summary>
        public void AddMarker(NPC npc)
        {
            MarkerCount++;
            DecayTimer = DecayDelay;
        }

        /// <summary>Consume all markers for detonation. Returns the marker count consumed.</summary>
        public int ConsumeMarkers()
        {
            int count = MarkerCount;
            MarkerCount = 0;
            DecayTimer = 0;
            return count;
        }

        /// <summary>Whether this NPC has enough markers for detonation (3+).</summary>
        public bool CanDetonate => MarkerCount >= 3;

        /// <summary>Whether this NPC has exactly 5 markers (Perfect Pitch).</summary>
        public bool IsPerfectPitch => MarkerCount == 5;

        public override void ResetEffects(NPC npc)
        {
            if (MarkerCount > 0)
            {
                DecayTimer--;
                if (DecayTimer <= 0)
                {
                    MarkerCount = 0;
                }
            }
        }

        public override void DrawEffects(NPC npc, ref Color drawColor)
        {
            if (MarkerCount <= 0 || !npc.active) return;

            // Draw golden bell marker indicators orbiting the NPC
            float time = (float)Main.timeForVisualEffects * 0.04f;
            for (int i = 0; i < MarkerCount && i < 8; i++)
            {
                float angle = time + MathHelper.TwoPi / Math.Max(MarkerCount, 1) * i;
                float radius = 20f + npc.width * 0.3f;
                Vector2 offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius;
                Vector2 dustPos = npc.Center + offset;

                // Pulse frequency increases with marker count
                float pulse = 0.5f + 0.5f * (float)Math.Sin(time * (2f + MarkerCount * 0.5f) + i);

                Dust d = Dust.NewDustPerfect(dustPos, Terraria.ID.DustID.GoldFlame, Vector2.Zero, 0,
                    new Color(255, 200, 60) * pulse, 0.6f);
                d.noGravity = true;
                d.noLight = false;
            }

            // Tint enemy slightly gold when marked
            float goldTint = MarkerCount * 0.04f;
            drawColor = Color.Lerp(drawColor, new Color(255, 210, 80), goldTint);
        }
    }
}
