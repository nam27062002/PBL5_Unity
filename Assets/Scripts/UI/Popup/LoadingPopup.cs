using UnityEngine;
using UnityEngine.UI;

public class LoadingPopup : PopupBase
{
    [SerializeField] private Image loadingImage;

    private bool isRotating = false;
    public float rotateSpeed = 200f;

    protected override void OnUpdate()
    {
        if (isRotating && loadingImage != null)
        {
            loadingImage.transform.Rotate(Vector3.forward, -rotateSpeed * Time.unscaledDeltaTime);
        }
    }

    public override void Open(IBaseEventParamsUI baseEventParamsUI)
    {
        base.Open(baseEventParamsUI);
        isRotating = true;
    }

    public override void Close()
    {
        base.Close();
        isRotating = false;
    }
}