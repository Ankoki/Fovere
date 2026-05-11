using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace TMPro
{
    [System.Serializable]
    public class ActionEvent : UnityEvent<string>
    {
    }

    [System.Serializable]
    public class TextRevealEvent : UnityEvent<char>
    {
    }

    [System.Serializable]
    public class DialogueFinishEvent : UnityEvent
    {
    }

    /// <summary>
    /// Class that extends from TextPro to be able to create our own animated dialogue.
    /// </summary>
    public class TMP_Animated : TextMeshProUGUI
    {
        
        /// <summary>
        /// Method to check if a given tag is of a custom type, not built into TMP.
        /// </summary>
        /// <param name="tag">The tag to check.</param>
        /// <returns>True if custom, else false.</returns>
        private static bool IsCustomTag(string tag)
        {
            return tag.StartsWith("speed=") || tag.StartsWith("pause=") || tag.StartsWith("action=");
        }
        
        [SerializeField] private float speed = 10;
        private float speedModifier = 1;
        public ActionEvent onAction;
        public TextRevealEvent onReveal;
        public DialogueFinishEvent onDialogueFinish;

        /// <summary>
        /// Reads the given text to the dialogue.
        /// </summary>
        /// <param name="text">The text to </param>
        public void ReadText(string text)
        {
            text = string.Empty;
            var subTexts = text.Split('<', '>');
            var displayText = "";
            for (var i = 0; i < subTexts.Length; i++)
            {
                if (i % 2 == 0)
                    displayText += subTexts[i];
                else if (!IsCustomTag(subTexts[i].Replace(" ", "")))
                    displayText += $"<{subTexts[i]}>";
            }

            text = displayText;
            maxVisibleCharacters = 0;
            speedModifier = 1;
            StartCoroutine(Read(subTexts));
            onDialogueFinish.Invoke();
        }
        
        /// <summary>
        /// Enumerator used to read all text in the dialogue and parse all custom tags.
        /// </summary>
        /// <param name="subTexts">The subtexts of this dialogue.</param>
        /// <returns>Wait or null on completion.</returns>
        private IEnumerator Read(string[] subTexts)
        {
            var subCounter = 0;
            var visibleCounter = 0;
            while (subCounter < subTexts.Length)
            {
                if (subCounter % 2 == 1)
                    yield return EvaluateTag(subTexts[subCounter].Replace(" ", ""));
                else
                {
                    while (visibleCounter < subTexts[subCounter].Length)
                    {
                        onReveal.Invoke(subTexts[subCounter][visibleCounter]);
                        visibleCounter++;
                        maxVisibleCharacters++;
                        yield return new WaitForSeconds(1f / speed * speedModifier);
                    }
                    visibleCounter = 0;
                }
                subCounter++;
            }
            yield return null;
        }
        
        /// <summary>
        /// Speeds up the current dialogue to double the speed.
        /// </summary>
        public void SpeedUp()
        {
            speedModifier = 2;
        }

        /// <summary>
        /// Evaluates our custom tags.
        /// </summary>
        /// <param name="tag">The tag to evaluate.</param>
        /// <returns>WaitForSeconds if pause tag </returns>
        private WaitForSeconds EvaluateTag(string tag)
        {
            if (tag.Length > 0 && tag.StartsWith("pause="))
                return new WaitForSeconds(float.Parse(tag.Split('=')[1]));
            if (tag.Length <= 0)
                return null;
            if (tag.StartsWith("speed="))
                speed = float.Parse(tag.Split("=")[1]);
            else if (tag.StartsWith("action="))
                onAction.Invoke(tag.Split("=")[1]);
            return null;
        }

    }
}