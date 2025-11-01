import { useCallback, useEffect, useMemo, useState } from "react";

import ExpandableCardGrid, {
    ExpandableCardItem,
} from "@/components/expandable-card-grid";
import AISupport from "../components/AISupport";
import SearchBar from "../components/Search";
import { useProductsList } from "../hooks/useProducts";
import { ProductResponse, ProductVariantResponse } from "../models/Product";
import { Spinner } from "@/components/ui/spinner";

type VariantOption = {
    sku: string;
    price: number;
    label: string;
    attributes: {
        color?: string | null;
        size?: string | null;
        material?: string | null;
    };
};

function buildVariantLabel(variant: ProductVariantResponse) {
    const parts = [variant.color, variant.size, variant.material]
        .filter((part): part is string => Boolean(part && part.trim().length > 0));

    if (parts.length === 0) {
        return "标准款";
    }

    return parts.join(" / ");
}

type ProductVariantPanelProps = {
    productId: number;
    richDescription: string | null;
    basePrice: number;
    variants: VariantOption[];
};

function ProductVariantPanel({
    productId,
    richDescription,
    basePrice,
    variants,
}: ProductVariantPanelProps) {
    const [selectedSku, setSelectedSku] = useState<string | null>(
        variants.length > 0 ? variants[0].sku : null,
    );

    useEffect(() => {
        if (variants.length === 0) {
            setSelectedSku(null);
            return;
        }

        setSelectedSku((current) =>
            current && variants.some((variant) => variant.sku === current)
                ? current
                : variants[0]?.sku ?? null,
        );
    }, [variants]);

    const selectedVariant = useMemo(() => {
        return variants.find((variant) => variant.sku === selectedSku) ?? null;
    }, [selectedSku, variants]);

    const selectedPrice = Number(selectedVariant?.price ?? basePrice ?? 0);

    return (
        <div className="space-y-4 text-left">
            <p className="text-sm leading-relaxed text-neutral-600 dark:text-neutral-300">
                {richDescription ?? "暂无详细介绍"}
            </p>

            {variants.length > 0 ? (
                <div className="space-y-4 rounded-xl border border-border/60 bg-muted/20 p-4 text-sm">
                    <div className="flex flex-col gap-2">
                        <div className="flex flex-wrap gap-2">
                            {variants.map((variant) => {
                                const isActive = variant.sku === selectedSku;
                                return (
                                    <button
                                        key={variant.sku}
                                        type="button"
                                        onClick={() => setSelectedSku(variant.sku)}
                                        className={`rounded-full border px-3 py-1 text-xs transition ${
                                            isActive
                                                ? "border-primary bg-primary text-primary-foreground shadow"
                                                : "border-border bg-background text-foreground hover:border-primary/50"
                                        }`}
                                    >
                                        {variant.label}
                                    </button>
                                );
                            })}
                        </div>
                    </div>
                </div>
            ) : null}

            <div className="text-sm font-semibold text-primary">
                当前售价：￥{selectedPrice.toFixed(2)}
            </div>

            <div className="text-xs text-muted-foreground">商品编号 #{productId}</div>
        </div>
    );
}

export default function ProductPage() {
    const { data: products, isLoading } = useProductsList();
    const [displayed, setDisplayed] = useState<ProductResponse[] | undefined>(undefined);

    useEffect(() => {
        if (Array.isArray(products)) {
            setDisplayed(products);
        }
    }, [products]);

    const normalizedProducts = useMemo<ProductResponse[]>(() => {
        return Array.isArray(displayed) ? displayed : [];
    }, [displayed]);

    const productCards: ExpandableCardItem[] = useMemo(() => {
        return normalizedProducts.map((product) => {
            const normalizeImageUrl = (url?: string | null) => {
                if (!url) return "";
                return url.startsWith("http") || url.startsWith("/") ? url : `/${url}`;
            };

            const gallerySources = [
                product.primaryImageUrl,
                ...product.images.map((image) => image.url),
                ...product.variants.map((variant) => variant.imageUrl),
            ];

            const gallery = gallerySources
                .map((candidate) => normalizeImageUrl(candidate))
                .filter((candidate): candidate is string => candidate.length > 0)
                .filter((candidate, index, array) => array.indexOf(candidate) === index);

            const imageSrc = gallery[0] ?? "/vite.svg";

            const variants: VariantOption[] = product.variants.map((variant) => ({
                sku: variant.sku,
                price:
                    typeof variant.price === "number" ? variant.price : product.basePrice,
                label: buildVariantLabel(variant),
                attributes: {
                    color: variant.color,
                    size: variant.size,
                    material: variant.material,
                },
            }));

            const priceRange = variants.reduce(
                (range, variant) => ({
                    min: Math.min(range.min, variant.price),
                    max: Math.max(range.max, variant.price),
                }),
                { min: product.basePrice, max: product.basePrice },
            );

            const hasVariants = variants.length > 0;

            return {
                title: product.name,
                description:
                    product.description?.slice(0, 80) ?? "这款产品暂时没有描述",
                src: imageSrc,
                fallbackSrc: "/vite.svg",
                badge:
                    hasVariants && priceRange.min !== priceRange.max
                        ? `￥${priceRange.min.toFixed(0)}起`
                        : `￥${priceRange.min.toFixed(0)}`,
                // ✅ 恢复外层 CTA 按钮
                ctaText: "加入购物车",
                ctaLink:
                    hasVariants && variants.length > 0
                        ? `/products/${product.id}?variant=${encodeURIComponent(
                              variants[0].sku,
                          )}`
                        : `/products/${product.id}`,
                gallery,
                content: (
                    <ProductVariantPanel
                        productId={product.id}
                        richDescription={product.richDescription}
                        basePrice={product.basePrice}
                        variants={variants}
                    />
                ),
            } satisfies ExpandableCardItem;
        });
    }, [normalizedProducts]);

    const handleResults = useCallback((list: ProductResponse[]) => {
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
            <SearchBar onResults={handleResults} onClear={handleClear} mode="ai" />

            <section className="relative min-h-screen overflow-hidden bg-linear-to-b from-muted/30 via-background/60 to-background pt-28 pb-10">
                <div className="mx-auto flex w-full max-w-6xl flex-col gap-10 px-4 sm:px-6 lg:px-8">
                    <header className="space-y-2 text-center md:text-left">
                        <p className="text-sm font-medium uppercase tracking-wide text-primary/80">
                            精选商品
                        </p>
                        <h1 className="text-3xl font-bold tracking-tight sm:text-4xl">
                            产品列表
                        </h1>
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
