using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MK.Glow.Legacy
{
    public class CamerasGlow : MonoBehaviour
    {
        [SerializeField] private float _glowIntensity = .18f;
        [SerializeField] private float _outlineIntensity = 10f;

        private void Start()
        {
            StartCoroutine(WaitAndAddGlow());
        }

        IEnumerator WaitAndAddGlow()
        {
            yield return new WaitForEndOfFrame();
            foreach (Camera camera in GetComponentsInChildren<Camera>())
            {
                GameObject secondaryCamera = Instantiate(camera.gameObject, camera.transform);

                MKGlow mkGlow = camera.gameObject.AddComponent<MKGlow>();
                mkGlow.workflow = Workflow.Selective;
                mkGlow.bloomIntensity = _glowIntensity;

                GlowComposite outline = camera.gameObject.AddComponent<GlowComposite>();
                outline.Intensity = _outlineIntensity;
                secondaryCamera.AddComponent<GlowPrePass>();
            }
        }
    }
}
