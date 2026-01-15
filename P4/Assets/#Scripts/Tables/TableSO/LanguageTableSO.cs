using UnityEngine;

[CreateAssetMenu(fileName = "Language", menuName = "TableFactories/LanguageFactory")]
public class LanguageTableSO : TableFactorySO
{
    public override ITable CreateTable()
    {
        return new LanguageTable();
    }
}
