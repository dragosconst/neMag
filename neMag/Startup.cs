using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin;
using neMag.Models;
using Owin;

[assembly: OwinStartupAttribute(typeof(neMag.Startup))]
namespace neMag
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);

            // create roles
            CreateAdminUserAndApplicationRoles();
        }

        private void CreateAdminUserAndApplicationRoles()
        {
            ApplicationDbContext context = new ApplicationDbContext();
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
            var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
            
            // Se Adaugă rolurile aplicatiei
            if (!roleManager.RoleExists("Admin"))
            {
                // Se Adaugă rolul de administrator
                var role = new IdentityRole();
                role.Name = "Admin";
                roleManager.Create(role);
                
                // se Adaugă utilizatorul administrator
                var user = new ApplicationUser();

                user.LastName = "Adminescu";
                user.FirstName = "Adminu";
                user.UserName = "admin1@gmail.com";
                user.Email = "admin1@gmail.com"; // am schimbat slightly datele de logare pt admin
              
                var adminCreated = UserManager.Create(user, "parolasimpla");

                if (adminCreated.Succeeded)
                {
                    UserManager.AddToRole(user.Id, "Admin");
                }
            }

            if (!roleManager.RoleExists("Collaborator"))
            {
                var role = new IdentityRole();
                role.Name = "Collaborator";
                roleManager.Create(role);
            }


            if (!roleManager.RoleExists("User"))
            {
                var role = new IdentityRole();
                role.Name = "User";
                roleManager.Create(role);
            }

            if (!roleManager.RoleExists("RestrictedUser"))
            {
                var role = new IdentityRole();
                role.Name = "RestrictedUser";
                roleManager.Create(role);
            }

            // for testing
            var userCol = new ApplicationUser();

             userCol.UserName = "col1@gmail.com";
             userCol.Email = "col1@gmail.com";
          
            userCol.FirstName = "colaborator";
            userCol.LastName = "colaborator";
            var userCreated = UserManager.Create(userCol, "parolasimpla");

            if(userCreated.Succeeded)
            {
                UserManager.AddToRole(userCol.Id, "Collaborator");
            }

            var userCol2 = new ApplicationUser();
            userCol2.UserName = "user1@gmail.com";
            userCol2.Email = "user1@gmail.com";
            userCol2.FirstName = "user";
            userCol2.LastName = "user";
            var userCreated2 = UserManager.Create(userCol2, "parolasimpla");

            if (userCreated2.Succeeded)
            {
                UserManager.AddToRole(userCol2.Id, "User");
            }
        }
    }
}
