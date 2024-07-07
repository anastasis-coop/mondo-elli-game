using System;
using System.Collections;
using System.Collections.Generic;
using Common;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Room5
{
    public class LampsCableController : MonoBehaviour, IEnumerable<CompositeBooleanSwapper>
    {
        [SerializeField]
        private List<CompositeBooleanSwapper> _lamps;

        private readonly List<CompositeBooleanSwapper> _activeLamps = new();

        private readonly List<CompositeBooleanSwapper> _inactiveLamps = new();

        private bool _isPaused;

        private Coroutine _routine;

        public int AvailableLampsCount => _lamps.Count;

        public int ActiveLampsCount => _activeLamps.Count;

        public int InactiveLampsCount => _inactiveLamps.Count;

        public IReadOnlyList<CompositeBooleanSwapper> ActiveLamps => _activeLamps.AsReadOnly();
        
        public IReadOnlyList<CompositeBooleanSwapper> InactiveLamps => _inactiveLamps.AsReadOnly();

        private IEnumerator Execute(float onOffPeriod)
        {
            if (onOffPeriod <= 0)
            {
                _activeLamps.ForEach(l => l.Set(true));
                yield break;
            }
            
            var wait = new WaitForSeconds(onOffPeriod);
            var waitIfPaused = new WaitUntil(() => !_isPaused);
            while (true)
            {
                if (_isPaused) yield return waitIfPaused;
                _activeLamps.ForEach(l => l.Set(!l.CurrentValue));
                yield return wait;
            }
        }

        private IEnumerator MechanicTrial(CompositeBooleanSwapper lamp, Action onEnd)
        {
            yield return null;
            
            var camera = Camera.main;
            RaycastHit hit;
            while (true)
            {
                if (Input.GetMouseButtonDown(0) && 
                    Physics.Raycast(camera.ScreenPointToRay(Input.mousePosition), out hit, 100, ~lamp.gameObject.layer) && 
                    hit.collider.GetComponent<CompositeBooleanSwapper>() == lamp)
                {
                    lamp.Set(false);
                    _routine = null;
                    onEnd?.Invoke();
                    yield break;
                }
                yield return null;
            }
        }

        private void ResetLamps()
        {
            _activeLamps.ForEach(l => l.Set(false));
            _activeLamps.Clear();
            
            _inactiveLamps.Clear();
            
            if (_routine != null) StopCoroutine(_routine);
            _routine = null;

            _isPaused = false;
        }

        public void ConfigureLights(int inactiveCount, float onOffPeriod)
        {
            ResetLamps();
            
            _activeLamps.AddRange(_lamps);
            while (_activeLamps.Count > _lamps.Count - inactiveCount)
                _activeLamps.RemoveAt(Random.Range(0, _activeLamps.Count));
            
            foreach (var l in _lamps)
            {
                if (_activeLamps.Contains(l)) continue; 
                _inactiveLamps.Add(l);
            }

            _routine = StartCoroutine(Execute(onOffPeriod));
        }

        public void Stop() => ConfigureLights(_lamps.Count, 0);

        public void Pause()
        {
            _isPaused = true;
            _activeLamps.ForEach(l => l.Set(false));
        }

        public void Resume() => _isPaused = false;

        public void ExecuteMechanicTrial(Action onEnd)
        {
            ResetLamps();
            var lamp = _lamps[_lamps.Count / 2];
            lamp.Set(true);
            _routine = StartCoroutine(MechanicTrial(lamp, onEnd));
        }

        public bool Contains(GameObject possibleLamp) 
            => possibleLamp.TryGetComponent(out CompositeBooleanSwapper lamp) && Contains(lamp);

        public bool Contains(CompositeBooleanSwapper lamp) => _lamps.Contains(lamp);

        public IEnumerator<CompositeBooleanSwapper> GetEnumerator() => _lamps.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
