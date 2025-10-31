import React from 'react';
import ReactDOM from 'react-dom/client';
import './index.css';
import { RouterProvider } from 'react-router-dom';
import { router } from './app/routes';
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { ThemeProvider, DEFAULT_THEME_STORAGE_KEY } from './components/theme-provider';
import { OidcProvider } from './app/oidc';
import { oidcEarlyInit } from 'oidc-spa/entrypoint';

const queryClient = new QueryClient();

(async () => {
  let shouldLoadApp = true;
  try {
    const result = oidcEarlyInit({
      freezeFetch: true,
      freezeXMLHttpRequest: true,
      freezeWebSocket: true,
      BASE_URL: import.meta.env.BASE_URL,
    });
    shouldLoadApp = result.shouldLoadApp;
  } catch (error) {
    // oidcEarlyInit already called, skip
    if (error instanceof Error && error.message.includes('Should be called only once')) {
      // Continue loading the app
    } else {
      throw error;
    }
  }

  if (!shouldLoadApp) {
    return;
  }

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
})();
