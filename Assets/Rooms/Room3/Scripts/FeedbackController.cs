using System.Collections;
using System.Collections.Generic;
using Common;
using UnityEngine;

namespace Room3 {
  public class FeedbackController : MonoBehaviour
  {

    public AudioClip rightAnswerSound;
    public AudioClip wrongAnswerSound;

    public BooleanSwapper VisualFeedback;

    private AudioSource _source;

    private AudioSource Source => _source ??= Camera.main.GetComponent<AudioSource>();
    
    public void rightAnswerFeedback()
    {
      Source.PlayOneShot(rightAnswerSound);
      VisualFeedback.Set(true);
    }

    public void wrongAnswerFeedback()
    {
      Source.PlayOneShot(wrongAnswerSound);
      VisualFeedback.Set(false);
    }

  }
}
