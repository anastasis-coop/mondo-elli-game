using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Cutscene
{
    public class CutsceneMessageSystem : MonoBehaviour
    {
        [SerializeField]
        private BigElloSays bigElloSays;
        
        [field: SerializeField]
        public bool AutoRead { get; set; }
        [field: SerializeField, Min(0)]
        public float AutoReadExtraTime { get; set; }

        public UnityEvent<int> messageChange;
        public UnityEvent<int> messageShown;
        public UnityEvent<int> messageRead;
        public UnityEvent messageBatchFinished;

        public List<BigElloMessage> currentMessagesBatch;

        public BigElloSays BigElloSays => bigElloSays;

        public int LastShownMessageIndex { get; private set; } = -1;
        public int LastReadMessageIndex { get; private set; } = -1;

        private HashSet<int> _skipped = new();
        private int _virtualCap;
        private bool _showingOneShot;
        private int _inhibitionsStack;

        public void SetCurrentMessagesBatch(IEnumerable<BigElloMessage> messagesBatch)
        {
            if (messagesBatch == null) return;
            
            currentMessagesBatch = messagesBatch.ToList();
            LastShownMessageIndex = -1;
            _virtualCap = currentMessagesBatch.Count - 1;
        }

        public bool TryShowCurrentMessage()
        {
            int messageToShowIndex = LastShownMessageIndex;

            if (_virtualCap < messageToShowIndex || messageToShowIndex < 0)
                return false;

            ShowMessage(messageToShowIndex);

            return true;
        }

        // This is here to allow UnityEvents to call this
        public void ShowNextMessage() => TryShowNextMessage();

        public bool TryShowNextMessage()
        {
            int messageToShowIndex = LastShownMessageIndex + 1;

            if (_virtualCap < messageToShowIndex || messageToShowIndex < 0)
                return false;

            var messageToShow = currentMessagesBatch[messageToShowIndex];

            while (messageToShow.SkipAtFirstRun && !_skipped.Contains(messageToShowIndex))
            {
                if (_virtualCap < messageToShowIndex)
                    return false;

                messageToShowIndex++;
                _skipped.Add(messageToShowIndex);
                messageToShow = currentMessagesBatch[messageToShowIndex];
            }

            ShowMessage(messageToShowIndex);
           
            return true;
        }

        // TODO move click / audio logic inside BigElloSays
        // TODO unify show message logic by passing the finished callback
        private void ShowMessage(int index)
        {
            BigElloMessage message = currentMessagesBatch[index];
            
            bigElloSays.MessageFinished -= OnMessageFinished;
            bigElloSays.MessageClick -= OnMessageClick;

            if (AutoRead) bigElloSays.MessageFinished += OnMessageFinished;

            messageChange.Invoke(index);
            bigElloSays.Show(message);

            bigElloSays.MessageClick += OnMessageClick;

            LastShownMessageIndex = index;
            messageShown?.Invoke(LastShownMessageIndex);
        }

        private IEnumerator OnMessageFinishedRoutine()
        {
            yield return new WaitForSeconds(AutoReadExtraTime);
            yield return new WaitUntil(() => _inhibitionsStack == 0);
            OnMessageClick();
        }

        private void OnMessageFinished() => StartCoroutine(OnMessageFinishedRoutine());

        private void OnMessageClick()
        {
            CancelInvoke(nameof(OnMessageClick));
            bigElloSays.AudioFinished -= OnMessageClick;
            bigElloSays.MessageClick -= OnMessageClick;

            bigElloSays.Hide();
            LastReadMessageIndex = LastShownMessageIndex;

            messageRead?.Invoke(LastShownMessageIndex);

            if (LastShownMessageIndex == currentMessagesBatch.Count - 1 || LastShownMessageIndex == _virtualCap)
            {
                messageBatchFinished?.Invoke();
            }
        }

        public void SetPause(bool pause)
        {
            if (pause) _inhibitionsStack++;
            else _inhibitionsStack--;

            bigElloSays.PauseRead(pause);
        }

        public void RemoveInhibition() => _inhibitionsStack = Mathf.Max(0, _inhibitionsStack - 1);
        
        public void RequestInhibition() => _inhibitionsStack++;
    }
}