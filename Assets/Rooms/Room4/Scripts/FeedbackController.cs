using UnityEngine;

namespace Room4 {
  public class FeedbackController : MonoBehaviour
  {

    public AudioClip rightAnswerSound;
    public AudioClip wrongAnswerSound;

    public ParticleSystem rightParticles;
    public ParticleSystem wrongParticles;
    
    public void rightAnswerFeedback(Vector3 position)
    {
      AudioSource audio = GetComponent<AudioSource>();
      audio.clip = rightAnswerSound;
      audio.Play();
      
      rightParticles.transform.position = position;
      rightParticles.Play();
    }

    public void wrongAnswerFeedback(Vector3 position)
    {
      missedAnswerFeedback();

      wrongParticles.transform.position = position;
      wrongParticles.Play();
    }

    public void missedAnswerFeedback()
    {
      AudioSource audio = GetComponent<AudioSource>();
      audio.clip = wrongAnswerSound;
      audio.Play();
    }

  }
}
