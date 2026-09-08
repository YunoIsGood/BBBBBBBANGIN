using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // 클릭 감지를 위해 필수

// IPointerClickHandler를 상속받아 마우스 클릭을 감지합니다.
public class InventorySlot : MonoBehaviour, IPointerClickHandler
{
    [Header("UI 컴포넌트")]
    public Image iconImage;
    public Image highlightImage; // 선택되었을 때 켜질 테두리 이미지

    // 이 슬롯이 담당하고 있는 아이템 정보
    public InventoryItem currentItem { get; private set; } 

    public void SetItem(InventoryItem item, bool isSelected)
    {
        currentItem = item;
        if (item != null && item.data != null)
        {
            iconImage.sprite = item.data.itemIcon;
            iconImage.enabled = true;
            SetHighlight(isSelected);
        }
        else
        {
            ClearSlot();
        }
    }

    public void SetHighlight(bool isSelected)
    {
        if (highlightImage != null)
        {
            highlightImage.enabled = isSelected;
        }
    }

    public void ClearSlot()
    {
        currentItem = null;
        iconImage.sprite = null;
        iconImage.enabled = false;
        SetHighlight(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (currentItem == null) return;

        // 좌클릭: 아이템 선택
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            // 기존처럼 아이템 선택 (노란 테두리 켜기)
            InventoryManager.Instance.SelectItem(currentItem);

            // [추가됨] 만약 이미 관찰 모드 중이라면 즉시 관찰 화면 교체
            if (GameManager.Instance.CurrentState == GameState.Inspecting)
            {
                InspectManager.Instance.StartInspect(currentItem);
            }
        }
        // 우클릭: 관찰 모드 최초 진입
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            InspectManager.Instance.StartInspect(currentItem);
        }
    }
}