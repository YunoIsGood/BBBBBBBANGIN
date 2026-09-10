using System;
using DG.Tweening;
using UnityEngine;

[DisallowMultipleComponent]
public class PullItem : MonoBehaviour, IInteractable
{
    private Camera mainCamera; // pullPosition이 MainCamera 자식임
    [SerializeField] private Transform pullPosition;
    [SerializeField, Min(0.01f)] private float duration = 0.5f;

    public bool IsActive { get; private set; }
    public bool IsReady { get; private set; }
    public bool IsReturning { get; private set; }
    public event Action Finished;

    private Transform anchor;
    private Transform originalParent;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Vector3 originalScale;
    private Tween movement;
    private Collider[] colliders;
    private bool[] colliderStates;
    private BodyState[] bodies;

    private struct BodyState
    {
        public Rigidbody body;
        public bool kinematic;
        public bool gravity;
        public bool collisions;
        public bool sleeping;
        public Vector3 velocity;
        public Vector3 angularVelocity;
    }

    private void Awake()
    {
        mainCamera = Camera.main;
        pullPosition = mainCamera.transform.GetChild(0).transform;
    }

    public void OnInteract()
    {
        if (TryGetComponent(out ItemPickup pickup))
        {
            pickup.OnInteract();
            return;
        }
        InspectManager.Instance.StartPull(transform, pullPosition);
    }

    public bool StartPull(Transform destination)
    {
        if (!isActiveAndEnabled || IsActive) return false;
        anchor = pullPosition != null ? pullPosition : destination;
        if (anchor == null)
        {
            GameObject fallback = GameObject.Find("PullPosition");
            if (fallback != null) anchor = fallback.transform;
        }
        if (anchor == null || anchor.IsChildOf(transform))
        {
            Debug.LogWarning("[PullItem] 아이템 외부의 Pull Position 또는 Inspect Anchor를 지정하세요.", this);
            return false;
        }

        originalParent = transform.parent;
        originalPosition = transform.localPosition;
        originalRotation = transform.localRotation;
        originalScale = transform.localScale;
        SuspendPhysics();

        IsActive = true;
        IsReady = false;
        IsReturning = false;
        movement = transform.DOMove(anchor.position, Mathf.Max(0.01f, duration))
            .SetEase(Ease.OutCubic).SetUpdate(true)
            .OnComplete(() => IsReady = true);
        return true;
    }

    public void Rotate(Vector2 delta, Transform view, float speed)
    {
        if (!IsReady || IsReturning || view == null) return;
        transform.Rotate(view.up, -delta.x * speed, Space.World);
        transform.Rotate(view.right, delta.y * speed, Space.World);
    }

    public void StopPull()
    {
        if (!IsActive || IsReturning) return;
        movement?.Kill();
        IsReady = false;
        IsReturning = true;
        transform.SetParent(originalParent, true);

        float seconds = Mathf.Max(0.01f, duration);
        Sequence returning = DOTween.Sequence();
        returning.Append(transform.DOLocalMove(originalPosition, seconds));
        returning.Join(transform.DOLocalRotateQuaternion(originalRotation, seconds));
        returning.SetEase(Ease.InOutSine).SetUpdate(true);
        returning.OnComplete(() => Finish(true));
        movement = returning;
    }

    // 획득 시 원래 자리로 돌아가는 애니메이션 없이 제어를 넘깁니다.
    public bool ReleaseForPickup()
    {
        if (!IsActive || IsReturning) return false;
        movement?.Kill();
        Finish(false);
        return true;
    }

    public void CancelImmediately()
    {
        if (!IsActive) return;
        movement?.Kill();
        Finish(true);
    }

    private void Finish(bool restorePose)
    {
        if (restorePose)
        {
            transform.SetParent(originalParent, false);
            transform.localPosition = originalPosition;
            transform.localRotation = originalRotation;
            transform.localScale = originalScale;
        }
        RestorePhysics();
        movement = null;
        anchor = null;
        IsActive = false;
        IsReady = false;
        IsReturning = false;
        Finished?.Invoke();
    }

    private void SuspendPhysics()
    {
        Rigidbody[] foundBodies = GetComponentsInChildren<Rigidbody>(true);
        bodies = new BodyState[foundBodies.Length];
        for (int i = 0; i < foundBodies.Length; i++)
        {
            Rigidbody body = foundBodies[i];
            bodies[i] = new BodyState
            {
                body = body, kinematic = body.isKinematic, gravity = body.useGravity,
                collisions = body.detectCollisions, sleeping = body.IsSleeping(),
                velocity = body.linearVelocity, angularVelocity = body.angularVelocity
            };
            body.isKinematic = true;
            body.useGravity = false;
            body.detectCollisions = false;
        }

        colliders = GetComponentsInChildren<Collider>(true);
        colliderStates = new bool[colliders.Length];
        for (int i = 0; i < colliders.Length; i++)
        {
            colliderStates[i] = colliders[i].enabled;
            colliders[i].enabled = false;
        }
    }

    private void RestorePhysics()
    {
        for (int i = 0; i < colliders.Length; i++)
            if (colliders[i] != null) colliders[i].enabled = colliderStates[i];

        foreach (BodyState state in bodies)
        {
            if (state.body == null) continue;
            state.body.isKinematic = state.kinematic;
            state.body.useGravity = state.gravity;
            state.body.detectCollisions = state.collisions;
            if (!state.kinematic)
            {
                state.body.linearVelocity = state.velocity;
                state.body.angularVelocity = state.angularVelocity;
                if (state.sleeping) state.body.Sleep();
            }
        }
        bodies = null;
        colliders = null;
        colliderStates = null;
    }

    private void LateUpdate()
    {
        if (!IsActive || IsReturning) return;
        if (anchor == null)
        {
            CancelImmediately();
            return;
        }
        // 부모를 바꾸지 않고 카메라 앞 위치를 유지해 아이템 스케일을 보존합니다.
        if (IsReady) transform.position = anchor.position;
    }

    private void OnDisable()
    {
        CancelImmediately();
    }
}
