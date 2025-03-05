export function createLayoutComponent(globalState) {
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
                                    <router-link class="nav-link" to="/about">About</router-link>
                                </li>
                                <li class="nav-item">
                                    <button @click="logoutUser" v-if="isLoggedIn" class="btn btn-danger">Log Out</button>
                                    <router-link class="btn btn-success" to="/login" v-else>Log In</router-link>
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
            const isLoggedIn = Vue.computed(() => globalState.isLoggedIn);

            const logoutUser = async () => {
                try {
                    await axios.post("/Users/logout");
                    globalState.isLoggedIn = false;
                } catch (error) {
                    console.error("Logout failed", error);
                }
            };

            return { isLoggedIn, logoutUser };
        }
    };
}
