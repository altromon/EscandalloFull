# 📊 Formal Quality and Release Gate Report (AI-SDLC)

> **Generation Date:** 2026-10-16T11:50:19.913Z
> **Release Gate Verdict:** 🔴 BLOCKED (VIOLATIONS DETECTED)
> **Global Rating:** **`B`** (MI Index: 63/100, Average CC: 4.2)

---

## 1. Executive Metric Summary

| Key Metric | Measured Value | Policy Threshold | Compliance |
| :--- | :---: | :---: | :---: |
| **Analyzed Files** | `29` | N/A | ℹ️ |
| **Evaluated Functions** | `99` | N/A | ℹ️ |
| **Lines of Code (LOC)** | `1262` | N/A | ℹ️ |
| **Cyclomatic Complexity (Average)** | `4.2` | $\le 10$ | ✅ COMPLIANT |
| **Cognitive Complexity (Average)** | `2.9` | $\le 15$ | ✅ COMPLIANT |
| **Maintainability Index (SEI MI)** | `63 / 100` | $\ge 50$ | ✅ COMPLIANT |
| **Line Coverage** | `65%` | $\ge 85\%$ | ⚠️ BELOW THRESHOLD |
| **Branch Coverage** | `45.6%` | $\ge 80\%$ | ⚠️ BELOW THRESHOLD |
| **Function Coverage** | `71.4%` | $\ge 90\%$ | ⚠️ BELOW THRESHOLD |
| **Functions in Violation** | `15` | $0$ (Mode STRICT) | ❌ BLOCKED |

---

## 2. Polyglot Breakdown by Language Ecosystem

| Language | Functions | Total LOC | Average MI | Average CC | Rating |
| :--- | :---: | :---: | :---: | :---: | :---: |
| **C#** | `12` | `72` | `73.4` | `1.3` | `B` |
| **JavaScript** | `87` | `1190` | `61.6` | `4.6` | `B` |

---

## 3. Quality Gate Violations Log

- **`src/Escandallo.Core/Services/EscandalloRepository.cs`** [`new`]: Maintainability Index 49.9 below minimum of 50
- **`src/Escandallo.Web/wwwroot/js/app.js`** [`switchAuthTab`]: Cyclomatic Complexity 12 exceeds threshold of 10, Cognitive Complexity 21 exceeds threshold of 15
- **`src/Escandallo.Web/wwwroot/js/app.js`** [`handleRegisterSubmit`]: Maintainability Index 44.1 below minimum of 50
- **`src/Escandallo.Web/wwwroot/js/app.js`** [`handleCreateCenterSubmit`]: Maintainability Index 43.1 below minimum of 50, Function lines 41 exceeds maximum of 40
- **`src/Escandallo.Web/wwwroot/js/app.js`** [`handleLogin`]: Maintainability Index 45.8 below minimum of 50
- **`src/Escandallo.Web/wwwroot/js/app.js`** [`initNavbar`]: Maintainability Index 46.9 below minimum of 50
- **`src/Escandallo.Web/wwwroot/js/app.js`** [`fetchCostParameters`]: Cyclomatic Complexity 21 exceeds threshold of 10, Maintainability Index 45.7 below minimum of 50
- **`src/Escandallo.Web/wwwroot/js/app.js`** [`renderWorkersTable`]: Maintainability Index 47.7 below minimum of 50
- **`src/Escandallo.Web/wwwroot/js/app.js`** [`renderProductsTable`]: Cyclomatic Complexity 14 exceeds threshold of 10, Maintainability Index 44.2 below minimum of 50
- **`src/Escandallo.Web/wwwroot/js/app.js`** [`handleCreateProductSubmit`]: Maintainability Index 46.9 below minimum of 50
- **`src/Escandallo.Web/wwwroot/js/app.js`** [`handleUpdateProductFullSubmit`]: Maintainability Index 46.4 below minimum of 50
- **`src/Escandallo.Web/wwwroot/js/app.js`** [`renderCreateModalOptions`]: Function lines 42 exceeds maximum of 40
- **`src/Escandallo.Web/wwwroot/js/app.js`** [`populateAuditCenterSelect`]: Cyclomatic Complexity 11 exceeds threshold of 10, Maintainability Index 47 below minimum of 50
- **`src/Escandallo.Web/wwwroot/js/app.js`** [`fetchAuditHistory`]: Cyclomatic Complexity 12 exceeds threshold of 10, Maintainability Index 41.2 below minimum of 50
- **`src/Escandallo.Web/wwwroot/js/app.js`** [`fetchSystemLogs`]: Maintainability Index 45.3 below minimum of 50

---

## 4. Evaluation Criteria and Standards
- **McCabe Cyclomatic Complexity (CC)**: Number of linearly independent paths.
- **Maintainability Index (SEI MI)**: Normalized formula [0 - 100] combining Halstead Volume, CC, and LOC.
- **Automated Test Coverage**: Passively ingested from `coverage/coverage-summary.json` or `lcov.info` when present.
- **Clean Code Guardrails**: Prohibition of implicit `any` typing, function length limits ($\le 40$ lines), and zero unjustified suppressions.