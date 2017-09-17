using HotelBookingStartupProject.Models;
using System;
using System.Collections.Generic;

namespace HotelBookingStartupProject.BusinessLogic
{
    public interface IBookingManager
    {
        bool CreateBooking(Booking booking);
        int FindAvailableRoom(DateTime startDate, DateTime endDate);
        List<DateTime> GetFullyOccupiedDates(DateTime startDate, DateTime endDate);
    }
}
