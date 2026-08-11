import { Link } from "react-router-dom";
import { useAuth } from "../AuthProvider";

export default function NavBar() {
  const auth = useAuth();

  const isAdmin = auth.user?.roles?.some((role) =>
    ["RestaurantAdmin", "SuperAdmin"].includes(role)
  );

  return (
    <header className="nav-bar">
      <div className="nav-logo">
        <Link to="/dashboard">Restaurant Manager</Link>
      </div>

      <nav>
        <Link to="/restaurants">Restaurants</Link>

        {/* Cart and normal Orders are only for Normal Users */}
        {!isAdmin && <Link to="/cart">Cart</Link>}
        {!isAdmin && <Link to="/orders">Orders</Link>}

        {/* Admin options */}
        {isAdmin && (
          <Link to="/admin/dashboard">Admin Dashboard</Link>
        )}

        {isAdmin && (
          <Link to="/admin/menu">Menu</Link>
        )}

        {isAdmin && (
          <Link to="/admin/orders">Orders</Link>
        )}

        {isAdmin && (
          <Link to="/admin/profile">Profile</Link>
        )}

        {/* Super Admin only */}
        {auth.user?.roles?.includes("SuperAdmin") && (
          <Link to="/admin/users">Users</Link>
        )}
      </nav>

      <div className="nav-actions">
        {auth.user ? (
          <>
            <span>{auth.user.userName}</span>

            <button onClick={auth.logout}>
              Sign Out
            </button>
          </>
        ) : (
          <Link to="/login">Sign In</Link>
        )}
      </div>
    </header>
  );
}