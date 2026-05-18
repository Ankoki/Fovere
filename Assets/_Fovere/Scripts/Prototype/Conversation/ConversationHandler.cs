using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Fovere
{
    public class ConversationHandler : MonoBehaviour
    {
        public bool inDialogue;

        public static ConversationHandler Instance;

        public PrototypePlayerController player; // Should be relocated, doesn't belong here.
        public TMP_Animated animatedText;
        public Image nameBubble;
        public TextMeshProUGUI nameText;
        public Image continuePointer;

        [HideInInspector] public PrototypeResidentHandler currentPrototypeResident;
        [HideInInspector] public DialogueData currentDialogue;

        private CanvasGroup _canvasGroup;
        public int dialogueIndex;
        public bool canExit;
        public bool nextDialogue;
        public bool canSpeedUp = true;

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

        private void OnEnable()
        {
            animatedText.OnDialogueFinish += FinishDialogue;
            continuePointer.gameObject.SetActive(false);
        }

        private void OnDisable()
        {
            animatedText.OnDialogueFinish -= FinishDialogue;
        }

        /// <summary>
        /// Sets the colours of all bubble components to the residents.
        /// </summary>
        public void SetupBubble()
        {
            if (currentPrototypeResident != null)
            {
                nameText.text = currentPrototypeResident.data.residentName;
                nameText.color = currentPrototypeResident.data.invertColour;
                nameBubble.color = currentPrototypeResident.data.residentColour;
            }
            else
            {
                nameText.text = "Fovēre"; // This should be a global constant key. 
                nameText.color = Color.white;
                nameBubble.color = Color.goldenRod;
            }
        }

        /// <summary>
        /// Clears the conversation text.
        /// </summary>
        public void ClearText()
        {
            animatedText.text = string.Empty;
        }

        /// <summary>
        /// Starts a conversation between a resident and a player.
        /// </summary>
        /// <param name="resident">The resident to speak too.</param>
        /// <param name="player">The player speaking.</param>
        public void StartConversation(PrototypeResidentHandler resident, PrototypePlayerController player) // Keep here, field player will be relocated.
        {
            if (inDialogue)
                return;
            currentDialogue = resident.dialogue;
            resident.Freeze();
            resident.isTalking = true;
            player.IsFrozen = true;
            var movement = player.GetComponentInParent<PrototypeMovementController>();
            movement.TurnTo(resident.transform.position);
            resident.TurnTo(player.transform.position);
            // TODO zoom in and lock camera? maybe not though.
            currentPrototypeResident = resident;
            inDialogue = true;
            SetupBubble();
            ClearText();
            canSpeedUp = false;
            FadeUI(true, 0.5f, 0);
        }

        /// <summary>
        /// Shows a dialogue to a player. Does not include a resident.
        /// </summary>
        /// <param name="data">The dialogue data.</param>
        /// <param name="player">The player to display too.</param>
        public void ShowDialogue(DialogueData data, PrototypePlayerController player)
        {
            player.IsFrozen = true;
            currentDialogue = data;
            inDialogue = true;
            currentPrototypeResident = null;
            SetupBubble();
            ClearText();
            canSpeedUp = false;
            FadeUI(true, 0.5f, 0);
        }

        /// <summary>
        /// Reset the conversation. This should be called at the end.
        /// </summary>
        public void EndConversation()
        {
            if (currentPrototypeResident != null)
            {
                currentPrototypeResident.Idle();
                currentPrototypeResident.isTalking = false;
            }
            currentDialogue = null;
            inDialogue = false;
            canExit = false;
            dialogueIndex = 0;
            animatedText.enabled = true;
            player.IsFrozen = false;
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
            dialogueIndex = 0;
            sequence.Join(_canvasGroup.transform.DOScale(0, time * 2).From().SetEase(Ease.OutBack));
            sequence.AppendCallback(() => animatedText.ReadText(currentDialogue.conversationBlock[0]));
        }
        
        /// <summary>
        /// Advances the current dialogue.
        /// </summary>
        public void Advance()
        {
            if (!inDialogue)
                return;
            continuePointer.gameObject.SetActive(false);
            if (canExit)
            {
                canExit = false;
                FadeUI(false, 0.2f, 0);
                var sequence = DOTween.Sequence();
                sequence.AppendInterval(0.8f);
                sequence.AppendCallback(EndConversation);
                return;
            }
            if (nextDialogue)
            {
                nextDialogue = false;
                canSpeedUp = false;
                animatedText.ReadText(currentDialogue.conversationBlock[dialogueIndex]);
                return;
            }

            if (!canSpeedUp) 
                return;
            canSpeedUp = false;
            animatedText.SpeedUp();
        }

        /// <summary>
        /// Calls for the current dialogue to finish.
        /// This will call the following if present in the conversation.
        /// </summary>
        private void FinishDialogue()
        {
            if (dialogueIndex < currentDialogue.conversationBlock.Count - 1)
            {
                dialogueIndex++;
                nextDialogue = true;
            }
            else
            {
                nextDialogue = false;
                canExit = true;
            }
            continuePointer.gameObject.SetActive(true);
            currentPrototypeResident?.ResetSpeech();
        }

        private void OnApplicationQuit()
        {
            if (Instance != null)
                Destroy(Instance.gameObject);
        }
        
    }
}