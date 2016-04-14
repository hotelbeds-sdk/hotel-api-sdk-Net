using System;
using System.Collections.Generic;
using System.Linq;
using com.hotelbeds.distribution.hotel_api_sdk;
using com.hotelbeds.distribution.hotel_api_model.auto.messages;
using com.hotelbeds.distribution.hotel_api_sdk.helpers;
using com.hotelbeds.distribution.hotel_api_model.auto.model;
using Newtonsoft.Json;

namespace com.hotelbeds.distribution.hotel_api_sdk_demo
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                HotelApiClient client = new HotelApiClient();
                StatusRS status = client.status();

                if (status != null && status.error == null)
                    Console.WriteLine("StatusRS: " + status.status);
                else if (status != null && status.error != null)
                {
                    Console.WriteLine("StatusRS: " + status.status + " " + status.error.code + ": " + status.error.message);
                    return;
                }
                else if (status == null)
                {
                    Console.WriteLine("StatusRS: Is not available.");
                    return;
                }

                List<Tuple<string, string>> param;


                Availability avail = new Availability();
                avail.checkIn = DateTime.Now.AddDays(10);
                avail.checkOut = DateTime.Now.AddDays(13);
                avail.destination = "PMI";
                avail.zone = 90;
                avail.language = "CAS";
                avail.shiftDays = 2;
                AvailRoom room = new AvailRoom();
                room.adults = 2;
                room.children = 0;
                room.details = new List<RoomDetail>();
                room.adultOf(30);
                room.adultOf(30);
                //room.childOf(4);                
                avail.rooms.Add(room);
                avail.payed = Availability.Pay.AT_HOTEL;
                //avail.ofTypes = new HashSet<hotel_api_model.auto.common.SimpleTypes.AccommodationType>();
                //avail.ofTypes.Add(hotel_api_model.auto.common.SimpleTypes.AccommodationType.HOTEL);
                //avail.ofTypes.Add(hotel_api_model.auto.common.SimpleTypes.AccommodationType.APARTMENT);

                //avail.minCategory = 4;
                //avail.limitHotelsTo = 10;
                //avail.numberOfTripAdvisorReviewsHigherThan = 2;
                //avail.tripAdvisorScoreHigherThan = 2

                //avail.matchingKeywords = new HashSet<int>();
                //avail.matchingKeywords.Add(34);
                //avail.matchingKeywords.Add(81);
                //avail.keywordsMatcher = Availability.Matcher.ALL;

                //avail.includeHotels = new List<int>();
                //avail.includeHotels.Add(111637);
                //avail.includeHotels.Add(2818);
                //avail.includeHotels.Add(138465);
                //avail.includeHotels.Add(164471);

                //avail.excludeHotels = new List<int>();
                //avail.excludeHotels.Add(187013);
                //avail.excludeHotels.Add(188330);

                //avail.useGiataCodes = false;
                //avail.limitHotelsTo = 250;
                //avail.limitRoomsPerHotelTo = 5;
                //avail.limitRatesPerRoomTo = 5;
                //avail.ratesHigherThan = 50;
                //avail.ratesLowerThan = 350;

                //avail.hbScoreHigherThan = 3;
                //avail.hbScoreLowerThan = 5;
                //avail.numberOfHBReviewsHigherThan = 50;

                //avail.tripAdvisorScoreHigherThan = 1;
                //avail.tripAdvisorScoreLowerThan = 4;
                //avail.numberOfHBReviewsHigherThan = 50;

                //avail.withinThis = new Availability.Circle() { latitude = 2.646633999999949, longitude = 39.57119, radiusInKilometers = 50 };
                //avail.withinThis = new Availability.Square() { northEastLatitude = 45.37680856570233, northEastLongitude = -2.021484375, southWestLatitude = 38.548165423046584, southWestLongitude = 8.658203125 };

                //avail.includeBoards = new List<string>();
                //avail.includeBoards.Add("R0-E10");
                //avail.includeBoards.Add("BB-E10");
                //avail.excludeBoards = new List<string>();
                //avail.excludeBoards.Add("RO");

                //avail.includeRoomCodes = new List<string>();
                //avail.includeRoomCodes.Add("DBL.ST");
                //avail.includeRoomCodes.Add("DBL.SU");
                ////avail.includeRoomCodes.AddRange(new string[]{ "DBL.ST", "DBL.SU" });
                //avail.excludeRoomCodes = new List<string>();
                //avail.excludeRoomCodes.Add("TPL.ST");

                AvailabilityRQ availabilityRQ = avail.toAvailabilityRQ();
                if (availabilityRQ == null)
                    throw new Exception("Availability RQ can't be null", new ArgumentNullException());

                //Console.WriteLine("Availability Request:");
                //Console.WriteLine(JsonConvert.SerializeObject(availabilityRQ, Formatting.Indented, new JsonSerializerSettings() { DefaultValueHandling = DefaultValueHandling.Ignore }));

                AvailabilityRS responseAvail = client.doAvailability(availabilityRQ);

                if (responseAvail != null && responseAvail.hotels != null && responseAvail.hotels.hotels != null && responseAvail.hotels.hotels.Count > 0)
                {
                    Console.WriteLine(string.Format("Availability answered with {0} hotels!", responseAvail.hotels.hotels.Count));
                    Console.WriteLine(JsonConvert.SerializeObject(responseAvail, Formatting.Indented, new JsonSerializerSettings() { DefaultValueHandling = DefaultValueHandling.Ignore }));

                    //  ***********************************
                    //  Try to check reservation with rate
                    //  ***********************************

                    Hotel firstHotel = responseAvail.hotels.hotels.First();
                    string rateKey = string.Empty;

                    for (int r = 0; r < firstHotel.rooms.Count && String.IsNullOrEmpty(rateKey); r++)
                    {
                        for (int rk = 0; firstHotel.rooms[r].rates != null && rk < firstHotel.rooms[r].rates.Count && String.IsNullOrEmpty(rateKey); rk++)
                        {                            
                            rateKey = firstHotel.rooms[r].rates[rk].rateKey;                            
                        }                        
                    }

                    if (String.IsNullOrEmpty(rateKey))
                    {
                        Console.WriteLine("No hotel available");
                        return;
                    }

                    Console.WriteLine("Checking reservation with rate " + rateKey);

                    ConfirmRoom confirmRoom = new ConfirmRoom();
                    confirmRoom.details = new List<RoomDetail>();
                    confirmRoom.detailed(RoomDetail.GuestType.ADULT, 30, "NombrePasajero1", "ApellidoPasajero1", 1);
                    confirmRoom.detailed(RoomDetail.GuestType.ADULT, 30, "NombrePasajero2", "ApellidoPasajero2", 1);

                    BookingCheck bookingCheck = new BookingCheck();
                    bookingCheck.addRoom(rateKey, confirmRoom);
                    CheckRateRQ checkRateRQ = bookingCheck.toCheckRateRQ();

                    if (checkRateRQ != null)
                    {
                        CheckRateRS responseRate = client.doCheck(checkRateRQ);
                        if (responseRate != null && responseRate.error == null)
                        {
                            Console.WriteLine("CheckRate Response:");
                            Console.WriteLine(JsonConvert.SerializeObject(responseRate, Formatting.Indented, new JsonSerializerSettings() { DefaultValueHandling = DefaultValueHandling.Ignore }));

                            com.hotelbeds.distribution.hotel_api_sdk.helpers.Booking booking = new com.hotelbeds.distribution.hotel_api_sdk.helpers.Booking();
                            booking.createHolder("Rosetta", "Pruebas");
                            booking.clientReference = "SDK Test";
                            booking.remark = "***SDK***TESTING";

                            //NOTE: ONLY LIBERATE (PAY AT HOTEL MODEL) USES PAYMENT DATA NODES. FOR OTHER PRICING MODELS THESE NODES MUST NOT BE USED.
                            booking.cardType = "VI";
                            booking.cardNumber = "4444333322221111";
                            booking.expiryDate = "0620";
                            booking.cardCVC = "0620";
                            booking.email = "pmayol@multinucleo.com";
                            booking.phoneNumber = "654654654";
                            booking.cardHolderName = "AUTHORISED";

                            booking.addRoom(rateKey, confirmRoom);
                            BookingRQ bookingRQ = booking.toBookingRQ();
                            if (bookingRQ != null)
                            {
                                BookingRS responseBooking = client.confirm(bookingRQ);
                                Console.WriteLine("Booking Response:");
                                if (responseBooking != null)
                                    Console.WriteLine(JsonConvert.SerializeObject(responseBooking, Formatting.Indented, new JsonSerializerSettings() { DefaultValueHandling = DefaultValueHandling.Ignore }));
                                else
                                    Console.WriteLine("ResponseBooking Object Response is null");

                                if (responseBooking != null && responseBooking.error == null && responseBooking.booking != null)
                                {
                                    Console.WriteLine("Confirmation succedded. Canceling reservation with id " + responseBooking.booking.reference);
                                    param = new List<Tuple<string, string>>
                                    {
                                        new Tuple<string, string>("${bookingId}", responseBooking.booking.reference),
                                        //new Tuple<string, string>("${bookingId}", "1-3087550"),
                                        new Tuple<string, string>("${flag}", "C")
                                    };


                                    BookingCancellationRS bookingCancellationRS = client.Cancel(param);

                                    if (bookingCancellationRS != null)
                                    {
                                        Console.WriteLine("Id cancelled: " + responseBooking.booking.reference);
                                        Console.WriteLine(JsonConvert.SerializeObject(bookingCancellationRS, Formatting.Indented, new JsonSerializerSettings() { DefaultValueHandling = DefaultValueHandling.Ignore }));
                                        Console.ReadLine();

                                        Console.WriteLine("Getting detail after cancelation of id " + responseBooking.booking.reference);
                                        param = new List<Tuple<string, string>>
                                        {
                                            new Tuple<string, string>("${bookingId}", responseBooking.booking.reference)
                                        };
                                        BookingDetailRS bookingDetailRS = client.Detail(param);
                                        if (bookingDetailRS != null)
                                            Console.WriteLine(JsonConvert.SerializeObject(bookingDetailRS, Formatting.Indented, new JsonSerializerSettings() { DefaultValueHandling = DefaultValueHandling.Ignore }));
                                    }
                                }
                            }
                        }
                        else
                            Console.WriteLine("No hotel available");
                    }
                }
                else
                    Console.WriteLine("No availability!");

                Console.WriteLine("Requesting booking list...");

                param = new List<Tuple<string, string>>
                {
                    new Tuple<string, string>("${start}", DateTime.Now.AddDays(-30).ToString("yyyy-MM-dd")),
                    new Tuple<string, string>("${end}", DateTime.Now.ToString("yyyy-MM-dd")),
                    new Tuple<string, string>("${includeCancelled}", "true"),
                    new Tuple<string, string>("${filterType}", "CREATION"),
                    new Tuple<string, string>("${from}", "1"),
                    new Tuple<string, string>("${to}", "25"),
                };

                BookingListRS bookingListRS = client.List(param);
                if (bookingListRS != null)
                {
                    Console.WriteLine(JsonConvert.SerializeObject(bookingListRS, Formatting.Indented, new JsonSerializerSettings() { DefaultValueHandling = DefaultValueHandling.Ignore }));
                    foreach (com.hotelbeds.distribution.hotel_api_model.auto.model.Booking booking in bookingListRS.bookings.bookings)
                    {
                        param = new List<Tuple<string, string>>
                        {
                            new Tuple<string, string>("${bookingId}", booking.reference)
                        };
                        BookingDetailRS bookingDetailRS = client.Detail(param);
                        if (bookingDetailRS != null)
                        {
                            Console.WriteLine(JsonConvert.SerializeObject(bookingDetailRS, Formatting.Indented, new JsonSerializerSettings() { DefaultValueHandling = DefaultValueHandling.Ignore }));
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e.Message + " " + e.StackTrace);
            }
            Console.ReadLine();
        }
    }
}
