using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.OdeToJoy.Weapons.AnthemOfGlory.Utilities
{
    /// <summary>
    /// Tracks Anthem of Glory channeling state: crescendo level, glory note spawning, kill count.
    /// Crescendo scales from 1.0x → 2.0x over 5s of sustained channeling.
    /// Victory Fanfare triggers on 3+ kills during a single channel.
    /// </summary>
    public class AnthemPlayer : ModPlayer
    {
        /// <summary>Frames of continuous channeling (capped at 300 = 5s).</summary>
        public int ChannelFrames;

        /// <summary>Timer for spawning Glory Notes — one note every 120 frames (2s).</summary>
        public int GloryNoteTimer;

        /// <summary>Active Glory Note count (max 6).</summary>
        public int ActiveGloryNotes;

        /// <summary>Kills during current channel for Victory Fanfare.</summary>
        public int ChannelKills;

        /// <summary>Whether Victory Fanfare has triggered this channel (one-shot).</summary>
        public bool FanfareTriggered;

        /// <summary>Whether player is currently channeling.</summary>
        public bool IsChanneling;

        /// <summary>
        /// Crescendo multiplier: 1.0 at start → 2.0 at 300 frames.
        /// Scales beam width, brightness, and damage.
        /// </summary>
        public float CrescendoMultiplier => 1f + (MathHelper.Clamp(ChannelFrames / 300f, 0f, 1f));

        /// <summary>
        /// Crescendo progress 0-1 for visual scaling.
        /// </summary>
        public float CrescendoProgress => MathHelper.Clamp(ChannelFrames / 300f, 0f, 1f);

        /// <summary>
        /// Whether Glory Note should spawn this frame.
        /// </summary>
        public bool ShouldSpawnGloryNote()
        {
            if (!IsChanneling) return false;
            GloryNoteTimer++;
            if (GloryNoteTimer >= 120 && ActiveGloryNotes < 6)
            {
                GloryNoteTimer = 0;
                ActiveGloryNotes++;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Register a kill during channeling. Returns true when Victory Fanfare should trigger.
        /// </summary>
        public bool RegisterKill()
        {
            ChannelKills++;
            if (ChannelKills >= 3 && !FanfareTriggered)
            {
                FanfareTriggered = true;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Start or continue channeling.
        /// </summary>
        public void UpdateChannel()
        {
            IsChanneling = true;
            ChannelFrames = System.Math.Min(ChannelFrames + 1, 300);
        }

        /// <summary>
        /// Reset channeling state when player stops.
        /// </summary>
        public void ResetChannel()
        {
            if (!IsChanneling) return;
            IsChanneling = false;
            ChannelFrames = 0;
            GloryNoteTimer = 0;
            ActiveGloryNotes = 0;
            ChannelKills = 0;
            FanfareTriggered = false;
        }

        public override void PostUpdate()
        {
            // Reset channeling if player isn't using a channel weapon this frame
            // (The weapon AI calls UpdateChannel() each frame during use)
            if (!IsChanneling)
                ResetChannel();
            IsChanneling = false; // Will be set true next frame if still channeling
        }
    }
}
