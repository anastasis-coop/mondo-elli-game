using UnityEngine;

namespace Room2
{
    public class FeedbackController : MonoBehaviour
    {
        
        private AudioSource rightAudioSource;
        private AudioSource wrongAudioSource;
        
        [SerializeField] private ParticleSystem rightAnswerPrefab;
        [SerializeField] private ParticleSystem wrongAnswerPrefab;
        [SerializeField] private AudioClip rightAnswerClip;
        [SerializeField] private AudioClip wrongAnswerClip;
        
        [SerializeField] private Color rightColor;
        [SerializeField] private Color wrongColor;

        // Start is called before the first frame update
        void Start()
        {
            rightAudioSource = gameObject.AddComponent<AudioSource>();
            wrongAudioSource = gameObject.AddComponent<AudioSource>();
        }

        public void ShowRightFeedback(Vector3 obj, GameObject mesh = null)
        {
            //VFX
            rightAnswerPrefab.transform.position = obj;
            rightAnswerPrefab.Play();
        
            //SFX
            rightAudioSource.PlayOneShot(rightAnswerClip);

            if (mesh == null) return;
            //Animation
            TargetAnimation anim = mesh.AddComponent<TargetAnimation>();
            anim.overlayColor = rightColor;
        }

        public void ShowWrongFeedback(Vector3 obj,  GameObject mesh = null)
        {
            //VFX
            wrongAnswerPrefab.transform.position = obj;
            wrongAnswerPrefab.Play();
        
            //SFX
            wrongAudioSource.PlayOneShot(wrongAnswerClip);
            
            if (mesh == null) return;
            //Animation
            TargetAnimation anim = mesh.AddComponent<TargetAnimation>();
            anim.overlayColor = wrongColor;
        }

    }
    
    public class TargetAnimation : MonoBehaviour
    {
        public Color overlayColor;

        private void Start()
        {
            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            foreach (Renderer meshRenderer in renderers)
            {
                foreach (Material material in meshRenderer.materials)
                {
                    material.color = overlayColor;  
                }
            }
        }
    }
}


