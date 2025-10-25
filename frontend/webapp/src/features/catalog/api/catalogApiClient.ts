import axios from "axios";

import { Product } from "../../../models/Product";
import { apiConfig } from "../../../app/config/apiConfig";

const httpClient = axios.create({
    baseURL: apiConfig.catalogApiBaseUrl
});

export async function getProducts(): Promise<Product[]> {
    const response = await httpClient.get<Product[]>('/products');
    return response.data;
}

export async function getProductById(id: number): Promise<Product> {
    const response = await httpClient.get<Product>(`/products/${id}`);
    return response.data;
}

export async function supportProducts(query: string) {
    const response = await httpClient.get<string>(`/products/support/${query}`);
    return response.data;
}