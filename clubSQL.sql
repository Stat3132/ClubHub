-- Create the clubhub database
CREATE DATABASE clubhub;
GO

-- Switch to the new database
USE clubhub;
GO

-- Create the [user] table
CREATE TABLE [user] (
    userID INT NOT NULL IDENTITY(1,1),
    firstName VARCHAR(20) NOT NULL,
    lastName VARCHAR(40) NOT NULL,
    email VARCHAR(255) NOT NULL,
    phoneNumber VARCHAR(10) NOT NULL,
    password VARBINARY(256) NOT NULL,
    role VARCHAR(20) NOT NULL CHECK (role IN ('guest', 'student', 'president', 'advisor', 'admin')),
    PRIMARY KEY(userID)
);
GO

-- Create the club table
CREATE TABLE club (
    clubID INT NOT NULL IDENTITY(1,1),
    clubName VARCHAR(50) NOT NULL,
    clubDeclaration TEXT NOT NULL,
    presidentName VARCHAR(60) NOT NULL,
    presidentID INT NOT NULL,
    advisorID INT NOT NULL,
    PRIMARY KEY(clubID),
    FOREIGN KEY (presidentID) REFERENCES [user](userID),
    FOREIGN KEY (advisorID) REFERENCES [user](userID)
);
GO

-- Create the userclub junction table
CREATE TABLE userclub (
    userClubID INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
    userID INT NOT NULL,
    clubID INT NOT NULL,
    FOREIGN KEY (userID) REFERENCES [user](userID),
    FOREIGN KEY (clubID) REFERENCES club(clubID)
);
GO
