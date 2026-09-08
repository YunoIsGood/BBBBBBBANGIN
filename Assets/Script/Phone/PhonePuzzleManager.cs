using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro; // TextMeshPro 필수

[System.Serializable]
public class PhoneResponse
{
    public string phoneNumber;
    [TextArea(2, 4)]
    public string[] dialogueLines;
    public UnityEvent onCallComplete; 
}

public class PhonePuzzleManager : MonoBehaviour
{
    public static PhonePuzzleManager Instance { get; private set; }

    [Header("전화번호부 세팅")]
    public List<PhoneResponse> phoneBook = new List<PhoneResponse>();

    [Header("UI 연결")]
    public TextMeshProUGUI dialogueText; // 화면 하단에 크게 뜨는 자막 (Canvas용)
    
    // 👇 3D Text (TextMeshPro)를 넣을 수 있도록 TMP_Text로 변경된 부분
    public TMP_Text lcdText;             

    [Header("타이핑 연출 설정")]
    public float typingSpeed = 0.05f;      
    public float delayBetweenLines = 1.5f; 

    [Header("현재 상태 (디버깅용)")]
    public bool isHandsetLifted = false;
    public string currentInput = "";
    private bool isCallActive = false; 

    [Header("공통 이벤트")]
    public UnityEvent OnFailure; 
    public UnityEvent OnNumberPressed; 

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (dialogueText != null) dialogueText.text = "";
        if (lcdText != null) lcdText.text = ""; // 시작할 땐 LCD 화면 끄기
    }

    public void ToggleHandset()
    {
        if (isCallActive) return; 

        isHandsetLifted = !isHandsetLifted;
        
        if (isHandsetLifted)
        {
            Debug.Log("[공중전화] 수화기를 들었습니다.");
            if (dialogueText != null) dialogueText.text = "";
            UpdateLCD("");
        }
        else
        {
            currentInput = "";
            Debug.Log("[공중전화] 수화기를 내려놓았습니다.");
            if (dialogueText != null) dialogueText.text = "";
            UpdateLCD(""); 
        }
    }

    public void PressButton(string buttonValue)
    {
        if (!isHandsetLifted || isCallActive) return;

        if (buttonValue == "*")
        {
            currentInput = "";
            OnNumberPressed?.Invoke();
            UpdateLCD(""); // 화면 초기화
        }
        else if (buttonValue == "#")
        {
            CheckPhoneNumber();
        }
        else
        {
            currentInput += buttonValue;
            OnNumberPressed?.Invoke();
            UpdateLCD(currentInput); // 누를 때마다 LCD에 번호 표시
        }
    }

    private void CheckPhoneNumber()
    {
        UpdateLCD("CALLING..."); // 통화 연결 중 표시

        PhoneResponse response = phoneBook.Find(x => x.phoneNumber == currentInput);

        if (response != null)
        {
            StartCoroutine(PlayDialogueRoutine(response));
        }
        else
        {
            OnFailure?.Invoke();
            currentInput = ""; 
            UpdateLCD("ERROR"); // 없는 번호일 때 에러 표시
        }
    }

    private IEnumerator PlayDialogueRoutine(PhoneResponse response)
    {
        isCallActive = true;
        UpdateLCD("CONNECTED"); // 통화 연결됨 표시
        
        if (dialogueText != null) dialogueText.text = "";

        foreach (string line in response.dialogueLines)
        {
            dialogueText.text = ""; 
            
            foreach (char c in line)
            {
                dialogueText.text += c;
                yield return new WaitForSeconds(typingSpeed); 
            }

            yield return new WaitForSeconds(delayBetweenLines);
        }

        if (dialogueText != null) dialogueText.text = "";
        
        response.onCallComplete?.Invoke(); 
        
        isCallActive = false;
        currentInput = ""; 
        UpdateLCD("END"); // 통화 종료 표시
    }

    private void UpdateLCD(string msg)
    {
        if (lcdText != null)
        {
            lcdText.text = msg;
        }
    }
}