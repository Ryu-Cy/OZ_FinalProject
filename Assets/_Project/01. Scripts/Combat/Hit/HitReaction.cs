using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 피격 시 본을 회전시켜 반응하는 클래스 <br/>
/// 아직 적 애니메이션 적용 전이라 애니메이션 적용 후에 수정될 가능성 높음.
/// </summary>
public class HitReaction : MonoBehaviour
{
    // 피격 시 회전을 줄 본 정보
    [System.Serializable]
    public class ReactBone
    {
        [SerializeField] private string boneName;
        [SerializeField] private Transform boneTransform;

        // 프로퍼티
        public string BoneName => boneName;
        public Transform BoneTransform => boneTransform;

        public Vector3 RotAxis { get; set; }
        public float CurrentAngle { get; set; }
        public float AngleVelocity { get; set; }

        public ReactBone(string name, Transform tf)
        {
            boneName = name;
            boneTransform = tf;
        }
    }

    [Header("Bones To Track")]
    [SerializeField] private List<ReactBone> reactionBones = new List<ReactBone>();

    [Header("Rotation Spring Settings")]
    [Tooltip("복귀 탄성력")]
    [SerializeField] private float stiffness = 160.0f;

    [Tooltip("감쇄력")]
    [SerializeField] private float damping = 14.0f;

    [Tooltip("타격 시 꺾이는 최대 각도(도)")]
    [SerializeField] private float maxAngle = 35.0f;

    [Tooltip("대미지 대비 꺾임 강도 계수")]
    [SerializeField] private float forceMultiplier = 1.2f;

    private void Awake()
    {
        if (reactionBones == null || reactionBones.Count == 0)
        {
            AutoFindBones();
        }
    }

    /// <summary>
    /// 피격 시 반응을 적용하는 함수
    /// </summary>
    /// <param name="hitPoint">맞은 위치</param>
    /// <param name="hitDirection">맞은 방향</param>
    /// <param name="hitAmount">맞은 대미지 양</param>
    public void ApplyHitReaction(Vector3 hitPoint, Vector3 hitDirection, float hitAmount)
    {
        ReactBone closestBone = GetClosestBone(hitPoint);
        if (closestBone == null || closestBone.BoneTransform == null) return;

        // 회전 축 계산
        Vector3 axis = Vector3.Cross(Vector3.up, hitDirection.normalized);
        if (axis.sqrMagnitude < 0.001f)
        {
            axis = Vector3.right;
        }
        closestBone.RotAxis = axis.normalized;

        // 힘을 주는 대신 대미지 양에 비례하여 회전 각도를 결정
        float targetAngle = Mathf.Clamp(hitAmount * forceMultiplier, 10.0f, maxAngle);
        closestBone.CurrentAngle = targetAngle;
        closestBone.AngleVelocity = 0.0f;
    }

    /// <summary>
    /// 맞은 위치에 가장 가까운 본을 찾는 함수
    /// </summary>
    /// <param name="hitPoint">맞은 위치</param>
    /// <returns></returns>
    private ReactBone GetClosestBone(Vector3 hitPoint)
    {
        ReactBone bestBone = null;
        float minSqrDistance = float.MaxValue;

        // 가장 가까운 본을 찾기 위해 모든 본을 순회
        for (int i = 0; i < reactionBones.Count; i++)
        {
            var bone = reactionBones[i];
            if (bone.BoneTransform == null) continue;

            // 본 위치와 맞은 위치 간의 제곱 거리 계산
            float sqrDist = (bone.BoneTransform.position - hitPoint).sqrMagnitude;
            // 가장 가까운 본을 찾기 위해 최소 제곱 거리와 비교
            if (sqrDist < minSqrDistance)
            {
                minSqrDistance = sqrDist;
                bestBone = bone;
            }
        }

        return bestBone;
    }

    private void LateUpdate()
    {
        float dt = Time.deltaTime;
        if (dt <= 0f) return;

        // 각 본에 대해 스프링-댐퍼를 적용하여 회전 반응 계산
        for (int i = 0; i < reactionBones.Count; i++)
        {
            var bone = reactionBones[i];
            if (bone.BoneTransform == null || bone.RotAxis == Vector3.zero) continue;

            // 각도와 각속도가 거의 0에 가까우면 회전 계산을 생략
            if (Mathf.Abs(bone.CurrentAngle) < 0.01f && Mathf.Abs(bone.AngleVelocity) < 0.01f)
            {
                bone.CurrentAngle = 0.0f;
                bone.AngleVelocity = 0.0f;
                continue;
            }

            float previousAngle = bone.CurrentAngle;

            // 스프링-댐퍼 계산
            float springTorque = -stiffness * bone.CurrentAngle - damping * bone.AngleVelocity;
            bone.AngleVelocity += springTorque * dt;
            bone.CurrentAngle += bone.AngleVelocity * dt;

            // 복귀 시 튕기지 않도록 보정
            // 각도가 원위치를 통과하여 반대 부호로 넘어가는 순간 강제 정지
            if ((previousAngle > 0.0f && bone.CurrentAngle <= 0.0f) ||
                (previousAngle < 0.0f && bone.CurrentAngle >= 0.0f))
            {
                bone.CurrentAngle = 0.0f;
                bone.AngleVelocity = 0.0f;
            }

            // 회전 적용
            Quaternion offsetRotation = Quaternion.AngleAxis(bone.CurrentAngle, bone.RotAxis);
            bone.BoneTransform.rotation = offsetRotation * bone.BoneTransform.rotation;
        }
    }

    /// <summary>
    /// 휴머노이드 본을 찾아 등록하는 함수
    /// </summary>
    [ContextMenu("Auto Find Humanoid Bones")]
    public void AutoFindBones()
    {
        if (reactionBones == null)
            reactionBones = new List<ReactBone>();
        else
            reactionBones.Clear();

        var animator = GetComponentInChildren<Animator>();
        if (animator == null || !animator.isHuman) return;

        // 회전 반응에 적합한 상체 부위 위주 등록
        HumanBodyBones[] targetBones = {
            HumanBodyBones.Chest,
            HumanBodyBones.UpperChest,
            HumanBodyBones.Neck,
            HumanBodyBones.Head
        };

        // 각 본을 찾아 ReactBone으로 등록
        foreach (var b in targetBones)
        {
            Transform t = animator.GetBoneTransform(b);
            if (t != null)
            {
                reactionBones.Add(new ReactBone(b.ToString(), t));
            }
        }
    }

    /// <summary>
    /// 락온 조준점 및 상체 기준 피벗으로 활용할 가슴/척추 본 Transform 반환
    /// </summary>
    public Transform ChestTransform
    {
        get
        {
            if (reactionBones == null || reactionBones.Count == 0) return null;

            // Chest 본 우선 탐색
            var chest = reactionBones.Find(b => b.BoneName == HumanBodyBones.Chest.ToString());
            if (chest != null && chest.BoneTransform != null) return chest.BoneTransform;

            // 없으면 UpperChest 탐색
            var upper = reactionBones.Find(b => b.BoneName == HumanBodyBones.UpperChest.ToString());
            if (upper != null && upper.BoneTransform != null) return upper.BoneTransform;

            // 둘 다 없으면 등록된 첫 번째 본 반환
            return reactionBones[0].BoneTransform;
        }
    }
}