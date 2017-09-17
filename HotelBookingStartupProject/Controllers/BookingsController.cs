using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using HotelBookingStartupProject.Models;
using HotelBookingStartupProject.Data.Repositories;

namespace HotelBookingStartupProject.Controllers
{
    public class BookingsController : Controller
    {
        private IRepository<Booking> bookingRepository;
        private IRepository<Customer> customerRepository;
        private IRepository<Room> roomRepository;

        public BookingsController(IRepository<Booking> bookingRepos, IRepository<Room> roomRepos, IRepository<Customer> customerRepos)
        {
            bookingRepository = bookingRepos;
            roomRepository = roomRepos;
            customerRepository = customerRepos;
        }

        // GET: Bookings
        public IActionResult Index(int? id)
        {
            var bookings = bookingRepository.GetAll();

            var bookingStartDates = bookings.Select(b => b.StartDate);
            DateTime minBookingDate = bookingStartDates.Any() ? bookingStartDates.Min() : DateTime.MinValue;

            var bookingEndDates = bookings.Select(b => b.EndDate);
            DateTime maxBookingDate = bookingEndDates.Any() ? bookingEndDates.Max() : DateTime.MaxValue;


            List<DateTime> fullyOccupiedDates = new List<DateTime>();

            int noOfRooms = roomRepository.GetAll().Count();

            if (bookings.Any())
            {
                for (DateTime d = minBookingDate; d <= maxBookingDate; d = d.AddDays(1))
                {
                    var noOfBookings = from b in bookings
                                       where b.IsActive && d >= b.StartDate && d <= b.EndDate
                                       select b;
                    if (noOfBookings.Count() >= noOfRooms)
                        fullyOccupiedDates.Add(d);
                }
            }

            ViewBag.FullyOccupiedDates = fullyOccupiedDates;

            int minBookingYear = minBookingDate.Year;
            int maxBookingYear = maxBookingDate.Year;
            if (id == null)
                id = DateTime.Today.Year;
            else if (id < minBookingYear)
                id = minBookingYear;
            else if (id > maxBookingYear)
                id = maxBookingYear;

            ViewBag.YearToDisplay = id;

            return View(bookings);
        }

        // GET: Bookings/Details/5
        public IActionResult Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Booking booking = bookingRepository.Get(id.Value);
            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }

        // GET: Bookings/Create
        public IActionResult Create()
        {
            ViewData["CustomerId"] = new SelectList(customerRepository.GetAll(), "Id", "Name");
            return View();
        }

        // POST: Bookings/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([Bind("StartDate,EndDate,CustomerId")] Booking booking)
        {
            if (ModelState.IsValid)
            {
                int roomId = -1;
                DateTime startDate = booking.StartDate;
                DateTime endDate = booking.EndDate;

                if (startDate < DateTime.Today || startDate > endDate)
                {
                    ViewData["CustomerId"] = new SelectList(customerRepository.GetAll(), "Id", "Name", booking.CustomerId);
                    ViewBag.Status = "The start date cannot be in the past or later than the end date.";
                    return View(booking);
                }

                var activeBookings = bookingRepository.GetAll().Where(b => b.IsActive);
                foreach (var room in roomRepository.GetAll())
                {
                    var activeBookingsForCurrentRoom = activeBookings.Where(b => b.RoomId == room.Id);
                    if (activeBookingsForCurrentRoom.All(b => startDate < b.StartDate &&
                        endDate < b.StartDate || startDate > b.EndDate && endDate > b.EndDate))
                    {
                        roomId = room.Id;
                        break;
                    }
                }

                if (roomId >= 0)
                {
                    booking.RoomId = roomId;
                    booking.IsActive = true;
                    bookingRepository.Add(booking);
                    return RedirectToAction(nameof(Index));
                }
            }

            ViewData["CustomerId"] = new SelectList(customerRepository.GetAll(), "Id", "Name", booking.CustomerId);
            ViewBag.Status = "The booking could not be created. There were no available room.";
            return View(booking);
        }

        // GET: Bookings/Edit/5
        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Booking booking = bookingRepository.Get(id.Value);
            if (booking == null)
            {
                return NotFound();
            }
            ViewData["CustomerId"] = new SelectList(customerRepository.GetAll(), "Id", "Name", booking.CustomerId);
            ViewData["RoomId"] = new SelectList(roomRepository.GetAll(), "Id", "Description", booking.RoomId);
            return View(booking);
        }

        // POST: Bookings/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, [Bind("StartDate,EndDate,IsActive,CustomerId,RoomId")] Booking booking)
        {
            if (id != booking.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    bookingRepository.Edit(booking);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (bookingRepository.Get(booking.Id) == null)
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CustomerId"] = new SelectList(customerRepository.GetAll(), "Id", "Name", booking.CustomerId);
            ViewData["RoomId"] = new SelectList(roomRepository.GetAll(), "Id", "Description", booking.RoomId);
            return View(booking);
        }

        // GET: Bookings/Delete/5
        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Booking booking = bookingRepository.Get(id.Value);
            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }

        // POST: Bookings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            bookingRepository.Remove(id);
            return RedirectToAction(nameof(Index));
        }

    }
}
