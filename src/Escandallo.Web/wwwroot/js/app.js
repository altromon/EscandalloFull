// Escandallo Full SPA - Clean Client Architecture (CMP-WEB-UI-001)

const State = {
  activeCenterId: 'ctr-madrid-1',
  activeCenterName: 'Centro Belleza Madrid',
  services: [],
  subscription: { plan: 'Free', active: 1, max: 2 },
  uiState: 'Loading',
  errorMessage: null
};

document.addEventListener('DOMContentLoaded', () => {
  initNavbar();
  initCostsForm();
  loadCenterData();
});

function initNavbar() {
  const centerSelect = document.getElementById('centerSelect');
  if (centerSelect) {
    centerSelect.value = State.activeCenterId;
    centerSelect.addEventListener('change', (e) => {
      State.activeCenterId = e.target.value;
      State.activeCenterName = e.target.options[e.target.selectedIndex].text;
      updateNavbarCenterBadge();
      loadCenterData();
    });
  }
  updateNavbarCenterBadge();
}

function updateNavbarCenterBadge() {
  const badge = document.getElementById('activeCenterBadgeText');
  if (badge) {
    badge.textContent = `${State.activeCenterName} (${State.activeCenterId})`;
  }
}

async function loadCenterData() {
  setUiState('Loading');
  try {
    await Promise.all([fetchSubscriptionStatus(), fetchServices()]);
  } catch (err) {
    setUiState('Error', err.message || 'Error al conectar con la API.');
  }
}

async function fetchSubscriptionStatus() {
  try {
    const res = await fetch(`/api/v1/billing/status/${State.activeCenterId}`);
    if (res.ok) {
      const data = await res.json();
      const isPrem = data.plan === 1 || data.plan === 'Premium';
      State.subscription = {
        plan: isPrem ? 'Premium' : 'Free',
        active: data.activeEscandallos ?? 0,
        max: data.maxAllowedEscandallos ?? 2
      };
      renderQuota();
    }
  } catch (e) {
    console.warn('Subscription fetch error:', e);
  }
}

async function fetchServices() {
  const res = await fetch('/api/v1/escandallos', {
    headers: { 'X-Center-Id': State.activeCenterId }
  });
  if (!res.ok) {
    throw new Error(`Error ${res.status}: fallo al cargar escandallos.`);
  }
  const data = await res.json();
  State.services = Array.isArray(data) ? data : [];
  setUiState(State.services.length === 0 ? 'Empty' : 'Nominal');
}

function setUiState(uiState, errorMsg = null) {
  State.uiState = uiState;
  State.errorMessage = errorMsg;
  renderUiMatrix();
}

function renderQuota() {
  const badge = document.getElementById('quotaBadge');
  if (!badge) return;
  const isFull = State.subscription.plan === 'Free' && State.subscription.active >= State.subscription.max;
  badge.textContent = `Escandallos activos: ${State.subscription.active}/${State.subscription.max} (Plan ${State.subscription.plan})`;
  badge.className = `quota-badge ${isFull ? 'quota-full' : ''}`;
}

function toggleElement(id, isVisible) {
  const el = document.getElementById(id);
  if (el) el.style.display = isVisible ? 'block' : 'none';
}

function renderUiMatrix() {
  toggleElement('stateLoading', State.uiState === 'Loading');
  toggleElement('stateEmpty', State.uiState === 'Empty');
  renderErrorState();
  renderNominalState();
}

function renderErrorState() {
  const errorEl = document.getElementById('stateError');
  if (!errorEl) return;
  const isErr = State.uiState === 'Error';
  errorEl.style.display = isErr ? 'flex' : 'none';
  if (isErr) {
    const msgEl = document.getElementById('errorMessageText');
    if (msgEl) msgEl.textContent = State.errorMessage || 'Error desconocido';
  }
}

function renderNominalState() {
  const isNominal = State.uiState === 'Nominal';
  toggleElement('stateNominal', isNominal);
  if (isNominal) renderTable();
}

function getTrafficColorName(color) {
  if (typeof color === 'number') {
    return color === 0 ? 'Verde' : color === 1 ? 'Amarillo' : 'Rojo';
  }
  return color || 'Amarillo';
}

function buildServiceActionButtons(s) {
  const toggleLabel = s.isActive ? '⏸️ Pausar' : '▶️ Activar';
  return `
    <div style="display:flex; gap:0.35rem;">
      <button class="btn btn-secondary btn-sm" onclick="handleDuplicate('${s.id}')">📋 Duplicar</button>
      <button class="btn btn-secondary btn-sm" onclick="handleToggleStatus('${s.id}', ${!s.isActive})">${toggleLabel}</button>
    </div>`;
}

function createServiceRow(s) {
  const tr = document.createElement('tr');
  if (!s.isActive) tr.classList.add('inactive-row');

  const color = getTrafficColorName(s.trafficLightColor);
  const marginText = (s.netMarginPercentage ?? 0).toFixed(1) + '%';
  const warningHtml = s.lossWarningMessage ? `<div class="loss-warning">⚠️ ${escapeHtml(s.lossWarningMessage)}</div>` : '';
  const statusLabel = s.isActive ? 'Activo' : 'Inactivo';

  tr.innerHTML = `
    <td><strong>${escapeHtml(s.serviceName)}</strong>${warningHtml}</td>
    <td>${s.durationMinutes} min</td>
    <td>${(s.totalEscandalloCost ?? 0).toFixed(2)} €</td>
    <td>${(s.suggestedPvp ?? 0).toFixed(2)} €</td>
    <td>${(s.appliedPvr ?? 0).toFixed(2)} €</td>
    <td>${marginText}</td>
    <td><span class="badge-traffic ${color}">${color}</span></td>
    <td><span class="badge-traffic ${s.isActive ? 'Verde' : ''}">${statusLabel}</span></td>
    <td>${buildServiceActionButtons(s)}</td>`;
  return tr;
}

function renderTable() {
  const tbody = document.getElementById('servicesTableBody');
  if (!tbody) return;
  tbody.innerHTML = '';
  State.services.forEach((s) => tbody.appendChild(createServiceRow(s)));
}

async function handleCreateNew() {
  if (State.subscription.plan === 'Free' && State.subscription.active >= State.subscription.max) {
    openUpgradeModal();
    return;
  }
  openCreateModal();
}

async function handleDuplicate(id) {
  if (State.subscription.plan === 'Free' && State.subscription.active >= State.subscription.max) {
    openUpgradeModal();
    return;
  }

  try {
    const res = await fetch(`/api/v1/escandallos/${id}/duplicate`, {
      method: 'POST',
      headers: { 'X-Center-Id': State.activeCenterId }
    });
    if (res.status === 403) {
      openUpgradeModal();
      return;
    }
    if (res.ok) await loadCenterData();
  } catch (err) {
    alert('Error al duplicar: ' + err.message);
  }
}

async function handleToggleStatus(id, newStatus) {
  try {
    const res = await fetch(`/api/v1/escandallos/${id}/status`, {
      method: 'PATCH',
      headers: { 'Content-Type': 'application/json', 'X-Center-Id': State.activeCenterId },
      body: JSON.stringify({ isActive: newStatus })
    });
    if (res.status === 403) {
      openUpgradeModal();
      return;
    }
    if (res.ok) await loadCenterData();
  } catch (err) {
    alert('Error al actualizar estado: ' + err.message);
  }
}

function initCostsForm() {
  const hoursInput = document.getElementById('inputHours');
  const minutesLabel = document.getElementById('calcMinutes');
  if (hoursInput && minutesLabel) {
    hoursInput.addEventListener('input', () => {
      const hours = parseFloat(hoursInput.value) || 0;
      minutesLabel.textContent = `${(hours * 60).toLocaleString()} min/mes`;
    });
  }
}

function readCostsInput() {
  return {
    monthlyOpeningHours: parseFloat(document.getElementById('inputHours')?.value) || 176,
    localRent: parseFloat(document.getElementById('inputRent')?.value) || 335,
    supplies: parseFloat(document.getElementById('inputSupplies')?.value) || 100,
    generalCabinConsumables: parseFloat(document.getElementById('inputCabinConsumables')?.value) || 440
  };
}

async function handleSaveCosts(e) {
  e.preventDefault();
  try {
    const res = await fetch('/api/v1/costs/parameters', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(readCostsInput())
    });
    if (res.ok) {
      const data = await res.json();
      const el = document.getElementById('calculatedOverheadMinute');
      if (el) el.textContent = `${data.overheadCostPerMinute.toFixed(5)} €/min`;
      alert('Costes generales actualizados correctamente.');
    }
  } catch (err) {
    alert('Error al guardar costes: ' + err.message);
  }
}

async function handleLoadDemo() {
  try {
    const res = await fetch('/api/v1/demo/load', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ centerId: State.activeCenterId, overrideIfRealExists: true })
    });
    if (res.ok) await loadCenterData();
  } catch (e) {
    alert('Error al cargar demo: ' + e.message);
  }
}

async function handlePurgeDemo() {
  if (!confirm('¿Deseas eliminar los datos de demostración?')) return;
  try {
    const res = await fetch(`/api/v1/demo/purge/${State.activeCenterId}`, { method: 'POST' });
    if (res.ok) await loadCenterData();
  } catch (e) {
    alert('Error al purgar demo: ' + e.message);
  }
}

function handleExportPdf() {
  window.open(`/api/v1/export/pdf?centerId=${State.activeCenterId}`, '_blank');
}

function handleExportXlsx() {
  window.open(`/api/v1/export/xlsx?centerId=${State.activeCenterId}`, '_blank');
}

function openUpgradeModal() {
  const modal = document.getElementById('upgradeModal');
  if (modal) modal.style.display = 'flex';
}

function closeUpgradeModal() {
  const modal = document.getElementById('upgradeModal');
  if (modal) modal.style.display = 'none';
}

async function simulateUpgradeToPremium() {
  const now = Math.floor(Date.now() / 1000);
  const payload = JSON.stringify({
    id: `evt_sim_${Date.now()}`,
    type: 'checkout.session.completed',
    center_id: State.activeCenterId,
    plan: 'Premium'
  });

  try {
    const res = await fetch('/api/v1/billing/stripe-webhook', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json', 'Stripe-Signature': `t=${now},v1=sim_sig` },
      body: payload
    });
    if (res.ok) {
      closeUpgradeModal();
      alert('Suscripción Premium activada con éxito.');
      await loadCenterData();
    }
  } catch (err) {
    alert('Error al simular suscripción: ' + err.message);
  }
}

function openCreateModal() {
  const modal = document.getElementById('createModal');
  if (modal) modal.style.display = 'flex';
}

function closeCreateModal() {
  const modal = document.getElementById('createModal');
  if (modal) modal.style.display = 'none';
}

function updateConsumableDetails() {
  const sel = document.getElementById('createConsumableProduct');
  const unitLabel = document.getElementById('createConsumableUnitLabel');
  if (sel && unitLabel) {
    const opt = sel.selectedOptions[0];
    unitLabel.textContent = opt ? opt.dataset.unitName || 'ud' : 'ml';
  }
}

function extractConsumableUsage() {
  const select = document.getElementById('createConsumableProduct');
  const option = select ? select.selectedOptions[0] : null;
  const amount = parseFloat(document.getElementById('createConsumableAmount')?.value) || 0;

  if (!option || option.value === 'none' || amount <= 0) {
    return [];
  }

  const unitCost = parseFloat(option.dataset.unitCost) || 0;
  const unit = parseInt(option.dataset.unit, 10) || 0;
  return [{
    productId: option.value,
    productName: option.dataset.name,
    amountUsed: amount,
    unitOfMeasure: unit,
    computedCost: Math.round(amount * unitCost * 100) / 100
  }];
}

function buildServicePayload() {
  return {
    serviceName: document.getElementById('createServiceName')?.value || 'Nuevo Servicio',
    durationMinutes: parseInt(document.getElementById('createDuration')?.value, 10) || 60,
    appliedPvr: parseFloat(document.getElementById('createPvr')?.value) || 60,
    taxRatePercentage: 21,
    workerCategoryId: document.getElementById('createWorkerCategory')?.value || 'Esteticista',
    consumables: extractConsumableUsage(),
    packs: []
  };
}

async function handleCreateServiceSubmit(e) {
  e.preventDefault();
  try {
    const res = await fetch('/api/v1/escandallos', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json', 'X-Center-Id': State.activeCenterId },
      body: JSON.stringify(buildServicePayload())
    });
    if (res.status === 403) {
      closeCreateModal();
      openUpgradeModal();
      return;
    }
    if (res.ok) {
      closeCreateModal();
      await loadCenterData();
    }
  } catch (err) {
    alert('Error al crear servicio: ' + err.message);
  }
}

function escapeHtml(str) {
  if (!str) return '';
  return str.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;').replace(/"/g, '&quot;');
}
