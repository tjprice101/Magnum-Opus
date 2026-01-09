using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;

namespace MagnumOpus.Common.Systems
{
    /// <summary>
    /// Advanced VFX utility class for MagnumOpus.
    /// Provides methods for lightning effects, beam drawing, additive glow, fractal sparks, and more.
    /// </summary>
    public static class MagnumVFX
    {
        // ================== ADDITIVE BLENDING UTILITIES ==================
        
        /// <summary>
        /// Begins additive blend mode for glowing effects. Call EndAdditiveBlend() when done.
        /// Use this for energy effects, magic glow, beams, etc.
        /// </summary>
        public static void BeginAdditiveBlend(SpriteBatch spriteBatch)
        {
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, 
                SamplerState.LinearClamp, DepthStencilState.None, 
                RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }
        
        /// <summary>
        /// Returns to normal alpha blend mode after additive drawing.
        /// </summary>
        public static void EndAdditiveBlend(SpriteBatch spriteBatch)
        {
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, 
                SamplerState.LinearClamp, DepthStencilState.None, 
                RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }
        
        // ================== FRACTAL LIGHTNING / SPARK EFFECTS ==================
        // These create the branching lightning effects like Moon Lord weapons
        
        /// <summary>
        /// Generic fractal lightning - use themed versions below for better visuals.
        /// </summary>
        public static void DrawFractalLightning(Vector2 start, Vector2 end, Color color, 
            int segments = 12, float spread = 30f, int branches = 3, float branchLength = 0.4f)
        {
            DrawFractalLightningCustom(start, end, color, Color.White, DustID.Torch, DustID.Torch, 
                segments, spread, branches, branchLength, 6f);
        }
        
        // ================== MOONLIGHT SONATA LIGHTNING (Purple/Silver/White) ==================
        
        /// <summary>
        /// Moonlight-themed fractal lightning with ethereal purple and silver.
        /// Smoother, more flowing appearance with crystal sparkles.
        /// Use for: ResurrectionProjectile, MoonlightBeam, Moonlight weapons
        /// </summary>
        public static void DrawMoonlightLightning(Vector2 start, Vector2 end, 
            int segments = 14, float spread = 25f, int branches = 4, float branchLength = 0.5f)
        {
            List<Vector2> mainPath = GenerateLightningPath(start, end, segments, spread);
            
            // Outer ethereal purple glow
            DrawLightningPathDustCustom(mainPath, new Color(120, 60, 180), DustID.PurpleTorch, 2.2f);
            // Mid silver/lavender - use Shadowflame for purple tint
            DrawLightningPathDustCustom(mainPath, new Color(200, 180, 255), DustID.Shadowflame, 1.6f);
            // Inner bright white-purple core - use SilverCoin for silver/white sparkle
            DrawLightningPathDustCustom(mainPath, new Color(230, 210, 255), DustID.SilverCoin, 1.0f);
            
            // Branches with purple crystal theme
            for (int i = 0; i < branches; i++)
            {
                if (mainPath.Count < 4) continue;
                int branchPoint = Main.rand.Next(2, mainPath.Count - 2);
                Vector2 branchStart = mainPath[branchPoint];
                Vector2 direction = (mainPath[branchPoint + 1] - mainPath[branchPoint - 1]).SafeNormalize(Vector2.UnitX);
                direction = direction.RotatedBy(Main.rand.NextFloat(-0.8f, 0.8f));
                float length = Vector2.Distance(start, end) * branchLength * Main.rand.NextFloat(0.4f, 0.9f);
                Vector2 branchEnd = branchStart + direction * length;
                
                List<Vector2> branchPath = GenerateLightningPath(branchStart, branchEnd, segments / 2, spread * 0.6f);
                DrawLightningPathDustCustom(branchPath, new Color(150, 100, 200), DustID.PurpleCrystalShard, 1.0f);
            }
            
            // Crystal sparkles along path
            foreach (Vector2 point in mainPath)
            {
                if (Main.rand.NextBool(2))
                {
                    int sparkType = Main.rand.NextBool() ? DustID.PurpleCrystalShard : DustID.Enchanted_Pink;
                    Dust spark = Dust.NewDustPerfect(point + Main.rand.NextVector2Circular(8f, 8f), sparkType,
                        Main.rand.NextVector2Circular(2f, 2f), 100, default, 1.0f);
                    spark.noGravity = true;
                    spark.fadeIn = 1.3f;
                }
            }
            
            // Soft purple lighting
            foreach (Vector2 point in mainPath)
            {
                Lighting.AddLight(point, 0.4f, 0.2f, 0.6f);
            }
        }
        
        // ================== EROICA LIGHTNING (Crimson/Gold/Heroic) ==================
        
        /// <summary>
        /// Eroica-themed fractal lightning with crimson fire and golden sparks.
        /// More aggressive, jagged appearance with flame particles.
        /// Use for: TriumphantFractal, FuneralPrayer, Eroica weapons
        /// </summary>
        public static void DrawEroicaLightning(Vector2 start, Vector2 end, 
            int segments = 10, float spread = 40f, int branches = 3, float branchLength = 0.35f)
        {
            List<Vector2> mainPath = GenerateLightningPath(start, end, segments, spread);
            
            // Outer dark crimson flame
            DrawLightningPathDustCustom(mainPath, new Color(180, 30, 50), DustID.CrimsonTorch, 2.5f);
            // Mid bright red/orange
            DrawLightningPathDustCustom(mainPath, new Color(255, 80, 40), DustID.Torch, 1.8f);
            // Inner golden-white core
            DrawLightningPathDustCustom(mainPath, new Color(255, 220, 100), DustID.GoldCoin, 1.0f);
            
            // Aggressive branches
            for (int i = 0; i < branches; i++)
            {
                if (mainPath.Count < 4) continue;
                int branchPoint = Main.rand.Next(2, mainPath.Count - 2);
                Vector2 branchStart = mainPath[branchPoint];
                Vector2 direction = (mainPath[branchPoint + 1] - mainPath[branchPoint - 1]).SafeNormalize(Vector2.UnitX);
                direction = direction.RotatedBy(Main.rand.NextFloat(-1.2f, 1.2f)); // More spread
                float length = Vector2.Distance(start, end) * branchLength * Main.rand.NextFloat(0.5f, 1f);
                Vector2 branchEnd = branchStart + direction * length;
                
                List<Vector2> branchPath = GenerateLightningPath(branchStart, branchEnd, segments / 2, spread * 0.8f);
                DrawLightningPathDustCustom(branchPath, new Color(255, 60, 30), DustID.RedTorch, 1.2f);
            }
            
            // Fire sparks and embers
            foreach (Vector2 point in mainPath)
            {
                if (Main.rand.NextBool(2))
                {
                    // Fire particles
                    Dust fire = Dust.NewDustPerfect(point + Main.rand.NextVector2Circular(6f, 6f), DustID.Torch,
                        Main.rand.NextVector2Circular(3f, 3f) + new Vector2(0, -1f), 100, new Color(255, 150, 50), 1.3f);
                    fire.noGravity = true;
                }
                if (Main.rand.NextBool(4))
                {
                    // Golden sparkle
                    Dust gold = Dust.NewDustPerfect(point, DustID.GoldCoin,
                        Main.rand.NextVector2Circular(2f, 2f), 0, default, 0.9f);
                    gold.noGravity = true;
                }
            }
            
            // Warm crimson lighting
            foreach (Vector2 point in mainPath)
            {
                Lighting.AddLight(point, 0.7f, 0.2f, 0.15f);
            }
        }
        
        // ================== SAKURA/PINK LIGHTNING (For Pink themed attacks) ==================
        
        /// <summary>
        /// Sakura-themed fractal lightning with pink cherry blossom aesthetic.
        /// Soft, flowing with petal-like particles.
        /// Use for: PinkFlamingBolt, SakuraBlossom weapons
        /// </summary>
        public static void DrawSakuraLightning(Vector2 start, Vector2 end, 
            int segments = 12, float spread = 35f, int branches = 5, float branchLength = 0.45f)
        {
            List<Vector2> mainPath = GenerateLightningPath(start, end, segments, spread);
            
            // Outer soft pink glow
            DrawLightningPathDustCustom(mainPath, new Color(255, 150, 180), DustID.PinkTorch, 2.0f);
            // Mid bright magenta - use CrimsonTorch for warm pink
            DrawLightningPathDustCustom(mainPath, new Color(255, 120, 160), DustID.CrimsonTorch, 1.4f);
            // Inner white-pink core - use Torch which takes color well
            DrawLightningPathDustCustom(mainPath, new Color(255, 200, 220), DustID.Torch, 1.0f);
            
            // Delicate petal-like branches (more of them, shorter)
            for (int i = 0; i < branches; i++)
            {
                if (mainPath.Count < 4) continue;
                int branchPoint = Main.rand.Next(2, mainPath.Count - 2);
                Vector2 branchStart = mainPath[branchPoint];
                Vector2 direction = (mainPath[branchPoint + 1] - mainPath[branchPoint - 1]).SafeNormalize(Vector2.UnitX);
                direction = direction.RotatedBy(Main.rand.NextFloat(-0.9f, 0.9f));
                float length = Vector2.Distance(start, end) * branchLength * Main.rand.NextFloat(0.3f, 0.8f);
                Vector2 branchEnd = branchStart + direction * length;
                
                List<Vector2> branchPath = GenerateLightningPath(branchStart, branchEnd, 4, spread * 0.5f);
                DrawLightningPathDustCustom(branchPath, new Color(255, 180, 200), DustID.PinkTorch, 0.8f);
            }
            
            // Petal/fairy sparkles
            foreach (Vector2 point in mainPath)
            {
                if (Main.rand.NextBool(2))
                {
                    Dust petal = Dust.NewDustPerfect(point + Main.rand.NextVector2Circular(10f, 10f), DustID.PinkFairy,
                        Main.rand.NextVector2Circular(1.5f, 1.5f) + new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -0.5f), 
                        150, default, 0.8f);
                    petal.noGravity = true;
                    petal.fadeIn = 1.2f;
                }
            }
            
            // Soft pink lighting
            foreach (Vector2 point in mainPath)
            {
                Lighting.AddLight(point, 0.6f, 0.3f, 0.4f);
            }
        }
        
        // ================== FUNERAL PRAYER LIGHTNING (Dark Red/Black Electric) ==================
        
        /// <summary>
        /// Dark electric lightning for Funeral Prayer - ominous red with black smoke.
        /// Sharp, crackling appearance.
        /// </summary>
        public static void DrawFuneralLightning(Vector2 start, Vector2 end, 
            int segments = 8, float spread = 45f, int branches = 2, float branchLength = 0.3f)
        {
            List<Vector2> mainPath = GenerateLightningPath(start, end, segments, spread);
            
            // Outer black/dark red shadow
            DrawLightningPathDustCustom(mainPath, new Color(80, 20, 30), DustID.Smoke, 2.8f);
            // Mid dark crimson
            DrawLightningPathDustCustom(mainPath, new Color(180, 40, 60), DustID.CrimsonTorch, 2.0f);
            // Inner bright red-white core - use Torch for warm glow
            DrawLightningPathDustCustom(mainPath, new Color(255, 100, 120), DustID.Torch, 1.2f);
            
            // Sharp angular branches
            for (int i = 0; i < branches; i++)
            {
                if (mainPath.Count < 4) continue;
                int branchPoint = Main.rand.Next(2, mainPath.Count - 2);
                Vector2 branchStart = mainPath[branchPoint];
                Vector2 direction = (mainPath[branchPoint + 1] - mainPath[branchPoint - 1]).SafeNormalize(Vector2.UnitX);
                // More extreme angles for sharp look
                direction = direction.RotatedBy(Main.rand.NextBool() ? Main.rand.NextFloat(0.7f, 1.4f) : Main.rand.NextFloat(-1.4f, -0.7f));
                float length = Vector2.Distance(start, end) * branchLength * Main.rand.NextFloat(0.5f, 1f);
                Vector2 branchEnd = branchStart + direction * length;
                
                List<Vector2> branchPath = GenerateLightningPath(branchStart, branchEnd, 4, spread * 0.6f);
                DrawLightningPathDustCustom(branchPath, new Color(200, 50, 70), DustID.RedTorch, 1.0f);
            }
            
            // Dark smoke wisps and red crackles
            foreach (Vector2 point in mainPath)
            {
                if (Main.rand.NextBool(3))
                {
                    Dust smoke = Dust.NewDustPerfect(point + Main.rand.NextVector2Circular(5f, 5f), DustID.Smoke,
                        Main.rand.NextVector2Circular(1f, 1f), 150, Color.Black, 1.0f);
                    smoke.noGravity = true;
                }
                if (Main.rand.NextBool(3))
                {
                    // Red/pink crackle spark
                    Dust crackle = Dust.NewDustPerfect(point, DustID.RedTorch,
                        Main.rand.NextVector2Circular(2f, 2f), 0, new Color(255, 150, 160), 0.9f);
                    crackle.noGravity = true;
                }
            }
            
            // Dark red lighting
            foreach (Vector2 point in mainPath)
            {
                Lighting.AddLight(point, 0.5f, 0.1f, 0.15f);
            }
        }
        
        // ================== HELPER: Custom dust lightning path ==================
        
        private static void DrawLightningPathDustCustom(List<Vector2> path, Color color, int dustType, float scale)
        {
            for (int i = 0; i < path.Count - 1; i++)
            {
                Vector2 segStart = path[i];
                Vector2 segEnd = path[i + 1];
                float segLength = Vector2.Distance(segStart, segEnd);
                Vector2 direction = (segEnd - segStart).SafeNormalize(Vector2.Zero);
                
                for (float d = 0; d < segLength; d += 5f)
                {
                    Vector2 pos = segStart + direction * d;
                    Dust dust = Dust.NewDustPerfect(pos, dustType, Vector2.Zero, 0, color, scale);
                    dust.noGravity = true;
                    dust.noLight = false;
                    dust.fadeIn = 0.4f;
                }
            }
        }
        
        /// <summary>
        /// Custom fractal lightning with full control over dust types and colors.
        /// </summary>
        public static void DrawFractalLightningCustom(Vector2 start, Vector2 end, 
            Color outerColor, Color coreColor, int outerDustType, int coreDustType,
            int segments = 12, float spread = 30f, int branches = 3, float branchLength = 0.4f, float dustSpacing = 6f)
        {
            // Generate the main lightning path
            List<Vector2> mainPath = GenerateLightningPath(start, end, segments, spread);
            
            // Draw the main bolt using dust particles (safe to call anytime)
            DrawLightningPathDustCustom(mainPath, outerColor, outerDustType, 2.0f);
            DrawLightningPathDustCustom(mainPath, coreColor, coreDustType, 1.0f);
            
            // Add branches at random points
            if (branches > 0 && mainPath.Count >= 4)
            {
                for (int i = 0; i < branches; i++)
                {
                    int branchPoint = Main.rand.Next(2, mainPath.Count - 2);
                    Vector2 branchStart = mainPath[branchPoint];
                    
                    // Branch direction - perpendicular to main path with randomization
                    Vector2 direction = (mainPath[branchPoint + 1] - mainPath[branchPoint - 1]).SafeNormalize(Vector2.UnitX);
                    direction = direction.RotatedBy(Main.rand.NextFloat(-1f, 1f));
                    
                    float length = Vector2.Distance(start, end) * branchLength * Main.rand.NextFloat(0.5f, 1f);
                    Vector2 branchEnd = branchStart + direction * length;
                    
                    // Smaller, dimmer branches
                    List<Vector2> branchPath = GenerateLightningPath(branchStart, branchEnd, segments / 2, spread * 0.7f);
                    DrawLightningPathDustCustom(branchPath, outerColor, outerDustType, 1.2f);
                }
            }
            
            // Add extra sparkle dust particles along the bolt
            foreach (Vector2 point in mainPath)
            {
                if (Main.rand.NextBool(3))
                {
                    Dust dust = Dust.NewDustPerfect(point, coreDustType, 
                        Main.rand.NextVector2Circular(2f, 2f), 0, coreColor, 0.8f);
                    dust.noGravity = true;
                    dust.fadeIn = 1.2f;
                }
            }
            
            // Add lighting along path
            foreach (Vector2 point in mainPath)
            {
                Lighting.AddLight(point, outerColor.R / 255f * 0.5f, outerColor.G / 255f * 0.5f, outerColor.B / 255f * 0.5f);
            }
        }
        
        private static List<Vector2> GenerateLightningPath(Vector2 start, Vector2 end, int segments, float spread)
        {
            List<Vector2> path = new List<Vector2>();
            path.Add(start);
            
            Vector2 direction = end - start;
            float length = direction.Length();
            direction.Normalize();
            Vector2 perpendicular = new Vector2(-direction.Y, direction.X);
            
            for (int i = 1; i < segments; i++)
            {
                float progress = i / (float)segments;
                Vector2 basePos = Vector2.Lerp(start, end, progress);
                
                // Add random offset perpendicular to the line
                // More offset in the middle, less at the ends
                float offsetMultiplier = (float)Math.Sin(progress * Math.PI);
                float offset = Main.rand.NextFloat(-spread, spread) * offsetMultiplier;
                
                path.Add(basePos + perpendicular * offset);
            }
            
            path.Add(end);
            return path;
        }
        
        /// <summary>
        /// Draws lightning path using dust particles - safe to call from anywhere.
        /// </summary>
        private static void DrawLightningPathDust(List<Vector2> path, Color color, float scale)
        {
            for (int i = 0; i < path.Count - 1; i++)
            {
                Vector2 segStart = path[i];
                Vector2 segEnd = path[i + 1];
                float segLength = Vector2.Distance(segStart, segEnd);
                Vector2 direction = (segEnd - segStart).SafeNormalize(Vector2.Zero);
                
                // Place dust along segment
                for (float d = 0; d < segLength; d += 6f)
                {
                    Vector2 pos = segStart + direction * d;
                    
                    // Use Electric dust for white, appropriate torch for color
                    int dustType = color == Color.White ? DustID.Electric : DustID.SparksMech;
                    Dust dust = Dust.NewDustPerfect(pos, dustType, Vector2.Zero, 0, color, scale);
                    dust.noGravity = true;
                    dust.noLight = false;
                    dust.fadeIn = 0.5f;
                }
            }
            
            // Add lighting along path
            foreach (Vector2 point in path)
            {
                Lighting.AddLight(point, color.R / 255f * 0.5f, color.G / 255f * 0.5f, color.B / 255f * 0.5f);
            }
        }
        
        /// <summary>
        /// Draws lightning path using SpriteBatch - ONLY call from PreDraw/PostDraw!
        /// </summary>
        private static void DrawLightningPathSprite(List<Vector2> path, Color color, float thickness)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            
            for (int i = 0; i < path.Count - 1; i++)
            {
                Vector2 start = path[i] - Main.screenPosition;
                Vector2 end = path[i + 1] - Main.screenPosition;
                Vector2 direction = end - start;
                float length = direction.Length();
                float rotation = direction.ToRotation();
                
                Main.spriteBatch.Draw(pixel, start, new Rectangle(0, 0, 1, 1), color, 
                    rotation, Vector2.Zero, new Vector2(length, thickness), SpriteEffects.None, 0f);
            }
        }
        
        // ================== FRACTAL SPARK BURST (Like Lunar Portal) ==================
        
        /// <summary>
        /// Creates a burst of fractal sparks emanating from a point.
        /// Similar to the Lunar Portal Staff impact effect.
        /// Safe to call from anywhere (uses dust particles).
        /// </summary>
        public static void CreateFractalSparkBurst(Vector2 position, Color color, int sparkCount = 8, 
            float maxLength = 100f, float spread = 40f)
        {
            for (int i = 0; i < sparkCount; i++)
            {
                float angle = MathHelper.TwoPi * i / sparkCount + Main.rand.NextFloat(-0.3f, 0.3f);
                Vector2 direction = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
                float length = maxLength * Main.rand.NextFloat(0.5f, 1f);
                Vector2 end = position + direction * length;
                
                // Draw each spark as a mini lightning bolt (uses dust now)
                DrawFractalLightning(position, end, color, 6, spread * 0.5f, 1, 0.3f);
            }
            
            // Central bright flash - more particles for impact
            for (int i = 0; i < 20; i++)
            {
                Dust flash = Dust.NewDustPerfect(position, DustID.SparksMech, 
                    Main.rand.NextVector2Circular(6f, 6f), 0, Color.White, 1.8f);
                flash.noGravity = true;
                flash.fadeIn = 1.3f;
            }
            
            // Colored outer burst - use Torch which takes color parameter
            for (int i = 0; i < 15; i++)
            {
                Dust outer = Dust.NewDustPerfect(position, DustID.Torch, 
                    Main.rand.NextVector2Circular(8f, 8f), 0, color, 1.5f);
                outer.noGravity = true;
            }
            
            // Add lighting
            Lighting.AddLight(position, color.R / 255f * 1.5f, color.G / 255f * 1.5f, color.B / 255f * 1.5f);
        }
        
        /// <summary>
        /// Moonlight-themed spark burst with purple crystals and silver sparkles.
        /// </summary>
        public static void CreateMoonlightSparkBurst(Vector2 position, int sparkCount = 10, float maxLength = 120f)
        {
            // Lightning sparks emanating outward
            for (int i = 0; i < sparkCount; i++)
            {
                float angle = MathHelper.TwoPi * i / sparkCount + Main.rand.NextFloat(-0.3f, 0.3f);
                Vector2 direction = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
                float length = maxLength * Main.rand.NextFloat(0.5f, 1f);
                Vector2 end = position + direction * length;
                
                DrawMoonlightLightning(position, end, 5, 20f, 0, 0f);
            }
            
            // Central purple crystal burst
            for (int i = 0; i < 25; i++)
            {
                Dust crystal = Dust.NewDustPerfect(position, DustID.PurpleCrystalShard, 
                    Main.rand.NextVector2Circular(8f, 8f), 100, default, 1.5f);
                crystal.noGravity = true;
                crystal.fadeIn = 1.4f;
            }
            
            // Silver/white sparkles
            for (int i = 0; i < 15; i++)
            {
                Dust silver = Dust.NewDustPerfect(position, DustID.SilverCoin, 
                    Main.rand.NextVector2Circular(6f, 6f), 0, default, 1.2f);
                silver.noGravity = true;
            }
            
            // Enchanted pink orbs floating outward
            for (int i = 0; i < 10; i++)
            {
                Dust fairy = Dust.NewDustPerfect(position, DustID.Enchanted_Pink, 
                    Main.rand.NextVector2Circular(4f, 4f), 150, default, 1.0f);
                fairy.noGravity = true;
                fairy.fadeIn = 1.5f;
            }
            
            Lighting.AddLight(position, 0.6f, 0.3f, 0.8f);
        }
        
        /// <summary>
        /// Eroica-themed spark burst with crimson flames and golden sparks.
        /// </summary>
        public static void CreateEroicaSparkBurst(Vector2 position, int sparkCount = 8, float maxLength = 100f)
        {
            // Fiery lightning sparks emanating outward
            for (int i = 0; i < sparkCount; i++)
            {
                float angle = MathHelper.TwoPi * i / sparkCount + Main.rand.NextFloat(-0.4f, 0.4f);
                Vector2 direction = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
                float length = maxLength * Main.rand.NextFloat(0.5f, 1f);
                Vector2 end = position + direction * length;
                
                DrawEroicaLightning(position, end, 4, 35f, 0, 0f);
            }
            
            // Central crimson fire burst
            for (int i = 0; i < 30; i++)
            {
                Dust fire = Dust.NewDustPerfect(position, DustID.Torch, 
                    Main.rand.NextVector2Circular(10f, 10f) + new Vector2(0, -2f), 100, new Color(255, 80, 50), 2.0f);
                fire.noGravity = true;
            }
            
            // Golden coin sparks
            for (int i = 0; i < 20; i++)
            {
                Dust gold = Dust.NewDustPerfect(position, DustID.GoldCoin, 
                    Main.rand.NextVector2Circular(7f, 7f), 0, default, 1.3f);
                gold.noGravity = true;
            }
            
            // Red embers
            for (int i = 0; i < 15; i++)
            {
                Dust ember = Dust.NewDustPerfect(position, DustID.CrimsonTorch, 
                    Main.rand.NextVector2Circular(5f, 5f) + new Vector2(Main.rand.NextFloat(-1f, 1f), -3f), 
                    0, default, 1.5f);
                ember.noGravity = true;
            }
            
            Lighting.AddLight(position, 1.0f, 0.4f, 0.2f);
        }
        
        /// <summary>
        /// Sakura/Pink-themed spark burst with cherry blossom petals.
        /// </summary>
        public static void CreateSakuraSparkBurst(Vector2 position, int sparkCount = 12, float maxLength = 110f)
        {
            // Soft pink lightning sparks
            for (int i = 0; i < sparkCount; i++)
            {
                float angle = MathHelper.TwoPi * i / sparkCount + Main.rand.NextFloat(-0.3f, 0.3f);
                Vector2 direction = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
                float length = maxLength * Main.rand.NextFloat(0.4f, 0.9f);
                Vector2 end = position + direction * length;
                
                DrawSakuraLightning(position, end, 5, 25f, 1, 0.3f);
            }
            
            // Central pink burst
            for (int i = 0; i < 20; i++)
            {
                Dust pink = Dust.NewDustPerfect(position, DustID.PinkTorch, 
                    Main.rand.NextVector2Circular(6f, 6f), 100, new Color(255, 180, 200), 1.6f);
                pink.noGravity = true;
                pink.fadeIn = 1.3f;
            }
            
            // Fairy sparkles floating like petals
            for (int i = 0; i < 25; i++)
            {
                Vector2 petalVelocity = Main.rand.NextVector2Circular(5f, 5f) + new Vector2(Main.rand.NextFloat(-2f, 2f), -1f);
                Dust petal = Dust.NewDustPerfect(position + Main.rand.NextVector2Circular(20f, 20f), 
                    DustID.PinkFairy, petalVelocity, 150, default, 0.9f);
                petal.noGravity = true;
                petal.fadeIn = 1.5f;
            }
            
            Lighting.AddLight(position, 0.7f, 0.4f, 0.5f);
        }
        
        /// <summary>
        /// Funeral Prayer-themed spark burst with dark crimson and black smoke.
        /// </summary>
        public static void CreateFuneralSparkBurst(Vector2 position, int sparkCount = 6, float maxLength = 80f)
        {
            // Dark crackling lightning
            for (int i = 0; i < sparkCount; i++)
            {
                float angle = MathHelper.TwoPi * i / sparkCount + Main.rand.NextFloat(-0.5f, 0.5f);
                Vector2 direction = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
                float length = maxLength * Main.rand.NextFloat(0.5f, 1f);
                Vector2 end = position + direction * length;
                
                DrawFuneralLightning(position, end, 4, 35f, 0, 0f);
            }
            
            // Central dark crimson explosion
            for (int i = 0; i < 15; i++)
            {
                Dust crimson = Dust.NewDustPerfect(position, DustID.RedTorch, 
                    Main.rand.NextVector2Circular(8f, 8f), 100, new Color(180, 30, 50), 1.8f);
                crimson.noGravity = true;
            }
            
            // Black smoke clouds
            for (int i = 0; i < 20; i++)
            {
                Dust smoke = Dust.NewDustPerfect(position + Main.rand.NextVector2Circular(15f, 15f), 
                    DustID.Smoke, Main.rand.NextVector2Circular(3f, 3f), 200, Color.Black, 1.5f);
                smoke.noGravity = true;
            }
            
            // Red/crimson crackles instead of blue electric
            for (int i = 0; i < 10; i++)
            {
                Dust crackle = Dust.NewDustPerfect(position, DustID.RedTorch, 
                    Main.rand.NextVector2Circular(5f, 5f), 0, new Color(255, 150, 160), 1.2f);
                crackle.noGravity = true;
            }
            
            Lighting.AddLight(position, 0.6f, 0.15f, 0.2f);
        }
        
        // ================== GLOWING TRAIL DRAWING ==================
        
        /// <summary>
        /// Draws a smooth glowing trail using oldPos array.
        /// Great for projectile trails with fade-out effect.
        /// </summary>
        public static void DrawGlowingTrail(SpriteBatch spriteBatch, Projectile projectile, 
            Color startColor, Color endColor, float startWidth = 20f, float endWidth = 2f)
        {
            if (projectile.oldPos.Length < 2) return;
            
            Texture2D texture = TextureAssets.MagicPixel.Value;
            
            for (int i = 0; i < projectile.oldPos.Length - 1; i++)
            {
                if (projectile.oldPos[i] == Vector2.Zero || projectile.oldPos[i + 1] == Vector2.Zero) 
                    continue;
                
                float progress = i / (float)projectile.oldPos.Length;
                Color color = Color.Lerp(startColor, endColor, progress) * (1f - progress);
                float width = MathHelper.Lerp(startWidth, endWidth, progress);
                
                Vector2 start = projectile.oldPos[i] + projectile.Size / 2f - Main.screenPosition;
                Vector2 end = projectile.oldPos[i + 1] + projectile.Size / 2f - Main.screenPosition;
                Vector2 direction = end - start;
                float length = direction.Length();
                float rotation = direction.ToRotation();
                
                // Draw segment
                spriteBatch.Draw(texture, start, new Rectangle(0, 0, 1, 1), color,
                    rotation, new Vector2(0, 0.5f), new Vector2(length, width), SpriteEffects.None, 0f);
            }
        }
        
        // ================== SWING ARC / WHOOSH TRAIL EFFECTS ==================
        
        /// <summary>
        /// Draws a curved swing arc trail like True Biome Blade / Zenith weapons.
        /// Call from PreDraw with additive blending enabled.
        /// Uses the projectile's oldPos and oldRot arrays for smooth arc.
        /// </summary>
        /// <param name="spriteBatch">The SpriteBatch (should be in additive blend mode)</param>
        /// <param name="projectile">The swing projectile with oldPos/oldRot populated</param>
        /// <param name="trailColor">Primary color of the arc (will fade to transparent)</param>
        /// <param name="trailLength">How much of oldPos to use (0-1, default uses all)</param>
        /// <param name="startWidth">Width at the newest part of the trail</param>
        /// <param name="endWidth">Width at the oldest part (tip) of the trail</param>
        public static void DrawSwingArcTrail(SpriteBatch spriteBatch, Projectile projectile,
            Color trailColor, float trailLength = 1f, float startWidth = 40f, float endWidth = 4f)
        {
            if (projectile.oldPos.Length < 2) return;
            
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            int trailCount = (int)(projectile.oldPos.Length * trailLength);
            
            for (int i = 0; i < trailCount - 1; i++)
            {
                if (projectile.oldPos[i] == Vector2.Zero || projectile.oldPos[i + 1] == Vector2.Zero)
                    continue;
                
                float progress = i / (float)trailCount;
                float opacity = (1f - progress) * (1f - progress); // Quadratic falloff for smoother fade
                Color segmentColor = trailColor * opacity;
                float width = MathHelper.Lerp(startWidth, endWidth, progress);
                
                Vector2 start = projectile.oldPos[i] + projectile.Size / 2f - Main.screenPosition;
                Vector2 end = projectile.oldPos[i + 1] + projectile.Size / 2f - Main.screenPosition;
                Vector2 direction = end - start;
                float length = direction.Length();
                if (length < 1f) continue;
                float rotation = direction.ToRotation();
                
                // Outer glow (wider, more transparent)
                spriteBatch.Draw(pixel, start, new Rectangle(0, 0, 1, 1), segmentColor * 0.3f,
                    rotation, new Vector2(0, 0.5f), new Vector2(length, width * 2.5f), SpriteEffects.None, 0f);
                
                // Main trail
                spriteBatch.Draw(pixel, start, new Rectangle(0, 0, 1, 1), segmentColor * 0.7f,
                    rotation, new Vector2(0, 0.5f), new Vector2(length, width), SpriteEffects.None, 0f);
                
                // Bright core
                spriteBatch.Draw(pixel, start, new Rectangle(0, 0, 1, 1), Color.White * opacity * 0.5f,
                    rotation, new Vector2(0, 0.5f), new Vector2(length, width * 0.3f), SpriteEffects.None, 0f);
            }
        }
        
        /// <summary>
        /// Draws a curved swing arc with gradient from one color to another.
        /// Great for weapons with multi-colored effects.
        /// </summary>
        public static void DrawSwingArcTrailGradient(SpriteBatch spriteBatch, Projectile projectile,
            Color innerColor, Color outerColor, float trailLength = 1f, float startWidth = 50f, float endWidth = 6f)
        {
            if (projectile.oldPos.Length < 2) return;
            
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            int trailCount = (int)(projectile.oldPos.Length * trailLength);
            
            for (int i = 0; i < trailCount - 1; i++)
            {
                if (projectile.oldPos[i] == Vector2.Zero || projectile.oldPos[i + 1] == Vector2.Zero)
                    continue;
                
                float progress = i / (float)trailCount;
                float opacity = (1f - progress);
                opacity *= opacity; // Quadratic falloff
                
                // Color transitions from inner to outer along the trail
                Color currentColor = Color.Lerp(innerColor, outerColor, progress);
                float width = MathHelper.Lerp(startWidth, endWidth, progress);
                
                Vector2 start = projectile.oldPos[i] + projectile.Size / 2f - Main.screenPosition;
                Vector2 end = projectile.oldPos[i + 1] + projectile.Size / 2f - Main.screenPosition;
                Vector2 direction = end - start;
                float length = direction.Length();
                if (length < 1f) continue;
                float rotation = direction.ToRotation();
                
                // Outer glow with outer color
                spriteBatch.Draw(pixel, start, new Rectangle(0, 0, 1, 1), outerColor * opacity * 0.25f,
                    rotation, new Vector2(0, 0.5f), new Vector2(length, width * 3f), SpriteEffects.None, 0f);
                
                // Main colored trail
                spriteBatch.Draw(pixel, start, new Rectangle(0, 0, 1, 1), currentColor * opacity * 0.8f,
                    rotation, new Vector2(0, 0.5f), new Vector2(length, width), SpriteEffects.None, 0f);
                
                // White-ish core using inner color lightened
                Color coreColor = Color.Lerp(currentColor, Color.White, 0.6f);
                spriteBatch.Draw(pixel, start, new Rectangle(0, 0, 1, 1), coreColor * opacity * 0.6f,
                    rotation, new Vector2(0, 0.5f), new Vector2(length, width * 0.25f), SpriteEffects.None, 0f);
            }
        }
        
        /// <summary>
        /// Moonlight Sonata themed swing arc - purple to white gradient with sparkles.
        /// </summary>
        public static void DrawMoonlightSwingArc(SpriteBatch spriteBatch, Projectile projectile,
            float startWidth = 45f, float endWidth = 5f)
        {
            DrawSwingArcTrailGradient(spriteBatch, projectile, 
                new Color(180, 120, 255), // Inner purple
                new Color(100, 50, 180),  // Outer deep purple
                1f, startWidth, endWidth);
        }
        
        /// <summary>
        /// Eroica themed swing arc - crimson to gold gradient with fire.
        /// </summary>
        public static void DrawEroicaSwingArc(SpriteBatch spriteBatch, Projectile projectile,
            float startWidth = 50f, float endWidth = 6f)
        {
            DrawSwingArcTrailGradient(spriteBatch, projectile,
                new Color(255, 200, 80),  // Inner gold
                new Color(200, 50, 30),   // Outer crimson
                1f, startWidth, endWidth);
        }
        
        /// <summary>
        /// Creates dust particles along a swing arc for extra flair.
        /// Call from AI() to spawn particles along the swing path.
        /// </summary>
        public static void CreateSwingArcDust(Projectile projectile, Color color, int dustType = DustID.Torch,
            float dustChance = 0.5f, float scale = 1.5f)
        {
            if (projectile.oldPos.Length < 2) return;
            
            // Spawn dust at current position and a few old positions
            for (int i = 0; i < Math.Min(5, projectile.oldPos.Length); i++)
            {
                if (projectile.oldPos[i] == Vector2.Zero) continue;
                if (Main.rand.NextFloat() > dustChance) continue;
                
                Vector2 pos = projectile.oldPos[i] + projectile.Size / 2f;
                Vector2 velocity = Main.rand.NextVector2Circular(1f, 1f);
                
                Dust dust = Dust.NewDustPerfect(pos, dustType, velocity, 100, color, scale * (1f - i * 0.15f));
                dust.noGravity = true;
                dust.fadeIn = 1.2f;
            }
        }
        
        /// <summary>
        /// Creates sparkle/star particles along swing arc - great for magical weapons.
        /// </summary>
        public static void CreateSwingArcSparkles(Projectile projectile, Color color, 
            float sparkleChance = 0.3f)
        {
            if (projectile.oldPos.Length < 2) return;
            
            for (int i = 0; i < Math.Min(8, projectile.oldPos.Length); i++)
            {
                if (projectile.oldPos[i] == Vector2.Zero) continue;
                if (Main.rand.NextFloat() > sparkleChance) continue;
                
                Vector2 pos = projectile.oldPos[i] + projectile.Size / 2f + Main.rand.NextVector2Circular(10f, 10f);
                Vector2 velocity = Main.rand.NextVector2Circular(2f, 2f);
                
                // Star-like sparkle - uses SparksMech which takes color parameter well
                Dust sparkle = Dust.NewDustPerfect(pos, DustID.SparksMech, velocity, 150, color, 0.8f);
                sparkle.noGravity = true;
                sparkle.fadeIn = 1.3f;
            }
        }
        
        // ================== PULSE / SHOCKWAVE RING ==================
        
        /// <summary>
        /// Creates an expanding ring/shockwave effect.
        /// </summary>
        public static void CreateShockwaveRing(Vector2 center, Color color, float radius, 
            float thickness = 4f, int dustCount = 30)
        {
            for (int i = 0; i < dustCount; i++)
            {
                float angle = MathHelper.TwoPi * i / dustCount;
                Vector2 offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius;
                Vector2 velocity = offset.SafeNormalize(Vector2.Zero) * 2f;
                
                Dust dust = Dust.NewDustPerfect(center + offset, DustID.SparksMech, velocity, 0, color, thickness / 4f);
                dust.noGravity = true;
                dust.fadeIn = 1.3f;
            }
            
            Lighting.AddLight(center, color.R / 255f, color.G / 255f, color.B / 255f);
        }
        
        // ================== SWIRLING VORTEX PARTICLES ==================
        
        /// <summary>
        /// Creates swirling particles converging toward or emanating from a point.
        /// Great for charging effects or magical impacts.
        /// </summary>
        public static void CreateVortexParticles(Vector2 center, Color color, float radius, 
            int particleCount = 15, bool converging = true, int dustType = DustID.PurpleTorch)
        {
            for (int i = 0; i < particleCount; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float dist = Main.rand.NextFloat(radius * 0.3f, radius);
                Vector2 offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * dist;
                Vector2 position = center + offset;
                
                // Velocity tangent to circle (creates spiral effect)
                Vector2 tangent = new Vector2(-offset.Y, offset.X).SafeNormalize(Vector2.Zero);
                Vector2 radial = converging ? -offset.SafeNormalize(Vector2.Zero) : offset.SafeNormalize(Vector2.Zero);
                Vector2 velocity = (tangent * 3f + radial * 2f) * (converging ? 1f : -1f);
                
                Dust dust = Dust.NewDustPerfect(position, dustType, velocity, 100, color, 1.5f);
                dust.noGravity = true;
                dust.fadeIn = 1.2f;
            }
        }
        
        // ================== AFTERIMAGE / GHOST DRAWING ==================
        
        /// <summary>
        /// Draws ghostly afterimages of a texture at old positions.
        /// </summary>
        public static void DrawAfterimages(SpriteBatch spriteBatch, Texture2D texture, Projectile projectile,
            Color color, int imageCount = 7, float startOpacity = 0.5f)
        {
            Vector2 origin = texture.Size() / 2f;
            
            for (int i = 0; i < Math.Min(imageCount, projectile.oldPos.Length); i++)
            {
                if (projectile.oldPos[i] == Vector2.Zero) continue;
                
                float progress = i / (float)imageCount;
                float opacity = startOpacity * (1f - progress);
                float scale = projectile.scale * (1f - progress * 0.3f);
                
                Vector2 drawPos = projectile.oldPos[i] + projectile.Size / 2f - Main.screenPosition;
                float rotation = projectile.oldRot.Length > i ? projectile.oldRot[i] : projectile.rotation;
                
                spriteBatch.Draw(texture, drawPos, null, color * opacity, rotation, origin, scale, SpriteEffects.None, 0f);
            }
        }
        
        // ================== PULSING GLOW EFFECT ==================
        
        /// <summary>
        /// Returns a pulsing alpha/scale multiplier for glow effects.
        /// </summary>
        public static float GetPulse(float speed = 0.1f, float minValue = 0.7f, float maxValue = 1f)
        {
            float pulse = (float)Math.Sin(Main.GameUpdateCount * speed);
            return MathHelper.Lerp(minValue, maxValue, (pulse + 1f) / 2f);
        }
        
        // ================== MUSICAL NOTE PARTICLES (Themed for your mod) ==================
        
        /// <summary>
        /// Creates musical note-shaped particle bursts using dust arrangements.
        /// Perfect for the musical theme of MagnumOpus.
        /// </summary>
        public static void CreateMusicalBurst(Vector2 position, Color primaryColor, Color secondaryColor, 
            int intensity = 1)
        {
            int baseCount = 8 * intensity;
            
            // Main burst - alternating colors
            for (int i = 0; i < baseCount; i++)
            {
                float angle = MathHelper.TwoPi * i / baseCount;
                Vector2 velocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * (4f + intensity);
                
                Color dustColor = i % 2 == 0 ? primaryColor : secondaryColor;
                int dustType = i % 2 == 0 ? DustID.PurpleTorch : DustID.IceTorch;
                
                Dust dust = Dust.NewDustPerfect(position, dustType, velocity, 100, default, 1.5f + intensity * 0.3f);
                dust.noGravity = true;
                dust.fadeIn = 1.3f;
            }
            
            // White sparkle core
            for (int i = 0; i < 5 * intensity; i++)
            {
                Dust sparkle = Dust.NewDustPerfect(position, DustID.SparksMech, 
                    Main.rand.NextVector2Circular(3f, 3f), 0, Color.White, 1.2f);
                sparkle.noGravity = true;
            }
            
            // Light
            Lighting.AddLight(position, primaryColor.R / 255f * 0.8f, primaryColor.G / 255f * 0.8f, primaryColor.B / 255f * 0.8f);
        }
        
        // ================== MOONLIGHT-THEMED BEAM ==================
        
        /// <summary>
        /// Creates a moonlight-themed beam effect with the signature purple-white gradient.
        /// </summary>
        public static void DrawMoonlightBeam(Vector2 start, Vector2 end, float width = 30f, float alpha = 1f)
        {
            Vector2 direction = end - start;
            float length = direction.Length();
            float rotation = direction.ToRotation();
            
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            
            // Outer purple glow
            Main.spriteBatch.Draw(pixel, start - Main.screenPosition, new Rectangle(0, 0, 1, 1),
                new Color(150, 80, 200) * alpha * 0.4f, rotation, new Vector2(0, 0.5f), 
                new Vector2(length, width * 1.5f), SpriteEffects.None, 0f);
            
            // Mid layer - light purple
            Main.spriteBatch.Draw(pixel, start - Main.screenPosition, new Rectangle(0, 0, 1, 1),
                new Color(200, 150, 255) * alpha * 0.6f, rotation, new Vector2(0, 0.5f), 
                new Vector2(length, width), SpriteEffects.None, 0f);
            
            // Core - white
            Main.spriteBatch.Draw(pixel, start - Main.screenPosition, new Rectangle(0, 0, 1, 1),
                Color.White * alpha * 0.8f, rotation, new Vector2(0, 0.5f), 
                new Vector2(length, width * 0.4f), SpriteEffects.None, 0f);
            
            // Add lighting along the beam
            for (float i = 0; i < length; i += 50f)
            {
                Vector2 lightPos = start + direction.SafeNormalize(Vector2.Zero) * i;
                Lighting.AddLight(lightPos, 0.6f, 0.4f, 0.9f);
            }
        }
        
        // ================== EROICA-THEMED EFFECTS (Pink/Crimson) ==================
        
        /// <summary>
        /// Creates Eroica-themed heroic burst with pink/crimson colors and sakura-like particles.
        /// </summary>
        public static void CreateEroicaBurst(Vector2 position, int intensity = 1)
        {
            // Crimson/pink main burst
            for (int i = 0; i < 12 * intensity; i++)
            {
                float angle = MathHelper.TwoPi * i / (12 * intensity);
                Vector2 velocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * (5f + intensity * 2f);
                
                int dustType = i % 3 == 0 ? DustID.PinkTorch : (i % 3 == 1 ? DustID.Firework_Pink : DustID.CrimsonTorch);
                Dust dust = Dust.NewDustPerfect(position, dustType, velocity, 100, default, 1.8f);
                dust.noGravity = true;
                dust.fadeIn = 1.5f;
            }
            
            // Golden hero sparks
            for (int i = 0; i < 8 * intensity; i++)
            {
                Dust gold = Dust.NewDustPerfect(position, DustID.GoldCoin, 
                    Main.rand.NextVector2Circular(4f, 4f), 0, default, 1.3f);
                gold.noGravity = true;
            }
            
            Lighting.AddLight(position, 0.9f, 0.4f, 0.6f);
        }
        
        // ================== CALAMITY-STYLE PARTICLE INTEGRATION ==================
        // These methods use the new particle system for higher quality effects
        
        /// <summary>
        /// Creates a burst of bloom particles for explosion/impact effects.
        /// Uses the new particle system for smooth scaling and fading.
        /// </summary>
        public static void CreateBloomBurst(Vector2 position, Color color, int count = 8, 
            float minScale = 0.3f, float maxScale = 1f, int minLife = 20, int maxLife = 40)
        {
            for (int i = 0; i < count; i++)
            {
                Vector2 velocity = Main.rand.NextVector2Circular(3f, 3f);
                float scale = Main.rand.NextFloat(minScale, maxScale);
                int lifetime = Main.rand.Next(minLife, maxLife);
                
                var particle = new Particles.BloomParticle(position, velocity, color, scale, scale * 1.8f, lifetime);
                Particles.MagnumParticleHandler.SpawnParticle(particle);
            }
        }
        
        /// <summary>
        /// Creates an expanding animated shockwave ring particle effect.
        /// </summary>
        public static void CreateExpandingShockwave(Vector2 position, Color color, float startScale = 0.5f, 
            float expansionRate = 0.1f, int lifetime = 30)
        {
            var ring = new Particles.BloomRingParticle(position, Vector2.Zero, color, startScale, lifetime, expansionRate);
            Particles.MagnumParticleHandler.SpawnParticle(ring);
        }
        
        /// <summary>
        /// Creates a burst of sparkle particles.
        /// </summary>
        public static void CreateSparkleBurst(Vector2 position, Color color, int count = 12, 
            float speedMultiplier = 1f, int minLife = 40, int maxLife = 80)
        {
            Color bloomColor = color * 0.7f;
            
            for (int i = 0; i < count; i++)
            {
                float angle = MathHelper.TwoPi * i / count + Main.rand.NextFloat(-0.2f, 0.2f);
                Vector2 velocity = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 5f) * speedMultiplier;
                float scale = Main.rand.NextFloat(0.4f, 1f);
                int lifetime = Main.rand.Next(minLife, maxLife);
                
                var sparkle = new Particles.SparkleParticle(position, velocity, color, bloomColor, 
                    scale, lifetime, 0.05f, 1.5f);
                Particles.MagnumParticleHandler.SpawnParticle(sparkle);
            }
        }
        
        /// <summary>
        /// Creates directional spark particles that follow a velocity.
        /// Great for sword slashes, impacts, etc.
        /// </summary>
        public static void CreateDirectionalSparks(Vector2 position, Vector2 direction, Color color, 
            int count = 6, float spread = 0.5f, float minSpeed = 4f, float maxSpeed = 8f)
        {
            float baseAngle = direction.ToRotation();
            
            for (int i = 0; i < count; i++)
            {
                float angle = baseAngle + Main.rand.NextFloat(-spread, spread);
                float speed = Main.rand.NextFloat(minSpeed, maxSpeed);
                Vector2 velocity = angle.ToRotationVector2() * speed;
                
                var spark = new Particles.GlowSparkParticle(position, velocity, color, 
                    Main.rand.NextFloat(0.4f, 0.8f), Main.rand.Next(20, 40));
                Particles.MagnumParticleHandler.SpawnParticle(spark);
            }
        }
        
        /// <summary>
        /// Creates an impact effect with bloom, ring, and sparks combined.
        /// </summary>
        public static void CreateImpactEffect(Vector2 position, Color primaryColor, Color secondaryColor, float intensity = 1f)
        {
            // Central bloom
            int bloomCount = (int)(5 * intensity);
            CreateBloomBurst(position, primaryColor, bloomCount, 0.4f * intensity, 0.8f * intensity, 15, 30);
            
            // Shockwave ring
            CreateExpandingShockwave(position, primaryColor * 0.5f, 0.3f * intensity, 0.08f, 25);
            
            // Sparks
            int sparkCount = (int)(8 * intensity);
            for (int i = 0; i < sparkCount; i++)
            {
                Vector2 velocity = Main.rand.NextVector2Circular(6f, 6f) * intensity;
                var spark = new Particles.GlowSparkParticle(position, velocity, true, 
                    Main.rand.Next(30, 50), Main.rand.NextFloat(0.3f, 0.6f) * intensity, 
                    Color.Lerp(primaryColor, secondaryColor, Main.rand.NextFloat()),
                    new Vector2(0.4f, 1.8f), true, true);
                Particles.MagnumParticleHandler.SpawnParticle(spark);
            }
            
            // Lighting
            float lightIntensity = 0.8f * intensity;
            Lighting.AddLight(position, primaryColor.ToVector3() * lightIntensity);
        }
        
        /// <summary>
        /// Creates a Moonlight-themed particle burst using the new particle system.
        /// Purple/silver/white ethereal effect.
        /// </summary>
        public static void CreateMoonlightParticleBurst(Vector2 position, int intensity = 1)
        {
            Color purple = new Color(150, 100, 200);
            Color silver = new Color(200, 180, 255);
            Color white = new Color(230, 220, 255);
            
            // Bloom particles
            CreateBloomBurst(position, purple, 4 * intensity, 0.3f, 0.7f, 25, 45);
            
            // Sparkles
            CreateSparkleBurst(position, silver, 8 * intensity, 1.2f, 50, 90);
            
            // Ring effect
            CreateExpandingShockwave(position, purple * 0.6f, 0.4f, 0.06f, 35);
            
            // Small glowing particles
            for (int i = 0; i < 6 * intensity; i++)
            {
                Vector2 velocity = Main.rand.NextVector2Circular(2f, 2f);
                var glow = new Particles.GenericGlowParticle(position, velocity, 
                    Color.Lerp(silver, white, Main.rand.NextFloat()), 
                    Main.rand.NextFloat(0.2f, 0.5f), Main.rand.Next(40, 70));
                Particles.MagnumParticleHandler.SpawnParticle(glow);
            }
        }
        
        /// <summary>
        /// Creates an Eroica-themed particle burst using the new particle system.
        /// Golden/crimson heroic fire effect.
        /// </summary>
        public static void CreateEroicaParticleBurst(Vector2 position, int intensity = 1)
        {
            Color crimson = new Color(200, 50, 60);
            Color gold = new Color(255, 200, 80);
            Color orange = new Color(255, 150, 50);
            
            // Fiery bloom particles
            CreateBloomBurst(position, crimson, 5 * intensity, 0.4f, 0.9f, 20, 40);
            CreateBloomBurst(position, gold, 3 * intensity, 0.2f, 0.5f, 15, 30);
            
            // Golden sparkles
            CreateSparkleBurst(position, gold, 6 * intensity, 1.5f, 40, 70);
            
            // Fire sparks
            for (int i = 0; i < 8 * intensity; i++)
            {
                Vector2 velocity = Main.rand.NextVector2Circular(5f, 5f) + new Vector2(0, -2f);
                var spark = new Particles.GlowSparkParticle(position, velocity, true, 
                    Main.rand.Next(25, 45), Main.rand.NextFloat(0.4f, 0.8f), 
                    Color.Lerp(crimson, orange, Main.rand.NextFloat()),
                    new Vector2(0.5f, 1.6f), false, true);
                Particles.MagnumParticleHandler.SpawnParticle(spark);
            }
            
            // Ring
            CreateExpandingShockwave(position, gold * 0.5f, 0.5f, 0.1f, 25);
        }
        
        // ================== PRIMITIVE TRAIL HELPERS ==================
        // Convenience methods for using the PrimitiveTrailRenderer
        
        /// <summary>
        /// Draws a glowing projectile trail using the new primitive renderer.
        /// </summary>
        public static void DrawPrimitiveProjectileTrail(Projectile proj, Color trailColor, 
            float startWidth = 20f, float endWidth = 0f, Color? glowColor = null)
        {
            if (proj.oldPos[0] == Vector2.Zero) return;
            
            Color glow = glowColor ?? trailColor * 0.5f;
            
            var settings = new PrimitiveTrailRenderer.TrailSettings(
                p => MathHelper.Lerp(startWidth, endWidth, p),
                p => Color.Lerp(trailColor, glow * (1f - p), p),
                null,
                null,
                true,
                false
            );
            
            PrimitiveTrailRenderer.RenderTrail(proj.oldPos, settings, 40);
        }
        
        /// <summary>
        /// Draws a simple projectile trail with automatic afterimages.
        /// </summary>
        public static void DrawSimpleProjectileTrail(Projectile proj, Color color, int afterimageMode = 0)
        {
            MagnumDrawingUtils.DrawAfterimagesCentered(proj, afterimageMode, color);
        }
        
        /// <summary>
        /// Draws a projectile with backglow effect.
        /// </summary>
        public static void DrawProjectileWithGlow(Projectile proj, Color lightColor, Color glowColor, float glowSize = 3f)
        {
            proj.DrawProjectileWithBackglow(glowColor, lightColor, glowSize);
        }
    }
}
