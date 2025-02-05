export const LoginPage = {
    template: `
        <div>
            <h1>Login</h1>
            <form @submit.prevent="loginUser">
                <div>
                    <label for="email">Email:</label>
                    <input type="email" v-model="loginData.email" required>
                </div>
                <div>
                    <label for="password">Password:</label>
                    <input type="password" v-model="loginData.password" required minlength="8">
                </div>
                <button type="submit">Login</button>
            </form>
            <div v-if="message">
                <p>{{ message }}</p>
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
                const response = await axios.post(url, this.loginData);

                if (response.status === 200) {
                    this.message = "Login successful!";
                }

            } catch (error) {
                if (error.response && error.response.status === 400) {
                    this.message = "Bad request: " + error.response.data;
                } else if (error.response && error.response.status === 401) {
                    this.message = "Unauthorized: Invalid email or password";
                } else {
                    this.message = "Login failed: " + error.message;
                }
            }
        }
    }
};
