
namespace DUJAL.Systems.Dialogue
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using TMPro;
    using UnityEngine.Events;
    using DUJAL.Systems.Utils;
    public class InspectorDialogue : MonoBehaviour
    {
        //SO
        [SerializeField] private DialogueContainerScriptableObject _dialogueContainerSO;
        [SerializeField] private GroupScriptableObject _groupSO;
        [SerializeField] private DialogueScriptableObject _dialogueSO;
        //filter
        [SerializeField] private bool _startingDialogueFilter;
        [SerializeField] private bool _groupedDialogueFilter;

        //index
        [SerializeField] private int _selectedGroup;
        [SerializeField] private int _selectedDialogue;

        //UI Data
        [SerializeField] private TextMeshProUGUI _text;
        [SerializeField] [Range(0,1)] private float _textSpeed;
        [SerializeField] [Range(0,10)] private int _maxLineCount;
        [SerializeField] private AudioStyle _audioStyle;
        [SerializeField] private bool _autoText;

        [SerializeField] private UnityEvent _enter = new UnityEvent();
        [SerializeField] private UnityEvent _exit = new UnityEvent();

        private void Awake()
        {
            _enter.Invoke();
            ClearText();
            PlayText();
            _exit.Invoke();
        }

        public void PlayText()
        {
            StartCoroutine(PlayTextC());
        }

        private IEnumerator PlayTextC() 
        {
            for (int i = 0; i < _dialogueSO.Text.Length; i++)
            {
                yield return new WaitForSeconds(_textSpeed);
                if (CheckIfWorkWillOverFlow(ref i)) 
                {
                    ClearText();
                }
                _text.text += _dialogueSO.Text[i];
            }
        }

        private bool CheckIfWorkWillOverFlow(ref int index) 
        {
            if (_text.textInfo.lineCount > _maxLineCount) 
            {
                for (int i = _text.text.Length-1; i > 0; i--) 
                {
                    Debug.Log(i);
                    if (string.IsNullOrEmpty(_text.text[i].ToString().RemoveWhitespaces()))
                    {
                        Debug.Log("index = " + i);
                        index = i-1;
                    }
                }
                return true;
            }
            return false;
        }
       
        public void ClearText() 
        {
            _text.text = "";
        }
    }
}
