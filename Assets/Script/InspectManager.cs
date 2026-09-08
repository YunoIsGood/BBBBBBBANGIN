using UnityEngine;
using UnityEngine.InputSystem;

public class InspectManager : Singleton<InspectManager>
{
    //public static InspectManager Instance { get; private set; }

    [Header("관찰 설정")]
    public float rotationSpeed = 0.5f;   // 드래그 회전 속도
    public Transform inspectAnchor;      // 3D 모델이 띄워질 위치 (카메라 앞)

    private GameObject currentInspectObject;

    private void Awake()
    {
        //if (Instance == null) Instance = this;
        //else Destroy(gameObject);
    }

    public void StartInspect(InventoryItem item)
    {
        if (item.inspectInstance == null)
        {
            if (GameManager.Instance.CurrentState == GameState.Inspecting)
            {
                StopInspect();
            }
            return;
        }

        if (currentInspectObject != null && currentInspectObject != item.inspectInstance)
        {
            currentInspectObject.SetActive(false);
        }

        GameManager.Instance.CurrentState = GameState.Inspecting;
        currentInspectObject = item.inspectInstance;

        // [핵심 변경점] 이 물건을 처음 관찰하는 거라면 부모를 카메라(Anchor)로 바꿉니다.
        if (currentInspectObject.transform.parent != inspectAnchor)
        {
            // 부모를 Anchor로 설정 (false: 프리팹 고유의 로컬 회전값을 그대로 유지함)
            currentInspectObject.transform.SetParent(inspectAnchor, false);
            
            // 위치만 앵커의 정중앙(0,0,0)으로 맞추고, 회전값은 절대 강제로 덮어씌우지 않습니다!
            currentInspectObject.transform.localPosition = Vector3.zero;
        }
        
        currentInspectObject.SetActive(true);
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void StopInspect()
    {
        if (currentInspectObject != null)
        {
            // 모델을 파괴하지 않고 다시 숨김 (상태 유지의 핵심!)
            currentInspectObject.SetActive(false);
            currentInspectObject = null;
        }

        // 상태를 다시 기본 탐색 모드로 복귀
        GameManager.Instance.CurrentState = GameState.FreeLook;
    }

    private void Update()
    {
        // 관찰 모드가 아니거나, 띄워진 물건이 없으면 무시
        if (GameManager.Instance.CurrentState != GameState.Inspecting || currentInspectObject == null) return;

        // [종료 조건] 우클릭이나 ESC를 누르면 관찰 종료
        if (Mouse.current.rightButton.wasPressedThisFrame || Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            StopInspect();
            return;
        }

        // [회전 조작] 좌클릭을 '유지(꾹 누름)'한 상태로 마우스를 움직이면 회전
        if (Mouse.current.leftButton.isPressed)
        {
            Vector2 delta = Mouse.current.delta.ReadValue();
            
            // 카메라가 바라보는 방향(상하좌우)을 기준으로 자연스럽게 회전
            currentInspectObject.transform.Rotate(Camera.main.transform.up, -delta.x * rotationSpeed, Space.World);
            currentInspectObject.transform.Rotate(Camera.main.transform.right, delta.y * rotationSpeed, Space.World);
        }
    }
}