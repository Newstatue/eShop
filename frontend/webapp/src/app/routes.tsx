import App from "@/App";
import ProductPage from "../features/catalog/pages/ProductPage";
import { createBrowserRouter } from "react-router-dom";
import { withLoginEnforced } from "./oidc";

const ProtectedProductPage = withLoginEnforced(ProductPage, {
  onRedirecting: () => <div>Authenticating...</div>,
});

export const router = createBrowserRouter([
  {
    path: "/",
    element: <App />,
    children: [
      { index: true, element: <div>Home Page</div> },
      {
        path: "products",
        element: <ProtectedProductPage />, // /products requires authentication
      },
      { path: "about", element: <div>About Page</div> },
      { path: "contact", element: <div>Contact Page</div> },
    ],
  },
]);
