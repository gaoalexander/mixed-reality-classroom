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
            int i = 0;
            foreach (Camera camera in GetComponentsInChildren<Camera>())
            {
                //if (i == 0)
                //{
                    GameObject secondaryCamera = Instantiate(camera.gameObject, camera.transform);
                    secondaryCamera.transform.localPosition = Vector3.zero;
                    secondaryCamera.GetComponent<Camera>().depth = 0;

                    MKGlow mkGlow = camera.gameObject.AddComponent<MKGlow>();
                    mkGlow.workflow = Workflow.Selective;
                    mkGlow.bloomIntensity = _glowIntensity;

                    GlowComposite outlineComposite = camera.gameObject.AddComponent<GlowComposite>();
                    outlineComposite.intensity = _outlineIntensity;
                    GlowPrePass outlinePrePass = secondaryCamera.AddComponent<GlowPrePass>();
                    outlinePrePass.size = _outlineSize;

                    if (i == 0)
                    {
                        outlineComposite.glowCompositeName = "Hidden/GlowCompositeRight";
                        outlinePrePass.glowPrePassTexName = "_GlowPrePassTexRight";
                        outlinePrePass.glowBlurredTexName = "_GlowBlurredTexRight";
                    }
                    else
                    {
                        outlineComposite.glowCompositeName = "Hidden/GlowCompositeLeft";
                        outlinePrePass.glowPrePassTexName = "_GlowPrePassTexLeft";
                        outlinePrePass.glowBlurredTexName = "_GlowBlurredTexLeft";
                    }

                    i++;
                //}
            }
        }
    }
}
