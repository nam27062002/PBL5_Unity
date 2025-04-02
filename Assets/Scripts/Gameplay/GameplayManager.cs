using System;
using UnityEngine;

public class GameplayManager : SingletonMonoBehavior<GameplayManager>
{
    [SerializeField] private MenuType menuType;
    private void Start()
    {
        UIManager.Instance.OpenMenu(menuType, null);
    }
}