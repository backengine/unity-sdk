using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Schema("users")]
public class UserModel
{
    public string id;
    public string name;
    public string email;
    public string password;
    public string deviceId;
    public int score;
}