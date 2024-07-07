using System;
using Common;
using UnityEngine;

namespace Room5
{
    public class TutorialSequenceHighlighter : MonoBehaviour
    {
        [SerializeField, Min(0)]
        private float _highlightInterval;

        [SerializeField]
        private Highlighter _highlighter;
        
        [SerializeField]
        private MaterialHighlighter[] _materialHighlighters;

        private int _currentlyHighlighted;
        private float _timer;

        private void OnEnable()
        {
            _currentlyHighlighted = -1;
            _timer = _highlightInterval;

            _highlighter.Hide();
            foreach (var materialHighlighter in _materialHighlighters) materialHighlighter.DeEmphasize();
        }

        private void Update()
        {
            if (_timer <= 0)
            {
                _currentlyHighlighted++;

                if (_currentlyHighlighted >= _materialHighlighters.Length)
                {
                    _currentlyHighlighted = -1;
                    _highlighter.Hide();
                }
                else
                {
                    _highlighter.Highlight(_materialHighlighters[_currentlyHighlighted].gameObject);
                }
                
                _timer += _highlightInterval;

                return;
            }

            _timer -= Time.deltaTime;
        }

        private void OnDisable() => _highlighter.Hide();
    }
}
