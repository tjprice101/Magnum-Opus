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
    /// </summary>
    public class HarmonicCoreUIState : UIState
    {
        private UIPanel mainPanel;
        private UIPanel collapsedPanel;
        private CoreSlotPanel[] coreSlots = new CoreSlotPanel[3];
        private UIText titleText;
        private UIText descriptionText;
        private UIText classBonusText;
        
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
            
            // Main panel
            mainPanel = new UIPanel();
            mainPanel.Width.Set(340f, 0f);
            mainPanel.Height.Set(200f, 0f);
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
                coreSlots[i].Height.Set(80f, 0f);
                coreSlots[i].Left.Set(30f + i * 95f, 0f);
                coreSlots[i].Top.Set(35f, 0f);
                mainPanel.Append(coreSlots[i]);
            }
            
            descriptionText = new UIText("", 0.55f, false);
            descriptionText.HAlign = 0.5f;
            descriptionText.Top.Set(122f, 0f);
            descriptionText.TextColor = new Color(180, 180, 200);
            mainPanel.Append(descriptionText);
            
            classBonusText = new UIText("", 0.6f);
            classBonusText.HAlign = 0.5f;
            classBonusText.Top.Set(175f, 0f);
            classBonusText.TextColor = new Color(120, 200, 120);
            mainPanel.Append(classBonusText);
            
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
                    "Click to equip cores\nRight-click to toggle mode" : 
                    "Right-click core to toggle\nChromatic ↔ Diatonic");
            
            int tier = player.GetHighestTier();
            classBonusText.SetText(tier > 0 ? 
                $"All Classes: +{HarmonicCoreModPlayer.TierDamageBonus[tier] * 100f:0}% Damage" : "");
        }
        
        public override void Draw(SpriteBatch spriteBatch)
        {
            if (Main.gameMenu || Main.LocalPlayer == null) return;
            shimmerTimer += 0.02f;
            
            if (!isCollapsed) DrawShimmerBorder(spriteBatch, mainPanel.GetDimensions());
            else DrawShimmerBorder(spriteBatch, collapsedPanel.GetDimensions());
            
            base.Draw(spriteBatch);
            
            if (hoveredSlot >= 0 && !string.IsNullOrEmpty(hoveredDescription))
                DrawHoverTooltip(spriteBatch);
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
        private UIText modeLabel;
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
            
            modeLabel = new UIText("", 0.45f);
            modeLabel.HAlign = 0.5f;
            modeLabel.Top.Set(62f, 0f);
            Append(modeLabel);
        }
        
        public void RefreshSlot()
        {
            var player = Main.LocalPlayer.GetModPlayer<HarmonicCoreModPlayer>();
            string coreName = player.GetCoreName(slotIndex);
            
            if (!string.IsNullOrEmpty(coreName))
            {
                bool isChromatic = player.CoreModes[slotIndex];
                modeLabel.SetText(isChromatic ? "ATK" : "DEF");
                modeLabel.TextColor = isChromatic ? new Color(255, 150, 150) : new Color(150, 150, 255);
                BorderColor = isChromatic ? new Color(180, 80, 80, 180) : new Color(80, 80, 180, 180);
            }
            else
            {
                modeLabel.SetText("");
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
            
            if (player.EquippedCores[slotIndex] != null && !player.EquippedCores[slotIndex].IsAir)
            {
                if (Main.mouseItem == null || Main.mouseItem.IsAir)
                {
                    player.ToggleCoreMode(slotIndex);
                    string modeName = player.CoreModes[slotIndex] ? "Chromatic (Offensive)" : "Diatonic (Defensive)";
                    Main.NewText($"{GetDisplayName(player.GetCoreName(slotIndex))}: {modeName}",
                        player.CoreModes[slotIndex] ? new Color(255, 180, 180) : new Color(180, 180, 255));
                    parentUI.RefreshDisplay();
                }
                else if (IsHarmonicCore(Main.mouseItem.type))
                {
                    Item old = player.EquippedCores[slotIndex].Clone();
                    player.EquipCore(slotIndex, Main.mouseItem);
                    Main.mouseItem = old;
                    parentUI.RefreshDisplay();
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
                bool isChromatic = player.CoreModes[slotIndex];
                string buffName = isChromatic ? HarmonicCoreModPlayer.GetChromaticBuffName(coreName) : HarmonicCoreModPlayer.GetDiatonicBuffName(coreName);
                string buffDesc = isChromatic ? HarmonicCoreModPlayer.GetChromaticBuffDesc(coreName) : HarmonicCoreModPlayer.GetDiatonicBuffDesc(coreName);
                string modeText = isChromatic ? "[Chromatic - Offensive]" : "[Diatonic - Defensive]";
                string setBonus = HarmonicCoreModPlayer.GetActiveSetBonusName(coreName, isChromatic);
                parentUI.SetHoveredSlot(slotIndex, $"{GetDisplayName(coreName)}\n{modeText}\n{buffName}\n{buffDesc}\n{setBonus}");
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
