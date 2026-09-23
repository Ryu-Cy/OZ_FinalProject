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

    // 메뉴 토글 및 커서 상태 변경 이벤트
    public event Action<bool> OnMenuStateChanged;
    public event Action<bool> OnCursorLockChanged;

    public void Initialize() { }

    protected override void Awake()
    {
        base.Awake();
    }

    private void OnEnable()
    {
        if (SceneLoadManager.Instance != null)
        {
            SceneLoadManager.Instance.OnSceneLoadCompleted += HandleSceneLoadCompleted;
        }

        BindPlayerInput();
    }

    private void Start()
    {
        if (SceneLoadManager.Instance != null)
        {
            SceneLoadManager.Instance.OnSceneLoadCompleted -= HandleSceneLoadCompleted;
            SceneLoadManager.Instance.OnSceneLoadCompleted += HandleSceneLoadCompleted;

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

    private void HandleSceneLoadCompleted(SceneType loadedScene)
    {
        isMenuOpened = false;

        if (loadedScene == SceneType.MainGame
//#if UNITY_EDITOR
            || loadedScene == SceneType.Test
//#endif
           )
        {
            BindPlayerInput();
            StartCoroutine(CoEnsureCursorLockedOnStart());
        }
        else
        {
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
            if (SceneLoadManager.Instance != null &&
                (SceneLoadManager.Instance.CurrentScene == SceneType.MainGame
//#if UNITY_EDITOR
                || SceneLoadManager.Instance.CurrentScene == SceneType.Test
//#endif
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

        if (Cursor.lockState != CursorLockMode.Locked)
        {
            SetCursorLock(true);
        }
    }

    /// <summary>
    /// 마우스 커서 잠금/해제 및 상태 변경 이벤트 전달
    /// </summary>
    public void SetCursorLock(bool isLocked)
    {
        Cursor.lockState = isLocked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !isLocked;

        // 커서 잠금 상태 변경 알림
        OnCursorLockChanged?.Invoke(isLocked);
    }
}