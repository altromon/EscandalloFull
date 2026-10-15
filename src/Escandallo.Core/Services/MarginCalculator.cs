using System.Globalization;
using Escandallo.Core.Contracts;

namespace Escandallo.Core.Services;

/// <summary>
/// Calculador de márgenes, precios sugeridos y semáforos de rentabilidad (BR-COST-FORMULA-001, BR-COLOR-THRESHOLDS-001).
/// </summary>
public class MarginCalculator : IMarginCalculator
{
    public (decimal suggestedBasePrice, decimal suggestedPvp) CalculateSuggestedPrices(
        decimal totalEscandallo,
        decimal markupPercentage,
        decimal taxRate)
    {
        decimal suggestedBasePrice = totalEscandallo * (1m + (markupPercentage / 100m));
        decimal suggestedPvp = suggestedBasePrice * (1m + (taxRate / 100m));

        return (suggestedBasePrice, suggestedPvp);
    }

    public (decimal netMarginAbsolute, decimal netMarginPercentage) CalculateRealMargin(
        decimal appliedPvr,
        decimal totalEscandallo,
        decimal taxRate)
    {
        decimal netRevenue = appliedPvr / (1m + (taxRate / 100m));
        decimal netMarginAbsolute = netRevenue - totalEscandallo;
        decimal netMarginPercentage = netRevenue != 0m ? (netMarginAbsolute / netRevenue) * 100m : 0m;

        return (netMarginAbsolute, netMarginPercentage);
    }

    public (decimal effectiveSessionPrice, decimal netMarginPercentage) CalculatePackMargin(
        decimal totalPackPvr,
        int sessionCount,
        decimal totalEscandallo,
        decimal taxRate)
    {
        if (sessionCount <= 0)
            throw new ArgumentOutOfRangeException(nameof(sessionCount), "El número de sesiones debe ser mayor a cero.");

        decimal netPackRevenue = totalPackPvr / (1m + (taxRate / 100m));
        decimal effectiveSessionPrice = netPackRevenue / sessionCount;
        decimal netMarginAbsolute = effectiveSessionPrice - totalEscandallo;
        decimal netMarginPercentage = effectiveSessionPrice != 0m ? (netMarginAbsolute / effectiveSessionPrice) * 100m : 0m;

        return (effectiveSessionPrice, netMarginPercentage);
    }

    public TrafficLightColor DetermineTrafficLight(decimal marginPercentage)
    {
        if (marginPercentage >= 40.0m)
            return TrafficLightColor.Verde;

        if (marginPercentage >= 20.0m)
            return TrafficLightColor.Amarillo;

        return TrafficLightColor.Rojo;
    }

    public string? FormatLossWarning(decimal netMarginAbsolute, bool isPack = false)
    {
        if (netMarginAbsolute >= 0m)
            return null;

        decimal absoluteLoss = Math.Abs(Math.Round(netMarginAbsolute, 2));
        string formattedLoss = absoluteLoss.ToString("0.00", CultureInfo.InvariantCulture);

        return $"Atención: Estás perdiendo {formattedLoss} € por cada sesión";
    }
}
