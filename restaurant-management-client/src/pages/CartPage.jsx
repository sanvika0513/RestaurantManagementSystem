import { useEffect, useState } from "react";
import { getCart, updateCartItem, deleteCartItem, placeOrder } from "../api";

export default function CartPage() {
  const [items, setItems] = useState([]);
  const [loading, setLoading] = useState(true);

  const refreshCart = async () => {
    setLoading(true);
    const cartItems = await getCart();
    setItems(cartItems);
    setLoading(false);
  };

  useEffect(() => {
    refreshCart();
  }, []);

  const updateQuantity = async (itemId, quantity) => {
    await updateCartItem(itemId, { quantity });
    await refreshCart();
  };

  const removeItem = async (itemId) => {
    await deleteCartItem(itemId);
    await refreshCart();
  };

  const submitOrder = async () => {
    const restaurantIds = [...new Set(items.map((item) => item.menuItem?.restaurantId))].filter(Boolean);
    if (restaurantIds.length !== 1) {
      alert("Your cart must contain items from a single restaurant to place an order.");
      return;
    }

    const restaurantId = restaurantIds[0];
    if (!restaurantId) {
      return;
    }

    const orderPayload = {
      restaurantId,
      items: items.map((item) => ({ menuItemId: item.menuItemId, quantity: item.quantity })),
    };

    await placeOrder(orderPayload);
    alert("Order placed successfully.");
    await refreshCart();
  };

  if (loading) {
    return <main className="page">Loading cart…</main>;
  }

  return (
    <main className="page">
      <section className="section">
        <h1>Your Cart</h1>
        {items.length === 0 ? (
          <p>Your cart is empty.</p>
        ) : (
          <div className="list-grid">
            {items.map((item) => (
              <div key={item.id} className="card">
                <h2>{item.menuItem.name}</h2>
                <p>{item.menuItem.description}</p>
                <p>${(item.menuItem.price * item.quantity).toFixed(2)}</p>
                <div className="card-actions">
                  <input
                    type="number"
                    min="1"
                    value={item.quantity}
                    onChange={(e) => updateQuantity(item.id, Number(e.target.value))}
                  />
                  <button onClick={() => removeItem(item.id)}>Remove</button>
                </div>
              </div>
            ))}
          </div>
        )}
        {items.length > 0 && (
          <button className="primary-button" onClick={submitOrder}>
            Place Order
          </button>
        )}
      </section>
    </main>
  );
}
