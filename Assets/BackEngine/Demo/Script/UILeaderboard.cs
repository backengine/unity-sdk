using System.Collections;
using System.Collections.Generic;
using BE.Models;
using BE.NetWork;
using UnityEngine;
using UnityEngine.UI;

namespace BE.Demo
{

    public class UILeaderboard : MonoBehaviour
    {
        public DemoManager Manager;

        public Text Name;
        public Text Email;
        public Text BestScore;
        public Text CurrentScore;

        int currentScore=0;

        ScoreModel myScore;

        public GameObject ScorePrefab;
        public Transform ScoreContainer;

        private void Start()
        {
            Name.text = Manager.User.name;
            Email.text = Manager.User.email;
            RequestScores();
        }

        public void RequestScores()
        {
            Manager.ShowLoading();
            var requestData = new RequestData<ScoreModel>();
            requestData.GetField(x=>x.score).GetRefs(x=>x.user).Sort(x=>x.score, SortType.Desc);
            BERequest.Instance.SelectMany(requestData, (error, response) => {
                Manager.HideLoading();
                if (!error)
                {
                    for(int index=ScoreContainer.childCount-1; index>0; index--)
                    {
                        Destroy(ScoreContainer.GetChild(index).gameObject);
                    }
                    List<ScoreModel> scores =response.data;
                    myScore = scores.Find(score => score.userRefs.id == Manager.User.id);
                    if (myScore != null)
                        BestScore.text = "BestScore: " + myScore.score.ToString();
                    else BestScore.text = "BestScore: 0";
                    if(scores!=null && scores.Count > 0)
                    {
                        for (int index =0; index<scores.Count;index++) {
                            GameObject scoreItem = Instantiate(ScorePrefab, ScoreContainer);
                            scoreItem.GetComponent<ScoreItem>().Init(index+1, scores[index]);
                            scoreItem.SetActive(true);
                        }
                    }
                }
            });
        }

        public void OnAddPointClick()
        {
            currentScore++;
            CurrentScore.text = currentScore.ToString();
        }

        public void OnSubmitScore()
        {
            if (myScore!=null && currentScore <= myScore.score)
            {
                Debug.Log("Your current score less than your best score.");
            }
            else
            {
                Manager.ShowLoading();
                if (myScore != null)
                {
                    myScore.score = currentScore;
                    BERequest.Instance.UpdateOne(myScore, (error, response) => {
                        Manager.HideLoading();
                        if (!error)
                        {
                            RequestScores();
                        }
                    });
                }
                else
                {
                    myScore = new ScoreModel();
                    myScore.user = Manager.User.id;
                    myScore.score = currentScore;
                    BERequest.Instance.Insert(myScore, (error, response) => {
                        if (!error)
                        {
                            myScore = response.data;
                            RequestScores();
                        }
                    });
                }
            }
        }
    }
}
