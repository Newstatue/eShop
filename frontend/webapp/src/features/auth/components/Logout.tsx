import { useOidc } from "@/app/oidc";
import { Button } from "@/components/ui/button";

export default function Logout() {
  const oidc = useOidc();

  if (!oidc.isUserLoggedIn) {
    return null;
  }

  const handleLogout = async () => {
    await oidc.logout({
      redirectTo: "specific url",
      url: window.location.origin, // ✅ 登出后返回首页
    });
  };

  return (
    <div className="flex items-center justify-center gap-4 py-8">
      <Button variant="destructive" onClick={handleLogout}>
        Logout
      </Button>
    </div>
  );
}
