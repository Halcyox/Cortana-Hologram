using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Character to be seen during the experience.
/// </summary>
public class Cortana : MonoBehaviour
{
    /// <summary>
    /// Singleton object of the Character.
    /// We create and set the instance of this singleton before the 1st frame in Awake().
    /// </summary>
    public static Cortana Instance { get; protected set; }

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(this.gameObject);
            return;
        }
        else
        {
            Instance = this;
        }
    }

    #region Variables
    #region Cortana
    [Header("Cortana Properties")]
    public GameObject CortanaObj;
    public GameObject particles;
    public GameObject glasses;
    public AudioClip[] audioClips;
    [SerializeField]
    protected string startAnimName;
    [SerializeField]
    protected string endAnimName;
    #endregion

    private Renderer[] renderers;
    public Animator anim;
    #endregion

    /// <summary>
    /// Sets various variables on the 1st frame.
    /// </summary>
    void Start()
    {
        renderers = CortanaObj.GetComponentsInChildren<Renderer>();
        anim = CortanaObj.GetComponent<Animator>();
        glasses.SetActive(false);
    }


    #region Materials
    /// <summary>
    /// Fades the character model out over a period of time, based on the dissolve speed over time.
    /// </summary>
    /// <param name="dissolveSpd">Speed at which the model dissolves.</param>
    public void Disappear(float dissolveSpd)
    {
        foreach (Renderer rend in renderers)
        {
            // Check if material has the float in the 1st place

            rend.material.SetFloat("_DissolveAmount", Mathf.MoveTowards(rend.material.GetFloat("_DissolveAmount"), 1.25f, dissolveSpd * Time.deltaTime));
        }

        particles.SetActive(true);
    }

    /// <summary>
    /// Fades the character model in over a period of time, based on the dissolve speed over time.
    /// </summary>
    /// <param name="dissolveSpd">Speed at which the model dissolves.</param>
    public void Reappear(float dissolveSpd)
    {
        foreach (Renderer rend in renderers)
        {
            rend.material.SetFloat("_DissolveAmount", Mathf.MoveTowards(rend.material.GetFloat("_DissolveAmount"), -0.25f, dissolveSpd * Time.deltaTime));
        }

        particles.SetActive(false);
    }
    #endregion

    #region Animation & Audio
    /// <summary>
    /// Plays the character animations from the AnimationController based on the current TriggerState.
    /// Turns glasses on the character on.
    /// </summary>
    public void PlayAnim()
    {
        if (GameManager.Instance.currentState == TriggerState.isInSceneToStart ||
            GameManager.Instance.currentState == TriggerState.hasPutOnGlasses)
        {
            anim.SetTrigger("NextAnim");
        }

        if (GameManager.Instance.currentState == TriggerState.hasPutOnGlasses)
        {
            glasses.SetActive(true);
        }

        if (GameManager.Instance.currentState == TriggerState.isReadyToStart)
        {
            anim.SetTrigger("NextYes");
        }
        if (GameManager.Instance.currentState == TriggerState.isNotReadyToStart)
        {
            anim.SetTrigger("NextNo");
        }
    }

    /// <summary>
    /// Plays audio clips based on the current TriggerState.
    /// </summary>
    /// <param name="audio">Audio component, inherited from the GameManager.</param>
    public void PlayAudio(AudioSource audio)
    {
        switch (GameManager.Instance.currentState)
        {
            case TriggerState.isInSceneToStart:
                // hands in scene
                audio.clip = audioClips[0];
                break;
            case TriggerState.hasNotPutOnGlasses:
                // hasn't found BCI
                audio.clip = audioClips[1];
                break;
            case TriggerState.hasPutOnGlasses:
                // chess line, ready to start
                audio.clip = audioClips[2];
                break;
            case TriggerState.isReadyToStart:
                // welcome home, want to do the experience again
                audio.clip = audioClips[3];
                break;
            case TriggerState.isNotReadyToStart:
                // try again
                audio.clip = audioClips[4];
                break;
            case TriggerState.willTryAgain:
                // play again
                audio.clip = audioClips[5];
                break;
            case TriggerState.wontTryAgain:
                // leave
                audio.clip = audioClips[6];
                break;
            default:
                break;
        }

        audio.Play();
    }

    /// <summary>
    /// Loops the character animations back to the 1st.
    /// </summary>
    public void loopBackToIdles()
    {
        if (anim.GetCurrentAnimatorStateInfo(0).IsName(endAnimName))
        {
            //Disappear(0.5f);
        }
        if (anim.GetNextAnimatorStateInfo(0).IsName(startAnimName))
        {
            CortanaObj.transform.position = new Vector3(0, 0, 0);
            //Reappear(0.5f);
        }
    }
    #endregion
}
