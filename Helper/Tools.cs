using System;
using System.Collections.Generic;
using UnityEngine;

namespace KadaXuanwu.Utils.Runtime.Helper {
    /// <summary>
    /// Collection of general-purpose utility methods for common operations in Unity.
    /// Includes color conversion, GameObject manipulation, math utilities, and type checking.
    /// </summary>
    public static class Tools {
        /// <summary>
        /// Converts RGB values (0-255 range) to Unity Color (0-1 range).
        /// </summary>
        /// <param name="r">Red component (0-255).</param>
        /// <param name="g">Green component (0-255).</param>
        /// <param name="b">Blue component (0-255).</param>
        /// <param name="a">Alpha component (0-1).</param>
        /// <returns>A Unity Color with normalized values.</returns>
        public static Color ConvertRGBToColor(float r, float g, float b, float a) {
            return new Color(r / 255f, g / 255f, b / 255f, a);
        }

        /// <summary>
        /// Converts a hexadecimal color string to Unity Color.
        /// </summary>
        /// <param name="hexValue">6-character hex string (e.g., "FF0000" for red). Without '#' prefix.</param>
        /// <param name="a">Alpha component (0-1).</param>
        /// <returns>A Unity Color, or magenta (1, 0, 1) if hex string is invalid.</returns>
        public static Color ConvertRGBToColor(string hexValue, float a) {
            if (hexValue == null) {
                return new Color(1f, 0f, 1f);
            }

            if (hexValue.Length != 6) {
                return new Color(1f, 0f, 1f);
            }

            if (!System.Text.RegularExpressions.Regex.IsMatch(hexValue, "^[0-9a-fA-F]+$")) {
                return new Color(1f, 0f, 1f);
            }

            float r = Convert.ToInt32(hexValue.Substring(0, 2), 16);
            float g = Convert.ToInt32(hexValue.Substring(2, 2), 16);
            float b = Convert.ToInt32(hexValue.Substring(4, 2), 16);

            return new Color(r / 255f, g / 255f, b / 255f, a);
        }

        /// <summary>
        /// Finds the closest GameObject to the reference GameObject from an array of candidates.
        /// </summary>
        /// <param name="referenceGameObject">The GameObject to measure distances from.</param>
        /// <param name="gameObjects">Array of candidate GameObjects to check.</param>
        /// <returns>The closest GameObject, or null if array is null or all objects are null/same as reference.</returns>
        public static GameObject GetClosestGameObject(GameObject referenceGameObject, GameObject[] gameObjects) {
            if (gameObjects == null) {
                return null;
            }

            GameObject closestObject = null;
            float closestSqrDistance = Mathf.Infinity;

            Vector3 referencePosition = referenceGameObject.transform.position;

            foreach (GameObject gameObject in gameObjects) {
                if (gameObject != null && !ReferenceEquals(gameObject, referenceGameObject)) {
                    Vector3 offset = gameObject.transform.position - referencePosition;
                    float sqrDistance = offset.sqrMagnitude;

                    if (sqrDistance < closestSqrDistance) {
                        closestObject = gameObject;
                        closestSqrDistance = sqrDistance;
                    }
                }
            }

            return closestObject;
        }

        /// <summary>
        /// Changes the layer of a transform and all its children recursively.
        /// </summary>
        /// <param name="parent">The root transform to start from.</param>
        /// <param name="layer">The layer index to apply.</param>
        public static void ChangeLayerRecursively(Transform parent, int layer) {
            parent.gameObject.layer = layer;

            foreach (Transform child in parent) {
                ChangeLayerRecursively(child, layer);
            }
        }

        /// <summary>
        /// Converts a 2D direction vector to a normalized angle value between 0 and 1.
        /// Uses atan2 to calculate angle from positive X-axis.
        /// </summary>
        /// <param name="direction">The direction vector (uses X and Z components).</param>
        /// <returns>Normalized angle value (0-1 range).</returns>
        public static float GetNormalizedAngle(Vector3 direction) {
            float angle = Mathf.Atan2(direction.z, direction.x);
            return (angle + Mathf.PI) / (2 * Mathf.PI);
        }

        /// <summary>
        /// Gets the current date and time formatted as "yyyy-MM-dd HH:mm:ss".
        /// </summary>
        /// <returns>Formatted date-time string.</returns>
        public static string GetFormattedDateTimeNow() {
            DateTime now = DateTime.Now;
            string formattedDateTime = now.ToString("yyyy-MM-dd HH:mm:ss");
            return formattedDateTime;
        }

        /// <summary>
        /// Calculates the normalized direction vector from attacker to hit player.
        /// </summary>
        /// <param name="hitPlayerPos">Position of the player being hit.</param>
        /// <param name="attackerPlayerPos">Position of the attacking player.</param>
        /// <returns>Normalized direction vector pointing from attacker to target.</returns>
        public static Vector3 CalculateHitDirection(Vector3 hitPlayerPos, Vector3 attackerPlayerPos) {
            Vector3 positionDifference = hitPlayerPos - attackerPlayerPos;
            positionDifference.Normalize();
            return positionDifference;
        }

        /// <summary>
        /// Checks if a type is a subclass of a generic type definition.
        /// </summary>
        /// <param name="toCheck">The type to check.</param>
        /// <param name="generic">The generic type definition to check against.</param>
        /// <returns>True if toCheck is a subclass of the generic type.</returns>
        public static bool IsSubclassOfRawGeneric(Type toCheck, Type generic) {
            while (toCheck != null && toCheck != typeof(object)) {
                var cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
                if (generic == cur) {
                    return true;
                }
                toCheck = toCheck.BaseType;
            }
            return false;
        }

        /// <summary>
        /// Converts a local UI position to world position accounting for camera and canvas scale.
        /// </summary>
        /// <param name="cameraPos">The camera position.</param>
        /// <param name="localPos">The local position on the canvas.</param>
        /// <param name="localScale">The canvas scale factor.</param>
        /// <returns>The corresponding world position.</returns>
        public static Vector3 ConvertLocalPosToRealPos(Vector3 cameraPos, Vector3 localPos, float localScale) {
            Vector3 offset = (localPos - cameraPos) / localScale;
            return cameraPos + offset;
        }

        /// <summary>
        /// Gets the name of an enum value at the specified index.
        /// </summary>
        /// <typeparam name="TEnum">The enum type.</typeparam>
        /// <param name="index">The integer value of the enum.</param>
        /// <returns>The enum name, or empty string if invalid index.</returns>
        public static string GetEnumName<TEnum>(int index) where TEnum : Enum {
            string enumName = Enum.GetName(typeof(TEnum), index);
            if (string.IsNullOrEmpty(enumName)) {
                return "";
            }

            return enumName;
        }

        /// <summary>
        /// Gets the name of an enum value with spaces between words (PascalCase to Title Case).
        /// </summary>
        /// <typeparam name="TEnum">The enum type.</typeparam>
        /// <param name="index">The integer value of the enum.</param>
        /// <returns>The enum name with spaces (e.g., "MyEnumValue" becomes "My Enum Value"), or empty string if invalid.</returns>
        public static string GetEnumNameWordsSpaced<TEnum>(int index) where TEnum : Enum {
            string enumName = Enum.GetName(typeof(TEnum), index);
            if (string.IsNullOrEmpty(enumName)) {
                return "";
            }

            var words = new List<string>();
            var currentWord = enumName[0].ToString().ToUpper();

            for (int i = 1; i < enumName.Length; i++) {
                if (char.IsUpper(enumName[i])) {
                    words.Add(currentWord);
                    currentWord = enumName[i].ToString().ToUpper();
                }
                else {
                    currentWord += enumName[i].ToString().ToLower();
                }
            }

            words.Add(currentWord);

            return string.Join(" ", words);
        }

        /// <summary>
        /// Gets the bounds of a transform by checking for Renderer or Collider components.
        /// </summary>
        /// <param name="transform">The transform to get bounds for.</param>
        /// <returns>The bounds from Renderer or Collider, or a default 1x1x1 bounds at the transform's position if neither found.</returns>
        public static Bounds GetBoundsFromTransform(Transform transform) {
            if (transform.TryGetComponent<Renderer>(out var renderer)) {
                return renderer.bounds;
            }

            if (transform.TryGetComponent<Collider>(out var collider)) {
                return collider.bounds;
            }

            return new Bounds(transform.position, Vector3.one);
        }
    }
}
