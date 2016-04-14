using System;
using System.Text;

using com.hotelbeds.distribution.hotel_api_model.auto.messages;
using com.hotelbeds.distribution.hotel_api_sdk.types;
using System.Net.Http;
using System.Security.Cryptography;
using Newtonsoft.Json;
using System.Configuration;
using System.Collections.Generic;

namespace com.hotelbeds.distribution.hotel_api_sdk
{
    /// <summary>
    /// 
    /// </summary>
    public class HotelApiClient
    {
        //https://developer.hotelbeds.com/docs/read/apitude_booking/
        
        /// <summary>
        /// CONSTANTS 
        /// </summary>
        private const int REST_TEMPLATE_READ_TIME_OUT = 5000;

        /// <summary>
        /// Atributos
        /// </summary>
        public int readTimeout { get; set; } = REST_TEMPLATE_READ_TIME_OUT;
        
        private readonly string basePath;
        private readonly HotelApiVersion version;        
        private readonly string apiKey;
        private readonly string sharedSecret;
        private string environtment;

        public HotelApiClient()
        {
            this.apiKey = GetHotelApiKeyFromConfig();
            this.sharedSecret = GetHotelSharedSecretFromConfig();
            this.version = new HotelApiVersion(HotelApiVersion.versions.V1);
            this.basePath = GetEnvironment();
            CheckHotelApiClientConfig();
        }        

        public HotelApiClient(HotelApiVersion version)
        {
            this.apiKey = GetHotelApiKeyFromConfig();
            this.sharedSecret = GetHotelSharedSecretFromConfig();
            this.version = version;
            this.basePath = GetEnvironment();
            CheckHotelApiClientConfig();
        }

        public HotelApiClient(string apiKey, string sharedSecret)
        {
            this.apiKey = apiKey;
            this.sharedSecret = sharedSecret;
            this.version = new HotelApiVersion(HotelApiVersion.versions.V1);
            this.basePath = GetEnvironment();
            CheckHotelApiClientConfig();
        }

        public HotelApiClient(HotelApiVersion version, string apiKey, string sharedSecret)
        {
            this.apiKey = apiKey;
            this.sharedSecret = sharedSecret;
            this.version = version;
            this.basePath = GetEnvironment();
            CheckHotelApiClientConfig();
        }

        private void CheckHotelApiClientConfig()
        {
            if (String.IsNullOrEmpty(this.apiKey) || version == null || String.IsNullOrEmpty(this.basePath) || String.IsNullOrEmpty(this.sharedSecret))
                throw new Exception("HotelApiClient cannot be created without specifying an API key, Shared Secret, the Hotel API version and the service you are connecting to.", new ArgumentNullException());
        }

        private string GetHotelApiKeyFromConfig()
        {
            try
            {
                string returnValue = ConfigurationManager.AppSettings.Get("ApiKey");
                
                return returnValue;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private string GetHotelSharedSecretFromConfig()
        {
            try
            {
                string returnValue = ConfigurationManager.AppSettings.Get("SharedSecret");
                
                return returnValue;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private string GetEnvironment()
        {
            try
            {
                string returnValue = string.Empty;
                environtment = ConfigurationManager.AppSettings.Get("ENVIRONTMENT");
                if (!String.IsNullOrEmpty(environtment))
                {
                    returnValue = ConfigurationManager.AppSettings.Get(environtment);
                }
                return returnValue;
            }
            catch(Exception e)
            {
                throw e;
            }
        }

        
        public StatusRS status()
        {
            HotelApiPaths.STATUS avail = new HotelApiPaths.STATUS();
            StatusRS status = callRemoteApi<StatusRS, Object>(null, avail, null);
            return status;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public AvailabilityRS doAvailability(AvailabilityRQ request)
        {
            try
            {
                HotelApiPaths.AVAILABILITY avail = new HotelApiPaths.AVAILABILITY();
                AvailabilityRS response = callRemoteApi<AvailabilityRS, AvailabilityRQ>(request, avail, null);
                return response;
            }
            catch(HotelSDKException e)
            {
                throw e;
            }
        }
        
        public CheckRateRS doCheck(CheckRateRQ checkRateRQ)
        {
            try
            {
                HotelApiPaths.CHECK_AVAIL checkRate = new HotelApiPaths.CHECK_AVAIL();
                CheckRateRS response = callRemoteApi<CheckRateRS, CheckRateRQ>(checkRateRQ, checkRate, null);
                return response;
            }
            catch(HotelSDKException e)
            {
                throw e;
            }
        }

        public BookingRS confirm(BookingRQ bookingRQ)
        {
            try
            {
                HotelApiPaths.BOOKING_CONFIRM bookingConfirm = new HotelApiPaths.BOOKING_CONFIRM();
                BookingRS response = callRemoteApi<BookingRS, BookingRQ>(bookingRQ, bookingConfirm, null);
                return response;
            }
            catch (HotelSDKException e)
            {
                throw e;
            }
        }

        public BookingCancellationRS Cancel(List<Tuple<string, string>> param)
        {
            try
            {
                HotelApiPaths.BOOKING_CANCEL bookingCancel = new HotelApiPaths.BOOKING_CANCEL();
                BookingCancellationRS response = callRemoteApi<BookingCancellationRS, Tuple<string, string>[]>(null, bookingCancel, param);
                return response;
            }
            catch (HotelSDKException e)
            {
                throw e;
            }
        }

        public BookingDetailRS Detail(List<Tuple<string, string>> param)
        {
            try
            {
                HotelApiPaths.BOOKING_DETAIL bookingDetail = new HotelApiPaths.BOOKING_DETAIL();
                BookingDetailRS response = callRemoteApi<BookingDetailRS, Tuple<string, string>[]>(null, bookingDetail, param);
                return response;
            }
            catch (HotelSDKException e)
            {
                throw e;
            }
        }

        public BookingListRS List(List<Tuple<string, string>> param)
        {
            try
            {
                HotelApiPaths.BOOKING_LIST bookingList = new HotelApiPaths.BOOKING_LIST();
                BookingListRS response = callRemoteApi<BookingListRS, Tuple<string, string>[]>(null, bookingList, param);
                return response;
            }
            catch (HotelSDKException e)
            {
                throw e;
            }
        }

        private T callRemoteApi<T, U>(U request, HotelApiPaths.HotelApiPathsBase path, List<Tuple<string, string>> param)
        {
            try
            {
                T response = default(T);

                using (var client = new HttpClient())
                {
                    if (request == null && (path.GetType() != typeof(HotelApiPaths.STATUS) 
                        && path.GetType() != typeof(HotelApiPaths.BOOKING_CANCEL) && path.GetType() != typeof(HotelApiPaths.BOOKING_DETAIL) && path.GetType() != typeof(HotelApiPaths.BOOKING_LIST)))
                        throw new Exception("Object request can't be null");

                    client.BaseAddress = new Uri(path.getUrl(this.basePath, this.version));
                    client.DefaultRequestHeaders.Clear();
                    client.Timeout = new TimeSpan(0, 0, this.readTimeout);
                    client.DefaultRequestHeaders.Add("Api-Key", this.apiKey);
                    client.DefaultRequestHeaders.Add("Accept-Enconding", "Gzip");
                    client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
                    client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json");

                    long ts = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds / 1000;
                    SHA256 hashstring = SHA256Managed.Create();
                    byte[] hash = hashstring.ComputeHash(Encoding.UTF8.GetBytes(this.apiKey + this.sharedSecret + ts));
                    string signature = BitConverter.ToString(hash).Replace("-", "");
                    client.DefaultRequestHeaders.Add("X-Signature", signature.ToString());

                    // GET Method
                    if (path.getHttpMethod() == HttpMethod.Get)
                    {
                        string Uri = path.getEndPoint();

                        if (param != null)
                        {
                            Uri = path.getEndPoint(param);
                        }

                        HttpResponseMessage resp = client.GetAsync(Uri).Result;                        
                        response = resp.Content.ReadAsAsync<T>().Result;
                        return response;
                    }

                    // DELETE Method
                    if (path.getHttpMethod() == HttpMethod.Delete)
                    {
                        string Uri = path.getEndPoint();

                        if (param != null)
                        {
                            Uri = path.getEndPoint(param);
                        }

                        HttpResponseMessage resp = client.DeleteAsync(Uri).Result;
                        response = resp.Content.ReadAsAsync<T>().Result;
                        return response;
                    }

                    StringContent contentToSend = null;
                    if (request != null)
                    { 
                        string objectSerialized = JsonConvert.SerializeObject(request, Formatting.Indented, new JsonSerializerSettings() { DefaultValueHandling = DefaultValueHandling.Ignore });
                        contentToSend = new StringContent(objectSerialized, Encoding.UTF8, "application/json");
                    }

                    if (path.getHttpMethod() == HttpMethod.Post)
                    {
                        HttpResponseMessage resp = null;
                        if ( param == null )
                            resp = client.PostAsync(path.getEndPoint(), contentToSend).Result;
                        else
                            resp = client.PostAsync(path.getEndPoint(param), contentToSend).Result;

                        response = resp.Content.ReadAsAsync<T>().Result;
                    }
                }

                return response;
            }
            catch (HotelSDKException e)
            {
                throw e;
            }
        }
    }
}
