using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BE.Demo
{

    public class DemoManager : MonoBehaviour
    {
        public GameObject LoadingPanel;
        public GameObject LoginPanel;
        public GameObject RegisterPanel;
        public GameObject ScorePanel;
        public UserModel User;

        public void ShowLogin()
        {
            LoginPanel.SetActive(true);
        }

        public void HideLogin()
        {
            LoginPanel.SetActive(false);
        }

        public void ShowRegister()
        {
            RegisterPanel.SetActive(true);
        }

        public void HideRegister()
        {
            RegisterPanel.SetActive(false);
        }

        public void ShowLeader()
        {
            ScorePanel.SetActive(true);
        }

        public void HideLeader()
        {
            ScorePanel.SetActive(false);
        }

        public void ShowLoading()
        {
            LoadingPanel.SetActive(true);
        }

        public void HideLoading()
        {
            LoadingPanel.SetActive(false);
        }

    }

}