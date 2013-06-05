using System.Linq;
using System.Web.Security;
using Ninject;

namespace NuGetGallery.Infrastructure
{
  public class GalleryRoleProvider : RoleProvider
  {
    public override bool IsUserInRole(string username, string roleName)
    {
      var user = Container.Kernel.Get<IUserService>().FindByUsername(username);
      return user != null && user.Roles.Any(_ => _.Name == roleName);
    }

    public override string[] GetRolesForUser(string username)
    {
      var user = Container.Kernel.Get<IUserService>().FindByUsername(username);
      return user != null ? user.Roles.Select(_ => _.Name).ToArray() : new string[0];
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