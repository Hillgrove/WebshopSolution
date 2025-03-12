export const ProductsPage = {
    template: `
        <div class="container mt-4">
            <h1 class="text-center mb-4">Products</h1>
            <div class="row">
                <div class="col-md-4 mb-4" v-for="product in products" :key="product.id">
                    <div class="card h-100">
                        <div class="card-body">
                            <h5 class="card-title">{{ product.name }}</h5>
                            <p class="card-text text-muted">{{ product.category }}</p>
                            <p class="card-text">{{ product.description }}</p>
                            <p class="card-text fw-bold">{{ product.price }} kr.</p>
                        </div>
                        <div class="card-footer text-center">

                        <div v-if="cart.some(item => item.productId === product.id)" class="btn-group">
                            <button class="btn btn-outline-secondary" @click="changeQuantity(product.id, -1)">-</button>
                            <span class="mx-2">{{ cart.find(item => item.productId === product.id).quantity }}</span>
                            <button class="btn btn-outline-secondary" @click="changeQuantity(product.id, 1)">+</button>
                        </div>
                        <button v-else class="btn btn-primary" @click="addToCart(product.id)">Add to Cart</button>

                        </div>
                    </div>
                </div>
            </div>
        </div>
    `,

    data() {
        return {
            // product: { id, name: "", Quantity,  price},
            products: [],
            cart: []
        };
    },

    async mounted() {
        try {
            const response = await axios.get("/Products");
            this.products = response.data;
        }
        catch (error) {
            console.error("Error fetching products:", error);
        }
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

        async addToCart(productId) {
            try {
                await axios.post("/Cart/add", productId, {
                    headers: { "Content-Type": "application/json" }
                });

                await this.loadCart();
                // alert("Product added to cart!");
            }
            catch (error) {
                console.error("Error adding product to cart:" + error);
            }
        },

        async changeQuantity(productId, delta) {
            try {
                await axios.post("/Cart/update", { productId, delta });
                await this.loadCart();
            }
            catch (error) {
                console.error("Error updating cart:", error);
            }
        }
    }
};
