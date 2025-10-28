import React from 'react';
import ReactDOM from 'react-dom/client';
import './index.css';
import { RouterProvider } from 'react-router-dom';
import { router } from './app/routes';
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { ThemeProvider, DEFAULT_THEME_STORAGE_KEY } from './components/theme-provider';
import { OidcProvider } from './app/oidc';

const queryClient = new QueryClient();

ReactDOM.createRoot(document.getElementById('root')!).render(
  <React.StrictMode>
    <OidcProvider fallback={null}>
      <QueryClientProvider client={queryClient}>
        <ThemeProvider defaultTheme="light" storageKey={DEFAULT_THEME_STORAGE_KEY}>
          <RouterProvider router={router} />
        </ThemeProvider>
      </QueryClientProvider>
    </OidcProvider>
  </React.StrictMode>,
);
