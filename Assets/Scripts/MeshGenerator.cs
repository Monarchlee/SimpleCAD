﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VolumeData;

public class MeshGenerator : MonoBehaviour
{
    [SerializeField] ComputeShader march = null;
    [SerializeField] ComputeShader transfer = null;
    [SerializeField] Material mat = null;

    [SerializeField] float isoLevel = 0;

    ComputeBuffer triangleBuffer;
    ComputeBuffer pointsBuffer;
    ComputeBuffer triCountBuffer;

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
        triCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);
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

        int[] triCountArray = { 0 };
        ComputeBuffer.CopyCount(triangleBuffer, triCountBuffer, 0);
        triCountBuffer.GetData(triCountArray);
        int numTris = triCountArray[0];
        Debug.Log("Triangles:" + numTris + "/" + volume.VoxelCount * 5);

        Triangle[] tris = new Triangle[numTris];
        triangleBuffer.GetData(tris, 0, 0, numTris);

        var vertices = new Vector3[numTris * 3];
        var meshTriangles = new int[numTris * 3];

        for (int i = 0; i < numTris; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                meshTriangles[i * 3 + j] = i * 3 + j;
                vertices[i * 3 + j] = tris[i][j];
            }
        }
        mesh.vertices = vertices;
        mesh.triangles = meshTriangles;

        mesh.RecalculateNormals();

        return mesh;
    }

    public void UnloadBuffer()
    {
        triangleBuffer.Dispose();
        pointsBuffer.Dispose();
        triCountBuffer.Dispose();
    }

    private void Start()
    {
        Volume sphere = new Volume();
        ReadData(sphere);
        Mesh mesh = GenerateMesh(sphere);
        GetComponent<MeshFilter>().mesh = mesh;
        UnloadBuffer();

        foreach(Vector3 v in mesh.vertices)
        {
            Debug.Log(v);
        }
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