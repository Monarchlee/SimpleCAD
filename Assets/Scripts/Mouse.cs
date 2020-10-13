using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Mouse : MonoBehaviour
{
    public Camera m_Camera;
    public Vector3 size = new Vector3(0.5f, 0.5f, 0.5f);
    //public GameObject sphere;
    private Vector3 position = new Vector3(0, 0, 0);
    [SerializeField] GameObject cursor;
    [SerializeField] MeshGenerator generator;


    // Start is called before the first frame update
    void Start()
    {
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
                
                generator.GetCubeIDs(hit.point, 1f, out Vector3 centerID, out Vector3 voxelRange);
                generator.CleanTriangles(centerID, voxelRange);
                generator.Remarch(centerID, voxelRange);
                Mesh mesh = generator.GenerateMesh();
                generator.SetMesh(mesh);
            }
        }
    }
}
