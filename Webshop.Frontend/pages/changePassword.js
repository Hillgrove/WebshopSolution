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

                                <div class="form-outline mb-4">
                                    <input class="form-control" type="email" v-model="changeData.email" id="email" required>
                                    <label class="form-label" for="email">Email address</label>
                                </div>

                                <div class="form-outline mb-4">
                                    <input class="form-control" type="password" v-model="changeData.oldPassword" id="oldPassword" required>
                                    <label class="form-label" for="oldPassword">Old Password</label>
                                </div>

                                <div class="form-outline mb-4">
                                    <input class="form-control" type="password" v-model="changeData.newPassword" id="newPassword" required>
                                    <label class="form-label" for="newPassword">New Password</label>
                                </div>

                                <div class="form-outline mb-4">
                                    <input class="form-control" type="password" v-model="changeData.repeatPassword" id="repeatPassword" required>
                                    <label class="form-label" for="newPassword">Repeat New Password</label>
                                </div>

                                <button type="submit" class="btn btn-primary btn-block mb-4">Change Password</button>
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
            changeData: { email: "", oldPassword: "", newPassword: "" },
            message: ""
        };
    },

    methods: {
        async changePassword() {
            try {
                // Retrieve visitorId from local storage
                //const visitorId = localStorage.getItem('visitorId');

                // Send login request
                this.changeData.email = this.changeData.email.trim().toLowerCase()

                const response = await axios.post("/Users/change-password", {
                    ...this.changeData
                });

                if (response.status === 200)
                {
                    this.message = "Password successfully changed."
                }

            } catch (error) {""
                this.message = "Error: " + error.message;
            }
        }
    }
};