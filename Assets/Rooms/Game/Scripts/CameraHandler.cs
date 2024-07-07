using System;
using Cinemachine;
using UnityEngine;

namespace Game
{
    public class CameraHandler : MonoBehaviour
    {
        // Used just for backend
        private enum CameraAngleType
        {
            BACK_NEAR,
            BACK_MID,
            BACK_FAR,
            TOP_DOWN_NEAR,
            TOP_DOWN_MID,
            TOP_DOWN_FAR
        }

        [Serializable]
        private class CameraAngle
        {
            public CameraAngleType AngleType;
            public CinemachineVirtualCamera Camera;
        }

        [SerializeField]
        private CameraAngle[] gameplayCameraAngles;
        public CinemachineVirtualCamera frontCamera;
        public int startingCameraIndex = 0;
        public CinemachineTargetGroup targetGroup;
        public Transform mainTarget;
        public Transform detachedTarget;
        
        [SerializeField]
        private float panSpeed = 5;

        [SerializeField]
        private float panDelay = 0.5f;

        [SerializeField, Tooltip("Normalized screen rect where pan doesn't happen")]
        private Rect noPanRect;

        [SerializeField]
        private Bounds worldBounds;

        public bool PanEnabled = true;

        private int currentCameraIndex;
        private bool detached;
        private float _timer;

        public void Start()
        {
            if (GameState.Instance != null)
            {
                if (GameState.Instance.firstLoad || GameState.Instance.testMode)
                {
                    GameState.Instance.cameraIndex = startingCameraIndex;

                    currentCameraIndex = startingCameraIndex;
                }
                else
                {
                    currentCameraIndex = GameState.Instance.cameraIndex;
                }
            }


            UpdateEnabledCamera();
        }

        private void Update()
        {
            if (!PanEnabled || frontCamera.enabled) return;

            Vector2 normalized = Input.mousePosition;
            normalized.x /= Screen.width;
            normalized.y /= Screen.height;

            // Inside no pan area or outside window
            if (noPanRect.Contains(normalized)
                || normalized.x > 1 || normalized.x < 0
                || normalized.y > 1 || normalized.y < 0)
            {
                _timer = 0;
                return;
            }

            if (_timer < panDelay)
            {
                _timer += Time.deltaTime;
                return;
            }

            if (!detached)
            {
                DetachCamera();
                // TODO change camera button to recenter button?
            }

            // Screen center = (0,0)
            Vector2 screenDir = normalized - Vector2.one / 2;

            // Screen space -> World space

            Vector3 worldDir = gameplayCameraAngles[currentCameraIndex].Camera.transform
                .TransformDirection(screenDir);

            // No Y movement
            worldDir.y = 0;

            Vector3 targetPos = detachedTarget.position + worldDir * panSpeed;

            if (worldBounds.Contains(targetPos))
                detachedTarget.position = worldBounds.ClosestPoint(targetPos);
        }

        public string GetCurentCamera() => gameplayCameraAngles[currentCameraIndex].AngleType.ToString();

        // Funzione per cambiare la camera corrente
        public void SwitchCamera()
        {
            if (frontCamera.enabled)
            {
                ToggleFrontCamera(false);
                // todo change camera button to recenter button?
            }
            else if (detached)
            {
                AttachCamera();
            }
            else
            {
                currentCameraIndex++;

                if (currentCameraIndex >= gameplayCameraAngles.Length)
                    currentCameraIndex = 0;

                UpdateEnabledCamera();
            }
        }

        public void ToggleFrontCamera(bool enabled)
        {
            gameplayCameraAngles[currentCameraIndex].Camera.enabled = !enabled;
            frontCamera.enabled = enabled;
        }

        public void UpdateEnabledCamera()
        {
            for (int i = 0; i < gameplayCameraAngles.Length; i++)
            {
                if (currentCameraIndex == i)
                {
                    gameplayCameraAngles[i].Camera.enabled = true;
                }
                else
                    gameplayCameraAngles[i].Camera.enabled = false;
            }
        }

        public void SaveCameraInfo()
        {
            GameState.Instance.cameraIndex = currentCameraIndex;
        }

        public void DetachCamera()
        {
            if (detached) return;

            detachedTarget.position = mainTarget.position;
            detachedTarget.rotation = mainTarget.rotation;

            var target = new CinemachineTargetGroup.Target()
            {
                target = detachedTarget,
                weight = 1,
                radius = 1,
            };

            // HACK RemoveTarget seem to be bugged
            targetGroup.m_Targets = new[] { target };

            detached = true;
        }

        public void AttachCamera()
        {
            if (!detached) return;

            var target = new CinemachineTargetGroup.Target()
            {
                target = mainTarget,
                weight = 1,
                radius = 1,
            };

            // HACK RemoveTarget seem to be bugged
            targetGroup.m_Targets = new[] { target };

            detached = false;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireCube(worldBounds.center, worldBounds.size);
        }
#endif
    }
}