using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorTree;
using System.Linq;

public class TreeV2
{
    public Transform allyGoalTransform;
    public Transform enemyGoalTransform;

    public List<Player> Allies;
    public List<Player> Enemies;

    public Player player;

    public float shootThreshold;

    public RootNode root;

    public void Setup(Team iAllies, Team iEnemies, Player iplayer, float ishootThreshold)
    {
        Allies = iAllies.Players.ToList();
        Enemies = iEnemies.Players.ToList();
        allyGoalTransform = iAllies.transform;
        enemyGoalTransform = iEnemies.transform;
        player = iplayer;
        shootThreshold = ishootThreshold;

        root = new RootNode(this, new List<Node>()
        {
            new Selector(new List<Node>{
                new CoucouNode(),
                new Sequence(new List<Node>
                {
                    new BallOwnership(SearchType.Enemies)
                }),
                new Sequence(new List<Node>
                {
                    new BallOwnership(SearchType.Allies),
                    new Selector(new List<Node>
                    {
                        new Sequence(new List<Node>
                        {
                            
                            new BallOwnership(SearchType.PlayerSpecific),
                            new Selector(new List<Node>
                            {
                                new Sequence(new List<Node>
                                {
                                    new PositionToShoot(),
                                    new Shoot()
                                }),
                                new GoToPosition()
                            })
                        })

                    })
                }),
                new Sequence(new List<Node>
                {
                    new Proximity(),
                    new GoToPosition()
                })
            })
        }) ;
    }

    public void UpdateVariables()
    {

    }
}