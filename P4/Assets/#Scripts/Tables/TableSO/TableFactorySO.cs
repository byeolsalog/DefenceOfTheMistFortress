using UnityEngine;

public abstract class TableFactorySO : ScriptableObject
{
    public string TableName => this.name;
    public abstract ITable CreateTable();
}
