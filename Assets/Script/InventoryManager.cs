using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class InventoryItem 
{
    public ItemData data;
    public GameObject inspectInstance;
}

public class InventoryManager : Singleton<InventoryManager>
{
    //public static InventoryManager Instance { get; private set; }

    public List<InventoryItem> inventory = new List<InventoryItem>();
    
    // [추가됨] 현재 선택된 아이템
    public InventoryItem SelectedItem { get; private set; } 

    public event Action OnInventoryChanged;
    // [추가됨] 아이템 선택 상태가 바뀔 때마다 실행될 이벤트
    public event Action<InventoryItem> OnSelectionChanged; 

    private void Awake()
    {
        //DontDestroyOnLoad(gameObject);
        //if (Instance == null) { Instance = this; }
        //else { Destroy(gameObject); }
    }

    public void AddItem(ItemData newData)
    {
        InventoryItem newItem = new InventoryItem();
        newItem.data = newData;

        if (newData.inspectPrefab != null)
        {
            newItem.inspectInstance = Instantiate(newData.inspectPrefab);
            newItem.inspectInstance.transform.SetParent(this.transform);
            newItem.inspectInstance.SetActive(false); 
        }

        inventory.Add(newItem);
        OnInventoryChanged?.Invoke();
    }

    // [추가됨] 슬롯을 클릭했을 때 호출될 함수
    public void SelectItem(InventoryItem itemToSelect)
    {
        // 이미 선택된 아이템을 또 누르면 '선택 해제', 아니면 '새로 선택'
        if (SelectedItem == itemToSelect)
        {
            SelectedItem = null;
        }
        else
        {
            SelectedItem = itemToSelect;
        }

        // 선택 상태가 바뀌었음을 UI에 알림
        OnSelectionChanged?.Invoke(SelectedItem);
        
        Debug.Log(SelectedItem != null ? $"[인벤토리] '{SelectedItem.data.itemName}' 선택됨" : "[인벤토리] 선택 해제됨");
    }

    // (InventoryManager.cs 내부에 추가)
    
    // 아이템 사용/버리기 처리
    public void RemoveItem(InventoryItem itemToRemove)
    {
        if (inventory.Contains(itemToRemove))
        {
            // 1. 만약 3D 관찰용으로 숨겨둔 모델링이 있다면 그것도 완전히 파괴
            if (itemToRemove.inspectInstance != null)
            {
                Destroy(itemToRemove.inspectInstance);
            }
            
            // 2. 만약 방금 사용한 게 '현재 선택된 아이템'이었다면 선택 상태 해제
            if (SelectedItem == itemToRemove)
            {
                SelectItem(null); 
            }

            // 3. 리스트에서 제거 후 UI 새로고침 방송
            inventory.Remove(itemToRemove);
            OnInventoryChanged?.Invoke();
            
            Debug.Log($"[인벤토리] '{itemToRemove.data.itemName}' 제거됨.");
        }
    }
}