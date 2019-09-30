using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomlySpawnOrganelles : MonoBehaviour
{
    [SerializeField] private TCPTestClient _client = null;

    private bool _waitForDelay = false;
    private bool _touchPressed = false;

    // Update is called once per frame
    void Update()
    {
        if (_client.playLocally)
        {
#if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.O) && !_waitForDelay && !_touchPressed)
            {
                _touchPressed = true;
                Debug.Log("Start animation!!!");

                StartCoroutine(RandomlyFindNonActiveOrganelle());

                _waitForDelay = true;
                StartCoroutine(WaitAndReenable());
            }
            if (Input.GetKeyUp(KeyCode.O))
            {
                //stop animation
                _touchPressed = false;
                Debug.Log("Stop animation!!!");
            }
#elif UNITY_IOS

            if (MiraController.TouchPressed && !_waitForDelay && !_touchPressed)
            {
                _touchPressed = true;
                Debug.Log("Start animation!!!");

                StartCoroutine(RandomlyFindNonActiveOrganelle());

                _waitForDelay = true;
                StartCoroutine(WaitAndReenable());
            }
            if (MiraController.TouchReleased)
            {
                //stop animation
                _touchPressed = false;
                Debug.Log("Stop animation!!!");
            }
#endif
        }
    }

    IEnumerator RandomlyFindNonActiveOrganelle()
    {
        bool allAlreadyActive = true;
        for (int i = 0; i < _client.objects.Length; i++)
        {
            if (!_client.objects[i].gameObject.activeSelf)
            {
                allAlreadyActive = false;
            }
        }
        if (!allAlreadyActive)
        {
            int id = Random.Range(0, 12);
            while (_client.objects[id].gameObject.activeSelf)
            {
                id = Random.Range(0, 12);
                yield return null;
            }

            _client.InterpretMarker(_client.objects[id].GetComponent<OrganelleController>().objectId, -1);
        }
    }

    //Add delay so when a marker is detected it doesn't detect it multiple times right away
    IEnumerator WaitAndReenable()
    {
        yield return new WaitForSeconds(1f);
        _waitForDelay = false;
    }
}
