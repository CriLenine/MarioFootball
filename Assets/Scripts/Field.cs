using UnityEngine;

public class Field : MonoBehaviour
{
    private Team team1, team2;

    private void Start()
    {
        GameManager.BreedMePlease(team1, team2);
    }
}