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
                await axios.post(url, this.loginData);
                this.message = "Login successful!";
            } catch (error) {
                this.message = "Login failed: " + error.message;
            }
        }
    }
};
