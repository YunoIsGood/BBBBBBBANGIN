using UnityEngine;

// IInteractable 인터페이스를 상속받아 레이캐스트 클릭에 반응하도록 만듭니다.
public class ItemPickup : MonoBehaviour, IInteractable
{
    [Header("이 오브젝트의 정체")]
    public ItemData itemData; // 인스펙터에서 ItemData(스크립터블 오브젝트)를 끌어다 넣습니다.

    // 유저가 좌클릭(InteractController의 레이캐스트 감지)했을 때 실행됨
    public void OnInteract()
    {
        if (itemData == null)
        {
            Debug.LogError($"[오류] {gameObject.name}에 ItemData가 할당되지 않았습니다!");
            return;
        }

        // 1. 인벤토리 매니저에게 내 데이터를 넘겨 가방에 넣게 함
        InventoryManager.Instance.AddItem(itemData);

        // 2. 주웠으니 월드에서 내 자신을 파괴함
        Destroy(gameObject);
    }
}