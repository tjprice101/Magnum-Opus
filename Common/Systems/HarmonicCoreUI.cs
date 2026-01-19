using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace MagnumOpus.Common.Systems
{
    /// <summary>
    /// Harmonic Core UI with 3 slots, pearlescent shimmer border, matte black background.
    /// Includes stats subsection showing equipped core details.
    /// </summary>
    public class HarmonicCoreUIState : UIState
    {
        private UIPanel mainPanel;
        private UIPanel collapsedPanel;
        private CoreSlotPanel[] coreSlots = new CoreSlotPanel[3];
        private UIText titleText;
        private UIText descriptionText;
        private UIText classBonusText;
        
        // Stats subsection elements
        private UIPanel statsPanel;
        private UIText statsHeaderText;
        private UIText[] statsCoreLines = new UIText[3]; // One line per equipped core
        private UIText statsTotalLine;
        
        private bool isCollapsed = false;
        private float shimmerTimer = 0f;
        private int hoveredSlot = -1;
        private string hoveredDescription = "";
        
        private static readonly Color MatteBlack = new Color(12, 12, 18);
        private static readonly Color DarkPanelBg = new Color(8, 8, 12, 245);
        
        public override void OnInitialize()
        {
            // Collapsed button
            collapsedPanel = new UIPanel();
            collapsedPanel.Width.Set(32f, 0f);
            collapsedPanel.Height.Set(32f, 0f);
            collapsedPanel.Left.Set(20f, 0f);
            collapsedPanel.Top.Set(260f, 0f);
            collapsedPanel.BackgroundColor = MatteBlack * 0.95f;
            collapsedPanel.BorderColor = new Color(200, 200, 220, 180);
            collapsedPanel.OnLeftClick += (evt, elem) => { isCollapsed = false; UpdatePanelVisibility(); };
            
            var expandIcon = new UIText("♫", 1f);
            expandIcon.HAlign = 0.5f;
            expandIcon.VAlign = 0.5f;
            expandIcon.TextColor = new Color(220, 220, 240);
            collapsedPanel.Append(expandIcon);
            
            // Main panel - increased height for stats subsection
            mainPanel = new UIPanel();
            mainPanel.Width.Set(360f, 0f);
            mainPanel.Height.Set(460f, 0f); // Increased height for stats subsection
            mainPanel.Left.Set(20f, 0f);
            mainPanel.Top.Set(260f, 0f);
            mainPanel.BackgroundColor = DarkPanelBg;
            mainPanel.BorderColor = Color.Transparent;
            
            titleText = new UIText("Harmonic Cores", 0.9f);
            titleText.HAlign = 0.5f;
            titleText.Top.Set(8f, 0f);
            titleText.TextColor = new Color(220, 220, 240);
            mainPanel.Append(titleText);
            
            // Collapse button
            var collapseBtn = new UIPanel();
            collapseBtn.Width.Set(16f, 0f);
            collapseBtn.Height.Set(16f, 0f);
            collapseBtn.Left.Set(-22f, 1f);
            collapseBtn.Top.Set(6f, 0f);
            collapseBtn.BackgroundColor = new Color(40, 20, 20, 200);
            collapseBtn.BorderColor = new Color(80, 50, 50);
            collapseBtn.OnLeftClick += (evt, elem) => { isCollapsed = true; UpdatePanelVisibility(); };
            mainPanel.Append(collapseBtn);
            
            var collapseX = new UIText("−", 0.7f);
            collapseX.HAlign = 0.5f;
            collapseX.VAlign = 0.5f;
            collapseX.TextColor = new Color(200, 120, 120);
            collapseBtn.Append(collapseX);
            
            // 3 core slots
            for (int i = 0; i < 3; i++)
            {
                coreSlots[i] = new CoreSlotPanel(i, this);
                coreSlots[i].Width.Set(70f, 0f);
                coreSlots[i].Height.Set(100f, 0f); // Increased height for enhance button
                coreSlots[i].Left.Set(30f + i * 95f, 0f);
                coreSlots[i].Top.Set(35f, 0f);
                mainPanel.Append(coreSlots[i]);
            }
            
            descriptionText = new UIText("", 0.48f, false);
            descriptionText.HAlign = 0.5f;
            descriptionText.Top.Set(145f, 0f); // Adjusted for new slot height
            descriptionText.TextColor = new Color(180, 180, 200);
            descriptionText.Width.Set(320f, 0f);
            mainPanel.Append(descriptionText);
            
            classBonusText = new UIText("", 0.52f);
            classBonusText.HAlign = 0.5f;
            classBonusText.Top.Set(180f, 0f);
            classBonusText.TextColor = new Color(120, 200, 120);
            mainPanel.Append(classBonusText);
            
            // === STATS SUBSECTION ===
            // Separator line (visual divider)
            var separator = new UIPanel();
            separator.Width.Set(300f, 0f);
            separator.Height.Set(2f, 0f);
            separator.HAlign = 0.5f;
            separator.Top.Set(210f, 0f);
            separator.BackgroundColor = new Color(80, 80, 100, 100);
            separator.BorderColor = Color.Transparent;
            mainPanel.Append(separator);
            
            // Stats header
            statsHeaderText = new UIText("◆ Equipped Core Stats ◆", 0.55f);
            statsHeaderText.HAlign = 0.5f;
            statsHeaderText.Top.Set(222f, 0f);
            statsHeaderText.TextColor = new Color(200, 200, 220);
            mainPanel.Append(statsHeaderText);
            
            // Individual core stat lines
            for (int i = 0; i < 3; i++)
            {
                statsCoreLines[i] = new UIText("", 0.40f, false);
                statsCoreLines[i].Left.Set(15f, 0f);
                statsCoreLines[i].Top.Set(245f + i * 55f, 0f); // Better spacing
                statsCoreLines[i].TextColor = new Color(180, 180, 200);
                mainPanel.Append(statsCoreLines[i]);
            }
            
            // Total stats line
            statsTotalLine = new UIText("", 0.5f);
            statsTotalLine.HAlign = 0.5f;
            statsTotalLine.Top.Set(420f, 0f); // Moved down to account for increased spacing
            statsTotalLine.TextColor = new Color(255, 220, 100);
            mainPanel.Append(statsTotalLine);
            
            UpdatePanelVisibility();
        }
        
        private void UpdatePanelVisibility()
        {
            if (mainPanel.Parent != null) mainPanel.Remove();
            if (collapsedPanel.Parent != null) collapsedPanel.Remove();
            Append(isCollapsed ? collapsedPanel : mainPanel);
            Recalculate();
        }
        
        public void SetHoveredSlot(int slot, string desc) { hoveredSlot = slot; hoveredDescription = desc; }
        public void ClearHover() { hoveredSlot = -1; hoveredDescription = ""; }
        
        public void RefreshDisplay()
        {
            if (Main.gameMenu || Main.LocalPlayer == null) return;
            
            var player = Main.LocalPlayer.GetModPlayer<HarmonicCoreModPlayer>();
            for (int i = 0; i < 3; i++) coreSlots[i].RefreshSlot();
            
            if (hoveredSlot >= 0 && !string.IsNullOrEmpty(hoveredDescription))
                descriptionText.SetText(hoveredDescription);
            else
                descriptionText.SetText(player.GetEquippedCoreCount() == 0 ? 
                    "Click to equip cores" : 
                    "");
            
            // Calculate total damage bonus including enhancements
            float totalBonus = 0f;
            for (int i = 0; i < 3; i++)
            {
                totalBonus += player.GetEnhancedDamageBonus(i);
            }
            
            int bonusPercent = (int)(totalBonus * 100f);
            classBonusText.SetText(totalBonus > 0 ? 
                $"All Classes: +{bonusPercent}%% Damage" : "");
            
            // === UPDATE STATS SUBSECTION ===
            var coreStats = player.GetEquippedCoreStats();
            
            // Update individual core stat lines
            for (int i = 0; i < 3; i++)
            {
                if (i < coreStats.Count)
                {
                    var stats = coreStats[i];
                    string enhText = stats.EnhancementLevel > 0 ? $" +{stats.EnhancementLevel}" : "";
                    int dmgPercent = (int)(stats.DamageBonus * 100f);
                    
                    // Format: "◇ Name [T1] +Enh — +5% Dmg"
                    // Second line: "  Effect: description"
                    string line1 = $"◇ {stats.DisplayName} [T{stats.Tier}]{enhText}";
                    string line2 = $"   → +{dmgPercent}% Damage";
                    string line3 = $"   {stats.EffectName}";
                    
                    statsCoreLines[i].SetText($"{line1}\n{line2}\n{line3}");
                    statsCoreLines[i].TextColor = stats.ThemeColor * 0.9f;
                }
                else
                {
                    statsCoreLines[i].SetText("");
                }
            }
            
            // Update total line
            if (coreStats.Count > 0)
            {
                float totalDmg = player.GetTotalDamageBonus();
                int totalDmgPercent = (int)(totalDmg * 100f);
                statsTotalLine.SetText($"◆ Total: +{totalDmgPercent}%% All Damage ◆");
                statsHeaderText.TextColor = new Color(200, 200, 220);
            }
            else
            {
                statsTotalLine.SetText("");
                statsHeaderText.TextColor = new Color(100, 100, 120);
            }
        }
        
        public override void Draw(SpriteBatch spriteBatch)
        {
            if (Main.gameMenu || Main.LocalPlayer == null) return;
            shimmerTimer += 0.02f;
            
            if (!isCollapsed) DrawShimmerBorder(spriteBatch, mainPanel.GetDimensions());
            else DrawShimmerBorder(spriteBatch, collapsedPanel.GetDimensions());
            
            base.Draw(spriteBatch);
            
            // Tooltip disabled - info now shows in-panel via descriptionText
            // if (hoveredSlot >= 0 && !string.IsNullOrEmpty(hoveredDescription))
            //     DrawHoverTooltip(spriteBatch);
        }
        
        private void DrawShimmerBorder(SpriteBatch spriteBatch, CalculatedStyle dims)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            int bw = 3;
            float x = dims.X - bw, y = dims.Y - bw;
            float w = dims.Width + bw * 2, h = dims.Height + bw * 2;
            
            for (int i = 0; i < 4; i++)
            {
                float phase = shimmerTimer + i * 0.25f;
                float r = 0.85f + 0.15f * (float)Math.Sin(phase * 2f);
                float g = 0.85f + 0.10f * (float)Math.Sin(phase * 2.3f + 0.5f);
                float b = 0.90f + 0.10f * (float)Math.Sin(phase * 1.8f + 1f);
                float highlight = Math.Max(0, (float)Math.Sin(phase * 3f + i * MathHelper.PiOver2));
                
                Color borderColor = new Color(
                    Math.Min(1f, r + highlight * 0.3f),
                    Math.Min(1f, g + highlight * 0.25f),
                    Math.Min(1f, b + highlight * 0.35f)) * (0.7f + highlight * 0.3f);
                
                Rectangle rect = i switch
                {
                    0 => new Rectangle((int)x, (int)y, (int)w, bw),
                    1 => new Rectangle((int)(x + w - bw), (int)y, bw, (int)h),
                    2 => new Rectangle((int)x, (int)(y + h - bw), (int)w, bw),
                    _ => new Rectangle((int)x, (int)y, bw, (int)h)
                };
                spriteBatch.Draw(pixel, rect, borderColor);
            }
        }
        
        private void DrawHoverTooltip(SpriteBatch spriteBatch)
        {
            Vector2 mousePos = Main.MouseScreen;
            var font = Terraria.GameContent.FontAssets.MouseText.Value;
            string[] lines = hoveredDescription.Split('\n');
            
            float maxWidth = 0, totalHeight = 0;
            foreach (string line in lines)
            {
                Vector2 size = font.MeasureString(line) * 0.45f;
                maxWidth = Math.Max(maxWidth, size.X);
                totalHeight += size.Y + 2;
            }
            
            // Ensure tooltip stays within screen bounds
            maxWidth = Math.Min(maxWidth, Main.screenWidth - 60);
            float tooltipX = mousePos.X + 15;
            if (tooltipX + maxWidth + 20 > Main.screenWidth)
                tooltipX = Main.screenWidth - maxWidth - 30;
            float tooltipY = mousePos.Y - totalHeight - 10;
            if (tooltipY < 10) tooltipY = mousePos.Y + 20;
            
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            Rectangle bg = new Rectangle((int)tooltipX - 6, (int)tooltipY - 4, (int)maxWidth + 12, (int)totalHeight + 8);
            spriteBatch.Draw(pixel, bg, MatteBlack * 0.95f);
            
            Color border = new Color(180, 180, 200, 200);
            spriteBatch.Draw(pixel, new Rectangle(bg.X, bg.Y, bg.Width, 1), border);
            spriteBatch.Draw(pixel, new Rectangle(bg.X, bg.Y + bg.Height - 1, bg.Width, 1), border);
            spriteBatch.Draw(pixel, new Rectangle(bg.X, bg.Y, 1, bg.Height), border);
            spriteBatch.Draw(pixel, new Rectangle(bg.X + bg.Width - 1, bg.Y, 1, bg.Height), border);
            
            float currentY = tooltipY;
            foreach (string line in lines)
            {
                Utils.DrawBorderString(spriteBatch, line, new Vector2(tooltipX, currentY), new Color(240, 240, 255), 0.45f);
                currentY += font.MeasureString(line).Y * 0.45f + 2;
            }
        }
        
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            if ((isCollapsed ? collapsedPanel : mainPanel).ContainsPoint(Main.MouseScreen))
                Main.LocalPlayer.mouseInterface = true;
        }
    }
    
    public class CoreSlotPanel : UIPanel
    {
        private int slotIndex;
        private HarmonicCoreUIState parentUI;
        private UIText enhancementLabel;
        private UIPanel enhanceButton;
        private UIText enhanceButtonText;
        private float pulseTimer = 0f;
        
        public CoreSlotPanel(int index, HarmonicCoreUIState parent)
        {
            slotIndex = index;
            parentUI = parent;
            BackgroundColor = new Color(5, 5, 8, 250);
            BorderColor = new Color(60, 60, 80, 150);
            
            var slotLabel = new UIText($"{index + 1}", 0.5f);
            slotLabel.HAlign = 0.5f;
            slotLabel.Top.Set(2f, 0f);
            slotLabel.TextColor = new Color(100, 100, 120);
            Append(slotLabel);
            
            // Enhancement level display
            enhancementLabel = new UIText("", 0.45f);
            enhancementLabel.HAlign = 0.5f;
            enhancementLabel.Top.Set(58f, 0f);
            enhancementLabel.TextColor = new Color(220, 180, 255);
            Append(enhancementLabel);
            
            // Enhance Melody button
            enhanceButton = new UIPanel();
            enhanceButton.Width.Set(60f, 0f);
            enhanceButton.Height.Set(16f, 0f);
            enhanceButton.Left.Set(5f, 0f);
            enhanceButton.Top.Set(76f, 0f);
            enhanceButton.BackgroundColor = new Color(40, 30, 60, 220);
            enhanceButton.BorderColor = new Color(120, 100, 180, 180);
            enhanceButton.OnLeftClick += OnEnhanceClick;
            enhanceButton.OnMouseOver += (evt, elem) => { enhanceButton.BackgroundColor = new Color(60, 45, 90, 240); };
            enhanceButton.OnMouseOut += (evt, elem) => { enhanceButton.BackgroundColor = new Color(40, 30, 60, 220); };
            Append(enhanceButton);
            
            enhanceButtonText = new UIText("Enhance", 0.35f);
            enhanceButtonText.HAlign = 0.5f;
            enhanceButtonText.VAlign = 0.5f;
            enhanceButtonText.TextColor = new Color(200, 180, 255);
            enhanceButton.Append(enhanceButtonText);
        }
        
        private void OnEnhanceClick(UIMouseEvent evt, UIElement listeningElement)
        {
            if (Main.gameMenu || Main.LocalPlayer == null) return;
            var player = Main.LocalPlayer.GetModPlayer<HarmonicCoreModPlayer>();
            
            if (player.TryEnhanceCore(slotIndex))
            {
                parentUI.RefreshDisplay();
            }
            else
            {
                // Check why it failed
                if (player.EquippedCores[slotIndex] == null || player.EquippedCores[slotIndex].IsAir)
                {
                    Main.NewText("No core equipped in this slot!", new Color(255, 150, 150));
                }
                else if (player.EnhancementLevels[slotIndex] >= HarmonicCoreModPlayer.MaxEnhancementLevel)
                {
                    Main.NewText("This core is already at maximum enhancement (+5)!", new Color(255, 200, 100));
                }
                else
                {
                    Main.NewText("You need a Seed of Universal Melodies to enhance!", new Color(255, 150, 150));
                }
                SoundEngine.PlaySound(SoundID.MenuClose with { Volume = 0.5f });
            }
        }
        
        public void RefreshSlot()
        {
            var player = Main.LocalPlayer.GetModPlayer<HarmonicCoreModPlayer>();
            string coreName = player.GetCoreName(slotIndex);
            
            if (!string.IsNullOrEmpty(coreName))
            {
                int enhLevel = player.GetEnhancementLevel(slotIndex);
                if (enhLevel > 0)
                {
                    enhancementLabel.SetText($"+{enhLevel}");
                    enhancementLabel.TextColor = Color.Lerp(new Color(180, 150, 220), new Color(255, 220, 100), enhLevel / 5f);
                }
                else
                {
                    enhancementLabel.SetText("");
                }
                
                // Update button appearance based on enhancement state
                if (enhLevel >= HarmonicCoreModPlayer.MaxEnhancementLevel)
                {
                    enhanceButtonText.SetText("MAX");
                    enhanceButtonText.TextColor = new Color(255, 220, 100);
                    enhanceButton.BackgroundColor = new Color(60, 50, 30, 220);
                    enhanceButton.BorderColor = new Color(180, 150, 80, 180);
                }
                else
                {
                    enhanceButtonText.SetText("Enhance");
                    enhanceButtonText.TextColor = new Color(200, 180, 255);
                    enhanceButton.BackgroundColor = new Color(40, 30, 60, 220);
                    enhanceButton.BorderColor = new Color(120, 100, 180, 180);
                }
                
                // Core color border
                if (HarmonicCoreModPlayer.CoreColors.ContainsKey(coreName))
                {
                    Color coreColor = HarmonicCoreModPlayer.CoreColors[coreName];
                    BorderColor = coreColor * 0.6f;
                    BorderColor.A = 180;
                }
            }
            else
            {
                enhancementLabel.SetText("");
                enhanceButtonText.SetText("--");
                enhanceButtonText.TextColor = new Color(100, 100, 120);
                enhanceButton.BackgroundColor = new Color(20, 20, 25, 200);
                enhanceButton.BorderColor = new Color(60, 60, 80, 150);
                BorderColor = new Color(60, 60, 80, 150);
            }
        }
        
        public override void LeftClick(UIMouseEvent evt)
        {
            if (Main.gameMenu || Main.LocalPlayer == null) return;
            var player = Main.LocalPlayer.GetModPlayer<HarmonicCoreModPlayer>();
            
            // If holding a core, try to equip it
            if (Main.mouseItem != null && !Main.mouseItem.IsAir && IsHarmonicCore(Main.mouseItem.type))
            {
                if (player.EquippedCores[slotIndex] != null && !player.EquippedCores[slotIndex].IsAir)
                {
                    // Swap with equipped core
                    Item old = player.EquippedCores[slotIndex].Clone();
                    player.EquipCore(slotIndex, Main.mouseItem);
                    Main.mouseItem = old;
                }
                else
                {
                    // Equip into empty slot
                    player.EquipCore(slotIndex, Main.mouseItem);
                    Main.mouseItem = new Item();
                }
                parentUI.RefreshDisplay();
            }
            // If hand is empty and slot has core, pick it up
            else if ((Main.mouseItem == null || Main.mouseItem.IsAir) && player.EquippedCores[slotIndex] != null && !player.EquippedCores[slotIndex].IsAir)
            {
                Main.mouseItem = player.EquippedCores[slotIndex].Clone();
                player.UnequipCore(slotIndex);
                parentUI.RefreshDisplay();
            }
        }
        
        public override void RightClick(UIMouseEvent evt)
        {
            if (Main.gameMenu || Main.LocalPlayer == null) return;
            var player = Main.LocalPlayer.GetModPlayer<HarmonicCoreModPlayer>();
            
            // Right-click on equipped core to ENHANCE (not swap)
            if (player.EquippedCores[slotIndex] != null && !player.EquippedCores[slotIndex].IsAir)
            {
                // If holding a core, swap it
                if (Main.mouseItem != null && !Main.mouseItem.IsAir && IsHarmonicCore(Main.mouseItem.type))
                {
                    Item old = player.EquippedCores[slotIndex].Clone();
                    player.EquipCore(slotIndex, Main.mouseItem);
                    Main.mouseItem = old;
                    parentUI.RefreshDisplay();
                }
                else
                {
                    // Empty hand - try to enhance the core!
                    if (player.TryEnhanceCore(slotIndex))
                    {
                        parentUI.RefreshDisplay();
                    }
                    else
                    {
                        // Check why it failed and give feedback
                        if (player.EnhancementLevels[slotIndex] >= HarmonicCoreModPlayer.MaxEnhancementLevel)
                        {
                            Main.NewText("This core is already at maximum enhancement (+5)!", new Color(255, 200, 100));
                        }
                        else
                        {
                            Main.NewText("You need a Seed of Universal Melodies to enhance!", new Color(255, 150, 150));
                        }
                        SoundEngine.PlaySound(SoundID.MenuClose with { Volume = 0.5f });
                    }
                }
            }
            else if (Main.mouseItem != null && !Main.mouseItem.IsAir && IsHarmonicCore(Main.mouseItem.type))
            {
                player.EquipCore(slotIndex, Main.mouseItem);
                Main.mouseItem = new Item();
                parentUI.RefreshDisplay();
            }
        }
        
        public override void MiddleClick(UIMouseEvent evt)
        {
            if (Main.gameMenu || Main.LocalPlayer == null) return;
            var player = Main.LocalPlayer.GetModPlayer<HarmonicCoreModPlayer>();
            
            if (player.EquippedCores[slotIndex] != null && !player.EquippedCores[slotIndex].IsAir)
            {
                if (Main.mouseItem == null || Main.mouseItem.IsAir)
                    Main.mouseItem = player.EquippedCores[slotIndex].Clone();
                else
                    Main.LocalPlayer.QuickSpawnItem(Main.LocalPlayer.GetSource_Misc("HarmonicCore"),
                        player.EquippedCores[slotIndex].Clone(), 1);
                player.UnequipCore(slotIndex);
                parentUI.RefreshDisplay();
            }
        }
        
        public override void MouseOver(UIMouseEvent evt)
        {
            var player = Main.LocalPlayer.GetModPlayer<HarmonicCoreModPlayer>();
            string coreName = player.GetCoreName(slotIndex);
            
            if (!string.IsNullOrEmpty(coreName))
            {
                int tier = player.GetCoreTier(slotIndex);
                int enhLevel = player.GetEnhancementLevel(slotIndex);
                float damageBonus = player.GetEnhancedDamageBonus(slotIndex) * 100f;
                string effectName = HarmonicCoreModPlayer.GetCoreEffectName(coreName);
                string effectDesc = HarmonicCoreModPlayer.GetCoreEffectDescWithEnhancement(coreName, enhLevel);
                
                string enhText = enhLevel > 0 ? $" +{enhLevel}" : "";
                string tierText = $"[Tier {tier} Harmonic Core{enhText}]";
                int dmgPercent = (int)damageBonus;
                string damageText = $"+{dmgPercent}%% All Damage";
                
                // Build upgrade preview if not maxed
                string upgradePreview = "";
                if (enhLevel < HarmonicCoreModPlayer.MaxEnhancementLevel)
                {
                    int nextLevel = enhLevel + 1;
                    float nextDamageBonus = player.GetEnhancedDamageBonus(slotIndex) / (1f + enhLevel * 0.2f) * (1f + nextLevel * 0.2f) * 100f;
                    string nextEffectDesc = HarmonicCoreModPlayer.GetCoreEffectDescWithEnhancement(coreName, nextLevel);
                    upgradePreview = $"\n\n[Right-click to enhance to +{nextLevel}]\n→ +{(int)nextDamageBonus}%% Damage\n→ {nextEffectDesc}";
                }
                else
                {
                    upgradePreview = "\n\n[MAX ENHANCEMENT]";
                }
                
                parentUI.SetHoveredSlot(slotIndex, $"{GetDisplayName(coreName)}\n{tierText}\n{damageText}\n◆ {effectName}\n{effectDesc}{upgradePreview}");
            }
            else
                parentUI.SetHoveredSlot(slotIndex, "Empty Slot\nLeft-click with core");
            
            SoundEngine.PlaySound(SoundID.MenuTick with { Volume = 0.3f });
        }
        
        public override void MouseOut(UIMouseEvent evt) => parentUI.ClearHover();
        
        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            base.DrawSelf(spriteBatch);
            pulseTimer += 0.025f;
            
            var player = Main.LocalPlayer.GetModPlayer<HarmonicCoreModPlayer>();
            var dims = GetDimensions();
            
            if (player.EquippedCores[slotIndex] != null && !player.EquippedCores[slotIndex].IsAir)
            {
                Item core = player.EquippedCores[slotIndex];
                Texture2D tex = TextureAssets.Item[core.type].Value;
                float scale = Math.Min(36f / tex.Width, 36f / tex.Height) * (1f + (float)Math.Sin(pulseTimer) * 0.025f);
                Vector2 center = new Vector2(dims.X + dims.Width / 2, dims.Y + 35f);
                Vector2 origin = new Vector2(tex.Width / 2f, tex.Height / 2f);
                
                string coreName = player.GetCoreName(slotIndex);
                Color glow = HarmonicCoreModPlayer.CoreColors.ContainsKey(coreName) ? HarmonicCoreModPlayer.CoreColors[coreName] : Color.White;
                glow *= 0.25f; glow.A = 0;
                
                spriteBatch.Draw(tex, center, null, glow, 0f, origin, scale * 1.3f, SpriteEffects.None, 0f);
                spriteBatch.Draw(tex, center, null, Color.White, 0f, origin, scale, SpriteEffects.None, 0f);
            }
            else
            {
                Vector2 center = new Vector2(dims.X + dims.Width / 2, dims.Y + 35f);
                Utils.DrawBorderString(spriteBatch, "◇", center - new Vector2(6, 10), new Color(80, 80, 100), 0.9f);
            }
        }
        
        private bool IsHarmonicCore(int type)
        {
            return type == ModContent.ItemType<Content.MoonlightSonata.HarmonicCores.HarmonicCoreOfMoonlightSonata>() ||
                   type == ModContent.ItemType<Content.Eroica.HarmonicCores.HarmonicCoreOfEroica>() ||
                   type == ModContent.ItemType<Content.SwanLake.HarmonicCores.HarmonicCoreOfSwanLake>() ||
                   type == ModContent.ItemType<Content.LaCampanella.HarmonicCores.HarmonicCoreOfLaCampanella>() ||
                   type == ModContent.ItemType<Content.EnigmaVariations.HarmonicCores.HarmonicCoreOfEnigma>() ||
                   type == ModContent.ItemType<Content.Fate.HarmonicCores.HarmonicCoreOfFate>();
        }
        
        private string GetDisplayName(string coreName) => coreName switch
        {
            "MoonlightSonata" => "Moonlight Sonata", "SwanLake" => "Swan Lake",
            "LaCampanella" => "La Campanella", _ => coreName
        };
    }
}
