using System;
using UnityEngine;

/// <summary>
/// 애니메이션 클립 이벤트 수신 및 외부에 브로드캐스팅하는 컴포넌트
/// </summary>
public class PlayerAnimationEventController : MonoBehaviour
{
    // 공격 판정 및 콤보 이벤트
    public event Action OnAttackHitStart;
    public event Action OnAttackHitEnd;
    public event Action OnComboInputEnd;

    /// <summary>
    /// 무기 히트박스 활성화
    /// </summary>
    public void AttackHitStart()
    {
        OnAttackHitStart?.Invoke();
    }

    /// <summary>
    /// 무기 히트박스 비활성화
    /// </summary>
    public void AttackHitEnd()
    {
        OnAttackHitEnd?.Invoke();
    }

    /// <summary>
    /// 콤보 입력 허용 종료
    /// </summary>
    public void AttackComboInputEnd()
    {
        OnComboInputEnd?.Invoke();
    }
}
