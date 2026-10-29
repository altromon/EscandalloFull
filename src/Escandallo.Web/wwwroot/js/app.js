// Escandallo Full SPA - Clean Client Architecture (CMP-WEB-UI-001)

const State = {
  authToken: sessionStorage.getItem('escandallo_token') || null,
  currentUser: JSON.parse(sessionStorage.getItem('escandallo_user') || 'null'),
  activeCenterId: sessionStorage.getItem('escandallo_center_id') || 'ctr-madrid-1',
  activeCenterName: sessionStorage.getItem('escandallo_center_name') || 'Centro Belleza Madrid',
  authorizedCenters: [],
  services: [],
  workers: [],
  products: [],
  subscription: { plan: 'Free', active: 1, max: 2 },
  uiState: 'Loading',
  errorMessage: null
};

document.addEventListener('DOMContentLoaded', () => {
  initNavbar();
  initCostsForm();
  checkAuthAndInit();
});

function getAuthHeaders(extraHeaders = {}) {
  const headers = { ...extraHeaders, 'X-Center-Id': State.activeCenterId };
  if (State.authToken) {
    headers['Authorization'] = `Bearer ${State.authToken}`;
  }
  return headers;
}

async function authenticatedFetch(url, options = {}) {
  const mergedOptions = {
    ...options,
    headers: getAuthHeaders(options.headers || {})
  };

  const res = await fetch(url, mergedOptions);
  if (res.status === 401) {
    handleSessionExpired();
    throw new Error('Sesión expirada o credenciales inválidas. Por favor inicia sesión de nuevo.');
  }
  return res;
}

function handleSessionExpired() {
  State.authToken = null;
  State.currentUser = null;
  sessionStorage.removeItem('escandallo_token');
  sessionStorage.removeItem('escandallo_user');
  updateAuthVisibility();
  openLoginModal();
}

function checkAuthAndInit() {
  updateAuthVisibility();
  if (!State.authToken) {
    openLoginModal();
    setUiState('Empty');
  } else {
    closeLoginModal();
    initUserSession();
  }
}

function updateAuthVisibility() {
  const userBadge = document.getElementById('navUserBadge');
  const userEmailSpan = document.getElementById('userEmailSpan');
  const btnLogout = document.getElementById('btnLogout');
  const btnOpenNewCenter = document.getElementById('btnOpenNewCenter');

  if (State.authToken && State.currentUser) {
    if (userBadge) userBadge.style.display = 'flex';
    if (userEmailSpan) userEmailSpan.textContent = State.currentUser.email;
    if (btnLogout) btnLogout.style.display = 'inline-flex';
    if (btnOpenNewCenter) btnOpenNewCenter.style.display = 'inline-flex';
  } else {
    if (userBadge) userBadge.style.display = 'none';
    if (btnLogout) btnLogout.style.display = 'none';
    if (btnOpenNewCenter) btnOpenNewCenter.style.display = 'none';
  }
}

async function initUserSession() {
  try {
    if (State.currentUser?.userId) {
      const res = await fetch(`/api/v1/auth/centers/${State.currentUser.userId}`);
      if (res.ok) {
        State.authorizedCenters = await res.json();
        renderCenterSelect();
      }
    }
    await loadCenterData();
  } catch (err) {
    console.warn('Error al inicializar sesión:', err);
    await loadCenterData();
  }
}

function renderCenterSelect() {
  const centerSelect = document.getElementById('centerSelect');
  if (!centerSelect || State.authorizedCenters.length === 0) return;

  centerSelect.innerHTML = '';
  State.authorizedCenters.forEach(c => {
    const opt = document.createElement('option');
    opt.value = c.centerId;
    opt.textContent = c.name;
    if (c.centerId === State.activeCenterId) opt.selected = true;
    centerSelect.appendChild(opt);
  });

  const selectedOpt = centerSelect.selectedOptions[0];
  if (selectedOpt) {
    State.activeCenterName = selectedOpt.textContent;
    updateNavbarCenterBadge();
  }
}

function openLoginModal() {
  const modal = document.getElementById('loginModal');
  if (modal) modal.style.display = 'flex';
}

function closeLoginModal() {
  const modal = document.getElementById('loginModal');
  if (modal) modal.style.display = 'none';
}

function storeSessionData(session, email) {
  State.authToken = session.token;
  State.currentUser = { userId: session.userId, email, role: session.role };
  State.activeCenterId = session.activeCenterId;
  sessionStorage.setItem('escandallo_token', State.authToken);
  sessionStorage.setItem('escandallo_user', JSON.stringify(State.currentUser));
  sessionStorage.setItem('escandallo_center_id', State.activeCenterId);
}

function displayFormError(errorDivId, message) {
  const errorDiv = document.getElementById(errorDivId);
  if (errorDiv) {
    errorDiv.textContent = message;
    errorDiv.style.display = 'block';
  }
}

async function postJson(url, data, fallbackMsg) {
  const res = await fetch(url, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(data)
  });
  if (!res.ok) {
    const err = await res.json().catch(() => ({}));
    throw new Error(err.message || fallbackMsg);
  }
  return res.json();
}

function switchAuthTab(tab) {
  const isReg = tab === 'register';
  const loginForm = document.getElementById('loginForm');
  const registerForm = document.getElementById('registerForm');
  const tabLogin = document.getElementById('tabBtnLogin');
  const tabRegister = document.getElementById('tabBtnRegister');
  const title = document.getElementById('authModalTitle');

  if (loginForm) loginForm.style.display = isReg ? 'none' : 'block';
  if (registerForm) registerForm.style.display = isReg ? 'block' : 'none';
  if (tabLogin) tabLogin.classList.toggle('active', !isReg);
  if (tabRegister) tabRegister.classList.toggle('active', isReg);
  if (title) title.textContent = isReg ? '✨ Crear Cuenta de Salón' : '🔐 Acceso al Salón';
}

async function handleRegisterSubmit(e) {
  e.preventDefault();
  const email = document.getElementById('registerEmail')?.value.trim() || '';
  const password = document.getElementById('registerPassword')?.value.trim() || '';
  const centerName = document.getElementById('registerCenterName')?.value.trim() || '';
  try {
    const session = await postJson('/api/v1/auth/register', { email, password, centerName }, 'Error al registrar usuario.');
    storeSessionData(session, email);
    closeLoginModal();
    updateAuthVisibility();
    await initUserSession();
  } catch (err) {
    displayFormError('registerError', err.message);
  }
}

function openCreateCenterModal() {
  const modal = document.getElementById('createCenterModal');
  const errorDiv = document.getElementById('createCenterError');
  if (errorDiv) errorDiv.style.display = 'none';
  if (modal) modal.style.display = 'flex';
}

function closeCreateCenterModal() {
  const modal = document.getElementById('createCenterModal');
  if (modal) modal.style.display = 'none';
}

async function refreshAuthorizedCenters() {
  if (!State.currentUser?.userId) return;
  const centersRes = await fetch(`/api/v1/auth/centers/${State.currentUser.userId}`);
  if (centersRes.ok) {
    State.authorizedCenters = await centersRes.json();
    renderCenterSelect();
  }
}

async function postAuthCenter(centerName) {
  const res = await authenticatedFetch('/api/v1/auth/centers', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ userId: State.currentUser?.userId, centerName })
  });
  if (!res.ok) {
    const err = await res.json().catch(() => ({}));
    throw new Error(err.message || 'Error al crear el centro.');
  }
  return res.json();
}

async function handleCreateCenterSubmit(e) {
  e.preventDefault();
  const nameInput = document.getElementById('newCenterNameInput');
  try {
    const newCenter = await postAuthCenter(nameInput?.value.trim() || '');
    closeCreateCenterModal();
    if (nameInput) nameInput.value = '';
    State.activeCenterId = newCenter.centerId;
    State.activeCenterName = newCenter.name;
    sessionStorage.setItem('escandallo_center_id', State.activeCenterId);
    sessionStorage.setItem('escandallo_center_name', State.activeCenterName);
    await refreshAuthorizedCenters();
    updateNavbarCenterBadge();
    await loadCenterData();
  } catch (err) {
    displayFormError('createCenterError', err.message);
  }
}

async function handleLogin(e) {
  e.preventDefault();
  const email = document.getElementById('loginEmail')?.value.trim() || '';
  const password = document.getElementById('loginPassword')?.value.trim() || '';
  try {
    const session = await postJson('/api/v1/auth/login', { email, password }, 'Credenciales inválidas.');
    storeSessionData(session, email);
    closeLoginModal();
    updateAuthVisibility();
    await initUserSession();
  } catch (err) {
    displayFormError('loginError', err.message);
  }
}

function handleLogout() {
  State.authToken = null;
  State.currentUser = null;
  sessionStorage.removeItem('escandallo_token');
  sessionStorage.removeItem('escandallo_user');
  updateAuthVisibility();
  setUiState('Empty');
  openLoginModal();
}

async function handleCenterSelectionChange(newCenterId, newCenterName) {
  if (State.currentUser?.userId) {
    try {
      const res = await fetch('/api/v1/auth/select-center', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ userId: State.currentUser.userId, centerId: newCenterId })
      });
      if (res.ok) {
        const updated = await res.json();
        State.authToken = updated.token;
        sessionStorage.setItem('escandallo_token', State.authToken);
      }
    } catch (err) {
      console.warn('Error selecting center:', err);
    }
  }
  State.activeCenterId = newCenterId;
  State.activeCenterName = newCenterName;
  sessionStorage.setItem('escandallo_center_id', State.activeCenterId);
  sessionStorage.setItem('escandallo_center_name', State.activeCenterName);
  updateNavbarCenterBadge();
  loadCenterData();
}

function initNavbar() {
  const centerSelect = document.getElementById('centerSelect');
  if (centerSelect) {
    centerSelect.addEventListener('change', (e) => {
      const newCenterId = e.target.value;
      const newCenterName = e.target.options[e.target.selectedIndex].text;
      handleCenterSelectionChange(newCenterId, newCenterName);
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
  if (!State.authToken) {
    setUiState('Empty');
    return;
  }

  setUiState('Loading');
  try {
    await Promise.all([
      fetchSubscriptionStatus(),
      fetchCostParameters(),
      fetchServices(),
      fetchWorkers(),
      fetchProducts()
    ]);
  } catch (err) {
    setUiState('Error', err.message || 'Error al conectar con la API.');
  }
}

function setUnmodifiedField(id, val, decimals = null) {
  const el = document.getElementById(id);
  if (el && !el.dataset.modified && val != null) {
    el.value = decimals !== null ? Number(val).toFixed(decimals) : val;
  }
}

function applyCostParameters(p) {
  const hours = p.monthlyOpeningHours || 176;
  setUnmodifiedField('inputHours', hours);
  const minEl = document.getElementById('calcMinutes');
  const hoursEl = document.getElementById('inputHours');
  if (minEl && hoursEl && !hoursEl.dataset.modified) {
    minEl.textContent = `${(hours * 60).toLocaleString()} min/mes`;
  }
  setUnmodifiedField('inputRent', p.localRent, 2);
  setUnmodifiedField('inputSupplies', p.supplies, 2);
  setUnmodifiedField('inputCabinConsumables', p.generalCabinConsumables, 2);
  const el = document.getElementById('calculatedOverheadMinute');
  if (el && typeof p.overheadCostPerMinute === 'number') {
    el.textContent = `${p.overheadCostPerMinute.toFixed(5)} €/min`;
  }
}

async function fetchCostParameters() {
  try {
    const res = await authenticatedFetch('/api/v1/costs/parameters');
    if (res.ok) {
      const p = await res.json();
      applyCostParameters(p);
    }
  } catch (err) {
    console.warn('Error fetching cost parameters:', err);
  }
}

async function fetchSubscriptionStatus() {
  try {
    const res = await authenticatedFetch(`/api/v1/billing/status/${State.activeCenterId}`);
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
  const res = await authenticatedFetch('/api/v1/escandallos');
  if (!res.ok) {
    throw new Error(`Error ${res.status}: fallo al cargar escandallos.`);
  }
  const data = await res.json();
  State.services = Array.isArray(data) ? data : [];
  setUiState(State.services.length === 0 ? 'Empty' : 'Nominal');
}

async function fetchWorkers() {
  try {
    const res = await authenticatedFetch('/api/v1/costs/workers');
    if (res.ok) {
      State.workers = await res.json();
      renderWorkersTable();
      renderCreateModalOptions();
    }
  } catch (err) {
    console.warn('Error fetching workers:', err);
  }
}

function buildWorkerRow(w) {
  const monthly = typeof w.monthlyCompanyCost === 'number' ? w.monthlyCompanyCost : (w.monthlyCost ?? 0);
  const perMin = typeof w.costPerMinute === 'number' ? w.costPerMinute : 0;
  const tr = document.createElement('tr');
  tr.innerHTML = `
    <td><strong>${escapeHtml(w.categoryName)}</strong></td>
    <td>${monthly.toFixed(2)} €</td>
    <td>${perMin.toFixed(4)} €</td>
    <td>
      <div style="display:flex; gap:0.25rem;">
        <button class="btn btn-secondary btn-sm" onclick="openEditWorkerModal('${w.id}', '${escapeHtml(w.categoryName)}', ${monthly})">✏️</button>
        <button class="btn btn-danger btn-sm" onclick="handleDeleteWorker('${w.id}')">🗑️</button>
      </div>
    </td>`;
  return tr;
}

function renderWorkersTable() {
  const tbody = document.getElementById('workersTableBody');
  if (!tbody) return;
  tbody.innerHTML = '';
  if (!State.workers || State.workers.length === 0) {
    tbody.innerHTML = '<tr><td colspan="4" style="text-align:center; color:var(--text-muted);">Sin personal registrado</td></tr>';
    return;
  }
  State.workers.forEach(w => tbody.appendChild(buildWorkerRow(w)));
}

async function fetchProducts() {
  try {
    const res = await authenticatedFetch('/api/v1/costs/products');
    if (res.ok) {
      State.products = await res.json();
      renderProductsTable();
      renderCreateModalOptions();
    }
  } catch (err) {
    console.warn('Error fetching products:', err);
  }
}

function getUnitName(unit) {
  if (unit === 0 || unit === 'Milliliter' || unit === 'Ml') return 'ml';
  if (unit === 1 || unit === 'Gram' || unit === 'Gramo') return 'g';
  return 'ud';
}

function getUnitCode(u) {
  if (typeof u === 'number') return u;
  if (u === 'Gram') return 1;
  return u === 'Unidad' ? 2 : 0;
}

function extractProductMetrics(p) {
  return {
    unit: getUnitName(p.unitOfMeasure),
    cap: p.capacity ?? p.packageCapacity ?? 0,
    price: p.packagePrice ?? 0,
    unitCost: p.effectiveUnitCost ?? p.grossUnitCost ?? p.costPerUnit ?? 0,
    waste: p.wastePercentage ?? 0,
    unitVal: getUnitCode(p.unitOfMeasure)
  };
}

function buildProductRow(p) {
  const m = extractProductMetrics(p);
  const tr = document.createElement('tr');
  tr.innerHTML = `
    <td><strong>${escapeHtml(p.name)}</strong></td>
    <td>${m.cap} ${m.unit} (${m.price.toFixed(2)} €)</td>
    <td>${m.unitCost.toFixed(4)} €/${m.unit}</td>
    <td>
      <div style="display:flex; gap:0.25rem;">
        <button class="btn btn-secondary btn-sm" onclick="openEditProductModal('${p.id}', '${escapeHtml(p.name)}', ${m.price}, ${m.cap}, ${m.unitVal}, ${m.waste})">✏️</button>
        <button class="btn btn-danger btn-sm" onclick="handleDeleteProduct('${p.id}')">🗑️</button>
      </div>
    </td>`;
  return tr;
}

function renderProductsTable() {
  const tbody = document.getElementById('productsTableBody');
  if (!tbody) return;
  tbody.innerHTML = '';
  if (!State.products || State.products.length === 0) {
    tbody.innerHTML = '<tr><td colspan="4" style="text-align:center; color:var(--text-muted);">Sin cosméticos registrados</td></tr>';
    return;
  }
  State.products.forEach(p => tbody.appendChild(buildProductRow(p)));
}

function switchConfigTab(tabName) {
  const tabs = ['salon', 'workers', 'products'];
  tabs.forEach(t => {
    const btn = document.getElementById(`tabBtn${capitalize(t)}`);
    const content = document.getElementById(`tabContent${capitalize(t)}`);
    if (btn) btn.classList.toggle('active', t === tabName);
    if (content) content.classList.toggle('active', t === tabName);
  });
}

function capitalize(s) {
  return s.charAt(0).toUpperCase() + s.slice(1);
}

function setUiState(uiState, errorMsg = null) {
  State.uiState = uiState;
  State.errorMessage = errorMsg;
  renderUiMatrix();
}

function renderQuota() {
  const badge = document.getElementById('quotaBadge');
  if (!badge) return;
  const isPremium = State.subscription.plan === 'Premium';
  const isFull = !isPremium && State.subscription.active >= State.subscription.max;

  if (isPremium) {
    badge.textContent = `Escandallos activos: ${State.subscription.active} (Plan Pro Ilimitado)`;
    badge.className = 'quota-badge';
  } else {
    badge.textContent = `Escandallos activos: ${State.subscription.active}/${State.subscription.max} (Plan Free)`;
    badge.className = `quota-badge ${isFull ? 'quota-full' : ''}`;
  }

  const btnSub = document.getElementById('btnSubscribePro');
  const btnCancel = document.getElementById('btnCancelPro');
  if (btnSub) btnSub.style.display = isPremium ? 'none' : 'inline-flex';
  if (btnCancel) btnCancel.style.display = isPremium ? 'inline-flex' : 'none';
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
      <button class="btn btn-danger btn-sm" onclick="handleDeleteService('${s.id}')">🗑️ Borrar</button>
    </div>`;
}

async function handleDeleteService(id) {
  if (!confirm('¿Deseas eliminar este escandallo de tratamiento?')) return;
  try {
    const res = await authenticatedFetch(`/api/v1/escandallos/${id}`, {
      method: 'DELETE'
    });
    if (res.ok) {
      await loadCenterData();
    } else {
      const err = await res.json().catch(() => ({}));
      alert(err.message || 'Error al eliminar escandallo.');
    }
  } catch (err) {
    alert('Error: ' + err.message);
  }
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
    const res = await authenticatedFetch(`/api/v1/escandallos/${id}/duplicate`, {
      method: 'POST'
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
    const res = await authenticatedFetch(`/api/v1/escandallos/${id}/status`, {
      method: 'PATCH',
      headers: { 'Content-Type': 'application/json' },
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
  const form = document.getElementById('costsForm');
  if (form) {
    form.addEventListener('input', (e) => {
      if (e.target && e.target.tagName === 'INPUT') {
        e.target.dataset.modified = 'true';
      }
    });
  }
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
    const res = await authenticatedFetch('/api/v1/costs/parameters', {
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
    const res = await authenticatedFetch('/api/v1/demo/load', {
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
    const res = await authenticatedFetch(`/api/v1/demo/purge/${State.activeCenterId}`, { method: 'POST' });
    if (res.ok) await loadCenterData();
  } catch (e) {
    alert('Error al purgar demo: ' + e.message);
  }
}

async function handleExportPdf() {
  try {
    const res = await authenticatedFetch(`/api/v1/export/pdf?centerId=${State.activeCenterId}`);
    if (!res.ok) {
      throw new Error(`Error ${res.status}: no se pudo generar el PDF.`);
    }
    const blob = await res.blob();
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `Escandallos_${State.activeCenterId}.pdf`;
    document.body.appendChild(a);
    a.click();
    a.remove();
    window.URL.revokeObjectURL(url);
  } catch (err) {
    alert('Error al descargar PDF: ' + err.message);
  }
}

async function handleExportXlsx() {
  try {
    const res = await authenticatedFetch(`/api/v1/export/xlsx?centerId=${State.activeCenterId}`);
    if (!res.ok) {
      throw new Error(`Error ${res.status}: no se pudo exportar a Excel.`);
    }
    const blob = await res.blob();
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `Escandallos_${State.activeCenterId}.xlsx`;
    document.body.appendChild(a);
    a.click();
    a.remove();
    window.URL.revokeObjectURL(url);
  } catch (err) {
    alert('Error al descargar Excel: ' + err.message);
  }
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
  await handleSubscribePro();
}

async function handleSubscribePro() {
  try {
    const res = await authenticatedFetch(`/api/v1/billing/mock-complete-checkout?centerId=${State.activeCenterId}`, {
      method: 'POST'
    });
    if (res.ok) {
      closeUpgradeModal();
      alert('¡Plan Pro contratado con éxito! Ahora tienes escandallos y bonos ilimitados.');
      await loadCenterData();
    } else {
      const err = await res.json().catch(() => ({}));
      alert(err.message || 'Error al contratar Plan Pro.');
    }
  } catch (err) {
    alert('Error al contratar Plan Pro: ' + err.message);
  }
}

async function handleCancelPro() {
  if (!confirm('¿Deseas cancelar la suscripción al Plan Pro y volver al Plan Free?')) return;
  try {
    const res = await authenticatedFetch(`/api/v1/billing/mock-cancel-subscription?centerId=${State.activeCenterId}`, {
      method: 'POST'
    });
    if (res.ok) {
      alert('Suscripción Pro cancelada. Has vuelto al Plan Free.');
      await loadCenterData();
    } else {
      const err = await res.json().catch(() => ({}));
      alert(err.message || 'Error al cancelar suscripción.');
    }
  } catch (err) {
    alert('Error al cancelar suscripción: ' + err.message);
  }
}

function openCreateModal() {
  renderCreateModalOptions();
  const modal = document.getElementById('createModal');
  if (modal) modal.style.display = 'flex';
}

function closeCreateModal() {
  const modal = document.getElementById('createModal');
  if (modal) modal.style.display = 'none';
}

function openAddWorkerModal() {
  const modal = document.getElementById('addWorkerModal');
  if (modal) modal.style.display = 'flex';
}

function closeAddWorkerModal() {
  const modal = document.getElementById('addWorkerModal');
  if (modal) modal.style.display = 'none';
}

async function handleCreateWorkerSubmit(e) {
  e.preventDefault();
  const catInput = document.getElementById('workerCategoryName');
  const costInput = document.getElementById('workerMonthlyCost');
  try {
    const res = await authenticatedFetch('/api/v1/costs/workers', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        categoryName: catInput.value.trim(),
        monthlyCompanyCost: parseFloat(costInput.value) || 0
      })
    });
    if (res.ok) {
      closeAddWorkerModal();
      catInput.value = '';
      await fetchWorkers();
    } else {
      const err = await res.json().catch(() => ({}));
      alert(err.message || 'Error al guardar puesto.');
    }
  } catch (err) {
    alert('Error: ' + err.message);
  }
}

function openAddProductModal() {
  const modal = document.getElementById('addProductModal');
  if (modal) modal.style.display = 'flex';
}

function closeAddProductModal() {
  const modal = document.getElementById('addProductModal');
  if (modal) modal.style.display = 'none';
}

async function sendProductPayload(url, method, payload, errorMsg) {
  const res = await authenticatedFetch(url, {
    method,
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(payload)
  });
  if (!res.ok) {
    const err = await res.json().catch(() => ({}));
    throw new Error(err.message || errorMsg);
  }
}

async function handleCreateProductSubmit(e) {
  e.preventDefault();
  const name = document.getElementById('productName')?.value.trim() || '';
  const packagePrice = parseFloat(document.getElementById('productPrice')?.value) || 0;
  const capacity = parseFloat(document.getElementById('productCapacity')?.value) || 1;
  const unitOfMeasure = parseInt(document.getElementById('productUnit')?.value, 10) || 0;
  const wastePercentage = parseFloat(document.getElementById('productWaste')?.value) || 0;

  try {
    await sendProductPayload('/api/v1/costs/products', 'POST', { name, packagePrice, capacity, unitOfMeasure, wastePercentage }, 'Error al guardar cosmético.');
    closeAddProductModal();
    const input = document.getElementById('productName');
    if (input) input.value = '';
    await fetchProducts();
  } catch (err) {
    alert(err.message);
  }
}

function openEditProductPriceModal(id, name, currentPrice) {
  const modal = document.getElementById('editProductPriceModal');
  const label = document.getElementById('editProductLabel');
  const idInput = document.getElementById('editProductId');
  const priceInput = document.getElementById('editProductNewPrice');

  if (idInput) idInput.value = id;
  if (label) label.textContent = name;
  if (priceInput) priceInput.value = currentPrice;
  if (modal) modal.style.display = 'flex';
}

function closeEditProductPriceModal() {
  const modal = document.getElementById('editProductPriceModal');
  if (modal) modal.style.display = 'none';
}

async function handleUpdateProductPriceSubmit(e) {
  e.preventDefault();
  const id = document.getElementById('editProductId')?.value;
  const newPrice = parseFloat(document.getElementById('editProductNewPrice')?.value) || 0;

  try {
    const res = await authenticatedFetch(`/api/v1/costs/products/${id}`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ newPackagePrice: newPrice })
    });
    if (res.ok) {
      closeEditProductPriceModal();
      await fetchProducts();
    } else {
      const err = await res.json().catch(() => ({}));
      alert(err.message || 'Error al actualizar precio.');
    }
  } catch (err) {
    alert('Error: ' + err.message);
  }
}

function openEditWorkerModal(id, categoryName, monthlyCost) {
  const idEl = document.getElementById('editWorkerId');
  const catEl = document.getElementById('editWorkerCategoryName');
  const costEl = document.getElementById('editWorkerMonthlyCost');
  const modal = document.getElementById('editWorkerModal');
  if (idEl) idEl.value = id;
  if (catEl) catEl.value = categoryName;
  if (costEl) costEl.value = monthlyCost;
  if (modal) modal.style.display = 'flex';
}

function closeEditWorkerModal() {
  const modal = document.getElementById('editWorkerModal');
  if (modal) modal.style.display = 'none';
}

async function handleUpdateWorkerSubmit(e) {
  e.preventDefault();
  const id = document.getElementById('editWorkerId')?.value;
  const name = document.getElementById('editWorkerCategoryName')?.value.trim();
  const cost = parseFloat(document.getElementById('editWorkerMonthlyCost')?.value) || 0;

  try {
    const res = await authenticatedFetch(`/api/v1/costs/workers/${id}`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ categoryName: name, monthlyCompanyCost: cost })
    });
    if (res.ok) {
      closeEditWorkerModal();
      await fetchWorkers();
    } else {
      const err = await res.json().catch(() => ({}));
      alert(err.message || 'Error al actualizar puesto.');
    }
  } catch (err) {
    alert('Error: ' + err.message);
  }
}

async function handleDeleteWorker(id) {
  if (!confirm('¿Deseas eliminar esta categoría de personal?')) return;
  try {
    const res = await authenticatedFetch(`/api/v1/costs/workers/${id}`, {
      method: 'DELETE'
    });
    if (res.ok) {
      await fetchWorkers();
    } else {
      const err = await res.json().catch(() => ({}));
      alert(err.message || 'Error al eliminar trabajador.');
    }
  } catch (err) {
    alert('Error: ' + err.message);
  }
}

function openEditProductModal(id, name, price, capacity, unit, waste) {
  const idEl = document.getElementById('editProductFullId');
  const nameEl = document.getElementById('editProductFullName');
  const priceEl = document.getElementById('editProductFullPrice');
  const capEl = document.getElementById('editProductFullCapacity');
  const unitEl = document.getElementById('editProductFullUnit');
  const wasteEl = document.getElementById('editProductFullWaste');
  const modal = document.getElementById('editProductModal');

  if (idEl) idEl.value = id;
  if (nameEl) nameEl.value = name;
  if (priceEl) priceEl.value = price;
  if (capEl) capEl.value = capacity;
  if (unitEl) unitEl.value = unit;
  if (wasteEl) wasteEl.value = waste;
  if (modal) modal.style.display = 'flex';
}

function closeEditProductModal() {
  const modal = document.getElementById('editProductModal');
  if (modal) modal.style.display = 'none';
}

async function handleUpdateProductFullSubmit(e) {
  e.preventDefault();
  const id = document.getElementById('editProductFullId')?.value;
  const name = document.getElementById('editProductFullName')?.value.trim() || '';
  const packagePrice = parseFloat(document.getElementById('editProductFullPrice')?.value) || 0;
  const capacity = parseFloat(document.getElementById('editProductFullCapacity')?.value) || 1;
  const unitOfMeasure = parseInt(document.getElementById('editProductFullUnit')?.value, 10) || 0;
  const wastePercentage = parseFloat(document.getElementById('editProductFullWaste')?.value) || 0;

  try {
    await sendProductPayload(`/api/v1/costs/products/${id}`, 'PUT', { name, packagePrice, capacity, unitOfMeasure, wastePercentage }, 'Error al actualizar producto.');
    closeEditProductModal();
    await fetchProducts();
  } catch (err) {
    alert(err.message);
  }
}

async function handleDeleteProduct(id) {
  if (!confirm('¿Deseas eliminar este producto cosmético?')) return;
  try {
    const res = await authenticatedFetch(`/api/v1/costs/products/${id}`, {
      method: 'DELETE'
    });
    if (res.ok) {
      await fetchProducts();
    } else {
      const err = await res.json().catch(() => ({}));
      alert(err.message || 'Error al eliminar cosmético.');
    }
  } catch (err) {
    alert('Error: ' + err.message);
  }
}

function populateWorkerCategoryOptions(catSelect) {
  const currentVal = catSelect.value;
  catSelect.innerHTML = '';
  State.workers.forEach(w => {
    const opt = document.createElement('option');
    opt.value = w.categoryName;
    const perMin = typeof w.costPerMinute === 'number' ? w.costPerMinute : 0;
    opt.textContent = `${w.categoryName} (${perMin.toFixed(4)} €/min)`;
    if (w.categoryName === currentVal) opt.selected = true;
    catSelect.appendChild(opt);
  });
}

function createConsumableOption(p, currentVal) {
  const unit = getUnitName(p.unitOfMeasure);
  const unitCost = typeof p.effectiveUnitCost === 'number' ? p.effectiveUnitCost : (p.grossUnitCost ?? 0);
  const opt = document.createElement('option');
  opt.value = p.id;
  opt.dataset.name = p.name;
  opt.dataset.unit = p.unitOfMeasure;
  opt.dataset.unitCost = unitCost;
  opt.dataset.unitName = unit;
  opt.textContent = `${p.name} (${unitCost.toFixed(4)} €/${unit})`;
  if (p.id === currentVal) opt.selected = true;
  return opt;
}

function populateConsumableProductOptions(prodSelect) {
  const currentVal = prodSelect.value;
  prodSelect.innerHTML = '';
  State.products.forEach(p => prodSelect.appendChild(createConsumableOption(p, currentVal)));
  const noneOpt = document.createElement('option');
  noneOpt.value = 'none';
  noneOpt.textContent = '(Sin consumibles adicionales)';
  prodSelect.appendChild(noneOpt);
  updateConsumableDetails();
}

function renderCreateModalOptions() {
  const catSelect = document.getElementById('createWorkerCategory');
  if (catSelect && State.workers.length > 0) {
    populateWorkerCategoryOptions(catSelect);
  }
  const prodSelect = document.getElementById('createConsumableProduct');
  if (prodSelect && State.products.length > 0) {
    populateConsumableProductOptions(prodSelect);
  }
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
    const res = await authenticatedFetch('/api/v1/escandallos', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
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

function openAuditModal() {
  const modal = document.getElementById('auditModal');
  if (modal) modal.style.display = 'flex';
  populateAuditCenterSelect();
  fetchAuditHistory();
}

function closeAuditModal() {
  const modal = document.getElementById('auditModal');
  if (modal) modal.style.display = 'none';
}

function appendCenterOption(select, id, text) {
  const opt = document.createElement('option');
  opt.value = id;
  opt.textContent = text;
  select.appendChild(opt);
}

function populateAuditCenterSelect() {
  const select = document.getElementById('auditCenterSelect');
  if (!select) return;
  const currentVal = select.value;
  select.innerHTML = '';

  const activeId = State.activeCenterId || 'ctr-madrid-1';
  const activeName = State.activeCenterName || 'Principal';
  appendCenterOption(select, activeId, `Centro actual (${activeName})`);

  (State.centers || []).forEach(c => {
    if (c.centerId && c.centerId !== activeId) {
      appendCenterOption(select, c.centerId, c.centerName || c.centerId);
    }
  });

  appendCenterOption(select, 'all', '🌐 Todos los centros');
  if (currentVal && Array.from(select.options).some(o => o.value === currentVal)) {
    select.value = currentVal;
  }
}

function setAuditMessage(tbody, msg, isError = false) {
  const color = isError ? 'var(--accent-rojo)' : 'var(--text-muted)';
  tbody.innerHTML = `<tr><td colspan="5" style="text-align:center; color:${color};">${escapeHtml(msg)}</td></tr>`;
}

function buildAuditRow(e, targetCenter) {
  const tr = document.createElement('tr');
  const hashShort = e.chainedHashSha256 ? `${e.chainedHashSha256.substring(0, 16)}...` : 'n/a';
  const centerBadge = targetCenter === 'all'
    ? `<span style="display:inline-block; font-size:0.68rem; padding:1px 4px; border-radius:3px; background:var(--bg-card); border:1px solid var(--border-color); margin-right:4px;">${escapeHtml(e.centerId || '')}</span> `
    : '';
  tr.innerHTML = `
    <td>${new Date(e.timestampUtc).toLocaleString()}</td>
    <td>${centerBadge}<strong>${escapeHtml(e.changeType)}</strong></td>
    <td>${Number(e.previousValue).toFixed(2)} €</td>
    <td>${Number(e.newValue).toFixed(2)} €</td>
    <td><code title="${escapeHtml(e.chainedHashSha256)}">${escapeHtml(hashShort)}</code></td>
  `;
  return tr;
}

function renderAuditRows(tbody, entries, targetCenter) {
  tbody.innerHTML = '';
  if (!entries || entries.length === 0) {
    setAuditMessage(tbody, 'Sin registros de auditoría para este centro');
    return;
  }
  entries.reverse().forEach(e => tbody.appendChild(buildAuditRow(e, targetCenter)));
}

async function fetchAuditHistory() {
  const tbody = document.getElementById('auditTableBody');
  if (!tbody) return;
  setAuditMessage(tbody, 'Cargando histórico...');

  const select = document.getElementById('auditCenterSelect');
  const targetCenter = select?.value || State.activeCenterId || 'ctr-madrid-1';

  try {
    const res = await authenticatedFetch(`/api/v1/audit/history?centerId=${encodeURIComponent(targetCenter)}`);
    if (!res.ok) {
      setAuditMessage(tbody, 'Error al consultar el histórico', true);
      return;
    }
    const entries = await res.json();
    renderAuditRows(tbody, entries, targetCenter);
  } catch (err) {
    setAuditMessage(tbody, err.message, true);
  }
}

function openLogsModal() {
  const modal = document.getElementById('logsModal');
  if (modal) modal.style.display = 'flex';
  fetchSystemLogs();
}

function closeLogsModal() {
  const modal = document.getElementById('logsModal');
  if (modal) modal.style.display = 'none';
}

function buildLogLine(l) {
  const line = document.createElement('div');
  line.style.marginBottom = '0.35rem';
  const color = l.logLevel === 'ERROR' ? '#f87171' : (l.logLevel === 'WARN' || l.logLevel === 'WARNING') ? '#fbbf24' : '#60a5fa';
  const trace = l.traceId ? ` [trace:${escapeHtml(l.traceId.substring(0, 8))}]` : '';
  line.innerHTML = `<span style="color:#64748b;">${new Date(l.timestampUtc).toLocaleTimeString()}</span> <span style="color:${color}; font-weight:bold;">[${escapeHtml(l.logLevel)}]</span> <span style="color:#a78bfa;">${escapeHtml(l.category)}</span>${trace}: ${escapeHtml(l.message)}`;
  return line;
}

async function fetchSystemLogs() {
  const consoleEl = document.getElementById('logsConsole');
  if (!consoleEl) return;
  consoleEl.innerHTML = '<div style="color:#94a3b8;">Cargando trazas del sistema...</div>';

  try {
    const res = await authenticatedFetch('/api/v1/system/logs?limit=100');
    if (!res.ok) {
      consoleEl.innerHTML = '<div style="color:#f87171;">Fallo al consultar los logs del sistema.</div>';
      return;
    }
    const logs = await res.json();
    consoleEl.innerHTML = '';
    if (!logs || logs.length === 0) {
      consoleEl.innerHTML = '<div style="color:#94a3b8;">Sin logs registrados en el buffer en memoria.</div>';
      return;
    }
    logs.forEach(l => consoleEl.appendChild(buildLogLine(l)));
  } catch (err) {
    consoleEl.innerHTML = `<div style="color:#f87171;">Error: ${escapeHtml(err.message)}</div>`;
  }
}
