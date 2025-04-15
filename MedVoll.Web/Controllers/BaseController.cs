using MedVoll.Web.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MedVoll.Web.Controllers
{
    public class BaseController : Controller
    {
        public BaseController()
        {
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            ViewData["Especialidades"] = GetEspecialidades();
            ViewData["VollMedCard"] = HttpContext.Session.GetString("VollMedCard");
            base.OnActionExecuting(context);
        }

        private List<Especialidade> GetEspecialidades()
        {
            var especialidades = (Especialidade[])Enum.GetValues(typeof(Especialidade));
            return especialidades.ToList();
        }
    }
}
