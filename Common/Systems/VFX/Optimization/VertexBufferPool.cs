using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;

namespace MagnumOpus.Common.Systems.VFX.Optimization
{
    /// <summary>
    /// Pool of reusable vertex buffers to minimize GPU memory allocation.
    /// Renting and returning buffers avoids GC pressure and allocation latency.
    /// </summary>
    /// <typeparam name="T">Vertex type (must implement IVertexType)</typeparam>
    public class VertexBufferPool<T> where T : struct, IVertexType
    {
        private class PooledBuffer
        {
            public DynamicVertexBuffer Buffer;
            public bool InUse;
            public int Capacity;
            public int LastUsedFrame;

            public PooledBuffer(GraphicsDevice device, int capacity)
            {
                Capacity = capacity;
                Buffer = new DynamicVertexBuffer(
                    device,
                    typeof(T),
                    capacity,
                    BufferUsage.WriteOnly
                );
                InUse = false;
                LastUsedFrame = 0;
            }

            public void Dispose()
            {
                Buffer?.Dispose();
                Buffer = null;
            }
        }

        private List<PooledBuffer> buffers;
        private GraphicsDevice device;
        private int frameCounter;

        public int TotalBuffers => buffers.Count;
        public int ActiveBuffers => buffers.Count(b => b.InUse);
        public int IdleBuffers => buffers.Count(b => !b.InUse);

        public VertexBufferPool(GraphicsDevice device)
        {
            this.device = device;
            buffers = new List<PooledBuffer>();
            frameCounter = 0;
        }

        /// <summary>
        /// Rent a vertex buffer with at least the required capacity.
        /// Returns an existing idle buffer if available, or creates a new one.
        /// </summary>
        public DynamicVertexBuffer Rent(int requiredCapacity)
        {
            // Try to find existing buffer with enough capacity
            foreach (var pooled in buffers)
            {
                if (!pooled.InUse && pooled.Capacity >= requiredCapacity)
                {
                    pooled.InUse = true;
                    pooled.LastUsedFrame = frameCounter;
                    return pooled.Buffer;
                }
            }

            // No suitable buffer found - create new one
            // Round up capacity to reduce fragmentation
            int actualCapacity = NextPowerOfTwo(requiredCapacity);
            var newPooled = new PooledBuffer(device, actualCapacity);
            newPooled.InUse = true;
            newPooled.LastUsedFrame = frameCounter;
            buffers.Add(newPooled);

            return newPooled.Buffer;
        }

        /// <summary>
        /// Return a buffer to the pool for reuse.
        /// </summary>
        public void Return(DynamicVertexBuffer buffer)
        {
            foreach (var pooled in buffers)
            {
                if (pooled.Buffer == buffer)
                {
                    pooled.InUse = false;
                    return;
                }
            }
        }

        /// <summary>
        /// Reset all buffers to idle state. Call at end of frame.
        /// </summary>
        public void ResetAll()
        {
            frameCounter++;
            foreach (var pooled in buffers)
            {
                pooled.InUse = false;
            }
        }

        /// <summary>
        /// Dispose unused buffers that haven't been used recently.
        /// Call periodically to prevent unlimited pool growth.
        /// </summary>
        /// <param name="keepCount">Minimum number of idle buffers to keep</param>
        /// <param name="maxIdleFrames">Dispose buffers idle for this many frames</param>
        public void TrimExcess(int keepCount = 10, int maxIdleFrames = 300)
        {
            var unusedBuffers = buffers
                .Where(b => !b.InUse && (frameCounter - b.LastUsedFrame) > maxIdleFrames)
                .OrderBy(b => b.Capacity)
                .Skip(keepCount)
                .ToList();

            foreach (var pooled in unusedBuffers)
            {
                pooled.Dispose();
                buffers.Remove(pooled);
            }
        }

        /// <summary>
        /// Dispose all buffers and clear the pool.
        /// </summary>
        public void Dispose()
        {
            foreach (var pooled in buffers)
            {
                pooled.Dispose();
            }
            buffers.Clear();
        }

        /// <summary>
        /// Get total GPU memory used by this pool (approximate).
        /// </summary>
        public long GetApproximateMemoryUsage()
        {
            long total = 0;
            int vertexSize = System.Runtime.InteropServices.Marshal.SizeOf(typeof(T));
            foreach (var pooled in buffers)
            {
                total += pooled.Capacity * vertexSize;
            }
            return total;
        }

        private int NextPowerOfTwo(int value)
        {
            int power = 64; // Minimum buffer size
            while (power < value)
                power *= 2;
            return power;
        }
    }

    /// <summary>
    /// Ring buffer for streaming vertex data with minimal GPU stalls.
    /// Writes data in a circular fashion, reusing the same buffer.
    /// </summary>
    /// <typeparam name="T">Vertex type</typeparam>
    public class RingVertexBuffer<T> where T : struct, IVertexType
    {
        private DynamicVertexBuffer buffer;
        private int capacity;
        private int writePosition;
        private int vertexSize;

        public int Capacity => capacity;
        public int WritePosition => writePosition;
        public DynamicVertexBuffer Buffer => buffer;

        public RingVertexBuffer(GraphicsDevice device, int capacity)
        {
            this.capacity = capacity;
            this.writePosition = 0;
            this.vertexSize = System.Runtime.InteropServices.Marshal.SizeOf(typeof(T));

            buffer = new DynamicVertexBuffer(
                device,
                typeof(T),
                capacity,
                BufferUsage.WriteOnly
            );
        }

        /// <summary>
        /// Write vertex data to the ring buffer.
        /// Returns the starting position of the written data.
        /// Automatically wraps around when reaching the end.
        /// </summary>
        public int Write(T[] data)
        {
            if (data.Length > capacity)
                throw new System.ArgumentException("Data larger than buffer capacity");

            int startPosition = writePosition;

            // Check if we need to wrap around
            if (writePosition + data.Length > capacity)
            {
                // Discard and restart from beginning
                buffer.SetData(
                    0,
                    data,
                    0,
                    data.Length,
                    vertexSize,
                    SetDataOptions.Discard
                );
                writePosition = data.Length;
                startPosition = 0;
            }
            else
            {
                // Write without overwriting existing data
                buffer.SetData(
                    writePosition * vertexSize,
                    data,
                    0,
                    data.Length,
                    vertexSize,
                    SetDataOptions.NoOverwrite
                );
                writePosition += data.Length;
            }

            return startPosition;
        }

        /// <summary>
        /// Reset write position to beginning.
        /// Call at start of frame if buffer should be reused completely.
        /// </summary>
        public void Reset()
        {
            writePosition = 0;
        }

        /// <summary>
        /// Get remaining capacity before wrap-around.
        /// </summary>
        public int GetRemainingCapacity()
        {
            return capacity - writePosition;
        }

        /// <summary>
        /// Dispose the buffer.
        /// </summary>
        public void Dispose()
        {
            buffer?.Dispose();
            buffer = null;
        }
    }
}
