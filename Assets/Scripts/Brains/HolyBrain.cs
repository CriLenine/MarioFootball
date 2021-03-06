using UnityEngine;
using System.Collections.Generic;

public class HolyBrain : PlayerBrain
{
    private TreeV4 behaviorTree = new TreeV4();

    private float shootThreshold = Field.Width / 4;
    private float defendThreshold = Field.Width / 20;
    private float attackThreshold = 0.3f;
    private float markThreshold = Field.Width / 25;
    private float headButtThreshold = 1f;
    private float passAlignementThreshold = 0.8f;
    private float shootAlignementThreshold = 0.9f;
    private float dangerRangeThreshold = 3f;

    private void Start()
    {
        List<float> Thresholds = new List<float>
        {
            shootThreshold,
            defendThreshold,
            attackThreshold,
            headButtThreshold,
            markThreshold,
            passAlignementThreshold,
            shootAlignementThreshold,
            dangerRangeThreshold
        };

        behaviorTree.Setup(Allies, Enemies, Player, Thresholds);
    }

    public override Action GetAction()
    {
        return behaviorTree.root.Evaluate().Item2;
    }
}
