using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.Nachtmusik.Accessories;
using MagnumOpus.Content.DiesIrae.Accessories;
using MagnumOpus.Content.OdeToJoy.Accessories;
using MagnumOpus.Content.ClairDeLune.Accessories;

namespace MagnumOpus.Common
{
    /// <summary>
    /// Centralized handler for Wing HP Amplification (K-key).
    /// All wings share this mechanic: pressing K doubles effective HP
    /// (50% damage reduction) for a theme-specific duration with a 5-minute cooldown.
    /// </summary>
    public class WingAmplificationPlayer : ModPlayer
    {
        // Wing flags — set by each wing's UpdateAccessory
        // Original 5 themes
        public bool hasMoonlightWings;
        public bool hasEroicaWings;
        public bool hasLaCampanellaWings;
        public bool hasEnigmaWings;
        public bool hasSwanLakeWings;
        // Fate
        public bool hasFateWings;
        // Post-Fate
        public bool hasNachtmusikWings;
        public bool hasDiesIraeWings;
        public bool hasOdeToJoyWings;
        public bool hasClairDeLuneWings;

        // Cooldown (shared across all wings)
        public int amplificationCooldown;
        private const int CooldownDuration = 18000; // 5 minutes = 300s * 60 ticks

        // Key press captured in ProcessTriggers, consumed in PostUpdate.
        // ProcessTriggers fires before UpdateAccessory, so wing flags aren't set yet.
        // PostUpdate fires after UpdateAccessory, so flags are reliable there.
        private bool amplifyKeyPressed;
        // Manual edge detection for reliability across tModLoader versions
        private bool wasKeyDown;

        public override void ResetEffects()
        {
            hasMoonlightWings = false;
            hasEroicaWings = false;
            hasLaCampanellaWings = false;
            hasEnigmaWings = false;
            hasSwanLakeWings = false;
            hasFateWings = false;
            hasNachtmusikWings = false;
            hasDiesIraeWings = false;
            hasOdeToJoyWings = false;
            hasClairDeLuneWings = false;
        }

        public override void ProcessTriggers(TriggersSet triggersSet)
        {
            // Use manual edge detection (Current + wasKeyDown) for reliability.
            // JustPressed can sometimes miss inputs depending on tModLoader version.
            bool isDown = MagnumOpus.WingAmplifyKeybind?.Current == true;
            if (isDown && !wasKeyDown)
                amplifyKeyPressed = true;
            wasKeyDown = isDown;
        }

        public override void PostUpdate()
        {
            if (amplificationCooldown > 0)
                amplificationCooldown--;

            if (!amplifyKeyPressed)
                return;

            amplifyKeyPressed = false;

            // Check if any wings are equipped
            bool hasAnyWings = hasClairDeLuneWings || hasOdeToJoyWings || hasDiesIraeWings ||
                hasNachtmusikWings || hasFateWings || hasSwanLakeWings || hasEnigmaWings ||
                hasLaCampanellaWings || hasEroicaWings || hasMoonlightWings;

            if (!hasAnyWings)
                return;

            if (amplificationCooldown > 0)
            {
                // Show cooldown remaining
                int secondsLeft = amplificationCooldown / 60;
                CombatText.NewText(Player.getRect(), Color.Gray, $"Cooldown: {secondsLeft}s");
                return;
            }

            int buffType = -1;
            int duration = 0;

            // Highest tier wing takes priority (Clair de Lune > ... > Moonlight)
            if (hasClairDeLuneWings)
            {
                buffType = ModContent.BuffType<ClairDeLuneWingAmplifyBuff>();
                duration = 55 * 60;
            }
            else if (hasOdeToJoyWings)
            {
                buffType = ModContent.BuffType<OdeToJoyWingAmplifyBuff>();
                duration = 50 * 60;
            }
            else if (hasDiesIraeWings)
            {
                buffType = ModContent.BuffType<DiesIraeWingAmplifyBuff>();
                duration = 45 * 60;
            }
            else if (hasNachtmusikWings)
            {
                buffType = ModContent.BuffType<NachtmusikWingAmplifyBuff>();
                duration = 40 * 60;
            }
            else if (hasFateWings)
            {
                buffType = ModContent.BuffType<FateWingAmplifyBuff>();
                duration = 35 * 60;
            }
            else if (hasSwanLakeWings)
            {
                buffType = ModContent.BuffType<SwanLakeWingAmplifyBuff>();
                duration = 25 * 60;
            }
            else if (hasEnigmaWings)
            {
                buffType = ModContent.BuffType<EnigmaWingAmplifyBuff>();
                duration = 20 * 60;
            }
            else if (hasLaCampanellaWings)
            {
                buffType = ModContent.BuffType<LaCampanellaWingAmplifyBuff>();
                duration = 15 * 60;
            }
            else if (hasEroicaWings)
            {
                buffType = ModContent.BuffType<EroicaWingAmplifyBuff>();
                duration = 13 * 60;
            }
            else if (hasMoonlightWings)
            {
                buffType = ModContent.BuffType<MoonlightWingAmplifyBuff>();
                duration = 10 * 60;
            }

            if (buffType > 0)
            {
                Player.AddBuff(buffType, duration);
                amplificationCooldown = CooldownDuration;

                // Visual and audio feedback
                CombatText.NewText(Player.getRect(), new Color(255, 220, 100), "HP Amplified!");
                SoundEngine.PlaySound(SoundID.Item4, Player.Center);
            }
        }
    }
}
