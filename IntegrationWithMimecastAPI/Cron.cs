using Elmah;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace IntegrationWithMimecastAPI
{
    public class Cron
    {
        public List<MimecastHeldEmailItemModel> GetAndSaveMimecastHeldEmails()
        {
            try
            {
                //Initialize
                var heldEmailList = new List<MimecastHeldEmailItemModel>();
                var receivedEmailCount = 0;
                var totalEmailCount = 0;
                var nextToken = "";
                var pageSize = 0;

                //Execute the first request/response anyway, then check if need to request or not afterward.
                do
                {
                    var request = CreateMimecastHeldEmailListRequest(nextToken);
                    var response = SendMimecastHeldEmailsRequest(request);
                    var emailModels = DeconstructMimecastHeldEmails(response, ref pageSize, ref totalEmailCount, ref nextToken);
                    receivedEmailCount += pageSize;
                    heldEmailList.AddRange(emailModels);
                } while (receivedEmailCount >= 0 && receivedEmailCount < totalEmailCount);

                return heldEmailList;
            }
            catch (Exception ex)
            {
                ErrorSignal.FromCurrentContext().Raise(ex);
                return null;
            }
        }

        private HttpWebRequest CreateMimecastHeldEmailListRequest(string nextToken)
        {
            try
            {
                //Generate header for getting held email list
                string uri = ConfigHelper.GetMimecastHeldEmailListUri();
                var request = GenerateMimecastHttpHeader(uri);

                if (request != null)
                    throw new Exception("Could not generate the Mimecast Http Header.");

                //Contruct payload
                var retrieveTimeInterval = ConfigHelper.GetMimecastHeldEmailsInterval();
                var startDateTime = DateTime.Now.AddMinutes(retrieveTimeInterval);
                var endDateTime = DateTime.Now;

                string postData = ContructMimecastPostData(startDateTime, endDateTime, nextToken);
                if (postData == null)
                    throw new Exception("Could not contruct Mimecast Post data.");

                byte[] payload = Encoding.UTF8.GetBytes(postData);

                System.IO.Stream stream = request.GetRequestStream();
                stream.Write(payload, 0, payload.Length);
                stream.Close();
                return request;
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return null;
            }
        }

        private System.Net.HttpWebRequest GenerateMimecastHttpHeader(string uri)
        {
            try
            {
                string baseUrl = ConfigHelper.GetMimecastBaseUrl();
                string accessKey = ConfigHelper.GetMimecastAccessKey();
                string secretKey = ConfigHelper.GetMimecastSecretKey();
                string appId = ConfigHelper.GetMimecastApplicationId();
                string appKey = ConfigHelper.GetMimecastApplicationKey();

                //Generate request header values
                string hdrDate = DateTime.Now.ToUniversalTime().ToString("R");
                string requestId = Guid.NewGuid().ToString();

                //Create the HMAC SHA1 of the Base64 decoded secret key for the Authorization header
                System.Security.Cryptography.HMAC h = new System.Security.Cryptography.HMACSHA1(System.Convert.FromBase64String(secretKey));

                //Use the HMAC SHA1 value to sign the hdrDate + ":" requestId + ":" + URI + ":" + appkey
                byte[] hash = h.ComputeHash(Encoding.Default.GetBytes(hdrDate + ":" + requestId + ":" + uri + ":" +appKey));

                //Build the signature to be included in the Authorization header in your request
                string signature = "MC" + accessKey + ":" + Convert.ToBase64String(hash);

                //Build Request
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(baseUrl + uri);
                request.Method = "POST";
                request.ContentType = "application/json";

                //Add Headers
                request.Headers[HttpRequestHeader.Authorization] = signature;
                request.Headers.Add("x-mc-date", hdrDate);
                request.Headers.Add("x-mc-req-id", requestId);
                request.Headers.Add("x-mc-app-id", appId);
                return request;
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return null;
            }
        }

        private string ContructMimecastPostData(DateTime startDatetime, DateTime endDatetime, string nextToken)
        {
            try
            {
                //Based on Mimecast Json data structure
                var metaOfRequest = new Meta();
                metaOfRequest.pagination = new Pagination()
                {
                    pageSize = ConfigHelper.GetMimecastHeldEmailsPageSize()
                };

                if (nextToken != "") 
                {
                    metaOfRequest.pagination.pageToken = nextToken;
                }

                var postDataObject = new MimecastRootRequest()
                {
                    meta = metaOfRequest,
                    data = new List<DataItem>()
                };

                var dataItem = new DataItem()
                {
                    admin = true,
                    start = startDatetime.ToString("yyyy-MM-ddTHH:mm:ss+0000"),
                    end = endDatetime.ToString("yyyy-MM-ddTHH:mm:ss+0000")
                };
                postDataObject.data.Add(dataItem);

                var postData = JsonConvert.SerializeObject(postDataObject, Newtonsoft.Json.Formatting.Indented);
                return postData;
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return null;
            }
        }

        public string SendMimecastHeldEmailsRequest(HttpWebRequest request)
        {
            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    System.IO.StreamReader reader = new System.IO.StreamReader(response.GetResponseStream());
                    string responseBody = "";
                    string temp = null;
                    while ((temp = reader.ReadLine()) != null)
                    {
                        responseBody += temp;
                    }
                    return responseBody;
                }
                else
                {
                    throw new Exception("Send Mimecast request failed.");
                }
            }
            catch (Exception ex)
            {
                ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return null;
        }

        public List<MimecastHeldEmailItemModel> DeconstructMimecastHeldEmails(string responseData, ref int pageSize, ref int totalEmailCount, ref string nextToken)
        {
            try
            {
                var heldEmailsInOnePage = new List<MimecastHeldEmailItemModel>();
                Root deserializedObjects = JsonConvert.DeserializeObject<Root>(responseData);
                
                var responseStatus =deserializedObjects.meta.status;
                if(responseStatus == 200 && deserializedObjects.fail.Count() < 1)
                {
                    pageSize = deserializedObjects.meta.pagination.pageSize;

                    if (pageSize > 0)
                    {
                        totalEmailCount = deserializedObjects.meta.pagination.totalCount;
                        foreach (var emailItem in deserializedObjects.data)
                        {
                            var heldEmail = new MimecastHeldEmailItemModel
                            {
                                HeldEmailId = emailItem.Id,
                                SenderName = emailItem.from.displayableName,
                                SenderEmailAddress = emailItem.from.emailAddress,
                                ReceiverName = emailItem.to.displayableName,
                                ReceiverEmailAddress = emailItem.to.emailAddress,
                                Subject = emailItem.subject,
                                HeldReason = emailItem.reason,
                                EmailSize = emailItem.size,
                                Route = emailItem.route,
                                PolicyInfo = emailItem.policyInfo,
                            };
                            heldEmailsInOnePage.Add(heldEmail);
                        }
                        if (deserializedObjects.meta.pagination.next != null && !string.IsNullOrEmpty(deserializedObjects.meta.pagination.next))
                            nextToken = deserializedObjects.meta.pagination.next;
                        else
                            nextToken = "";
                    }
                }
                else if (responseStatus == 200)
                {
                    var failReason = deserializedObjects.fail[0].errors[0].message.ToString();
                    throw new Exception(failReason);
                }
                else
                    throw new Exception("Http response failed.");

                return heldEmailsInOnePage;
            }
            catch (Exception ex)
            {
                ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return null;
        }
    }
}
