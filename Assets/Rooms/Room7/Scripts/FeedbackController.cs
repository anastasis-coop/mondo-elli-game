using UnityEngine;

namespace Room7 {
  public class FeedbackController : MonoBehaviour
  {

    public AudioClip rightAnswerSound;
    public AudioClip wrongAnswerSound;

    public void rightAnswerFeedback()
    {
      AudioSource audio = Camera.main.GetComponent<AudioSource>();
      audio.clip = rightAnswerSound;
      audio.Play();
    }

    public void wrongAnswerFeedback()
    {
      AudioSource audio = Camera.main.GetComponent<AudioSource>();
      audio.clip = wrongAnswerSound;
      audio.Play();
    }

  }
}
