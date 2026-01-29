using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.UI;
using MagnumOpus.Content.Common.Accessories.MeleeChain;

namespace MagnumOpus.Common.Systems
{
    /// <summary>
    /// UI System that displays the Resonance Combo meter near the top of the screen.
    /// Shows an ornate silver bar with flowing energy that fills as resonance stacks build.
    /// </summary>
    public class ResonanceComboUISystem : ModSystem
    {
        private UserInterface userInterface;
        private ResonanceComboUIState uiState;
        
        public override void Load()
        {
            if (!Main.dedServ)
            {
                userInterface = new UserInterface();
                uiState = new ResonanceComboUIState();
                uiState.Activate();
            }
        }
        
        public override void Unload()
        {
            uiState = null;
            userInterface = null;
        }
        
        public override void UpdateUI(GameTime gameTime)
        {
            if (userInterface?.CurrentState != null)
            {
                userInterface.Update(gameTime);
            }
        }
        
        public override void ModifyInterfaceLayers(System.Collections.Generic.List<GameInterfaceLayer> layers)
        {
            int inventoryIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Inventory"));
            if (inventoryIndex != -1)
            {
                layers.Insert(inventoryIndex, new LegacyGameInterfaceLayer(
                    "MagnumOpus: Resonance Combo UI",
                    delegate
                    {
                        // Only show if player has a resonance accessory equipped
                        if (Main.LocalPlayer != null && !Main.LocalPlayer.dead)
                        {
                            var resonancePlayer = Main.LocalPlayer.GetModPlayer<ResonanceComboPlayer>();
                            if (resonancePlayer.hasResonantRhythmBand && resonancePlayer.maxResonance > 0)
                            {
                                if (userInterface?.CurrentState == null)
                                {
                                    userInterface?.SetState(uiState);
                                }
                                uiState?.Draw(Main.spriteBatch);
                            }
                            else if (userInterface?.CurrentState != null)
                            {
                                userInterface?.SetState(null);
                            }
                        }
                        return true;
                    },
                    InterfaceScaleType.UI));
            }
        }
    }
    
    /// <summary>
    /// The actual UI state that renders the Resonance Combo meter.
    /// Features an ornate silver bar with pulsing, flowing energy inside.
    /// </summary>
    public class ResonanceComboUIState : UIState
    {
        // Bar dimensions
        private const float BarWidth = 200f;
        private const float BarHeight = 18f;
        private const float BorderThickness = 3f;
        
        // Animation timers
        private float pulseTimer = 0f;
        private float flowTimer = 0f;
        private float[] waveOffsets = new float[8];
        
        // Colors
        private static readonly Color SilverBorder = new Color(192, 192, 210);
        private static readonly Color SilverHighlight = new Color(230, 230, 245);
        private static readonly Color DarkBackground = new Color(15, 12, 20);
        private static readonly Color EnergyBase = new Color(180, 130, 255);
        
        public ResonanceComboUIState()
        {
            // Initialize wave offsets for flowing effect
            for (int i = 0; i < waveOffsets.Length; i++)
            {
                waveOffsets[i] = i * 0.5f;
            }
        }
        
        public new void Draw(SpriteBatch spriteBatch)
        {
            if (Main.LocalPlayer == null) return;
            
            var resonancePlayer = Main.LocalPlayer.GetModPlayer<ResonanceComboPlayer>();
            if (!resonancePlayer.hasResonantRhythmBand || resonancePlayer.maxResonance <= 0)
                return;
            
            // Update animation timers
            pulseTimer += 0.05f;
            flowTimer += 0.08f;
            
            // Position - top center of screen, below hotbar
            Vector2 position = new Vector2(Main.screenWidth / 2f - BarWidth / 2f, 80f);
            
            // Get fill percentage
            float fillPercent = resonancePlayer.GetResonancePercent();
            Color energyColor = resonancePlayer.GetResonanceColor();
            
            // Draw the ornate bar
            DrawOrnateBar(spriteBatch, position, fillPercent, energyColor, resonancePlayer);
            
            // Draw stack count text
            DrawStackText(spriteBatch, position, resonancePlayer, energyColor);
        }
        
        private void DrawOrnateBar(SpriteBatch spriteBatch, Vector2 position, float fillPercent, Color energyColor, ResonanceComboPlayer player)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            
            // === OUTER ORNATE BORDER (Silver with highlights) ===
            // Main border rectangle
            Rectangle outerBorder = new Rectangle(
                (int)(position.X - BorderThickness - 2),
                (int)(position.Y - BorderThickness - 2),
                (int)(BarWidth + (BorderThickness + 2) * 2),
                (int)(BarHeight + (BorderThickness + 2) * 2)
            );
            
            // Draw shadow
            spriteBatch.Draw(pixel, new Rectangle(outerBorder.X + 3, outerBorder.Y + 3, outerBorder.Width, outerBorder.Height),
                Color.Black * 0.4f);
            
            // Draw ornate silver border with pulsing highlight
            float borderPulse = (float)Math.Sin(pulseTimer * 0.8f) * 0.15f + 0.85f;
            Color borderColor = Color.Lerp(SilverBorder, SilverHighlight, borderPulse * 0.3f);
            
            // Top border with ornate edge
            spriteBatch.Draw(pixel, new Rectangle(outerBorder.X, outerBorder.Y, outerBorder.Width, (int)BorderThickness), borderColor);
            // Bottom border
            spriteBatch.Draw(pixel, new Rectangle(outerBorder.X, outerBorder.Y + outerBorder.Height - (int)BorderThickness, outerBorder.Width, (int)BorderThickness), borderColor);
            // Left border
            spriteBatch.Draw(pixel, new Rectangle(outerBorder.X, outerBorder.Y, (int)BorderThickness, outerBorder.Height), borderColor);
            // Right border
            spriteBatch.Draw(pixel, new Rectangle(outerBorder.X + outerBorder.Width - (int)BorderThickness, outerBorder.Y, (int)BorderThickness, outerBorder.Height), borderColor);
            
            // Inner highlight line
            Color innerHighlight = SilverHighlight * (0.4f + borderPulse * 0.2f);
            spriteBatch.Draw(pixel, new Rectangle((int)position.X - 1, (int)position.Y - 1, (int)BarWidth + 2, 1), innerHighlight);
            spriteBatch.Draw(pixel, new Rectangle((int)position.X - 1, (int)position.Y - 1, 1, (int)BarHeight + 2), innerHighlight);
            
            // === DARK BACKGROUND ===
            Rectangle bgRect = new Rectangle((int)position.X, (int)position.Y, (int)BarWidth, (int)BarHeight);
            spriteBatch.Draw(pixel, bgRect, DarkBackground);
            
            // === FLOWING ENERGY FILL ===
            if (fillPercent > 0)
            {
                int fillWidth = (int)(BarWidth * fillPercent);
                Rectangle fillRect = new Rectangle((int)position.X, (int)position.Y, fillWidth, (int)BarHeight);
                
                // Base energy fill with gradient
                for (int x = 0; x < fillWidth; x++)
                {
                    float xPercent = (float)x / BarWidth;
                    float waveOffset = 0f;
                    
                    // Create flowing wave pattern
                    for (int w = 0; w < waveOffsets.Length; w++)
                    {
                        waveOffset += (float)Math.Sin(flowTimer + xPercent * 4f + waveOffsets[w]) * 0.08f;
                    }
                    waveOffset /= waveOffsets.Length;
                    
                    // Color intensity varies with wave
                    float intensity = 0.6f + waveOffset + (float)Math.Sin(pulseTimer + xPercent * 2f) * 0.15f;
                    intensity = Math.Clamp(intensity, 0.4f, 1f);
                    
                    // Gradient from darker edge to brighter center
                    Color fillColor = energyColor * intensity;
                    
                    // Draw vertical strip
                    for (int y = 0; y < (int)BarHeight; y++)
                    {
                        float yPercent = (float)y / BarHeight;
                        // Center is brighter
                        float yIntensity = 1f - Math.Abs(yPercent - 0.5f) * 0.6f;
                        
                        // Add flowing horizontal waves
                        float horizontalWave = (float)Math.Sin(flowTimer * 1.5f + y * 0.3f + xPercent * 3f) * 0.12f;
                        yIntensity += horizontalWave;
                        
                        Color pixelColor = fillColor * Math.Clamp(yIntensity, 0.3f, 1f);
                        spriteBatch.Draw(pixel, new Rectangle((int)position.X + x, (int)position.Y + y, 1, 1), pixelColor);
                    }
                }
                
                // === BRIGHT CORE LINE (center of the energy) ===
                float coreY = position.Y + BarHeight / 2f;
                float coreWave = (float)Math.Sin(flowTimer * 2f) * 2f;
                for (int x = 0; x < fillWidth; x++)
                {
                    float xPercent = (float)x / fillWidth;
                    float localWave = (float)Math.Sin(flowTimer + xPercent * 6f) * 1.5f;
                    int coreYPos = (int)(coreY + coreWave + localWave);
                    
                    // Bright white-ish core
                    Color coreColor = Color.Lerp(energyColor, Color.White, 0.6f + (float)Math.Sin(pulseTimer + xPercent * 4f) * 0.2f);
                    spriteBatch.Draw(pixel, new Rectangle((int)position.X + x, coreYPos - 1, 1, 3), coreColor * 0.8f);
                }
                
                // === SPARKLE PARTICLES along the fill ===
                if (Main.GameUpdateCount % 3 == 0 && fillPercent > 0.1f)
                {
                    int sparkleX = (int)(position.X + Main.rand.NextFloat(fillWidth * 0.8f, fillWidth));
                    int sparkleY = (int)(position.Y + Main.rand.NextFloat(BarHeight));
                    float sparkleIntensity = 0.5f + Main.rand.NextFloat(0.5f);
                    Color sparkleColor = Color.Lerp(energyColor, Color.White, 0.4f) * sparkleIntensity;
                    
                    // Draw small sparkle
                    spriteBatch.Draw(pixel, new Rectangle(sparkleX - 1, sparkleY, 3, 1), sparkleColor);
                    spriteBatch.Draw(pixel, new Rectangle(sparkleX, sparkleY - 1, 1, 3), sparkleColor);
                }
                
                // === LEADING EDGE GLOW ===
                if (fillWidth > 5)
                {
                    float edgePulse = (float)Math.Sin(pulseTimer * 2f) * 0.3f + 0.7f;
                    Color edgeColor = Color.Lerp(energyColor, Color.White, 0.3f) * edgePulse;
                    
                    // Vertical glow line at the edge
                    for (int y = 0; y < (int)BarHeight; y++)
                    {
                        float yIntensity = 1f - Math.Abs((float)y / BarHeight - 0.5f) * 1.5f;
                        yIntensity = Math.Max(0, yIntensity);
                        spriteBatch.Draw(pixel, new Rectangle((int)position.X + fillWidth - 2, (int)position.Y + y, 3, 1), edgeColor * yIntensity);
                    }
                }
            }
            
            // === ORNATE CORNER ACCENTS ===
            DrawCornerAccents(spriteBatch, position, borderColor);
            
            // === MUSICAL NOTE DECORATIONS ===
            DrawMusicalAccents(spriteBatch, position, energyColor, fillPercent);
        }
        
        private void DrawCornerAccents(SpriteBatch spriteBatch, Vector2 position, Color borderColor)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            
            // Small diamond accents at corners
            int accentSize = 4;
            Vector2[] corners = new Vector2[]
            {
                new Vector2(position.X - BorderThickness - 4, position.Y - BorderThickness - 4),
                new Vector2(position.X + BarWidth + BorderThickness, position.Y - BorderThickness - 4),
                new Vector2(position.X - BorderThickness - 4, position.Y + BarHeight + BorderThickness),
                new Vector2(position.X + BarWidth + BorderThickness, position.Y + BarHeight + BorderThickness)
            };
            
            float accentPulse = (float)Math.Sin(pulseTimer * 1.2f) * 0.3f + 0.7f;
            Color accentColor = Color.Lerp(borderColor, SilverHighlight, accentPulse);
            
            foreach (var corner in corners)
            {
                // Draw small diamond shape
                spriteBatch.Draw(pixel, new Rectangle((int)corner.X + accentSize / 2, (int)corner.Y, 1, accentSize), accentColor);
                spriteBatch.Draw(pixel, new Rectangle((int)corner.X, (int)corner.Y + accentSize / 2, accentSize, 1), accentColor);
            }
        }
        
        private void DrawMusicalAccents(SpriteBatch spriteBatch, Vector2 position, Color energyColor, float fillPercent)
        {
            if (fillPercent < 0.3f) return;
            
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            
            // Small musical note symbols at the sides
            float notePulse = (float)Math.Sin(pulseTimer * 1.5f) * 0.3f + 0.7f;
            Color noteColor = energyColor * notePulse * 0.6f;
            
            // Left note (♪ approximation)
            Vector2 leftNote = new Vector2(position.X - 12, position.Y + BarHeight / 2f);
            spriteBatch.Draw(pixel, new Rectangle((int)leftNote.X, (int)leftNote.Y - 4, 2, 8), noteColor);
            spriteBatch.Draw(pixel, new Rectangle((int)leftNote.X, (int)leftNote.Y - 4, 4, 2), noteColor);
            spriteBatch.Draw(pixel, new Rectangle((int)leftNote.X + 2, (int)leftNote.Y + 2, 3, 3), noteColor);
            
            // Right note
            Vector2 rightNote = new Vector2(position.X + BarWidth + 8, position.Y + BarHeight / 2f);
            spriteBatch.Draw(pixel, new Rectangle((int)rightNote.X, (int)rightNote.Y - 4, 2, 8), noteColor);
            spriteBatch.Draw(pixel, new Rectangle((int)rightNote.X - 2, (int)rightNote.Y - 4, 4, 2), noteColor);
            spriteBatch.Draw(pixel, new Rectangle((int)rightNote.X - 3, (int)rightNote.Y + 2, 3, 3), noteColor);
        }
        
        private void DrawStackText(SpriteBatch spriteBatch, Vector2 position, ResonanceComboPlayer player, Color energyColor)
        {
            // Stack count display below the bar
            string stackText = $"{player.resonanceStacks}/{player.maxResonance}";
            Vector2 textSize = FontAssets.MouseText.Value.MeasureString(stackText) * 0.7f;
            Vector2 textPos = new Vector2(position.X + BarWidth / 2f - textSize.X / 2f, position.Y + BarHeight + 4f);
            
            // Text shadow
            Utils.DrawBorderString(spriteBatch, stackText, textPos + new Vector2(1, 1), Color.Black * 0.6f, 0.7f);
            
            // Main text with energy color
            float textPulse = (float)Math.Sin(pulseTimer) * 0.15f + 0.85f;
            Color textColor = Color.Lerp(energyColor, Color.White, 0.3f) * textPulse;
            Utils.DrawBorderString(spriteBatch, stackText, textPos, textColor, 0.7f);
            
            // Label above the bar
            string label = "RESONANCE";
            Vector2 labelSize = FontAssets.MouseText.Value.MeasureString(label) * 0.5f;
            Vector2 labelPos = new Vector2(position.X + BarWidth / 2f - labelSize.X / 2f, position.Y - 16f);
            
            Utils.DrawBorderString(spriteBatch, label, labelPos + new Vector2(1, 1), Color.Black * 0.5f, 0.5f);
            Utils.DrawBorderString(spriteBatch, label, labelPos, SilverHighlight * 0.8f, 0.5f);
            
            // Show special state indicators
            if (player.IsGraceful && player.hasSwansPerfectMeasure)
            {
                string graceText = "GRACEFUL";
                Vector2 gracePos = new Vector2(position.X + BarWidth + 20f, position.Y + BarHeight / 2f - 6f);
                Utils.DrawBorderString(spriteBatch, graceText, gracePos, new Color(255, 255, 255) * textPulse, 0.5f);
            }
            
            if (player.IsBurstOnCooldown)
            {
                float cdPercent = player.BurstCooldown / 120f;
                string cdText = $"CD: {player.BurstCooldown / 60f:F1}s";
                Vector2 cdPos = new Vector2(position.X - 60f, position.Y + BarHeight / 2f - 6f);
                Utils.DrawBorderString(spriteBatch, cdText, cdPos, Color.Gray * 0.8f, 0.45f);
            }
        }
    }
}
