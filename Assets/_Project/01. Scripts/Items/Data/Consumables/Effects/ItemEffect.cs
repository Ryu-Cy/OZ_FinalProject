using UnityEngine;

/// <summary>
/// 아이템 효과를 정의하는 추상 클래스
/// </summary>
public abstract class ItemEffect : ScriptableObject
{
    /// <summary>
    /// 아이템 효과 실행
    /// </summary>
    /// <param name="user">아이템을 사용하는 주체</param>
    public abstract bool Execute(GameObject user);
}