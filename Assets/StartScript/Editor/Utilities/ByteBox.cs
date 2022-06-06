using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;


namespace StartScript
{
    [Serializable]
    public class ByteBox
    {
        [SerializeField]
        byte[] buffer;

        private readonly BinaryFormatter formatter = new BinaryFormatter();

        object cache;

        public void Set(object value)
        {
            if (value is null)
            {
                cache = null;
                buffer = null;
                return;
            }
            using var stream = new MemoryStream();
            formatter.Serialize(stream, value);
            buffer = stream.ToArray();
            cache = value;
        }


        public object Get()
        {
            if (buffer is null)
                return default;

            if (cache is null)
            {
                using var ms = new MemoryStream(buffer);
                cache = formatter.Deserialize(ms);
            }
            return cache;
        }

    }

}
