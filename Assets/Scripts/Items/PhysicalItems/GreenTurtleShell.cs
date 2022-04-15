using UnityEngine;

public class GreenTurtleShell : PhysicalItem
{
    protected override void ApplyEffect(Player player)
    {
        player.Fall((player.transform.position - transform.position).normalized);
    }
    public override void DestroyItem()
    {
        Destroy(gameObject);
    }
}
