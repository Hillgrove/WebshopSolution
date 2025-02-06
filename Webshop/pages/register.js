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
                    <input type="password" v-model="registerData.password" required minlength="8" maxlength="64" @input="analyzePassword">
                </div>
                <button type="submit" :disabled="passwordFeedback === 'Very weak' ||
                                                 passwordFeedback === 'Weak' ||
                                                 passwordFeedback === '' ||
                                                 registerData.password.length < 8"
                >
                    Register
                </button>
            </form>

            <div v-if="passwordFeedback">
                <p>Password must be between 8 and 64 chars long</p>
                <p>Password must be at least fair</p>
                <p>Strength: {{ passwordFeedback }}</p>
            </div>

            <div v-if="message">
                <p>{{ message }}</p>
            </div>
        </div>
    `,
    data() {
        return {
            registerData: { email: "", password: "" },
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
                const url = "https://localhost:7016/api/Users/register"
                const response = await axios.post(url, this.registerData)
                this.message = "User registered successfully!"
            } catch (error) {
                this.registerData.password = ""
                this.passwordFeedback = ""
                alert("Registration failed: " + error.response.data)
                //this.message = "Registration failed: " + error.response.data
            }
        }
    }
};
