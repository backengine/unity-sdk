using System.Collections;
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
                RequestData requestData = new RequestData();
                requestData=requestData.Where(
                    x => x["email"].Equals(Email.text) & x["password"].Equals(Password.text)
                );
                BERequest.Instance.Auth<UserModel>("users", requestData, (error, response) =>
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
