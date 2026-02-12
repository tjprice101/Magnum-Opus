using System;
using System.Collections.Generic;

namespace MagnumOpus.Common.Systems.VFX.Core
{
    /// <summary>
    /// Generic object pool for avoiding GC allocations.
    /// Essential for particle systems and other frequently created/destroyed objects.
    /// 
    /// USAGE:
    /// var pool = new ObjectPool&lt;MyClass&gt;(100);
    /// pool.OnAcquire = obj => obj.Reset();
    /// pool.OnRelease = obj => obj.Cleanup();
    /// 
    /// var obj = pool.Get();
    /// // Use obj...
    /// pool.Return(obj);
    /// </summary>
    /// <typeparam name="T">Type to pool (must have parameterless constructor)</typeparam>
    public class ObjectPool<T> where T : class, new()
    {
        #region Fields
        
        private Stack<T> pool;
        private int maxSize;
        private int createdCount;
        
        #endregion
        
        #region Callbacks
        
        /// <summary>
        /// Called when an object is acquired from the pool.
        /// Use to initialize/reset the object.
        /// </summary>
        public Action<T> OnAcquire { get; set; }
        
        /// <summary>
        /// Called when an object is returned to the pool.
        /// Use to cleanup/reset the object.
        /// </summary>
        public Action<T> OnRelease { get; set; }
        
        #endregion
        
        #region Properties
        
        /// <summary>
        /// Number of objects currently available in the pool.
        /// </summary>
        public int AvailableCount => pool.Count;
        
        /// <summary>
        /// Total number of objects created by this pool.
        /// </summary>
        public int TotalCreated => createdCount;
        
        /// <summary>
        /// Number of objects currently in use (not in pool).
        /// </summary>
        public int InUseCount => createdCount - pool.Count;
        
        /// <summary>
        /// Maximum pool size.
        /// </summary>
        public int MaxSize => maxSize;
        
        /// <summary>
        /// Whether the pool can expand beyond initial size.
        /// </summary>
        public bool CanExpand => maxSize > pool.Count;
        
        #endregion
        
        #region Constructor
        
        /// <summary>
        /// Create a new object pool.
        /// </summary>
        /// <param name="initialSize">Initial pool capacity (pre-allocated)</param>
        /// <param name="maxSize">Maximum pool size (limits expansion)</param>
        public ObjectPool(int initialSize, int maxSize = int.MaxValue)
        {
            this.maxSize = maxSize;
            this.pool = new Stack<T>(initialSize);
            
            // Pre-allocate objects
            for (int i = 0; i < initialSize; i++)
            {
                pool.Push(new T());
                createdCount++;
            }
        }
        
        #endregion
        
        #region Core Methods
        
        /// <summary>
        /// Get an object from the pool.
        /// </summary>
        /// <returns>Object instance</returns>
        /// <exception cref="InvalidOperationException">Pool exhausted and cannot expand</exception>
        public T Get()
        {
            T obj;
            
            if (pool.Count > 0)
            {
                obj = pool.Pop();
            }
            else if (createdCount < maxSize)
            {
                // Pool exhausted but can expand
                obj = new T();
                createdCount++;
            }
            else
            {
                // Pool exhausted and at max size
                throw new InvalidOperationException($"Object pool exhausted (max: {maxSize})");
            }
            
            OnAcquire?.Invoke(obj);
            return obj;
        }
        
        /// <summary>
        /// Try to get an object from the pool.
        /// </summary>
        /// <param name="obj">Output object (null if failed)</param>
        /// <returns>True if successful</returns>
        public bool TryGet(out T obj)
        {
            try
            {
                obj = Get();
                return true;
            }
            catch (InvalidOperationException)
            {
                obj = null;
                return false;
            }
        }
        
        /// <summary>
        /// Return an object to the pool.
        /// </summary>
        /// <param name="obj">Object to return</param>
        public void Return(T obj)
        {
            if (obj == null)
                return;
            
            OnRelease?.Invoke(obj);
            
            if (pool.Count < maxSize)
            {
                pool.Push(obj);
            }
            // Else discard to prevent unlimited growth
        }
        
        /// <summary>
        /// Clear the pool (call OnRelease on all pooled objects).
        /// </summary>
        public void Clear()
        {
            while (pool.Count > 0)
            {
                T obj = pool.Pop();
                OnRelease?.Invoke(obj);
            }
        }
        
        /// <summary>
        /// Pre-warm the pool by creating objects up to a target count.
        /// </summary>
        /// <param name="targetCount">Number of objects to have ready</param>
        public void PreWarm(int targetCount)
        {
            int toCreate = Math.Min(targetCount, maxSize) - createdCount;
            
            for (int i = 0; i < toCreate; i++)
            {
                pool.Push(new T());
                createdCount++;
            }
        }
        
        #endregion
        
        #region Statistics
        
        /// <summary>
        /// Get pool statistics as a formatted string.
        /// </summary>
        public string GetStats()
        {
            return $"Pool<{typeof(T).Name}>: Available={AvailableCount}, InUse={InUseCount}, Total={TotalCreated}, Max={maxSize}";
        }
        
        #endregion
    }
    
    #region Poolable Interface
    
    /// <summary>
    /// Interface for objects that can be pooled.
    /// Provides standard lifecycle methods.
    /// </summary>
    public interface IPoolable
    {
        /// <summary>
        /// Reset the object to initial state (called on acquire).
        /// </summary>
        void OnPoolAcquire();
        
        /// <summary>
        /// Cleanup before returning to pool (called on release).
        /// </summary>
        void OnPoolRelease();
        
        /// <summary>
        /// Whether this object is currently active (in use).
        /// </summary>
        bool IsActive { get; set; }
    }
    
    #endregion
    
    #region Typed Pool Manager
    
    /// <summary>
    /// Manages multiple typed object pools.
    /// Useful for systems with many different pooled object types.
    /// </summary>
    public class PoolManager
    {
        private Dictionary<Type, object> pools = new Dictionary<Type, object>();
        
        /// <summary>
        /// Register a pool for a specific type.
        /// </summary>
        public void RegisterPool<T>(int initialSize, int maxSize = int.MaxValue) where T : class, new()
        {
            var pool = new ObjectPool<T>(initialSize, maxSize);
            pools[typeof(T)] = pool;
        }
        
        /// <summary>
        /// Get the pool for a specific type.
        /// </summary>
        public ObjectPool<T> GetPool<T>() where T : class, new()
        {
            if (pools.TryGetValue(typeof(T), out var pool))
            {
                return pool as ObjectPool<T>;
            }
            return null;
        }
        
        /// <summary>
        /// Get an object from the appropriate pool.
        /// </summary>
        public T Get<T>() where T : class, new()
        {
            var pool = GetPool<T>();
            if (pool != null)
            {
                return pool.Get();
            }
            
            // Fallback: create new if no pool exists
            return new T();
        }
        
        /// <summary>
        /// Return an object to the appropriate pool.
        /// </summary>
        public void Return<T>(T obj) where T : class, new()
        {
            var pool = GetPool<T>();
            pool?.Return(obj);
        }
        
        /// <summary>
        /// Clear all pools.
        /// </summary>
        public void ClearAll()
        {
            foreach (var pool in pools.Values)
            {
                var clearMethod = pool.GetType().GetMethod("Clear");
                clearMethod?.Invoke(pool, null);
            }
        }
        
        /// <summary>
        /// Get statistics for all pools.
        /// </summary>
        public string GetAllStats()
        {
            var sb = new System.Text.StringBuilder();
            foreach (var kvp in pools)
            {
                var statsMethod = kvp.Value.GetType().GetMethod("GetStats");
                if (statsMethod != null)
                {
                    sb.AppendLine(statsMethod.Invoke(kvp.Value, null) as string);
                }
            }
            return sb.ToString();
        }
    }
    
    #endregion
}
