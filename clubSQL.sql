-- Create the clubhub database
CREATE DATABASE clubhub;
GO

-- Switch to the new database
USE clubhub;
GO

-- Create the [user] table
CREATE TABLE [user] (
    userID UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    firstName VARCHAR(20) NOT NULL,
    lastName VARCHAR(40) NOT NULL,
    email VARCHAR(255) NOT NULL UNIQUE,
    phoneNumber VARCHAR(15) NOT NULL,
    password VARBINARY(256) NOT NULL,
    role VARCHAR(20) NOT NULL CHECK (role IN ('guest', 'student', 'president', 'advisor', 'admin')),
    PRIMARY KEY(userID)
);
GO

CREATE TABLE club (
    clubID UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    clubName VARCHAR(50) NOT NULL,
    clubDeclaration TEXT NOT NULL,
    presidentName VARCHAR(60) NOT NULL,
    presidentID UNIQUEIDENTIFIER NOT NULL,
    advisorID UNIQUEIDENTIFIER NOT NULL,
    PRIMARY KEY(clubID),
    FOREIGN KEY (presidentID) REFERENCES [user](userID),
    FOREIGN KEY (advisorID) REFERENCES [user](userID)
);
GO

CREATE TABLE userclub (
    userClubID UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    userID UNIQUEIDENTIFIER NOT NULL,
    clubID UNIQUEIDENTIFIER NOT NULL,
    PRIMARY KEY(userClubID),
    FOREIGN KEY (userID) REFERENCES [user](userID),
    FOREIGN KEY (clubID) REFERENCES club(clubID)
);
GO
