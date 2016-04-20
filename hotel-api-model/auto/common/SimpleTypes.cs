using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace com.hotelbeds.distribution.hotel_api_model.auto.common
{
    public class SimpleTypes
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public enum HotelbedsCustomerType
        {
            [EnumMember(Value = "AD")]
            AD,
            [EnumMember(Value = "CH")]
            CH
        }

        

        [JsonConverter(typeof(StringEnumConverter))]
        public enum ShowDirectPaymentType
        {
            [EnumMember(Value = "S")]
            AT_HOTEL,
            [EnumMember(Value = "N")]
            AT_WEB,
            [EnumMember(Value = "A")]
            BOTH
        }        

        public enum PaymentType
        {
            AT_HOTEL,
            AT_WEB
        }        

        public enum TaxType
        {
            TAX,
            FEE
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum BookingStatus
        {
            CONFIRMED,
            CANCELLED
        };


        public enum HotelPackage { YES, NO, BOTH };        

        public enum HotelCodeType { HOTELBEDS, GIATA };        

        public enum ChannelType { B2B, B2C, META, NEWSLETTER };        

        public enum DeviceType { MOBILE, WEB, TABLET };        

        public enum RateType { BOOKABLE, RECHECK };        

        public enum AccommodationType
        {
            HOTEL,
            APARTMENT,
            RURAL,
            HOSTEL,
            APTHOTEL,
            CAMPING,
            HISTORIC,
            PENDING,
            RESORT,
            HOMES
        }        

        public enum ReviewsType { TRIPDAVISOR, HOTELBEDS };
            
    }
}