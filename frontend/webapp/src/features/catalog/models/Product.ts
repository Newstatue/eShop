export interface ProductResponse {
    id: number;
    name: string;
    description: string;
    brand: string;
    richDescription: string | null;
    isRichDescriptionAIGenerated: boolean;
    basePrice: number;
    isActive: boolean;
    createdAt: string;
    primaryImageUrl: string | null;
    categoryId: number;
    categoryName: string | null;
    images: ProductImageResponse[];
    variants: ProductVariantResponse[];
    tags: string[];
    aiStatus: string[];
}

export interface ProductImageResponse {
    url: string;
    isPrimary: boolean;
}

export interface ProductVariantResponse {
    sku: string;
    price: number;
    stockQuantity: number;
    imageUrl: string | null;
    color: string | null;
    size: string | null;
    material: string | null;
}

export interface ProductUpsertRequest {
    name: string;
    description: string;
    brand: string;
    richDescription: string | null;
    useAIGeneratedRichDescription: boolean;
    isActive: boolean;
    categoryId: number;
    images: ProductImageRequest[];
    variants: ProductVariantRequest[];
}

export interface ProductImageRequest {
    url: string;
    isPrimary: boolean;
}

export interface ProductVariantRequest {
    sku: string;
    price: number;
    stockQuantity: number;
    imageUrl: string | null;
    color: string | null;
    size: string | null;
    material: string | null;
}