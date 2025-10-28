import { useEffect, useState } from "react";
import { getOidc, useOidc } from "@/app/oidc";

export default function SecurePage() {
  const oidc = useOidc();
  const [accessToken, setAccessToken] = useState<string>();

  useEffect(() => {
    let unsubscribe: (() => void) | undefined;
    let active = true;

    (async () => {
      const instance = await getOidc();
      if (!active || !instance.isUserLoggedIn) {
        return;
      }

      const updateFromTokens = async () => {
        const tokens = await instance.getTokens();
        if (active) {
          setAccessToken(tokens.accessToken);
        }
      };

      await updateFromTokens();

      const subscription = instance.subscribeToTokensChange(({ accessToken }) => {
        if (active) {
          setAccessToken(accessToken);
        }
      });

      unsubscribe = subscription.unsubscribe;
    })();

    return () => {
      active = false;
      unsubscribe?.();
    };
  }, [oidc.isUserLoggedIn]);

  if (!oidc.isUserLoggedIn) {
    return <div>Loading or not authenticated...</div>;
  }

  return (
    <div className="space-y-4 rounded-lg border p-6 shadow-sm">
      <p>Welcome, {oidc.decodedIdToken.email ?? oidc.decodedIdToken.preferred_username}</p>
      <p className="break-all text-xs text-muted-foreground">
        Your access token (keep it safe!): {accessToken ?? "Loading..."}
      </p>
    </div>
  );
}
