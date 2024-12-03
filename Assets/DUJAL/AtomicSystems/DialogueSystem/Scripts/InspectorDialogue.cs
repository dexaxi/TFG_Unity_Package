
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
        [SerializeField] private Image _speakerImage;
        [SerializeField] private List<Button> _choiceButtons;
        [SerializeField] private InspectorDialogue _nextDialogueObject;

        [SerializeField] public UnityEvent Enter = new UnityEvent();
        [SerializeField] public UnityEvent Exit = new UnityEvent();
        [SerializeField] public UnityEvent LetterRevealed = new UnityEvent();
        [SerializeField] public UnityEvent OnTextboxFull = new UnityEvent();
        [SerializeField] public UnityEvent OnTextBoxPassed = new UnityEvent();
        [SerializeField] public UnityEvent OnTextSkipped = new UnityEvent();

        [HideInInspector] public List<EffectInstance> EffectInstances = new();

        private int _currentChoiceIndex;
        
        private int _auxTMProCharIndex;
        
        private float _previousTextSpeed;
        
        private bool _performNextDialogue;
        private bool _waitingForPerformNextDialogue;
        private bool _previousDialogueAutoText;
        
        private DialogueInputActions _dialogueInputActions;
        private TextMeshProUGUI _auxTMPro;
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
            _auxTMProCharIndex = 0;
            EffectInstances.Clear();
        }

        private void PreParseTextTags()
        {
            int nextCloseTagEndIndex = -1;

            for (int i = 0; i < _currentPlayedDialogue.Text.Length; i++)
            {
                int nextOpenTagStartIndex = _currentPlayedDialogue.Text.IndexOf('<', i);
                if (nextCloseTagEndIndex < i && nextOpenTagStartIndex != -1)
                {
                    int nextOpenTagEndIndex = _currentPlayedDialogue.Text.IndexOf('>', i) + 1;
                    int nextCloseTagStartIndex = _currentPlayedDialogue.Text.IndexOf(DialogueConstants.CloseTag, i);
                    nextCloseTagEndIndex = nextCloseTagStartIndex + DialogueConstants.CloseTag.Length;
                    int textStartIndex = nextOpenTagEndIndex;
                    int textEndIndex = nextCloseTagStartIndex;
                    string tag = _currentPlayedDialogue.Text.Substring(nextOpenTagStartIndex, nextOpenTagEndIndex - nextOpenTagStartIndex);
                    string text = _currentPlayedDialogue.Text.Substring(textStartIndex, textEndIndex - textStartIndex);

                    EffectInstance effectInstance = new()
                    {
                        Tag = tag,
                        Text = text
                    };
                    EffectInstances.Add(effectInstance);
                    
                    i = nextCloseTagEndIndex + 1;
                }
            }

            for(int i = 0; i < EffectInstances.Count; i++) 
            {
                int textStartIndex = RemoveExtraText(EffectInstances[i]);
                EffectInstances[i].TextStartIndex = textStartIndex;
            }
        }

        //Returns new index of text
        private int RemoveExtraText(EffectInstance effect)
        {
            string tag = effect.Tag;
            int indexOfTag = _currentPlayedDialogue.Text.IndexOf(tag);
            
            _currentPlayedDialogue.Text = _currentPlayedDialogue.Text.Remove(indexOfTag, tag.Length);

            int indexOfCloseTag = _currentPlayedDialogue.Text.IndexOf(DialogueConstants.CloseTag);
            _currentPlayedDialogue.Text = _currentPlayedDialogue.Text.Remove(indexOfCloseTag, DialogueConstants.CloseTag.Length);

            return _currentPlayedDialogue.Text.IndexOf(effect.Text);
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
            UpdateSpeakerSprite();
            _text.ForceMeshUpdate();

            HandleTextEffects(EffectInstances);

            for (int i = 0; i < _text.text.Length; i++)
            {
                CopyParsedTMProUGUI(_text);
                _text.ForceMeshUpdate();
                    
                if (CheckOverflow(_auxTMProCharIndex))
                {
                    if (!_autoText)
                    {
                        _waitingForPerformNextDialogue = true;
                        yield return new WaitUntil(() => _performNextDialogue);
                        _performNextDialogue = false;
                        _waitingForPerformNextDialogue = false;
                    }

                    ModifyTextSpeed(_previousTextSpeed);
                    string textLeft = _text.text.Substring(i);
                    ClearText(textLeft);
                    Destroy(_auxTMPro.gameObject);
                    _auxTMProCharIndex = 0;
                    i = 0;
                    OnTextboxFull.Invoke();
                }

                _auxTMProCharIndex++;
                _text.maxVisibleCharacters++;
                if (!_text.text[i].IsWhitespace()) LetterRevealed.Invoke();

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

            Destroy(_auxTMPro.gameObject);
            Exit.Invoke();
        }

        private bool CheckOverflow(int currentTextboxIndex)
        {
            return _auxTMPro.textInfo.characterInfo[currentTextboxIndex].lineNumber >= _text.maxVisibleLines;
        }

        private void CopyParsedTMProUGUI(TextMeshProUGUI original)
        {
            if(_auxTMPro == null) _auxTMPro = Instantiate(original, transform);
            _auxTMPro.textInfo.characterInfo = new TMP_CharacterInfo[_auxTMPro.text.Length];
            for (int i = 0; i < _auxTMPro.text.Length; i++) 
            {
                TMP_CharacterInfo charInfo = original.textInfo.characterInfo[i];
                _auxTMPro.textInfo.characterInfo[i] = charInfo;
            }
            _auxTMPro.text = original.GetParsedText();
        }

        private void HandleTextEffects(List<EffectInstance> effectInstances) 
        {
            _animationHandler?.HandleTextEffects(effectInstances, _text);
        }

        private void StopTextEffects()
        {
            _animationHandler?.StopTextEffects();
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
