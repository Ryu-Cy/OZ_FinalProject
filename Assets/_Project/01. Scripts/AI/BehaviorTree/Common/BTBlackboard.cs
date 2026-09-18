using System;
using UnityEngine;

/// <summary>
/// Behavior Tree에서 공통적으로 사용되는 데이터를 저장하는 블랙보드 베이스 클래스
/// </summary>
[Serializable]
public class BTBlackboard
{
    [Header("공통 기본 정보")]
    [SerializeField] protected Transform owner;

    // 프로퍼티
    public Transform Owner
    {
        get => owner;
        private set => owner = value;
    }

    /// <summary>
    /// AI 본체 저장
    /// </summary>
    public virtual void Initialize(Transform owner)
    {
        this.owner = owner;
    }

    /// <summary>
    /// 데이터 갱신
    /// </summary>
    public virtual void Update()
    {
    }
}