using UnityEngine;

[CreateAssetMenu(fileName = "WaveFactorySO", menuName = "TableFactories/WaveFactory")]
public class WaveFactorySO : TableFactorySO
{
    public override ITable CreateTable()
    {
        return new WaveTable();
    }
}
