using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;

// IInteractable 인터페이스를 상속받아 레이캐스트 클릭에 반응하도록 만듭니다.
public class ItemPickup : MonoBehaviour, IInteractable
{
    [Header("이 오브젝트의 정체")]
    public ItemData itemData; // 인스펙터에서 ItemData(스크립터블 오브젝트)를 끌어다 넣습니다.

    //
    // // 유저가 좌클릭(InteractController의 레이캐스트 감지)했을 때 실행됨
    // public void OnInteract()
    // {
    //     if (itemData == null)
    //     {
    //         Debug.LogError($"[오류] {gameObject.name}에 ItemData가 할당되지 않았습니다!");
    //         return;
    //     }
    //
    //         InventoryManager.Instance.AddItem(itemData);
    //         Destroy(gameObject);
    // }
    
    public InspectManager.InspectState inspectMode;
    public Transform approachPoint; // 카메라가 도착할 위치와 바라볼 방향

    private bool collected;

    public void OnInteract()
    {
        if (itemData == null || collected) return;

        InspectManager.Instance.StartWorldInspect(this);
    }

    public void Collect()
    {
        if (itemData == null || collected) return;

        collected = true;
        InventoryManager.Instance.AddItem(itemData);
        Destroy(gameObject);
    }
}
