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
                var requestData = new RequestData<UserModel>();
                var st =20.23f;
                int score = Mathf.RoundToInt(st);
                string deviceId = "123435465656";
                requestData = requestData.Where(user => user.deviceId == deviceId).SetValue(user => user.score, score);
                BERequest.Instance.UpdateOne(requestData, (error, response) =>
                {

                });


               requestData =requestData.Where(
                    x => (x.email==Email.text) && (x.password== Password.text)
                );
                requestData.SetValue(x => x.email , "bongvd");
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
