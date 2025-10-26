import { ReactNode } from "react";
import { useOidc } from "@axa-fr/react-oidc";
import Login from "./Login";

interface SecurityGuyProps {
  children: ReactNode;
}

export default function SecurityGuy({ children }: SecurityGuyProps) {
  try {
    const { isAuthenticated } = useOidc();
    return isAuthenticated ? <>{children}</> : <Login />;
  } catch (error) {
    // If OIDC is not initialized, show login
    console.error("OIDC not initialized:", error);
    return <Login />;
  }
}
