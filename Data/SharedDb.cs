using System.Collections.Concurrent;
using BobsBetting.DBModels;
using BobsBetting.Models;

namespace BobsBetting.DataService;

public class SharedDb
{
    private readonly ConcurrentDictionary<string, UserConnection> _connections = [];

    public ConcurrentDictionary<string, UserConnection> connections => _connections;
}