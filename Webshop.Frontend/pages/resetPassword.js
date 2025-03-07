export const ResetPasswordPage = {
    template: `
        <div class="container mt-5">
            <div class="row justify-content-center">
                <div class="col-12 col-sm-10 col-md-8 col-lg-6">

                    <!-- Card -->
                    <div class="card">

                        <!-- Card Header -->
                        <div class="card-header text-center">
                            <h1>Reset Password</h1>
                        </div>

                        <!-- Card Body -->
                        <div class="card-body">
                            <form @submit.prevent="resetPassword">

                                <!-- Password input -->
                                <div class="form-outline mb-4">
                                    <input class="form-control" type="password" v-model="resetData.password" id="password" required minlength="8" maxlength="64">
                                    <label class="form-label" for="password">New Password</label>
                                </div>

                                <!-- Repeat Password input -->
                                <div class="form-outline mb-4">
                                    <input class="form-control" type="password" v-model="resetData.repeatPassword" id="repeatPassword" required minlength="8" maxlength="64">
                                    <label class="form-label" for="repeatPassword">Repeat Password</label>
                                </div>

                                <!-- Password Mismatch Warning -->
                                <div v-if="passwordMismatch" class="text-danger">
                                    <p>Passwords do not match.</p>
                                </div>

                                <!-- Submit button -->
                                <button type="submit" class="btn btn-primary btn-block mb-4" :disabled="passwordMismatch
                                                                        || passwordFeedback === 'Very weak'
                                                                        || passwordFeedback === 'Weak'
                                                                        || resetData.password.length < 8
                                                                        || !resetData.password
                                                                        || !resetData.repeatPassword">
                                    Reset Password
                                </button>
                            </form>

                            <!-- Password Strength Feedback -->
                            <div v-if="passwordFeedback">
                                <p>Password must be between 8 and 64 chars long</p>
                                <p>Password must be at least fair</p>
                                <p>Strength: {{ passwordFeedback }}</p>
                            </div>

                            <!-- Status Messages -->
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
            resetData: { password: "", repeatPassword: "" },
            message: "",
            token: null,
            passwordFeedback: "",
        };
    },

    created() {
        this.token = decodeURIComponent(this.$route.query.token.replace(/ /g, '+'));
        // this.token = this.$route.query.token || "dummy-test-token"; // Enable this for testing
    },

    computed: {
        passwordMismatch() {
            return this.resetData.password !== this.resetData.repeatPassword
        }
    },

    watch: {
        "resetData.password"(newPassword) {
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
        async resetPassword() {
            // Retrieve visitorId from local storage
            const visitorId = localStorage.getItem('visitorId');

            const payload = {
                newPassword: this.resetData.password,
                token: this.token
            };

            try {
                const response = await axios.post("/Users/reset-password", {
                    ...payload,
                    visitorId
                });

                console.log("Response:", response);

                if (response.status === 200) {
                    this.message = "Password has been reset successfully.";
                }

            } catch (error) {
                this.message = "Error: " + error.message;
                console.error("Error response:", error.response);
            }
        }
    }
};