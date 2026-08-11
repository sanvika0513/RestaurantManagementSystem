import { Navigate, Outlet, useLocation } from "react-router-dom";
import { useAuth } from "../AuthProvider";

export default function ProtectedRoute({ requiredRoles = [] }) {
  const auth = useAuth();
  const location = useLocation();

  if (!auth.isAuthenticated) {
    return <Navigate to="/login" state={{ from: location }} replace />;
  }

  if (requiredRoles.length > 0 && !requiredRoles.some((role) => auth.user?.roles?.includes(role))) {
    return (
      <div className="page">
        <h2>Unauthorized</h2>
        <p>You do not have permission to view this page.</p>
      </div>
    );
  }

  return <Outlet />;
}
