using UnityEngine;
using UnityEngine.EventSystems; // UI 클릭 감지용
using UnityEngine.Events;

// IPointerClickHandler를 상속받아 UI 클릭을 감지합니다.
public class UIRequireItem : MonoBehaviour, IPointerClickHandler
{
    [Header("퍼즐 요구 조건")]
    public string requiredItemID; // 예: "Photo_Part1"
    
    [Header("이벤트 (기획자 연결용)")]
    public UnityEvent OnSuccess; // 사진 끼우기 성공 시 실행 (숨겨둔 사진 켜기)

    public void OnPointerClick(PointerEventData eventData)
    {
        // 좌클릭만 반응
        if (eventData.button != PointerEventData.InputButton.Left) return;

        InventoryItem selected = InventoryManager.Instance.SelectedItem;

        if (selected != null && selected.data.itemID == requiredItemID)
        {
            Debug.Log($"[수첩] '{selected.data.itemName}'을(를) 수첩에 끼웠습니다!");
            
            // 인벤토리에서 사진 삭제
            InventoryManager.Instance.RemoveItem(selected);
            
            // 인스펙터에 연결해 둔 이벤트(UI 이미지 켜기 등) 실행
            OnSuccess?.Invoke(); 
        }
        else
        {
            Debug.Log("[수첩] 맞지 않는 사진이거나 아이템이 선택되지 않았습니다.");
        }
    }
}