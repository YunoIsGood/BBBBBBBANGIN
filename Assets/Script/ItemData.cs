using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item Data")]
public class ItemData : ScriptableObject
{
    [Header("기본 정보")]
    public string itemID;         // 시스템에서 아이템을 구분할 고유 ID (예: "Coin_1998")
    public string itemName;       // 인벤토리 UI에 표시될 이름 (예: "1998년도 동전")
    public Sprite itemIcon;       // 인벤토리 슬롯에 들어갈 2D 이미지

    [Header("3D 관찰 모델 (선택)")]
    // 지갑처럼 분해 기믹이 있거나 돌려볼 수 있는 3D 프리팹.
    // 줍는 순간 한 번만 Instantiate 되어 가방 속에 상태가 유지된 채 보관됩니다.
    public GameObject inspectPrefab; 
}