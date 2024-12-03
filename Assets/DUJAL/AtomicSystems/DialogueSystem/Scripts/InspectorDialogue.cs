
namespace DUJAL.Systems.Dialogue
{
    using DUJAL.MovementComponents;
    using DUJAL.Systems.Audio;
    using DUJAL.Systems.Utils;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using TMPro;
    using UnityEngine.Events;
    using UnityEngine.UI;
    using System;
    using DUJAL.Systems.Dialogue.Constants;
    using UnityEngine.UIElements;

    public class InspectorDialogue : MonoBehaviour
    {
        //SO
        [SerializeField] private DialogueContainerScriptableObject _dialogueContainerSO;
        [SerializeField] private GroupScriptableObject _groupSO;
        [SerializeField] private DialogueScriptableObject _dialogueSO;
        [SerializeField] private DialogueScriptableObject _currentPlayedDialogue;
        //filter
        [SerializeField] private bool _startingDialogueFilter;
        [SerializeField] private bool _groupedDialogueFilter;

        //index
        [SerializeField] private int _selectedGroup;
        [SerializeField] private int _selectedDialogue;

        //Settings
        [SerializeField] private AudioStyle _audioStyle;
        [SerializeField][Range(0, 1)] private float _textSpeed;
        [SerializeField][Range(0, 10)] private int _maxLineCount;
        [SerializeField] private bool _autoText;
        [SerializeField] private bool _isStartingDialogue;

        //UI Data
        [SerializeField] private CanvasGroup _dialogueCanvasGroup;
        [SerializeField] private TextMeshProUGUI _text;
        [SerializeField] private UnityEngine.UI.Image _speakerImage;
        [SerializeField] private List<UnityEngine.UI.Button> _choiceButtons;
        [SerializeField] private InspectorDialogue _nextDialogueObject;

        [SerializeField] public UnityEvent Enter = new UnityEvent();
        [SerializeField] public UnityEvent Exit = new UnityEvent();
        [SerializeField] public UnityEvent LetterRevealed = new UnityEvent();
        [SerializeField] public UnityEvent OnTextboxFull = new UnityEvent();
        [SerializeField] public UnityEvent OnTextBoxPassed = new UnityEvent();
        [SerializeField] public UnityEvent OnTextSkipped = new UnityEvent();

        [HideInInspector] public List<EffectInstance> EffectInstances = new();

        private int _currentChoiceIndex;

        private float _previousTextSpeed;

        private bool _performNextDialogue;
        private bool _waitingForPerformNextDialogue;
        private bool _previousDialogueAutoText;

        private DialogueInputActions _dialogueInputActions;
        private TextAnimatorInspector _animationHandler;


        private void Awake()
        {
            _animationHandler = GetComponent<TextAnimatorInspector>();
            if (_isStartingDialogue) _currentPlayedDialogue = DialogueScriptableObject.CopyInto(_dialogueSO, _currentPlayedDialogue);
            ToggleIndividualChoiceButtonVisibility(false);

            Enter.AddListener(HandleMultipleChoiceButtons);
            Exit.AddListener(HandleTextboxEnd);

            AssignAudioType();

            _currentChoiceIndex = 0;
            HandleInput();
            OpenDialogueObject();
        }

        private void HandleInput()
        {
            for (int i = 0; i < _choiceButtons.Count; i++)
            {
                int x = i;
                _choiceButtons[i].onClick.AddListener(() => TriggerSelectChoice((DialogueChoice)x));
            }

            _dialogueInputActions = new DialogueInputActions();

            _dialogueInputActions.TextBoxActionMap.NextDialogue.performed += ctx =>
            {
                if (_waitingForPerformNextDialogue)
                    _performNextDialogue = true;
                else if (!_autoText)
                {
                    ModifyTextSpeed(0);
                    SkipAudio();
                    OnTextSkipped.Invoke();
                }
            };

            _dialogueInputActions.TextBoxActionMap.SelectChoice1.performed += ctx =>
            {
                TriggerSelectChoice(DialogueChoice.Choice1);
            };

            _dialogueInputActions.TextBoxActionMap.SelectChoice2.performed += ctx =>
            {
                TriggerSelectChoice(DialogueChoice.Choice2);
            };

            _dialogueInputActions.TextBoxActionMap.SelectChoice3.performed += ctx =>
            {
                TriggerSelectChoice(DialogueChoice.Choice3);
            };

            _dialogueInputActions.TextBoxActionMap.SelectChoice4.performed += ctx =>
            {
                TriggerSelectChoice(DialogueChoice.Choice4);
            };

        }

        private void TriggerSelectChoice(DialogueChoice choice)
        {
            if ((int)choice >= _currentPlayedDialogue.Choices.Count) return;
            _currentChoiceIndex = (int)choice;
            _performNextDialogue = true;
        }

        public void PlayText()
        {
            ResetDialogueSystem();
            StartCoroutine(TextDisplayLoop());
        }

        private void ResetDialogueSystem()
        {
            _waitingForPerformNextDialogue = false;
            _performNextDialogue = false;
            _previousTextSpeed = _textSpeed;
            _previousDialogueAutoText = _autoText;
            EffectInstances.Clear();
        }

        private void PreParseTextTags()
        {
            int nextCloseTagEndIndex = -1;

            for (int i = 0; i < _currentPlayedDialogue.Text.Length; i++)
            {
                if (_currentPlayedDialogue.Text[i].IsWhitespace()) continue;

                int nextOpenTagStartIndex = _currentPlayedDialogue.Text.IndexOf('<', i);
                int nextOpenTagEndIndex = _currentPlayedDialogue.Text.IndexOf('>', i);

                if (nextOpenTagStartIndex == -1) continue;

                int tagsize = (nextOpenTagEndIndex + 1) - nextOpenTagStartIndex;

                if (tagsize < 1)
                {
                    Debug.Log("Invalid tag size");
                }

                string tag = _currentPlayedDialogue.Text.Substring(nextOpenTagStartIndex, tagsize);

                bool isCustomTag = TextEffectUtils.IsCustomTag(tag);

                if (isCustomTag && nextCloseTagEndIndex < i && nextOpenTagStartIndex != -1)
                {
                    int nextCloseTagStartIndex = _currentPlayedDialogue.Text.IndexOf(DialogueConstants.CloseTag, i);
                    nextCloseTagEndIndex = nextCloseTagStartIndex + DialogueConstants.CloseTag.Length - 1;
                    int textStartIndex = nextOpenTagEndIndex + 1;
                    int textEndIndex = nextCloseTagStartIndex;
                    string text = _currentPlayedDialogue.Text.Substring(textStartIndex, textEndIndex - textStartIndex);

                    EffectInstance effectInstance = new()
                    {
                        Tag = tag,
                        Text = text
                    };
                    EffectInstances.Add(effectInstance);

                    i = nextCloseTagEndIndex;
                }
                else
                {
                    i = nextOpenTagEndIndex;
                }
            }

            for (int i = 0; i < EffectInstances.Count; i++)
            {
                RemoveExtraText(EffectInstances[i]);
            }
        }

        private void RemoveExtraText(EffectInstance effect)
        {
            string tag = effect.Tag;
            int indexOfTag = _currentPlayedDialogue.Text.IndexOf(tag);

            _currentPlayedDialogue.Text = _currentPlayedDialogue.Text.Remove(indexOfTag, tag.Length);

            int indexOfCloseTag = _currentPlayedDialogue.Text.IndexOf(DialogueConstants.CloseTag);
            _currentPlayedDialogue.Text = _currentPlayedDialogue.Text.Remove(indexOfCloseTag, DialogueConstants.CloseTag.Length);
        }

        private void SetEffectIndexes()
        {
            string parsedText = _text.GetParsedText();
            int prevEffectIndex = 0;
            for (int i = 0; i < EffectInstances.Count; i++)
            {
                int textStartIndex = parsedText.IndexOf(EffectInstances[i].Text, prevEffectIndex);
                EffectInstances[i].TextStartIndex = textStartIndex;
                EffectInstances[i].tagType = TextEffectUtils.GetEnumFromTag(EffectInstances[i].Tag);
                prevEffectIndex = EffectInstances[i].GetTextEndIndex();
            }
#if DEBUG
            foreach (EffectInstance effect in EffectInstances)
            {
                Debug.Assert(effect.IsValid(), "Invalid Effect Instance " + effect.Tag + ": " + effect.Text + " StartIndex: " + effect.TextStartIndex);
            }
#endif
        }

    private IEnumerator TextDisplayLoop()
    {
        ClearText();
        Enter.Invoke();

        //Handle Pre Loop
        _text.maxVisibleCharacters = 0;
        _text.maxVisibleLines = _maxLineCount;
        PreParseTextTags();
        _text.text = _currentPlayedDialogue.Text;
        _text.ForceMeshUpdate();
        SetEffectIndexes();
        
        UpdateSpeakerSprite();
        HandleTextEffects();

        for (int i = 0; i < _text.textInfo.characterCount; i++)
        {
            _text.ForceMeshUpdate();

            if (CheckOverflow(i))
            {
                if (!_autoText)
                {
                    _waitingForPerformNextDialogue = true;
                    yield return new WaitUntil(() => _performNextDialogue);
                    _performNextDialogue = false;
                    _waitingForPerformNextDialogue = false;
                }

                ModifyTextSpeed(_previousTextSpeed);
                int overflowIndex = GetOverflowIndex(i);
                HandleCutoffCustomTags(i);
                string textLeft = _text.text[overflowIndex..];
                ClearText(textLeft);
                _text.ForceMeshUpdate();
                UpdateTextEffectIndexes();
                i = 0;
                OnTextboxFull.Invoke();
            }

            if (!_text.textInfo.characterInfo[i].character.IsWhitespace()) LetterRevealed.Invoke();
            _text.maxVisibleCharacters++;

            yield return new WaitForSeconds(_textSpeed);
        }

        if (!_autoText)
        {
            _waitingForPerformNextDialogue = true;
            yield return new WaitUntil(() => _performNextDialogue);
            _performNextDialogue = false;
            _waitingForPerformNextDialogue = false;
            OnTextBoxPassed.Invoke();
        }

        Exit.Invoke();
    }

        private int GetOverflowIndex(int currentIndex)
        {
            int overflowIndex = _text.textInfo.characterInfo[currentIndex].index;
            string overflowCandidate = _text.text[overflowIndex..];
            int indexOfTagOpen = overflowCandidate.IndexOf('<');
            int indexOfTagClose = overflowCandidate.IndexOf("</");
            if (indexOfTagClose != -1 && indexOfTagClose <= indexOfTagOpen)
            {
                while (overflowIndex != 0)
                {
                    if (_text.text[overflowIndex] == '<')
                    {
                        break;
                    }
                    overflowIndex--;
                }
            }
            return overflowIndex;
        }

        private bool CheckOverflow(int currentTextboxIndex)
        {
            var isOverflow = false;
            if (currentTextboxIndex < _text.textInfo.characterCount)
            {
                if (_text.textInfo.characterInfo[currentTextboxIndex].lineNumber >= _text.maxVisibleLines)
                {
                    isOverflow = true;
                }
            }
            else
            {
                Debug.Log("Error: Textbox index out of bounds");
            }
            return isOverflow;
        }

        private void HandleTextEffects()
        {
            _animationHandler.HandleTextEffects(EffectInstances, _text);
        }

        private void StopTextEffects()
        {
            _animationHandler.StopTextEffects();
        }

        private void UpdateTextEffectIndexes() 
        {
            int currentEffectIndex = 0;
            string parsedText = _text.GetParsedText();
            foreach (EffectInstance effect in EffectInstances)
            {
                if (effect.RemainingText.Length != 0)
                {
                    int newTextIndex = parsedText.IndexOf(effect.RemainingText, currentEffectIndex);
                    int cutoffIndex = newTextIndex + effect.RemainingText.Length;
                    effect.TextStartIndex = newTextIndex;
                    effect.CutoffIndex = cutoffIndex;
                    currentEffectIndex = cutoffIndex;
                }
                else 
                {
                    int newTextIndex = parsedText.IndexOf(effect.Text, currentEffectIndex);
                    if (newTextIndex != -1) 
                    {
                        effect.TextStartIndex = newTextIndex;
                        currentEffectIndex = effect.GetTextEndIndex();
                    }
                }
                string currentString = parsedText.Substring(currentEffectIndex);
            }
            StopTextEffects();
            HandleTextEffects();
        }

        private void HandleCutoffCustomTags(int currentTextBoxIndex)
        {
            if (EffectInstances.Count < 1) return;
            var currentEffect = EffectInstances[0];
            
            if (currentTextBoxIndex < currentEffect.TextStartIndex || currentTextBoxIndex > currentEffect.GetTextEndIndex()) return;

            int diff = currentTextBoxIndex - currentEffect.TextStartIndex;
            currentEffect.RemainingText = currentEffect.Text.Substring(0, diff);
        }

        private void HandleTextboxEnd()
        {
            StopTextEffects();
            if (_currentPlayedDialogue.Choices[0].NextDialogue != null)
            {
                FindNextDialogue();
                ModifyTextSpeed(_previousTextSpeed);
                PlayText();
            }
            else
            {
                if (_nextDialogueObject != null)
                {
                    _currentPlayedDialogue = DialogueScriptableObject.CopyInto(_nextDialogueObject._dialogueSO, _currentPlayedDialogue);
                    _nextDialogueObject.PlayText();
                }
                else CloseDialogueObject();
            }
        }

        private void FindNextDialogue()
        {
            DialogueScriptableObject nextDialogue = _currentPlayedDialogue.Choices[_currentChoiceIndex].NextDialogue;
            DialogueScriptableObject.CopyInto(nextDialogue, _currentPlayedDialogue);
        }

        private void ClearText(string s = "")
        {
            _text.text = s;
            _text.maxVisibleCharacters = 1;
        }

        private void SkipAudio()
        {
            if (AudioManager.Instance != null && _currentPlayedDialogue.VoiceLine != null)
            {
                AudioManager.Instance.GetAudioSource(_currentPlayedDialogue.VoiceLine)?.Stop();
            }
        }

        private void PlayAudio()
        {
            if (AudioManager.Instance != null && _currentPlayedDialogue.VoiceLine != null)
            {
                AudioManager.Instance.Play(_currentPlayedDialogue.VoiceLine);
            }
        }

        public void ModifyTextSpeed(float newValue)
        {
            _textSpeed = newValue;
        }

        private void AssignAudioType()
        {
            switch (_audioStyle)
            {
                case AudioStyle.OneSoundAudioStyle:
                    Enter.AddListener(PlayAudio);
                    OnTextboxFull.AddListener(PlayAudio);
                    return;

                case AudioStyle.GibberishAudioStyle:
                    LetterRevealed.AddListener(PlayAudio);
                    return;

                case AudioStyle.DubbedAudioStyle:
                    Enter.AddListener(PlayAudio);
                    return;
            }
        }

        private void HandleMultipleChoiceButtons()
        {
            switch (_currentPlayedDialogue.DialogueType)
            {
                case DialogueType.MultipleChoice:
                    _previousDialogueAutoText = _autoText;
                    _autoText = false;
                    ToggleIndividualChoiceButtonVisibility(true);
                    _dialogueInputActions.TextBoxActionMap.NextDialogue.Disable();
                    EnableChoiceInputs();
                    return;

                case DialogueType.SingleChoice:
                    _autoText = _previousDialogueAutoText;
                    ToggleIndividualChoiceButtonVisibility(false);
                    _currentChoiceIndex = 0;
                    _dialogueInputActions.TextBoxActionMap.NextDialogue.Enable();
                    DisableChoiceInputs();
                    return;
            }
        }

        private void EnableChoiceInputs()
        {
            _dialogueInputActions.TextBoxActionMap.SelectChoice1.Enable();
            _dialogueInputActions.TextBoxActionMap.SelectChoice2.Enable();
            _dialogueInputActions.TextBoxActionMap.SelectChoice3.Enable();
            _dialogueInputActions.TextBoxActionMap.SelectChoice4.Enable();
        }

        private void DisableChoiceInputs()
        {
            _dialogueInputActions.TextBoxActionMap.SelectChoice1.Disable();
            _dialogueInputActions.TextBoxActionMap.SelectChoice2.Disable();
            _dialogueInputActions.TextBoxActionMap.SelectChoice3.Disable();
            _dialogueInputActions.TextBoxActionMap.SelectChoice4.Disable();
        }

        private void ToggleIndividualChoiceButtonVisibility(bool vis)
        {
            for (int i = 0; i < _choiceButtons.Count; i++)
            {
                if (vis && i >= _currentPlayedDialogue.Choices.Count) return;
                _choiceButtons[i].GetComponent<CanvasGroup>().alpha = vis ? 1 : 0;
                _choiceButtons[i].GetComponent<CanvasGroup>().blocksRaycasts = vis;
                _choiceButtons[i].GetComponent<CanvasGroup>().interactable = vis;
                if (vis) _choiceButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = _currentPlayedDialogue.Choices[i].Text;
            }
        }

        private void UpdateSpeakerSprite() 
        {
            if (_currentPlayedDialogue.SpeakerSprite == null)
            {
                _speakerImage.color = Color.clear;
            }
            else
            {
                _speakerImage.color = Color.white;
               _speakerImage.sprite = _currentPlayedDialogue.SpeakerSprite;
            }
        }

        public void CloseDialogueObject()
        {
            InputHanlder.Instance.LockCursor();
            InputHanlder.Instance.UnlockInput();
            DisableChoiceInputs();
            _dialogueInputActions.TextBoxActionMap.NextDialogue.Disable();
            ToggleIndividualChoiceButtonVisibility(false);
            _currentChoiceIndex = 0;
            _dialogueCanvasGroup.alpha = 0;
            _dialogueCanvasGroup.interactable = false;
            _dialogueCanvasGroup.blocksRaycasts = false;
        }

        public void OpenDialogueObject()
        {
            InputHanlder.Instance.UnlockCursor();
            InputHanlder.Instance.LockInput();
            _currentChoiceIndex = 0;
            _dialogueCanvasGroup.alpha = 1;
            _dialogueCanvasGroup.interactable = true;
            _dialogueCanvasGroup.blocksRaycasts = true;
        }

        public void UpdateAutoText(bool newValue) 
        {
            _autoText = newValue;
            _previousDialogueAutoText = newValue;
        }
    }
}

/*
Simple <color=yellow>example</color> of text created with <#80ff80>TextMesh</color><#8080ff>Pro</color>! 
<wobble>pollas 

pollas</> 

<rainbow>pollas pollas</> 

<wobble>pollas pollas</> 

<wobble>pollas pollas</> 

<color=yellow>cipote</color>
 */