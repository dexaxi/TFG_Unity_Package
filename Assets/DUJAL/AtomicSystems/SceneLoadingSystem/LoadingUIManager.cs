namespace DUJAL.Systems.Loading 
{
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    public class LoadingUIManager : MonoBehaviour
    {
        [field: SerializeField] public TextMeshProUGUI LoadingText { get; private set; }
    
        private CanvasGroup _loadingCanvas;
        private Slider _progressBar;
        private void Start()
        {
            _progressBar = GetComponentInChildren<Slider>();
            _loadingCanvas = GetComponent<CanvasGroup>();
            if (_progressBar != null) _progressBar.value = 0;
            if(LoadingText != null) LoadingText.text = "0%";
        }

        private void Update()
        {
            //Loading progress bar + % text
            if (SceneLoader.Instance.IgnoreLoadingScreen) 
            {
                _loadingCanvas.alpha = 0.0f;
                return;
            }
            if (SceneLoader.Instance.IsLoading)
            {
                float progressValue = Mathf.Clamp01(SceneLoader.Instance.GetLoadingProgress());
                if(_progressBar != null) _progressBar.value = progressValue;
                if(LoadingText != null) LoadingText.text = Mathf.Round(progressValue * 100) + "%";
            }
        }
    }
}