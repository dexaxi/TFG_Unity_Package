
namespace DUJAL.Systems.Dialogue
{
    using DUJAL.MovementComponents;
    using DUJAL.Systems.Audio;
    using DUJAL.Systems.Utils;
    using DUJAL.Systems.Dialogue.Constants;
    using DUJAL.Systems.Dialogue.Animations;
    using DUJAL.Systems.Dialogue.Types;
    using DUJAL.Systems.Dialogue.Animations.Utils;
    
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using TMPro;
    using UnityEngine.Events;
    using System;

    public class DialogueSystem : MonoBehaviour
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
        [SerializeField] private DialogueSystem _nextDialogueObject;

        [SerializeField] public UnityEvent Enter = new UnityEvent();
        [SerializeField] public UnityEvent Exit = new UnityEvent();
        [SerializeField] public UnityEvent LetterRevealed = new UnityEvent();
        [SerializeField] public UnityEvent OnTextboxFull = new UnityEvent();
        [SerializeField] public UnityEvent OnTextBoxPassed = new UnityEvent();
        [SerializeField] public UnityEvent OnTextSkipped = new UnityEvent();

        [HideInInspector] public List<EffectInstance> EffectInstances = new();

        private int _currentChoiceIdx;

        private float _previousTextSpeed;

        private bool _performNextDialogue;
        private bool _waitingForPerformNextDialogue;
        private bool _previousDialogueAutoText;

        private DialogueInputActions _dialogueInputActions;
        private TextAnimationHandler _animationHandler;


        private void Start()
        {
            _animationHandler = GetComponent<TextAnimationHandler>();
            if (_isStartingDialogue) _currentPlayedDialogue = DialogueScriptableObject.CopyInto(_dialogueSO, _currentPlayedDialogue);
            ToggleIndividualChoiceButtonVisibility(false);

            Enter.AddListener(HandleMultipleChoiceButtons);
            Exit.AddListener(HandleTextboxEnd);

            AssignAudioType();

            _currentChoiceIdx = 0;
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
            _currentChoiceIdx = (int)choice;
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
            int nextCloseTagEndIdx = -1;

            for (int i = 0; i < _currentPlayedDialogue.Text.Length; i++)
            {
                if (_currentPlayedDialogue.Text[i].IsWhitespace()) continue;

                int nextOpenTagStartIdx = _currentPlayedDialogue.Text.IndexOf('<', i);
                int nextOpenTagEndIdx = _currentPlayedDialogue.Text.IndexOf('>', i);

                if (nextOpenTagStartIdx == -1) continue;

                int tagsize = (nextOpenTagEndIdx + 1) - nextOpenTagStartIdx;

                if (tagsize < 1)
                {
                    Debug.Log("Invalid tag size");
                }

                string tag = _currentPlayedDialogue.Text.Substring(nextOpenTagStartIdx, tagsize);

                bool isCustomTag = TextEffectUtils.IsCustomTag(tag);

                if (isCustomTag && nextCloseTagEndIdx < i && nextOpenTagStartIdx != -1)
                {
                    int nextCloseTagStartIdx = _currentPlayedDialogue.Text.IndexOf(DialogueConstants.CloseTag, i);
                    nextCloseTagEndIdx = nextCloseTagStartIdx + DialogueConstants.CloseTag.Length - 1;
                    int textStartIdx = nextOpenTagEndIdx + 1;
                    int textEndIdx = nextCloseTagStartIdx;
                    string text = _currentPlayedDialogue.Text.Substring(textStartIdx, textEndIdx - textStartIdx);

                    EffectInstance effectInstance = new()
                    {
                        Tag = tag,
                        Text = text
                    };
                    EffectInstances.Add(effectInstance);

                    i = nextCloseTagEndIdx;
                }
                else
                {
                    i = nextOpenTagEndIdx;
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
            int prevEffectIdx = 0;
            for (int i = 0; i < EffectInstances.Count; i++)
            {
                int textStartIdx = parsedText.IndexOf(EffectInstances[i].Text, prevEffectIdx);
                EffectInstances[i].TextStartIdx = textStartIdx;
                EffectInstances[i].tagType = TextEffectUtils.GetEnumFromTag(EffectInstances[i].Tag);
                prevEffectIdx = EffectInstances[i].GetTextEndIndex();
            }
#if DEBUG
            foreach (EffectInstance effect in EffectInstances)
            {
                Debug.Assert(effect.IsValid(), "Invalid Effect Instance " + effect.Tag + ": " + effect.Text + " StartIndex: " + effect.TextStartIdx);
            }
#endif
        }

    private IEnumerator TextDisplayLoop()
    {
        ClearText();
        Enter.Invoke();

        //Handle Pre Loop
        PreParseTextTags();
        
        _text.text = _currentPlayedDialogue.Text;
        _text.maxVisibleCharacters = 0;
        _text.maxVisibleLines = _maxLineCount;
        _text.ForceMeshUpdate();
        
        SetEffectIndexes();
        UpdateSpeakerSprite();
        HandleTextEffects();

        for (int i = 0; i < _text.textInfo.characterCount; i++)
        {
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
                int overflowIdx = GetOverflowIndex(i);
                HandleCutoffCustomTags(i);
                string textLeft = _text.text[overflowIdx..];
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

        private int GetOverflowIndex(int currentIdx)
        {
            int overflowIdx = _text.textInfo.characterInfo[currentIdx].index;
            string overflowCandidate = _text.text[overflowIdx..];
            int tagOpenIdx = overflowCandidate.IndexOf('<');
            int tagCloseIdx = overflowCandidate.IndexOf("</");
            if (tagCloseIdx != -1 && tagCloseIdx <= tagOpenIdx)
            {
                while (overflowIdx != 0)
                {
                    if (_text.text[overflowIdx] == '<')
                    {
                        break;
                    }
                    overflowIdx--;
                }
            }
            return overflowIdx;
        }

        private bool CheckOverflow(int currentTextboxIdx)
        {
            var isOverflow = false;
            if (currentTextboxIdx < _text.textInfo.characterCount)
            {
                if (_text.textInfo.characterInfo[currentTextboxIdx].lineNumber >= _text.maxVisibleLines)
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
            int currentEffectIdx = 0;
            string parsedText = _text.GetParsedText();
            foreach (EffectInstance effect in EffectInstances)
            {
                if (effect.RemainingText.Length != 0)
                {
                    int newTextIdx = parsedText.IndexOf(effect.RemainingText, currentEffectIdx);
                    int cutoffIdx = newTextIdx + effect.RemainingText.Length;
                    effect.TextStartIdx = newTextIdx;
                    effect.CutoffIdx = cutoffIdx;
                    currentEffectIdx = effect.CutoffIdx;
                }
                else 
                {
                    int newTextIdx = parsedText.IndexOf(effect.Text, currentEffectIdx);
                    if (newTextIdx != -1) 
                    {
                        effect.TextStartIdx = newTextIdx;
                        currentEffectIdx = effect.GetTextEndIndex();
                    }
                }
            }
            StopTextEffects();
            HandleTextEffects();
        }

        private void HandleCutoffCustomTags(int currentTextBoxIdx)
        {
            if (EffectInstances.Count < 1) return;
            var currentEffect = EffectInstances[0];
            
            if (currentTextBoxIdx < currentEffect.TextStartIdx || currentTextBoxIdx > currentEffect.GetTextEndIndex()) return;

            int diff = currentTextBoxIdx - currentEffect.TextStartIdx;
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
            DialogueScriptableObject nextDialogue = _currentPlayedDialogue.Choices[_currentChoiceIdx].NextDialogue;
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
                    _currentChoiceIdx = 0;
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
            _currentChoiceIdx = 0;
            _dialogueCanvasGroup.alpha = 0;
            _dialogueCanvasGroup.interactable = false;
            _dialogueCanvasGroup.blocksRaycasts = false;
        }

        public void OpenDialogueObject()
        {
            InputHanlder.Instance.UnlockCursor();
            InputHanlder.Instance.LockInput();
            _currentChoiceIdx = 0;
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