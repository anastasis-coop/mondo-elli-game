using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WorldSpaceToScreenSpaceIconController : MonoBehaviour
{
    public bool isEnabled = true;
    public bool isContainedOnScreenSpace = true;

    [SerializeField] private Vector3 viewportOffset;
    [SerializeField] Transform _worldSpaceTarget;
    [SerializeField] Camera _camera;
    [SerializeField] Image _screenSpaceIcon;
    [SerializeField] Image _screenSpaceDirectionalImage;
    [SerializeField] private float distanceThreshold = 5;



    private void Start()
    {
        if (_worldSpaceTarget == null) Debug.LogError($"{nameof(WorldSpaceToScreenSpaceIconController)} - I need a world space target to work properly");
        if (_camera == null) Debug.LogError($"{nameof(WorldSpaceToScreenSpaceIconController)} - I need a camera to work properly");
        if (_screenSpaceIcon == null) Debug.LogError($"{nameof(WorldSpaceToScreenSpaceIconController)} - I need a screen space image to work properly");
    }

    private void Update()
    {
        if (_worldSpaceTarget == null || _camera == null || _screenSpaceIcon == null)
        {
            if (_screenSpaceIcon != null && _screenSpaceIcon.enabled) _screenSpaceIcon.enabled = false;
            return;
        }

        isEnabled = Vector3.Distance(_camera.transform.position, _worldSpaceTarget.position) > distanceThreshold;

        if (isEnabled)
        {
            if (!_screenSpaceIcon.gameObject.activeSelf) _screenSpaceIcon.gameObject.SetActive(true);
            if (!_screenSpaceDirectionalImage.gameObject.activeSelf) _screenSpaceDirectionalImage.gameObject.SetActive(true);
            
            Vector3 viewportPos = _camera.WorldToViewportPoint(_worldSpaceTarget.position);
            viewportPos += viewportOffset;

            Vector3 screenPos = _camera.ViewportToScreenPoint(viewportPos);

            if (isContainedOnScreenSpace)
            {
                float marginLeft = 0 + _screenSpaceIcon.rectTransform.rect.width / 2;
                float marginRight = _camera.pixelWidth - _screenSpaceIcon.rectTransform.rect.width / 2;
                float clampedX = Mathf.Clamp(screenPos.x, marginLeft, marginRight);


                float marginBottom = 0 + _screenSpaceIcon.rectTransform.rect.height / 2;
                float marginTop = _camera.pixelHeight - _screenSpaceIcon.rectTransform.rect.height / 2;
                float clampedY = Mathf.Clamp(screenPos.y, marginBottom, marginTop);
                
                Vector2 clampedPos = new Vector2(clampedX, clampedY);
                _screenSpaceIcon.transform.position = clampedPos;
                _screenSpaceDirectionalImage.transform.position = clampedPos;

                _screenSpaceDirectionalImage.gameObject.SetActive(screenPos.x >= marginLeft 
                                                       && screenPos.x <= marginRight
                                                       && screenPos.y >= marginBottom
                                                       && screenPos.y <= marginTop);
            }
            else
            {
                _screenSpaceIcon.transform.position = screenPos;
                _screenSpaceDirectionalImage.transform.position = screenPos;
            }

            //Debug.Log($"ScreenSpace coords starting from bottom-left - {screenPos.x} x, {screenPos.y} y");
        }
        else
        {
            if (_screenSpaceIcon.gameObject.activeSelf) _screenSpaceIcon.gameObject.SetActive(false);
            if (_screenSpaceDirectionalImage.gameObject.activeSelf) _screenSpaceDirectionalImage.gameObject.SetActive(false);
        }
    }
}