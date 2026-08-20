using Features.MaterialAccumulation.Config;
using Features.MaterialAccumulation.Core;
using UnityEngine;
using Utilities.Lifetimes;
using Zenject;

namespace Features.MaterialAccumulation.View
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public sealed class HeightFieldMeshView : MonoBehaviour, ILifetimeInitializable
    {
        [SerializeField] private MeshFilter _meshFilter;

        private MaterialAccumulationSettings _settings;
        private HeightField _heightField;
        private Mesh _mesh;
        private Vector3[] _vertices;
        private Vector2[] _uv;
        private int[] _triangles;

        public HeightField HeightField => _heightField;

        [Inject]
        private void Construct(MaterialAccumulationSettings settings)
        {
            _settings = settings;
            _heightField = new HeightField(settings.ResolutionX, settings.ResolutionZ, settings.SurfaceSize);
        }

        public void Initialize(Lifetime lifetime)
        {
            EnsureMesh();
            Apply();
            lifetime.OnTermination(CleanupMesh);
        }

        public void Apply()
        {
            if (_heightField == null || !_heightField.HasDirtyRegion)
                return;

            EnsureMesh();

            for (var z = _heightField.DirtyMinZ; z <= _heightField.DirtyMaxZ; z++)
            {
                for (var x = _heightField.DirtyMinX; x <= _heightField.DirtyMaxX; x++)
                {
                    var index = x + z * _heightField.ResolutionX;
                    _vertices[index].y = _heightField.Heights[index];
                }
            }

            _mesh.SetVertices(_vertices);
            _mesh.RecalculateNormals();
            _mesh.RecalculateBounds();
            _heightField.ClearDirtyRegion();
        }

        private void EnsureMesh()
        {
            if (_mesh != null)
                return;

            if (_meshFilter == null)
                _meshFilter = GetComponent<MeshFilter>();

            var vertexCount = _settings.ResolutionX * _settings.ResolutionZ;
            _vertices = new Vector3[vertexCount];
            _uv = new Vector2[vertexCount];
            _triangles = new int[(_settings.ResolutionX - 1) * (_settings.ResolutionZ - 1) * 6];

            var origin = -_settings.SurfaceSize * 0.5f;
            var cellSizeX = _settings.SurfaceSize.x / (_settings.ResolutionX - 1);
            var cellSizeZ = _settings.SurfaceSize.y / (_settings.ResolutionZ - 1);

            for (var z = 0; z < _settings.ResolutionZ; z++)
            {
                for (var x = 0; x < _settings.ResolutionX; x++)
                {
                    var index = x + z * _settings.ResolutionX;
                    _vertices[index] = new Vector3(origin.x + x * cellSizeX, 0f, origin.y + z * cellSizeZ);
                    _uv[index] = new Vector2(
                        (float)x / (_settings.ResolutionX - 1),
                        (float)z / (_settings.ResolutionZ - 1));
                }
            }

            var triangleIndex = 0;
            for (var z = 0; z < _settings.ResolutionZ - 1; z++)
            {
                for (var x = 0; x < _settings.ResolutionX - 1; x++)
                {
                    var bottomLeft = x + z * _settings.ResolutionX;
                    var bottomRight = bottomLeft + 1;
                    var topLeft = bottomLeft + _settings.ResolutionX;
                    var topRight = topLeft + 1;

                    _triangles[triangleIndex++] = bottomLeft;
                    _triangles[triangleIndex++] = topLeft;
                    _triangles[triangleIndex++] = topRight;
                    _triangles[triangleIndex++] = bottomLeft;
                    _triangles[triangleIndex++] = topRight;
                    _triangles[triangleIndex++] = bottomRight;
                }
            }

            _mesh = new Mesh { name = "MaterialAccumulationSurface" };
            _mesh.MarkDynamic();
            _mesh.vertices = _vertices;
            _mesh.uv = _uv;
            _mesh.triangles = _triangles;
            _mesh.RecalculateNormals();
            _mesh.RecalculateBounds();
            _meshFilter.sharedMesh = _mesh;
        }

        private void CleanupMesh()
        {
            if (_mesh == null)
                return;

            if (Application.isPlaying)
                Destroy(_mesh);
            else
                DestroyImmediate(_mesh);

            _mesh = null;
        }
    }
}
