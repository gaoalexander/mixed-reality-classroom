using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GyroRot : MonoBehaviour
{
    public Quaternion imu_pose;
    public GameObject camera;
    // Start is called before the first frame update
    void Start()
    {
        imu_pose = Quaternion.identity;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        /*
        Quaternion unity = 
            Quaternion.Euler(
            -1 * camera.transform.rotation.eulerAngles.x,
            -1 * camera.transform.rotation.eulerAngles.y,
            camera.transform.rotation.eulerAngles.z);
        
        Quaternion imu_rot = (Quaternion.Euler(0, 180, 0) * Quaternion.Euler(0, 0, 90) * unity) * Quaternion.Euler(-45, 0, 0);
        Quaternion imu_sign = new Quaternion(-imu_rot.x, imu_rot.z, imu_rot.y, imu_rot.w);
        camera.transform.rotation = imu_sign;*/

        //Debug.Log("Camera position to set:" + new Vector3(-1 * camera.transform.position.x, camera.transform.position.y, -1 * camera.transform.position.z));
        //camera.transform.position = new Vector3(-1* camera.transform.position.x, camera.transform.position.y, -1* camera.transform.position.z);
        
        //Quaternion imu_rot = Quaternion.Euler(0, 215, 0) * camera.transform.rotation;
        //camera.transform.rotation = new Quaternion(-imu_rot.x, imu_rot.y, -imu_rot.z, imu_rot.w);

        //Quaternion imu_rot = Quaternion.Inverse(camera.transform.rotation);
        //camera.transform.rotation = new Quaternion(-imu_rot.x, imu_rot.z, imu_rot.y, imu_rot.w);

        //Debug.Log(" IMU: " + camera.transform.rotation.eulerAngles);
        //Debug.Log("Actual position:" + camera.transform.position);
    }
}
