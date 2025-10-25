import App from '@/App';
import ProductPage from '../features/catalog/pages/ProductPage';
import { createBrowserRouter } from 'react-router-dom';

export const router = createBrowserRouter([
    {
        path: "/",
        element: <App />,       // 全局 layout
        children: [
            { index: true, element: <div>Home Page</div> },  // 默认首页
            { path: "products", element: <ProductPage /> },  // /products 会渲染到 <Outlet />
        ],
    },
]);
