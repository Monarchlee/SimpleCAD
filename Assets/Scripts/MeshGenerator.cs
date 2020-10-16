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
    [SerializeField] ComputeShader recoverer = null;
    [SerializeField] ComputeShader modifier = null;
    [SerializeField] ComputeShader remarch = null;

    Volume volume;
    Dictionary<Vector3, int> hash = new Dictionary<Vector3, int>();

    readonly float isoLevel = 0;
    #endregion

    #region Buffers
    ComputeBuffer triangleBufferA;
    ComputeBuffer triangleBufferB;

    int numTris = 0;//储存frontBuffer中的三角形数

    ComputeBuffer frontBuffer;
    ComputeBuffer backBuffer;

    ComputeBuffer pointsBuffer;
    #endregion

    #region Mesh
    MeshFilter filter = null;
    new MeshCollider collider = null;
    #endregion

    #region Operation
    public void InitBuffers(Volume volume)
    {
        this.volume = volume;

        pointsBuffer = new ComputeBuffer(volume.SamplesCount, sizeof(float) * 4);
        triangleBufferA = new ComputeBuffer(volume.VoxelCount * 5, sizeof(float) * 3 * 3, ComputeBufferType.Append);
        triangleBufferB = new ComputeBuffer(volume.VoxelCount * 5, sizeof(float) * 3 * 3, ComputeBufferType.Append);

        frontBuffer = triangleBufferA;
        backBuffer = triangleBufferB;

        frontBuffer.SetCounterValue(0);
        backBuffer.SetCounterValue(0);
    }

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
        dataBuffer.Dispose();
    }

    public void ReadData(Volume volume)
    {
        this.volume = volume;

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

    public void MarchAll()
    {
        Vector3Int threadCount = volume.VoxelThreadCount;
        frontBuffer.SetCounterValue(0);
        march.SetBuffer(0, "points", pointsBuffer);
        march.SetBuffer(0, "triangles", frontBuffer);
        march.SetVector("base", Vector4.zero);
        march.SetInt("numPointsX", volume.SamplesDensity.x);
        march.SetInt("numPointsY", volume.SamplesDensity.y);
        march.SetInt("numPointsZ", volume.SamplesDensity.z);
        march.SetFloat("isoLevel", isoLevel);
        march.Dispatch(0, threadCount.x, threadCount.y, threadCount.z);

        numTris = GetNumTris(frontBuffer);
    }

    public void GetCubeIDs(Vector3 center, float radius, out Vector3 objectCenter, out Vector3 centerID, out Vector3 voxelRange, out Vector3 baseID, out Vector3 voxelCount, out Vector3Int threadCount)
    {      
        objectCenter = transform.InverseTransformPoint(center);
        Vector3 baseCenter = objectCenter + volume.Size * 0.5f;
        centerID = new Vector3(baseCenter.x / volume.VoxelSize.x, baseCenter.y / volume.VoxelSize.y, baseCenter.z / volume.VoxelSize.z);
        voxelRange = new Vector3(
            Mathf.CeilToInt(radius / volume.VoxelSize.x - 0.5f),
            Mathf.CeilToInt(radius / volume.VoxelSize.y - 0.5f),
            Mathf.CeilToInt(radius / volume.VoxelSize.z - 0.5f));

        baseID = centerID - voxelRange + Vector3.one;
        voxelCount = voxelRange * 2;
        threadCount = new Vector3Int(
            Mathf.CeilToInt(voxelCount.x / 8),
            Mathf.CeilToInt(voxelCount.y / 8),
            Mathf.CeilToInt(voxelCount.z / 8));
    }

    public void CleanTriangles(Vector3 centerID, Vector3 voxelRange)
    {
        frontBuffer.SetCounterValue((uint)numTris);
        backBuffer.SetCounterValue(0);
        recoverer.SetBuffer(0, "input", frontBuffer);
        recoverer.SetBuffer(0, "output", backBuffer);
        recoverer.SetInt("numTris", numTris);
        recoverer.SetVector("voxelRange", voxelRange);
        recoverer.SetVector("centerID", centerID);

        recoverer.Dispatch(0, Mathf.CeilToInt(numTris / 256f), 1, 1);

        numTris = GetNumTris(backBuffer);
        Debug.Log("Clear " + numTris + "(Remain)");

        SwitchTriangleBuffers();
    }

    public void Modify(Vector3 baseID, Vector3 voxelCount, Vector3Int threadCount, Vector3 center, float range, float strength, float damping)
    {
        modifier.SetBuffer(0, "points", pointsBuffer);
        modifier.SetVector("base", baseID);
        modifier.SetVector("voxelCount", voxelCount);
        modifier.SetVector("center", center);
        modifier.SetFloat("radius", range);
        modifier.SetFloat("strength", strength);
        modifier.SetFloat("damping", damping);
        modifier.SetInt("numPointsX", volume.SamplesDensity.x);
        modifier.SetInt("numPointsY", volume.SamplesDensity.y);
        modifier.SetInt("numPointsZ", volume.SamplesDensity.z);

        modifier.Dispatch(0, threadCount.x, threadCount.y, threadCount.z);
    }

    public void Remarch(Vector3 baseID, Vector3 voxelCount, Vector3Int threadCount)
    {
        remarch.SetBuffer(0, "points", pointsBuffer);
        remarch.SetBuffer(0, "triangles", frontBuffer);
        remarch.SetVector("base", baseID);
        remarch.SetVector("voxelCount", voxelCount);
        remarch.SetInt("numPointsX", volume.SamplesDensity.x);
        remarch.SetInt("numPointsY", volume.SamplesDensity.y);
        remarch.SetInt("numPointsZ", volume.SamplesDensity.z);
        remarch.SetFloat("isoLevel", isoLevel);
        remarch.Dispatch(0, threadCount.x, threadCount.y, threadCount.z);

        numTris = GetNumTris(frontBuffer);
        Debug.Log("Remarch " + GetNumTris(frontBuffer) + "(front) :" + GetNumTris(backBuffer) + "(back)"); 
    }

    public Mesh GenerateMesh()
    {
        Mesh mesh = new Mesh();

        Vector3[] vertices = UnpackTriangles(frontBuffer, numTris);
        int vertexCount = CompressVertices(ref vertices, out int[] triangles);

        if(vertexCount >= 65536) mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;     
        else mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt16;

        mesh.vertices = vertices;
        mesh.triangles = triangles;

        mesh.RecalculateNormals();

        mesh.bounds = new Bounds(Vector3.zero, volume.Size);

        return mesh;
    }

    public void UnloadBuffer()
    {
        triangleBufferA.Dispose();
        triangleBufferB.Dispose();
        pointsBuffer.Dispose();
    }

    public void SetMesh(Mesh mesh)
    {
        filter.mesh = mesh;
        collider.sharedMesh = mesh;
    }
    #endregion

    #region Lifetime
    private void Start()
    {
        filter = GetComponent<MeshFilter>();
        collider = GetComponent<MeshCollider>();

        Volume sphere = new Volume();

        InitBuffers(sphere);
        ReadData(sphere);
        MarchAll();
        Mesh mesh = GenerateMesh();
        SetMesh(mesh);
        //UnloadBuffer();
    }
    #endregion

    #region Method
    Vector3[] UnpackTriangles(ComputeBuffer triangleBuffer, int numTris)
    {
        Vector3[] vertices = new Vector3[numTris * 3];
        if (numTris == 0) throw new System.Exception("The numTris input is 0.");
        ComputeBuffer verticeBuffer = new ComputeBuffer(numTris * 3, sizeof(float) * 3);
        unpacker.SetInt("triangleCount", numTris);
        unpacker.SetBuffer(0, "triangles", triangleBuffer);
        unpacker.SetBuffer(0, "points", verticeBuffer);
        unpacker.Dispatch(0, Mathf.CeilToInt(numTris / 64f), 1, 1);
        verticeBuffer.GetData(vertices);
        verticeBuffer.Dispose();
        return vertices;
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

    int CompressVertices(ref Vector3[] vertices, out int[] triangles)
    {
        int index = 0;
        hash.Clear();

        triangles = new int[vertices.Length];
        List<Vector3> vertexs = new List<Vector3>();

        for (int i = 0; i < vertices.Length; i++)
        {
            if (!hash.ContainsKey(vertices[i]))
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

    void SwitchTriangleBuffers()
    {
        ComputeBuffer temp = frontBuffer;
        frontBuffer = backBuffer;
        backBuffer = temp;
    }
    #endregion
}