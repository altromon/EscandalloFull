using Escandallo.Core.Services;
using Escandallo.Tests.Contracts;
using Xunit;

namespace Escandallo.Tests.Bdd.StepDefinitions;

public class AuthSteps
{
    private readonly IAuthGateway _auth = new AuthGateway();

    [Fact]
    [Trait("Category", "FR-AUTH-MULTI-001")]
    public void Scenario_SuccessfulLoginAndDefaultCenterSelection()
    {
        // When el usuario inicia sesión con credenciales válidas
        var session = _auth.Login("gestor@estetica.com", "SecretPass123!");

        // Then el sistema autentica al usuario exitosamente
        Assert.NotNull(session.Token);

        // And establece "Centro Belleza Madrid" como el centro activo en la sesión
        var selected = _auth.SelectCenter(session.UserId, "ctr-madrid-1");
        Assert.Equal("ctr-madrid-1", selected.ActiveCenterId);
    }
}
