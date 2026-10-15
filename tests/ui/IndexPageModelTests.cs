using Escandallo.Core.Services;
using Escandallo.Web.Pages;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Xunit;

namespace Escandallo.Tests.UI;

public class IndexPageModelTests
{
    [Fact]
    public void OnGet_DefaultCenter_LoadsMadridDataAndSubscription()
    {
        var repo = new EscandalloRepository();
        var subMgr = new SubscriptionManager();
        var pageModel = new IndexModel(repo, subMgr);

        pageModel.OnGet();

        Assert.Equal("ctr-madrid-1", pageModel.CenterId);
        Assert.NotNull(pageModel.Subscription);
        Assert.NotEmpty(pageModel.Services);
    }

    [Fact]
    public void OnGet_WithCustomCenterHeader_LoadsSelectedCenterData()
    {
        var repo = new EscandalloRepository();
        var subMgr = new SubscriptionManager();
        var pageModel = new IndexModel(repo, subMgr);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["X-Center-Id"] = "ctr-radiante-1";

        var modelState = new ModelStateDictionary();
        var actionContext = new Microsoft.AspNetCore.Mvc.ActionContext(httpContext, new RouteData(), new PageActionDescriptor(), modelState);
        var modelMetadataProvider = new EmptyModelMetadataProvider();
        var viewData = new ViewDataDictionary(modelMetadataProvider, modelState);
        pageModel.PageContext = new PageContext(actionContext) { ViewData = viewData };

        pageModel.OnGet();

        Assert.Equal("ctr-radiante-1", pageModel.CenterId);
        Assert.NotEmpty(pageModel.Services);
        Assert.Contains(pageModel.Services, s => s.Id.Contains("rad"));
    }
}
