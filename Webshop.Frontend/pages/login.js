export const LoginPage = {
    template: `
        <div class="container mt-5">
            <div class="row justify-content-center">
                <div class="col-12 col-sm-8 col-md-6 col-lg-4">
                    <div class="card">
                        <div class="card-header text-center">
                            <h1>Login</h1>
                        </div>
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

                                <!-- Remember me and Forgot Password -->
                                <div class="row mb-4">
                                    <div class="col d-flex justify-content-center">
                                        <div class="form-check">
                                            <input class="form-check-input" type="checkbox" value="" id="remember" checked>
                                            <label class="form-check-label" for="remember">Remember me</label>
                                        </div>
                                    </div>
                                    <div class="col text-end">
                                        <router-link to="/forgot">Forgot Password?</router-link>
                                    </div>
                                </div>

                                <!-- Submit button -->
                                <button type="submit" class="btn btn-primary btn-block mb-4">Sign in</button>

                                <!-- Register link -->
                                <div class="text-center">
                                    <p>Not a member? <router-link to="/register">Register</router-link></p>
                                </div>
                            </form>
                            <div v-if="message" class="alert alert-info mt-3">
                                <p>{{ message }}</p>
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
            try {
                const url = "https://localhost:7016/api/Users/login";

                // Load FingerPrintJS
                const fp = await window.fpPromise;
                const result = await fp.get();
                const visitorId = result.visitorId;

                // Send login request
                const response = await axios.post(url, {
                    ...this.loginData,
                    visitorId
                });

                if (response.status === 200) {
                    this.message = "Login successful!";
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
        }
    }
};
