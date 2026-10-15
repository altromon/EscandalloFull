using Escandallo.Core.Contracts;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Escandallo.Web.Pages;

public class IndexModel : PageModel
{
    private readonly IEscandalloRepository _repository;
    private readonly ISubscriptionManager _subscriptionManager;

    public IndexModel(IEscandalloRepository repository, ISubscriptionManager subscriptionManager)
    {
        _repository = repository;
        _subscriptionManager = subscriptionManager;
    }

    public string CenterId { get; set; } = "ctr-madrid-1";
    public string CenterName { get; set; } = "Centro Belleza Madrid";
    public SubscriptionStatus Subscription { get; set; } = new();
    public List<CompleteEscandallo> Services { get; set; } = new();

    public void OnGet()
    {
        CenterId = Request?.Headers["X-Center-Id"].FirstOrDefault() ?? "ctr-madrid-1";
        Subscription = _subscriptionManager.GetStatus(CenterId);
        Services = _repository.GetByCenter(CenterId).ToList();
    }
}
