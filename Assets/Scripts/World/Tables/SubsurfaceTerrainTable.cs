/// <summary>
/// Class used to generate underlying blocks for the terrain.
/// </summary>
public class SubsurfaceTerrainTable : WeightedTable
{
    
    /// <summary>
    /// Basic table to show the idea for prototype.
    /// </summary>
    public SubsurfaceTerrainTable()
    {
        Place(ItemType.Get(ItemType.Keys.Dirt), 100);
        Place(ItemType.Get(ItemType.Keys.Stone), 100);
        Place(ItemType.Get(ItemType.Keys.ExpraDeposit), 25);
    }
    
}