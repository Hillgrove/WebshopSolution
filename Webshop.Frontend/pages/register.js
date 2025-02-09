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
                                    <input class="form-control" type="password" v-model="registerData.password" id="password" required minlength="8" maxlength="64" @input="analyzePassword">
                                    <label class="form-label" for="password">Password</label>
                                </div>


                                <!-- Repeat Password input -->
                                <div class="form-outline mb-4">
                                    <input class="form-control" type="password" v-model="registerData.repeatPassword" id="repeatpassword" required minlength="8" maxlength="64" @input="analyzePassword">
                                    <label class="form-label" for="repeatpassword">Repeat Password</label>
                                </div>

                                <!-- Submit button -->
                                <button type="submit" class="btn btn-primary btn-block mb-4" :disabled="passwordFeedback === 'Very weak' ||
                                    passwordFeedback === 'Weak' ||
                                    passwordFeedback === '' ||
                                    registerData.password.length < 8"
                                >Register</button>

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

    methods: {
        analyzePassword() {
            if (!this.registerData.password) {
                this.passwordFeedback = ""
                return
            }

            const result = zxcvbn(this.registerData.password)
            const strengthLabels = ["Very weak", "Weak", "Fair", "Strong", "Very strong"]
            this.passwordFeedback = strengthLabels[result.score]
        },

        async registerUser() {
            this.registerData.email = this.registerData.email.trim().toLowerCase()

            try {
                const response = await axios.post("/Users/register", this.registerData)
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
