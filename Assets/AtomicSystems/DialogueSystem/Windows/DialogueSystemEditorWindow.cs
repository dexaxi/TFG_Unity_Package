using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using DUJAL.Systems.Utils;
using DUJAL.Systems.Dialogue.Utils;
using DUJAL.Systems.Dialogue.Constants;
namespace DUJAL.Systems.Dialogue
{

    public class DialogueSystemEditorWindow : EditorWindow
    {
        private Button _saveButton;
        private TextField _fileNameTF;
        [MenuItem(DialogueConstants.DialogueGraphWindowStream)]
        public static void Open()
        {
            GetWindow<DialogueSystemEditorWindow>(DialogueConstants.DialogueGraphWindowTitle);
        }

        private void OnEnable()
        {
            AddGraphView();
            AddToolBar();
            AddStyles();
        }

        private void AddToolBar()
        {
            Toolbar toolbar = new Toolbar();
            _fileNameTF = DialogueSystemUtils.CreateTextField(DialogueConstants.DefaultAssetFilename, DialogueConstants.FileNameLabel, callback => 
            {
                _fileNameTF.value = callback.newValue.RemoveWhitespaces().RemoveSpecialCharacters();
            });
            _saveButton = DialogueSystemUtils.CreateButon(DialogueConstants.SaveAssetButonText);
            toolbar.Add(_fileNameTF);
            toolbar.Add(_saveButton);
            toolbar.AddStyleSheets(DialogueConstants.EditorDefaultResourcesPath + DialogueConstants.ToolbarStyleFilename);
            rootVisualElement.Add(toolbar);
        }

        private void AddGraphView()
        {
            DialogueSystemGraphView gView = new DialogueSystemGraphView(this);
            gView.StretchToParentSize();
            rootVisualElement.Add(gView);
        }

        private void AddStyles()
        {
            rootVisualElement.AddStyleSheets(
                DialogueConstants.EditorDefaultResourcesPath + DialogueConstants.VariablesFilename
            );
        }

        public void ToggleSaving(bool enabled) 
        {
            _saveButton.SetEnabled(enabled); 
        }

    }
}