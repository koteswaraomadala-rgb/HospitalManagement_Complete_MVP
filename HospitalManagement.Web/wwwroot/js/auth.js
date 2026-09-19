document.addEventListener("DOMContentLoaded", () => {
    const form = document.getElementById("login-form");
    if (!form) return;

    if (ApiClient.getToken()) {
        window.location.href = "/dashboard.html";
        return;
    }
    

    form.addEventListener("submit", async event => {
        event.preventDefault();
        clearFieldErrors(form);

        const valid = validateRequired(form, ["username", "password"]);
        if (!valid) return;

        const button = form.querySelector("button[type=submit]");
        setButtonBusy(button, true, "Signing in...");

        try {
            const username = form.username.value.trim();
            const password = form.password.value;
            const result = await ApiClient.post("auth/login", { username, password });

            localStorage.setItem("medicare_token", result.token);
            localStorage.setItem("medicare_user", JSON.stringify({
                fullName: result.fullName,
                role: result.role,
                username
            }));

            window.location.href = "/dashboard.html";
        } catch (error) {
            const summary = document.getElementById("login-form-error");
            if (error.details?.errors) {
                showFieldErrors(form, error.details.errors);
            }
            if (summary) summary.textContent = error.message || "Unable to sign in.";
        } finally {
            setButtonBusy(button, false);
        }
    });

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
});
