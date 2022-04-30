//This Page is used for testing to learn how use the framework.


using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MyApp.Namespace
{

    public class TestModel : PageModel
    {
        [BindProperty(SupportsGet=true)]
        public string val {get; set;}


                    [BindProperty(SupportsGet=true)]
                   public List<int> Ints {get; set;} = new List<int>(){2,3,4,5} ;
        public void OnGet()
        {
        }

        public void OnPostTest()
        {
            val ="POST TEST";

        }
        [ActionName("Get1")]
        public void OnGetTest()
        {
            val ="GET TEST";

        }

        public void OnPostCustom()
        {
            val = "POST CUSTOM";
        }


    }
}
