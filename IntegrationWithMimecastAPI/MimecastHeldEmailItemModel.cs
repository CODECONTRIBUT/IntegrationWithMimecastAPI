using Elmah;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationWithMimecastAPI
{
    public class MimecastHeldEmailItemModel
    {
        public int HeldEmailId { get; set; }

        public string SenderName { get; set; }

        public string SenderEmailAddress { get; set; }

        public string ReceiverName { get; set; }

        public string ReceiverEmailAddress { get; set; }

        public string Subject { get; set; }

        public string HeldReason { get; set; }

        public int? EmailSize { get; set; }

        public string Route { get; set; }

        public string PolicyInfo { get; set; }

    }


    //Based on Mimecase request and response
    public class Meta
    {
        public Pagination pagination { get; set; }

        public int? status { get; set; }
    }
    public class Pagination
    {
        public int pageSize { get; set; }

        public string pageToken { get; set; }

        public string next { get; set; }

        public int totalCount { get; set; }
    }

    public class MimecastRootRequest
    {
        public Meta meta { get; set; }
        public List<DataItem> data { get; set; }
    }

    public class DataItem
    {
        public Boolean admin { get; set; }

        public string start { get; set; }

        public string end { get; set; }
    }

    public class Root
    {
        public Meta meta { get; set; }

        public fail[] fail { get; set; }

        //List<T>
        public List<ResponseDataItem> data { get; set; }
    }

    public class fail
    {
        public errors[] errors { get; set; }
    }

    public class errors
    {
        public string message { get; set; }

        public string code { get; set; }

        public bool retryable { get; set; }
    }

    public class ResponseDataItem
    {
        public int Id { get; set; }

        public From from { get; set; }

        public To to { get; set; }

        public string subject { get; set; }

        public string reason { get; set; }

        public int size { get; set; }

        public string route { get; set; }

        public string policyInfo { get; set; }
    }

    public class From
    {
        public string displayableName { get; set; }
        public string emailAddress { get; set; }
    }

    public class To
    {
        public string displayableName { get; set; }
        public string emailAddress { get; set; }
    }
}
