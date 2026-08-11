import { createContext, useContext, useEffect, useMemo, useState } from "react";
import { useNavigate } from "react-router-dom";
import { login as apiLogin } from "./api";

const AuthContext = createContext(null);

export function AuthProvider({ children }) {
  const navigate = useNavigate();

  const [user, setUser] = useState(() => {
    const storedUser = localStorage.getItem("restaurant_user");

    try {
      return storedUser ? JSON.parse(storedUser) : null;
    } catch {
      localStorage.removeItem("restaurant_user");
      return null;
    }
  });

  const [token, setToken] = useState(() =>
    localStorage.getItem("restaurant_token")
  );

  const [error, setError] = useState(null);

  useEffect(() => {
    if (user) {
      localStorage.setItem("restaurant_user", JSON.stringify(user));
    } else {
      localStorage.removeItem("restaurant_user");
    }
  }, [user]);

  useEffect(() => {
    if (token) {
      localStorage.setItem("restaurant_token", token);
    } else {
      localStorage.removeItem("restaurant_token");
    }
  }, [token]);

  const login = async (credentials) => {
    setError(null);

    try {
      const response = await apiLogin(credentials);

      console.log("Login response:", response);

      if (!response || !response.token) {
        throw new Error("Login response did not contain a token.");
      }

      const responseUser = response.user || {};

      const normalizedUser = {
        ...responseUser,
        roles:
          responseUser.roles ??
          responseUser.Roles ??
          [],
      };

      setToken(response.token);
      setUser(normalizedUser);

      /*
       * Redirect based on role.
       */
      const roles = normalizedUser.roles || [];

      if (
        roles.includes("SuperAdmin") ||
        roles.includes("superadmin")
      ) {
        navigate("/admin/dashboard", { replace: true });
      } else if (
        roles.includes("RestaurantAdmin") ||
        roles.includes("restaurantadmin")
      ) {
        navigate("/admin/dashboard", { replace: true });
      } else {
        navigate("/dashboard", { replace: true });
      }

      return response;
    } catch (err) {
      console.error("Login error:", err);

      const message =
        err?.response?.data?.message ||
        err?.response?.data?.error ||
        err?.message ||
        "Unable to sign in.";

      setError(message);

      throw err;
    }
  };

  const logout = () => {
    setToken(null);
    setUser(null);
    setError(null);

    localStorage.removeItem("restaurant_token");
    localStorage.removeItem("restaurant_user");

    navigate("/login", { replace: true });
  };

  const value = useMemo(
    () => ({
      user,
      token,
      error,
      login,
      logout,
      isAuthenticated: Boolean(user && token),
    }),
    [user, token, error]
  );

  return (
    <AuthContext.Provider value={value}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const context = useContext(AuthContext);

  if (!context) {
    throw new Error("useAuth must be used within an AuthProvider");
  }

  return context;
}