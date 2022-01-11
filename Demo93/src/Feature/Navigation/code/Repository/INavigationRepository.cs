using Demo93.Feature.Navigation.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Demo93.Feature.Navigation.Repository
{
    public interface INavigationRepository
    {
        HeaderModel GetHeader();
    }
}