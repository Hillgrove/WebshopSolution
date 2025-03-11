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
                            <button class="btn btn-primary" @click="addToCart(product)">Add to basket</button>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    `,

    data() {
        return {
            // product: { id, name: "", Quantity,  price},
            products: []
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
        async addToCart(product) {
            try {
                await axios.post("/Cart/add", {
                    ProductId: product.id,
                    ProductName: product.name,
                    Quantity: 1,
                    PriceInOere: product.price * 100
                });
                alert("Product added to cart!");
            }
            catch (error) {
                console.error("Error adding product to cart:" + error);
            }

        }

    }
};
