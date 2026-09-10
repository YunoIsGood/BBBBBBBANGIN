using System;
using DG.Tweening;
using Unity.Cinemachine;
using UnityEngine;

[DisallowMultipleComponent]
public class ApproachItem : MonoBehaviour
{
    [SerializeField] private Camera approachCamera;
    [SerializeField, Min(0.01f)] private float duration = 0.5f;

    public bool IsActive { get; private set; }
    public bool IsReturning { get; private set; }
    public event Action<bool> Returned;

    private Transform cameraTransform;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Sequence movement;
    private CinemachineBrain brain;
    private bool brainWasEnabled;

    public bool StartApproach(Transform destination)
    {
        if (!isActiveAndEnabled || IsActive || destination == null) return false;

        Camera targetCamera = approachCamera != null ? approachCamera : Camera.main;
        if (targetCamera == null || destination.IsChildOf(targetCamera.transform))
        {
            Debug.LogWarning("접근 관찰에는 카메라와 카메라 외부의 목적지가 필요합니다.", this);
            return false;
        }

        cameraTransform = targetCamera.transform;
        originalPosition = cameraTransform.localPosition;
        originalRotation = cameraTransform.localRotation;
        
        brain = targetCamera.GetComponent<CinemachineBrain>();
        brainWasEnabled = brain != null && brain.enabled;
        if (brainWasEnabled) brain.enabled = false;

        IsActive = true;
        IsReturning = false;
        float seconds = Mathf.Max(0.01f, duration);
        movement = DOTween.Sequence();
        movement.Append(cameraTransform.DOMove(destination.position, seconds));
        movement.Join(cameraTransform.DORotateQuaternion(destination.rotation, seconds));
        movement.SetEase(Ease.InOutSine).SetUpdate(true);
        return true;
    }

    public void StopApproach()
    {
        if (!IsActive || IsReturning) return;

        movement?.Kill();
        IsReturning = true;
        if (cameraTransform == null)
        {
            FinishReturn(false);
            return;
        }

        float seconds = Mathf.Max(0.01f, duration);
        movement = DOTween.Sequence();
        movement.Append(cameraTransform.DOLocalMove(originalPosition, seconds));
        movement.Join(cameraTransform.DOLocalRotateQuaternion(originalRotation, seconds));
        movement.SetEase(Ease.InOutSine).SetUpdate(true);
        movement.OnComplete(() => FinishReturn(true));
    }

    // 관찰 관리자나 이 컴포넌트가 비활성화되어도 카메라 제어를 복원합니다.
    public void CancelImmediately()
    {
        if (!IsActive) return;
        movement?.Kill();
        FinishReturn(false);
    }

    private void FinishReturn(bool completed)
    {
        if (cameraTransform != null)
        {
            cameraTransform.localPosition = originalPosition;
            cameraTransform.localRotation = originalRotation;
        }
        if (brain != null) brain.enabled = brainWasEnabled;

        movement = null;
        cameraTransform = null;
        brain = null;
        IsActive = false;
        IsReturning = false;
        Returned?.Invoke(completed);
    }

    private void OnDisable()
    {
        CancelImmediately();
    }
}
