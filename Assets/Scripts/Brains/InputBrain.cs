using UnityEngine;

public class InputBrain : PlayerBrain
{
    /// <summary>
    /// Calcule le d�placement que la manette applique au joueur 
    /// </summary>
    /// <param name="team">L'�quipe du joueur</param>
    /// <returns>Le vecteur de d�placement.</returns>
    public override Vector2 Move(Team team)
    {
        return Vector2.zero;
    }
}
