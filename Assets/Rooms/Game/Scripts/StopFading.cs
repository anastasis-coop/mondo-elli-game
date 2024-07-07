using UnityEngine;

namespace Game
{
    public class StopFading : StateMachineBehaviour {

        // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            GameObject fadeScreen = GameObject.Find("Fade screen");
            fadeScreen.SetActive(false);

            GameController.Instance.GameStart();
        }
    }
}
