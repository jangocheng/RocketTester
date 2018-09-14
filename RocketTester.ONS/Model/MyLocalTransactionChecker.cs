﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Web;
using System.Configuration;
using Newtonsoft.Json;
using ons;
using RocketTester.ONS.Util;
using Redis.Framework;

namespace RocketTester.ONS.Model
{
    public class MyLocalTransactionChecker : LocalTransactionChecker
    {
        static string _RedisExchangeHosts = ConfigurationManager.AppSettings["RedisExchangeHosts"] ?? "";
        static int _ONSRedisDBNumber = string.IsNullOrEmpty(ConfigurationManager.AppSettings["ONSRedisDBNumber"]) ? 11 : int.Parse(ConfigurationManager.AppSettings["ONSRedisDBNumber"]);
        static int _ONSRedisTransactionResultExpireIn = string.IsNullOrEmpty(ConfigurationManager.AppSettings["ONSRedisTransactionResultExpireIn"]) ? 18000 : int.Parse(ConfigurationManager.AppSettings["ONSRedisTransactionResultExpireIn"]);

        /*/
        Func<T, string> _func;
        T _model;

        public string FuncResult { get; set; }

        public MyLocalTransactionChecker(Func<T, string> fun, T model)
        {
            _func = fun;
            _model = model;
        }
        //*/

        public MyLocalTransactionChecker()
        {

        }

        ~MyLocalTransactionChecker()
        {
        }

        public override TransactionStatus check(Message value)
        {
            Console.WriteLine("check topic: {0}, tag:{1}, key:{2}, msgId:{3},msgbody:{4}, userProperty:{5}",
            value.getTopic(), value.getTag(), value.getKey(), value.getMsgID(), value.getBody(), value.getUserProperties("VincentNoUser"));
            // 消息 ID(有可能消息体一样，但消息 ID 不一样。当前消息 ID 在控制台无法查询)
            //string msgId = value.getMsgID();
            // 消息体内容进行 crc32, 也可以使用其它的如 MD5
            // 消息 ID 和 crc32id 主要是用来防止消息重复
            // 如果业务本身是幂等的， 可以忽略，否则需要利用 msgId 或 crc32Id 来做幂等
            // 如果要求消息绝对不重复，推荐做法是对消息体 body 使用 crc32或 md5来防止重复消息 
            TransactionStatus transactionStatus = TransactionStatus.Unknow;
            string key = value.getKey();
            LogHelper.Log("MyLocalTransactionChecker.execute.key  " + key);

            try
            {
                LogHelper.Log("MyLocalTransactionChecker.execute.after try...");
                LogHelper.Log("MyLocalTransactionChecker.execute.checkerFunc " + value.getUserProperties("checkerFunc"));
                LogHelper.Log("MyLocalTransactionChecker.execute.checkerFuncModel" + value.getUserProperties("checkerFuncModel"));
                //*
                Func<string, TransactionResult> checkerFunc = ONSHelper.CheckerFuncDictionary[value.getUserProperties("checkerFunc")];
                string checkerFuncModel = value.getUserProperties("checkerFuncModel");
                TransactionResult transactionResult = checkerFunc(checkerFuncModel);

                LogHelper.Log("MyLocalTransactionChecker.execute.data:" + transactionResult.Data);
                LogHelper.Log("MyLocalTransactionChecker.execute.isToPush:" + transactionResult.IsToPush);

                string result = JsonConvert.SerializeObject(transactionResult);

                try
                {
                    RedisTool RT = new RedisTool(_ONSRedisDBNumber, _RedisExchangeHosts);
                    bool isSaved = RT.StringSet(key, result, TimeSpan.FromSeconds(_ONSRedisTransactionResultExpireIn));
                    if (!isSaved)
                    {
                        transactionStatus = TransactionStatus.Unknow;
                        return transactionStatus;
                    }
                    LogHelper.Log("MyLocalTransactionChecker.execute.result:true");
                }
                catch (Exception e)
                {
                    LogHelper.Log("MyLocalTransactionChecker.execute.result:false, error:" + e.Message);
                }

                if (transactionResult.IsToPush)
                {
                    // 本地事务成功、提交消息
                    transactionStatus = TransactionStatus.CommitTransaction;
                }
                else
                {
                    // 本地事务失败、回滚消息
                    transactionStatus = TransactionStatus.RollbackTransaction;
                }
            }
            catch (Exception e)
            {
                //exception handle
                LogHelper.Log("MyLocalTransactionChecker.execute.error:" + e.Message);
            }
            return transactionStatus;
        }
    }
}
