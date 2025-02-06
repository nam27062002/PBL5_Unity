using Sirenix.OdinInspector;
using UnityEngine;

public abstract class MenuBase : UIBase
{
    [Title("Menu Base"), Space]
    [SerializeField] private MenuType menuType;

    protected override string OnOpenMessage => $"{menuType}: OnOpen";
    protected override string OnCloseMessage => $"{menuType}: OnClose";
}