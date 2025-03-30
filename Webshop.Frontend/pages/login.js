import { updateLoginState } from "../index.js";

export const LoginPage = {
    template: `
        <div class="container mt-5">
            <div class="row justify-content-center">
                <div class="col-10 col-sm-10 col-md-8 col-lg-6">

                    <!-- Card -->
                    <div class="card">

                        <!-- Card Header -->
                        <div class="card-header text-center">
                            <h1>Login</h1>
                        </div>

                        <!-- Card Body -->
                        <div class="card-body">
                            <form @submit.prevent="loginUser">
                                <!-- Email input -->
                                <div class="form-outline mb-4">
                                    <input class="form-control" type="email" v-model="loginData.email" id="email" required>
                                    <label class="form-label" for="email">Email address</label>
                                </div>

                                <!-- Password input -->
                                <div class="form-outline mb-4">
                                    <input class="form-control" type="password" v-model="loginData.password" id="password" required minlength="8" maxlength="64">
                                    <label class="form-label" for="password">Password</label>
                                </div>

                                <!-- Submit button -->
                                <button type="submit" class="btn btn-primary btn-block mb-4">Sign in</button>

                                <!-- Autofill buttons -->
                                <div class="d-flex justify-content-between mt-3">
                                    <button @click="fillAdmin" type="button" class="btn btn-outline-secondary btn-sm">
                                        Autofill Admin
                                    </button>
                                    <button @click="fillCustomer" type="button" class="btn btn-outline-secondary btn-sm">
                                        Autofill Customer
                                    </button>
                                </div>

                            </form>

                            <div v-if="message" class="alert alert-info mt-3">
                                <p>{{ message }}</p>
                            </div>
                        </div>

                        <!-- Card Footer -->
                        <div class="card-footer">
                            <div class="row">
                                <div class="col-6">
                                    <router-link to="/register">Register</router-link>
                                </div>
                                <div class="col-6 text-end">
                                    <router-link to="/forgot-password">Forgot password?</router-link>
                                </div>
                            </div>
                        </div>

                    </div>
                </div>
            </div>
        </div>
    `,

    data() {
        return {
            loginData: { email: "", password: "" },
            message: ""
        };
    },

    methods: {
        async loginUser() {
            // Normalize email
            this.loginData.email = this.loginData.email.trim().toLowerCase();

            // Retrieve visitorId from local storage
            let visitorId = localStorage.getItem("visitorId");
            if (!visitorId) {
                console.log("Visitor ID missing. Waiting for FingerprintJS...");
                visitorId = await window.fpPromise.then(fp => fp.get()).then(result => {
                    localStorage.setItem("visitorId", result.visitorId);
                    return result.visitorId;
                });
            }

            try {
                // Send login request
                const response = await axios.post("/Users/login", {
                    ...this.loginData,
                    visitorId
                });

                if (response.status === 200) {
                    updateLoginState(true, response.data.role);

                    // Check if login was triggered from the cart/checkout flow
                    const redirectTo = localStorage.getItem("redirectAfterLogin");
                    if (redirectTo && this.$router.resolve(redirectTo).matched.length > 0) {
                        localStorage.removeItem("redirectAfterLogin");
                        this.$router.push(redirectTo);
                    } else {
                        localStorage.removeItem("redirectAfterLogin");
                        this.$router.push("/"); // Default to home page if no redirect is set
                    }
                }

            } catch (error) {
                if (error.response && error.response.status === 400) {
                    this.message = "Bad request: " + error.response.data;
                } else if (error.response && error.response.status === 401) {
                    this.message = "Unauthorized: Invalid email or password";
                } else if (error.response && error.response.status === 429) {
                    this.message = "Too many requests. Please try again later.";
                } else {
                    this.message = "Login failed: " + error.message;
                }
            }
        },

        fillAdmin() {
            this.loginData.email = "webshopadmin@hillgrove.dk";
            this.loginData.password = "webshopadmin@hillgrove.dk";
        },

        fillCustomer() {
            this.loginData.email = "customer@hillgrove.dk";
            this.loginData.password = "customer@hillgrove.dk";
        }
    }
};
