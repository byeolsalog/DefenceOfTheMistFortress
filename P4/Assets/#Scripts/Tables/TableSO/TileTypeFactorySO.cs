using UnityEngine;

[CreateAssetMenu(fileName = "TileTypeFactorySO", menuName = "TableFactories/TileTypeFactory")]
public class TileTypeFactorySO : TableFactorySO
{
    public override ITable CreateTable()
    {
        return new TileTypeTable();
    }
}
