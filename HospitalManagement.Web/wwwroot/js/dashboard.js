document.addEventListener("DOMContentLoaded", async () => {
    setPageContent(`
        <div class="page-header">
            <div><h1>Dashboard</h1><p>Overview of today's hospital activity.</p></div>
            <div class="page-actions"><a class="btn btn-primary" href="/appointments.html">New appointment</a></div>
        </div>
        <section id="dashboard-stats" class="stats-grid"></section>
        <section class="content-grid">
            <div class="panel">
                <div class="panel-head"><h2>Today's appointments</h2><a href="/appointments.html" class="muted">View all</a></div>
                <div id="dashboard-appointments" class="table-wrap"></div>
            </div>
            <div class="panel">
                <div class="panel-head"><h2>Patient flow</h2><a href="/patient-flow.html" class="muted">Open queue</a></div>
                <div id="dashboard-queue" class="panel-body"></div>
            </div>
        </section>
    `);

    await loadDashboard();
});

async function loadDashboard() {
    try {
        const [patients, doctors, appointments, prescriptions] = await Promise.all([
            ApiClient.get("patients"),
            ApiClient.get("doctors"),
            ApiClient.get(`appointments?date=${encodeURIComponent(new Date().toISOString().slice(0, 10))}`),
            ApiClient.get("prescriptions")
        ]);

        const today = appointments || [];
        const pending = Math.max(0, today.filter(item => !["Completed", "Cancelled", "No Show"].includes(item.status)).length - (prescriptions || []).length);

        document.getElementById("dashboard-stats").innerHTML = `
            ${statCard("Patients", patients.length, "Registered patients")}
            ${statCard("Doctors", doctors.length, "Active clinical staff")}
            ${statCard("Appointments", today.length, "Scheduled today")}
            ${statCard("Pending", pending, "Open clinical items")}
        `;

        document.getElementById("dashboard-appointments").innerHTML = today.length
            ? `<table><thead><tr><th>Time</th><th>Patient</th><th>Doctor</th><th>Status</th></tr></thead><tbody>
                ${today.slice(0, 8).map(item => `<tr><td>${escapeHtml(item.appointmentTime)}</td><td>${escapeHtml(item.patientName)}</td><td>${escapeHtml(item.doctorName)}</td><td>${statusBadge(item.status)}</td></tr>`).join("")}
              </tbody></table>`
            : `<div class="empty-state"><strong>No appointments today</strong>There are no appointments scheduled for today.</div>`;

        const queue = today.filter(item => !["Completed", "Cancelled", "No Show"].includes(item.status)).sort((a,b) => a.appointmentTime.localeCompare(b.appointmentTime));
        document.getElementById("dashboard-queue").innerHTML = queue.length
            ? `<div class="queue-list">${queue.slice(0, 5).map((item, index) => queueItem(item, index + 1)).join("")}</div>`
            : `<div class="empty-state"><strong>Queue is clear</strong>No waiting patients right now.</div>`;
    } catch (error) {
        showToast(error.message, "error");
    }
}

function statCard(label, value, foot) {
    return `<div class="card stat-card"><div class="stat-label">${label}</div><div class="stat-value">${value}</div><div class="stat-foot">${foot}</div></div>`;
}

function queueItem(item, position) {
    return `<div class="queue-item"><div class="queue-number">${position}</div><div><div class="queue-name">${escapeHtml(item.patientName)}</div><div class="queue-meta">${escapeHtml(item.doctorName)} · ${escapeHtml(item.appointmentTime)}</div></div>${statusBadge(item.status)}</div>`;
}
