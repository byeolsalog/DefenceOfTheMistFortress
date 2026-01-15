using UnityEngine;

[CreateAssetMenu(fileName = "UnitTypeFactorySO", menuName = "TableFactories/UnitTypeFactory")]
public class UnitTypeFactorySO : TableFactorySO
{
    public override ITable CreateTable()
    {
        return new UnitTypeTable();
    }
}
