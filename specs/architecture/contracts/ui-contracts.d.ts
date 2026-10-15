/**
 * Escandallo Estética - Shared Full-Stack TypeScript Contracts
 * Single source of truth for Frontend (UI) and Backend (API DTOs).
 *
 * All values have explicit units of measurement.
 * All percentages are represented in tanto por 100 (0.0 to 100.0, with symbol %).
 */

export type RoleType = "ACT-USER-001" | "ACT-GUEST-001" | "ACT-MAINTAINER-001";
export type TrafficLightColor = "verde" | "amarillo" | "rojo";
export type TaxRate = 21.0 | 10.0 | 0.0;
export type UnitOfMeasure = "ml" | "g" | "dosis" | "unidad";

export interface AuthSession {
  token: string;
  expiresIn: number; // Unit: seconds
  userId: string;
  tenantId: string;
  activeCenterId: string;
  role: RoleType;
}

export interface CenterMembership {
  centerId: string;
  name: string;
  role: RoleType;
  isPlanPremium: boolean;
}

export interface CenterCostParameters {
  monthlyOpeningHours: number;   // Unit: h/mes (Standard: 176 h/mo)
  monthlyOpeningMinutes: number; // Unit: min/mes (Calculated: hours * 60 = 10,560 min/mo)
  monthlyOverheadCost: number;   // Unit: EUR/mes (e.g. 875.00 EUR)
  overheadCostPerMinute: number; // Unit: EUR/min (Imputable cost per min)
}

export interface WorkerRate {
  id: string;
  categoryName: string;
  monthlyCompanyCost: number; // Unit: EUR/mes
  costPerMinute: number;      // Unit: EUR/min
}

export interface CosmeticProduct {
  id: string;
  name: string;
  packagePrice: number;       // Unit: EUR
  capacity: number;           // Quantity in unitOfMeasure
  unitOfMeasure: UnitOfMeasure;
  wastePercentage: number;    // Unit: % (0.0 to 100.0, tanto por 100)
  grossUnitCost: number;      // Unit: EUR / unitOfMeasure
  effectiveUnitCost: number;  // Unit: EUR / unitOfMeasure (including merma)
  isDemo: boolean;
}

export interface ConsumableUsage {
  productId: string;
  productName: string;
  amountUsed: number;         // Quantity in unitOfMeasure
  unitOfMeasure: UnitOfMeasure;
  computedCost: number;       // Unit: EUR
}

export interface GuestConsumableUsage {
  productId: string;
  productName: string;
  amountUsed: number;
  unitOfMeasure: UnitOfMeasure;
}

export interface SessionPack {
  id: string;
  packName: string;                         // e.g. "Bono 5 Sesiones"
  sessionCount: number;                     // e.g. 5
  totalPackPrice: number;                   // Unit: EUR (with VAT)
  effectiveSessionPriceWithoutTax: number;  // Unit: EUR (per session revenue without VAT)
  netMarginPercentage: number;              // Unit: % (0.0 to 100.0)
  trafficLightColor: TrafficLightColor;
  lossWarningMessage?: string | null;       // Warning if per-session revenue < escandallo
}

export interface GuestSessionPack {
  id: string;
  packName: string;
  sessionCount: number;
  totalPackPrice: number;                   // Unit: EUR
  trafficLightColor: TrafficLightColor;
}

/** Complete Escandallo DTO for ACT-USER-001 */
export interface CompleteEscandallo {
  id: string;
  serviceName: string;
  durationMinutes: number;                  // Unit: min
  taxRatePercentage: TaxRate;               // Unit: % (21.0, 10.0, 0.0)
  workerCategory: string;
  laborCost: number;                        // Unit: EUR (wage, hidden from guests)
  overheadCost: number;                     // Unit: EUR (facility, hidden from guests)
  cosmeticsCost: number;                    // Unit: EUR
  totalEscandalloCost: number;              // Unit: EUR (sum of 3 cost pillars)
  suggestedBasePrice: number;               // Unit: EUR (50% markup without tax)
  suggestedPvp: number;                     // Unit: EUR (suggested retail price with tax)
  appliedPvr: number;                       // Unit: EUR (actual retail price with tax)
  netMarginAbsolute: number;                // Unit: EUR (profit per session)
  netMarginPercentage: number;              // Unit: % (0.0 to 100.0, net profit on sales)
  trafficLightColor: TrafficLightColor;     // verde (>=40%), amarillo (20-39%), rojo (<20%)
  lossWarningMessage?: string | null;       // Explicit alert if netMarginAbsolute < 0
  isActive: boolean;                        // Controls active portfolio within freemium quota
  isDemo: boolean;
  packs: SessionPack[];
  consumables: ConsumableUsage[];
}

/** Censored Escandallo DTO for ACT-GUEST-001 (BR-GUEST-CENSOR-001) */
export interface GuestEscandallo {
  id: string;
  serviceName: string;
  durationMinutes: number;                  // Unit: min
  trafficLightColor: TrafficLightColor;
  isActive: boolean;
  packs: GuestSessionPack[];
  consumables: GuestConsumableUsage[];
}

export type EscandalloView = CompleteEscandallo | GuestEscandallo;

export function isGuestEscandallo(view: EscandalloView): view is GuestEscandallo {
  return !("laborCost" in view);
}

export interface SubscriptionStatus {
  plan: "Free" | "Premium";
  activeEscandallos: number;                // Count of active escandallos (is_active = true)
  maxAllowedEscandallos: number;            // 2 for Free, Infinity for Premium
  canCreateMore: boolean;                   // false if Free and activeEscandallos >= 2
  isMockProvider: boolean;                  // true if FakeSubscriptionGateway active
  renewsAtUtc?: string | null;
}
