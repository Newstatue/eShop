import { Spinner } from "@/components/ui/spinner";
import { useProductsList } from "../hooks/useProducts";
import AISupport from "../components/AISupport";
import SearchBar from "../components/Search";
import { useCallback, useEffect, useMemo, useState } from "react";
import type { Product } from "@/models/Product";
import ExpandableCardGrid, {
    ExpandableCardItem,
} from "@/components/expandable-card-demo-grid";

export default function ProductPage() {
    const { data: products, isLoading } = useProductsList();
    const [displayed, setDisplayed] = useState<Product[] | undefined>(undefined);

    useEffect(() => {
        if (Array.isArray(products)) {
            setDisplayed(products);
        }
    }, [products]);

    const normalizedProducts = useMemo<Product[]>(() => {
        return Array.isArray(displayed) ? displayed : [];
    }, [displayed]);

    const productCards: ExpandableCardItem[] = useMemo(() => {
        return normalizedProducts.map((product) => {
            const rawSrc = product.imageUrl ?? "";
            const imageSrc =
                rawSrc.startsWith("http") || rawSrc.startsWith("/")
                    ? rawSrc
                    : `/${rawSrc}`;

            return {
                title: product.name,
                description:
                    product.description?.slice(0, 80) ?? "这款产品暂时没有描述。",
                src: imageSrc || "/vite.svg",
                fallbackSrc: "/vite.svg",
                badge: `￥${product.price.toFixed(2)}`,
                ctaText: "加入购物车",
                ctaLink: `/products/${product.id}`,
                content: (
                    <div className="space-y-4 text-left">
                        <p className="text-sm leading-relaxed text-neutral-600 dark:text-neutral-300">
                            {product.description ?? "暂无详细介绍。"}
                        </p>
                        <div className="text-sm font-semibold text-primary">
                            单价：￥{product.price.toFixed(2)}
                        </div>
                        <div className="text-xs text-muted-foreground">
                            商品编号 #{product.id}
                        </div>
                    </div>
                ),
            } satisfies ExpandableCardItem;
        });
    }, [normalizedProducts]);

    const handleResults = useCallback((list: Product[]) => {
        setDisplayed(Array.isArray(list) ? list : []);
    }, []);

    const handleClear = useCallback(() => {
        setDisplayed(Array.isArray(products) ? products : []);
    }, [products]);

    if (isLoading) {
        return (
            <div className="flex h-screen items-center justify-center bg-linear-to-b from-muted/30 via-background/60 to-background">
                <Spinner />
            </div>
        );
    }

    return (
        <>
            <SearchBar
                onResults={handleResults}
                onClear={handleClear}
                mode="ai"
            />

            <section className="relative min-h-screen overflow-hidden bg-linear-to-b from-muted/30 via-background/60 to-background pt-28 pb-10">
                <div className="mx-auto flex w-full max-w-6xl flex-col gap-10 px-4 sm:px-6 lg:px-8">
                    <header className="space-y-2 text-center md:text-left">
                        <p className="text-sm font-medium uppercase tracking-wide text-primary/80">
                            精选商品
                        </p>
                        <h1 className="text-3xl font-bold tracking-tight sm:text-4xl">产品列表</h1>
                        <p className="text-muted-foreground">
                            精心挑选的好物，让你的日常更有质感。
                        </p>
                    </header>
                    {productCards.length > 0 ? (
                        <ExpandableCardGrid items={productCards} />
                    ) : (
                        <div className="rounded-2xl border border-border/60 bg-background/80 p-12 text-center text-muted-foreground shadow-sm">
                            暂无符合条件的商品。
                        </div>
                    )}
                </div>
                <AISupport maxHeight="100vh" minHeight={600} />
            </section>
        </>
    );
}
