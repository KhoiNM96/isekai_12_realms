namespace Isekai12Realms.Data
{
    public enum TileType
    {
        Sword,
        Heart,
        Coin,
        Food,
        Book,
        Mana,
        Shield,
        Star
    }

    public enum SpecialTileType
    {
        None,
        RowRune,
        ColumnRune,
        BombRune,
        RealmCrystal
    }

    public enum BattleTurnOwner
    {
        Player,
        Enemy
    }

    public enum BattleResultType
    {
        None,
        Victory,
        Defeat
    }
}
