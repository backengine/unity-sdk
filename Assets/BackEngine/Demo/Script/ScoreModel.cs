using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Schema("score")]
public class ScoreModel
{
    public string id;
    public string user;
    public UserModel userRefs;
    public int score;
}
