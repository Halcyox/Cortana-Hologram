using System.Collections;
using System.Collections.Generic;
using System.Net.WebSockets;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.Events;
using System;

/// <summary>
/// Tells us what current state of the experience we're in.
/// </summary>
public enum TriggerState
{
    none,
    isInSceneToStart,
    hasPutOnGlasses,
    hasNotPutOnGlasses,
    isReadyToStart,
    isNotReadyToStart,
    willTryAgain,
    wontTryAgain
}

/// <summary>
/// Logic handler for the entire experience.
/// </summary>
public class GameManager : MonoBehaviour
{
    /// <summary>
    /// Singleton object of the GameManager.
    /// We create and set the instance of this singleton before the 1st frame in Awake().
    /// </summary>
    public static GameManager Instance { get; protected set; }

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
    #region Character
    [Header("Character Properties")]
    public GameObject CortanaPrefab;
    public GameObject GlassesPrefab;
    #endregion

    #region Hands
    [Header("Hands Properties")]
    public GameObject handL;
    public GameObject handLPalm;
    public GameObject handR;
    public GameObject handRPalm;
    #endregion

    #region Controllers
    [Header("Controller Properties")]
    [SerializeField]
    private float dissolveSpd = 2.0f;
    public GameObject displayMsgTxt;
    [SerializeField]
    private float timeDuration = 20.0f;
    [SerializeField]
    private float delayDuration = 1.0f;
    [SerializeField]
    private float timer;
    [SerializeField]
    private float delayTimer;

    [Header("Game Logic Flags Properties")]
    public TriggerState currentState;
    #endregion

    // "m_" specifier is a denotation in Unity in order to not use names of inherited members
    // It'd still work fine without it, but I'm sure no one wants Unity to needlessly show a squiggle
    //      or look angry for no good reason
    private AudioSource m_audio;
    private bool eventFired = false;
    #endregion

    /// <summary>
    /// Sets various variables on the 1st frame.
    /// </summary>
    void Start()
    {
        currentState = TriggerState.none;
        ResetTimer();

        m_audio = GetComponent<AudioSource>();

        CortanaPrefab = Instantiate(CortanaPrefab);
        GlassesPrefab.SetActive(false); }

    void Update()
    {
        Cortana.Instance.loopBackToIdles();
        HandsOutOfFocus();
        AudioCheckers();
    }

    #region Hand Reactions
    /// <summary>
    /// Hides character and pauses audio if hands are outside the camera's view.
    /// Called on the blocking volumes in the level, from the InteractionBehavior component in the ContactStay() event.
    /// </summary>
    public void OnInteracted()
    {
        //m_audio.Pause();
        displayMsgTxt.GetComponent<TextMeshPro>().text = "Hands are out of view";
        displayMsgTxt.SetActive(true);
        //Cortana.Instance.Disappear(dissolveSpd);
    }

    /// <summary>
    /// Reveals character and unpauses audio if hands are inside the camera's view.
    /// Called on the blocking volumes in the level, from the InteractionBehavior component in the ContactLeave() event.
    /// </summary>
    public void OnExit()
    {
        //m_audio.UnPause();
        displayMsgTxt.SetActive(false);
        //Cortana.Instance.Reappear(dissolveSpd);
    }

    /// <summary>
    /// Checks if the hands are outside the camera's view.
    /// </summary>
    public void HandsOutOfFocus()
    {
        if ((!handL.activeSelf || !handR.activeSelf))
        {
            //CortanaPrefab.SetActive(false);
            //Cortana.Instance.Disappear(dissolveSpd);
            TeleportCharacterToOrigin();
        }
        else
        {
            //CortanaPrefab.SetActive(true);
            //Cortana.Instance.Reappear(dissolveSpd);
            //Cortana.Instance.Disappear(dissolveSpd);
            TeleportCharacterToHand();
        }

        if ((handL.activeSelf && handR.activeSelf))
        {
            // hands are in scene initially
            if (currentState == TriggerState.none)
            {
                ChangeState(TriggerState.isInSceneToStart);
                //Cortana.Instance.Disappear(dissolveSpd);
                StartCoroutine(SetIsInScene());
            }
        }
    }

    /// <summary>
    /// Teleports and reparents the character to the hand.
    /// </summary>
    public void TeleportCharacterToHand()
    {
        CortanaPrefab.transform.parent = handLPalm.transform;
        CortanaPrefab.transform.localPosition = new Vector3(0, 0, 0);
        Cortana.Instance.CortanaObj.transform.localPosition = new Vector3(0, 0.01f, 0);
        CortanaPrefab.transform.eulerAngles = new Vector3(handLPalm.transform.eulerAngles.x, handLPalm.transform.eulerAngles.y, handLPalm.transform.eulerAngles.z + 180);
        // makes sure that the character's animation keeps them in-place
        Cortana.Instance.anim.applyRootMotion = false;
        Cortana.Instance.Reappear(dissolveSpd);
    }

    /// <summary>
    /// Teleports and reparents the character to the root of the scene.
    /// </summary>
    public void TeleportCharacterToOrigin()
    {
        CortanaPrefab.transform.parent = null;
        CortanaPrefab.transform.localPosition = new Vector3(0, 0, 0);
        Cortana.Instance.CortanaObj.transform.localPosition = new Vector3(0, 0, 0);
        CortanaPrefab.transform.eulerAngles = new Vector3(0, 0, 0);
        // makes sure that the character's animation keeps them in-place
        Cortana.Instance.anim.applyRootMotion = false;
        Cortana.Instance.Reappear(dissolveSpd);
    }
    #endregion

    #region Experience Drivers
    #region General Functions
    /// <summary>
    /// Changes the TriggerState.
    /// </summary>
    /// <param name="newState">TriggerState to be changed.</param>
    public void ChangeState(TriggerState newState)
    {
        currentState = newState;

        Cortana.Instance.PlayAnim();
        Cortana.Instance.PlayAudio(m_audio);
    }

    /// <summary>
    /// Yes answer. 
    /// Changes state, plays the respective animation and audio.
    /// </summary>
    /// <param name="newState">TriggerState to be changed.</param>
    public void OnYes(TriggerState newState)
    {
        ChangeState(newState);
        ResetTimer();
    }

    /// <summary>
    /// No answer. 
    /// Changes state, plays the respective animation and audio.
    /// </summary>
    /// <param name="newState">TriggerState to be changed.</param>
    public void OnNot(TriggerState newState)
    {
        ChangeState(newState);
        ResetTimer();
    }
    #endregion

    #region Experience Coroutines
    /// <summary>
    /// Begins the interaction portion of the experience.
    /// Transforms the character to appear on the left hand palm, plays the respective animation and audio.
    /// </summary>
    /// <returns>End of the frame.</returns>
    IEnumerator SetIsInScene()
    {

        if (currentState == TriggerState.isInSceneToStart)
        {
            yield return new WaitForEndOfFrame();

            TeleportCharacterToHand();
            GlassesPrefab.SetActive(true);
            if (!displayMsgTxt.activeInHierarchy) displayMsgTxt.SetActive(true);
            displayMsgTxt.GetComponent<TextMeshPro>().text = "Please put on the glasses";
        }
    }

    /// <summary>
    /// Checks to see if the user has put on the glasses.
    /// Plays the respective animation and audio.    
    /// </summary>
    /// <returns>End of the frame.</returns>
    IEnumerator SetAreYouReady()
    {
        if (GetComponent<ConnectionWebSocket>().BciConnected)
        {
            yield return new WaitForEndOfFrame();

            ChangeState(TriggerState.hasPutOnGlasses);
            AreYouReady();
        }
    }
    #endregion

    #region Experience Functions
    /// <summary>
    /// Checks if the user is ready to start.
    /// </summary>
    void AreYouReady()
    {
        GlassesPrefab.SetActive(false);
        displayMsgTxt.SetActive(false);

        ExpectAnswerFromUser(answer =>
        {
            switch (answer)
            {
                case "yes":
                    OnYes(TriggerState.isReadyToStart);
                    AskUserToRetry();
                    break;
                case "no":
                    OnNot(TriggerState.isNotReadyToStart);
                    break;
            }
        });
    }

    /// <summary>
    /// Asks if the user wants to retry the experience.
    /// A bit of a redux of the ExpectAnswerfromUser() function, but for capping the experience.
    /// </summary>
    public void AskUserToRetry()
    {
        ExpectAnswerFromUser(answer =>
        {
            switch (answer)
            {
                case "yes":
                    ChangeState(TriggerState.willTryAgain);
                    break;
                case "no":
                    ChangeState(TriggerState.wontTryAgain);
                    break;
            }
        });
    }
    #endregion
    #endregion

    #region Condition Checkers
    #region General Checks
    /// <summary>
    /// Checks what actions to perform based on if the audio is done playing and what the current state is.
    /// </summary>
    public void AudioCheckers()
    {
        if (m_audio.isPlaying) return;

        switch (currentState)
        {
            case TriggerState.isInSceneToStart:
                //ChangeState(TriggerState.hasNotPutOnGlasses);
                // set state directly so we don't necessarily play the audio/anim
                currentState = TriggerState.hasNotPutOnGlasses;
                break;
            case TriggerState.hasNotPutOnGlasses:
                //CountdownTimer(TriggerState.hasNotPutOnGlasses);
                StartCoroutine(SetAreYouReady());
                break;
            case TriggerState.hasPutOnGlasses:
                //StartCoroutine(AddDelayBetweenStates(10, TriggerState.isNotReadyToStart));
                CountdownTimer(TriggerState.isNotReadyToStart);
                break;
            case TriggerState.isReadyToStart:
                CountdownTimer(TriggerState.isNotReadyToStart);
                break;
            case TriggerState.isNotReadyToStart:
                CountdownTimer(TriggerState.isNotReadyToStart);
                break;
            case TriggerState.willTryAgain:
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                break;
            case TriggerState.wontTryAgain:
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #endif
                //Application.Quit();
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                break;
            default:
                break;
        }
    }
    #endregion

    #region Experience Checks
    /// <summary>
    /// Run one round of user interaction.
    /// </summary>
    /// <returns>Answer user responded with.</returns>
    void ExpectAnswerFromUser(Action<string> answer)
    {
        eventFired = false;
        GetComponent<ConnectionWebSocket>().AnswerEvent.AddListener(ans =>
        {
            eventFired = true;
            answer.Invoke(ans);
        });
    }
    #endregion
    #endregion

    #region Timers
    /// <summary>
    /// Timer that counts down to an answer.
    /// </summary>
    public void CountdownTimer(TriggerState notState)
    {
        if (delayTimer > 0)
        {
            delayTimer -= Time.deltaTime;
        }
        else
        {
            delayTimer = 0;

            if (timer == timeDuration && GetComponent<ConnectionWebSocket>().BciConnected)
            {
                GetComponent<ConnectionWebSocket>().SendStart();
            }

            if (timer > 0)
            {
                timer -= Time.deltaTime;
                int currTime = Mathf.RoundToInt(timer);
                displayMsgTxt.SetActive(true);

                if (timer > 10)
                {
                    displayMsgTxt.GetComponent<TextMeshPro>().text = "Yes - " + currTime.ToString();
                }
                if (timer <= 10)
                {
                    displayMsgTxt.GetComponent<TextMeshPro>().text = "No - " + currTime.ToString();
                }
            }
            else if (timer <= 0.05)
            {
                OnNot(notState);
                //StartCoroutine(Timeout());
            }
        }
    }

    /// <summary>
    /// Resets the timer and hides the display messages.
    /// </summary>
    public void ResetTimer()
    {
        timer = timeDuration;
        delayTimer = delayDuration;
        displayMsgTxt.SetActive(false);
    }

    /// <summary>
    /// Coroutine that lets us know that we timed out.
    /// </summary>
    /// <returns>Time left until timeout.</returns>
    IEnumerator Timeout()
    {
        ResetTimer();
        yield return new WaitForSeconds(timeDuration);
        if (!eventFired)
        {
            // no longer interested in the answer, remove it.
            Debug.Log("Timed out waiting for answer!");
            GetComponent<ConnectionWebSocket>().AnswerEvent.RemoveAllListeners();
        }
    }

    /// <summary>
    /// Coroutine that adds an additional delay between given states.
    /// </summary>
    /// <param name="additionalDelay">Additional delay time.</param>
    /// <param name="notState">Not state to be passed in for the CountdownTimer().</param>
    /// <returns></returns>
    IEnumerator AddDelayBetweenStates(float additionalDelay, TriggerState notState)
    {
        yield return new WaitForSeconds(additionalDelay);

        CountdownTimer(notState);
    }
    #endregion
}
