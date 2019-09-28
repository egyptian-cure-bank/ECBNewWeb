using System.Web;
using System.Web.Optimization;

namespace ECBNewWeb
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-3.0.0.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/chosen").Include(
                        "~/Scripts/chosen.jquery.js"));
            
            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));
            bundles.Add(new ScriptBundle("~/bundles/jqueryui").Include("~/Scripts/jquery-ui-{version}.js"));
            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));
            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.min.js",
                      "~/Scripts/respond.js"));
            bundles.Add(new ScriptBundle("~/bundles/sweetalert").Include("~/Content/assets/demo/default/custom/components/base/sweetalert2.js"));
            bundles.Add(new ScriptBundle("~/bundles/select2").Include("~/Content/assets/demo/default/custom/crud/forms/widgets/select2.js"));
            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/dataTables.bootstrap4.min.css",
                      "~/Content/assets/vendors/custom/datatables/datatables.bundle.rtl.css",
                      "~/Content/assets/demo/default/base/select.dataTables.min.css",
                      "~/Content/site.css",
                      //"~/Content/font-awesome.css",
                      "~/Content/chosen.css"));
        }
    }
}
