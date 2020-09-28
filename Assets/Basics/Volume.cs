using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Volume
{
    public abstract class Volume
    {
        /// <summary>
        /// 展示箱的大小
        /// </summary>
        public Vector3 Scale;
        /// <summary>
        /// 数据储存步数
        /// </summary>
        public Vector3Int StorageSteps;

        public long Size => (StorageSteps.x + 1) * (StorageSteps.y + 1) * (StorageSteps.z + 1);

        public virtual float this[Vector3Int v]
        {
            get { return 0; }
            set { }
        }

    }

    public class LogicalVolume : Volume
    {
        /// <summary>
        /// 储存体积
        /// </summary>
        byte[] volume;

        public LogicalVolume(Vector3 scale, Vector3Int storageSteps)
        {
            Scale = scale;
            StorageSteps = storageSteps;
            long length = Size / 8 + (((Size % 8) == 0) ? 0 : 1);
            volume = new byte[length];
        }
        
        public override float this[Vector3Int v]
        {
            get
            {
                long index = (v.z * (StorageSteps.z + 1) + v.y) * (StorageSteps.y + 1) + v.x;
                byte target = volume[index / 8];
                if ((target & (1 << (int)(index % 8))) == 0) return 0;
                else return 1;
            }
            set
            {
                long index = (v.z * (StorageSteps.z + 1) + v.y) * (StorageSteps.y + 1) + v.x;
                byte target = volume[index / 8];
                int insert = value > 0 ? 1 : 0;
                target &= (byte)(insert << (int)(index % 8));
                volume[index / 8] = target;
            }
        }
    }

    public class DensityVolume : Volume
    {
        /// <summary>
        /// 储存体积
        /// </summary>
        byte[,,] volume;

        public DensityVolume(Vector3 scale, Vector3Int storageSteps)
        {
            Scale = scale;
            StorageSteps = storageSteps;
            volume = new byte[storageSteps.x, storageSteps.y, storageSteps.z];
        }

        public override float this[Vector3Int v]
        {
            get
            {
                byte target = volume[v.x, v.y, v.z];
                target -= 128;
                return (float)target / 128;
            }
            set
            {
                byte target = (byte)(value * 128);
                volume[v.x, v.y, v.z] = target;
            }
        }
    }
}

