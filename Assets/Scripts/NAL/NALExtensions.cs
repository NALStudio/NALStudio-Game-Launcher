using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

namespace NALStudio.Extensions
{
	public static class StringExtensions
	{
		/// <summary>
		/// Capitalizes the string.
		/// </summary>
		/// <param name="str"></param>
		/// <returns></returns>
		public static string Capitalize(this string str)
		{
			return char.ToUpper(str[0]) + str.Substring(1);
		}

		/// <summary>
		/// Capitalizes the string.
		/// </summary>
		/// <param name="str"></param>
		/// <param name="lower">If true; will lower all other characters</param>
		/// <returns></returns>
		public static string Capitalize(this string str, bool lower)
		{
			return Capitalize(lower ? str.ToLower() : str);
		}

		/// <summary>
		/// Return the text ready to be displayed on the UI as the supplied color.
		/// </summary>
		/// <param name="str"></param>
		/// <param name="colour"></param>
		/// <returns></returns>
		public static string Colored(this string str, Color color)
		{
			return $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{str}</color>";
		}

        /// <summary>
        /// If the string ends with the suffix string, return the string with the end suffix removed. Otherwise, return the original string.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="suffix"></param>
        /// <returns></returns>
        public static string RemoveSuffix(this string str, string suffix)
		{
            if (str.EndsWith(suffix))
                return str.Substring(0, str.Length - suffix.Length);
            return str;
		}

        /// <summary>
        /// If the string starts with the prefix string, return the string with the start prefix removed. Otherwise, return the original string.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="prefix"></param>
        /// <returns></returns>
        public static string RemovePrefix(this string str, string prefix)
		{
            if (str.StartsWith(prefix))
                return str.Substring(prefix.Length);
            return str;
		}
	}

	public static class GameObjectExtensions
	{
		/// <summary>
		/// Return a component after either finding it on the game object or otherwise attaching it.
		/// </summary>
		/// <typeparam name="T">Component To Attatch</typeparam>
		/// <param name="gameObject"></param>
		/// <returns>Instance of the component</returns>
		public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
		{
			// Get the component if it exists on the game object and return it
			if (gameObject.TryGetComponent<T>(out T requestedComponent))
			{
				return requestedComponent;
			}

			// Otherwise add a new instance of component, then return it
			return gameObject.AddComponent<T>();
		}
	}

	public static class TransformExtensions
	{
		/// <summary>
		/// Destroy all children of this transform.
		/// </summary>
		/// <param name="transform"></param>
		public static void DestroyChildren(this Transform transform)
		{
			foreach (Transform child in transform)
				Object.Destroy(child.gameObject);
		}
	}

    public static class VectorExtensions
    {
        /// <summary>
        /// Return a copy of this vector with an altered x and/or y and/or z component.
        /// </summary>
        /// <param name="v"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public static Vector3 With(this Vector3 original, float? x = null, float? y = null, float? z = null)
        {
            return new Vector3(x ?? original.x, y ?? original.y, z ?? original.z);
        }

        /// <summary>
        /// Return a copy of this vector with an altered x and/or y component.
        /// </summary>
        /// <param name="v"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static Vector2 With(this Vector2 original, float? x = null, float? y = null)
        {
            return new Vector2(x ?? original.x, y ?? original.y);
        }

        /// <summary>
        /// Return this vector with only its x and y components.
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static Vector2 ToVector2(this Vector3 v)
        {
            // Create a Vector2 from the Vector3 but ignore its z component
            return new Vector2(v.x, v.y);
        }

        /// <summary>
        /// Return a copy of this vector with an altered x component.
        /// </summary>
        /// <param name="v"></param>
        /// <param name="x"></param>
        /// <returns></returns>
        public static Vector2 ChangeX(this Vector2 v, float x)
        {
            return new Vector2(x, v.y);
        }

        /// <summary>
        /// Return a copy of this vector with an altered y component.
        /// </summary>
        /// <param name="v"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static Vector2 ChangeY(this Vector2 v, float y)
        {
            return new Vector2(v.x, y);
        }

        /// <summary>
        /// Return a copy of this vector with an altered x component.
        /// </summary>
        /// <param name="v"></param>
        /// <param name="x"></param>
        /// <returns></returns>
        public static Vector3 ChangeX(this Vector3 v, float x)
        {
            return new Vector3(x, v.y, v.z);
        }

        /// <summary>
        /// Return a copy of this vector with an altered y component.
        /// </summary>
        /// <param name="v"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static Vector3 ChangeY(this Vector3 v, float y)
        {
            return new Vector3(v.x, y, v.z);
        }

        /// <summary>
        /// Return a copy of this vector with an altered z component.
        /// </summary>
        /// <param name="v"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public static Vector3 ChangeZ(this Vector3 v, float z)
        {
            return new Vector3(v.x, v.y, z);
        }

        /// <summary>
        /// Return a Vector3 with this vector's components as well as the supplied z component.
        /// </summary>
        /// <param name="v"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public static Vector3 ChangeZ(this Vector2 v, float z)
        {
            return new Vector3(v.x, v.y, z);
        }
    }

    public static class EnumerableExtensions
	{
        public static string ToString<T>(this IEnumerable<T> item, bool formatted)
		{
            if (!formatted)
				return item.ToString();

			string output = "[";
            foreach (T i in item)
                output += $" {i},";
            return output.RemoveSuffix(",") + " ]";
		}
	}
}