import { useEffect, useState } from "react";
import { getUsers } from "../api";

export default function UsersPage() {
  const [users, setUsers] = useState([]);

  useEffect(() => {
    getUsers().then(setUsers).catch(console.error);
  }, []);

  return (
    <main className="page">
      <section className="section">
        <h1>Users</h1>
        {users.length === 0 ? (
          <p>No users found.</p>
        ) : (
          <div className="list-grid">
            {users.map((user) => (
              <div key={user.id} className="card">
                <h2>{user.userName}</h2>
                <p>{user.email}</p>
                <p>Role: {(user.roles ?? user.Roles)?.join(", ") ?? "N/A"}</p>
                <p>Restaurant: {user.restaurantId ?? user.RestaurantId ?? "None"}</p>
              </div>
            ))}
          </div>
        )}
      </section>
    </main>
  );
}
