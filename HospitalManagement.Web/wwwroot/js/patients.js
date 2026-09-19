let editingPatientId = 0;

const patientFormHtml = `
    <form id="patient-form" class="card panel-body" novalidate>
        <div class="form-grid">
            <div class="field-group"><label for="firstName">First name *</label><input id="firstName" name="firstName" maxlength="50"><div class="field-error" data-error-for="firstName"></div></div>
            <div class="field-group"><label for="lastName">Last name *</label><input id="lastName" name="lastName" maxlength="50"><div class="field-error" data-error-for="lastName"></div></div>
            <div class="field-group"><label for="gender">Gender *</label><select id="gender" name="gender"><option value="">Select gender</option><option>Male</option><option>Female</option><option>Other</option></select><div class="field-error" data-error-for="gender"></div></div>
            <div class="field-group"><label for="dateOfBirth">Date of birth</label><input id="dateOfBirth" name="dateOfBirth" type="date"><div class="field-error" data-error-for="dateOfBirth"></div></div>
            <div class="field-group"><label for="phone">Phone *</label><input id="phone" name="phone" type="tel"><div class="field-error" data-error-for="phone"></div></div>
            <div class="field-group"><label for="email">Email</label><input id="email" name="email" type="email"><div class="field-error" data-error-for="email"></div></div>
            <div class="field-group full"><label for="address">Address</label><textarea id="address" name="address" maxlength="300"></textarea><div class="field-error" data-error-for="address"></div></div>
            <div class="field-group"><label for="status">Status *</label><select id="status" name="status"><option>Active</option><option>Inactive</option></select><div class="field-error" data-error-for="status"></div></div>
        </div>
        <div class="form-error"></div>
        <div class="form-actions"><button type="button" class="btn btn-secondary" id="cancel-patient">Cancel</button><button type="submit" class="btn btn-primary">Save patient</button></div>
    </form>`;

document.addEventListener("DOMContentLoaded", () => {
    setPageContent(`
        <div class="page-header"><div><h1>Patients</h1><p>Manage patient records without leaving the page.</p></div><button id="new-patient" class="btn btn-primary">Add patient</button></div>
        <div id="patient-form-container" class="hidden"></div>
        <div class="filter-bar"><input id="patient-search" placeholder="Search patients..."><button id="patient-search-button" class="btn btn-secondary">Search</button></div>
        <div class="panel"><div id="patient-table" class="table-wrap"></div></div>
    `);

    document.getElementById("new-patient").addEventListener("click", openPatientForm);
    document.getElementById("patient-search-button").addEventListener("click", loadPatients);
    document.getElementById("patient-search").addEventListener("keydown", event => { if (event.key === "Enter") loadPatients(); });
    document.getElementById("patient-form-container").addEventListener("submit", handlePatientSubmit);
    document.getElementById("patient-form-container").addEventListener("click", event => {
        if (event.target.id === "cancel-patient") closePatientForm();
        if (event.target.dataset.editId) editPatient(Number(event.target.dataset.editId));
        if (event.target.dataset.deleteId) deletePatient(Number(event.target.dataset.deleteId));
    });

    loadPatients();
});

function openPatientForm(patient = null) {
    editingPatientId = patient?.id || 0;
    const container = document.getElementById("patient-form-container");
    container.classList.remove("hidden");
    container.innerHTML = patientFormHtml;
    if (patient) fillPatientForm(patient);
    container.scrollIntoView({ behavior: "smooth", block: "start" });
}

function closePatientForm() {
    editingPatientId = 0;
    const container = document.getElementById("patient-form-container");
    container.classList.add("hidden");
    container.innerHTML = "";
}

function fillPatientForm(patient) {
    const form = document.getElementById("patient-form");
    form.firstName.value = patient.firstName || "";
    form.lastName.value = patient.lastName || "";
    form.gender.value = patient.gender || "";
    form.dateOfBirth.value = patient.dateOfBirth ? patient.dateOfBirth.slice(0, 10) : "";
    form.phone.value = patient.phone || "";
    form.email.value = patient.email || "";
    form.address.value = patient.address || "";
    form.status.value = patient.status || "Active";
}

async function handlePatientSubmit(event) {
    event.preventDefault();
    event.stopPropagation();

    const form = event.target;
    clearFieldErrors(form);

    if (!validateRequired(form, ["firstName", "lastName", "gender", "phone", "status"])) return;

    const button = form.querySelector("button[type=submit]");
    setButtonBusy(button, true);

    const payload = {
        firstName: form.firstName.value.trim(),
        lastName: form.lastName.value.trim(),
        gender: form.gender.value,
        dateOfBirth: form.dateOfBirth.value || new Date().toISOString().slice(0, 10),
        phone: form.phone.value.trim(),
        email: form.email.value.trim(),
        address: form.address.value.trim(),
        status: form.status.value
    };

    try {
        if (editingPatientId) {
            await ApiClient.put(`patients/${editingPatientId}`, payload);
            showToast("Patient updated successfully.");
        } else {
            await ApiClient.post("patients", payload);
            showToast("Patient added successfully.");
        }

        closePatientForm();
        await loadPatients();
    } catch (error) {
        if (error.details?.errors) showFieldErrors(form, error.details.errors);
        form.querySelector(".form-error").textContent = error.message;
    } finally {
        setButtonBusy(button, false);
    }
}

async function loadPatients() {
    try {
        const search = document.getElementById("patient-search")?.value.trim() || "";
        const patients = await ApiClient.get(`patients?search=${encodeURIComponent(search)}`);
        document.getElementById("patient-table").innerHTML = patients.length
            ? `<table><thead><tr><th>Number</th><th>Patient</th><th>Gender</th><th>Phone</th><th>Status</th><th>Actions</th></tr></thead><tbody>${patients.map(patient => `<tr><td>${escapeHtml(patient.patientNumber)}</td><td><strong>${escapeHtml(patient.firstName)} ${escapeHtml(patient.lastName)}</strong></td><td>${escapeHtml(patient.gender)}</td><td>${escapeHtml(patient.phone)}</td><td>${statusBadge(patient.status)}</td><td><button class="btn btn-secondary" data-edit-id="${patient.id}">Edit</button> <button class="btn btn-danger" data-delete-id="${patient.id}">Delete</button></td></tr>`).join("")}</tbody></table>`
            : `<div class="empty-state"><strong>No patients found</strong>Add your first patient to get started.</div>`;
    } catch (error) {
        showToast(error.message, "error");
    }
}

async function editPatient(id) {
    try { openPatientForm(await ApiClient.get(`patients/${id}`)); }
    catch (error) { showToast(error.message, "error"); }
}

async function deletePatient(id) {
    if (!confirm("Delete this patient?")) return;
    try { await ApiClient.delete(`patients/${id}`); showToast("Patient deleted successfully."); await loadPatients(); }
    catch (error) { showToast(error.message, "error"); }
}
