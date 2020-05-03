using BE.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Schema("score")]
public class ScoreModel: BEModel
{
    public string user;
    public UserModel userRefs;
    public int score;
}
