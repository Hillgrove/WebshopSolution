const baseURL = "https://localhost:7016/api/Users"

Vue.createApp({
    data() {
        return {
            addData: { email: "", password: "" },
            message: ""
        }
    },

    methods: {
        async registerUser() {
            try {
                const url = baseURL + "/register"
                console.log(url)
                response = await axios.post(url, this.addData)
                this.message = "User registered successfully!"
            }

            catch (error) {
                alert(error.message)
            }
        }
    }
}).mount('#app')
