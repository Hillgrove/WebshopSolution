// Import all page components
import { LayoutComponent } from "./layoutComponent.js";
import { HomePage } from "./pages/home.js";
import { LoginPage } from "./pages/login.js";
import { RegisterPage } from "./pages/register.js";
import { AboutPage } from "./pages/about.js";
import { ForgotPasswordPage } from "./pages/forgotPassword.js";


// Define Routes
const routes = [
    { path: "/", component: HomePage },
    { path: "/login", component: LoginPage },
    { path: "/register", component: RegisterPage },
    { path: "/about", component: AboutPage },
    { path: "/forgot", component: ForgotPasswordPage }
];

// Axios baseurl
axios.defaults.baseURL = "https://localhost:7016/api";

// Initialize FingerprintJS
window.fpPromise = window.FingerprintJS.load();

// Initialize Vue Router
const router = VueRouter.createRouter({
    history: VueRouter.createWebHashHistory(),
    routes
});

// Initialize Vue App
const app = Vue.createApp({
    template: `<layout-component></layout-component>`
});
app.component("layout-component", LayoutComponent);
app.use(router);
app.mount("#app");

console.log("Vue app initialized");