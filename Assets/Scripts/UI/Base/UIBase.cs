using UnityEngine;

public abstract class UIBase : MonoBehaviour
{
    protected virtual string OnOpenMessage => "UIBase: Opened";
    protected virtual string OnCloseMessage => "UIBase: Closed";
    protected UIManager UIManager => UIManager.Instance;
    protected virtual void Awake()
    {
        OnRegisterEvents();
    }

    protected virtual void OnDestroy()
    {
        OnUnRegisterEvents();        
    }
    
    
    public virtual void Open()
    {
        gameObject.SetActiveIfNeeded(true);   
        AlkawaDebug.Log(ELogCategory.UI, OnOpenMessage);
    }

    public virtual void Close()
    {
        gameObject.SetActive(false);
        AlkawaDebug.Log(ELogCategory.UI, OnOpenMessage);
    }
    
    protected virtual void OnRegisterEvents(){}
    protected virtual void OnUnRegisterEvents(){}
}