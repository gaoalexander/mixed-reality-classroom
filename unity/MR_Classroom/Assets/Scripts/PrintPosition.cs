using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrintPosition : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log("Object Orientation:" + transform.rotation.ToEuler());
        Debug.Log("Object Position:" + transform.position);
    }
}
