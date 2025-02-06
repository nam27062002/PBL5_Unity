using System;

public class GameplayManager : SingletonMonoBehavior<GameplayManager>
{
    private void Start()
    {
        UIManager.Instance.OpenMenu(MenuType.OnBoarding);
    }
}