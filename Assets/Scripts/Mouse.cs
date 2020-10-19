using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using VolumeData;

public class Mouse : MonoBehaviour
{
    Camera m_Camera = null;
    public Vector3 size = new Vector3(0.5f, 0.5f, 0.5f);
    private Vector3 position = new Vector3(0, 0, 0);
    [SerializeField] GameObject cursor = null;
    [SerializeField] MeshGenerator generator = null;

    [SerializeField] Vector3 canvasSize = Vector3.one * 16;
    [SerializeField] Vector3Int canvasDensity = Vector3Int.one * 64;
    [SerializeField] float initSphereRadius = 10;

    [SerializeField] static public float radius = 10;
    [SerializeField] static public float strength = 10;
    [SerializeField] static public float damping = 1;
    [SerializeField] static public bool iseraser = false;
    [SerializeField] static public int shape = 0;    //sphere:0 cube:1

    public bool mirror_x = false;
    public bool mirror_y = false;
    public bool mirror_z = false;

    // Start is called before the first frame update
    void Start()
    {
        m_Camera = Camera.main;
        m_Camera.depthTextureMode = DepthTextureMode.None;

        cursor = Instantiate(cursor);
        cursor.transform.localScale = size;
        cursor.transform.position = position;
        cursor.SetActive(false);

        Volume sphere = new Volume(canvasSize, canvasDensity);
        generator.InitVolume(sphere, initSphereRadius);

        generator.InitBuffers(sphere);
        generator.ReadData(sphere);
        generator.MarchAll();
        Mesh mesh = generator.GenerateMesh();
        generator.SetMesh(mesh);
    }

    // Update is called once per frame
    void Update()
    {
        Ray ray = m_Camera.ScreenPointToRay(Input.mousePosition);
        if (cursor.activeSelf == false)
            cursor.SetActive(true);
        if (Physics.Raycast(ray, out RaycastHit hit))//击中mesh
        {
            position = hit.point;
            cursor.transform.position = position;
            cursor.transform.LookAt(position + hit.normal);
            if (Input.GetMouseButton(0))
            {
                Mirror(hit.point, out Vector3[] pos);
                foreach(Vector3 p in pos) Modify(p, radius);
                Mesh mesh = generator.GenerateMesh();
                generator.SetMesh(mesh);
            }
        }
    }

    void Mirror(Vector3 position, out Vector3[] positions)
    {
        List<Vector3> pos = new List<Vector3>();

        pos.Add(position);

        if(mirror_x)
        {
            int length = pos.Count;
            for(int i = 0; i < length; i++)
            {
                pos.Add(new Vector3(-pos[i].x, pos[i].y, pos[i].z));
            }
        }
        if (mirror_y)
        {
            int length = pos.Count;
            for (int i = 0; i < length; i++)
            {
                pos.Add(new Vector3(pos[i].x, -pos[i].y, pos[i].z));
            }
        }
        if (mirror_z)
        {
            int length = pos.Count;
            for (int i = 0; i < length; i++)
            {
                pos.Add(new Vector3(pos[i].x, pos[i].y, -pos[i].z));
            }
        }

        positions = pos.ToArray();
    }

    void Modify(Vector3 position, float radius)
    {
        generator.GetCubeIDs(position, radius, out Vector3 objectCenter, out Vector3 centerID, out Vector3 voxelRange, out Vector3 baseID, out Vector3 voxelCount, out Vector3Int threadCount);
        generator.CleanTriangles(centerID, voxelRange);
        generator.Modify(baseID, voxelCount, threadCount, objectCenter, radius, strength, damping);
        generator.Remarch(baseID, voxelCount, threadCount);
    }
}
