export const ChangePasswordPage = {
    template: `
        <div class="container mt-5">
            <div class="row justify-content-center">
                <div class="col-10 col-sm-10 col-md-8 col-lg-6">

                    <!-- Card -->
                    <div class="card">

                        <!-- Card Header -->
                        <div class="card-header text-center">
                            <h1>Change Password</h1>
                        </div>

                        <!-- Card Body -->
                        <div class="card-body">
                            <form @submit.prevent="changePassword">

                                <!-- Email input -->
                                <div class="form-outline mb-4">
                                    <input class="form-control" type="email" v-model="changeData.email" id="email" required>
                                    <label class="form-label" for="email">Email address</label>
                                </div>

                                <!-- Password input -->
                                <div class="form-outline mb-4">
                                    <input class="form-control" type="password" v-model="changeData.oldPassword" id="oldPassword">
                                    <label class="form-label" for="oldPassword">Old Password</label>
                                </div>


                                <!-- New Password input -->
                                <div class="form-outline mb-4">
                                    <input class="form-control" type="password" v-model="changeData.newPassword" id="newPassword" required minlength="8" maxlength="64" @input="analyzePassword">
                                    <label class="form-label" for="newPassword">New Password</label>
                                </div>

                                <!-- Repeat new Password input -->
                                <div class="form-outline mb-4">
                                    <input class="form-control" type="password" v-model="changeData.repeatNewPassword" id="repeatNewPassword" required minlength="8" maxlength="64" @input="analyzePassword">
                                    <label class="form-label" for="repeatNewPassword">Repeat New Password</label>
                                </div>

                                <!-- Submit button -->
                                <button type="submit" class="btn btn-primary btn-block mb-4" :disabled="passwordFeedback === 'Very weak' ||
                                    passwordFeedback === 'Weak' ||
                                    passwordFeedback === '' ||
                                    changeData.newPassword.length < 8"
                                >Change Password</button>

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

                    </div>
                </div>
            </div>
        </div>
    `,
    data() {
        return {
            changeData: { email: "", oldPassword: "", newPassword: "", repeatNewPassword: "" },
            message: "",
            passwordFeedback: "",
        }
    },

    methods: {
        analyzePassword() {
            if (!this.changeData.password) {
                this.passwordFeedback = ""
                return
            }

            const result = zxcvbn(this.changeData.password)
            const strengthLabels = ["Very weak", "Weak", "Fair", "Strong", "Very strong"]
            this.passwordFeedback = strengthLabels[result.score]
        },

        async changePassword() {
            this.changeData.email = this.changeData.email.trim().toLowerCase()

            try {
                const response = await axios.post("/Users/change-password", this.changeData)
                this.message = "Password changed sucessfully!"
            } catch (error) {
                this.changeData.oldPassword = ""
                this.changeData.newPassword = ""
                this.changeData.repeatNewPassword = ""
                this.passwordFeedback = ""
                this.message = "Registration failed: " + error.response.data
            }
        }
    }
};
