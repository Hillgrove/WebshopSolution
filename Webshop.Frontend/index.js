// ============================
// Section: Import Statements
// ============================
import { HomePage } from "./pages/home.js";
import { LoginPage } from "./pages/login.js";
import { RegisterPage } from "./pages/register.js";
import { AboutPage } from "./pages/about.js";
import { ChangePasswordPage } from "./pages/changePassword.js";
import { ForgotPasswordPage } from "./pages/forgotPassword.js";
import { ResetPasswordPage } from "./pages/resetPassword.js";
import { createLayoutComponent } from "./layoutComponent.js";


// ============================
// Section: Route Definitions
// ============================
const routes = [
    { path: "/", component: HomePage },
    { path: "/login", component: LoginPage },
    { path: "/register", component: RegisterPage },
    { path: "/about", component: AboutPage },
    { path: "/change-password", component: ChangePasswordPage },
    { path: "/forgot-password", component: ForgotPasswordPage },
    { path: "/reset-password", component: ResetPasswordPage }
];


// ============================
// Section: Axios Configuration
// ============================
// axios.defaults.baseURL = "https://localhost:7016/api";
// axios.defaults.baseURL = "https://api.webshop.hillgrove.dk/api"; // Requires Custom Domains setup in Azure for SSL binding
axios.defaults.baseURL = "https://sikkersoftwarewebshop.azurewebsites.net/api";

axios.defaults.withCredentials = true; // Ensure cookies (session + CSRF) are sent

// Interceptor to attach CSRF token from cookies to headers
axios.interceptors.request.use((config) => {
    const csrfToken = document.cookie.split('; ')
                                     .find(row => row.startsWith('XSRF-TOKEN='))
                                     ?.split('=')[1];

    if (csrfToken) {
        config.headers['X-XSRF-TOKEN'] = csrfToken;
    }

    return config;
});


// ============================
// Section: Global State for Login
// ============================
export const globalState = Vue.reactive({
    isLoggedIn: false
});

// Function to check login status
export async function checkLoginStatus() {
    try {
        const response = await axios.get("/Users/me");
        globalState.isLoggedIn = response.status === 200;
    } catch (error) {
        if (error.response?.status === 401) {
            globalState.isLoggedIn = false; // User is not logged in, no need to warn
            return;
        }
        console.warn("Login status check failed:", error.response?.status, error.response?.data);
        globalState.isLoggedIn = false;
    }
}


// ============================
// Section: Initialize FingerprintJS
// ============================
window.fpPromise = FingerprintJS.load();


// ============================
// Section: Visitor ID Initialization
// ============================
export async function initializeVisitorId() {
    let visitorId = localStorage.getItem("visitorId");
    if (!visitorId) {
        console.log("Generating new visitorId...");
        const result = await window.fpPromise.then(fp => fp.get());
        visitorId = result.visitorId;
        localStorage.setItem("visitorId", visitorId);
    }
}

// Call this at startup after FingerprintJS is initialized
window.fpPromise.then(() => {
    initializeVisitorId().then(() => checkLoginStatus());
});


// ============================
// Section: Vue Router Initialization
// ============================
const router = VueRouter.createRouter({
    history: VueRouter.createWebHashHistory(),
    routes
});

// ============================
// Section: Vue App Initialization
// ============================
const app = Vue.createApp({
    template: `<layout-component></layout-component>`,
    setup() {
        return { globalState };
    }
});

// Dynamically create and register the layout component
app.component("layout-component", createLayoutComponent(globalState));

app.use(router);
app.mount("#app");

console.log("Vue app initialized");
