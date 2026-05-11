using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConversationHandler : MonoBehaviour
{
    public bool inDialogue;

    public static ConversationHandler Instance;

    public TMP_Animated animatedText;
    public Image nameBubble;
    public TextMeshProUGUI nameText;

    [HideInInspector] public ResidentHandler currentResident;

    private CanvasGroup _canvasGroup;
    private int _dialogueIndex;
    public bool canExit;
    public bool nextDialogue;
    private bool _canSpeedUp = true;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    private void Start()
    {
        animatedText.onDialogueFinish.AddListener(() => FinishDialogue());
    }

    private void Update()
    {
        if (inDialogue) // TODO ON CONTINUE PRESSED. 
        {
            if (canExit)
            {
                
                FadeUI(false, .2f, 0);
                var sequence = DOTween.Sequence();
                sequence.AppendInterval(0.8f);
                sequence.AppendCallback(() => ResetState());
            }
            else if (nextDialogue)
            {
                _canSpeedUp = true;
                animatedText.ReadText(currentResident.dialogue.conversationBlock[_dialogueIndex]);
            }
            else if (_canSpeedUp)
            {
                _canSpeedUp = false;
                animatedText.SpeedUp();
            }
        }
    }

    /// <summary>
    /// Sets the colours of all bubble components to the residents.
    /// </summary>
    public void SetupBubble()
    {
        nameText.text = currentResident.data.residentName;
        nameText.color = Color.white;
        nameBubble.color = currentResident.data.residentColour;
    }

    /// <summary>
    /// Clears the conversation text.
    /// </summary>
    public void ClearText()
    {
        animatedText.text = string.Empty;
    }

    /// <summary>
    /// Reset the conversation. This should be called at the end.
    /// </summary>
    public void ResetState()
    {
        currentResident.Idle();
        inDialogue = false;
        canExit = false;
        // TODO unfreeze player
    }

    /// <summary>
    /// Fades out the UI.
    /// </summary>
    /// <param name="show">Whether to show the fade or not.</param>
    /// <param name="time">The time the fade should take.</param>
    /// <param name="delay">The delay before fading.</param>
    public void FadeUI(bool show, float time, float delay)
    {
        var sequence = DOTween.Sequence();
        sequence.AppendInterval(delay);
        sequence.Append(_canvasGroup.DOFade(show ? 1 : 0, time));
        if (!show)
            return;
        _dialogueIndex = 0;
        sequence.Join(_canvasGroup.transform.DOScale(0, time * 2).From().SetEase(Ease.OutBack));
        sequence.AppendCallback(() => animatedText.ReadText(currentResident.dialogue.conversationBlock[0]));
    }

    /// <summary>
    /// Calls for the current dialogue to finish.
    /// This will call the following if present in the conversation.
    /// </summary>
    private void FinishDialogue()
    {
        if (_dialogueIndex < currentResident.dialogue.conversationBlock.Count - 1)
        {
            _dialogueIndex++;
            nextDialogue = true;
        }
        else
        {
            nextDialogue = false;
            canExit = true;
        }
    }
    
    private void OnApplicationQuit()
    {
        if (Instance != null)
            Destroy(Instance.gameObject);
    }
    
}