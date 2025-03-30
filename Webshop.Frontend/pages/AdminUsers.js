export const AdminUsersPage = {
    template: `
        <div class="container mt-4">
            <h1 class="text-center mb-4">User Overview</h1>

            <div v-if="users.length === 0" class="alert alert-info text-center">
                No users found.
            </div>

            <div v-else>
                <table class="table table-bordered text-center">
                    <thead class="table-dark">
                        <tr>
                            <th>ID</th>
                            <th>Email</th>
                            <th>Role</th>
                            <th>Created At</th>
                            <th>Actions</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr v-for="user in users" :key="user.id">
                            <td>{{ user.id }}</td>
                            <td>{{ user.email }}</td>
                            <td>{{ user.role }}</td>
                            <td>{{ new Date(user.createdAt).toLocaleString() }}</td>
                            <td>
                                <button v-if="user.role !== 'Admin' && user.role !== 'Guest'" @click="deleteUser(user.id)" class="btn btn-danger btn-sm">Delete</button>
                                <span v-else>&nbsp;</span>
                            </td>

                        </tr>
                    </tbody>
                </table>
            </div>
        </div>
    `,

    data() {
        return {
            users: []
        };
    },

    async mounted() {
        try {
            const response = await axios.get("/Users/all");
            this.users = response.data;
        } catch (error) {
            console.error("Failed to fetch users:", error);
        }
    },

    methods: {
        async deleteUser(userId) {
            if (!confirm("Are you sure you want to delete this user?")) return;

            try {
                await axios.delete(`/Users/${userId}`);
                this.users = this.users.filter(u => u.id !== userId);
            } catch (error) {
                console.error("Failed to delete user:", error);
                alert("Deletion failed.");
            }
        }
    }
};
