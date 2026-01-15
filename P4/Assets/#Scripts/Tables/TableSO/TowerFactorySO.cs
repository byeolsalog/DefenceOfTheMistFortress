using UnityEngine;

[CreateAssetMenu(fileName = "TowerFactorySO", menuName = "TableFactories/TowerFactory")]
public class TowerFactorySO : TableFactorySO
{
    public override ITable CreateTable()
    {
        return new TowerTable();
    }
}
