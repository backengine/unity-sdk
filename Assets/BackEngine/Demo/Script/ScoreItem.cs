using System.Collections;
using System.Collections.Generic;
using BE.Models;
using BE.NetWork;
using UnityEngine;
using UnityEngine.UI;

namespace BE.Demo {

    public class ScoreItem : MonoBehaviour
    {
        public Text Index;
        public Text Name;
        public Text Score;
        ScoreModel scoreModel;

        public void Init(int index, ScoreModel scoreModel)
        {
            Index.text = index + ".";
            Name.text = scoreModel.userRefs.name;
            Score.text = scoreModel.score.ToString();
            this.scoreModel = scoreModel;
        }

        public void Delete()
        {
            //RequestData requestData = new RequestData();
           // requestData = requestData.Where(x => x.Equals("id", scoreModel.id));
            BERequest.Instance.DeleteOne(scoreModel, (error, response) => {
                if (!error)
                {
                    GameObject.Find("ScorePanel").GetComponent<UILeaderboard>().RequestScores();
                }
            });
        }
    }
}
