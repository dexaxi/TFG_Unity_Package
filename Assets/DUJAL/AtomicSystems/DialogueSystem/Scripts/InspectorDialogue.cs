
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
        [SerializeField] [Range(0, 1)] private float _textSpeed;
        [SerializeField] [Range(0, 10)] private int _maxLineCount;
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

        private int _currentChoiceIndex;
        private float _previousTextSpeed;
        private bool _performNextDialogue;
        private bool _waitingForPerformNextDialogue;
        DialogueInputActions _dialogueInputActions;
        private bool _previousDialogueAutoText;

        private TextMeshProUGUI _auxTMPro;

        private int _auxTMProCharIndex;
        private int _parsedTextIndexDelta;

        private void Awake()
        {
            if(_isStartingDialogue) _currentPlayedDialogue = DialogueScriptableObject.CopyInto(_dialogueSO, _currentPlayedDialogue);
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
                _choiceButtons[i].onClick.AddListener(() => TriggerSelectChoice(x));
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
                TriggerSelectChoice(0);
            };

            _dialogueInputActions.TextBoxActionMap.SelectChoice2.performed += ctx =>
            {
                TriggerSelectChoice(1);
            };

            _dialogueInputActions.TextBoxActionMap.SelectChoice3.performed += ctx =>
            {
                TriggerSelectChoice(2);
            };

            _dialogueInputActions.TextBoxActionMap.SelectChoice4.performed += ctx =>
            {
                TriggerSelectChoice(3);
            };

        }
        private void TriggerSelectChoice(int n)
        {
            if (n >= _currentPlayedDialogue.Choices.Count) return;
            _currentChoiceIndex = n;
            _performNextDialogue = true;
        }

        public void PlayText()
        {
            _waitingForPerformNextDialogue = false;
            _performNextDialogue = false;
            _previousTextSpeed = _textSpeed;
            _previousDialogueAutoText = _autoText;
            _auxTMProCharIndex = 0;
            _parsedTextIndexDelta = 0;
            StartCoroutine(PlayTextC());
        }

        private int _nextOpenTagIndex;
        private int _nextCloseTagIndex;
        private IEnumerator PlayTextC()
        {
            ClearText();
            Enter.Invoke();

            _text.maxVisibleCharacters = 0;
            _text.maxVisibleLines = _maxLineCount;
            _text.text = _currentPlayedDialogue.Text;
            UpdateSpeakerSprite();
            _text.ForceMeshUpdate();
            
            for (int i = 0; i < _text.text.Length; i++)
            {
                CopyParsedTMProUGUI(_text);
                _nextOpenTagIndex = _text.text.IndexOf('<', i);
                _nextCloseTagIndex = _text.text.IndexOf('>', i);

                if (_nextOpenTagIndex > _nextCloseTagIndex || _nextOpenTagIndex == i ||_nextCloseTagIndex == i)
                {
                    _parsedTextIndexDelta = _nextCloseTagIndex - i;
                }
                else if (_parsedTextIndexDelta == 0) 
                {                
                    HandleTextEffects();
                    _text.ForceMeshUpdate();

                    if (!_text.text[i].IsWhitespace()) LetterRevealed.Invoke();
                    
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

                    yield return new WaitForSeconds(_textSpeed);
                }
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
            if(_auxTMPro == null)  _auxTMPro = Instantiate(original, transform);
            _auxTMPro.textInfo.characterInfo = new TMP_CharacterInfo[_auxTMPro.text.Length];
            for (int i = 0; i < _auxTMPro.text.Length; i++) 
            {
                TMP_CharacterInfo charInfo = original.textInfo.characterInfo[i];
                _auxTMPro.textInfo.characterInfo[i] = charInfo;
            }
            _auxTMPro.text = original.GetParsedText();
        }

        private void HandleTextEffects() 
        {
            GetComponent<TextAnimatorInspector>().HandleTextEffects(_text);
        }

        private void HandleTextboxEnd()
        {
            WobbleText.DO = false;
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
