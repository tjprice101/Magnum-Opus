using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace MagnumOpus.Common.Systems.VFX
{
    /// <summary>
    /// ModSystem that hooks CinematicVFX into Terraria's update and render loops.
    /// Renders after projectiles but before interface for proper layering.
    /// </summary>
    public class CinematicVFXSystem : ModSystem
    {
        public override void PostUpdateProjectiles()
        {
            // Update all cinematic effects
            CinematicVFX.Update();
        }
        
        public override void PostDrawTiles()
        {
            // Render cinematic effects after tiles but before entities for proper layering
            SpriteBatch spriteBatch = Main.spriteBatch;
            
            // Begin with default game matrix
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            try
            {
                CinematicVFX.Render(spriteBatch);
            }
            finally
            {
                spriteBatch.End();
            }
        }
        
        public override void Unload()
        {
            CinematicVFX.Unload();
        }
        
        public override void OnWorldUnload()
        {
            CinematicVFX.Clear();
        }
    }
    
    /// <summary>
    /// Global Projectile hooks for automatic cinematic VFX on projectile kills.
    /// Applies impact glints and energy streaks based on projectile type.
    /// </summary>
    public class CinematicVFXProjectile : GlobalProjectile
    {
        public override void OnKill(Projectile projectile, int timeLeft)
        {
            // Skip if not friendly or if hostile to player
            if (!projectile.friendly && !projectile.hostile) return;
            
            // Check for MagnumOpus theme detection
            string fullName = projectile.GetType().FullName ?? "";
            bool isMagnumOpus = fullName.Contains("MagnumOpus");
            
            if (!isMagnumOpus) return;
            
            // Detect theme from namespace
            Color primaryColor = Color.White;
            Color secondaryColor = Color.LightGray;
            bool shouldGlint = true;
            
            if (fullName.Contains("Eroica"))
            {
                primaryColor = new Color(200, 50, 50);    // Scarlet
                secondaryColor = new Color(255, 200, 80); // Gold
            }
            else if (fullName.Contains("Fate"))
            {
                primaryColor = new Color(140, 50, 90);    // Dark pink
                secondaryColor = new Color(255, 240, 255); // Cosmic white
            }
            else if (fullName.Contains("SwanLake"))
            {
                primaryColor = Color.White;
                secondaryColor = new Color(200, 200, 220);
            }
            else if (fullName.Contains("MoonlightSonata"))
            {
                primaryColor = new Color(100, 60, 160);   // Purple
                secondaryColor = new Color(150, 200, 255); // Ice blue
            }
            else if (fullName.Contains("LaCampanella"))
            {
                primaryColor = new Color(255, 120, 30);   // Orange
                secondaryColor = new Color(255, 200, 80); // Gold
            }
            else if (fullName.Contains("Enigma"))
            {
                primaryColor = new Color(100, 50, 150);   // Purple
                secondaryColor = new Color(50, 200, 100); // Green
            }
            else
            {
                shouldGlint = false; // Unknown theme, don't add automatic glints
            }
            
            if (shouldGlint)
            {
                // Spawn impact glint
                CinematicVFX.SpawnImpactGlint(projectile.Center, primaryColor, 0.6f, 10, true, false);
                
                // Occasional energy streak from impact
                if (Main.rand.NextBool(3))
                {
                    Vector2 streakVel = -projectile.velocity.SafeNormalize(Vector2.Zero) * 2f;
                    CinematicVFX.SpawnEnergyStreak(projectile.Center, streakVel, primaryColor, secondaryColor, 0.5f, 15, 1.5f);
                }
            }
        }
    }
    
    /// <summary>
    /// GlobalNPC hooks for boss phase transitions and deaths.
    /// </summary>
    public class CinematicVFXNPC : GlobalNPC
    {
        public override void OnKill(NPC npc)
        {
            // Major boss death lens flare
            if (npc.boss)
            {
                string fullName = npc.GetType().FullName ?? "";
                
                Color primaryColor = Color.White;
                Color secondaryColor = Color.Gold;
                
                if (fullName.Contains("Eroica"))
                {
                    primaryColor = new Color(255, 80, 50);
                    secondaryColor = new Color(255, 200, 80);
                }
                else if (fullName.Contains("Fate"))
                {
                    primaryColor = new Color(180, 60, 100);
                    secondaryColor = new Color(255, 240, 255);
                }
                else if (fullName.Contains("SwanLake"))
                {
                    primaryColor = Color.White;
                    secondaryColor = new Color(180, 180, 220);
                }
                else if (fullName.Contains("LaCampanella"))
                {
                    primaryColor = new Color(255, 140, 40);
                    secondaryColor = new Color(255, 220, 120);
                }
                else if (fullName.Contains("Enigma"))
                {
                    primaryColor = new Color(120, 60, 180);
                    secondaryColor = new Color(60, 220, 120);
                }
                
                // Epic boss death limit break flare
                CinematicVFX.SpawnBossLimitBreak(npc.Center, primaryColor, secondaryColor, 2f);
            }
        }
    }
}
