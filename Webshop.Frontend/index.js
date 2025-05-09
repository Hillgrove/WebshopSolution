// ============================
// Section: Import Statements
// ============================
import { HomePage } from "./pages/home.js";
import { LoginPage } from "./pages/login.js";
import { RegisterPage } from "./pages/register.js";
import { ChangePasswordPage } from "./pages/changePassword.js";
import { ForgotPasswordPage } from "./pages/forgotPassword.js";
import { ResetPasswordPage } from "./pages/resetPassword.js";
import { ProductsPage } from "./pages/products.js";
import { createLayoutComponent } from "./layoutComponent.js";
import { CartPage } from "./pages/cart.js";
import { OrderConfirmationPage } from "./pages/orderConfirmation.js";
import { OrderHistoryPage } from "./pages/orderHistory.js";
import { AdminUsersPage } from "./pages/AdminUsers.js";


// ============================
// Section: Route Definitions
// ============================
const routes = [
    { path: "/", component: HomePage },
    { path: "/login", component: LoginPage },
    { path: "/register", component: RegisterPage },
    { path: "/change-password", component: ChangePasswordPage },
    { path: "/forgot-password", component: ForgotPasswordPage },
    { path: "/reset-password", component: ResetPasswordPage },
    { path: "/products", component: ProductsPage },
    { path: "/cart", component: CartPage },
    { path: "/order-confirmation", component: OrderConfirmationPage },
    { path: "/orders", component: OrderHistoryPage },
    { path: "/admin-users", component: AdminUsersPage }
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
// Section: Global State Management
// ============================
window.isLoggedIn = false;
window.userRole = "Guest";

export function updateLoginState(status, role = "Guest") {
    window.isLoggedIn = status;
    window.userRole = role;
    window.dispatchEvent(new CustomEvent("auth-changed", { detail: status }));
    window.dispatchEvent(new CustomEvent("role-changed", { detail: role }));
}



// ============================
// Section: Axios Interceptor
// ============================
axios.interceptors.response.use(
    response => response,
    async error => {
        if (error.response && error.response.status === 401) {
            updateLoginState(false);
            window.location.href = "/#/login";
        }

        return Promise.reject(error);
    }
);


// ============================
// Section: Check Login Status
// ============================
async function checkLoginStatus() {
    try {
        const response = await axios.get("/Users/me");
        if (response.data.role !== "Guest") {
            updateLoginState(true, response.data.role);
        }
        else {
            updateLoginState(false, "Guest");
        }

    } catch (error) {
        updateLoginState(false);
    }
}

await checkLoginStatus();



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
window.fpPromise.then(() => {
    initializeVisitorId();
});


// ============================
// Section: Vue Router Initialization
// ============================
const router = VueRouter.createRouter({
    history: VueRouter.createWebHashHistory(),
    routes
});


// ============================
// Router Guard for Access Control
// ============================
const protectedRoutes = ["/change-password", "/orders", "/order-confirmation"];
const publicOnlyRoutes = ["/login", "/register", "/forgot-password", "/reset-password"];

router.beforeEach(async (to, from, next) => {
    const isLoggedIn = window.isLoggedIn;

    // If user is not logged in, redirect to login
    if (protectedRoutes.includes(to.path) && !isLoggedIn) {
        next("/login");
    }

    // If user is logged in, redirect to frontpage
    else if (publicOnlyRoutes.includes(to.path) && isLoggedIn) {
        next("/")
    }

    // Allow rest
    else {
        next();
    }
});


// ============================
// Section: Vue App Initialization
// ============================
const app = Vue.createApp({
    template: `<layout-component></layout-component>`
});

// Dynamically create and register the layout component
app.component("layout-component", createLayoutComponent());

app.use(router);
app.mount("#app");

console.log("Vue app initialized");
