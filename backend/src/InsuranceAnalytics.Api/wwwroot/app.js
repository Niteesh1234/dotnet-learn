const policyTypeMap = {
    1: "Auto",
    2: "Health",
    3: "Life",
    4: "Home",
    5: "Travel"
};

const claimStatusMap = {
    1: "Open",
    2: "Closed",
    3: "Pending"
};

const state = {
    token: localStorage.getItem("ia_token") || "",
    role: localStorage.getItem("ia_role") || "",
    expiresAtUtc: localStorage.getItem("ia_expiresAtUtc") || "",
    filters: {
        fromDate: "",
        toDate: "",
        region: "",
        policyType: ""
    },
    currentTab: "dashboard"
};

const elements = {
    loginSection: document.getElementById("loginSection"),
    appSection: document.getElementById("appSection"),
    dashboardSection: document.getElementById("dashboardSection"),
    policiesSection: document.getElementById("policiesSection"),
    claimsSection: document.getElementById("claimsSection"),
    loginForm: document.getElementById("loginForm"),
    filtersForm: document.getElementById("filtersForm"),
    policyForm: document.getElementById("policyForm"),
    claimForm: document.getElementById("claimForm"),
    logoutButton: document.getElementById("logoutButton"),
    refreshButton: document.getElementById("refreshButton"),
    seedFakeDataButton: document.getElementById("seedFakeDataButton"),
    reloadPoliciesButton: document.getElementById("reloadPoliciesButton"),
    reloadClaimsButton: document.getElementById("reloadClaimsButton"),
    dashboardTab: document.getElementById("dashboardTab"),
    policiesTab: document.getElementById("policiesTab"),
    claimsTab: document.getElementById("claimsTab"),
    sessionInfo: document.getElementById("sessionInfo"),
    sessionRole: document.getElementById("sessionRole"),
    sessionExpiry: document.getElementById("sessionExpiry"),
    message: document.getElementById("message"),
    regionFilter: document.getElementById("regionFilter"),
    policyTypeFilter: document.getElementById("policyTypeFilter"),
    policyTypeInput: document.getElementById("policyTypeInput"),
    claimPolicyId: document.getElementById("claimPolicyId"),
    claimStatus: document.getElementById("claimStatus"),
    kpiCards: document.getElementById("kpiCards"),
    monthlyTrendBody: document.getElementById("monthlyTrendBody"),
    regionBreakdownBody: document.getElementById("regionBreakdownBody"),
    policiesBody: document.getElementById("policiesBody"),
    claimsBody: document.getElementById("claimsBody")
};

function isAuthenticated() {
    return Boolean(state.token);
}

function isAdmin() {
    return state.role === "Admin";
}

function showMessage(text, type = "success") {
    elements.message.textContent = text;
    elements.message.className = `message ${type}`;
}

function clearMessage() {
    elements.message.textContent = "";
    elements.message.className = "message hidden";
}

function formatCurrency(value) {
    return new Intl.NumberFormat("en-US", {
        style: "currency",
        currency: "USD",
        maximumFractionDigits: 2
    }).format(Number(value || 0));
}

function formatPercent(value) {
    return `${(Number(value || 0) * 100).toFixed(2)}%`;
}

function serializeQuery(params) {
    const search = new URLSearchParams();

    Object.entries(params).forEach(([key, value]) => {
        if (value !== null && value !== undefined && value !== "") {
            search.set(key, value);
        }
    });

    const query = search.toString();
    return query ? `?${query}` : "";
}

async function apiFetch(path, options = {}) {
    const headers = {
        "Content-Type": "application/json",
        ...(options.headers || {})
    };

    if (state.token) {
        headers.Authorization = `Bearer ${state.token}`;
    }

    const response = await fetch(path, {
        ...options,
        headers
    });

    if (response.status === 401) {
        logout();
        throw new Error("Your session expired. Please sign in again.");
    }

    if (!response.ok) {
        let errorMessage = `Request failed with status ${response.status}`;

        try {
            const body = await response.json();
            errorMessage = body.message || JSON.stringify(body);
        } catch {
            const text = await response.text();
            if (text) errorMessage = text;
        }

        throw new Error(errorMessage);
    }

    return response.status === 204 ? null : response.json();
}

function saveSession(login) {
    state.token = login.token;
    state.role = login.role;
    state.expiresAtUtc = login.expiresAtUtc;

    localStorage.setItem("ia_token", login.token);
    localStorage.setItem("ia_role", login.role);
    localStorage.setItem("ia_expiresAtUtc", login.expiresAtUtc);
}

function logout(showNotice = false) {
    state.token = "";
    state.role = "";
    state.expiresAtUtc = "";

    localStorage.removeItem("ia_token");
    localStorage.removeItem("ia_role");
    localStorage.removeItem("ia_expiresAtUtc");

    renderShell();

    if (showNotice) {
        showMessage("You have been logged out.", "success");
    }
}

function renderShell() {
    const authenticated = isAuthenticated();
    const admin = isAdmin();

    elements.loginSection.classList.toggle("hidden", authenticated);
    elements.appSection.classList.toggle("hidden", !authenticated);
    elements.sessionInfo.classList.toggle("hidden", !authenticated);
    elements.sessionRole.textContent = state.role || "Guest";
    elements.sessionExpiry.textContent = state.expiresAtUtc
        ? `Expires ${new Date(state.expiresAtUtc).toLocaleString()}`
        : "";

    // Show/hide admin-only forms
    document.querySelectorAll(".admin-only").forEach(el => {
        el.classList.toggle("hidden", !admin);
    });

    // Show current tab
    elements.dashboardSection.classList.toggle("hidden", state.currentTab !== "dashboard");
    elements.policiesSection.classList.toggle("hidden", state.currentTab !== "policies");
    elements.claimsSection.classList.toggle("hidden", state.currentTab !== "claims");

    // Update tab buttons
    elements.dashboardTab.classList.toggle("active", state.currentTab === "dashboard");
    elements.dashboardTab.classList.toggle("secondary", state.currentTab !== "dashboard");
    elements.policiesTab.classList.toggle("active", state.currentTab === "policies");
    elements.policiesTab.classList.toggle("secondary", state.currentTab !== "policies");
    elements.claimsTab.classList.toggle("active", state.currentTab === "claims");
    elements.claimsTab.classList.toggle("secondary", state.currentTab !== "claims");
}

function renderSelectOptions(selectElement, items, mapper, placeholder) {
    const values = [];
    if (placeholder) values.push(`<option value="">${placeholder}</option>`);

    items.forEach((item) => {
        const { value, label } = mapper(item);
        values.push(`<option value="${value}">${label}</option>`);
    });

    selectElement.innerHTML = values.join("");
}

function renderKpis(data) {
    const cards = [
        { label: "Total Premium", value: formatCurrency(data.kpis.totalPremium) },
        { label: "Total Claims", value: formatCurrency(data.kpis.totalClaims) },
        { label: "Loss Ratio", value: formatPercent(data.kpis.lossRatio) },
        { label: "Policy Count", value: `${data.kpis.policyCount}` },
        { label: "Claim Frequency", value: formatPercent(data.kpis.claimFrequency) }
    ];

    elements.kpiCards.innerHTML = cards.map((card) => `
        <article class="kpi-card">
            <div class="label">${card.label}</div>
            <div class="value">${card.value}</div>
        </article>
    `).join("");
}

function renderTableRows(target, rows, emptyText, template, colspan = 4) {
    if (!rows.length) {
        target.innerHTML = `<tr><td colspan="${colspan}">${emptyText}</td></tr>`;
        return;
    }

    target.innerHTML = rows.map(template).join("");
}

function renderPolicies(policies) {
    renderTableRows(
        elements.policiesBody,
        policies,
        "No policies found.",
        (policy) => `
            <tr>
                <td>${policy.policyId}</td>
                <td>${policy.customerName}</td>
                <td>${policyTypeMap[policy.policyType] || policy.policyType}</td>
                <td>${formatCurrency(policy.premiumAmount)}</td>
                <td>${policy.startDate}</td>
                <td>${policy.endDate}</td>
                <td>${policy.region}</td>
                <td>${policy.claimCount}</td>
            </tr>
        `,
        8
    );
}

function renderClaims(claims) {
    renderTableRows(
        elements.claimsBody,
        claims,
        "No claims found.",
        (claim) => `
            <tr>
                <td>${claim.claimId}</td>
                <td>${claim.policyId}</td>
                <td>${claim.customerName}</td>
                <td>${formatCurrency(claim.claimAmount)}</td>
                <td>${claim.claimDate}</td>
                <td>${claimStatusMap[claim.status] || claim.status}</td>
            </tr>
        `,
        6
    );
}

function renderDashboard(data) {
    renderKpis(data);

    renderTableRows(
        elements.monthlyTrendBody,
        data.monthlyTrends || [],
        "No trend data available for the selected filters.",
        (item) => `
            <tr>
                <td>${item.month}</td>
                <td>${formatCurrency(item.premium)}</td>
                <td>${formatCurrency(item.claims)}</td>
                <td>${formatPercent(item.lossRatio)}</td>
            </tr>
        `,
        4
    );

    renderTableRows(
        elements.regionBreakdownBody,
        data.regionBreakdown || [],
        "No regional data available for the selected filters.",
        (item) => `
            <tr>
                <td>${item.region}</td>
                <td>${formatCurrency(item.premium)}</td>
                <td>${formatCurrency(item.claims)}</td>
            </tr>
        `,
        3
    );

    renderPieChart(data.monthlyTrends || []);
}

function renderPieChart(monthlyTrends) {
    const container = document.getElementById("pieChart");
    if (!container) return;

    if (!monthlyTrends.length) {
        container.innerHTML = '<p class="muted">No trend data available.</p>';
        return;
    }

    // Sum totals for the pie (premium vs claims)
    const totalPremium = monthlyTrends.reduce((sum, m) => sum + (Number(m.premium) || 0), 0);
    const totalClaims = monthlyTrends.reduce((sum, m) => sum + (Number(m.claims) || 0), 0);
    const grandTotal = totalPremium + totalClaims || 1;

    const slices = [
        { label: "Premium", value: totalPremium, color: "#2563eb" },
        { label: "Claims", value: totalClaims, color: "#f97316" }
    ];

    let cumulative = 0;
    const arcs = slices.map(({ value, color }) => {
        const start = cumulative / grandTotal;
        cumulative += value;
        const end = cumulative / grandTotal;

        const largeArc = end - start > 0.5 ? 1 : 0;
        const startAngle = start * 2 * Math.PI - Math.PI / 2;
        const endAngle = end * 2 * Math.PI - Math.PI / 2;
        const r = 100;
        const cx = 110;
        const cy = 110;
        const x1 = cx + r * Math.cos(startAngle);
        const y1 = cy + r * Math.sin(startAngle);
        const x2 = cx + r * Math.cos(endAngle);
        const y2 = cy + r * Math.sin(endAngle);
        const d = `M ${cx} ${cy} L ${x1} ${y1} A ${r} ${r} 0 ${largeArc} 1 ${x2} ${y2} Z`;

        return `<path d="${d}" fill="${color}" />`;
    }).join("");

    const legend = slices.map(({ label, value, color }) => {
        const pct = ((value / grandTotal) * 100).toFixed(1);
        return `<div class="pie-legend-item"><span class="swatch" style="background:${color}"></span>${label}: ${formatCurrency(value)} (${pct}%)</div>`;
    }).join("");

    container.innerHTML = `
        <svg class="pie" viewBox="0 0 220 220" role="img" aria-label="Premium vs Claims pie chart">
            ${arcs}
        </svg>
        <div class="pie-legend">
            ${legend}
            <div class="pie-summary">Total: ${formatCurrency(grandTotal)}</div>
        </div>
    `;
}

async function loadFilters() {
    const data = await apiFetch("/api/filters");

    renderSelectOptions(
        elements.regionFilter,
        data.regions || [],
        (item) => ({ value: item, label: item }),
        "All regions"
    );

    renderSelectOptions(
        elements.policyTypeFilter,
        (data.policyTypes || []).map((value) => Number(value)),
        (value) => ({ value, label: policyTypeMap[value] || `Type ${value}` }),
        "All policy types"
    );

    // Populate policy type select for create form
    renderSelectOptions(
        elements.policyTypeInput,
        Object.keys(policyTypeMap).map(key => Number(key)),
        (value) => ({ value, label: policyTypeMap[value] }),
        "Select policy type"
    );

    // Populate claim status select
    renderSelectOptions(
        elements.claimStatus,
        Object.keys(claimStatusMap).map(key => Number(key)),
        (value) => ({ value, label: claimStatusMap[value] }),
        "Select status"
    );
}

function buildFilterQuery() {
    return serializeQuery({
        fromDate: state.filters.fromDate,
        toDate: state.filters.toDate,
        region: state.filters.region,
        policyType: state.filters.policyType
    });
}

async function loadPolicies() {
    const query = buildFilterQuery();
    const data = await apiFetch(`/api/policies${query}`);
    renderPolicies(data || []);
    return data || [];
}

async function loadClaims() {
    const query = buildFilterQuery();
    const data = await apiFetch(`/api/claims${query}`);
    renderClaims(data || []);
    return data || [];
}

async function seedFakeData() {
    try {
        clearMessage();
        await apiFetch('/api/policies/seed-fake?policyCount=20&maxClaimsPerPolicy=3', { method: 'POST' });
        await switchToTab(state.currentTab);
        showMessage('Random fake data seeded successfully.', 'success');
    } catch (error) {
        showMessage(error.message || 'Failed to seed fake data.', 'error');
    }
}

async function loadDashboard() {
    const query = serializeQuery({
        fromDate: state.filters.fromDate,
        toDate: state.filters.toDate,
        region: state.filters.region,
        policyType: state.filters.policyType
    });

    const data = await apiFetch(`/api/kpi${query}`);
    renderDashboard(data);
}

async function initializeDashboard() {
    clearMessage();
    await loadFilters();
    await loadDashboard();
}

async function switchToTab(tab) {
    state.currentTab = tab;
    renderShell();
    clearMessage();

    try {
        if (tab === "dashboard") {
            await initializeDashboard();
        } else if (tab === "policies") {
            const policies = await loadPolicies();
            populatePolicySelect(policies);
        } else if (tab === "claims") {
            const claims = await loadClaims();
            // Claims don't need select population for now
        }
    } catch (error) {
        showMessage(error.message || `Failed to load ${tab}.`, "error");
    }
}

function populatePolicySelect(policies) {
    renderSelectOptions(
        elements.claimPolicyId,
        policies,
        (policy) => ({ value: policy.policyId, label: `${policy.policyId} - ${policy.customerName}` }),
        "Select a policy"
    );
}

async function handleLogin(event) {
    event.preventDefault();
    clearMessage();

    try {
        const data = await apiFetch("/api/auth/login", {
            method: "POST",
            body: JSON.stringify({
                username: document.getElementById("username").value.trim(),
                password: document.getElementById("password").value
            })
        });

        saveSession(data);
        renderShell();
        await initializeDashboard();
        showMessage(`Signed in successfully as ${data.role}.`, "success");
    } catch (error) {
        showMessage(error.message || "Unable to sign in.", "error");
    }
}

async function handleFiltersSubmit(event) {
    event.preventDefault();
    clearMessage();

    state.filters = {
        fromDate: document.getElementById("fromDate").value,
        toDate: document.getElementById("toDate").value,
        region: elements.regionFilter.value,
        policyType: elements.policyTypeFilter.value
    };

    try {
        await loadDashboard();
        await loadPolicies();
        await loadClaims();
        showMessage("Dashboard, policies, and claims updated for selected range.", "success");
    } catch (error) {
        showMessage(error.message || "Failed to apply filters.", "error");
    }
}

async function handlePolicySubmit(event) {
    event.preventDefault();
    clearMessage();

    try {
        const data = await apiFetch("/api/policies", {
            method: "POST",
            body: JSON.stringify({
                customerName: document.getElementById("policyCustomerName").value.trim(),
                policyType: Number(document.getElementById("policyTypeInput").value),
                premiumAmount: Number(document.getElementById("policyPremiumAmount").value),
                startDate: document.getElementById("policyStartDate").value,
                endDate: document.getElementById("policyEndDate").value,
                region: document.getElementById("policyRegion").value.trim()
            })
        });

        showMessage("Policy created successfully.", "success");
        event.target.reset();
        await loadPolicies();
    } catch (error) {
        showMessage(error.message || "Failed to create policy.", "error");
    }
}

async function handleClaimSubmit(event) {
    event.preventDefault();
    clearMessage();

    try {
        const data = await apiFetch("/api/claims", {
            method: "POST",
            body: JSON.stringify({
                policyId: Number(document.getElementById("claimPolicyId").value),
                claimAmount: Number(document.getElementById("claimAmount").value),
                claimDate: document.getElementById("claimDate").value,
                status: Number(document.getElementById("claimStatus").value)
            })
        });

        showMessage("Claim created successfully.", "success");
        event.target.reset();
        await loadClaims();
    } catch (error) {
        showMessage(error.message || "Failed to create claim.", "error");
    }
}

elements.loginForm.addEventListener("submit", handleLogin);
elements.filtersForm.addEventListener("submit", handleFiltersSubmit);
elements.policyForm.addEventListener("submit", handlePolicySubmit);
elements.claimForm.addEventListener("submit", handleClaimSubmit);
elements.logoutButton.addEventListener("click", () => logout(true));
elements.refreshButton.addEventListener("click", async () => {
    try {
        await initializeDashboard();
        showMessage("Dashboard refreshed.", "success");
    } catch (error) {
        showMessage(error.message || "Failed to refresh dashboard.", "error");
    }
});
if (elements.seedFakeDataButton) {
    elements.seedFakeDataButton.addEventListener("click", async () => {
        await seedFakeData();
    });
}
elements.reloadPoliciesButton.addEventListener("click", async () => {
    try {
        await loadPolicies();
        showMessage("Policies reloaded.", "success");
    } catch (error) {
        showMessage(error.message || "Failed to reload policies.", "error");
    }
});
elements.reloadClaimsButton.addEventListener("click", async () => {
    try {
        await loadClaims();
        showMessage("Claims reloaded.", "success");
    } catch (error) {
        showMessage(error.message || "Failed to reload claims.", "error");
    }
});
elements.dashboardTab.addEventListener("click", () => switchToTab("dashboard"));
elements.policiesTab.addEventListener("click", () => switchToTab("policies"));
elements.claimsTab.addEventListener("click", () => switchToTab("claims"));

renderShell();

if (isAuthenticated()) {
    initializeDashboard().catch((error) => {
        showMessage(error.message || "Failed to restore session.", "error");
    });
}