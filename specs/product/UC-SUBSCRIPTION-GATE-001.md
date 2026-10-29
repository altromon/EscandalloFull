---
id: "UC-SUBSCRIPTION-GATE-001"
type: "use-case"
title: "Control de Cuota Freemium y Pasarela de Suscripción Stripe"
status: "draft"
version: "1.0.0"
primary-actor: "ACT-USER-001"
supporting-actors: []
---

# Use Case: Control de Cuota Freemium y Pasarela de Suscripción Stripe (`UC-SUBSCRIPTION-GATE-001`)

## 1. Intent and Outcome
As a `ACT-USER-001`, I want to test the application under a free plan with up to 2 escandallos per center and upgrade to an active monthly or annual Stripe subscription, to unlock unlimited service escandallos and full commercial capabilities.

## 2. Preconditions
- The aesthetic center is registered and has an active account.

## 3. Main Success Scenario
1. The user creates services within an aesthetic center.
2. If the user is on the Free tier and currently has 2 active escandallos, attempting to create a 3rd triggers a monetization gate modal.
3. The modal highlights the benefits of the Premium plan and offers Monthly and Annual billing options.
4. The user clicks "Subscribe" and is redirected to a secure Stripe Checkout session.
5. The user completes payment on Stripe.
6. Stripe sends a signed webhook (`checkout.session.completed` / `customer.subscription.created`) to the application backend.
7. The system activates the Premium subscription for the tenant and lifts the 2-escandallo cap immediately.

## 4. Alternative and Error Flows
- **Payment Failure / Cancellation**: If the user cancels on Stripe Checkout, they return to the application with the free tier intact and no charge applied.
- **Subscription Expiry / Chargeback**: When Stripe issues a `customer.subscription.deleted` or past-due event, the tenant transitions to grace period before re-enforcing the quota limit.
