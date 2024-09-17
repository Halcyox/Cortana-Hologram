using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Glasses : MonoBehaviour
{
    #region Variables
    public GameObject glassesObj;
    public float dissolveSpd = 2.0f;
    public GameObject particles;

    private Material dissolveMat;
    #endregion

    void Start()
    {
        //dissolveMat = glassesObj.GetComponent<Renderer>().material;
    }

    void Update()
    {
        
    }

    public void Disappear(float dissolveSpd)
    {
        dissolveMat.SetFloat("_DissolveAmount", Mathf.MoveTowards(dissolveMat.GetFloat("_DissolveAmount"), 1.25f, dissolveSpd * Time.deltaTime));
        particles.SetActive(true);
    }

    public void Reappear(float dissolveSpd)
    {
        dissolveMat.SetFloat("_DissolveAmount", Mathf.MoveTowards(dissolveMat.GetFloat("_DissolveAmount"), -0.25f, dissolveSpd * Time.deltaTime));
        particles.SetActive(false);
    }
}
