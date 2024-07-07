using UnityEngine;

namespace Room5
{
    public class TutorialCableControllerActivator : MonoBehaviour
    {
        [SerializeField]
        private LampsCableController _controller;

        [SerializeField, Min(0)]
        private int _activeLamps;

        [SerializeField, Min(0)]
        private float _onOffPeriod;

        private void OnEnable()
        {
            _controller.ConfigureLights(_controller.AvailableLampsCount - _activeLamps, _onOffPeriod);
        }

        private void OnDisable()
        {
            _controller.Stop();
        }
    }
}
