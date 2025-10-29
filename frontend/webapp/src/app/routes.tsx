import App from "@/App";
import ProductPage from "../features/catalog/pages/ProductPage";
import { createBrowserRouter } from "react-router-dom";
import { withLoginEnforced } from "./oidc";
import UserPage from "@/features/auth/pages/UserPage";

const ProtectedProductPage = withLoginEnforced(ProductPage, {
  onRedirecting: () => <div>认证中...</div>,
});

const ProtectedUserPage = withLoginEnforced(UserPage, {
  onRedirecting: () => <div>认证中...</div>,
});

export const router = createBrowserRouter([
  {
    path: "/",
    element: <App />,
    children: [
      { index: true, element: <div>Home Page</div> },
      {
        path: "products",
        element: <ProtectedProductPage />,
      },
      {
        path: "user",
        element: <ProtectedUserPage />,
      },
      { path: "about", element: <div>About Page</div> },
      { path: "contact", element: <div>Contact Page</div> },
    ],
  },
]);
