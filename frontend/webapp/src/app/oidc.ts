import { createReactOidc } from "oidc-spa/react";
import { buildThemeQueryParams } from "@/components/theme-provider";
import { authConfig } from "./config/authConfig";

const issuerUri = `${authConfig.url}/realms/${authConfig.realm}`;

export const {
  OidcProvider,
  useOidc,
  getOidc,
  withLoginEnforced,
  enforceLogin,
} = createReactOidc(async () => ({
  issuerUri,
  clientId: authConfig.clientId,
  scopes: ["profile", "email", "offline_access"],
  BASE_URL: import.meta.env.BASE_URL,
  extraQueryParams: () => buildThemeQueryParams(),
}));
