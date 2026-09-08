using UnityEngine;

public class PhoneHandset : MonoBehaviour, IInteractable
{
    public void OnInteract()
    {
        // 클릭하면 매니저에게 수화기를 들거나 내려놓으라고 지시합니다.
        if (PhonePuzzleManager.Instance != null)
        {
            PhonePuzzleManager.Instance.ToggleHandset();
        }
    }
}