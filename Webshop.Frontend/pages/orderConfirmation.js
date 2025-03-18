export const OrderConfirmationPage = {
    template: `
        <div class="container text-center mt-5">
            <h1>Thank You!</h1>
            <p>Your order has been placed successfully.</p>
            <p><strong>Total:</strong> {{ formattedTotal }} kr.</p>
            <router-link class="btn btn-primary" to="/">Return to Home</router-link>
        </div>
    `,

    data() {
        return {
            total: localStorage.getItem("lastOrderTotal") || 0
        };
    },

    computed: {
        formattedTotal() {
            return (this.total / 100).toFixed(2);
        }
    },

    mounted() {
        localStorage.removeItem("lastOrderTotal"); // Ryd midlertidig lagring
    }
};
