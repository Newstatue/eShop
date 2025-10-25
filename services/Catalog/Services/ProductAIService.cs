namespace Catalog.Services;

public class ProductAIService(
    IChatClient chatClient,
    ProductDbContext dbContext,
    IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
    VectorStoreCollection<int, ProductVector> productVectorCollection
)
{
    public async Task<string> SupportAsync(string query)
    {
        var systemPrompt =
            """
              你是一个**户外运动产品专家助手**。
              你总是用**中文**回答问题。
              你总是回复**简洁、友好且富有趣味性**的信息。
              如果你不知道答案，请说“我不知道。”。
              你只回答与**户外运动产品**相关的问题。
              对于其他问题，请告知用户你只能回答与**户外运动产品**相关的问题。
              在回答后，推荐一种我们的产品：太阳能手电筒、登山杖、户外雨衣、生存套件、户外背包、露营炊具、露营炉、露营灯、露营帐篷。
            """;


        var chatHistory = new List<ChatMessage> { new(ChatRole.System, systemPrompt), new(ChatRole.User, query) };

        var response = await chatClient.GetResponseAsync(chatHistory);

        var content = response.Text;

        content = Regex.Replace(content, @"<think>.*?</think>", string.Empty,
            RegexOptions.Singleline | RegexOptions.IgnoreCase);

        return content.Trim();
    }

    public async Task<IEnumerable<Product>> SearchProductsAsync(string query, int topN = 5, float threshold = 0.65f)
    {
        if (!await productVectorCollection.CollectionExistsAsync())
            await InitEmbeddingsAsync();

        var queryEmbedding = await embeddingGenerator.GenerateAsync(query);

        var searchOptions = new VectorSearchOptions<ProductVector>
        {
            VectorProperty = r => r.Vector, IncludeVectors = false
        };

        var results = new List<VectorSearchResult<ProductVector>>();

        await foreach (var result in productVectorCollection.SearchAsync(queryEmbedding, topN, searchOptions))
        {
            // 只保留相似度高于阈值的
            if (result.Score >= threshold)
                results.Add(result);
        }

        // 如果都不满足阈值，兜底返回最相似的一个
        if (!results.Any())
        {
            await foreach (var result in productVectorCollection.SearchAsync(queryEmbedding, 1, searchOptions))
            {
                results.Add(result);
                break;
            }
        }

        return results.Select(r => new Product
        {
            Id = r.Record.Id,
            Name = r.Record.Name,
            Description = r.Record.Description,
            Price = r.Record.Price,
            ImageUrl = r.Record.ImageUrl
        });
    }


    public async Task InitEmbeddingsAsync()
    {
        await productVectorCollection.EnsureCollectionExistsAsync();

        var products = await dbContext.Products.ToListAsync();

        foreach (var product in products)
        {
            var productInfo = $"产品名称: {product.Name}；价格: {product.Price}元；描述: {product.Description}。";


            var embedding = await embeddingGenerator.GenerateAsync(productInfo);

            var productVector = new ProductVector
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                ImageUrl = product.ImageUrl,
                Vector = embedding.Vector
            };

            await productVectorCollection.UpsertAsync(productVector);
        }
    }
}
