import { Product } from "@/models/Product";
import { useQuery } from "@tanstack/react-query";
import { getProductById, getProducts, searchProducts, supportProducts, aiSearchProducts } from "../api/catalogApiClient";

export function useProductsList() {
    return useQuery<Product[]>({
        queryKey: ['products'],
        queryFn: getProducts,
    })
}

export function useProduct(id: number) {
    return useQuery<Product>({
        queryKey: ['products', id],
        queryFn: ({ queryKey }) => getProductById(queryKey[1] as number),
        enabled: !!id,
    })
}

export function useSupportProducts(query: string) {
    return useQuery<string>({
        queryKey: ['products', query],
        queryFn: ({ queryKey }) => supportProducts(queryKey[1] as string),
        enabled: !!query,
    })
}

export function useSearch(query: string) {
    return useQuery<Product[]>({
        queryKey: ['products', query],
        queryFn: ({ queryKey }) => searchProducts(queryKey[1] as string),
        enabled: !!query,
    })
}

export function useAISearch(query: string) {
    return useQuery<Product[]>({
        queryKey: ['products', query],
        queryFn: ({ queryKey }) => aiSearchProducts(queryKey[1] as string),
        enabled: !!query,
    })
}