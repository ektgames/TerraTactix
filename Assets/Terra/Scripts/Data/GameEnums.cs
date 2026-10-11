namespace TerraTactix.Data
{
    public enum RegionState
    {
        Locked = 0,
        Available = 1,
        Attacking = 2,
        PartiallyCaptured = 3,
        Captured = 4
    }

    public enum RewardType
    {
        Gold = 0,
        Hearts = 1,
        Gems = 2,
        MaxHearts = 3,
        AttackSpeed = 4
    }

    public enum ShopProductKind
    {
        Heart = 0,
        HeartPack = 1,
        MaxHearts = 2,
        AttackSpeed = 3
    }
}
