import { useOidc, useOidcAccessToken, useOidcUser } from "@axa-fr/react-oidc";

export default function SecurePage() {
  const { isAuthenticated } = useOidc();
  const { accessToken } = useOidcAccessToken();
  const { oidcUser } = useOidcUser();

  if (!isAuthenticated) {
    return <div>Loading or not authenticated...</div>;
  }

  return (
    <div className="secure-page">
      <p>Welcome, {oidcUser?.email}</p>
      <p>Your access token (keep it safe!): {accessToken}</p>
    </div>
  );
}
