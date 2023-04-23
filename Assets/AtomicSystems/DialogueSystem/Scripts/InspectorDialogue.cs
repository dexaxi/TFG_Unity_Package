
namespace DUJAL.Systems.Dialogue
{
    using System.Collections;
    using UnityEngine;
    using TMPro;
    using UnityEngine.Events;
    using DUJAL.Systems.Utils;
    using UnityEngine.UI;
    using DUJAL.Systems.Audio;

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

        //UI Data
        [SerializeField] private TextMeshProUGUI _text;
        [SerializeField] private Image _speakerImage;
        [SerializeField] [Range(0,1)] private float _textSpeed;
        [SerializeField] [Range(0,10)] private int _maxLineCount;
        [SerializeField] private AudioStyle _audioStyle;
        [SerializeField] private bool _autoText;

        [SerializeField] public UnityEvent Enter = new UnityEvent();
        [SerializeField] public UnityEvent Exit = new UnityEvent();
        [SerializeField] public UnityEvent LetterRevealed = new UnityEvent();
        [SerializeField] public UnityEvent OnTextboxFull = new UnityEvent();
        [SerializeField] public UnityEvent OnTextBoxPassed = new UnityEvent();
        [SerializeField] public UnityEvent OnTexSkipped = new UnityEvent();

        private int _currentChoiceIndex;
        private void Awake()
        {
            _currentPlayedDialogue = DialogueScriptableObject.CopyInto(_dialogueSO, _currentPlayedDialogue);

            Exit.AddListener(HandleTextboxEnd);
            LetterRevealed.AddListener(PlayGibberish);

            _currentChoiceIndex = 0;
            
            PlayText();
        }

        public void PlayText()
        {
            StartCoroutine(PlayTextC());
        }

        private IEnumerator PlayTextC() 
        {
            ClearText();
            Enter.Invoke();

            _text.maxVisibleLines = _maxLineCount;
            _text.text = _currentPlayedDialogue.Text;
            _speakerImage.sprite = _currentPlayedDialogue.SpeakerSprite;

            Debug.Log("Current Text: " + _currentPlayedDialogue.Text);

            for (int i = 0; i < _text.text.Length; i++)
            {
                _text.maxVisibleCharacters++;
                LetterRevealed.Invoke();

                _text.ForceMeshUpdate();

                CheckOverflow(ref i);

                yield return new WaitForSeconds(_textSpeed);
            }
            Debug.Log("Exit Coroutine");
            Exit.Invoke();
        }

        private void CheckOverflow(ref int i)
        {
            if (!_text.text[i].IsWhitespace() && !_text.textInfo.characterInfo[i].isVisible)
            {
                Debug.Log("Text box overflow.");
                string textLeft = _text.text.Substring(i);
                ClearText(textLeft);
                i = 0;
                OnTextboxFull.Invoke();
            }
        }

        private void HandleTextboxEnd() 
        {
            if (_currentPlayedDialogue.Choices[0].NextDialogue != null) 
            {
                Debug.Log("Current Text: " + _currentPlayedDialogue.Text + " Next Text: " + _currentPlayedDialogue.Choices[0].NextDialogue.Text);
                FindNextDialogue();
                PlayText();
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

        private void PlayGibberish() 
        {
            if (AudioManager.Instance != null) 
            {
                AudioManager.Instance.Play(_currentPlayedDialogue.VoiceLine);
            }
        }

    }
}
