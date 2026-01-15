using UnityEngine;
using System.Linq;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "TableRegistrySO", menuName = "TableFactories/TableRegistry")]
public class TableRegistrySO : ScriptableObject
{
    [SerializeField] private List<TableFactorySO> _allTableFactories;
    private Dictionary<string, TableFactorySO> _factoryMap;

    public void Init()
    {
        if (_factoryMap != null) return;
        _factoryMap = _allTableFactories.ToDictionary(factory => factory.TableName, factory => factory);
    }

    // Table과 Scriptable Object의 이름 매칭 해줘야함.
    // TableRegistrySO에 등록 해줘야함.
    public ITable CreateTableInstance(string tableName)
    {
        Init();

        if (_factoryMap.TryGetValue(tableName, out TableFactorySO factory))        
            return factory.CreateTable();


        Debug.LogWarning($"[TableRegistry] 팩토리를 찾을 수 없음: {tableName}");
        return null;
    }
}
