using UnityEngine;

[System.Serializable]
public abstract class PlayerBrain : MonoBehaviour
{
    /// <summary>
    /// Calcule le d�placement que l'IA doit appliquer au joueur 
    /// </summary>
    /// <param name="team">L'�quipe du joueur</param>
    /// <returns>Le vecteur de d�placement.</returns>
    public abstract Vector2 Move(Team team);
}
