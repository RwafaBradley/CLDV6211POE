-- DATABASE CREATION

USE master

IF EXISTS (SELECT * FROM sys.databases WHERE name = 'EventEase')
DROP DATABASE EventEase
CREATE DATABASE EventEase

USE EventEase


CREATE TABLE Venue (
    VenueId INT IDENTITY(1,1) PRIMARY KEY,
    VenueName NVARCHAR(255) NOT NULL,
    Locations NVARCHAR(255) NOT NULL,
    Capacity INT NOT NULL,
    ImageUrl NVARCHAR(500) NOT NULL,
	isAvailabile NVARCHAR(20)
    
);

CREATE TABLE EventType (
EventTypeId INT IDENTITY(1,1) PRIMARY KEY,
Name NVARCHAR(100) NOT NULL
);

CREATE TABLE Event (
    EventId INT IDENTITY(1,1) PRIMARY KEY,
    EventName NVARCHAR(255) NOT NULL,
    EventDate DATE NOT NULL,
    Descriptions NVARCHAR(500),
    EventTypeId INT NOT NULL,
    VenueId INT NULL,
    CONSTRAINT FK_Event_Venue FOREIGN KEY (VenueId) REFERENCES Venue(VenueId) ON DELETE SET NULL,
    CONSTRAINT FK_Event_EventType FOREIGN KEY (EventTypeId) REFERENCES EventType(EventTypeId) ON DELETE CASCADE
);

CREATE TABLE Booking (
    BookingId INT IDENTITY(1,1) PRIMARY KEY,
    EventId INT NOT NULL,
    VenueId INT NOT NULL,
    BookingDate DATE NOT NULL,
    CONSTRAINT FK_Booking_Event FOREIGN KEY (EventId) REFERENCES Event(EventId) ON DELETE CASCADE,
    CONSTRAINT FK_Booking_Venue FOREIGN KEY (VenueId) REFERENCES Venue(VenueId) ON DELETE CASCADE,
    CONSTRAINT UQ_Event_Venue UNIQUE (EventId, VenueId)
	-- The last line is for preventiun of duplicate bookings
);


IF OBJECT_ID(N'dbo.__EFMigrationsHistory', 'U') IS NOT NULL
    DROP TABLE dbo.__EFMigrationsHistory;

--creating events before venue avilability
INSERT INTO Venue (VenueName, Locations, Capacity, ImageUrl, isAvailabile)
VALUES ('Ballortor', 'Las Vegas', 300, 'www.ballortor.com',1);

INSERT INTO Event (EventName, EventDate, Descriptions,EventType , VenueId,EventTypeId)
VALUES ('Dance Fest', '2025-09-11', 'A fun dance fest for all to see','Concert', 1,1);

--updating event prior

INSERT INTO EventType (Name)
VALUES ('Conference'),('Wedding'),('Naming'),('Birthday'),('Concert');


INSERT INTO Booking (EventId, VenueId,BookingDate)
VALUES (1, 1,GETDATE());


--TABLE MANIPULATION
SELECT *
FROM Venue;

SELECT *
FROM Event;

SELECT *
FROM Booking;


--check why bookingid appeared as 2 when inserting values into venueid