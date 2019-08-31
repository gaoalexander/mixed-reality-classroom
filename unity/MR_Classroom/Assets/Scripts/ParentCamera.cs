using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParentCamera : MonoBehaviour
{
    public GameObject parent;
    public GameObject child;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(WaitAndParentCamera());
    }

    // Update is called once per frame
    IEnumerator WaitAndParentCamera()
    {
        yield return new WaitForEndOfFrame();
        child.transform.SetParent(parent.transform);     

    }
}
