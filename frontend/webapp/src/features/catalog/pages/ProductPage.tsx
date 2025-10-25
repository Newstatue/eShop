import { Spinner } from "@/components/ui/spinner";
import { Card, CardHeader, CardContent, CardFooter } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { useProductsList } from "../hooks/useProducts";
import { Button } from "@/components/ui/button";
import { AspectRatio } from "@/components/ui/aspect-ratio";
import AISupport from "../components/AISupport";
import SearchBar from "../components/Search";
import { useEffect, useState } from "react";
import type { Product } from "@/models/Product";

export default function ProductPage() {
    const { data: products, isLoading } = useProductsList();
    const [displayed, setDisplayed] = useState<Product[]>([]);

    useEffect(() => {
        if (products) {
            setDisplayed(products);
        }
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
                onResults={(list) => setDisplayed(list)}
                onClear={() => setDisplayed(products ?? [])}
                initialMode="ai"
                leftReserve={75}
                rightReserve={75}
            />

            <section className="min-h-screen bg-linear-to-b from-muted/30 via-background/60 to-background pt-28 pb-10">
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
                    <div className="grid grid-cols-1 gap-8 sm:grid-cols-2 xl:grid-cols-3">
                        {(displayed ?? []).map((product) => {
                            const raw = product.imageUrl ?? '';
                            const imageSrc = raw.startsWith('http') || raw.startsWith('/') ? raw : `/${raw}`;

                            return (
                                <Card
                                    key={product.id}
                                    className="group flex h-full flex-col overflow-hidden border-border/60 bg-card/95 shadow-sm transition duration-200 hover:-translate-y-1 hover:shadow-lg gap-0 p-0"
                                >
                                    <CardHeader className="grid gap-5 p-0">
                                        <div className="relative w-full overflow-hidden">
                                            <AspectRatio ratio={4 / 3} className="w-full bg-secondary/30">
                                                <img
                                                    src={imageSrc}
                                                    alt={product.name}
                                                    className="h-full w-full object-cover transition-transform duration-500 group-hover:scale-105"
                                                    loading="lazy"
                                                    onError={(e) => {
                                                        const target = e.currentTarget as HTMLImageElement;
                                                        target.onerror = null;
                                                        // use a safe built-in public asset as fallback
                                                        target.src = '/vite.svg';
                                                    }}
                                                />
                                            </AspectRatio>
                                            <Badge
                                                variant="secondary"
                                                className="absolute right-4 top-4 border border-border/60 bg-background/90 text-sm font-semibold tracking-wide shadow"
                                            >
                                                ￥{product.price.toFixed(2)}
                                            </Badge>
                                        </div>
                                        <div className="space-y-2 px-6">
                                            <h2 className="text-xl font-semibold leading-tight text-foreground">
                                                {product.name}
                                            </h2>
                                            <p className="text-xs uppercase tracking-[0.2em] text-muted-foreground/80">
                                                编号 #{product.id}
                                            </p>
                                        </div>
                                    </CardHeader>
                                    <CardContent className="flex flex-1 flex-col px-6 pb-6 pt-0">
                                        <p className="line-clamp-4 text-sm text-muted-foreground">
                                            {product.description}
                                        </p>
                                    </CardContent>
                                    <CardFooter className="border-t border-border/60 px-6 py-5">
                                        <Button className="w-full" size="lg">
                                            立即购买
                                        </Button>
                                    </CardFooter>
                                </Card>
                            );
                        })}
                    </div>
                </div>
                <AISupport />
            </section>
        </>
    );
}
