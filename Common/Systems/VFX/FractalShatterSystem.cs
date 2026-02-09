using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Common.Systems.VFX
{
    /// <summary>
    /// FRACTAL SHATTER / BOIDS FLOCKING SYSTEM
    /// 
    /// Implements the "Fractal Shatter" melee concept:
    /// - Weapon teleports to cursor and "shatters" into 50 shards
    /// - Shards use Boids Flocking Algorithm to swarm target
    /// - Shards reform in player's hand after attack
    /// 
    /// Boids Algorithm:
    /// - Separation: Steer away from nearby boids
    /// - Alignment: Steer toward average heading of neighbors
    /// - Cohesion: Steer toward average position of neighbors
    /// - Target Seeking: Also steer toward the attack target
    /// 
    /// Usage:
    ///   FractalShatterSystem.CreateShatter(weaponPos, targetPos, shardCount, color);
    /// </summary>
    public class FractalShatterSystem : ModSystem
    {
        private static List<ShatterEffect> _activeEffects = new();
        private static Texture2D _shardTexture;
        
        private const int MaxActiveEffects = 5;
        
        #region Boid Configuration
        
        private struct BoidConfig
        {
            public float SeparationWeight;
            public float AlignmentWeight;
            public float CohesionWeight;
            public float TargetSeekWeight;
            public float MaxSpeed;
            public float MaxForce;
            public float PerceptionRadius;
        }
        
        private static readonly BoidConfig DefaultConfig = new BoidConfig
        {
            SeparationWeight = 1.5f,
            AlignmentWeight = 1.0f,
            CohesionWeight = 1.0f,
            TargetSeekWeight = 2.0f,
            MaxSpeed = 12f,
            MaxForce = 0.8f,
            PerceptionRadius = 50f
        };
        
        #endregion
        
        #region Shard Data
        
        private class Shard
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Rotation;
            public float AngularVelocity;
            public float Scale;
            public Color Color;
            public int ShardIndex;
            
            public void ApplyForce(Vector2 force)
            {
                Velocity += force;
            }
        }
        
        private class ShatterEffect
        {
            public List<Shard> Shards;
            public Vector2 TargetPosition;
            public Vector2 ReformPosition;
            public ShatterPhase Phase;
            public int Timer;
            public int PhaseDuration;
            public Color PrimaryColor;
            public Color SecondaryColor;
            public bool ReformToPlayer;
            public int PlayerIndex;
            
            public bool IsComplete => Phase == ShatterPhase.Complete;
        }
        
        private enum ShatterPhase
        {
            Explode,        // Initial explosion outward
            Swarm,          // Boids behavior, seeking target
            Attack,         // Converging on target
            Reform,         // Returning to player
            Complete
        }
        
        #endregion
        
        #region Initialization
        
        public override void Load()
        {
            if (Main.dedServ) return;
        }
        
        public override void Unload()
        {
            _activeEffects?.Clear();
            
            // Cache reference and null immediately (safe on any thread)
            var shard = _shardTexture;
            _shardTexture = null;
            
            // Queue texture disposal on main thread to avoid ThreadStateException
            if (shard != null)
            {
                Main.QueueMainThreadAction(() =>
                {
                    try { shard.Dispose(); } catch { }
                });
            }
        }
        
        public override void PostUpdateEverything()
        {
            for (int i = _activeEffects.Count - 1; i >= 0; i--)
            {
                UpdateEffect(_activeEffects[i]);
                
                if (_activeEffects[i].IsComplete)
                    _activeEffects.RemoveAt(i);
            }
        }
        
        #endregion
        
        #region Public API
        
        /// <summary>
        /// Creates a fractal shatter effect.
        /// </summary>
        public static void CreateShatter(
            Vector2 originPosition,
            Vector2 targetPosition,
            int shardCount = 50,
            Color? primaryColor = null,
            Color? secondaryColor = null,
            bool reformToPlayer = true,
            int playerIndex = 0)
        {
            if (_activeEffects.Count >= MaxActiveEffects)
                return;
            
            var effect = new ShatterEffect
            {
                Shards = new List<Shard>(),
                TargetPosition = targetPosition,
                ReformPosition = originPosition,
                Phase = ShatterPhase.Explode,
                Timer = 0,
                PhaseDuration = 20,
                PrimaryColor = primaryColor ?? Color.White,
                SecondaryColor = secondaryColor ?? Color.Cyan,
                ReformToPlayer = reformToPlayer,
                PlayerIndex = playerIndex
            };
            
            // Create shards with random velocities
            for (int i = 0; i < shardCount; i++)
            {
                float angle = MathHelper.TwoPi * i / shardCount + Main.rand.NextFloat(-0.3f, 0.3f);
                float speed = Main.rand.NextFloat(8f, 15f);
                
                effect.Shards.Add(new Shard
                {
                    Position = originPosition,
                    Velocity = angle.ToRotationVector2() * speed,
                    Rotation = Main.rand.NextFloat(MathHelper.TwoPi),
                    AngularVelocity = Main.rand.NextFloat(-0.3f, 0.3f),
                    Scale = Main.rand.NextFloat(0.5f, 1.2f),
                    Color = Color.Lerp(effect.PrimaryColor, effect.SecondaryColor, Main.rand.NextFloat()),
                    ShardIndex = i
                });
            }
            
            _activeEffects.Add(effect);
        }
        
        /// <summary>
        /// Creates a weapon shatter that reforms in the player's hand.
        /// </summary>
        public static void CreateWeaponShatter(
            Player player,
            Vector2 targetPosition,
            Color weaponColor,
            int shardCount = 40)
        {
            Vector2 weaponPos = player.Center + player.direction * new Vector2(30f, 0f);
            CreateShatter(weaponPos, targetPosition, shardCount, weaponColor, Color.White, true, player.whoAmI);
        }
        
        #endregion
        
        #region Update Logic
        
        private static void UpdateEffect(ShatterEffect effect)
        {
            effect.Timer++;
            
            // Update reform position if tracking player
            if (effect.ReformToPlayer && effect.PlayerIndex >= 0 && effect.PlayerIndex < Main.maxPlayers)
            {
                Player player = Main.player[effect.PlayerIndex];
                if (player.active)
                {
                    effect.ReformPosition = player.Center + player.direction * new Vector2(30f, 0f);
                }
            }
            
            // Phase transitions
            switch (effect.Phase)
            {
                case ShatterPhase.Explode:
                    UpdateExplodePhase(effect);
                    if (effect.Timer >= effect.PhaseDuration)
                    {
                        effect.Phase = ShatterPhase.Swarm;
                        effect.Timer = 0;
                        effect.PhaseDuration = 45;
                    }
                    break;
                    
                case ShatterPhase.Swarm:
                    UpdateSwarmPhase(effect);
                    if (effect.Timer >= effect.PhaseDuration)
                    {
                        effect.Phase = ShatterPhase.Attack;
                        effect.Timer = 0;
                        effect.PhaseDuration = 20;
                    }
                    break;
                    
                case ShatterPhase.Attack:
                    UpdateAttackPhase(effect);
                    if (effect.Timer >= effect.PhaseDuration)
                    {
                        // Deal damage here if needed
                        SpawnAttackImpact(effect);
                        
                        effect.Phase = ShatterPhase.Reform;
                        effect.Timer = 0;
                        effect.PhaseDuration = 30;
                    }
                    break;
                    
                case ShatterPhase.Reform:
                    UpdateReformPhase(effect);
                    if (effect.Timer >= effect.PhaseDuration)
                    {
                        effect.Phase = ShatterPhase.Complete;
                    }
                    break;
            }
            
            // Update individual shard physics
            foreach (var shard in effect.Shards)
            {
                shard.Position += shard.Velocity;
                shard.Rotation += shard.AngularVelocity;
                
                // Apply drag
                shard.Velocity *= 0.98f;
                shard.AngularVelocity *= 0.95f;
            }
        }
        
        private static void UpdateExplodePhase(ShatterEffect effect)
        {
            // Shards just fly outward, slowing down
            foreach (var shard in effect.Shards)
            {
                shard.Velocity *= 0.92f;
            }
        }
        
        private static void UpdateSwarmPhase(ShatterEffect effect)
        {
            // Apply Boids algorithm
            var config = DefaultConfig;
            
            foreach (var shard in effect.Shards)
            {
                Vector2 separation = CalculateSeparation(shard, effect.Shards, config.PerceptionRadius);
                Vector2 alignment = CalculateAlignment(shard, effect.Shards, config.PerceptionRadius);
                Vector2 cohesion = CalculateCohesion(shard, effect.Shards, config.PerceptionRadius);
                Vector2 targetSeek = SeekTarget(shard, effect.TargetPosition, config.MaxSpeed);
                
                // Combine forces with weights
                Vector2 totalForce = separation * config.SeparationWeight +
                                     alignment * config.AlignmentWeight +
                                     cohesion * config.CohesionWeight +
                                     targetSeek * config.TargetSeekWeight * 0.5f; // Reduced during swarm
                
                // Limit force
                if (totalForce.Length() > config.MaxForce)
                    totalForce = Vector2.Normalize(totalForce) * config.MaxForce;
                
                shard.ApplyForce(totalForce);
                
                // Limit speed
                if (shard.Velocity.Length() > config.MaxSpeed)
                    shard.Velocity = Vector2.Normalize(shard.Velocity) * config.MaxSpeed;
            }
        }
        
        private static void UpdateAttackPhase(ShatterEffect effect)
        {
            // All shards converge on target
            var config = DefaultConfig;
            
            foreach (var shard in effect.Shards)
            {
                Vector2 targetSeek = SeekTarget(shard, effect.TargetPosition, config.MaxSpeed * 1.5f);
                Vector2 separation = CalculateSeparation(shard, effect.Shards, config.PerceptionRadius * 0.5f);
                
                Vector2 totalForce = targetSeek * 3f + separation * 0.5f;
                
                if (totalForce.Length() > config.MaxForce * 2f)
                    totalForce = Vector2.Normalize(totalForce) * config.MaxForce * 2f;
                
                shard.ApplyForce(totalForce);
                
                if (shard.Velocity.Length() > config.MaxSpeed * 1.5f)
                    shard.Velocity = Vector2.Normalize(shard.Velocity) * config.MaxSpeed * 1.5f;
            }
        }
        
        private static void UpdateReformPhase(ShatterEffect effect)
        {
            // Shards return to reform position
            var config = DefaultConfig;
            
            foreach (var shard in effect.Shards)
            {
                Vector2 returnSeek = SeekTarget(shard, effect.ReformPosition, config.MaxSpeed);
                
                shard.ApplyForce(returnSeek * 2f);
                
                if (shard.Velocity.Length() > config.MaxSpeed)
                    shard.Velocity = Vector2.Normalize(shard.Velocity) * config.MaxSpeed;
                
                // Fade out as they reform
                float reformProgress = (float)effect.Timer / effect.PhaseDuration;
                shard.Scale = MathHelper.Lerp(shard.Scale, 0.1f, reformProgress * 0.1f);
            }
        }
        
        private static void SpawnAttackImpact(ShatterEffect effect)
        {
            // Create impact particles at target
            for (int i = 0; i < 20; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(8f, 8f);
                Dust dust = Dust.NewDustPerfect(effect.TargetPosition, Terraria.ID.DustID.MagicMirror,
                    vel, 0, effect.PrimaryColor, 1.5f);
                dust.noGravity = true;
            }
        }
        
        #endregion
        
        #region Boids Algorithm
        
        /// <summary>
        /// Separation: Steer away from nearby boids.
        /// </summary>
        private static Vector2 CalculateSeparation(Shard shard, List<Shard> allShards, float perceptionRadius)
        {
            Vector2 steering = Vector2.Zero;
            int count = 0;
            
            foreach (var other in allShards)
            {
                if (other == shard) continue;
                
                float distance = Vector2.Distance(shard.Position, other.Position);
                if (distance < perceptionRadius && distance > 0.001f)
                {
                    Vector2 diff = shard.Position - other.Position;
                    diff /= distance * distance; // Weight by inverse distance
                    steering += diff;
                    count++;
                }
            }
            
            if (count > 0)
            {
                steering /= count;
                if (steering.Length() > 0.001f)
                    steering = Vector2.Normalize(steering);
            }
            
            return steering;
        }
        
        /// <summary>
        /// Alignment: Steer toward average heading of neighbors.
        /// </summary>
        private static Vector2 CalculateAlignment(Shard shard, List<Shard> allShards, float perceptionRadius)
        {
            Vector2 avgVelocity = Vector2.Zero;
            int count = 0;
            
            foreach (var other in allShards)
            {
                if (other == shard) continue;
                
                float distance = Vector2.Distance(shard.Position, other.Position);
                if (distance < perceptionRadius)
                {
                    avgVelocity += other.Velocity;
                    count++;
                }
            }
            
            if (count > 0)
            {
                avgVelocity /= count;
                if (avgVelocity.Length() > 0.001f)
                    avgVelocity = Vector2.Normalize(avgVelocity);
            }
            
            return avgVelocity;
        }
        
        /// <summary>
        /// Cohesion: Steer toward average position of neighbors.
        /// </summary>
        private static Vector2 CalculateCohesion(Shard shard, List<Shard> allShards, float perceptionRadius)
        {
            Vector2 centerOfMass = Vector2.Zero;
            int count = 0;
            
            foreach (var other in allShards)
            {
                if (other == shard) continue;
                
                float distance = Vector2.Distance(shard.Position, other.Position);
                if (distance < perceptionRadius)
                {
                    centerOfMass += other.Position;
                    count++;
                }
            }
            
            if (count > 0)
            {
                centerOfMass /= count;
                return SeekTarget(shard, centerOfMass, 1f);
            }
            
            return Vector2.Zero;
        }
        
        /// <summary>
        /// Seek: Steer toward a target position.
        /// </summary>
        private static Vector2 SeekTarget(Shard shard, Vector2 target, float maxSpeed)
        {
            Vector2 desired = target - shard.Position;
            if (desired.Length() > 0.001f)
            {
                desired = Vector2.Normalize(desired) * maxSpeed;
                return desired - shard.Velocity;
            }
            return Vector2.Zero;
        }
        
        #endregion
        
        #region Rendering
        
        /// <summary>
        /// Renders all active shatter effects.
        /// </summary>
        public static void RenderAll(SpriteBatch spriteBatch)
        {
            if (_activeEffects.Count == 0) return;
            
            EnsureTexture();
            
            foreach (var effect in _activeEffects)
            {
                RenderEffect(spriteBatch, effect);
            }
        }
        
        private static void RenderEffect(SpriteBatch spriteBatch, ShatterEffect effect)
        {
            // Calculate overall alpha based on phase
            float alpha = 1f;
            if (effect.Phase == ShatterPhase.Reform)
            {
                alpha = 1f - (float)effect.Timer / effect.PhaseDuration;
            }
            
            try { spriteBatch.End(); } catch { }
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            foreach (var shard in effect.Shards)
            {
                // Draw bloom layer
                spriteBatch.Draw(
                    _shardTexture,
                    shard.Position - Main.screenPosition,
                    null,
                    shard.Color * 0.3f * alpha,
                    shard.Rotation,
                    new Vector2(_shardTexture.Width / 2f, _shardTexture.Height / 2f),
                    shard.Scale * 2f,
                    SpriteEffects.None,
                    0f
                );
                
                // Draw core
                spriteBatch.Draw(
                    _shardTexture,
                    shard.Position - Main.screenPosition,
                    null,
                    Color.White * 0.8f * alpha,
                    shard.Rotation,
                    new Vector2(_shardTexture.Width / 2f, _shardTexture.Height / 2f),
                    shard.Scale * 0.5f,
                    SpriteEffects.None,
                    0f
                );
            }
            
            try { spriteBatch.End(); } catch { }
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }
        
        #endregion
        
        #region Texture Generation
        
        private static void EnsureTexture()
        {
            if (_shardTexture == null || _shardTexture.IsDisposed)
            {
                _shardTexture = CreateShardTexture(16, 8);
            }
        }
        
        /// <summary>
        /// Creates a diamond/shard-shaped texture.
        /// </summary>
        private static Texture2D CreateShardTexture(int width, int height)
        {
            var texture = new Texture2D(Main.graphics.GraphicsDevice, width, height);
            var data = new Color[width * height];
            
            float centerX = width / 2f;
            float centerY = height / 2f;
            
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Diamond shape: |x - cx| / (w/2) + |y - cy| / (h/2) <= 1
                    float dx = MathF.Abs(x - centerX) / centerX;
                    float dy = MathF.Abs(y - centerY) / centerY;
                    float diamond = dx + dy;
                    
                    float alpha = MathF.Max(0f, 1f - diamond);
                    alpha = MathF.Pow(alpha, 0.7f);
                    
                    data[y * width + x] = Color.White * alpha;
                }
            }
            
            texture.SetData(data);
            return texture;
        }
        
        #endregion
    }
}
