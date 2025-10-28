import { useOidc } from "@/app/oidc";
import { Button } from "@/components/ui/button";
import {
  buildThemeQueryParams,
  useResolvedTheme,
} from "@/components/theme-provider";
import { useMemo } from "react";

export default function Login() {
  const oidc = useOidc();
  const resolvedTheme = useResolvedTheme();
  const extraQueryParams = useMemo(
    () => buildThemeQueryParams(resolvedTheme),
    [resolvedTheme],
  );

  if (oidc.isUserLoggedIn) {
    return null;
  }

  const handleLogin = () =>
    oidc.login({
      doesCurrentHrefRequiresAuth: false,
      extraQueryParams,
    });

  return (
    <div className="flex items-center justify-center gap-4 py-8">
      <Button onClick={handleLogin}>Login</Button>
    </div>
  );
}
