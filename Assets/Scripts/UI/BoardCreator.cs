using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BoardCreator : MonoBehaviour
{
    [SerializeField] private PlayerDisplay _template;
    [SerializeField] private Transform _container;

    private Dictionary<int, GameObject> _cache = new Dictionary<int, GameObject>();

    public IReadOnlyList<int> ActorNumbers => _cache.Keys.ToList();

    public void Create(Player player)
    {
        if (_cache.ContainsKey(player.ActorNumber) == false)
        {
            var createdPlayerDisplay = Instantiate(_template, _container);
            createdPlayerDisplay.Init(_cache.Count + 1, player);

            _cache.Add(player.ActorNumber, createdPlayerDisplay.gameObject);
        }
    } 

    public void TryDestroy(Player player)
    {
        if (_cache.TryGetValue(player.ActorNumber, out GameObject gameObject))
        {
            Destroy(gameObject);
            _cache.Remove(player.ActorNumber);
        }
    }

    public void Clear()
    {
        foreach (var cache in _cache)
            Destroy(cache.Value);

        _cache.Clear();
    }
}
