using System.Collections.Generic;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [Header("UI 연결")]
    public Transform slotGrid;      
    public GameObject slotPrefab;   

    // 생성된 슬롯들을 추적하기 위한 리스트
    private List<InventorySlot> activeSlots = new();

    private void Start()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnInventoryChanged += RefreshUI;
            // 선택 이벤트 구독 추가
            InventoryManager.Instance.OnSelectionChanged += UpdateHighlights; 
        }
    }

    private void RefreshUI()
    {
        foreach (Transform child in slotGrid)
        {
            Destroy(child.gameObject);
        }
        activeSlots.Clear(); // 리스트 초기화

        var currentInventory = InventoryManager.Instance.inventory;
        var selectedItem = InventoryManager.Instance.SelectedItem;

        foreach (var item in currentInventory)
        {
            GameObject newSlot = Instantiate(slotPrefab, slotGrid);
            InventorySlot slotScript = newSlot.GetComponent<InventorySlot>();
            
            // ItemData 대신 InventoryItem 객체 전체를 넘깁니다. (현재 선택된 아이템인지 여부도 함께 전달)
            slotScript.SetItem(item, item == selectedItem);
            
            activeSlots.Add(slotScript); // 리스트에 추가
        }
    }

    // 아이템 선택 시 전체 UI를 부수고 새로 만들지 않고, 테두리만 빠르게 껐다 켭니다.
    private void UpdateHighlights(InventoryItem selectedItem)
    {
        foreach (var slot in activeSlots)
        {
            // 이 슬롯의 아이템이 현재 선택된 아이템과 일치하면 하이라이트를 켭니다.
            bool isSelected = (slot.currentItem == selectedItem);
            slot.SetHighlight(isSelected);
        }
    }
}