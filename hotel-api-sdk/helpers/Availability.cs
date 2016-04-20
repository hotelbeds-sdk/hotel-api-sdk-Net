using System;
using System.Collections.Generic;
using System.Linq;

using com.hotelbeds.distribution.hotel_api_model.auto.common;
using com.hotelbeds.distribution.hotel_api_model.auto.messages;
using com.hotelbeds.distribution.hotel_api_model.auto.model;
using com.hotelbeds.distribution.hotel_api_model.util;

namespace com.hotelbeds.distribution.hotel_api_sdk.helpers
{
    public class Availability
    {
        public enum Matcher { ALL, ANY };
        public enum Pay { AT_WEB, AT_HOTEL, INDIFFERENT };
        
        public interface DelimitedShape { }

        public class Circle : DelimitedShape
        {            
            public double longitude { get; set; }
            public double latitude { get; set; }
            public long radiusInKilometers { get; set; }
        }

        public class Square : DelimitedShape
        {
            public double northEastLatitude { get; set; }
            public double northEastLongitude { get; set; }
            public double southWestLatitude { get; set; }
            public double southWestLongitude { get; set; }
        }

        public string language { get; set; }
        public DateTime checkIn { get; set; }
        public DateTime checkOut { get; set; }
        public int shiftDays { get; set; }
        public string destination { get; set; }
        public int? zone { get; set; } = null;

        public HashSet<int> matchingKeywords { get; set; }
        public Matcher keywordsMatcher { get; set; }

        public bool dailyRate { get; set; }
        public bool? packaging { get; set; } = null;

        public int? minCategory { get; set; } = null;
        public int? maxCategory { get; set; } = null;

        public HashSet<SimpleTypes.AccommodationType> ofTypes { get; set; } = null;

        public List<int> includeHotels { get; set; }
        public List<int> excludeHotels { get; set; }
        public bool useGiataCodes { get; set; }


        /// <summary>
        /// Limits
        /// </summary>
        public int? limitHotelsTo { get; set; } = null;
        public int? limitRoomsPerHotelTo { get; set; } = null;
        public decimal? ratesHigherThan { get; set; } = null;
        public decimal? ratesLowerThan { get; set; } = null;
        public int? limitRatesPerRoomTo { get; set; } = null;

        /// <summary>
        ///  Reviews
        /// </summary>
        public decimal? hbScoreHigherThan { get; set; } = null;
        public decimal? hbScoreLowerThan { get; set; } = null;
        public int? numberOfHBReviewsHigherThan { get; set; } = null;
        public decimal? tripAdvisorScoreHigherThan { get; set; } = null;
        public decimal? tripAdvisorScoreLowerThan { get; set; } = null;
        public int? numberOfTripAdvisorReviewsHigherThan { get; set; } = null;

        /// <summary>
        /// GeoLocation filters
        /// </summary>
        public DelimitedShape withinThis { get; set; }
        public List<string> includeBoards { get; set; }
        public List<string> excludeBoards { get; set; }
        public List<string> includeRoomCodes { get; set; }
        public List<string> excludeRoomCodes { get; set; }

        public List<AvailRoom> rooms;
        public Pay? payed { get; set; } = null;

        public Availability()
        {
            rooms = new List<AvailRoom>();
        }       

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public AvailabilityRQ toAvailabilityRQ()
        {
            try
            {
                AvailabilityRQ availabilityRQ = new AvailabilityRQ();
                availabilityRQ.language = this.language;
                availabilityRQ.stay = new Stay(checkIn, checkOut, shiftDays, true);

                if (rooms != null && rooms.Count > 0)
                {
                    availabilityRQ.occupancies = new List<Occupancy>();
                    for (int i = 0; i < rooms.Count; i++)
                    {
                        Occupancy occupancy = new Occupancy();
                        occupancy.adults = rooms[i].adults;
                        occupancy.children = rooms[i].children;
                        occupancy.rooms = rooms[i].numberOfRooms;

                        if (rooms[i].details != null && rooms[i].details.Count > 0 )
                        {
                            occupancy.paxes = new List<Pax>();
                            Pax[] paxes = new Pax[rooms[i].details.Count];                          
                            for(int d = 0; d < rooms[i].details.Count; d++)
                            {
                                Pax pax = new Pax();
                                pax.type = (rooms[i].details[d].getType() == RoomDetail.GuestType.ADULT) ? SimpleTypes.HotelbedsCustomerType.AD : SimpleTypes.HotelbedsCustomerType.CH;
                                pax.age = rooms[i].details[d].getAge();
                                pax.name = rooms[i].details[d].getName();
                                pax.surname = rooms[i].details[d].getSurname();
                                paxes[d] = pax;
                            }
                            occupancy.paxes.AddRange(paxes);
                        }
                        availabilityRQ.occupancies.Add(occupancy);
                    }
                }
                // Linea 224 Availability.java
                if ( withinThis != null )
                {
                    GeoLocation geolocation = new GeoLocation();
                    geolocation.unit = UnitMeasure.UnitMeasureType.km;
                    if ( withinThis.GetType() == typeof(Circle) )
                    {
                        Circle circle = (Circle)withinThis;
                        geolocation.latitude = circle.latitude;
                        geolocation.longitude = circle.longitude;
                        geolocation.radius = circle.radiusInKilometers;
                    }
                    else if ( withinThis.GetType() == typeof(Square) )
                    {
                        Square square = (Square)withinThis;
                        geolocation.latitude = square.northEastLatitude;
                        geolocation.longitude = square.northEastLongitude;
                        geolocation.secondaryLatitude = square.southWestLatitude;
                        geolocation.secondaryLongitude = square.southWestLongitude;
                    }
                    availabilityRQ.geolocation = geolocation;
                }

                if (!String.IsNullOrEmpty(destination))
                {
                    Destination dest = new Destination();
                    dest.code = destination;
                    if (zone != null)
                        dest.zone = zone.Value;
                    availabilityRQ.destination = dest;
                }

                if (matchingKeywords != null && matchingKeywords.Count > 0 )
                {
                    availabilityRQ.keywords = new KeywordsFilter(matchingKeywords.ToList<int>(), keywordsMatcher.Equals(Matcher.ALL));// matchingKeywords.ToList();                       
                }

                if (includeHotels != null && includeHotels.Count > 0 && excludeHotels != null && excludeHotels.Count > 0)
                    foreach (int e in excludeHotels)
                        includeHotels.RemoveAll(i => i == e);

                if (includeHotels != null && includeHotels.Count > 0)
                {
                    HotelsFilter hotelsFilter = new HotelsFilter();
                    hotelsFilter.included = true;
                    hotelsFilter.hotel = includeHotels;
                    hotelsFilter.type = (useGiataCodes) ? SimpleTypes.HotelCodeType.GIATA : SimpleTypes.HotelCodeType.HOTELBEDS;
                    availabilityRQ.hotels = hotelsFilter;
                }
                else if (excludeHotels != null && excludeHotels.Count > 0)
                {
                    HotelsFilter hotelsFilter = new HotelsFilter();
                    hotelsFilter.included = false;
                    hotelsFilter.hotel = excludeHotels;
                    hotelsFilter.type = (useGiataCodes) ? SimpleTypes.HotelCodeType.GIATA : SimpleTypes.HotelCodeType.HOTELBEDS;
                    availabilityRQ.hotels = hotelsFilter;
                }

                if (includeBoards != null && includeBoards.Count > 0)
                {
                    Boards boardFilter = new Boards();
                    boardFilter.included = true;
                    boardFilter.board = includeBoards;
                    availabilityRQ.boards = boardFilter;
                }

                else if (excludeBoards != null && excludeBoards.Count > 0)
                {
                    Boards boardFilter = new Boards();
                    boardFilter.included = false;                    
                    boardFilter.board = excludeBoards;
                    availabilityRQ.boards = boardFilter;
                }


                if (includeRoomCodes != null && includeRoomCodes.Count > 0)
                {
                    Rooms roomFilter = new Rooms();
                    roomFilter.included = true;
                    roomFilter.room = includeRoomCodes;
                    availabilityRQ.rooms = roomFilter;
                }
                else if(excludeRoomCodes != null && excludeRoomCodes.Count > 0)
                {
                    Rooms roomFilter = new Rooms();
                    roomFilter.included = false;
                    roomFilter.room = excludeRoomCodes;
                    availabilityRQ.rooms = roomFilter;
                }

                availabilityRQ.dailyRate = dailyRate;

                if ( ofTypes != null && ofTypes.Count > 0 )
                {
                    availabilityRQ.accommodations = new List<SimpleTypes.AccommodationType>();
                    availabilityRQ.accommodations.AddRange(ofTypes);
                }

                List<ReviewFilter> reviewsFilter = new List<ReviewFilter>();
                if ( hbScoreHigherThan != null || hbScoreLowerThan != null || numberOfHBReviewsHigherThan != null)
                {
                    ReviewFilter reviewFilter = new ReviewFilter();
                    if ( hbScoreLowerThan.HasValue ) reviewFilter.maxRate = hbScoreLowerThan.Value;
                    if (hbScoreHigherThan.HasValue) reviewFilter.minRate = hbScoreHigherThan.Value;
                    if (numberOfHBReviewsHigherThan.HasValue) reviewFilter.minReviewCount = numberOfHBReviewsHigherThan.Value;
                    reviewFilter.type = SimpleTypes.ReviewsType.HOTELBEDS;
                    reviewsFilter.Add(reviewFilter);
                }
                
                if ( tripAdvisorScoreHigherThan != null || tripAdvisorScoreLowerThan != null || numberOfTripAdvisorReviewsHigherThan != null)
                {
                    ReviewFilter reviewFilter = new ReviewFilter();
                    if (tripAdvisorScoreLowerThan.HasValue) reviewFilter.maxRate = tripAdvisorScoreLowerThan.Value;
                    if (tripAdvisorScoreHigherThan.HasValue) reviewFilter.minRate = tripAdvisorScoreHigherThan.Value;
                    if (numberOfTripAdvisorReviewsHigherThan.HasValue) reviewFilter.minReviewCount = numberOfTripAdvisorReviewsHigherThan.Value;
                    reviewFilter.type = SimpleTypes.ReviewsType.TRIPDAVISOR;
                    reviewsFilter.Add(reviewFilter);
                }

                if ( reviewsFilter.Count > 1)
                    availabilityRQ.reviews = reviewsFilter;

                if ( limitHotelsTo != null || maxCategory != null || minCategory != null || limitRoomsPerHotelTo != null || limitRatesPerRoomTo != null || ratesLowerThan != null
                    || ratesHigherThan != null || packaging != null || payed != null)
                {
                    Filter filter = new Filter();
                    if (maxCategory.HasValue)
                        filter.maxCategory = maxCategory.Value;
                    if (minCategory.HasValue)
                        filter.minCategory = minCategory.Value;
                    if (packaging.HasValue)
                        filter.packaging = packaging.Value;
                    if (limitHotelsTo.HasValue)
                        filter.maxHotels = limitHotelsTo.Value;
                    if (limitRoomsPerHotelTo.HasValue)
                        filter.maxRooms = limitRoomsPerHotelTo.Value;
                    if (limitRatesPerRoomTo.HasValue)
                        filter.maxRatesPerRoom = limitRatesPerRoomTo.Value;
                    if (ratesLowerThan.HasValue)
                        filter.maxRate = ratesLowerThan.Value;
                    if (ratesHigherThan.HasValue)
                        filter.minRate = ratesHigherThan.Value;
                    if(payed.HasValue)
                    {
                        switch(payed.Value)
                        {
                            case Pay.AT_HOTEL:
                                filter.paymentType = SimpleTypes.ShowDirectPaymentType.AT_HOTEL;
                                break;
                            case Pay.AT_WEB:
                                filter.paymentType = SimpleTypes.ShowDirectPaymentType.AT_WEB;
                                break;
                            case Pay.INDIFFERENT:
                                filter.paymentType = SimpleTypes.ShowDirectPaymentType.BOTH;
                                break;
                        }
                    }
                    availabilityRQ.filter = filter;
                }

                availabilityRQ.Validate();

                return availabilityRQ;                
            }
            catch (Exception e)
            {
                throw e;
            }
        }        
    }

}
