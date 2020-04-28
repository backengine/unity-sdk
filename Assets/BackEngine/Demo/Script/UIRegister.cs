using System.Collections;
using System.Collections.Generic;
using BE.Models;
using BE.NetWork;
using UnityEngine;
using UnityEngine.UI;

namespace BE.Demo
{

    public class UIRegister : MonoBehaviour
    {
        public DemoManager Manager;
        public InputField Name;
        public InputField Email;
        public InputField Password;

        public void OnRegisterClick()
        {
            if (!string.IsNullOrEmpty(Email.text) && !string.IsNullOrEmpty(Password.text)&&!string.IsNullOrEmpty(Name.text))
            {
                Manager.ShowLoading();
                UserModel model = new UserModel();
                model.email = Email.text;
                model.name = Name.text;
                model.password = Password.text;
                BERequest.Instance.InsertAuth(model, (error, response) => {
                    if (!error)
                    {
                        UserModel user = response.data;
                        Manager.User = user;
                        Manager.HideRegister();
                        Manager.ShowLeader();
                    }
                    else
                    {

                    }
                    Manager.HideLoading();
                });
            }
        }

        public void OnLoginClick()
        {
            Manager.HideRegister();
            Manager.ShowLogin();
        }
    }
}
