using UnityEngine;

[CreateAssetMenu(fileName = "MosterFactorySO", menuName = "TableFactories/MosterFactory")]
public class MosterFactorySO : TableFactorySO
{
    public override ITable CreateTable()
    {
        return new MonsterTable();
    }
}
