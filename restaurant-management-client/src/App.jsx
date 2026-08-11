import { BrowserRouter, Navigate, Route, Routes } from "react-router-dom";
import { AuthProvider } from "./AuthProvider";
import ProtectedRoute from "./components/ProtectedRoute";
import NavBar from "./components/NavBar";

import LoginPage from "./pages/LoginPage";
import RegisterPage from "./pages/RegisterPage";

import DashboardPage from "./pages/DashboardPage";
import RestaurantsPage from "./pages/RestaurantsPage";
import CartPage from "./pages/CartPage";
import OrdersPage from "./pages/OrdersPage";
import UsersPage from "./pages/UsersPage";

import AdminDashboardPage from "./pages/AdminDashboardPage";
import AdminMenuPage from "./pages/AdminMenuPage";
import AdminOrdersPage from "./pages/AdminOrdersPage";
import AdminProfilePage from "./pages/AdminProfilePage";

import "./App.css";

function App() {
  return (
    <BrowserRouter>
      <AuthProvider>
        <NavBar />

        <Routes>

          {/* =========================
              LOGIN & REGISTER
          ========================== */}

          <Route
            path="/login"
            element={<LoginPage />}
          />

          <Route
            path="/register"
            element={<RegisterPage />}
          />


          {/* =========================
              NORMAL AUTHENTICATED USERS
          ========================== */}

          <Route element={<ProtectedRoute />}>

            <Route
              path="/dashboard"
              element={<DashboardPage />}
            />

            <Route
              path="/restaurants"
              element={<RestaurantsPage />}
            />

            <Route
              path="/cart"
              element={<CartPage />}
            />

            <Route
              path="/orders"
              element={<OrdersPage />}
            />

          </Route>


          {/* =========================
              RESTAURANT ADMIN + SUPER ADMIN
          ========================== */}

          <Route
            element={
              <ProtectedRoute
                requiredRoles={["RestaurantAdmin", "SuperAdmin"]}
              />
            }
          >

            <Route
              path="/admin/dashboard"
              element={<AdminDashboardPage />}
            />

            <Route
              path="/admin/menu"
              element={<AdminMenuPage />}
            />

            <Route
              path="/admin/orders"
              element={<AdminOrdersPage />}
            />

            <Route
              path="/admin/profile"
              element={<AdminProfilePage />}
            />

          </Route>


          {/* =========================
              SUPER ADMIN ONLY
          ========================== */}

          <Route
            element={
              <ProtectedRoute
                requiredRoles={["SuperAdmin"]}
              />
            }
          >

            <Route
              path="/admin/users"
              element={<UsersPage />}
            />

          </Route>


          {/* =========================
              DEFAULT ROUTE
          ========================== */}

          <Route
            path="*"
            element={
              <Navigate
                to="/dashboard"
                replace
              />
            }
          />

        </Routes>

      </AuthProvider>
    </BrowserRouter>
  );
}

export default App;