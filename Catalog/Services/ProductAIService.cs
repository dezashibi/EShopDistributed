using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;

namespace Catalog.Services;

public sealed class ProductAIService(
    ProductDbContext dbContext,
    IChatClient chatClient,
    IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
    VectorStoreCollection<int, ProductVector> productVectorCollection)
{
    public async Task<string> SupportAsync(string query, CancellationToken ct = default)
    {
        var systemPrompt = """
        You are a useful assistant.
        You always reply with a short and funny message.
        If you do not know an answer, you say 'I don't know that.'
        You only answer questions related to outdoor camping products.
        For any other type of questions, explain to the user that you only answer outdoor camping products questions.
        At the end, Offer one of our products: Hiking Poles-$24, Outdoor Rain Jacket-$12, Outdoor Backpack-$32, Camping Tent-$22
        Do not store memory of the chat conversation.
        """;

        var chatHistory = new List<ChatMessage>
        {
            new(ChatRole.System, systemPrompt),
            new(ChatRole.User, query)
        };

        var response = await chatClient.GetResponseAsync(chatHistory, cancellationToken: ct);
        return response.Messages[0].Contents[0].ToString()!;
    }

    public async Task<List<Product>> SearchProductsAsync(string query, CancellationToken ct = default)
    {
        if (!await productVectorCollection.CollectionExistsAsync(ct))
        {
            await InitEmbeddingsAsync(ct);
        }

        ReadOnlyMemory<float> queryVector =
            await embeddingGenerator.GenerateVectorAsync(query, cancellationToken: ct);

        var options = new VectorSearchOptions<ProductVector>
        {
            // Only needed if you have multiple vector properties; harmless otherwise.
            VectorProperty = v => v.Vector,
            IncludeVectors = false
        };

        var products = new List<Product>();

        await foreach (var result in productVectorCollection.SearchAsync(queryVector, top: 5, options, ct))
        {
            products.Add(new Product
            {
                Id = result.Record.Id,
                Name = result.Record.Name,
                Description = result.Record.Description,
                Price = result.Record.Price,
                ImageUrl = result.Record.ImageUrl
            });
        }

        return products;
    }

    private async Task InitEmbeddingsAsync(CancellationToken ct)
    {
        await productVectorCollection.EnsureCollectionExistsAsync(ct);

        var products = await dbContext.Products.AsNoTracking().ToListAsync(ct);

        // Build the text used for embeddings.
        var inputs = products.Select(p =>
            $"[{p.Name}] is a product that costs [{p.Price}] and is described as [{p.Description}]"
        ).ToArray();

        // Batch-generate embeddings using the current generator API.
        var generated = await embeddingGenerator.GenerateAsync(inputs, cancellationToken: ct);

        // Zip products + embeddings into vector records.
        var vectors = products.Zip(generated, (p, e) => new ProductVector
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Price = p.Price,
            ImageUrl = p.ImageUrl,
            Vector = e.Vector
        });

        await productVectorCollection.UpsertAsync(vectors);
    }
}
