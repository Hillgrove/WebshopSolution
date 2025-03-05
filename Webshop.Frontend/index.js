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
import { createLayoutComponent } from "./layoutComponent.js"; // Import function instead of component

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

axios.defaults.baseURL = "https://localhost:7016/api";
// axios.defaults.baseURL = "https://sikkersoftwarewebshop.azurewebsites.net/api";

// Enable sending cookies with requests
axios.defaults.withCredentials = true;


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
        console.warn("Login status check failed:", error.response?.status, error.response?.data);
        globalState.isLoggedIn = false;
    }
}

// Run login check on startup
checkLoginStatus();


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
// Section: Visitor ID Initialization
// ============================

// Check if visitorId exists in local storage
let visitorId;
try {
    visitorId = localStorage.getItem('visitorId');
} catch (error) {
    console.warn("LocalStorage access denied", error);
}

if (!visitorId) {
    // Initialize FingerprintJS and store visitorId in local storage
    window.fpPromise = window.FingerprintJS.load().then(fp => {
        return fp.get();
    }).then(result => {
        visitorId = result.visitorId;
        localStorage.setItem('visitorId', visitorId);
    });
}

// ============================
// Section: Vue Router Initialization
// ============================

// Initialize Vue Router
const router = VueRouter.createRouter({
    history: VueRouter.createWebHashHistory(),
    routes
});

// ============================
// Section: Vue App Initialization
// ============================

// Initialize Vue App
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
