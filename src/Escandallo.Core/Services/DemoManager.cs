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
    private readonly IEscandalloRepository? _escandalloRepo;
    private readonly ISubscriptionManager? _subscriptionManager;

    public DemoManager(
        IEscandalloRepository? escandalloRepo = null,
        ISubscriptionManager? subscriptionManager = null)
    {
        _escandalloRepo = escandalloRepo;
        _subscriptionManager = subscriptionManager;

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

        if (_escandalloRepo != null)
        {
            var existing = _escandalloRepo.GetByCenter(centerId).ToList();
            if (existing.Count == 0 || overrideIfRealExists)
            {
                if (overrideIfRealExists)
                {
                    _escandalloRepo.Clear(centerId);
                }
                _escandalloRepo.Save(centerId, EscandalloRepository.CreateDefaultMesodermolifting($"esc-demo-1-{centerId}"));
                _escandalloRepo.Save(centerId, EscandalloRepository.CreateSecondaryFacial($"esc-demo-2-{centerId}"));
                if (_subscriptionManager != null)
                {
                    var status = _subscriptionManager.GetStatus(centerId);
                    status.ActiveEscandallos = 2;
                }
            }
        }
    }

    public int PurgeDemoData(string centerId)
    {
        int purgedCount = 0;
        if (_states.TryGetValue(centerId, out var state))
        {
            purgedCount = state.DemoCount;
            state.DemoCount = 0;
            state.HasDemoData = false;
        }

        if (_escandalloRepo != null)
        {
            var repoDeleted = _escandalloRepo.Clear(centerId);
            if (purgedCount == 0)
            {
                purgedCount = repoDeleted;
            }
        }

        _subscriptionManager?.ResetActiveCount(centerId);

        return purgedCount;
    }
}
