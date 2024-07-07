using System;
using System.Collections;
using System.Collections.Generic;
using Room1;
using UnityEngine;

public class FeedbackManager : MonoBehaviour
{

    [SerializeField] private GameObject objectToSpawn;
    [SerializeField] private bool rightObject = true;
    [SerializeField] private ParticleSystem rightAnswerPrefab;
    [SerializeField] private ParticleSystem wrongAnswerPrefab;
    [SerializeField] private Color rightColor;
    [SerializeField] private Color wrongColor;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip rightAnswerClip;
    [SerializeField] private AudioClip wrongAnswerClip;
 
    
    void Update()
    {
        if (Input.GetMouseButtonDown(0)) {
            RaycastHit hit;
            // Check if an object is clicked
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 100, LayerMask.GetMask("Objects"))) {
                onObjectClick(hit.transform);
            }
        }
    }
    
    private bool checkValidity()
    {
        return rightObject;
    }
    
    private bool isTargetNotTaken(Transform obj) {
        return (obj.GetComponent<TargetAnimation>() == null);
    }
    
    private void onObjectClick(Transform obj) {
        if (isTargetNotTaken(obj)) {
            if (checkValidity()) 
            {
                onRightObjectClick(obj);
                StartCoroutine(respawnObject());
            } else 
            {
                onWrongObjectClick(obj);
                StartCoroutine(respawnObject());
            }
        }
    }

    private IEnumerator respawnObject()
    {
        yield return new WaitForSeconds(2f);
        Instantiate(objectToSpawn);
    }
    
    private void onRightObjectClick(Transform obj) {
        //VFX
        rightAnswerPrefab.transform.position = obj.position;
        rightAnswerPrefab.Play();
        
        //SFX
        audioSource.PlayOneShot(rightAnswerClip);
        
        //Animation
        TargetAnimation anim = obj.gameObject.AddComponent<TargetAnimation>();
        anim.overlayColor = rightColor;

    }

    private void onWrongObjectClick(Transform obj) {
        //VFX
        wrongAnswerPrefab.transform.position = obj.position;
        wrongAnswerPrefab.Play();

        //SFX
        audioSource.PlayOneShot(wrongAnswerClip);
        
        //Animation
        TargetAnimation anim = obj.gameObject.AddComponent<TargetAnimation>();
        anim.overlayColor = wrongColor;
    }
}

public class TargetAnimation : MonoBehaviour
{
    public Color overlayColor;

    private float elapsed = 0;
    private float rotationSpeed = 0;

    private void Start()
    {
        MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer meshRenderer in renderers)
        {
            foreach (Material material in meshRenderer.materials)
            {
                material.color = overlayColor;  
            }
            
        }
    }

    private void Update()
    {
        elapsed += Time.deltaTime;
        if (elapsed is > 0 and < 0.15f)
        {
            transform.localScale += Vector3.one * 0.004f;
        }

        if (elapsed is >= 0.15f)
        {
            transform.localScale -= Vector3.one * 0.01f;
        }

        if (transform.localScale.x < 0.2f) 
            Destroy(gameObject);
    }
}
