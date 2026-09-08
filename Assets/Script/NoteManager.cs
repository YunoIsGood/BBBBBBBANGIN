using UnityEngine;
using UnityEngine.InputSystem;

public class NoteManager : MonoBehaviour
{
    public static NoteManager Instance { get; private set; }

    [Header("수첩 UI 연결")]
    public GameObject noteUIPanel; // 화면 중앙에 띄울 수첩 UI 패널 전체

    private bool hasNote = false;  // 수첩을 주웠는지 여부
    private bool isNoteOpen = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // 시작할 땐 수첩 UI를 꺼둡니다.
        if (noteUIPanel != null) noteUIPanel.SetActive(false);
    }

    // 수첩을 처음 바닥에서 주웠을 때 호출되는 함수
    public void AcquireNote()
    {
        hasNote = true;
        ToggleNote(true); // 줍자마자 강제로 화면에 띄움
        Debug.Log("[수첩] 수첩을 획득했습니다!");
    }

    private void Update()
    {
        if (Keyboard.current == null) return;

        // 수첩을 얻은 상태에서만 Tab 키 작동
        if (hasNote && Keyboard.current.tabKey.wasPressedThisFrame)
        {
            // 관찰 모드(Inspecting)일 때는 수첩이 안 열리게 방어
            if (GameManager.Instance.CurrentState == GameState.FreeLook || 
                GameManager.Instance.CurrentState == GameState.NoteView)
            {
                ToggleNote(!isNoteOpen);
            }
        }
    }

    public void ToggleNote(bool show)
    {
        isNoteOpen = show;
        noteUIPanel.SetActive(isNoteOpen);

        if (isNoteOpen)
        {
            GameManager.Instance.CurrentState = GameState.NoteView;
            // 수첩 조작을 위해 커서를 강제로 켬
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            GameManager.Instance.CurrentState = GameState.FreeLook;
            // 수첩을 닫으면 기본 1인칭 상태(커서 숨김)로 복귀
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}