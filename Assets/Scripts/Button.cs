using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Button : MonoBehaviour
{

    public void ClickQ()
    {
        MouseView.state = 0;
    }
    public void ClickW()
    {
        MouseView.state = 1;
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            ClickQ();
        }
        else if (Input.GetKeyDown(KeyCode.W))
        {
            ClickW();
        }
    }
}
