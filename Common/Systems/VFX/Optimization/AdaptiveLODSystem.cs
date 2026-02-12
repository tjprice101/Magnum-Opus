using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Common.Systems.VFX.Optimization
{
    /// <summary>
    /// Adaptive LOD system that manages update frequency for registered objects
    /// based on their distance from the screen center.
    /// Objects farther away update less frequently, saving CPU time.
    /// </summary>
    public class AdaptiveLODSystem : ModSystem
    {
        /// <summary>
        /// Interface for objects that can be managed by the adaptive LOD system.
        /// </summary>
        public interface ILODUpdatable
        {
            void Update();
            Vector2 GetPosition();
            bool NeedsUpdateWhenInvisible();
        }

        private class LODObject
        {
            public ILODUpdatable Object;
            public Vector2 Position;
            public LODManager.LODLevel CurrentLOD;
            public int UpdateCounter;
            public int UpdateFrequency;
            public bool IsActive;
        }

        private List<LODObject> objects;
        private LODManager lodManager;

        private static AdaptiveLODSystem _instance;
        public static AdaptiveLODSystem Instance => _instance;

        public int TotalObjects => objects.Count;
        public int ActiveObjects { get; private set; }

        public override void Load()
        {
            _instance = this;
            objects = new List<LODObject>();
            lodManager = LODManager.Instance;
        }

        public override void Unload()
        {
            _instance = null;
            objects?.Clear();
            objects = null;
        }

        /// <summary>
        /// Register an object to be managed by the adaptive LOD system.
        /// </summary>
        public void Register(ILODUpdatable obj)
        {
            objects.Add(new LODObject
            {
                Object = obj,
                Position = obj.GetPosition(),
                CurrentLOD = LODManager.LODLevel.High,
                UpdateCounter = 0,
                UpdateFrequency = 1,
                IsActive = true
            });
        }

        /// <summary>
        /// Unregister an object from the adaptive LOD system.
        /// </summary>
        public void Unregister(ILODUpdatable obj)
        {
            objects.RemoveAll(o => o.Object == obj);
        }

        /// <summary>
        /// Update all registered objects based on their LOD level.
        /// Objects farther from screen center update less frequently.
        /// </summary>
        public override void PostUpdateEverything()
        {
            ActiveObjects = 0;

            for (int i = objects.Count - 1; i >= 0; i--)
            {
                var lodObj = objects[i];

                if (!lodObj.IsActive)
                    continue;

                // Update position and LOD level
                lodObj.Position = lodObj.Object.GetPosition();
                lodObj.CurrentLOD = lodManager.GetLODLevel(lodObj.Position);

                // Determine update frequency based on LOD
                lodObj.UpdateFrequency = lodManager.GetUpdateFrequency(lodObj.CurrentLOD);

                // Update object based on frequency
                lodObj.UpdateCounter++;

                bool shouldUpdate = false;

                if (lodObj.CurrentLOD == LODManager.LODLevel.Culled)
                {
                    // Only update if object needs it when invisible
                    shouldUpdate = lodObj.Object.NeedsUpdateWhenInvisible();
                }
                else if (lodObj.UpdateFrequency > 0 && lodObj.UpdateCounter >= lodObj.UpdateFrequency)
                {
                    shouldUpdate = true;
                }

                if (shouldUpdate)
                {
                    lodObj.Object.Update();
                    lodObj.UpdateCounter = 0;
                    ActiveObjects++;
                }
            }
        }

        /// <summary>
        /// Get the current LOD level for a registered object.
        /// </summary>
        public LODManager.LODLevel GetObjectLOD(ILODUpdatable obj)
        {
            foreach (var lodObj in objects)
            {
                if (lodObj.Object == obj)
                    return lodObj.CurrentLOD;
            }
            return LODManager.LODLevel.High;
        }

        /// <summary>
        /// Set active state for an object (paused objects don't update).
        /// </summary>
        public void SetActive(ILODUpdatable obj, bool active)
        {
            foreach (var lodObj in objects)
            {
                if (lodObj.Object == obj)
                {
                    lodObj.IsActive = active;
                    break;
                }
            }
        }

        /// <summary>
        /// Clear all registered objects.
        /// </summary>
        public void Clear()
        {
            objects.Clear();
        }

        /// <summary>
        /// Get statistics about LOD distribution.
        /// </summary>
        public Dictionary<LODManager.LODLevel, int> GetLODStats()
        {
            var stats = new Dictionary<LODManager.LODLevel, int>
            {
                { LODManager.LODLevel.High, 0 },
                { LODManager.LODLevel.Medium, 0 },
                { LODManager.LODLevel.Low, 0 },
                { LODManager.LODLevel.VeryLow, 0 },
                { LODManager.LODLevel.Culled, 0 }
            };

            foreach (var lodObj in objects)
            {
                if (lodObj.IsActive)
                    stats[lodObj.CurrentLOD]++;
            }

            return stats;
        }
    }
}
