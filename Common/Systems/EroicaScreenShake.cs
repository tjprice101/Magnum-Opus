using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Graphics.CameraModifiers;

namespace MagnumOpus.Common.Systems
{
    /// <summary>
    /// Helper class for screen shake effects during the Eroica boss fight.
    /// </summary>
    public static class EroicaScreenShake
    {
        /// <summary>
        /// Applies a small screen shake for minor attacks (minion projectiles).
        /// </summary>
        public static void SmallShake(Vector2 position)
        {
            if (Main.dedServ || Main.LocalPlayer == null)
                return;
                
            if (Main.LocalPlayer.Distance(position) < 2000f)
            {
                PunchCameraModifier shake = new PunchCameraModifier(
                    position,
                    Main.rand.NextVector2CircularEdge(1f, 1f),
                    3f,     // Strength
                    6f,     // Vibrations per second
                    10,     // Frames duration
                    1000f,  // Distance falloff
                    "EroicaSmall"
                );
                Main.instance.CameraModifiers.Add(shake);
            }
        }

        /// <summary>
        /// Applies a medium screen shake for minion charges.
        /// </summary>
        public static void MediumShake(Vector2 position)
        {
            if (Main.dedServ || Main.LocalPlayer == null)
                return;
                
            if (Main.LocalPlayer.Distance(position) < 2500f)
            {
                PunchCameraModifier shake = new PunchCameraModifier(
                    position,
                    Main.rand.NextVector2CircularEdge(1f, 1f),
                    6f,     // Strength
                    8f,     // Vibrations per second
                    15,     // Frames duration
                    1500f,  // Distance falloff
                    "EroicaMedium"
                );
                Main.instance.CameraModifiers.Add(shake);
            }
        }

        /// <summary>
        /// Applies a large screen shake for boss charges.
        /// </summary>
        public static void LargeShake(Vector2 position)
        {
            if (Main.dedServ || Main.LocalPlayer == null)
                return;
                
            if (Main.LocalPlayer.Distance(position) < 3000f)
            {
                PunchCameraModifier shake = new PunchCameraModifier(
                    position,
                    Main.rand.NextVector2CircularEdge(1f, 1f),
                    10f,    // Strength
                    10f,    // Vibrations per second
                    20,     // Frames duration
                    2000f,  // Distance falloff
                    "EroicaLarge"
                );
                Main.instance.CameraModifiers.Add(shake);
            }
        }

        /// <summary>
        /// Applies an extreme screen shake for the beam attack.
        /// </summary>
        public static void BeamShake(Vector2 position)
        {
            if (Main.dedServ || Main.LocalPlayer == null)
                return;
                
            if (Main.LocalPlayer.Distance(position) < 4000f)
            {
                PunchCameraModifier shake = new PunchCameraModifier(
                    position,
                    Main.rand.NextVector2CircularEdge(1f, 1f),
                    18f,    // Strength
                    15f,    // Vibrations per second
                    40,     // Frames duration
                    3000f,  // Distance falloff
                    "EroicaBeam"
                );
                Main.instance.CameraModifiers.Add(shake);
            }
        }

        /// <summary>
        /// Applies a MASSIVE screen shake when entering Phase 2 enrage.
        /// This is the biggest shake in the fight!
        /// </summary>
        public static void Phase2EnrageShake(Vector2 position)
        {
            if (Main.dedServ || Main.LocalPlayer == null)
                return;
                
            if (Main.LocalPlayer.Distance(position) < 5000f)
            {
                PunchCameraModifier shake = new PunchCameraModifier(
                    position,
                    Main.rand.NextVector2CircularEdge(1f, 1f),
                    25f,    // HUGE strength
                    12f,    // Vibrations per second
                    60,     // Long duration (1 second)
                    4000f,  // Very wide distance falloff
                    "EroicaPhase2Enrage"
                );
                Main.instance.CameraModifiers.Add(shake);
            }
        }
    }
}
