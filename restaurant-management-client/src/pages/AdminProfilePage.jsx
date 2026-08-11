import { useEffect, useState } from "react";
import { useAuth } from "../AuthProvider";
import { getAllRestaurants, getRestaurantProfile, updateRestaurantProfile } from "../api";

export default function AdminProfilePage() {
  const auth = useAuth();
  const isSuperAdmin = auth.user?.roles?.includes("SuperAdmin");
  const [restaurants, setRestaurants] = useState([]);
  const [restaurantId, setRestaurantId] = useState(auth.user?.restaurantId ?? null);
  const [restaurant, setRestaurant] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [form, setForm] = useState({ name: "", address: "" });

  useEffect(() => {
    if (isSuperAdmin) {
      getAllRestaurants().then(setRestaurants).catch((err) => setError(err?.response?.data?.message || "Unable to load restaurants."));
    }
  }, [isSuperAdmin]);

  useEffect(() => {
    if (!restaurantId) {
      setLoading(false);
      return;
    }

    setLoading(true);
    getRestaurantProfile(restaurantId)
      .then((data) => {
        setRestaurant(data);
        setForm({ name: data.name, address: data.address });
        setError(null);
      })
      .catch((err) => setError(err?.response?.data?.message || "Unable to load restaurant profile."))
      .finally(() => setLoading(false));
  }, [restaurantId]);

  const handleSave = async () => {
    setLoading(true);
    try {
      await updateRestaurantProfile(restaurantId, form);
      setRestaurant((prev) => ({ ...prev, ...form }));
      setError(null);
      alert("Profile updated successfully.");
    } catch (err) {
      setError(err?.response?.data?.message || "Unable to update restaurant profile.");
    } finally {
      setLoading(false);
    }
  };

  return (
    <main className="page">
      <section className="section">
        <h1>Restaurant Profile</h1>
        {isSuperAdmin && (
          <div className="card">
            <label htmlFor="restaurant-select">Select restaurant:</label>
            <select
              id="restaurant-select"
              value={restaurantId ?? ""}
              onChange={(e) => setRestaurantId(Number(e.target.value) || null)}
            >
              <option value="">Choose a restaurant</option>
              {restaurants.map((restaurantItem) => (
                <option key={restaurantItem.id} value={restaurantItem.id}>
                  {restaurantItem.name}
                </option>
              ))}
            </select>
          </div>
        )}
        {!restaurantId && <p>You do not have a restaurant assigned.</p>}
        {loading && <p>Loading restaurant profile...</p>}
        {error && <p className="error">{error}</p>}
        {restaurant && (
          <div className="card">
            <label>
              Restaurant Name
              <input value={form.name} onChange={(e) => setForm((prev) => ({ ...prev, name: e.target.value }))} />
            </label>
            <label>
              Address
              <input value={form.address} onChange={(e) => setForm((prev) => ({ ...prev, address: e.target.value }))} />
            </label>
            <button className="primary-button" onClick={handleSave}>
              Save Profile
            </button>
          </div>
        )}
      </section>
    </main>
  );
}
