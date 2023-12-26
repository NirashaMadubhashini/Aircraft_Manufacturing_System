namespace Aircraft.Models;

public class Cart
{
    public List<CartItem> CartItemsList { get; set; } = new List<CartItem>();

    public virtual void AddItem(int airplaneSizeId, int quantity)
    {
        CartItem? cartItemFromSession = CartItemsList
            .FirstOrDefault(p => p.AirplaneSizeId == airplaneSizeId);
        if (cartItemFromSession == null)
        {
            CartItemsList.Add(new CartItem
            {
                AirplaneSizeId = airplaneSizeId,
                Count = quantity
            });
        }
        else
        {
            cartItemFromSession.Count += quantity;
        }
    }

    public virtual void SubtractItem(int airplaneSizeId, int quantity)
    {
        CartItem? line = CartItemsList
            .FirstOrDefault(p => p.AirplaneSizeId == airplaneSizeId);
        if (line != null)
        {
            line.Count -= quantity;
            if (line.Count <= 0)
            {
                RemoveLine(airplaneSizeId);
            }
        }
    }

    public virtual void RemoveLine(int airplaneSizeId) =>
        CartItemsList.RemoveAll(l => l.AirplaneSizeId == airplaneSizeId);

    public virtual void Clear() => CartItemsList.Clear();
}