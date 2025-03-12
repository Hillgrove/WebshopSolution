export function createLayoutComponent() {
    return {
        template: `
            <div class="container">
                <nav class="navbar navbar-expand-lg bg-dark" data-bs-theme="dark">
                    <div class="container-fluid">
                        <a class="navbar-brand" href="#">Webshop</a>
                        <button class="navbar-toggler" type="button" data-bs-toggle="collapse" data-bs-target="#navbarNav" aria-controls="navbarNav" aria-expanded="false" aria-label="Toggle navigation">
                            <span class="navbar-toggler-icon"></span>
                        </button>

                        <div class="collapse navbar-collapse" id="navbarNav">
                            <ul class="navbar-nav me-auto mb-2 mb-lg-0">
                                <li class="nav-item">
                                    <router-link class="nav-link" to="/">Home</router-link>
                                </li>
                                <li class="nav-item">
                                    <router-link class="nav-link" to="/products">Products</router-link>
                                </li>
                                <li class="nav-item">
                                    <router-link class="nav-link" to="/cart">Cart</router-link>
                                </li>
                                <li class="nav-item">
                                    <router-link class="btn btn-success" to="/login">Log In</router-link>
                                </li>
                                <li class="nav-item">
                                    <button @click="logoutUser" class="btn btn-danger">Log Out</button>
                                </li>
                                <li class="nav-item">
                                    <router-link class="nav-link" to="/change-password">Change Password</router-link>
                                </li>
                            </ul>
                        </div>
                    </div>
                </nav>
                <router-view></router-view>
            </div>
        `,

        setup() {
            const logoutUser = async () => {
                try {
                    await axios.post("/Users/logout");
                    localStorage.clear();
                    setTimeout(() => {
                        window.location.href = "/#/login";
                        location.reload();
                    }, 500);
                } catch (error) {
                    console.error("Logout failed", error);
                }
            };

            return { logoutUser };
        }
    };
}
