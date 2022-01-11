using Demo93.Feature.Navigation.Repository;
using Sitecore.Mvc.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Demo93.Feature.Navigation.Controllers
{
    public class NavigationController : SitecoreController
    {
        private readonly INavigationRepository _navigationRepository;

        public NavigationController(INavigationRepository navigationRepository)
        {
            _navigationRepository = navigationRepository;
        }

        // GET: Navigation
        public ActionResult Header()
        {
            var headerModel = _navigationRepository.GetHeader();
            return View("/Views/Demo93/Navigation/Header.cshtml", headerModel);
        }
    }
}