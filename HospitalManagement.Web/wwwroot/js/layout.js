const navigation = [
    { key: "dashboard", label: "Dashboard", href: "/dashboard.html", icon: "▦" },
    { key: "patients", label: "Patients", href: "/patients.html", icon: "♙" },
    { key: "doctors", label: "Doctors", href: "/doctors.html", icon: "✚" },
    { key: "appointments", label: "Appointments", href: "/appointments.html", icon: "◷" },
    { key: "prescriptions", label: "Prescriptions", href: "/prescriptions.html", icon: "▤" },
    { key: "patient-flow", label: "Patient Flow", href: "/patient-flow.html", icon: "⇢" },
    { key: "reports", label: "Reports", href: "/reports.html", icon: "▥" },
    { key: "settings", label: "Settings", href: "/settings.html", icon: "⚙" }
];

function currentPage() {
    return document.body.dataset.page || "dashboard";
}

function getStoredUser() {
    try {
        return JSON.parse(localStorage.getItem("medicare_user") || "null");
    } catch {
        return null;
    }
}

function renderShell() {
    const shell = document.getElementById("app-shell");
    const page = currentPage();
    const user = getStoredUser() || { fullName: "Administrator", role: "Administrator" };

    const navHtml = navigation.map(item => `
        <a class="nav-item ${item.key === page ? "active" : ""}" href="${item.href}">
            <span class="nav-icon">${item.icon}</span>
            <span>${item.label}</span>
        </a>
    `).join("");

    shell.innerHTML = `
        <aside class="sidebar">
            <div class="sidebar-brand">
                <div class="brand-mark">M</div>
                <div>
                    <div class="brand-title">MediCare</div>
                    <div class="brand-subtitle">Hospital Management</div>
                </div>
            </div>
            <div class="nav-section-title">Workspace</div>
            <nav class="sidebar-nav">${navHtml}</nav>
            <div class="sidebar-footer">
                <div class="sidebar-user">${escapeHtml(user.fullName || "Administrator")}</div>
                <div class="sidebar-role">${escapeHtml(user.role || "Administrator")}</div>
            </div>
        </aside>
        <header class="topbar">
            <div>
                <div class="topbar-title">${pageTitle(page)}</div>
                <div class="topbar-subtitle">Hospital operations workspace</div>
            </div>
            <div class="topbar-right">
                <input id="global-search" class="global-search" type="search" placeholder="Search patients, doctors...">
                <div class="topbar-user">
                    <div class="topbar-avatar">${initials(user.fullName || "Administrator")}</div>
                    <div class="topbar-user-info">
                        <div class="topbar-user-name">${escapeHtml(user.fullName || "Administrator")}</div>
                        <div class="topbar-user-role">${escapeHtml(user.role || "Administrator")}</div>
                    </div>
                </div>
                <button id="logout-button" class="btn btn-secondary" type="button">Logout</button>
            </div>
        </header>
    `;

    document.getElementById("logout-button")?.addEventListener("click", () => {
        ApiClient.clearSession();
        window.location.href = "/login.html";
    });
}

function pageTitle(page) {
    const item = navigation.find(item => item.key === page);
    return item?.label || "Dashboard";
}

function initials(name) {
    return name.split(" ").filter(Boolean).slice(0, 2).map(part => part[0]).join("").toUpperCase() || "M";
}

function escapeHtml(value) {
    return String(value ?? "")
        .replaceAll("&", "&amp;")
        .replaceAll("<", "&lt;")
        .replaceAll(">", "&gt;")
        .replaceAll('"', "&quot;")
        .replaceAll("'", "&#039;");
}

document.addEventListener("DOMContentLoaded", () => {
    if (!ApiClient.getToken()) {
        window.location.href = "/login.html";
        return;
    }

    renderShell();
});
