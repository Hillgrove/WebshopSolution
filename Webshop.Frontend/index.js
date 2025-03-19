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
import { OrderConfirmationPage } from "./pages/orderConfirmation.js";
import { OrderHistoryPage } from "./pages/orderHistory.js";


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
    { path: "/cart", component: CartPage },
    { path: "/order-confirmation", component: OrderConfirmationPage },
    { path: "/orders", component: OrderHistoryPage },
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
// Section: Function to Check Login Status
// ============================
export async function checkLoginStatus() {
    try {
        const response = await axios.get("/Users/me"); // Check if session is active
        // localStorage.setItem("userId", response.data.email); // Cache userId
        window.dispatchEvent(new CustomEvent("auth-changed", { detail: true}));
        return true;
    } catch (error) {
        // localStorage.removeItem("userId"); // Clear cache if session is inactive
        window.dispatchEvent(new CustomEvent("auth-changed", { detail: false }));
        return false;
    }
}


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
