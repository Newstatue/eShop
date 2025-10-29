import {
  KeyboardEvent,
  use,
  useCallback,
  useEffect,
  useMemo,
  useRef,
  useState,
} from "react";
import ReactMarkdown from "react-markdown";
import { Bot, Send, ShoppingCart, User, X } from "lucide-react";

import { useSupportProducts } from "../hooks/useProducts";

import { Button } from "@/components/ui/button";
import { Card } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { ScrollArea } from "@/components/ui/scroll-area";
import { Spinner } from "@/components/ui/spinner";
import { Sheet, SheetClose, SheetContent } from "@/components/ui/sheet";
import { FloatingDock } from "@/components/ui/floating-dock";
import {
  buildThemeQueryParams,
  useResolvedTheme,
} from "@/components/theme-provider";
import { useOidc } from "@/app/oidc";
import { cn } from "@/lib/utils";

interface SupportMessage {
  id: string;
  role: "user" | "assistant";
  content: string;
  timestamp: Date;
}

type SupportView = "cart" | "assistant";

export interface AISupportProps {
  maxHeight?: number | string;
  minHeight?: number | string;
}

const TEXT = {
  initialMessage: "你好，我是 AI 助手，有什么可以帮你？",
  spinner: "思考中...",
  cartTitle: "购物车",
  assistantTitle: "AI 助手",
  assistantSubtitle: "随时为你解答",
  cartSubtitleLoggedIn: "最近添加的商品会出现在这里",
  cartSubtitleLoggedOut: "登录后即可查看购物车",
  cartPlaceholder: "你的购物车暂时为空，去看看其他好物吧。",
  cartHelper: "我们会为你推荐适合的商品搭配。",
  cartLoginPrompt: "请先登录以查看购物车内容。",
  loginButton: "立即登录",
  inputPlaceholder: "请输入问题或需求...",
  userPageTitle: "用户中心",
};

const toCssSize = (
  value: number | string | undefined,
  fallback: string,
) => (typeof value === "number" ? `${value}px` : value ?? fallback);

export default function AISupport({
  maxHeight,
  minHeight,
}: AISupportProps) {
  const oidc = useOidc();
  const resolvedTheme = useResolvedTheme();

  const [isOpen, setIsOpen] = useState(false);
  const [activeView, setActiveView] =
    useState<SupportView>("assistant");
  const [isMobile, setIsMobile] = useState(false);

  const [messages, setMessages] = useState<SupportMessage[]>([
    {
      id: "1",
      role: "assistant",
      content: TEXT.initialMessage,
      timestamp: new Date(),
    },
  ]);
  const [inputValue, setInputValue] = useState("");
  const [currentQuery, setCurrentQuery] = useState("");
  const messagesEndRef = useRef<HTMLDivElement>(null);

  const { data: answer, isLoading } = useSupportProducts(currentQuery);

  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: "smooth" });
  }, [messages, isLoading]);

  useEffect(() => {
    if (answer && currentQuery) {
      const newMessage: SupportMessage = {
        id: Date.now().toString(),
        role: "assistant",
        content: answer,
        timestamp: new Date(),
      };
      setMessages((prev) => [...prev, newMessage]);
      setCurrentQuery("");
    }
  }, [answer, currentQuery]);

  useEffect(() => {
    const mq = window.matchMedia("(max-width: 640px)");
    const update = () => setIsMobile(mq.matches);
    update();
    mq.addEventListener("change", update);
    return () => mq.removeEventListener("change", update);
  }, []);

  const handleSendMessage = () => {
    if (!inputValue.trim() || isLoading) return;
    const userMessage: SupportMessage = {
      id: Date.now().toString(),
      role: "user",
      content: inputValue,
      timestamp: new Date(),
    };
    setMessages((prev) => [...prev, userMessage]);
    setCurrentQuery(inputValue);
    setInputValue("");
  };

  const handleKeyDown = (event: KeyboardEvent<HTMLInputElement>) => {
    if (event.key === "Enter" && !event.shiftKey) {
      event.preventDefault();
      handleSendMessage();
    }
  };

  const handleDockSelect = useCallback(
    (view: SupportView) => {
      setIsOpen((prevOpen) =>
        activeView === view ? !prevOpen : true,
      );
      if (activeView !== view) {
        setActiveView(view);
      }
    },
    [activeView],
  );

  const handleLogin = () => {
    oidc.login?.({
      doesCurrentHrefRequiresAuth: false,
      extraQueryParams: buildThemeQueryParams(resolvedTheme),
    });
  };

  const dockItems = useMemo(
    () => [
      {
        title: TEXT.cartTitle,
        icon: (
          <ShoppingCart className="h-5 w-5 text-neutral-600 dark:text-neutral-200" />
        ),
        onClick: () => handleDockSelect("cart"),
      },
      {
        title: TEXT.assistantTitle,
        icon: (
          <Bot className="h-5 w-5 text-neutral-600 dark:text-neutral-200" />
        ),
        onClick: () => handleDockSelect("assistant"),
      },
      {
        title: TEXT.userPageTitle,
        icon: (
          <User className="h-5 w-5 text-neutral-600 dark:text-neutral-200" />
        ),
        onClick: () => {
          window.location.href = "/user"; // ✅ 直接跳转用户页面
        },
      },
    ],
    [handleDockSelect],
  );

  const isAssistantView = activeView === "assistant";

  const headerIcon = isAssistantView ? (
    <Bot className="h-5 w-5 text-neutral-600 dark:text-neutral-200" />
  ) : (
    <ShoppingCart className="h-5 w-5 text-neutral-600 dark:text-neutral-200" />
  );

  const headerTitle = isAssistantView
    ? TEXT.assistantTitle
    : TEXT.cartTitle;

  const headerSubtitle = isAssistantView
    ? TEXT.assistantSubtitle
    : oidc.isUserLoggedIn
      ? TEXT.cartSubtitleLoggedIn
      : TEXT.cartSubtitleLoggedOut;

  const body = isAssistantView ? (
    <ScrollArea className="flex h-full w-full flex-1">
      <div className="relative flex-1">
        <div className="pointer-events-none absolute inset-x-4 top-0 h-3 bg-gradient-to-b from-background via-background/60 to-transparent sm:inset-x-4 sm:h-4" />
        <div className="pointer-events-none absolute inset-x-4 bottom-0 h-3 bg-gradient-to-t from-background via-background/60 to-transparent sm:inset-x-4 sm:h-4" />
        <div className="space-y-2.5 px-4 py-3 sm:px-4 sm:py-4">
          {messages.map((message) => (
            <div
              key={message.id}
              className={`flex ${message.role === "user" ? "justify-end" : "justify-start"}`}
            >
              <div
                className={`max-w-[85%] rounded-2xl px-3.5 py-2 text-sm shadow-sm ${message.role === "user"
                  ? "bg-primary text-primary-foreground"
                  : "bg-muted text-foreground"
                  }`}
              >
                {message.role === "assistant" ? (
                  <div className="prose prose-sm max-w-none dark:prose-invert prose-p:my-1.5 prose-p:last:mb-0 prose-ul:my-1 prose-ul:ml-4 prose-ol:my-1 prose-ol:ml-4 prose-li:my-1 prose-strong:font-semibold prose-code:rounded prose-code:bg-muted-foreground/10 prose-code:px-1 prose-code:py-0.5">
                    <ReactMarkdown>{message.content}</ReactMarkdown>
                  </div>
                ) : (
                  <p className="leading-relaxed">{message.content}</p>
                )}
                <p className="mt-1 text-[11px] text-muted-foreground/80">
                  {message.timestamp.toLocaleTimeString("zh-CN", {
                    hour: "2-digit",
                    minute: "2-digit",
                  })}
                </p>
              </div>
            </div>
          ))}
          {isLoading && (
            <div className="flex justify-start">
              <div className="flex max-w-[80%] items-center gap-2 rounded-2xl bg-muted px-3 py-1.5 text-sm text-muted-foreground shadow-sm">
                <Spinner className="h-4 w-4" />
                <span>{TEXT.spinner}</span>
              </div>
            </div>
          )}
          <div ref={messagesEndRef} />
        </div>
      </div>
    </ScrollArea>
  ) : (
    <div className="flex h-full flex-col items-center justify-center gap-3 px-6 text-center text-sm text-muted-foreground">
      {oidc.isUserLoggedIn ? (
        <>
          <p>{TEXT.cartPlaceholder}</p>
          <p className="text-xs text-muted-foreground/80">
            {TEXT.cartHelper}
          </p>
        </>
      ) : (
        <>
          <p>{TEXT.cartLoginPrompt}</p>
          <Button onClick={handleLogin} variant="default" className="mt-2">
            {TEXT.loginButton}
          </Button>
        </>
      )}
    </div>
  );

  const footerSection = isAssistantView ? (
    <div className="flex-shrink-0 border-t border-border/60 bg-muted/20 px-3 py-2.5 sm:px-4 sm:py-3">
      <div className="flex w-full gap-2.5">
        <Input
          placeholder={TEXT.inputPlaceholder}
          value={inputValue}
          onChange={(event) => setInputValue(event.target.value)}
          onKeyDown={handleKeyDown}
          disabled={isLoading}
          className="h-9 flex-1 text-sm"
        />
        <Button
          size="icon"
          onClick={handleSendMessage}
          disabled={!inputValue.trim() || isLoading}
          className="h-9 w-9"
        >
          <Send className="h-4 w-4" />
        </Button>
      </div>
    </div>
  ) : null;

  const renderHeader = (closeType: "sheet" | "card" | "none") => (
    <div className="flex flex-shrink-0 items-center justify-between gap-2 rounded-t-3xl border-b border-border/60 bg-background/95 px-3 py-2 sm:px-4 sm:py-3">
      <div className="flex items-center gap-2">
        <div className="flex h-9 w-9 items-center justify-center rounded-full border border-border/70 bg-background/70">
          {headerIcon}
        </div>
        <div>
          <h3 className="text-sm font-semibold">{headerTitle}</h3>
          <p className="text-xs text-muted-foreground">{headerSubtitle}</p>
        </div>
      </div>
      {closeType === "sheet" ? (
        <SheetClose asChild>
          <Button
            variant="ghost"
            size="icon"
            className="h-8 w-8 rounded-full hover:bg-muted"
          >
            <X className="h-4 w-4" />
          </Button>
        </SheetClose>
      ) : closeType === "card" ? (
        <Button
          variant="ghost"
          size="icon"
          className="h-8 w-8 rounded-full hover:bg-muted"
          onClick={() => setIsOpen(false)}
        >
          <X className="h-4 w-4" />
        </Button>
      ) : null}
    </div>
  );

  const renderPanel = (
    closeType: "sheet" | "card",
    shape: "sheet" | "card",
  ) => (
    <div
      className={cn(
        "flex flex-1 flex-col overflow-hidden bg-background/95",
        shape === "card" ? "rounded-3xl" : "rounded-t-3xl",
      )}
    >
      {renderHeader(closeType)}
      <div className="min-h-0 flex-1 basis-0 overflow-hidden">{body}</div>
      {footerSection}
    </div>
  );

  return (
    <div className="pointer-events-none fixed bottom-6 right-6 z-50 flex flex-col items-end gap-3">
      <div className="pointer-events-auto flex flex-col items-end gap-3">
        <FloatingDock
          items={dockItems}
          desktopClassName="ml-auto mr-0 bg-background/90 border border-border/60 shadow-lg px-3 pb-2"
          mobileClassName="md:hidden"
        />

        {isMobile ? (
          <Sheet open={isOpen} onOpenChange={setIsOpen}>
            <SheetContent
              side="bottom"
              hideCloseButton
              className="flex h-[85vh] flex-col gap-0 rounded-t-3xl border-border/60 bg-background/95 p-0 shadow-2xl backdrop-blur"
            >
              {renderPanel("sheet", "sheet")}
            </SheetContent>
          </Sheet>
        ) : (
          isOpen && (
            <Card
              className="pointer-events-auto flex w-[300px] flex-col overflow-hidden rounded-3xl border border-border/60 bg-background/95 shadow-2xl backdrop-blur supports-backdrop-filter:bg-background/80 sm:w-[360px] p-0"
              style={{
                maxHeight: toCssSize(maxHeight, "75vh"),
                minHeight: toCssSize(minHeight, "340px"),
              }}
            >
              {renderPanel("card", "card")}
            </Card>
          )
        )}
      </div>
    </div>
  );
}
