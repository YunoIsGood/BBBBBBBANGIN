using UnityEngine;
using System.Collections;
using Coffee.UIEffects; // UIEffect 컴포넌트 제어용

public class ImagePatch : MonoBehaviour
{
    [Header("디졸브 설정")]
    public float duration = 1.5f; // 나타나는 데 걸리는 시간 (초)

    private UIEffect uiEffect;

    void Awake()
    {
        // 시작할 때 자신의 UIEffect 컴포넌트를 찾아 변수에 저장
        uiEffect = GetComponent<UIEffect>();

        // 시작 시 자동으로 Transition Rate를 1로 만들어 화면에서 숨김
        if (uiEffect != null)
        {
            uiEffect.transitionRate = 1f;
        }
    }

    // UIRequireItem의 OnSuccess 이벤트에서 호출할 공개 함수
    public void ShowImage()
    {
        // 만약 게임오브젝트가 꺼져있었다면 켜줌
        gameObject.SetActive(true);
        
        // 디졸브 코루틴 시작
        StartCoroutine(DissolveRoutine());
    }

    private IEnumerator DissolveRoutine()
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            
            // 1에서 0으로 시간에 맞춰 부드럽게 값 변경
            float currentRate = Mathf.Lerp(1f, 0f, elapsedTime / duration);
            
            if (uiEffect != null)
            {
                uiEffect.transitionRate = currentRate;
            }
            
            yield return null; // 다음 프레임까지 대기
        }

        // 코루틴이 끝난 후 최종값을 정확히 0으로 맞춤
        if (uiEffect != null)
        {
            uiEffect.transitionRate = 0f;
        }
    }
}