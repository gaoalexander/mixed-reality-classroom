using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NucleusActivate : MonoBehaviour
{

    #region

    public Transform snap_nucleus;
    public Vector3 nucleus_final_pos = new Vector3(0.0f,0.0f,0.0f);
    private bool nucleusInTargetZone = true;
    public GameObject Nucleus;

    #endregion

    void Update()
    {
        if (nucleusInTargetZone)
        {
            Nucleus.transform.position = nucleus_final_pos;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            nucleusInTargetZone = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        nucleusInTargetZone = false;
    }

}
