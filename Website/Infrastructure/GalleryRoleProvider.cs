using System;
using System.Linq;
using System.Web.Security;
using DotNetCasClient;
using Ninject;

namespace NuGetGallery.Infrastructure
{
  public class GalleryRoleProvider : RoleProvider
  {
    public override bool IsUserInRole(string username, string roleName)
    {
      var user = GetUser(username);
      return user != null && user.Roles.Any(_ => _.Name == roleName);
    }

    public override string[] GetRolesForUser(string username)
    {
      var user = GetUser(username);
      return user != null ? user.Roles.Select(_ => _.Name).ToArray() : new string[0];
    }

    private static User GetUser(string username)
    {
      var user = Container.Kernel.Get<IUserService>().FindByUsername(username);
      if (user != null)
        return user;
      var ticketManager = CasAuthentication.ServiceTicketManager;
      if (ticketManager == null)
        return null;
      var formsAuthenticationTicket = CasAuthentication.GetFormsAuthenticationTicket();
      if (formsAuthenticationTicket == null)
        return null;
      var serviceTicket = formsAuthenticationTicket.UserData;
      if (string.IsNullOrEmpty(serviceTicket))
        return null;
      var ticket = ticketManager.GetTicket(serviceTicket);
      if (ticket == null)
        return null;
      string emailAddress = null;
      var userService = Container.Kernel.Get<IUserService>();
      foreach (var email in ticket.Assertion.Attributes["mail"])
      {
        emailAddress = email;
        user = userService.FindByEmailAddress(emailAddress);
        if (user != null) break;
      }
      if (user == null)
      {
        user = userService.Create(formsAuthenticationTicket.Name, Guid.NewGuid().ToString(), emailAddress);
        userService.ConfirmEmailAddress(user, user.EmailConfirmationToken);
      }
      return user;
    }

    public override void CreateRole(string roleName)
    {
      throw new System.NotImplementedException();
    }

    public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
    {
      throw new System.NotImplementedException();
    }

    public override bool RoleExists(string roleName)
    {
      throw new System.NotImplementedException();
    }

    public override void AddUsersToRoles(string[] usernames, string[] roleNames)
    {
      throw new System.NotImplementedException();
    }

    public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
    {
      throw new System.NotImplementedException();
    }

    public override string[] GetUsersInRole(string roleName)
    {
      throw new System.NotImplementedException();
    }

    public override string[] GetAllRoles()
    {
      throw new System.NotImplementedException();
    }

    public override string[] FindUsersInRole(string roleName, string usernameToMatch)
    {
      throw new System.NotImplementedException();
    }

    public override string ApplicationName { get; set; }
  }
}