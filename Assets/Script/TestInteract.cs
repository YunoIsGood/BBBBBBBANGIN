using UnityEngine;

public class TestInteract : MonoBehaviour, IInteractable
{
    public void OnInteract()
    {
        Debug.Log("이 큐브를 클릭했습니다!");
        // 원한다면 여기에 Destroy(gameObject); 를 넣어 줍는 연출을 테스트할 수도 있습니다.
    }
}