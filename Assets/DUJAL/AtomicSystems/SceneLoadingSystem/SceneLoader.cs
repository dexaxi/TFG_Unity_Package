namespace DUJAL.Systems.Loading
{
    using Cysharp.Threading.Tasks;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using UnityEngine.Events;

    public class SceneLoader : MonoBehaviour
    {
        public static SceneLoader Instance;
        [HideInInspector] public bool IsLoading { get; private set; }
        
        [HideInInspector] public bool HoldLoading;
        [HideInInspector] public bool IgnoreLoadingScreen;

        private UnityEvent _onSceneLoaded;
        private UnityEvent _onScenePreloaded;
        private UnityEvent _onSceneUnloaded;

        private AsyncOperation _sceneLoadOp;
        private AsyncOperation _sceneUnloadOp;
        private AsyncOperation _loadingSceneOp;
        private AsyncOperation _unloadingSceneOp;

        private readonly Queue<SceneIndex> _loadingQueue = new();
        private SceneIndex _previousQueuedItem;

        private float _loadingProgress = 0;
        private bool _loadingSceneLoaded;
        private bool _sceneLoaded;
        private bool _sceneUnloaded;
        private bool _processingQueue;

        private void Awake()
        {
            if (Instance)
            {
                Destroy(gameObject);
                return;
            }
            else
            {
                Instance = this;
                transform.parent = null;
                DontDestroyOnLoad(gameObject);
            }

            HoldLoading = false;
            ResetLoadingQueueControl();
            _onScenePreloaded = new();
            _onSceneLoaded = new();
            _onSceneUnloaded = new();
            _previousQueuedItem = GetActiveScene();
        }

        // Public function to load scene from index. Add new scenes to SceneIndex.cs.
        // Wait is the time to wait before loading. Ignore loading screen hides the loading canvas (Useful for debugging).
        public void LoadScene(SceneIndex nextScene, int wait = 0, bool ignoreLoadingScreen = false)
        {
            if (_loadingQueue.Count == 0) _loadingProgress = 0.05f;
            HoldLoading = false;
            IsLoading = true;
            IgnoreLoadingScreen = ignoreLoadingScreen;
            LoadSceneTask(nextScene, wait).Forget();
        }

        // Load next scene in build index (given wait and ignore loding screen)
        public void LoadNextScene(int wait = 0, bool ignoreLoadingScreen = false) { LoadScene(GetActiveScene() + 1, wait, ignoreLoadingScreen); }

        // Load previous scene in build index (given wait and ignore loding screen)
        public void LoadPreviousScene(int wait = 0, bool ignoreLoadingScreen = false) { LoadScene(GetActiveScene() - 1, wait ,ignoreLoadingScreen); }

        // Reload active scene (given wait and ignore loding screen)
        public void ReloadScene(int wait = 0, bool ignoreLoadingScreen = false)
        {
            LoadScene(GetActiveScene(), wait, ignoreLoadingScreen);
        }

        // Return loading progress from the current async operation.
        public float GetLoadingProgress()
        {
            _loadingProgress = _sceneLoadOp != null && _loadingProgress < _sceneLoadOp.progress ? _sceneLoadOp.progress : _loadingProgress;
            return _loadingProgress;
        }

        // Event called when scene has finished loading.
        public void AddOnSceneLoadedEvent(UnityEvent func)
        {
            _onSceneLoaded.AddListener(func.Invoke);
        }

        public void RemoveOnSceneLoadedEvent(UnityEvent func)
        {
            _onSceneLoaded.RemoveListener(func.Invoke);
        }

        public void ClearOnSceneLoadedEvents() { _onSceneLoaded.RemoveAllListeners(); }

        // Event called when scene has started loading.
        public void AddOnScenePreloadedEvent(UnityEvent func)
        {
            _onScenePreloaded.AddListener(func.Invoke);
        }

        public void RemoveOnScenePreloadedEvent(UnityEvent func)
        {
            _onScenePreloaded.RemoveListener(func.Invoke);
        }

        public void ClearOnScenePreloadedEvents() { _onScenePreloaded.RemoveAllListeners(); }

        // Event called when scene has finished unloading.
        public void AddOnSceneUnloadedEvent(UnityEvent func)
        {
            _onSceneUnloaded.AddListener(func.Invoke);
        }
        public void RemoveOnSceneUnloadedEvent(UnityEvent func)
        {
            _onSceneUnloaded.RemoveListener(func.Invoke);
        }

        public void ClearOnSceneUnloadedEvents() { _onSceneUnloaded.RemoveAllListeners(); }
        
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private async UniTask LoadSceneTask(SceneIndex nextScene, int wait = 0)
        {
            HandleLoadingSceneLoad().Forget();
            await UniTask.WaitUntil(() => _loadingSceneLoaded);
            _loadingQueue.Enqueue(nextScene);
            LoadSceneFlow(wait).Forget();
        }

        //Coroutine
        private async UniTask LoadSceneFlow(int wait = 0)
        {
            await UniTask.WaitUntil(() => !_processingQueue);

            await UniTask.Delay(wait);

            _processingQueue = true;
            if (_loadingQueue.Count == 0)
            {
                HandleLoadingQueueError(_previousQueuedItem);
                return;
            }

            SceneIndex nextScreen = _loadingQueue.Dequeue();

            PrintUnloadingScene(_previousQueuedItem);

            HandleSceneUnload(_previousQueuedItem).Forget();

            await UniTask.WaitUntil(() => _sceneUnloaded);
            _onSceneUnloaded.Invoke();

            PrintUnloadingFinished(_previousQueuedItem);
            _previousQueuedItem = nextScreen;
            PrintLoadingScene(nextScreen);

            HandleSceneLoad(nextScreen).Forget();
            _onScenePreloaded.Invoke();

            await UniTask.WaitUntil(() => _sceneLoaded);

            await UniTask.WaitUntil(() => !HoldLoading);
            _onSceneLoaded.Invoke();

            PrintLoadingFinished(nextScreen);
            ResetLoadingQueueControl();

            if (_loadingQueue.Count == 0)
            {
                HandleLoadEnd().Forget();
            }
        }

        private async UniTask HandleLoadEnd()
        {
            HandleLoadingSceneUnload().Forget();
            await UniTask.WaitUntil(() => !_loadingSceneLoaded);
            _sceneLoadOp = null;
            _sceneUnloadOp = null;
            _loadingSceneOp = null;
            _unloadingSceneOp = null;
            IsLoading = false;
            _loadingProgress = 1.0f;
        }

        private void HandleLoadingQueueError(SceneIndex previousLoadedScene)
        {
            Debug.LogWarning($"[FINISHING LOAD] Warning: Trying to Dequeue Loading Sceen while queue is empty, last loaded item: {previousLoadedScene}. ");
            IsLoading = false;
            HoldLoading = false;
            ResetLoadingQueueControl();
            HandleLoadingSceneUnload().Forget();
        }

        private void ResetLoadingQueueControl()
        {
            _sceneLoaded = false;
            _sceneUnloaded = false;
            _processingQueue = false;
        }

        private async UniTask HandleSceneLoad(SceneIndex scene)
        {
            _sceneLoadOp = LoadAsync(scene, LoadSceneMode.Additive);

            //wait until loading is done
            await UniTask.WaitUntil(() => _sceneLoadOp.isDone && IsSceneLoaded(scene));

            _sceneLoaded = true;
        }

        private async UniTask HandleSceneUnload(SceneIndex scene)
        {
            _sceneUnloadOp = UnloadAsync(scene);

            await UniTask.WaitUntil(() => _sceneUnloadOp.isDone
                && !IsSceneLoaded(_previousQueuedItem));

            _sceneUnloaded = true;
        }

        private async UniTask HandleLoadingSceneLoad()
        {
            if (!IsSceneLoaded(SceneIndex.LoadingScreen))
            {
                _loadingSceneOp = LoadAsync(SceneIndex.LoadingScreen, LoadSceneMode.Additive);
            }
            await UniTask.WaitUntil(() => _loadingSceneOp.isDone && IsSceneLoaded(SceneIndex.LoadingScreen));
            _loadingSceneLoaded = true;
        }

        private async UniTask HandleLoadingSceneUnload()
        {
            if (IsSceneLoaded(SceneIndex.LoadingScreen))
            {
                _unloadingSceneOp = UnloadAsync(SceneIndex.LoadingScreen);
                await UniTask.WaitUntil(() => _unloadingSceneOp.isDone);
            }
            await UniTask.WaitUntil(() => !IsSceneLoaded(SceneIndex.LoadingScreen));
            _loadingSceneLoaded = false;
        }

        public static bool IsSceneLoaded(SceneIndex Scene)
        {
            return SceneManager.GetSceneByBuildIndex((int)Scene).isLoaded;
        }

        public static SceneIndex GetActiveScene()
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if ((SceneIndex)scene.buildIndex != SceneIndex.LoadingScreen)
                {
                    return (SceneIndex) scene.buildIndex;
                }
            }

            Debug.LogWarning("WARNING: No active scene found");
            return SceneIndex.NoScene;
        }

        private AsyncOperation LoadAsync(SceneIndex index, LoadSceneMode sceneMode)
        {
            return SceneManager.LoadSceneAsync(index, sceneMode);
        }

        private AsyncOperation UnloadAsync(SceneIndex index)
        {
            return SceneManager.UnloadSceneAsync(index);
        }

        private void PrintLoadingScene(SceneIndex index)
        {
            Debug.Log("[LOADING] " + index.String);
        }

        private void PrintUnloadingScene(SceneIndex index)
        {
            Debug.Log("[UNLOADING] " + index.String);
        }

        private void PrintLoadingFinished(SceneIndex index)
        {
            Debug.Log("[LOADING] " + index.String + " FINISHED");
        }

        private void PrintUnloadingFinished(SceneIndex index)
        {
            Debug.Log("[UNLOADING] " + index.String+ " FINISHED");
        }
    }
}