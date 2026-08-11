import { useEffect, useState } from "react";
import { getMyOrders } from "../api";

export default function OrdersPage() {
  const [orders, setOrders] = useState([]);

  useEffect(() => {
    getMyOrders().then(setOrders);
  }, []);

  return (
    <main className="page">
      <section className="section">
        <h1>My Orders</h1>
        {orders.length === 0 ? (
          <p>You have not placed any orders yet.</p>
        ) : (
          orders.map((order) => (
            <div key={order.id} className="card">
              <div className="order-row">
                <span>Order #{order.id}</span>
                <span>{order.status}</span>
              </div>
              <p>{new Date(order.createdAt).toLocaleString()}</p>
              <p>Total: ${order.totalPrice.toFixed(2)}</p>
              <ul>
                {order.orderItems.map((item) => (
                  <li key={item.id}>
                    {item.quantity} × {item.menuItem.name}
                  </li>
                ))}
              </ul>
            </div>
          ))
        )}
      </section>
    </main>
  );
}
