export const OrderHistoryPage = {
    template: `
        <div class="container mt-4">
            <h1 class="text-center mb-4">Order History</h1>

            <div v-if="orders.length === 0" class="alert alert-info text-center">
                You have no past orders.
            </div>

            <div v-else class="accordion" id="orderAccordion">
                <div class="card mb-3" v-for="order in orders" :key="order.id">
                    <div class="card-header bg-dark text-white d-flex justify-content-between align-items-center"
                         :id="'orderHeader' + order.id">
                        <span>
                            <strong>Order #{{ order.id }}</strong> -
                            {{ new Date(order.createdAt).toLocaleString() }}
                        </span>
                        <span class="fw-bold">{{ (order.totalPriceInOere / 100).toFixed(2) }} kr.</span>
                        <button class="btn btn-light btn-sm"
                                type="button" data-bs-toggle="collapse"
                                :data-bs-target="'#order' + order.id">
                            Show Details
                        </button>
                    </div>

                    <div :id="'order' + order.id" class="collapse" data-bs-parent="#orderAccordion">
                        <div class="card-body">
                            <table class="table table-striped text-center">
                                <thead class="table-secondary">
                                    <tr>
                                        <th>Product</th>
                                        <th>Quantity</th>
                                        <th>Price</th>
                                        <th>Total</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    <tr v-for="item in order.items" :key="item.productId">
                                        <td>{{ item.productName }}</td>
                                        <td>{{ item.quantity }}</td>
                                        <td>{{ (item.priceAtPurchaseInOere / 100).toFixed(2) }} kr.</td>
                                        <td>{{ ((item.quantity * item.priceAtPurchaseInOere) / 100).toFixed(2) }} kr.</td>
                                    </tr>
                                </tbody>
                            </table>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    `,

    data() {
        return {
            orders: []
        };
    },

    async mounted() {
        await this.loadOrders();
    },

    methods: {
        async loadOrders() {
            try {
                const response = await axios.get("/Orders/my-orders");
                this.orders = response.data;
            } catch (error) {
                console.error("Error fetching orders:", error);
            }
        }
    }
};
