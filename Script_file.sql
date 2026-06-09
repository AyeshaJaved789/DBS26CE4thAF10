create database cinema_booking_system;
use cinema_booking_system;
-- 1. ROLES TABLE
-- ============================================
CREATE TABLE Roles (
    RoleID   INT PRIMARY KEY AUTO_INCREMENT,
    RoleName VARCHAR(50) UNIQUE NOT NULL
);
 
-- ============================================
-- 2. USERS TABLE
-- ============================================
CREATE TABLE Users (
    UserID   INT PRIMARY KEY AUTO_INCREMENT,
    RoleID   INT          NOT NULL,
    Username VARCHAR(50)  UNIQUE NOT NULL,
    Password VARCHAR(255) NOT NULL,
    Email    VARCHAR(100) UNIQUE NOT NULL,
    FOREIGN KEY (RoleID) REFERENCES Roles(RoleID)
);
 
-- ============================================
-- 3. CUSTOMERS TABLE
-- ============================================
CREATE TABLE Customers (
    CustomerID INT PRIMARY KEY AUTO_INCREMENT,
    UserID     INT          UNIQUE NOT NULL,
    FullName   VARCHAR(100) NOT NULL,
    Phone      VARCHAR(20)  UNIQUE NOT NULL,
    Gender     ENUM('Male','Female','Other'),
    FOREIGN KEY (UserID) REFERENCES Users(UserID)
);
 
-- ============================================
-- 4. GENRES TABLE
-- ============================================
CREATE TABLE Genres (
    GenreID   INT PRIMARY KEY AUTO_INCREMENT,
    GenreName VARCHAR(50) UNIQUE NOT NULL
);
 
-- ============================================
-- 5. MOVIES TABLE
-- ============================================
CREATE TABLE Movies (
    MovieID     INT PRIMARY KEY AUTO_INCREMENT,
    GenreID     INT          NOT NULL,
    Title       VARCHAR(100) NOT NULL,
    Duration    INT          NOT NULL,       -- minutes
    Language    VARCHAR(50)  NOT NULL,
    ReleaseDate DATE         NOT NULL,
    Rating      DECIMAL(3,1),
    FOREIGN KEY (GenreID) REFERENCES Genres(GenreID)
);
 
-- ============================================
-- 6. HALLS TABLE
-- ============================================
CREATE TABLE Halls (
    HallID   INT PRIMARY KEY AUTO_INCREMENT,
    HallName VARCHAR(50) UNIQUE NOT NULL,
    Capacity INT NOT NULL
);
 
-- ============================================
-- 7. SEATTYPES TABLE
-- ============================================
CREATE TABLE SeatTypes (
    SeatTypeID INT PRIMARY KEY AUTO_INCREMENT,
    TypeName   VARCHAR(30)    UNIQUE NOT NULL,
    ExtraPrice DECIMAL(10,2)  NOT NULL DEFAULT 0.00
);
 
-- ============================================
-- 8. SEATS TABLE
-- ============================================
CREATE TABLE Seats (
    SeatID     INT PRIMARY KEY AUTO_INCREMENT,
    HallID     INT         NOT NULL,
    SeatTypeID INT         NOT NULL,
    SeatNumber VARCHAR(10) NOT NULL,
    Status     ENUM('Available','Booked') NOT NULL DEFAULT 'Available',
    UNIQUE KEY uq_hall_seat (HallID, SeatNumber),
    FOREIGN KEY (HallID)     REFERENCES Halls(HallID),
    FOREIGN KEY (SeatTypeID) REFERENCES SeatTypes(SeatTypeID)
);

CREATE TABLE Shows (
    ShowID      INT PRIMARY KEY AUTO_INCREMENT,
    MovieID     INT           NOT NULL,
    HallID      INT           NOT NULL,
    ShowDate    DATE          NOT NULL,
    StartTime   TIME          NOT NULL,
    EndTime     TIME          NULL,        -- ye wala normal column
    TicketPrice DECIMAL(10,2) NOT NULL,
    FOREIGN KEY (MovieID) REFERENCES Movies(MovieID),
    FOREIGN KEY (HallID)  REFERENCES Halls(HallID)
);

-- 10. PAYMENTMETHODS TABLE
-- ============================================
-- FIX: Moved up so Payments can reference it via FK.
CREATE TABLE PaymentMethods (
    MethodID   INT PRIMARY KEY AUTO_INCREMENT,
    MethodName VARCHAR(50) UNIQUE NOT NULL
);
 
-- ============================================
-- 11. BOOKINGS TABLE
-- ============================================
CREATE TABLE Bookings (
    BookingID   INT PRIMARY KEY AUTO_INCREMENT,
    CustomerID  INT           NOT NULL,
    ShowID      INT           NOT NULL,
    BookingDate DATETIME      NOT NULL DEFAULT CURRENT_TIMESTAMP,
    TotalAmount DECIMAL(10,2) NOT NULL,
    Status      ENUM('Booked','Cancelled') NOT NULL DEFAULT 'Booked',
    FOREIGN KEY (CustomerID) REFERENCES Customers(CustomerID),
    FOREIGN KEY (ShowID)     REFERENCES Shows(ShowID)
);
 
-- ============================================
-- 12. BOOKINGDETAILS TABLE
-- ============================================
-- FIX: PriceAtBooking added to preserve the exact price paid per seat
--      even if TicketPrice or SeatType ExtraPrice changes later.
CREATE TABLE BookingDetails (
    BookingDetailID INT PRIMARY KEY AUTO_INCREMENT,
    BookingID       INT           NOT NULL,
    SeatID          INT           NOT NULL,
    PriceAtBooking  DECIMAL(10,2) NOT NULL,   -- snapshot: TicketPrice + ExtraPrice at booking time
    UNIQUE KEY uq_booking_seat (BookingID, SeatID),
    FOREIGN KEY (BookingID) REFERENCES Bookings(BookingID),
    FOREIGN KEY (SeatID)    REFERENCES Seats(SeatID)
);
 
-- ============================================
-- 13. PAYMENTS TABLE
-- ============================================
-- FIX: PaymentMethod VARCHAR replaced with MethodID FK referencing
--      PaymentMethods table. Eliminates the redundancy / sync issue.
CREATE TABLE Payments (
    PaymentID   INT PRIMARY KEY AUTO_INCREMENT,
    BookingID   INT           NOT NULL,
    MethodID    INT           NOT NULL,       -- FK to PaymentMethods
    Amount      DECIMAL(10,2) NOT NULL,
    PaymentDate DATETIME      NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (BookingID) REFERENCES Bookings(BookingID),
    FOREIGN KEY (MethodID)  REFERENCES PaymentMethods(MethodID)
);
 
-- ============================================
-- 14. REVIEWS TABLE
-- ============================================
-- FIX: UNIQUE (CustomerID, MovieID) added so one customer
--      can leave only one review per movie.
CREATE TABLE Reviews (
    ReviewID   INT PRIMARY KEY AUTO_INCREMENT,
    CustomerID INT NOT NULL,
    MovieID    INT NOT NULL,
    Rating     INT CHECK (Rating BETWEEN 1 AND 5),
    Comment    TEXT,
    UNIQUE KEY uq_customer_movie_review (CustomerID, MovieID),
    FOREIGN KEY (CustomerID) REFERENCES Customers(CustomerID),
    FOREIGN KEY (MovieID)    REFERENCES Movies(MovieID)
);
 
-- ============================================
-- 15. CANCELLATIONPOLICY TABLE
-- ============================================
CREATE TABLE CancellationPolicy (
    PolicyID          INT PRIMARY KEY AUTO_INCREMENT,
    DaysBeforeShow    INT           NOT NULL,
    RefundPercentage  DECIMAL(5,2)  NOT NULL,
    Description       VARCHAR(255)
);
 

-- triggers
DELIMITER $$
CREATE TRIGGER trg_shows_endtime_insert
BEFORE INSERT ON Shows
FOR EACH ROW
BEGIN
    DECLARE dur INT;
    SELECT Duration INTO dur FROM Movies WHERE MovieID = NEW.MovieID;
    SET NEW.EndTime = ADDTIME(NEW.StartTime, SEC_TO_TIME(dur * 60));
END$$

CREATE TRIGGER trg_shows_endtime_update
BEFORE UPDATE ON Shows
FOR EACH ROW
BEGIN
    DECLARE dur INT;
    SELECT Duration INTO dur FROM Movies WHERE MovieID = NEW.MovieID;
    SET NEW.EndTime = ADDTIME(NEW.StartTime, SEC_TO_TIME(dur * 60));
END$$

DELIMITER ;

-- trigger 3
-- TRIGGER 3
-- Jab koi seat book ho (BookingDetail insert)
-- us seat ka status automatically 'Booked' ho jaye
-- Double booking prevent hoti hai
-- ============================================
DELIMITER $$
CREATE TRIGGER trg_seat_status_on_bookingdetail_insert
AFTER INSERT ON BookingDetails
FOR EACH ROW
BEGIN
    UPDATE Seats
    SET Status = 'Booked'
    WHERE SeatID = NEW.SeatID;
END$$
 
-- ============================================
-- TRIGGER 4
-- Jab booking cancel ho
-- us booking ki saari seats wapas 'Available' ho jayein
-- ============================================
DELIMITER $$
CREATE TRIGGER trg_booking_seat_status
AFTER UPDATE ON Bookings
FOR EACH ROW
BEGIN
    IF NEW.Status = 'Cancelled' AND OLD.Status = 'Booked' THEN
        UPDATE Seats
        SET Status = 'Available'
        WHERE SeatID IN (
            SELECT SeatID FROM BookingDetails
            WHERE BookingID = NEW.BookingID
        );
    END IF;
END$$



-- 1. ROLES
-- ============================================
INSERT INTO Roles (RoleName) VALUES
('Admin'),
('Customer'),
('Staff');
 
-- ============================================
-- 2. USERS
-- ============================================
INSERT INTO Users (RoleID, Username, Password, Email) VALUES
(1, 'admin_ali',     'Admin@123',    'ali.admin@cinema.com'),
(1, 'admin_sara',    'Admin@456',    'sara.admin@cinema.com'),
(3, 'staff_hassan',  'Staff@123',    'hassan.staff@cinema.com'),
(3, 'staff_zara',    'Staff@456',    'zara.staff@cinema.com'),
(3, 'staff_usman',   'Staff@789',    'usman.staff@cinema.com'),
(2, 'ahmed_ch',      'Pass@1234',    'ahmed.ch@gmail.com'),
(2, 'fatima_k',      'Pass@2345',    'fatima.k@gmail.com'),
(2, 'bilal_m',       'Pass@3456',    'bilal.m@gmail.com'),
(2, 'ayesha_r',      'Pass@4567',    'ayesha.r@gmail.com'),
(2, 'usman_t',       'Pass@5678',    'usman.t@gmail.com'),
(2, 'sana_q',        'Pass@6789',    'sana.q@gmail.com'),
(2, 'hamza_n',       'Pass@7890',    'hamza.n@gmail.com'),
(2, 'maryam_s',      'Pass@8901',    'maryam.s@gmail.com'),
(2, 'zain_a',        'Pass@9012',    'zain.a@gmail.com'),
(2, 'hina_b',        'Pass@0123',    'hina.b@gmail.com');
 
-- ============================================
-- 3. CUSTOMERS
-- ============================================
INSERT INTO Customers (UserID, FullName, Phone, Gender) VALUES
(6,  'Ahmed Chaudhry',   '03001234567', 'Male'),
(7,  'Fatima Khan',      '03011234567', 'Female'),
(8,  'Bilal Mahmood',    '03021234567', 'Male'),
(9,  'Ayesha Raza',      '03031234567', 'Female'),
(10, 'Usman Tariq',      '03041234567', 'Male'),
(11, 'Sana Qadir',       '03051234567', 'Female'),
(12, 'Hamza Nawaz',      '03061234567', 'Male'),
(13, 'Maryam Saeed',     '03071234567', 'Female'),
(14, 'Zain Abbas',       '03081234567', 'Male'),
(15, 'Hina Baig',        '03091234567', 'Female');
 
-- ============================================
-- 4. GENRES
-- ============================================
INSERT INTO Genres (GenreName) VALUES
('Action'),
('Comedy'),
('Drama'),
('Horror'),
('Romance'),
('Thriller'),
('Animation'),
('Sci-Fi');
 
-- ============================================
-- 5. MOVIES
-- ============================================
INSERT INTO Movies (GenreID, Title, Duration, Language, ReleaseDate, Rating) VALUES
(1, 'Lahore Express',        120, 'Urdu',    '2024-01-15', 4.2),
(2, 'Punjabi Dhamaka',       105, 'Punjabi', '2024-02-20', 3.8),
(3, 'Teri Yaad',             135, 'Urdu',    '2024-03-10', 4.5),
(6, 'Raaz Ki Raat',          110, 'Urdu',    '2024-04-05', 4.0),
(1, 'Operation Karachi',     125, 'Urdu',    '2024-05-01', 4.3),
(8, 'Galaxia',               140, 'English', '2024-06-15', 4.7),
(4, 'Andhere Mein',          100, 'Urdu',    '2024-07-20', 3.5),
(5, 'Dil Wala',              115, 'Urdu',    '2024-08-10', 4.1),
(7, 'Cartoon Baaz',           90, 'Urdu',    '2024-09-05', 3.9),
(3, 'Zindagi Ek Safar',      130, 'Urdu',    '2024-10-01', 4.4);
 
-- ============================================
-- 6. HALLS
-- ============================================
INSERT INTO Halls (HallName, Capacity) VALUES
('Hall A', 50),
('Hall B', 80),
('Hall C', 30),
('VIP Lounge', 20),
('IMAX Hall', 100);
 
-- ============================================
-- 7. SEATTYPES
-- ============================================
INSERT INTO SeatTypes (TypeName, ExtraPrice) VALUES
('Standard',  0.00),
('Premium',  200.00),
('VIP',      500.00),
('Couple',   300.00),
('IMAX',     400.00);
 
-- ============================================
-- 8. SEATS (Hall A - 10 seats, Hall B - 10 seats, Hall C - 5 seats, VIP - 5 seats, IMAX - 10 seats)
-- ============================================
-- Hall A (HallID=1) - Standard seats
INSERT INTO Seats (HallID, SeatTypeID, SeatNumber, Status) VALUES
(1, 1, 'A1',  'Available'),
(1, 1, 'A2',  'Available'),
(1, 1, 'A3',  'Available'),
(1, 1, 'A4',  'Available'),
(1, 1, 'A5',  'Available'),
(1, 2, 'B1',  'Available'),
(1, 2, 'B2',  'Available'),
(1, 2, 'B3',  'Available'),
(1, 4, 'C1',  'Available'),
(1, 4, 'C2',  'Available');
 
-- Hall B (HallID=2) - Mix of Standard and Premium
INSERT INTO Seats (HallID, SeatTypeID, SeatNumber, Status) VALUES
(2, 1, 'A1',  'Available'),
(2, 1, 'A2',  'Available'),
(2, 1, 'A3',  'Available'),
(2, 1, 'A4',  'Available'),
(2, 2, 'B1',  'Available'),
(2, 2, 'B2',  'Available'),
(2, 2, 'B3',  'Available'),
(2, 4, 'C1',  'Available'),
(2, 4, 'C2',  'Available'),
(2, 4, 'C3',  'Available');
 
-- Hall C (HallID=3) - Standard
INSERT INTO Seats (HallID, SeatTypeID, SeatNumber, Status) VALUES
(3, 1, 'A1',  'Available'),
(3, 1, 'A2',  'Available'),
(3, 1, 'A3',  'Available'),
(3, 2, 'B1',  'Available'),
(3, 2, 'B2',  'Available');
 
-- VIP Lounge (HallID=4) - VIP seats
INSERT INTO Seats (HallID, SeatTypeID, SeatNumber, Status) VALUES
(4, 3, 'V1',  'Available'),
(4, 3, 'V2',  'Available'),
(4, 3, 'V3',  'Available'),
(4, 3, 'V4',  'Available'),
(4, 3, 'V5',  'Available');
 
-- IMAX Hall (HallID=5) - IMAX seats
INSERT INTO Seats (HallID, SeatTypeID, SeatNumber, Status) VALUES
(5, 5, 'I1',  'Available'),
(5, 5, 'I2',  'Available'),
(5, 5, 'I3',  'Available'),
(5, 5, 'I4',  'Available'),
(5, 5, 'I5',  'Available'),
(5, 5, 'I6',  'Available'),
(5, 5, 'I7',  'Available'),
(5, 5, 'I8',  'Available'),
(5, 5, 'I9',  'Available'),
(5, 5, 'I10', 'Available');
 
-- ============================================
-- 9. SHOWS
-- (EndTime will be auto-filled by trigger)
-- ============================================
INSERT INTO Shows (MovieID, HallID, ShowDate, StartTime, TicketPrice) VALUES
(1,  1, '2024-11-01', '10:00:00', 500.00),
(1,  1, '2024-11-01', '14:00:00', 500.00),
(1,  1, '2024-11-01', '18:00:00', 600.00),
(2,  2, '2024-11-01', '11:00:00', 450.00),
(2,  2, '2024-11-01', '15:00:00', 450.00),
(3,  3, '2024-11-02', '12:00:00', 550.00),
(4,  4, '2024-11-02', '20:00:00', 900.00),
(5,  5, '2024-11-03', '13:00:00', 800.00),
(6,  5, '2024-11-03', '17:00:00', 850.00),
(7,  1, '2024-11-04', '10:00:00', 480.00),
(8,  2, '2024-11-04', '14:00:00', 500.00),
(9,  3, '2024-11-05', '11:00:00', 400.00),
(10, 4, '2024-11-05', '19:00:00', 950.00),
(3,  2, '2024-11-06', '16:00:00', 550.00),
(5,  5, '2024-11-06', '20:00:00', 850.00);
 
-- ============================================
-- 10. PAYMENTMETHODS
-- ============================================
INSERT INTO PaymentMethods (MethodName) VALUES
('Cash'),
('Credit Card'),
('Debit Card'),
('JazzCash'),
('EasyPaisa'),
('Bank Transfer');
 
-- ============================================
-- 11. BOOKINGS
-- ============================================
INSERT INTO Bookings (CustomerID, ShowID, BookingDate, TotalAmount, Status) VALUES
(1,  1,  '2024-10-28 10:30:00', 1000.00, 'Booked'),
(2,  2,  '2024-10-28 11:00:00',  500.00, 'Booked'),
(3,  3,  '2024-10-29 09:00:00', 1200.00, 'Booked'),
(4,  4,  '2024-10-29 12:00:00',  900.00, 'Booked'),
(5,  5,  '2024-10-30 10:00:00',  450.00, 'Cancelled'),
(6,  6,  '2024-10-30 14:00:00', 1100.00, 'Booked'),
(7,  7,  '2024-10-31 09:30:00', 1800.00, 'Booked'),
(8,  8,  '2024-10-31 11:00:00', 1600.00, 'Booked'),
(9,  9,  '2024-11-01 10:00:00', 1700.00, 'Booked'),
(10, 10, '2024-11-01 13:00:00',  960.00, 'Booked'),
(1,  11, '2024-11-02 09:00:00', 1000.00, 'Booked'),
(2,  12, '2024-11-02 10:30:00',  400.00, 'Cancelled'),
(3,  13, '2024-11-03 11:00:00', 1900.00, 'Booked'),
(4,  14, '2024-11-03 14:00:00', 1100.00, 'Booked'),
(5,  15, '2024-11-04 10:00:00', 1700.00, 'Booked');
 
-- ============================================
-- 12. BOOKINGDETAILS
-- ============================================
INSERT INTO BookingDetails (BookingID, SeatID, PriceAtBooking) VALUES
-- (1,  1,  500.00),
(1,  2,  500.00),
(2,  3,  500.00),
(3,  6,  700.00),
(3,  7,  700.00),
(4,  11, 450.00),
(4,  12, 450.00),
(5,  13, 450.00),
(6,  21, 550.00),
(6,  22, 550.00),
(7,  26, 1400.00),
(7,  27, 1400.00),
(8,  31, 1200.00),
(9,  36, 1250.00),
(10, 4,  480.00),
(10, 5,  480.00),
(11, 14, 500.00),
(11, 15, 500.00),
(12, 23, 400.00),
(13, 28, 1450.00),
(13, 29, 1450.00),
(14, 16, 550.00),
(14, 17, 550.00),
(15, 37, 1250.00),
(15, 38, 1250.00);
 
-- ============================================
-- 13. PAYMENTS
-- ============================================
INSERT INTO Payments (BookingID, MethodID, Amount, PaymentDate) VALUES
(1,  2, 1000.00, '2024-10-28 10:35:00'),
(2,  1,  500.00, '2024-10-28 11:05:00'),
(3,  3, 1200.00, '2024-10-29 09:10:00'),
(4,  4,  900.00, '2024-10-29 12:10:00'),
(6,  2, 1100.00, '2024-10-30 14:15:00'),
(7,  3, 1800.00, '2024-10-31 09:40:00'),
(8,  5, 1600.00, '2024-10-31 11:10:00'),
(9,  2, 1700.00, '2024-11-01 10:15:00'),
(10, 4,  960.00, '2024-11-01 13:10:00'),
(11, 1, 1000.00, '2024-11-02 09:10:00'),
(13, 2, 1900.00, '2024-11-03 11:10:00'),
(14, 3, 1100.00, '2024-11-03 14:15:00'),
(15, 5, 1700.00, '2024-11-04 10:10:00');
 
-- ============================================
-- 14. REVIEWS
-- ============================================
INSERT INTO Reviews (CustomerID, MovieID, Rating, Comment) VALUES
(1,  1, 5, 'Zabardast film thi! Bilkul achi lagi.'),
(2,  1, 4, 'Bohat acha tha, zaroor dekhain.'),
(3,  3, 5, 'Dil ko chu lene wali kahani.'),
(4,  4, 3, 'Theek tha, lekin thora aur better ho sakta tha.'),
(5,  2, 4, 'Comedy bohat achi thi, pura family enjoy kiya.'),
(6,  3, 5, 'Best movie of the year!'),
(7,  7, 4, 'VIP experience was amazing.'),
(8,  5, 5, 'Action scenes were thrilling!'),
(9,  6, 5, 'IMAX mein dekh k maza aa gaya.'),
(10, 8, 4, 'Romantic story bohat achi thi.');
 
-- ============================================
-- 15. CANCELLATIONPOLICY
-- ============================================
INSERT INTO CancellationPolicy (DaysBeforeShow, RefundPercentage, Description) VALUES
(7,  100.00, '7 din pehle cancel karo - poora refund'),
(5,   75.00, '5 din pehle cancel karo - 75% refund'),
(3,   50.00, '3 din pehle cancel karo - 50% refund'),
(1,   25.00, '1 din pehle cancel karo - 25% refund'),
(0,    0.00, 'Show ke din cancel - koi refund nahi');
 


CREATE OR REPLACE VIEW vw_available_seats AS
SELECT
    sh.ShowID,
    m.Title          AS MovieTitle,
    h.HallName,
    sh.ShowDate,
    sh.ShowDate      AS ShowDate2,
    sh.StartTime,
    sh.EndTime,
    s.SeatID,
    s.SeatNumber,
    st.TypeName      AS SeatType,
    sh.TicketPrice   + st.ExtraPrice AS TotalSeatPrice,
    s.Status
FROM Shows sh
JOIN Movies  m  ON sh.MovieID    = m.MovieID
JOIN Halls   h  ON sh.HallID     = h.HallID
JOIN Seats   s  ON s.HallID      = sh.HallID
JOIN SeatTypes st ON s.SeatTypeID = st.SeatTypeID
WHERE s.Status = 'Available';

-- ============================================
-- VIEW 2: Booking History Per Customer
-- Customer ki poori booking history
-- Java mein "My Bookings" screen pe use hoga
-- ============================================
CREATE OR REPLACE VIEW vw_booking_history AS
SELECT
    b.BookingID,
    c.FullName          AS CustomerName,
    c.Phone,
    m.Title             AS MovieTitle,
    h.HallName,
    sh.ShowDate,
    sh.StartTime,
    sh.EndTime,
    GROUP_CONCAT(s.SeatNumber ORDER BY s.SeatNumber SEPARATOR ', ') AS Seats,
    b.TotalAmount,
    b.Status            AS BookingStatus,
    b.BookingDate,
    pm.MethodName       AS PaymentMethod
FROM Bookings b
JOIN Customers    c   ON b.CustomerID  = c.CustomerID
JOIN Shows        sh  ON b.ShowID      = sh.ShowID
JOIN Movies       m   ON sh.MovieID    = m.MovieID
JOIN Halls        h   ON sh.HallID     = h.HallID
JOIN BookingDetails bd ON b.BookingID  = bd.BookingID
JOIN Seats        s   ON bd.SeatID     = s.SeatID
LEFT JOIN Payments    p  ON b.BookingID  = p.BookingID
LEFT JOIN PaymentMethods pm ON p.MethodID = pm.MethodID
GROUP BY
    b.BookingID, c.FullName, c.Phone,
    m.Title, h.HallName,
    sh.ShowDate, sh.StartTime, sh.EndTime,
    b.TotalAmount, b.Status, b.BookingDate,
    pm.MethodName;
 
-- ============================================
-- VIEW 3: Revenue Per Movie
-- Har movie ka total revenue
-- Java mein reports screen pe use hoga
-- ============================================
CREATE OR REPLACE VIEW vw_movie_revenue AS
SELECT
    m.MovieID,
    m.Title             AS MovieTitle,
    g.GenreName,
    m.Language,
    COUNT(DISTINCT b.BookingID)   AS TotalBookings,
    COUNT(bd.BookingDetailID)     AS TotalTicketsSold,
    SUM(bd.PriceAtBooking)        AS TotalRevenue,
    AVG(bd.PriceAtBooking)        AS AvgTicketPrice
FROM Movies m
JOIN Genres        g   ON m.GenreID   = g.GenreID
JOIN Shows         sh  ON sh.MovieID  = m.MovieID
JOIN Bookings      b   ON b.ShowID    = sh.ShowID  AND b.Status = 'Booked'
JOIN BookingDetails bd  ON bd.BookingID = b.BookingID
GROUP BY
    m.MovieID, m.Title, g.GenreName, m.Language;
 
-- ============================================
-- VIEW 4: Today's Shows
-- Aaj ke saare shows with details
-- Java mein home screen / dashboard pe use hoga
-- ============================================
CREATE OR REPLACE VIEW vw_todays_shows AS
SELECT
    sh.ShowID,
    m.Title             AS MovieTitle,
    g.GenreName,
    m.Language,
    m.Duration,
    h.HallName,
    h.Capacity,
    sh.ShowDate,
    sh.StartTime,
    sh.EndTime,
    sh.TicketPrice,
    COUNT(s.SeatID)     AS TotalSeats,
    SUM(CASE WHEN s.Status = 'Available' THEN 1 ELSE 0 END) AS AvailableSeats,
    SUM(CASE WHEN s.Status = 'Booked'    THEN 1 ELSE 0 END) AS BookedSeats
FROM Shows sh
JOIN Movies  m  ON sh.MovieID = m.MovieID
JOIN Genres  g  ON m.GenreID  = g.GenreID
JOIN Halls   h  ON sh.HallID  = h.HallID
JOIN Seats   s  ON s.HallID   = sh.HallID
WHERE sh.ShowDate = CURDATE()
GROUP BY
    sh.ShowID, m.Title, g.GenreName, m.Language,
    m.Duration, h.HallName, h.Capacity,
    sh.ShowDate, sh.StartTime, sh.EndTime, sh.TicketPrice;
 
-- ============================================
-- VIEW 5: Top Rated Movies
-- Rating aur reviews ke hisaab se top movies
-- Java mein movies listing screen pe use hoga
-- ============================================
CREATE OR REPLACE VIEW vw_top_rated_movies AS
SELECT
    m.MovieID,
    m.Title             AS MovieTitle,
    g.GenreName,
    m.Language,
    m.Duration,
    m.ReleaseDate,
    m.Rating            AS SystemRating,
    COUNT(r.ReviewID)   AS TotalReviews,
    ROUND(AVG(r.Rating), 1) AS CustomerRating,
    COUNT(DISTINCT bd.BookingDetailID) AS TotalTicketsSold
FROM Movies m
JOIN Genres g ON m.GenreID = g.GenreID
LEFT JOIN Reviews      r   ON r.MovieID   = m.MovieID
LEFT JOIN Shows        sh  ON sh.MovieID  = m.MovieID
LEFT JOIN Bookings     b   ON b.ShowID    = sh.ShowID AND b.Status = 'Booked'
LEFT JOIN BookingDetails bd ON bd.BookingID = b.BookingID
GROUP BY
    m.MovieID, m.Title, g.GenreName, m.Language,
    m.Duration, m.ReleaseDate, m.Rating
ORDER BY CustomerRating DESC, TotalReviews DESC;
 
-- ============================================
-- TEST VIEWS
-- ============================================
SELECT * FROM vw_available_seats    LIMIT 5;
SELECT * FROM vw_booking_history    LIMIT 5;
SELECT * FROM vw_movie_revenue      LIMIT 5;
SELECT * FROM vw_todays_shows       LIMIT 5;
SELECT * FROM vw_top_rated_movies   LIMIT 5;



USE cinema_booking_system;
 
DELIMITER $$
 
-- ============================================
-- PROCEDURE 1: Book Tickets
-- Booking + Seats + Payment ek saath
-- Agar kuch bhi fail ho to sab rollback
-- ============================================
DROP PROCEDURE IF EXISTS sp_book_tickets$$
 
CREATE PROCEDURE sp_book_tickets(
    IN  p_CustomerID  INT,
    IN  p_ShowID      INT,
    IN  p_SeatIDs     VARCHAR(255),  -- comma separated: '1,2,3'
    IN  p_MethodID    INT,
    OUT p_BookingID   INT,
    OUT p_Message     VARCHAR(255)
)
BEGIN
    DECLARE v_TicketPrice   DECIMAL(10,2);
    DECLARE v_TotalAmount   DECIMAL(10,2) DEFAULT 0;
    DECLARE v_BookedCount   INT DEFAULT 0;
 
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        ROLLBACK;
        SET p_Message   = 'Error occurred. Transaction rolled back.';
        SET p_BookingID = 0;
    END;
 
    START TRANSACTION;
 
    -- Show ka ticket price lo
    SELECT TicketPrice INTO v_TicketPrice
    FROM Shows WHERE ShowID = p_ShowID;
 
    -- Check karo koi seat already booked toh nahi
    SELECT COUNT(*) INTO v_BookedCount
    FROM Seats
    WHERE SeatID IN (
        SELECT TRIM(val) FROM JSON_TABLE(
            CONCAT('["', REPLACE(p_SeatIDs, ',', '","'), '"]'),
            '$[*]' COLUMNS(val VARCHAR(10) PATH '$')
        ) AS jt
    )
    AND Status = 'Booked';
 
    IF v_BookedCount > 0 THEN
        ROLLBACK;
        SET p_Message   = 'One or more selected seats are already booked.';
        SET p_BookingID = 0;
    ELSE
        -- Total amount calculate karo
        SELECT SUM(v_TicketPrice + st.ExtraPrice) INTO v_TotalAmount
        FROM Seats s
        JOIN SeatTypes st ON s.SeatTypeID = st.SeatTypeID
        WHERE s.SeatID IN (
            SELECT TRIM(val) FROM JSON_TABLE(
                CONCAT('["', REPLACE(p_SeatIDs, ',', '","'), '"]'),
                '$[*]' COLUMNS(val VARCHAR(10) PATH '$')
            ) AS jt
        );
 
        -- Booking insert karo
        INSERT INTO Bookings (CustomerID, ShowID, BookingDate, TotalAmount, Status)
        VALUES (p_CustomerID, p_ShowID, NOW(), v_TotalAmount, 'Booked');
 
        SET p_BookingID = LAST_INSERT_ID();
 
        -- BookingDetails insert karo
        -- (Trigger 3 automatically seat status 'Booked' kar dega)
        INSERT INTO BookingDetails (BookingID, SeatID, PriceAtBooking)
        SELECT p_BookingID, s.SeatID, (v_TicketPrice + st.ExtraPrice)
        FROM Seats s
        JOIN SeatTypes st ON s.SeatTypeID = st.SeatTypeID
        WHERE s.SeatID IN (
            SELECT TRIM(val) FROM JSON_TABLE(
                CONCAT('["', REPLACE(p_SeatIDs, ',', '","'), '"]'),
                '$[*]' COLUMNS(val VARCHAR(10) PATH '$')
            ) AS jt
        );
 
        -- Payment insert karo
        INSERT INTO Payments (BookingID, MethodID, Amount, PaymentDate)
        VALUES (p_BookingID, p_MethodID, v_TotalAmount, NOW());
 
        COMMIT;
        SET p_Message = 'Booking successful!';
    END IF;
END$$

DELIMITER $$
 
-- ============================================
-- PROCEDURE 1: Book Tickets
-- Single seat book karo
-- Java mein loop laga ke multiple seats book karna
-- ============================================


DELIMITER $$
CREATE PROCEDURE sp_add_review(
    IN  p_CustomerID INT,
    IN  p_MovieID    INT,
    IN  p_Rating     INT,       -- 1 to 5
    IN  p_Comment    TEXT,
    OUT p_Message    VARCHAR(255)
)
BEGIN
    IF p_Rating NOT BETWEEN 1 AND 5 THEN
        SET p_Message = 'Rating 1 se 5 ke darmiyan honi chahiye.';
    ELSE
        INSERT INTO Reviews
            (CustomerID, MovieID, Rating, Comment)
        VALUES
            (p_CustomerID, p_MovieID, p_Rating, p_Comment)
        ON DUPLICATE KEY UPDATE
            Rating  = p_Rating,
            Comment = p_Comment;
        SET p_Message = 'Review has been saved!';
    END IF;
END$$
DELIMITER ;

-- Test:
CALL sp_add_review(1, 2, 5, 'excellent!', @msg);
SELECT @msg;



DELIMITER $$
CREATE PROCEDURE sp_cancel_booking(
    IN  p_BookingID  INT,
    OUT p_Refund     DECIMAL(10,2),
    OUT p_Message    VARCHAR(255)
)
BEGIN
    DECLARE v_Status      VARCHAR(20);
    DECLARE v_ShowDate    DATE;
    DECLARE v_TotalAmount DECIMAL(10,2);
    DECLARE v_DaysDiff    INT;
    DECLARE v_RefundPct   DECIMAL(5,2) DEFAULT 0;

    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        ROLLBACK;
        SET p_Message = 'Error occurred. Transaction rolled back.';
        SET p_Refund  = 0;
    END;

    START TRANSACTION;

    SELECT b.Status, sh.ShowDate, b.TotalAmount
    INTO   v_Status, v_ShowDate, v_TotalAmount
    FROM   Bookings b
    JOIN   Shows sh ON b.ShowID = sh.ShowID
    WHERE  b.BookingID = p_BookingID;

    IF v_Status = 'Cancelled' THEN
        COMMIT;
        SET p_Message = 'Booking is already cancelled.';
        SET p_Refund  = 0;
    ELSE
        SET v_DaysDiff = DATEDIFF(v_ShowDate, CURDATE());

        SELECT RefundPercentage INTO v_RefundPct
        FROM   CancellationPolicy
        WHERE  DaysBeforeShow <= v_DaysDiff
        ORDER  BY DaysBeforeShow DESC
        LIMIT  1;

        SET v_RefundPct = COALESCE(v_RefundPct, 0);
        SET p_Refund    = ROUND((v_TotalAmount * v_RefundPct) / 100, 2);

        -- Trigger 4 automatically seats 'Available' kar dega
        UPDATE Bookings
        SET    Status = 'Cancelled'
        WHERE  BookingID = p_BookingID;

        COMMIT;
        SET p_Message = CONCAT('Cancelled. Refund: Rs. ', p_Refund);
    END IF;
END$$
DELIMITER ;

-- Test:
CALL sp_cancel_booking(5, @refund, @msg);
SELECT @refund AS Refund, @msg AS Message;

DELIMITER ;



-- Insert new shows for today and future
INSERT INTO Shows (MovieID, HallID, ShowDate, StartTime, TicketPrice) VALUES
(1,  1, CURDATE(), '10:00:00', 500.00),
(1,  1, CURDATE(), '14:00:00', 500.00),
(1,  1, CURDATE(), '18:00:00', 600.00),
(2,  2, CURDATE(), '11:00:00', 450.00),
(2,  2, CURDATE(), '15:00:00', 450.00),
(3,  3, DATE_ADD(CURDATE(), INTERVAL 1 DAY), '12:00:00', 550.00),
(4,  4, DATE_ADD(CURDATE(), INTERVAL 1 DAY), '20:00:00', 900.00),
(5,  5, DATE_ADD(CURDATE(), INTERVAL 1 DAY), '13:00:00', 800.00),
(6,  5, DATE_ADD(CURDATE(), INTERVAL 2 DAY), '17:00:00', 850.00),
(7,  1, DATE_ADD(CURDATE(), INTERVAL 2 DAY), '10:00:00', 480.00),
(8,  2, DATE_ADD(CURDATE(), INTERVAL 2 DAY), '14:00:00', 500.00),
(9,  3, DATE_ADD(CURDATE(), INTERVAL 3 DAY), '11:00:00', 400.00),
(10, 4, DATE_ADD(CURDATE(), INTERVAL 3 DAY), '19:00:00', 950.00),
(3,  2, DATE_ADD(CURDATE(), INTERVAL 3 DAY), '16:00:00', 550.00),
(5,  5, DATE_ADD(CURDATE(), INTERVAL 4 DAY), '20:00:00', 850.00);

-- Verify
SELECT s.ShowID, m.Title, s.ShowDate, s.StartTime, h.HallName, s.TicketPrice
FROM Shows s
JOIN Movies m ON s.MovieID = m.MovieID
JOIN Halls h ON s.HallID = h.HallID
WHERE s.ShowDate >= CURDATE()
ORDER BY s.ShowDate, s.StartTime;


alter table movies add column description text null;