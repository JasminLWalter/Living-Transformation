﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class ObjectTransitions : MonoBehaviour
{

    private new SkinnedMeshRenderer _rend;
    public GameObject objMesh;

    public Material[] newMats;

    private float[] smoothnessOriginalList = new float[]{0.0f, 0.0f, 0.0f};
    private float[] bumpScaleOriginalList = new float[]{0.0f, 0.0f, 0.0f};
    private float[] occlusionOriginalList = new float[]{0.0f, 0.0f, 0.0f};
    private float[] metallicOriginalList = new float[]{0.0f, 0.0f, 0.0f};
    private float glowAlphaOriginal;
    private float maxTransparencyVal = 1;
    
    
    public int transitions;
    public int previousTransitionsID;
    public bool upwards;
    public bool makeTransition = false;

    private Animator objectAnim;
    private float slowDownDuration = 1.0f;
    private float speedUpDuration = 2.0f;

    

    private float shrinkingDuration = 0.7f;
    private float growingDuration = 2.0f;
    private float shakeDuration = 2.0f;
    
    
    private float fade2BlackDuration = 2.0f;
    private float fade2WhiteDuration = 2.0f;
    private Color fadeColor;

    /// variables from other transition tries



    // public Material placeholderMat;

    // public Material defaultMat;
    // public Material objectDark;
    // public Material objectColor;
    // public Material objectGlow;

    // public float fadeOutDuration = 2.0f;
    // public float fadeInDuration = 2.0f;

    // public Material material1;
    // public Material material2;
    // public float duration = 2.0f;
    // public Renderer rend;


    public bool testTransitions = false;
    
    
    
    
    // Start is called before the first frame update
    void Start()
    {
        _rend = objMesh.GetComponent<SkinnedMeshRenderer>();
        transitions = 0;
        upwards = true;

        objectAnim = gameObject.GetComponent<Animator>();
        
        // assign all initial values of the materials to lists
        for (int i = 0; i < 3; i++)
        {
            smoothnessOriginalList[i] = newMats[i].GetFloat("_Smoothness");
            bumpScaleOriginalList[i] = newMats[i].GetFloat("_BumpScale");
            occlusionOriginalList[i] = newMats[i].GetFloat("_OcclusionStrength");
            metallicOriginalList[i] = newMats[i].GetFloat("_Metallic");
        }

        glowAlphaOriginal = newMats[2].color.a;
    }

    // Update is called once per frame
    void Update()
    {

        if (testTransitions)
        {
            MakeTransition();
            testTransitions = false;
        }
        
    }

    public void MakeTransition()
    {
        StartCoroutine(Transitioning(objectAnim));
    }

    public void ChangeLiveMat(int newMatNum)
    {
        // Find the material in the array
         Material materialToUse = newMats[newMatNum];


         _rend.material = materialToUse;

    }
    

    private IEnumerator Transitioning(Animator objectAnim)
    {

        StartCoroutine(SlowDownAnimation(objectAnim));
        yield return new WaitForSeconds(slowDownDuration);
        
        StartCoroutine(ShakeObject());
        yield return new WaitForSeconds(shakeDuration);
        

        StartCoroutine(FadeToBlackMaterial());
        yield return new WaitForSeconds(fade2BlackDuration);

        previousTransitionsID = transitions;
        // select upcoming material
        if (upwards)
        {
            transitions++;
            if(transitions == 2)
            {
                upwards = false;
            }
        }
        else
        {
            transitions--;
            if (transitions == 0)
            {
                upwards = true;
            }
        }


        PrepareUpcomingMaterial(transitions);
            
        ChangeLiveMat(transitions);

        Return2defaultMaterial(previousTransitionsID);

        // fade new material in
        StartCoroutine(FadeToWhiteMaterial());
        yield return new WaitForSeconds(fade2WhiteDuration);
        // StartCoroutine(GrowModel());
        // yield return new WaitForSeconds(growingDuration);

        
        StartCoroutine(SpeedUpAnimation(objectAnim));
        yield return new WaitForSeconds(speedUpDuration);

        GazeSphereInteraction.Instance().coolOffPeriod = true;
        yield return null;
    }

   
    
    private IEnumerator SlowDownAnimation(Animator currentAnim)
    {
        float timer = 0.0f;
        while (timer <= slowDownDuration)
        {
            currentAnim.speed =  Mathf.Lerp(1, 0, timer / slowDownDuration);
            
            timer += Time.deltaTime;
            yield return null;
        }

        currentAnim.speed = 0;

        yield return null;
    }
    
    private IEnumerator SpeedUpAnimation(Animator currentAnim)
    {
        float timer = 0.0f;
        while (timer <= speedUpDuration)
        {
            currentAnim.speed =  Mathf.Lerp(0, 1, timer / speedUpDuration);
            
            timer += Time.deltaTime;
            yield return null;
        }

        currentAnim.speed = 1;

        yield return null;
    }

    private IEnumerator ShakeObject()
    {
        Vector3 initialPosition = transform.position;
        float timer = 0.0f;
        
        Vector3 directionOfShake = transform.up;
        float amplitude = 0.01f; // the amount it moves
        float frequency = 20.0f;
        
        
        while (timer <= shakeDuration)
        {

            transform.position = initialPosition + directionOfShake *
                (-amplitude + Mathf.PingPong(frequency * Time.deltaTime, 2.0f * amplitude));
            timer += Time.deltaTime;

            yield return null;
        }

        transform.position = initialPosition;
            

        yield return null;
    }

    private void PrepareUpcomingMaterial(int identifier)
    {
        // fade out color
        Color newColor = fadeColor;
        newColor = Color.black;
            
        if (identifier == 2)
        {
            newColor.a = maxTransparencyVal;
        }
        
        newMats[identifier].color = newColor;
        newMats[identifier].SetFloat("_Smoothness",0.0f);
        newMats[identifier].SetFloat("_BumpScale", 0.0f);
        newMats[identifier].SetFloat("_OcclusionStrength",0.0f);
        newMats[identifier].SetFloat("_Metallic", 0.0f);

    }

    private void Return2defaultMaterial(int identifier)
    {
        // fade out color
        Color newColor = fadeColor;
        newColor = Color.white;
            
        if (identifier == 2)
        {
            newColor.a = glowAlphaOriginal;
        }
        
        newMats[identifier].color = newColor;
        newMats[identifier].SetFloat("_Smoothness",smoothnessOriginalList[identifier]);
        newMats[identifier].SetFloat("_BumpScale", bumpScaleOriginalList[identifier]);
        newMats[identifier].SetFloat("_OcclusionStrength",occlusionOriginalList[identifier]);
        newMats[identifier].SetFloat("_Metallic", metallicOriginalList[identifier]);

    }
    private IEnumerator FadeToBlackMaterial()
    {
        
        float timer = 0.0f;
        
        while (timer <= fade2BlackDuration)
        {
            // fade out color
            Color newColor = fadeColor;
            newColor = Color.Lerp(Color.white, Color.black, timer / fade2BlackDuration);
            
            if (transitions == 2)
            {
                newColor.a = Mathf.Lerp(glowAlphaOriginal, maxTransparencyVal, timer / fade2BlackDuration);   
            }
            
            _rend.material.color = newColor;
            
            // other properties of material
            float newSmoothVal = Mathf.Lerp(smoothnessOriginalList[transitions], 0.0f, timer / fade2BlackDuration);
            _rend.material.SetFloat("_Smoothness",newSmoothVal);
            
            float newBumpVal = Mathf.Lerp(bumpScaleOriginalList[transitions], 0.0f, timer / fade2BlackDuration);
            _rend.material.SetFloat("_BumpScale", newBumpVal);
            
            float newOccVal = Mathf.Lerp(occlusionOriginalList[transitions], 0.0f, timer / fade2BlackDuration);
            _rend.material.SetFloat("_OcclusionStrength",newOccVal);
            
            float newMetallVal = Mathf.Lerp(metallicOriginalList[transitions], 0.0f, timer / fade2BlackDuration);
            _rend.material.SetFloat("_Metallic", newMetallVal);
            
            
            timer += Time.deltaTime;
            yield return null;
        }
    
        
        // make sure final values are reached
        Color newColor2 = fadeColor;
        newColor2 = Color.black;
            
        if (transitions  == 2)
        {
            newColor2.a = maxTransparencyVal;
        }
        _rend.material.color = newColor2;
        _rend.material.SetFloat("_Smoothness",0.0f);
        _rend.material.SetFloat("_BumpScale", 0.0f);
        _rend.material.SetFloat("_OcclusionStrength",0.0f);
        _rend.material.SetFloat("_Metallic", 0.0f);

        yield return null;
    }
    
    private IEnumerator FadeToWhiteMaterial()
    {
        
        float timer = 0.0f;
        
        while (timer <= fade2WhiteDuration)
        {
            Color newColor = fadeColor;
            newColor = Color.Lerp(Color.black, Color.white, timer / fade2WhiteDuration);
            
            if (transitions == 2)
            {
                newColor.a = Mathf.Lerp(maxTransparencyVal, glowAlphaOriginal, timer / fade2WhiteDuration);   
            }
            _rend.material.color = newColor;
            
            
            // other properties of material
            float newSmoothVal = Mathf.Lerp(0.0f,smoothnessOriginalList[transitions], timer / fade2WhiteDuration);
            _rend.material.SetFloat("_Smoothness",newSmoothVal);
            
            float newBumpVal = Mathf.Lerp(0.0f, bumpScaleOriginalList[transitions], timer / fade2WhiteDuration);
            _rend.material.SetFloat("_BumpScale", newBumpVal);
            
            float newOccVal = Mathf.Lerp(0.0f, occlusionOriginalList[transitions], timer / fade2WhiteDuration);
            _rend.material.SetFloat("_OcclusionStrength",newOccVal);
            
            float newMetallVal = Mathf.Lerp(0.0f, metallicOriginalList[transitions], timer / fade2WhiteDuration);
            _rend.material.SetFloat("_Metallic", newMetallVal);
            
            
            timer += Time.deltaTime;
            yield return null;
        }
    
    
        // make sure final values are reached
        Color newColor2 = fadeColor;
        newColor2 = Color.white;
            
        if (transitions== 2)
        {
            newColor2.a = glowAlphaOriginal;
        }
        _rend.material.color = newColor2;
        _rend.material.SetFloat("_Smoothness",smoothnessOriginalList[transitions]);
        _rend.material.SetFloat("_BumpScale", bumpScaleOriginalList[transitions]);
        _rend.material.SetFloat("_OcclusionStrength",occlusionOriginalList[transitions]);
        _rend.material.SetFloat("_Metallic", metallicOriginalList[transitions]);
    
        yield return null;
    }
    
    public void Return2DefaultState4NewParticipant()
    {
        for (int i = 0; i < 3; i++)
        {
            Color newColor = fadeColor;
            newColor = Color.white;
            
            if (i == 2)
            {
                newColor.a = glowAlphaOriginal;
            }
          
            newMats[i].color = newColor;
            newMats[i].SetFloat("_Smoothness", smoothnessOriginalList[i]);
            newMats[i].SetFloat("_BumpScale",  bumpScaleOriginalList[i]);
            newMats[i].SetFloat("_OcclusionStrength", occlusionOriginalList[i]);
            newMats[i].SetFloat("_Metallic", metallicOriginalList[i]);
        }

        ChangeLiveMat(0);
        transitions = 0;
        upwards = true;
        
        
    }

    public void OnApplicationQuit()
    {
        Return2DefaultState4NewParticipant();
    }

    #region Unused Transition Effects
    // private IEnumerator ShrinkModel()
    // {
    //     float timer = 0.0f;
    //     
    //     while (timer <= shrinkingDuration)
    //     {
    //         
    //         float shrinkVal = Mathf.Lerp(1, 0, timer / shrinkingDuration);
    //         this.GetComponent<Transform>().localScale = new Vector3(shrinkVal, shrinkVal, shrinkVal);
    //         timer += Time.deltaTime;
    //         yield return null;
    //     }
    //     
    //     this.GetComponent<Transform>().localScale = new Vector3(0, 0, 0);
    //     
    //     yield return null;
    // }
    //
    // private IEnumerator GrowModel()
    // {
    //     float timer = 0.0f;
    //     
    //     while (timer <= growingDuration)
    //     {
    //         
    //         float shrinkVal = Mathf.Lerp(0, 1, timer / growingDuration);
    //         this.GetComponent<Transform>().localScale = new Vector3(shrinkVal, shrinkVal, shrinkVal);
    //         timer += Time.deltaTime;
    //         yield return null;
    //     }
    //     
    //     this.GetComponent<Transform>().localScale = new Vector3(1, 1, 1);
    //     
    //     yield return null;
    // }
    
    // private IEnumerator SwitchMaterials()
    // {
    //     float timer = 0.0f;
    //     
    //     while (timer <= fadeBlackDuration)
    //     {
    //
    //         // Material newColor = objectColor;
    //         // newColor.Lerp(objectColor, objectGlow, timer / fadeBlackDuration);
    //         // _rend.material = newColor;
    //         
    //         timer += Time.deltaTime;
    //         yield return null;
    //     }
    //
    //
    //     // _rend.material = objectGlow;
    // }
    //
    //
    
    
    // private IEnumerator FadeOutMaterial()
    // {
    //     float timer = 0.0f;
    //     float hue;
    //     float saturation;
    //     float value;
    //
    //
    //     Color.RGBToHSV(placeholderMat.color, out hue, out saturation, out value);
    //     
    //     
    //     while (timer <= fadeBlackDuration)
    //     {
    //         var newValue = Mathf.Lerp(100, 0, timer / fadeBlackDuration);
    //         Color newColor = Color.HSVToRGB(hue, saturation, newValue);
    //         
    //         // newColor = Color.HSVToRGB()
    //         placeholderMat.SetColor("_Color", newColor);
    //             
    //         timer += Time.deltaTime;
    //         yield return null;
    //     }
    //         
    //     Color newColor2 = fadeColor;
    //     newColor2 = Color.black;
    //             
    //     placeholderMat.SetColor("_Color", newColor2);
    //
    // }
    //
    // private IEnumerator FadeBackMaterial()
    // {
    //     float timer = 0.0f;
    //     float hue;
    //     float saturation;
    //     float value;
    //
    //
    //     Color.RGBToHSV(placeholderMat.color, out hue, out saturation, out value);
    //
    //
    //     while (timer <= fadeBack2Normal)
    //     {
    //         var newValue = Mathf.Lerp(0, 100, timer / fadeBlackDuration);
    //         Color newColor = Color.HSVToRGB(hue, saturation, newValue);
    //
    //         // newColor = Color.HSVToRGB()
    //         placeholderMat.SetColor("_Color", newColor);
    //
    //         timer += Time.deltaTime;
    //         yield return null;
    //     }
    //
    //     Color newColor2 = fadeColor;
    //     newColor2 = Color.white;
    //
    //     placeholderMat.SetColor("_Color", newColor2);
    //     yield return null;
    // }
    
    // private IEnumerator FadeOutRoutine()
    // {
    //     float timer = 0.0f;
    //     while (timer <= fadeOutDuration)
    //     {
    //         Color newColor = fadeColor;
    //         newColor.a = Mathf.Lerp(1, 0, timer / fadeOutDuration);
    //         _rend.material.SetColor("_Color", newColor);
    //         
    //         timer += Time.deltaTime;
    //         yield return null;
    //     }
    //     
    //     Color newColor2 = fadeColor;
    //     newColor2.a = 0;
    //         
    //     _rend.material.SetColor("_Color", newColor2);
    //     
    // }
    //
    // private IEnumerator FadeInRoutine()
    // {
    //     float timer = 0.0f;
    //     while (timer <= fadeInDuration)
    //     {
    //         Color newColor = fadeColor;
    //         newColor.a = Mathf.Lerp(0, 1, timer / fadeInDuration);
    //         _rend.material.SetColor("_Color", newColor);
    //         
    //         timer += Time.deltaTime;
    //         yield return null;
    //     }
    //     
    //     Color newColor2 = fadeColor;
    //     newColor2.a = 1;
    //         
    //     _rend.material.SetColor("_Color", newColor2);
    //     
    // }

    #endregion
}
