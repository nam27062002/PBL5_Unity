using UnityEngine;

public class LoadSaveManager : SingletonMonoBehavior<LoadSaveManager>
{
    #region String
    private const string OnBoardingKey = "OnBoardingFinished";
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
    
    public void ClearAllData()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
    }

}