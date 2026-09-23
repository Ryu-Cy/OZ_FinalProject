using UnityEngine;

/// <summary>
/// Enemy 전용 BT 루트 노드
/// </summary>
public class EnemyBTRootNode : BTRootNode
{
    protected override BTBlackboard CreateBlackboard()
    {
        return new EnemyBlackboard();
    }
}