﻿namespace UZonMail.Core.Services.SendCore.Sender
{
    public class ResultParser
    {
        private string _result;
        public ResultParser(string result)
        {
            _result = result;
        }


        // 返回回执 id
        public string GetReceiptId()
        {
            return _result;
        }
    }
}
