using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MK.Glow.Legacy
{
    public class CamerasGlow : MonoBehaviour
    {
        [SerializeField] private float _intensity = .18f;

        private void Start()
        {
            StartCoroutine(WaitAndAddGlow());
        }

        IEnumerator WaitAndAddGlow()
        {
            yield return new WaitForEndOfFrame();
            foreach (Camera camera in GetComponentsInChildren<Camera>())
            {
                MKGlow mkGlow = camera.gameObject.AddComponent<MKGlow>();
                mkGlow.workflow = Workflow.Selective;
                mkGlow.bloomIntensity = _intensity;
            }
        }
    }
}
