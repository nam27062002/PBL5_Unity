using Sirenix.OdinInspector;
using UnityEngine;

public class LoadSaveManager : SingletonMonoBehavior<LoadSaveManager>
{
    #region String
    private const string OnBoardingKey = "OnBoardingFinished";
    private const string AllowUseCameraKey = "AllowUseCamera";
    #endregion
    
    public bool OnBoardingFinished
    {
        get => PlayerPrefs.GetInt(OnBoardingKey, 0) == 1;
        set
        {
            PlayerPrefs.SetInt(OnBoardingKey, value ? 1 : 0);
            PlayerPrefs.Save();
        }
    }

    public bool AllowUseCamera
    {
        get => PlayerPrefs.GetInt(AllowUseCameraKey, 0) == 1;
        private set
        {
            PlayerPrefs.SetInt(AllowUseCameraKey, value ? 1 : 0);
            PlayerPrefs.Save();
        }
    }

    public void SetAllowUseCamera()
    {
        AllowUseCamera = true;
    }
    
    [Button("Clear All Data")]
    public void ClearAllData()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
    }

}