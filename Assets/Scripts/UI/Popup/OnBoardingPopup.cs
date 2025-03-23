using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class OnBoardingPopup : PopupBase
{
    [Title("OnBoarding Popup"), Space]
    [SerializeField] private Button startButton;
    [SerializeField] private Button skipButton;
}