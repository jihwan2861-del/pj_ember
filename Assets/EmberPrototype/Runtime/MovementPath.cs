using System;
using System.Collections.Generic;
using UnityEngine;

namespace EmberPrototype
{
    [AddComponentMenu("Ember Prototype/Movement Path")]
    [DisallowMultipleComponent]
    public sealed class MovementPath : MonoBehaviour
    {
        // Existing serialized enum values must remain stable.
        public enum PathShape { Linear = 0, Smooth = 1, Circle = 2 }

        [SerializeField] private PathShape shape = PathShape.Linear;
        [SerializeField] private bool autoCollectChildPoints = true;
        [SerializeField] private Transform[] points = Array.Empty<Transform>();
        [SerializeField, Range(4, 32)] private int smoothSamplesPerSegment = 12;
        [SerializeField] private Color pathColor = Color.black;
        [SerializeField] private bool closed;
        [SerializeField] private Vector2 circleCenter;
        [SerializeField, Min(0.01f)] private float circleRadius = 2f;
        [SerializeField] private float circleStartAngle;
        [SerializeField] private bool circleClockwise;

        private Vector2[] bakedPositions = Array.Empty<Vector2>();
        private float[] bakedDistances = Array.Empty<float>();
        private Vector2 bakedCircleCenter;
        private float bakedCircleRadius;
        private float bakedCircleStartAngle;
        private bool bakedCircleClockwise;
        private bool bakedAsCircle;

        public int PointCount => points != null ? points.Length : 0;
        public float TotalLength { get; private set; }
        public bool IsValid => TotalLength > 0.001f && (bakedAsCircle || bakedPositions.Length >= 2);
        public PathShape Shape => shape;
        public bool IsClosed => shape == PathShape.Circle || closed;
        public Vector2 CircleCenterWorld => transform.TransformPoint(circleCenter);
        public float CircleRadius => circleRadius;
        public float CircleStartAngle => circleStartAngle;
        public bool CircleClockwise => circleClockwise;
        public Vector2 StartPosition => shape == PathShape.Circle
            ? CircleCenterWorld + CircleOffset(circleStartAngle, circleRadius)
            : PointCount > 0 && points[0] != null ? (Vector2)points[0].position : (Vector2)transform.position;

        private void Awake() => Rebuild();

        private void OnValidate()
        {
            smoothSamplesPerSegment = Mathf.Clamp(smoothSamplesPerSegment, 4, 32);
            circleRadius = Mathf.Max(0.01f, circleRadius);
            if (autoCollectChildPoints) CollectChildPoints();
            Rebuild();
        }

        [ContextMenu("Collect Child Points")]
        public void CollectChildPoints()
        {
            List<Transform> children = new List<Transform>(transform.childCount);
            for (int i = 0; i < transform.childCount; i++) children.Add(transform.GetChild(i));
            points = children.ToArray();
        }

        public void Rebuild()
        {
            if (autoCollectChildPoints) CollectChildPoints();
            bakedAsCircle = shape == PathShape.Circle;
            if (bakedAsCircle)
            {
                bakedCircleCenter = CircleCenterWorld;
                bakedCircleRadius = Mathf.Max(0.01f, circleRadius);
                bakedCircleStartAngle = circleStartAngle;
                bakedCircleClockwise = circleClockwise;
                bakedPositions = Array.Empty<Vector2>();
                bakedDistances = Array.Empty<float>();
                TotalLength = 2f * Mathf.PI * bakedCircleRadius;
                return;
            }
            // Missing waypoints invalidate the path rather than silently using world zero.
            bool validPoints = PointCount >= 2;
            for (int i = 0; i < PointCount && validPoints; i++) validPoints = points[i] != null;
            if (!validPoints)
            {
                bakedPositions = Array.Empty<Vector2>();
                bakedDistances = Array.Empty<float>();
                TotalLength = 0f;
                return;
            }
            int segmentCount = closed ? PointCount : PointCount - 1;
            int samples = shape == PathShape.Linear ? 1 : Mathf.Clamp(smoothSamplesPerSegment, 4, 32);
            bakedPositions = new Vector2[segmentCount * samples + 1];
            bakedDistances = new float[bakedPositions.Length];
            bakedPositions[0] = points[0].position;
            int index = 1;
            for (int segment = 0; segment < segmentCount; segment++)
                for (int sample = 1; sample <= samples; sample++)
                    bakedPositions[index++] = shape == PathShape.Linear
                        ? (Vector2)points[(segment + 1) % PointCount].position
                        : EvaluateSmoothSegment(segment, sample / (float)samples);
            TotalLength = 0f;
            for (int i = 1; i < bakedPositions.Length; i++)
            {
                TotalLength += Vector2.Distance(bakedPositions[i - 1], bakedPositions[i]);
                bakedDistances[i] = TotalLength;
            }
        }

        public Vector2 GetPositionAtDistance(float distance)
        {
            if (bakedAsCircle)
            {
                float fraction = TotalLength > 0f ? Mathf.Clamp01(distance / TotalLength) : 0f;
                // Exact circumference parameterization keeps circle speed and wrap tangents continuous.
                float angle = bakedCircleStartAngle + (bakedCircleClockwise ? -360f : 360f) * fraction;
                return bakedCircleCenter + CircleOffset(angle, bakedCircleRadius);
            }
            if (bakedPositions.Length == 0) return transform.position;
            if (distance <= 0f) return bakedPositions[0];
            if (distance >= TotalLength) return bakedPositions[bakedPositions.Length - 1];
            int upper = Array.BinarySearch(bakedDistances, distance);
            if (upper >= 0) return bakedPositions[upper];
            upper = ~upper;
            int lower = upper - 1;
            float segmentLength = bakedDistances[upper] - bakedDistances[lower];
            float t = segmentLength > 0f ? (distance - bakedDistances[lower]) / segmentLength : 0f;
            return Vector2.LerpUnclamped(bakedPositions[lower], bakedPositions[upper], t);
        }

        private Vector2 PointAt(int index)
        {
            index = closed ? (index % PointCount + PointCount) % PointCount : Mathf.Clamp(index, 0, PointCount - 1);
            return points[index].position;
        }

        private Vector2 EvaluateSmoothSegment(int segment, float t)
        {
            Vector2 p0 = PointAt(segment - 1), p1 = PointAt(segment);
            Vector2 p2 = PointAt(segment + 1), p3 = PointAt(segment + 2);
            float t2 = t * t, t3 = t2 * t;
            return 0.5f * (2f * p1 + (-p0 + p2) * t
                + (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2
                + (-p0 + 3f * p1 - 3f * p2 + p3) * t3);
        }

        private static Vector2 CircleOffset(float degrees, float radius)
        {
            float angle = degrees * Mathf.Deg2Rad;
            return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = pathColor;
            if (shape == PathShape.Circle)
            {
                Vector2 center = CircleCenterWorld;
                Vector2 previous = center + CircleOffset(circleStartAngle, circleRadius);
                for (int sample = 1; sample <= 96; sample++)
                {
                    float angle = circleStartAngle + (circleClockwise ? -360f : 360f) * sample / 96f;
                    Vector2 current = center + CircleOffset(angle, circleRadius);
                    Gizmos.DrawLine(previous, current);
                    previous = current;
                }
                return;
            }
            if (PointCount < 2) return;
            for (int i = 0; i < PointCount; i++) if (points[i] == null) return;
            int segments = closed ? PointCount : PointCount - 1;
            Vector2 last = points[0].position;
            for (int i = 0; i < PointCount; i++) Gizmos.DrawWireSphere(points[i].position, 0.12f);
            for (int segment = 0; segment < segments; segment++)
            {
                int samples = shape == PathShape.Linear ? 1 : Mathf.Clamp(smoothSamplesPerSegment, 4, 32);
                for (int sample = 1; sample <= samples; sample++)
                {
                    Vector2 next = shape == PathShape.Linear ? PointAt(segment + 1)
                        : EvaluateSmoothSegment(segment, sample / (float)samples);
                    Gizmos.DrawLine(last, next);
                    last = next;
                }
            }
        }
    }
}
