using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VolumeData;

public class MarchingCube : MonoBehaviour
{ 
    [SerializeField]ComputeShader m_marchingCubes = null;

    ComputeBuffer vertexBuffer;
    ComputeBuffer normalBuffer;
    ComputeBuffer volumeBuffer;

    public Mesh GenerateMesh(Volume volume)
    {
        Mesh mesh = new Mesh();

        int Size = volume.BufferSize;
        vertexBuffer = new ComputeBuffer(Size, sizeof(float) * 3);
        normalBuffer = new ComputeBuffer(Size, sizeof(float) * 3);
        volumeBuffer = new ComputeBuffer(volume.DataLength, sizeof(int));

        float[] val = new float[Size * 3];
        for(int i = 0; i < Size * 3; i++)
        {
            val[i] = -1;
        }
        vertexBuffer.SetData(val);
        normalBuffer.SetData(val);

        volumeBuffer.SetData(volume.data);

        m_marchingCubes.SetVector("_Density", (Vector3)volume.Density);
        m_marchingCubes.SetVector("_Size", volume.Size);
        m_marchingCubes.SetBuffer(0, "_Vertex", vertexBuffer);
        m_marchingCubes.SetBuffer(0, "_Normal", normalBuffer);
        m_marchingCubes.SetBuffer(0, "_Volume", volumeBuffer);

        m_marchingCubes.Dispatch(0, Div(volume.Density.x, 16), Div(volume.Density.y, 16), Div(volume.Density.z, 32));

        Vector3[] vertex = new Vector3[Size], normal = new Vector3[Size];
        vertexBuffer.GetData(vertex);
        normalBuffer.GetData(normal);

        mesh.SetVertices(vertex);
        mesh.SetNormals(normal);

        int[] indices = new int[Size * 3];
        for(int i = 0; i < Size * 3; i++)
        {
            indices[i] = i;
        }
        mesh.SetIndices(indices, MeshTopology.Triangles, 0);

        return mesh;
    }

    int Div(int a, int b)
    {
        a = (a + b - 1) & ~(b - 1);
        return a / b;
    }

    private void Start()
    {
        //Testing
        Volume voxel = new Volume();
        Mesh mesh = GenerateMesh(voxel);
        mesh.name = "Generator";
        GetComponent<MeshFilter>().mesh = mesh;
    }
}
