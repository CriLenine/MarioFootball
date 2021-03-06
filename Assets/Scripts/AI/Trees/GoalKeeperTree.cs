using BehaviorTree;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GoalKeeperTree
{
    public RootNode root;

    public List<Player> Allies;
    public List<Player> Enemies;
    public Player player;

    public Transform allyGoalTransform;
    public Transform enemyGoalTransform;

    public Vector3 InitialGoalPosition;

    public void Setup(Team iAllies, Team iEnemies, Player iplayer, Vector3 iInitialGoalPosition)
    {
        Allies = iAllies.Players.ToList();
        Enemies = iEnemies.Players.ToList();

        allyGoalTransform = iAllies.transform;
        enemyGoalTransform = iEnemies.transform;

        InitialGoalPosition = iInitialGoalPosition;

        player = iplayer;

        root = new RootNode(this, new List<Node>
        {
            new Sequence(new List<Node>
            {
                new Selector(new List<Node>
                {
                    new Sequence(new List<Node>
                    {
                        new T_ShootInProgress(),
                        new T_EndPointInMyGoal(),
                        new S_MoveGoalKeeper_Block()
                    }),
                    new Sequence(new List<Node>
                    {
                        new T_PassInProgress(),
                        new T_PassTargetIsMe(),
                        new S_Static()
                    }),
                    new Sequence(new List<Node>
                    {
                        new T_BallHolderIsMe(),
                        new Selector(new List<Node>
                        {
                            new Sequence(new List<Node>
                            {
                                new T_NearbyAllyUnmarked(),
                                new S_Pass()
                            }),
                            new S_MoveGoalKeeper_Default()
                        })
                    }),
                    new Sequence(new List<Node>
                    {
                        new T_BallInAllyHalf(),
                        new Selector(new List<Node>
                        {
                            new Sequence(new List<Node>
                            {
                                new S_UpdateBallHolder(),
                                new T_BallHolderIsAlly(),
                                new S_MoveGoalKeeper_Support()
                            }),
                            new Selector(new List<Node>
                            {
                                new Sequence(new List<Node>
                                {
                                    new T_BallHolderIsNone(),
                                    new T_BallInGoalRange(),
                                    new S_MoveGoalKeeper_GetBall()
                                }),
                                new S_MoveGoalKeeper_Defend()
                            })
                        })
                    }),
                    new S_MoveGoalKeeper_Default()
                }),
                new Selector(new List<Node>
                {
                    new Sequence(new List<Node>
                    {
                        new T_ActionType_Move(),
                        new A_Move()
                    }),
                    new Sequence(new List<Node>
                    {
                        new T_ActionType_Pass(),
                        new A_Pass()
                    }),
                    new Sequence(new List<Node>
                    {
                        new T_ActionType_HeadButt(),
                        new A_HeadButt()
                    })
                })
            })
        });
    }
}
