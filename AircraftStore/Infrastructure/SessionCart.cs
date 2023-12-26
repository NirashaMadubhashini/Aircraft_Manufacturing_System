using System.Text.Json.Serialization;
using Aircraft.Models;

namespace Aircraft.Infrastructure;

public class SessionCart : Cart
{
    public static Cart GetCart(IServiceProvider services)
    {
        ISession? session = services.GetRequiredService<IHttpContextAccessor>()
            .HttpContext?.Session;
        SessionCart cart = session?.GetJson<SessionCart>("Cart")
                           ?? new SessionCart();
        cart.Session = session;
        return cart;
    }

    [JsonIgnore] public ISession? Session { get; set; }

    public override void AddItem(int airplaneSizeId, int quantity)
    {
        base.AddItem(airplaneSizeId, quantity);
        Session?.SetJson("Cart", this);
    }

    public override void SubtractItem(int airplaneSizeId, int quantity)
    {
        base.SubtractItem(airplaneSizeId, quantity);
        Session?.SetJson("Cart", this);
    }

    public override void RemoveLine(int airplaneSizeId)
    {
        base.RemoveLine(airplaneSizeId);
        Session?.SetJson("Cart", this);
    }

    public override void Clear()
    {
        base.Clear();
        Session?.Remove("Cart");
    }
}