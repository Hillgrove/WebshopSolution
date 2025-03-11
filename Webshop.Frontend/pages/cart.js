export const CartPage = {
    template: `
            <div class="container mt-4">
            <h1 class="text-center mb-4">Shopping Cart</h1>
            <div v-if="cart.length === 0">
                <p class="text-center">Your cart is empty.</p>
            </div>
            <div v-else>
                <table class="table">
                    <thead>
                        <tr>
                            <th>Product</th>
                            <th>Quantity</th>
                            <th>Price</th>
                            <th>Total</th>
                            <th>Actions</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr v-for="item in cart" :key="item.ProductId">
                            <td>{{ item.ProductName }}</td>
                            <td>
                                <input type="number" v-model="item.Quantity" min="1" class="form-control"
                                    @change="updateCart(item)">
                            </td>
                            <td>{{ item.PriceInOere / 100 }} kr.</td>
                            <td>{{ (item.Quantity * item.PriceInOere) / 100 }} kr.</td>
                            <td>
                                <button class="btn btn-danger" @click="removeFromCart(item.ProductId)">Remove</button>
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
                console.error("Error fetch cart:", error);
            }
        },

        async updateCart(item) {
            try {
                await axios.post("/Cart/updat", {
                    ProductId: item.ProductId,
                    Quantity: item.Quantity
                });
                await this.loadCart();
            }
            catch (error) {
                console.error("Error updating cart:", error);
            }
        },

        async removeFromCart(productId) {
            try {
                await axios.post("/Cart/remove", productId);
                await this.loadCart();
            }
            catch (error) {
                console.error("Error removing from cart:" + error);
            }
        }
    }
};