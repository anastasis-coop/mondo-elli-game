using System.Collections;
using UnityEngine;

namespace Game
{
    public class MovementHandler : MonoBehaviour
    {
        private Vector3 startPosition;
        private Vector3 endPosition;

        private float startRotation;
        private float endRotation;

        private bool isMoving = false;
        private bool translate = false;
        private bool rotate = false;

        public GameObject CurrentGround { get; set; }
        private GameObject nextGround = null;

        [SerializeField]
        private GameController controller;

        [SerializeField]
        private Transform playerTransform;

        [SerializeField]
        private Animator playerAnimator;

        private float movementSpeedMs = 1;

        private readonly int turnLeftHash = Animator.StringToHash("TurnLeft");
        private readonly int turnRightHash = Animator.StringToHash("TurnRight");
        private readonly int walkAnimationHash = Animator.StringToHash("Walk");

        public void InitCurrentGround()
        {
            Ray r = new(controller.player.transform.position + (Vector3.down * 20), Vector3.up);

            Physics.Raycast(r, out RaycastHit hitInfo);

            CurrentGround = hitInfo.collider.gameObject;
            CurrentGround.GetComponent<GroundHighlight>().LightUp();
        }

        public void MoveUp(int amount = 1)
        {
            if (!isMoving)
            {
                isMoving = true;

                startPosition = playerTransform.position;
                endPosition = playerTransform.position + playerTransform.TransformDirection(Vector3.forward * 20 * amount);

                movementSpeedMs = amount;

                if (!CheckPath())
                {
                    isMoving = false;
                    return;
                }

                StartCoroutine(WalkAnimation());
            }
        }

        public IEnumerator MoveUpSequence(int numStep)
        {
            for (int i = 0; i < numStep; i++)
            {
                yield return new WaitUntil(IsNotMoving);

                MoveUp();
            }

            GameEvent ev = new GameEvent();
            ev.eventType = GameEventType.PHASE_END;
            ev.triggerObject = null;
            controller.HandleEvent(ev);
        }

        public void MoveRight()
        {
            if (!isMoving)
            {
                isMoving = true;

                startRotation = playerTransform.rotation.eulerAngles.y;
                endRotation = playerTransform.rotation.eulerAngles.y + 90;

                StartCoroutine(TurnAnimation(clockwise: true));
            }
        }

        public void MoveLeft()
        {
            if (!isMoving)
            {
                isMoving = true;

                startRotation = playerTransform.rotation.eulerAngles.y;
                endRotation = playerTransform.rotation.eulerAngles.y - 90;

                StartCoroutine(TurnAnimation(clockwise: false));
            }
        }

        public void MoveSwap(float angle)
        {
            isMoving = true;

            startRotation = playerTransform.rotation.eulerAngles.y;
            endRotation = angle;

            StartCoroutine(TurnAnimation(clockwise: (endRotation - startRotation) > 0));
        }

        public void MoveAngle(float angle)
        {
            isMoving = true;

            startRotation = playerTransform.rotation.eulerAngles.y;
            endRotation = playerTransform.rotation.eulerAngles.y + angle;

            StartCoroutine(TurnAnimation(clockwise: angle > 0));
        }

        public bool IsNotMoving()
        {
            return !isMoving;
        }

        private IEnumerator WalkAnimation()
        {
            translate = true;
            playerAnimator.SetBool(walkAnimationHash, true);

            yield return new WaitWhile(() => isMoving);

            yield return null;

            if (!isMoving)
                playerAnimator.SetBool(walkAnimationHash, false);
        }

        private IEnumerator TurnAnimation(bool clockwise)
        {
            playerAnimator.SetTrigger(clockwise ? turnRightHash : turnLeftHash);

            rotate = true;

            yield return new WaitUntil(() => rotationEnded);
        }

        /* Funzione che rileva se muovendosi in avanti di un passo si incontra un blocco navigabile */
        private bool CheckPath()
        {
            Ray r = new Ray(endPosition + (Vector3.down * 20), Vector3.up);

            RaycastHit hitInfo;
            if (Physics.Raycast(r, out hitInfo))
            {
                nextGround = hitInfo.collider.gameObject;

                if (nextGround != null && nextGround.GetComponent<GroundHighlight>().traversable)
                    return true;
                else
                    return false;
            }
            return false;
        }

        /* Funzione di Update, gestisce le rotazioni e il movimento del personaggio */

        private float tRot = 0;
        private float tMove = 0;

        private bool rotationEnded = false;
        private bool translationEnded = false;

        void Update()
        {
            if (rotate)
            {
                // TODO don't use magic here
                const float TURN_DURATION = 0.5f;
                const float TURN_WAIT_DURATION = 1.3f;

                tRot += Time.deltaTime;

                float r = Mathf.Lerp(startRotation, endRotation, tRot / TURN_DURATION);

                if (tRot >= TURN_WAIT_DURATION)
                {
                    tRot = 0;
                    r = endRotation;

                    rotate = false;
                    rotationEnded = true;
                }

                controller.player.transform.rotation = Quaternion.Euler(0, r, 0);
            }

            if (translate)
            {
                tMove += Time.deltaTime / movementSpeedMs;
                Vector3 p = Vector3.Lerp(startPosition, endPosition, tMove);
                if (tMove >= 1)
                {
                    tMove = 0;
                    p = endPosition;
                    translate = false;
                    GroundHighlight ch = CurrentGround.GetComponent<GroundHighlight>();
                    GroundHighlight nh = nextGround.GetComponent<GroundHighlight>();

                    // Il blocco precedente viene ripristinato
                    if (ch != null)
                    {
                        ch.SetColorH(controller.currentUnlightedColor);

                        if (ch.GetPathState())
                            ch.HighlightOn();
                        else
                            ch.HighlightOff();
                    }

                    // Mi salvo il colore del blocco per ripristinarlo al movimento successivo
                    if (nh != null)
                    {
                        controller.currentUnlightedColor = nh.GetColorH();

                        nh.SetColorH(controller.pathHandler.DefaultColorH);
                        nh.HighlightOn();
                    }

                    CurrentGround = nextGround;
                    nextGround = null;
                    translationEnded = true;
                }

                playerTransform.position = p;

                // HACK not the best place to update progress bar
                controller.UpdateProgressBar();
            }

            if (isMoving && (rotationEnded || translationEnded))
                isMoving = false;

            rotationEnded = false;
            translationEnded = false;
        }
    }
}
