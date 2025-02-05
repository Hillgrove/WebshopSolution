export const LayoutComponent = {
    template: `
        <div>
            <nav>
                <ul>
                    <li><router-link to="/">Home</router-link></li>
                    <li><router-link to="/register">Register</router-link></li>
                    <li><router-link to="/login">Login</router-link></li>
                    <li><router-link to="/about">About</router-link></li>
                </ul>
            </nav>
            <router-view></router-view>
        </div>
    `
};
