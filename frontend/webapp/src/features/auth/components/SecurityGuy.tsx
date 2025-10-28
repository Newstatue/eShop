import { ReactNode } from "react";
import { useOidc } from "@/app/oidc";
import Login from "./Login";

interface SecurityGuyProps {
  children: ReactNode;
}

export default function SecurityGuy({ children }: SecurityGuyProps) {
  const oidc = useOidc();

  return oidc.isUserLoggedIn ? <>{children}</> : <Login />;
}
