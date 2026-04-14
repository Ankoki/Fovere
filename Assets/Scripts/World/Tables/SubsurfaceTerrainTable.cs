/// <summary>
/// Class used to generate underlying blocks for the terrain.
/// </summary>
public class SubsurfaceTerrainTable : WeightedTable
{
    
    public SubsurfaceTerrainTable()
    {
        Place(ItemType.Dirt, 100);
        Place(ItemType.Stone, 100);
        Place(ItemType.ExpraDeposit, 25);
    }
    
}