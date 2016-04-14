using System;
using System.Collections.Generic;

namespace com.hotelbeds.distribution.hotel_api_sdk.helpers
{
    public class AvailRoom
    {
        public int adults { get; set; }
        public int numberOfRooms { get; set; }
        public int children { get; set; }
        public List<RoomDetail> details { get; set; }        

        /// <summary>
        /// Method to add a custom GuestType
        /// </summary>
        /// <param name="type">RoomDetail.GuestType</param>
        /// <param name="age">Pax age</param>
        /// <param name="name">Pax name</param>
        /// <param name="surname">Pax surname</param>
        public void detailed(RoomDetail.GuestType type, int age, String name, String surname, int numberOfRooms)
        {
            details.Add(new RoomDetail(type, age, name, surname, numberOfRooms));
        }

        /// <summary>
        /// Method to add a adult
        /// </summary>
        /// <param name="age"></param>
        public void adultOf(int age)
        {
            details.Add(new RoomDetail(RoomDetail.GuestType.ADULT, age, null, null, numberOfRooms));
        }

        /// <summary>
        /// Method to add a child
        /// </summary>
        /// <param name="age"></param>
        public void childOf(int age)
        {
            details.Add(new RoomDetail(age));
        }
    }
}
