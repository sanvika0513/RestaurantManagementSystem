import { useEffect, useState } from "react";
import { useAuth } from "../AuthProvider";
import {
  getRestaurants,
  getRestaurantMenu,
  addToCart,
} from "../api";

export default function RestaurantsPage() {
  const auth = useAuth();

  const [restaurants, setRestaurants] = useState([]);
  const [selected, setSelected] = useState(null);
  const [menu, setMenu] = useState([]);
  const [quantity, setQuantity] = useState(1);

  // Get logged-in user's roles
  const roles = auth.user?.roles || [];

  const isUser = roles.includes("User");
  const isRestaurantAdmin = roles.includes("RestaurantAdmin");
  const isSuperAdmin = roles.includes("SuperAdmin");

  useEffect(() => {
    const loadRestaurants = async () => {
      try {
        const data = await getRestaurants();

        // Restaurant Admin should see ONLY their own restaurant
        if (isRestaurantAdmin && !isSuperAdmin) {
          const userRestaurantId =
            auth.user?.restaurantId ??
            auth.user?.RestaurantId;

          console.log("Logged-in user:", auth.user);
          console.log("Restaurant ID:", userRestaurantId);

          if (userRestaurantId !== null && userRestaurantId !== undefined) {
            const filteredRestaurants = data.filter(
              (restaurant) =>
                Number(restaurant.id) === Number(userRestaurantId)
            );

            setRestaurants(filteredRestaurants);

            // Automatically load the admin's restaurant menu
            if (filteredRestaurants.length > 0) {
              const restaurant = filteredRestaurants[0];

              setSelected(restaurant);

              try {
                const menuItems = await getRestaurantMenu(
                  restaurant.id
                );

                setMenu(menuItems);
              } catch (error) {
                console.error(
                  "Failed to load restaurant menu:",
                  error
                );

                setMenu([]);
              }
            }
          } else {
            console.error(
              "RestaurantAdmin does not have RestaurantId."
            );

            setRestaurants([]);
            setSelected(null);
            setMenu([]);
          }
        } else {
          // Normal User and SuperAdmin can see all restaurants
          setRestaurants(data);
        }
      } catch (error) {
        console.error(
          "Failed to load restaurants:",
          error
        );

        setRestaurants([]);
      }
    };

    if (auth.user) {
      loadRestaurants();
    }
  }, [auth.user, isRestaurantAdmin, isSuperAdmin]);

  const loadMenu = async (restaurant) => {
    try {
      setSelected(restaurant);

      const menuItems = await getRestaurantMenu(
        restaurant.id
      );

      setMenu(menuItems);
    } catch (error) {
      console.error(
        "Failed to load restaurant menu:",
        error
      );

      setMenu([]);
    }
  };

  const addItem = async (item) => {
    if (!isUser) {
      alert(
        "Only normal users can add items to the cart."
      );
      return;
    }

    try {
      await addToCart({
        menuItemId: item.id,
        quantity: quantity,
      });

      alert(`${item.name} added to cart`);
    } catch (error) {
      console.error(
        "Failed to add item to cart:",
        error
      );

      alert("Unable to add item to cart.");
    }
  };

  return (
    <main>
      <h1>Restaurants</h1>

      {restaurants.length === 0 && (
        <p className="small-text">
          No restaurants available.
        </p>
      )}

      <div className="list-grid">
        {restaurants.map((restaurant) => (
          <button
            key={restaurant.id}
            className="card"
            onClick={() => loadMenu(restaurant)}
          >
            <h2>{restaurant.name}</h2>

            <p>{restaurant.address}</p>
          </button>
        ))}
      </div>

      {selected && (
        <section className="section">
          <h2>
            Menu for {selected.name}
          </h2>

          <div className="list-grid">
            {menu.map((item) => (
              <div
                key={item.id}
                className="card"
              >
                <h3>{item.name}</h3>

                <p>
                  {item.description}
                </p>

                <p>
                  $
                  {Number(item.price).toFixed(2)}
                </p>

                <div className="card-actions">
                  <input
                    type="number"
                    min="1"
                    value={quantity}
                    onChange={(e) =>
                      setQuantity(
                        Math.max(
                          1,
                          Number(e.target.value)
                        )
                      )
                    }
                  />

                  <button
                    onClick={() =>
                      addItem(item)
                    }
                    disabled={!isUser}
                  >
                    Add to cart
                  </button>

                  {!isUser && (
                    <p className="small-text">
                      Only users can order.
                    </p>
                  )}
                </div>
              </div>
            ))}
          </div>
        </section>
      )}
    </main>
  );
}