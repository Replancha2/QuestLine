/// <summary>
/// Envoltorio que representa un ítem ofrecido en la tienda inter-día.
/// Puede ser un <see cref="BuffData"/> o un <see cref="ConsumableData"/>; uno de los dos será null.
/// </summary>
public class ShopOffer
{
    public readonly BuffData        Buff;
    public readonly ConsumableData  Consumable;

    public bool   IsBuff        => Buff != null;
    public string Name          => IsBuff ? Buff.BuffName   : Consumable.Name;
    public string Description   => IsBuff ? Buff.Description : Consumable.Description;
    public int    Cost          => IsBuff ? Buff.Cost        : Consumable.Cost;
    public UnityEngine.Sprite Icon => IsBuff ? Buff.Icon    : Consumable.Icon;

    public ShopOffer(BuffData buff)             { Buff = buff; }
    public ShopOffer(ConsumableData consumable) { Consumable = consumable; }
}
