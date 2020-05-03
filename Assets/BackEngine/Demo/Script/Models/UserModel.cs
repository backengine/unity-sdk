using BE.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Schema("users")]
public class UserModel : BEModel
{
    public string name;
    public string email;
    public string password;
    public string deviceId;
    public int score;
}