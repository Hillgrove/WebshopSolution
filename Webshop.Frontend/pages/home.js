export const HomePage = {
    template: `<h1>You're logged in as the {{ role }} role</h1>`,

    data() {
        return {
            role: window.userRole
        };
    },

    mounted() {
        window.addEventListener("role-changed", (event) => {
            this.role = event.detail;
        });
    }
};
