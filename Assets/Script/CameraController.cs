using System;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [Header("시야 회전 설정")]
    public float sensitivity = 0.1f;    // 마우스 감도 (0.05 ~ 0.2 권장)
    public float maxYAngle = 70f;       // 상하 고개 꺾임 제한

    private float rotationX = 0f;
    private float rotationY = 0f;

    // 토글 상태 저장용 변수
    private bool isCursorMode = false;

    // 마우스 복귀 스파이크 방지용 플래그
    private bool skipNextFrameDelta = false;
    private bool wasFreeLook = true;

    void Start()
    {
        Vector3 rot = transform.localRotation.eulerAngles;
        rotationY = rot.y;
        rotationX = rot.x;

        Cursor.lockState = CursorLockMode.Locked;

        // 시작 시 1인칭 모드(커서 숨김)로 초기화
        SetCursorMode(false);
    }

    void Update()
    {
        if (Keyboard.current == null || Mouse.current == null) return;


        if (GameManager.Instance.CurrentState == GameState.FreeLook)
        {
            // 관찰 종료 시 복원된 커서 상태와 내부 토글 상태를 맞춥니다.
            if (!wasFreeLook)
            {
                isCursorMode = Cursor.lockState != CursorLockMode.Locked;
                skipNextFrameDelta = true;
            }
            wasFreeLook = true;
            // 1. 스페이스바를 '딸깍' 누른 순간 상태 반전 (토글)
            if (Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                isCursorMode = !isCursorMode;
                SetCursorMode(isCursorMode);

                // 마우스가 화면 중앙으로 강제 복귀할 때 화면이 튀는 현상 방지
                if (!isCursorMode)
                {
                    skipNextFrameDelta = true;
                }
            }

            // 2. 커서 모드가 아닐 때(1인칭 모드)만 카메라 회전 적용
            if (!isCursorMode)
            {
                // 스파이크 방지 프레임 건너뛰기
                if (skipNextFrameDelta)
                {
                    skipNextFrameDelta = false;
                    return;
                }

                Vector2 mouseDelta = Mouse.current.delta.ReadValue();

                float mouseX = mouseDelta.x * sensitivity;
                float mouseY = mouseDelta.y * sensitivity;

                rotationY += mouseX;
                rotationX -= mouseY;

                rotationX = Mathf.Clamp(rotationX, -maxYAngle, maxYAngle);

                transform.localRotation = Quaternion.Euler(rotationX, rotationY, 0f);
            }
        }
        else
        {
            wasFreeLook = false;
            // 수첩을 펴거나 공중전화로 줌인하는 등 특수 상태로 넘어가면
            // 나중을 위해 토글 상태를 1인칭 모드로 초기화해 둠
            isCursorMode = false;
        }
    }

    // 커서 표시 및 잠금 제어 함수
    private void SetCursorMode(bool showCursor)
    {
        Cursor.lockState = showCursor ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = showCursor;

        // [Tip] 나중에 인벤토리 UI 게임오브젝트를 끄고 켜는 코드를 이 줄 아래에 추가하면 됩니다.
    }
}
