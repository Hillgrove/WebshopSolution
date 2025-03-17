export const CartPage = {
    template: `
        <div class="container mt-4">
            <h1 class="text-center mb-4">Shopping Cart</h1>

            <div v-if="cart.length === 0" class="alert alert-info text-center">
                Your cart is empty.
            </div>

            <div v-else>
                <table class="table table-bordered text-center">
                    <thead class="table-dark">
                        <tr>
                            <th>Product</th>
                            <th>Quantity</th>
                            <th>Price</th>
                            <th>Total</th>
                            <th>Actions</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr v-for="item in cart" :key="item.productId">
                            <td class="align-middle">{{ item.productName }}</td>
                            <td class="align-middle">
                                <div class="d-flex justify-content-center align-items-center">

                                    <button class="btn btn-outline-secondary btn-sm"
                                        @click="changeQuantity(item.productId, -1)"
                                        :disabled="item.quantity <= 1">-</button>

                                    <span class="mx-2">{{ item.quantity }}</span>

                                    <button class="btn btn-outline-secondary btn-sm"
                                        @click="changeQuantity(item.productId, 1)">+</button>
                                </div>
                            </td>
                            <td class="align-middle">{{ (item.priceInOere / 100).toFixed(2) }} kr.</td>
                            <td class="align-middle">{{ ((item.quantity * item.priceInOere) / 100).toFixed(2) }} kr.</td>
                            <td class="align-middle">
                                <button class="btn btn-danger btn-sm" @click="removeFromCart(item.productId)">Remove</button>
                            </td>
                        </tr>
                    </tbody>
                </table>
            </div>
        </div>
    `,

    data() {
        return {
            cart: []
        };
    },

    async mounted() {
        await this.loadCart();
    },

    methods: {
        async loadCart() {
            try {
                const response = await axios.get("/Cart");
                this.cart = response.data;
            }
            catch (error) {
                console.error("Error fetching cart:", error);
            }
        },

        async changeQuantity(productId, delta) {
            const item = this.cart.find(i => i.productId === productId);
            if (!item) return;

            const newQuantity = item.quantity + delta;
            if (newQuantity < 1) return; // Prevent negative values

            try {
                await axios.post("/Cart/update", {
                    productId: productId,
                    delta: delta
                });
                await this.loadCart();
            }
            catch (error) {
                console.error("Error updating cart:", error);
            }
        },

        async removeFromCart(productId) {
            try {
                await axios.post("/Cart/remove", productId, {
                    headers: { "Content-Type": "application/json" }
                });

                await this.loadCart();
            }
            catch (error) {
                console.error("Error removing from cart:", error);
            }
        }
    }
};
