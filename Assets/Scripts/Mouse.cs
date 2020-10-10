using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Mouse : MonoBehaviour
{
    public Camera m_Camera;
    public Vector3 size = new Vector3(0.5f, 0.5f, 0.5f);
    public GameObject sphere;
    private Vector3 position = new Vector3(0, 0, 0);



    // Start is called before the first frame update
    void Start()
    {
        sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.localScale = size;
        sphere.transform.position = position;
        Destroy(sphere.GetComponent<SphereCollider>());
        sphere.SetActive(false);

    }

    // Update is called once per frame
    void Update()
    {
        Ray ray = m_Camera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (sphere.activeSelf == false)
            sphere.SetActive(true);
        if (Physics.Raycast(ray, out hit))//击中mesh
        {
            position = hit.point;
            sphere.transform.position = position;
        }
        if (Input.GetMouseButtonDown(0))
        {
            //修改密度值

        }
    }
}
