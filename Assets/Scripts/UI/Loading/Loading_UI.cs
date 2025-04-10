using System.Collections;
using UnityEngine;

public class Loading_UI : MonoBehaviour
{
    [SerializeField] private ProcessBar progressBar;
    [SerializeField] private float timeLoading = 1f;

    private void Start()
    {
        StartLoading(timeLoading);
    }

    public void StartLoading(float timeLoading)
    {
        this.timeLoading = timeLoading;
        CoroutineDispatcher.RunCoroutine(StartLoading());
    }

    private IEnumerator StartLoading()
    {
        // #if UNITY_EDITOR
        //         timeLoading = 0.1f;
        // #endif
        var elapsedTime = 0f;
        while (elapsedTime < timeLoading)
        {
            elapsedTime += Time.deltaTime;
            var progress = Mathf.Clamp01(elapsedTime / timeLoading);
            progressBar.SetValue(progress, $"Loading {(progress * 100):F0}%");
            yield return null;
        }

        progressBar.SetValue(1f, "Loading 100%");
        GameManager.Instance.OnLoadComplete?.Invoke();

        // Chỉ unload scene Loading nếu đang ở trong scene Loading
        if (SceneLoader.GetSceneByName(ESceneType.Loading.ToString()).isLoaded &&
            SceneLoader.GetSceneByName(ESceneType.Loading.ToString()).IsValid())
        {
            SceneLoader.UnloadSceneAsync(ESceneType.Loading);
        }
    }
}