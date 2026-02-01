using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.UI;
using MagnumOpus.Content.Common.Accessories.MeleeChain;
using MagnumOpus.Content.Common.Accessories.DefenseChain;
using MagnumOpus.Content.Common.Accessories.MageChain;
using MagnumOpus.Content.Common.Accessories.SummonerChain;
using MagnumOpus.Content.Common.Accessories.RangerChain;
using MagnumOpus.Content.Common.Accessories.MobilityChain;

namespace MagnumOpus.Common.Systems
{
    /// <summary>
    /// UI System that displays separate meters for each resonance type in Phase 7 chains.
    /// Each meter only appears when the player has the corresponding accessory equipped.
    /// 
    /// Meters displayed:
    /// - Resonance Stacks (Melee Chain) - Purple bar, shows combo stacks
    /// - Resonant Shield (Defense Chain) - Blue shield bar over health
    /// - Mana Overflow (Mage Chain) - Inverted mana bar showing negative mana
    /// - Conductor Focus (Summoner Chain) - Focus bar showing conduct duration/cooldown
    /// - Mark Count (Ranger Chain) - Red crosshairs showing marked enemies
    /// - Momentum (Mobility Chain) - Gold speedometer-style bar
    /// </summary>
    public class ResonanceMetersUI : ModSystem
    {
        private UserInterface _meterInterface;
        private ResonanceMetersUIState _meterState;
        
        public override void Load()
        {
            if (!Main.dedServ)
            {
                _meterInterface = new UserInterface();
                _meterState = new ResonanceMetersUIState();
                _meterState.Activate();
            }
        }
        
        public override void Unload()
        {
            _meterInterface = null;
            _meterState = null;
        }
        
        public override void UpdateUI(GameTime gameTime)
        {
            if (_meterInterface?.CurrentState != null)
            {
                _meterInterface.Update(gameTime);
            }
        }
        
        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            // Insert our UI layer after the Resource Bars layer (health/mana)
            int resourceBarsIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Resource Bars"));
            if (resourceBarsIndex != -1)
            {
                layers.Insert(resourceBarsIndex + 1, new LegacyGameInterfaceLayer(
                    "MagnumOpus: Resonance Meters",
                    delegate
                    {
                        if (_meterState != null)
                        {
                            _meterInterface.SetState(_meterState);
                            _meterState.Draw(Main.spriteBatch);
                        }
                        return true;
                    },
                    InterfaceScaleType.UI));
            }
        }
    }
    
    public class ResonanceMetersUIState : UIState
    {
        // Colors for each chain
        private static readonly Color MeleeResonancePurple = new Color(180, 130, 255);
        private static readonly Color DefenseShieldBlue = new Color(100, 180, 255);
        private static readonly Color MageOverflowPink = new Color(255, 100, 180);
        private static readonly Color SummonerConductGold = new Color(255, 200, 100);
        private static readonly Color RangerMarkRed = new Color(255, 100, 100);
        private static readonly Color MobilityMomentumGold = new Color(255, 220, 100);
        
        // Bar dimensions
        private const int BAR_WIDTH = 180;
        private const int BAR_HEIGHT = 16;
        private const int BAR_SPACING = 24;
        private const int ICON_SIZE = 20;
        
        // Position offset from health bar
        private const int X_OFFSET = 32;
        private const int Y_OFFSET_BASE = 100; // Below health/mana bars
        
        public override void Draw(SpriteBatch spriteBatch)
        {
            Player player = Main.LocalPlayer;
            if (player == null || !player.active || player.dead)
                return;
            
            // Calculate base position (near health bars on left side)
            Vector2 basePos = new Vector2(X_OFFSET, Y_OFFSET_BASE);
            int currentYOffset = 0;
            
            // ===== MELEE: RESONANCE STACKS - SKIPPED =====
            // NOTE: Melee chain already has its own dedicated UI (ResonanceComboUI.cs)
            // with ornate visuals, so we skip it here to avoid duplicate meters.
            
            // ===== DEFENSE: RESONANT SHIELD =====
            var shieldPlayer = player.GetModPlayer<ResonantShieldPlayer>();
            if (shieldPlayer.MaxShield > 0)
            {
                DrawShieldMeter(spriteBatch, basePos + new Vector2(0, currentYOffset), shieldPlayer, player);
                currentYOffset += BAR_SPACING;
            }
            
            // ===== MAGE: MANA OVERFLOW =====
            var overflowPlayer = player.GetModPlayer<OverflowPlayer>();
            if (overflowPlayer.maxOverflow > 0)
            {
                DrawOverflowMeter(spriteBatch, basePos + new Vector2(0, currentYOffset), overflowPlayer);
                currentYOffset += BAR_SPACING;
            }
            
            // ===== SUMMONER: CONDUCTOR FOCUS =====
            var conductorPlayer = player.GetModPlayer<ConductorPlayer>();
            if (conductorPlayer.HasConductorsWand)
            {
                DrawConductorMeter(spriteBatch, basePos + new Vector2(0, currentYOffset), conductorPlayer);
                currentYOffset += BAR_SPACING;
            }
            
            // ===== RANGER: MARKED ENEMIES =====
            var markingPlayer = player.GetModPlayer<MarkingPlayer>();
            if (markingPlayer.maxMarkedEnemies > 0 && markingPlayer.hasResonantSpotter)
            {
                DrawMarkingMeter(spriteBatch, basePos + new Vector2(0, currentYOffset), markingPlayer);
                currentYOffset += BAR_SPACING;
            }
            
            // ===== MOBILITY: MOMENTUM =====
            var momentumPlayer = player.GetModPlayer<MomentumPlayer>();
            if (momentumPlayer.MaxMomentum > 0)
            {
                DrawMomentumMeter(spriteBatch, basePos + new Vector2(0, currentYOffset), momentumPlayer);
                currentYOffset += BAR_SPACING;
            }
        }
        
        /// <summary>
        /// Draws the Melee Resonance meter - shows combo stacks as musical notes
        /// </summary>
        private void DrawResonanceMeter(SpriteBatch spriteBatch, Vector2 position, ResonanceComboPlayer player)
        {
            float percent = player.GetResonancePercent();
            Color barColor = Color.Lerp(MeleeResonancePurple * 0.5f, MeleeResonancePurple, percent);
            
            // Icon (musical note symbol)
            DrawMeterIcon(spriteBatch, position, "♪", MeleeResonancePurple);
            
            // Background bar
            Vector2 barPos = position + new Vector2(ICON_SIZE + 4, 0);
            DrawBarBackground(spriteBatch, barPos, BAR_WIDTH, BAR_HEIGHT);
            
            // Filled portion with pulsing effect at high stacks
            float pulse = player.resonanceStacks >= 7 ? 1f + (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.1f : 1f;
            DrawBarFill(spriteBatch, barPos, BAR_WIDTH, BAR_HEIGHT, percent, barColor * pulse);
            
            // Stack indicators (small vertical lines for each stack)
            for (int i = 1; i < player.maxResonance; i++)
            {
                float tickX = barPos.X + (BAR_WIDTH * i / player.maxResonance);
                DrawVerticalTick(spriteBatch, new Vector2(tickX, barPos.Y), BAR_HEIGHT, Color.Black * 0.3f);
            }
            
            // Text overlay
            string stackText = $"{player.resonanceStacks}/{player.maxResonance}";
            DrawCenteredText(spriteBatch, barPos, BAR_WIDTH, BAR_HEIGHT, stackText, Color.White);
            
            // Label
            DrawLabel(spriteBatch, barPos + new Vector2(BAR_WIDTH + 4, 0), "RESONANCE", MeleeResonancePurple * 0.8f);
        }
        
        /// <summary>
        /// Draws the Defense Shield meter - shows shield health as a barrier overlay
        /// </summary>
        private void DrawShieldMeter(SpriteBatch spriteBatch, Vector2 position, ResonantShieldPlayer player, Player terrPlayer)
        {
            float percent = player.ShieldPercent;
            Color barColor = Color.Lerp(DefenseShieldBlue * 0.5f, DefenseShieldBlue, percent);
            
            // Shield icon
            DrawMeterIcon(spriteBatch, position, "◆", DefenseShieldBlue);
            
            // Background bar
            Vector2 barPos = position + new Vector2(ICON_SIZE + 4, 0);
            DrawBarBackground(spriteBatch, barPos, BAR_WIDTH, BAR_HEIGHT);
            
            // Shield fill with subtle shimmer
            float shimmer = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.05f) * 0.05f;
            DrawBarFill(spriteBatch, barPos, BAR_WIDTH, BAR_HEIGHT, percent, barColor * shimmer);
            
            // Shield value text
            string shieldText = $"{(int)player.CurrentShield}/{(int)player.MaxShield}";
            DrawCenteredText(spriteBatch, barPos, BAR_WIDTH, BAR_HEIGHT, shieldText, Color.White);
            
            // Label
            DrawLabel(spriteBatch, barPos + new Vector2(BAR_WIDTH + 4, 0), "SHIELD", DefenseShieldBlue * 0.8f);
        }
        
        /// <summary>
        /// Draws the Mage Overflow meter - shows negative mana as a depleting bar
        /// </summary>
        private void DrawOverflowMeter(SpriteBatch spriteBatch, Vector2 position, OverflowPlayer player)
        {
            float percent = player.maxOverflow > 0 ? (float)player.currentOverflow / player.maxOverflow : 0f;
            
            // Color shifts from blue to pink as overflow increases
            Color barColor = Color.Lerp(new Color(100, 100, 255), MageOverflowPink, percent);
            
            // Warning icon when in overflow
            string icon = player.isInOverflow ? "⚡" : "✧";
            Color iconColor = player.isInOverflow ? MageOverflowPink : new Color(100, 150, 255);
            DrawMeterIcon(spriteBatch, position, icon, iconColor);
            
            // Background bar
            Vector2 barPos = position + new Vector2(ICON_SIZE + 4, 0);
            DrawBarBackground(spriteBatch, barPos, BAR_WIDTH, BAR_HEIGHT);
            
            // Overflow fill - pulses when in overflow state
            float pulse = player.isInOverflow ? 1f + (float)Math.Sin(Main.GameUpdateCount * 0.15f) * 0.15f : 1f;
            DrawBarFill(spriteBatch, barPos, BAR_WIDTH, BAR_HEIGHT, percent, barColor * pulse);
            
            // Overflow text
            string overflowText = player.isInOverflow ? $"-{player.currentOverflow}/{player.maxOverflow}" : $"0/{player.maxOverflow}";
            Color textColor = player.isInOverflow ? Color.Yellow : Color.White;
            DrawCenteredText(spriteBatch, barPos, BAR_WIDTH, BAR_HEIGHT, overflowText, textColor);
            
            // Label
            string label = player.isInOverflow ? "OVERFLOW!" : "OVERFLOW";
            DrawLabel(spriteBatch, barPos + new Vector2(BAR_WIDTH + 4, 0), label, MageOverflowPink * 0.8f);
        }
        
        /// <summary>
        /// Draws the Summoner Conductor meter - shows focus duration and cooldown
        /// </summary>
        private void DrawConductorMeter(SpriteBatch spriteBatch, Vector2 position, ConductorPlayer player)
        {
            // Conductor baton icon
            string icon = player.IsConducting ? "♫" : "♪";
            Color iconColor = player.IsConducting ? SummonerConductGold : SummonerConductGold * 0.6f;
            DrawMeterIcon(spriteBatch, position, icon, iconColor);
            
            // Background bar
            Vector2 barPos = position + new Vector2(ICON_SIZE + 4, 0);
            DrawBarBackground(spriteBatch, barPos, BAR_WIDTH, BAR_HEIGHT);
            
            if (player.IsConducting)
            {
                // Show conduct duration remaining
                float percent = (float)player.ConductDuration / player.ConductMaxDuration;
                DrawBarFill(spriteBatch, barPos, BAR_WIDTH, BAR_HEIGHT, percent, SummonerConductGold);
                DrawCenteredText(spriteBatch, barPos, BAR_WIDTH, BAR_HEIGHT, "CONDUCTING!", Color.White);
            }
            else if (player.ConductCooldown > 0)
            {
                // Show cooldown
                float cooldownPercent = 1f - (float)player.ConductCooldown / GetConductCooldownMax(player);
                DrawBarFill(spriteBatch, barPos, BAR_WIDTH, BAR_HEIGHT, cooldownPercent, SummonerConductGold * 0.5f);
                string cdText = $"CD: {player.ConductCooldown / 60f:F1}s";
                DrawCenteredText(spriteBatch, barPos, BAR_WIDTH, BAR_HEIGHT, cdText, Color.Gray);
            }
            else
            {
                // Ready to conduct
                DrawBarFill(spriteBatch, barPos, BAR_WIDTH, BAR_HEIGHT, 1f, SummonerConductGold * 0.8f);
                DrawCenteredText(spriteBatch, barPos, BAR_WIDTH, BAR_HEIGHT, "READY", Color.White);
            }
            
            // Label
            DrawLabel(spriteBatch, barPos + new Vector2(BAR_WIDTH + 4, 0), "CONDUCT", SummonerConductGold * 0.8f);
        }
        
        /// <summary>
        /// Draws the Ranger Marking meter - shows marked enemy count as crosshairs
        /// </summary>
        private void DrawMarkingMeter(SpriteBatch spriteBatch, Vector2 position, MarkingPlayer player)
        {
            int markedCount = player.CountMarkedEnemies();
            float percent = (float)markedCount / player.maxMarkedEnemies;
            
            // Crosshair icon
            string icon = markedCount > 0 ? "⊕" : "○";
            Color iconColor = markedCount > 0 ? RangerMarkRed : RangerMarkRed * 0.5f;
            DrawMeterIcon(spriteBatch, position, icon, iconColor);
            
            // Background bar
            Vector2 barPos = position + new Vector2(ICON_SIZE + 4, 0);
            DrawBarBackground(spriteBatch, barPos, BAR_WIDTH, BAR_HEIGHT);
            
            // Show individual mark slots instead of continuous bar
            float slotWidth = BAR_WIDTH / (float)player.maxMarkedEnemies;
            for (int i = 0; i < player.maxMarkedEnemies; i++)
            {
                Rectangle slotRect = new Rectangle(
                    (int)(barPos.X + i * slotWidth + 1),
                    (int)barPos.Y + 1,
                    (int)slotWidth - 2,
                    BAR_HEIGHT - 2);
                
                Color slotColor = i < markedCount ? RangerMarkRed : Color.Gray * 0.3f;
                spriteBatch.Draw(TextureAssets.MagicPixel.Value, slotRect, slotColor);
            }
            
            // Mark count text
            string markText = $"{markedCount}/{player.maxMarkedEnemies}";
            DrawCenteredText(spriteBatch, barPos, BAR_WIDTH, BAR_HEIGHT, markText, Color.White);
            
            // Label with bonus info
            string label = player.markedDamageBonus > 0 ? $"MARKS +{(int)(player.markedDamageBonus * 100)}%" : "MARKS";
            DrawLabel(spriteBatch, barPos + new Vector2(BAR_WIDTH + 4, 0), label, RangerMarkRed * 0.8f);
        }
        
        /// <summary>
        /// Draws the Mobility Momentum meter - speedometer style
        /// </summary>
        private void DrawMomentumMeter(SpriteBatch spriteBatch, Vector2 position, MomentumPlayer player)
        {
            float percent = player.CurrentMomentum / player.MaxMomentum;
            
            // Color gradient from yellow to orange to red at max
            Color barColor;
            if (percent < 0.5f)
                barColor = Color.Lerp(MobilityMomentumGold * 0.6f, MobilityMomentumGold, percent * 2f);
            else
                barColor = Color.Lerp(MobilityMomentumGold, new Color(255, 100, 50), (percent - 0.5f) * 2f);
            
            // Speed icon
            string icon = percent >= 0.8f ? "»»" : (percent >= 0.5f ? "»" : "›");
            DrawMeterIcon(spriteBatch, position, icon, barColor);
            
            // Background bar
            Vector2 barPos = position + new Vector2(ICON_SIZE + 4, 0);
            DrawBarBackground(spriteBatch, barPos, BAR_WIDTH, BAR_HEIGHT);
            
            // Momentum fill with speed lines at high momentum
            float pulse = percent >= 0.8f ? 1f + (float)Math.Sin(Main.GameUpdateCount * 0.2f) * 0.1f : 1f;
            DrawBarFill(spriteBatch, barPos, BAR_WIDTH, BAR_HEIGHT, percent, barColor * pulse);
            
            // Threshold markers
            DrawVerticalTick(spriteBatch, new Vector2(barPos.X + BAR_WIDTH * 0.5f, barPos.Y), BAR_HEIGHT, Color.White * 0.3f);
            DrawVerticalTick(spriteBatch, new Vector2(barPos.X + BAR_WIDTH * 0.8f, barPos.Y), BAR_HEIGHT, Color.Orange * 0.5f);
            
            // Momentum value
            string momentumText = $"{(int)player.CurrentMomentum}/{(int)player.MaxMomentum}";
            DrawCenteredText(spriteBatch, barPos, BAR_WIDTH, BAR_HEIGHT, momentumText, Color.White);
            
            // Label
            string label = percent >= 1f ? "MAX SPEED!" : "MOMENTUM";
            DrawLabel(spriteBatch, barPos + new Vector2(BAR_WIDTH + 4, 0), label, barColor * 0.8f);
        }
        
        #region Drawing Helpers
        
        private void DrawMeterIcon(SpriteBatch spriteBatch, Vector2 position, string icon, Color color)
        {
            // Draw icon background circle
            Rectangle bgRect = new Rectangle((int)position.X, (int)position.Y, ICON_SIZE, BAR_HEIGHT);
            spriteBatch.Draw(TextureAssets.MagicPixel.Value, bgRect, Color.Black * 0.5f);
            
            // Draw icon text using Utils.DrawBorderString
            Vector2 textSize = FontAssets.MouseText.Value.MeasureString(icon);
            Vector2 textPos = position + new Vector2(ICON_SIZE / 2f - textSize.X / 2f, BAR_HEIGHT / 2f - textSize.Y / 2f - 4f);
            Utils.DrawBorderString(spriteBatch, icon, textPos, color, 0.8f);
        }
        
        private void DrawBarBackground(SpriteBatch spriteBatch, Vector2 position, int width, int height)
        {
            // Outer border
            Rectangle borderRect = new Rectangle((int)position.X - 1, (int)position.Y - 1, width + 2, height + 2);
            spriteBatch.Draw(TextureAssets.MagicPixel.Value, borderRect, Color.Black * 0.8f);
            
            // Inner background
            Rectangle bgRect = new Rectangle((int)position.X, (int)position.Y, width, height);
            spriteBatch.Draw(TextureAssets.MagicPixel.Value, bgRect, new Color(30, 30, 40) * 0.9f);
        }
        
        private void DrawBarFill(SpriteBatch spriteBatch, Vector2 position, int maxWidth, int height, float percent, Color color)
        {
            int fillWidth = (int)(maxWidth * Math.Clamp(percent, 0f, 1f));
            if (fillWidth > 0)
            {
                Rectangle fillRect = new Rectangle((int)position.X, (int)position.Y, fillWidth, height);
                spriteBatch.Draw(TextureAssets.MagicPixel.Value, fillRect, color);
                
                // Highlight on top edge
                Rectangle highlightRect = new Rectangle((int)position.X, (int)position.Y, fillWidth, 2);
                spriteBatch.Draw(TextureAssets.MagicPixel.Value, highlightRect, Color.White * 0.3f);
            }
        }
        
        private void DrawVerticalTick(SpriteBatch spriteBatch, Vector2 position, int height, Color color)
        {
            Rectangle tickRect = new Rectangle((int)position.X, (int)position.Y, 1, height);
            spriteBatch.Draw(TextureAssets.MagicPixel.Value, tickRect, color);
        }
        
        private void DrawCenteredText(SpriteBatch spriteBatch, Vector2 barPos, int barWidth, int barHeight, string text, Color color)
        {
            float scale = 0.65f;
            Vector2 textSize = FontAssets.MouseText.Value.MeasureString(text) * scale;
            Vector2 textPos = barPos + new Vector2(barWidth / 2f - textSize.X / 2f, barHeight / 2f - textSize.Y / 2f - 2f);
            
            // Use Terraria's built-in bordered text drawing
            Utils.DrawBorderString(spriteBatch, text, textPos, color, scale);
        }
        
        private void DrawLabel(SpriteBatch spriteBatch, Vector2 position, string label, Color color)
        {
            float scale = 0.55f;
            Vector2 textSize = FontAssets.MouseText.Value.MeasureString(label) * scale;
            Vector2 textPos = position + new Vector2(0, BAR_HEIGHT / 2f - textSize.Y / 2f - 2f);
            
            // Use Terraria's built-in bordered text drawing
            Utils.DrawBorderString(spriteBatch, label, textPos, color, scale);
        }
        
        private int GetConductCooldownMax(ConductorPlayer player)
        {
            // Return cooldown max based on equipped tier
            if (player.HasFatesCosmicDominion) return 300; // 5 seconds
            if (player.HasSwansGracefulDirection) return 420; // 7 seconds
            if (player.HasEnigmasHivemindLink) return 480; // 8 seconds
            if (player.HasInfernalChoirMastersRod) return 540; // 9 seconds
            if (player.HasHeroicGeneralsBaton) return 600; // 10 seconds
            if (player.HasMoonlitSymphonyWand) return 660; // 11 seconds
            if (player.HasVivaldisOrchestraBaton) return 720; // 12 seconds
            if (player.HasPermafrostCommandersCrown) return 780; // 13 seconds
            if (player.HasHarvestBeastlordsHorn) return 840; // 14 seconds
            if (player.HasSolarDirectorsCrest) return 780; // 13 seconds
            if (player.HasSpringMaestrosBadge) return 720; // 12 seconds
            return 900; // 15 seconds base
        }
        
        #endregion
    }
}
