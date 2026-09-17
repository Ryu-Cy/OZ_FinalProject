using UnityEngine;

/// <summary>
/// 피격 시 전달할 상세 대미지 정보
/// </summary>
public struct DamageInfo
{
    public float Amount;           // 대미지 수치
    public Vector3 HitPoint;       // 타격된 월드 좌표
    public Vector3 HitNormal;      // 피격 표면의 법선 벡터
    public Vector3 HitDirection;   // 공격이 들어온 방향
    public GameObject Instigator;  // 공격을 가한 주체

    public DamageInfo(float amount, Vector3 hitPoint, Vector3 hitNormal, Vector3 hitDirection, GameObject instigator = null)
    {
        Amount = amount;
        HitPoint = hitPoint;
        HitNormal = hitNormal;
        HitDirection = hitDirection;
        Instigator = instigator;
    }
}