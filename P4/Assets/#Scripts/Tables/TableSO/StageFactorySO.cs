using UnityEngine;

[CreateAssetMenu(fileName = "StageFactorySO", menuName = "TableFactories/StageFactory")]
public class StageFactorySO : TableFactorySO
{
    public override ITable CreateTable()
    {
        return new StageTable();
    }
}
