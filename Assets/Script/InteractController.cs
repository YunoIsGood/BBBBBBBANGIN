using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class InteractController : MonoBehaviour
{
    [Header("상호작용 설정")]
    public float interactRange = 3f; 
    
    [Header("드래그 판정 (관찰 모드용)")]
    public float dragThreshold = 10f; // 이 픽셀 수치 이상 마우스가 움직이면 드래그로 판정

    private Camera mainCamera;
    
    // 클릭 vs 드래그 구분을 위한 내부 변수
    private Vector2 mousePressPos;
    private bool isDragging = false;

    void Start()
    {
        mainCamera = GetComponent<Camera>();
        if (mainCamera == null) mainCamera = Camera.main;
    }

    void Update()
    {
        if (Mouse.current == null) return;

        // 1. 마우스를 누른 '순간'
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

            mousePressPos = Mouse.current.position.ReadValue();
            isDragging = false;

            // [FPS 모드] 커서가 잠겨있을 때는 즉각적인 손맛을 위해 누르자마자 바로 레이캐스트 발사!
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                PerformRaycast();
            }
        }

        // 2. 마우스를 '누르고 있는 동안' (드래그 감지)
        if (Mouse.current.leftButton.isPressed && Cursor.lockState != CursorLockMode.Locked)
        {
            // 누른 위치에서 마우스가 임계치(10px) 이상 벗어났다면 '드래그 중'으로 판정
            if (Vector2.Distance(mousePressPos, Mouse.current.position.ReadValue()) > dragThreshold)
            {
                isDragging = true;
            }
        }

        // 3. 마우스를 '뗀 순간' (클릭 판정)
        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            // [커서 모드 & 관찰 모드] 마우스를 뗐을 때, 드래그를 하지 않은 순수 '클릭'일 때만 레이캐스트 발사!
            if (Cursor.lockState != CursorLockMode.Locked && !isDragging)
            {
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;
                PerformRaycast();
            }
        }
    }

    // 실제 레이저를 쏘고 상호작용하는 함수 (중복 코드를 하나로 묶음)
    private void PerformRaycast()
    {
        Ray ray;
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            // FPS 모드: 화면 중앙
            ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        }
        else
        {
            // 커서 모드: 현재 마우스 위치
            ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        }

        if (Physics.Raycast(ray, out RaycastHit hit, interactRange))
        {
            if (hit.collider.TryGetComponent(out IInteractable interactable))
            {
                interactable.OnInteract();
            }
        }
    }
}