import { useAuth } from "../AuthProvider";

export default function DashboardPage() {
  const auth = useAuth();

  return (
    <main className="page">
      <section className="section">
        <h1>Welcome, {auth.user?.userName}</h1>
        <p>Role: {auth.user?.roles?.join(", ")}</p>
        <p>Your restaurant ID: {auth.user?.restaurantId ?? "None"}</p>
        <p>
          Use the admin panel links when you need to manage menu items, review restaurant orders,
          or update restaurant profile details.
        </p>
      </section>
    </main>
  );
}
