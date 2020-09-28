using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Volume
{
    [System.Serializable]
    public class Volume
    {
        /// <summary>
        /// 储存体积
        /// </summary>
        byte[] data;

        /// <summary>
        /// 展示箱的大小
        /// </summary>
        public Vector3 Size;
        /// <summary>
        /// 数据储存步数
        /// </summary>
        public Vector3Int Density
        {
            get { return density; }
            set
            {
                density = value;
            }
        }
        Vector3Int density;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="size">展示箱的大小</param>
        /// <param name="density">密度(步数)</param>
        public Volume(Vector3 size, Vector3Int density)
        {
            Size = size;
            Density = density;
            long length = Length / 8 + (((Length % 8) == 0) ? 0 : 1);
            data = new byte[length];
        }

        public long Length => (Density.x + 1) * (Density.y + 1) * (Density.z + 1);

        public float this[Vector3Int v]
        {
            get
            {
                long index = (v.z * (Density.z + 1) + v.y) * (Density.y + 1) + v.x;
                byte target = data[index / 8];
                if ((target & (1 << (int)(index % 8))) == 0) return 0;
                else return 1;
            }
            set
            {
                long index = (v.z * (Density.z + 1) + v.y) * (Density.y + 1) + v.x;
                byte target = data[index / 8];
                int insert = value > 0 ? 1 : 0;
                target &= (byte)(insert << (int)(index % 8));
                data[index / 8] = target;
            }
        }

        public static void Serialize(Volume v, string path)
        {
            using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate))
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(fs, v);
                fs.Close();
                fs.Dispose();
            }
        }

        public static Volume Deserialize(string path)
        {
            Volume v;
            using (FileStream fs = new FileStream(path, FileMode.Open))
            {
                BinaryFormatter bf = new BinaryFormatter();
                v = (Volume)bf.Deserialize(fs);
                fs.Close();
                fs.Dispose();
            }
            return v;
        }
    }
}

