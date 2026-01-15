using System.Collections.Generic;
using UnityEngine;

public class PoolManager
{
    private Dictionary<int, Stack<Monster>> _monsters = new();
    private GameObject _root;

    public void Init()
    {
        if (_root == null)
        {
            _root = new GameObject("@Pool_Root");
            Object.DontDestroyOnLoad(_root);
        }
    }

    public void CreateMonsterPool(int id, GameObject prefab, int count = 10)
    {
        if (!_monsters.ContainsKey(id))
            _monsters[id] = new Stack<Monster>();

        for (int i = 0; i < count; i++)
        {
            Monster monster = CreateNewMonster(prefab);
            PushMonster(monster);
        }
    }

    public Unit PopMonster(int id, GameObject prefab, Vector3 position, Quaternion rotation)
    {
        Unit unit;

        if (_monsters.ContainsKey(id) && _monsters[id].Count > 0)
        {
            unit = _monsters[id].Pop();
        }
        else
        {
            unit = CreateNewMonster(prefab);
        }

        unit.transform.SetPositionAndRotation(position, rotation);
        unit.gameObject.SetActive(true);
        return unit;
    }

    public void PushMonster(Monster monster)
    {
        if (monster == null) return;

        int id = monster.Id;
        if (!_monsters.ContainsKey(id))
            _monsters[id] = new Stack<Monster>();

        monster.gameObject.SetActive(false);
        monster.transform.SetParent(_root.transform);
        _monsters[id].Push(monster);
    }

    private Monster CreateNewMonster(GameObject prefab)
    {
        GameObject go = Object.Instantiate(prefab, _root.transform);
        Monster unit = go.GetComponent<Monster>();
        return unit;
    }
}