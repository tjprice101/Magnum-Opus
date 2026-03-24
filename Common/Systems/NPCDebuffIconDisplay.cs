using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Common.Systems
{
    /// <summary>
    /// Draws debuff icons above NPC health bars when they have active debuffs.
    /// Makes debuffs more visible during combat.
    /// </summary>
    public class NPCDebuffIconDisplay : ModSystem
    {
        // Max distance to draw icons (performance optimization)
        private const float MaxDrawDistance = 1500f;

        // Icon display settings
        private const float IconScale = 0.65f;
        private const float IconSpacing = 22f;
        private const float IconVerticalOffset = 8f; // Below the NPC

        // Cache for mod buff textures
        private static Dictionary<int, bool> isModDebuff = new Dictionary<int, bool>();

        public override void Load()
        {
            On_Main.DrawNPCs += DrawDebuffIconsAfterNPCs;
        }

        public override void Unload()
        {
            On_Main.DrawNPCs -= DrawDebuffIconsAfterNPCs;
            isModDebuff?.Clear();
            isModDebuff = null;
        }

        private void DrawDebuffIconsAfterNPCs(On_Main.orig_DrawNPCs orig, Main self, bool behindTiles)
        {
            // Call original NPC drawing first
            orig(self, behindTiles);

            // Only draw icons when NPCs are in front (not behind tiles)
            if (behindTiles) return;

            SpriteBatch spriteBatch = Main.spriteBatch;

            try
            {
                // Draw debuff icons for all active NPCs
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (!npc.active || npc.friendly || npc.life <= 0) continue;
                    if (npc.townNPC || npc.CountsAsACritter) continue;

                    // Check distance from player
                    float distance = Vector2.Distance(Main.LocalPlayer.Center, npc.Center);
                    if (distance > MaxDrawDistance) continue;

                    // Get active debuffs on this NPC
                    List<int> activeDebuffs = GetActiveDebuffs(npc);
                    if (activeDebuffs.Count == 0) continue;

                    DrawNPCDebuffIcons(spriteBatch, npc, activeDebuffs);
                }
            }
            catch (Exception)
            {
                // Silently ignore drawing errors
            }
        }

        private List<int> GetActiveDebuffs(NPC npc)
        {
            List<int> debuffs = new List<int>();

            for (int i = 0; i < NPC.maxBuffs; i++)
            {
                int buffType = npc.buffType[i];
                int buffTime = npc.buffTime[i];

                if (buffType <= 0 || buffTime <= 0) continue;
                if (!Main.debuff[buffType]) continue;

                // Skip hidden debuffs
                if (BuffID.Sets.IsATagBuff[buffType]) continue;
                if (buffType == BuffID.Confused && npc.boss) continue; // Bosses don't show confused

                debuffs.Add(buffType);

                // Limit to 8 icons max
                if (debuffs.Count >= 8) break;
            }

            return debuffs;
        }

        private void DrawNPCDebuffIcons(SpriteBatch spriteBatch, NPC npc, List<int> debuffs)
        {
            // Calculate position below the NPC
            Vector2 npcScreenPos = npc.Bottom - Main.screenPosition;

            // Center the icon row
            float totalWidth = debuffs.Count * IconSpacing;
            float startX = npcScreenPos.X - totalWidth / 2f + IconSpacing / 2f;
            float yPos = npcScreenPos.Y + IconVerticalOffset;

            // Add subtle pulsing effect
            float pulse = 0.9f + 0.1f * (float)Math.Sin(Main.GameUpdateCount * 0.1f);

            for (int i = 0; i < debuffs.Count; i++)
            {
                int buffType = debuffs[i];
                Vector2 iconPos = new Vector2(startX + i * IconSpacing, yPos);

                DrawSingleDebuffIcon(spriteBatch, buffType, iconPos, pulse);
            }
        }

        private void DrawSingleDebuffIcon(SpriteBatch spriteBatch, int buffType, Vector2 position, float pulse)
        {
            try
            {
                // Get buff texture
                Texture2D buffTexture = TextureAssets.Buff[buffType].Value;
                if (buffTexture == null) return;

                // Calculate scale and origin
                float scale = IconScale * pulse;
                Vector2 origin = new Vector2(buffTexture.Width / 2f, buffTexture.Height / 2f);

                // Draw dark outline/shadow for visibility
                Color shadowColor = Color.Black * 0.6f;
                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        if (x == 0 && y == 0) continue;
                        spriteBatch.Draw(buffTexture, position + new Vector2(x, y), null, shadowColor, 0f, origin, scale, SpriteEffects.None, 0f);
                    }
                }

                // Draw the icon
                spriteBatch.Draw(buffTexture, position, null, Color.White, 0f, origin, scale, SpriteEffects.None, 0f);

                // Add subtle glow for mod debuffs
                if (IsModDebuff(buffType))
                {
                    Color glowColor = GetDebuffGlowColor(buffType) * 0.3f * pulse;
                    spriteBatch.Draw(buffTexture, position, null, glowColor, 0f, origin, scale * 1.15f, SpriteEffects.None, 0f);
                }
            }
            catch
            {
                // Skip this icon if there's an error
            }
        }

        private bool IsModDebuff(int buffType)
        {
            if (isModDebuff.TryGetValue(buffType, out bool isMod))
                return isMod;

            // Check if this is a modded buff (buffType > vanilla count)
            isMod = buffType >= BuffID.Count;
            isModDebuff[buffType] = isMod;
            return isMod;
        }

        private Color GetDebuffGlowColor(int buffType)
        {
            // Get the ModBuff if it exists and try to determine a thematic color
            if (buffType >= BuffID.Count)
            {
                ModBuff modBuff = BuffLoader.GetBuff(buffType);
                if (modBuff != null)
                {
                    string name = modBuff.Name.ToLower();

                    // Theme-based coloring
                    if (name.Contains("resonant") || name.Contains("resonance"))
                        return Main.hslToRgb((Main.GameUpdateCount * 0.02f) % 1f, 0.8f, 0.7f); // Rainbow
                    if (name.Contains("flame") || name.Contains("burn") || name.Contains("fire"))
                        return new Color(255, 150, 50); // Orange
                    if (name.Contains("frost") || name.Contains("ice") || name.Contains("cold"))
                        return new Color(100, 200, 255); // Ice blue
                    if (name.Contains("poison") || name.Contains("venom"))
                        return new Color(100, 255, 100); // Green
                    if (name.Contains("shadow") || name.Contains("dark") || name.Contains("void"))
                        return new Color(150, 50, 200); // Purple
                    if (name.Contains("holy") || name.Contains("light") || name.Contains("radiant"))
                        return new Color(255, 255, 200); // Golden
                    if (name.Contains("blood") || name.Contains("bleed"))
                        return new Color(200, 50, 50); // Blood red
                    if (name.Contains("moon") || name.Contains("lunar"))
                        return new Color(140, 100, 200); // Moonlight purple
                    if (name.Contains("swan"))
                        return Color.Lerp(Color.White, Color.Black, (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.5f + 0.5f);
                }
            }

            // Default glow color
            return new Color(255, 100, 100);
        }
    }
}
