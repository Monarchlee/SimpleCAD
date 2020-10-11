using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VolumeData;

public class MeshGenerator : MonoBehaviour
{
    #region Properties
    [SerializeField] ComputeShader march = null;
    [SerializeField] ComputeShader transfer = null;
    [SerializeField] ComputeShader unpacker = null;

    float isoLevel = 0;
    #endregion

    #region Buffers
    ComputeBuffer triangleBuffer;
    ComputeBuffer pointsBuffer;
    #endregion

    #region Mesh
    MeshFilter filter = null;
    new MeshCollider collider = null;
    #endregion

    public void WriteData(Volume volume)
    {
        Vector3Int threadCount = volume.SamplesThreadCount;

        ComputeBuffer dataBuffer = new ComputeBuffer(volume.SamplesCount, sizeof(float));
        transfer.SetInt("numPointsX", volume.SamplesDensity.x);
        transfer.SetInt("numPointsY", volume.SamplesDensity.y);
        transfer.SetInt("numPointsZ", volume.SamplesDensity.z);
        transfer.SetVector("cellSize", volume.VoxelSize);
        transfer.SetBuffer(1, "datas", dataBuffer);
        transfer.SetBuffer(1, "points", pointsBuffer);

        transfer.Dispatch(1, threadCount.x, threadCount.y, threadCount.z);

        dataBuffer.GetData(volume.data);
        //理论上,pointsBuffer中的数据转移到了volume中
        dataBuffer.Dispose();
    }

    public void ReadData(Volume volume)
    {
        pointsBuffer = new ComputeBuffer(volume.SamplesCount, sizeof(float) * 4);
        triangleBuffer = new ComputeBuffer(volume.VoxelCount * 5, sizeof(float) * 3 * 3, ComputeBufferType.Append);

        Vector3Int threadCount = volume.SamplesThreadCount;

        ComputeBuffer dataBuffer = new ComputeBuffer(volume.SamplesCount, sizeof(float));
        transfer.SetInt("numPointsX", volume.SamplesDensity.x);
        transfer.SetInt("numPointsY", volume.SamplesDensity.y);
        transfer.SetInt("numPointsZ", volume.SamplesDensity.z);
        transfer.SetVector("cellSize", volume.VoxelSize);
        dataBuffer.SetData(volume.data);
        transfer.SetBuffer(0, "datas", dataBuffer);
        transfer.SetBuffer(0, "points", pointsBuffer);
        transfer.Dispatch(0, threadCount.x, threadCount.y, threadCount.z);
        //理论上,pointsBuffer已经设置完毕

        dataBuffer.Dispose();
    }

    public Mesh GenerateMesh(Volume volume)
    {
        Mesh mesh = new Mesh();

        Vector3Int threadCount = volume.VoxelThreadCount;
        triangleBuffer.SetCounterValue(0);
        march.SetBuffer(0, "points", pointsBuffer);
        march.SetBuffer(0, "triangles", triangleBuffer);
        march.SetInt("numPointsX", volume.SamplesDensity.x);
        march.SetInt("numPointsY", volume.SamplesDensity.y);
        march.SetInt("numPointsZ", volume.SamplesDensity.z);
        march.SetFloat("isoLevel", isoLevel);
        march.Dispatch(0, threadCount.x, threadCount.y, threadCount.z);

        int numTris = GetNumTris(triangleBuffer);

        int[] triangles;
        Vector3[] vertices = UnpackTriangles(numTris);
        int vertexCount = CompressVertices(ref vertices, out triangles);

        if(vertexCount >= 65536) mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;     
        else mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt16;

        mesh.vertices = vertices;
        mesh.triangles = triangles;

        mesh.RecalculateNormals();

        return mesh;
    }

    int GetNumTris(ComputeBuffer triangleBuffer)
    {
        int[] triCountArray = { 0 };
        ComputeBuffer triCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);
        ComputeBuffer.CopyCount(triangleBuffer, triCountBuffer, 0);
        triCountBuffer.GetData(triCountArray);
        int numTris = triCountArray[0];
        triCountBuffer.Dispose();
        return numTris;
    }

    Vector3[] UnpackTriangles(int numTris)
    {
        Vector3[] vertices = new Vector3[numTris * 3];
        ComputeBuffer verticeBuffer = new ComputeBuffer(numTris * 3, sizeof(float) * 3);
        unpacker.SetInt("triangleCount", numTris);
        unpacker.SetBuffer(0, "triangles", triangleBuffer);
        unpacker.SetBuffer(0, "points", verticeBuffer);
        unpacker.Dispatch(0, Mathf.CeilToInt(numTris / 64f), 1, 1);
        verticeBuffer.GetData(vertices);
        verticeBuffer.Dispose();
        return vertices;
    }

    int CompressVertices(ref Vector3[] vertices, out int[] triangles)
    {
        Dictionary<Vector3, int> hash = new Dictionary<Vector3, int>();
        int index = 0;

        triangles = new int[vertices.Length];
        List<Vector3> vertexs = new List<Vector3>();

        for (int i = 0; i < vertices.Length; i++)
        {
            if(!hash.ContainsKey(vertices[i]))
            {
                hash.Add(vertices[i], index);
                vertexs.Add(vertices[i]);
                index++;
            }
            triangles[i] = hash[vertices[i]];
        }

        vertices = vertexs.ToArray();

        return hash.Count;
    }

    public void UnloadBuffer()
    {
        triangleBuffer.Dispose();
        pointsBuffer.Dispose();
    }

    public void SetMesh(Mesh mesh)
    {
        filter.mesh = mesh;
        collider.sharedMesh = mesh;
    }

    private void Start()
    {
        filter = GetComponent<MeshFilter>();
        collider = GetComponent<MeshCollider>();

        Volume sphere = new Volume();
        ReadData(sphere);
        Mesh mesh = GenerateMesh(sphere);
        SetMesh(mesh);
        UnloadBuffer();
    }

}

struct Triangle
{
#pragma warning disable 649 // disable unassigned variable warning
    public Vector3 a;
    public Vector3 b;
    public Vector3 c;

    public Vector3 this[int i]
    {
        get
        {
            switch (i)
            {
                case 0:
                    return a;
                case 1:
                    return b;
                default:
                    return c;
            }
        }
    }
}