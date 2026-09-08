using UnityEngine;

public class PhoneButton : MonoBehaviour, IInteractable
{
    [Header("버튼 설정")]
    [Tooltip("이 버튼이 상징하는 글자 (예: 1, 2, 3, #)")]
    public string buttonValue; 

    public void OnInteract()
    {
        // 클릭하면 이 버튼에 적힌 숫자(또는 기호)를 매니저에게 넘겨줍니다.
        if (PhonePuzzleManager.Instance != null)
        {
            PhonePuzzleManager.Instance.PressButton(buttonValue);
        }
    }
}