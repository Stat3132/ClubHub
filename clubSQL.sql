CREATE DATABASE IF NOT EXISTS clubhub;

USE clubhub;

CREATE TABLE `user` (
	`userID` INT NOT NULL AUTO_INCREMENT,
    `firstName` VARCHAR(20) NOT NULL,
    `lastName` VARCHAR(40) NOT NULL,
    `email` VARCHAR(255) NOT NULL,
    `phoneNumber` VARCHAR(10) NOT NULL,
    `role` ENUM('guest', 'student', 'president', 'advisor','admin') NOT NULL,
    PRIMARY KEY(`userID`));
    
CREATE TABLE `club`(
	`clubID` INT NOT NULL AUTO_INCREMENT,
    `clubName` VARCHAR(50) NOT NULL,
    `clubDeclaration` TEXT NOT NULL,
    `presidentName` VARCHAR(60) NOT NULL,
    `presidentID` INT NOT NULL,
    `advisorID` INT NOT NULL,
    PRIMARY KEY(`clubID`));

CREATE TABLE `userclub`(
	`userClubID` INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    `userID` INT NOT NULL,
    `clubID` INT NOT NULL
	);
    
ALTER TABLE `club` 
ADD CONSTRAINT `presidentIDuserID`
FOREIGN KEY (`presidentID`)
REFERENCES `user`(`userID`);

ALTER TABLE `userclub` 
ADD CONSTRAINT `userID`
FOREIGN KEY (`userID`)
REFERENCES `user`(`userID`);

ALTER TABLE `userclub` 
ADD CONSTRAINT `clubID`
FOREIGN KEY (`clubID`)
REFERENCES `club`(`clubID`);