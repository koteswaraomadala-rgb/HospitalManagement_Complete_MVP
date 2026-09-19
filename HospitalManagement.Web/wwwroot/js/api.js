var ApiClient = {
    getToken() {
        return localStorage.getItem("medicare_token");
    },

    clearSession() {
        localStorage.removeItem("medicare_token");
        localStorage.removeItem("medicare_user");
    },

    async request(path, options = {}) {
        const headers = new Headers(options.headers || {});
        headers.set("Accept", "application/json");

        if (options.body && !(options.body instanceof FormData)) {
            headers.set("Content-Type", "application/json");
        }

        const token = this.getToken();
        if (token) {
            headers.set("Authorization", `Bearer ${token}`);
        }

        const response = await fetch(`${window.MediCareConfig.apiBaseUrl}/${path}`, {
            ...options,
            headers
        });

        const contentType = response.headers.get("content-type") || "";
        const data = contentType.includes("application/json")
            ? await response.json()
            : await response.text();

        if (response.status === 401) {
            this.clearSession();
            window.location.href = "/login.html";
            throw new Error("Your session has expired.");
        }

        if (!response.ok) {
            const error = new Error(data?.message || "Request failed.");
            error.status = response.status;
            error.details = data;
            throw error;
        }

        return data;
    },

    get(path) {
        return this.request(path);
    },

    post(path, body) {
        return this.request(path, {
            method: "POST",
            body: JSON.stringify(body)
        });
    },

    put(path, body) {
        return this.request(path, {
            method: "PUT",
            body: JSON.stringify(body)
        });
    },

    patch(path, body) {
        return this.request(path, {
            method: "PATCH",
            body: JSON.stringify(body)
        });
    },

    delete(path) {
        return this.request(path, { method: "DELETE" });
    }
};
