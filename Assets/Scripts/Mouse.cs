using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Mouse : MonoBehaviour
{
    Camera m_Camera = null;
    public Vector3 size = new Vector3(0.5f, 0.5f, 0.5f);
    private Vector3 position = new Vector3(0, 0, 0);
    [SerializeField] GameObject cursor = null;
    [SerializeField] MeshGenerator generator = null;

    [SerializeField] float radius = 1;
    [SerializeField] float strength = 1;
    [SerializeField] float damping = 0;

    // Start is called before the first frame update
    void Start()
    {
        m_Camera = Camera.main;
        cursor = Instantiate(cursor);
        cursor.transform.localScale = size;
        cursor.transform.position = position;
        cursor.SetActive(false);
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
                //修改密度值       
                generator.GetCubeIDs(hit.point, radius, out Vector3 objectCenter, out Vector3 centerID, out Vector3 voxelRange, out Vector3 baseID, out Vector3 voxelCount, out Vector3Int threadCount);
                generator.CleanTriangles(centerID, voxelRange);
                generator.Modify(baseID, voxelCount, threadCount, objectCenter, radius, strength, damping);
                generator.Remarch(baseID, voxelCount, threadCount);
                Mesh mesh = generator.GenerateMesh();
                generator.SetMesh(mesh);
            }
        }
    }
}
