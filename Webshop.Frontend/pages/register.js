export const RegisterPage = {
    template: `
        <div class="container mt-5">
            <div class="row justify-content-center">
                <div class="col-10 col-sm-10 col-md-8 col-lg-6">

                    <!-- Card -->
                    <div class="card">

                        <!-- Card Header -->
                        <div class="card-header text-center">
                            <h1>Sign Up</h1>
                        </div>

                        <!-- Card Body -->
                        <div class="card-body">
                            <form @submit.prevent="registerUser">

                                <!-- Email input -->
                                <div class="form-outline mb-4">
                                    <input class="form-control" type="email" v-model="registerData.email" id="email" required>
                                    <label class="form-label" for="email">Email address</label>
                                </div>

                                <!-- Password input -->
                                <div class="form-outline mb-4">
                                    <input class="form-control" type="password" v-model="registerData.password" id="password" required minlength="8" maxlength="64">
                                    <label class="form-label" for="password">Password</label>
                                </div>

                                <!-- Repeat Password input -->
                                <div class="form-outline mb-4">
                                    <input class="form-control" type="password" v-model="registerData.repeatPassword" id="repeatpassword" required minlength="8" maxlength="64">
                                    <label class="form-label" for="repeatpassword">Repeat Password</label>
                                </div>

                                <!-- Password Mismatch Warning -->
                                <div v-if="passwordMismatch" class="text-danger">
                                    <p>Passwords do not match.</p>
                                </div>

                                <!-- Submit button -->
                                <button type="submit" class="btn btn-primary btn-block mb-4" :disabled="passwordMismatch
                                                                                                     || passwordFeedback === 'Very weak'
                                                                                                     || passwordFeedback === 'Weak'
                                                                                                     || registerData.password.length < 8
                                                                                                     || !registerData.password
                                                                                                     || !registerData.repeatPassword">
                                    Register
                                </button>


                            </form>

                            <div v-if="passwordFeedback">
                                <p>Password must be between 8 and 64 chars long</p>
                                <p>Password must be at least fair</p>
                                <p>Strength: {{ passwordFeedback }}</p>
                            </div>

                            <div v-if="message" class="alert alert-info mt-3">
                                <p>{{ message }}</p>
                            </div>

                        </div>

                        <!-- Card Footer -->
                        <div class="card-footer">
                            <div class="row">
                                <div class="col-6">
                                    <router-link to="/login">Login</router-link>
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
            registerData: { email: "", password: "", repeatPassword: "" },
            message: "",
            passwordFeedback: "",
        }
    },

    computed: {
        passwordMismatch() {
            return this.registerData.password !== this.registerData.repeatPassword
        }
    },

    watch: {
        "registerData.password"(newPassword) {
            if (!newPassword) {
                this.passwordFeedback = "";
                return;
            }
            const result = zxcvbn(newPassword);
            const strengthLabels = ["Very weak", "Weak", "Fair", "Strong", "Very strong"];
            this.passwordFeedback = strengthLabels[result.score];
        }
    },

    methods: {
        async registerUser() {
            if (this.registerData.password !== this.registerData.repeatPassword) {
                this.message = "Passwords do not match.";
                return;
            }

            this.registerData.email = this.registerData.email.trim().toLowerCase()

            // Retrieve visitorId from local storage
            const visitorId = localStorage.getItem('visitorId');

            try {
                // Send register request
                const response = await axios.post("/Users/register", {
                    ...this.registerData,
                    visitorId
                })

                this.message = "User registered successfully!"

            } catch (error) {
                this.registerData.password = ""
                this.registerData.repeatPassword = ""
                this.passwordFeedback = ""
                this.message = "Registration failed: " + error.response.data
            }
        }
    }
};
