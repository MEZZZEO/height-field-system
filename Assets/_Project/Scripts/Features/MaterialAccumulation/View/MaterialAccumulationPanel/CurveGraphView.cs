using UnityEngine;
using UnityEngine.UI;

namespace Features.MaterialAccumulation.View
{
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class CurveGraphView : Graphic
    {
        [SerializeField, Min(2)] private int _sampleCount = 64;
        [SerializeField, Min(0.001f)] private float _lineWidth = 2f;

        private AnimationCurve _curve;

        public void SetCurve(AnimationCurve curve)
        {
            _curve = curve;
            
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vertexHelper)
        {
            vertexHelper.Clear();

            if (_curve == null)
                return;

            var rect = rectTransform.rect;
            var previous = EvaluatePoint(0f, rect);
            var samples = Mathf.Max(2, _sampleCount);

            for (var i = 1; i < samples; i++)
            {
                var t = (float)i / (samples - 1);
                var current = EvaluatePoint(t, rect);
                AddSegment(vertexHelper, previous, current);
                previous = current;
            }
        }

        private Vector2 EvaluatePoint(float time, Rect rect)
        {
            var value = Mathf.Clamp01(_curve.Evaluate(time));
            return new Vector2(
                rect.xMin + rect.width * time,
                rect.yMin + rect.height * value);
        }

        private void AddSegment(VertexHelper vertexHelper, Vector2 start, Vector2 end)
        {
            var direction = end - start;
            if (direction.sqrMagnitude <= Mathf.Epsilon)
                return;

            direction.Normalize();
            var normal = new Vector2(-direction.y, direction.x) * (_lineWidth * 0.5f);
            var vertex = UIVertex.simpleVert;
            vertex.color = color;

            vertex.position = start - normal;
            vertexHelper.AddVert(vertex);
            vertex.position = start + normal;
            vertexHelper.AddVert(vertex);
            vertex.position = end + normal;
            vertexHelper.AddVert(vertex);
            vertex.position = end - normal;
            vertexHelper.AddVert(vertex);

            var index = vertexHelper.currentVertCount - 4;
            vertexHelper.AddTriangle(index, index + 1, index + 2);
            vertexHelper.AddTriangle(index, index + 2, index + 3);
        }
    }
}