// ============================
// Section: Import Statements
// ============================

import { LayoutComponent } from "./layoutComponent.js";
import { HomePage } from "./pages/home.js";
import { LoginPage } from "./pages/login.js";
import { RegisterPage } from "./pages/register.js";
import { AboutPage } from "./pages/about.js";
import { ForgotPasswordPage } from "./pages/forgotPassword.js";
import { ResetPasswordPage } from "./pages/resetPassword.js";
import { ChangePasswordPage } from "./pages/changePassword.js";


// ============================
// Section: Route Definitions
// ============================

const routes = [
    { path: "/", component: HomePage },
    { path: "/login", component: LoginPage },
    { path: "/register", component: RegisterPage },
    { path: "/about", component: AboutPage },
    { path: "/forgot-password", component: ForgotPasswordPage },
    { path: "/reset-password", component: ResetPasswordPage },
    { path: "/change-password", component: ChangePasswordPage },
    { path: "/reset-password", component: ResetPasswordPage }
];


// ============================
// Section: Axios Configuration
// ============================

// axios.defaults.baseURL = "https://localhost:7016/api";
axios.defaults.baseURL = "https://sikkersoftwarewebshop.azurewebsites.net/api";

// enable sending cookies with requests
axios.defaults.withCredentials = true;


// ============================
// Section: Visitor ID Initialization
// ============================

// Check if visitorId exists in local storage
let visitorId = localStorage.getItem('visitorId');

if (!visitorId) {
    // Initialize FingerprintJS and store visitorId in local storage
    window.fpPromise = window.FingerprintJS.load().then(fp => {
        return fp.get();
    }).then(result => {
        visitorId = result.visitorId;
        localStorage.setItem('visitorId', visitorId);
        console.log("Generated new Visitor ID:", visitorId);
    });
} else {
    console.log("Using existing Visitor ID:", visitorId);
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
    template: `<layout-component></layout-component>`
});
app.component("layout-component", LayoutComponent);
app.use(router);
app.mount("#app");

console.log("Vue app initialized");
