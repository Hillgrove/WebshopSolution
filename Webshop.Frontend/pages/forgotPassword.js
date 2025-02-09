export const ForgotPasswordPage = {
    template: `
        <div class="container mt-5">
            <div class="row justify-content-center">
                <div class="col-10 col-sm-10 col-md-8 col-lg-6">

                    <!-- Card -->
                    <div class="card">

                        <!-- Card Header -->
                        <div class="card-header text-center">
                            <h1>Password Reset</h1>
                        </div>

                        <!-- Card Body -->
                        <div class="card-body">
                            <form @submit.prevent="resetPassword">
                                <div class="form-outline mb-4">
                                    <input class="form-control" type="email" v-model="forgotData.email" id="email" required>
                                    <label class="form-label" for="email">Email address</label>
                                </div>
                                <button type="submit" class="btn btn-primary btn-block mb-4">Reset</button>
                            </form>
                            <div v-if="message" class="alert alert-info mt-3">
                                <p>{{ message }}</p>
                            </div>
                        </div>

                        <!-- Card Footer -->
                        <div class="card-footer">
                            <div class="container-fluid">
                                <div class="row">
                                    <div class="col-6">
                                        <router-link to="/login">Login</router-link>
                                    </div>
                                    <div class="col-6 text-end">
                                        <router-link to="/register">Register</router-link>
                                    </div>
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
            forgotData: { email: "" },
            message: ""
        };
    },

    methods: {
        async resetPassword() {
            try {
                // Retrieve visitorId from local storage
                const visitorId = localStorage.getItem('visitorId');

                // Send login request
                this.forgotData.email = this.forgotData.email.trim().toLowerCase()

                const response = await axios.post("/Users/forgot-password", {
                    ...this.forgotData,
                    visitorId
                });

                if (response.status === 200)
                {
                    this.message = "If this email exists in our system, you will receive a password reset email."
                }

            } catch (error) {""
                this.message = "Error: " + error.message;
            }
        }
    }
};