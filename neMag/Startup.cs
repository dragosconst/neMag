using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(neMag.Startup))]
namespace neMag
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
        
    }
}
