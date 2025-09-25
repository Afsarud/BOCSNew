using Microsoft.AspNetCore.Mvc;

namespace BOCS.ViewComponents
{
    public class StudentEnrolledMenuViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            if (!(User?.Identity?.IsAuthenticated ?? false))
                return Content(string.Empty);

            return View();
        }
    }
}