import { useState } from "react";
import { useAuth } from "../AuthProvider";
import { Link } from "react-router-dom";

export default function LoginPage() {
  const auth = useAuth();

  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);

  const handleSubmit = async (event) => {
    event.preventDefault();

    setIsSubmitting(true);

    try {
      await auth.login({
        userName: username,
        password,
      });
    } catch (error) {
      console.error("Login failed:", error);
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <main className="page">
      <section className="section auth-card">
        <h1>Sign in</h1>

        <p>
          Use your restaurant credentials to access the dashboard.
        </p>

        <form onSubmit={handleSubmit}>
          <label htmlFor="username">
            Username
          </label>

          <input
            id="username"
            type="text"
            value={username}
            onChange={(e) => setUsername(e.target.value)}
            required
          />

          <label htmlFor="password">
            Password
          </label>

          <input
            id="password"
            type="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            required
          />

          {auth.error && (
            <p className="error">
              {auth.error}
            </p>
          )}

          <button type="submit" disabled={isSubmitting}>
            {isSubmitting ? "Signing in..." : "Sign in"}
          </button>
        </form>

        <p className="small-text">
          New user?{" "}
          <Link to="/register">
            Create an account
          </Link>
        </p>
      </section>
    </main>
  );
}