using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class MessageSystem : MonoBehaviour
{
    public enum MessageSystemButtons
    {
        RedButton,
        YellowButton,
        GreenButton
    }

    [SerializeField]
    private BigElloSays bigElloSays;

    [SerializeField]
    private GameObject redButton;

    [SerializeField]
    private GameObject yellowButton;

    [SerializeField]
    private TMP_Text yellowButtonText;

    [SerializeField]
    private GameObject greenButton;

    public UnityEvent<int> messageChange;
    public UnityEvent<int> messageShown;
    public UnityEvent<int> messageRead;
    public UnityEvent messageBatchFinished;
    public UnityEvent redButtonClicked;
    public UnityEvent yellowButtonClicked;
    public UnityEvent greenButtonClicked;
    public UnityEvent oneShotShown;
    public UnityEvent oneShotRead;

    public List<BigElloSaysConfig> currentMessagesBatch;

    public BigElloSays BigElloSays => bigElloSays;

    public int LastShownMessageIndex { get; private set; } = -1;
    public int LastReadMessageIndex { get; private set; } = -1;

    private HashSet<int> _skipped = new();
    private int _virtualCap;
    private bool _showingOneShot;

    [SerializeField]
    private float autoplayMuteDelaySeconds = 6;

    public void SetCurrentMessagesBatch(IEnumerable<BigElloSaysConfig> messagesBatch)
    {
        if (messagesBatch == null) return;
        
        currentMessagesBatch = messagesBatch.ToList();
        LastShownMessageIndex = -1;
        _virtualCap = currentMessagesBatch.Count - 1;
        _skipped.Clear();
    }

    public void SetYellowButtonText(string text)
    {
        yellowButtonText.text = text;
    }

    public bool ShowCurrentMessage() => _ = TryShowCurrentMessage();

    public bool TryShowCurrentMessage()
    {
        int messageToShowIndex = LastShownMessageIndex;

        if (_virtualCap < messageToShowIndex || messageToShowIndex < 0)
            return false;

        ShowMessage(messageToShowIndex);

        return true;
    }

    // This is here to allow UnityEvents to call this
    public void ShowNextMessage() => _ = TryShowNextMessage();

    public bool TryShowNextMessage()
    {
        int messageToShowIndex = LastShownMessageIndex + 1;

        if (_virtualCap < messageToShowIndex || messageToShowIndex < 0)
            return false;

        BigElloSaysConfig messageToShow = currentMessagesBatch[messageToShowIndex];

        while (messageToShow.Message.SkipAtFirstRun && !_skipped.Contains(messageToShowIndex))
        {
            if (_virtualCap < messageToShowIndex)
                return false;

            _skipped.Add(messageToShowIndex);
            messageToShow = currentMessagesBatch[++messageToShowIndex];
        }

        ShowMessage(messageToShowIndex);
       
        return true;
    }

    // TODO move click / audio logic inside BigElloSays
    // TODO unify show message logic by passing the finished callback
    private void ShowMessage(int index)
    {
        BigElloSaysConfig message = currentMessagesBatch[index];

        messageChange.Invoke(index);
        bigElloSays.Show(message, index == 0);
        ShowButtons(message);

        CancelInvoke(nameof(OnMessageClick));
        CancelInvoke(nameof(OnOneShotMessageClick));
        bigElloSays.AudioFinished -= OnMessageClick;
        bigElloSays.AudioFinished -= OnOneShotMessageClick;
        bigElloSays.MessageClick -= OnMessageClick;
        bigElloSays.MessageClick -= OnOneShotMessageClick;

        if (message.Message.FinishWithAudio)
        {
            if (!message.Message.LocalizedAudio.IsEmpty)
                bigElloSays.AudioFinished += OnMessageClick;
            else
                Invoke(nameof(OnMessageClick), autoplayMuteDelaySeconds);
        }

        bigElloSays.MessageClick += OnMessageClick;

        LastShownMessageIndex = index;
        messageShown?.Invoke(LastShownMessageIndex);
    }

    public void ShowMessage(BigElloSaysConfig message, bool skipAnimations = true)
    {
        bigElloSays.Show(message, skipAnimations);
        ShowButtons(message);

        CancelInvoke(nameof(OnMessageClick));
        CancelInvoke(nameof(OnOneShotMessageClick));
        bigElloSays.AudioFinished -= OnMessageClick;
        bigElloSays.AudioFinished -= OnOneShotMessageClick;
        bigElloSays.MessageClick -= OnMessageClick;
        bigElloSays.MessageClick -= OnOneShotMessageClick;

        if (message.Message.FinishWithAudio)
        {
            if (!message.Message.LocalizedAudio.IsEmpty)
                bigElloSays.AudioFinished += OnOneShotMessageClick;
            else
                Invoke(nameof(OnOneShotMessageClick), autoplayMuteDelaySeconds);
        }

        bigElloSays.MessageClick += OnOneShotMessageClick;

        oneShotShown?.Invoke();
    }

    private void OnOneShotMessageClick()
    {
        CancelInvoke(nameof(OnMessageClick));
        CancelInvoke(nameof(OnOneShotMessageClick));
        bigElloSays.AudioFinished -= OnMessageClick;
        bigElloSays.AudioFinished -= OnOneShotMessageClick;
        bigElloSays.MessageClick -= OnMessageClick;
        bigElloSays.MessageClick -= OnOneShotMessageClick;

        bigElloSays.Hide();

        oneShotRead?.Invoke();
    }

    public void ShowButtons(BigElloSaysConfig messageToShow)
    {
        if (redButton)
            redButton.SetActive(messageToShow.ShowRedButton);
        if (yellowButton)
            yellowButton.SetActive(messageToShow.ShowYellowButton);
        if (greenButton)
            greenButton.SetActive(messageToShow.ShowGreenButton);
    }

    public void HideButtons()
    {
        if (redButton)
            redButton.SetActive(false);
        if (yellowButton)
            yellowButton.SetActive(false);
        if (greenButton)
            greenButton.SetActive(false);
    }

    private void OnMessageClick()
    {
        CancelInvoke(nameof(OnMessageClick));
        CancelInvoke(nameof(OnOneShotMessageClick));
        bigElloSays.AudioFinished -= OnMessageClick;
        bigElloSays.AudioFinished -= OnOneShotMessageClick;
        bigElloSays.MessageClick -= OnMessageClick;
        bigElloSays.MessageClick -= OnOneShotMessageClick;

        bigElloSays.Hide();
        LastReadMessageIndex = LastShownMessageIndex;

        messageRead?.Invoke(LastShownMessageIndex);

        if (LastReadMessageIndex == currentMessagesBatch.Count - 1 || LastReadMessageIndex == _virtualCap)
        {
            messageBatchFinished?.Invoke();
        }
    }

    public void OnButtonClicked(int messageSystemButtons)
    {
        bigElloSays.Hide();

        HideButtons();

        switch ((MessageSystemButtons)messageSystemButtons)
        {
            case MessageSystemButtons.RedButton:
                redButtonClicked?.Invoke();
                break;
            case MessageSystemButtons.YellowButton:
                yellowButtonClicked?.Invoke();
                break;
            case MessageSystemButtons.GreenButton:
                greenButtonClicked?.Invoke();
                break;
        }
    }

    public void RestartMessagesList()
    {
        bigElloSays.Hide();

        SetCurrentMessagesBatch(currentMessagesBatch);
        ShowNextMessage();
    }

    public void ShowSpecificRangeOfMessages(int startingIndex, int endingIndex)
    {
        bigElloSays.Hide();
        LastShownMessageIndex = startingIndex - 1;
        _virtualCap = endingIndex;
        ShowNextMessage();
    }
}