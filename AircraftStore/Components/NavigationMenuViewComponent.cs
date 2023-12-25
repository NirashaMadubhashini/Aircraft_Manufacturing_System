using Microsoft.AspNetCore.Mvc;
using Aircraft.DataAccess.Data;

namespace Aircraft.Components;

public class NavigationMenuViewComponent : ViewComponent
{
    private DataContext _context;

    public NavigationMenuViewComponent(DataContext context)
    {
        _context = context;
    }

    public IViewComponentResult Invoke()
    {
        return View();
    }
}