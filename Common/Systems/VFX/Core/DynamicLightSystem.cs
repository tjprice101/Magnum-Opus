using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.WorldBuilding;

namespace MagnumOpus.Common.Systems.VFX.Core
{
    /// <summary>
    /// Dynamic lighting system for creating advanced light effects.
    /// Supports point lights, directional lights (beams), and spotlights.
    /// 
    /// USAGE:
    /// var lightSystem = new DynamicLightSystem();
    /// lightSystem.AddLight(position, Color.Cyan, 1.5f, 80f, 60); // Point light
    /// lightSystem.AddDirectionalLight(start, direction, 500f, Color.Orange, 1.2f, 32f);
    /// lightSystem.Update();
    /// lightSystem.ApplyLights(); // Call in PostAI or similar
    /// </summary>
    public class DynamicLightSystem
    {
        #region Light Source Definition
        
        /// <summary>
        /// Represents a single light source with all properties.
        /// </summary>
        public class LightSource
        {
            public Vector2 Position;
            public Color Color;
            public float Intensity;
            public float Radius;
            public LightType Type;
            public Vector2 Direction;  // For directional lights
            public float ConeAngle;    // For spotlights (degrees)
            public int Lifetime;       // -1 for permanent
            public int Age;
            
            // Animation
            public bool Pulse;
            public float PulseSpeed;
            public float PulseAmplitude;
            public float BaseIntensity;
            
            // Flicker effect
            public bool Flicker;
            public float FlickerIntensity;
            
            public bool IsExpired => Lifetime > 0 && Age >= Lifetime;
        }
        
        public enum LightType
        {
            Point,
            Directional,
            Spotlight
        }
        
        #endregion
        
        #region Fields
        
        private List<LightSource> activeLights;
        private const int MaxLights = 100;
        
        // Performance settings
        public bool Enabled { get; set; } = true;
        public int MaxRadiusTiles { get; set; } = 30; // Limit radius for performance
        
        #endregion
        
        #region Constructor
        
        public DynamicLightSystem()
        {
            activeLights = new List<LightSource>(MaxLights);
        }
        
        #endregion
        
        #region Add Light Methods
        
        /// <summary>
        /// Add a point light (radial falloff from source).
        /// </summary>
        /// <param name="position">World position</param>
        /// <param name="color">Light color</param>
        /// <param name="intensity">Brightness multiplier (1.0 = normal)</param>
        /// <param name="radius">Light radius in pixels</param>
        /// <param name="lifetime">Duration in frames (-1 for permanent)</param>
        public LightSource AddLight(Vector2 position, Color color, float intensity, float radius, int lifetime = -1)
        {
            if (activeLights.Count >= MaxLights)
            {
                // Remove oldest temporary light
                for (int i = 0; i < activeLights.Count; i++)
                {
                    if (activeLights[i].Lifetime > 0)
                    {
                        activeLights.RemoveAt(i);
                        break;
                    }
                }
            }
            
            var light = new LightSource
            {
                Position = position,
                Color = color,
                Intensity = intensity,
                BaseIntensity = intensity,
                Radius = radius,
                Type = LightType.Point,
                Lifetime = lifetime,
                Age = 0
            };
            
            activeLights.Add(light);
            return light;
        }
        
        /// <summary>
        /// Add a directional light (beam) along a path.
        /// </summary>
        public LightSource AddDirectionalLight(Vector2 start, Vector2 direction, float length,
                                                Color color, float intensity, float width, int lifetime = -1)
        {
            var light = new LightSource
            {
                Position = start,
                Direction = Vector2.Normalize(direction),
                Color = color,
                Intensity = intensity,
                BaseIntensity = intensity,
                Radius = length,
                ConeAngle = width,
                Type = LightType.Directional,
                Lifetime = lifetime,
                Age = 0
            };
            
            activeLights.Add(light);
            return light;
        }
        
        /// <summary>
        /// Add a spotlight (cone-shaped light).
        /// </summary>
        public LightSource AddSpotlight(Vector2 position, Vector2 direction, float range,
                                         float coneAngle, Color color, float intensity, int lifetime = -1)
        {
            var light = new LightSource
            {
                Position = position,
                Direction = Vector2.Normalize(direction),
                Color = color,
                Intensity = intensity,
                BaseIntensity = intensity,
                Radius = range,
                ConeAngle = coneAngle,
                Type = LightType.Spotlight,
                Lifetime = lifetime,
                Age = 0
            };
            
            activeLights.Add(light);
            return light;
        }
        
        /// <summary>
        /// Add a pulsing point light with animated intensity.
        /// </summary>
        public LightSource AddPulsingLight(Vector2 position, Color color, float intensity,
                                            float radius, float pulseSpeed, float pulseAmplitude, int lifetime = -1)
        {
            var light = AddLight(position, color, intensity, radius, lifetime);
            light.Pulse = true;
            light.PulseSpeed = pulseSpeed;
            light.PulseAmplitude = pulseAmplitude;
            return light;
        }
        
        /// <summary>
        /// Add a flickering light (fire/electricity effect).
        /// </summary>
        public LightSource AddFlickeringLight(Vector2 position, Color color, float intensity,
                                               float radius, float flickerIntensity, int lifetime = -1)
        {
            var light = AddLight(position, color, intensity, radius, lifetime);
            light.Flicker = true;
            light.FlickerIntensity = flickerIntensity;
            return light;
        }
        
        #endregion
        
        #region Update
        
        /// <summary>
        /// Update all light sources. Call once per frame.
        /// </summary>
        public void Update()
        {
            if (!Enabled)
                return;
            
            for (int i = activeLights.Count - 1; i >= 0; i--)
            {
                var light = activeLights[i];
                light.Age++;
                
                // Remove expired lights
                if (light.IsExpired)
                {
                    activeLights.RemoveAt(i);
                    continue;
                }
                
                // Update pulsing
                if (light.Pulse)
                {
                    float pulse = (float)Math.Sin(light.Age * light.PulseSpeed * 0.1f);
                    pulse = pulse * 0.5f + 0.5f; // 0-1 range
                    light.Intensity = light.BaseIntensity * (1f + pulse * light.PulseAmplitude);
                }
                
                // Update flickering
                if (light.Flicker)
                {
                    float flicker = Main.rand.NextFloat(1f - light.FlickerIntensity, 1f);
                    light.Intensity = light.BaseIntensity * flicker;
                }
            }
        }
        
        #endregion
        
        #region Apply Lights
        
        /// <summary>
        /// Apply all lights to Terraria's lighting system.
        /// Call this in PostAI or similar hook.
        /// </summary>
        public void ApplyLights()
        {
            if (!Enabled)
                return;
            
            foreach (var light in activeLights)
            {
                switch (light.Type)
                {
                    case LightType.Point:
                        ApplyPointLight(light);
                        break;
                        
                    case LightType.Directional:
                        ApplyDirectionalLight(light);
                        break;
                        
                    case LightType.Spotlight:
                        ApplySpotlight(light);
                        break;
                }
            }
        }
        
        private void ApplyPointLight(LightSource light)
        {
            int radiusTiles = Math.Min((int)(light.Radius / 16f), MaxRadiusTiles);
            Point center = light.Position.ToTileCoordinates();
            
            Vector3 rgb = light.Color.ToVector3() * light.Intensity;
            
            for (int y = -radiusTiles; y <= radiusTiles; y++)
            {
                for (int x = -radiusTiles; x <= radiusTiles; x++)
                {
                    int tileX = center.X + x;
                    int tileY = center.Y + y;
                    
                    if (!WorldGen.InWorld(tileX, tileY, 10))
                        continue;
                    
                    // Calculate distance falloff
                    Vector2 tileCenter = new Vector2(tileX * 16 + 8, tileY * 16 + 8);
                    float dist = Vector2.Distance(light.Position, tileCenter);
                    
                    if (dist > light.Radius)
                        continue;
                    
                    // Smooth falloff (inverse square law approximation)
                    float attenuation = 1f - (dist / light.Radius);
                    attenuation = attenuation * attenuation;
                    
                    Vector3 finalColor = rgb * attenuation;
                    
                    Lighting.AddLight(tileX, tileY, finalColor.X, finalColor.Y, finalColor.Z);
                }
            }
        }
        
        private void ApplyDirectionalLight(LightSource light)
        {
            Vector2 direction = light.Direction;
            float length = light.Radius;
            float width = light.ConeAngle;
            
            int steps = Math.Min((int)(length / 16f), MaxRadiusTiles * 2);
            Vector3 rgb = light.Color.ToVector3() * light.Intensity;
            
            for (int i = 0; i <= steps; i++)
            {
                float t = i / (float)steps;
                Vector2 pos = light.Position + direction * (t * length);
                
                // Attenuation along length
                float attenuation = 1f - t;
                
                Point tileCoords = pos.ToTileCoordinates();
                int widthTiles = (int)(width / 16f);
                
                for (int dy = -widthTiles; dy <= widthTiles; dy++)
                {
                    for (int dx = -widthTiles; dx <= widthTiles; dx++)
                    {
                        int tileX = tileCoords.X + dx;
                        int tileY = tileCoords.Y + dy;
                        
                        if (!WorldGen.InWorld(tileX, tileY, 10))
                            continue;
                        
                        // Width falloff
                        float widthDist = (float)Math.Sqrt(dx * dx + dy * dy);
                        float widthFalloff = 1f - MathHelper.Clamp(widthDist / widthTiles, 0f, 1f);
                        
                        Vector3 finalColor = rgb * attenuation * widthFalloff;
                        
                        Lighting.AddLight(tileX, tileY, finalColor.X, finalColor.Y, finalColor.Z);
                    }
                }
            }
        }
        
        private void ApplySpotlight(LightSource light)
        {
            int radiusTiles = Math.Min((int)(light.Radius / 16f), MaxRadiusTiles);
            Point center = light.Position.ToTileCoordinates();
            
            Vector3 rgb = light.Color.ToVector3() * light.Intensity;
            float coneAngleRad = MathHelper.ToRadians(light.ConeAngle);
            
            for (int y = -radiusTiles; y <= radiusTiles; y++)
            {
                for (int x = -radiusTiles; x <= radiusTiles; x++)
                {
                    int tileX = center.X + x;
                    int tileY = center.Y + y;
                    
                    if (!WorldGen.InWorld(tileX, tileY, 10))
                        continue;
                    
                    Vector2 tileCenter = new Vector2(tileX * 16 + 8, tileY * 16 + 8);
                    Vector2 toTile = tileCenter - light.Position;
                    float dist = toTile.Length();
                    
                    if (dist > light.Radius || dist < 0.001f)
                        continue;
                    
                    // Check if within cone angle
                    toTile /= dist; // Normalize
                    float dot = Vector2.Dot(toTile, light.Direction);
                    float angle = (float)Math.Acos(MathHelper.Clamp(dot, -1f, 1f));
                    
                    if (angle > coneAngleRad * 0.5f)
                        continue;
                    
                    // Distance attenuation
                    float distAttenuation = 1f - (dist / light.Radius);
                    distAttenuation = distAttenuation * distAttenuation;
                    
                    // Cone angle attenuation (brighter in center)
                    float angleAttenuation = 1f - (angle / (coneAngleRad * 0.5f));
                    
                    Vector3 finalColor = rgb * distAttenuation * angleAttenuation;
                    
                    Lighting.AddLight(tileX, tileY, finalColor.X, finalColor.Y, finalColor.Z);
                }
            }
        }
        
        #endregion
        
        #region Utility Methods
        
        /// <summary>
        /// Clear all lights.
        /// </summary>
        public void Clear()
        {
            activeLights.Clear();
        }
        
        /// <summary>
        /// Remove a specific light.
        /// </summary>
        public bool RemoveLight(LightSource light)
        {
            return activeLights.Remove(light);
        }
        
        /// <summary>
        /// Get active light count.
        /// </summary>
        public int ActiveLightCount => activeLights.Count;
        
        #endregion
    }
    
    #region Light Cookie System
    
    /// <summary>
    /// Applies textured light patterns (cookies) for advanced effects.
    /// Use grayscale textures where white = full light, black = no light.
    /// </summary>
    public static class LightCookie
    {
        /// <summary>
        /// Apply a textured light pattern at the specified position.
        /// </summary>
        /// <param name="position">World position (center)</param>
        /// <param name="cookieData">Grayscale color data from texture</param>
        /// <param name="width">Texture width</param>
        /// <param name="height">Texture height</param>
        /// <param name="color">Light color</param>
        /// <param name="intensity">Light intensity</param>
        /// <param name="scale">Scale multiplier</param>
        public static void ApplyTexturedLight(Vector2 position, Color[] cookieData, int width, int height,
                                               Color color, float intensity, float scale)
        {
            if (cookieData == null || cookieData.Length == 0)
                return;
            
            Point center = position.ToTileCoordinates();
            Vector3 rgb = color.ToVector3() * intensity;
            
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Sample cookie texture
                    float cookieValue = cookieData[y * width + x].R / 255f;
                    
                    if (cookieValue < 0.01f)
                        continue;
                    
                    // Convert to world position
                    int tileX = center.X + (int)((x - width * 0.5f) * scale / 16f);
                    int tileY = center.Y + (int)((y - height * 0.5f) * scale / 16f);
                    
                    if (!WorldGen.InWorld(tileX, tileY, 10))
                        continue;
                    
                    Vector3 finalColor = rgb * cookieValue;
                    
                    Lighting.AddLight(tileX, tileY, finalColor.X, finalColor.Y, finalColor.Z);
                }
            }
        }
    }
    
    #endregion
}
