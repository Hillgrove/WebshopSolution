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
                                <div class="form-outline mb-4">
                                    <input class="form-control" type="password" v-model="resetData.password" id="password" required minlength="8" maxlength="64">
                                    <label class="form-label" for="password">New Password</label>
                                </div>
                                <div class="form-outline mb-4">
                                    <input class="form-control" type="password" v-model="resetData.confirmPassword" id="confirmPassword" required minlength="8" maxlength="64">
                                    <label class="form-label" for="confirmPassword">Confirm New Password</label>
                                </div>
                                <button type="submit" class="btn btn-primary btn-block mb-4">Reset Password</button>
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
            resetData: { password: "", confirmPassword: "" },
            message: "",
            token: null
        };
    },

    created() {
        this.token = this.$route.query.token;
    },

    methods: {
        async resetPassword() {
            if (this.resetData.password !== this.resetData.confirmPassword) {
                this.message = "Passwords do not match.";
                return;
            }

            try {
                const payload = {
                    newPassword: this.resetData.password,
                    token: this.token
                };

                const response = await axios.post("/Users/reset-password", payload);

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