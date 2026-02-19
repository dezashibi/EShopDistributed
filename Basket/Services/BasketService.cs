using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace Basket.Services;

public class BasketService(IDistributedCache cache, CatalogApiClient catalogApiClient)
{
    private string GetIndex(string indexName) => $"indexes:{indexName}";
    private string GetUserNameIndex() => GetIndex("user-name");

    public async Task<ShoppingCart?> GetBasket(string userName)
    {
        var basket = await cache.GetStringAsync(userName);
        return string.IsNullOrEmpty(basket) ? null : JsonSerializer.Deserialize<ShoppingCart>(basket);
    }

    public async Task UpdateBasket(ShoppingCart basket)
    {
        // Before update(Add/Remove Item) into SC, I should call Catalog ms GetProductById method
        // Get latest product information and set Price and ProductName when adding item into SC

        foreach (var item in basket.Items)
        {
            var product = await catalogApiClient.GetProductById(item.ProductId);
            item.Price = product.Price;
            item.ProductName = product.Name;
        }

        await cache.SetStringAsync(basket.UserName, JsonSerializer.Serialize(basket));

        // Update indexes
        await UpdateUserNamesIndex(basket.UserName, false);
    }

    public async Task DeleteBasket(string userName)
    {
        await cache.RemoveAsync(userName);

        // Update indexes
        await UpdateUserNamesIndex(userName, true);
    }

    public async Task UpdateBasketItemProductPrices(int productId, decimal price)
    {
        // IDistributedCache not supported list of keys function
        // https://github.com/dotnet/runtime/issues/36402

        // NOTE:: Because of the issues above, here I demonstrate with a pre-defined user name
        //        But in real scenario, I need to be able to iterate through all the keys
        /*var basket = await GetBasket("navid");

        var item = basket!.Items.FirstOrDefault(x => x.ProductId == productId);
        if (item is not null)
        {
            item.Price = price;
            await cache.SetStringAsync(basket.UserName, JsonSerializer.Serialize(basket));
        }*/

        // NOTE:: My implementations for the said problem

        // get users index
        var userNames = await GetUserNamesIndex();
        foreach (var user in userNames)
        {
            var basket = await GetBasket(user);
            if (basket is null)
                continue;

            var item = basket.Items.FirstOrDefault(x => x.ProductId == productId);
            if (item is not null)
            {
                item.Price = price;
                await cache.SetStringAsync(basket.UserName, JsonSerializer.Serialize(basket));
            }
        }
    }

    private async Task<HashSet<string>> GetUserNamesIndex()
    {
        var userNameIndexes = await cache.GetStringAsync(GetUserNameIndex());

        HashSet<string> userNames = [];

        if (!string.IsNullOrEmpty(userNameIndexes))
        {
            userNames = JsonSerializer.Deserialize<HashSet<string>>(userNameIndexes) ?? [];
        }

        return userNames;
    }

    private async Task UpdateUserNamesIndex(string userName, bool shouldRemove)
    {
        var userNames = await GetUserNamesIndex();

        if (shouldRemove)
        {
            userNames.Remove(userName);
        }
        else
        {
            userNames.Add(userName);
        }

        await cache.SetStringAsync(GetUserNameIndex(), JsonSerializer.Serialize(userNames));
    }
}
