using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;

public class RestartButton : MonoBehaviour
{
    public AssetReference sceneToReload;

    public int displayAfterSeconds = 60;
    public int minRightAnswers = 2;
    public Timer timer;
    public Score score;

    public int hideHintAfterSeconds = 2;

    private float cachedTimeScale;

    [SerializeField]
    private Button button;
    private bool canRestart;

    [SerializeField]
    private Transform hint;

    [SerializeField]
    private Transform comicTag;

    [SerializeField]
    private Transform comic;

    [SerializeField]
    private Transform hintText;

    [SerializeField]
    private Transform question;

    void Start()
    {
        button.gameObject.SetActive(false);
        canRestart = !GameState.Instance.hasBeenRestarted();
    }

    private bool buttonIsHidden()
    {
        return !button.gameObject.activeSelf;
    }

    private int secondsPassed()
    {
        if (timer == null)
            return 0;
        return Mathf.RoundToInt(timer.totalTime - timer.timeLeft);
    }

    private int rightAnswers()
    {
        if (score == null)
            return 0;
        return score.RightCounter;
    }

    void Update()
    {
        if (canRestart && buttonIsHidden() && timer.activation && (secondsPassed() > displayAfterSeconds) && (rightAnswers() < minRightAnswers))
        {
            button.gameObject.SetActive(true);
            hint.gameObject.SetActive(true);
            Invoke(nameof(HideHint), hideHintAfterSeconds);
        }
    }

    private void HideHint()
    {
        if (question.gameObject.activeSelf) return;

        hint.gameObject.SetActive(false);
    }

    public void RestartButtonPressed()
    {
        if (hint.gameObject.activeSelf && question.gameObject.activeSelf)
        {
            NoButtonPressed();
            return;
        }

        cachedTimeScale = Time.timeScale;
        Time.timeScale = 0;
        hint.gameObject.SetActive(true);
        hintText.gameObject.SetActive(false);
        question.gameObject.SetActive(true);
    }

    public void YesButtonPressed()
    {
        hint.gameObject.SetActive(false);
        Time.timeScale = cachedTimeScale;
        ReloadCurrentScene();
    }

    public void NoButtonPressed()
    {
        hint.gameObject.SetActive(false);
        Time.timeScale = cachedTimeScale;
    }

    public void ReloadCurrentScene()
    {
        GameState.Instance.setRestarted();
        Addressables.LoadSceneAsync(sceneToReload);
    }

}
