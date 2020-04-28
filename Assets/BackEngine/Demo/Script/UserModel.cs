using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Schema("users")]
public class UserModel
{
    [ColumnIdentity()]
    public string id;
    public string name;
    public string email;
    public string password;
}