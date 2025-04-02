using System;
using UnityEngine;
using UnityEngine.PlayerLoop;

public abstract class UIBase : MonoBehaviour
{
    protected virtual string OnOpenMessage => "UIBase: Opened";
    protected virtual string OnCloseMessage => "UIBase: Closed";
    protected UIManager UIManager => UIManager.Instance;
    protected bool IsActive => gameObject.activeSelf;
    protected virtual void Awake()
    {
        Initialization();
        OnRegisterEvents();
    }

    protected virtual void Initialization()
    {

    }

    protected virtual void OnDestroy()
    {
        OnUnRegisterEvents();
    }

    protected void Update()
    {
        if (IsActive) OnUpdate();
    }

    protected virtual void OnUpdate()
    {

    }

    public virtual void Open(IBaseEventParamsUI baseEventParamsUI)
    {
        gameObject.SetActiveIfNeeded(true);
        AlkawaDebug.Log(ELogCategory.UI, OnOpenMessage);
    }

    public virtual void Close()
    {
        gameObject.SetActive(false);
        AlkawaDebug.Log(ELogCategory.UI, OnCloseMessage);
    }

    protected virtual void OnRegisterEvents() { }
    protected virtual void OnUnRegisterEvents() { }
}