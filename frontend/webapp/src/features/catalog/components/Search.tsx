"use client";

import { useEffect, useMemo, useState } from "react";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import { ToggleGroup, ToggleGroupItem } from "@/components/ui/toggle-group";
import { Spinner } from "@/components/ui/spinner";
import { Search as SearchIcon, Sparkles, X } from "lucide-react";
import { useAISearch, useSearch } from "../hooks/useProducts";
import type { Product } from "@/models/Product";

type Mode = "normal" | "ai";

export interface SearchBarProps {
    onResults: (products: Product[]) => void;
    onClear?: () => void;
    initialMode?: Mode;
    placeholder?: string;
    // 预留左右空间（可传 number: px 或字符串: '12rem'/'15vw'）
    leftReserve?: number | string;
    rightReserve?: number | string;
    smLeftReserve?: number | string;
    smRightReserve?: number | string;
    mdLeftReserve?: number | string;
    mdRightReserve?: number | string;
    lgLeftReserve?: number | string;
    lgRightReserve?: number | string;
}

function useDebouncedValue<T>(value: T, delay = 400) {
    const [debounced, setDebounced] = useState(value);
    useEffect(() => {
        const id = setTimeout(() => setDebounced(value), delay);
        return () => clearTimeout(id);
    }, [value, delay]);
    return debounced;
}

export default function SearchBar({
    onResults,
    onClear,
    initialMode = "ai",
    placeholder = "搜索商品，试试：户外雨衣，露营帐篷…",
    leftReserve,
    rightReserve,
    smLeftReserve,
    smRightReserve,
    mdLeftReserve,
    mdRightReserve,
    lgLeftReserve,
    lgRightReserve,
}: SearchBarProps) {
    const [mode, setMode] = useState<Mode>(initialMode);
    const [query, setQuery] = useState("");
    const debouncedQuery = useDebouncedValue(query, 450);

    // Enable only the active mode's hook by passing an empty query to the other one
    const normalQuery = mode === "normal" ? debouncedQuery.trim() : "";
    const aiQuery = mode === "ai" ? debouncedQuery.trim() : "";

    const {
        data: normalData,
        isFetching: fetchingNormal,
        error: normalError,
    } = useSearch(normalQuery);

    const {
        data: aiData,
        isFetching: fetchingAI,
        error: aiError,
    } = useAISearch(aiQuery);

    const isFetching = fetchingNormal || fetchingAI;
    const activeError = mode === "normal" ? normalError : aiError;
    const errorMessage = activeError ? "搜索失败，请重试" : undefined;

    // Emit results when data changes
    useEffect(() => {
        if (!debouncedQuery) {
            onClear?.();
            return;
        }
        const data = mode === "normal" ? normalData : aiData;
        if (data && Array.isArray(data)) {
            onResults(data);
        }
    }, [debouncedQuery, normalData, aiData, mode, onResults, onClear]);

    // Fixed bar height for layout offset consumers
    const barHeight = 100; // px

    // 将传入值统一为 CSS 长度
    const toCss = (v: number | string | undefined, fallback: string) =>
        typeof v === "number" ? `${v}px` : (v && String(v).trim()) || fallback;

    // 变量默认值（与之前布局一致），并支持级联继承：
    // 未显式传入某断点的值时，自动继承上一个断点的设置，避免重复传参。
    const leftBase = toCss(leftReserve, "100px");
    const rightBase = toCss(rightReserve, "100px");
    const leftSm = toCss(smLeftReserve, leftBase);
    const rightSm = toCss(smRightReserve, rightBase);
    const leftMd = toCss(mdLeftReserve, leftSm);
    const rightMd = toCss(mdRightReserve, rightSm);
    const leftLg = toCss(lgLeftReserve, leftMd);
    const rightLg = toCss(lgRightReserve, rightMd);

    const cssVars: React.CSSProperties = {
        // base（<640px）
        ["--sb-left-base" as any]: leftBase,
        ["--sb-right-base" as any]: rightBase,
        // sm（>=640px）
        ["--sb-left-sm" as any]: leftSm,
        ["--sb-right-sm" as any]: rightSm,
        // md（>=768px）
        ["--sb-left-md" as any]: leftMd,
        ["--sb-right-md" as any]: rightMd,
        // lg（>=1024px）
        ["--sb-left-lg" as any]: leftLg,
        ["--sb-right-lg" as any]: rightLg,
    };

    const modeLabel = useMemo(
        () => (mode === "normal" ? "普通搜索" : "AI 搜索"),
        [mode]
    );

    return (
        <div
            className="fixed left-0 right-0 top-0 z-30 border-b border-border/60 bg-background/80 backdrop-blur supports-backdrop-filter:bg-background/60"
            style={{ height: barHeight, ...cssVars }}
        >
            {/* 外层根据视口左右做内边距，真正避开 Logo/按钮 */}
            <div className="h-full w-full pl-(--sb-left-base) pr-(--sb-right-base) sm:pl-(--sb-left-sm) sm:pr-(--sb-right-sm) md:pl-(--sb-left-md) md:pr-(--sb-right-md) lg:pl-(--sb-left-lg) lg:pr-(--sb-right-lg)">
                <div className="mx-auto flex h-full w-full max-w-6xl items-center gap-3 px-4 sm:px-6 lg:px-8">
                    {/* 搜索区 */}
                    <ToggleGroup
                        type="single"
                        value={mode}
                        onValueChange={(v) => v && setMode(v as Mode)}
                        className="hidden sm:flex"
                        spacing={0}
                    >
                        <ToggleGroupItem value="normal" aria-label="普通搜索">
                            <SearchIcon className="mr-2 size-4" /> 普通
                        </ToggleGroupItem>
                        <ToggleGroupItem value="ai" aria-label="AI搜索">
                            <Sparkles className="mr-2 size-4" /> AI
                        </ToggleGroupItem>
                    </ToggleGroup>

                    <div className="relative flex-1">
                        <Input
                            value={query}
                            onChange={(e) => setQuery(e.target.value)}
                            placeholder={`${modeLabel}：${placeholder}`}
                            className="h-11 pl-10 pr-12"
                            aria-label="搜索商品"
                            aria-invalid={!!activeError}
                            title={errorMessage}
                        />
                        {mode === "ai" ? (
                            <Sparkles className="pointer-events-none absolute left-3 top-1/2 size-4 -translate-y-1/2 text-muted-foreground" />
                        ) : (
                            <SearchIcon className="pointer-events-none absolute left-3 top-1/2 size-4 -translate-y-1/2 text-muted-foreground" />
                        )}
                        {query && !isFetching && (
                            <Button
                                type="button"
                                size="icon"
                                variant="ghost"
                                className="absolute right-1 top-1/2 size-8 -translate-y-1/2"
                                aria-label="清空搜索"
                                onClick={() => {
                                    setQuery("");
                                    onClear?.();
                                }}
                            >
                                <X className="size-4" />
                            </Button>
                        )}
                        {isFetching && (
                            <div className="pointer-events-none absolute right-3 top-1/2 -translate-y-1/2">
                                <Spinner className="size-4" />
                            </div>
                        )}
                    </div>
                </div>
            </div>
        </div>
    );
}