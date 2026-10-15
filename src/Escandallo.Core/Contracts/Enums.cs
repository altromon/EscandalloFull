namespace Escandallo.Core.Contracts;

public enum RoleType
{
    User,      // ACT-USER-001 (Manager / Owner)
    Guest,     // ACT-GUEST-001 (Guest / Sanitized view)
    Maintainer // ACT-MAINTAINER-001
}

public enum TrafficLightColor
{
    Verde,    // >= 40.0%
    Amarillo, // 20.0% - 39.99%
    Rojo      // < 20.0% or negative
}

public enum TaxRate
{
    General21 = 21,  // 21% Standard Aesthetic
    Reducido10 = 10, // 10% Thermal / Spa
    Exento0 = 0      // 0% Medical Aesthetic (Art. 20 Ley IVA)
}

public enum UnitOfMeasure
{
    Ml,
    G,
    Dosis,
    Unidad
}

public enum PlanType
{
    Free,
    Premium
}
