using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class InspectManager : Singleton<InspectManager>
{
    public enum InspectState {Pull, Approach}

    [Header("관찰 설정")]
    public float rotationSpeed = 0.5f;   // 드래그 회전 속도
    public Transform inspectAnchor;      // 3D 모델이 띄워질 위치 (카메라 앞)

    [SerializeField] private ApproachItem approach;
    private bool approachSession;
    private bool collectAfterReturn;
    private GameState stateBeforeApproach;
    private CursorLockMode cursorBeforeApproach;
    private bool cursorVisibleBeforeApproach;
    private GameManager approachGameManager;
    private PullItem activePull;
    private GameManager pullGameManager;
    private GameState stateBeforePull;
    private CursorLockMode cursorBeforePull;
    private bool cursorVisibleBeforePull;

    private GameObject currentInspectObject;
    private ItemPickup currentPickup;

    public void ConfirmPickup()
    {
        if (currentPickup == null) return;

        if (activePull != null)
        {
            ItemPickup pickup = currentPickup;
            if (activePull.ReleaseForPickup()) pickup.Collect();
            return;
        }

        if (approachSession)
        {
            if (approach.IsReturning) return;
            collectAfterReturn = true;
            StopApproach();
            return;
        }

        ItemPickup target = currentPickup;

        // StopInspect가 currentPickup을 비우므로 먼저 보관합니다.
        StopInspect();

        target.Collect();
    }

    public void StartWorldInspect(ItemPickup target)
    {
        if (target == null || target.itemData == null || approachSession || activePull != null) return;
        if (target.inspectMode == InspectState.Pull)
        {
            StartPull(target.transform, inspectAnchor);
            return;
        }

        if (StartApproach(target.approachPoint))
        {
            currentPickup = target;
        }
    }

    public void StartInspect(InventoryItem item)
    {
        // 카메라가 복귀하기 전에 인벤토리 관찰로 전환하지 않습니다.
        if (approachSession || activePull != null) return;
        if (item == null || item.inspectInstance == null)
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

        GameManager.Instance.ChangeState(GameState.Inspecting);
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
        if (activePull != null)
        {
            StopPull();
            return;
        }
        if (approachSession)
        {
            StopApproach();
            return;
        }

        if (currentInspectObject != null)
        {
            // 모델을 파괴하지 않고 다시 숨김 (상태 유지의 핵심!)
            currentInspectObject.SetActive(false);
            currentInspectObject = null;
        }

        // 상태를 다시 기본 탐색 모드로 복귀
        GameManager.Instance.ChangeState(GameState.FreeLook);
    }


    public bool StartApproach(Transform destination)
    {
        if (!isActiveAndEnabled || approachSession || activePull != null || destination == null ||
            GameManager.Instance.CurrentState != GameState.FreeLook) return false;

        // 기존 씬에도 별도 수동 연결 없이 사용할 수 있습니다.
        if (approach == null) approach = GetComponent<ApproachItem>();
        if (approach == null) approach = gameObject.AddComponent<ApproachItem>();
        if (!approach.StartApproach(destination)) return false;

        stateBeforeApproach = GameManager.Instance.CurrentState;
        approachGameManager = GameManager.Instance;
        cursorBeforeApproach = Cursor.lockState;
        cursorVisibleBeforeApproach = Cursor.visible;
        approachSession = true;
        collectAfterReturn = false;
        approach.Returned += OnApproachReturned;

        GameManager.Instance.ChangeState(GameState.Inspecting);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        return true;
    }

    public void StopApproach()
    {
        if (approachSession) approach.StopApproach();
    }

    private void OnApproachReturned(bool completed)
    {
        approach.Returned -= OnApproachReturned;
        ItemPickup itemToCollect = completed && collectAfterReturn ? currentPickup : null;
        currentPickup = null;
        approachSession = false;
        collectAfterReturn = false;
        Cursor.lockState = cursorBeforeApproach;
        Cursor.visible = cursorVisibleBeforeApproach;
        if (approachGameManager != null)
            approachGameManager.ChangeState(stateBeforeApproach);
        approachGameManager = null;
        if (itemToCollect != null) itemToCollect.Collect();
    }

    private void OnDisable()
    {
        // 강제 종료/비활성화는 획득 확정으로 처리하지 않습니다.
        collectAfterReturn = false;
        if (approachSession && approach != null) approach.CancelImmediately();
        if (activePull != null) activePull.CancelImmediately();
    }

    public bool StartPull(Transform target, Transform destination)
    {
        if (!isActiveAndEnabled || target == null || activePull != null || approachSession ||
            GameManager.Instance.CurrentState != GameState.FreeLook) return false;

        PullItem pull = target.GetComponent<PullItem>();
        if (pull == null) pull = target.gameObject.AddComponent<PullItem>();
        if (!pull.StartPull(destination != null ? destination : inspectAnchor)) return false;

        activePull = pull;
        currentPickup = target.GetComponent<ItemPickup>();
        pullGameManager = GameManager.Instance;
        stateBeforePull = pullGameManager.CurrentState;
        cursorBeforePull = Cursor.lockState;
        cursorVisibleBeforePull = Cursor.visible;
        pull.Finished += OnPullFinished;
        pullGameManager.ChangeState(GameState.Inspecting);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        return true;
    }

    public void StopPull()
    {
        if (activePull != null) activePull.StopPull();
    }

    private void OnPullFinished()
    {
        if (activePull != null) activePull.Finished -= OnPullFinished;
        activePull = null;
        currentPickup = null;
        Cursor.lockState = cursorBeforePull;
        Cursor.visible = cursorVisibleBeforePull;
        if (pullGameManager != null) pullGameManager.ChangeState(stateBeforePull);
        pullGameManager = null;
    }

    private void Update()
    {
        if (activePull != null)
        {
            if ((Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame) ||
                (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame))
            {
                StopPull();
                return;
            }
            if (Mouse.current != null && Mouse.current.leftButton.isPressed &&
                Camera.main != null &&
                (EventSystem.current == null || !EventSystem.current.IsPointerOverGameObject()))
                activePull.Rotate(Mouse.current.delta.ReadValue(), Camera.main.transform, rotationSpeed);
            return;
        }
        if (approachSession)
        {
            if ((Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame) ||
                (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame))
                StopApproach();
            return;
        }

        // 관찰 모드가 아니거나, 띄워진 물건이 없으면 무시
        if (GameManager.Instance.CurrentState != GameState.Inspecting || currentInspectObject == null) return;

        // [종료 조건] 우클릭이나 ESC를 누르면 관찰 종료
        if ((Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame) ||
            (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame))
        {
            StopInspect();
            return;
        }

        // [회전 조작] 좌클릭을 '유지(꾹 누름)'한 상태로 마우스를 움직이면 회전
        if (Mouse.current != null && Mouse.current.leftButton.isPressed && Camera.main != null)
        {
            Vector2 delta = Mouse.current.delta.ReadValue();

            // 카메라가 바라보는 방향(상하좌우)을 기준으로 자연스럽게 회전
            currentInspectObject.transform.Rotate(Camera.main.transform.up, -delta.x * rotationSpeed, Space.World);
            currentInspectObject.transform.Rotate(Camera.main.transform.right, delta.y * rotationSpeed, Space.World);
        }
    }
}
