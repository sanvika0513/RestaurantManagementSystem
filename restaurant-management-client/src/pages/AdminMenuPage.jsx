import { useEffect, useState } from "react";
import { useAuth } from "../AuthProvider";
import { createMenuItem, deleteMenuItem, getAdminMenuItems, getAllRestaurants, getRestaurantProfile, updateMenuItem } from "../api";

export default function AdminMenuPage() {
  const auth = useAuth();
  const isSuperAdmin = auth.user?.roles?.includes("SuperAdmin");
  const [restaurantId, setRestaurantId] = useState(auth.user?.restaurantId ?? null);
  const [restaurants, setRestaurants] = useState([]);
  const [restaurant, setRestaurant] = useState(null);
  const [menuItems, setMenuItems] = useState([]);
  const [newItem, setNewItem] = useState({ name: "", description: "", price: 0, isAvailable: true });
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    if (isSuperAdmin) {
      getAllRestaurants()
        .then(setRestaurants)
        .catch((err) => setError(err?.response?.data?.message || "Unable to load restaurants."));
    }
  }, [isSuperAdmin]);

  useEffect(() => {
    if (!restaurantId) return;

    setLoading(true);
    Promise.all([getRestaurantProfile(restaurantId), getAdminMenuItems(restaurantId)])
      .then(([restaurantData, items]) => {
        setRestaurant(restaurantData);
        setMenuItems(items);
        setError(null);
      })
      .catch((err) => setError(err?.response?.data?.message || "Unable to load menu."))
      .finally(() => setLoading(false));
  }, [restaurantId, isSuperAdmin]);

  const refresh = async () => {
    setLoading(true);
    try {
      const [restaurantData, items] = await Promise.all([
        getRestaurantProfile(restaurantId),
        getAdminMenuItems(restaurantId)
      ]);
      setRestaurant(restaurantData);
      setMenuItems(items);
      setError(null);
    } catch (err) {
      setError(err?.response?.data?.message || "Unable to load menu.");
    } finally {
      setLoading(false);
    }
  };

  const handleCreate = async () => {
    try {
      await createMenuItem({
        ...newItem,
        restaurantId,
      });
      setNewItem({ name: "", description: "", price: 0, isAvailable: true });
      await refresh();
    } catch (err) {
      setError(err?.response?.data?.message || "Unable to create menu item.");
    }
  };

  const handleToggleAvailability = async (item) => {
    try {
      await updateMenuItem(item.id, { ...item, isAvailable: !item.isAvailable });
      await refresh();
    } catch (err) {
      setError(err?.response?.data?.message || "Unable to update item.");
    }
  };

  const handleDelete = async (item) => {
    try {
      await deleteMenuItem(item.id);
      await refresh();
    } catch (err) {
      setError(err?.response?.data?.message || "Unable to delete item.");
    }
  };

  if (!restaurantId) {
    return (
      <main className="page">
        <section className="section">
          <h1>Admin Menu Management</h1>
          {isSuperAdmin ? (
            <p>Select a restaurant from the admin dashboard or refresh the page after selection.</p>
          ) : (
            <p>You must belong to a restaurant to manage menu items.</p>
          )}
        </section>
      </main>
    );
  }

  return (
    <main className="page">
      <section className="section">
        <h1>Manage Menu for {restaurant?.name ?? "Your Restaurant"}</h1>
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
        {loading && <p>Loading menu manager...</p>}
        {error && <p className="error">{error}</p>}

        <div className="card">
          <h2>Add New Item</h2>
          <label>
            Name
            <input
              value={newItem.name}
              onChange={(e) => setNewItem((prev) => ({ ...prev, name: e.target.value }))}
            />
          </label>
          <label>
            Description
            <input
              value={newItem.description}
              onChange={(e) => setNewItem((prev) => ({ ...prev, description: e.target.value }))}
            />
          </label>
          <label>
            Price
            <input
              type="number"
              min="0"
              step="0.01"
              value={newItem.price}
              onChange={(e) => setNewItem((prev) => ({ ...prev, price: Number(e.target.value) }))}
            />
          </label>
          <label>
            Available
            <input
              type="checkbox"
              checked={newItem.isAvailable}
              onChange={(e) => setNewItem((prev) => ({ ...prev, isAvailable: e.target.checked }))}
            />
          </label>
          <button className="primary-button" onClick={handleCreate}>
            Create Menu Item
          </button>
        </div>

        <div className="list-grid">
          {menuItems.map((item) => (
            <div key={item.id} className="card">
              <h3>{item.name}</h3>
              <p>{item.description}</p>
              <p>${item.price.toFixed(2)}</p>
              <p>Status: {item.isAvailable ? "Available" : "Unavailable"}</p>
              <div className="card-actions">
                <button onClick={() => handleToggleAvailability(item)}>
                  {item.isAvailable ? "Mark Unavailable" : "Mark Available"}
                </button>
                <button onClick={() => handleDelete(item)}>Delete</button>
              </div>
            </div>
          ))}
        </div>
      </section>
    </main>
  );
}
