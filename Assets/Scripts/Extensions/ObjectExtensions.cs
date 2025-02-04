using UnityEngine;

public static class ObjectExtensions
{
    public static void SetActiveIfNeeded(this GameObject gameObject, bool active)
    {
        if (gameObject == null)
        {
            return;
        }
        if (gameObject.activeSelf != active)
        {
            gameObject.SetActive(active);
        }
    }    
}