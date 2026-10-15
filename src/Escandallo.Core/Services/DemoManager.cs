using System.Collections.Concurrent;
using Escandallo.Core.Contracts;

namespace Escandallo.Core.Services;

/// <summary>
/// Gestor de entorno y aislamiento de datos de demostración (FR-DEMO-MODE-001, SEC-REQ-DEMO-ISOLATION-001).
/// </summary>
public class DemoManager : IDemoManager
{
    private class CenterDemoState
    {
        public bool HasDemoData { get; set; }
        public int DemoCount { get; set; }
        public int RealCount { get; set; }
    }

    private readonly ConcurrentDictionary<string, CenterDemoState> _states = new();

    public DemoManager()
    {
        _states["ctr-mixed-data"] = new CenterDemoState
        {
            HasDemoData = true,
            DemoCount = 5,
            RealCount = 3
        };
    }

    public (bool hasDemoData, int demoCount, int realCount) GetStatus(string centerId)
    {
        var state = _states.GetOrAdd(centerId, _ => new CenterDemoState
        {
            HasDemoData = false,
            DemoCount = 0,
            RealCount = 0
        });

        return (state.HasDemoData, state.DemoCount, state.RealCount);
    }

    public void LoadDemoData(string centerId, bool overrideIfRealExists)
    {
        var state = _states.GetOrAdd(centerId, _ => new CenterDemoState());
        state.HasDemoData = true;
        state.DemoCount = 2; // Cleasing Milk y Mesodermolifting estándar
    }

    public int PurgeDemoData(string centerId)
    {
        if (!_states.TryGetValue(centerId, out var state))
            return 0;

        int purgedCount = state.DemoCount;
        state.DemoCount = 0;
        state.HasDemoData = false;

        return purgedCount;
    }
}
