using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// 게임의 순수 진행 상태와 라이프사이클을 통괄하는 코어 매니저 클래스.
/// </summary>
public class GameManager : Singleton<GameManager>, IInitializable
{
    [Header("References")]
    [SerializeField] private PlayerInputController playerInput;

    [Header("State")]
    [SerializeField] private bool isMenuOpened = false;

    public bool IsMenuOpened => isMenuOpened;

    // UI 매니저 등 외부에서 메뉴 토글을 알 수 있는 이벤트
    public event Action<bool> OnMenuStateChanged;

    public void Initialize()
    {
        // CoreSystems 순차 초기화 진입점
    }

    protected override void Awake()
    {
        base.Awake();
    }

    private void OnEnable()
    {
        // 씬 로드 완료 이벤트 구독
        if (SceneLoadManager.Instance != null)
        {
            SceneLoadManager.Instance.OnSceneLoadCompleted += HandleSceneLoadCompleted;
        }

        BindPlayerInput();
    }

    private void Start()
    {
        // 씬 매니저 구독 안전 보정 및 최초 씬 상태 평가
        if (SceneLoadManager.Instance != null)
        {
            SceneLoadManager.Instance.OnSceneLoadCompleted -= HandleSceneLoadCompleted;
            SceneLoadManager.Instance.OnSceneLoadCompleted += HandleSceneLoadCompleted;

            // 현재 시작된 씬이 인게임 또는 테스트 씬인지 즉시 판별
            HandleSceneLoadCompleted(SceneLoadManager.Instance.CurrentScene);
        }
    }

    private void OnDisable()
    {
        if (SceneLoadManager.Instance != null)
        {
            SceneLoadManager.Instance.OnSceneLoadCompleted -= HandleSceneLoadCompleted;
        }

        UnbindPlayerInput();
    }

    /// <summary>
    /// 씬 로드가 끝났을 때 씬 타입에 맞춰 커서 및 플레이어 입력을 자동 갱신
    /// </summary>
    private void HandleSceneLoadCompleted(SceneType loadedScene)
    {
        isMenuOpened = false;

        // 인게임 혹은 에디터 테스트 씬 진입 시 커서 잠금
        if (loadedScene == SceneType.MainGame
#if UNITY_EDITOR
            || loadedScene == SceneType.Test
#endif
           )
        {
            BindPlayerInput();
            StartCoroutine(CoEnsureCursorLockedOnStart());
        }
        else
        {
            // 타이틀이나 로딩 화면 등에서는 커서 해제
            UnbindPlayerInput();
            SetCursorLock(false);
        }
    }

    private void BindPlayerInput()
    {
        UnbindPlayerInput();

        playerInput = FindFirstObjectByType<PlayerInputController>();

        if (playerInput != null)
        {
            playerInput.OnEscapeTriggered += HandleEscapeTriggered;
            playerInput.OnAttackTriggered += HandleAttackTriggered;
        }
    }

    private void UnbindPlayerInput()
    {
        if (playerInput != null)
        {
            playerInput.OnEscapeTriggered -= HandleEscapeTriggered;
            playerInput.OnAttackTriggered -= HandleAttackTriggered;
            playerInput = null;
        }
    }

    private IEnumerator CoEnsureCursorLockedOnStart()
    {
        for (int i = 0; i < 5; i++)
        {
            SetCursorLock(true);
            yield return null;
        }
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus && !isMenuOpened)
        {
            // 현재 씬이 인게임 상태일 때만 포커스 복귀 시 잠금 체결
            if (SceneLoadManager.Instance != null &&
                (SceneLoadManager.Instance.CurrentScene == SceneType.MainGame
#if UNITY_EDITOR
                || SceneLoadManager.Instance.CurrentScene == SceneType.Test
#endif
                ))
            {
                SetCursorLock(true);
            }
        }
    }

    private void HandleEscapeTriggered()
    {
        isMenuOpened = !isMenuOpened;
        SetCursorLock(!isMenuOpened);

        OnMenuStateChanged?.Invoke(isMenuOpened);
    }

    private void HandleAttackTriggered()
    {
        if (isMenuOpened) return;

        // 에디터 등에서 포커스가 풀려있다가 첫 공격 클릭 시 커서 즉시 잠금
        if (Cursor.lockState != CursorLockMode.Locked)
        {
            SetCursorLock(true);
        }
    }

    public void SetCursorLock(bool isLocked)
    {
        Cursor.lockState = isLocked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !isLocked;
    }
}