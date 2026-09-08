using UnityEngine;
using UnityEngine.Events; // 인스펙터에서 이벤트를 연결하기 위해 필수

public class RequireItemObject : MonoBehaviour, IInteractable
{
    [Header("퍼즐 요구 조건")]
    [Tooltip("ItemData에 적어둔 itemID와 정확히 일치해야 합니다. (예: Key_01)")]
    public string requiredItemID; 
    
    [Tooltip("퍼즐을 풀고 나면 인벤토리에서 이 아이템을 삭제할까요?")]
    public bool consumeItemOnSuccess = true; 

    [Header("이벤트 (기획자 연결용)")]
    public UnityEvent OnSuccess; // 알맞은 아이템을 들고 클릭했을 때 실행
    public UnityEvent OnFailure; // 빈손이거나 틀린 아이템을 들고 클릭했을 때 실행

    public void OnInteract()
    {
        // 1. 현재 유저가 인벤토리에서 선택한(노란 테두리) 아이템을 가져옵니다.
        InventoryItem selected = InventoryManager.Instance.SelectedItem;

        // 2. 선택한 아이템이 있고, 그 아이템의 ID가 요구하는 ID와 일치한다면? (성공)
        if (selected != null && selected.data.itemID == requiredItemID)
        {
            Debug.Log($"[퍼즐 성공] '{selected.data.itemName}'을(를) 사용했습니다!");
            
            // 아이템 소모가 켜져 있다면 가방에서 지움
            if (consumeItemOnSuccess)
            {
                InventoryManager.Instance.RemoveItem(selected);
            }
            
            // 인스펙터에 연결된 성공 이벤트들(자물쇠 파괴 등)을 모조리 실행!
            OnSuccess?.Invoke(); 
        }
        // 3. 빈손이거나 다른 아이템을 들고 있다면? (실패)
        else 
        {
            Debug.Log("[퍼즐 실패] 알맞은 아이템이 없거나 선택되지 않았습니다.");
            
            // 인스펙터에 연결된 실패 이벤트들(철컥 소리 재생 등)을 실행
            OnFailure?.Invoke(); 
        }
    }
}