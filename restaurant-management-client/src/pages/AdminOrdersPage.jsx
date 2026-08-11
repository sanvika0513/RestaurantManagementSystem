import { useEffect, useState } from "react";
import {
  getAllRestaurants,
  getRestaurantOrders,
  updateOrderStatus,
} from "../api";
import { useAuth } from "../AuthProvider";

export default function AdminOrdersPage() {
  const auth = useAuth();

  const isSuperAdmin =
    auth.user?.roles?.includes("SuperAdmin");

  const [restaurants, setRestaurants] = useState([]);
  const [restaurantId, setRestaurantId] = useState(
    auth.user?.restaurantId ?? null
  );
  const [orders, setOrders] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  // ============================================================
  // LOAD RESTAURANTS FOR SUPER ADMIN
  // ============================================================

  useEffect(() => {
    if (isSuperAdmin) {
      getAllRestaurants()
        .then(setRestaurants)
        .catch((err) =>
          setError(
            err?.response?.data?.message ||
              "Unable to load restaurants."
          )
        );
    }
  }, [isSuperAdmin]);

  // ============================================================
  // LOAD ORDERS
  // ============================================================

  useEffect(() => {
    if (!restaurantId) {
      setLoading(false);
      return;
    }

    setLoading(true);

    getRestaurantOrders(restaurantId)
      .then(setOrders)
      .catch((err) =>
        setError(
          err?.response?.data?.message ||
            "Unable to load orders."
        )
      )
      .finally(() => setLoading(false));
  }, [restaurantId]);

  // ============================================================
  // UPDATE ORDER STATUS
  // ============================================================

  const handleStatusChange = async (orderId, status) => {
    try {
      await updateOrderStatus(orderId, { status });

      setOrders((prev) =>
        prev.map((order) =>
          order.id === orderId
            ? { ...order, status }
            : order
        )
      );
    } catch (err) {
      setError(
        err?.response?.data?.message ||
          "Unable to update order status."
      );
    }
  };

  // ============================================================
  // PAGE
  // ============================================================

  return (
    <main className="page">
      <section className="section">
        <h1>Restaurant Orders</h1>

        {/* ====================================================
            RESTAURANT SELECTOR - SUPER ADMIN ONLY
        ==================================================== */}

        {isSuperAdmin && (
          <div className="card">
            <label htmlFor="restaurant-select">
              Select restaurant:
            </label>

            <select
              id="restaurant-select"
              value={restaurantId ?? ""}
              onChange={(e) =>
                setRestaurantId(
                  Number(e.target.value) || null
                )
              }
            >
              <option value="">
                Choose a restaurant
              </option>

              {restaurants.map((restaurant) => (
                <option
                  key={restaurant.id}
                  value={restaurant.id}
                >
                  {restaurant.name}
                </option>
              ))}
            </select>
          </div>
        )}

        {/* ====================================================
            MESSAGES
        ==================================================== */}

        {!restaurantId && (
          <p>You do not have a restaurant assigned.</p>
        )}

        {loading && <p>Loading orders...</p>}

        {error && (
          <p className="form-error">
            {error}
          </p>
        )}

        {!loading && orders.length === 0 && restaurantId && (
          <p>
            No orders found for this restaurant.
          </p>
        )}

        {/* ====================================================
            ORDERS
        ==================================================== */}

        <div className="list-grid">
          {orders.map((order) => (
            <article
              className="card"
              key={order.id}
            >
              <h2>Order #{order.id}</h2>

              <p>
                Status: {order.status}
              </p>

              <p>
                Total: $
                {order.totalPrice.toFixed(2)}
              </p>

              <p>
                Created:{" "}
                {new Date(
                  order.createdAt
                ).toLocaleString()}
              </p>

              {/* ==================================================
                  ORDER ITEMS
              ================================================== */}

              <ul>
                {order.orderItems.map((item) => (
                  <li key={item.id}>
                    {item.quantity} ×{" "}
                    {item.menuItem?.name}
                  </li>
                ))}
              </ul>

              {/* ==================================================
                  STATUS BUTTONS
                  
                  SUPER ADMIN:
                  - Can view status
                  - Cannot update status
                  - Buttons are hidden

                  RESTAURANT ADMIN:
                  - Can update status
                  - Buttons are visible
              ================================================== */}

              {!isSuperAdmin &&
                order.status !== "Completed" && (
                  <div className="card-actions">
                    <button
                      onClick={() =>
                        handleStatusChange(
                          order.id,
                          "Confirmed"
                        )
                      }
                    >
                      Confirm
                    </button>

                    <button
                      onClick={() =>
                        handleStatusChange(
                          order.id,
                          "InPreparation"
                        )
                      }
                    >
                      In Preparation
                    </button>

                    <button
                      onClick={() =>
                        handleStatusChange(
                          order.id,
                          "Ready"
                        )
                      }
                    >
                      Ready
                    </button>

                    <button
                      onClick={() =>
                        handleStatusChange(
                          order.id,
                          "Completed"
                        )
                      }
                    >
                      Complete
                    </button>
                  </div>
                )}
            </article>
          ))}
        </div>
      </section>
    </main>
  );
}