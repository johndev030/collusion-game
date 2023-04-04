using System;
using System.Collections.Generic;

[Serializable]
public class Group
{
    public string Id { get; set; }
    public string Type { get; set; }
    public string TypeString { get; set; }

}
[Serializable]
public class Roles
{
    public string RoleName { get; set; }
    public string RoleId { get; set; }

}
[Serializable]
public class Groups
{
    public string GroupName { get; set; }
    public Group Group { get; set; }
    public int ProfileVersion { get; set; }
    public List<Roles> Roles { get; set; }

}
[Serializable]
public class AllGroups
{
    public List<Groups> Groups { get; set; }

}