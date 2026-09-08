using UnityEngine;
using System;

// 게임의 4가지 핵심 상태
public enum GameState { FreeLook, FocusView, InspectView, NoteView, Inspecting }

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public GameState CurrentState { get; set; }

    // 상태가 바뀔 때 다른 스크립트들에게 알려주는 방송국(이벤트) 역할
    public event Action<GameState> OnStateChanged;

    private void Awake()
    {
        // 씬에 GameManager가 하나만 존재하도록 강제하는 안전장치
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }
    }

    private void Start()
    {
        // 게임 시작 시 기본 상태로 설정
        ChangeState(GameState.FreeLook);
    }

    // 상태를 바꿀 때는 무조건 이 함수만 사용합니다.
    public void ChangeState(GameState newState)
    {
        CurrentState = newState;
        OnStateChanged?.Invoke(newState);
        Debug.Log($"[시스템] 게임 상태 변경됨: {CurrentState}");
    }
}