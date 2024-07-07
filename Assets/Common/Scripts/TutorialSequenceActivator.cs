using System;
using UnityEngine;

namespace Common
{
    public class TutorialSequenceActivator : MonoBehaviour
    {
        [SerializeField, Min(0)]
        private float _highlightInterval;

        [SerializeField]
        private bool _oneAtATime;
        
        [SerializeField]
        private GameObject[] _elements;

        private int _current;
        private float _timer;

        private void OnEnable()
        {
            _current = -1;
            _timer = _highlightInterval;

            foreach (var materialHighlighter in _elements) materialHighlighter.SetActive(false);
        }

        private void Update()
        {
            if (_timer <= 0)
            {
                if (_current >= 0 && _oneAtATime) _elements[_current].SetActive(false);

                _current++;

                if (_current >= _elements.Length) OnEnable();
                else _elements[_current].SetActive(true);
                
                _timer += _highlightInterval;

                return;
            }

            _timer -= Time.deltaTime;
        }
    }
}
