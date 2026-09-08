using UnityEngine;

public class NotePickup : MonoBehaviour, IInteractable
{
    public void OnInteract()
    {
        // 수첩 매니저에게 "나 주웠다!" 라고 알림 (자동으로 UI가 켜짐)
        NoteManager.Instance.AcquireNote();
        
        // 바닥에 있는 3D 수첩은 파괴함
        Destroy(gameObject);
    }
}