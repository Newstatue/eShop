import { useOidc } from "@/app/oidc";
import { Button } from "@/components/ui/button";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import { Separator } from "@/components/ui/separator";
import { LogOut, UserCog, ArrowLeft } from "lucide-react";
import { authConfig } from "@/app/config/authConfig";
import { useNavigate } from "react-router-dom";

export default function UserPage() {
  const oidc = useOidc();
  const decoded = oidc.decodedIdToken;
  const navigate = useNavigate();

  const username =
    decoded?.preferred_username ?? decoded?.name ?? "(未知用户)";
  const email = decoded?.email ?? "未绑定邮箱";
  const roles = (decoded as any)?.realm_access?.roles ?? [];
  const realm = decoded?.iss?.split("/realms/")[1] ?? authConfig.realm;

  const handleOpenAccount = () => {
    window.open(
      `${authConfig.url}/realms/${authConfig.realm}/account`,
      "_blank"
    );
  };

  const handleLogout = async () => {
    if (oidc.isUserLoggedIn) {
      await oidc.logout({
        redirectTo: "specific url",
        url: window.location.origin,
      });
    }
  };

  const handleGoBack = () => {
    navigate(-1); // ✅ 返回上一页
  };

  return (
    <section className="relative min-h-screen overflow-hidden bg-linear-to-b from-muted/30 via-background/60 to-background pt-24 pb-10">
      {/* 宽度和产品列表页对齐 */}
      <div className="mx-auto flex w-full max-w-6xl flex-col gap-10 px-4 sm:px-6 lg:px-8">
        {/* 顶部导航栏 */}
        <header className="flex items-center justify-between border-b border-border/50 pb-4">
          <h1 className="text-3xl font-bold tracking-tight">个人中心</h1>
          <div className="flex gap-3">
            <Button variant="outline" onClick={handleGoBack}>
              <ArrowLeft className="mr-2 h-4 w-4" />
              返回上一页
            </Button>
          </div>
        </header>

        {/* 用户信息 */}
        <main className="flex flex-col gap-8">
          <div className="flex items-center gap-6">
            <Avatar className="h-20 w-20">
              <AvatarImage src="/default-avatar.png" alt={username} />
              <AvatarFallback>
                {username ? username[0].toUpperCase() : "U"}
              </AvatarFallback>
            </Avatar>

            <div>
              <h2 className="text-2xl font-bold">{username}</h2>
              <p className="text-muted-foreground">{email}</p>
            </div>
          </div>

          <Separator />

          <div className="space-y-2 text-sm">
            <p>
              <strong>Realm：</strong> {realm}
            </p>
            {roles.length > 0 && (
              <p>
                <strong>角色：</strong> {roles.join(", ")}
              </p>
            )}
          </div>

          <Separator />

          <div className="flex flex-col sm:flex-row gap-3">
            <Button
              variant="secondary"
              onClick={handleOpenAccount}
              className="flex-1"
            >
              <UserCog className="mr-2 h-4 w-4" />
              管理 Keycloak 账户
            </Button>

            <Button
              variant="destructive"
              onClick={handleLogout}
              className="flex-1"
            >
              <LogOut className="mr-2 h-4 w-4" />
              退出登录
            </Button>
          </div>
        </main>
      </div>
    </section>
  );
}
