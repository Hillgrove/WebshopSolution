export const RegisterPage = {
    template: `
        <div>
            <h1>Register</h1>
            <form @submit.prevent="registerUser">
                <div>
                    <label for="email">Email:</label>
                    <input type="email" v-model="registerData.email" required>
                </div>
                <div>
                    <label for="password">Password:</label>
                    <input type="password" v-model="registerData.password" required minlength="8">
                </div>
                <button type="submit">Register</button>
            </form>
            <div v-if="message">
                <p>{{ message }}</p>
            </div>
        </div>
    `,
    data() {
        return {
            registerData: { email: "", password: "" },
            message: ""
        };
    },
    methods: {
        async registerUser() {
            try {
                const url = "https://localhost:7016/api/Users/register";
                await axios.post(url, this.registerData);
                this.message = "User registered successfully!";
            } catch (error) {
                this.message = "Registration failed: " + error.message;
            }
        }
    }
};
