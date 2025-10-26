import React from 'react';
import ReactDOM from 'react-dom/client';
import './index.css';
import { RouterProvider } from 'react-router-dom';
import { router } from './app/routes';
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { ThemeProvider } from './components/theme-provider';
import { OidcProvider } from '@axa-fr/react-oidc';
import { authConfig } from './app/config/authConfig';

const configuration = {
  client_id: authConfig.clientId,
  redirect_uri: window.location.origin + '/authentication/callback',
  silent_redirect_uri: window.location.origin + '/authentication/silent-callback',
  scope: 'openid profile email offline_access',
  authority: authConfig.url + '/realms/' + authConfig.realm,
  service_worker_only: false,
  service_worker_relative_url: undefined,
};

const queryClient = new QueryClient();

ReactDOM.createRoot(document.getElementById('root')!).render(
  <React.StrictMode>
    <OidcProvider configuration={configuration}>
      <QueryClientProvider client={queryClient}>
        <ThemeProvider defaultTheme="dark" storageKey="eshop-theme">
          <RouterProvider router={router} />
        </ThemeProvider>
      </QueryClientProvider>
    </OidcProvider>
  </React.StrictMode>,
);
