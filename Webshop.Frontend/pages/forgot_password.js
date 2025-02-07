export const ForgotPasswordPage = {
    template: `
        <div class="container mt-5">
            <div class="row justify-content-center">
                <div class="col-12 col-sm-8 col-md-6 col-lg-4">
                    <div class="card">
                        <div class="card-header text-center">
                            <h1>Reset Password</h1>
                        </div>
                        <div class="card-body">
                            <form @submit.prevent="resetPassword">
                                <div class="form-outline mb-4">
                                    <input class="form-control" type="email" v-model="loginData.email" id="email" required>
                                    <label class="form-label" for="email">Email address</label>
                                </div>
                                <button type="submit" class="btn btn-primary btn-block mb-4">Reset</button>
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
            loginData: { email: "" },
            message: ""
        };
    },

    methods: {
        resetPassword() {
            this.message = "If this email exists in our system, you will receive an email with instructions on how to reset your password";
            // Implement the reset password logic here
        }
    }
};