using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MK.Glow.Legacy
{
    public class CamerasGlow : MonoBehaviour
    {
        [SerializeField] private float _glowIntensity = .18f;
        [SerializeField] private float _outlineIntensity = 10f;
        [SerializeField] private float _outlineSize = 1f;

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

                GlowComposite outlineComposite = camera.gameObject.AddComponent<GlowComposite>();
                outlineComposite.intensity = _outlineIntensity;
                GlowPrePass outlinePrePass = secondaryCamera.AddComponent<GlowPrePass>();
                outlinePrePass.size = _outlineSize;
            }
        }
    }
}
