using UnityEngine;
using System;

// 게임의 4가지 핵심 상태
public enum GameState { FreeLook, FocusView, InspectView, NoteView, Inspecting}


public class GameManager : Singleton<GameManager>
{
    public GameState CurrentState { get; set; }

    [Header("Inspect Item")]
    public bool IsPull { get; private set; } = false;
    public bool IsApproach { get; private set; } = false;
    public event Action<GameState> OnStateChanged;

    private void Start()
    {
        ChangeState(GameState.FreeLook);
    }
    
    public void ChangeState(GameState newState)
    {
        CurrentState = newState;
        OnStateChanged?.Invoke(newState);
        Debug.Log($"[시스템] 게임 상태 변경됨: {CurrentState}");
    }
}