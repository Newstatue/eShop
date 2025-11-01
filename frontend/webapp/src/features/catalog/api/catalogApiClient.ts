import axios from "axios";

import { apiConfig } from "../../../app/config/apiConfig";
import { ProductResponse } from "../models/Product";

const httpClient = axios.create({
    baseURL: apiConfig.catalogApiBaseUrl
});

export async function getProducts(): Promise<ProductResponse[]> {
    const response = await httpClient.get<ProductResponse[]>('/products');
    return response.data;
}

export async function getProductById(id: number): Promise<ProductResponse> {
    const response = await httpClient.get<ProductResponse>(`/products/${id}`);
    return response.data;
}

export async function supportProducts(query: string) {
    const response = await httpClient.get<string>(`/products/support/${query}`);
    return response.data;
}

export async function searchProducts(query: string): Promise<ProductResponse[]> {
    const response = await httpClient.get<ProductResponse[]>(`/products/search/${query}`);
    return response.data;
}
export async function aiSearchProducts(query: string): Promise<ProductResponse[]> {
    const response = await httpClient.get<ProductResponse[]>(`/products/aisearch/${query}`);
    return response.data;
}