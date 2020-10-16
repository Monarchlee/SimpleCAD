using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace VolumeData
{
    [System.Serializable]
    public class Volume
    {
        #region 可修改的参数和数据
        /// <summary>
        /// 储存体积
        /// </summary>
        public float[] data;

        /// <summary>
        /// 展示箱的大小
        /// </summary>
        public Vector3 Size;
        /// <summary>
        /// 每个方向上点的个数
        /// </summary>
        public Vector3Int SamplesDensity
        {
            get { return samplesDensity; }
            set
            {
                samplesDensity = value;
            }
        }
        Vector3Int samplesDensity;
        #endregion

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="size">展示箱的大小</param>
        /// <param name="density">密度(步数)</param>
        public Volume(Vector3 size, Vector3Int density)
        {
            Size = size;
            SamplesDensity = density;
            data = new float[SamplesCount];
        }
        /// <summary>
        /// 这个构造函数用于返回一个球体
        /// </summary>
        /*public Volume()
        {
            Size = Vector3.one * 20;
            SamplesDensity = Vector3Int.one * 64;
            Vector3 center = Vector3.one * 31.5f;
            data = new float[SamplesCount];
            for(int j = 0; j < SamplesDensity.y; j++)
            {
                for(int i = 0; i < SamplesDensity.x; i++)
                {
                    for(int k = 0; k < SamplesDensity.z; k++)
                    {
                        Vector3Int v = new Vector3Int(i, j, k);
                        this[v] = 500 - (v - center).sqrMagnitude;
                    }
                }
            }
        }*/

        #region 属性
        /// <summary>
        /// 每个方向上Cube的个数
        /// </summary>
        public Vector3Int VoxelsDensity => SamplesDensity - Vector3Int.one;
        /// <summary>
        /// 单个Cube的大小
        /// </summary>
        public Vector3 VoxelSize => new Vector3(Size.x / (float)VoxelsDensity.x, Size.y / (float)VoxelsDensity.y, Size.z / (float)VoxelsDensity.z);
        /// <summary>
        /// 点的总个数
        /// </summary>
        public int SamplesCount => SamplesDensity.x * SamplesDensity.y * SamplesDensity.z;
        /// <summary>
        /// Cube的总个数
        /// </summary>
        public int VoxelCount => VoxelsDensity.x * VoxelsDensity.y * VoxelsDensity.z;
        /// <summary>
        /// 每个方向上线程的个数
        /// </summary>
        public Vector3Int VoxelThreadCount => new Vector3Int(Mathf.CeilToInt((float)VoxelsDensity.x / 8f), Mathf.CeilToInt((float)VoxelsDensity.y / 8f), Mathf.CeilToInt((float)VoxelsDensity.z / 8f));
        /// <summary>
        /// 每个方向上线程的个数
        /// </summary>
        public Vector3Int SamplesThreadCount => new Vector3Int(Mathf.CeilToInt((float)SamplesDensity.x / 8f), Mathf.CeilToInt((float)SamplesDensity.y / 8f), Mathf.CeilToInt((float)SamplesDensity.z / 8f));
        #endregion

        private int GetIndex(Vector3Int v)
        {
            return (v.y * SamplesDensity.y + v.x) * SamplesDensity.x + v.z;
        }

        /// <summary>
        /// 获得某点的密度
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public float this[Vector3Int v]
        {
            get
            {
                if (v.x >= SamplesDensity.x || v.y >= SamplesDensity.y || v.z >= SamplesDensity.z) throw new System.IndexOutOfRangeException();
                int index = GetIndex(v);
                return data[index];
            }
            set
            {
                if (v.x >= SamplesDensity.x || v.y >= SamplesDensity.y || v.z >= SamplesDensity.z) throw new System.IndexOutOfRangeException();
                int index = GetIndex(v);
                data[index] = value;
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

