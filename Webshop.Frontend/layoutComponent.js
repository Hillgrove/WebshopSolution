import { updateLoginState } from "./index.js";

export function createLayoutComponent() {
    return {
        template: `
            <div class="container">
                <nav class="navbar navbar-expand-lg bg-dark" data-bs-theme="dark">
                    <div class="container-fluid">
                        <a class="navbar-brand" href="#">Webshop</a>
                        <button class="navbar-toggler" type="button" data-bs-toggle="collapse" data-bs-target="#navbarNav" aria-controls="navbarNav" aria-expanded="false" aria-label="Toggle navigation">
                            <span class="navbar-toggler-icon"></span>
                        </button>

                        <div class="collapse navbar-collapse" id="navbarNav">
                            <ul class="navbar-nav me-auto mb-2 mb-lg-0">

                                <li class="nav-item">
                                    <router-link class="nav-link" to="/">Home</router-link>
                                </li>

                                <li class="nav-item">
                                    <router-link class="nav-link" to="/products">Products</router-link>
                                </li>

                                <li v-if="isLoggedIn && userRole === 'Admin'" class="nav-item">
                                    <router-link class="nav-link" to="/admin-users">All Users</router-link>
                                </li>

                                <li v-if="userRole !== 'Admin'" class="nav-item">
                                    <router-link class="nav-link" to="/cart">Cart</router-link>
                                </li>

                                <li v-if="isLoggedIn && userRole !== 'Admin'" class="nav-item">
                                    <router-link class="nav-link" to="/orders">Orders</router-link>
                                </li>
                            </ul>

                            <ul class="navbar-nav mb-2 mb-lg-0 navbar-right">

                                <span class="navbar-text me-3">
                                    Role: {{ userRole }}
                                </span>

                                <li v-if="isLoggedIn && userRole !== 'Admin'" class="nav-item">
                                    <router-link class="nav-link" to="/change-password">Change Password</router-link>
                                </li>

                                <!-- Show logout if logged in, otherwise show login -->
                                <li v-if="isLoggedIn" class="nav-item">
                                    <button @click="logoutUser" class="btn btn-danger">Log Out</button>
                                </li>
                                <li v-else class="nav-item">
                                    <router-link class="btn btn-success" to="/login">Log In</router-link>
                                </li>
                            </ul>

                        </div>
                    </div>
                </nav>
                <router-view></router-view>
            </div>
        `,

        data() {
            return {
                isLoggedIn: window.isLoggedIn,
                userRole: window.userRole
            };
        },

        mounted() {
            window.addEventListener("auth-changed", (event) => {
                this.isLoggedIn = event.detail;
                this.$forceUpdate();
            });

            window.addEventListener("role-changed", (event) => {
                this.userRole = event.detail;
                this.$forceUpdate();
            });
        },

        methods: {
            async logoutUser() {
                try {
                    await axios.post("/Users/logout");
                localStorage.clear();
                updateLoginState(false);
                window.location.href = "/#/login"
                }
                catch (error) {
                    console.error("Logout failed", error)
                }

            }
        }
    };
}
