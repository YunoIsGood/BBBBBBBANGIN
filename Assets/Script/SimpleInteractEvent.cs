using UnityEngine;
using UnityEngine.Events;

// IInteractable을 상속받아 클릭을 감지합니다.
public class SimpleInteractEvent : MonoBehaviour, IInteractable
{
    [Header("클릭 시 실행할 이벤트")]
    [Tooltip("애니메이터 작동, 소리 재생, 오브젝트 켜기/끄기 등을 자유롭게 연결하세요.")]
    public UnityEvent onInteractEvent;

    public void OnInteract()
    {
        Debug.Log($"[상호작용] {gameObject.name} 클릭됨!");
        onInteractEvent?.Invoke();
    }
}