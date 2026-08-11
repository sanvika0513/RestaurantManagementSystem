import axios from "axios";

const baseURL = "https://restaurantmanagementsystem-qs1a.onrender.com/api";

const getToken = () => localStorage.getItem("restaurant_token");

const api = axios.create({
  baseURL,
  headers: {
    "Content-Type": "application/json",
  },
});
api.interceptors.request.use((config) => {
  const token = getToken();
  if (token && config.headers) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

export const login = (credentials) => api.post("/auth/login", credentials).then((res) => res.data);
export const getRestaurants = () => api.get("/restaurants").then((res) => res.data);
export const getRestaurantMenu = (restaurantId) => api.get(`/menu/restaurant/${restaurantId}`).then((res) => res.data);
export const getAdminMenuItems = (restaurantId) => api.get(`/menu/restaurant/${restaurantId}/all`).then((res) => res.data);
export const createMenuItem = (payload) => api.post("/menu", payload).then((res) => res.data);
export const updateMenuItem = (id, payload) => api.put(`/menu/${id}`, payload).then((res) => res.data);
export const deleteMenuItem = (id) => api.delete(`/menu/${id}`).then((res) => res.data);
export const getCart = () => api.get("/cart").then((res) => res.data);
export const addToCart = (payload) => api.post("/cart", payload).then((res) => res.data);
export const updateCartItem = (id, payload) => api.put(`/cart/${id}`, payload);
export const deleteCartItem = (id) => api.delete(`/cart/${id}`);
export const placeOrder = (payload) => api.post("/orders", payload).then((res) => res.data);
export const getMyOrders = () => api.get("/orders/my").then((res) => res.data);
export const getAllRestaurants = () => api.get("/restaurants/all").then((res) => res.data);
export const getRestaurantDashboard = (restaurantId) => api.get(`/restaurants/dashboard?restaurantId=${restaurantId}`).then((res) => res.data);
export const getRestaurantProfile = (restaurantId) => api.get(`/restaurants/${restaurantId}`).then((res) => res.data);
export const updateRestaurantProfile = (restaurantId, payload) =>
  api.put(`/restaurants/${restaurantId}/profile`, { id: restaurantId, ...payload }).then((res) => res.data);
export const getRestaurantOrders = (restaurantId) => api.get(`/orders/restaurant/${restaurantId}`).then((res) => res.data);
export const updateOrderStatus = (orderId, payload) => api.put(`/orders/${orderId}/status`, payload).then((res) => res.data);
export const getUsers = () => api.get("/users").then((res) => res.data);
