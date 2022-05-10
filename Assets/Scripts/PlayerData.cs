using System.Collections.Generic;

public class PlayerData
{
    public readonly Player Player;
    public readonly City PlayerCity;
    public readonly List<Unit> PlayerUnits;
    public readonly List<Unit> EnemiesThreateningCity;

    public PlayerData(Player player, City playerCity, List<Unit> playerUnits, UnitsSpawner unitsSpawner)
    {
        Player = player;
        PlayerCity = playerCity;
        PlayerUnits = playerUnits;
        EnemiesThreateningCity = new List<Unit>();
        playerCity.UnitsSpawner = unitsSpawner;
        playerCity.Initialize(this);
    }
    
}
