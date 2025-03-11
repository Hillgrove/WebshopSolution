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
import { ProductsPage } from "./pages/products.js";
import { createLayoutComponent } from "./layoutComponent.js";
import { CartPage } from "./pages/cart.js";


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
    { path: "/reset-password", component: ResetPasswordPage },
    { path: "/products", component: ProductsPage },
    { path: "/cart", component: CartPage }
];


// ============================
// Section: Axios Configuration
// ============================
const devBaseURL = "https://localhost:7016/api";
const prodBaseURL = "https://sikkersoftwarewebshop.azurewebsites.net/api";

if (window.location.hostname === "localhost") {
    console.log("Using development base URL:", devBaseURL);
    axios.defaults.baseURL = devBaseURL;
} else {
    console.log("Using production base URL");
    axios.defaults.baseURL = prodBaseURL;
}

axios.defaults.withCredentials = true; // Ensures cookies are sent with requests


// ============================
// Section: Global State for Login
// ============================
// export const globalState = Vue.reactive({
//     isLoggedIn: false
// });

// // Function to check login status
// export async function checkLoginStatus() {
//     try {
//         const response = await axios.get("/Users/me");
//         globalState.isLoggedIn = response.status === 200;
//     } catch {
//         globalState.isLoggedIn = false;
//     }
// }


// ============================
// Section: Axios Interceptors
// ============================
axios.interceptors.response.use(
    response => response, // Pass-through success responses
    async error => {
        if (error.response?.status === 401) {
            globalState.isLoggedIn = false;
        }
        return Promise.reject(error);
    }
);

axios.interceptors.request.use(config => {
    const csrfToken = document.cookie
        .split('; ')
        .find(row => row.startsWith('csrf-token='))
        ?.split('=')[1]; // Extract CSRF token from cookies

    if (csrfToken) {
        console.log("Adding CSRF Token to request:", csrfToken);
        config.headers["X-CSRF-Token"] = csrfToken;
    } else {
        console.warn("No CSRF Token found in cookies!");
    }

    return config;
}, error => {
    return Promise.reject(error);
});



/// ============================
// Section: FingerprintJS Initialization
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

// Call once at startup after FingerprintJS is initialized
// window.fpPromise.then(() => {
//     initializeVisitorId().then(() => checkLoginStatus());
// });


// ============================
// Section: Vue Router Initialization
// ============================
const router = VueRouter.createRouter({
    history: VueRouter.createWebHashHistory(),
    routes
});

// Ensure login status is checked after each navigation
// router.afterEach(() => {
//     checkLoginStatus();
// });


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

// Ensure login status is checked when the page loads
// window.addEventListener("load", checkLoginStatus);

console.log("Vue app initialized");
