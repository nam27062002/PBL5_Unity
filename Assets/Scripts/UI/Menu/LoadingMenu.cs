using Sirenix.OdinInspector;
using UnityEngine;

public class LoadingMenu : MenuBase
{
    [SerializeField] private Loading_UI loadingUI;

    public override void Open(IBaseEventParamsUI baseEventParamsUI)
    {
        base.Open(baseEventParamsUI);
        if (baseEventParamsUI is LoadingMenuEventParams loadingMenuEventParams)
        {
            loadingUI.StartLoading(loadingMenuEventParams.loadingTime);
        }
    }
}