using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;
using MagnumOpus.Content.Eroica.Bosses;

namespace MagnumOpus.Common.Systems
{
    /// <summary>
    /// Custom sky background effect for Eroica boss fight.
    /// Black at surface, gradient up to deep scarlet red with golden flares and dust particles.
    /// </summary>
    public class EroicaSkyEffect : CustomSky
    {
        private bool isActive = false;
        private float intensity = 0f;
        
        public override void OnLoad()
        {
            // Register the sky effect
        }

        public override void Update(GameTime gameTime)
        {
            if (isActive && intensity < 1f)
            {
                intensity += 0.01f;
            }
            else if (!isActive && intensity > 0f)
            {
                intensity -= 0.02f;
            }
            
            intensity = MathHelper.Clamp(intensity, 0f, 1f);
        }

        public override void Draw(SpriteBatch spriteBatch, float minDepth, float maxDepth)
        {
            if (maxDepth >= 0 && minDepth < 0)
            {
                // Draw gradient background
                Texture2D pixel = Terraria.GameContent.TextureAssets.MagicPixel.Value;
                
                // Calculate player's surface position for gradient
                float surfaceY = (float)Main.worldSurface * 16f;
                float playerRelativeY = Main.LocalPlayer.Center.Y - surfaceY;
                
                // Draw multiple layers for gradient effect
                for (int y = 0; y < Main.screenHeight; y += 4)
                {
                    // Calculate world Y position for this screen row
                    float worldY = Main.screenPosition.Y + y;
                    float relativeY = worldY - surfaceY;
                    
                    // Gradient factor: 0 at surface/below, 1 high in sky
                    float gradientFactor = MathHelper.Clamp(-relativeY / 2000f, 0f, 1f);
                    
                    // Black at bottom, deep scarlet red at top
                    Color baseColor = Color.Lerp(
                        new Color(0, 0, 0), // Black
                        new Color(120, 20, 30), // Deep scarlet red
                        gradientFactor
                    );
                    
                    // Apply intensity
                    Color finalColor = baseColor * intensity;
                    
                    Rectangle rect = new Rectangle(0, y, Main.screenWidth, 4);
                    spriteBatch.Draw(pixel, rect, finalColor);
                }
            }
        }

        public override float GetCloudAlpha()
        {
            return 1f - intensity * 0.8f; // Reduce cloud visibility during fight
        }

        public override void Activate(Vector2 position, params object[] args)
        {
            isActive = true;
        }

        public override void Deactivate(params object[] args)
        {
            isActive = false;
        }

        public override void Reset()
        {
            isActive = false;
            intensity = 0f;
        }

        public override bool IsActive()
        {
            return isActive || intensity > 0f;
        }
    }
    
    /// <summary>
    /// ModSystem that manages the Eroica sky effect and spawns golden flare particles.
    /// </summary>
    public class EroicaSkySystem : ModSystem
    {
        private static bool skyRegistered = false;
        private static bool lastEroicaActive = false;
        
        public override void Load()
        {
            if (!Main.dedServ)
            {
                // Register the sky effect
                SkyManager.Instance["MagnumOpus:EroicaSky"] = new EroicaSkyEffect();
                skyRegistered = true;
            }
        }
        
        public override void Unload()
        {
            skyRegistered = false;
        }

        public override void PostUpdateWorld()
        {
            if (Main.dedServ || !skyRegistered) return;
            
            bool eroicaActive = IsEroicaActive();
            
            // Activate/deactivate sky
            if (eroicaActive && !lastEroicaActive)
            {
                SkyManager.Instance.Activate("MagnumOpus:EroicaSky");
            }
            else if (!eroicaActive && lastEroicaActive)
            {
                SkyManager.Instance.Deactivate("MagnumOpus:EroicaSky");
            }
            
            lastEroicaActive = eroicaActive;
            
            // Spawn golden flare particles during fight
            if (eroicaActive)
            {
                SpawnGoldenFlares();
            }
        }
        
        private bool IsEroicaActive()
        {
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                if (Main.npc[i].active && Main.npc[i].type == ModContent.NPCType<EroicasRetribution>())
                {
                    return true;
                }
            }
            return false;
        }
        
        private void SpawnGoldenFlares()
        {
            Player player = Main.LocalPlayer;
            
            // Spawn golden flares and dust across the screen
            if (Main.rand.NextBool(2))
            {
                // Random position across visible area
                float spawnX = player.Center.X + Main.rand.NextFloat(-1500, 1500);
                float spawnY = player.Center.Y - 400 + Main.rand.NextFloat(-400, 400);
                
                // Golden flare particle
                Dust flare = Dust.NewDustDirect(new Vector2(spawnX, spawnY), 1, 1, DustID.GoldFlame, 0f, 0f, 100, default, Main.rand.NextFloat(1.5f, 2.5f));
                flare.noGravity = true;
                flare.velocity = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(0.5f, 2f));
                flare.fadeIn = 1.5f;
            }
            
            // Occasional larger sparkle
            if (Main.rand.NextBool(4))
            {
                float spawnX = player.Center.X + Main.rand.NextFloat(-1200, 1200);
                float spawnY = player.Center.Y - 300 + Main.rand.NextFloat(-300, 300);
                
                Dust sparkle = Dust.NewDustDirect(new Vector2(spawnX, spawnY), 1, 1, DustID.GoldCoin, 0f, 0f, 0, default, Main.rand.NextFloat(1f, 1.8f));
                sparkle.noGravity = true;
                sparkle.velocity = Main.rand.NextVector2Circular(1f, 1f);
            }
            
            // Red dust mixed in
            if (Main.rand.NextBool(5))
            {
                float spawnX = player.Center.X + Main.rand.NextFloat(-1400, 1400);
                float spawnY = player.Center.Y - 200 + Main.rand.NextFloat(-400, 400);
                
                Dust red = Dust.NewDustDirect(new Vector2(spawnX, spawnY), 1, 1, DustID.CrimsonTorch, 0f, 0f, 150, default, Main.rand.NextFloat(1.2f, 2f));
                red.noGravity = true;
                red.velocity = new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(0.5f, 1.5f));
            }
        }
    }
}
