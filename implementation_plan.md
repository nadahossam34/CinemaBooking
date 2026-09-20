# Phase 3 — Complete System Integration, Routing, Booking Flow, and Final UI

This plan establishes the end-to-end integration of the Starlight Cinemas application. It addresses all 15 parts and final success criteria specified in Phase 3, ensuring real database connectivity, seamless booking flow, exact admin and customer navbar designs matching `screen.png`, and complete elimination of hardcoded dynamic data.

## User Review Required

> [!IMPORTANT]
> - Database integrity: All entities (Movies, Cinemas, Halls, Seats, SeatTypes, Showtimes, Bookings, BookingSeats, Payments, Users) will be unified in the single existing SQL Server database (`CinemaBookingDb`).
> - Seeding: Automatic startup check will ensure standard `SeatTypes` (Standard, VIP, IMAX) and hall seats (Rows A–E, Seats 1–8) are present for all active auditoriums so any scheduled showtime has real bookable seats.
> - Admin Navigation: Admin header will follow the visual reference `screen.png` with "ST★RLIGHT Cinemas ADMIN PORTAL", admin avatar/meta, and the 8 admin tabs with red rounded pill active styling.

---

## Proposed Changes

### 1. Database Seeding & Seat Availability Foundation

#### [MODIFY] [Program.cs](file:///c:/Users/Admin/source/repos/Poppy-joe/CinemaBooking_phase2/CinemaBooking/Program.cs)
- Update startup initialization:
  - Ensure `SeatTypes` are seeded (Standard multiplier 1.0, VIP 1.5, IMAX 1.25) if empty.
  - For all existing `Halls` with 0 seats, generate real `Seat` records (e.g., Rows A to E, Seats 1 to 10, with row E as VIP and A-D as Standard).
  - Ensure initial showtimes exist for current catalog movies so customer booking is immediately functional.
- Add explicit route mapping for `Admin/Dashboard` -> `AdminController.Dashboard`.

---

### 2. Admin Dashboard & Admin Capabilities

#### [MODIFY] [AdminController.cs](file:///c:/Users/Admin/source/repos/Poppy-joe/CinemaBooking_phase2/CinemaBooking/Controllers/AdminController.cs)
- Add route attributes `[Route("Admin/Dashboard")]`, `[Route("Admin")]`, `[Route("Admin/Index")]`.
- Add `Dashboard()` action returning `View("Dashboard", stats)` calculating real metrics:
  - Total Movies (`_context.Movies.CountAsync()`)
  - Total Cinemas (`_context.Cinemas.CountAsync()`)
  - Total Bookings (`_context.Bookings.CountAsync()`)
  - Total Users (`_context.Users.CountAsync()`)
  - Total Showtimes (`_context.Showtimes.CountAsync(s => s.Date >= today)`)
  - Tickets Sold (`_context.BookingSeats.CountAsync()`)
  - Revenue (`_context.BookingSeats.SumAsync(bs => (decimal?)bs.PricePaid) ?? 0m`)
  - Pass recent bookings list and recent movies.
- Implement actions for all required admin sections so every navigation link points to a real working route:
  - `Showtimes` (`/Admin/Showtimes`): view all scheduled showtimes, with ability to add/delete showtimes.
  - `CreateShowtime` (POST): creates a real showtime for selected movie, cinema, hall, date, time, and base price.
  - `DeleteShowtime` (POST): safely removes showtime.
  - `Bookings` (`/Admin/Bookings`): view all real customer bookings with customer name, email, movie, cinema, hall, seats, total paid, and reference.
  - `Concessions` (`/Admin/Concessions`): concessions catalog management view.
  - `Reports` (`/Admin/Reports`): real financial & occupancy analytics derived from DB bookings.
  - `Settings` (`/Admin/Settings`): exhibition engine & TMDB sync settings view.

#### [NEW] [Dashboard.cshtml](file:///c:/Users/Admin/source/repos/Poppy-joe/CinemaBooking_phase2/CinemaBooking/Views/Admin/Dashboard.cshtml)
- Premium Starlight dashboard view with real metrics cards, recent bookings table, quick actions, and revenue breakdown.

#### [NEW] [Showtimes.cshtml](file:///c:/Users/Admin/source/repos/Poppy-joe/CinemaBooking_phase2/CinemaBooking/Views/Admin/Showtimes.cshtml)
- Showtimes & Schedules admin management view with interactive schedule table and modal/form to schedule new showtimes.

#### [NEW] [Bookings.cshtml](file:///c:/Users/Admin/source/repos/Poppy-joe/CinemaBooking_phase2/CinemaBooking/Views/Admin/Bookings.cshtml)
- Admin Bookings & Tickets view displaying real booking records.

#### [NEW] [Concessions.cshtml](file:///c:/Users/Admin/source/repos/Poppy-joe/CinemaBooking_phase2/CinemaBooking/Views/Admin/Concessions.cshtml)
- Admin Concessions view.

#### [NEW] [Reports.cshtml](file:///c:/Users/Admin/source/repos/Poppy-joe/CinemaBooking_phase2/CinemaBooking/Views/Admin/Reports.cshtml)
- Admin Reports & Analytics view with real sales metrics.

#### [NEW] [Settings.cshtml](file:///c:/Users/Admin/source/repos/Poppy-joe/CinemaBooking_phase2/CinemaBooking/Views/Admin/Settings.cshtml)
- Admin Settings view.

---

### 3. Movie -> Cinema Connection & Showtimes in Admin

#### [MODIFY] [AdminMoviesController.cs](file:///c:/Users/Admin/source/repos/Poppy-joe/CinemaBooking_phase2/CinemaBooking/Controllers/AdminMoviesController.cs)
- In `Create` (GET): pass list of real cinemas from `_context.Cinemas` to the view.
- In `Create` (POST):
  - Receive selected cinema IDs (`List<int> SelectedCinemaIds`).
  - Store comma-separated cinema names in `movie.Locations`.
  - For each selected cinema, find its hall(s) and automatically schedule initial showtimes (e.g. for today & tomorrow at 18:00 and 21:00 with base ticket price) so the newly added movie is immediately linked to those cinemas and has real showtimes.

#### [MODIFY] [Create.cshtml](file:///c:/Users/Admin/source/repos/Poppy-joe/CinemaBooking_phase2/CinemaBooking/Views/Admin/Movies/Create.cshtml)
- Replace hardcoded cinema multiplex checkboxes with a dynamic loop over `_context.Cinemas`.
- Bind selected cinema checkboxes to `SelectedCinemaIds`.

#### [MODIFY] [Index.cshtml](file:///c:/Users/Admin/source/repos/Poppy-joe/CinemaBooking_phase2/CinemaBooking/Views/Admin/Movies/Index.cshtml)
- Display actual cinema names from `movie.Showtimes.Select(s => s.Cinema.Name).Distinct()`.

---

### 4. Admin Layout & Navbar (Matching `screen.png`)

#### [NEW] [_AdminLayout.cshtml](file:///c:/Users/Admin/source/repos/Poppy-joe/CinemaBooking_phase2/CinemaBooking/Views/Shared/_AdminLayout.cshtml)
- Exact header from `screen.png`:
  - Brand: `ST★RLIGHT Cinemas` with red star + `ADMIN PORTAL` badge
  - Search bar: "Search admin records, movies, bookings..."
  - Chrome: online green indicator, notification bell with badge, Administrator profile with initials/avatar, Admin name, "Super Administrator"
  - Subnavigation tabs:
    - Dashboard (`/Admin/Dashboard`)
    - Movies (`/AdminMovies`)
    - Showtimes & Schedules (`/Admin/Showtimes`)
    - Cinemas & Auditoriums (`/AdminCinemas`)
    - Bookings & Tickets (`/Admin/Bookings`)
    - Concessions (`/Admin/Concessions`)
    - Reports & Analytics (`/Admin/Reports`)
    - Settings (`/Admin/Settings`)
  - Visual active state: red rounded pill (`bg-[#e50914] text-white`).
  - Shared across all Admin pages.

---

### 5. Customer Navbar & Home Page Dynamic Data

#### [MODIFY] [_Layout.cshtml](file:///c:/Users/Admin/source/repos/Poppy-joe/CinemaBooking_phase2/CinemaBooking/Views/Shared/_Layout.cshtml)
- Starlight branding for customers (no `ADMIN PORTAL`, no admin controls):
  - HOME (`/`)
  - MOVIES (`/Movies`)
  - CINEMAS (`/Cinemas`)
  - SHOWTIMES (`/Showtimes`)
  - MY BOOKINGS (`/Booking/MyBookings`)
  - Profile / Login / Logout
  - If user is Admin, an unobtrusive "Admin Console" link to jump to `/Admin/Dashboard`.

#### [MODIFY] [HomeController.cs](file:///c:/Users/Admin/source/repos/Poppy-joe/CinemaBooking_phase2/CinemaBooking/Controllers/HomeController.cs)
- Query real "Now Showing" movies from database.
- Pass to `Home/Index.cshtml`.

#### [MODIFY] [Home/Index.cshtml](file:///c:/Users/Admin/source/repos/Poppy-joe/CinemaBooking_phase2/CinemaBooking/Views/Home/Index.cshtml)
- Replace 3 hardcoded fake movies with real movies from database.
- Link "Book Now" directly to `/Movies/Details/{id}`.

---

### 6. Connected Customer Booking Flow (Steps A -> J)

#### [MODIFY] [ShowtimesController.cs](file:///c:/Users/Admin/source/repos/Poppy-joe/CinemaBooking_phase2/CinemaBooking/Controllers/ShowtimesController.cs)
- Allow `movieId` to be optional (`int? movieId = null`).
- When `movieId` is specified, show showtimes for that movie.
- When `movieId` is null, show all upcoming showtimes with a movie/cinema filter so `/Showtimes` from customer navbar works directly.

#### [MODIFY] [Showtimes/Index.cshtml](file:///c:/Users/Admin/source/repos/Poppy-joe/CinemaBooking_phase2/CinemaBooking/Views/Showtimes/Index.cshtml)
- Upgrade to Starlight dark theme.
- Date tabs dynamically generated from actual showtime dates.
- Showtime cards showing Cinema, Hall, Time, Price, and "Select Seats" button.

#### [MODIFY] [Seats/SelectSeats.cshtml](file:///c:/Users/Admin/source/repos/Poppy-joe/CinemaBooking_phase2/CinemaBooking/Views/Seats/SelectSeats.cshtml)
- Upgrade to Starlight luxury auditorium seat map:
  - Screen curve graphic with "Screen" indicator.
  - Rows A to E, seat numbers, VIP vs Standard seats.
  - Unavailable/booked seats disabled with distinct visual indicator.
  - Real-time seat counter and total price display.
  - Proceeds to `/Booking/Checkout`.

#### [MODIFY] [Booking/Checkout.cshtml](file:///c:/Users/Admin/source/repos/Poppy-joe/CinemaBooking_phase2/CinemaBooking/Views/Booking/Checkout.cshtml)
- Upgrade to Starlight luxury checkout layout:
  - Order breakdown: Movie title, cinema, hall, formatted date/time, seat labels, unit price, total price.
  - Customer contact inputs.
  - Proceeds to Payment.

#### [MODIFY] [Payment/Payment.cshtml](file:///c:/Users/Admin/source/repos/Poppy-joe/CinemaBooking_phase2/CinemaBooking/Views/Payment/Payment.cshtml)
- Transform bare form into a Starlight checkout payment interface:
  - Order recap with booking reference and total amount.
  - Payment method choices (Card, Cash at Box Office).
  - Form validation and smooth processing.
  - Proceeds to Ticket Confirmation.

#### [MODIFY] [Booking/Ticket.cshtml](file:///c:/Users/Admin/source/repos/Poppy-joe/CinemaBooking_phase2/CinemaBooking/Views/Booking/Ticket.cshtml)
- Upgrade to cinema boarding pass design:
  - Perforated ticket styling, verified badge, real QR code.
  - Booking Reference, Movie, Cinema, Hall, Date & Time, Seat Numbers, Total Paid, Payment Status.
  - "My Bookings" CTA button.

---

## Verification Plan

### Automated & Sanity Checks
- `dotnet build` to ensure 0 errors and clean compilation across all 3 projects.
- Database queries with `sqlcmd` to confirm:
  - `Seats` exist for all `Halls`.
  - `SeatTypes` are populated.
  - `Showtimes` link movies to cinemas and halls.

### Manual Verification via Browser Subagent / HTTP
1. **Admin Dashboard**:
   - Log in as admin (`admin@starlight.com` / `Admin123!`).
   - Navigate to `/Admin/Dashboard`.
   - Verify all numbers reflect database counts (0 or real counts, no fake numbers).
   - Verify Admin Navbar matches `screen.png` with red pill active tab and all 8 sections.
2. **Admin Adds Movie & Schedules Showtime**:
   - Go to `/AdminMovies/Create`.
   - Select movie and select cinema(s).
   - Save movie.
   - Verify movie is stored in DB with showtimes in selected cinema.
   - Go to `/Admin/Showtimes` and verify showtime is listed.
3. **Customer Discovery & Navigation**:
   - Verify customer navbar has: Home, Movies, Cinemas, Showtimes, My Bookings, Profile (no admin links).
   - Go to `/Home` -> verify real movies appear in "Now Screening".
   - Go to `/Movies` -> verify newly added movie appears.
   - Open Movie Details -> verify selected cinemas appear with showtime options.
4. **Complete Booking Flow**:
   - Select Movie -> Date -> Showtime -> Select Seats (e.g. A1, A2).
   - Checkout -> verify exact movie, cinema, hall, date, time, seats, and calculated total.
   - Complete payment.
   - Verify Ticket Confirmation page with QR code and booking details.
   - Open `/Booking/MyBookings` -> verify booking appears under "Upcoming".
5. **Seat Availability Test**:
   - Revisit the same showtime -> verify A1 and A2 are now booked / disabled.
   - Check a different showtime -> verify A1 and A2 are available.
6. **User Isolation Test**:
   - Log in as a different user -> verify they cannot see other users' bookings.
7. **Admin Dashboard Reflection**:
   - Return to `/Admin/Dashboard` -> verify Total Bookings, Tickets Sold, and Revenue increased by the exact purchase.
