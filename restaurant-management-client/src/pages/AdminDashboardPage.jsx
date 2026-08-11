import { useEffect, useState } from "react";
import { useAuth } from "../AuthProvider";
import { getRestaurantDashboard, getAllRestaurants } from "../api";

export default function AdminDashboardPage() {
  const auth = useAuth();
  const [restaurantId, setRestaurantId] = useState(auth.user?.restaurantId ?? null);
  const [restaurants, setRestaurants] = useState([]);
  const [dashboard, setDashboard] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  const isSuperAdmin = auth.user?.roles?.includes("SuperAdmin");

  useEffect(() => {
    if (isSuperAdmin) {
      getAllRestaurants()
        .then(setRestaurants)
        .catch((err) => setError(err?.response?.data?.message || "Unable to load restaurants."));
    }
  }, [isSuperAdmin]);

  useEffect(() => {
    if (!restaurantId) {
      setDashboard(null);
      return;
    }

    setLoading(true);
    getRestaurantDashboard(restaurantId)
      .then((data) => {
        setDashboard(data);
        setError(null);
      })
      .catch((err) => setError(err?.response?.data?.message || "Unable to load dashboard."))
      .finally(() => setLoading(false));
  }, [restaurantId]);

  return (
    <main className="page">
      <section className="section">
        <h1>Restaurant Admin Dashboard</h1>
        <p>Use this dashboard to monitor restaurant menu counts, order status, and sales.</p>

        {isSuperAdmin && (
          <div className="card">
            <label htmlFor="restaurant-select">Select restaurant:</label>
            <select
              id="restaurant-select"
              value={restaurantId ?? ""}
              onChange={(e) => setRestaurantId(Number(e.target.value) || null)}
            >
              <option value="">Choose a restaurant</option>
              {restaurants.map((restaurant) => (
                <option key={restaurant.id} value={restaurant.id}>
                  {restaurant.name}
                </option>
              ))}
            </select>
          </div>
        )}

        {loading && <p>Loading dashboard...</p>}
        {error && <p className="error">{error}</p>}
        {dashboard && (
          <div className="grid-columns">
            <div className="card">
              <h2>{dashboard.name}</h2>
              <p>{dashboard.address}</p>
              <p>Status: {dashboard.isActive ? "Active" : "Inactive"}</p>
            </div>
            <div className="card">
              <h2>Menu items</h2>
              <p>{dashboard.menuItemCount}</p>
            </div>
            <div className="card">
              <h2>Pending orders</h2>
              <p>{dashboard.pendingOrders}</p>
            </div>
            <div className="card">
              <h2>Completed orders</h2>
              <p>{dashboard.completedOrders}</p>
            </div>
            <div className="card">
              <h2>Total sales</h2>
              <p>${dashboard.totalSales?.toFixed(2)}</p>
            </div>
          </div>
        )}
      </section>
    </main>
  );
}
