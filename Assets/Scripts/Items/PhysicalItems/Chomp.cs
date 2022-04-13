using UnityEngine;

public class Chomp : PhysicalItem
{
    [SerializeField] private float _duration;
    private float _timer;

    protected void Update()
    {
        if ((_timer += Time.deltaTime) > _duration)
            DestroyItem();
        Debug.Log("Chomp chomp");
    }
    protected override void ApplyEffect(Player player)
    {
        Debug.Log("Manged " + player.name);
    }

    public override void DestroyItem()
    {
        Destroy(gameObject);
    }
}