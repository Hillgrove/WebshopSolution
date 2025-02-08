export const LayoutComponent = {
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
                                <router-link class="nav-link" to="/login">Login</router-link>
                            </li>
                        </ul>
                    </div>

                </div>
            </nav>

            <router-view></router-view>
        </div>
    `
};
