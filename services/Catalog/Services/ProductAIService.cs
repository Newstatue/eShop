

namespace Catalog.Services;

/// <summary>
/// 综合 AI 服务：包含聊天问答、向量检索、自动标签生成
/// </summary>
public class ProductAIService(
    IChatClient chatClient,
    ProductDbContext dbContext,
    IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
    VectorStoreCollection<int, ProductVector> productVectorCollection)
{
    // -------------------------------
    // 🧠 产品问答助手
    // -------------------------------
    public async Task<string> SupportAsync(string query, CancellationToken cancellationToken = default)
    {
        var systemPrompt =
            """
            你是一名户外运动产品专家助手。
            你始终使用简洁、友好、略带趣味的中文回答。
            如果不知道答案，请直接回复“我不知道。”。
            你只能回答与户外运动产品相关的问题，并在回答后推荐
            至少一种户外产品（如手电筒、帐篷、登山杖、背包等）。
            """;

        var chatHistory = new List<ChatMessage> { new(ChatRole.System, systemPrompt), new(ChatRole.User, query) };

        var response = await chatClient.GetResponseAsync(chatHistory, cancellationToken: cancellationToken);

        var content = Regex.Replace(
            response.Text,
            @"<think>.*?</think>",
            string.Empty,
            RegexOptions.Singleline | RegexOptions.IgnoreCase);

        return content.Trim();
    }

    // -------------------------------
    // 🔍 语义检索（基于向量）
    // -------------------------------
    public async Task<IEnumerable<Product>> SearchProductsAsync(
        string query,
        int topN = 5,
        float threshold = 0.65f,
        CancellationToken cancellationToken = default)
    {
        if (!await productVectorCollection.CollectionExistsAsync())
        {
            await InitEmbeddingsAsync(cancellationToken);
        }

        var queryEmbedding = await embeddingGenerator.GenerateAsync(query, cancellationToken: cancellationToken);

        var results = new List<VectorSearchResult<ProductVector>>();
        var searchOptions = new VectorSearchOptions<ProductVector>
        {
            VectorProperty = r => r.Vector, IncludeVectors = false
        };

        await foreach (var result in productVectorCollection.SearchAsync(queryEmbedding, topN, searchOptions))
        {
            if (result.Score >= threshold)
                results.Add(result);
        }

        if (results.Count == 0)
        {
            await foreach (var result in productVectorCollection.SearchAsync(queryEmbedding, 1, searchOptions))
            {
                results.Add(result);
                break;
            }
        }

        var ids = results.Select(r => r.Record.Id).Distinct().ToList();
        if (ids.Count == 0)
        {
            return Enumerable.Empty<Product>();
        }

        var products = await dbContext.Products
            .AsNoTracking()
            .Include(p => p.Images)
            .Include(p => p.Tags)
            .Include(p => p.Variants)
            .Where(p => ids.Contains(p.Id))
            .ToListAsync(cancellationToken);

        var productLookup = products.ToDictionary(p => p.Id);

        return results
            .Where(r => productLookup.ContainsKey(r.Record.Id))
            .Select(r => productLookup[r.Record.Id]);
    }

    // -------------------------------
    // 🧩 初始化产品向量
    // -------------------------------
    public async Task InitEmbeddingsAsync(CancellationToken cancellationToken = default)
    {
        await productVectorCollection.EnsureCollectionExistsAsync();

        var products = await dbContext.Products
            .AsNoTracking()
            .Include(p => p.Images)
            .ToListAsync(cancellationToken);

        foreach (var product in products)
        {
            await GenerateEmbeddingAsync(product, cancellationToken);
        }
    }

    // -------------------------------
    // ✨ AI 自动标签生成
    // -------------------------------
    public async Task TagProductAsync(Product product, CancellationToken cancellationToken = default)
    {
        var systemPrompt = """
                           你是一位专业的户外用品标签助手。
                           请根据商品名称和描述生成 3~6 个简洁的中文标签。
                           标签需短小、清晰、与产品特征或用途相关。
                           输出格式：JSON 数组，例如：
                           ["防水", "轻量", "露营", "耐用", "应急"]
                           """;

        var chatMessages = new List<ChatMessage>
        {
            new(ChatRole.System, systemPrompt), new(ChatRole.User, $"名称: {product.Name}\n描述: {product.Description}")
        };

        var response = await chatClient.GetResponseAsync(chatMessages, cancellationToken: cancellationToken);
        var cleanText = Regex.Replace(response.Text, @"<think>.*?</think>", "", RegexOptions.Singleline);

        try
        {
            var tags = JsonSerializer.Deserialize<List<string>>(cleanText,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (tags is not null && tags.Count > 0)
            {
                product.Tags.Clear();
                foreach (var tagName in tags)
                {
                    var existing = await dbContext.Tags.FirstOrDefaultAsync(t => t.Name == tagName, cancellationToken);
                    product.Tags.Add(existing ?? new ProductTag { Name = tagName });
                }

                Console.WriteLine($"✅ [{product.Name}] 标签生成成功: {string.Join(", ", tags)}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ 标签生成失败: {cleanText}\n错误: {ex.Message}");
        }
    }

    // -------------------------------
    // ✨ AI 详细描述生成
    // -------------------------------
    public async Task GenerateRichDescriptionAsync(Product product, CancellationToken cancellationToken = default)
    {
        if (product.IsRichDescriptionAIGenerated)
        {
            return;
        }

        var systemPrompt = """
                           你是一位专业的电商内容撰写专家，任务是为商品生成自然可信的中文详情描述。
                           请严格遵循以下要求：
                           1. 输出应包含 2 至 3 个自然段，总长度约 150~200 字。
                           2. 第一段：介绍核心功能、材质或设计亮点。
                           3. 第二段：描述使用体验、适用场景或优势。
                           4. 内容必须基于已提供信息，不得虚构功能或参数。
                           5. 语气自然、可信，不使用夸张广告语言。
                           6. 输出纯文本，不含 Markdown 或 HTML。
                           """;

        var chatMessages = new List<ChatMessage>
        {
            new(ChatRole.System, systemPrompt),
            new(ChatRole.User, $"""
                                请根据以下商品信息生成详情描述：
                                - 名称：{product.Name}
                                - 品牌：{product.Brand}
                                - 描述：{product.Description}
                                """)
        };

        var response = await chatClient.GetResponseAsync(chatMessages, cancellationToken: cancellationToken);
        var cleanText = Regex.Replace(response.Text, @"<think>.*?</think>", "", RegexOptions.Singleline);

        if (string.IsNullOrWhiteSpace(cleanText))
        {
            Console.WriteLine($"⚠️ [{product.Name}] 生成描述失败，模型未返回有效内容。");
            return;
        }

        product.RichDescription = cleanText.Trim();
        product.IsRichDescriptionAIGenerated = true;
        Console.WriteLine($"📝 [{product.Name}] 自动生成详细描述成功。");
    }

// -------------------------------
// 🧠 生成 Embedding
// -------------------------------
    public async Task GenerateEmbeddingAsync(Product product, CancellationToken cancellationToken = default)
    {
        await productVectorCollection.EnsureCollectionExistsAsync(cancellationToken);

        var text = $"{product.Name} {product.Description} {string.Join(' ', product.Tags.Select(t => t.Name))}";
        var embedding = await embeddingGenerator.GenerateAsync(text, cancellationToken: cancellationToken);

        var vector = new ProductVector
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Brand = product.Brand,
            BasePrice = product.BasePrice,
            PrimaryImageUrl = product.PrimaryImageUrl,
            Vector = embedding.Vector
        };

        await productVectorCollection.UpsertAsync(vector, cancellationToken: cancellationToken);
    }

// -------------------------------
// ⚡ 新商品处理：打标签 + 嵌入
// -------------------------------
    public async Task ProcessNewProductAsync(
        Product product,
        bool persistChanges = false,
        bool regenerateRichDescription = true,
        CancellationToken cancellationToken = default)
    {
        await TagProductAsync(product, cancellationToken);
        await GenerateEmbeddingAsync(product, cancellationToken);

        if (regenerateRichDescription && !product.IsRichDescriptionAIGenerated)
        {
            await GenerateRichDescriptionAsync(product, cancellationToken);
        }

        if (persistChanges)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
