using Escandallo.Tests.Contracts;
using Xunit;

namespace Escandallo.Tests.UI;

public enum UiState
{
    Loading,
    Empty,
    Nominal,
    Error
}

public class EscandalloMatrixViewModel
{
    public UiState State { get; set; } = UiState.Loading;
    public string? ErrorMessage { get; set; }
    public List<CompleteEscandallo> Escandallos { get; set; } = new();

    public void LoadServices(Func<IEnumerable<CompleteEscandallo>> fetchServices)
    {
        try
        {
            var services = fetchServices()?.ToList() ?? new List<CompleteEscandallo>();
            Escandallos = services;
            State = services.Count > 0 ? UiState.Nominal : UiState.Empty;
        }
        catch (Exception ex)
        {
            State = UiState.Error;
            ErrorMessage = ex.Message;
        }
    }

    public string FormatMarginPercentage(decimal marginPct)
    {
        return $"{marginPct.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture)}%";
    }
}

public class EscandalloMatrixComponentTests
{
    [Fact]
    public void ZeroMock_UiStateTransitions_HandlesLoadingToNominal()
    {
        // Arrange
        var viewModel = new EscandalloMatrixViewModel();
        Assert.Equal(UiState.Loading, viewModel.State);

        // Act: Transition to nominal with services
        viewModel.LoadServices(() => new List<CompleteEscandallo>
        {
            new() { ServiceName = "Mesodermolifting", TotalEscandalloCost = 34.71m, NetMarginPercentage = 33.3m }
        });

        // Assert
        Assert.Equal(UiState.Nominal, viewModel.State);
        Assert.Single(viewModel.Escandallos);
    }

    [Fact]
    public void ZeroMock_UiStateTransitions_HandlesLoadingToEmptyWhenNoServices()
    {
        // Arrange
        var viewModel = new EscandalloMatrixViewModel();

        // Act
        viewModel.LoadServices(() => Enumerable.Empty<CompleteEscandallo>());

        // Assert
        Assert.Equal(UiState.Empty, viewModel.State);
        Assert.Empty(viewModel.Escandallos);
    }

    [Fact]
    public void ZeroMock_UiStateTransitions_HandlesLoadingToErrorOnNetworkFailure()
    {
        // Arrange
        var viewModel = new EscandalloMatrixViewModel();

        // Act & Assert
        viewModel.LoadServices(() => throw new HttpRequestException("Network failure"));

        Assert.Equal(UiState.Error, viewModel.State);
        Assert.NotNull(viewModel.ErrorMessage);
    }

    [Fact]
    public void Presentation_TantoPorCientoFormatting_AlwaysIncludesPercentSignAndNoRawDecimals()
    {
        // Arrange
        var viewModel = new EscandalloMatrixViewModel();

        // Act
        string formatted = viewModel.FormatMarginPercentage(33.3m);

        // Assert: CMP-WEB-UI-001 and BR-COST-FORMULA-001 Sec 2.1
        Assert.Equal("33.3%", formatted);
        Assert.DoesNotContain("0.33", formatted);
    }
}
