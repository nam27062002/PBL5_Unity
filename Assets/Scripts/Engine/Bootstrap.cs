using UnityEngine;
using UnityEngine.SceneManagement;

public class Bootstrap : MonoBehaviour
{
    private void Awake()
    {
        Application.targetFrameRate = ApplicationConfig.TargetFrameRate;
    }
    
    private void Start()
    {
        SceneLoader.LoadSceneAsync(ESceneType.GameManager, LoadSceneMode.Single);
    }
}
