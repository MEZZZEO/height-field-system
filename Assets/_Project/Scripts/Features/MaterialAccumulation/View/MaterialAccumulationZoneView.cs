using UnityEngine;

namespace Features.MaterialAccumulation.View
{
    public sealed class MaterialAccumulationZoneView : MonoBehaviour
    {
        [SerializeField] private Transform _visualRoot;

        public void SetRadius(float radius)
        {
            var target = _visualRoot != null ? _visualRoot : transform;
            target.localScale = Vector3.one * radius;
        }
    }
}
