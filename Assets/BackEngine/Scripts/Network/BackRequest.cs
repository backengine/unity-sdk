﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using BE.Models;
using BE.Util;
using UnityEngine;
using UnityEngine.Networking;

namespace BE.NetWork
{
    /// <summary>
    /// Class makes requests
    /// </summary>
    public class BERequest : MonoBehaviour
    {

        public static BERequest Instance
        {
            get
            {
                object obj = deadLock;
                lock (obj)
                {
                    if (instance == null)
                    {
                        instance = FindObjectOfType<BERequest>();
                    }
                    if (instance == null)
                    {
                        GameObject gameObject = new GameObject("BACK_REQUEST");
                        instance = gameObject.AddComponent<BERequest>();
                        DontDestroyOnLoad(gameObject);
                    }
                }
                return instance;
            }
        }

        private static BERequest instance;

        private static object deadLock = new object();

        private BackConfiguration backConfig;

        private string token;

        private void Start()
        {
            startSession();
        }

        public void Auth<T>(string schema, RequestData requestData, Action<bool, BackResponse<T>> callback = null) where T : BEModel
        {
            string data = Helper.GetRequestString("auth", schema, requestData);
            StartCoroutine(ProcessQuery(data, callback));
        }
        public void Auth<T>(RequestData requestData, Action<bool, BackResponse<T>> callback = null) where T : BEModel
        {
            Type t = typeof(T);
            var schema = GetSchema(t);
            string data = Helper.GetRequestString("auth", schema, requestData);
            StartCoroutine(ProcessQuery(data, callback));
        }
        public void Auth<T>(RequestData<T> requestData, Action<bool, BackResponse<T>> callback = null) where T : BEModel
        {
            Type t = typeof(T);
            var schema = GetSchema(t);
            string data = Helper.GetRequestString("auth", schema, requestData);
            StartCoroutine(ProcessQuery(data, callback));
        }
        public void Auth<T>(T o, Action<bool, BackResponse<T>> callback = null) where T : BEModel
        {
            Type t = typeof(T);
            var schema = GetSchema(t);
            var requestData = new RequestData<T>();
            Helper.MakeWhereRequest(t, o, requestData, false);
            string data = Helper.GetRequestString("auth", schema, requestData);
            StartCoroutine(ProcessQuery(data, callback));
        }
        protected string GetSchema(Type t)
        {
            var attr = t.GetCustomAttribute(typeof(SchemaAttribute));
            var schema = "";
            if (attr != null)
            {
                schema = ((SchemaAttribute)attr).Name;
            }
            else
            {
                schema = t.Name;
            }
            return schema;
        }
        public void InsertAuth<T>(T o, Action<bool, BackResponse<T>> callback = null) where T : BEModel
        {
            var schema = GetSchema(typeof(T));
            InsertAuth(schema, o, callback);
        }
        public void InsertAuth<T>(string schema, T o, Action<bool, BackResponse<T>> callback = null) where T : BEModel
        {
            Type t = o.GetType();
            RequestData requestData = new RequestData();
            Helper.SetValueRequest(t, o, requestData);
            string data = Helper.GetRequestString("insertAuth", schema, requestData);
            StartCoroutine(ProcessQuery(data, callback));
        }

        /// <summary>
        /// Execute a select "schema" command with the filter condition of "conditions"
        /// </summary>
        ///
        /// <param name="schema">Name of Schema need to query</param>
        /// <param name="requestData">Request Data Object</param>
        public void SelectMany<T>(RequestData<T> requestData = null, Action<bool, BackResponse<List<T>>> callback = null) where T : BEModel
        {
            var schema = GetSchema(typeof(T));
            if (requestData != null)
            {
                Helper.DefaultIncludeFields(requestData);
            }
            SelectMany<T>(schema, requestData, callback);
        }
        /// <summary>
        /// Execute a select "schema" command with the filter condition of "conditions"
        /// </summary>
        ///
        /// <param name="schema">Name of Schema need to query</param>
        /// <param name="requestData">Request Data Object</param>
        public void SelectMany<T>(string schema, RequestData requestData = null, Action<bool, BackResponse<List<T>>> callback = null) where T : class
        {
            string data = Helper.GetRequestString("select", schema, requestData);
            StartCoroutine(ProcessQuery(data, callback));
        }
        /// <summary>
        /// Execute a select "schema" command with the filter condition of "conditions"
        /// </summary>
        ///
        /// <param name="schema">Name of Schema need to query</param>
        /// <param name="requestData">Request Data Object</param>
        public void SelectMany<T>(Action<bool, BackResponse<List<T>>> callback = null) where T : BEModel
        {
            SelectMany<T>(null, callback);
        }
        /// <summary>
        /// Execute a select one document command with the filter condition of "conditions", the return is random if there are more than one documents
        /// </summary>
        ///
        /// <param name="schema">Name of Schema need to query</param>
        /// <param name="requestData">Request Data Object</param>
        public void SelectOne<T>(string schema, RequestData requestData = null, Action<bool, BackResponse<T>> callback = null) where T : class
        {
            string data = Helper.GetRequestString("selectOne", schema, requestData);
            StartCoroutine(ProcessQuery<T>(data, callback));
        }
        /// <summary>
        /// Execute a select one document command with the filter condition of "conditions", the return is random if there are more than one documents
        /// </summary>
        ///
        /// <param name="requestData">Request Data Object</param>
        public void SelectOne<T>(RequestData<T> requestData = null, Action<bool, BackResponse<T>> callback = null) where T : BEModel
        {
            var schema = GetSchema(typeof(T));
            if (requestData != null)
            {
                Helper.DefaultIncludeFields(requestData);
            }
            SelectOne(schema, requestData, callback);
        }
        /// <summary>
        /// Excute a insert into "schema" command with the insertFields data
        /// </summary>
        /// <param name="schema"></param>
        /// <param name="requestData">Request Data Object</param>
        public void Insert<T>(string schema, RequestData requestData, Action<bool, BackResponse<T>> callback = null) where T : BEModel
        {
            string data = Helper.GetRequestString("insert", schema, requestData);
            StartCoroutine(ProcessQuery(data, callback));
        }
        /// <summary>
        /// Excute a insert into "schema" command with the insertFields data
        /// </summary>
        /// <param name="o">The object related to table need to insert</param>
        public void Insert<T>(T o, Action<bool, BackResponse<T>> callback = null) where T : class
        {
            Type t = o.GetType();
            RequestData requestData = new RequestData();
            Helper.SetValueRequest(t, o, requestData);
            var attr = t.GetCustomAttribute(typeof(SchemaAttribute));
            var schema = "";
            if (attr != null)
            {
                schema = ((SchemaAttribute)attr).Name;
            }
            else
            {
                schema = t.Name;
            }
            string data = Helper.GetRequestString("insert", schema, requestData);
            StartCoroutine(ProcessQuery(data, callback));
        }

        /// <summary>
        /// Execute an update "schema" command with the filter condition of "conditions" and updateFields values
        /// </summary>
        /// <param name="schema"></param>
        /// <param name="requestData">Request Data Object</param>
        public void UpdateMany(string schema, RequestData requestData, Action<bool, BackResponse<int>> callback = null)
        {
            string data = Helper.GetRequestString("update", schema, requestData);
            StartCoroutine(ProcessQuery(data, callback));
        }
        /// <summary>
        /// Execute an update "schema" command with the filter condition of "conditions" and updateFields values
        /// </summary>
        /// <param name="requestData">Request Data Object</param>
        public void UpdateMany<T>(RequestData<T> requestData, Action<bool, BackResponse<int>> callback = null) where T : BEModel
        {
            var schema = GetSchema(typeof(T));
            string data = Helper.GetRequestString("update", schema, requestData);
            StartCoroutine(ProcessQuery(data, callback));
        }
        /// <summary>
        /// Execute an update "schema" command with the filter condition of "conditions" and updateFields values
        /// </summary>
        /// <param name="schema"></param>
        /// <param name="requestData">Request Data Object</param>
        public void UpdateOne(string schema, RequestData requestData, Action<bool, BackResponse<int>> callback = null)
        {
            string data = Helper.GetRequestString("updateOne", schema, requestData);
            StartCoroutine(ProcessQuery(data, callback));
        }
        /// <summary>
        /// Excute a insert into "schema" command with the insertFields data
        /// </summary>
        /// <param name="o">The object related to table need to insert</param>
        public void UpdateOne<T>(RequestData<T> requestData, Action<bool, BackResponse<T>> callback = null) where T : BEModel
        {
            Type t = typeof(T);
            var schema = GetSchema(t);
            string data = Helper.GetRequestString("updateOne", schema, requestData);
            StartCoroutine(ProcessQuery(data, callback));
        }

        /// <summary>
        /// Excute a insert into "schema" command with the insertFields data
        /// </summary>
        /// <param name="o">The object related to table need to insert</param>
        public void UpdateOne<T>(T o, Action<bool, BackResponse<T>> callback = null) where T : BEModel
        {
            Type t = o.GetType();
            RequestData requestData = new RequestData();
            Helper.SetValueRequest(t, o, requestData, true);
            Helper.MakeWhereRequest(t, o, requestData);
            var schema = GetSchema(t);
            string data = Helper.GetRequestString("updateOne", schema, requestData);
            StartCoroutine(ProcessQuery(data, callback));
        }


        /// <summary>
        /// Execute a delete command in "schema" with the filter condition of "conditions"
        /// </summary>
        /// <param name="schema"></param>
        /// <param name="requestData">Request Data Object</param>
        public void DeleteMany(string schema, RequestData requestData, Action<bool, BackResponse<int>> callback = null)
        {

            string data = Helper.GetRequestString("delete", schema, requestData);
            StartCoroutine(ProcessQuery(data, callback));
        }

        /// <summary>
        /// Execute a delete command in "schema" with the filter condition of "conditions"
        /// </summary>
        /// <param name="requestData">Request Data Object</param>
        public void DeleteMany<T>(RequestData<T> requestData, Action<bool, BackResponse<int>> callback = null) where T : BEModel
        {
            string schema = GetSchema(typeof(T));
            string data = Helper.GetRequestString("delete", schema, requestData);
            StartCoroutine(ProcessQuery(data, callback));
        }
        /// <summary>
        /// Execute a delete command in "schema" with the filter condition of "conditions"
        /// </summary>
        /// <param name="schema"></param>
        /// <param name="requestData">Request Data Object</param>
        public void DeleteOne(string schema, RequestData requestData, Action<bool, BackResponse<int>> callback = null)
        {
            string data = Helper.GetRequestString("deleteOne", schema, requestData);
            StartCoroutine(ProcessQuery(data, callback));
        }
        /// <summary>
        /// Execute a delete command in "schema" with the filter condition of "conditions"
        /// </summary>
        /// <param name="schema"></param>
        /// <param name="requestData">Request Data Object</param>
        public void DeleteOne<T>(T o, Action<bool, BackResponse<int>> callback = null) where T : BEModel
        {
            RequestData request = new RequestData();
            Type t = o.GetType();
            RequestData requestData = new RequestData();
            Helper.MakeWhereRequest(t, o, requestData);
            var attr = t.GetCustomAttribute(typeof(SchemaAttribute));
            var schema = "";
            if (attr != null)
            {
                schema = ((SchemaAttribute)attr).Name;
            }
            else
            {
                schema = t.Name;
            }
            string data = Helper.GetRequestString("deleteOne", schema, requestData);
            StartCoroutine(ProcessQuery(data, callback));
        }

        void startSession()
        {
            BELog log = new BELog();
            var requestData = new RequestData();
            Helper.SetValueRequest(typeof(BELog), log, requestData, true);
            string data = Helper.GetRequestString("startSession", null, requestData);
            StartCoroutine(ProcessQuery<BELog>(data, null));
        }

        /// <summary>
        /// Process request query
        /// </summary>
        /// <param name="jsonData"></param>
        /// <returns></returns>
        IEnumerator ProcessQuery<T>(string jsonData, Action<bool, BackResponse<T>> callback)
        {
            Debug.Log(jsonData);
            if (backConfig == null)
            {
                backConfig = Resources.Load<BackConfiguration>("BackConfig");
            }
            if (backConfig == null)
            {
                Debug.LogError("Cannot find BackConfiguration object, please try import package again.");
                yield break;
            }
            if (string.IsNullOrEmpty(backConfig.appSecret))
            {
                Debug.LogError("You must input a valid App Secret to BackConfig object in BackEngine/Resources folder.");
                yield break;
            }
            using (UnityWebRequest request = UnityWebRequest.Put(backConfig.getEndPoint() + "/dynamic", jsonData))
            {
                request.method = "POST";
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("Secret", "Bearer " + backConfig.appSecret);
                if (!string.IsNullOrEmpty(token))
                {
                    request.SetRequestHeader("Authorization", "Bearer " + token);
                }
                yield return request.SendWebRequest();
                BackResponse<T> backResponse;
                if (request.isNetworkError)
                {
                    backResponse = new BackResponse<T>();
                    backResponse.isError = true;
                    backResponse.message = request.error;
                    Debug.Log(request.error);
                }
                else if (request.isHttpError)
                {
                    backResponse = new BackResponse<T>();
                    backResponse.isError = true;
                    backResponse.message = string.IsNullOrEmpty(request.downloadHandler.text) ? request.error : request.downloadHandler.text;
                    Debug.Log(backResponse.message);
                }
                else
                {
                    Debug.Log(request.downloadHandler.text);
                    backResponse = request.downloadHandler.text.ToObject<BackResponse<T>>();
                    if (!string.IsNullOrEmpty(backResponse.token))
                    {
                        token = backResponse.token;
                    }
                }
                callback?.Invoke(backResponse.isError, backResponse);
            }
        }

    }

    public class BackResponse : BackResponse<object>
    {

    }
    /// <summary>
    /// Response class for BackRequest
    /// </summary>
    public class BackResponse<T>
    {
        /// <summary>
        /// return true if request is error
        /// </summary>
        public bool isError;

        /// <summary>
        /// response message
        /// </summary>
        public string message;

        /// <summary>
        /// response token
        /// </summary>
        public string token;

        /// <summary>
        /// Object contains all resposne data
        /// </summary>
        [JsonName("data")]
        public T data;
        public int totalPage;
    }

}
