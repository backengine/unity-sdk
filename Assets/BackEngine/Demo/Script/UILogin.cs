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

                UserModel model = new UserModel();
                model.email = Email.text;
                model.password = Password.text;

                BERequest.Instance.Auth(model, (error, response) =>
                {
                    if (!error)
                    {
                        model = response.data;
                        Manager.User = model;
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
