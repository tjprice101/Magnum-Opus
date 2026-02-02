using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;
using ReLogic.Content;

namespace MagnumOpus.Common.Systems
{
    /// <summary>
    /// Custom boss health bar system for MagnumOpus bosses.
    /// Features pulsing gradient colors, damage feedback, ornate gold borders, and sparkle effects.
    /// Supports: Moonlight (purple/light blue), Eroica (red/gold), La Campanella (black/orange),
    /// Enigma (green/purple), Fate (dark pink/red), Swan Lake (black/white rainbow)
    /// </summary>
    public class BossHealthBarUI : ModSystem
    {
        // Tracked boss NPCs
        private static Dictionary<int, BossHealthBarData> trackedBosses = new Dictionary<int, BossHealthBarData>();
        
        // UI positioning - sized like other mod boss bars
        private const float BarWidth = 450f;
        private const float BarHeight = 28f;
        private const float BarBottomOffset = 100f; // Distance from bottom of screen
        private const float BorderThickness = 5f;
        private const float OuterGlowSize = 6f;
        
        // Animation timing
        private static float globalTimer = 0f;
        
        // Sparkle tracking for border
        private static List<BorderSparkle> borderSparkles = new List<BorderSparkle>();
        
        public override void Load()
        {
            On_Main.DrawInterface += DrawBossHealthBars;
        }
        
        public override void Unload()
        {
            On_Main.DrawInterface -= DrawBossHealthBars;
            trackedBosses?.Clear();
            trackedBosses = null;
            borderSparkles?.Clear();
            borderSparkles = null;
        }
        
        public override void PostUpdateNPCs()
        {
            globalTimer += 0.016f; // ~60fps timing
            
            // Update tracked bosses
            List<int> toRemove = new List<int>();
            foreach (var kvp in trackedBosses)
            {
                var data = kvp.Value;
                NPC npc = Main.npc[kvp.Key];
                
                if (!npc.active || npc.life <= 0)
                {
                    toRemove.Add(kvp.Key);
                    continue;
                }
                
                // Track damage flashes with stronger response
                if (npc.life < data.LastHealth)
                {
                    data.DamageFlashTimer = 0.4f; // Bright flash duration
                    data.DamageFlashIntensity = Math.Min(1f, (data.LastHealth - npc.life) / (float)(npc.lifeMax * 0.05f)); // Intensity based on damage
                    data.LastDamageTaken = data.LastHealth - npc.life;
                    
                    // Spawn extra border sparkles on damage
                    SpawnDamageSparkles(data);
                }
                data.LastHealth = npc.life;
                
                // Smooth health display interpolation
                float targetHealth = npc.life;
                data.DisplayedHealth = MathHelper.Lerp(data.DisplayedHealth, targetHealth, 0.15f);
                if (Math.Abs(data.DisplayedHealth - targetHealth) < 1f)
                    data.DisplayedHealth = targetHealth;
                
                // Decay flash timer
                if (data.DamageFlashTimer > 0)
                    data.DamageFlashTimer -= 0.016f;
            }
            
            foreach (int id in toRemove)
                trackedBosses.Remove(id);
                
            // Update sparkles
            UpdateBorderSparkles();
        }
        
        private static void SpawnDamageSparkles(BossHealthBarData data)
        {
            int sparkleCount = 6 + (int)(data.DamageFlashIntensity * 8);
            for (int i = 0; i < sparkleCount; i++)
            {
                borderSparkles.Add(new BorderSparkle
                {
                    Position = Main.rand.NextFloat(),
                    Velocity = Main.rand.NextFloat(-0.3f, 0.3f),
                    Life = 0.6f + Main.rand.NextFloat(0.3f),
                    MaxLife = 0.6f + Main.rand.NextFloat(0.3f),
                    Size = 2f + Main.rand.NextFloat(2f),
                    Theme = data.Theme
                });
            }
        }
        
        private static void UpdateBorderSparkles()
        {
            for (int i = borderSparkles.Count - 1; i >= 0; i--)
            {
                var sparkle = borderSparkles[i];
                sparkle.Position += sparkle.Velocity * 0.016f;
                sparkle.Life -= 0.016f;
                
                if (sparkle.Life <= 0 || sparkle.Position < 0 || sparkle.Position > 1)
                    borderSparkles.RemoveAt(i);
            }
        }
        
        /// <summary>
        /// Register a boss to be tracked with the custom health bar system.
        /// Call this in the boss NPC's OnSpawn or AI method.
        /// </summary>
        public static void RegisterBoss(NPC npc, BossColorTheme theme)
        {
            if (!trackedBosses.ContainsKey(npc.whoAmI))
            {
                trackedBosses[npc.whoAmI] = new BossHealthBarData
                {
                    Theme = theme,
                    LastHealth = npc.life,
                    DisplayedHealth = npc.life,
                    DisplayName = npc.GivenOrTypeName
                };
            }
        }
        
        /// <summary>
        /// Unregister a boss from tracking (call on death/despawn).
        /// </summary>
        public static void UnregisterBoss(int npcIndex)
        {
            trackedBosses.Remove(npcIndex);
        }
        
        /// <summary>
        /// Alternative drawing method using ModSystem's PostDrawInterface.
        /// This serves as a backup if the On_Main hook doesn't work.
        /// </summary>
        public override void PostDrawInterface(SpriteBatch spriteBatch)
        {
            if (trackedBosses == null || trackedBosses.Count == 0) return;
            
            // Draw all tracked boss health bars
            int barIndex = 0;
            foreach (var kvp in trackedBosses)
            {
                if (kvp.Key < 0 || kvp.Key >= Main.maxNPCs) continue;
                
                NPC npc = Main.npc[kvp.Key];
                if (!npc.active) continue;
                
                var data = kvp.Value;
                DrawSingleBossBar(spriteBatch, npc, data, barIndex);
                barIndex++;
            }
        }
        
        private void DrawBossHealthBars(On_Main.orig_DrawInterface orig, Main self, GameTime gameTime)
        {
            orig(self, gameTime);
            
            if (trackedBosses == null || trackedBosses.Count == 0) return;
            
            SpriteBatch spriteBatch = Main.spriteBatch;
            
            try
            {
                // Begin drawing with safe state handling
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, 
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);
                
                int barIndex = 0;
                foreach (var kvp in trackedBosses)
                {
                    if (kvp.Key < 0 || kvp.Key >= Main.maxNPCs) continue;
                    
                    NPC npc = Main.npc[kvp.Key];
                    if (!npc.active) continue;
                    
                    var data = kvp.Value;
                    DrawSingleBossBar(spriteBatch, npc, data, barIndex);
                    barIndex++;
                }
                
                spriteBatch.End();
            }
            catch (System.Exception)
            {
                // If spritebatch was already active, try ending it first and redrawing
                try { spriteBatch.End(); } catch { }
            }
        }
        
        private void DrawSingleBossBar(SpriteBatch spriteBatch, NPC npc, BossHealthBarData data, int barIndex)
        {
            // Calculate position (stacked if multiple bosses)
            float screenWidth = Main.screenWidth;
            float screenHeight = Main.screenHeight;
            float yPos = screenHeight - BarBottomOffset - (barIndex * (BarHeight + 35f));
            float xPos = (screenWidth - BarWidth) / 2f;
            
            Rectangle barRect = new Rectangle((int)xPos, (int)yPos, (int)BarWidth, (int)BarHeight);
            Rectangle innerRect = new Rectangle(
                (int)(xPos + BorderThickness), 
                (int)(yPos + BorderThickness), 
                (int)(BarWidth - BorderThickness * 2), 
                (int)(BarHeight - BorderThickness * 2));
            
            // Get theme colors
            GetThemeColors(data.Theme, out Color color1, out Color color2);
            
            // Calculate health percentage using smooth displayed health
            float healthPercent = MathHelper.Clamp(data.DisplayedHealth / npc.lifeMax, 0f, 1f);
            
            // Draw outer glow (theme colored)
            DrawOuterGlow(spriteBatch, barRect, color1, color2, data);
            
            // Draw ornate gold border with sparkles
            DrawOrnateBorder(spriteBatch, barRect, data);
            
            // Draw background (dark with subtle gradient)
            DrawBarBackground(spriteBatch, innerRect);
            
            // Draw health bar with animated gradient
            Rectangle healthRect = new Rectangle(
                innerRect.X, 
                innerRect.Y, 
                (int)(innerRect.Width * healthPercent), 
                innerRect.Height);
            DrawAnimatedHealthBar(spriteBatch, healthRect, innerRect, color1, color2, data);
            
            // Draw damage flash overlay
            if (data.DamageFlashTimer > 0)
            {
                float flashIntensity = EaseOutQuad(data.DamageFlashTimer / 0.4f) * data.DamageFlashIntensity;
                Color flashColor = Color.White * flashIntensity * 0.7f;
                DrawRectangle(spriteBatch, healthRect, flashColor);
                
                // Bright edge flash
                if (healthRect.Width > 0)
                {
                    Rectangle edgeFlash = new Rectangle(healthRect.X + healthRect.Width - 3, healthRect.Y, 6, healthRect.Height);
                    DrawRectangle(spriteBatch, edgeFlash, Color.White * flashIntensity);
                }
            }
            
            // Draw shine/gloss effect on health bar
            DrawHealthBarShine(spriteBatch, healthRect);
            
            // Draw health text (centered in bar)
            DrawHealthText(spriteBatch, barRect, npc, data);
            
            // Draw boss name above bar
            DrawBossName(spriteBatch, barRect, data, color1, color2);
        }
        
        private void DrawOuterGlow(SpriteBatch spriteBatch, Rectangle rect, Color color1, Color color2, BossHealthBarData data)
        {
            float glowPulse = (float)Math.Sin(globalTimer * 1.5f) * 0.2f + 0.8f;
            float damageBoost = data.DamageFlashTimer > 0 ? EaseOutQuad(data.DamageFlashTimer / 0.4f) * 0.5f : 0f;
            
            Color glowColor = Color.Lerp(color1, color2, 0.5f) * (0.25f + damageBoost) * glowPulse;
            
            for (int i = (int)OuterGlowSize; i > 0; i--)
            {
                float alpha = (1f - (float)i / OuterGlowSize) * 0.3f;
                Rectangle glowRect = new Rectangle(
                    rect.X - i, 
                    rect.Y - i, 
                    rect.Width + i * 2, 
                    rect.Height + i * 2);
                DrawRectangle(spriteBatch, glowRect, glowColor * alpha);
            }
        }
        
        private void DrawBarBackground(SpriteBatch spriteBatch, Rectangle rect)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            
            // Dark gradient background
            for (int y = 0; y < rect.Height; y++)
            {
                float gradientT = (float)y / rect.Height;
                int brightness = (int)MathHelper.Lerp(35, 15, gradientT);
                Color bgColor = new Color(brightness, brightness - 5, brightness + 5, 240);
                spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Y + y, rect.Width, 1), bgColor);
            }
        }
        
        private void DrawOrnateBorder(SpriteBatch spriteBatch, Rectangle rect, BossHealthBarData data)
        {
            // Gold border color with pulsing animation
            float goldPulse = (float)Math.Sin(globalTimer * 2.5f) * 0.12f + 0.88f;
            float damageBoost = data.DamageFlashTimer > 0 ? EaseOutQuad(data.DamageFlashTimer / 0.4f) * 0.3f : 0f;
            
            Color goldDark = new Color((int)(180 * goldPulse), (int)(140 * goldPulse), 30);
            Color goldBase = new Color((int)(218 * (goldPulse + damageBoost)), (int)(165 * (goldPulse + damageBoost)), (int)(32 + damageBoost * 60));
            Color goldHighlight = new Color(255, 230, 120);
            Color goldBright = new Color(255, 248, 200);
            
            // Outer dark gold edge
            DrawRectangleOutline(spriteBatch, new Rectangle(rect.X - 1, rect.Y - 1, rect.Width + 2, rect.Height + 2), goldDark, 2);
            
            // Main gold border
            DrawRectangleOutline(spriteBatch, rect, goldBase, (int)BorderThickness);
            
            // Inner highlight lines (ornate effect)
            Rectangle innerHighlight = new Rectangle(rect.X + 3, rect.Y + 3, rect.Width - 6, rect.Height - 6);
            DrawRectangleOutline(spriteBatch, innerHighlight, goldHighlight * 0.6f, 1);
            
            // Corner accents (ornate corners)
            DrawOrnateCorners(spriteBatch, rect, goldBright, goldHighlight);
            
            // Draw traveling sparkles along the border
            DrawBorderSparkles(spriteBatch, rect, goldBright);
            
            // Draw damage-triggered sparkles
            DrawDamageSparkles(spriteBatch, rect, data);
        }
        
        private void DrawOrnateCorners(SpriteBatch spriteBatch, Rectangle rect, Color brightColor, Color baseColor)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            int cornerSize = 12;
            float pulse = (float)Math.Sin(globalTimer * 3f) * 0.15f + 0.85f;
            
            // Top-left corner
            spriteBatch.Draw(pixel, new Rectangle(rect.X - 2, rect.Y - 2, cornerSize, 3), brightColor * pulse);
            spriteBatch.Draw(pixel, new Rectangle(rect.X - 2, rect.Y - 2, 3, cornerSize), brightColor * pulse);
            
            // Top-right corner
            spriteBatch.Draw(pixel, new Rectangle(rect.X + rect.Width - cornerSize + 2, rect.Y - 2, cornerSize, 3), brightColor * pulse);
            spriteBatch.Draw(pixel, new Rectangle(rect.X + rect.Width - 1, rect.Y - 2, 3, cornerSize), brightColor * pulse);
            
            // Bottom-left corner
            spriteBatch.Draw(pixel, new Rectangle(rect.X - 2, rect.Y + rect.Height - 1, cornerSize, 3), brightColor * pulse);
            spriteBatch.Draw(pixel, new Rectangle(rect.X - 2, rect.Y + rect.Height - cornerSize + 2, 3, cornerSize), brightColor * pulse);
            
            // Bottom-right corner
            spriteBatch.Draw(pixel, new Rectangle(rect.X + rect.Width - cornerSize + 2, rect.Y + rect.Height - 1, cornerSize, 3), brightColor * pulse);
            spriteBatch.Draw(pixel, new Rectangle(rect.X + rect.Width - 1, rect.Y + rect.Height - cornerSize + 2, 3, cornerSize), brightColor * pulse);
        }
        
        private void DrawBorderSparkles(SpriteBatch spriteBatch, Rectangle rect, Color sparkleColor)
        {
            // Animated sparkle positions traveling along border
            int sparkleCount = 12;
            float sparkleSpeed = 0.4f;
            
            for (int i = 0; i < sparkleCount; i++)
            {
                float t = ((globalTimer * sparkleSpeed + i * (1f / sparkleCount)) % 1f);
                
                // Position along perimeter
                float perimeter = 2 * (rect.Width + rect.Height);
                float pos = t * perimeter;
                
                Vector2 sparklePos;
                if (pos < rect.Width)
                    sparklePos = new Vector2(rect.X + pos, rect.Y);
                else if (pos < rect.Width + rect.Height)
                    sparklePos = new Vector2(rect.X + rect.Width, rect.Y + (pos - rect.Width));
                else if (pos < 2 * rect.Width + rect.Height)
                    sparklePos = new Vector2(rect.X + rect.Width - (pos - rect.Width - rect.Height), rect.Y + rect.Height);
                else
                    sparklePos = new Vector2(rect.X, rect.Y + rect.Height - (pos - 2 * rect.Width - rect.Height));
                
                // Pulsing sparkle intensity
                float sparkleSize = (float)Math.Sin(globalTimer * 6f + i * 0.7f) * 0.4f + 1.8f;
                float alpha = (float)Math.Sin(globalTimer * 5f + i * 0.5f) * 0.25f + 0.75f;
                
                DrawSparkle(spriteBatch, sparklePos, sparkleColor * alpha, sparkleSize);
            }
        }
        
        private void DrawDamageSparkles(SpriteBatch spriteBatch, Rectangle rect, BossHealthBarData data)
        {
            GetThemeColors(data.Theme, out Color color1, out Color color2);
            
            foreach (var sparkle in borderSparkles)
            {
                if (sparkle.Theme != data.Theme) continue;
                
                float perimeter = 2 * (rect.Width + rect.Height);
                float pos = sparkle.Position * perimeter;
                
                Vector2 sparklePos;
                if (pos < rect.Width)
                    sparklePos = new Vector2(rect.X + pos, rect.Y);
                else if (pos < rect.Width + rect.Height)
                    sparklePos = new Vector2(rect.X + rect.Width, rect.Y + (pos - rect.Width));
                else if (pos < 2 * rect.Width + rect.Height)
                    sparklePos = new Vector2(rect.X + rect.Width - (pos - rect.Width - rect.Height), rect.Y + rect.Height);
                else
                    sparklePos = new Vector2(rect.X, rect.Y + rect.Height - (pos - 2 * rect.Width - rect.Height));
                
                float lifePercent = sparkle.Life / sparkle.MaxLife;
                float alpha = EaseOutQuad(lifePercent);
                Color sparkleColor = Color.Lerp(color2, Color.White, 0.5f) * alpha;
                
                DrawSparkle(spriteBatch, sparklePos, sparkleColor, sparkle.Size * lifePercent);
            }
        }
        
        private void DrawSparkle(SpriteBatch spriteBatch, Vector2 position, Color color, float size)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            
            // Main cross
            spriteBatch.Draw(pixel, new Rectangle((int)(position.X - size), (int)position.Y, (int)(size * 2), 1), color);
            spriteBatch.Draw(pixel, new Rectangle((int)position.X, (int)(position.Y - size), 1, (int)(size * 2)), color);
            
            // Diagonal sparkle lines (smaller)
            float diagSize = size * 0.6f;
            Color diagColor = color * 0.7f;
            
            // Draw small diagonal dots
            spriteBatch.Draw(pixel, new Rectangle((int)(position.X - diagSize * 0.7f), (int)(position.Y - diagSize * 0.7f), 1, 1), diagColor);
            spriteBatch.Draw(pixel, new Rectangle((int)(position.X + diagSize * 0.7f), (int)(position.Y - diagSize * 0.7f), 1, 1), diagColor);
            spriteBatch.Draw(pixel, new Rectangle((int)(position.X - diagSize * 0.7f), (int)(position.Y + diagSize * 0.7f), 1, 1), diagColor);
            spriteBatch.Draw(pixel, new Rectangle((int)(position.X + diagSize * 0.7f), (int)(position.Y + diagSize * 0.7f), 1, 1), diagColor);
            
            // Center bright dot
            spriteBatch.Draw(pixel, new Rectangle((int)position.X, (int)position.Y, 1, 1), Color.White * (color.A / 255f));
        }
        
        private void DrawAnimatedHealthBar(SpriteBatch spriteBatch, Rectangle rect, Rectangle fullRect, Color color1, Color color2, BossHealthBarData data)
        {
            if (rect.Width <= 0) return;
            
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            
            // Animated flowing wave gradient
            float waveSpeed = 2f;
            float waveFrequency = 0.03f;
            float secondWaveSpeed = 1.2f;
            float secondWaveFrequency = 0.05f;
            
            for (int x = 0; x < rect.Width; x++)
            {
                // Multi-layered wave effect for flowing motion
                float normalizedX = (float)x / fullRect.Width;
                float wave1 = (float)Math.Sin((x * waveFrequency) + (globalTimer * waveSpeed)) * 0.2f;
                float wave2 = (float)Math.Sin((x * secondWaveFrequency) + (globalTimer * secondWaveSpeed) + 1.5f) * 0.1f;
                float combinedWave = wave1 + wave2;
                
                float blend = MathHelper.Clamp(normalizedX + combinedWave, 0f, 1f);
                
                // Slow overall pulsing
                float pulse = (float)Math.Sin(globalTimer * 1.8f) * 0.08f + 0.92f;
                
                Color currentColor = Color.Lerp(color1, color2, blend) * pulse;
                
                // Brightness boost when recently damaged
                if (data.DamageFlashTimer > 0)
                {
                    float flashBoost = EaseOutQuad(data.DamageFlashTimer / 0.4f) * data.DamageFlashIntensity * 0.5f;
                    currentColor = Color.Lerp(currentColor, Color.White, flashBoost);
                }
                
                // Draw vertical slice with slight vertical gradient
                for (int y = 0; y < rect.Height; y++)
                {
                    float yGradient = 1f - ((float)y / rect.Height * 0.15f);
                    spriteBatch.Draw(pixel, new Rectangle(rect.X + x, rect.Y + y, 1, 1), currentColor * yGradient);
                }
            }
        }
        
        private void DrawHealthBarShine(SpriteBatch spriteBatch, Rectangle rect)
        {
            if (rect.Width <= 0) return;
            
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            
            // Top shine (glossy effect)
            Color shineColor = Color.White * 0.25f;
            spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Y, rect.Width, 2), shineColor);
            spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Y + 2, rect.Width, 1), shineColor * 0.5f);
            
            // Bottom shadow
            Color shadowColor = Color.Black * 0.2f;
            spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Y + rect.Height - 1, rect.Width, 1), shadowColor);
        }
        
        private void DrawHealthText(SpriteBatch spriteBatch, Rectangle barRect, NPC npc, BossHealthBarData data)
        {
            // Format: current / max
            string healthText = $"{(int)data.DisplayedHealth:N0} / {npc.lifeMax:N0}";
            
            // Calculate percentage for additional display
            float percent = (data.DisplayedHealth / npc.lifeMax) * 100f;
            
            Vector2 textSize = FontAssets.MouseText.Value.MeasureString(healthText);
            float scale = 0.85f;
            Vector2 textPos = new Vector2(
                barRect.X + (barRect.Width - textSize.X * scale) / 2f,
                barRect.Y + (barRect.Height - textSize.Y * scale) / 2f - 1f);
            
            // Draw shadow (multiple layers for better readability)
            Color shadowColor = Color.Black * 0.9f;
            Utils.DrawBorderString(spriteBatch, healthText, textPos + new Vector2(1, 1), shadowColor, scale);
            Utils.DrawBorderString(spriteBatch, healthText, textPos + new Vector2(-1, 1), shadowColor, scale);
            Utils.DrawBorderString(spriteBatch, healthText, textPos + new Vector2(1, -1), shadowColor, scale);
            Utils.DrawBorderString(spriteBatch, healthText, textPos + new Vector2(-1, -1), shadowColor, scale);
            
            // Draw main text (white with slight gold tint)
            Color textColor = Color.Lerp(Color.White, new Color(255, 248, 220), 0.2f);
            Utils.DrawBorderString(spriteBatch, healthText, textPos, textColor, scale);
        }
        
        private void DrawBossName(SpriteBatch spriteBatch, Rectangle barRect, BossHealthBarData data, Color color1, Color color2)
        {
            // Animated name color - flowing between theme colors
            float blend = (float)Math.Sin(globalTimer * 1.2f) * 0.5f + 0.5f;
            Color nameColor = Color.Lerp(color1, color2, blend);
            
            // Brighten the name color
            nameColor = Color.Lerp(nameColor, Color.White, 0.3f);
            
            Vector2 textSize = FontAssets.DeathText.Value.MeasureString(data.DisplayName);
            float scale = Math.Min(0.7f, (BarWidth - 40) / textSize.X);
            textSize *= scale;
            
            Vector2 textPos = new Vector2(
                barRect.X + (barRect.Width - textSize.X) / 2f,
                barRect.Y - 28f);
            
            // Outer glow effect
            Color glowColor = Color.Lerp(color1, color2, 0.5f) * 0.4f;
            for (int i = 0; i < 4; i++)
            {
                float angle = MathHelper.TwoPi * i / 4f;
                Vector2 offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 2f;
                Utils.DrawBorderString(spriteBatch, data.DisplayName, textPos + offset, glowColor, scale);
            }
            
            // Shadow
            Utils.DrawBorderString(spriteBatch, data.DisplayName, textPos + new Vector2(2, 2), Color.Black * 0.85f, scale);
            
            // Main text
            Utils.DrawBorderString(spriteBatch, data.DisplayName, textPos, nameColor, scale);
        }
        
        private void DrawRectangle(SpriteBatch spriteBatch, Rectangle rect, Color color)
        {
            spriteBatch.Draw(TextureAssets.MagicPixel.Value, rect, color);
        }
        
        private void DrawRectangleOutline(SpriteBatch spriteBatch, Rectangle rect, Color color, int thickness)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            
            // Top
            spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Y, rect.Width, thickness), color);
            // Bottom
            spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Y + rect.Height - thickness, rect.Width, thickness), color);
            // Left
            spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Y, thickness, rect.Height), color);
            // Right
            spriteBatch.Draw(pixel, new Rectangle(rect.X + rect.Width - thickness, rect.Y, thickness, rect.Height), color);
        }
        
        private static void GetThemeColors(BossColorTheme theme, out Color color1, out Color color2)
        {
            switch (theme)
            {
                case BossColorTheme.Moonlight:
                    color1 = new Color(148, 53, 236); // Vibrant purple
                    color2 = new Color(135, 206, 250); // Light sky blue
                    break;
                case BossColorTheme.Eroica:
                    color1 = new Color(230, 45, 45); // Bright red
                    color2 = new Color(255, 215, 0); // Rich gold
                    break;
                case BossColorTheme.LaCampanella:
                    color1 = new Color(40, 30, 45); // Dark black-purple
                    color2 = new Color(255, 150, 30); // Vibrant orange
                    break;
                case BossColorTheme.Enigma:
                    color1 = new Color(50, 220, 80); // Bright green
                    color2 = new Color(160, 32, 240); // Rich purple
                    break;
                case BossColorTheme.Fate:
                    color1 = new Color(219, 41, 143); // Hot pink
                    color2 = new Color(200, 40, 40); // Deep red
                    break;
                case BossColorTheme.SwanLake:
                    // Special rainbow-shifting for Swan Lake
                    float hue = (globalTimer * 0.15f) % 1f;
                    color1 = new Color(20, 20, 30);
                    color2 = Main.hslToRgb(hue, 0.85f, 0.75f);
                    break;
                case BossColorTheme.Nachtmusik:
                    color1 = new Color(45, 27, 78);    // Deep purple
                    color2 = new Color(255, 215, 0);   // Gold
                    break;
                default:
                    color1 = Color.White;
                    color2 = Color.Gray;
                    break;
            }
        }
        
        private static float EaseOutQuad(float t)
        {
            return 1f - (1f - t) * (1f - t);
        }
    }
    
    /// <summary>
    /// Color themes for boss health bars.
    /// </summary>
    public enum BossColorTheme
    {
        Moonlight,   // Purple and light blue
        Eroica,      // Red and gold
        LaCampanella,// Black and orange
        Enigma,      // Green and purple
        Fate,        // Dark pink and red
        SwanLake,    // Black with rainbow shifting
        Spring,      // Green and pink (renewal)
        Summer,      // Gold and orange (blazing)
        Autumn,      // Brown and dark orange (decay)
        Winter,      // White and deep blue (frozen)
        Nachtmusik,  // Deep purple and gold (celestial)
        DiesIrae     // Blood red and ember orange (hellfire)
    }
    
    /// <summary>
    /// Data tracked for each boss's health bar.
    /// </summary>
    public class BossHealthBarData
    {
        public BossColorTheme Theme { get; set; }
        public int LastHealth { get; set; }
        public float DisplayedHealth { get; set; } // Smoothly interpolated display
        public int LastDamageTaken { get; set; }
        public float DamageFlashTimer { get; set; }
        public float DamageFlashIntensity { get; set; } // Based on damage taken
        public string DisplayName { get; set; }
    }
    
    /// <summary>
    /// Sparkle particle for border effects.
    /// </summary>
    public class BorderSparkle
    {
        public float Position { get; set; } // 0-1 along perimeter
        public float Velocity { get; set; }
        public float Life { get; set; }
        public float MaxLife { get; set; }
        public float Size { get; set; }
        public BossColorTheme Theme { get; set; }
    }
}
