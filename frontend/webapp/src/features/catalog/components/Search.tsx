"use client";

import { useEffect, useMemo, useState } from "react";
import { Spinner } from "@/components/ui/spinner";
import { PlaceholdersAndVanishInput } from "@/components/ui/placeholders-and-vanish-input";
import { useAISearch, useSearch } from "../hooks/useProducts";
import { ProductResponse } from "../models/Product";

type SearchMode = "normal" | "ai";

export interface FloatingSearchProps {
  onResults: (products: ProductResponse[]) => void;
  onClear?: () => void;
  mode?: SearchMode;
  placeholders?: string[];
  debounceMs?: number;
}

function useDebouncedValue<T>(value: T, delay = 400) {
  const [debounced, setDebounced] = useState(value);
  useEffect(() => {
    const id = setTimeout(() => setDebounced(value), delay);
    return () => clearTimeout(id);
  }, [value, delay]);
  return debounced;
}

const DEFAULT_PLACEHOLDERS_AI: string[] = [
  "想找一件防雨又透气的外衣",
  "推荐适合露营的帐篷",
  "晚上走山路太暗，有什么照明装备？",
  "轻便好穿的登山鞋有哪些？",
];

export default function SearchBar({
  onResults,
  onClear,
  mode = "ai",
  placeholders = DEFAULT_PLACEHOLDERS_AI,
  debounceMs = 450,
}: FloatingSearchProps) {
  const [inputValue, setInputValue] = useState("");

  const debouncedQuery = useDebouncedValue(inputValue, debounceMs).trim();

  const normalQuery = mode === "normal" ? debouncedQuery : "";
  const aiQuery = mode === "ai" ? debouncedQuery : "";

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

  const activeData = mode === "normal" ? normalData : aiData;
  const isFetching = mode === "normal" ? fetchingNormal : fetchingAI;
  const activeError = mode === "normal" ? normalError : aiError;

  useEffect(() => {
    if (!debouncedQuery) {
      onClear?.();
      return;
    }
    if (Array.isArray(activeData)) {
      onResults(activeData);
    }
  }, [debouncedQuery, activeData, onResults, onClear]);

  const statusMessage = useMemo(() => {
    if (activeError) return "搜索失败，请稍后重试";
    if (isFetching) return mode === "ai" ? "AI 正在思考…" : "正在搜索…";
    if (!debouncedQuery)
      return mode === "ai"
        ? "AI 搜索：输入关键词或问题试试"
        : "普通搜索：输入商品名称试试";
    return undefined;
  }, [activeError, isFetching, mode, debouncedQuery]);

  const handleReset = () => {
    setInputValue("");
  };

  return (
    <div className="pointer-events-none fixed inset-x-0 top-6 z-30 flex justify-center">
      <div className="pointer-events-auto w-full px-4 sm:px-6 lg:px-8 pl-[100px] pr-[100px] sm:pl-[120px] sm:pr-[120px] md:pl-[140px] md:pr-[140px] lg:pl-[160px] lg:pr-[160px]">
        <div className="relative mx-auto w-full max-w-3xl">
          <PlaceholdersAndVanishInput
            placeholders={placeholders}
            onChange={(e) => setInputValue(e.target.value)}
            onSubmit={() => {
              setInputValue((value) => value.trim());
            }}
            onReset={handleReset}
            value={inputValue}
          />

          {isFetching && (
            <div className="pointer-events-none absolute right-16 top-1/2 -translate-y-1/2">
              <Spinner className="size-4" />
            </div>
          )}

          {statusMessage && (
            <p className="mt-2 text-center text-xs text-muted-foreground">
              {statusMessage}
            </p>
          )}
        </div>
      </div>
    </div>
  );
}