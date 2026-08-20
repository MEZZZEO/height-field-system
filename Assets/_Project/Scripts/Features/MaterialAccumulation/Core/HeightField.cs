using System;
using UnityEngine;

namespace Features.MaterialAccumulation.Core
{
    public sealed class HeightField
    {
        private readonly int _resolutionX;
        private readonly int _resolutionZ;
        private readonly Vector2 _surfaceSize;
        private readonly Vector2 _origin;
        private readonly float _cellSizeX;
        private readonly float _cellSizeZ;
        private readonly float[] _gridWorldX;
        private readonly float[] _gridWorldZ;
        private readonly float[] _heights;

        private int _dirtyMinX;
        private int _dirtyMaxX;
        private int _dirtyMinZ;
        private int _dirtyMaxZ;

        public int ResolutionX => _resolutionX;
        public int ResolutionZ => _resolutionZ;
        public Vector2 SurfaceSize => _surfaceSize;
        public Vector2 Origin => _origin;
        public float CellSizeX => _cellSizeX;
        public float CellSizeZ => _cellSizeZ;
        public float[] Heights => _heights;
        public bool HasDirtyRegion { get; private set; }
        public int DirtyMinX => _dirtyMinX;
        public int DirtyMaxX => _dirtyMaxX;
        public int DirtyMinZ => _dirtyMinZ;
        public int DirtyMaxZ => _dirtyMaxZ;

        public HeightField(int resolutionX, int resolutionZ, Vector2 surfaceSize)
        {
            if (resolutionX < 2 || resolutionZ < 2)
                throw new ArgumentOutOfRangeException(nameof(resolutionX), "HeightField resolution must be at least 2x2.");
            if (surfaceSize.x <= 0f || surfaceSize.y <= 0f)
                throw new ArgumentOutOfRangeException(nameof(surfaceSize), "HeightField surface size must be positive.");

            _resolutionX = resolutionX;
            _resolutionZ = resolutionZ;
            _surfaceSize = surfaceSize;
            _origin = -surfaceSize * 0.5f;
            _cellSizeX = surfaceSize.x / (resolutionX - 1);
            _cellSizeZ = surfaceSize.y / (resolutionZ - 1);
            _gridWorldX = new float[resolutionX];
            _gridWorldZ = new float[resolutionZ];
            _heights = new float[resolutionX * resolutionZ];

            for (var x = 0; x < resolutionX; x++)
                _gridWorldX[x] = _origin.x + x * _cellSizeX;
            for (var z = 0; z < resolutionZ; z++)
                _gridWorldZ[z] = _origin.y + z * _cellSizeZ;

            MarkDirtyAll();
        }

        public void AddSweep(
            Vector2 previous,
            Vector2 current,
            float previousRadius,
            float currentRadius,
            float accumulationSpeed,
            float deltaTime,
            float sweepStepFactor)
        {
            if (deltaTime <= 0f || accumulationSpeed <= 0f || sweepStepFactor <= 0f)
                return;

            var distance = Vector2.Distance(previous, current);
            var stepLength = Mathf.Min(_cellSizeX, _cellSizeZ) * sweepStepFactor;
            var steps = Mathf.Max(1, Mathf.CeilToInt(distance / stepLength));
            var subDeltaTime = deltaTime / steps;

            for (var i = 0; i < steps; i++)
            {
                var t = (i + 0.5f) / steps;
                var position = Vector2.LerpUnclamped(previous, current, t);
                var radius = Mathf.LerpUnclamped(previousRadius, currentRadius, t);
                AddStamp(position, radius, accumulationSpeed, subDeltaTime);
            }
        }
        
        public void Reset()
        {
            Array.Clear(_heights, 0, _heights.Length);
            MarkDirtyAll();
        }

        public void ClearDirtyRegion()
        {
            HasDirtyRegion = false;
        }

        private void AddStamp(Vector2 center, float radius, float accumulationSpeed, float deltaTime)
        {
            if (radius <= 0f || accumulationSpeed <= 0f || deltaTime <= 0f)
                return;

            if (!TryGetGridBounds(center, radius, out var minX, out var maxX, out var minZ, out var maxZ))
                return;

            var radiusSquared = radius * radius;
            var scale = accumulationSpeed * deltaTime;

            for (var z = minZ; z <= maxZ; z++)
            {
                var dz = _gridWorldZ[z] - center.y;

                for (var x = minX; x <= maxX; x++)
                {
                    var dx = _gridWorldX[x] - center.x;
                    var distanceSquared = dx * dx + dz * dz;

                    if (distanceSquared >= radiusSquared)
                        continue;

                    var normalizedSquared = distanceSquared / radiusSquared;
                    var profile = Mathf.Sqrt(Mathf.Max(0f, 1f - normalizedSquared));
                    var index = x + z * _resolutionX;
                    _heights[index] += scale * profile;
                    MarkDirty(x, z);
                }
            }
        }

        private bool TryGetGridBounds(
            Vector2 center,
            float radius,
            out int minX,
            out int maxX,
            out int minZ,
            out int maxZ)
        {
            minX = Mathf.FloorToInt((center.x - radius - _origin.x) / _cellSizeX);
            maxX = Mathf.CeilToInt((center.x + radius - _origin.x) / _cellSizeX);
            minZ = Mathf.FloorToInt((center.y - radius - _origin.y) / _cellSizeZ);
            maxZ = Mathf.CeilToInt((center.y + radius - _origin.y) / _cellSizeZ);

            minX = Mathf.Clamp(minX, 0, _resolutionX - 1);
            maxX = Mathf.Clamp(maxX, 0, _resolutionX - 1);
            minZ = Mathf.Clamp(minZ, 0, _resolutionZ - 1);
            maxZ = Mathf.Clamp(maxZ, 0, _resolutionZ - 1);

            return minX <= maxX && minZ <= maxZ;
        }

        private void MarkDirty(int x, int z)
        {
            if (!HasDirtyRegion)
            {
                _dirtyMinX = _dirtyMaxX = x;
                _dirtyMinZ = _dirtyMaxZ = z;
                HasDirtyRegion = true;
                return;
            }

            _dirtyMinX = Mathf.Min(_dirtyMinX, x);
            _dirtyMaxX = Mathf.Max(_dirtyMaxX, x);
            _dirtyMinZ = Mathf.Min(_dirtyMinZ, z);
            _dirtyMaxZ = Mathf.Max(_dirtyMaxZ, z);
        }

        private void MarkDirtyAll()
        {
            _dirtyMinX = 0;
            _dirtyMaxX = _resolutionX - 1;
            _dirtyMinZ = 0;
            _dirtyMaxZ = _resolutionZ - 1;
            HasDirtyRegion = true;
        }
    }
}
