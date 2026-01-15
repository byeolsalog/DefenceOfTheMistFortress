using UnityEngine;

[CreateAssetMenu(fileName = "WaveEventFactorySO", menuName = "TableFactories/WaveEventFactory")]
public class WaveEventFactorySO : TableFactorySO
{
    public override ITable CreateTable()
    {
        return new WaveEventTable();
    }
}
