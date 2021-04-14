﻿using Microsoft.AspNet.Identity;
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
            // Se adauga rolurile aplicatiei
            if (!roleManager.RoleExists("Admin"))
            {
                // Se adauga rolul de administrator
                var role = new IdentityRole();
                role.Name = "Admin";
                roleManager.Create(role);
                // se adauga utilizatorul administrator
                var user = new ApplicationUser();
                user.UserName = "admin@gmail.com";
                user.Email = "admin@gmail.com"; // am schimbat slightly datele de logare pt admin
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
        }
    }
}
