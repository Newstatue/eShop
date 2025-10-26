import App from '@/App';
import ProductPage from '../features/catalog/pages/ProductPage';
import { createBrowserRouter } from 'react-router-dom';
import { OidcSecure } from '@axa-fr/react-oidc';


export const router = createBrowserRouter([
    {
        path: "/",
        element: <App />, // 全局 layout
        children: [
            { index: true, element: <div>Home Page</div> }, // 默认首页
            {
                path: "products",
                element: (
                    <OidcSecure>
                        <ProductPage />
                    </OidcSecure>
                ), // /products 需要登录
            },
            { path: "about", element: <div>About Page</div> },
            { path: "contact", element: <div>Contact Page</div> },
            { path: "authentication/callback", element: <div>Authenticating...</div> },
            { path: "authentication/silent-callback", element: <div>Silent Authenticating...</div> },
        ],
    },
]);
