using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NALStudio.JSON
{
    public static class JsonHelper
    {
        public static T[] FromJsonArray<T>(string json)
        {
            string newJson = "{ \"array\":" + json + "}";
            Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(newJson);
            return wrapper.array;
        }

        public static string ToJsonArray(object[] objs)
		{
            List<string> jsons = new List<string>();
            foreach (object obj in objs)
                jsons.Add(JsonUtility.ToJson(obj));

            bool addExtension = false;
            string json = "[";
            foreach (string _json in jsons)
            {
                if (addExtension)
                    json += ",";
                addExtension = true;

                json += _json;
            }
            json += "]";
            return json;
		}

        public static string ToJsonArray(List<object> objs)
		{
            return ToJsonArray(objs.ToArray());
		}

        [Serializable]
        private class Wrapper<T>
        {
            public T[] array;
        }
    }

}
