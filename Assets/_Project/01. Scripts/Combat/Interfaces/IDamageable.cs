/// <summary>
/// 대미지를 입을 수 있는 모든 객체가 구현할 인터페이스
/// </summary>
public interface IDamageable
{
    /// <summary>
    /// 대미지 처리 메서드
    /// </summary>
    void TakeDamage(DamageInfo damageInfo);
}