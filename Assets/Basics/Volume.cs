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
        int[] data;

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
            long length = Length / 32;
            data = new int[length];
        }

        public long Length => Density.x * Density.y * ((Density.z + 31) & (~31));
        public long BufferSize => Length * 5;

        //注意，储存顺序是z→x→y，排列最密的是z，其次是x，最疏的是y
        public int this[Vector3Int v]
        {
            get
            {
                if (v.x >= Density.x || v.y >= Density.y || v.z >= Density.z) throw new System.IndexOutOfRangeException();
                long index = (v.y * Density.y + v.x) * Density.x + v.z;
                int target = data[index / 32];
                if ((target & (1 << (int)(index % 32))) == 0) return 0;
                else return 1;
            }
            set
            {
                if (v.x >= Density.x || v.y >= Density.y || v.z >= Density.z) throw new System.IndexOutOfRangeException();
                long index = (v.y * Density.y + v.x) * Density.x + v.z;
                int target = data[index / 32];
                int insert = value > 0 ? 1 : 0;
                target &= (byte)(insert << (int)(index % 32));
                data[index / 32] = target;
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

