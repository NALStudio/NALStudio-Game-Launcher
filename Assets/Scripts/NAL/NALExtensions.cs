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

        /// <summary>
        /// Computes the Damerau-Levenshtein Distance between two strings, represented as arrays of
        /// integers, where each integer represents the code point of a character in the source string.
        /// Includes an optional threshhold which can be used to indicate the maximum allowable distance.
        /// </summary>
        /// <param name="source">An array of the code points of the first string</param>
        /// <param name="target">An array of the code points of the second string</param>
        /// <param name="threshold">Maximum allowable distance</param>
        /// <returns>Int.MaxValue if threshhold exceeded; otherwise the Damerau-Leveshteim distance between the strings</returns>
        /// Computes and returns the Damerau-Levenshtein edit distance between two strings, 
        /// i.e. the number of insertion, deletion, sustitution, and transposition edits
        /// required to transform one string to the other. This value will be >= 0, where 0
        /// indicates identical strings. Comparisons are case sensitive, so for example, 
        /// "Fred" and "fred" will have a distance of 1. This algorithm is basically the
        /// Levenshtein algorithm with a modification that considers transposition of two
        /// adjacent characters as a single edit.
        /// http://blog.softwx.net/2015/01/optimizing-damerau-levenshtein_15.html
        /// </summary>
        /// <remarks>See http://en.wikipedia.org/wiki/Damerau%E2%80%93Levenshtein_distance
        /// Note that this is based on Sten Hjelmqvist'str "Fast, memory efficient" algorithm, described
        /// at http://www.codeproject.com/Articles/13525/Fast-memory-efficient-Levenshtein-algorithm.
        /// This version differs by including some optimizations, and extending it to the Damerau-
        /// Levenshtein algorithm.
        /// Note that this is the simpler and faster optimal string alignment (aka restricted edit) distance
        /// that difers slightly from the classic Damerau-Levenshtein algorithm by imposing the restriction
        /// that no substring is edited more than once. So for example, "CA" to "ABC" has an edit distance
        /// of 2 by a complete application of Damerau-Levenshtein, but a distance of 3 by this method that
        /// uses the optimal string alignment algorithm. See wikipedia article for more detail on this
        /// distinction.
        /// </remarks>
        /// <param name="str">String being compared for distance.</param>
        /// <param name="value">String being compared against other string.</param>
        /// <param name="maxDistance">The maximum edit distance of interest.</param>
        /// <returns>int edit distance, >= 0 representing the number of edits required
        /// to transform one string to the other, or -1 if the distance is greater than the specified maxDistance.</returns>
        public static int DamerauLevenshteinDistance(this string str, string value, int maxDistance = int.MaxValue)
        {
            if (string.IsNullOrEmpty(str))
                return ((value ?? "").Length <= maxDistance) ? (value ?? "").Length : -1;
            if (string.IsNullOrEmpty(value))
                return (str.Length <= maxDistance) ? str.Length : -1;

            // if strings of different lengths, ensure shorter string is in str. This can result in a little
            // faster speed by spending more time spinning just the inner loop during the main processing.
            if (str.Length > value.Length)
            {
                var temp = str; str = value; value = temp; // swap str and value
            }
            int sLen = str.Length; // this is also the minimun length of the two strings
            int tLen = value.Length;

            // suffix common to both strings can be ignored
            while ((sLen > 0) && (str[sLen - 1] == value[tLen - 1])) { sLen--; tLen--; }

            int start = 0;
            if ((str[0] == value[0]) || (sLen == 0))
            { // if there'str a shared prefix, or all str matches value'str suffix
              // prefix common to both strings can be ignored
                while ((start < sLen) && (str[start] == value[start])) start++;
                sLen -= start; // length of the part excluding common prefix and suffix
                tLen -= start;

                // if all of shorter string matches prefix and/or suffix of longer string, then
                // edit distance is just the delete of additional characters present in longer string
                if (sLen == 0)
                    return (tLen <= maxDistance) ? tLen : -1;

                value = value.Substring(start, tLen); // faster than value[start+j] in inner loop below
            }
            int lenDiff = tLen - sLen;
            if ((maxDistance < 0) || (maxDistance > tLen))
				maxDistance = tLen;
			else if (lenDiff > maxDistance)
                return -1;

            var v0 = new int[tLen];
            var v2 = new int[tLen]; // stores one level further back (offset by +1 position)
            int j;
            for (j = 0; j < maxDistance; j++) v0[j] = j + 1;
            for (; j < tLen; j++) v0[j] = maxDistance + 1;

            int jStartOffset = maxDistance - (tLen - sLen);
            bool haveMax = maxDistance < tLen;
            int jStart = 0;
            int jEnd = maxDistance;
            char sChar = str[0];
            int current = 0;
            for (int i = 0; i < sLen; i++)
            {
                char prevsChar = sChar;
                sChar = str[start + i];
                char tChar = value[0];
                int left = i;
                current = left + 1;
                int nextTransCost = 0;
                // no need to look beyond window of lower right diagonal - maxDistance cells (lower right diag is i - lenDiff)
                // and the upper left diagonal + maxDistance cells (upper left is i)
                jStart += (i > jStartOffset) ? 1 : 0;
                jEnd += (jEnd < tLen) ? 1 : 0;
                for (j = jStart; j < jEnd; j++)
                {
                    int above = current;
                    int thisTransCost = nextTransCost;
                    nextTransCost = v2[j];
                    v2[j] = current = left; // cost of diagonal (substitution)
                    left = v0[j];    // left now equals current cost (which will be diagonal at next iteration)
                    char prevtChar = tChar;
                    tChar = value[j];
                    if (sChar != tChar)
                    {
                        if (left < current) current = left;   // insertion
                        if (above < current) current = above; // deletion
                        current++;
                        if ((i != 0) && (j != 0)
                            && (sChar == prevtChar)
                            && (prevsChar == tChar))
                        {
                            thisTransCost++;
                            if (thisTransCost < current) current = thisTransCost; // transposition
                        }
                    }
                    v0[j] = current;
                }
                if (haveMax && (v0[i + lenDiff] > maxDistance)) return -1;
            }
            return (current <= maxDistance) ? current : -1;
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

    public static class ColorExtensions
	{
        public static Color Mode(this ICollection<Color> colors)
		{
            if (colors.Count < 1)
                throw new System.ArgumentOutOfRangeException("Colors cannot be empty!");

            Dictionary<Color, int> colorCounts = new Dictionary<Color, int>();

            foreach (Color c in colors)
			{
                if (!colorCounts.ContainsKey(c))
                    colorCounts.Add(c, 0);

                colorCounts[c]++;
			}
            Color? maxValue = null;
            int maxCount = 0;
            foreach (KeyValuePair<Color, int> kv in colorCounts)
			{
                if (kv.Value > maxCount)
				{
                    maxValue = kv.Key;
                    maxCount = kv.Value;
				}
			}

            if (maxValue is Color ret)
                return ret;
            throw new System.InvalidOperationException("Tried to operate Mode with an empty list.");
		}

        public static Color Average(this ICollection<Color> colors)
		{
            if (colors.Count < 1)
                throw new System.ArgumentOutOfRangeException("Colors cannot be empty!");

            float rSum = 0;
            float gSum = 0;
            float bSum = 0;

            foreach (Color c in colors)
			{
                rSum += c.r;
                gSum += c.g;
                bSum += c.b;
			}

            return new Color(rSum / colors.Count, gSum / colors.Count, bSum / colors.Count);
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