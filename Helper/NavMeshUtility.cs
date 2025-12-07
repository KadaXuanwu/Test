using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace KadaXuanwu.Utils.Runtime.Helper {
    /// <summary>
    /// Static utility class for NavMesh operations including random point generation, pathfinding validation, and bounds calculations.
    /// </summary>
    public static class NavMeshUtility {
        /// <summary>
        /// Finds the closest valid position on the NavMesh to the given position.
        /// </summary>
        /// <param name="position">The world position to find the nearest NavMesh position for.</param>
        /// <param name="maxDistance">Maximum search distance. Use float.MaxValue for unlimited search.</param>
        /// <returns>The closest position on the NavMesh.</returns>
        public static Vector3 GetClosestPositionOnNavMesh(Vector3 position, float maxDistance = float.MaxValue) {
            NavMesh.SamplePosition(position, out NavMeshHit hit, maxDistance, NavMesh.AllAreas);
            return hit.position;
        }

        /// <summary>
        /// Gets a random point within one of the specified areas, weighted by volume.
        /// Larger areas have higher probability of being selected.
        /// </summary>
        /// <param name="areas">Array of transforms representing spawn areas (uses their scale as bounds).</param>
        /// <returns>A random point within one of the areas, or Vector3.zero if areas is null or empty.</returns>
        public static Vector3 GetRandomPoint(Transform[] areas) {
            if (areas == null || areas.Length == 0) {
                return Vector3.zero;
            }

            int chosenIndex = GetRandomListIndexDependingOnVolume(areas);

            if (chosenIndex >= areas.Length) {
                return Vector3.zero;
            }

            Vector3 center = areas[chosenIndex].position;
            float halfExtentX = areas[chosenIndex].localScale.x * 0.5f;
            float halfExtentY = areas[chosenIndex].localScale.y * 0.5f;
            float halfExtentZ = areas[chosenIndex].localScale.z * 0.5f;

            float randomX = Random.Range(center.x - halfExtentX, center.x + halfExtentX);
            float randomY = Random.Range(center.y - halfExtentY, center.y + halfExtentY);
            float randomZ = Random.Range(center.z - halfExtentZ, center.z + halfExtentZ);

            return new Vector3(randomX, randomY, randomZ);
        }

        /// <summary>
        /// Checks if the NavMesh agent can reach the target position via pathfinding.
        /// </summary>
        /// <param name="agent">The agent to use for pathfinding.</param>
        /// <param name="targetPosition">The position to check reachability for.</param>
        /// <returns>True if a complete path exists to the target position.</returns>
        public static bool IsReachable(NavMeshAgent agent, Vector3 targetPosition) {
            if (agent == null) {
                return false;
            }

            NavMeshPath path = new NavMeshPath();
            if (agent.CalculatePath(targetPosition, path)) {
                return path.status == NavMeshPathStatus.PathComplete;
            }

            return false;
        }

        /// <summary>
        /// Warps an agent to the nearest valid NavMesh position.
        /// </summary>
        /// <param name="agent">The agent to warp.</param>
        /// <param name="maxSearchDistance">Maximum distance to search for NavMesh.</param>
        /// <returns>True if successfully warped to a valid position.</returns>
        public static bool EnsureAgentOnNavMesh(NavMeshAgent agent, float maxSearchDistance = 10f) {
            if (agent == null) {
                return false;
            }

            if (NavMesh.SamplePosition(agent.transform.position, out NavMeshHit hit, maxSearchDistance, NavMesh.AllAreas)) {
                agent.Warp(hit.position);
                return true;
            }

            Debug.LogError($"NavMeshAgent {agent.name} is not on a valid NavMesh position.");
            return false;
        }

        /// <summary>
        /// Calculates the combined bounds from a list of custom bounds, or returns the entire NavMesh bounds if none provided.
        /// </summary>
        /// <param name="customBounds">List of bounds to combine. If null or empty, returns NavMesh bounds.</param>
        /// <returns>A single Bounds encapsulating all custom bounds, or the entire NavMesh bounds.</returns>
        public static Bounds CalculateEffectiveBounds(List<Bounds> customBounds) {
            if (customBounds == null || customBounds.Count == 0) {
                return CalculateNavMeshBounds();
            }

            Bounds effectiveBounds = customBounds[0];

            for (int i = 1; i < customBounds.Count; i++) {
                effectiveBounds.Encapsulate(customBounds[i]);
            }

            return effectiveBounds;
        }

        /// <summary>
        /// Checks if a point is contained within any of the specified custom bounds.
        /// </summary>
        /// <param name="point">The point to check.</param>
        /// <param name="customBounds">List of bounds to check against. If null or empty, returns true.</param>
        /// <returns>True if point is within at least one of the bounds, or if no bounds specified.</returns>
        public static bool IsPointWithinBounds(Vector3 point, List<Bounds> customBounds) {
            if (customBounds == null || customBounds.Count == 0) {
                return true;
            }

            foreach (Bounds bound in customBounds) {
                if (bound.Contains(point)) {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Selects a random index from an array of transforms, weighted by their volume (lossyScale).
        /// Larger transforms have higher probability of being selected.
        /// </summary>
        /// <param name="areas">Array of transforms to select from.</param>
        /// <returns>The selected index, or 0 if areas is null or empty.</returns>
        private static int GetRandomListIndexDependingOnVolume(Transform[] areas) {
            List<float> volumes = new List<float>();
            foreach (Transform area in areas) {
                Vector3 lossyScale = area.lossyScale;
                volumes.Add(lossyScale.x * lossyScale.y * lossyScale.z);
            }

            float totalSum = 0;
            foreach (float volume in volumes) {
                totalSum += volume;
            }

            int selectedIndex = 0;
            float cumulativeSum = 0;
            float randomValue = Random.Range(0f, totalSum);
            for (int i = 0; i < volumes.Count; i++) {
                cumulativeSum += volumes[i];

                if (randomValue <= cumulativeSum) {
                    selectedIndex = i;
                    break;
                }
            }

            return selectedIndex;
        }

        /// <summary>
        /// Calculates the axis-aligned bounding box that encompasses the entire NavMesh.
        /// </summary>
        /// <returns>Bounds representing the entire NavMesh, or a zero-size bounds at origin if NavMesh has no vertices.</returns>
        private static Bounds CalculateNavMeshBounds() {
            NavMeshTriangulation triangulation = NavMesh.CalculateTriangulation();
            Vector3[] vertices = triangulation.vertices;

            if (vertices.Length == 0) {
                Debug.LogError("NavMesh has no vertices.");
                return new Bounds(Vector3.zero, Vector3.zero);
            }

            Vector3 min = vertices[0];
            Vector3 max = vertices[0];

            foreach (Vector3 vertex in vertices) {
                min = Vector3.Min(min, vertex);
                max = Vector3.Max(max, vertex);
            }

            return new Bounds((min + max) / 2, max - min);
        }
    }
}
