﻿using System.Collections;
using System.Collections.Generic;
using BE.Models;
using BE.NetWork;
using UnityEngine;
using UnityEngine.UI;

namespace BE.Demo
{

    public class UILogin : MonoBehaviour
    {
        public DemoManager Manager;
        public InputField Email;
        public InputField Password;

        public void OnLoginClick()
        {
            if (!string.IsNullOrEmpty(Email.text) && !string.IsNullOrEmpty(Password.text))
            {
                Manager.ShowLoading();
                var requestData = new RequestData<UserModel>();
                List<string> listEmails = new List<string>();
                listEmails.Add( Email.text);
                requestData = requestData.Where(
                    x => listEmails.Contains( x.email)&& x.password == Password.text
                );
                BERequest.Instance.Auth(requestData, (error, response) =>
                {
                    if (!error)
                    {
                        UserModel user = response.data;
                        Manager.User = user;
                        Manager.HideLogin();
                        Manager.ShowLeader();
                    }
                    else
                    {

                    }
                    Manager.HideLoading();
                });
            }
        }

        public void OnRegisterClick()
        {
            Manager.HideLogin();
            Manager.ShowRegister();
        }
    }
}
