function setPageContent(html) {
    const container = document.getElementById("page-content");
    if (container) {
        container.innerHTML = html;
    }
}

function showToast(message, type = "success") {
    const container = document.getElementById("toast-container");
    if (!container) return;

    const toast = document.createElement("div");
    toast.className = `toast ${type}`;
    toast.textContent = message;
    container.appendChild(toast);

    setTimeout(() => toast.remove(), 3500);
}

function clearFieldErrors(form) {
    form.querySelectorAll(".field-error").forEach(element => {
        element.textContent = "";
    });

    form.querySelectorAll(".input-error").forEach(input => {
        input.classList.remove("input-error");
    });

    const summary = form.querySelector(".form-error");
    if (summary) summary.textContent = "";
}

function showFieldErrors(form, errors) {
    if (!errors) return;

    Object.entries(errors).forEach(([field, messages]) => {
        const normalized = field.split(".").pop();
        const input = form.querySelector(`[name="${normalized}"], #${normalized}`);
        const errorElement = form.querySelector(`[data-error-for="${normalized}"]`);

        if (input) input.classList.add("input-error");
        if (errorElement) errorElement.textContent = Array.isArray(messages) ? messages[0] : messages;
    });
}

function validateRequired(form, fields) {
    let valid = true;

    fields.forEach(fieldName => {
        const input = form.querySelector(`[name="${fieldName}"]`);
        const errorElement = form.querySelector(`[data-error-for="${fieldName}"]`);

        if (!input || input.value.trim()) return;

        input.classList.add("input-error");
        if (errorElement) errorElement.textContent = "This field is required.";
        valid = false;
    });

    return valid;
}

function setButtonBusy(button, busy, text = "Saving...") {
    if (!button) return;

    if (busy) {
        button.dataset.originalText = button.innerHTML;
        button.disabled = true;
        button.innerHTML = `<span class="spinner"></span>${text}`;
    } else {
        button.disabled = false;
        button.innerHTML = button.dataset.originalText || "Save";
    }
}

function formatDate(value) {
    if (!value) return "-";
    return new Date(value).toLocaleDateString("en-IN", {
        day: "2-digit",
        month: "short",
        year: "numeric"
    });
}

function statusBadge(status) {
    const value = String(status || "").toLowerCase().replaceAll(" ", "-");
    let className = "badge-confirmed";

    if (value.includes("waiting")) className = "badge-waiting";
    if (value.includes("consultation")) className = "badge-consultation";
    if (value.includes("completed")) className = "badge-completed";
    if (value.includes("cancelled")) className = "badge-cancelled";
    if (value.includes("no-show")) className = "badge-no-show";
    if (value.includes("active")) className = "badge-active";

    return `<span class="badge ${className}">${escapeHtml(status || "Unknown")}</span>`;
}

function escapeHtml(value) {
    return String(value ?? "")
        .replaceAll("&", "&amp;")
        .replaceAll("<", "&lt;")
        .replaceAll(">", "&gt;")
        .replaceAll('"', "&quot;")
        .replaceAll("'", "&#039;");
}
